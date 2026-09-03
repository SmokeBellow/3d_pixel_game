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
   arena; level 2 is a bigger arena with more/tankier enemies and a denser
   pillar field (a maze-lite, since enemy AI has no corridor pathing — pillars
   give the "navigate around obstacles" feel safely); level 3 is a single
   boss with a large HP pool that alternates a point-blank slam AoE and a
   telegraphed charge dash. Clearing level 3 loops back to level 1.
10. Each school's tier-2 ("advanced") spell has a real mechanical hook, not
    just bigger numbers: Fire's Огненный шар splashes nearby enemies, Water's
    Волна soaks a whole group instead of one target, Lightning's Цепная
    молния auto-chains at reduced damage even without a Water setup. Tier-2
    spells also render visibly bigger (thicker bolt / larger projectile).

Networking: PeerJS (WebRTC data channels), P2P, host-authoritative — mirrors
ADR-0001's listen-server topology in spirit. Host disconnect ends the session
(same policy as ADR-0001).

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
- **One element per player, chosen at lobby** — each school has a 2-spell
  unlock ladder (basic → advanced), each with its own icon (✨💥 Fire,
  💧🌊 Water, ⚡🌩️ Lightning). Fire/Water = flying projectiles, Lightning =
  hitscan; Water→Lightning = Chain Shock synergy (x3 dmg + chain)
- **Spell leveling by use**: only landed hits count; damage scales +12%/level
- **Gold** (random per kill) and **shared party XP/level** — clearing a level
  guarantees enough XP for the next party level; leveling raises everyone's
  max HP; every other level (3, 5, 7…) offers a passive-skill choice (mentor).
  The tier-2 spell is **not** auto-granted — it must be bought at the shop
  stand, same as leveling up an existing spell
- **Physical shop/mentor in the hub room**: no modal dialog — glowing stands
  with floating price tags you interact with via E (buy your next spell,
  level up your active spell, pick a passive when one's pending, or step to
  the portal to continue); continuing spawns the next level with more
  enemies (scaled HP) and respawns everyone at that level's spawn point.
  Trying to buy/level up without enough gold flashes the price tag and a
  bottom-screen prompt red instead of silently failing
- **Sessions**: a level ends when all its enemies are dead; no more
  auto-respawning enemies mid-level
- Humanoid enemies with walk + attack-lunge animation; AI: patrol (wander
  near home) → aggro (chase, **stopping at a visible distance** rather than
  hugging the player) within AGGRO_RADIUS → de-aggro beyond the larger
  PATROL_LEASH_RADIUS (returns home)
- Player HP, melee damage from enemies, downed state (movement/casting
  locked) → timed respawn
- Procedural stone-brick textures, flickering torches (brightened after
  playtest feedback), dim ambient + fog + player glow for a lit-circle
  vignette — **press L to disable for debugging**
- One fixed **spawn/respawn point with a canvas-drawn pentagram** (glowing
  circle + five-point star + runes, pulsing light) — no external image file.
  Enemies cannot enter its `SPAWN_SAFE_RADIUS` ward
- **Player color = element color** (each school has a fixed color, not a
  round-robin per-connection color)
- **Lightning renders as a zigzag bolt** (hit detection unchanged — still a
  straight raycast from the camera; only the visual is jagged)
- **Lobby with chat**: joining players see a waiting room (with player list
  + chat, shared with the host's lobby chat) until the host starts; late
  joiners after the game has started also get chat/lobby state correctly
- **Player visuals**: Mixamo "Ganfaul M Aure" model (idle/walk/cast animations,
  cast synced to actual cast pace) tinted per element for third-person/remote
  players; first-person view kept the original placeholder box/sphere arms
  (a real FPS viewmodel was tried and reverted — see Findings)
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
- (multiplayer-specific findings to be filled after a real 2+ client playtest)
