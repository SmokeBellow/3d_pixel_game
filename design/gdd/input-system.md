# Input System

> **Status**: In Design
> **Author**: user + agents
> **Last Updated**: 2026-08-10
> **Implements Pillar**: Foundation (enables Challenge/Fantasy aesthetics — responsive input is a prerequisite for "fast, responsive combat")
> **Creative Director Review (CD-GDD-ALIGN)**: skipped — Lean mode

## Overview

The Input System is the foundation layer that translates raw device input
(keyboard, mouse, and partial gamepad) into abstract, named actions that
gameplay systems consume — Combat System, Camera System, and eventually Skill
Tree/Hub navigation all read from this layer rather than polling devices
directly. Built on Unity's Input System Package (Input Actions asset +
generated C# wrapper class), not the legacy `Input` class the core-combat
prototype used for speed. This system has no player fantasy of its own —
players don't "feel" the Input System, they feel the responsiveness of Combat
and Camera, which this system exists to make possible without perceptible
lag.

## Player Fantasy

Players never consciously notice the Input System. Its entire "fantasy" is
negative-space: the absence of input lag, dropped inputs, or awkward-feeling
controls. Success is measured by what players say about *other* systems —
"combat feels responsive" — never by anything said about input itself. If a
playtester ever mentions input directly, that is a failure signal, not a
compliment.

## Detailed Design

### Core Rules

1. A single Input Actions asset (`PlayerControls`) defines two Action Maps:
   **Gameplay** and **UI**.
2. Gameplay map actions: `Move` (Vector2 — WASD + gamepad left stick), `Look`
   (Vector2 — mouse delta + gamepad right stick), `Attack` (Button — LMB +
   gamepad face button), `Interact` (Button — E + gamepad face button),
   `Pause` (Button — Escape + gamepad Start).
3. Gameplay code reads actions through the generated C# wrapper class, never
   by polling `Keyboard`/`Mouse` singletons directly — this is Unity's
   recommended pattern and keeps gameplay code decoupled from device specifics.
4. `Look` directly drives character/view rotation (first-person mouselook).
   There is no separate camera-only free-look mode, since the game has no
   third-person orbit rig (see game-concept.md Technical Considerations →
   Camera Perspective).
5. Input System reports only raw `Attack` press/release events. All combo
   buffering and timing logic lives in Combat System — Input System does not
   interpret input beyond translating device signals into named actions.
6. Active device (keyboard/mouse vs. gamepad) is auto-detected from the last
   input received, to drive on-screen prompt/icon switching.

### States and Transitions

| State | Active Action Map | Transition |
| ---- | ---- | ---- |
| In-game (dungeon/hub, not paused) | Gameplay | → UI on `Pause` press |
| Pause menu / interaction dialog | UI | → Gameplay on Resume/Close |

The Gameplay map is disabled whenever the UI map is active, preventing
movement/attack input from leaking through under an open menu.

### Interactions with Other Systems

- **Camera System** (not yet designed) — consumes `Look` directly to drive
  view rotation.
- **Combat System** (not yet designed) — consumes `Move` and `Attack`
  press/release events; owns all combo timing logic itself.
- **Hub Sanctuary System / Guild Narrative Delivery** (not yet designed) —
  consume `Interact`.
- **Combat/Trial UI** (not yet designed) — owns the pause menu itself; Input
  System only reports the `Pause` press event.

These are provisional assumptions about undesigned dependencies — their
exact interfaces may be refined once those systems are designed.

## Formulas

### mouse_look_rotation

The `mouse_look_rotation` formula is defined as:
`mouse_look_rotation = raw_delta * sensitivity * base_scale`

| Variable | Symbol | Type | Range | Description |
| ---- | ---- | ---- | ---- | ---- |
| Raw mouse delta | raw_delta | float | unbounded (device-reported, typically -100–100 px) | Pixel delta from the Input System this frame |
| Sensitivity setting | sensitivity | float | 0.1–10.0 | User-adjustable slider value (1.0 = default) |
| Base scale constant | base_scale | float | tuned, ~0.02 deg/px | Converts pixels to degrees at sensitivity = 1.0 |
| Rotation applied | mouse_look_rotation | float | unbounded per axis, clamped on pitch | Degrees to apply to yaw/pitch this frame |

**Output Range:** Unbounded in raw magnitude (fast flicks can exceed 100°/frame), but pitch is clamped to ±89° from horizon post-accumulation to prevent camera flip; yaw wraps 0–360°. No frame-time multiplier — mouse delta is already a per-frame absolute value, not a rate.
**Example:** raw_delta = 15 px, sensitivity = 2.0, base_scale = 0.02 → rotation = 15 × 2.0 × 0.02 = 0.6° this frame.

### gamepad_stick_response

The `gamepad_stick_response` formula is defined as:
`gamepad_stick_response = 0 if magnitude <= deadzone, else ((magnitude - deadzone) / (1 - deadzone))^curve_exponent * direction`

| Variable | Symbol | Type | Range | Description |
| ---- | ---- | ---- | ---- | ---- |
| Raw stick magnitude | magnitude | float | 0.0–1.0 | Length of raw stick vector |
| Deadzone threshold | deadzone | float | 0.05–0.20 (default 0.12) | Minimum magnitude before input registers, avoids drift |
| Response curve exponent | curve_exponent | float | 1.0 (linear)–3.0 | >1 softens low-end for precision, 1.0 = linear |
| Stick direction unit vector | direction | Vector2 | unit length | Normalized raw stick direction |
| Processed input | gamepad_stick_response | Vector2 | 0.0–1.0 magnitude | Rescaled stick value fed into Look/Move |

**Output Range:** 0.0 at/below deadzone, rescales linearly (or curved) from deadzone to full magnitude at 1.0 — full range is reachable so max stick push always yields max output.
**Example:** magnitude = 0.5, deadzone = 0.12, curve_exponent = 1.5 → ((0.5-0.12)/0.88)^1.5 = 0.432^1.5 ≈ 0.284.

### frame_rate_independence (mouse vs. gamepad handling)

Mouse delta is a per-frame absolute displacement (not a rate), so
`mouse_look_rotation` above must **not** be multiplied by `Time.deltaTime` —
doing so would make look speed vary with frame rate. Gamepad stick input, by
contrast, represents a held rate (like a velocity), so it must be scaled by
frame time:

`gamepad_look_rotation = gamepad_stick_response * look_speed_deg_per_sec * delta_time`

| Variable | Symbol | Type | Range | Description |
| ---- | ---- | ---- | ---- | ---- |
| Processed stick value | gamepad_stick_response | float | 0.0–1.0 | Output of gamepad_stick_response formula |
| Look speed constant | look_speed_deg_per_sec | float | tuned, ~180 | Max rotation rate at full stick deflection |
| Frame delta time | delta_time | float | ~0.008–0.033s (30–120fps) | Unity `Time.deltaTime` (unscaled, since pause uses timescale 0) |
| Rotation applied | gamepad_look_rotation | float | 0–6° per frame typical | Degrees applied to yaw/pitch this frame |

**Output Range:** 0 to `look_speed_deg_per_sec * delta_time`; bounded by frame time so rotation-per-second stays constant regardless of frame rate.
**Example:** stick fully deflected (1.0), look_speed = 180°/s, delta_time = 1/60s → rotation = 180 × (1/60) = 3° this frame — same 180°/s at any frame rate.

## Edge Cases

- **If the window loses focus (alt-tab) during gameplay**: the cursor
  unlocks and the game automatically pauses (Gameplay → UI map transition),
  preventing phantom mouse deltas from being applied when focus returns.
- **If multiple gamepads are connected simultaneously**: only
  `Gamepad.current` (the most recently active device, per Unity's pattern)
  is used — others are ignored; no device-selection UI at MVP.
- **If the active gamepad disconnects mid-session**: automatic fallback to
  keyboard/mouse; on-screen prompts switch to keyboard icons within one
  frame.
- **If the Gameplay map is disabled while `Move` is held (transition to
  UI map)**: the `Move` value is force-reset to zero on map disable — no
  "stuck" movement can persist once control returns to Gameplay.
- **Sharp mouse-delta spikes (high DPI, micro-stutters)**: because raw
  device delta is read directly through the new Input System (not derived
  from cursor position), the "cursor hits the screen edge → delta spike"
  bug found in the core-combat prototype's legacy-Input implementation does
  not reproduce here by construction.
- **Cursor lock/visibility**: the cursor is locked (`CursorLockMode.Locked`)
  and hidden for the entire Gameplay state; it unlocks and becomes visible
  on transition to the UI map (pause/dialog) and re-locks on return.

## Dependencies

**Depends On**: — (Foundation layer; no dependencies)

**Depended On By** (hard dependencies — cannot function without this system):
- **Camera System** — consumes `Look`
- **Combat System** — consumes `Move`, `Attack`
- **Hub Sanctuary System / Guild Narrative Delivery** — consume `Interact`
- **Combat/Trial UI** — consumes `Pause`

## Tuning Knobs

| Knob | Range | Default | Effect | Too Low | Too High |
| ---- | ---- | ---- | ---- | ---- | ---- |
| `sensitivity` (mouse) | 0.1–10.0 | 1.0 | Scales mouse_look_rotation | View feels sluggish/unresponsive | View flicks wildly, hard to aim combos |
| `base_scale` | tuned, ~0.02 deg/px | 0.02 | Global px-to-degree conversion at sensitivity=1.0 | Same as low sensitivity globally | Same as high sensitivity globally |
| `deadzone` (gamepad) | 0.05–0.20 | 0.12 | Minimum stick magnitude before input registers | Stick drift causes unwanted look/move | Small deliberate stick nudges get ignored |
| `curve_exponent` (gamepad) | 1.0–3.0 | 1.5 | Shapes stick response curve | 1.0 = fully linear, less fine-control precision | >3.0 makes low-end feel unresponsive/dead |
| `look_speed_deg_per_sec` (gamepad) | tuned, ~90–270 | 180 | Max rotation rate at full stick deflection | Gamepad users can't turn fast enough to track threats | Overshoots targets, hard to track combo timing |

All knobs are exposed to players via a Settings/Accessibility menu (sensitivity
at minimum) — not code-only constants — since sensitivity preference varies
significantly per player and directly affects combat feel.

## Visual/Audio Requirements

None — Input System is pure infrastructure with no visual or audio output of
its own. Any input-driven feedback (hit-stop, camera shake, sound on attack)
belongs to Combat System and HitFeedback-equivalent systems, which consume
this system's actions but own their own presentation.

## UI Requirements

- On-screen control prompts/icons (e.g., "E to Interact", attack button icon)
  must switch automatically between keyboard/mouse and gamepad glyphs based
  on the last-active input device (see Core Rules #6 and Edge Cases —
  gamepad disconnect).
- A Settings/Accessibility menu must expose `sensitivity` at minimum (see
  Tuning Knobs); rebinding UI is out of scope for MVP (not listed as an MVP
  requirement in game-concept.md) but the Input Actions asset structure
  supports it later via `PerformInteractiveRebinding()` without rework.

> **📌 UX Flag — Input System**: This system has UI requirements (device-prompt
> icon switching, sensitivity settings). In Phase 4 (Pre-Production), run
> `/ux-design` for the Settings screen before writing epics.

## Acceptance Criteria

**Action Maps**
1. GIVEN the Gameplay map is active, WHEN `Pause` is pressed, THEN the UI map becomes active and the Gameplay map is disabled.
2. GIVEN the UI map is active, WHEN Resume/Close is triggered, THEN the Gameplay map becomes active and the UI map is disabled.
3. GIVEN the UI map is active, WHEN `Move`/`Attack`/`Interact` inputs are pressed, THEN no Gameplay-side action fires (map mutual exclusivity holds).

**Mouse Look**
4. GIVEN a fixed raw mouse delta of 15px, sensitivity 2.0, base_scale 0.02, WHEN one frame elapses, THEN rotation = 0.6° regardless of frame rate.
5. GIVEN the game running at 30fps, WHEN the mouse is moved a fixed total pixel distance over 1 second, THEN total rotation applied equals the same total rotation as at 144fps for the same pixel distance (mouse rotation is delta-based, not time-based).
6. GIVEN accumulated pitch reaches 89° from horizon, WHEN further upward mouse delta is applied, THEN pitch does not exceed 89°.
7. GIVEN accumulated yaw crosses 360°, WHEN further yaw delta is applied, THEN yaw wraps to 0° rather than growing unbounded.

**Gamepad**
8. GIVEN stick magnitude ≤ deadzone (0.12 default), WHEN the value is sampled, THEN gamepad_stick_response = 0.
9. GIVEN stick magnitude = 0.5, deadzone = 0.12, curve_exponent = 1.5, WHEN sampled, THEN output ≈ 0.284 (±0.01 tolerance).
10. GIVEN stick is pushed to full magnitude (1.0), WHEN sampled, THEN output magnitude = 1.0 exactly, at any curve_exponent.
11. GIVEN full stick deflection held for 1 second at 30fps, and the same held for 1 second at 144fps, WHEN total rotation is measured, THEN both produce the same total rotation (180° at look_speed_deg_per_sec = 180).
12. GIVEN multiple gamepads connected, WHEN input is received from a second device, THEN only `Gamepad.current` drives input; the other device's input is ignored.
13. GIVEN the active gamepad disconnects mid-session, WHEN the next frame renders, THEN on-screen prompts switch to keyboard/mouse icons within one frame.

**Edge Cases**
14. GIVEN the game window is focused and Gameplay map active, WHEN the window loses focus (alt-tab), THEN the cursor unlocks, becomes visible, and the map transitions Gameplay → UI (auto-pause).
15. GIVEN the game was auto-paused by alt-tab, WHEN focus returns and the mouse re-enters the window, THEN no phantom rotation is applied from the alt-tab period.
16. GIVEN `Move` is held with nonzero value, WHEN the map transitions Gameplay → UI, THEN `Move`'s value is force-reset to zero.
17. GIVEN the Gameplay map is active, WHEN observed, THEN `CursorLockMode` = Locked and cursor is hidden.
18. GIVEN the map transitions Gameplay → UI, WHEN observed, THEN `CursorLockMode` = None and cursor is visible.
19. GIVEN the map transitions UI → Gameplay, WHEN observed, THEN `CursorLockMode` returns to Locked and cursor is hidden again.

## Open Questions

- Final default values for `sensitivity`, `deadzone`, `curve_exponent`, and
  `look_speed_deg_per_sec` are tuned placeholders — need real playtesting
  once first-person mouselook is actually running in-engine (owner:
  gameplay-programmer; target: after the planned first-person re-validation
  spike referenced in game-concept.md).
- Rebinding UI is explicitly out of MVP scope — confirm this stays true once
  the accessibility-specialist reviews the full control scheme (owner:
  accessibility-specialist; target: before Vertical Slice).
- Whether gamepad support needs its own dedicated playtest pass given the
  game concept notes gamepad as "partial" support, not primary (owner:
  qa-lead; target: Alpha tier).
