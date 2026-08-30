# Health & Damage System

> **Status**: Designed (pending review)
> **Author**: user + agents
> **Last Updated**: 2026-08-26
> **Implements Pillar**: Foundation (enables Pillar 1 "Магия лучше вместе" — Elemental Status System is built entirely on top of HP/damage; enables Pillar 2 "Каждая смерть — история" via a clean, readable death moment)
> **Creative Director Review (CD-GDD-ALIGN)**: skipped — Lean mode

## Overview

Health & Damage System owns the numeric core every combat interaction in
Covenant of Mages ultimately reduces to: how much HP a player or enemy has,
how damage is calculated and applied, and the exact moment a player or enemy
dies. It is the single source of truth other systems read from and write to —
Elemental Status System layers elemental flags on top of it, Enemy AI reads
it to decide behavior, Spectator/Death System reacts to its death event, and
Combat HUD displays its numbers directly. Beyond the raw numbers, this system
is what makes damage *legible*: per the game concept's design intent,
difficulty comes from enemy combination and elemental vulnerability, not from
raw HP inflation — so this system must expose damage in a way that reads
clearly (a hit landed, it mattered, here's how much) rather than as an opaque
number going down. Players don't manage Health & Damage System directly the
way they manage a spell loadout, but they feel it in every hit taken and
dealt — a mage who can't tell whether their last cast actually hurt something
will blame this system's feedback, not their own aim.

## Player Fantasy

Direct: the visceral certainty of "that spell landed and it mattered." A
mage's power fantasy isn't the cast animation — it's watching an enemy's HP
visibly drop, watching it stagger or flinch in response, and eventually
watching it die from a hit you or your team delivered. Damage feedback must
feel proportional and immediate — a heavy hit reads as heavy, a killing blow
reads as final. On the receiving end, taking damage must telegraph clearly
enough that death never feels cheap or unexplained: per Pillar 2 ("Каждая
смерть — история"), a player's death should be a moment they can point to and
explain ("I got caught between the fire wall and the boss slam"), not a
mystery HP drain. This system is the difference between combat feeling
*responsive* (numbers that clearly track cause and effect) and feeling
*arbitrary* (HP that changes for reasons the player can't reconstruct).

*Note: `creative-director` not consulted — Lean mode (Section B is not a
Section D/H high-risk section).*

## Detailed Design

### Core Rules

1. Every damageable entity (player character or enemy) carries a **Health
   component** with `currentHP` and `maxHP`. `currentHP` is
   server-authoritative (`NetworkVariable<float>`,
   `NetworkVariableWritePermission.Server`), following the exact pattern
   ADR-0001 already established for `_isDead` — clients read, only the server
   writes.
2. Damage is applied exclusively through a server-side method,
   `ApplyDamage(DamageInfo info)`, never by a client directly decrementing
   HP. `DamageInfo` carries: `amount` (float), `sourceSchool` (elemental
   school enum, for feedback/logging — this system does not interpret
   elemental rules, it just carries the tag), `isSynergyTriggered` (bool —
   flagged by Elemental Synergy System when a combo fired, so this system can
   surface the "special effect and sound" the game concept calls for), and
   `sourceObject`/`targetObject` (`NetworkObjectReference`).
3. Healing is applied exclusively through a parallel server-side method,
   `ApplyHeal(float amount, NetworkObjectReference source)`, which increases
   `currentHP` clamped to `maxHP` (see Formulas). Included in MVP scope now
   specifically so the API shape doesn't need to change when the Nature
   school (a later MVP+ addition) needs it.
4. **Max HP values are not defined by this system.** `maxHP` is set by
   whichever system spawns the entity (Enemy AI System for enemies, Player
   Controller / Character Progression for players) as configuration data on
   the Health component — Health & Damage System owns the *mechanism* (apply
   damage, clamp, detect death), not the specific numbers per enemy/player
   type. This keeps difficulty tuning in "enemy combination and
   vulnerability," per the game concept's explicit design intent, rather than
   in this GDD's tuning knobs.
5. **Death threshold**: when `ApplyDamage` or any other HP-reducing event
   brings `currentHP` to ≤ 0, the entity's Health component immediately
   transitions to `IsDead = true` (server-side) and fires an `OnDeath` event
   (server) that downstream systems subscribe to. For player entities, this
   directly sets the same `NetworkVariable<bool> _isDead` ADR-0001 already
   defines. For enemy entities, `IsDead` drives despawn (via
   `NetworkObject.Despawn()`, not `Destroy()`) after any death-reaction
   window Enemy AI System needs (e.g. a death animation) — the exact despawn
   timing is Enemy AI System's call, not this system's; Health & Damage only
   guarantees the `OnDeath` event fires exactly once per entity.
6. `currentHP` never goes negative and never exceeds `maxHP` — both
   `ApplyDamage` and `ApplyHeal` clamp their result (see Formulas).
7. This system does **not** implement passive HP regeneration over time —
   the game concept's "no raw HP scaling" design intent extends to no
   drip-regen; HP only changes through explicit `ApplyDamage`/`ApplyHeal`
   calls.

### States and Transitions

| State | Description | Transition |
|---|---|---|
| Alive | `currentHP > 0`, `IsDead = false`. Entity can take damage/healing normally. | → Dead when `ApplyDamage` brings `currentHP` to ≤ 0 |
| Dead | `currentHP = 0`, `IsDead = true`. `ApplyDamage`/`ApplyHeal` calls are ignored (see Edge Cases). | → Alive on respawn (player, via Checkpoint System) or entity despawn (enemy) |

### Interactions with Other Systems

- **Networking Foundation** (not yet designed, but ADR-0001 already
  establishes the pattern) — `currentHP`/`IsDead` are `NetworkVariable`s
  written only by the server; this system's methods must run server-side,
  matching the `CastSpellServerRpc` precedent.
- **Elemental Status System** (not yet designed, provisional) — does not
  write to HP directly; it applies its own status flags (Wet/Burning/etc.)
  and is expected to call `ApplyDamage` for any damage-over-time effects
  (e.g. Burning) it owns, tagging `sourceSchool` accordingly.
- **Spell Casting System** (not yet designed, provisional) — resolves hit
  detection and calls `ApplyDamage`/`ApplyHeal` with the appropriate
  `DamageInfo` once a cast server-validates a hit.
- **Elemental Synergy System** (not yet designed, provisional) — sets
  `isSynergyTriggered = true` on the `DamageInfo` it passes through when a
  combo (e.g. Chain Shock) fires, so this system's feedback layer can
  distinguish combo damage from normal damage.
- **Enemy AI System** (not yet designed, provisional) — subscribes to
  `OnDeath` to trigger death animation/loot/despawn timing; may also read
  `currentHP`/`maxHP` ratio to drive AI behavior (e.g. low-HP flee state),
  though that behavior itself belongs to Enemy AI System, not here.
- **Spectator/Death System** (not yet designed, provisional) — subscribes to
  the player's `OnDeath` event (mirrors `_isDead` becoming true) to trigger
  the Active→Spectator camera transition already defined in
  `camera-system.md`.
- **Combat HUD** (not yet designed, provisional) — reads `currentHP`/`maxHP`
  directly for display; read-only, no write access.
- **Checkpoint System** (not yet designed, provisional) — calls a
  `Revive()`/`ResetHP()` method (reverse of death) when respawning players at
  a checkpoint after a team wipe.

## Formulas

### synergy_damage_multiplier

The `synergy_damage_multiplier` formula is defined as:

`final_damage_amount = base_damage * synergy_multiplier`

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| Base damage | base_damage | float | ≥ 0 | Pre-synergy damage value, computed by Spell Casting System / Elemental Status System and passed into `DamageInfo` |
| Synergy multiplier | synergy_multiplier | float | ≥ 0, default 1.0 | Multiplier supplied by Elemental Synergy System when `isSynergyTriggered = true`; 1.0 when `isSynergyTriggered = false` |
| Final damage amount | final_damage_amount | float | ≥ 0, unbounded above | Value passed to `hp_clamp` as the magnitude of the damage delta |

**Output Range:** [0, ∞). Not clamped at this stage — any ceiling on burst damage comes from the `hp_clamp` clamp against `maxHP`, not from this formula. This system does not police "is 3x too strong" — that's an Elemental Synergy System balance decision; this system only needs `synergy_multiplier` to exist as an input with a safe default of 1.0 for non-synergy hits.
**Reference value:** `synergy_multiplier = 3.0` is used here strictly as the placeholder this formula would receive for a Chain Shock-style hit, matching the co-op-spellcasting prototype's hypothesis text. This is **not** a design ruling that Chain Shock = 3x — that belongs to the (not yet designed) Elemental Synergy System GDD, which owns which combos exist and what multiplier each produces.
**Example (normal hit):** base_damage = 20, synergy_multiplier = 1.0 → final_damage_amount = 20.0.
**Example (synergy hit):** base_damage = 20, synergy_multiplier = 3.0 → final_damage_amount = 60.0, isSynergyTriggered = true.

### hp_clamp

The `hp_clamp` formula is defined as:

`currentHP_new = clamp(currentHP_prev + delta_amount, 0, maxHP)`

Where `delta_amount` is `-final_damage_amount` for a damage event (from `synergy_damage_multiplier` above) or `+heal_amount` for a heal event. This is the single clamp rule applied by both `ApplyDamage` and `ApplyHeal` — they differ only in the sign of `delta_amount`, not in the clamp logic itself.

| Variable | Symbol | Type | Range | Description |
|---|---|---|---|---|
| Previous current HP | currentHP_prev | float | [0, maxHP] | HP value immediately before this call resolves |
| Delta amount | delta_amount | float | unbounded (typically -final_damage_amount or +heal_amount) | Signed change to apply; negative for damage, positive for healing |
| Maximum HP | maxHP | float | > 0, externally defined | Entity's max HP, set by the spawning system (not owned here) |
| New current HP | currentHP_new | float | [0, maxHP] | Resulting HP after clamp |

**Output Range:** [0, maxHP], always. Clamping is symmetric — overkill damage floors at 0 (triggering death, Core Rule 5), overheal ceilings at maxHP with no banking/shield mechanic.
**Example (damage):** currentHP_prev = 40, maxHP = 100, final_damage_amount = 65 → delta_amount = -65 → currentHP_new = clamp(-25, 0, 100) = 0 → death fires.
**Example (heal):** currentHP_prev = 90, maxHP = 100, heal_amount = 25 → delta_amount = +25 → currentHP_new = clamp(115, 0, 100) = 100 (15 HP of healing "wasted" — no overheal banking).

### Deferred: Defense/Mitigation

No defense/mitigation formula is defined in this GDD. **This is a deliberate deferral, not an oversight**: the game concept's design intent places difficulty in enemy combination and elemental vulnerability, not stat walls, and no equipment/defense stat spec exists yet in any approved document. The extension point is already reserved: a future `mitigation_multiplier` would slot in multiplicatively alongside `synergy_multiplier` —

`final_damage_amount = base_damage * synergy_multiplier * mitigation_multiplier`

— defaulting to 1.0 (no mitigation) until a future system (Character Progression / Loadout) defines it. No signature change to `DamageInfo` or `ApplyDamage` would be needed when that happens.

*Formulas proposed by `systems-designer` (2026-08-26).*

## Edge Cases

- **If `ApplyDamage` or `ApplyHeal` is called on an entity already in the Dead state**: both calls are silently ignored (no HP change, no duplicate `OnDeath` event, no exception) — a dead entity's HP is frozen at 0 until it explicitly transitions back to Alive (respawn/despawn).
- **If two lethal `ApplyDamage` calls arrive for the same entity in the same server tick**: whichever call is processed first brings `currentHP` to 0 and fires `OnDeath`; the second call is then covered by the previous rule (already Dead, ignored). `OnDeath` fires exactly once regardless of how much overkill damage arrives in the same tick.
- **If `ApplyDamage` is called with a negative `amount`**: treated as invalid input and clamped to 0 damage (a no-op), never converted into healing. Damage and healing are separate methods by design (Core Rules 2–3) specifically to prevent this class of bug.
- **If `ApplyHeal` is called with a negative `amount`**: same treatment — clamped to 0 healing, never converted into damage.
- **If `ApplyDamage`/`ApplyHeal` targets a `NetworkObjectReference` that no longer resolves** (entity already despawned): the call is a no-op and logs a warning server-side — this should not throw, since network latency can produce a damage call that arrives just after a target's despawn.
- **Disconnection is not death**: if a player disconnects mid-session, this system does **not** fire `OnDeath` or set `IsDead = true` — a live-but-disconnected player is Networking Foundation's concern (connection state), not a Health & Damage state. `IsDead` means "reached 0 HP," not "left the session."
- **Full-team simultaneous wipe**: Health & Damage System fires `OnDeath` independently and correctly for each player — it does not attempt to detect or special-case a "whole team is dead" condition. Recognizing a wipe and triggering the checkpoint-return flow is explicitly Checkpoint System's responsibility, not this system's.

## Dependencies

**Depends On** (hard):
- **Networking Foundation** (not yet designed, but ADR-0001 already
  establishes the pattern) — `currentHP`/`IsDead` must be `NetworkVariable`s
  written server-side only.

**Depended On By**:
- **Elemental Status System** (not yet designed) — **hard**. Calls
  `ApplyDamage` for damage-over-time effects (e.g. Burning ticks). Each DoT
  tick is an independent call through `synergy_damage_multiplier` →
  `hp_clamp` — this system does not own tick cadence, duration, or per-tick
  scaling; that state machine belongs entirely to Elemental Status System.
  `ApplyDamage` must be safe to call at high frequency (multiple ticks/second
  across 4-5 players) with no additional logic, which the current
  stateless-per-call design already satisfies.
- **Spell Casting System** (not yet designed) — **hard**. Calls
  `ApplyDamage`/`ApplyHeal` after server-validating a hit.
- **Elemental Synergy System** (not yet designed) — **hard**. Supplies
  `synergy_multiplier` and `isSynergyTriggered` on `DamageInfo`.
- **Enemy AI System** (not yet designed) — **hard**. Subscribes to
  `OnDeath`; may read `currentHP`/`maxHP` ratio for behavior decisions.
- **Spectator/Death System** (not yet designed) — **hard**. Subscribes to
  the player's `OnDeath` to trigger the Active→Spectator transition already
  defined in `camera-system.md`.
- **Combat HUD** (not yet designed) — **soft**. Reads `currentHP`/`maxHP`
  for display; degrades gracefully without this system, though obviously
  non-functional as a combat HUD.
- **Checkpoint System** (not yet designed) — **hard**. Calls
  `Revive()`/`ResetHP()` to reverse death after a team-wipe checkpoint
  return.
- **Character Progression/Level System** (not yet designed) — **soft**. May
  eventually supply a `maxHP` bonus as a passive level-up benefit (per
  game-concept.md: "player level gives passive bonuses only") — this system
  requires no change to support that, since `maxHP` is already external
  data.

## Tuning Knobs

**This system defines no tuning knobs of its own.** By design, every numeric
value that affects difficulty or balance is intentionally owned elsewhere:

| Value | Owned By | Why not here |
|---|---|---|
| `maxHP` per player/enemy type | Enemy AI System / Player Controller / Character Progression | Core Rule 4 — keeps difficulty tuning in "enemy combination," not raw HP inflation |
| `synergy_multiplier` values (e.g. Chain Shock's 3x) | Elemental Synergy System | This system only applies whatever multiplier it's given; it doesn't decide combo values |
| Future `mitigation_multiplier` | Character Progression / Loadout (whenever defense stats are designed) | Deferred extension point, per Formulas section |
| DoT tick rate/duration | Elemental Status System | This system only receives individual `ApplyDamage` calls, it doesn't schedule them |

Health & Damage System is a pure **mechanism** layer — clamp math and state
transitions — with no balance levers of its own. This is a deliberate
outcome, not a gap: it keeps every actual game-feel/difficulty decision in
the system that has the context to make it correctly.

## Visual/Audio Requirements

This system owns the **damage-number / hit-reaction / death** feedback layer
only — not persistent elemental state visuals (Elemental Status System, not
yet designed) and not the death→spectator camera transition itself (already
locked in `camera-system.md`'s Core Rule 7, instant re-parent, no blend).
What follows is the feedback contract this system is responsible for firing
at the moment `ApplyDamage`/`ApplyHeal`/`OnDeath` resolve.

### Hit Feedback: Normal vs. Synergy

Per the game concept's explicit requirement ("урон от комбо — с особым
эффектом и звуком"), a synergy-triggered hit (`isSynergyTriggered = true`)
must be **categorically distinguishable at a glance**, not just "the same
effect, bigger."

- **Normal hit** (`isSynergyTriggered = false`): a brief hit-flash on the
  target (2–4 discrete frame-steps, matching the pixel-art render's stepped
  motion rather than a smooth engine-frame fade) + a small, contained
  particle burst tinted with `sourceSchool`'s established color. This is a
  momentary spark tied to the hit event — not the persistent elemental-state
  VFX, which is a different system's job even though it happens to reuse the
  same color language.
- **Synergy hit** (`isSynergyTriggered = true`): the normal-hit flash/burst
  plays, **plus** an additional distinct layer: a larger radial burst, a
  secondary white/gold "synergy accent" flash layered on top of the school
  color (so it reads as "amplified," not just "different color"), and a
  stronger camera impulse via `camera-system.md`'s existing
  `GenerateImpulse()` hook. **Recommended, provisional**: a brief hitstop
  (1–2 frozen frames) on synergy hits only — a feel/timing call for
  `technical-artist`/gameplay-programmer to scope, not an art-asset
  requirement.

### Floating Damage Numbers: Secondary, Not Primary

**Recommendation: damage numbers exist, but as a secondary/optional layer,
not the primary "it landed" signal.** Reasoning: the game concept's own
feedback-clarity language is explicit about *color and particles*, not text;
numbers require foveal attention while color/particle-bursts read
peripherally, which matters in a 4-5 player FPS co-op fight with multiple
simultaneous casts on screen. Concretely: damage numbers render in
world-space above the hit point, small and low-contrast for normal hits; for
synergy hits they render larger and in the gold/white synergy accent color —
reinforcing, not carrying, the normal-vs-synergy distinction.
**Provisional**: default-on vs. togglable accessibility option is an open UI
question, not decided here.

### Death Moment: Enemy vs. Player

This system fires `OnDeath` exactly once (Core Rule 5) but does **not** own
despawn timing (Enemy AI System) or the camera transition (Camera System) —
its contribution is the instantaneous visual cue coincident with the death
frame itself.

- **Enemy death**: the lethal hit's flash/burst (normal or synergy) plays as
  normal, plus one additional "kill-confirm" beat this system owns — a
  brief, more intense flash than a normal hit-flash, so a killing blow reads
  as *final* rather than "just another hit that happened to be last."
  Everything after (death animation, ragdoll, loot drop, despawn timing)
  belongs to Enemy AI System.
- **Player death**: because `camera-system.md` already locks the
  Active→Spectator transition as an **instant cut with no fade/blend**, this
  system deliberately does **not** add its own fade-to-black, vignette, or
  slow-motion beat on top of that cut — doing so would contradict the
  already-approved instant-cut design. This system's only obligation is that
  the *lethal hit's* flash/burst has already been visible in the frame(s)
  immediately preceding the cut, so the cut reads as "I saw what killed me,
  then it was spectator," never a mystery blackout. Any "You Died" card or
  spectator-mode framing UI is Spectator/Death System's or Session UI's
  responsibility.

### Audio Direction (categories, not assets — no sound-designer content exists yet)

1. **Impact — normal**: a short, percussive hit sound. Exact timbre
   (flesh/armored/elemental material variation) is deferred to Enemy AI /
   Audio System.
2. **Impact — synergy**: the normal impact sound **plus** a short layered
   "resonance" stinger on top (not a full sound replacement) — mirrors the
   visual layering approach so normal and synergy hits share a recognizable
   identity while synergy reads as amplified.
3. **Heal**: a soft, rising, non-alarming tone — deliberately in a different
   register from damage SFX so players don't have to look at the HUD to know
   whether an event was good or bad news.
4. **Death confirm**: a short, distinct "kill confirm" sting for enemy
   deaths (separate from Enemy AI's own death-animation audio). For player
   death, the lethal hit's impact SFX must be allowed to complete (or be
   cleanly cut, not truncated mid-transient) at the instant the camera snap
   fires, so audio doesn't glitch-cut against the visual finality. Exact
   implementation (ducking, snapshot mixing) is an Audio System coordination
   point, not specified numerically here.

**Provisional / open**: a low-HP audio cue (e.g. muffled heartbeat under some
threshold) is a plausible future addition but is **not locked here** — it
would require a threshold value this system has no tuning knob for (per
Tuning Knobs, HP-derived behavior belongs elsewhere) and overlaps with Combat
HUD's low-HP visual state.

### Provisional vs. Locked

**Safe to lock now**:
- Every damage event produces a dual-layer response: a hit-flash/particle-
  burst *and* an audio impact — never audio-only or visual-only.
- Synergy hits always add a distinct *additional* layer (visual accent +
  audio stinger) on top of the normal-hit response, never a simple palette
  swap or volume bump.
- Damage numbers, if used, are secondary reinforcement, not the primary
  signal — color/particle/audio carry that job.
- This system's death-moment contribution is a single coincident VFX/SFX
  beat, not an owned transition effect — the instant-cut behavior already
  locked in `camera-system.md` is respected, not layered over.
- The four SFX categories (normal impact, synergy impact, heal, death
  confirm) exist as separate triggerable events, regardless of final sound.

**Provisional, revisit once `art-bible.md` exists**:
- Exact hit-flash color values and how strongly they reuse vs. deliberately
  differ from Elemental Status System's persistent-state colors.
- Particle rendering technique and budget (ties into the game concept's own
  flagged VFX-performance risk).
- Damage-number typography, anchor point, and default-on/off.
- Hitstop duration/frame-count for synergy hits — best resolved against an
  actual combat prototype.
- Actual SFX asset design/timbre — categories are locked, content is not.

📌 **Asset Spec** — Visual/Audio requirements are defined at the category
level. After `design/gdd/art-bible.md` is approved, run
`/asset-spec system:health-damage-system` to produce per-asset visual/VFX
descriptions from this section.

*Proposed by `art-director` (2026-08-26).*

## UI Requirements

- `currentHP`/`maxHP` must be exposed read-only for **Combat HUD** (not yet
  designed) to display — this system provides the data, HUD owns the visual
  bar/number presentation.
- Damage numbers (per Visual/Audio Requirements) are a HUD/world-space UI
  element whose default-on/toggle behavior is an open UI question, not
  decided in this GDD.

> **📌 UX Flag — Health & Damage System**: This system feeds Combat HUD (HP
> display, damage numbers) and Enemy AI's low-HP behavior indicators. In
> Phase 4 (Pre-Production), run `/ux-design` for the Combat HUD screen before
> writing epics — this GDD only guarantees the data exists, not its layout.

## Acceptance Criteria

*Validated by `qa-lead` (2026-08-26). All criteria independently verifiable
without reading the rest of the GDD.*

**HP State & Clamping**

1. GIVEN a Health component's `currentHP` is a `NetworkVariable<float>` with `NetworkVariableWritePermission.Server`, WHEN any non-server peer attempts to write to `currentHP` directly (not via `ApplyDamage`/`ApplyHeal`), THEN the write is rejected and `currentHP` is unchanged as observed on the server and all clients.
2. GIVEN a Health component instantiated with `maxHP = 100` by an external caller, WHEN no call is made to any Health & Damage System method, THEN `maxHP` remains exactly 100, and this system's public API exposes no method that sets `maxHP`.
3. GIVEN an entity with `currentHP = 100`, `maxHP = 100`, WHEN `ApplyDamage` is called with `amount = 150` (`synergy_multiplier = 1.0`), THEN `currentHP` equals exactly 0 (never negative) and `IsDead` becomes `true`.
4. GIVEN an entity with `currentHP = 100`, `maxHP = 100`, WHEN `ApplyHeal` is called with `amount = 50`, THEN `currentHP` equals exactly 100 (never exceeding `maxHP`).
5. GIVEN an entity with `currentHP = 50`, `maxHP = 100`, WHEN 60 real-time seconds elapse with zero calls to `ApplyDamage`/`ApplyHeal`, THEN `currentHP` is still exactly 50 (no passive regen).

**Damage/Heal API**

6. GIVEN the server calls `ApplyDamage(info)` with a fully populated `DamageInfo`, WHEN the target is Alive, THEN `currentHP` decreases by the formula-computed amount and no exception is thrown regardless of which `sourceSchool` value is passed.
7. GIVEN the server calls `ApplyHeal(amount, source)` with `amount = 30` on a target with no healer school implemented at MVP, WHEN the call executes, THEN `currentHP` increases by 30 (clamped to `maxHP`) — proving the API needs no change when a healer school is added later.

**Formulas**

8. GIVEN `base_damage = 20`, `isSynergyTriggered = false` (`synergy_multiplier = 1.0`), WHEN `ApplyDamage` is called, THEN `final_damage_amount = 20.0` exactly, and `currentHP` decreases by 20.0.
9. GIVEN `base_damage = 20`, `isSynergyTriggered = true`, `synergy_multiplier = 3.0`, WHEN `ApplyDamage` is called, THEN `final_damage_amount = 60.0` exactly, and `currentHP` decreases by 60.0.
10. GIVEN `currentHP_prev = 40`, `maxHP = 100`, `final_damage_amount = 65`, WHEN `ApplyDamage` resolves, THEN `currentHP_new = 0` exactly (not -25), and `OnDeath` fires exactly once.
11. GIVEN `currentHP_prev = 90`, `maxHP = 100`, `heal_amount = 25`, WHEN `ApplyHeal` resolves, THEN `currentHP_new = 100` exactly (not 115) — 15 HP of overheal discarded, not banked.

**Death Transition**

12. GIVEN a player entity with `currentHP = 10`, WHEN `ApplyDamage` brings `currentHP` to 0, THEN `NetworkVariable<bool> _isDead` (ADR-0001's field) transitions to `true` server-side, and `OnDeath` fires exactly once.
13. GIVEN an enemy entity with `currentHP = 10`, WHEN `ApplyDamage` brings `currentHP` to 0, THEN `IsDead` becomes `true`, `OnDeath` fires exactly once, and `NetworkObject.Despawn()` (never `Destroy()`) is the only mechanism removing the entity from the session.

**Edge Case Robustness**

14. GIVEN an entity with `IsDead = true`, `currentHP = 0`, WHEN `ApplyDamage(50)` or `ApplyHeal(50)` is called, THEN `currentHP` remains exactly 0, no exception is thrown, and `OnDeath` does not fire again.
15. GIVEN an entity with `currentHP = 10`, WHEN two `ApplyDamage(50)` calls are both processed within the same server tick, THEN `OnDeath` fires exactly once, and final `currentHP = 0`.
16. GIVEN an entity with `currentHP = 50`, `maxHP = 100`, WHEN `ApplyDamage(amount = -20)` is called, THEN `currentHP` remains exactly 50 (must NOT become 70 — never converts to healing).
17. GIVEN an entity with `currentHP = 50`, `maxHP = 100`, WHEN `ApplyHeal(amount = -20)` is called, THEN `currentHP` remains exactly 50 (must NOT become 30 — never converts to damage).
18. GIVEN a `NetworkObjectReference` pointing to an already-despawned target, WHEN `ApplyDamage`/`ApplyHeal` is called with that reference, THEN no exception is thrown, the call is a no-op, and a warning is logged server-side.
19. GIVEN a connected player with `currentHP = 80`, `IsDead = false`, WHEN that player's client disconnects, THEN `IsDead` remains `false` and `currentHP` remains 80 — `OnDeath` does NOT fire from disconnection alone.
20. GIVEN 4 players simultaneously reach `currentHP = 0` via independent `ApplyDamage` calls in the same/adjacent ticks, WHEN each death resolves, THEN `OnDeath` fires exactly once per player (4 total), and this system makes no call/event indicating "team wipe" — verified absent, since that detection is explicitly out of scope (Checkpoint System's job).

**Performance**

21. GIVEN `ApplyDamage` is called at a sustained rate of 10 calls/second on a single entity (representative DoT cadence), WHEN this runs continuously for 30 seconds, THEN no exception, no memory growth from per-call allocation, and no measurable server frame-time spike beyond the clamp math itself (O(1) per call, no per-call heap allocation expected).

*Two testability gaps found and resolved before finalizing: the deferred
mitigation formula has no criterion (nothing to test — it's explicitly
unimplemented, see Formulas); `NetworkVariable<float> currentHP`'s
replication bandwidth under frequent DoT ticks has no defined budget/batching
rule anywhere in the project — accepted as a documented risk (see Open
Questions) rather than inventing an untested number.*

## Open Questions

- **NetworkVariable<float> currentHP replication bandwidth** under frequent
  DoT ticks has no defined send-rate/batching budget anywhere in the project
  yet. Accepted as a documented risk for now (default NGO replication, no
  custom batching) — revisit when Networking Foundation GDD is authored
  (owner: network-programmer; target: Networking Foundation GDD, next in the
  bottleneck-first design order).
- **Defense/mitigation formula** is explicitly deferred (see Formulas) — no
  acceptance criterion exists for it since nothing is implemented yet. Add a
  criterion retroactively once a system defining equipment/defense stats is
  designed (owner: game-designer; target: Character Progression / Loadout
  GDD, whichever ends up owning equipment passive stats).
- **Hitstop duration/frame-count for synergy hits** (art-director's
  recommendation) is a feel-tuning value best resolved against an actual
  combat build rather than specified numerically now (owner: technical-artist;
  target: Vertical Slice).
- **Damage number default-on vs. accessibility toggle** is an open UI
  question (owner: ux-designer; target: Combat HUD UX spec).
- **Low-HP audio/visual cue threshold** (e.g. muffled heartbeat) is a
  plausible future addition with no owner yet — flagged but not committed
  (owner: TBD; target: post-MVP polish pass).
