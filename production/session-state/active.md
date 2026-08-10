# Session State — Hollow Vow

*Last updated: 2026-08-10*

## User Preferences (durable — apply every session)

- Respond in Russian.
- Deliver step-by-step instructions (setup guides, how-tos) directly in chat — do NOT create separate instructional files in the repo (e.g. no more SETUP.md-style docs). Code/config files that are part of the actual deliverable are fine; user-facing walkthroughs are not.

## COMPLETE: /design-system input-system — GDD written, not yet reviewed

- **Status**: `design/gdd/input-system.md` fully written (all 8 required sections + Visual/Audio, UI Requirements, Open Questions). Built on Unity's new Input System Package (Input Actions asset, Gameplay/UI action maps), NOT the legacy Input class the prototype used. Formulas cover mouse-look sensitivity, gamepad deadzone/curve, and frame-rate independence (mouse delta NOT scaled by deltaTime; gamepad stick IS). 19 GIVEN-WHEN-THEN acceptance criteria written with qa-lead input.
- **Registry updated**: 5 constants + 3 formulas registered in `design/registry/entities.yaml` (sensitivity, base_scale, deadzone, curve_exponent, look_speed_deg_per_sec; mouse_look_rotation, gamepad_stick_response, gamepad_look_rotation) — Camera System GDD must reference these, not reinvent them.
- **systems-index.md updated**: Input System status → Designed, linked, progress tracker updated (1/17 MVP systems designed).
- **Not yet done**: `/design-review design/gdd/input-system.md` in a fresh session (never run in the same session as authoring).
- **Next action if resuming**: either run `/design-review` on input-system.md in a fresh session, or continue to the next system in design order — **Camera System** (per systems-index.md Recommended Design Order) — now explicitly first-person/mouselook-only, no orbit rig. Camera System GDD should reference the registered sensitivity/deadzone/look-speed constants from Input System rather than redefining them.

## COMPLETE (superseded architecture note, see above): /design-system input-system — resumed after first-person camera pivot

- **Pivot resolved**: `game-concept.md` and `design/gdd/systems-index.md` both updated for the first-person (standard FPS mouselook, no third-person orbit) pivot. Camera perspective was never explicit in game-concept.md before — this is an addition, not a contradiction of prior text. Accepted risk documented in both files: core-combat prototype's PROCEED verdict was earned third-person; a follow-up spike to re-validate hit-feedback/combo-legibility in first-person is planned but NOT scheduled yet (explicit user decision — deal with it later, not now).
- **Resuming**: `design/gdd/input-system.md` Section C (Detailed Design → Core Rules). Overview and Player Fantasy sections already written and don't need changes (camera-agnostic). Confirmed with user: mouse turns the character/view directly (standard FPS mouselook), not a locked-forward view.

## PAUSED (resolved above): /design-system input-system — blocked on first-person camera pivot

- **Status**: PAUSED mid-Section C. Overview and Player Fantasy sections written (indirect/infrastructure framing, no camera-look action assumed). While drafting Core Rules, the user confirmed a major pivot: **the game is switching from third-person orbit camera to first-person with NO camera rotation control at all** (player always looks forward). This contradicts the currently-approved `game-concept.md` (Core Mechanics explicitly describes third-person combat) and the core-combat prototype's PROCEED verdict (earned specifically for third-person feel — orbit camera, camera shake, hit-stop tuned for that perspective).
- **User decision on sequencing**: update `game-concept.md` FIRST for the first-person pivot, then `systems-index.md` (Camera System entry needs rework — may become trivial/removed if there's truly no camera rotation), THEN return to finish `design/gdd/input-system.md` Section C onward.
- **Next action if resuming**: if `game-concept.md` still says third-person/orbit, the pivot work hasn't been done yet — start there. If `game-concept.md` already reflects first-person, check `systems-index.md` next, then resume `/design-system input-system` from Section C (Core Rules) — Overview and Player Fantasy are already written and don't need first-person changes (they were written camera-agnostic).
- **Open question not yet resolved with user**: what happens to the core-combat prototype's PROCEED verdict given it validated third-person feel specifically — does it need to be re-run in first-person before design continues, or is that risk accepted for now? Surface this explicitly during the game-concept.md pivot conversation.

## COMPLETE: /map-systems — systems index written

- **Status**: DONE. `design/gdd/systems-index.md` written — 18 systems enumerated, dependency-mapped, priority-tiered. 17 systems MVP, 1 (Skill Tree/Progression) Vertical Slice. Design order determined (see index's Recommended Design Order table) — starts with Input System, Camera System, Health & Damage, Save/Load, Pixelation Rendering Pipeline, Audio System (Foundation), then Combat System, Enemy AI, Checkpoint & Death (Core), then Feature-layer systems.
- **Bottleneck systems flagged**: Combat System, Save/Load & Persistence, Guild Trial Dungeon System.
- **High-risk systems flagged**: Enemy AI System (no dodge/block — must validate multi-enemy encounters), Combat System (combo legibility needs real animation budget), Pixelation Rendering Pipeline (technique never spiked), Guild Trial Dungeon System (level-design effort may be underestimated), Skill Tree (untested verb/number split).
- **Next action if resuming**: run `/design-system [system-name]` starting with Input System (first in design order), or `/map-systems next` to auto-pick. `/gate-check systems-design` also available for a formal director sign-off (skipped this session — review-mode is lean, TD-SYSTEM-BOUNDARY/PR-SCOPE/CD-SYSTEMS gates all auto-skipped).

## COMPLETE: /prototype core-combat — final verdict PROCEED (after 1 PIVOT iteration)

- **Status**: DONE. Iteration 2 changes (procedural swing tween on `weaponVisual`, cursor-lock fix on `OrbitCamera`, placeholder weapon in scene builder) tested. Camera stutter fix confirmed working. Combo visual read still imperfect ("не хватает визуального отображения комбо") but tester judged the prototype acceptable overall — **final verdict: PROCEED**, with combo legibility flagged as a real-animation requirement to solve during production, not a fundamental feel blocker.
- **Reports updated**: `prototypes/core-combat-concept/REPORT.md` (added Iteration 2 section + revised Recommendation/If Proceeding), `prototypes/index.md` (verdict now PROCEED after 1 PIVOT iteration).
- **Key production-informing takeaway**: fast/responsive input + weighty hit feedback do NOT conflict — the project's biggest flagged risk is resolved. Combo chain legibility needs real windup/attack animation or motion-designed VFX/trail in production; a pure procedural tween wasn't enough on its own. The combat system GDD should call this out explicitly as an animation/VFX requirement.
- **Next action if resuming**: proceed per the PROCEED path — `/design-review design/gdd/game-concept.md` → `/gate-check` → `/map-systems` → `/design-system [mechanic]` (embed combo-legibility learning in that GDD's Tuning Knobs/Formulas). CD review was skipped both iterations (review-mode = lean).

## COMPLETE (iteration 1): /prototype core-combat — PIVOT

- **Status**: DONE. Full run completed: Unity project set up (new drive, Active Input Handling switched to "Both" to support legacy Input Manager code), scripts copied in, scene built (floor + walls added mid-session after dummies fell through empty scene), played, and Phase 6 Playtest Debrief completed.
- **Hypothesis**: Fast/responsive melee combat with combo chains in Unity can still feel weighty — a 3+ hit combo feels connected, hits give clear feedback with no perceived input lag.
- **Verdict**: PARTIALLY CONFIRMED hypothesis → **PIVOT** recommendation. Hit-feedback stack (knockback, flash, hit-stop, camera shake) landed well — called out as a pleasant surprise. Combo chain was "completely unclear" due to zero swing/windup animation. Camera orbit had an intermittent stutter, named as equally disruptive.
- **Reports written**: `prototypes/core-combat-concept/REPORT.md` (full playtest report) and `prototypes/core-combat-concept/PIVOT-NOTE.md` (what to keep / what to change / revised hypothesis). `prototypes/index.md` created with the entry.
- **What to keep for next iteration**: all current mechanics (movement, orbit camera, 3-hit combo + buffering, dodge roll, hit-feedback stack) — no changes needed there.
- **What to change for next iteration**: add placeholder swing/windup + hit animation so combo timing is visually legible (crude procedural tween is enough — arc motion or squash/stretch); fix the camera orbit stutter.
- **Next action if resuming**: run `/prototype core-combat` again (revised iteration) using the hypothesis in PIVOT-NOTE.md — this time include a minimal swing/windup animation from the start rather than treating animation as fully separable from the combo-feel question. CD review was skipped (review-mode = lean).

## Current Stage

- `production/stage.txt` = `Concept`
- `production/review-mode.txt` = `lean` (director reviews only at phase gates)

## What's Done

1. **Agent framework installed**: Claude Code Game Studios (49 agents, 73 skills, hooks, rules) extracted into `.claude/` from the bundled zip. See `README.md` and `.claude/docs/quick-start.md`.
2. **CI**: `.github/workflows/ci.yml` validates agent/skill frontmatter and YAML/JSON across the repo on every PR/push to `main`. This is a placeholder gate until `/test-setup` scaffolds the real Unity test runner.
3. **Auto-merge**: user creates PRs manually; ask this session (or a future one) to call `enable_pr_auto_merge` on a PR to have GitHub merge it automatically once CI is green. Two one-time manual steps are still needed in GitHub repo settings (not achievable via available tools): enable "Allow auto-merge" (Settings → General → Pull Requests), and add a branch protection rule on `main` requiring the `Validate CCGS framework` check.
4. **Visual references**: `references/` contains dark-fantasy pixel-art and "Project: Shadowglass"-style screenshots (grainy first-person dungeon crawl aesthetic) — these informed the Visual Identity Anchor below.
5. **`/start` onboarding completed**: user has a clear concept, chose to formalize via `/brainstorm` first, review mode set to `lean`.
6. **`/brainstorm` completed**: full game concept written to `design/gdd/game-concept.md` (commit `07f384e`).
7. **`/setup-engine` completed**: Unity 6.3 LTS + C# chosen (commit `09ed40d`). See Engine Setup section below.

## Game Concept Summary (Hollow Vow)

- **Genre**: Dark-fantasy 3D action RPG / dungeon crawler, single-player, PC (Steam/Epic).
- **Core fantasy**: prove yourself to the last surviving guilds of a dying kingdom by descending into hand-crafted trial dungeons; each ends in a staged dark→gold "payoff room" where you earn your class's signature ability.
- **Progression**: free-spend skill tree per class for general growth + guild-trial-earned signature abilities for class-defining power (deliberately NOT gated behind a rigid tree only — hybrid, see Pillar 2).
- **Pillars**: Earned Identity · Structured Growth, Earned Mastery · Darkness Earns Light · Compact but Deep · Fellowship of Rivals.
- **Visual Identity Anchor**: combination — clean pixel-art as the base render style, with Project: Shadowglass-style grain/scanline texture inside dungeons; golden, clean pixel-art in payoff rooms/hub for contrast.
- **Biggest risk flagged**: this would be the user's first *completed* 3D project (prior unfinished work: a Godot 2D RPG and a Hogwarts-management sim) and the timeline target is "months" — aggressive for 3D combat + staged lighting set-pieces. Recommended mitigation: prototype core combat before writing full GDDs.
- Full detail in `design/gdd/game-concept.md` — read that file for the complete document (Core Loop, MDA analysis, MVP definition, Scope Tiers, Risks).

## Engine Setup Summary

- **Engine**: Unity 6.3 LTS, C#, URP + custom post-process pixelation shader, Unity Physics.
- **Why Unity over Godot/Unreal**: the project's biggest technical risk is responsive real-time melee combat with combo chains on a first completed 3D project — Unity's asset-store/tutorial ecosystem for action combat outweighed Godot's gentler learning curve and free-forever licensing for this specific risk. Unreal ruled out (overkill for pixelated stylized art, steepest learning curve).
- **Platform/Input**: PC only (Steam/Epic), keyboard/mouse primary, partial gamepad support.
- **Performance budget**: 60fps / 16.6ms frame budget, ≤2000 draw calls (defaults set now).
- **Testing**: Unity Test Framework (NUnit) — not yet scaffolded (`/test-setup` not run yet).
- **Reference docs**: `docs/engine-reference/unity/` already had accurate Unity 6.3 LTS data pre-populated in the framework; verification dates refreshed to 2026-08-10 after a fresh WebSearch confirmed no changes since Feb 2026.
- **Specialist routing**: `unity-specialist` (primary), `unity-shader-specialist` (owns the pixelation post-process shader), `unity-ui-specialist`, `unity-dots-specialist` / `unity-addressables-specialist` (only if those systems get used). Version Awareness sections added to all 5 agent files.
- Full detail in `CLAUDE.md` Technology Stack and `.claude/docs/technical-preferences.md`.

## Next Step (not started yet)

Per the game concept's "Next Steps" and the recommended **Path B — Prototype-First**:

1. `/prototype core-combat` — validate combat feel (fast/responsive melee + combo chains) and the pixelated 3D rendering approach are achievable in Unity before writing any system GDDs. This is the resolution step for the project's single biggest risk.
2. If prototype PROCEEDS → `/art-bible` → `/map-systems` → `/design-system [system]` per system
3. If prototype PIVOTS → back to `/brainstorm` with learnings

## Open Questions (from game-concept.md Risks section)

- Is the fast-but-weighty combat feel achievable within the timeline in Unity? (→ resolve via `/prototype core-combat`)
- 2 or 3 classes/guilds realistic for MVP? (→ resolve after prototype gives a real per-dungeon time cost)
- Exact pixelation/rendering technique (render-target downscaling vs. shader pixelation vs. vertex snapping)? (→ needs a technical spike inside the prototype — this is now a Unity-specific question, e.g. custom URP Renderer Feature vs. simple post-process shader)

## Recovery Instructions

If starting a fresh session/chat (including a new **local** Claude Code CLI session on the
user's Windows machine, run from inside the cloned repo — that session has direct filesystem
access this cloud session does not, so it can copy the prototype scripts into the Unity
project itself instead of just giving instructions):

1. Read this file first.
2. Read `design/gdd/game-concept.md` for full concept detail.
3. Read `CLAUDE.md` and `.claude/docs/technical-preferences.md` to confirm engine config (Unity 6.3 LTS / C#).
4. Check "IN PROGRESS: /prototype core-combat" above — most likely resume point is either
   (a) helping the user get the prototype running in Unity (copy scripts, run the Editor menu
   scene builder, press Play), or (b) if it's already running, jump straight to the Phase 6
   Playtest Debrief questions from the `/prototype` skill.
5. Apply User Preferences above (Russian language; instructions in chat, not new repo files) —
   this applies regardless of whether the session is cloud or local.
