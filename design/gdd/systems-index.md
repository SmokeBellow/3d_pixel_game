# Systems Index: Hollow Vow

> **Status**: Approved
> **Created**: 2026-08-10
> **Last Updated**: 2026-08-10
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

> **Update 2026-08-10**: camera perspective pivoted to first-person (standard
> FPS mouselook, no third-person orbit rig) while authoring the Input System
> GDD. `game-concept.md` and the Camera System row below reflect this; the
> Combat System GDD (not yet started) must be authored first-person-first —
> do not carry over third-person assumptions from the core-combat prototype
> without re-validating them.

Hollow Vow's mechanical scope centers on one repeatable loop: descend through a
hand-crafted guild trial dungeon using fast, responsive first-person melee/spell
combat with positioning-based evasion (mouselook doubles as both awareness and
evasion — there is no dodge or block), survive checkpointed encounters against
telegraphing
enemies, and emerge into a staged golden sanctum to earn a class-defining
signature ability. The MVP is deliberately narrow — one dungeon, one class — so
almost every system below is MVP-priority; only the skill tree (a post-MVP,
numeric-only power-tuning layer per Pillar 2) is deferred to Vertical Slice.
Combat System, Save/Load & Persistence, and Guild Trial Dungeon System are the
three bottleneck systems most other systems depend on — get these right first.

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Input System (inferred) | Core | MVP | Designed | [input-system.md](input-system.md) | — |
| 2 | Camera System (inferred) | Core | MVP | Not Started | — | Input System — **first-person, standard FPS mouselook; no third-person orbit rig** (pivot decided 2026-08-10 during Input System GDD authoring — see game-concept.md Technical Considerations) |
| 3 | Health & Damage System (inferred) | Gameplay | MVP | Not Started | — | — |
| 4 | Save/Load & Persistence (inferred) | Persistence | MVP | Not Started | — | — |
| 5 | Pixelation Rendering Pipeline | Core | MVP | Not Started | — | — |
| 6 | Audio System (inferred) | Audio | MVP | Not Started | — | — |
| 7 | Combat System | Gameplay | MVP | Not Started | — | Input System, Camera System, Health & Damage System |
| 8 | Enemy AI System (inferred) | Gameplay | MVP | Not Started | — | Health & Damage System, Combat System (hit-detection contract) |
| 9 | Checkpoint & Death System (inferred) | Gameplay | MVP | Not Started | — | Save/Load & Persistence, Health & Damage System |
| 10 | Guild Trial Dungeon System | Gameplay | MVP | Not Started | — | Combat System, Enemy AI System, Checkpoint & Death System, Camera System |
| 11 | Signature Ability System | Progression | MVP | Not Started | — | Combat System |
| 12 | Lighting & Staging System | Core | MVP | Not Started | — | Pixelation Rendering Pipeline |
| 13 | Loot & Trial Rewards | Economy | MVP | Not Started | — | Guild Trial Dungeon System, Save/Load & Persistence |
| 14 | Guild Narrative Delivery | Narrative | MVP | Not Started | — | Guild Trial Dungeon System, Hub Sanctuary System |
| 15 | Hub Sanctuary System | UI | MVP | Not Started | — | Save/Load & Persistence, Guild Trial Dungeon System |
| 16 | Golden Sanctum Sequence | Presentation | MVP | Not Started | — | Guild Trial Dungeon System, Signature Ability System, Lighting & Staging System |
| 17 | Combat/Trial UI (inferred) | UI | MVP | Not Started | — | Combat System, Health & Damage System, Loot & Trial Rewards |
| 18 | Skill Tree / Progression System | Progression | Vertical Slice | Not Started | — | Combat System, Save/Load & Persistence, Checkpoint & Death System |

---

## Categories

| Category | Description | Typical Systems |
|----------|-------------|-----------------|
| **Core** | Foundation systems everything depends on | Input, camera, rendering pipeline, lighting/staging |
| **Gameplay** | The systems that make the game fun | Combat, enemy AI, health/damage, checkpoints, dungeon structure |
| **Progression** | How the player grows over time | Signature abilities, skill tree |
| **Economy** | Resource creation and consumption | Loot & trial rewards (deliberately minimal per MVP scope) |
| **Persistence** | Save state and continuity | Save/load |
| **UI** | Player-facing information displays | Hub sanctuary, combat/trial HUD |
| **Audio** | Sound and music systems | Audio system |
| **Narrative** | Story and dialogue delivery | Guild narrative delivery (entrance monologue + environmental storytelling) |
| **Presentation** | Scripted sequences wrapping gameplay | Golden sanctum sequence |

---

## Priority Tiers

| Tier | Definition | Target Milestone | Design Urgency |
|------|------------|------------------|----------------|
| **MVP** | Required for the core loop to function — 1 guild trial dungeon, 1 class | First playable prototype | Design FIRST |
| **Vertical Slice** | Skill tree, introduced once the verb/number split with signature abilities can be validated | Vertical slice / demo | Design SECOND |
| **Alpha** | Content scaling to 2–3 guild trial dungeons, all classes; per-guild golden sanctum variance content | Alpha milestone | Design THIRD (mostly content authored via existing systems, not new systems) |
| **Full Vision** | 5+ guilds/classes, multiple biomes, hub NPCs/side content | Beta / Release | Design as needed — not yet enumerated |

---

## Dependency Map

### Foundation Layer (no dependencies)

1. **Input System** — keyboard/mouse primary, partial gamepad; nothing else can be tested without it
2. **Camera System** — third-person orbit/follow, already partially validated in the core-combat prototype
3. **Health & Damage System** — HP and damage math needed by both combat and death handling
4. **Save/Load & Persistence** — cross-session progress; nothing about guild/class/loot state survives a session without it
5. **Pixelation Rendering Pipeline** — the custom shader/render-target pipeline defining the visual identity; independent of gameplay logic
6. **Audio System** — SFX/music playback framework; independent of gameplay logic

### Core Layer (depends on foundation)

1. **Combat System** — depends on: Input System, Camera System, Health & Damage System
2. **Enemy AI System** — depends on: Health & Damage System, Combat System's shared hit-detection contract
3. **Checkpoint & Death System** — depends on: Save/Load & Persistence, Health & Damage System

### Feature Layer (depends on core)

1. **Guild Trial Dungeon System** — depends on: Combat System, Enemy AI System, Checkpoint & Death System, Camera System
2. **Signature Ability System** — depends on: Combat System
3. **Lighting & Staging System** — depends on: Pixelation Rendering Pipeline
4. **Loot & Trial Rewards** — depends on: Guild Trial Dungeon System, Save/Load & Persistence
5. **Guild Narrative Delivery** — depends on: Guild Trial Dungeon System, Hub Sanctuary System
6. **Hub Sanctuary System** — depends on: Save/Load & Persistence, Guild Trial Dungeon System
7. **Skill Tree / Progression System** (Vertical Slice) — depends on: Combat System, Save/Load & Persistence, Checkpoint & Death System

### Presentation Layer (depends on features)

1. **Golden Sanctum Sequence** — depends on: Guild Trial Dungeon System, Signature Ability System, Lighting & Staging System
2. **Combat/Trial UI** — depends on: Combat System, Health & Damage System, Loot & Trial Rewards

### Polish Layer (depends on everything)

None enumerated yet at concept stage — MVP explicitly excludes deep economy, dialogue, and multiplayer systems (see game-concept.md Anti-Pillars and MVP Definition).

---

## Recommended Design Order

| Order | System | Priority | Layer | Agent(s) | Est. Effort |
|-------|--------|----------|-------|----------|-------------|
| 1 | Input System | MVP | Foundation | gameplay-programmer | S |
| 2 | Camera System | MVP | Foundation | gameplay-programmer | S — core-combat prototype already validates the orbit/shake approach |
| 3 | Health & Damage System | MVP | Foundation | systems-designer | S |
| 4 | Save/Load & Persistence | MVP | Foundation | engine-programmer | M |
| 5 | Pixelation Rendering Pipeline | MVP | Foundation | unity-shader-specialist | L — unresolved rendering-technique open question from game-concept.md |
| 6 | Audio System | MVP | Foundation | audio-director | S |
| 7 | Combat System | MVP | Core | game-designer, systems-designer | L — bottleneck system; use core-combat prototype learnings directly (combo legibility requires real windup/swing animation, not just timing) |
| 8 | Enemy AI System | MVP | Core | ai-programmer | L — must resolve the accepted "no dodge, no block/parry" risk via enemy pacing/telegraph design (see game-concept.md Design Risks) |
| 9 | Checkpoint & Death System | MVP | Core | systems-designer | M |
| 10 | Guild Trial Dungeon System | MVP | Feature | level-designer | L — must resolve open questions on branching structure and loot/lore retry persistence |
| 11 | Signature Ability System | MVP | Feature | systems-designer, game-designer | M |
| 12 | Lighting & Staging System | MVP | Feature | unity-shader-specialist, technical-artist | M |
| 13 | Loot & Trial Rewards | MVP | Feature | economy-designer | S — deliberately minimal per MVP scope |
| 14 | Guild Narrative Delivery | MVP | Feature | narrative-director, writer | M |
| 15 | Hub Sanctuary System | MVP | Feature | game-designer, ui-programmer | S |
| 16 | Golden Sanctum Sequence | MVP | Presentation | technical-artist, gameplay-programmer | M |
| 17 | Combat/Trial UI | MVP | Presentation | ui-programmer, ux-designer | S |
| 18 | Skill Tree / Progression System | Vertical Slice | Feature | systems-designer, economy-designer | M — must validate the Pillar 2 verb/number split holds up in practice |

---

## Circular Dependencies

- None found. Combat System and Enemy AI System are tightly coupled but not
  circular: Combat System defines a shared attacker/target hit-detection
  contract; Enemy AI System consumes it without requiring changes back to
  Combat System.

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|--------|-----------|-----------------|------------|
| Enemy AI System | Design | No dodge and no block/parry verb exist (game-concept.md accepted risk) — enemy pacing/telegraphs are the *only* defense against unrecoverable "cornered" situations in fast combo combat | Explicitly flagged in game-concept.md Open Questions: validate with a multi-enemy encounter test before this GDD is finalized — the core-combat prototype only tested single-target combat |
| Combat System | Technical/Design | Combo legibility confirmed broken without real animation in the core-combat prototype (procedural tween alone wasn't enough) | Budget real windup/attack animation or motion-designed VFX/trail explicitly in this GDD's scope, not just timing formulas |
| Camera System / Combat System | Design | Camera perspective pivoted to first-person (2026-08-10) AFTER the core-combat prototype validated hit-feedback and combat feel third-person — none of that prototype's findings are guaranteed to hold in first-person | Run a follow-up spike re-validating hit-stop/knockback/camera-shake feel and combo legibility in first-person before the Combat System GDD is finalized (not yet scheduled — accepted risk for now per game-concept.md) |
| Pixelation Rendering Pipeline | Technical | Exact technique (render-target downscaling vs. shader-based pixelation vs. vertex snapping) is still an open question — never spiked | Run a technical spike (`/prototype --spike`) before writing this GDD's Formulas section |
| Guild Trial Dungeon System | Scope | First 3D project + hand-crafted-only dungeons (Anti-Pillar rules out procedural generation) — level-design effort per dungeon may be underestimated even at the revised 10–12 week MVP timeline | Cost the first dungeon by room/encounter count explicitly in this GDD; re-validate the MVP timeline after |
| Skill Tree / Progression System | Design | Verb/number split with Signature Ability System (Pillar 2) is a new, untested design rule — must hold up once real numbers exist | Design this GDD adversarially against Pillar 2's design test before implementation; run `/design-review` with extra scrutiny on this point |

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | 18 |
| Design docs started | 1 |
| Design docs reviewed | 0 |
| Design docs approved | 0 |
| MVP systems designed | 1/17 |
| Vertical Slice systems designed | 0/1 |

---

## Next Steps

- [x] Review and approve this systems enumeration
- [ ] Design MVP-tier systems first (use `/design-system [system-name]`), starting with Input System per the Recommended Design Order
- [ ] Run `/design-review` on each completed GDD
- [ ] Run `/gate-check pre-production` when MVP systems are designed
- [ ] Validate the highest-risk systems (Enemy AI multi-enemy encounter, Pixelation rendering technique) with a spike or `/vertical-slice` before committing to Production
