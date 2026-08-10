# Pivot Note: Core Combat

> **Date**: 2026-08-10
> **From verdict**: PARTIALLY CONFIRMED → PIVOT

## Original Hypothesis

If the player fights using a fast/responsive 3-hit combo chain with input
buffering, combat will still feel weighty — evidenced by clear hit feedback
(knockback, flash, hit-stop, camera shake) with no perceived input lag, and a
combo chain that reads as connected rather than disjointed.

## What to Keep

All current mechanics worked and should be preserved as-is:
- WASD movement + third-person orbit camera
- 3-hit combo chain on left-click with input buffering
- Dodge roll with i-frames
- Hit-feedback stack: knockback, color flash, hit-stop (`Time.timeScale` blip),
  camera shake — this landed better than expected and is a strong positive
  signal on the project's core "weighty" risk.

## What to Change

Add swing/windup and hit animations to the combo chain. The current prototype
has zero visual read on attack timing (no windup, no swing motion) — this
made the combo "completely unclear" to the tester even though the underlying
timing code is functioning. Even a crude procedural tween (arc motion, or
squash/stretch on the weapon/attack point) is likely enough to make the combo
legible. Also fix the intermittent camera orbit stutter noted during
playtesting — it competed for attention with judging the core combat feel.

## Revised Hypothesis for Next Prototype

If the player fights using a fast/responsive 3-hit combo chain **with a
visible swing/windup cue per hit**, combat will feel both responsive and
weighty — evidenced by the tester being able to identify when each hit lands
and when the next combo input window is open, in addition to the existing
hit-feedback signals (knockback, flash, hit-stop, camera shake) continuing to
read as satisfying.

**Next step:** `/prototype core-combat` (revised)
