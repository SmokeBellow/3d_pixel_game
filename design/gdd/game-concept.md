# Game Concept: Hollow Vow

*Created: 2026-08-09*
*Status: Draft*
*Revised: 2026-08-10 — addressed /design-review MAJOR REVISION NEEDED findings (skill tree cut from MVP, dodge removed, death penalty, dungeon length reconciled, Scope Tiers rewritten). See `design/gdd/reviews/game-concept-review-log.md`.*
*Revised: 2026-08-10 (second pass) — locked camera perspective to first-person with standard FPS mouselook (no third-person orbit). This was never explicit in this document before (only implicit in the core-combat prototype's third-person camera code) — see Technical Considerations and Core Mechanics.*

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
| **Session Length** | 30–60 min (one guild trial per session — reconciled with per-dungeon content estimate; see Core Loop) |
| **Monetization** | Premium (none yet decided) |
| **Estimated Scope** | Medium (10–12 weeks MVP → 5–6 months Alpha, solo dev; MVP: hub + 1 guild trial dungeon, Alpha: 2–3 dungeons — see Scope Tiers) |
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
| **Challenge** (obstacle course, mastery) | 2 | Fast, responsive combat with combo chains, positioning-based evasion (no dedicated dodge/i-frame verb — see Core Mechanics), and punishing enemy patterns in trial dungeons |
| **Fellowship** (social connection) | 4 | Each guild has a distinct identity/worldview; light narrative rivalry between factions |
| **Discovery** (exploration, secrets) | 3 | Dense, compact trial dungeons with hidden loot and lore fragments |
| **Expression** (self-expression, creativity) | 3 | Skill tree per class (post-MVP), differentiated combo visuals per build |
| **Submission** (relaxation, comfort zone) | N/A | Not a design goal — the game is deliberately tense, not relaxing |

### Key Dynamics (Emergent player behaviors)

- Players will replay or revisit earlier guild trials once a new signature ability changes how their combat feels (and, post-MVP, once new skill-tree points do the same).
- Players will compare which guild's signature ability to prioritize first based on their preferred combat style.
- Players will treat the "golden sanctum" payoff room as a checkpoint worth pushing hard for, even at low resources ("one more push" psychology).

### Core Mechanics (Systems we build)

1. Fast, responsive real-time melee/spell combat, played entirely in **first-person** (standard FPS mouselook — mouse turns the character/view directly; no separate third-person orbit camera), with positioning-based evasion and combo chains (per-class movesets) — defense comes from reading enemy tells and moving/turning out of danger, not a dedicated dodge/i-frame button; combo legibility requires a visible windup/swing cue, delivered as a first-person viewmodel weapon animation (confirmed by the core-combat prototype — see Technical Considerations, though the prototype itself was built third-person and needs re-validation for first-person feel)
2. Guild trial dungeons: hand-crafted, linear-to-branching descents (30–60 min, with in-trial checkpoints) ending in a signature-ability payoff chamber
3. Skill tree per class, introduced **post-MVP**, that only tunes numeric power (damage, cooldowns, resource costs) on combat verbs the player already has — it can never grant a new verb; only guild-earned signature abilities do that (see Pillar 2)
4. Hub sanctuary with guild factions, each with distinct visual/narrative identity

---

## Player Motivation Profile

### Primary Psychological Needs Served

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** (freedom, meaningful choice) | Choice of which guild/class to pursue and in what order; skill tree (post-MVP) for freedom in how to tune an earned build | Core |
| **Competence** (mastery, skill growth) | Visible growth in combo depth and damage output; hard-won signature abilities | Core |
| **Relatedness** (connection, belonging) | Distinct guild factions with their own worldview create a sense of joining something | Supporting |

### Player Type Appeal (Bartle Taxonomy)

- [x] **Achievers** (goal completion, collection, progression) — How: earning signature abilities per class (MVP), plus fully-invested skill trees post-MVP
- [x] **Explorers** (discovery, understanding systems, finding secrets) — How: dense trial dungeons with hidden loot/lore, discovering each guild's distinct mechanical identity
- [ ] **Socializers** (relationships, cooperation, community) — not a design target; single-player
- [ ] **Killers/Competitors** (domination, PvP, leaderboards) — explicitly excluded (see Anti-Pillars)

### Flow State Design

- **Onboarding curve**: First guild trial is the tutorial — teaches core combat verbs (positioning-based evasion, combo) at a forgiving pace before the first real challenge encounter. No block/parry verb — see the accepted risk on defense below.
- **Difficulty scaling**: Later guild trials assume the player has at least one signature ability (and, post-MVP, skill-tree points); enemy patterns increase in speed and punish read errors more heavily.
- **Feedback clarity**: Clear hit-stop/audio on landed hits and blocks, with a visible windup/swing cue so combo timing reads clearly (see Technical Considerations); skill tree UI (post-MVP) shows immediate stat deltas; signature ability unlock is a scripted, unmistakable moment.
- **Recovery from failure**: Death costs something real, not just time. On death, the player respawns at the most recently reached in-trial checkpoint (not the trial's entrance — checkpoints exist so repeated deaths don't force a full re-traversal of a 30–60 minute trial) and permanently loses a small amount of invested skill-tree power (spent points are refunded to the pool, requiring re-investment; pre-MVP, before the tree exists, this penalty is deferred/waived). This exists specifically to close the exploit where a zero-cost death invites infinite reset-farming a single room, and keeps even the "bought" half of progression carrying real stakes. Exact loss formula is deferred to the progression system GDD.

---

## Core Loop

### Moment-to-Moment (30 seconds)
Fast, responsive melee/spell combat — position to avoid hits, land combo chains, react to enemy tells. Combat must feel good in total isolation: hit-stop, screen shake, weighty audio even at high speed, with a visible windup/swing cue so combo timing reads clearly.

### Short-Term (5-15 minutes)
Enter a room or corridor segment of a guild trial → fight or solve an environmental obstacle → find loot or lore → decide whether to push deeper or (if allowed) retreat. "One more room" tension — death costs real progress (see Flow State Design → Recovery from Failure), so this tension has teeth, not just a time cost.

### Session-Level (30-60 minutes)
A full descent through one guild's trial dungeon — with in-trial checkpoints softening the cost of repeated deaths — from entrance to the golden sanctum payoff room, ending with a signature ability unlock — a complete, satisfying arc with a hard stop.

### Long-Term Progression
Unlocking new classes by completing their guild's trial; investing skill points across an ever-growing tree (post-MVP) to tune combat verbs already earned; collecting gear found in trials. The long-term goal is completing every guild's trial and mastering a preferred build.

### Retention Hooks
- **Curiosity**: Which guild's trial and signature ability is next; what the next golden sanctum will look like.
- **Investment**: Signature abilities already earned; gear found in prior trials; post-MVP, skill points already invested.
- **Social**: Not a design focus (single-player).
- **Mastery**: Combo depth and difficulty of later trials give a clear skill ceiling to climb.

---

## Game Pillars

### Pillar 1: Earned Identity
Every signature class ability is earned through a meaningful in-world trial, not bought with abstract XP.

*Design test*: If we're debating between a menu-based unlock and an in-world trial for a class-defining ability, this pillar says we choose the trial.

### Pillar 2: Structured Growth, Earned Mastery
Character growth follows two channels that never overlap: guild-trial-earned signature abilities are the **only** source of new combat verbs (new moves/tools a class can perform); the skill tree (introduced post-MVP) only tunes numeric power — damage, cooldowns, resource costs — on verbs the player already has. The tree can never grant a new verb. This split exists specifically so the skill tree can never functionally substitute for an earned signature ability, protecting Pillar 1.

*Design test*: If we're debating whether a new capability changes what the player can **do** (a verb) or only **how well** they do it (a number), this pillar says: verbs are trial-only, numbers are tree-only.

### Pillar 3: Darkness Earns Light
Every guild trial builds toward a contrasting moment of revelation — visual, narrative, or mechanical.

*Design test*: If we're debating whether a trial dungeon needs a big payoff room at the end, this pillar says yes, always.

### Pillar 4: Compact but Deep
Fewer, denser locations rather than sprawling open world; depth of systems over breadth of content.

*Design test*: If we're debating between adding a new biome and deepening an existing guild's trial, this pillar says deepen.

### Pillar 5: Fellowship of Rivals
Each guild/class faction has a distinct identity and worldview, creating flavor and light narrative tension. Delivered primarily through a short guild-master monologue at each trial's entrance (stating that guild's worldview and motive for testing the player) plus environmental storytelling and item/lore fragments found within the trial — not through branching dialogue trees, which keeps scope lean while still giving this pillar a concrete delivery mechanism. The golden sanctum payoff room also carries guild identity visually: its gold lighting is tinted/accented by a color or elemental motif tied to that guild (e.g., cold silver-gold for a mage guild, warm ember-gold for a warrior guild), so the repeated dark→gold structural beat (Pillar 3) doesn't read identically across guilds.

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
| **Time availability** | 30–60 minute sessions, evenings/weekends |
| **Platform preference** | PC (Steam/Epic) |
| **Current games they play** | Dark Souls III, Elden Ring, Skyrim, Diablo 4 |
| **What they're looking for** | Earned, tangible character progression tied to real challenges rather than idle grinding |
| **What would turn them away** | Sluggish or unresponsive combat, padded/generic dungeon content, forced multiplayer/PvP |

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Recommended Engine** | Unity 6.3 LTS, C# — decided via `/setup-engine` |
| **Camera Perspective** | **First-person**, standard FPS mouselook (mouse turns the player character directly — no separate third-person orbit rig). Decided after the core-combat prototype was already built and validated third-person; a follow-up spike is planned (not yet scheduled) to re-validate hit feedback (knockback, hit-stop, camera shake) and combo legibility specifically in first-person before the combat system GDD is finalized. This pivot also better matches the "Project: Shadowglass" visual reference (grainy first-person dungeon crawl aesthetic), which the third-person prototype had actually drifted away from. |
| **Key Technical Challenges** | Responsive first-person melee/spell combat feel (input buffering, hit detection, positioning-based evasion timing — turning to track threats now doubles as the primary defensive tool, since there's no dodge or block); **combo legibility** — combo timing must be communicated visually via a first-person viewmodel weapon animation or VFX cue, confirmed necessary in principle by the core-combat prototype (a code-correct combo timer read as "completely unclear" without one — see `prototypes/core-combat-concept/REPORT.md`), though that finding was under third-person and needs first-person re-validation; pixelated/retro 3D rendering pipeline (post-process shader, low-res render targets); staged lighting for dark→gold set-piece payoff rooms, varied per guild via a color/elemental motif (see Pillar 5) |
| **Art Style** | 3D stylized — pixelated/retro-rendered ("Shadowglass Grain" in dungeons, clean "Golden Sanctum Pixel" in payoff rooms and hub) |
| **Art Pipeline Complexity** | Medium (low-poly 3D models + custom pixelation/post-process shader; no need for high-fidelity PBR texturing) |
| **Audio Needs** | Moderate — weighty combat sound design is critical to the Sensation aesthetic and confirmed hit-feedback feel (see Player Experience Analysis, core-combat prototype); ambient/atmospheric music per guild identity |
| **Networking** | None (single-player) |
| **Content Volume (MVP)** | 1 hub + 1 guild trial dungeon (1 class), ~30–60 min of content; 2–3 dungeons is the Alpha tier — see Scope Tiers |
| **Procedural Systems** | None — explicitly excluded by Anti-Pillars (hand-crafted trial dungeons only) |

---

## Risks and Open Questions

### Design Risks
- The skill tree/signature-ability split (Pillar 2: verbs are trial-only, numbers are tree-only) must be enforced strictly during `/design-system` — any future feature that blurs this line (e.g., a tree node that grants a new move, not just tunes an existing one) directly compromises Pillar 1.
- Combo legibility — confirmed by the core-combat prototype to require real windup/swing animation or VFX, not just correct timing code — is now a named production requirement, not a nice-to-have (see Technical Considerations).
- The permanent skill-tree-power death penalty (see Flow State Design → Recovery from Failure) needs a concrete loss formula in the progression GDD that feels fair rather than punishing-for-punishment's-sake, especially stacked with in-trial checkpoints.
- **Accepted risk — no burst-mobility/disengage verb**: with dodge removed entirely, positioning-based evasion is the only defensive tool and there's no dedicated block/parry either. This risks unrecoverable "cornered" situations in a fast-combo game once an enemy closes distance, which could read as unwinnable rather than skill-testable (in tension with the Challenge aesthetic and the "fast, responsive" pillar). **Compounded by the first-person pivot**: mouselook now has to do double duty as both awareness (seeing threats) and evasion (turning/moving away) — there's no third-person peripheral view to compensate. **Decision**: accept this risk at concept stage rather than add a new verb now; resolve entirely through enemy pacing/telegraph design (attack windup length, movement speed, never fully surrounding the player) in the combat system GDD. This must be explicitly validated with a multi-enemy encounter in the next combat prototype or vertical slice — the core-combat prototype only tested single-target combat in isolation, and did so third-person.

### Technical Risks
- No completed 3D project to date (prior work: an unfinished Godot 2D RPG and a Hogwarts-management sim); this is effectively a first 3D project. **Confirmed by the core-combat prototype**: a single combat mechanic (movement, combo, hit feedback) took a full prototyping session, and combo legibility is still an open production task.
- Achieving a convincing, performant pixelated 3D rendering style (not just a 2D sprite look) is a nontrivial shader/rendering pipeline problem — not yet spiked.
- Real-time combat feel (hit detection, animation responsiveness, positioning-based evasion timing) is one of the hardest things to get right in any engine, more so under a compressed timeline. **Partially resolved, perspective now changed**: the core-combat prototype confirmed fast/responsive input and weighty hit feedback (knockback, hit-stop, camera shake) coexist without fighting each other in **third-person** — the game has since pivoted to first-person (see Technical Considerations), so this finding is directional evidence, not a guarantee; it must be re-validated in first-person, along with combo legibility, before the combat system GDD is finalized.
- **New risk from the first-person pivot**: with no dodge, no block/parry, and no third-person orbit, mouselook (turning to track a threat) is now the player's only tool for both awareness and evasion simultaneously. This raises the stakes on the previously accepted "no burst-mobility verb" risk (see Design Risks) — encounter and enemy-telegraph design now carries even more weight than it did under the third-person assumption.

### Market Risks
- The dark-fantasy action-RPG/dungeon-crawler space is well-served by AAA titles (Souls-likes, Diablo); a solo/small-scope title must lean hard on its specific hook (guild-trial structure + earned signature abilities) to stand out.

### Scope Risks
- "Months" timeline is aggressive for 3D combat + staged lighting set-pieces + multiple guild dungeons; MVP is now ruthlessly scoped to 1 dungeon/1 class (see MVP Definition and Scope Tiers), with 2–3 dungeons deferred to the Alpha tier.
- First 3D project risk compounds scope risk — expect the first dungeon to take disproportionately longer as pipeline and tooling get established; this is already reflected in the revised MVP timeline (10–12 weeks, up from an initial 6–8 week estimate the core-combat prototype's pace didn't support).

### Open Questions
- ~~Is the combat feel (fast/responsive + weighty) achievable within the timeline in the chosen engine?~~ **RESOLVED** via `/prototype core-combat` (2026-08-10, 2 iterations): PROCEED verdict — fast/responsive input and weighty hit feedback coexist; combo legibility requires real animation/VFX investment in production, tracked as a named technical challenge above. See `prototypes/core-combat-concept/REPORT.md`.
- ~~How many classes/guilds are realistic for MVP — 2 or 3?~~ **RESOLVED**: MVP = 1 class/guild (see MVP Definition); 2–3 is the Alpha tier (see Scope Tiers).
- Exact pixelation/rendering technique (render-target downscaling vs. shader-based pixelation vs. vertex snapping) — needs a technical spike; still open.
- Exact branching structure for "linear-to-branching descents" (branch width, reconvergence, backtracking to locked shortcuts) — deferred to the level-design GDD.
- Whether hidden loot/lore found in a trial stays permanently collected across in-trial checkpoint respawns and full trial retries, or is re-lootable — deferred to the level-design GDD.
- Exact skill-tree-power loss formula on death (see Flow State Design → Recovery from Failure) — deferred to the progression system GDD.
- Whether enemy pacing/telegraph design alone (no dodge, no block/parry, now first-person mouselook as the only tracking/evasion tool) can avoid unrecoverable "cornered" situations against multiple enemies — accepted as a risk for now (see Design Risks); must be validated with a multi-enemy encounter test before the combat system GDD is finalized, since the core-combat prototype only tested single-target combat, and did so third-person.
- **New**: whether third-person hit-feedback findings (knockback, hit-stop, camera shake, combo legibility) hold up in first-person — a follow-up spike is planned but not yet scheduled (accepted risk for now; see Technical Considerations → Camera Perspective).

---

## MVP Definition

**Core hypothesis**: Players find the "descend through darkness → fight → earn a signature ability in a golden payoff room" loop compelling enough to want to repeat it for a second and third class.

**Required for MVP**:
1. One fully realized guild trial dungeon (30–60 min, entrance → in-trial checkpoint(s) → combat encounters → golden sanctum payoff room → signature ability unlock)
2. Core combat system: positioning-based evasion, combo chains with a legible windup/swing animation cue (not just code-correct timing — see Technical Considerations), at least one class's full moveset plus its signature ability
3. Minimal hub space to select/return to the guild and view progress

**Explicitly NOT in MVP** (defer to later):
- Skill tree (deferred to Vertical Slice tier — introduced only once the verb/number split with signature abilities, per Pillar 2, can be validated)
- Additional guilds/classes beyond the first 1
- Deep narrative/dialogue systems (guild-master entrance monologue + environmental storytelling only — see Pillar 5)
- Full loot/economy systems beyond basic trial rewards
- Any multiplayer or online features

### Scope Tiers (if budget/time shrinks)

| Tier | Content | Features | Timeline |
| ---- | ---- | ---- | ---- |
| **MVP** | 1 guild trial dungeon, 1 class | Core combat (positioning-based evasion + legible combo swing) + signature ability + in-trial checkpoints + minimal hub | 10–12 weeks |
| **Vertical Slice** | 1 guild trial dungeon, fully polished | + skill tree (post-MVP, numeric-only per Pillar 2) introduced and validated | 14–16 weeks |
| **Alpha** | 2–3 guild trial dungeons, all classes | All core systems, per-guild golden sanctum color/elemental variance (Pillar 5), rough polish | 5–6 months |
| **Full Vision** | 5+ guilds/classes, multiple biomes, hub NPCs/side content | All features, polished | Multi-year (not targeted now) |

> **Timeline note**: these numbers reflect the core-combat prototype's actual pace — a single combat mechanic took a full prototyping session before combo legibility was even partially resolved (see `prototypes/core-combat-concept/REPORT.md`). Treat even the revised numbers above as still optimistic for a first completed 3D project; re-validate after the first dungeon's combat + one full guild trial are built.

---

## Next Steps

- [ ] Get concept approval from creative-director
- [x] Fill in CLAUDE.md technology stack based on engine choice (`/setup-engine`) — Unity 6.3 LTS, C#
- [ ] Create game pillars document (`/design-review` to validate)
- [x] **Prototype core idea** (`/prototype core-combat`) — PROCEED verdict (2 iterations); see `prototypes/core-combat-concept/REPORT.md`
- [ ] Decompose concept into systems (`/map-systems`)
- [ ] Design each system (`/design-system [system-name]`) — use prototype learnings (combo legibility requires real animation) in Tuning Knobs and Formulas sections; enforce the Pillar 2 verb/number split explicitly in the progression GDD
- [ ] Build vertical slice in Pre-Production (`/vertical-slice`) — validate full game loop before committing to Production
- [ ] Validate core loop with playtest (`/playtest-report`)
- [ ] Plan first milestone (`/sprint-plan new`)
