# Concept Prototype Report: Co-op Spellcasting

> **Date**: 2026-08-26
> **Prototype Path**: Engine (Unity 6.3 LTS)
> **Concept File**: design/gdd/game-concept.md

---

## Hypothesis

If two players independently cast elemental spells (Fire/Water/Lightning) on the same
enemies in real time, cross-player synergy (Water soaks a target, then Lightning
triggers a 3x-damage Chain Shock that jumps to nearby enemies) will feel spontaneous
and fun to discover — evidenced by players triggering Chain Shock without being told
the combo exists, and describing the moment as satisfying/exciting in debrief.

---

## Riskiest Assumption Tested

That the *emergent discovery* of the Water+Lightning combo (not just the mechanic's
existence) is what creates the fun. This is why the prototype used two independently
controlled local characters rather than one player controlling both — but see
"Limitation" below: this assumption was ultimately **not tested**, because a real
second player was not available during this session.

---

## Approach

Built a local two-player split-screen Unity scene: WASD/Arrow-key movement,
auto-target-nearest-enemy casting (no FPS mouse-aim), 4 respawning enemy dummies,
one synergy pair (Water+Lightning = Chain Shock, x3 damage + chains to up to 2
nearby dummies within 4m). No networking — deliberately isolates "is the mechanic
interesting" from "does it feel good under real network latency."

**Path chosen:** Engine (Unity)
**Reason for path:** Feel and real-time timing are part of the hypothesis; a browser
prototype would introduce latency that lies about the result.

**Shortcuts taken (intentional):**
- Hardcoded tuning values (damage, cooldowns, radii)
- Auto-targeting instead of FPS mouse-aim (aim precision explicitly out of scope)
- Legacy Input Manager (matches existing prototype convention; production will use
  the new Input System)
- OnGUI debug HUD instead of real UI
- No networking — single machine, local split-screen only
- Debug shapes (capsules, spheres) instead of art

---

## Result

**Bugs found and fixed during this session (both now resolved):**

1. **Respawn coroutine bug**: `EnemyDummy.Die()` originally called
   `gameObject.SetActive(false)` before `StartCoroutine(RespawnRoutine())`. Unity
   cannot run (or resume) a coroutine on an inactive GameObject, so dead dummies
   never respawned and threw `Coroutine couldn't be started because the game object
   is inactive!` errors. Fixed by keeping the GameObject active and instead disabling
   only the renderer/collider/HP-label to fake "death" — the coroutine now runs
   correctly and dummies respawn after the configured delay.

2. **Auto-target drift (not a bug, a design consequence)**: Because both Water and
   Lightning casts independently call `FindNearestEnemy()` at cast time, a player
   who moves between casts (or stands near two closely-spaced dummies) can have each
   spell land on a *different* dummy. This silently prevents the Wet+Lightning combo
   from triggering, with no error and no feedback — it just looks like "lightning
   stopped working." Added a `[TARGET]` debug log (target name + Wet state per cast)
   to confirm this diagnosis. Once the tester deliberately stood still and aimed both
   casts at the same dummy, Chain Shock triggered reliably and repeatedly.

**Confirmed working after fixes:**
- Water correctly applies the Wet status and its visual color change
- Lightning on a Wet target correctly applies 3x damage and triggers Chain Shock
- Chain Shock correctly deals damage to up to 2 nearby dummies within the 4m radius
- Dummies respawn after death and can be re-targeted
- The `[SYNERGY]` console log fires exactly when expected

**Not tested — key limitation:** All testing in this session was solo (one person
verifying the mechanic works correctly, not two independent players discovering it
together). The core hypothesis is specifically about *emergent, spontaneous*
discovery between two people who are not coordinating on purpose. The tester's own
qualitative read, testing solo: the mechanic is "interesting" conceptually, but this
is not equivalent to observing two independent players stumble into it.

---

## Metrics

| Metric | Value |
|--------|-------|
| Path used | Engine (Unity 6.3 LTS) |
| Iterations to playable | 3 (initial build, respawn-coroutine fix, target-drift diagnostic) |
| Prototype duration | ~1 session |
| Playtesters | 1 internal (solo) / 0 external |
| Feel assessment | Auto-target selection is invisible to the player — no on-screen indicator of which enemy will be hit, which caused real confusion during solo testing ("lightning stopped dealing damage") until a debug log exposed the cause |
| Hypothesis verdict | INCONCLUSIVE — mechanic confirmed technically functional and correctly tuned; the core "feels spontaneous and fun between two independent players" claim remains untested |

---

## Recommendation: PROCEED (with a follow-up test required)

The underlying mechanic is implemented correctly and is conceptually sound — the
tester found the Wet+Lightning interaction interesting even without a second player.
The bugs found were fixable and revealed a real design risk (invisible auto-targeting)
rather than a fundamental flaw in the synergy concept. There is no signal here that
would justify PIVOT or KILL. However, the prototype's central hypothesis — that
*discovery between two independent players* feels spontaneous and fun — was not
actually exercised, so this verdict should not be treated as full validation.

**Before committing further design work to this synergy model**, run a short
follow-up session with a real second player (or async: two people playing the same
build at different times without coordinating) to observe genuine discovery behavior.

---

## If Proceeding

- **Core tuning values discovered:** Wet duration of 4s, chain radius of 4m, and
  chain damage of 15 flat all felt reasonable in solo testing — no immediate red
  flags, but not stress-tested with two simultaneous casters.
- **Assumptions confirmed:** The elemental status-flag model (Wet as a timed boolean
  flag checked by other spells) is simple to implement and easy to reason about —
  supports the ADR-0001 `NetworkVariable<ElementalStatusFlags>` design.
- **Assumptions disproved:** None outright, but the auto-targeting simplification
  used to avoid the "two mice on one PC" testing problem introduced an unexpected
  UX risk: if the production game's targeting/aiming system is ever ambiguous about
  which enemy a spell will hit, synergy setups will silently fail with no feedback.
  **This is a real production risk, not just a prototype artifact** — the input-system
  GDD and combat GDD should explicitly address target feedback (e.g., a reticle or
  highlight showing the currently-targeted enemy) so players can reliably set up
  combos on purpose.
- **Emergent mechanics:** None beyond the one synergy tested.

**Next steps:**
1. Schedule a short real 2-player (or async 2-session) test of this same build to
   validate the core discovery hypothesis before it's treated as proven.
2. `/map-systems` — re-map systems for the co-op concept (this was already queued).
3. Resolve `design/gdd/input-system.md` — explicitly capture the "visible current
   target" requirement surfaced by this prototype.
4. `/design-system [combat or targeting system]` — carry forward the tuning values
   and the target-feedback requirement above.

---

## Lessons Learned

- **What assumptions were broken by actually building this?** The assumption that
  "auto-targeting nearest enemy" is a harmless simplification for solo/local testing
  turned out to hide a real interaction-design question (target feedback) that
  applies to the production game, not just the prototype.
- **What surprised us that didn't show up in the brainstorm?** How silent the
  targeting-mismatch failure mode was — no error, no log, the spell just "did less
  damage than expected." This kind of invisible failure is worth explicitly testing
  for in future prototypes of any auto-assisted mechanic.
- **What would we test differently next time?** Get a second real tester (or at
  least two separate solo sessions with fresh eyes) lined up *before* starting the
  build, so the core hypothesis can actually be exercised in the same session instead
  of being deferred.

---

> *Prototype code location: `prototypes/co-op-spellcasting-concept/`*
> *This code is throwaway. Never refactor into production.*
