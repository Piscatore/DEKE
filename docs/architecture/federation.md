# Federation Protocol

Technical design for DEKE's federation protocol, enabling multiple DEKE instances to share knowledge across organizational and domain boundaries.

For federation's role in the product roadmap, see [product/roadmap.md](../product/roadmap.md). For the main technical reference, see [specification.md](specification.md).

---

## Overview and Design Principles

Federation allows DEKE instances to discover each other, advertise their domain expertise, and delegate search queries across instance boundaries. The design follows these principles:

- **Autonomy first.** Each instance is fully functional standalone. Federation is additive -- removing peers degrades breadth but not correctness.
- **No central authority.** There is no registry server or coordinator. Instances discover peers through static configuration, DNS, or mutual registration.
- **Eventual consistency.** Peer manifests and health status are cached locally and refreshed periodically. Stale data is tolerated; incorrect data is not.
- **Locality preference.** Local results are always preferred over federated results. Federation fills gaps; it does not replace local knowledge.
- **Loop prevention by construction.** Every federated request carries a visited set, hop counter, and request ID. The protocol makes infinite loops structurally impossible.

---

## Architecture

### Components

| Component | Location | Responsibility |
|-----------|----------|----------------|
| FederationPeer (model) | Deke.Core | Peer entity mapped to `federation_peers` table |
| FederationConfig (model) | Deke.Core | Configuration: instance identity, domains, capabilities, locality weights |
| FederationManifest (model) | Deke.Core | Response DTO for the manifest endpoint |
| IFederationPeerRepository | Deke.Core | Peer data access contract |
| FederationPeerRepository | Deke.Infrastructure | Dapper implementation of peer persistence |
| FederationClient | Deke.Infrastructure | HTTP client for calling peer search and manifest endpoints |
| FederatedSearchService | Deke.Infrastructure | Orchestrates local search + peer delegation + result merging |
| IFederatedSearchService | Deke.Core | Orchestration interface |
| FederationEndpoints | Deke.Api | REST endpoints for manifest and peer CRUD |
| PeerHealthCheckService | Deke.Worker | Background service for periodic peer health monitoring |
| FederationTools | Deke.Mcp | MCP tools with federation awareness |

### How Instances Communicate

```
Instance A (fishing)              Instance B (software)
┌─────────────────┐               ┌─────────────────┐
│ Search request   │               │                 │
│ domain=software  │               │                 │
│                  │  HTTP POST    │                 │
│  FederatedSearch ├──────────────►│ /api/search     │
│  Service         │  + headers    │                 │
│                  │◄──────────────┤ ranked results  │
│  merge + rank    │               │ + provenance    │
│  return to user  │               │                 │
└─────────────────┘               └─────────────────┘
```

The FederatedSearchService decides whether to delegate based on:

1. Local results are insufficient (below minimum count or similarity threshold), OR
2. The queried domain is not served locally but a healthy peer advertises it.

Delegation is transparent to the caller. The merged response includes provenance metadata indicating which results are local and which are federated.

---

## Service Registry

DEKE supports three peer discovery mechanisms, used individually or in combination:

### Static Configuration

Peers defined in `appsettings.json` under the `Federation:Peers` section:

```json
{
  "Federation": {
    "Peers": [
      {
        "InstanceId": "deke-fishing",
        "BaseUrl": "https://fishing.deke.example.com"
      }
    ]
  }
}
```

On startup, the Worker's `PeerHealthCheckService` fetches each static peer's manifest and populates the `federation_peers` table.

### Mutual Registration

Peers register themselves via `POST /api/federation/peers`. This enables dynamic discovery when instances know each other's URLs but not their identities or capabilities. The registration payload includes instance ID and base URL; the receiving instance fetches the manifest to populate domain and capability data.

### DNS-Based Discovery (Planned)

SRV record lookup for `_deke._tcp.{domain}` to discover instances in the same DNS zone. Intended for enterprise deployments where multiple DEKE instances operate within a shared infrastructure.

---

## API Endpoints

### Manifest

```
GET /api/federation/manifest
```

Returns this instance's self-description. No authentication required.

Response:

```json
{
  "instanceId": "deke-primary",
  "instanceName": "DEKE Primary",
  "protocolVersion": "1",
  "domains": [
    { "name": "fishing", "factCount": 342 },
    { "name": "software-product-advisor", "factCount": 47 }
  ],
  "capabilities": ["search", "context"],
  "federationEnabled": true
}
```

### Federated Search

```
POST /api/search
```

The search endpoint accepts federation context via HTTP headers. When these headers are absent, the request is treated as a client-initiated search and federation delegation may occur. When headers are present, the request is a peer-delegated search and no further delegation occurs (to prevent loops).

**Federation Headers (inbound)**

| Header | Type | Description |
|--------|------|-------------|
| `X-Federation-Request-Id` | string | Unique ID for this federation chain. Used for deduplication and tracing. |
| `X-Federation-Visited` | comma-separated | Instance IDs already visited in this chain. |
| `X-Federation-Hop-Count` | integer | Current hop depth. |
| `X-Federation-Max-Hops` | integer | Maximum allowed hops (default: 3). |

**Federation Metadata (in response)**

Federated results include provenance metadata:

```json
{
  "results": [
    {
      "factId": "...",
      "content": "...",
      "similarity": 0.87,
      "provenance": {
        "source": "local",
        "instanceId": null
      }
    },
    {
      "factId": "...",
      "content": "...",
      "similarity": 0.82,
      "provenance": {
        "source": "federated",
        "instanceId": "deke-fishing"
      }
    }
  ],
  "federationMetadata": {
    "peersQueried": 1,
    "peersResponded": 1,
    "peersFailed": 0
  }
}
```

### Replication (Planned -- Phase 4)

Selective fact replication between instances for domains where low-latency access is required. Not yet designed. The current protocol is query-time delegation only.

---

## Data Model

### federation_peers Table

See [specification.md](specification.md) for the full schema. Key fields:

- `instance_id` -- the peer's self-declared identity (from its manifest)
- `base_url` -- the peer's API base URL
- `domains` -- JSONB array of domain objects with name and fact count
- `capabilities` -- JSONB array of supported operations
- `is_healthy` -- set by PeerHealthCheckService based on manifest fetch success
- `last_seen_at` -- updated on each successful health check

### Facts Metadata Extension

Federated search results carry a `Provenance` object:

```csharp
public record Provenance
{
    public required string Source { get; init; }   // "local" or "federated"
    public string? InstanceId { get; init; }       // peer instance ID if federated
}
```

This is a response-only decoration; it is not persisted. The caller sees where each result came from but DEKE does not store federated facts locally (except in the planned Phase 4 replication scenario).

---

## Loop Prevention

Federation delegation without loop prevention creates a risk of infinite recursion. DEKE prevents loops through four complementary mechanisms:

### Visited Set

Every federated request carries an `X-Federation-Visited` header containing the instance IDs of all nodes already visited in this chain. Before delegating to a peer, the FederatedSearchService checks whether the peer's instance ID appears in the visited set. If it does, that peer is skipped.

### Hop Limit

The `X-Federation-Hop-Count` header increments at each delegation. When it reaches `X-Federation-Max-Hops` (default: 3, configurable per instance), no further delegation occurs. The receiving instance executes only a local search.

### Request ID

The `X-Federation-Request-Id` header is a unique identifier generated at the originating instance. If an instance receives a request with a request ID it has already seen (due to a network topology that creates multiple paths), it returns its cached response rather than re-executing the search.

### Timeout

Each peer delegation has a configurable timeout (default: 10 seconds). If a peer does not respond within the timeout, its results are omitted and the response proceeds with available results. The peer is not marked unhealthy on a single timeout; the PeerHealthCheckService makes that determination based on sustained failures.

---

## Result Ranking

When local and federated results are merged, they are scored using a weighted formula:

```
final_score = similarity * locality_weight
```

Where `locality_weight` is configured per instance:

| Source | Default Weight | Rationale |
|--------|---------------|-----------|
| Local | 1.0 | Local facts are managed by this instance; full trust |
| Peer | 0.8 | Peer facts are outside this instance's control; slight discount |

This ensures that a local fact with similarity 0.80 outranks a federated fact with similarity 0.85 (0.80 * 1.0 = 0.80 vs 0.85 * 0.80 = 0.68). Locality weights are configurable to accommodate trust relationships between specific instances.

Results are sorted by `final_score` descending, with ties broken by local-first ordering.

---

## Security

### Shared Secret (Current)

Peer-to-peer requests include an `X-Api-Key` header with a shared secret. The receiving instance validates the key against its configured API key. This is adequate for trusted environments (internal networks, known peers).

### Mutual TLS (Planned)

For production federation across organizational boundaries, mutual TLS with client certificates provides stronger authentication. Each instance presents a certificate; the receiving instance validates it against a trusted CA or pinned certificate list.

### Authorization

- **Manifest endpoint**: Open (no authentication). Discovery must be possible without pre-shared credentials.
- **Search delegation**: Requires shared secret or mTLS. Peers that fail authentication receive 401 and are marked unhealthy.
- **Peer registration**: Requires API key. Only authorized administrators can add peers.
- **Write operations**: Never federated. Facts are only created locally. Federation is read-only.

---

## MCP Integration

### consult_domain_expert

The primary MCP tool for LLM consumers. Replaces the earlier `search_knowledge` tool with federation awareness.

Behavior:

1. Searches local facts by semantic similarity.
2. If local results are insufficient and healthy peers advertise the queried domain, delegates to those peers.
3. Merges and ranks results using locality-weighted scoring.
4. Returns results with provenance metadata so the LLM can distinguish local from federated facts.

### list_available_domains

Replaces the earlier `list_domains` tool. Returns all domains available across local and federated instances.

Response includes:

- Domain name
- Fact count (local or as reported by peer manifest)
- Source: "local" or the peer's instance ID
- Whether the domain is locally served or only available via federation

---

## Configuration Reference

```json
{
  "Federation": {
    "InstanceId": "deke-primary",
    "InstanceName": "DEKE Primary Instance",
    "ProtocolVersion": "1",
    "Domains": ["fishing", "software-product-advisor"],
    "Capabilities": ["search", "context"],
    "HealthCheckIntervalMinutes": 5,
    "MaxHops": 3,
    "TimeoutSeconds": 10,
    "LocalityWeights": {
      "Local": 1.0,
      "Peer": 0.8
    },
    "Peers": [
      {
        "InstanceId": "deke-fishing",
        "BaseUrl": "https://fishing.deke.example.com"
      }
    ]
  }
}
```

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| InstanceId | string | required | This instance's unique identity |
| InstanceName | string | optional | Human-readable name |
| ProtocolVersion | string | "1" | Federation protocol version |
| Domains | string[] | [] | Domains this instance serves |
| Capabilities | string[] | ["search","context"] | Supported federation operations |
| HealthCheckIntervalMinutes | int | 5 | Peer health check frequency |
| MaxHops | int | 3 | Maximum delegation depth |
| TimeoutSeconds | int | 10 | Per-peer request timeout |
| LocalityWeights.Local | float | 1.0 | Score multiplier for local results |
| LocalityWeights.Peer | float | 0.8 | Score multiplier for federated results |

---

## Implementation Status

### Phase 1: Discovery and Manifest -- Complete

- `federation_peers` table in init.sql
- FederationPeer, FederationConfig, FederationManifest models in Deke.Core
- IFederationPeerRepository interface and Dapper implementation
- FederationEndpoints (manifest GET, peer CRUD)
- PeerHealthCheckService in Deke.Worker
- Federation configuration section in appsettings.json
- DI registration via AddDekeFederation()

### Phase 2: Federated Search -- Complete

- FederatedSearchService orchestration (local + peer delegation + merge)
- FederationClient HTTP client for peer search calls
- Federation header parsing and propagation in SearchEndpoints
- Visited set, hop count, and request ID loop prevention
- Locality-weighted result ranking
- Provenance metadata on search results

### Phase 3: MCP Tools -- Complete (bundled with Phase 2)

- `consult_domain_expert` tool replacing `search_knowledge`
- `list_available_domains` tool replacing `list_domains`
- `get_context` updated to use FederatedSearchService

### Phase 4: Selective Replication -- Planned

- Peer-to-peer fact replication for low-latency access to remote domains
- Replication policy configuration (which domains, frequency, conflict resolution)
- Not yet designed

### Phase 5: Trust Federation -- Planned

- Cross-instance trust metadata propagation
- Peer credibility scoring based on result quality over time
- Federation-aware Package 3 delta propagation
- Not yet designed
