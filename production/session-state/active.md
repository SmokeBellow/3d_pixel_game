# Session State — Hollow Vow

*Last updated: 2026-08-10*

## IN PROGRESS: /prototype core-combat

- **Status**: Phase 5 — Implement (Engine path, Unity)
- **Hypothesis**: Fast/responsive melee combat with combo chains in Unity can still feel weighty — a 3+ hit combo feels connected, hits give clear feedback with no perceived input lag.
- **Riskiest assumption being tested**: combining "fast/responsive" with "weighty" in the same combat feel — first completed 3D project.
- **Path**: Engine (Unity) — chosen because feel IS the hypothesis, browser latency would lie.
- **Scope** (explicitly minimal):
  - Capsule player, WASD movement, simple third-person orbit camera
  - One weapon: 3-hit combo chain on left-click with input buffering
  - Dodge roll with i-frames
  - 2-3 stationary target dummy capsules: knockback + color flash + hit-stop (`Time.timeScale` blip) + camera shake on hit
  - Placeholder hit sound (generated tone via `AudioSource`)
- **Explicitly cut**: pixelation post-process shader (deferred to a separate future spike — it's a rendering question, not a combat-feel question), enemy AI, real animations (squash/stretch placeholder only), health/damage UI, menus, multiple weapons, dungeon environment.
- **Output location**: `prototypes/core-combat-concept/`
- **Next action if resuming**: check `prototypes/core-combat-concept/` for what's been written so far; continue the Engine-path multi-turn loop (write code → user runs in Unity Editor → reports errors/feel → iterate) until playable, then move to Phase 6 (Playtest Debrief).

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

If starting a fresh session/chat:
1. Read this file first.
2. Read `design/gdd/game-concept.md` for full concept detail.
3. Read `CLAUDE.md` and `.claude/docs/technical-preferences.md` to confirm engine config (Unity 6.3 LTS / C#).
4. Resume at `/prototype core-combat`.
