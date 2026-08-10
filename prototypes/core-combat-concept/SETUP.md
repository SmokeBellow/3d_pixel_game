# core-combat-concept — Setup Instructions

**Question being tested**: Can fast/responsive melee combat with combo chains in Unity 6.3 LTS still feel weighty?

This prototype ships as plain C# scripts, not a full Unity project folder — Unity scene files
are binary/YAML-serialized by the Editor itself and unsafe to hand-author. Follow these steps
in your local Unity 6.3 LTS install (URP template) to wire it up. Should take ~10-15 minutes.

## 1. Create the project

1. Unity Hub → New Project → **3D (URP)** template → Unity 6.3 LTS.
2. Copy the 6 scripts from `Assets/Scripts/` in this folder into your project's `Assets/Scripts/`.

## 2. Build the scene hierarchy

Create these GameObjects in a new empty scene (delete the default `Main Light` is fine to keep):

### Player
1. Create a **Capsule** (GameObject → 3D Object → Capsule), rename to `Player`.
2. Remove its `CapsuleCollider` (the `CharacterController` replaces it) — actually, you can leave
   the CapsuleCollider, it won't conflict, but it's not needed for movement.
3. Add Component → `Character Controller` (leave defaults — Center Y ≈ 1, Height ≈ 2, Radius ≈ 0.5).
4. Add Component → `Player Combat Controller` (the script).
5. Create an empty child GameObject under `Player`, name it `AttackPoint`, position it at
   roughly `(0, 1, 1)` (in front of and at chest height on the player).
6. On `AttackPoint`, Add Component → `Box Collider` → check **Is Trigger** → set size to
   roughly `(1, 1, 1.2)` (a wide-ish swing arc).
7. On `AttackPoint`, Add Component → `Attack Hitbox` (the script). Drag its own `Box Collider`
   into the `Hitbox Collider` field.
8. Back on `Player`'s `Player Combat Controller` component: drag `AttackPoint` into the
   `Attack Hitbox` field.

### Target dummies (make 3)
1. Create a **Capsule**, rename to `Dummy_1`. Position it a few meters in front of the player,
   e.g. `(2, 1, 3)`.
2. Add Component → `Rigidbody` — **uncheck "Use Gravity" is fine to leave checked**, but set
   **Constraints → Freeze Rotation X/Y/Z** so hits don't send it tumbling (knockback should
   read as a clean shove, not a physics ragdoll flail — that would confound the "weighty hit"
   signal with "funny physics glitch").
3. Add Component → `Target Dummy` (the script).
4. Duplicate twice more (`Dummy_2`, `Dummy_3`), spread them out a couple meters apart so combo
   step 3 (the finisher, which has the widest/longest active window) can plausibly clip more
   than one if you swing while moving between them.

### Camera rig
1. Create an empty GameObject, name it `CameraRig`, position at `(0, 0, 0)`.
2. Add Component → `Orbit Camera` and `Simple Camera Shake` (both scripts) to `CameraRig`.
3. Drag `Player` into `Orbit Camera`'s `Target` field.
4. Drag `CameraRig`'s own `Simple Camera Shake` component into `Orbit Camera`'s `Shake` field.
5. Make `Main Camera` a **child of `CameraRig`**, reset its local position to `(0,0,0)` and local
   rotation to identity — `OrbitCamera` moves the rig, the camera itself just sits at the rig's origin.
6. On `Player`'s `Player Combat Controller`, drag `Main Camera` into the `Camera Transform` field
   (used for camera-relative movement).

### Hit feedback
1. Create an empty GameObject, name it `HitFeedback`.
2. Add Component → `Audio Source` — uncheck **Play On Awake**.
3. Add Component → `Hit Feedback` (the script). Drag the `Audio Source` into its `Audio Source`
   field, and drag `CameraRig`'s `Simple Camera Shake` into its `Camera Shake` field.

## 3. Input check

This prototype deliberately uses the **legacy Input Manager** (`Input.GetAxis`, `Input.GetMouseButtonDown`)
for speed — Unity 6.3 projects created from the URP template usually have both the legacy manager
and the new Input System available side-by-side. If you get a compile error about `Input` being
ambiguous or unavailable, go to **Edit → Project Settings → Player → Active Input Handling** and
set it to **Both** (not "Input System Package (New)" only).

Controls:
- **WASD** — move (camera-relative)
- **Left mouse button** — attack (press again during the flash-window near the end of a swing to chain)
- **Right mouse button (hold) + mouse move** — orbit camera
- **Space** — dodge roll (has a brief i-frame window; cancels an in-progress attack)

## 4. Press Play

Walk up to a dummy, left-click to swing. Try to land 3 hits in a row (the full combo). Try dodging
right before a swing would land. Report back what it feels like — see the questions in the main
chat once you've played for a few minutes.
