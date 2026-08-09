# Session State — Hollow Vow

*Last updated: 2026-08-09*

## Current Stage

- `production/stage.txt` = `Concept`
- `production/review-mode.txt` = `lean` (director reviews only at phase gates)

## What's Done

1. **Agent framework installed**: Claude Code Game Studios (49 agents, 73 skills, hooks, rules) extracted into `.claude/` from the bundled zip. See `README.md` and `.claude/docs/quick-start.md`.
2. **CI**: `.github/workflows/ci.yml` validates agent/skill frontmatter and YAML/JSON across the repo on every PR/push to `main`. This is a placeholder gate until an engine is chosen and `/test-setup` scaffolds the real engine-specific test runner.
3. **Auto-merge**: user creates PRs manually; ask this session (or a future one) to call `enable_pr_auto_merge` on a PR to have GitHub merge it automatically once CI is green. Two one-time manual steps are still needed in GitHub repo settings (not achievable via available tools): enable "Allow auto-merge" (Settings → General → Pull Requests), and add a branch protection rule on `main` requiring the `Validate CCGS framework` check.
4. **Visual references**: `references/` contains dark-fantasy pixel-art and "Project: Shadowglass"-style screenshots (grainy first-person dungeon crawl aesthetic) — these informed the Visual Identity Anchor below.
5. **`/start` onboarding completed**: user has a clear concept, chose to formalize via `/brainstorm` first, review mode set to `lean`.
6. **`/brainstorm` completed**: full game concept written to `design/gdd/game-concept.md` (commit `07f384e`).

## Game Concept Summary (Hollow Vow)

- **Genre**: Dark-fantasy 3D action RPG / dungeon crawler, single-player, PC (Steam/Epic).
- **Core fantasy**: prove yourself to the last surviving guilds of a dying kingdom by descending into hand-crafted trial dungeons; each ends in a staged dark→gold "payoff room" where you earn your class's signature ability.
- **Progression**: free-spend skill tree per class for general growth + guild-trial-earned signature abilities for class-defining power (deliberately NOT gated behind a rigid tree — this was revised once during brainstorming, see Pillar 2).
- **Pillars**: Earned Identity · Structured Growth, Earned Mastery · Darkness Earns Light · Compact but Deep · Fellowship of Rivals.
- **Visual Identity Anchor**: combination — clean pixel-art as the base render style, with Project: Shadowglass-style grain/scanline texture inside dungeons; golden, clean pixel-art in payoff rooms/hub for contrast.
- **Engine**: NOT YET CHOSEN. `.claude/docs/technical-preferences.md` still says `[TO BE CONFIGURED]`.
- **Biggest risk flagged**: this would be the user's first *completed* 3D project (prior unfinished work: a Godot 2D RPG and a Hogwarts-management sim) and the timeline target is "months" — aggressive for 3D combat + staged lighting set-pieces. Recommended mitigation: prototype core combat before writing full GDDs.
- Full detail in `design/gdd/game-concept.md` — read that file for the complete document (Core Loop, MDA analysis, MVP definition, Scope Tiers, Risks).

## Next Step (not started yet)

Per the game concept's "Next Steps" and the recommended **Path B — Prototype-First**:

1. `/setup-engine` — choose the engine (PC-only target, 3D, pixelated/retro rendering, real-time combat requirement, user has past Godot experience but no completed 3D project — do not assume Godot, walk the user through the actual decision)
2. `/prototype core-combat` — validate combat feel + pixelated 3D rendering approach are achievable before writing any system GDDs
3. If prototype PROCEEDS → `/art-bible` → `/map-systems` → `/design-system [system]` per system
4. If prototype PIVOTS → back to `/brainstorm` with learnings

## Open Questions (from game-concept.md Risks section)

- Is the fast-but-weighty combat feel achievable within the timeline in the chosen engine? (→ resolve via prototype)
- 2 or 3 classes/guilds realistic for MVP? (→ resolve after prototype gives a real per-dungeon time cost)
- Exact pixelation/rendering technique (render-target downscaling vs. shader pixelation vs. vertex snapping)? (→ needs a technical spike once engine is chosen)

## Recovery Instructions

If starting a fresh session/chat:
1. Read this file first.
2. Read `design/gdd/game-concept.md` for full concept detail.
3. Read `.claude/docs/technical-preferences.md` to confirm engine is still unconfigured.
4. Resume at `/setup-engine`.
