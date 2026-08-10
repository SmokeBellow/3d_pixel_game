# Concept Prototype Report: Core Combat

> **Date**: 2026-08-10
> **Prototype Path**: Engine (Unity 6.3 LTS, C#)
> **Concept File**: design/gdd/game-concept.md

---

## Hypothesis

If the player fights using a fast/responsive 3-hit combo chain with input
buffering, combat will still feel weighty — evidenced by clear hit feedback
(knockback, flash, hit-stop, camera shake) with no perceived input lag, and a
combo chain that reads as connected rather than disjointed.

---

## Riskiest Assumption Tested

Combining "fast/responsive" with "weighty" in the same combat feel — this is
the project's first completed 3D project, and the concern was that fast input
handling and weighty hit feedback would fight each other. The prototype tested
this directly by pairing snappy input buffering with strong hit-reaction
feedback (knockback, hit-stop, camera shake) on stationary target dummies.

---

## Approach

Built a minimal Unity scene: capsule player with WASD movement and a
third-person orbit camera, a 3-hit combo chain on left-click with input
buffering, a dodge roll with i-frames, and 2-3 stationary target dummy
capsules with knockback + color flash + hit-stop + camera shake on hit. A
floor and perimeter walls were added mid-session after the first playtest
attempt showed dummies falling through an empty scene.

**Path chosen:** Engine
**Reason for path:** Feel IS the hypothesis — browser latency would have
produced false signal on input responsiveness and hit weight.

**Shortcuts taken (intentional):**
- Legacy `UnityEngine.Input` (Input Manager) instead of the new Input System —
  required switching Active Input Handling to "Both" in Player Settings
- Placeholder capsule geometry for player and dummies, no character models
- No swing/attack animations — combo timing driven entirely by code timers,
  no visual windup or swing motion
- Generated placeholder hit tone via AudioSource, no real SFX
- No enemy AI — dummies are stationary hit-feedback targets only

---

## Result

Hit feedback (knockback, color flash, hit-stop, camera shake) landed well —
the tester specifically called out the knockback/push-back animation as a
pleasant surprise, better than expected. Dodge roll and camera orbit both
functioned. However, the 3-hit combo chain was "completely unclear" to the
tester specifically because there is no swing/windup animation — with no
visual cue for when a hit is landing or when the next input window opens, the
combo reads as disconnected hits rather than a chain. The camera orbit also
had an intermittent stutter/snap ("резкий поворот") during rotation. Both the
missing swing animation and the camera stutter were named as equally
disruptive to the experience.

---

## Metrics

| Metric | Value |
|--------|-------|
| Path used | Engine (Unity 6.3 LTS) |
| Iterations to playable | 3 (Input System conflict fix, missing floor/walls fix, Active Input Handling restart) |
| Prototype duration | ~1 session (same day) |
| Playtesters | 1 internal |
| Feel assessment | Hit feedback (knockback/flash/hit-stop/shake) read as weighty and satisfying; combo chaining unreadable without swing animation; camera orbit intermittently stutters |
| Hypothesis verdict | PARTIALLY CONFIRMED |

---

## Recommendation: PROCEED (revised after Iteration 2)

The core hit-feedback loop (knockback, flash, hit-stop, camera shake) proved
the "weighty" half of the hypothesis works even with fast, responsive input
handling — this is a real, positive signal on the project's biggest risk. The
initial pass could not yet prove the "combo chain feels connected" half of
the hypothesis, because there was no swing/attack animation to communicate
timing to the player. See **Iteration 2** below for the resolution.

---

## Iteration 2 (same session, post-PIVOT)

Per `PIVOT-NOTE.md`, two changes were made without touching any of the
mechanics that already worked:

- **`PlayerCombatController.cs`**: added a procedural (non-rigged) swing tween
  — windup, then a fast swing exactly synced to the hitbox's active window,
  then recovery — driven off the same `ComboStep` timing already used for
  hit detection. Wired to a placeholder weapon cube built by the scene setup
  script.
- **`OrbitCamera.cs`**: locked and hid the cursor while orbiting (RMB held).
  This was the suspected root cause of the reported camera "snap" — an
  unlocked cursor hitting the screen edge produces a large Mouse X delta
  spike on the next frame.

**Result:** the camera stutter fix landed — no further complaints about
camera snapping. The swing tween did **not** fully solve combo legibility —
tester feedback: "не хватает визуального отображения комбо" (still not
enough visual read on the combo). However, the tester assessed the prototype
as acceptable overall and gave a PROCEED verdict, treating combo legibility
as a real-animation problem to solve during production rather than a blocker
for this concept prototype.

**Revised hypothesis verdict: PARTIALLY CONFIRMED, PROCEED anyway** — the
"weighty hit feedback + responsive input" core of the hypothesis is solid.
Combo readability remains an open production-quality-animation problem, not
a fundamental feel problem with the fast/responsive approach.

---

## If Proceeding

- **Core tuning values discovered:** combo step timing (`activeStart`/`activeEnd`/`totalDuration`
  per `ComboStep`) is usable as a first pass for production tuning; hit-stop
  and knockback values felt satisfying at prototype defaults.
- **Assumptions confirmed:** fast/responsive input handling and weighty hit
  feedback do NOT fight each other — this was the project's biggest risk
  going in, and it's resolved.
- **Assumptions disproved:** a purely procedural (non-rigged) swing tween is
  not sufficient on its own to make a combo chain fully legible — real
  windup/attack animation (or at least motion-designed VFX/trail) will be
  needed in production, not just any visual motion.
- **Emergent mechanics:** none beyond the original scope.

**Next steps:**
1. `/design-review design/gdd/game-concept.md`
2. `/gate-check`
3. `/map-systems`
4. `/design-system [mechanic]` — combat system GDD should explicitly call out
   combo legibility as an animation/VFX requirement, not just a timing
   requirement.

---

## Lessons Learned

- **What assumptions were broken by actually building this?** The prototype
  scope description didn't flag that "no real animations, squash/stretch
  placeholder only" would leave the combo *illegible* rather than just
  visually rough — a functional-but-invisible combo timer is not the same as
  a testable one.

- **What surprised us that didn't show up in the brainstorm?** The
  hit-feedback stack (knockback push especially) landed better than expected
  on the first try — no iteration needed there.

- **What would we test differently next time?** Add at minimum a crude
  swing/windup tween before the first playtest, rather than treating
  animation as fully separable from the combo-feel hypothesis — timing
  feedback requires *some* visual signal, even placeholder-level.

---

> *Prototype code location: `prototypes/core-combat-concept/`*
> *This code is throwaway. Never refactor into production.*
