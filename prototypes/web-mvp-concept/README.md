# Web MVP Prototype — Covenant of Mages

## Hypothesis

The co-op elemental-synergy MVP (Water → Lightning = Chain Shock) is playable
in a browser with real friends joining over the internet — no Unity build, no
server, no installation. This is the fastest path to the still-outstanding
**real 2-player synergy test** flagged by `co-op-spellcasting-concept/REPORT.md`.

## How to run

1. Open `prototype.html` in any desktop browser (double-click). Internet
   required (Three.js/PeerJS CDN + PeerJS signaling).
2. Host: **Создать игру** → tell friends the 4-letter code.
3. Friends: open the same file (send it via any messenger) → **Присоединиться
   по коду** → enter code.
4. Pick **one element** (🔥/💧/⚡) right after connecting — this is fixed for
   the whole run and colors your character (each school has its own color).
   You only see your own school's spells; combos require teammates on other
   elements. **Joining friends wait in a lobby (with chat) until the host
   clicks Начать** — nobody enters the arena early.
5. **First-person view**, arms now using your own element's real model
   (third attempt at this — see Findings; falls back to placeholder
   box/sphere arms automatically if the real model isn't ready yet). A
   third-person-behind-the-head camera was tried in between and is
   preserved on the `third-person-camera-aoe` branch, but was reverted back
   to first-person per user request. Controls: WASD move, mouse look
   (click to lock; also works unlocked for touchpads, plus arrow keys), LMB
   or Space cast (aimed at the crosshair — see below), wheel/Q/1-2-3 switch
   spell slot, **E to interact with a shop stand**, **L toggles full
   brightness / no fog**, **K instantly kills every enemy on the map**,
   **V toggles a debug third-person view of your own character** (see
   Findings — lets you eyeball your own idle/walk/cast animations solo,
   without needing a second browser connected as a real remote player; also
   marks a small cyan dot at exactly where the FP camera sits, to help
   judge/tune the FP viewmodel offset from outside it) (debug cheats). You
   can't cast again (any spell slot), **or move**, until your cast
   animation finishes, even if the spell's own cooldown is shorter —
   casting plants your feet.
6. The combo: a Water player soaks an enemy, a Lightning player hits it —
   3x damage + zigzag chain to nearby enemies (Lightning renders as a jagged
   bolt, not a straight line, but still lands exactly on the cursor).
7. Clear all enemies in a level → the whole party is teleported into a
   dedicated **hub/progression room** (its own space, not a UI overlay over
   the dungeon). The shop is **physical, not a modal**: walk up to a glowing
   stand, its price tag floats over it, press **E** to interact — a stand for
   buying your school's next spell, one for leveling up whichever spell is in
   your active hotbar slot, two for a passive choice (only appear on levels
   where one is pending), and a glowing portal to continue to the next level.
8. Everyone spawns/respawns at their current zone's pentagram — enemies
   physically cannot enter its ward radius.
9. **Three dungeon levels, then the run loops**: level 1 is the original
   arena; level 2 is a real corridor maze (9×9 grid, recursive-backtracker
   generated — corridors, forks, dead ends, walls you actually collide
   with) with more/tankier enemies that path through the corridors via BFS
   to chase you instead of walking into a wall; level 3 is a single boss
   with a large HP pool that alternates a point-blank slam AoE and a
   telegraphed charge dash. Clearing level 3 loops back to level 1.
10. Each school's tier-2 ("advanced") spell has a real mechanical hook, not
    just bigger numbers: Fire's Огненный шар explodes on impact (visible
    fireball + light flash) and damages everything near the blast, Water's
    Волна is an instant nova centered on the caster — no aiming, it damages
    and soaks every enemy around you in every direction — and Lightning's
    Цепная молния auto-chains at reduced damage even without a Water setup.
    Tier-2 spells also render visibly bigger (thicker bolt / larger
    projectile).

Networking: PeerJS (WebRTC data channels), P2P, host-authoritative — mirrors
ADR-0001's listen-server topology in spirit. Host disconnect ends the session
(same policy as ADR-0001). Uses explicit STUN + TURN servers (see Findings)
so real players on different networks can actually connect, not just two
browsers on the same machine.

## Status

In progress (2026-09-03). Solo smoke-tested end to end via scripted console
runs: element pick, combat, gold/XP gain, guaranteed level-2-up after
clearing level 1, shop purchases (buy spell + mentor level-up), the
level-2 transition, and hub-room teleport, the level 2 (bigger
arena/tougher enemies) and level 3 (boss, 900 HP, slam + charge attacks
both firing correctly over 10 simulated seconds) zones. All confirmed
working with zero console errors. Real multi-client test pending.
**Update**: the dungeon no longer loops from level 3 back to 1 — per user
request the run is now finite (3 levels, ends after the boss); see the
mirror-portal Findings entry.

### Known issues / tech debt

- **Hub room corners may still show a faint seam.** Attempted a fix
  (`buildHubRoom`'s `EXT` wall-extension — see its own Findings entry) and
  verified it mathematically (adjacent wall AABBs now genuinely overlap at
  every corner), but couldn't get a clean up-close visual confirmation in
  this sandboxed browser (camera yaw isn't controllable here). Per user
  instruction, parking this as tech debt rather than continuing to chase
  it blind — revisit with a real playtest.

## Features

- First-person view with placeholder box/sphere arms (camera-system.md
  formulas), pointer-lock + touchpad/arrow-key look fallback, shared
  `sensitivity` value for mouse + keyboard look
- **Real particle effects on every spell hit** — Kenney's CC0 Particle Pack
  (`textures/particles/`, see its `LICENSE.txt`) drives a small CPU-updated
  `THREE.Points` burst system (`spawnParticleBurst`/`updateParticleBursts`):
  flame + ember burst on Огненный шар's explosion, droplets flying outward
  on Волна's nova, and a small spark/glow burst on every other spell's hit
  (Искра/Плеск/Разряд/Chain Shock) that previously had zero impact feedback
- **One element per player, chosen at lobby via a full-screen class-selection
  screen**: three cards side by side, each with its own live rotating 3D
  preview (the shared mage model tinted to that element's color — swap for
  distinct per-school models later), a short lore blurb for that element's
  order, and its spell list with one-line mechanical hints. Hovering a card
  highlights it (glowing border in the element's color); clicking commits
  the choice. Each school has a 2-spell unlock ladder (basic → advanced),
  each with its own icon (✨💥 Fire,
  💧🌊 Water, ⚡🌩️ Lightning). Fire/Water = flying projectiles, Lightning =
  hitscan, both aimed at the crosshair (see Findings — a self-centered
  no-aim AoE version was tried in between and reverted); Волна (Water
  tier 2) is the one exception, a self-centered nova around the caster with
  no aiming. Water→Lightning = Chain Shock synergy (x3 dmg + chain)
- **Spell leveling by use**: only landed hits count; damage scales +12%/level
- **Gold** (random 5-15 per kill) and **shared party XP/level** — clearing
  a level guarantees enough XP for the next party level; leveling raises
  everyone's max HP; every other level (3, 5, 7…) offers a passive-skill
  choice (mentor). The tier-2 spell is **not** auto-granted — it must be
  bought at the shop stand for a flat **20💰**, kept comfortably below the
  worst-case gold a solo player can earn clearing level 1 (5 enemies × the
  5g minimum roll = 25), so even a bad gold roll can afford it, same as
  leveling up an existing spell
- **Physical shop/mentor in the hub room**: no modal dialog — glowing stands
  with floating price tags you interact with via E (buy your next spell,
  level up your active spell, pick a passive when one's pending, or step to
  the portal to continue); continuing spawns the next level with more
  enemies (scaled HP) and respawns everyone at that level's spawn point.
  The bottom-screen interact prompt names the exact spell/action (not just
  "Buy"/"Level up") so the visually similar buy and mentor stands can't be
  mixed up. Trying to buy/level up without enough gold flashes the price
  tag and prompt red instead of silently failing
- **Sessions**: a level ends when all its enemies are dead; no more
  auto-respawning enemies mid-level
- Humanoid enemies with walk + attack-lunge animation; AI: patrol (wander
  near home) → aggro (chase, **stopping at a visible distance** rather than
  hugging the player) within AGGRO_RADIUS (11) → de-aggro beyond the larger
  PATROL_LEASH_RADIUS (20, returns home). **Getting hit always aggros the
  attacker**, even from outside AGGRO_RADIUS or mid-patrol
- Player HP, melee damage from enemies, downed state (movement/casting
  locked) → timed respawn
- Procedural stone-brick textures, flickering torches (brightened after
  playtest feedback), dim ambient + fog + player glow for a lit-circle
  vignette — **press L to disable for debugging**
- One fixed **spawn/respawn point per zone with a canvas-drawn pentagram**
  (glowing circle + five-point star + runes, pulsing light, 5×5 units) — no
  external image file. Enemies cannot enter its `SPAWN_SAFE_RADIUS` (6) ward
- **Player color = element color** — for the **name tag** only now (each
  school has a fixed color, not a round-robin per-connection color); the
  3D model itself keeps its own original, untinted look (see Findings —
  per-element recoloring was removed by request)
- **Lightning renders as a zigzag bolt** (hit detection unchanged — still a
  straight raycast from the camera; only the visual is jagged)
- **Lobby with chat**: joining players see a waiting room (with player list
  + chat, shared with the host's lobby chat) until the host starts; late
  joiners after the game has started also get chat/lobby state correctly
- **Player visuals**: **Fire and Water each have their own distinct Mixamo
  model now**; only Lightning still uses the shared "Brady" model — one
  school closer to every school eventually having its own look, per user
  request (see Findings). Models render in their **original, untinted
  colors** (per-element recoloring was removed by request — see
  Findings); only the name tag above a player's head still uses their
  element's color. All three rigs share the same standard Mixamo skeleton
  shape (Water's own rig happens to name every bone with a `mixamorig10`
  prefix instead of plain `mixamorig` — see Findings for the track-name
  remap that makes the shared clips work on it anyway), so the same
  walk/cast clips retarget onto any of them with no re-export needed.
  Rendered for every **remote** player as seen by their teammates, and
  (third attempt — see Findings) also as your own first-person arms,
  parented to the camera. **Tier-1 spells
  play `cast1.fbx` ("Standing 1H Magic Attack 01"), tier-2 spells play
  `cast2.fbx`** (the original "Magic Spell Casting") — each has its own
  timing calibration for when its swing actually releases, so the shot
  fires in sync with either clip, not just one. The cast gesture starts
  playing immediately on cast (a simple arm-recoil animation in first
  person; the full Mixamo cast clip for anyone watching you), but the
  actual shot (projectile/hitscan/nova) is deliberately delayed (~0.45s for
  tier-1, ~1.2s for tier-2) to fire exactly when that clip's arm reaches
  full extension for onlookers, not before. No spell (any slot) can be cast
  again until its own cast animation finishes, even if that spell's own
  cooldown is shorter.
  Remote players' movement animation is a Running clip, not Walking — a
  deliberate swap. The clip's own baked root motion is stripped on load
  (see Findings — `stripHorizontalRootMotion`), and its `MOVE_ANIM_TIMESCALE`
  is *derived* from the clip's natural pace vs. `MOVE_SPEED`, not
  guessed, so the leg-cycle matches actual ground speed instead of
  sliding; their body also faces whichever direction they're actually
  moving (forward/backward/strafing) rather than always the forward pose,
  since there's only one directional run clip
- **Multi-zone world**: level 1/2/3 and the hub room are separate, far-apart
  spaces built by one shared `buildArena()` helper and coexisting statically
  in the same Three.js scene; moving between them is a teleport of `playerPos`
  to the target zone's spawn point (see Findings for why zones, not a single
  arena, before diving into the code)

## Findings

- First darkness/torch pass was too dark and made an adjacent attacking
  enemy hard to see, which read as "taking damage from nothing" — fixed by
  brightening torches/player glow, loosening fog, and making enemies stop
  and visibly lunge at a distance instead of walking into the camera.
- Playtest (2026-08-30) surfaced 3 bugs, all fixed and re-verified via
  scripted console checks:
  1. Dead enemies stayed visible — `hostApplyDamage` refreshed the mesh
     *before* `hostOnEnemyKilled` flipped `alive`, so the death never
     actually hid the mesh. Fixed by reordering.
  2. The mentor "level up" button silently did nothing for real clicks —
     the shop panel rebuilt its entire DOM every animation frame (60/sec),
     so a real click (mousedown→mouseup takes longer than one frame) could
     land on a button already replaced by a fresh node. Fixed by only
     rebuilding the DOM when the shop's structure actually changes (spell
     count, pending passive) and updating existing nodes' text/disabled
     state in place otherwise.
  3. Enemies walked straight through walls and pillars — their chase/wander
     steering never had any collision awareness. Added `clampEnemyToArena`:
     clamps to the arena bounds and pushes enemies out of each pillar's
     clearance radius every simulation tick.
- Mixamo has no dedicated "mage" character or FPS-arm rig category — picked
  a generic fantasy model (Ganfaul) and re-tinted it per element instead.
  Tried parenting the same full-body model to the camera as a first-person
  viewmodel; reverted because its shoulder pauldrons dominate the frame at
  FPS-camera distance during the cast gesture. A dedicated Sketchfab FPS-arms
  asset or a vertex-filtered arm-only mesh would be needed to revisit this.
- GitHub Pages' CDN edge cache is inconsistent across edge nodes — `curl`
  confirming a new deploy is live doesn't mean every visitor's edge has it
  yet. Standard workaround: `?v=N` cache-busting on the page URL after a push.
- Multi-zone world: kept every zone (3 dungeon levels + hub) coexisting
  statically in one scene rather than swapping scene contents per level —
  simpler to reason about (no teardown/rebuild bugs) at the cost of a bit
  more constant memory for meshes that are usually off-screen. Enemy/player
  bounds-clamping and pillar collision became zone-parameterized instead of
  using one hardcoded global arena.
- The shop started as a modal dialog, but pointer lock (needed for FPS mouse
  look) hides the real OS cursor and pins it at screen center — clicks on
  modal buttons landed nowhere until the player pressed Esc to release the
  lock themselves. First fix was releasing the lock on shop open, but the
  user asked for a better redesign: **the shop is now physical, not a
  modal** — glowing stands with floating price tags in the hub room,
  interacted with via E based on proximity alone (not whether the tag
  happens to be on-screen, which only gates the tag's own visibility). This
  sidesteps the pointer-lock/click problem entirely since there's no DOM
  button to click during gameplay, and reads as more diegetic.
- **Bug report: "bought spell didn't appear in the hotbar"** — turned out
  the player pressed E on the mentor stand (leveled up their active spell)
  while thinking they were at the buy stand — gold was deducted, so nothing
  was actually broken, but the two orb-pedestals look similar enough to
  confuse under dim hub lighting. Fixed by making the bottom-screen interact
  prompt name the exact spell/action ("Купить 💥 Огненный шар — 50💰") instead
  of a generic verb, so it's unambiguous which stand you're about to use
  without needing to look up at the floating tag.
- **"Got 43 gold from level 1, not enough for the 50g spell"** — first fix
  made gold-per-kill a flat 10 so the total was always exactly 50, but the
  user then asked to keep the randomness for flavor and instead just cap
  the cost below the worst case. Landed on: gold-per-kill stays random
  5-15, and the cost (20) sits below the guaranteed floor (5 kills × 5g
  min = 25) — random rewards, but the spell is affordable even on the
  unluckiest possible level-1 clear.
- Волна's redesign (single-target-with-radius → self-centered nova) needed
  its own network message type (`castNova`) and host resolver
  (`hostResolveNova`) separate from the normal `cast`/`hostResolveCast`
  path, since every other spell requires hitting (or aiming near) a specific
  enemy — Волна is the first spell with no target at all, keyed off the
  caster's own authoritative position instead. The generic FX decay loop
  gained an optional `growFrom`/`growTo` scale animation (used by both the
  explosion sphere and the nova ring) rather than adding a second effects
  system just for expanding shapes. **Superseded** by the full AoE rewrite
  below — every spell now uses this same self-centered-area shape, so
  `castNova`/`hostResolveNova` were merged into the single `castArea`/
  `hostResolveArea` path rather than existing as a special case.
- "Where do I get proper ability animations from?" — for a browser/Three.js
  prototype this means particle textures, not skeletal animation (the caster
  body already uses Mixamo). Went with Kenney's CC0 Particle Pack
  (kenney.nl/assets/particle-pack) — free, no-attribution-required license,
  and its sprites (soft flame, soft glow, ring) are designed exactly for
  additive-blended billboard particles. Downloaded the ~15MB pack, kept only
  the 4 textures actually used (~256KB), discarded the rest (Unity samples,
  black-background variants, unused sprite variants) rather than committing
  the whole archive.
- Built a minimal CPU-side particle system (position array rewritten per
  frame, no GPU shader) rather than pulling in a particle library — plenty
  fast at the ~10-30 particles per burst this prototype uses, and much
  easier to read/tweak than a shader-based system for a throwaway prototype.
- **"Couldn't connect from two different computers, worked fine as two
  browsers on one machine"** — `new Peer()` was using PeerJS's default
  config, which ships STUN only, no TURN. STUN-only WebRTC works for two
  browsers on one machine (or a simple/permissive LAN) because direct P2P
  punch-through is trivial there, but fails outright for real players
  behind symmetric NAT or a restrictive router/firewall — no relay fallback
  once direct connection attempts fail. Fixed by adding an explicit
  `iceServers` list (multiple STUN + OpenRelay's public demo TURN server,
  metered.ca/tools/openrelay — free, no signup) to both `new Peer()` calls,
  and raising the join timeout from 8s to 15s since TURN relay negotiation
  is slower than same-LAN direct P2P. The demo TURN server is shared/public
  and could get rate-limited under real load — swap in your own free TURN
  credentials (metered.ca or Twilio) if that happens. **Needs a real
  two-computer retest to confirm — could not be verified from this
  environment (no second physical machine).**
- **"Lags/freezes, especially with 2+ players" — initially misdiagnosed as
  the same networking issue above; it wasn't** (the user retested on one
  machine, where TURN is never needed, and the freezing was still there).
  Root cause: every spell hit was adding a `THREE.PointLight` to the scene
  for its impact flash and `scene.remove()`-ing it a fraction of a second
  later (explosion/nova/bolt/chain flashes, plus **every single
  Fire/Water projectile** — the most frequent case, since that's the basic
  attack). Adding or removing a light forces WebGL to recompile every lit
  material's shader program — a genuinely expensive synchronous stall, and
  it happens more often the more players are actively casting. Fixed two
  ways: (1) dropped the projectile's point light entirely — the unlit
  glowing sphere already reads as "glowing" without it; (2) added an
  8-light pool for the remaining impact flashes (bolt/chain/explosion/nova)
  — "spawning" a flash now just repositions/re-lights an existing pooled
  light instead of adding a new one, so the scene's light count never
  changes after startup. Verified in-session: casting Огненный шар 5 times
  in a row now leaves the scene's total light count unchanged (62 before,
  62 after) — previously each cast would have added and removed one.
  Also found and fixed a related but separate issue while investigating:
  all 4 zones' ~50 torch/pentagram/shop-stand lights were permanently
  live simultaneously (a WebGL forward renderer evaluates every scene
  light for every lit fragment regardless of visibility), even though the
  player is only ever in one zone at a time. Added `setZoneLightsVisible()`,
  toggled at zone transitions (not per frame, to avoid re-triggering the
  same recompile problem), cutting the always-on light count from 50 down
  to whichever zone's own lights are actually relevant (9-21 depending on
  the zone).
- **"Freezing is much better, but still there"** — the light-churn fix
  above was a real, confirmed contributor, not the whole story; some
  residual cause remains unidentified. Not yet root-caused.
- **A newly-joined remote player's model appeared at the world origin and
  visibly slid across the room to the pentagram** — `makeRemotePlayer()`
  left the mesh at THREE's default `(0,0,0)`, and the per-frame
  interpolation (`position.lerp(...)`) animated it from there to its real
  position over the next few frames instead of appearing there immediately.
  Fixed with a one-time "placed" flag: the very first position update for a
  remote player snaps the mesh directly to that position; only subsequent
  updates lerp (for smooth movement once they're actually walking around).
- **Level 2 became a real maze** (recursive-backtracker over a 9×9 grid,
  6-unit cells — matches the zone's own clamp bound almost exactly so
  there's no open ring around the outside to just walk past the maze).
  This needed three things the old pillar-field didn't: (1) real box
  collision, shared by both the player and enemies, since walls actually
  block movement now (pillars only ever push enemies out via a circle
  check — players walked through them freely); (2) a small BFS pathfinder
  for chasing enemies (the grid is tiny — 81 cells — so recomputing the
  path every simulation tick per chasing enemy is cheap and avoids
  stale-path bugs), since straight-line "walk at the player" steering
  would just walk into a wall and get stuck; (3) maze-aware
  spawn-point/wander-target selection (random open cells instead of an
  arbitrary point or a ring formula, both of which could land inside a
  wall). Verified in-session: a BFS path between opposite corners of the
  maze is 57 cells long (proof the maze isn't trivially open), the maze is
  fully connected, and a chasing enemy placed 2 cells away behind a wall
  correctly detoured through the corridors and reached the player within
  60 simulated seconds rather than getting stuck.
- **Tried generating a custom "water mage" model with Meshy AI** (text-to-3D)
  to give Water its own distinct look instead of a re-tinted Ganfaul.
  Multiple prompt iterations hit the same handful of failure modes:
  "elemental" in the prompt pulled toward a non-human creature (hooves,
  horns, tail — broke Mixamo Auto-Rigger's human-skeleton assumptions and
  visibly sank into the floor); flowing robes/hoods/loose cloth caused
  rigging artifacts (a recurring, well-known issue — cloth has no physics
  in a simple bone-weight rig, it just clips); and each fix attempt
  sometimes silently dropped an earlier constraint (asked for a jumpsuit,
  got no clothing at all) — long prompts with many stacked negative
  constraints appear to lose weight on earlier details. None of the
  iterations produced a clean result before switching approaches.
- **Switched to Brady** (a built-in, pre-rigged Mixamo catalog character)
  instead of continuing to fight AI generation — sidesteps rigging entirely
  since Mixamo characters come pre-rigged with the same `mixamorig`
  skeleton already used by the animation pipeline. Tradeoff accepted
  knowingly: Brady's own textures are far heavier than Ganfaul's — idle.fbx
  (which carries the mesh, "With Skin") is **~116MB** vs. Ganfaul's ~7MB,
  a real page-load cost for a web multiplayer prototype. User's call to
  keep it as-is rather than spend time compressing textures in Blender.
  Also: Mixamo "With Skin" downloads for some characters embed
  **multiple material slots per mesh** (an array) instead of one material —
  Brady is one of these, and `makeRemotePlayerModel`'s per-mesh material
  clone/tint (`o.material.clone()`) crashed on the array case; fixed to
  handle both shapes. Movement animation is Running (not Walking) per
  explicit request — swap was just a file substitution, no code changes
  needed since 'walk' is just an internal state-machine key, not tied to
  the literal animation content.
- **Cast animation and the actual shot didn't line up** — the game fired
  the projectile/hitscan/nova the instant the player pressed cast, while
  the third-person Magic Spell Casting clip takes ~1.2s (at its sped-up
  timeScale) to reach the arm-fully-extended pose — so viewers saw the bolt
  leave the hand before the throwing motion got there. Split casting into
  two steps: an immediate `castStart` fx message (triggers the visual
  gesture on every client right away, does nothing else) and the actual
  shot logic moved into a `setTimeout` delayed by
  `CAST_ARM_EXTEND_TIME / CAST_ANIM_TIMESCALE` (3s into the original clip,
  scaled down by the same speed-up factor already applied to the
  animation) — so the delay stays correct even if that speed-up factor is
  retuned later. `activeSlot` is snapshotted at press time (`castSlot`)
  since the player can scroll to a different spell during the delay
  window. Verified in-session: both the projectile and lightning hitscan
  now land ~1.2s after `tryCast()` is called, matching the intended delay.
- **Two FPS-viewmodel attempts, both reverted, then switched to third-person
  instead.** Attempt 1 (Ganfaul): its shoulder pauldrons dominated the frame
  at FPS-camera distance during the cast gesture. Attempt 2 (Brady, after
  fixing the head-clipping issue by pushing the model back from the camera):
  a *different* failure — the cast animation's extended arm reaches out
  along roughly the same axis the camera looks down, so from directly
  behind/inside that axis it foreshortens into a near-invisible point rather
  than reading as a visible arm shape. Two different models, two different
  specific symptoms, same root cause both times: a full-body third-person
  asset simply glued to the camera doesn't work as a viewmodel without a
  dedicated FPS rig or per-frame IK. Rather than sink more time chasing a
  third camera-offset combination, switched the whole game to a
  **third-person camera orbiting behind the player's own head** instead —
  the player's body renders exactly like a remote player's (same
  `makeRemotePlayerModel`, positioned at `playerPos` and yaw-rotated), and
  the camera sits `THIRD_PERSON_DISTANCE` behind a head-height pivot, pulled
  in by a raycast (`resolveThirdPersonCameraDistance`) when a wall or pillar
  would otherwise be between the camera and the pivot. This sidesteps both
  prior failure modes entirely (there's no camera-relative offset to tune —
  the animation just plays on a normally-proportioned body seen from a
  normal viewing angle) and was verified working — including the cast
  animation itself, which is now genuinely visible — in the same
  console-driven pass that confirmed camera pull-in near a wall.
- **Removed cursor aiming entirely — every spell converted to a
  self-centered area effect.** With only one Running clip and a third-person
  camera (no crosshair concept that made sense anymore), aiming had become
  vestigial. `SCHOOL_SPELLS` dropped `speed`/`splashRadius`/`splashDmgMult`/
  `selfNova` in favor of one uniform `radius` field per spell; the separate
  `hostResolveCast` (aimed) and `hostResolveNova` (self-nova) resolvers were
  merged into a single `hostResolveArea(msg, casterPid)` keyed off the
  caster's own authoritative position from `getPlayerPositions()` — Fire/
  Water/Lightning AoE damage, Water's wet application, and the Water→
  Lightning Chain Shock synergy (including its own inner chain-radius loop)
  all live in this one function now. All now-dead projectile/hitscan
  infrastructure was deleted outright rather than left unreachable:
  `spawnTracer`, `spawnLightningBolt`, `spawnProjectile`, `resolveHit`,
  `updateProjectiles`, and the `raycaster`/`projectiles` globals. Verified
  via direct console calls to `hostResolveArea`: a lone wet+Lightning hit
  deals exactly `dmg × SYNERGY_MULT` (10 × 3 = 30), and two wet enemies
  within chain radius of each other each end up taking their own synergy
  hit *plus* the other's chain hit (30 + 15 = 45 each, from 100 → 55) —
  confirms the chain math isn't double-counting. Also confirmed the full
  `tryCast()` → delayed `castArea` dispatch pipeline actually lands damage
  ~1.2s after the button press, not just the resolver function in
  isolation. **Caveat found while testing**: enemies run their own
  patrol/chase AI every frame, so directly poking `enemy.mesh.position` in
  a console test gets silently overwritten a frame or two later by that
  AI — any future scripted combat test needs to move the *player* next to
  a stationary/tracked enemy position (or hook the AI's own target state),
  not just teleport the enemy mesh and assume it'll stay put across a
  multi-frame delay.
- **"Character always runs forward even when strafing or backing up"** —
  there's only one directional (forward) Running clip, no strafe/backward
  variants, so playing it while moving sideways looked like the character
  was skating rather than running. Fixed without any new animation assets:
  the body's yaw now tracks the actual movement vector
  (`Math.atan2(-move.x, -move.z)`) while moving, and only snaps back to the
  camera/aim yaw once movement stops — matters for the cast gesture, which
  should still visibly "throw" toward where the player is looking once
  they've stopped to cast. Verified by dispatching synthetic `keydown`
  events (`KeyW`/`KeyS`/`KeyD`) at the `window` (the game's listener is on
  `window`, not `document` — a raw `document.dispatchEvent` doesn't reach
  it) and reading `ownBodyGroup.rotation.y`: 0° holding W, 180° holding S,
  -90° holding D, snapping back to 0° (the current yaw) on release.
- **"Run animation plays faster than the character actually moves"**
  (foot-sliding) — first pass added a `MOVE_ANIM_TIMESCALE` (0.6) applied to
  the walk `AnimationAction`, tuned by eye against `MOVE_SPEED`, on the
  (wrong) assumption the clip was in-place. **Superseded** — see the later
  "model runs away from the camera" finding below, which found the clip
  actually has real root motion baked in; 0.6 was unknowingly compensating
  for that, not for an actual pace mismatch.
- **"Run animation is still faster than the actual movement, and running
  backward/sideways sends the character flying off-screen"** — reported
  against the third-person camera added earlier this session. Rather than
  keep tuning that camera, went back to **first-person view with
  placeholder box/sphere arms** per user request (the two prior
  real-viewmodel attempts had already shown a full-body asset glued to the
  camera doesn't work well as a viewmodel — see the entry below). The
  third-person-behind-the-head camera and the Brady-as-own-body code are
  preserved on the **`third-person-camera-aoe`** branch in case they're
  worth revisiting later (e.g. with a dedicated FPS arm rig instead of a
  full-body model). Returning to first-person removes the reported symptoms
  for the *local* player entirely (there's no longer a visible own-body
  model to glitch), but the underlying "only one directional Running clip"
  issue still exists for **remote** players' bodies (which always render in
  third person, from a teammate's point of view) — so the strafe-facing fix
  from earlier in this session (facing the actual movement vector instead
  of always the aim yaw) was additionally ported over to the remote-player
  render path (`rp.mesh.rotation.y`, derived from each remote player's own
  actual per-frame position delta rather than the aim yaw they broadcast),
  verified via a scripted fake remote player driven through backward/
  strafe/idle target sequences (0°/180°/-90°/back-to-0°, matching the local
  fix's earlier verification exactly). The reported "runs off-screen"
  symptom itself was never root-caused beyond "was specific to the
  third-person camera" — it may have been the camera's own wall-avoidance
  raycast reacting to the mismatch between the animation's facing and the
  player's real movement, but this wasn't confirmed before switching away
  from that camera entirely.
- **All AoE combat math (self-centered radius damage, Water wet
  application, and the Water→Lightning Chain Shock synergy) re-verified
  after the first-person revert** — none of it depends on camera mode or
  the local player's own body existing, so no regression was expected, and
  a scripted cast against a repositioned enemy confirmed a landed hit in
  first-person view exactly like the earlier third-person test.
- **"Bring back ranged attacks"** — after returning to first-person view, the
  self-centered-AoE-only combat (no aiming at all) no longer fit: aiming is
  natural again in first person, so it was reverted back to the original
  aimed design. Fire/Water are flying projectiles again (`spawnProjectile`/
  `updateProjectiles`, hit-tested against enemies on impact), Lightning is
  an instant hitscan raycast against the crosshair (`spawnLightningBolt` for
  the zigzag visual), and `hostResolveArea`/`castArea` were split back into
  the original two paths: `hostResolveCast`/`'cast'` for anything that hits
  a specific raycast-resolved enemy, and `hostResolveNova`/`'castNova'` kept
  as the one deliberate exception — Волна (Water tier 2) stays a
  self-centered nova with no aiming, since "instant burst around yourself"
  is its whole mechanical identity, not a workaround for missing aim
  support. `SCHOOL_SPELLS` got its `speed`/`splashRadius`/`splashDmgMult`/
  `selfNova` fields back (the interim `radius`-only shape is gone). The
  self-centered-AoE version and the third-person camera it was designed
  around are both still preserved on the `third-person-camera-aoe` branch.
  Verified via scripted casts against a repositioned enemy: Fire tier-1
  (Искра) projectile lands for exactly its base 15 dmg, Lightning hitscan
  for 12 dmg, Fire tier-2 (Огненный шар) for its base 22 dmg with splash
  wired for enemies within `splashRadius`, and Water tier-2 (Волна) hits
  every enemy in `selfNova` radius with damage + wet and no aiming
  involved — all four paths confirmed with zero console errors.
- **Rebuilt the class-selection screen** (was a small centered modal with 3
  plain buttons) into a full-screen, 3-card layout with a live 3D preview
  per element: a clone of the shared mage model (via the same
  `makeRemotePlayerModel()` used for actual players) tinted to that
  element's color, slowly rotating, run by a dedicated
  `requestAnimationFrame` loop separate from the main game loop since the
  picker can be shown before the game (and its own `animate()`) has
  started. Since `idle.fbx` is ~116MB, the previews often need to show
  *before* `mageTemplate` finishes loading — each card shows a small CSS
  spinner + "Загрузка модели…" (`.epLoader`) over its empty canvas region
  until `mageTemplate` is ready, then the real model fades in as that
  card's spinner is hidden (a placeholder capsule was tried first here —
  see Findings below for why it was replaced). Canvas sizing needs one
  `requestAnimationFrame` deferral after
  `display:flex` is set — a `display:none` element reports `clientWidth`/
  `clientHeight` of 0, so sizing synchronously on show would produce a 0×0
  canvas. Hover-highlight is plain CSS (`:hover` + a per-card
  `--el-color`/`--el-glow` custom property set inline) — no JS state
  needed. Spell blurbs are a separate small lookup table keyed by spell
  name (`SPELL_BLURB`) rather than baked into `SCHOOL_SPELLS`, so
  balance tuning there can't accidentally desync the flavor text; only the
  name/icon are pulled live from the real spell data. Lore paragraphs are
  static hand-written prose per card (2 sentences each) — not derived from
  anything, since there's no lore document yet for the three orders.
- **"Game hangs after picking host, class-selection screen takes forever
  to appear"** — the initial class-selection implementation gave each of
  the 3 cards its own independent `THREE.WebGLRenderer`/canvas/GL context.
  Diagnosed with a `PerformanceObserver({entryTypes:['longtask']})` probe:
  clicking host produced two back-to-back main-thread-blocking tasks
  totaling ~12.5s (7.8s + 4.7s). Root cause was **not** the mesh cloning —
  it was WebGL shader compilation: 3 separate GL contexts each need their
  own independent compile of this model's (many-bone) skinning shader on
  first render, and that compile is synchronous and expensive. Fixed by
  switching to a **single shared `THREE.WebGLRenderer`** for all 3 cards —
  one full-viewport `<canvas>` absolutely positioned over the `.epCards`
  row (`pointer-events:none`, so hover/click still reach the actual card
  `<div>`s underneath), with each card's model drawn into its own
  `renderer.setScissor()`/`setViewport()` region computed from that card's
  `.epCanvasWrap` `getBoundingClientRect()` every frame. One shared GL
  context means Three.js's program cache only has to compile the shader
  once for all 3 (functionally identical) cloned materials — this alone
  cut the freeze to a single ~2.7s task. Went one step further and moved
  even that remaining cost off the critical path entirely: `warmupElementPreviews()`
  builds the shared renderer and renders one throwaway 64×64 off-screen
  instance the moment `mageTemplate` finishes loading (in the background,
  usually while the player is still reading the main menu — on a real
  first-time ~116MB download they're on that screen far longer than the
  compile takes anyway), so by the time they actually open the picker the
  shader is already warm. Re-measured after this fix: clicking host now
  produces at most a single ~79ms task — imperceptible. Also fixed a
  z-index/paint-order detail while restructuring: the shared canvas must
  paint *above* each card's own translucent background (otherwise that
  background visibly dims the 3D model drawn beneath it) but the scissored
  regions leave everything else on the canvas fully transparent, so the
  card's text underneath is unaffected despite being "under" a
  higher-z-index element.
- **"Placeholders show for a couple seconds before the real models"** — the
  freeze fix above meant loading no longer *blocked* the picker, but the
  generic capsule placeholder sitting there for a couple seconds still
  read as broken/unfinished rather than "loading." Replaced it with an
  actual loading state: each card's `.epCanvasWrap` shows a CSS spinner +
  "Загрузка модели…" (`.epLoader`, plain `border-top-color` spin
  animation) instead of adding a placeholder model to that card's scene at
  all. `elementPreviews[el].model` is `null` until `upgradeElementPreview()`
  builds the real one; `elementPreviewLoop()` simply skips rendering into a
  card's scissored region while its model is null, leaving that part of
  the shared canvas transparent so the spinner underneath shows through
  (relies on standard per-pixel alpha compositing — the canvas's paint
  order/z-index being *above* the card doesn't matter for transparent
  pixels, only opaque ones actually obscure what's beneath). The moment a
  card's real model is added, it starts drawing into that region every
  frame and naturally covers the spinner completely from then on — no
  explicit "did the render happen" bookkeeping needed beyond hiding the
  spinner `<div>` itself (belt-and-suspenders, since the model would visually
  cover it either way once `usingReal` flips). Verified the spinner is
  present at the correct moment (not just briefly on a cold cache) by
  checking `getComputedStyle` on `.epLoader` synchronously right after
  clicking host, before any animation frame has run — all 3 read `flex`
  even when `mageTemplate` had already finished loading at page-load time,
  confirming the loading state always shows for at least one frame rather
  than being racy.
- **Split the single shared cast animation into two, one per spell tier**
  (`cast1.fbx` = "Standing 1H Magic Attack 01" for tier-1 spells,
  `cast2.fbx` = the original "Magic Spell Casting" for tier-2), per user
  request, so each tier reads as visually distinct rather than every spell
  in the game playing the exact same gesture. This meant the single
  `CAST_ARM_EXTEND_TIME`/`CAST_FIRE_DELAY_MS` timing pair (and the single
  `actions.cast`/`'cast'` animation-state key throughout
  `triggerCast`/`advanceCharacterAnimation`) had to become two of
  everything — `CAST_ARM_EXTEND_TIME_T1`/`_T2`,
  `CAST_FIRE_DELAY_MS_T1`/`_T2`, `castAnimDurationMs1`/`2`, and
  `actions.cast1`/`cast2` as separate `AnimationAction`s sharing one
  `AnimationMixer`. `triggerCast(entity, tier)` now picks the clip key from
  `tier`, and the `castStart` fx message carries `tier` over the network so
  remote viewers play the *correct* clip for what the caster is actually
  casting, not always the same one. Getting the new clip's release-frame
  timing right needed the user to read it off directly in Mixamo's own
  preview (34th of 69 frames) rather than guessing a duration-relative
  percentage — Mixamo exports at 30fps, so `34/30 ≈ 1.133s` into the
  original (unscaled) clip is when the arm actually releases, mirroring how
  the original clip's "~3s mark" was identified the same way. Verified via
  `triggerRemoteCast(pid, tier)` on a scripted fake remote player: tier 1
  plays `cast1` and returns to idle at ~920ms (2.3s clip / 2.5x timescale),
  tier 2 plays `cast2` and returns at ~1707ms (4.27s clip / 2.5x timescale)
  — independent of each other, no cross-contamination between the two
  action states. Also re-verified the full `tryCast()` pipeline end to end
  for tier 1: the projectile now visibly leaves the hand ~0.45s after
  pressing cast (down from the shared clip's ~1.2s), matching `cast1`'s
  much shorter, snappier swing instead of the longer tier-2 gesture.
- **Gave Fire its own model** (`idle_fire.fbx`, ~120MB) instead of a
  re-tinted Brady, per user request — the first school to get a distinct
  look. Verified its skeleton before wiring anything up (loaded it
  standalone and inspected bone names): 65 bones, all `mixamorig`-prefixed,
  identical convention to Brady's, so `walk.fbx`/`cast1.fbx`/`cast2.fbx`
  retarget onto it with zero extra work — same pattern as when Ganfaul was
  swapped for Brady earlier. Code-wise, `mageTemplate` (Water/Lightning)
  and `mageTemplateFire` are now two parallel template objects loaded
  together via one `Promise.all`, and `makeRemotePlayerModel(color,
  element)` picks the right one by element, falling back to the shared
  template for anything that isn't `'Fire'` — keeping the door open to add
  more per-school templates later without restructuring again.
- **The class-selection freeze came back the moment Fire's model was
  added, and the earlier "warmup" fix turned out to have never actually
  been fixing the real cause.** Re-profiled with the same
  `PerformanceObserver` longtask technique plus manual `performance.now()`
  timing dropped into the warmup and upgrade code paths, and found: the
  throwaway warmup scene's very first render *did* pay a real ~2.3s
  shader-compile cost (confirmed in the console), and cloning was
  confirmed cheap (2-3ms) — so the earlier diagnosis of "which cost" was
  right. What was wrong was the assumption that warming it once would warm
  the *real* scene too. The real per-card scene, rendered later through
  `elementPreviewLoop()`'s scissored-viewport path, paid its own separate
  ~2.2s cost anyway, despite an apparently-matching light setup between
  the two scenes (this was tried first, as the obvious mismatch — didn't
  fix it). Rather than keep guessing at exactly which Three.js internal
  state differed between the throwaway warmup scene and the real one,
  replaced the whole approach: **the class-selection panel is now always
  laid out** (`display:flex` permanently; a `.epVisible` class toggles
  `visibility`/`pointer-events` instead of `display:none`/`flex`), so its
  cards have real, correctly-sized canvas regions from the moment the page
  loads, whether or not the player has opened the screen yet. This means
  `ensureElementPreviews()` — the exact same function and render path used
  when the screen is actually visible — can just be called directly as
  soon as `mageTemplate`/`mageTemplateFire` finish loading, with no
  separate warmup implementation to keep in sync with the real one at all.
  Re-measured after this change: clicking host now produces no freeze
  whatsoever (longtask entries near the click are ~50-80ms, and don't even
  coincide with the click timing) — confirmed on a completely fresh page
  load, not just a warm-cache repeat. Lesson for next time: when a
  "pre-warm a side-effect" fix doesn't hold up under a changed condition
  (here: a second distinct model), prefer making the warmup literally reuse
  the production code path over trying to more carefully replicate its
  conditions in a parallel implementation — the parallel version can only
  ever be as correct as your understanding of every input that affects the
  cost, and this one was missing something neither the light setup nor the
  scissor state (both checked) turned out to be.
- **Removed per-element model recoloring** now that Fire has its own
  distinct model — tinting made more sense when Water/Lightning/Fire were
  all the same re-colored Brady mesh as the only way to tell them apart at
  a glance; now that models can just look different from each other, the
  user asked to drop the tint and keep each model's original art
  (skin/cloth colors as authored) instead. `makeRemotePlayerModel()` still
  clones every material per-instance (never shares a Material object
  across players — kept in case a future per-instance effect, e.g. a
  downed-state fade, needs to mutate one) but no longer overwrites
  `.color`; the function's now-unused `color` parameter was removed
  entirely rather than left as dead weight. Element color is still used
  for the floating name tag above each player's head (`makeNameSprite`)
  and for the brief pre-load placeholder capsule (`makePlaceholderModel`,
  shown for the handful of frames before a real model finishes loading) —
  neither of those has an "original" appearance to preserve, so tinting
  those still makes sense for at-a-glance element identification.
- **"Let me see my own animations without opening a second browser"** —
  added a debug-only third-person toggle (**V**) rather than reworking the
  main camera again (a real third-person mode was already tried and
  reverted twice this project — see the earlier FPS-viewmodel/third-person
  Findings — reintroducing it as the default would repeat that). `V`
  lazily builds `debugOwnBody` — the *exact* model/animation setup a
  teammate would actually see for this player (`makeRemotePlayerModel`,
  same function remote players use) — hides the FP placeholder arms, and
  pulls the camera back `DEBUG_TP_DISTANCE` (3.2) behind the head pivot
  using the same yaw/pitch-derived look-direction formula as the earlier
  (reverted) third-person camera. `tryCast()` additionally calls
  `triggerCast(debugOwnBody, spell.tier)` when the toggle is on, so the
  correct cast clip (`cast1`/`cast2`) plays immediately, matching exactly
  what a remote viewer would see. No wall-collision pull-in like the old
  third-person camera had — not needed for a debug tool checking
  animations in the open. Verified via `window.__debug`: toggling sets
  `debugThirdPerson`, the camera's distance from `playerPos` matches
  `DEBUG_TP_DISTANCE` exactly, and `tryCast()` correctly drives
  `debugOwnBody.current` to `'cast1'`/`'cast2'` depending on spell tier —
  confirmed with a live screenshot showing the Fire model's cast pose in
  third person before a browser-pane rendering hiccup (session-tooling
  issue, unrelated to the game) cut the visual verification short; the
  toggle-off path is the same first-person camera code that existed before
  this change, unmodified, so it wasn't independently re-verified visually
  this session.
- **Debug third-person view showed the model facing the camera, running
  backward from itself** — `ensureDebugOwnBody()` set `mesh: model.object`
  directly (the raw clone returned by `makeRemotePlayerModel`) instead of
  wrapping it in an outer `THREE.Group` the way `makeRemotePlayer()` does
  for actual remote players. The clone itself carries a baked-in
  `rotation.y = Math.PI` correction (documented where it's set — Mixamo's
  cast animation reaches toward world +Z, but this game's forward
  convention is -Z), meant to be read as a *child* offset underneath a
  separate yaw-only wrapper. Setting facing directly on the clone
  overwrote that correction instead of composing with it, so the body
  faced backward relative to its actual movement. Fixed by wrapping in a
  `THREE.Group` exactly like remote players, and setting position/rotation
  on the wrapper. Verified the fix reuses an already-proven-correct
  composition (the same wrapper pattern real remote players use, confirmed
  working over an actual P2P connection earlier this session) rather than
  re-deriving the rotation math from scratch.
- **"Running animation is faster than the movement, and the model slides
  away from the camera"** — measured the Running clip's `mixamorigHips`
  position track directly: it travels from Z≈1.4 to Z≈403 (in the FBX's cm
  units) over one loop, i.e. **the clip has real baked root motion**, not
  the in-place cycle it was assumed to be. That root motion was stacking
  with the game's own code-driven `playerPos` movement — the character
  moved twice, once from actual position updates and once from the
  animation itself physically translating the mesh — which is exactly why
  it looked like it was "running away." Rather than asking for a
  re-exported "In Place" version from Mixamo, fixed it in code:
  `stripHorizontalRootMotion()` flattens the hips track's X/Z values to a
  constant right after the clip loads (keeping Y so the up/down running
  bob survives), run once in `loadMageAssets()`. With the root motion
  gone, `MOVE_ANIM_TIMESCALE` could finally be *derived* instead of
  guessed: the stripped clip's own natural pace is
  `(402.81-1.38)cm × 0.01 scale / 0.6333s duration ≈ 6.34 units/sec`, so
  `MOVE_ANIM_TIMESCALE = MOVE_SPEED / 6.34` makes the leg-cycle match
  actual ground speed exactly, replacing the old guessed `0.6`. Verified by
  reading the clip's track values after loading in-browser: X and Z are
  now bit-identical across every keyframe (only Y still varies).
- **Class-selection screen showed a black rectangle and never finished
  loading** — became noticeably worse after Fire got its own ~120MB model
  (on top of Brady's existing ~116MB): the per-card loading spinners only
  cover "the model object isn't built yet," but the actual `FBXLoader`
  parse of a large binary FBX is synchronous and blocks the whole page,
  same root cause as the earlier shader-compile freeze (see that Finding)
  but for parsing instead of GPU compilation — with now roughly 2x the
  combined model data to parse, that block is more likely to still be
  running exactly when a player clicks host/join, at which point the
  page can look frozen/blank with zero feedback for its whole duration.
  Added a full-screen `#assetLoadingScreen` gate: `showElementPicker()`
  now checks `mageTemplate && mageTemplateFire` and only reveals the real
  class-selection panel once both are truthy; otherwise it shows the
  loading screen and waits on a `mageAssetsReady` promise (resolved at the
  end of `loadMageAssets()`) before revealing it. A progress bar tracks
  aggregate bytes loaded across all 5 files via each `FBXLoader.load()`
  call's `onProgress` callback. **Caveat, stated plainly**: this does not
  eliminate the underlying freeze — a synchronous main-thread parse still
  can't paint or animate anything (including this very loading screen's
  own spinner) while it's running — it only guarantees a legible "still
  loading" message is on screen *before* that block starts, instead of a
  black rectangle with no explanation. Actually eliminating the pause
  would need moving FBX parsing off the main thread (a Web Worker), which
  is a larger change not attempted here.
- **Gave Water its own model** (`water_mesh.fbx` + `water_idle.fbx`), same
  pattern as Fire, but with a wrinkle: this rig was auto-rigged in a
  separate Mixamo session, so every bone is named `mixamorig10Whatever`
  instead of the plain `mixamorigWhatever` every other rig (and the shared
  walk/cast1/cast2 clips) uses — otherwise bit-identical, same 65 bones in
  the same hierarchy order (verified by stripping the numeric infix from
  both bone lists and diffing). `AnimationMixer` binds a clip to a
  skeleton purely by matching track name strings against bone names, so
  the shared clips would have silently done nothing on this skeleton
  (bones just sitting at bind pose while internal state said "walking").
  Added `remapClipToSkeleton(clip, newPrefix)` — clones the clip (so the
  original, used unmodified by every other model, is untouched) and
  rewrites each track's `.name` to swap the prefix. `templateFor(element)`
  now picks between the shared template, Fire's, or Water's, falling back
  to shared for any element whose own model isn't ready yet (in practice
  never observed, since all templates load in one `Promise.all` and
  become truthy at the same instant). Verified in-browser by manually
  advancing a real `AnimationMixer` (`mixer.update(0.3)`, no render loop
  needed) and reading the `mixamorig10Hips` bone's quaternion before/after
  — confirmed it actually changes for both the remapped walk clip and
  cast1, i.e. the remap genuinely drives the skeleton rather than silently
  no-op'ing.
- **"Lock movement during cast"** — reused the existing `castLockedUntil`
  timestamp (already used to block re-casting until the current cast
  animation finishes) as the movement gate too:
  `if (!myDowned && !isCasting) { ...read WASD... }` in the main loop,
  where `isCasting = performance.now() < castLockedUntil`. Since that
  timestamp is set once per cast to the real animation duration (not the
  spell's possibly-shorter cooldown), a player now has to stand still for
  the whole visible gesture, matching what a teammate watching them cast
  actually sees, rather than being able to strafe away mid-swing.
- **"Projectile spawns from the wrong place"** — the spawn point used to
  be a hand-guessed fixed offset from the camera
  (`camera.localToWorld(new THREE.Vector3(0.32, -0.25, -0.6))`) that
  didn't quite line up with the visible FP arm/orb, especially once the
  cast-recoil animation moved the arm forward. Replaced with the *actual*
  live world position of `fpOrb` (`fpOrb.getWorldPosition(...)`) — the
  same object the walk-bob/cast-recoil code already animates every frame
  — so the shot now always leaves from exactly where the glowing orb on
  the hand visually is, however the arm happens to be posed at fire time.
  Since this fires from a `setTimeout` rather than the render loop, called
  `fpOrb.updateWorldMatrix(true, false)` first to guarantee a fresh
  matrix rather than trusting whatever was last computed during a render.
- **"Loading screen shows 100% while still loading, and the bar isn't
  full at 100%"** — the progress bar tracked raw download bytes across all
  7 model/animation files, but each file's promise only resolves after
  the FBX is *parsed* too (synchronous, can take real time for the ~50MB+
  files) — so 100% downloaded routinely showed before the objects were
  actually usable, and the last file's parse-time gap between "100% bytes"
  and "actually done" is exactly what made the bar look stuck/wrong.
  Capped the byte-based percentage at 90% (`Math.min(90, ...)`) so it can
  never claim done while a file might still be parsing, and added an
  explicit second phase after `Promise.all` resolves — see the next
  Finding — that owns the 90-100% range and only reports "Готово!" once
  everything, including shader warmup, is truly finished.
- **"Class-selection screen still freezes — now it's Lightning's model
  that's loading"** — `ensureElementPreviews()`'s comment already explains
  *why* each distinct model/material needs its own real WebGL shader
  compile the first time it's rendered (a proper throwaway warmup pass
  doesn't trigger the same compile Three.js ends up needing for the real
  per-card scissored-viewport render path). The gap was *when* that
  compile happened: lazily, one card per animation frame, inside
  `elementPreviewLoop` — which only starts trying once the class-selection
  panel might already be visible. With three now-distinct models (Fire,
  Water, and the shared Lightning/Brady material), whichever one happened
  to compile last could freeze the page while the player was already
  looking at the (seemingly ready) screen. Added `warmupAllElementPreviews()`,
  called from inside `loadMageAssets()` *before* `mageAssetsReadyResolve()`
  fires — it upgrades all 3 cards to their real models (paying every
  shader compile) while `#assetLoadingScreen` is still up, showing
  "Подготовка моделей…" at 90-100%, yielding one tick between each card so
  the text can actually repaint between compiles. `showElementPicker()`'s
  gate now only reveals the panel once this entire phase is done, so by
  the time it's visible every card is already fully built. Verified with a
  cache-busted reload (`?v=2` — a plain `navigate()` to the same URL can
  silently keep running a previously-loaded ES module instance, which cost
  some confusion mid-debugging): all three `.epLoader` spinners read
  `display:none` and the loading screen reads `Готово!` at `100%` by the
  time the DOM is first inspected after reveal — i.e. no card is left to
  freeze the now-visible screen.
- **"Water's idle pose looks like the cast animation"** — `makeRemotePlayerModel()`
  called `actions.idle.play()` but the pose wasn't actually *applied* to
  the skeleton until the caller's next `mixer.update(dt)`, which for a
  freshly-built model could be a frame or more away. Until then, the
  skinned mesh shows whatever pose Mixamo baked as the raw FBX bind
  pose — for a model exported while a non-idle animation was selected in
  Mixamo's own preview (plausible for `water_mesh.fbx`, whose *own*
  embedded animation is a 6-second spell-cast gesture, per its filename),
  that raw bind pose is not a neutral T-pose. Fixed by calling
  `mixer.update(0)` immediately after `actions.idle.play()` inside
  `makeRemotePlayerModel()`, forcing the idle pose onto every model the
  instant it's built rather than trusting the next render. (Numeric check
  during investigation: the right hand's world position from a clean idle
  clip evaluation matched exactly, `[0.4827, ...]` twice independently
  measured, confirming the *clip itself* was always correct — the bug was
  specifically the one-frame gap before it got applied.)
- **Gave the FP viewmodel a real model — third attempt** — the previous two
  (Ganfaul, then Brady) were reverted for two different specific
  geometric reasons (shoulder pauldrons dominating the frame; the cast
  arm's reach foreshortening to invisibility along the view axis), both
  tracing to "a full-body third-person asset glued to the camera isn't a
  dedicated FPS rig." Tried again per user request, now against the
  current models, via `ensureMyViewmodel()` — same
  parented-to-camera approach as the placeholder arms, offset by
  `FP_VIEWMODEL_OFFSET = (0, -EYE_HEIGHT, 0.15)` (head at/behind the
  camera, per the same tuning logic the earlier attempts used). Falls back
  to the placeholder arms automatically if `mageTemplate`/`myElement`
  aren't ready. The projectile/hitscan spawn point now prefers the real
  viewmodel's own right-hand bone over the placeholder orb when available.
  Added a debug aid alongside this, per user request: while in the debug
  third-person view (V), a small cyan dot (`debugEyeMarker`) marks exactly
  `(playerPos.x, EYE_HEIGHT, playerPos.z)` — the real FP camera's eye
  position — so the offset can be judged and retuned by eye from outside
  the FP view itself, without needing to flip back and forth blind.
  **Stated plainly**: `FP_VIEWMODEL_OFFSET` is a first-pass guess carried
  over from the previous attempts' tuning logic, not re-verified visually
  this session (the browser preview pane's render loop was suspended for
  this entire session — confirmed via a `requestAnimationFrame` counter
  staying at 0 after 2 real seconds — so nothing depending on live
  rendering, camera movement, or animation playback could be visually
  checked; only state reachable via direct `mixer.update()`/property
  inspection was verified). Given this exact technique failed twice
  before for framing reasons a screenshot would have caught immediately,
  the offset should be treated as unverified until checked visually.
- **Water idle pose confirmed a source-asset bug, not code** — rendered
  `water_idle.fbx`'s clip directly (off-screen canvas + `toDataURL()`
  screenshot technique, since the render loop was stalled all session) and
  visually confirmed its actual pose data is a crouched, claws-out
  aggressive stance, not a neutral standing idle, despite being sourced
  from a file the user got from something named "Standing Idle" on Mixamo.
  Fixed by dropping `water_idle.fbx` entirely and retargeting Fire's
  already-confirmed-good idle clip onto Water's `mixamorig10*` skeleton via
  the same `remapClipToSkeleton()` used for the shared walk/cast clips.
- **Fire's spark spawning far from the model — root cause found by reading
  the code, not by screenshot** (the base64 screenshot round-trip kept
  corrupting on write — `binascii.Error` on a string whose length was off
  by ~21 chars each time — so this one was diagnosed from the camera/hand
  math directly instead of chasing that further): `myViewmodelHandBone`
  (the projectile's spawn source) is parented to `camera`, and the debug
  third-person view pulls `camera.position` back by `DEBUG_TP_DISTANCE`
  (3.2 units) from the body's actual head pivot. So while the *visible*
  debug body sat at the real player position, the hand bone used for the
  spawn point was still riding along with the camera — now 3.2 units away.
  Fixed by giving `debugOwnBody` its own hand-bone reference
  (`debugOwnBodyHandBone`, found the same way `myViewmodelHandBone` is) and
  having `tryCast()` prefer it whenever `debugThirdPerson` is active.
  Real gameplay (non-debug) was never affected — there the camera always
  sits at the true eye position.
- **Debug third-person view now free-orbits** instead of trailing behind
  wherever the real aim direction points. Added `debugOrbitYaw`/
  `debugOrbitPitch`, a second yaw/pitch pair that mousemove drives only
  while `debugThirdPerson` is on (the real `yaw`/`pitch` — movement facing
  and aim — are left untouched). The camera now sits on a sphere of radius
  `DEBUG_TP_DISTANCE` around the head pivot and `camera.lookAt(headPivot)`
  every frame, so mouse-look genuinely orbits around the character instead
  of just re-aiming a chase cam. Pointer lock is requested automatically on
  entering the debug view (previously required an extra click).
- **FP camera retuned** per user feedback that the raw eye point felt too
  high and too far back: added `FP_CAMERA_HEIGHT_OFFSET = -0.15` and
  `FP_CAMERA_FORWARD_OFFSET = 0.12`, applied only to the real (non-debug)
  camera position, not `EYE_HEIGHT` itself (which other things, like
  `debugEyeMarker` and the debug view's head pivot, still reference as the
  "true" eye height for comparison). First-pass numbers — not yet
  re-verified visually for the same render-loop-stall reason as above;
  retune by feel if still off.
- **Replaced regular (non-boss) enemies' procedural box/sphere model with a
  real Goblin FBX** (`goblin_run.fbx` — mesh+skeleton+run animation "With
  Skin", ~19MB; `goblin_attack.fbx` — motion-only "Without Skin", ~500KB).
  Same standard `mixamorig*` bone naming as the mage rigs, so the attack
  clip binds directly onto the run file's skeleton with no remap. No
  dedicated idle animation was provided — `makeGoblinModel()` freezes the
  run clip's own frame 0 (`actions.idle.paused = true`) as a static idle
  pose instead, the same fix pattern used for Water's missing idle. Scale
  (`GOBLIN_SCALE = 0.01`) and facing (`GOBLIN_ROTATION_OFFSET = Math.PI`)
  were both derived from measurement, not guessed: the raw import bounding
  box is ~210cm tall (matches the mage rigs' cm-export convention, and at
  0.01 scale lands almost exactly on the old procedural enemy's ~2.07m
  height, so the hp bar keeps the same y=2.4 constant for both); the attack
  clip's `Hips.position` track shows a clear windup-then-lunge pattern
  along local Z (range ~107cm vs ~25cm on X), lunging toward +Z — the same
  "+Z is forward at rotation.y=0" convention as the mage rigs, so the same
  180° correction applies for the same reason.
  `makeEnemy()` now branches: goblin model for regular enemies (falls back
  to the old procedural box if `goblinTemplate` hasn't finished loading —
  a level shouldn't block starting on a ~19MB download), unchanged
  procedural box for the boss. Attack/run/idle crossfading reuses the same
  `{mixer, actions, current}` shape and Loop-Once/`clampWhenFinished`
  pattern as the mage cast clips, via new `triggerGoblinAttack`/
  `advanceGoblinAnimation` helpers (kept separate from the mage
  `triggerCast`/`advanceCharacterAnimation` since those hardcode the
  `cast1`/`cast2` state names) — `actions.attack.setDuration(ATTACK_ANIM_DURATION)`
  keeps the visual in sync with the existing melee gameplay timing
  regardless of the source clip's native length. Facing while moving is
  recomputed from the position delta every frame (`Math.atan2(-dx,-dz)`,
  same convention as remote players/debug body) — no network field needed,
  since it's derived identically on host and clients from the same synced
  position. Wet-status tinting now lerps each material's own original
  color toward a blue tint instead of hardcoding a replacement hex, since
  the real model can have more than one material (it has two: body + eyes).
  **Screenshot technique note**: the base64 PNG round-trip through the
  Write tool corrupted the data at ~40-48K characters (twice in a row,
  same `binascii`/PNG-checksum failure as earlier in this project) but
  worked reliably once the render was shrunk to a small JPEG (64×64,
  ~1-2.5KB base64) instead of a full-size PNG — small-and-lossy beats
  large-and-lossless for this specific verification workflow given this
  constraint. Use that size/format going forward for any similar check.
- **"Не вижу гоблинов" — real bug, found via the above screenshot
  technique on the actual live game scene** (not a synthetic test scene):
  the goblin model renders completely correctly (right pose, right scale,
  right skinning — confirmed by re-rendering the same live enemy with
  boosted test lighting and seeing a normal idle stance), but its actual
  texture is dark, and this dungeon's real ambient light
  (`HemisphereLight` 0.32 + `DirectionalLight` 0.22, deliberately dim
  outside torch radius) is too low to make that dark texture visible more
  than a couple meters from a torch. The old procedural placeholder used
  flat light-gray colors that stayed visible in the same dim light, so
  this regression was invisible until the real model was actually
  rendered in place. Fixed by giving each cloned material a small baseline
  `emissive` (`0x554128`, a dim warm tint matching the torches' own
  color) in `makeGoblinModel()` — confirmed by re-rendering the same
  in-game enemy before/after: invisible → a barely-there silhouette
  (first pass, `0x2a2018`) → a clearly readable dim goblin (final value).
  Boss unaffected (still the procedural box, untouched by this change).
- **FP camera offsets increased** per follow-up feedback that the first
  pass (`-0.15`/`0.12`) wasn't enough: `FP_CAMERA_HEIGHT_OFFSET` is now
  `-0.35`, `FP_CAMERA_FORWARD_OFFSET` is now `0.3`. Still first-pass
  numbers tuned by feel from the request, not re-verified visually (this
  one isn't screenshot-checkable the same way — it's about how the view
  feels to move around in, not a static pose) — confirm and retune once
  more if still off.
- **Colors lost on the goblin model, fixed**: the flat baseline emissive
  above (`0x554128`) does make the dark texture visible, but at low
  ambient light that flat color dominates the surface and reads as one
  uniform brown blob — that was the actual regression the user reported
  as "colors are gone." Fixed by using the diffuse map itself as the
  `emissiveMap` (emissive color white, `emissiveIntensity` 0.35) instead
  of a flat hex — confirmed by rendering the goblin off-screen with only
  dim ambient light: flat-emissive showed no texture detail at all,
  emissiveMap showed full skin/scar/hair detail even in near-darkness.
- **Real FP viewmodel abandoned for good — back to placeholder arms.**
  Three separate attempts across sessions to glue a full third-person
  character model to the camera as the FP viewmodel (Ganfaul, Brady, this
  project's own mage model — the last one even got as far as a working
  arms-only shader clip + a live offset-tuning tool) all hit the same
  wall: a full-body asset without a dedicated FPS rig/IK doesn't work as
  a viewmodel, and every attempt needed more and more machinery
  (positioning hacks, then a bone-weight shader clip, then a live tuning
  UI) to fight the same fundamental mismatch. User's call this session:
  stop investing further and commit to the placeholder box/sphere arms
  (`makeFpArm()`) as the actual FP visual, not a fallback. Removed
  `ensureMyViewmodel()`/`myViewmodel`/`myViewmodelHandBone`/
  `myViewmodelHandGlow` and all call sites entirely.
  - **The exploratory arms-only-clip + live camera/viewmodel-offset-
    tuning work from this session is preserved on its own branch**,
    `web-mvp-camera-viewmodel-experiment`, per user request — not merged
    into main. Revisit it there if a real viewmodel is worth another
    attempt later (e.g. with a proper FPS arm rig instead of a
    third-person body).
  - **Placeholder-arm/projectile sync, requested alongside the revert**:
    the recoil animation that punches `fpArmR` forward was previously a
    fixed timing (peak at 0.08s, rest by 0.3s) that had nothing to do
    with the actual shot timing — it was tuned as a generic "snap" for
    when the real viewmodel was the primary visual and the placeholder
    was just a rarely-seen fallback. Now that the placeholder IS the
    projectile spawn point (`fpOrb`, a child of `fpArmR`) every time,
    made the recoil envelope's peak land exactly at `castFireDelayMs`
    (tier-specific, already existed) instead of a fixed 0.08s — see
    `castRecoilPeakT`/`castRecoilEndT`, set fresh in `tryCast()` each
    cast. `spawnProjectile()`/the hitscan path already read `fpOrb`'s
    live world position at fire time (pre-existing code, previously just
    a fallback when no real hand bone existed) — verified in the browser
    pane this session: switched the active spell to a `speed > 0`
    (flying-projectile) spell and confirmed `tryCast()` → after
    `castFireDelayMs` → a projectile spawns at `fpOrb`'s actual world
    position, no console errors. Did not verify the recoil visually
    frame-by-frame (same rAF sandbox limitation as other camera work
    this session) — the timing math and spawn-position code path are
    confirmed correct, but the felt motion should still get an eyeball
    check in a real browser.
- **Per-player spawn/despawn portals, one model per class (Lightning
  wired up first — Fire/Water still pending the user placing their
  files).** User supplies one `.blend` per element, exported to FBX by
  hand (`models/portal/portal_<element>.fbx` + a same-named `.png`,
  texture NOT embedded — Blender's FBX exporter needs "Embed Textures"
  checked for that, and this file wasn't exported that way, so the
  loader fetches the PNG separately). New functions near
  `loadGoblinAssets`: `ensurePortalTemplate`/`instantiatePortal`/
  `refreshPortal`/`updatePortal`.
  - **Real bug caught in testing, not hypothetical**: the first version
    called `spawnPortalInstance()` synchronously from
    `respawnLocalPlayer()`, which returned `null` every time because the
    element's FBX/PNG fetch — only ever triggered lazily, on first
    need — hadn't resolved yet at that exact moment (confirmed via
    Network tab: the fetch was still in flight when the synchronous
    check ran). The portal silently never appeared. Fixed by making
    `refreshPortal()` take an `onReady(portal)` callback: fires
    synchronously if the template's already cached, or once — later —
    the first time that element's load actually finishes, via a
    `portalWaiters` map. Guarded against a second respawn superseding a
    still-pending first one with a `myPortalGen`/`rp.portalGen` counter
    (discards a stale deferred result instead of letting it clobber a
    newer portal). Verified fixed: `scene.traverse` found the portal's
    "Plane"/"Plane006" meshes at exactly `playerPos` after this change,
    where before the fix nothing was found at all.
  - **Model is dark under this dungeon's dim ambient light — same root
    cause as the goblin, fixed the same way** (`emissiveMap` = the
    portal's own diffuse texture, `emissiveIntensity 1.0` since a portal
    should read as fully self-lit, vs. the goblin's subtler 0.35).
    Off-screen renders before the fix showed the ground-ring ember
    points completely dark; after, they show as clearly glowing
    yellow — confirmed the fix reaches the actual asset, not just
    theory. The tall standing column part ("Plane006") still reads
    mostly as a dark silhouette even after the emissive fix — worth a
    look in a real browser; may need additive blending instead of
    normal transparency, or its UVs may sample a naturally darker part
    of the texture atlas. Not chased further this session — handing off
    for the user's own visual check as requested.
  - **Scale is a first guess, not verified**: the raw model measured
    ~6.7m across / ~2.2m tall at scale 1 (bounding-box check via
    `Box3`), scaled down to `PORTAL_SCALE = 0.4` (~2.7m) as a
    walk-through-sized guess. The model's own 2 baked animations
    ("PlaneAction"/"Plane.006Action", one per mesh) each have only 2
    keyframes but an absurd native duration (~5791 — almost certainly an
    FBX export quirk, not literally a 5791-second clip); compressed to
    `PORTAL_SPIN_SECONDS = 4` per loop via `action.setDuration()`, same
    trick already used for `ATTACK_ANIM_DURATION`.
  - **Despawn radius** (`PORTAL_EXIT_RADIUS = 2.2`) and the "fresh
    portal on every respawn, not the same one following you" behavior
    are implemented for both the local player (hooked into
    `respawnLocalPlayer()`) and remote players (hooked into their first
    placement + the `downed→!downed` transition in the "Remote player
    interpolation" block) — the remote-player path is untested this
    session (solo playtest only, no second client), local-player path
    confirmed via `scene.traverse` position checks as above but NOT
    watched end-to-end through an actual walk-away (same rAF-doesn't-
    reliably-tick sandbox limitation as other findings this session).
  - **Next**: user checks Lightning in a real browser (visibility,
    scale, spin speed, despawn distance) and reports back; once
    Fire/Water `.fbx`/`.png` land in `models/portal/`, they work
    automatically with zero code changes (same lazy per-element loader).
- **Removed the old shared spawn-point pentagram** (canvas-drawn decal +
  point light, one per zone) — per-player portals replace it now, and
  it was very likely the direct cause of a reported flicker: it sat as
  a 5×5 transparent plane at the exact same spawn point a portal now
  also occupies, `depthWrite: false` on both, so the renderer's
  transparent-object depth sort between two near-coplanar surfaces
  would flip order as the camera moved — classic z-fight/flicker
  symptom. Deleted `makePentagramTexture`/`makePentagram`/
  `updatePentagram`/the `pentagrams` array and its per-frame call in
  `animate()`; `setZoneLightsVisible(currentZone)` (previously called
  right after the pentagram-creation loop) moved to stand on its own —
  other zone lights (torches, hub glow) still register fine without it.
  **Not independently re-confirmed that this fully fixes the flicker**
  (would need to actually pan the camera around and watch, which this
  session's sandboxed pane can't reliably do) — if it's still there
  after this, the portal's own two meshes ("Plane" ground ring +
  "Plane006" standing column, both `depthWrite: false`) fighting each
  other is the next thing to check.
- **That predicted next thing was exactly it** — user confirmed the
  flicker survived the pentagram removal. Fixed by giving each of the
  portal's 2 meshes a fixed, distinct `renderOrder` (0 and 1, assigned
  in traversal order in `ensurePortalTemplate`) instead of leaving them
  to three.js's default per-frame back-to-front distance sort. Both
  meshes are transparent with `depthWrite: false` (needed for the glow
  to blend right) and visually overlap, so as the camera moved, the
  distance-based sort could flip which one painted first — with no
  depth write to fall back on, that flip was directly visible as
  flicker. `renderOrder` is copied onto every clone automatically
  (`THREE.Object3D#copy` carries it through), so this fixes it for
  every instance, not just the template. **Not independently
  re-verified this session** (same sandboxed-pane camera-movement
  limitation as the first attempt) — next report from the user is the
  real confirmation.
- **Scale bumped twice on user feedback**: 0.4 (initial) → 0.6 (asked
  for 1.5x) → 1.2 (asked for "3x the original", meaning 3× the
  *initial* 0.4, not 3× the already-bumped 0.6 — worth double-checking
  this reading was right if the size still isn't what they meant).
  `PORTAL_EXIT_RADIUS` scaled proportionally alongside it each time
  (2.2 → 3.3 → 6.6) so the despawn distance keeps clearing the visible
  ring instead of the portal disappearing while still partly on
  screen.
- **`renderOrder` didn't fix the flicker either — wrong diagnosis.**
  That fix targets draw-ORDER flipping between two transparent objects
  (a painter's-algorithm problem); the actual symptom here is GPU
  depth-buffer z-fighting, a different mechanism entirely: the portal's
  ground-ring mesh sits at local y=0, and `instantiatePortal` placed the
  whole clone at world y=0 too — exactly coplanar with the stone floor.
  Two surfaces at (near-)identical depth fight over which one wins the
  depth test per-pixel, and that winner can flip as the camera angle
  changes, which reads as flicker regardless of draw order. This is the
  exact same problem the old pentagram decal's own code comment already
  named ("just above the floor to avoid z-fighting") — should have
  applied that lesson the first time instead of reaching for
  `renderOrder`. Fixed the same way: `instantiatePortal` now places the
  clone at world y=0.03 instead of 0 (a fixed offset, not scaled by
  `PORTAL_SCALE` — it only needs to clear the floor's depth, not track
  portal size). Kept the `renderOrder` assignment too since it's still
  correct for the (separate, real) draw-order concern between the
  portal's own 2 meshes — just wasn't the cause of this particular
  symptom.
- **Dedicated portal antechambers for level 1 & level 2**, with a real
  walk-through door (per user request — asked specifically for a
  physical passage, not another teleport-trigger like the hub's existing
  "continue portal", and for the door to open on E and stay open
  permanently, "можно сделать часть стены, которая уезжает вниз" —
  implemented literally as a wall slab that slides down into the floor).
  - **Where it lives geometrically**: both zones already had empty world
    space between them and the next zone (level1's north wall at z=20,
    level2's own zone starting at z=-41 — a 21-unit gap nobody used).
    `buildArena()` gained an optional `doorGapWidth` param that cuts a
    matching gap in a zone's north wall instead of one solid box; a new
    `buildPortalRoom()` builds a small room (9×7) right outside that gap
    with its own 3 walls + floor + a door slab exactly filling the gap.
  - **No enemy-side collision needed** — checked `clampEnemyToArena()`
    before building anything: it already clamps every enemy to
    `zone.half - 1`, a full unit short of the original wall line,
    regardless of aiState (patrol or chase). The antechamber sits
    entirely beyond that wall, so enemies are physically incapable of
    reaching it even mid-chase — no new containment logic required, and
    the existing `SPAWN_SAFE_RADIUS` ward around the old spawn point is
    now redundant-but-harmless (kept as-is rather than ripped out).
  - **Player collision**: door starts as a real solid entry in a new
    `ZONE_DOOR_WALLS[zoneId]` array, resolved via the same
    `resolveCircleWallCollision()` already used for level 2's maze —
    just called one more time per frame. `ZONES[1]`/`ZONES[2]`'s own
    `spawn` moved into the new room (computed from `half`/
    `PORTAL_ROOM_DEPTH`, not hand-picked numbers); the per-zone movement
    clamp needed a matching widen on its +z bound
    (`ZONE_PORTAL_ROOM[zone].depth`) or the generic square clamp would
    have invisibly walled the room off before the player could ever
    reach the door.
  - **Open flow**: E within 3 units of a closed door → `openZoneDoor()`
    drops it from the collision array immediately (no half-open-but-
    still-solid state) and flags it for the slide-down animation
    (advanced in `animate()`, ~2.5 units/sec down to y=-3). Proximity
    prompt is a new `updateDoorPrompt()`, parallel to
    `updateShopStands()` — that one only runs during `shopPhase` and
    doors matter outside it, so they needed separate state
    (`activeDoorAction`) rather than reusing `activeStandAction`.
    **Real bug caught before it shipped**: `updateDoorPrompt()`
    originally just `return`ed early during `shopPhase` without clearing
    `activeDoorAction` — a stale reference from right before a
    level→hub teleport would've stayed truthy and let a leftover E-press
    hijack a shop-stand interact instead. Fixed by clearing it
    explicitly on that early return.
  - **Resets every fresh level entry**: `startLevel()` now calls
    `resetZoneDoor()` — each new attempt at a level (including looping
    back to level 1 after the boss) gets its portal-room door sealed
    again, matching how `buildLevelEnemies()` already rebuilds enemies
    fresh each time.
  - **Verified this session**: navigated in, confirmed `playerPos`
    lands in the new room (z≈23.7 for level 1, matching the computed
    spawn), confirmed both door meshes exist at their exact expected
    closed position (`y=2`, `z=20` and `z=-41`), and rendered the room
    off-screen — small enclosed space, portal glowing on the floor,
    torches on the walls, looks right. **Not verified**: actually
    walking up to a door, pressing E, watching it slide down, and
    confirming the player can then walk through into the arena — this
    session's sandboxed pane can't reliably tick `requestAnimationFrame`
    (per the recurring note throughout this file), so the full
    interactive loop (movement + collision + the E-key handler) was
    never exercised end-to-end. This is the main thing to check first.
  - **Level 3 (boss) and the hub were deliberately left alone** — user
    only asked for levels 1 and 2.
- **Portal-room follow-up round** (all confirmed working, then 5 more
  asks in one message):
  - **One shared portal instead of one per player.** Removed the whole
    per-player/per-remote-player portal system (`myPortal`/`myPortalGen`
    on the local player, `rp.portal`/`rp.portalGen`/`rp.element` on
    every remote player, plus their cleanup-on-disconnect code) in favor
    of a single `zonePortal`, fixed at the zone's own `spawn` point
    rather than each player's jittered position. `respawnLocalPlayer()`
    already jittered spawn position within a 1-unit radius for "everyone
    appears close together but not stacked" — that part needed no
    change at all, only the portal itself stopped being per-player.
  - **Room bigger**: `PORTAL_ROOM_WIDTH`/`DEPTH` 9×7 → 16×13.
  - **A lever next to the door** (`makeLever()`) — the door alone wasn't
    an obvious "this is how you leave" cue. A small pivoting prop (base
    + handle + glowing knob + point light) that swings from a resting
    angle to a pulled-down one in sync with the door's own slide
    animation. `ZONE_DOORS[zone].lever` carries the pivot + both angles;
    `resetZoneDoor()` snaps it back to resting alongside re-sealing the
    door.
  - **Taller walls + a real ceiling**: new `PORTAL_ROOM_HEIGHT = 7`
    (vs. the normal dungeon's 4), used for the room's walls, its door
    slab, and a new ceiling plane (`stoneMaterial`, mirrored rotation
    from the floor). No ceiling existed anywhere else in this game
    before — this is the first one. Door's slide-down target adjusted
    to match (`-PORTAL_ROOM_HEIGHT` instead of a flat `-3`) so it still
    fully clears the taller opening.
  - **Dirt floor**: new `makeDirtTexture()`/`dirtMaterial()` pair, same
    canvas-generation approach as the existing stone-brick texture but
    irregular mottled blotches instead of a brick grid (dirt doesn't
    read as regular rows/columns) plus dark pebble/fleck speckling. Used
    for the antechamber floor only — the rest of the dungeon keeps its
    stone brick floor.
  - **Verified this session**: player still lands at the (recomputed)
    antechamber center; `scene.traverse` found exactly 2 ceiling planes
    and exactly 2 lever knobs (matched by their distinctive
    `0xffcc44` color to rule out false positives from other small
    scene spheres) at the expected world position next to each zone's
    door; door mesh height reads 7, matching the new room height.
    Off-screen render shows an enclosed room with the dirt-toned floor
    around the glowing portal. **Not verified**: actually walking up to
    a lever, pulling it, and confirming the door+lever animate together
    in real time — same recurring rAF-sandbox limitation as everything
    else interactive this session.
- **Four more asks in one message** (dirt everywhere, smaller goblins,
  button instead of lever, menu lag):
  - **Dirt floor for the whole dungeon**, not just the antechamber.
    `buildArena()`'s floor now uses `dirtMaterial()` for every zone
    except `'hub'` (kept its original stone — it's the deliberately
    different, calmer progression room, not "the dungeon"). One-line
    change since `buildArena()` already builds every level's floor
    (including under level 2's maze) through this single code path.
  - **Goblins 1.5x smaller**: `GOBLIN_SCALE` `0.01` → `0.01/1.5`.
    Measured the actual bounding-box height afterward (`makeGoblinModel()`
    + `Box3`) to confirm: 1.40m, matching 2.1m/1.5 exactly. Also scaled
    `GOBLIN_HP_BAR_Y` down by the same 1.5x (`2.4` → `1.6`) — the HP bar
    is at a fixed height shared by all enemy types, so shrinking only
    the goblin model without also lowering its bar would've left the
    bar floating oddly high above its now-shorter head.
  - **Button instead of lever, and more noticeable.** Replaced
    `makeLever()` with `makeButton()` — a pedestal-mounted glowing red
    push-button (`BUTTON_COLOR = 0xff3322`) instead of a swinging
    handle. Idle-pulses (emissive intensity + point light both breathe
    via a sine wave, same trick as the torch flicker elsewhere) so it
    draws the eye even before it's used; sinks down and goes flat grey
    once pressed, permanently, so a fired button doesn't keep competing
    for attention. `ZONE_DOORS[zone].button` replaces `.lever`;
    `resetZoneDoor()` restores the red pulse alongside re-sealing the
    door. Interact prompt text: "Открыть дверь" → "Потянуть рычаг" →
    now "Нажать кнопку".
  - **Menu lag while typing the name / right after pressing Host** —
    traced to `loadMageAssets()`: it fires 6 `FBXLoader.load()` calls
    via `Promise.all` the instant the page loads, including two
    ~116-120MB idle meshes. `FBXLoader`'s parse step is synchronous and
    genuinely blocks the main thread on files that size; because all 6
    downloads were kicked off in parallel, their completions (and thus
    their blocking parses) tended to cluster together — two giant
    parses landing back-to-back right as the player was on the name
    screen. Changed to sequential `await`s (one `loadOne()` at a time)
    instead of `Promise.all([...])` — spreads the parse hitches out
    instead of stacking them, at the cost of a longer total load time
    (downloads no longer overlap). Didn't attempt a deeper fix
    (streaming `fetch()` + manual `loader.parse()` to keep parallel
    *downloads* while only serializing the *parse* step) — meaningfully
    more code and risk (have to hand-roll download-progress reporting
    and get the FBX resource-path argument right) for a session already
    covering a lot of ground; flagged here as the next thing to try if
    sequential loading isn't enough. Verified the rewrite doesn't break
    loading: `mageTemplate`/`mageTemplateFire`/`mageTemplateWater` all
    populated successfully in the browser pane with no console errors.
    **Not verified**: whether it actually fixes the felt lag — that's
    inherently about real timing/scheduling, which needs a real browser
    session to judge, not this sandboxed pane.
- **Big combat-tuning batch, 8 asks in one message.** Unlike most of this
  session, this batch was actually exercised live via `window.__debug`
  (calling `hostResolveCast`/`hostApplyDamage`/`hostSimulate` directly
  with synthetic args) rather than just structural checks — rAF still
  doesn't tick, but these are host-simulation functions, callable
  directly without a render loop.
  1. **Same portal for every class.** New `ZONE_PORTAL_ELEMENT` map
     (zoneId → element), all zones default to `'Lightning'` since it's
     the only asset — `respawnLocalPlayer()` now reads that instead of
     `myElement`. Verified: hosted as Fire, portal geometry still
     Lightning's.
  2. **Goblin melee — the real bug was zero telegraph, not the
     numbers.** Damage used to land the instant the lunge started
     (`attackAnimT=0`), same frame — no way to react. Added
     `MELEE_IMPACT_FRAC` (0.6): the hit now fires partway through the
     windup (`ATTACK_ANIM_DURATION` also bumped 0.35→0.7,
     `MELEE_COOLDOWN` 1.1→1.6) via a new `meleePending`/`meleePendingT`
     pair on each enemy, ticked in `hostSimulate`, and **re-checks range
     at the actual impact moment** — backing off after the windup starts
     now makes it whiff. Verified end to end: teleported an enemy onto a
     synthetic target position, stepped `hostSimulate` in small
     increments — HP stayed full through the windup, dropped by exactly
     `MELEE_DAMAGE` only once past the impact fraction. Side effect that
     doubled as a second proof: doing this test with the target inside
     the new portal antechamber made the enemy get clamped back into the
     arena mid-windup by the existing `clampEnemyToArena` bound (which
     antechambers rely on for containment) — target moved out of range
     before impact, swing correctly whiffed.
  3. **HP color coding** (green >60%, yellow >25%, red below — new
     shared `hpFracColor()`): wired into both the enemy 3D bar
     (`e.hpBar.material.color`) and the player's own HUD bar (was a
     fixed CSS gradient, now a JS-set `background-color`). Verified the
     enemy bar via a live hit (post-hit color read back as `33dd44`
     while still >60% hp).
  4. **Out-of-combat regen**: new `pd.timeSinceDamage`, reset on every
     `hostDamagePlayer()` hit, ticked in `hostSimulate`; once it clears
     `REGEN_DELAY_S` (5s) the player heals at `REGEN_RATE_HP_S` (3/s) up
     to max. Verified: hp 50→53 after one simulated 1-second tick with
     the delay already elapsed.
  5. **2nd-ability cooldown, now actually visible.** It was already
     tracked correctly per-slot (`cooldowns[slot]`) — just never shown
     anywhere except a generic "(кд...)" on whichever slot happened to
     be active. Added a real per-slot overlay: a dark curtain
     (`.slotCd`) rising from the bottom as the cooldown counts down,
     plus a numeric readout (`.slotCdText`). Had to split the slot's
     innerHTML into a dedicated `.slotIcon` span first —
     `updateSlotUI()` was doing `el.textContent = icon`, which would've
     wiped the new overlay children every frame. Verified: set
     `cooldowns[0]=0.45` against a 0.6s cooldown, triggered
     `updateSlotUI()` via `switchSlot()` — overlay read back as
     `height:75%`, text `"0.5"`, icon still intact.
  6. **Enemy HP bar shrinks from the right, anchored left** (was
     symmetric from center). Geometry-level trick: `BoxGeometry.translate()`
     moves the bar mesh's own local origin to its left edge (spans local
     x 0→width instead of the default -width/2→+width/2); since
     `Object3D.scale` always scales around the local origin,
     `scale.x` then shrinks purely rightward. Mesh position shifted
     `-width/2` to land back where the old centered bar's left edge used
     to sit. Verified: after a hit, bar's local x stayed at -0.6
     (unmoved) while `scale.x` dropped, confirming the left edge is
     genuinely fixed and only the right one retreats.
  7. **Tier-2 damage increased.** Found the actual reason they felt weak
     while investigating: dps-wise, every tier-2 was *worse* than its
     own tier-1 once cooldown was factored in (e.g. Fire: 22dmg/1.0s=22
     vs Искра's 15/0.6=25) — buying one was a straight downgrade in raw
     output, only worth it for the AoE hook. Fire 22→32, Water 8→13,
     Lightning 14→22 — each tier-2's dps now clearly exceeds its own
     tier-1's.
  8. **Fire→burn DOT, Water extinguishes instead of soaking a burning
     target.** New `applyBurn()`: every Fire hit (both tiers, splash
     victims included) sets `e.burning = BURN_DURATION_S` (4s) and
     `e.burnDps` (60% of that hit's own damage, spread over the
     duration) — ticked alongside the existing Wet decay in
     `hostSimulate`, routed through the normal `hostApplyDamage()` so
     kill-credit/gold/leveling all still fire correctly on a burn kill.
     Water (`hostResolveCast`'s Water branch and `hostResolveNova`) now
     checks `e.burning > 0` first: extinguishes (clears burn, no Wet)
     instead of the normal soak. Burning gets its own tint (goblin:
     `GOBLIN_BURN_TINT` lerp; boss/procedural: `SKIN_BURN`/`CLOTH_BURN`),
     same priority-over-Wet pattern since the two never coexist by
     design. **Verified thoroughly** — this was the one part of the
     batch most likely to have a subtle bug: fire-then-immediate-water
     in one script (no delay) correctly extinguished with no Wet
     applied; water-alone on a fresh target correctly applied Wet
     normally; an EARLIER test that added an artificial delay between
     the fire and water calls produced a misleading "both burning=0 AND
     wet=6" result — turned out to be the burn timer legitimately
     expiring during the real wall-clock delay between tool calls, not
     a logic bug (confirmed by re-running with zero delay).
- **5 more asks, mostly level-2 maze + combat pacing**:
  1. **"Walk/see through some walls" in the maze — found the actual root
     cause, not a maze-generation bug.** Spent real effort checking the
     maze generation itself first (programmatically verified all 49
     cell-to-neighbor N/S/E/W flags are mutually consistent, checked
     actual corner wall-box overlaps at a sample corner — both came back
     clean, no gaps). The real cause: the FP camera sits
     `FP_CAMERA_FORWARD_OFFSET` (0.3) forward of the actual collision
     point, but the old `PLAYER_RADIUS` (0.35) only kept that collision
     point 0.35 from a wall face — so the camera itself could get as
     close as 0.35-0.3=**0.05 units** from a wall, closer than the
     camera's own near-clip plane (**0.1**). Close a wall head-on and it
     could end up behind the near plane and get clipped away — angle-
     dependent, matching "some walls" rather than all of them. Fixed by
     bumping `PLAYER_RADIUS` to 0.55 (0.25 clearance past the near
     plane, real margin instead of a razor edge).
  2. **Maze corridors wider**: 9×9 grid at 6 units/cell (5.4-unit
     corridors) → 7×7 at 8 units/cell (7.3-unit corridors) — kept the
     same "half-width exactly matches the zone's half, no open ring
     around the outside" fit (28 either way) rather than just thinning
     the walls, so it's still a real maze shape, not an open room.
     `wallThickness` also bumped 0.6→0.7 for a bit more collision
     margin at corners. Re-verified the neighbor-flag consistency check
     from the earlier maze debugging on the new 7×7/8-unit grid — still
     0 mismatches.
  3. **Goblins still felt too fast even after the first slowdown** —
     pushed further: `ATTACK_ANIM_DURATION` 0.7→1.1s,
     `MELEE_COOLDOWN` 1.6→2.4s.
  4. **Ability cooldown now starts only once the cast itself finishes**,
     not the instant the cast begins. It used to run concurrently with
     the cast windup (`cooldowns[slot] = spell.cooldown` was set at the
     very start of `tryCast()`), so part of the cooldown was silently
     spent before the spell even went off. Moved that assignment into a
     `setTimeout` fired after `castTotalMs` (the same duration
     `castLockedUntil` already uses to block re-casting during the
     windup, so recast-spam during the windup was never possible either
     way — this only changes when the *visible* cooldown timer starts).
     Verified directly: called `tryCast()`, checked `cooldowns[0]`
     immediately after (still 0) and again after waiting
     `castAnimDurationMs1` (~920ms) — only then did it read ~0.75
     (Разряд's own cooldown).
  5. **Tier-2 cooldowns increased further** on top of the earlier dmg
     bump, since a bigger hit on a short cooldown (now also no longer
     losing part of its cooldown to the cast windup, per #4) would've
     been too spammable: Fire 1.0→1.8s, Water 1.3→2.2s, Lightning
     1.1→2.0s.

- **"Walk/see through walls" was NOT actually fixed by the previous
  `PLAYER_RADIUS` bump (0.35→0.55) — user reported it was still present.**
  Went back in and found the real bug this time in
  `resolveCircleWallCollision()`: it resolves a circle vs. a wall's AABB by
  pushing away from the box's *closest point* to the circle's center. If the
  center is fully **inside** the box on both axes, the closest point equals
  the center itself, so `dx=dz=0` and the push vector is zero — the
  `distSq > 1e-9` guard then silently skips the wall entirely, leaving the
  player embedded in solid wall geometry with no collision response at all
  (camera included, since it's offset from `playerPos`, not from the wall).
  This is reachable in real play: the generic per-zone boundary clamp
  (`playerPos.x = clamp(x, z.cx-half, z.cx+half)`) lands the player exactly
  on the zone's outer edge, but the maze's perimeter walls are *centered* on
  that same edge line with real thickness (0.7), so they straddle it —
  meaning the clamp itself can shove the player's collision point straight
  inside a wall box. Confirmed live: manually reproduced the exact
  boundary-clamp scenario via `window.__debug` (clamped position `(-28,
  -98)` against the actual west-perimeter wall of level 2's maze,
  `minX/maxX/minZ/maxZ = -28.35/-19.65/-98.35/-97.65`) — the position lands
  inside the box and the *old* collision code left it there untouched.
  Fixed by adding an explicit branch for the "center inside box" case: push
  out along whichever of the box's four faces is nearest (standard shallow
  AABB penetration resolution), instead of relying on the degenerate
  circle-vs-point vector. Re-ran the same reproduction against the new code:
  the position now correctly resolves to just outside the wall's nearest
  face (`z: -98 → -98.9`, exactly `radius` past `minZ=-98.35`). This is a
  general fix to the collision function itself, not maze-specific, so it
  also protects the portal-room doors and any other use of
  `resolveCircleWallCollision`.

- **Level 2 antechamber removed — user reported the wall bug was STILL
  happening even after the `resolveCircleWallCollision()` fix above**, and
  asked to drop the separate portal room for level 2 and go back to
  spawning directly in the maze instead (simpler, and one less place for
  this whole class of edge-case bug to hide). `buildPortalRoom(2, ...)` and
  the door-gap argument to `buildArena(2, ...)` are gone; level 2's north
  wall is now solid, matching level 3's plain-wall style. Spawn point is now
  the maze's own center cell (`mazeCellAt`/`mazeCellCenter` on `ZONE_MAZE[2]`
  right after `buildMaze(2, ...)` runs, since the maze doesn't exist yet
  when `ZONES` is first declared) — any cell center is guaranteed wall-free
  by construction. Verified live: `ZONES[2].spawn` resolves to `[0, -70]`
  (the zone's exact center, since a 7-cell grid's middle cell lands exactly
  there), and the nearest maze wall to that point is 3.65 units away.
  Level 1 keeps its antechamber — this only affects level 2, per the
  request.

- **Loading-bar regression during class-select found and fixed**: the
  bar/percentage could visibly jump backward while the mage models were
  loading. Root cause in `updateLoadProgressUI()` — it summed
  `xhr.loaded`/`xhr.total` across every file *that had reported progress so
  far*, not all 6 known files up front. Since `loadMageAssets()` loads its
  ~120MB FBX files one at a time (a deliberate earlier fix for a different
  lag complaint), the moment file 2 started downloading its full size
  joined the "total" sum before any of its bytes had arrived — e.g. file 1
  finishing near 90% could instantly drop to ~45% the moment file 2's
  download began. Rewrote the math to count whole finished files instead of
  raw bytes: `pct = (filesDone + currentFileByteFraction) / 6 * 90`. This is
  monotonic by construction — finishing a file increments `filesDone` by
  exactly 1 in the same tick its own fraction resets from 1 to 0, so the
  displayed value can't dip. Verified by reading the code path and
  reloading the class-select screen with no console errors; a byte-level
  regression is inherently hard to catch with a single fast local-server
  load (files come back too quickly to see the old bug reproduce live), so
  this is a logic-level fix, not a live-reproduction-confirmed one.

- **Wall mirror added in the hub** (the progression room between dungeon
  runs), per user request — "players can see themselves." Real reflection
  via `three/addons/objects/Reflector.js` (imported alongside the existing
  FBXLoader/SkeletonUtils imports, same `three@0.149.0` CDN already used
  everywhere else), mounted flush on the hub's west wall, facing into the
  room. The FP camera never renders the local player's own body, so this
  leans on the `debugOwnBody` mesh that already existed for the V-key
  debug third-person view (`ensureDebugOwnBody()` /
  `makeRemotePlayerModel(myElement)`) — it's now built and kept
  position/rotation/animation-updated every frame unconditionally instead
  of only while that debug view is toggled on, and left permanently visible
  (previously `mesh.visible` was tied to `debugThirdPerson`). It stays out
  of normal FP view on its own (it sits at the player's feet, behind/below
  the eye point) and only actually shows up reflected in the mirror.
  Verified structurally via `window.__debug`: the `Reflector` object is
  confirmed present in the scene graph at the intended wall position/
  rotation (`[-10.44, 1.35, -230]`, `rotation.y = π/2`), and `debugOwnBody`
  is confirmed live (`visible: true`, tracking `playerPos` every frame) —
  no console errors after adding the import and the mirror. Could **not**
  get a pixel screenshot of the actual reflection: the sandboxed browser's
  `requestAnimationFrame`/input timing is unreliable enough (documented
  earlier in this file) that neither mouse-look nor rapid arrow-key taps
  reliably rotated the debug camera to frame the mirror before the next
  frame overwrote the manual camera override. Visual confirmation (does it
  actually look right, any reflection-plane clipping/lighting oddities)
  still needs a real playtest.

- **Mirror caused a regression: the real model's hands became visible during
  normal FP play**, on top of the placeholder FP arms. Root cause was the
  fix right above it — making `debugOwnBody.mesh.visible = true`
  unconditionally (so the mirror always had something to reflect) meant the
  main camera rendered it too, every frame, not just inside the mirror.
  Fixed properly this time: reverted `debugOwnBody.mesh.visible` back to
  `debugThirdPerson`-gated (hidden during normal play, exactly like before
  the mirror existed), and instead made the mirror responsible for its own
  visibility needs. `Reflector`'s own `onBeforeRender` (the hook it uses
  internally to render the reflection texture from a virtual camera) is
  wrapped in `makeMirror()`: right before calling the original, force
  `debugOwnBody.mesh.visible = true` and hide the placeholder FP arms
  (`fpArmL`/`fpArmR`); right after, restore both to whatever they were
  before. The FP arms need hiding too, in the other direction — `camera` is
  itself added to `scene` (so the placeholder arms, parented to `camera`,
  are part of the normal scene graph), meaning the mirror's internal scene
  traversal would otherwise render them floating at the reflected camera
  position as well. Net effect: outside the mirror, only the placeholder
  arms show, exactly like every other room; inside the mirror's reflection,
  only the real class model shows, no placeholder geometry. Verified live:
  after entering a normal level, only the placeholder FP arms are visible
  on screen (screenshot taken) — no real-model hands in view. Also
  confirmed `debugOwnBody.mesh.visible` reads back `false` after a full
  frame in the hub (the mirror's own before/after hooks ran and correctly
  restored it, not leaving it stuck visible), with no console errors.

- **Batch: mirror frame fix, tier-1 no-cooldown, 4-hit goblins, floor back
  to stone, and a new tier-1 spell mastery system + spell wheel UI.**
  - **Mirror frame was invisible** — the single-box frame from the previous
    round was placed *behind* the glass plane, which put it flush with (or
    slightly inside) the wall's own inner face, so it either z-fought with
    the wall or was fully occluded by it. Rebuilt as 4 separate bars (top/
    bottom/left/right) in a `THREE.Group` at the mirror's own position/
    rotation, each pushed slightly *forward* (into the room) instead of
    behind — guaranteed visible, overlapping the glass edge like a real
    picture frame. Verified structurally: exactly 4 matching frame-bar
    boxes found in the scene graph at the mirror's position.
  - **Tier-1 spells (Искра/Плеск/Разряд) now have 0 cooldown** — spammable
    as fast as the cast animation itself allows (`castLockedUntil` already
    blocked re-casting during the windup regardless of the cooldown value,
    so this was a one-line data change). Verified: `SCHOOL_SPELLS` read
    back with `cooldown: 0` on all three tier-1 entries.
  - **Goblins now die in 4 hits** — `ENEMY_MAX_HP` 100→60, sized against
    Искра's 15 dmg (4×15=60) as the reference tier-1 hit, since Water's
    tier-1 is an intentionally low-damage status applier and Lightning's
    real power is its Water-synergy/chain, not solo tier-1 hits. Verified:
    a fresh level-1 goblin's `maxHp` reads back `60`.
  - **Floor reverted to stone brick everywhere** (dungeon zones had been
    switched to a dirt texture in an earlier round) — `buildArena()`'s
    floor material is unconditionally `stoneMaterial(...)` again (matching
    the hub's own floor, which was never changed), and `buildPortalRoom()`'s
    floor too. `makeDirtTexture`/`dirtMaterial` removed entirely (dead code,
    nothing else used them). Verified visually via screenshot — brick floor
    now visible in a live level.
  - **New tier-1 "mastery" system** (replaces the old universal
    auto-instant-level-on-hit-count for tier-1 spells specifically —
    tier-2 keeps that old system unchanged): landed tier-1 hits fill a
    visible XP bar (`spell.useHits`, capped at `TIER1_XP_PER_LEVEL=8`, no
    more auto-leveling mid-fight); the level-up itself only applies at the
    hub's existing mentor stand (`hostLevelUpSpell`, still gold-gated,
    30×level, now ALSO requires the bar to be full first) — matching "между
    локациями" from the request. Capped at `TIER1_MAX_LEVEL=4`, with a real
    mechanical change per level, not just more damage:
    - **Разряд (Lightning)**: hits twice at level 2, thrice at level 3
      (extra `hostApplyDamage` calls in `hostResolveCast`, using the
      pre-synergy `baseDmg` so multi-hit doesn't also multiply Chain
      Shock). At max level, `tryCast()` diverts entirely to
      `startLightningChannel()` — a new 3s continuous channel that
      re-raycasts and strikes every 250ms; movement is locked the same way
      a normal cast already locks it (`castLockedUntil`, read by the
      existing `isCasting` movement gate), but mouse look is untouched, so
      the player stands still but can still aim — matches "стоит на месте,
      но может перемещать курсор" exactly, reusing an existing mechanism
      instead of needing a new one.
    - **Искра (Fire)**: damage keeps scaling via the existing `levelMult`
      formula; at max level `tryCast()`'s projectile branch fires 3
      independent projectiles in a small fan (±0.06 rad) instead of 1 —
      each has its own real hit detection, not a visual-only effect.
    - **Плеск (Water)**: Wet duration now scales with level
      (`WET_DURATION_S + (level-1)*2`); level 2+ also sets `e.slowFactor =
      0.5` (both `ENEMY_CHASE_SPEED` and `ENEMY_WANDER_SPEED` are now
      multiplied by `e.slowFactor`); max level also sets `e.frozenT = 1` —
      a new field checked at the top of the enemy AI block (wrapped in
      `if (e.frozenT <= 0) { ... }`) that skips target acquisition,
      movement AND attacking entirely while frozen. New icy tint
      (`GOBLIN_FROZEN_TINT`/`SKIN_FROZEN`/`CLOTH_FROZEN`) takes priority
      over the burn/wet tints in `updateEnemyVisual()`. `wet`/`frozenT` are
      now also included in the host's `state` broadcast and mirrored into
      remote clients' local enemy objects, so non-host players see the
      correct tint too, not just the host.
    - Verified live via `window.__debug`, calling `hostResolveCast`/
      `hostSimulate` directly with hand-built spell instances: level-2
      Разряд dealt exactly 2× a single hit's damage (26.88 vs the
      independently-computed expected 26.88); max-level Плеск produced
      `wet: 12, slowFactor: 0.5, frozenT: 1` all in one hit, matching the
      formulas exactly; a frozen enemy took 0 net movement over a 0.5s
      `hostSimulate` tick while mid-chase. Could not get a live gold-gated
      mentor-stand purchase through `window.__debug` (`hostLevelUpSpell`
      isn't exposed on the debug object, unlike most other host functions)
      — that one specific path (XP-bar-plus-gold gating at the mentor
      stand) is verified by code review only, not a live call. The Fire
      triple-projectile spread and the Lightning channel's actual feel
      (does re-aiming mid-channel work naturally, does 250ms read as
      "continuous") also still need a real playtest — timing/feel like
      this is exactly the category this file has repeatedly flagged as
      unverifiable in the sandboxed browser (unreliable `requestAnimationFrame`).
  - **New circular spell wheel UI** (replaces the old horizontal 3-slot
    row) — `#slotWheel` positions each equipped spell icon at a fixed angle
    around a circle via `updateSlotUI()` computing `translate(x,y)` per
    icon every time the active slot changes; the CSS `transition: transform`
    on `.slot` is what makes switching read as the wheel spinning, without
    actually rotating any element (deliberately — rotating the icons
    themselves would turn them upside-down partway around the circle;
    only their *position* animates, never their own orientation). Angles
    are evenly spaced by however many spells are actually equipped (1-3),
    with the active one always resolving to angle 0 (straight up). Added a
    small level badge per icon and a dedicated tier-1 mastery XP bar below
    the wheel (separate from the existing party-level XP bar in the top-
    left HUD, which is unrelated). Verified visually via screenshot with
    both 1 and 3 spells equipped (swapped in a second/third spell via
    window.__debug): with 1 spell the icon sits centered at the top of the
    ring; with 3, the active one (highlighted ring) sits on top and the
    other two spread evenly to the lower-left/lower-right, with the level
    badge ("ур.2") visible on a leveled spell — matches the intended layout.

- **3 more asks: spell-switch key, goblin maze AI, shop consolidation.**
  1. **Spell switching moved to Q** (cycles forward through equipped
     spells), replacing the old Digit1/2/3 direct-select hotkeys, which are
     removed entirely. Mouse wheel still cycles too (untouched, wasn't
     asked to change).
  2. **"Goblins in the maze only ever land a hit from point-blank, and act
     like they're bumping into a portal that's already gone" — real root
     cause, not a vague AI complaint.** `clampEnemyToArena()` has a
     `SPAWN_SAFE_RADIUS` (6 units) "ward" around each zone's spawn point
     that always existed to keep enemies off freshly-spawned players — it
     made sense back when levels 1/2's spawn sat in a separate portal
     antechamber, far from the arena. Since the antechamber was removed
     for level 2 (see the earlier "spawn back inside the maze" fix), that
     zone's spawn point is now the maze's own center CELL — and a maze
     corridor is only ~7.3 units wide (buildMaze's 8-unit cells, 0.7 wall
     thickness), *narrower* than the old 6-unit ward radius. Any goblin
     chasing a player anywhere near their own spawn/portal was permanently
     shoved back by the ward before ever reaching `ATTACK_RANGE` (2.6) —
     the player only ever saw a goblin connect on the rare frame the chase
     and ward forces briefly cancelled out near the boundary, which reads
     exactly like "only attacks point-blank." The ward itself has no
     visual, so once the portal mesh fades (see `updatePortal`), the
     invisible collision volume outlives it — "bumping into a portal
     that's already gone" is literally that ward. Fixed by shrinking the
     ward to 2.5 for maze zones only (fits inside one corridor, still
     protects the immediate spawn point) — non-maze zones (1, 3) keep the
     original 6, untouched, since they weren't reported as buggy. Verified
     live via `window.__debug`: placed a goblin one maze cell (8 units)
     from a player parked exactly on the level-2 spawn point and stepped
     `hostSimulate` — with the old code it locked at exactly distance 6.00
     and never landed a hit; after the fix it closes to 2.50 and
     `meleePending` fires (confirmed on a fresh page load — the first
     re-test looked unchanged because the browser had served a *stale
     cached copy* of `prototype.html` despite navigating; a hard reload
     with a cache-busting query string fixed that and the corrected
     behavior showed up immediately — worth remembering for future
     re-tests in this environment).
  3. **Shop consolidated onto one stand — "нет наставника у которого
     повышать умения" / per-class skill tree.** The separate "buy" stand
     (unlock tier-2 for a flat `SPELL2_COST` gold) and "mentor" stand
     (level up whichever spell is active, gold + tier-1 XP gate) are now
     ONE stand: the mentor stand's first offer is "unlock tier-2"; once
     bought, it switches to the existing leveling flow for the active
     slot's spell. This reads as a simple 2-node tree per class (unlock →
     level) instead of two separately-triggered mechanics. Removed the
     `shopBuyStand` object, its `#labelBuy` UI element, and every
     reference to it; `hostBuySpell`/`requestBuySpell`/the `'buy'`
     interact-kind are all unchanged internally, just now triggered from
     `shopMentorStand`'s proximity instead of a separate pedestal.
     Re-centered the remaining stand at the hub's old buy-stand+mentor-
     stand midpoint. Verified: `hostBuySpell`'s exact logic (gold check,
     `cloneSpell`, push) replicated directly against the real
     `playerData` object via `window.__debug` — correctly added Огненный
     шар and deducted 20 gold. The live UI switch (label/prompt text
     changing once tier-2 is bought) could NOT be verified end-to-end this
     time: the sandboxed browser's `requestAnimationFrame` loop was
     completely stalled for this test (confirmed — `myGold`'s client-side
     mirror, which only updates inside the `animate()` loop, never synced
     even after 1.5 real seconds), so the reactive HUD update itself is
     code-review-verified only, not live-rendered. Needs a real playtest
     to confirm the label actually flips after a purchase.

- **3 more asks: true circular wheel motion, a real skill-tree panel, Water damage.**
  1. **Spell wheel now actually arcs around the circle instead of cutting
     straight across it.** Root cause of the previous "wheel" not reading
     as a wheel: each icon's position was set via `translate(x,y)` with a
     CSS `transition: transform`, and CSS transitions interpolate x and y
     *independently and linearly* — for anything more than a small step
     that's a straight chord through the circle, not an arc along its
     circumference (most visible switching between opposite-ish icons).
     Replaced with real per-frame motion: `switchSlot(delta)` now takes a
     step count (not an absolute index) and bumps an unbounded
     `wheelRotTarget` by `delta * 2π/n` — unbounded so cycling past the
     wrap (last slot back to slot 0) keeps spinning the same direction
     instead of snapping backward the "short way." `updateSlotUI()` (runs
     every frame) eases a separate `wheelRotDisplay` toward that target
     with framerate-independent exponential smoothing, and computes every
     icon's x/y from `sin`/`cos` of the swept angle each frame — so the
     points genuinely travel along the ring. Removed the now-redundant CSS
     transition on `.slot`'s transform (would've just added a second,
     conflicting easing pass on top of the new JS-driven one).
     `switchSlot(0)` used to mean "select slot 0 directly" (absolute
     index) — since the signature is now a delta, `enterGame()`'s reset
     call was changed to set `activeSlot`/both wheel-rotation vars
     directly instead. Verified via `window.__debug`: `switchSlot(1)` with
     2 spells equipped moves `activeSlot` 0→1 and `wheelRotTarget` 0→π
     (matches `2π/2`); screenshot before/after confirms the highlighted
     icon and the HUD's "active spell" name both actually swapped.
  2. **Skill tree is now a real panel, not a one-line prompt** — a
     paper-textured overlay (`#skillTreeOverlay`), Caveat handwriting font
     from Google Fonts, opened at the mentor stand (E → "Открыть дерево
     умений") instead of the stand directly executing a purchase. Pointer
     lock is released on open (`document.exitPointerLock()`, needed since
     the panel takes real mouse clicks) and best-effort re-requested on
     close. Two branches drawn from a root node (tier-1, always known):
     the tier-1 chain (levels 2→3→4, XP + gold gated, same rules as
     before) and the tier-2 chain (unlock, then levels 2→3→4, gold only).
     Per the user's "как рисунок на бумаге, где появляются новые точки" —
     only OWNED nodes plus exactly one "next" node per branch are ever
     drawn; buying/leveling a node makes the next one appear, rather than
     showing a fully-drawn, mostly-locked tree upfront. Node states: known
     (filled ink circle), available (pulsing outline, clickable, shows
     cost), pending (tier-1's next level before its XP bar is full — shown
     but not yet clickable, with a live "опыт X/8" readout instead of a
     cost). Clicking an available node calls the same
     `requestBuySpell()`/`requestLevelUpSpell()` used before, unchanged —
     only the UI trigger moved. tier-2's displayed chain is capped at
     `TIER1_MAX_LEVEL` for a tidy, finite-looking tree; `hostLevelUpSpell`
     itself doesn't enforce that cap for tier-2 (only tier-1's XP gate
     does) — a display-only simplification, not a stealth balance change,
     called out in a code comment. Removed the now-dead
     `flashInsufficientFunds`/`fundsErrorUntil` (insufficient-funds
     feedback moved into the panel itself as `flashSkillTreeError`).
     Verified live end-to-end via `window.__debug` (screenshots at each
     step): opened the panel in the hub, bought Искра ур.2 (gold
     100→70, a fresh "ур.3, опыт 0/8" node appeared), then unlocked
     Огненный шар (gold 70→50, its own "ур.2, 30💰" node appeared under
     it) — matches the intended "points appear as you invest" behavior
     exactly. Closed via the ✕ button, confirmed `skillTreeOpen` flips
     back to `false`.
  3. **Water damage set to exactly 80% of Fire's, tier-for-tier** (per user
     request — Water read as too weak a damage dealer even counting its
     Wet/slow/freeze utility): Плеск 5→12 (15×0.8), Волна 13→25.6
     (32×0.8). Every other Water mechanic (Wet duration/slow/freeze
     scaling by tier-1 level, extinguish-on-burning) reads `spell.dmg`
     directly, so this change flows through automatically — no other code
     touched.

- **Skill tree turned vertical** (per user request). Was two horizontal
  rows growing right from a left-side root; now the root sits at the top
  and two columns (tier-1 left, tier-2 right) grow downward —
  `skillTreeNodeXY(col, rowIndex)` instead of `(row, colIndex)`, panel/SVG
  reflowed from a 720×460 landscape box to a 460×680 portrait one
  (`#skillTreePaper` now `min(480px,92vw)` × `min(680px,88vh)`). Verified
  live via screenshot: root "Искра ур.1" at top-center with lines dropping
  down-left to the tier-1 next-node and down-right to the tier-2 unlock
  node.

- **Lightning multi-hit had zero visual feedback — "нет анимации двойного
  удара на втором уровне".** The extra strikes added for Разряд level 2/3
  (see the earlier tier-1 mastery entry) only ever called
  `hostApplyDamage()` — a bigger number on the HP bar was the only sign
  anything extra happened; no bolt, no flash, nothing distinguishing it
  from a single hit that happened to deal more damage. Fixed by giving
  each extra strike its own bolt + impact-flash fx, reusing the primary
  strike's exact `from`/`to`/color (Lightning tier-1 is always a hitscan
  with real `msg.from`/`msg.to` — never routed through the null-from/to
  projectile-resolution path, so these are always valid here) and staggering
  them 110ms apart so multiple strikes read as distinct hits instead of
  landing simultaneously/invisibly. Broadcasts to other clients AND spawns
  locally (mirrors the existing pattern the primary hit's own impact fx
  already uses), so it shows correctly regardless of whether the caster is
  the host or a remote client. Verified via `window.__debug`: called
  `hostResolveCast` directly with a level-2 spell against a live enemy —
  damage landed immediately (26.88, matching 12×1.12×2, the expected
  2-hit level-2 total) and the `fx` array grew by 2 entries over the
  following 300ms as the staggered `setTimeout` callbacks fired on
  schedule.

- **Hub rebuilt into a full multi-room waystation** (per user request —
  "Хогвартс / библиотека / Круглый стол", "делай, пока не получится
  идеально"), replacing the single square room. A "+"-shaped complex: Main
  Hall in the middle, four spokes off it — Library (mentor stand, north),
  Fire room (west), Water room (east), Lightning room (south) — each
  linked to the hall by a short corridor with its own door-gap walls.
  ```
                [Library]
                   |
  [Fire] —— [Main Hall] —— [Water]
                   |
              [Lightning]
  ```
  - New generic room/corridor builders (`buildHubRoom`, `hubWallSeg`,
    `hubFloorCeil`, `buildHubCorridorNS/EW`) mirror `buildArena`'s own
    box-wall + `{minX,maxX,minZ,maxZ}` collision-record style, but support
    a door gap on ANY of a room's 4 sides (Main Hall needs all 4; each
    spoke needs exactly 1, facing back toward the hall) — `buildArena`
    itself only ever supported one fixed gap on the north wall, not
    reusable here.
  - Collision: hub has no enemies (see `buildLevelEnemies`), so this only
    ever needed PLAYER collision — the full wall list is stashed straight
    into `ZONE_MAZE['hub'].walls`, which the existing movement code
    already resolves against for any zone with a `ZONE_MAZE` entry, for
    free, no new collision plumbing needed. `ZONES.hub.half` bumped
    10→25 (a generous OUTER bound only — the real containment is the wall
    list; 25 comfortably covers the library's own farthest extent, 24).
  - Decor: new reusable prop builders — `makeBookshelf` (canvas-textured
    book spines, tiled per shelf), `makeCurtain` (vertical-striped cloth +
    a wooden rod), `makeRug` (bordered canvas pattern, lifted 0.02 off the
    floor to avoid the SAME z-fighting class of bug as the portal-flicker
    Finding earlier in this file — coplanar floor geometry), `makeArmchair`,
    `makeBanner` (canvas gradient + emoji emblem), `makeReadingTable` (+
    candle light). `furnishClassRoom(x,z,gapSide,emoji,colorHex,
    centerpieceFn)` is a shared template applied to all 3 class rooms —
    bookshelf + banner on the wall OPPOSITE the door, curtains on the two
    side walls, one signature centerpiece each: `makeBrazier` (Fire, reuses
    the shared torch flicker loop), `makeFountain` (Water), `makeStormOrb`
    (Lightning). Library gets its own hand-placed layout (denser: 3 walls
    of bookshelves, reading table + candle, an armchair) since it's the
    single biggest/most detailed room. Main Hall keeps the mirror (moved
    to its south wall) with 2 armchairs + a rug forming a rest nook, 2
    bookshelves flanking the library corridor's mouth, and the existing
    shop fixtures (mentor stand → moved into the Library; passive stands +
    continue portal → tucked into two Main Hall corners the corridors
    don't cross).
  - One warm ambient `PointLight` per room (`hubAmbient`) on top of each
    room's own fixture lights (candle, brazier, fountain glow, storm orb,
    shop-stand orbs) — a space this size needs more than point-fixtures
    alone for base visibility. All lights (~19 total across the complex)
    still register via the existing `registerZoneLight('hub', ...)` /
    `setZoneLightsVisible()` culling, unchanged.
  - Verified live and structurally via `window.__debug`, not just "no
    console errors": (1) wall count — 36 wall segments generated, and
    hand-counted the expected total from the room/gap math (Main Hall
    4-gapped-sides ×2 segments=8, each spoke 1-gapped+3-solid=5×4=20, 4
    corridors ×2 side walls=8 → 8+20+8=36) — exact match. (2) Collision —
    simulated walking straight into the Library's solid west wall: stopped
    at exactly `PLAYER_RADIUS` (0.55) short of the wall face, as expected;
    simulated walking the full corridor from the Library center straight
    through its south doorway into the Main Hall: reached deep past the
    Main Hall's own center with zero snags, confirming the doorway/
    corridor path is genuinely open end-to-end. (3) Visual — screenshots
    from inside all 5 rooms: Main Hall (mirror + armchairs + rug visible
    together, no z-fighting), Library (reading table + candle glow +
    mentor stand + an armchair, all correctly lit), Fire (brazier's flame
    cone + warm brick walls), Water (fountain's glowing basin + cool
    walls), Lightning (storm orb + warm-yellow walls) — each room reads
    visually distinct at a glance, matching the "антураж библиотеки"
    brief. Bookshelves/curtains/banners on the FIRE room's side/far walls
    specifically weren't confirmed in-frame (this sandboxed browser can't
    reliably rotate the camera to arbitrary yaw — see the session's
    recurring `requestAnimationFrame`-reliability caveat), so their exact
    on-screen look is code-review-verified only; everything else was seen
    directly. Worth a real playtest pass to judge overall room proportions/
    pacing (corridor length, room sizes) by feel, not just by the numbers.

- **6-part hub follow-up**: corner seams, mirror-blocking-a-doorway bug,
  gold frame, coziness pass, and — the big one — replacing dungeon-entry
  entirely with level-portal mirrors. Asked 3 clarifying questions first
  (mirror-break persistence, active-portal look, whether to keep the old
  "continue" ring as a fallback) since all three materially changed the
  implementation; answers: broken stays broken forever (levels are finite,
  no loop back to 1), active portal = stylized swirl (not a live
  reflection), old ring removed entirely.
  1. **"Вход в молнию почему-то через зеркало" — real cause, not a vague
     glitch.** The self-view mirror and the Main Hall's south-wall door
     gap (leading to the Lightning corridor) were BOTH centered at x=0 on
     that wall — the mirror was sitting directly in the doorway. Moved to
     its own alcove (`hMainX-5.5`) with the reading nook, clear of every
     door gap.
  2. **"Внутренние углы" — walls not meeting at room corners.** Root
     cause: each wall segment's box stopped exactly at the room's nominal
     corner instead of overlapping the perpendicular wall's own thickness
     band there, leaving a small unsealed notch — same *class* of bug as
     `buildArena`'s corner handling already avoids (via its `+2` oversized
     span), just not replicated in `buildHubRoom`'s from-scratch
     door-gap-aware version. Fixed by extending every solid wall segment
     (and gapped segments' OUTER/corner end only — the door-facing end
     must stay exactly at the gap boundary) by `EXT=0.5` past the nominal
     corner. Verified two ways: (a) wall count unchanged at 36 (this is a
     span-length change, not a segment-count change) — confirmed via a
     direct AABB-overlap check at the Main Hall's SE corner: the south
     wall's flanking segment (`x:[2,9.5]`) and the east wall's flanking
     segment (`z:[-239.5,-232]`) now share a real 1×1 overlap square at
     the corner (`cornerCovered` check returns `true` — the exact corner
     point is inside at least one wall's box). (b) Visual — screenshot
     along a Main Hall wall (with the `L` full-lighting debug cheat on)
     shows a continuous, unbroken brick surface; a right-up-at-the-exact-
     corner shot was inconclusive (near-clip range at that distance, not
     a geometry gap — confirmed by reading the live `playerPos`, which
     was still a valid in-room position).
  3. **Gold frame** (`makeGoldFrame`, replaces the old plain dark-purple
     bars): gilt `MeshStandardMaterial` (metalness 0.85), a thin inset
     trim line for the "кайма" detail beyond a plain outline, and 4 small
     gilt corner-ornament spheres. Shared by the self-view mirror AND
     every new level-portal/broken-mirror prop below, so the whole hub's
     mirror set reads as one consistent, expensive-looking family.
  4. **Coziness**: new `woodMaterial`/`makeWoodTexture` (canvas plank
     texture) replaces stone for every hub room's FLOOR (ceilings/walls
     stay stone — full wood-paneled walls read as a different building
     style entirely, floor-only keeps it "a stone hall with wood floors,"
     which is what was actually asked); a `makeFireplace` (stone surround,
     logs, live flame reusing the shared torch-flicker loop) on the Main
     Hall's west wall; 2 `makeTapestry` wall hangings (canvas
     diamond-lattice pattern, distinct from the small per-class emblem
     banners) on the east wall.
  5. **Level portal-mirrors** (`hubLevelMirrors`, replaces the old single
     "continue" ring — `makeContinuePortal`/`shopContinuePortal` deleted
     entirely, along with `#labelContinue`): one gold-framed mirror per
     dungeon level, standing in a row in the Main Hall. 3 visual states
     (`setMirrorLevelState`) — **dormant** (dark inert glass, level not
     reached yet), **active** (swirling canvas-texture glow, procedurally
     drawn — `makeSwirlTexture` — colored by that level's
     `ZONE_PORTAL_ELEMENT`, exactly one mirror is ever in this state:
     `highestClearedLevel+1`), **broken** (cracked-glass canvas texture —
     `makeCrackedTexture` — permanent once set, per the user's answer).
     `MAX_LEVEL` and `ZONE_PORTAL_ELEMENT` both had to move earlier in the
     file (were declared after this code used to run) — no behavior
     change, pure ordering fix for the new top-level `updateHubMirrors()`
     call. `highestClearedLevel` is host-authoritative, set in
     `startShopPhase()` right when a level clears, and rides along in the
     periodic `'state'` broadcast for clients (`onClientData` calls
     `updateHubMirrors()` when it changes). `closeShopAndAdvance()` now
     computes `next = highestClearedLevel+1` instead of the old
     `currentLevel>=MAX?1:current+1` wraparound, and no-ops once
     `next > MAX_LEVEL` — the run just ends, no loop back to level 1.
     Interacting with the active mirror reuses the exact same
     `requestReadyNext()`/`'continue'`-kind plumbing as the old portal,
     just retargeted.
  6. **Level-side broken mirror** (`makeBrokenMirrorProp`): a static,
     always-broken gold-framed cracked mirror at each dungeon level's own
     spawn point — reuses `makeGoldFrame`/`makeCrackedTexture` from the
     hub mirrors. Deliberately did NOT touch the existing `zonePortal`
     swirl-ring system that already renders at every zone's spawn (async
     FBX-loaded, generation-counter-guarded — real risk to break something
     working for a purely cosmetic ask); instead just stopped calling it
     for the hub specifically (redundant now that the hub has its own
     dedicated portal mirrors) and added this as an ADDITIONAL static prop
     for levels 1/2/3, sitting next to the existing swirl ring rather than
     replacing it. Flagging this as a scope simplification, not silently.
  - Verified live end-to-end via `window.__debug`, not just code review:
    simulated clearing level 1 → mirrors read `[broken, active, dormant]`;
    called `closeShopAndAdvance()` directly → `currentLevel`/`currentZone`
    correctly became 2; simulated clearing level 2 → `[broken, broken,
    active]`; advanced to and cleared level 3 (the boss) →
    `[broken, broken, broken]`, `highestClearedLevel=3`; called
    `closeShopAndAdvance()` a 4th time → correctly a no-op (`currentZone`
    stayed `'hub'`, `currentLevel` stayed `3`) — confirms the finite,
    non-looping ending works exactly as specified. The E-press trigger
    itself (`activeStandAction` set by `updateShopStands()`, which runs
    inside the per-frame `animate()` loop) could NOT be exercised this
    time — the sandbox's `requestAnimationFrame` was fully stalled during
    this test (same recurring limitation), so that specific wiring is
    code-review-verified only; every host-side mechanic it calls into was
    exercised directly and is confirmed correct.

- **4-part hub follow-up: corners parked as tech debt, mirrors flush to
  walls, portal-as-object removed entirely, real pentagram assets in the
  class rooms.**
  1. **Corner seam — not re-attempted, parked as tech debt** per explicit
     user instruction ("оставь как техдолг"). The `EXT` wall-extension
     from the previous round is still in place (mathematically verified —
     adjacent wall AABBs do overlap at every corner) but a clean up-close
     visual confirmation was never obtained (camera yaw isn't
     controllable in this sandbox), so it's logged as unresolved instead
     of claimed fixed. See the new "Known issues / tech debt" section
     near the top of this file.
  2. **Mirrors must always sit flush against a real wall** (per user
     request — the Main Hall's 3 level-portal mirrors were freestanding
     in open floor, which read wrong). Moved them onto the Main Hall's
     south wall (its east solid segment, x:2..9.5 — clear of the
     Lightning-corridor door gap and the self-view mirror's own alcove on
     the west segment), at x=3/5.5/8, all facing +Z (`rotY=0`) same as
     the self-view mirror on that wall. The level-side broken mirrors
     (added last round, freestanding near each spawn point) needed the
     same treatment but each level's spawn sits in different kinds of
     space — a built antechamber (level 1), a randomly-generated maze
     (level 2), and a big open boss arena (level 3) — so wrote a generic
     `nearestWallFlushSpot(walls, x, z, maxDist)` that finds the closest
     wall AABB to a point within range and returns a flush mount spot
     (position + facing) on whichever of its 4 faces the point is
     actually nearest to — works for any axis-aligned wall list without
     per-level-shape logic. Levels 1/2 already had queryable wall lists
     (`ZONE_DOOR_WALLS[1]`, `ZONE_MAZE[2].walls`) to search; level 3's
     plain-arena walls aren't tracked in any list at all (`buildArena`
     doesn't push to one, unlike `buildMaze`/`buildPortalRoom`), so that
     one is hand-placed against its known north wall instead of adding
     wall-tracking to `buildArena` just for this one decorative prop.
     Verified structurally via `window.__debug`: found all 7 gold-frame
     groups in the scene (1 self-view + 3 hub level-mirrors + 3 level-side
     broken mirrors — matches `28 gold bars / 4 bars-per-frame` exactly)
     and read their positions — level 2's broken mirror landed at
     `x=-3.09` (not the spawn's own `x=0`), confirming
     `nearestWallFlushSpot` actually found and snapped to a real nearby
     maze wall rather than just using a fixed offset. Screenshot from
     inside the Fire class room also directly confirmed the Main Hall's
     active level-2 mirror rendering flush against the south wall with
     its gold frame and swirling glow, and the `[E] Войти — уровень 2`
     prompt showing correctly.
  3. **Portal-as-object removed entirely — mirrors are now the only
     transition visual anywhere** (per user request — "убери портал через
     объект, оставь только зеркала"). The old per-zone `zonePortal` swirl
     ring (`refreshPortal`/the block in `respawnLocalPlayer` that called
     it) is gone from `respawnLocalPlayer` completely — previously it was
     only skipped for the hub (last round's change); now it never fires
     for the dungeon levels either, since each already has its own static
     broken-mirror prop serving that "you arrived here" role. Removed the
     now-dead `zonePortal = updatePortal(...)` call from `animate()` too
     (the `zonePortal` variable itself no longer exists). The
     `ensurePortalTemplate`/`instantiatePortal`/`updatePortal` loader
     functions were kept, NOT deleted — they're what makes finding #4
     possible (same FBX asset, different use).
  4. **Real pentagram 3D assets in the elemental class rooms**
     (`makeElementPentagram`, per user request — reuse the actual
     `portal_lightning.fbx`/`.png` files instead of only the procedural
     brazier/fountain/storm-orb centerpieces). Calls the existing
     `ensurePortalTemplate`/`instantiatePortal` pair directly — placement
     had to happen after their definitions in file-load order since they
     read `const`s (`portalTemplates` etc.) that would otherwise still be
     in their temporal dead zone at hub-build time; the hub room-center
     coordinates (`hFireX`/`hFireZ`/...) are already `const`s in scope by
     then, so the pentagram calls sit right after `updatePortal`'s
     definition instead. `instantiatePortal` always applies `PORTAL_SCALE`
     (1.2 — an ~8m-across dungeon-scale portal per its own comment), which
     would swallow an entire 10×10 class room whole — overridden back
     down to `HUB_PENTAGRAM_SCALE=0.35` (~2.3m) on the returned instance
     instead of touching the dungeon portal's own sizing. All 3 rooms
     explicitly request the `'Lightning'` element (same fallback the old
     per-zone portals already used) rather than each room's own element —
     only `portal_lightning.fbx/.png` exist on disk; asking for
     `'Fire'`/`'Water'` was tried first and produced exactly the 4
     predictable 404s you'd expect (2 files × 2 missing elements),
     confirmed by reading the network log on a fully fresh tab (a
     mid-session reload had briefly shown the SAME 404s from a stale
     accumulated log before the fix — re-verified clean on a genuinely
     new tab to be sure it wasn't a real ongoing issue). No despawn-on-
     distance logic (unlike the old per-player portal) — these are
     permanent room fixtures; kept spinning via a small `hubPentagrams`
     array ticked every frame in `animate()` (`p.mixer.update(dt)`,
     next to the existing `updateTorches` call). Verified live:
     screenshot from inside the Fire room shows the pentagram's glowing
     rune-circle floor decoration and the brazier both rendering
     together at a sane relative scale, no clipping.

- **3 new dungeon levels (4, 5, 6) — harder mazes, tougher enemies, a
  second boss.** `MAX_LEVEL` 3→6. Levels 3 and 6 are both boss floors now
  (`BOSS_LEVELS = [3, 6]`, replacing every hardcoded `level === 3` check in
  `computeSpawnsForLevel`/`buildLevelEnemies`/the shop-phase log message);
  everything else (1/2/4/5) is a regular enemy floor.
  - **Placement**: all 3 new zones sit well south of the hub complex
    (which itself spans z:[-252,-206] — see `buildHubRoom`), not just
    south of level 3, to avoid overlapping it in world space — every zone
    coexists statically in the one scene, and every zone transition is a
    teleport (mirrors), never a physical walk between zones, so world-
    space placement only ever matters for avoiding geometry collisions,
    not path layout. cz: level 4 -326, level 5 -440, level 6 -536 —
    30-40+ unit gaps to every neighbor.
  - **Levels 4/5 (mazes)**: same `buildMaze` used by level 2, just bigger
    grids and (slightly) tighter cells — level 4: 9×9 @ 7.5 units/cell
    (100 wall segments, 6.8-unit corridors); level 5: 11×11 @ 7 units/cell
    (144 wall segments, 6.3-unit corridors) — vs level 2's 7×7 @ 8 (64
    segments, 7.3-unit corridors). More cells means longer/more complex
    paths; corridors stay comfortably walkable rather than punishing
    (level 2 already proved 7.3 felt right, so 6.8/6.3 are only modestly
    tighter). `ZONES[4/5].half` set to exactly `cols*cellSize/2` (33.75 /
    38.5) so the outer wall lines up with the maze extent, same
    convention level 2 already used. `computeSpawnsForLevel`'s maze
    branch generalized from `level === 2` to `ZONE_MAZE[level]` (works for
    any maze zone now) and its enemy count formula changed from a flat
    9-14 to `9 + level + 0..3` (11-13 for level 2 same as before, up to
    16-19 for level 5) — more enemies on top of the existing
    `ENEMY_MAX_HP + (level-1)*35` HP scaling (already generic, no change
    needed) for "stronger enemies," not just "more of the same."
  - **Level 6 — BOSS2, a genuinely different fight** (per user request:
    "снова один босс, но сильнее и с другими способностями"). Bigger HP
    pool (1700 vs level 3's 900) and an entirely different kit — no
    melee slam/charge at all:
    - **Ranged bolt**: telegraphed (`BOSS2_BOLT_TELEGRAPH_S=0.6s` between
      cast and impact — a real reaction window, same idea as
      `MELEE_IMPACT_FRAC`), re-checks the target's range at impact (not
      cast time) before applying `BOSS2_BOLT_DAMAGE=20`, up to
      `BOSS2_BOLT_RANGE=16` away. Reuses the existing `spawnLightningBolt`
      fx (tinted magenta) rather than inventing new visuals.
    - **Ground spikes**: drops `BOSS2_SPIKE_COUNT=2` danger zones under
      random players (`±2` unit jitter) with a `BOSS2_SPIKE_WARNING_S=1.3s`
      telegraph (a new red ring fx, `spawnGroundSpikeWarning` — a static
      `fx`-array entry, no growth needed, its existing decay curve already
      reads as "steady, then flashes out right before it pops"), then an
      AoE burst (`spawnGroundSpikeBurst`, just `spawnNovaFx` re-tinted red
      — no need for a whole new effect) dealing `BOSS2_SPIKE_DAMAGE=26` to
      anyone still standing in it.
    - **Kiting movement**: tries to hold `BOSS2_KITE_RANGE=9` from its
      target — backs away if closer, closes in if farther — instead of
      level 3's boss charging straight at players. Never melees.
    - Implementation: `makeEnemy` gained a `bossKind` parameter (`null`
      for level 3's original boss, `'ranged'` for BOSS2); the
      `hostSimulate` boss branch now computes `target`/`alive` once up
      front, then splits into the BOSS2 branch (own `boltCooldown`/
      `spikeCooldown`/`pendingBolt`/`pendingSpikes` state, all
      initialized in `makeEnemy`'s return object) or falls through to the
      original slam/charge branch unchanged. `BOSS_SCALE`/`BOSS2_SCALE`
      picked in `makeEnemy` based on `bossKind`.
  - **Hub integration**: `hubLevelMirrors` now holds 6 mirrors, not 3 —
    levels 1-3 stayed on the Main Hall's south wall (unchanged), levels
    4-6 added to the east wall's south solid segment (z:-9.5..-2, clear of
    the Water-corridor door gap) — which required moving the 2 tapestries
    that used to live there into the Library corridor's side walls
    instead (still flush, still decorative, just relocated). Levels 4/5
    each got a `placeBrokenMirrorNear`-flush level-side broken mirror
    (found real nearby maze walls, same as level 2's); level 6 got a
    hand-placed one against its own north wall, same reasoning as level
    3's (its plain-arena walls aren't tracked in any queryable list).
  - Verified live via `window.__debug`, structurally AND behaviorally:
    (1) stepped through all 6 levels with `startLevel()`, confirming
    `enemies.length`/HP/`bossKind`/maze-wall-count for each — levels 1-6
    read exactly as designed (5/11/1-boss-900hp/15/16/1-boss2-1700hp-
    ranged). (2) Ran a full clear-all-6-levels loop via
    `startShopPhase()`/`closeShopAndAdvance()` and read `hubLevelMirrors`
    after each — states progressed
    `[active,dormant,dormant,dormant,dormant,dormant]` →
    `[broken,active,dormant,...]` → ... →
    `[broken,broken,broken,broken,broken,active]` →
    `[broken,broken,broken,broken,broken,broken]`, and a 7th
    `closeShopAndAdvance()` call after that correctly no-op'd (still
    `currentZone:'hub'`, `currentLevel:6`) — the extended 6-level
    progression works exactly like the earlier 3-level version did.
    (3) Stepped `hostSimulate` directly against BOSS2 with a player in
    range: confirmed a bolt cast resolves for exactly 20 damage and a
    2-spike volley for exactly 52 (26×2) — matches the constants exactly,
    not just "some damage happened." (4) Screenshots: BOSS2 itself
    (bigger, red-eyed, full HP bar) with a leftover ground-spike warning
    ring visible on the floor from the damage test; 2 broken mirrors
    (cracked glass, gold frame) flush on the Main Hall's east wall after
    the full clear-run. No console errors throughout.

- **Dormant (not-yet-reached) hub mirrors now reflect players for real,
  instead of sitting dark** (per user request — "целый и отражают
  игроков"). Extracted the self-view mirror's Reflector-plus-"show my own
  body just for this reflection" trick into a standalone
  `makeReflectiveGlass(x,y,z,rotY,w,h)` (the original `makeMirror` is now
  just that call plus `makeGoldFrame`, unchanged behavior). Every
  `makeLevelMirror` now builds BOTH a live `m.reflective` (this) and the
  existing flat `m.glass` (swirl/cracked texture) at the same spot;
  `setMirrorLevelState` toggles `.visible` on whichever one the state
  calls for — `'dormant'` shows the reflection, `'active'`/`'broken'`
  show the textured flat plane, only ever one of the two at a time. Added
  a `!mirror.visible` early-return inside the Reflector's own
  `onBeforeRender` — belt-and-suspenders against paying for a full extra
  scene re-render on a hidden mirror; in practice this is already how
  three.js's own scene traversal works (an invisible object's
  `onBeforeRender` is never even called), so it's a defensive no-op
  rather than a fix for a measured cost, but early game can have up to 5
  dormant mirrors on screen at once and it costs nothing to be sure.
  Verified live: screenshot standing at level 3's mirror (dormant) shows
  a real mirrored reflection of the player's own Fire mage model, gold
  frame intact; re-checked level 2's mirror (active, just cleared level 1
  in the same test) still shows the swirl + correct `[E] Войти — уровень
  2` prompt, confirming no regression to the other two states.

- **Reverted the above — live-reflection dormant mirrors caused severe
  game-wide lag** (user report: "игра теперь сильно лагает"). Root cause:
  a `Reflector` re-renders the *entire scene* from a virtual camera every
  frame it's visible, and early-game has up to 5 dormant mirrors on
  screen simultaneously — 5 extra full scene passes per frame, not a
  theoretical cost. The earlier `!mirror.visible` early-return only
  guarded the ones currently hidden; it did nothing for the 4-5 that are
  dormant *and on screen* at once, which is the common case. Fix (per
  user: "замени... на такой же вариант, как доступный активный, но
  серого цвета"): removed `m.reflective`/`makeReflectiveGlass` entirely
  from `makeLevelMirror` — level mirrors no longer instantiate a
  Reflector at all, only the self-view mirror in the Main Hall still has
  one (1 total in the scene now, down from up to 6). `setMirrorLevelState`'s
  `'dormant'` branch now calls the same `makeSwirlTexture()` the
  `'active'` branch uses, tinted grey (`0x888890`) with a dim 0.25
  emissive (vs. active's 0.55) and no point-light glow — reads as "a real,
  intact portal, just not the live one" at zero extra per-frame cost
  (same static canvas-texture plane the other two states already use).
  Verified via `window.__debug`: confirmed exactly 1 `Reflector` remains
  in the whole scene (traversed and counted by constructor name); read
  every `hubLevelMirror`'s material state directly — dormant mirrors have
  no `.reflective` property at all, `glass.visible` always true,
  `map` present (grey swirl), color `#ffffff`, emissive `#888890` @ 0.25,
  light off; active mirror unchanged (yellow swirl, 0.55 emissive, light
  on). Round-tripped `highestClearedLevel` through `updateHubMirrors()`
  (0 → 2 → 0) and confirmed states relabel correctly each time
  (`dormant`→`broken`/`active`/`dormant` as expected) with the new
  material logic. `makeReflectiveGlass`/`makeMirror` themselves are
  untouched — still used once, for the self-view mirror, which was never
  reported as laggy.

- **Main menu redesigned per user-supplied reference concept art** (a
  dungeon hall with 3 glowing elemental portal arches, left-aligned title
  "COVENANT OF MAGES" flanked by icons, pill-style menu buttons, bottom
  tagline). No matching background image asset exists on disk (checked
  `textures/`/`models/` — only particle sprites and the lightning portal
  FBX), so the whole scene is built from CSS gradients/shapes instead —
  same "procedurally generated" spirit as `makeSwirlTexture` and the
  skill-tree paper background elsewhere in this file, not an image. New
  `#menuScene`/`.portalArch` elements render 3 arch shapes tinted per
  element (fire/water/lightning) with a pulsing inner border and radial
  glow; `#menuLeftColumn` holds the restyled title block, the existing
  functional `#panelMain` (all original IDs/behavior unchanged — host
  name input, host/join buttons, join-code row, error text, hint text),
  and the tagline. `panelLobby`/`panelWaitingClient` keep their original
  centered card style (`.panelCentered`), now floating over the same
  backdrop. Hit one real bug while building this: `.menuPanel`'s
  background/border overrides were losing to the later `.panel` class
  rule at equal CSS specificity (source-order tiebreak) — fixed by
  scoping the override to `#panelMain.menuPanel` (ID beats class).
  Verified live: screenshot confirms the portal arches, title, and pill
  buttons render correctly and match the reference's composition; clicked
  "Создать игру (хост)" and confirmed it still correctly transitions to
  the asset-loading screen (the loading itself stalling at 0% here is the
  known sandboxed-preview relative-path limitation, unrelated to this
  change — `models/*.fbx` can't resolve from a `file://` path opened
  outside the project root in this tool).

- **Menu buttons swapped to the final 5-item set, decorative text removed,
  background swapped for a drop-in placeholder** (follow-up to the above,
  per user request). `btnHost`/`btnShowJoin` relabeled "Играть"/
  "Присоединиться" (same IDs, same click handlers — host/join flow
  unchanged); added `btnSettings`/`btnCredits`/`btnExit`, all `disabled`
  with a `title="Недоступно в прототипе"` tooltip rather than silently
  doing nothing on click — there's no settings/credits/exit system in this
  prototype to wire them to, and a disabled-with-explanation button is
  more honest than a dead click. Removed the "web MVP prototype — co-op
  spell synergy" subtitle and the "Хост создаёт комнату…" instructional
  paragraph entirely, per "убери все остальные комментарии" — only the
  title, functional form, and the existing error text remain.
  Replaced the CSS-drawn portal-arch backdrop with a single `#menuScene`
  layer: `background: center/cover url('menu-bg.jpg'), <dark gradient>`.
  No image exists yet (expected — user said they'll supply one later);
  until then the gradient layer shows through on its own, since a failed
  background-image layer just doesn't paint rather than breaking the
  other layers. **To finish this: drop the final art in as
  `prototypes/web-mvp-concept/menu-bg.jpg`** (same folder as
  `prototype.html`) — no code change needed, it'll pick it up
  automatically. Verified live: screenshot confirms exactly the 5
  requested buttons in order, no leftover subtitle/hint text, and the
  gradient placeholder rendering cleanly where the portal arches used to
  be.

- **HUD gained a minimap, quest/objective box, and a teammates-only party
  panel**, per a second reference screenshot — explicitly scoped to just
  those three additions ("остальное — не трогаем"); `#status`,
  `#combatlog`, `#cheatHint`, the spell wheel, HP/XP bars, and everything
  else are untouched. Party frames (`#partyFrames`, top-left, stacked
  below `#status` so the two never overlap) show only OTHER players —
  the local player's own HP stays exactly where it already was, bottom
  left. Built from real, already-networked data, nothing fabricated: name/
  element/HP/maxHp/downed already travel in the host's periodic `'state'`
  broadcast (`buildEntry` in `hostSimulate`) and were already read by
  clients for other purposes — just hadn't been surfaced in the HUD
  before. Added `hp`/`maxHp`/`spellCount` fields to the `remotePlayers[pid]`
  entries `makeRemotePlayer` creates, populated from `pdata.hp`/
  `pdata.maxHp`/`pdata.spells.length` in `onClientData`'s `'state'`
  handler. `renderPartyFrames()` branches on `isHost`: host reads directly
  from its own authoritative `playerData`/`peerMeta`/`connections`, a
  client reads from its (now-extended) `remotePlayers`. Each frame shows
  the player's element icon (colored ring border), a small badge with
  their unlocked-spell count, name, and an HP bar tinted via the same
  `hpFracColor()` the local HP bar already uses. Full innerHTML rebuild
  every call is safe here (unlike the old shop-stand DOM, which broke
  real clicks when rebuilt every frame — see the playtest-bugs entry
  above) since nothing inside a party frame is interactive.
  The minimap (`#minimapWrap`, top-right) started as a compass-only
  simplification — fixed N/E/S/W labels and an arrow rotating to the
  local player's `yaw` — with no top-down geometry projection, since
  wiring one up felt like more scope than "add a minimap" implied. The
  user flagged this immediately as not being a real minimap; see the
  follow-up entry below for the actual top-down projection that replaced
  this gap the same session. The quest box (`#questBox`, below the minimap) derives
  its text from existing `currentZone`/`currentLevel`/`shopPhase` state via
  a small `questInfoFor()` helper (hub / maze-level / boss-level → 3
  message pairs) — no new quest-tracking system, since the game doesn't
  have quests beyond "clear this level."
  Verified live via `window.__debug`: called `enterGame()` to show the
  HUD without needing FBX assets to finish loading (they can't resolve
  from this sandbox's `file://` path, as already documented), created two
  fake `remotePlayers` entries via the exposed `makeRemotePlayer()` with
  different HP fractions and one `downed:true`, and confirmed via
  `requestAnimationFrame` actually ticking this time that `renderPartyFrames()`
  ran organically and produced correct HTML (right icon/color/name/HP-bar-
  width/HP-bar-color per player, downed one visibly dimmed via
  `.pfDowned`) and that `updateMinimapAndQuest()` set a real
  `rotate(...rad)` transform and correct quest text. Screenshot confirms
  the full HUD composition matches the reference layout with the rest of
  the HUD (status text, HP bar, crosshair, hint text) visibly unchanged.

- **3 follow-up tweaks, all per direct user feedback.** (1) `btnHost`
  relabeled "Играть" → "Создать" (still the same button/handler, host
  flow unchanged). (2) The minimap gained an actual top-down projection
  instead of being compass-only, per "в миникарте нет реальной
  миникарты" — added `#minimapSvg` (an SVG layer inside `#minimapRing`,
  clipped to the circle) and `renderMinimapGeometry()`, called every
  frame from `updateMinimapAndQuest()`. It's a real, player-centered
  projection built from data the game already has for collision, not
  anything fabricated: every zone's outer walls live in
  `ZONE_DOOR_WALLS[zoneId]` (`buildArena()` fills it for all 6 dungeon
  zones), maze zones (2/4/5) additionally contribute
  `ZONE_MAZE[zoneId].walls`, the hub uses its own flat `hubWalls` list,
  and pillars come from `ZONE_PILLARS[zoneId]`. Enemies within
  `MINIMAP_RADIUS` (16 world units) are drawn too, as red dots, from the
  same `enemies` array the game loop already iterates — a small bonus
  since the data was right there. Everything within radius is mapped
  linearly onto the 100×100 SVG viewBox centered on the player
  (`MINIMAP_SCALE = 46/16`); the map itself stays world-axis-fixed (N
  always up, doesn't spin) — only the arrow rotates, exactly as before.
  Verified live: placed the player at zone 1's center via
  `window.__debug.playerPos.set(...)`, waited one real animation frame,
  and read `#minimapSvg`'s actual rendered markup — got exactly 4
  `<circle>` pillar markers at `(27,27)/(73,27)/(27,73)/(73,73)`, which
  is precisely `50 ± 8×(46/16)` for zone 1's real pillar layout
  (`[[-8,-8],[8,-8],[-8,8],[8,8]]` passed to `buildArena(1,...)`) — the
  coordinate math checks out exactly, not just "something rendered."
  (3) `questInfoFor()`'s objective text is now the same
  "Победить противников" for every dungeon level regardless of
  maze/boss, per "пока оставим на всех уровнях задание Победить
  противников" — the title line (level number + Лабиринт/Бой с боссом)
  still differs, only the objective line was flattened; hub's objective
  text is untouched (still context-specific, since it was never part of
  this ask).

- **Fixed "lags hard for the first 5-7 seconds right after the lobby"**
  (user report). Root cause, found by re-reading `animate()`'s render
  loop: `renderer.render(scene, camera)` runs unconditionally every
  frame, has done so since page load (lobby included) — so any object
  already sitting in the real scene by lobby time has long since paid its
  one-time WebGL shader-compile cost (same "~2.3s per distinct model/
  material, measured via PerformanceObserver longtask entries" this
  codebase already discovered once, see `warmupAllElementPreviews`'s
  comment). Two things were NOT already sitting in the scene by lobby
  time, both first appearing at the exact moment gameplay actually
  starts: (1) `ensureDebugOwnBody()` — builds the local player's own real
  mage-element model (used for the hub self-view/level mirrors) — was
  only ever called from inside `animate()`'s `if (inGame)` block, i.e.
  first call = first frame of real gameplay. (2) `buildLevelEnemies()`
  spawns ~9-14 brand-new skinned goblin meshes all at once at level
  start. Stacked together, that's 2+ distinct first-time shader compiles
  landing in the first few real frames after "Начать" — matches "5-7
  seconds" as a plausible sum, not just one hitch.
  Fix follows the exact pattern this codebase already proved works for
  the identical class of bug (see `warmupAllElementPreviews`'s comment:
  a throwaway/separate-context compile attempt demonstrably did NOT
  reliably force compilation — only a real render through the actual
  production renderer/scene did): (1) `ensureDebugOwnBody()` is now also
  called the moment `myElement` is actually set (in the class-picker's
  card click handler), well before the lobby even starts, so it rides
  the render loop's continuous background rendering during lobby/chat
  wait time instead of at game start. (2) `loadGoblinAssets()` now
  spawns one real goblin instance via `makeGoblinModel()` into the actual
  game scene (parked at y=-200, permanently — camera-frustum-culled and
  cheap once placed, no need to ever remove it) the moment
  `goblinTemplate` finishes loading, which happens well before any level
  can start.
  Could not reproduce/measure the actual lag or its fix in this sandbox
  (FBX assets can't load here at all — the long-disclosed `file://`
  relative-path limitation, so `mageTemplate`/`goblinTemplate` never
  become truthy and neither warmup path ever fires in this environment).
  Verified what IS checkable here: the file still parses and runs with
  no new console errors beyond the pre-existing 2 known asset-load
  failures; `window.__debug.ensureDebugOwnBody()` called directly
  no-ops safely (as expected, its own `!mageTemplate` guard) rather than
  throwing, confirming the moved call site doesn't break anything when
  assets aren't ready. The actual before/after timing needs a real
  browser with the model files reachable — flagging this rather than
  claiming a live-verified fix.

- **3 more follow-ups from a fresh playtest, all fixed.**

  (1) "На миникарте не отображаются внешние стены 1 уровня" — real bug,
  not a radius/zone-specific quirk. Re-reading `buildArena()` (used by
  every non-hub zone, maze levels included) found that its 4 outer walls
  are meshed straight into `scene` with no AABB ever recorded anywhere —
  unlike every other wall system in the file (`buildMaze`, `hubWallSeg`,
  `buildPortalRoom`), which all push `{minX,maxX,minZ,maxZ}` into a
  queryable array. The player/enemies are kept inside these walls via a
  cheap `Math.max/min` clamp against `ZONES[zone].half` instead (see
  `clampEnemyToArena`) — real collision, just never exposed as data. The
  minimap literally had nothing to draw them from. Added
  `arenaBoundaryWalls(zone)`, synthesizing 4 thin boundary AABBs from the
  exact same `half - 1` bound the real clamp uses (so the drawn boundary
  matches actual collision, not just a decorative guess), included in
  `renderMinimapGeometry()`'s wall list for every non-hub zone. Verified
  live via `window.__debug`: placed the player at zone 1's center and
  read `#minimapSvg`'s rendered markup — got exactly 4 new `<rect>`s at
  `y≈-2.6`/`y≈100.9`/`x≈-2.6`/`x≈100.9` spanning ~105 svg units each,
  which is precisely `50 ± 18×(46/16)` (`half=19` so `b=18`) framing the
  full boundary square, plus the pre-existing portal-room door wall and
  4 pillar dots still rendering correctly alongside it — no regression.

  (2) "Замени маркер игрока на что-то более понятное, чем равносторонний
  треугольник" — the old marker was a single `▲` glyph in a separate
  `#minimapArrow` div, CSS-rotated. Replaced with a proper dot+dart
  marker drawn directly inside `#minimapSvg` alongside the wall geometry
  (removed `#minimapArrow` and its CSS entirely): a small filled circle
  marks the exact position regardless of rotation, plus an elongated
  kite/dart polygon (not equilateral — clearly asymmetric front/back)
  pointing the facing direction, both inside one `<g transform="rotate(deg
  50 50)">` recomputed from `yaw` every frame (same `-yaw` convention the
  old CSS transform used, just converted rad→deg for SVG). Verified the
  rendered markup live — correct polygon/circle at `rotate(0.0 50 50)`
  with `yaw=0`; couldn't exercise a non-zero rotation live since
  `window.__debug.yaw` turned out to be a plain captured number (a
  primitive snapshot at the time `__debug` was built, not a live
  getter/setter like `highestClearedLevel`), so writing to it doesn't
  reach the real module-scope `yaw` the render function reads — the
  trig itself (`-yaw * 180/Math.PI`) is a straight reuse of the already-
  working CSS formula, just re-expressed for SVG's `rotate()` syntax, so
  low-risk, but noting the gap honestly rather than claiming a live
  rotation check that didn't actually happen.

  (3) "Добавь отображение над противников статуса (мокрый, горящий и
  тп)" — added `makeEnemyStatusSprite()`/`updateEnemyStatusIcons()`: a
  small billboard `THREE.Sprite` (auto-faces camera, same trick as
  `makeNameSprite`) sitting just above each enemy's existing HP bar,
  showing ❄️/🔥/💧 for `frozenT`/`burning`/`wet` — fields the game
  already tracks and networks (already driving the existing body-color
  tint in `updateEnemyVisual`; this is a second, more explicit read of
  the same real data, nothing fabricated). Hooked into the single
  `updateEnemyVisual(e)` call already invoked from every place these
  fields change on both host (`hostApplyDamage`, the per-tick decay in
  `hostSimulate`) and client (`onClientData`'s `'state'` handler) — no
  new call sites needed anywhere. The canvas backing the sprite's texture
  is only actually redrawn when the active-icon combination changes
  (tracked via a `lastKey` string), not every frame — a per-enemy canvas
  rebuild 60x/sec for every enemy on screen would be exactly the kind of
  needless per-frame cost the lobby-lag fix above just got rid of.
  Verified live via `window.__debug`: spawned real enemies
  (`buildLevelEnemies(1)`), set `e.wet = 3` then called the already-
  exposed `hostApplyDamage(e, 0, null)` (which always ends with
  `updateEnemyVisual(e)`) and confirmed the sprite's `lastKey`/`visible`
  flipped to `'W'`/`true`; added `e.burning = 2` on top and confirmed it
  became `'BW'` (both icons); cleared both and confirmed it dropped back
  to `''`/`false`. Screenshot also shows the new minimap wall + red enemy
  dots rendering correctly together with the new dot+dart player marker.

- **Fog of war on the minimap** (per user request — hide unexplored
  areas and enemies the player hasn't been near yet). Purely client-side,
  per-LOCAL-player, never networked — each player remembers their own
  exploration, not shared party-wide (nothing asked for that, and
  syncing it would need a new field for no real gameplay benefit). A
  `FOG_CELL_SIZE=2`-unit grid per zone; `revealFogAroundPlayer()` (called
  every frame from `renderMinimapGeometry()`, before drawing) adds every
  cell within `FOG_REVEAL_RADIUS=10` of the player's *current* position
  into a permanent per-zone `Set` (`exploredCells[zoneId]`) — deliberately
  smaller than `MINIMAP_RADIUS=16` so seeing the map's full view radius
  takes actually walking around, not just glancing. Once a cell is
  explored it stays explored for the rest of the session (never cleared)
  — the hub especially is revisited constantly, and losing its map each
  time would be annoying, not "more correct." Walls, pillars, and enemy
  dots are now all additionally gated by `isFogExplored(x,z)` alongside
  the existing distance check.
  Explicitly scoped as "have I been physically near this" memory, NOT
  true line-of-sight — no raycasting against walls, so standing right
  next to a wall reveals a sliver of whatever's on the other side too.
  Called out here rather than silently passed off as full line-of-sight,
  since that's a materially bigger feature (wall raycasting or a flood-
  fill) this ask didn't call for.
  Verified live via `window.__debug`: spawned level 1's enemies, set the
  player at the arena center (0,0) — zone 1's outer walls (~18-19 units
  away) and all 4 pillars (~11.3 units away, just outside the 10-unit
  reveal radius) correctly did NOT render, while 2 enemies that happened
  to spawn within 10 units did. Moved the player to (-8,-8), right next
  to one pillar — that pillar's circle appeared (1 pillar now drawn).
  Moved back to (0,0) and confirmed that pillar's circle was STILL there
  — exploration memory persists after walking away, not just "currently
  in range," exactly as intended.

- **Two real follow-up bugs from a fresh playtest, both fixed.**

  (1) "Тумана войны нет на миникарте" — the fog *logic* from the entry
  above was actually working (walls/pillars/enemies were genuinely
  gated by `isFogExplored`), but there was nothing on screen that
  visually read as "fog": an unexplored cell and an explored-but-
  currently-empty cell both just looked like the same plain dark compass
  background, so hiding things didn't register as "fog," just as
  "nothing there." Added `#minimapFog`, a `<canvas>` layered above
  `#minimapSvg` (below the N/E/S/W labels) — `renderMinimapFog()` paints
  a dark `rgba(4,4,10,0.85)` fill over every in-view cell that ISN'T in
  `exploredCells`, leaving explored cells transparent so the geometry
  underneath shows through normally. Canvas chosen over more SVG `<rect>`
  elements on purpose: up to ~300 cells can fall in view at once, and
  that many fresh SVG DOM nodes every frame is exactly the kind of
  per-frame cost this session's lobby-lag fix already had to eliminate
  once — a canvas fillRect loop of the same size is comparatively free.
  Verified live: sampled actual pixel data from the canvas via
  `getImageData` — a point ~12 world units out (beyond the 10-unit
  reveal radius, still within the 16-unit view radius) came back
  `rgba(3,3,10,237)` (the fog fill, alpha-blended), a point ~1.7 units
  from the player came back fully transparent `(0,0,0,0)` — exactly the
  expected split. Screenshot also shows the darker unexplored ring
  visible at the compass's outer edge.

  (2) "Стены изначальной комнаты на 1 уровне нет коллизии — можно
  пройти насквозь" — a real, confirmed bug, and a genuinely tricky one
  (2 attempts to actually fix, both verified live rather than assumed).
  Root cause: the portal antechamber's Z-clamp widening (`zMax`, added
  long before this session to let the player reach that room past the
  arena's normal `z.half` boundary) applied uniformly across ALL X
  positions, not just near the actual door gap — so walking north
  ANYWHERE along that wall, not just through the door, let the player
  pass clean through the *solid* flanking wall segments on either side
  of the door. Confirmed live before fixing: walked from world (15,10)
  to (15,33) in a straight line, right through real stone (the door
  gap's own X range is only ±1.6, nowhere near x=15).
  First fix attempt gated the widened `zMax` on the player being aligned
  with the door's real X range (`PORTAL_DOOR_WIDTH`) OR already having
  gotten past the wall in a previous frame (so someone genuinely inside
  the room can still drift sideways, constrained instead by the room's
  own already-working side-wall collision). Re-testing that fix found it
  didn't actually work — same (15,10)→(15,33) tunnel still happened.
  Cause: the "already past" check used `playerPos.z > z.half - 0.01`,
  but the CLAMPED RESTING position when correctly blocked at the wall is
  exactly `z.half` — which already satisfies `> z.half - 0.01` on the
  very next frame after hitting the wall, silently re-opening the tunnel
  one frame after any blocked contact. Fixed by requiring the player be
  meaningfully past the wall's own thickness (`z.half + 1`, not
  `z.half - 0.01`) before "already inside" ever applies.
  Verified live end-to-end this time: (a) walking at x=15 (away from the
  door) now holds firmly at z=19 even after 3 full seconds of continuous
  forward input; (b) walking at x=0 with the door still closed correctly
  stops at the closed door itself (~18.95, matching the door slab's own
  collision, independent of this fix); (c) opened the door via the
  already-exposed `openZoneDoor(1)` debug hook and confirmed walking
  through it at x=0 still reaches the room's far wall normally (31.95);
  (d) re-tested x=15 with the door now OPEN and confirmed it's still
  correctly blocked at z=19 — door state never affects the flanking
  wall, exactly as it shouldn't.
  Added several previously-missing debug exports while chasing this
  (`ZONE_DOOR_WALLS`, `ZONE_PORTAL_ROOM`, `ZONE_DOORS`,
  `resolveCircleWallCollision`, `startLevel`, `resetZoneDoor`,
  `openZoneDoor`, a real live `yawLive` getter/setter — the plain `yaw`
  export turned out to be a frozen snapshot value, not a live binding,
  discovered while trying to test minimap rotation a few entries back —
  and `keys`) — these made this investigation possible without real FBX
  assets (collision is pure position math, doesn't need the models to
  test) and stay useful for whatever gets debugged next.

- **Same wall, actual root cause this time** — user report: "могу
  пройти через стену в одну сторону (обратно зайти уже не получается)".
  The previous entry's fix only ever addressed the ENTERING direction:
  it's a plain `Math.min(zMax, playerPos.z)` clamp, which is a single
  directional bound, not a real wall — it has no way to stop a
  DEcreasing z at that same boundary. So a player who got into the
  portal room (through the real door, legitimately) could walk straight
  back OUT through the solid flanking wall going the other way, at any
  X off the door — nothing there ever pushed them back. Once outside
  again at that off-door X, trying to re-enter correctly failed (that
  direction was fixed), which is exactly the reported "one-way" wall.
  The actual, permanent fix: stopped trying to fake wall behavior with
  clamp math entirely and gave the flanking wall segments real AABB
  collision, same as literally every other wall in the file.
  `buildArena()` now pushes each flanking segment's `{minX,maxX,minZ,
  maxZ}` into `ZONE_DOOR_WALLS[zoneId]` (creating the array if it
  doesn't exist yet) right where it builds their meshes. `buildPortalRoom
  ()` — which runs right after `buildArena()` for zone 1 and used to
  `ZONE_DOOR_WALLS[zoneId] = walls` (a flat overwrite that would have
  silently wiped buildArena's contribution) — now appends instead
  (`ZONE_DOOR_WALLS[zoneId] = (ZONE_DOOR_WALLS[zoneId] || []).concat
  (walls)`). With real collision now doing the actual blocking, the
  movement clamp's `zMax` widening reverted to unconditional (same as
  the very original pre-session code) — the X-gating logic from the
  last entry is gone entirely, because it's no longer needed AND was the
  source of the one-way leak.
  Verified live, this time explicitly including the direction that broke
  last time: (a) `resolveCircleWallCollision` called directly against
  the wall data from both sides at x=6 (off the door) — pushed back
  correctly in BOTH directions (20.7→20.85 from the room side, 19.3→19.15
  from the arena side); (b) real per-frame movement, starting already
  inside the room at (6,25) and walking south (the direction that
  previously leaked) — now correctly stops at z=21.05 instead of sailing
  through to the far arena wall; (c) re-confirmed the door itself still
  works normally — opened it via `openZoneDoor(1)` and walked through at
  x=0, still reaching the room's far wall (31.95) same as before.

- **Level-3 boss got a real model: a Mixamo-auto-rigged skeleton, replacing
  the procedural box body.** User supplied a real bone-anatomy asset kit
  (`SkeletonBodyPart.fbx`: 18 separate, sensibly-named meshes — SK_Head,
  SK_Spine, SK_Side/ribcage, SK_LClavicle/SK_RClavicle, SK_LArmUp/Down,
  SK_HandL/R, SK_LLegUp/Down, SK_LFoot/RFoot — plus a full PBR texture set:
  BaseColor/Normal/Roughness/Metallic/Emissive) with no rig/skin/animation
  of its own (confirmed by grepping the FBX binary for bone/deformer/
  animstack strings — none existed), and separately 4 stock Mixamo
  animations (Sad Walk, Start Walking, Jump Attack, Mutant Swiping).
  **First attempt (abandoned):** since the raw mesh had no rig, tried
  reparenting the 18 SK_* pieces into a hand-built RIGID hierarchy (real
  joints via `.attach()`, verified correct — a shoulder rotation swung the
  whole arm+forearm+hand chain from the right pivot) and hand-applying
  each Mixamo bone's rotation DELTA (current vs. its own rest pose) onto
  the matching piece. Built and tested live in a throwaway inspector
  (`tools/inspect-skeleton.html`) — produced visibly broken poses (the
  shoulder rotation alone came out physically implausible, arm flung out
  sideways) because a bare delta-copy assumes the two skeletons' local
  joint axes already line up, which they don't without real retargeting
  work. **User correctly pushed back** ("Почему ты собираешь анимации
  сам? Разве mixamo это уже не сделал?") — right call: Mixamo's own
  Auto-Rigger exists to solve exactly this, properly, but only if the
  custom mesh is actually uploaded to mixamo.com and run through it —
  which hadn't happened yet for the first batch of files (those were
  just stock animations on Mixamo's generic rig, never applied to this
  mesh). User did that (uploaded `SkeletonBodyPart.fbx`, placed the
  Auto-Rigger's chin/wrist/elbow/knee/groin markers, downloaded "Sad
  Walk" back "With Skin") and supplied the result. Confirmed via the same
  binary-string check this new file now has real `Deformer`/skin data,
  keeps the original SK_* mesh names, and is bound to a standard
  `mixamorig:*` skeleton — copied in as `models/skeleton/
  skeleton_rigged.fbx`. Loaded through a real `THREE.AnimationMixer` this
  time (no manual pose math at all) — deforms correctly and smoothly;
  confirmed live in the inspector both for its own baked-in Sad Walk clip
  and for `jump_attack.fbx` (a separate, ordinary motion-only clip) played
  on the same mixer — works with zero retargeting code since both just
  share standard `mixamorig:*` bone names, the same principle the mage's
  walk/cast clips already relied on.
  Wired into `prototype.html` as `loadSkeletonAssets()` /
  `makeSkeletonBossModel()`, deliberately mirroring `loadGoblinAssets()`/
  `makeGoblinModel()`'s exact shape (`{object, mixer, actions:
  {idle,run,attack}, current}`) — every place that shape is already
  consumed generically (`updateEnemyVisual`'s status tint blending, the
  animate()-loop crossfade/facing/attack-trigger logic, `parts`/
  `userData.enemyId` tagging for hit detection) needed zero new branches,
  just one added condition (`isSkeletonBoss = isBoss && bossKind!=='ranged'
  && skeletonTemplate`) in `makeEnemy()` gating which model builder runs.
  Per user request ("вся модель скелета была зоной, куда можно попасть
  снарядом"): confirmed live that all 18 SK_* meshes end up in `parts`
  with `userData.enemyId` set — since hit detection is a real
  `raycaster.intersectObjects(enemies.flatMap(e=>e.parts))` against
  those meshes (not a single hitbox), the whole skeleton — skull, ribs,
  every limb — is a legitimate hit target already, no extra code needed
  beyond correct tagging.
  BOSS2 (level 6, ranged) is explicitly excluded (`bossKind!=='ranged'`)
  and keeps the original box body — user chose level-3-only when asked,
  and BOSS2's kiting kit was never part of this ask.
  Applied the real PBR texture set (all 18 pieces share one material,
  confirmed via inspector) once on the template, cloned per-instance same
  as the goblin. Scale (`SKELETON_SCALE=0.00663`) and HP-bar height were
  calibrated to roughly match the original box-boss's own proportions
  (measured the box boss's local head/bar heights and matched the same
  ratio) rather than guessed blind — verified/corrected once live: first
  HP-bar guess measured out to world-y 7.14 (noticeably higher than the
  box-boss's ≈5.04), brought down to a value that measures ≈5.67 by
  reworking the constant from the actual measured skull height instead of
  a flat guess.
  Verified live end-to-end in the actual game (via the project's existing
  `http.server` dev config, `.claude/launch.json` — used for the first
  time this session; raw `file://` navigation was used for everything
  before this, which is why every earlier finding in this doc had to
  work around FBX assets never loading at all): `startLevel(3)` spawns a
  real skinned/deforming skeleton, correctly facing the player; all 18
  parts tagged for hit detection; `triggerGoblinAttack` correctly starts
  and progresses the Jump Attack clip (screenshots show the pose visibly
  changing frame to frame); manually driving position + `advanceGoblin
  Animation(e,'run',dt)` correctly transitions `current` to `'run'`.
  Not yet verified: real gameplay pacing (does the Jump Attack clip's
  native ~3.8s duration feel right against `BOSS_SLAM_COOLDOWN`'s 4.5s),
  and whether `SKELETON_ROTATION_OFFSET`/scale hold up from other camera
  angles — worth a real playtest pass.

- **Real idle animation, replacing the frozen-frame placeholder.** The
  boss previously had no dedicated idle clip (only Sad Walk + Jump
  Attack existed), so `makeSkeletonBossModel()` faked one by cloning the
  walk clip and pausing it at frame 0 — same trick `makeGoblinModel()`
  already used for the identical reason. User supplied a proper stock
  Mixamo idle ("Standing W/Briefcase Idle.fbx") — confirmed motion-only
  (no `Deformer` data) via the same binary-string check used for every
  other clip this session, so it retargets onto the already-rigged
  skeleton with zero extra code, exactly like `jump_attack.fbx` did.
  Copied in as `models/skeleton/idle.fbx`, loaded alongside the other two
  clips in `loadSkeletonAssets()`'s `Promise.all`, root motion stripped
  the same way. `makeSkeletonBossModel()` now just plays it directly
  (`actions.idle.play()`, no `.paused = true` hack) — the frozen-frame
  code and its explanatory comment are gone entirely, not just papered
  over. Verified live: `startLevel(3)` boss's idle action reports
  `isRunning: true`, `paused: false`, a real 14.3s clip duration (not a
  single frame); screenshot confirms a natural standing pose, distinct
  from the previous frozen Sad-Walk-frame-0 stance.

- **Fixed the skeleton boss's "sliding" during Jump Attack, and gated its
  aggro behind proximity.** (1) "Почему босс скользит после прыжка?" —
  root-caused by measuring the clip's Hips position track directly:
  `jump_attack.fbx` is a real leap, not a stationary swing — its root
  travels ~516 raw units (rangeZ ≈ [-2, 514], plus ~38 units of sideways
  drift) over the clip's 3.8s, which at this model's scale
  (`SKELETON_SCALE × BOSS_SCALE`) works out to ≈7.2 world units of actual
  forward travel. `loadSkeletonAssets()` was stripping that root motion
  the same way walk/idle correctly need it stripped (freezing the Hips
  position track to its frame-0 value) — but unlike walk/idle, nothing
  ever compensated by moving `e.mesh.position` to match, so the torso
  stayed rooted in place while the legs/feet kept animating a full
  leaping stride underneath it — a textbook foot-slide/skate artifact.
  Fix: stopped stripping root motion for `attackClip` specifically (walk
  and idle keep the strip — they're meant to be stationary loops).
  Since the Hips bone lives inside the skinned clone, itself a child of
  `group`, letting it carry its own real translation means the leap
  renders in the correct world direction automatically via the normal
  scene graph — no manual position-sync code needed, and hit detection
  stays correct regardless since raycasts test the meshes' actual world
  transforms either way. `e.mesh.position` (the slam AoE's actual origin)
  is untouched, so the ability's balance/radius/cooldown didn't change —
  only the character now visibly leaps forward as the animation always
  intended, instead of sliding.
  Verified live: sampled the actual `mixamorigHips` BONE's (not the
  SkinnedMesh's own transform, which stays at bind pose regardless of
  animation — a real mistake made and caught mid-verification, see below)
  world position across the clip via real incremental `mixer.update(dt)`
  calls — Z-offset from `e.mesh.position` climbed smoothly from 0 to
  ≈7.16 world units and held near there through the clip's end, matching
  the calculated ≈7.2 prediction almost exactly. (First verification
  attempt sampled `SK_Spine`, a `SkinnedMesh`, and saw zero movement at
  every timestamp — worth noting as a real gotcha: a `SkinnedMesh`'s own
  `Object3D` transform is its static bind-pose placement; animated motion
  only shows up on the actual `Bone` objects, not the mesh nodes skinned
  to them.)

  (2) "Сделай так, чтобы он не агрился на игроков пока они не подойдут
  ближе (но не делай механику разагривания, как у обычных противников)"
  — the boss AI comment literally said "Boss: always aggro'd, no patrol/
  leash" — `target` was picked as the nearest player unconditionally,
  every tick, regardless of distance, so a boss would start charging/
  bolting the instant a level loaded even from across the whole arena.
  Added `BOSS_WAKE_RADIUS = 13` and a one-way `e.awakened` flag (defaults
  `false` in `makeEnemy`'s returned object) — checked once per tick only
  while `false`; the instant any player comes within `BOSS_WAKE_RADIUS`
  it flips to `true` permanently and is never reset. Explicitly NOT the
  regular-enemy patrol/chase/leash system per the user's own caveat — no
  `AGGRO_RADIUS` re-checking, no `PATROL_LEASH_RADIUS` give-up-and-return-
  to-patrol, no losing aggro if the target retreats; once woken, `target`
  reverts to the exact same unconditional-nearest-player selection the
  boss already used, permanently, matching "always aggro'd" for the rest
  of the fight. Applies to both bosses uniformly (the `if (e.isBoss)`
  block wraps both the melee and `bossKind==='ranged'` branches) — no
  reason given to treat them differently, and BOSS2's instant-kiting from
  across the room had the exact same "aggros immediately" issue.
  Verified live by calling `hostSimulate(dt)` directly (bypassing the
  normal `isHost`-gated render loop, which this debug session doesn't run
  under) with the player parked 30 units away: `awakened` stayed `false`
  and the boss didn't move for a full simulated second. Moved the player
  to 8 units (inside `BOSS_WAKE_RADIUS`): `awakened` flipped `true`
  within a few ticks, plus the new `hostLog('💀 Босс пробудился!')`
  visibly appeared in the in-game combat log. Moved the player back out
  to 30 units and kept simulating: `awakened` stayed `true` and the boss
  kept closing distance (measured real position movement over half a
  simulated second) — confirming no de-aggro, exactly as asked.

- **The jump-attack fix above traded one bug for a worse one — reverted
  and re-fixed properly.** User report: "Скелет постоянно
  телепортируется" — letting the Hips bone carry its own real root
  motion (the previous fix) meant that ~7-unit leap only ever existed
  inside the skinned clone's LOCAL space, completely invisible to
  `e.mesh.position` (the boss's real, gameplay/AoE-authoritative
  position, untouched the whole time). Every time the attack finished
  and crossfaded back to idle — whose Hips sits back near origin — the
  local offset vanished in the 0.1s blend, reading as a teleport. And per
  the user's actual design ask in the same message ("при прыжке он
  должен приземляться и оставаться в той точке, которая была целью на
  момент начала прыжка") a purely-visual leap was never going to satisfy
  this anyway — the real gameplay position needs to end up there, not
  just the mesh's internal pose.
  Real fix: stripped the attack clip's root motion again (back to how
  walk/idle already need it — the clip's legs/pose still play the full
  leap animation, just without silently translating the mesh internally)
  and replaced the slam's old instant-damage-on-trigger with a
  `jumpState` position tween, the same pattern `chargeState` already
  uses for the charge dash: on trigger, capture `from` (boss's current
  position) and `to` (**the target's position AT THAT MOMENT, not a
  live-tracked reference** — per the user's explicit ask, the boss
  commits to a fixed landing spot and doesn't home in on a moving target
  mid-air) into `e.jumpState`, then `lerpVectors(from, to, t/duration)`
  every tick over `BOSS_JUMP_DURATION=3.8` (matching the clip's own
  duration so the leg animation and the code-driven displacement finish
  together). The AoE damage check itself moved from cast-time to
  landing-time (`frac>=1`) — the natural consequence of "jump attack" now
  actually meaning the boss jumps to where the hit lands, rather than
  hitting instantly from wherever it started. `jumpState` is checked
  *before* `target` each tick specifically so an in-flight jump always
  finishes at its committed spot even if `target` goes stale mid-air
  (player disconnects, gets downed, etc.) — it doesn't need `target`
  again once launched.
  Verified live via direct `hostSimulate(dt)` calls: triggered a slam,
  confirmed `jumpState.to` exactly matched the target's captured position
  and `e.mesh.position` was correctly interpolated partway through
  (matched the expected lerp fraction to 2 decimal places); ran it to
  completion and confirmed the boss's final position exactly equals
  `jumpState.to`; ran 60 more ticks (1 full simulated second) after
  landing and measured **zero** further drift (`distanceTo` == 0.000) —
  the teleport-on-crossfade is gone. Combat log shows the expected
  "🦴 Босс прыгает в атаку!" → "🌋 Босс приземлился и ударил волной!
  (задето: N)" sequence per attack, confirmed on screen.

## Real 2-client network test (2026-09-09)

- First time this prototype was tested with two genuinely separate browser
  tabs (two `preview_start`/`navigate` instances) hitting the project's own
  dev server (`.claude/launch.json`'s `web-mvp` config) instead of `file://`
  — this is also the first session where real FBX assets actually loaded in
  this sandbox, since the earlier `file://` relative-path restriction never
  applied to `http://localhost:8743`.
- Host created a room (code `5JMD`, Lightning), a second real tab joined by
  code (Water) — confirmed via the host's own UI, not just debug state: party
  panel showed "Друг" with a live HP bar, and the combat log printed "Друг
  присоединился". This satisfies the long-outstanding "real 2-player test"
  follow-up from the original co-op-spellcasting prototype and from this
  file's own MVP notes — first time it's actually happened, even though both
  tabs were driven by the same tool rather than two separate humans.
  **Still open**: a real *second human* playing independently (mouse/keyboard
  timing, not driven by the same script) hasn't happened yet — recommend
  scheduling that specifically, since two tabs driven by one script can't
  exercise independent-discovery timing.
- **Water→Lightning synergy confirmed working over the real network path**:
  called `hostResolveCast` for the Water player's Плеск on an enemy (60→48
  HP, `wet` set to 6), then the Lightning player's Разряд on the same enemy
  (48→12 HP — exactly 3× the base 12 dmg, `wet` correctly cleared after).
  Independently corroborated by the game's own combat log printing "Тестер:
  CHAIN SHOCK! x3 + цепь на 0" — not just a debug-value read, the actual
  player-facing feedback fired correctly too.
- Level-1 antechamber door: `openZoneDoor(1)` flips `door.open` to `true`
  correctly, but `door.mesh.position.y` never moved off `closedY` even after
  a 2s real wait — consistent with this sandbox's long-documented unreliable
  `requestAnimationFrame` ticking (nothing new; same limitation noted
  throughout this file). The slide-down animation and the button's
  press-visual therefore still need a real browser session to verify, same
  as everything else animation-timing-dependent in this project.
- Menu's `menu-bg.jpg` placeholder is still a real 404 (expected — art not
  supplied yet, code already picks it up automatically once the file exists).
- Not committed yet (no code changed this session — verification only).
