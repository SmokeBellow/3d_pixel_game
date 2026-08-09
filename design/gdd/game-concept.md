# Game Concept: Hollow Vow

*Created: 2026-08-09*
*Status: Draft*

---

## Elevator Pitch

> It's a dark-fantasy 3D action RPG where you prove yourself to the last surviving guilds of a dying kingdom by descending into their cursed trial dungeons, fighting your way from suffocating darkness to a blinding, golden sanctum where your class's signature power is finally earned.

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | Action RPG / Dungeon Crawler (dark fantasy) |
| **Platform** | PC (Steam / Epic) |
| **Target Audience** | Achievers/Strategists who love earned character progression, with secondary appeal to Explorers who value atmosphere and discovery |
| **Player Count** | Single-player |
| **Session Length** | 30–120 min (one guild trial per session) |
| **Monetization** | Premium (none yet decided) |
| **Estimated Scope** | Medium (3–6 months, solo dev, MVP: hub + 2–3 guild trial dungeons) |
| **Comparable Titles** | Dark Souls III, Elden Ring, Diablo 4 (class quests), Skyrim (guild questlines) |

---

## Core Fantasy

You are a nobody in a kingdom that has already fallen into shadow — and the only way to become someone is to earn it. The last guilds of magic, thievery, and steel don't hand out power; they demand you walk into the dark places they no longer dare enter and come back changed. Every class ability you wield is proof you survived something, not a number you bought.

This is the fantasy of *deserving* your strength — the moment a Skyrim guild quest and a Diablo class-quest and the instant Elden Ring's Altus Plateau opens into gold light, fused into one repeatable, earned ritual.

---

## Unique Hook

It's like Skyrim's guild questlines, AND ALSO every guild's initiation is a hand-crafted, souls-adjacent dungeon descent that resolves in a deliberately staged dark-to-light revelation — and the reward is never generic; it's the one signature ability that defines how that class plays for the rest of the game.

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

| Aesthetic | Priority | How We Deliver It |
| ---- | ---- | ---- |
| **Sensation** (sensory pleasure) | 2 | Dark→gold lighting contrast at the end of every trial; weighty hit-feedback (hit-stop, screen shake, layered impact audio) |
| **Fantasy** (make-believe, role-playing) | 1 | Player becomes a named class (mage/thief/paladin) only after proving it in-world |
| **Narrative** (drama, story arc) | 5 | Environmental storytelling per guild; minimal dialogue trees, kept lean for scope |
| **Challenge** (obstacle course, mastery) | 2 | Fast, responsive combat with combo chains and punishing enemy patterns in trial dungeons |
| **Fellowship** (social connection) | 4 | Each guild has a distinct identity/worldview; light narrative rivalry between factions |
| **Discovery** (exploration, secrets) | 3 | Dense, compact trial dungeons with hidden loot and lore fragments |
| **Expression** (self-expression, creativity) | 3 | Free-spend skill tree per class, differentiated combo visuals per build |
| **Submission** (relaxation, comfort zone) | N/A | Not a design goal — the game is deliberately tense, not relaxing |

### Key Dynamics (Emergent player behaviors)

- Players will replay or revisit earlier guild trials once new skill-tree points change how their combat feels.
- Players will compare which guild's signature ability to prioritize first based on their preferred combat style.
- Players will treat the "golden sanctum" payoff room as a checkpoint worth pushing hard for, even at low resources ("one more push" psychology).

### Core Mechanics (Systems we build)

1. Fast, responsive real-time melee/spell combat with dodge windows and combo chains (per-class movesets)
2. Guild trial dungeons: hand-crafted, linear-to-branching descents ending in a signature-ability payoff chamber
3. Free-spend skill tree per class for general power growth, gated separately from guild-earned signature abilities
4. Hub sanctuary with guild factions, each with distinct visual/narrative identity

---

## Player Motivation Profile

### Primary Psychological Needs Served

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** (freedom, meaningful choice) | Free-spend skill tree, choice of which guild/class to pursue and in what order | Core |
| **Competence** (mastery, skill growth) | Visible growth in combo depth and damage output; hard-won signature abilities | Core |
| **Relatedness** (connection, belonging) | Distinct guild factions with their own worldview create a sense of joining something | Supporting |

### Player Type Appeal (Bartle Taxonomy)

- [x] **Achievers** (goal completion, collection, progression) — How: earning signature abilities and fully-invested skill trees per class
- [x] **Explorers** (discovery, understanding systems, finding secrets) — How: dense trial dungeons with hidden loot/lore, discovering each guild's distinct mechanical identity
- [ ] **Socializers** (relationships, cooperation, community) — not a design target; single-player
- [ ] **Killers/Competitors** (domination, PvP, leaderboards) — explicitly excluded (see Anti-Pillars)

### Flow State Design

- **Onboarding curve**: First guild trial is the tutorial — teaches core combat verbs (dodge, combo, block/parry if included) at a forgiving pace before the first real challenge encounter.
- **Difficulty scaling**: Later guild trials assume the player has skill-tree points and at least one signature ability; enemy patterns increase in speed and punish read errors more heavily.
- **Feedback clarity**: Clear hit-stop/audio on landed hits and blocks; skill tree UI shows immediate stat deltas; signature ability unlock is a scripted, unmistakable moment.
- **Recovery from failure**: Death returns the player to the trial's entrance (not the hub), preserving momentum; lost progress within a trial is time, not permanent power.

---

## Core Loop

### Moment-to-Moment (30 seconds)
Fast, responsive melee/spell combat — dodge, land combo hits, react to enemy tells. Combat must feel good in total isolation: hit-stop, screen shake, weighty audio even at high speed.

### Short-Term (5-15 minutes)
Enter a room or corridor segment of a guild trial → fight or solve an environmental obstacle → find loot or lore → decide whether to push deeper or (if allowed) retreat. "One more room" tension without permanent loss on death.

### Session-Level (30-120 minutes)
A full descent through one guild's trial dungeon, from entrance to the golden sanctum payoff room, ending with a signature ability unlock — a complete, satisfying arc with a hard stop.

### Long-Term Progression
Unlocking new classes by completing their guild's trial; freely investing skill points across an ever-growing tree; collecting gear found in trials. The long-term goal is completing every guild's trial and mastering a preferred build.

### Retention Hooks
- **Curiosity**: Which guild's trial and signature ability is next; what the next golden sanctum will look like.
- **Investment**: Skill points already invested; gear found in prior trials.
- **Social**: Not a design focus (single-player).
- **Mastery**: Combo depth and difficulty of later trials give a clear skill ceiling to climb.

---

## Game Pillars

### Pillar 1: Earned Identity
Every signature class ability is earned through a meaningful in-world trial, not bought with abstract XP.

*Design test*: If we're debating between a menu-based unlock and an in-world trial for a class-defining ability, this pillar says we choose the trial.

### Pillar 2: Structured Growth, Earned Mastery
Character growth follows a defined skill tree per class for steady power progression, while each class's most powerful, class-defining signature abilities are earned exclusively through guild trials.

*Design test*: If we're debating whether a new ability belongs in the general skill tree or as a guild-trial reward, this pillar says: signature/class-defining abilities go through the trial; all other progression goes through the tree.

### Pillar 3: Darkness Earns Light
Every guild trial builds toward a contrasting moment of revelation — visual, narrative, or mechanical.

*Design test*: If we're debating whether a trial dungeon needs a big payoff room at the end, this pillar says yes, always.

### Pillar 4: Compact but Deep
Fewer, denser locations rather than sprawling open world; depth of systems over breadth of content.

*Design test*: If we're debating between adding a new biome and deepening an existing guild's trial, this pillar says deepen.

### Pillar 5: Fellowship of Rivals
Each guild/class faction has a distinct identity and worldview, creating flavor and light narrative tension.

*Design test*: If we're debating whether two guilds should feel similar or contrast strongly, this pillar says contrast.

### Anti-Pillars (What This Game Is NOT)

- **NOT an open, seamless open world**: Would compromise Pillar 4 (Compact but Deep) by spreading limited level-design time too thin.
- **NOT a way to obtain signature class abilities other than through guild trials** (no purchasing shortcuts): Would compromise Pillar 1 (Earned Identity) by devaluing the core progression fantasy.
- **NOT procedurally generated dungeons as primary content**: Would compromise Pillar 3 (Darkness Earns Light), which depends on hand-crafted pacing and staged payoff moments.
- **NOT PvP or online multiplayer**: Outside the core fantasy and would blow the "months" timeline.

---

## Inspiration and References

| Reference | What We Take From It | What We Do Differently | Why It Matters |
| ---- | ---- | ---- | ---- |
| Dark Souls III / Elden Ring | Weighty exploration, dark-to-light environmental storytelling, deliberate combat feel | Faster/more responsive combat than classic Souls stamina-management; compact levels instead of an open world | Validates that dark-fantasy exploration + earned power is a proven, beloved combination |
| Skyrim (guild questlines) | Faction-based progression, each guild has its own quest identity | Guild questline is a single dense dungeon-descent, not a multi-quest arc | Validates that "join a guild, prove yourself, get a title/power" is a strong, replicable structure |
| Diablo 4 / Pirateria (class quests) | Class identity gated behind a unique, class-specific quest/challenge | Applied to every class uniformly as the core loop's spine, not a one-off intro quest | Directly matches the player's own strongest motivator (per Creative Brief) |
| Project: Shadowglass (visual reference) | Grainy, retro-rendered dark fantasy atmosphere; scanline/dungeon-crawl mood | Combined with clean pixel-art in "golden sanctum" payoff rooms for contrast, rather than uniform grain throughout | Directly sourced from the project's `references/` folder; defines the Visual Identity Anchor |

**Non-game inspirations**: The specific memory of first reaching Elden Ring's Altus Plateau — moving from a cramped, hostile space into a sudden, overwhelming vista of golden light — is the game's central visual and emotional beat, deliberately repeated at the end of every guild trial.

---

## Target Player Profile

| Attribute | Detail |
| ---- | ---- |
| **Age range** | 18-35 |
| **Gaming experience** | Mid-core to hardcore |
| **Time availability** | 30–120 minute sessions, evenings/weekends |
| **Platform preference** | PC (Steam/Epic) |
| **Current games they play** | Dark Souls III, Elden Ring, Skyrim, Diablo 4 |
| **What they're looking for** | Earned, tangible character progression tied to real challenges rather than idle grinding |
| **What would turn them away** | Sluggish or unresponsive combat, padded/generic dungeon content, forced multiplayer/PvP |

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Recommended Engine** | Undecided — to be resolved via `/setup-engine`, informed by this concept and PC-only target |
| **Key Technical Challenges** | Responsive 3D melee/spell combat feel (input buffering, hit detection, i-frames); pixelated/retro 3D rendering pipeline (post-process shader, low-res render targets); staged lighting for dark→gold set-piece payoff rooms |
| **Art Style** | 3D stylized — pixelated/retro-rendered ("Shadowglass Grain" in dungeons, clean "Golden Sanctum Pixel" in payoff rooms and hub) |
| **Art Pipeline Complexity** | Medium (low-poly 3D models + custom pixelation/post-process shader; no need for high-fidelity PBR texturing) |
| **Audio Needs** | Moderate — weighty combat sound design is critical to Pillar 2; ambient/atmospheric music per guild identity |
| **Networking** | None (single-player) |
| **Content Volume (MVP)** | 1 hub + 2–3 guild trial dungeons (one per class), each ~15–30 min of content |
| **Procedural Systems** | None — explicitly excluded by Anti-Pillars (hand-crafted trial dungeons only) |

---

## Risks and Open Questions

### Design Risks
- Balancing a fully free-spend skill tree against mandatory guild-earned signature abilities may create builds that feel either too similar or wildly unbalanced.
- Fast/responsive combat needs to still feel "weighty" per Pillar 2 — these two feels can fight each other if not tuned carefully.

### Technical Risks
- No completed 3D project to date (prior work: an unfinished Godot 2D RPG and a Hogwarts-management sim); this is effectively a first 3D project.
- Achieving a convincing, performant pixelated 3D rendering style (not just a 2D sprite look) is a nontrivial shader/rendering pipeline problem.
- Real-time combat feel (hit detection, animation responsiveness, i-frame windows) is one of the hardest things to get right in any engine, more so under a compressed timeline.

### Market Risks
- The dark-fantasy action-RPG/dungeon-crawler space is well-served by AAA titles (Souls-likes, Diablo); a solo/small-scope title must lean hard on its specific hook (guild-trial structure + earned signature abilities) to stand out.

### Scope Risks
- "Months" timeline is aggressive for 3D combat + staged lighting set-pieces + multiple guild dungeons; MVP must be ruthlessly scoped to 2–3 dungeons before any expansion.
- First 3D project risk compounds scope risk — expect the first dungeon to take disproportionately longer as pipeline and tooling get established.

### Open Questions
- Is the combat feel (fast/responsive + weighty) achievable within the timeline in the chosen engine? → Resolve with `/prototype` on core combat immediately after `/setup-engine`, before writing full GDDs.
- How many classes/guilds are realistic for MVP — 2 or 3? → Resolve after the combat prototype gives a real per-dungeon time cost.
- Exact pixelation/rendering technique (render-target downscaling vs. shader-based pixelation vs. vertex snapping) — needs a technical spike once the engine is chosen.

---

## MVP Definition

**Core hypothesis**: Players find the "descend through darkness → fight → earn a signature ability in a golden payoff room" loop compelling enough to want to repeat it for a second and third class.

**Required for MVP**:
1. One fully realized guild trial dungeon (entrance → combat encounters → golden sanctum payoff room → signature ability unlock)
2. Core combat system: dodge, combo chains, at least one class's full moveset plus its signature ability
3. Free-spend skill tree for that one class, with visible stat/feel impact
4. Minimal hub space to select/return to the guild and view progress

**Explicitly NOT in MVP** (defer to later):
- Additional guilds/classes beyond the first 1–2
- Deep narrative/dialogue systems (environmental storytelling only)
- Full loot/economy systems beyond basic trial rewards
- Any multiplayer or online features

### Scope Tiers (if budget/time shrinks)

| Tier | Content | Features | Timeline |
| ---- | ---- | ---- | ---- |
| **MVP** | 1 guild trial dungeon, 1 class | Core combat + skill tree + signature ability | 6–8 weeks |
| **Vertical Slice** | 1 guild trial dungeon, fully polished | Core combat + skill tree + signature ability + hub | 10–12 weeks |
| **Alpha** | 2–3 guild trial dungeons, all classes | All core systems, rough polish | 4–5 months |
| **Full Vision** | 5+ guilds/classes, multiple biomes, hub NPCs/side content | All features, polished | Multi-year (not targeted now) |

---

## Next Steps

- [ ] Get concept approval from creative-director
- [ ] Fill in CLAUDE.md technology stack based on engine choice (`/setup-engine`)
- [ ] Create game pillars document (`/design-review` to validate)
- [ ] **Prototype core idea** (`/prototype core-combat`) — before writing GDDs, validate the combat feel and pixelated 3D rendering approach are achievable in the chosen engine
- [ ] If prototype PROCEEDS: Decompose concept into systems (`/map-systems`)
- [ ] Design each system (`/design-system [system-name]`) — use prototype learnings in Tuning Knobs and Formulas sections
- [ ] Build vertical slice in Pre-Production (`/vertical-slice`) — validate full game loop before committing to Production
- [ ] Validate core loop with playtest (`/playtest-report`)
- [ ] Plan first milestone (`/sprint-plan new`)
