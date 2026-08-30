# ADR-0001: Networking Stack — Netcode for GameObjects

## Status
Accepted

## Date
2026-08-26

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 6.3 LTS |
| **Domain** | Networking |
| **Knowledge Risk** | HIGH — Unity 6 released after LLM training cutoff |
| **References Consulted** | `docs/engine-reference/unity/modules/networking.md`, `docs/engine-reference/unity/VERSION.md`, `docs/engine-reference/unity/breaking-changes.md` |
| **Post-Cutoff APIs Used** | `Unity.Netcode` (NGO), Unity Transport 2.x, Unity Relay service |
| **Verification Required** | (1) Confirm NGO package version compatibility with Unity 6.3.x before first sprint; (2) Test `NetworkVariable<ElementalStatusFlags>` sync under 4-5 player load; (3) Benchmark Unity Relay latency PC-to-PC for target regions |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | None |
| **Enables** | All gameplay system ADRs (spell casting, elemental synergies, player state, dungeon state) |
| **Blocks** | All programming work — this decision determines the architecture of every networked system |
| **Ordering Note** | Must be Accepted before any GDD that specifies networked behaviour can be finalized |

## Context

### Problem Statement
Covenant of Mages is a co-op session-based first-person dungeon crawler for 4-5 players. Multiplayer is the entire game's core premise — it cannot be reduced to a feature. Every gameplay system (spell casting, elemental status effects, player death/spectator mode, dungeon state) requires a network layer to function. This decision must be made before any architecture or programming work begins.

### Constraints
- Solo developer's first 3D project — learning curve is a real cost
- PC platform only (Steam/Epic) — no mobile networking constraints
- Session-based (25-30 min) — no persistent server required between sessions
- Budget: indie/solo — ongoing CCU fees are a meaningful concern
- Unity 6.3 LTS is the pinned engine — deep engine integration is available
- Team of 1 — anti-cheat is a lower priority than for live-service titles

### Requirements
- Must support 4-5 simultaneous players in a shared dungeon session
- Must synchronize elemental status effects (Wet, Burning, Electrified, Frozen) across all clients in real-time
- Must handle FPS-style player movement with acceptable feel (<150 ms perceived latency acceptable — this is a spell-casting game, not a twitch shooter)
- Must support spectator mode (dead players observe until fight ends) without disconnecting them from the session
- Must handle host/player disconnects gracefully with a defined failure mode
- Must run without requiring players to open ports (NAT traversal required)
- Must be free or near-zero cost for development and early access phases

## Decision

**Use Unity Netcode for GameObjects (NGO) with Unity Transport 2.x and Unity Relay.**

The game uses a **listen-server (host-client) topology**: one player acts as the host (server + client simultaneously), the remaining 3-4 players connect as clients. Unity Relay handles NAT traversal via Unity Gaming Services — players do not need to configure their routers.

**Host migration policy**: NGO does not support automatic host migration. If the host disconnects, the session ends. This is a defined and documented behaviour, not a bug. Players are returned to the lobby with a "Host disconnected — session ended" message.

### Architecture Diagram

```
[Host: Server + Client 1]
           |
      Unity Relay (NAT traversal)
      /      |      \
[Client 2] [Client 3] [Client 4]

Authority:  Host (server-authoritative for all game state)
Transport:  Unity Transport 2.x (UDP-based, reliable/unreliable channels)
NAT:        Unity Relay (Unity Gaming Services — managed, pay-as-you-go)
```

### Key Interfaces

```csharp
// Elemental status effects — explicit int backing type required for NGO serialization
[System.Flags]
public enum ElementalStatusFlags : int
{
    None        = 0,
    Wet         = 1 << 0,
    Burning     = 1 << 1,
    Electrified = 1 << 2,
    Frozen      = 1 << 3,
}

// Per-player status sync (server writes, all clients read)
private NetworkVariable<ElementalStatusFlags> _statusFlags =
    new NetworkVariable<ElementalStatusFlags>(
        ElementalStatusFlags.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

// Spell cast: client → server
[ServerRpc]
void CastSpellServerRpc(int spellId, Vector3 targetPosition) { }

// Synergy triggered: server → all clients
// NetworkObjectReference used — GameObject cannot be passed in RPCs
[ClientRpc]
void TriggerSynergyClientRpc(
    SynergyType synergyType,
    NetworkObjectReference originPlayer,
    NetworkObjectReference targetPlayer) { }

// Player death state (spectator mode — client does NOT disconnect)
private NetworkVariable<bool> _isDead =
    new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
```

**Key NGO rules for this project (validated by unity-specialist):**
- Status effects written server-side only — `IsServer` check inside `ServerRpc` handler; never write from owning client
- Subscribe to `NetworkVariable.OnValueChanged` in `OnNetworkSpawn`, not in `Awake` or `Start`
- `NetworkObject` component must be on prefab root, not a nested child
- Dungeon entities (enemies, pickups) spawned via `NetworkObject.Spawn()` on the server — not `Instantiate()` per-client
- `[ServerRpc]` can only be called by the object's owner by default — use `[ServerRpc(RequireOwnership = false)]` for trigger zones or shared-world interactions

## Alternatives Considered

### Alternative A: Photon Fusion 2
- **Description**: Third-party managed multiplayer service with built-in relay, matchmaking, and two topologies (Shared State and Server Mode)
- **Pros**: Managed relay + matchmaking included; excellent built-in prediction/reconciliation; battle-tested in Unity indie games; good documentation
- **Cons**: Per-CCU pricing after free tier (100 CCU free, then ~$100+/month); third-party dependency outside Unity ecosystem; Fusion 2 broke API compatibility with Fusion 1 (risk of future churn); extra SDK surface area for a first-time 3D developer
- **Rejection Reason**: CCU pricing creates budget uncertainty for a solo indie project. The free tier is sufficient for development but creates a hard cliff for any commercial activity. NGO achieves equivalent results for this player count at near-zero cost with the official Unity toolchain and dedicated support.

### Alternative B: FishNet (Fish-Networking)
- **Description**: Open-source community-maintained Unity networking framework with prediction, reconciliation, and lag compensation built-in
- **Pros**: MIT license (completely free); more mature prediction/reconciliation than NGO; clean API design; well-regarded in the Unity community
- **Cons**: No official Unity support — bugs require community resolution; relay infrastructure not included (requires Edgegap partnership or self-hosting); smaller documentation base; higher risk for a solo developer's first 3D networked project
- **Rejection Reason**: Boss Room (NGO's official co-op dungeon sample) provides directly applicable reference for this exact game type — co-op dungeon crawler architecture. FishNet has no equivalent. For a first-time 3D networked game, access to official, directly applicable samples is more valuable than marginally better prediction at this player count and game pace.

## Consequences

### Positive
- Official Unity 6.3 LTS support — updates and bug reports through Unity's maintenance channel
- Boss Room reference project (official NGO co-op dungeon sample) is directly applicable to this game's architecture
- Near-zero cost: Unity Relay is pay-as-you-go (generous free tier; bandwidth cost is minimal at 4-5 players × 25 min/session)
- `NetworkVariable<ElementalStatusFlags>` natively handles elemental state sync without custom serialization
- Server-authoritative architecture provides an anti-cheat foundation if needed post-launch
- Single SDK — no third-party vendor dependency or pricing surprises
- Custom transport interface means Unity Transport can be swapped if relay pricing or reliability becomes a concern

### Negative
- NGO prediction and reconciliation are less mature than Photon Fusion — smooth movement at >100 ms latency may require custom interpolation work
- Listen-server gives the host a latency advantage — acceptable for a cooperative game, but worth communicating to players
- Unity Relay bandwidth costs scale with session count at any meaningful player count — requires monitoring post-launch
- NGO API has changed frequently across Unity 6 patch versions — package version must be pinned

### Risks
- **Risk**: NGO prediction insufficient for smooth FPS movement at 100-200 ms latency
  - **Mitigation**: Prototype netcode movement in the first `/prototype co-op-spellcasting` sprint. The spell-casting pace is much slower than a twitch shooter — basic interpolation is likely sufficient. If not, FishNet transport can be substituted without rewriting game logic.
- **Risk**: Host disconnect ends session with no recovery
  - **Mitigation**: Documented design decision, not a bug. UX: clear "Host disconnected" message + return to lobby. Post-MVP: evaluate host migration if player feedback identifies this as a frequent pain point.
- **Risk**: NGO package API changes between Unity 6.3 patch releases
  - **Mitigation**: Pin `com.unity.netcode.gameobjects` to a specific version in `Packages/manifest.json`. Update deliberately only, with a full test pass.
- **Risk**: Solo developer's first networked 3D game — multiplayer bugs are significantly harder to debug than single-player bugs
  - **Mitigation**: Establish "two-instance local test" protocol (editor + standalone build) before every PR merge. Use Unity Network Profiler and Network Simulator from day one.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| game-concept.md | Co-op 4-5 players in shared dungeon session | NGO listen-server supports host + up to 4 remote clients |
| game-concept.md | Cross-player elemental synergies (water+lightning → chain bolt) | `NetworkVariable<ElementalStatusFlags>` per player; server evaluates synergy conditions on `OnValueChanged` |
| game-concept.md | Dead player becomes spectator until fight ends | `NetworkVariable<bool> _isDead`; client switches camera without disconnecting from session |
| game-concept.md | Session-based (25-30 min) — no persistent server | Listen-server tears down with session end; no dedicated server infrastructure required |
| game-concept.md | MVP: co-op 2-4 players | NGO listen-server scales to 2 players for MVP with zero architecture changes |

## Performance Implications
- **CPU**: NGO `NetworkManager` tick (default 30 Hz) ≈ 0.5-1 ms/frame at peak load — within 16.6 ms frame budget
- **Memory**: 5× `NetworkVariable<ElementalStatusFlags>` + 5× `NetworkVariable<bool>` ≈ negligible (< 1 KB total)
- **Load Time**: No impact
- **Network**: Estimated ~5-15 KB/s per player (position + status + spell events at 30 Hz). 5 players × 15 KB/s × 25 min ≈ 110 MB/session — well within Unity Relay free tier (~50 GB/month during development)

## Migration Plan
No existing networking code. This is the foundation ADR for a new project. All future networking code will be written against NGO APIs from the start. Unity Transport 2.x's custom transport interface allows future substitution of the transport layer without changing game logic.

## Validation Criteria
1. 4 clients connect to a host — all see each other's positions synchronized in a shared scene
2. Elemental status applied to Player A appears on Player B's screen within 100 ms at LAN latency
3. `_isDead = true` switches the owning client to spectator camera without disconnecting from session
4. Host can start and end a session cleanly — no dangling connections remain
5. Two-instance local test (editor + standalone build) behaves consistently with expected networked behaviour
6. NGO package version is pinned in `Packages/manifest.json`

## Related Decisions
- **Depends on**: None (first ADR)
- **Enables**: ADR for spell casting architecture, ADR for elemental synergy system, all gameplay system ADRs
- **Related GDDs**: `design/gdd/game-concept.md`
