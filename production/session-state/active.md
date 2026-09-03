# Session State — Covenant of Mages

*Last updated: 2026-08-26*

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

## Recovery Instructions

1. Read this file.
2. Read `design/gdd/game-concept.md` for full new concept.
3. Read `CLAUDE.md` and `.claude/docs/technical-preferences.md` for engine config.
4. See "SUPERSEDED" and "BLOCKING" sections above for what needs attention first.
5. Apply User Preferences (Russian language; instructions in chat, not new repo files).
