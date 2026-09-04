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
5. Controls: WASD move, mouse look (click to lock; also works unlocked for
   touchpads, plus arrow keys), LMB or Space cast, wheel/Q/1-2-3 switch spell
   slot, **E to interact with a shop stand**, **L toggles full brightness /
   no fog**, **K instantly kills every enemy on the map** (debug cheats).
6. The combo: a Water player soaks an enemy, a Lightning player hits it —
   3x damage + zigzag chain to nearby enemies (Lightning renders as a jagged
   bolt now, not a straight line, but still lands exactly on the cursor).
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
level-2 transition, and — newly — hub-room teleport, the level 2 (bigger
arena/tougher enemies) and level 3 (boss, 900 HP, slam + charge attacks
both firing correctly over 10 simulated seconds) zones, and looping from
level 3 back to level 1. All confirmed working with zero console errors.
Real multi-client test pending.

## Features

- FPS view (camera-system.md formulas), pointer-lock + touchpad/arrow-key
  look fallback, shared `sensitivity` value for mouse + keyboard look
- **Real particle effects on every spell hit** — Kenney's CC0 Particle Pack
  (`textures/particles/`, see its `LICENSE.txt`) drives a small CPU-updated
  `THREE.Points` burst system (`spawnParticleBurst`/`updateParticleBursts`):
  flame + ember burst on Огненный шар's explosion, droplets flying outward
  on Волна's nova, and a small spark/glow burst on every other spell's hit
  (Искра/Плеск/Разряд/Chain Shock) that previously had zero impact feedback
- **One element per player, chosen at lobby** — each school has a 2-spell
  unlock ladder (basic → advanced), each with its own icon (✨💥 Fire,
  💧🌊 Water, ⚡🌩️ Lightning). Fire/Water = flying projectiles, Lightning =
  hitscan; Water→Lightning = Chain Shock synergy (x3 dmg + chain)
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
- **Player color = element color** (each school has a fixed color, not a
  round-robin per-connection color)
- **Lightning renders as a zigzag bolt** (hit detection unchanged — still a
  straight raycast from the camera; only the visual is jagged)
- **Lobby with chat**: joining players see a waiting room (with player list
  + chat, shared with the host's lobby chat) until the host starts; late
  joiners after the game has started also get chat/lobby state correctly
- **Player visuals**: Mixamo "Brady" model (idle/run/cast animations) tinted
  per element for third-person/remote players; first-person view kept the
  original placeholder box/sphere arms (a real FPS viewmodel was tried and
  reverted — see Findings). Movement animation is a Running clip, not
  Walking — a deliberate swap. The cast gesture starts playing immediately
  on cast, but the actual shot (projectile/hitscan/nova) is deliberately
  delayed ~1.2s to fire exactly when the animation's arm reaches full
  extension, not before
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
  system just for expanding shapes.
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
- (multiplayer-specific findings to be filled after a real 2+ client playtest)
