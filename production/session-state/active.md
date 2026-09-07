# Session State — Covenant of Mages

*Last updated: 2026-09-07*

## User Preferences (durable — apply every session)

- Respond in Russian.
- Deliver step-by-step instructions (setup guides, how-tos) directly in chat — do NOT create separate instructional files in the repo (e.g. no more SETUP.md-style docs). Code/config files that are part of the actual deliverable are fine; user-facing walkthroughs are not.

---

## COMPLETE: /brainstorm — Covenant of Mages concept written

- **Status**: DONE. `design/gdd/game-concept.md` fully rewritten (2026-08-26).
  This is a **full replacement** of the prior "Hollow Vow" single-player concept.
- **New concept**: Co-op session-based first-person dungeon crawler, 4-5 players,
  all mages with elemental spell schools (fire, water, lightning, nature, light, dark, air, etc.),
  class roles (damage, tank, support, debuffer). 25-30 min sessions.
  Cross-player elemental synergies are the core mechanic.
- **Pillars**: Магия лучше вместе · Каждая смерть — история · Быстрый вход глубокая глубина · Подземелье — живой противник
- **Death mechanic**: dead = spectator until fight ends (no in-fight revive)
- **Dungeon structure**: hand-crafted base + randomized enemies/loot
- **Visual**: pixel-art 3D + grain in dungeons (retained from Hollow Vow concept)
- **Engine**: Unity 6.3 LTS (already configured — still valid)
- **MVP**: 1 dungeon, 3 magic schools (fire/water/lightning), 2-3 synergies, co-op 2-4 players

## SUPERSEDED / REQUIRES REVIEW: Old Hollow Vow files

The following files were written for the old single-player concept and need to be
either archived or replaced before design work continues:

- **`design/gdd/systems-index.md`** — FULLY INVALID for new concept. Must be regenerated
  with `/map-systems`. New critical system: Networking (now the highest-risk bottleneck).
- **`design/gdd/input-system.md`** — ✅ RESOLVED (retrofit completed 2026-08-26).
  See "COMPLETE: Input System Retrofit" section below.
- **`design/registry/entities.yaml`** — constants/formulas from input-system.md
  (sensitivity, base_scale, deadzone, look_speed_deg_per_sec) confirmed unchanged
  and still accurate after the retrofit.

## COMPLETE: ADR-0001 — Networking Stack

- **Status**: DONE. `docs/architecture/adr-0001-networking-stack.md` written (2026-08-26). Status: **Accepted** (2026-08-26) — programming may proceed against this decision.
- **Decision**: Unity Netcode for GameObjects (NGO) + Unity Transport 2.x + Unity Relay
- **Topology**: Listen-server (host-client). Host disconnect = session ends (documented design decision).
- **Key interfaces**: `NetworkVariable<ElementalStatusFlags>`, `CastSpellServerRpc`, `TriggerSynergyClientRpc` (uses `NetworkObjectReference`), `NetworkVariable<bool> _isDead`
- **Registry updated**: 3 стансии в `docs/registry/architecture.yaml` (api_decision, state_ownership, forbidden_pattern)
- **Validated by**: unity-specialist (MINOR NOTES только — внесены в ADR)

## COMPLETE: Prototype — Co-op Spellcasting

- **Concept**: `prototypes/co-op-spellcasting-concept/`
- **Verdict**: **PROCEED (with a required follow-up)** — full detail in
  `prototypes/co-op-spellcasting-concept/REPORT.md`, indexed in `prototypes/index.md`.
- **Hypothesis**: Cross-player elemental synergy (Water soaks a target, Lightning
  triggers 3x-damage Chain Shock that jumps to nearby enemies) discovered independently
  by two players in real time feels spontaneous and fun.
- **Result**: Mechanic confirmed technically correct after two bug/behavior fixes
  (see below). Core hypothesis about *two independent players* spontaneously
  discovering the combo was **not exercised** — testing this session was solo only.
  Tester's solo read: mechanic is conceptually interesting.
- **Bugs found & fixed**:
  1. `EnemyDummy.Die()` deactivated the whole GameObject before starting the respawn
     coroutine — Unity can't run a coroutine on an inactive object, so dummies never
     respawned. Fixed by hiding renderer/collider/label instead of the whole object.
  2. Auto-targeting (`FindNearestEnemy()`) is called independently per cast, so Water
     and Lightning can silently land on different dummies if the player moves between
     casts — this breaks the combo with no error/feedback. Diagnosed via a `[TARGET]`
     debug log; not a code bug, but flagged as a **real production risk**: production
     needs visible target feedback (reticle/highlight) so players can reliably set up
     synergies on purpose. Carry this into `input-system.md` / combat GDD.
- **Required follow-up**: run a real 2-player (or async 2-session) test of this same
  build before treating the co-op-discovery hypothesis as validated.
- **Files**: `Assets/Scripts/*.cs` + `Editor/CoOpSpellcastingSceneSetup.cs` +
  `README.md` + `REPORT.md` (all complete). Live-project copies also placed in
  root `Assets/Scripts/` for testing (coexists with `core-combat-concept` scripts —
  no naming conflicts).

## COMPLETE: Input System Retrofit

- **Status**: DONE (2026-08-26). Chose **retrofit** over rewrite — ~80% of the
  document (all 3 formulas, all 6 edge cases, tuning knobs, player fantasy) was
  still valid; only 5 targeted edits were needed.
- **Spell slot control scheme decided**: "Active slot + cast" — `CastSpell`
  (LMB / gamepad South) casts the currently active slot; `ScrollSpell` (mouse
  wheel + Q + gamepad LB/RB) cycles the active slot 1→2→3→1. Chosen over
  direct 3-button binding and numeric-key binding.
- **Edits applied**: (1) Overview — Combat System → Spell Casting System,
  "responsive combat" → "responsive spellcasting"; (2) Core Rules — `Attack`
  replaced with `CastSpell`/`ScrollSpell`, combo-buffer language replaced with
  cooldown/slot/synergy language; (3) Interactions — renamed Combat System →
  Spell Casting System, added **Target Feedback System** as a provisional
  dependency (carries the prototype's "visible current target" requirement:
  Input `Look` → Camera System → Target Feedback System), added an explicit
  ADR-0001 networking-boundary note (Input System is client-side only,
  Spell Casting System owns the `CastSpellServerRpc` call); (4) Dependencies —
  same renames + Target Feedback System added; (5) Acceptance Criteria —
  updated criterion #3, added 4 new criteria (3a–3d) for CastSpell/ScrollSpell.
- **Registry**: no changes needed — all 3 formulas and 5 constants in
  `design/registry/entities.yaml` were already accurate and untouched.
- **`design/gdd/systems-index.md`** updated: Input System status → "Designed
  (retrofitted 2026-08-26)", Next Steps checklist items for ADR-0001 and the
  retrofit decision both checked off, Progress Tracker MVP count → 1/28.

## COMPLETE: Camera System GDD

- **Status**: DONE (2026-08-26). All 8 required sections + Visual/Audio, UI
  Requirements, Open Questions written to `design/gdd/camera-system.md`.
  Status header: "Designed (pending review)".
- **Confirmed constraint**: pure first-person camera (FPS aiming), no
  third-person orbit rig — per game-concept.md Technical Considerations table.
- **Key design decisions**:
  - Body owns yaw, camera pivot (child, at eye height) owns pitch only —
    avoids gimbal issues, keeps `Move` direction sane at any look angle.
  - Camera System is the accumulator/clamp owner for pitch (±89°) and yaw
    (0–360° wrap) — Input System only supplies stateless per-frame deltas.
  - Camera shake and FOV kick driven by Cinemachine Impulse
    (`CinemachineImpulseSource`/`Listener`) — other systems trigger shake
    without any reference to Camera System's internals.
  - Death→Spectator transition is an **instant snap** (no blend) — resolved
    this way specifically to keep it GWT-testable (qa-lead flagged "smooth"
    as an undefined threshold; instant snap removed the ambiguity).
  - Spectator *target selection* logic is explicitly out of scope — deferred
    to the not-yet-designed Spectator/Death System; Camera System only
    exposes `SetSpectatorTarget(Transform)`.
- **Formulas** (4, all registered in `design/registry/entities.yaml`):
  `pitch_accumulation`, `yaw_accumulation`, `fov_kick_response`, `current_fov`.
  Proposed by `systems-designer` agent.
- **Acceptance Criteria**: 22 GIVEN-WHEN-THEN criteria, proposed and validated
  by `qa-lead` agent. Two testability gaps found and resolved before writing:
  shake amplitude/duration scoped to Visual/Feel evidence (not GWT, by
  design); "smooth" re-parenting resolved to instant snap (see above).
- **Registry updates**: 4 new formulas + 5 new constants (`base_fov`,
  `eye_height_offset`, `kick_peak_offset_deg`, `kick_attack_time`,
  `kick_decay_time`) added to `design/registry/entities.yaml`; `input-system.md`'s
  `mouse_look_rotation`/`gamepad_look_rotation` entries updated with
  `camera-system.md` in `referenced_by`.
- **Open Questions carried forward**: spectator target-selection (owner:
  Spectator/Death System GDD), sprint FOV offset (owner: Player Controller
  GDD), per-character eye height, Cinemachine Impulse per-event tuning values
  (owner: technical-artist, target: Vertical Slice).
- **Systems index updated**: status → "Designed (pending review, 2026-08-26)",
  Progress Tracker → 2/28 MVP systems designed.
- **Not yet run**: `/design-review design/gdd/camera-system.md` — must be run
  in a fresh session (never in the authoring session).

## COMPLETE: Health & Damage System GDD

- **Status**: DONE (2026-08-26). All 8 required sections + Visual/Audio
  (mandatory for this category, done via `art-director`), UI Requirements,
  Open Questions written to `design/gdd/health-damage-system.md`. Status
  header: "Designed (pending review)".
- **Bottleneck-first order (user request)**: Health & Damage ✅ →
  Networking Foundation → Elemental Status → Spell Casting → Target Feedback
  → Elemental Synergy. Dungeon Structure System deferred (its own
  dependencies — Enemy AI, Player Controller, Checkpoint System — aren't
  bottleneck-flagged and need separate resolution first).
- **Key design decisions**:
  - `currentHP` follows the exact `NetworkVariable<bool> _isDead` pattern
    ADR-0001 already established — server-authoritative, clients read-only.
  - `maxHP` is explicitly NOT owned by this system — external data from
    whichever system spawns the entity (Enemy AI / Player Controller /
    Character Progression), matching the game concept's "difficulty via
    enemy combination, not raw HP scaling" intent.
  - `ApplyHeal` included in MVP scope now (even though no healer school
    exists at MVP) so the API shape doesn't need to change later.
  - Defense/mitigation formula **explicitly deferred** — documented
    multiplicative extension point (`mitigation_multiplier`, default 1.0),
    not implemented, since no equipment/defense stat spec exists yet.
  - `OnDeath` fires exactly once per entity; enemies despawn via
    `NetworkObject.Despawn()` (never `Destroy()`); players mirror
    ADR-0001's `_isDead`. This system doesn't own despawn timing (Enemy AI)
    or the death→spectator camera transition (already locked in
    `camera-system.md`).
  - Disconnection ≠ death — `IsDead` means "reached 0 HP," not "left the
    session" (Networking Foundation's concern).
  - This system has **zero tuning knobs of its own** — every balance-
    relevant number (maxHP, synergy multipliers, mitigation, DoT cadence) is
    deliberately owned by other systems. Recorded as a deliberate outcome,
    not a gap.
- **Formulas** (2, registered): `synergy_damage_multiplier`, `hp_clamp`.
  Proposed by `systems-designer`. The "3.0" Chain Shock reference value is
  explicitly NOT locked — passthrough example only, pending Elemental
  Synergy System.
- **Acceptance Criteria**: 21 GIVEN-WHEN-THEN criteria from `qa-lead`. Two
  gaps found and resolved: mitigation formula has no criterion (nothing to
  test — unimplemented by design); `NetworkVariable<float> currentHP`
  replication bandwidth under frequent DoT ticks has no defined budget —
  accepted as a documented risk (Open Questions), deferred to Networking
  Foundation GDD (next in the bottleneck-first order).
- **Visual/Audio** (mandatory for Combat/damage/health category) — proposed
  by `art-director`: dual-layer hit feedback (visual + audio always
  together), synergy hits get an *additive* accent layer (never a palette
  swap), damage numbers are secondary/reinforcement not primary signal,
  player death respects `camera-system.md`'s instant-cut (no fade/vignette
  layered on top). Clear provisional-vs-locked split pending `art-bible.md`.
- **Registry updates**: 2 new formulas added to
  `design/registry/entities.yaml`. No new constants (the 3.0 reference value
  deliberately NOT registered as authoritative per specialist recommendation).
- **Not yet run**: `/design-review design/gdd/health-damage-system.md` —
  must run in a fresh session.

## PIVOT: Documentation → Implementation (2026-08-26)

- User explicitly paused the full `/map-systems` GDD-first pipeline (27 of 30
  MVP systems still undesigned) to start building a playable MVP directly.
- **Chosen approach**: build on top of the already-validated
  `prototypes/co-op-spellcasting-concept/` code, evolving it toward the real
  MVP in the live Unity project (`Assets/Scripts/`). GDDs/ADRs/quick-designs
  are now written **reactively** — only when an actual architectural decision
  needs one — not proactively ahead of code.
- **First implementation slice (this session)**: production code for the two
  systems that already have approved GDDs, in `Assets/Scripts/Core/`:
  - `Input/PlayerInputRouter.cs` — implements `input-system.md`'s Gameplay/UI
    action maps in code (no `.inputactions` asset — built via
    `InputActionMap` API directly to avoid hand-authoring risky JSON).
  - `Camera/PlayerCameraController.cs` — implements `camera-system.md`
    (body=yaw/camera=pitch split, pitch clamp ±89°, yaw wrap, FOV kick
    envelope, spectator instant-snap hook). No hard Cinemachine dependency in
    code — shake is wired via a `CinemachineImpulseListener` component added
    in the Inspector, not referenced in this script.
  - `Combat/HealthDamage.cs` — implements `health-damage-system.md`
    (server-authoritative `NetworkVariable<float> currentHP` /
    `NetworkVariable<bool> IsDead` mirroring ADR-0001's exact pattern,
    `ApplyDamage`/`ApplyHeal`/`Revive`, `synergy_damage_multiplier` +
    `hp_clamp` formulas, `OnDeath` event). Requires Netcode for GameObjects
    package (not yet installed in this project).
- **Packages the user still needs to install** via Package Manager (Unity
  Registry) before these compile cleanly:
  1. Netcode for GameObjects (`com.unity.netcode.gameobjects`)
  2. Cinemachine (`com.unity.cinemachine`, 3.x) — only needed for the shake
     Impulse Source/Listener components, not referenced directly in code yet
- **Not yet built**: Spell Casting, Elemental Status, Elemental Synergy,
  Target Feedback, Networking Foundation wiring (NetworkManager setup, actual
  multiplayer test) — these don't have GDDs yet; next slice should tackle
  Networking Foundation (bottleneck-first order) enough to get a
  NetworkManager + host/client test working, since HealthDamage.cs already
  assumes NGO is present.
- **None of this session's new scripts are committed yet** — awaiting user
  test/compile pass in Unity before committing, per the established
  Engine-path multi-turn loop (write → user runs → reports errors → iterate).

## IN PROGRESS: MVP network test scene — mouse-look and cast not working

- **Status**: Scene built via `Tools/Covenant of Mages/Build MVP Network Test
  Scene`, NetworkManager configured, `Assets/Prefabs/Player.prefab` created
  and manually assigned to NetworkManager's **Default Player Prefab** field
  (note: the field is labeled "Default Player Prefab" in this Netcode
  version, not "Player Prefab" as I initially told the user — corrected
  in-session). Current scene added to Build Settings (required by Netcode's
  Scene Management when prompted on first Play).
- **Reported bugs (2026-08-30, end of session)**:
  1. Mouse does not rotate the camera.
  2. Left-click does nothing (test-cast raycast never seems to fire/hit).
- **Fix already applied this session**: `PlayerBootstrap.cs` was missing
  cursor lock entirely — `input-system.md`'s Edge Cases explicitly require
  `CursorLockMode.Locked` + hidden cursor for the whole Gameplay state, and
  I never implemented it in the first pass. Added `Cursor.lockState =
  CursorLockMode.Locked; Cursor.visible = false;` in `OnNetworkSpawn` (owner
  only), with the reverse in `OnNetworkDespawn`. **This has NOT been tested
  yet** — session ended before the user could re-test in Play mode.
- **Ruled out**: `ProjectSettings/ProjectSettings.asset` confirms
  `activeInputHandler: 2` (Both old + new Input System enabled) — this is
  NOT the cause.
- **Not yet checked (diagnose first, before writing more code)** — ask the
  user these in order next session:
  1. Did cursor-lock fix change anything? (test this first — most likely
     single root cause for bug #1, and would explain #2 too if the Game
     View never had reliable focus/input to begin with)
  2. After pressing Play, was **"Start Host"** actually clicked (NetworkManager's
     default runtime debug UI, bottom-left of Game view)? If not clicked, no
     player object ever spawns and nothing will respond — this alone would
     explain both bugs.
  3. Does a `Player(Clone)` object appear in the Hierarchy after Start Host?
     If yes, check its `NetworkObject` component — is `IsOwner` effectively
     true for the host's own instance? (Select it in Play mode, or add a
     temporary debug log in `PlayerBootstrap.OnNetworkSpawn`.)
  4. Is the `PlayerCamera` child active and is it the one rendering (check
     Game view — do you see through the capsule's eyes, or still the Scene
     view / no camera at all)?
  5. Any red errors in Console at the moment of clicking Start Host or at
     the moment of the failed click/mouse-move?
  6. Did the user click once inside the Game View window before trying to
     mouse-look? (Editor Game View sometimes needs a focus-click first,
     independent of cursor lock.)
- **Files involved**: `Assets/Scripts/Core/Player/PlayerBootstrap.cs` (fixed
  this session, untested), `Assets/Scripts/Core/Input/PlayerInputRouter.cs`,
  `Assets/Scripts/Core/Camera/PlayerCameraController.cs`,
  `Assets/Scripts/Editor/MvpNetworkTestSceneSetup.cs`,
  `Assets/Prefabs/Player.prefab`.
- **Earlier fix this session**: `PlayerInputRouter.cs` had 3× compile errors
  (CS1739 — `AddAction`'s parameter is named `expectedControlLayout`, not
  `expectedControlType`, in the installed Input System version). Fixed, and
  the project compiled clean after that (confirmed by user, exited Safe Mode
  automatically).
- **Packages installed this session** (confirmed working): Netcode for
  GameObjects, Cinemachine.
- **Nothing from this MVP-implementation slice is committed to git yet** —
  still mid-debug. Do not commit until mouse-look + test-cast are confirmed
  working end-to-end.

## IN PROGRESS: Web MVP prototype (browser, friends can join)

- **User pivot (2026-08-30)**: instead of continuing the Unity network-test
  debugging, build a playable MVP prototype that opens in the browser and
  lets friends join over the internet.
- **Built**: `prototypes/web-mvp-concept/prototype.html` — single-file HTML,
  Three.js (r149, CDN) + PeerJS (1.5.2, CDN, WebRTC P2P). Host creates a
  4-letter room code; friends open the same file and enter the code. No
  server. Host-authoritative combat (ADR-0001 listen-server in spirit; host
  disconnect ends session, same policy).
- **Gameplay in it**: FPS view (pitch clamp ±89°, eye 1.65m, FOV kick — all
  from camera-system.md), WASD + pointer lock, 3 spell slots
  (LMB cast, wheel/Q/1-2-3 switch — input-system.md verbs), 6 wandering
  enemy dummies with HP bars and respawn, hp_clamp from
  health-damage-system.md, and the ⭐ synergy: Water applies Wet (6s),
  Lightning on Wet = 3x damage + chain (1.5x) to enemies within 6m, with
  chain-lightning FX and combat log.
- **Smoke-tested in the browser pane**: menu renders, no console errors,
  host flow reaches the game scene, PeerJS cloud registration works (peer id
  `covenant-mages-mvp-XXXX` confirmed assigned → signaling reachable).
  Could NOT test an actual 2-client join from the sandboxed pane — needs two
  real browser windows.
- **Next**: user opens prototype.html in two browser windows (or with a
  friend) to verify join-by-code + synergy over the network. This doubles as
  the long-outstanding real 2-player synergy test from
  co-op-spellcasting-concept/REPORT.md. Record findings in
  `prototypes/web-mvp-concept/README.md` Findings section.
- **Unity MVP debugging paused** (mouse-look/LMB bugs — see the section
  below; cursor-lock fix applied but still untested in Unity).
- Not committed yet.

## Current Stage

- `production/stage.txt` = `Concept`
- `production/review-mode.txt` = `lean`

## What's Done

1. **Agent framework**: Claude Code Game Studios (49 agents, 73 skills) in `.claude/`
2. **CI**: `.github/workflows/ci.yml` validates agent/skill frontmatter on PR/push to main
3. **Engine setup**: Unity 6.3 LTS, C#, URP (still valid for new concept)
4. **Visual references**: `references/` — dark-fantasy pixel-art + grainy first-person dungeon
5. **New game concept**: `design/gdd/game-concept.md` — Covenant of Mages (2026-08-26)

## Game Concept Summary (Covenant of Mages)

- **Genre**: Co-op session-based first-person dungeon crawler / action RPG, 4-5 players, PC.
- **Core fantasy**: Be an irreplaceable part of a mage quartet where your elemental school
  synergizes with teammates — win through spontaneous combo coordination, not pre-planned builds.
- **Spell schools**: fire, water, lightning, nature, air, light, dark (MVP: 3 schools)
- **Cross-player synergies**: water+lightning=chain bolt, water extinguishes fire, etc.
- **Spell loadout**: 3 active slots per player, switchable in combat; expanded via manuscripts
- **Progression**: manuscripts (found/bought) unlock spells; equipment gives passive stats;
  player level gives passive bonuses only (not new spells); max 2 schools per player (MVP: 1)
- **Death**: dead = spectator until fight ends; full team wipe = checkpoint respawn
- **Dungeons**: hand-crafted base structure + randomized enemies/loot; mix of arenas,
  corridors, puzzle sections, platformer segments (platformer: post-MVP)
- **Biggest risk**: networking for 4-5 players on developer's first 3D project
- **MDA**: Fellowship (1) + Challenge (2) + Discovery (3)
- Full detail in `design/gdd/game-concept.md`

## Engine Setup Summary (unchanged from Hollow Vow session)

- **Engine**: Unity 6.3 LTS, C#, URP + custom post-process pixelation shader
- **Platform**: PC (Steam/Epic)
- **Input**: keyboard/mouse primary, partial gamepad
- **Performance budget**: 60fps / 16.6ms, ≤2000 draw calls
- Full detail in `CLAUDE.md` and `.claude/docs/technical-preferences.md`

## Next Steps (priority order)

1. **[DONE] ADR-0001: networking stack** — Netcode for GameObjects выбран. `docs/architecture/adr-0001-networking-stack.md`. Статус: **Accepted** (2026-08-26) — можно начинать программирование по этой архитектуре.
2. **[DONE] `/prototype co-op-spellcasting`** — Verdict: PROCEED (with required
   follow-up real 2-player test — not yet scheduled). See "COMPLETE: Prototype —
   Co-op Spellcasting" section above for full detail, including the target-feedback
   risk that must feed into input-system.md / combat design.
3. **[DONE] `/map-systems`** — 30 систем, карта зависимостей, порядок проектирования.
   `design/gdd/systems-index.md` перезаписан под Covenant of Mages (заменил Hollow Vow).
4. **[DONE] Resolve input-system.md** — retrofit completed 2026-08-26. See
   "COMPLETE: Input System Retrofit" section above.
5. **[DONE] `/design-system camera-system`** — completed 2026-08-26. See
   "COMPLETE: Camera System GDD" section above.
6. **[DONE] `/design-system health-damage-system`** — completed 2026-08-26.
   See "COMPLETE: Health & Damage System GDD" section above.
7. **`/design-system networking-foundation`** — next in bottleneck-first
   order (user request). ⚠️ Critical bottleneck, ADR-0001 already Accepted.
   After that: Elemental Status System → Spell Casting System → Target
   Feedback System → Elemental Synergy System.
8. **[FOLLOW-UP, not blocking]** Schedule a real 2-player test of
   `prototypes/co-op-spellcasting-concept/` to validate the co-op-discovery hypothesis
   before finalizing the synergy-dependent parts of the combat GDD.
9. **[SEPARATE TRACK, not part of the GDD/ADR pipeline above]** Ongoing manual
   iteration on `prototypes/web-mvp-concept/prototype.html` (a standalone
   browser/PeerJS co-op combat prototype, unrelated to the Unity production
   track above except as a feel/animation reference). See "ACTIVE: Web MVP
   Prototype iteration" section below for current state — that prototype's
   own README.md Findings section is the authoritative detail log; this
   section just points to it and lists the last commit.

## ACTIVE: Web MVP Prototype iteration (`prototypes/web-mvp-concept/`)

- **What it is**: a single-file Three.js + PeerJS browser prototype
  ("Covenant of Mages — Web MVP") the user has been iterating on across
  several sessions to test co-op elemental-synergy combat feel. Fully
  separate from the Unity production track — see
  `.claude/rules/prototype-code.md` (relaxed standards, no production
  dependency either direction).
- **Where the real detail lives**: `prototypes/web-mvp-concept/README.md`
  → **Findings** section (bottom of file). Every fix this session and
  prior sessions is logged there with root cause and the exact constant/
  function changed — read that before touching `prototype.html` again.
- **Last commit on this track**: `c00166d` — "Replace regular enemy model
  with Goblin FBX, fix visibility and FP camera" (2026-09-07). Also on
  this branch from the same session: `78b2146` (class-select models face
  the player + mouse-rotatable), `40202d4` (Water idle pose / Fire spark
  spawn point / debug camera orbit), `3324546` and earlier (loading
  screen, Water's own model, movement lock during cast).
- **Local dev server**: `.claude/launch.json` has a `web-mvp` config
  (`python -m http.server 8743 --directory prototypes/web-mvp-concept`) —
  use `preview_start` with name `web-mvp`, then open
  `http://localhost:8743/prototype.html`.
- **Known-good debugging technique** (rediscovered/refined this session,
  worth reusing directly rather than re-deriving): the Browser pane's own
  render loop does not run reliably in this tool environment, so the
  normal screenshot tool often shows nothing useful for anything that
  depends on live animation. Instead: call `renderer.render(scene, camera)`
  manually once (via `javascript_tool`) against a **small** off-screen
  canvas — 64×64, JPEG at ~0.6 quality — and return `canvas.toDataURL(...)`.
  Larger PNGs (~40K+ base64 chars) reliably corrupted when written to a
  file via the `Write` tool this session (twice); the small JPEG approach
  (~1-2.5KB base64) did not. Decode with a small Python script reading a
  file the base64 was written to, not embedded inline in a bash heredoc
  (quoting broke on one attempt with special characters in the string).
- **`window.__debug`** already exposes most internals needed for this
  (scene, THREE, enemies, goblinTemplate, mageTemplate*, playerPos, etc.)
  — check it before adding new exports.
- **Open/unverified items** (see README Findings for exact wording):
  the FP camera offset values are tuned by feel from user feedback, not
  independently re-verified; multiplayer-specific (2+ real client) testing
  has not happened this session.

## COMPLETE (this session, new): Goblin colors fixed, real FP viewmodel abandoned, placeholder arms are now the actual solution

- **Goblin colors fix**: replaced flat baseline emissive (which was
  washing out the texture into one brown blob) with `emissiveMap = map`.
  See README Findings for the before/after detail.
- **Decision this session**: after exploring an arms-only shader clip +
  live camera/viewmodel-offset tuning tools for the real mage-model FP
  viewmodel, user decided to stop investing in that approach (three
  attempts across sessions, same root problem each time — a third-person
  body isn't a viewmodel without a dedicated FPS rig) and commit to the
  placeholder box/sphere arms as the permanent FP visual.
  - `ensureMyViewmodel()`/`myViewmodel`/`myViewmodelHandBone`/
    `myViewmodelHandGlow` and every call site removed from `main`.
  - **That exploratory work (arms-only clip, live camera-offset adjuster
    C-key debug mode, live viewmodel-offset arrow-key tuning) is
    preserved on its own branch**: `web-mvp-camera-viewmodel-experiment`
    (one commit, `34b7a8b`). Not merged — kept for reference only, per
    explicit user request to keep it separate from main.
  - **Placeholder/projectile sync** (the other half of this session's
    request): the recoil animation that drives `fpArmR`'s punch-forward
    motion now peaks (`castRecoilPeakT`) at exactly `castFireDelayMs` —
    the same tier-specific timing already used to delay the actual shot
    — instead of a fixed 0.08s that had nothing to do with real fire
    timing. `fpOrb` (child of `fpArmR`) was already the projectile/
    hitscan spawn-position fallback; now that it's the only source (no
    more real hand bone), its position at fire time always matches
    wherever the recoil animation actually put the arm. Verified in the
    browser pane: switched to a flying-projectile spell, called
    `tryCast()`, confirmed a projectile spawns at `fpOrb`'s live world
    position with no console errors — did not verify the recoil motion
    visually frame-by-frame (rAF doesn't reliably tick in this sandbox).
- Full detail, including exact numbers and the branch note, in
  `prototypes/web-mvp-concept/README.md` Findings.
- Committed to `main`? **Not yet** — same as everything else on this
  track this session, awaiting explicit commit instruction.

## IN PROGRESS: Per-player spawn/despawn portals (Lightning wired, Fire/Water pending)

- User wants each player to spawn from/despawn into their own portal
  (one model per class), supplying 4 hand-authored `.blend` files
  (3 elements currently used in code + one spare). Only Lightning's
  files placed so far: `models/portal/portal_lightning.fbx` + `.png`
  (renamed from the user's `yellow.fbx`/`.png`).
- **Implemented and working**: `ensurePortalTemplate`/`instantiatePortal`/
  `refreshPortal`/`updatePortal` near `loadGoblinAssets` in
  `prototype.html`. Hooked into `respawnLocalPlayer()` (local player —
  covers both initial spawn and every respawn) and the remote-player
  interpolation block (first placement + downed→alive transition).
  Portal despawns once its owner walks `PORTAL_EXIT_RADIUS` (2.2) away;
  a fresh one spawns on every respawn rather than following the player.
- **Real bug found and fixed during testing**: portals never appeared
  on the very first try — `respawnLocalPlayer()` asks for the portal
  synchronously, but the element's FBX/PNG are only fetched lazily on
  first need, so that first ask always raced the still-in-flight fetch
  and got `null` back with no retry. Fixed with a deferred-callback +
  generation-counter pattern (`portalWaiters`, `myPortalGen`/
  `rp.portalGen`) — see README Findings for the exact mechanism.
- **Also applied the same emissive-map fix as the goblin** (dungeon's
  dim ambient light was making the portal read as a dark blob) —
  fixed.
- **User confirmed working** (goblin colors, flicker, portal visibility
  all good) after two follow-up fixes: (1) old shared pentagram decal
  removed — was directly coplanar with the portal's ground ring
  (z-fight candidate); (2) actual flicker cause was the portal itself
  sitting at world y=0, exactly coplanar with the stone floor — same
  z-fighting mechanism, fixed by lifting it to y=0.03. An earlier
  `renderOrder` fix (for a *different*, real but not-the-cause draw-
  order concern between the portal's own 2 meshes) did NOT fix the
  flicker — kept anyway since it's still correct for its own reason.
  Also scaled up twice on request: 0.4 → 0.6 → 1.2 (`PORTAL_SCALE`),
  `PORTAL_EXIT_RADIUS` scaled alongside each time (2.2 → 3.3 → 6.6).
- Fire/Water assets still pending — once placed in `models/portal/`,
  they work with zero code changes (same lazy loader, keyed by element
  name).

## IN PROGRESS: Portal antechambers + walk-through door for levels 1 & 2

- User wants portals to live in their own small room per level, with a
  **physical** doorway into the enemy arena (not another teleport-
  trigger like the hub's existing "continue portal") — door opens on E,
  slides down into the floor, stays open permanently (no re-closing).
- **Built**: `buildArena()` gained an optional `doorGapWidth` param
  (cuts a gap in a zone's north wall instead of one solid box); new
  `buildPortalRoom()` builds a 9×7 room right outside that gap (in
  empty world space that already existed between zones — level1's
  north wall is at z=20, level2 starts at z=-70+29=-41, nothing used
  that gap before) with its own walls/floor/door. `ZONES[1]`/`ZONES[2]`
  `.spawn` moved into the new rooms. Door collision via a new
  `ZONE_DOOR_WALLS[zoneId]`, resolved the same way as level 2's maze
  walls; player movement clamp widened to actually reach the room.
  `openZoneDoor()`/`resetZoneDoor()` + a slide-down animation in
  `animate()`; `startLevel()` reseals the door every fresh entry.
- **No enemy-side work needed** — confirmed by reading
  `clampEnemyToArena()` first: it already keeps every enemy a full unit
  short of the original wall line regardless of aiState, so they're
  physically incapable of ever reaching the new room. Saved a lot of
  otherwise-necessary work.
- **Real bug caught before it shipped**: the door's proximity prompt
  would have kept `activeDoorAction` truthy across a level→hub
  teleport (its own update function just returned early during
  shopPhase without clearing it), letting a stale E-press hijack a
  shop-stand interact. Fixed by clearing it on that early return.
- **Verified this session**: player spawn position and both door mesh
  positions match the computed design exactly (checked via
  `scene.traverse`); rendered the room off-screen — looks like a small
  correct enclosed space with the portal on the floor. **NOT verified**:
  the live E-to-open flow, the slide animation, or actually walking
  through into the arena — needs the same real-browser check as
  everything else this session (sandboxed pane can't tick
  `requestAnimationFrame` reliably). **This is what the user should
  test first.**
- Level 3 (boss) and the hub were deliberately left untouched — only
  levels 1 and 2 were asked for.
- **User confirmed this all works**, then asked for 5 more changes in
  one message, all done:
  1. **One shared portal, not one per player** — removed the whole
     per-player/per-remote-player portal system in favor of a single
     `zonePortal` fixed at the zone's `spawn` point; the existing
     1-unit jitter on `respawnLocalPlayer()` already covers "everyone
     appears close together but not stacked," untouched.
  2. Room bigger: 9×7 → 16×13 (`PORTAL_ROOM_WIDTH`/`DEPTH`).
  3. A lever next to the door (`makeLever()`) as a visible "this is how
     you leave" cue — swings in sync with the door's slide animation.
  4. Taller walls + a real ceiling — new `PORTAL_ROOM_HEIGHT = 7` (the
     first ceiling anywhere in this game).
  5. Dirt-look floor for the antechamber only (`makeDirtTexture()`/
     `dirtMaterial()`, mottled blotches instead of the usual brick
     pattern) — rest of the dungeon keeps stone.
  Verified structurally (spawn position, ceiling count, lever position
  by distinctive color, door height) — **not** verified live (pulling
  the lever, watching it animate) for the same recurring sandboxed-pane
  reason as everything interactive this session.
- Full detail in `prototypes/web-mvp-concept/README.md` Findings
  (search "portal antechambers" and "Portal-room follow-up round").
- **Then 4 more asks in one message**, all done:
  1. **Dirt floor everywhere in the dungeon** (was antechamber-only) —
     `buildArena()`'s floor is `dirtMaterial()` for every zone except
     `'hub'` now (hub keeps stone — deliberately different calmer room).
  2. **Goblins 1.5x smaller** (`GOBLIN_SCALE` `0.01`→`0.01/1.5`,
     confirmed 1.40m via bounding box) — HP bar height scaled down to
     match (`GOBLIN_HP_BAR_Y`) so it doesn't float above the shorter
     model.
  3. **Button instead of lever, more noticeable** — `makeButton()`
     replaces `makeLever()`: pulsing red push-button + point light,
     goes flat grey once pressed. Prompt text now "Нажать кнопку".
  4. **Menu lag while typing name / after pressing Host** — traced to
     `loadMageAssets()` firing 6 `FBXLoader.load()` calls (incl. two
     ~120MB files) via `Promise.all`, so their synchronous parses
     tended to land back-to-back on the main thread. Changed to
     sequential `await`s — spreads the hitches out (longer total load,
     smoother per-hitch). Confirmed the rewrite still loads correctly
     (`mageTemplate*` all populated, no console errors) but couldn't
     verify the felt lag is actually gone (needs real-time judgment in
     a real browser). Noted a deeper fix (parallel `fetch()` + serial
     `loader.parse()`) as the next step if this isn't enough.
  Full detail in README Findings ("Four more asks in one message").
- Not committed yet.

## COMPLETE: Combat tuning batch — 8 asks in one message

- 1. Same portal for every class (`ZONE_PORTAL_ELEMENT` map, defaults to
     Lightning). 2. Goblin melee re-timed with a real telegraph — the
     actual bug was damage landing instantly with zero windup, not the
     raw numbers; added `meleePending`/`MELEE_IMPACT_FRAC` so the hit
     fires partway through the windup and re-checks range at impact
     (backing off now makes it whiff). 3. HP color coding
     (green/yellow/red, `hpFracColor()`) on both enemy bars and the
     player HUD bar. 4. Out-of-combat regen (`timeSinceDamage`,
     `REGEN_DELAY_S`/`REGEN_RATE_HP_S`). 5. 2nd-ability cooldown now has
     a real visible overlay on its slot icon (was tracked correctly,
     just never shown). 6. Enemy HP bars shrink from the right,
     anchored left (geometry-translate trick). 7. Tier-2 damage bumped
     (Fire 22→32, Water 8→13, Lightning 14→22) — they were actually
     *lower* dps than their own tier-1 once cooldown was factored in.
     8. Fire applies a burn DOT (`applyBurn()`, 4s, 60% of the hit's own
     damage); Water extinguishes burn instead of applying Wet if the
     target was already on fire.
- **Verified live** via `window.__debug` calling `hostResolveCast`/
  `hostApplyDamage`/`hostSimulate` directly with synthetic args (these
  are host-simulation functions — callable without a render loop, unlike
  most things blocked by this session's rAF limitation). Confirmed: burn
  applies and Water correctly extinguishes it (no Wet) when there's no
  delay between the two calls — an earlier test WITH an artificial delay
  produced a misleading result because the burn timer legitimately
  expired during real wall-clock time between tool calls, not a logic
  bug. Confirmed the melee delay: HP stays full through the windup,
  drops by exactly `MELEE_DAMAGE` only at the impact fraction; a
  same-idea side effect (enemy teleported into the antechamber got
  clamped back out mid-windup by the existing arena bound) doubled as
  a second proof the impact-time range recheck works. Confirmed regen
  math, cooldown-overlay DOM state, and the HP-bar left-anchor geometry
  directly.
- Full detail in `prototypes/web-mvp-concept/README.md` Findings ("Big
  combat-tuning batch, 8 asks in one message").
- Not committed yet.

## COMPLETE: Maze fix + combat pacing round 2 (5 asks)

- **"Walk/see through some walls" in the level-2 maze — real root cause
  found, not a maze-generation gap.** Verified the maze generation
  itself first (all cell neighbor-flag pairs consistent, sample corner
  wall overlaps clean) before concluding the actual bug was elsewhere:
  FP camera sits 0.3 units forward of the actual collision point
  (`FP_CAMERA_FORWARD_OFFSET`), but `PLAYER_RADIUS` was only 0.35 — so
  the camera could get within 0.05 units of a wall, inside the camera's
  own 0.1 near-clip plane. Bumped `PLAYER_RADIUS` to 0.55 for real
  margin.
- **Maze corridors wider**: 9×9 grid @ 6 units/cell (5.4-unit
  corridors) → 7×7 @ 8 units/cell (7.3-unit corridors), same tight fit
  against the zone bounds (28 half-width either way).
- **Goblins slowed further** (still felt fast after the first pass):
  `ATTACK_ANIM_DURATION` 0.7→1.1s, `MELEE_COOLDOWN` 1.6→2.4s.
- **Ability cooldown now starts only when the cast finishes**, not when
  it begins — `cooldowns[slot]` assignment moved from the start of
  `tryCast()` into a `setTimeout` firing after the cast's own duration.
  Verified directly: cooldown reads 0 immediately after casting, ~0.75
  only after the cast animation duration has actually elapsed.
- **Tier-2 cooldowns increased again**: Fire 1.0→1.8s, Water 1.3→2.2s,
  Lightning 1.1→2.0s.
- Verified live via `window.__debug` (maze structural consistency
  re-checked on the new grid size; cooldown-timing behavior called and
  timed directly).
- Full detail in README Findings ("5 more asks, mostly level-2 maze +
  combat pacing").
- Not committed yet.

## COMPLETE: Real fix for "walk/see through walls" (previous fix was incomplete)

- User reported the maze wall bug was still happening after the
  `PLAYER_RADIUS` 0.35→0.55 bump from the previous round — that fix
  addressed a real but secondary near-clip risk, not the actual bug.
- Real root cause found in `resolveCircleWallCollision()`: when the
  player's position ends up fully *inside* a wall's AABB on both axes, the
  circle-vs-closest-point vector degenerates to zero (`dx=dz=0`), so the
  `distSq > 1e-9` guard skips the wall entirely — no push-out, player left
  embedded in solid geometry. This is reachable in real play because the
  per-zone boundary clamp lands the player exactly on the zone's outer
  edge, while the maze's perimeter walls (real thickness 0.7) are centered
  on that same edge and straddle it — the clamp itself can shove the
  player's collision point inside a wall box.
- Fixed by adding a branch for the "center inside box" case: push out along
  the nearest of the box's four faces (standard shallow AABB penetration
  resolution) instead of the degenerate closest-point vector.
- Verified live via `window.__debug`: reproduced the exact boundary-clamp
  scenario against level 2's actual west-perimeter wall — old code left the
  player's position inside the wall box untouched; new code correctly
  pushes it to just outside the nearest face.
- This is a fix to the shared collision function, so it also covers the
  portal-room doors and anywhere else it's used, not just the maze.
- Full detail in README Findings.
- Not committed yet.

## COMPLETE: Level 2 antechamber removed, loading-bar regression fix, hub mirror

- User reported the maze wall bug was STILL happening even after the
  `resolveCircleWallCollision()` fix above, and asked to simplify: no
  separate portal room for level 2 anymore — players spawn straight into
  the maze like before that room was added. `buildPortalRoom(2, ...)` call
  removed, level 2's north wall reverted to solid (no door gap), and
  `ZONES[2].spawn` now resolves to the maze's own center cell (computed
  right after `buildMaze(2, ...)` since the maze doesn't exist yet when
  `ZONES` is declared). Level 1 keeps its antechamber unchanged.
- **Loading-bar-goes-backward bug found and fixed**: `updateLoadProgressUI()`
  summed bytes across only the files that had started downloading so far,
  not all 6 known files — since mage models load one at a time, a new
  ~120MB file starting could instantly drop the shown percentage (e.g.
  90%→45%) the moment its full size joined the total before any of its
  bytes arrived. Rewrote to count whole finished files
  (`(filesDone + currentFileFraction) / 6`), which is monotonic by
  construction.
- **Wall mirror added in the hub** (the room between dungeon runs) via
  `three/addons/objects/Reflector.js`, mounted on the west wall. Reuses the
  existing `debugOwnBody` world-space avatar mesh (previously only built/
  shown for the V-key debug third-person view) — now always built, updated,
  and visible every frame so the mirror has the local player's own class
  model to actually reflect.
- Verified live via `window.__debug`: level 2 spawn point confirmed clear
  of walls (3.65 units to nearest); no new console errors after any of the
  three changes; the `Reflector` object confirmed present in the scene
  graph at the correct wall position/rotation, and `debugOwnBody` confirmed
  live/visible. Could NOT get an actual screenshot of the mirror's
  reflection — the sandbox's unreliable `requestAnimationFrame`/input
  timing (documented earlier) made it impossible to rotate the debug camera
  to frame it before the next frame overwrote the manual override. Visual
  quality of the mirror (lighting, reflection clarity) needs a real
  playtest.
- Full detail in README Findings.
- Not committed yet.

## COMPLETE: Mirror regression fix — real model's hands were showing during normal play

- User reported the previous mirror change made the real class model's
  hands visible during normal first-person play, stacked on the placeholder
  FP arms. Caused by making `debugOwnBody.mesh.visible` unconditionally
  true so the mirror always had something to reflect — the main camera
  rendered it too, not just the mirror.
- Fixed by separating the two concerns: `debugOwnBody.mesh.visible` is back
  to `debugThirdPerson`-gated (hidden during normal play, like before the
  mirror existed). The mirror now forces it visible (and hides the
  placeholder `fpArmL`/`fpArmR`, which are parented to `camera` and thus
  also in the scene graph) only for its own internal reflection render,
  via a wrapped `Reflector.onBeforeRender` in `makeMirror()`, restoring
  both right after.
- Verified live: only placeholder FP arms visible during normal play
  (screenshot); `debugOwnBody.mesh.visible` reads back `false` after a full
  frame in the hub, confirming the mirror's before/after hooks ran and
  restored it correctly; no console errors.
- Full detail in README Findings.
- Not committed yet.

## COMPLETE: Mirror frame fix, tier-1 rework (no CD, mastery system), 4-hit goblins, stone floor, spell wheel UI

Large batch, all verified live via `window.__debug` calls (math checked
against hand-computed expected values, not just "ran without crashing")
except where noted:

- **Mirror frame**: was invisible (placed behind the glass, inside/flush
  with the wall). Rebuilt as 4 bars in front, in a `THREE.Group` at the
  mirror's transform. Confirmed 4 frame-bar boxes in the scene graph.
- **Tier-1 spells (Искра/Плеск/Разряд) have 0 cooldown** — limited only by
  the cast animation (`castLockedUntil` already covered this).
- **Goblins die in 4 hits**: `ENEMY_MAX_HP` 100→60 (4× Искра's 15 dmg).
- **Floor back to stone brick everywhere** (dirt floor from an earlier
  round removed, including `makeDirtTexture`/`dirtMaterial` — dead code).
- **New tier-1 mastery system**: hits fill an XP bar (`useHits`, cap 8, no
  more auto-level mid-fight); actual level-up happens at the hub mentor
  stand only (still gold-gated, now ALSO needs the bar full). Capped at
  level 4, each level a real mechanical change:
  - Разряд: 2 hits at lvl 2, 3 at lvl 3 (verified: dealt exactly 2× a
    single hit). Max level → new 3s continuous channel
    (`startLightningChannel`), movement locked via the existing
    `castLockedUntil`/`isCasting` gate but mouse look untouched.
  - Искра: dmg scales via existing levelMult; max level fires 3 real
    independent projectiles in a fan.
  - Плеск: Wet duration scales with level; level 2+ slows (`e.slowFactor`,
    verified: enemy took 0 movement over 0.5s while frozen); max level
    freezes 1s (`e.frozenT`, gates the whole AI block). New icy tint added.
- **New circular spell wheel UI** replacing the 3-slot row — icons at fixed
  angles, `updateSlotUI()` recomputes `translate(x,y)` on switch, CSS
  transition makes it read as spinning without rotating any element.
  Verified visually with both 1 and 3 spells equipped — layout matches.
- Not verified live: gold-gated mentor-stand purchase itself
  (`hostLevelUpSpell` isn't on the debug object — code-review only), and
  the actual *feel* of the Fire triple-shot spread / Lightning channel
  (needs a real playtest, same rAF-reliability limitation as always).
- Full detail in README Findings.
- Not committed yet.

## Recovery Instructions

1. Read this file.
2. Read `design/gdd/game-concept.md` for full new concept.
3. Read `CLAUDE.md` and `.claude/docs/technical-preferences.md` for engine config.
4. See "SUPERSEDED" and "BLOCKING" sections above for what needs attention first.
5. Apply User Preferences (Russian language; instructions in chat, not new repo files).
6. If picking up the web prototype instead of the Unity/GDD track, skip to
   "ACTIVE: Web MVP Prototype iteration" above and read
   `prototypes/web-mvp-concept/README.md`'s Findings section first.
