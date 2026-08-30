# Camera System

> **Status**: Designed (pending review)
> **Author**: user + agents
> **Last Updated**: 2026-08-26
> **Implements Pillar**: Foundation (enables Fellowship/Challenge aesthetics — precise FPS aiming is a prerequisite for readable spellcasting and cross-player synergy setup)
> **Creative Director Review (CD-GDD-ALIGN)**: skipped — Lean mode

## Overview

The Camera System owns the player's point of view into the world: it consumes
the `Look` action from Input System (already resolved into
`mouse_look_rotation`/`gamepad_look_rotation` degrees-per-frame) and applies it
to a camera rigidly mounted at the character's eye height, producing a pure
first-person view with no third-person orbit option. Beyond raw look rotation,
Camera System owns everything that makes aiming *feel* like aiming a spell
rather than turning a security camera: FOV response to movement/casting,
recoil-style kick on cast, impact shake on taking or dealing damage, and the
death-to-spectator camera transition. Unlike Input System, which players
should never consciously notice, Camera System is felt directly — a mage who
can't track a fast-moving target through a lightning-chain combo, or whose
view jitters unpredictably, will blame *this* system, not Input.

## Player Fantasy

Directly, Camera System is the mage's eyes and hands-of-aim: the confidence of
tracking a soaked target through a chaotic 4-player fight and knowing your
Lightning bolt will land where you're actually looking. That confidence is
what makes cross-player synergy *readable* — Pillar 1 ("Магия лучше вместе")
depends on players being able to see and aim at what their teammates just set
up. Indirectly, it's the infrastructure of "feel": FOV kick on cast, camera
shake on a landed Chain Shock, the weightless drift into spectator view on
death (Pillar 2, "Каждая смерть — история") — players won't say "the camera
felt good," they'll say "that hit felt great" or "dying didn't feel like
getting kicked out."

*Note: `creative-director` not consulted — Lean mode (Section B is not a
Section D/H high-risk section). Recommend a manual pass before Production if
the tone needs sharpening.*

## Detailed Design

### Core Rules

1. The player character's root transform owns **yaw only** — horizontal
   `Look` rotation (mouse X / gamepad right-stick X) is applied to the
   character body, so movement direction (`Move`) and the visible body
   orientation (for teammates) always match where the player is facing
   horizontally.
2. A camera pivot, parented to the body at a fixed eye-height offset, owns
   **pitch only** — vertical `Look` rotation (mouse Y / gamepad right-stick Y)
   rotates this pivot exclusively. The body's pitch is always 0; only the
   head/camera tilts up and down. This avoids gimbal artifacts and keeps
   `Move`'s forward vector sane at any look angle.
3. Camera System is the **owner of accumulated pitch state and its clamp**.
   Input System supplies a per-frame pitch delta (already frame-rate-
   independent per its `mouse_look_rotation`/`gamepad_look_rotation`
   formulas); Camera System adds that delta to its running pitch value and
   clamps the result to ±89° from horizon every frame — the clamp is enforced
   here, not in Input System, since Input System is stateless.
4. Camera System is likewise the **owner of accumulated yaw state and its
   wrap**: yaw accumulates on the character body transform and wraps at
   0–360° every frame.
5. Base FOV is a tuning knob (see Tuning Knobs). Two additive FOV modifiers
   apply on top of base: a small **cast kick** (brief FOV widen-then-return
   on `CastSpell`) and a **sprint modifier** (if the game has sprint —
   provisional, deferred to Player Controller GDD).
6. Camera shake and FOV kick are driven by **Cinemachine Impulse**
   (`CinemachineImpulseSource` on the emitting event, `CinemachineImpulseListener`
   on the camera). Other systems (Spell Casting, Health & Damage) trigger
   `GenerateImpulse()` without any reference to Camera System's internals —
   this keeps Camera System decoupled from combat logic.
7. On the local player's own death, Camera System detaches its pivot from the
   dead body and exposes a `SetSpectatorTarget(Transform target)` hook.
   Camera System does not decide *who* to spectate or *when* to switch — that
   state machine belongs to **Spectator/Death System** (not yet designed);
   Camera System only guarantees an **instant re-parent (no blend/interpolation)**
   to any given transform — death already breaks visual continuity, so there
   is no smoothing requirement here, and an instant snap keeps this testable
   without an undefined "how smooth is smooth enough" threshold.

### States and Transitions

| State | Description | Transition |
|---|---|---|
| Active (own body) | Camera pivot follows local player's body/eye anchor; full pitch/yaw control | → Spectator on local player death |
| Spectator | Camera pivot follows an externally-assigned target transform (a living teammate); pitch/yaw control is provisional (see Open Questions) | → Active on respawn/checkpoint |

### Interactions with Other Systems

- **Input System** ✅ (designed) — supplies `Look` deltas (already
  sensitivity/deadzone/curve-processed). Camera System is the sole consumer
  that accumulates and clamps them into actual rotation state.
- **Player Controller** (not yet designed, provisional) — Camera System's yaw
  output *is* the body's yaw; Player Controller is expected to read this for
  `Move` direction. Networking replication of yaw/pitch to other clients is
  Player Controller's / Networking Foundation's responsibility, not Camera
  System's — Camera System only produces the local rotation values.
- **Spell Casting System** (not yet designed, provisional) — will read the
  camera pivot's forward vector as the raycast origin/direction for spell
  aiming, and will call `GenerateImpulse()` on cast.
- **Target Feedback System** (not yet designed, provisional) — reads the
  camera pivot's forward vector to compute the currently-aimed-at enemy (this
  is the exact interface `input-system.md` already flagged as required).
- **Health & Damage System** (not yet designed, provisional) — calls
  `GenerateImpulse()` on taking damage.
- **Spectator/Death System** (not yet designed, provisional) — calls
  `SetSpectatorTarget()` and owns all target-selection/switching logic.

## Formulas

### pitch_accumulation

The `pitch_accumulation` formula is defined as:
`pitch_accumulated_new = clamp(pitch_accumulated_prev + pitch_delta, -89, 89)`

| Variable | Symbol | Type | Range | Description |
| ---- | ---- | ---- | ---- | ---- |
| Previous accumulated pitch | pitch_accumulated_prev | float | -89–89° | Camera pivot's pitch state at the start of this frame, held by Camera System |
| Incoming pitch delta | pitch_delta | float | unbounded per frame (typically small) | The pitch-axis component of `mouse_look_rotation` or `gamepad_look_rotation` (whichever device is active this frame, per input-system.md), sign-adjusted so positive = look up |
| New accumulated pitch | pitch_accumulated_new | float | -89–89° | Pitch value applied to the camera pivot's local rotation this frame |

**Output Range:** Hard-clamped to [-89°, 89°] every frame — never reaches ±90° to avoid gimbal-lock/look-vector-degenerate cases at the poles. Clamping is unconditional (applies whether the delta came from mouse or gamepad; Camera System does not need to know which).
**Sign convention:** Camera System must invert the incoming Y-axis delta if the input device convention differs (standard FPS: moving mouse forward/up should decrease screen-space Y delta but increase pitch/look-up) — this inversion is an implementation detail of wiring `Look`'s Y component into `pitch_delta`, not a formula change.
**Example:** pitch_accumulated_prev = 87°, pitch_delta = +5° (from a fast mouse flick) → raw sum = 92° → clamped result = 89°. Next frame, even if pitch_delta is again positive, pitch_accumulated_new stays at 89° until a negative (look-down) delta arrives.

### yaw_accumulation

The `yaw_accumulation` formula is defined as:
`yaw_accumulated_new = ((yaw_accumulated_prev + yaw_delta) mod 360 + 360) mod 360`

| Variable | Symbol | Type | Range | Description |
| ---- | ---- | ---- | ---- | ---- |
| Previous accumulated yaw | yaw_accumulated_prev | float | 0–360° (exclusive of exactly 360, wraps to 0) | Character body's yaw state at the start of this frame, held by Camera System |
| Incoming yaw delta | yaw_delta | float | unbounded per frame (typically small) | The yaw-axis (horizontal) component of `mouse_look_rotation` or `gamepad_look_rotation` |
| New accumulated yaw | yaw_accumulated_new | float | 0–360° | Yaw value applied to the character body's local rotation (Y-axis) this frame |

**Output Range:** Wrapped (not clamped) to [0°, 360°) — unlike pitch, yaw has no physical limit, so the design goal is to prevent the underlying float from growing unbounded over a long play session (float precision loss after many hours) rather than to restrict player rotation. The double-mod expression is required because C#'s `%` operator returns negative results for negative operands (e.g., `-10 % 360 == -10` in C#, not `350`); the `+ 360` before the second `mod 360` corrects this.
**Example:** yaw_accumulated_prev = 358°, yaw_delta = +5° → raw sum = 363° → 363 mod 360 = 3° → result = 3°. Reverse case: yaw_accumulated_prev = 2°, yaw_delta = -5° → raw sum = -3° → ((-3 mod 360) + 360) mod 360 = (-3 + 360) mod 360 = 357°.

### fov_kick_response

The `fov_kick_response` formula is defined as:
`current_fov_offset = kick_peak_offset_deg * kick_envelope(t_since_trigger)`

where the envelope is a two-phase linear attack/decay (implementable as a single timer float in `Update`, no coroutine required):

`kick_envelope(t) = t / kick_attack_time,                                    for 0 ≤ t < kick_attack_time`
`kick_envelope(t) = 1 - (t - kick_attack_time) / kick_decay_time,            for kick_attack_time ≤ t < kick_attack_time + kick_decay_time`
`kick_envelope(t) = 0,                                                       for t ≥ kick_attack_time + kick_decay_time`

| Variable | Symbol | Type | Range | Description |
| ---- | ---- | ---- | ---- | ---- |
| Time since cast-kick triggered | t_since_trigger | float | 0–unbounded (seconds) | Timer Camera System increments in `Update` after `CastSpell` fires; reset to 0 on trigger |
| Peak FOV offset | kick_peak_offset_deg | float | tuned, ~2–8° | Max degrees the FOV widens above `base_fov` at the peak of the kick |
| Attack time | kick_attack_time | float | tuned, ~0.03–0.08s | Time from trigger to peak offset (fast — reads as a snap, not a build-up) |
| Decay time | kick_decay_time | float | tuned, ~0.15–0.30s | Time from peak back down to `base_fov` |
| Envelope multiplier | kick_envelope(t) | float | 0.0–1.0 | Normalized 0→1→0 shape applied to the peak offset |
| Resulting FOV offset | current_fov_offset | float | 0–kick_peak_offset_deg | Degrees added to `base_fov` this frame |

**Output Range:** `current_fov_offset` is bounded to [0, kick_peak_offset_deg] by construction. Once `t_since_trigger` passes `kick_attack_time + kick_decay_time`, the offset is exactly 0 (fully returned to `base_fov`, not asymptotic). If a new cast triggers while a previous kick is still decaying, Camera System resets `t_since_trigger` to 0 immediately (re-trigger, not additive stacking) — this prevents the offset from exceeding `kick_peak_offset_deg` under rapid-fire casting.
**Example:** kick_peak_offset_deg = 6°, kick_attack_time = 0.05s, kick_decay_time = 0.20s. At t = 0.025s: envelope = 0.5 → offset = 3°. At t = 0.05s (peak): envelope = 1.0 → offset = 6°. At t = 0.15s: envelope = 0.5 → offset = 3°. At t ≥ 0.25s: envelope = 0 → offset = 0°.

### current_fov (composition)

The `current_fov` formula is defined as:
`current_fov = base_fov + current_fov_offset + sprint_fov_offset`

| Variable | Symbol | Type | Range | Description |
| ---- | ---- | ---- | ---- | ---- |
| Base FOV | base_fov | float | tuned, ~70–100° | Tuning-knob default field of view |
| Cast-kick offset | current_fov_offset | float | 0–kick_peak_offset_deg | Output of `fov_kick_response` above |
| Sprint offset | sprint_fov_offset | float | placeholder — always 0 for now | Deferred; sprint mechanic and its FOV modifier are not yet designed. Camera System still sums this term (rather than omitting it) so wiring in a future sprint-FOV formula doesn't require restructuring the FOV pipeline |
| Final FOV applied | current_fov | float | base_fov to base_fov + kick_peak_offset_deg (sprint term TBD) | Value written to the camera lens this frame |

**Output Range:** Bounded to `[base_fov, base_fov + kick_peak_offset_deg]` until sprint is designed, since `sprint_fov_offset` is hardcoded to 0. This range is **not safe to hardcode a hard max on** once sprint is designed — revisit then.
**Example:** base_fov = 90°, current_fov_offset = 3° (mid-kick), sprint_fov_offset = 0 (placeholder) → current_fov = 93°.

**Note on camera shake**: per Core Rule 6, shake is implemented via a Cinemachine Impulse Definition asset (tuned in the Unity Inspector, not a GDD formula) — other systems call `GenerateImpulse()` and Camera System's `CinemachineImpulseListener` handles the response curve internally. No custom shake-decay formula is needed here.

*Formulas proposed by `systems-designer` (2026-08-26).*

## Edge Cases

- **If pitch is at ±89° and further same-direction input arrives**: `pitch_accumulation`'s clamp holds the value at ±89° exactly; no overshoot, no bounce-back.
- **If the Gameplay map is disabled (UI map active, e.g. pause/alt-tab)**: per `input-system.md`, `Look` produces no further deltas while the UI map is active, so Camera System receives zero pitch/yaw delta and holds its last rotation exactly — no phantom rotation accumulates from input received while a menu is open.
- **If the local player dies while an FOV cast-kick is mid-decay**: Camera System immediately resets `t_since_trigger` to a value past the kick's total duration (forcing `current_fov_offset` to 0) on the Active→Spectator transition, rather than letting the kick continue decaying on a camera that's about to re-parent. Spectator view always starts at a clean `base_fov`.
- **If `CastSpell` fires again while a previous FOV kick is still decaying**: per the formula's re-trigger rule, `t_since_trigger` resets to 0 immediately — kicks do not stack additively, preventing FOV from exceeding `kick_peak_offset_deg` under rapid-fire casting.
- **If `SetSpectatorTarget(null)` is called** (e.g. the teammate being spectated also dies or disconnects before Spectator/Death System assigns a new target): Camera System holds its pivot at its last-known world position/rotation rather than throwing or snapping to the origin. Selecting the *next* valid target is Spectator/Death System's responsibility, not Camera System's.
- **If two Cinemachine impulses fire in the same frame** (e.g. player is hit by damage and casts a spell simultaneously): Cinemachine's Impulse Manager natively sums concurrent impulses — Camera System does not need custom blending logic; this is delegated to the engine's built-in impulse-summation behavior.
- **Cross-client rotation visibility is explicitly out of scope here**: how other players see this player's body/head orientation over the network (interpolation, latency compensation) belongs to Player Controller / Networking Foundation, not Camera System. Camera System only guarantees the *local* pitch/yaw values are correct.

## Dependencies

**Depends On** (hard — cannot function without this system):
- **Input System** ✅ (designed) — consumes `Look` (mouse/gamepad deltas already sensitivity/deadzone/curve-processed via `mouse_look_rotation`/`gamepad_look_rotation`)

**Depended On By**:
- **Player Controller** (not yet designed) — **hard**. Camera System writes the character body's yaw rotation (Core Rule 1); Player Controller reads that same transform's forward vector to resolve `Move` direction. Neither system can be implemented correctly in isolation from the other's interface.
- **Spell Casting System** (not yet designed) — **hard**. Reads the camera pivot's forward vector as the spell-aim raycast direction; without it, there is no aim direction to cast along.
- **Target Feedback System** (not yet designed) — **hard**. Reads the camera pivot's forward vector to compute the currently-aimed-at enemy (this is the exact requirement `input-system.md` flagged from the prototype).
- **Health & Damage System** (not yet designed) — **soft**. Calls `GenerateImpulse()` on taking damage for camera shake feedback; Health & Damage functions correctly without it, shake is feel-polish, not a hard requirement.
- **Spectator/Death System** (not yet designed) — **hard**. Calls `SetSpectatorTarget()` to drive the death→spectator camera transition; without this hook, death has no camera behavior at all.

*Cross-reference: `input-system.md`'s Dependencies section already lists Camera
System under "Depended On By" — this is bidirectionally consistent.*

## Tuning Knobs

| Knob | Range | Default | Effect | Too Low | Too High |
|---|---|---|---|---|---|
| `base_fov` | 70–100° | 90° | Baseline field of view | Tunnel vision, harder to track flanking teammates/enemies | Fisheye distortion, harder to judge distances for aiming |
| `eye_height_offset` | 1.4–1.8m | 1.65m | Camera pivot's vertical offset from character root | Camera clips into torso/ground on uneven terrain | Camera floats visibly above the character model |
| `kick_peak_offset_deg` | 2–8° | 5° | Max FOV widen on cast | Cast feels unfeedbacked/flat | Disorienting, especially during rapid slot-cycling + cast spam |
| `kick_attack_time` | 0.03–0.08s | 0.05s | Speed of FOV snap-out on cast | Feels sluggish, more like a fade than a snap | Can feel like a visual glitch/pop if near-instant |
| `kick_decay_time` | 0.15–0.30s | 0.20s | Speed of FOV return to baseline | Kick feels like it "sticks" too long, muddies rapid casting | Kick barely registers before it's gone |

All knobs are exposed to designers as serialized fields (Inspector-tunable), not
hardcoded constants — consistent with the project's data-driven coding
standard. `base_fov` is additionally player-facing (accessibility/comfort —
some players prefer wider or narrower default FOV) and should be exposed in
the Settings menu alongside `sensitivity` from `input-system.md`.

*Note: the ±89° pitch clamp is intentionally **not** a tuning knob — it's a
fixed engineering constant tied to avoiding gimbal-lock/degenerate look
vectors at the poles, not a feel/balance parameter designers should adjust
per-playtest.*

## Visual/Audio Requirements

- **FOV kick** on `CastSpell` (per `fov_kick_response`) is the primary visual
  signature of this system — it must read as a fast, punchy "snap," not a
  smooth zoom. Attack time is deliberately much shorter than decay time to
  achieve this asymmetry.
- **Camera shake** on damage taken / spell impact is delivered via Cinemachine
  Impulse Definition assets (tuned in-Inspector per event type — a hit from a
  small enemy should shake less than a boss slam). Exact per-event amplitude/
  duration values are an implementation/tuning task, not specified numerically
  in this GDD (see Acceptance Criteria note on the Shake testability gap).
- **No audio requirements** — Camera System produces no sound of its own; any
  audio tied to cast/impact events belongs to Audio System, triggered by the
  same events that drive the Cinemachine impulses, not by Camera System.
- Death→Spectator transition is an **instant cut**, not a fade or blend (per
  Core Rule 7) — no visual transition effect (e.g. screen fade to black) is
  Camera System's responsibility; if the game wants a death-fade, that belongs
  to Spectator/Death System or Session UI layered on top.

📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible
is approved, run `/asset-spec system:camera-system` to produce per-asset
visual descriptions (e.g. Cinemachine Impulse Definition presets) from this
section.

## UI Requirements

- `base_fov` must be exposed in the Settings/Accessibility menu alongside
  `sensitivity` from `input-system.md` — some players need a wider or
  narrower default FOV for comfort (motion sensitivity).
- No other UI is owned by Camera System. Any on-screen reticle/crosshair
  belongs to **Target Feedback System** (not yet designed) — Camera System
  only supplies the forward vector that system needs, not the HUD element
  itself.

> **📌 UX Flag — Camera System**: This system has a UI requirement (FOV
> setting in the Settings/Accessibility menu). In Phase 4 (Pre-Production),
> run `/ux-design` for the Settings screen before writing epics — the same
> flag already exists from `input-system.md` for `sensitivity`; this adds
> `base_fov` to that same screen rather than creating a new one.

## Acceptance Criteria

*All criteria below are independently verifiable by a QA tester with no other
context than this section plus a debug readout showing `pitch_accumulated`,
`yaw_accumulated`, `current_fov_offset`, and `current_fov`.*

**Pitch / Yaw** (Core Rules 1–4; `pitch_accumulation`, `yaw_accumulation`)

1. GIVEN the player character is spawned and idle, WHEN a horizontal `Look` input is received, THEN the character body's transform rotates on its Y-axis (yaw) and the body's local pitch (X-axis rotation) remains exactly 0°.
2. GIVEN the player character is spawned and idle, WHEN a vertical `Look` input is received, THEN only the camera pivot (child transform at the eye-height offset) rotates in pitch, and the character body's Y-axis (yaw) rotation does not change.
3. GIVEN `pitch_accumulated_prev = 87°`, WHEN a frame delivers `pitch_delta = +5°`, THEN `pitch_accumulated_new = 89°` (not 92°) — the clamp fires before the value is applied to the pivot.
4. GIVEN `pitch_accumulated` is holding at exactly `89°`, WHEN three further consecutive frames each deliver a positive (look-up) `pitch_delta`, THEN `pitch_accumulated` reads exactly `89°` on every one of those frames — no overshoot past 89° and no bounce/oscillation below it.
5. GIVEN `yaw_accumulated_prev = 358°`, WHEN a frame delivers `yaw_delta = +5°`, THEN `yaw_accumulated_new = 3°` (confirms wrap, not clamp).
6. GIVEN `yaw_accumulated_prev = 2°`, WHEN a frame delivers `yaw_delta = -5°`, THEN `yaw_accumulated_new = 357°` (confirms the negative-wrap case, not a negative or NaN result).
7. GIVEN the Gameplay input map is active and the player is mid-rotation, WHEN the UI input map becomes active (pause menu / alt-tab), THEN `Look` delivers zero delta every frame the UI map is active, and both `pitch_accumulated` and `yaw_accumulated` remain frozen at their pre-pause values for the entire duration the menu is open.
8. GIVEN the pause menu from criterion 7 is then closed (Gameplay map re-activated), WHEN the tester resumes moving the mouse, THEN rotation resumes smoothly from the exact pre-pause `pitch_accumulated`/`yaw_accumulated` values with no snap or jump.

**FOV Kick** (Core Rule 5; `fov_kick_response`, `current_fov` composition)

9. GIVEN default tuning values (`kick_peak_offset_deg = 6°`, `kick_attack_time = 0.05s`, `kick_decay_time = 0.20s`) and `base_fov = 90°`, WHEN `CastSpell` fires at `t = 0`, THEN at `t = 0.025s` the debug readout shows `current_fov_offset = 3°` and `current_fov = 93°`.
10. GIVEN the same setup as criterion 9, WHEN time reaches `t = 0.05s` (end of attack phase), THEN `current_fov_offset` reads its peak value of exactly `6°` (`current_fov = 96°`).
11. GIVEN the same setup, WHEN time reaches `t = 0.15s` (mid-decay), THEN `current_fov_offset = 3°`.
12. GIVEN the same setup, WHEN time reaches `t = 0.25s` or later, THEN `current_fov_offset` reads exactly `0°` (`current_fov = base_fov = 90°`) — not asymptotically approaching zero.
13. GIVEN `CastSpell` fires and the kick is mid-decay (e.g. `t_since_trigger = 0.15s`, `current_fov_offset = 3°`), WHEN `CastSpell` fires again before decay completes, THEN `t_since_trigger` resets to `0` immediately and `current_fov_offset` begins climbing from its attack-phase curve again — it must NOT exceed `kick_peak_offset_deg` (6°) and must NOT show two kicks' offsets summed.
14. GIVEN no `CastSpell` has fired and no sprint mechanic exists yet, WHEN the tester inspects `current_fov` at rest, THEN it reads exactly `base_fov` (e.g. `90°`) — confirming `sprint_fov_offset` contributes `0` and does not throw/uninitialize.

**Shake / Impulse Wiring** (Core Rule 6; edge case — concurrent impulses)

15. GIVEN a test harness (or Health & Damage / Spell Casting, whichever exists first) calls `GenerateImpulse()` on a `CinemachineImpulseSource`, WHEN the impulse fires, THEN the camera (via its `CinemachineImpulseListener`) visibly displaces/shakes without any direct method call or reference from Camera System's own code to the emitting system (verify by code inspection: no `SpellCasting`/`HealthAndDamage` namespace imports in the camera script).
16. GIVEN two impulse sources (e.g. one from taking damage, one from casting) both call `GenerateImpulse()` in the same frame, WHEN the tester observes the resulting camera shake, THEN a single combined shake response is visible (not two sequential or conflicting shakes), with no custom blending code present in Camera System (verify by code inspection).

*Shake **amplitude, duration, and falloff quality** are intentionally not
GWT-testable here — Core Rule 6 defers the actual response curve to a
Cinemachine Impulse Definition asset tuned in the Inspector, and no numeric
target exists in this GDD for those properties by design. Per the project's
Visual/Feel test-evidence tier, shake feel is validated by screenshot/video +
lead sign-off (ADVISORY gate), not a BLOCKING Logic criterion.*

**Death / Spectator Transition** (Core Rule 7; edge cases — death mid-kick-decay, null target)

17. GIVEN the local player is alive, WHEN the local player dies, THEN the camera pivot detaches from the dead body's transform (no longer a child of / following the dead body) within the same frame death is registered.
18. GIVEN a cast-kick is mid-decay (`t_since_trigger = 0.10s`, `current_fov_offset > 0`) at the moment of death, WHEN the Active→Spectator transition completes, THEN `current_fov_offset` reads exactly `0°` and `current_fov = base_fov` immediately upon entering Spectator state — it does not continue decaying on-screen after the transition.
19. GIVEN the camera has entered Spectator state, WHEN `SetSpectatorTarget(someLivingTeammateTransform)` is called, THEN the camera pivot instantly re-parents/attaches to that transform in the same frame — no blend, no interpolation, no null reference exception. (Instant snap is the specified behavior, not a defect — see Core Rule 7.)
20. GIVEN the camera is in Spectator state following a valid target, WHEN `SetSpectatorTarget(null)` is called, THEN the camera pivot holds its last-known world position and rotation exactly (no drift, no snap to world origin `(0,0,0)`, no exception thrown).
21. GIVEN the local player is in Spectator state, WHEN the local player respawns/reaches a checkpoint, THEN the camera transitions from Spectator back to Active state and re-parents to the (new) body's pivot instantly.

**Performance**

22. GIVEN the game is running at the 60fps/16.6ms frame budget, WHEN Camera System's per-frame `Update` cost is profiled, THEN it produces zero per-frame GC allocations (no LINQ, no coroutines, no boxing) and its own cost stays well under 0.05ms — a regression above that threshold on a system this simple (pure scalar math) should trip a smoke-test/profiler alarm.

*Note: `qa-lead` consulted (2026-08-26) — proposed all 22 criteria above and
identified two testability gaps in the original Core Rules (shake
amplitude/duration, and "smooth" re-parenting with no defined threshold).
Both were resolved before this section was finalized: shake amplitude/duration
is explicitly scoped to Visual/Feel evidence (not a GWT criterion, by design,
not an oversight); "smooth" re-parenting was resolved to an instant, blend-free
snap (Core Rule 7 updated accordingly), making criterion 19 fully testable.*

## Open Questions

- **Spectator target selection logic** is entirely deferred to Spectator/Death
  System (not yet designed) — Camera System only exposes the hook. Who gets
  spectated first, how switching between living teammates works, and what
  happens if the whole team is dead simultaneously (wipe→checkpoint) all need
  resolution there (owner: game-designer; target: when Spectator/Death System
  is designed, next in MVP Feature-tier order).
- **Sprint mechanic and `sprint_fov_offset`** are explicitly placeholder-zero
  in the `current_fov` formula. If Player Controller GDD introduces sprint,
  this GDD will need a follow-up formula for that term (owner:
  gameplay-programmer; target: when Player Controller is designed).
- **Eye-height offset per-character variation**: `eye_height_offset` is
  currently a single global tuning knob. If different mage archetypes/models
  have visibly different heights later, this may need to become per-character-
  model rather than global (owner: art-director / gameplay-programmer;
  target: before character art is finalized).
- **Cinemachine Impulse Definition per-event tuning values** (shake amplitude/
  duration per damage type, per spell school) are unspecified numerically by
  design (see Acceptance Criteria note) — need playtesting once Health &
  Damage and Spell Casting systems exist (owner: technical-artist; target:
  Vertical Slice).
