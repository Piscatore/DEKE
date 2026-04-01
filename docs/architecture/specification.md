# Technical Specification

The main technical reference for DEKE's implementation. This document covers how the system is built: technology choices, project layout, database schema, domain models, API contracts, code patterns, and the architecture of planned packages.

For what DEKE is and why it exists, see [product/overview.md](../product/overview.md). For the theoretical foundations of the evolution engine, see [science/evolution-theory.md](../science/evolution-theory.md).

---

## Technology Stack

| Component | Technology | Version | NuGet / Notes |
|-----------|-----------|---------|---------------|
| Runtime | .NET | 9.0 | LTS target |
| Language | C# | 13 | File-scoped namespaces, records, required keyword |
| Web framework | ASP.NET Core Minimal API | 9.0 | MapGroup, TypedResults |
| Database | PostgreSQL | 16 | Primary data store |
| Vector search | pgvector | 0.7+ | IVFFlat index, cosine similarity |
| Data access | Dapper | 2.1+ | Raw SQL, parameterized queries |
| CRUD helpers | Dapper.FastCrud | 3.3+ | Single-entity CRUD operations |
| Vector types | Pgvector.Dapper | — | Vector type handler for Dapper |
| Embeddings | ONNX Runtime | 1.17+ | Microsoft.ML.OnnxRuntime |
| Embedding model | all-MiniLM-L6-v2 | — | 384-dimensional, local inference |
| MCP server | ModelContextProtocol SDK | — | Tool registration via attributes |
| LLM (planned) | Anthropic API | — | claude-haiku-4-5, claude-sonnet-4-6 |
| LLM local (planned) | Ollama | — | Zero-cost option for mature domains |
| Container runtime | Podman | — | podman-compose for local dev |
| Serialization | System.Text.Json | — | JSONB column mapping |

---

## Project Structure

```
DEKE/
├── DEKE.sln
├── CLAUDE.md                          # Project instructions
├── SPECIFICATION.md                   # (deprecated — content moved here)
├── init.sql                           # Database schema, runs on container start
├── podman-compose.yml                 # PostgreSQL + pgvector container
├── docs/
│   ├── INDEX.md                       # Documentation map
│   ├── architecture/                  # HOW — this directory
│   ├── product/                       # WHAT — requirements, roadmap
│   └── science/                       # WHY — theoretical foundations
├── models/
│   └── all-MiniLM-L6-v2/             # ONNX model + vocabulary
├── scripts/
│   └── download-model.sh             # One-time model download
├── src/
│   ├── Deke.Core/                     # Domain models, interfaces (no dependencies)
│   │   ├── Interfaces/                # IFactRepository, ISourceRepository, etc.
│   │   └── Models/                    # Fact, Source, Term, Pattern, etc.
│   ├── Deke.Infrastructure/           # Implementations
│   │   ├── Data/                      # DbConnectionFactory, DapperConfig, TypeHandlers/
│   │   ├── Embeddings/                # OnnxEmbeddingService, EmbeddingsConfig
│   │   ├── Extraction/                # SimpleExtractionService
│   │   ├── Federation/                # FederatedSearchService, FederationClient
│   │   ├── Harvesters/                # RssHarvester, WebPageHarvester
│   │   └── Repositories/             # Dapper implementations of Core interfaces
│   ├── Deke.Api/                      # REST API endpoints
│   │   ├── Auth/                      # ApiKeyAuthHandler
│   │   └── Endpoints/                 # FactEndpoints, SearchEndpoints, etc.
│   ├── Deke.Mcp/                      # MCP server for LLM integration
│   │   └── Tools/                     # SearchTools, FederationTools
│   ├── Deke.Worker/                   # Background services
│   │   └── Services/                  # SourceMonitorService, PeerHealthCheckService
│   └── Deke.Tests/                    # Unit and integration tests
└── thoughts/                          # RPI workflow artifacts (briefs, research, plans)
```

---

## Database Schema

### Current Tables (implemented in init.sql)

#### sources

Where facts come from. Each source is a monitorable endpoint (RSS feed, web page, API).

| Column | Type | Description |
|--------|------|-------------|
| id | UUID PK | Auto-generated |
| url | TEXT UNIQUE | Source URL |
| domain | VARCHAR(100) | Knowledge domain (e.g. "fishing") |
| name | VARCHAR(200) | Human-readable name |
| type | VARCHAR(20) | WebPage, Rss, Api, Manual |
| check_interval | INTERVAL | How often to poll |
| last_checked_at | TIMESTAMPTZ | Last poll timestamp |
| last_changed_at | TIMESTAMPTZ | Last detected content change |
| content_hash | VARCHAR(64) | SHA-256 of last content |
| credibility | REAL | 0.0-1.0 credibility score |
| is_active | BOOLEAN | Whether monitoring is enabled |
| created_at | TIMESTAMPTZ | Row creation time |
| metadata | JSONB | Extensible key-value store |

#### facts

The atomic unit of knowledge. A single claim with provenance metadata.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID PK | Auto-generated |
| content | TEXT | The fact text |
| domain | VARCHAR(100) | Knowledge domain |
| embedding | vector(384) | ONNX-generated embedding |
| confidence | REAL | 0.0-1.0 confidence score |
| source_id | UUID FK | Reference to source |
| related_fact_ids | UUID[] | Legacy relation array |
| entities | JSONB | Extracted named entities |
| metadata | JSONB | Extensible key-value store |
| created_at | TIMESTAMPTZ | Row creation time |
| updated_at | TIMESTAMPTZ | Last modification |
| is_outdated | BOOLEAN | Soft-delete flag |
| outdated_reason | VARCHAR(200) | Why marked outdated |

Indexes: IVFFlat on embedding (cosine ops, lists=100), domain, source_id, created_at DESC, composite (domain, is_outdated).

#### terms

Domain-specific terminology with variants and translations.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID PK | Auto-generated |
| canonical_form | VARCHAR(200) | Authoritative term form |
| domain | VARCHAR(100) | Knowledge domain |
| contexts | JSONB | Usage contexts and variants |
| translations | JSONB | Cross-language equivalents |
| created_at | TIMESTAMPTZ | Row creation time |
| updated_at | TIMESTAMPTZ | Last modification |

Unique constraint on (canonical_form, domain).

#### patterns

Discovered regularities across facts within a domain.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID PK | Auto-generated |
| description | TEXT | Pattern description |
| domain | VARCHAR(100) | Knowledge domain |
| pattern_type | VARCHAR(50) | observation, correlation, rule |
| evidence_fact_ids | UUID[] | Supporting fact references |
| confidence | REAL | Pattern confidence score |
| occurrence_count | INT | How many times observed |
| discovered_at | TIMESTAMPTZ | Discovery timestamp |
| last_validated_at | TIMESTAMPTZ | Last validation check |
| is_active | BOOLEAN | Whether pattern is current |

#### fact_relations

Explicit typed relationships between facts.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID PK | Auto-generated |
| from_fact_id | UUID FK | Source fact |
| to_fact_id | UUID FK | Target fact |
| relation_type | VARCHAR(50) | causes, contradicts, supports, requires, supersedes, instance_of, related_to |
| confidence | REAL | Relation confidence score |
| created_at | TIMESTAMPTZ | Row creation time |

Unique constraint on (from_fact_id, to_fact_id, relation_type).

#### learning_logs

Tracks system improvement cycles.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID PK | Auto-generated |
| domain | VARCHAR(100) | Knowledge domain |
| cycle_type | VARCHAR(50) | harvest, pattern_discovery, relation_mapping |
| started_at | TIMESTAMPTZ | Cycle start |
| completed_at | TIMESTAMPTZ | Cycle end |
| facts_added | INT | New facts this cycle |
| facts_updated | INT | Updated facts |
| facts_outdated | INT | Retired facts |
| patterns_discovered | INT | New patterns |
| relations_added | INT | New relations |
| notes | TEXT | Free-form notes |
| error_message | TEXT | Error details if failed |

#### federation_peers

Known DEKE instances for cross-instance queries.

| Column | Type | Description |
|--------|------|-------------|
| id | UUID PK | Auto-generated |
| instance_id | VARCHAR(100) UNIQUE | Peer's self-declared identity |
| base_url | TEXT | Peer API base URL |
| domains | JSONB | Array of domains the peer serves |
| capabilities | JSONB | Array of supported capabilities |
| protocol_version | VARCHAR(10) | Federation protocol version |
| last_seen_at | TIMESTAMPTZ | Last successful health check |
| is_healthy | BOOLEAN | Current health status |
| created_at | TIMESTAMPTZ | When peer was first discovered |

### Planned Tables (Package 1 Phase 1)

#### fact_provenance

One-to-many link between facts and the sources that assert them. Supports corroboration tracking without fact duplication.

| Column | Type | Description |
|--------|------|-------------|
| fact_id | UUID FK | Reference to fact |
| source_id | UUID FK | Reference to source |
| extracted_at | TIMESTAMPTZ | When this assertion was recorded |
| extraction_method | VARCHAR(30) | rss_harvest, web_harvest, manual_api, llm_extract |
| extraction_confidence | REAL | Confidence of the extraction process itself |

#### fact_version

Immutable change history. Enables "what did we believe on date X?" queries.

| Column | Type | Description |
|--------|------|-------------|
| fact_id | UUID FK | Reference to fact |
| content_snapshot | TEXT | Fact text at this version |
| embedding_snapshot | vector(384) | Embedding at this version |
| changed_at | TIMESTAMPTZ | When change occurred |
| change_reason | VARCHAR(30) | source_update, manual_correction, merge, contradiction_resolution |

#### domain_trust_policy

Per-domain configuration governing how strictly provenance is enforced.

| Column | Type | Description |
|--------|------|-------------|
| domain | VARCHAR(100) PK | Domain identifier |
| require_primary_source | BOOLEAN | Only primary-tier sources auto-accepted |
| min_corroboration | INT | Minimum independent sources before auto-accept (0 = disabled) |
| auto_accept_tiers | JSONB | Source tiers that bypass review queue |
| flag_for_review_tiers | JSONB | Source tiers that enter review queue |
| temporal_validity_required | BOOLEAN | Facts without valid_from are flagged |
| min_confidence_score | REAL | Facts below threshold enter review queue |

### Planned Source Table Additions

| Column | Type | Description |
|--------|------|-------------|
| credibility_score | REAL | 0.0-1.0, separate from fact confidence |
| source_tier | VARCHAR(20) | primary, secondary, aggregated, unverified |
| independence_fingerprint | VARCHAR(64) | Hash of publisher identity for corroboration independence |
| last_verified_at | TIMESTAMPTZ | Last confirmation source is still authoritative |

### Planned Fact Table Additions

| Column | Type | Description |
|--------|------|-------------|
| confidence_score | REAL | 0.0-1.0, assessed at extraction time |
| corroboration_count | INT | Independent sources asserting equivalent claim |
| valid_from | TIMESTAMPTZ | When the fact became true |
| valid_until | TIMESTAMPTZ | When the fact ceased to be true |
| last_verified_at | TIMESTAMPTZ | Last confirmation against source |
| contradiction_flag | BOOLEAN | Another fact with opposing polarity exists |
| trust_state | VARCHAR(20) | unscored, accepted, flagged, contested, rejected |

---

## Core Models

Models reside in `Deke.Core/Models/`. Mutable entities use classes; DTOs and immutable data use records.

### Fact

The atomic knowledge unit. Maps to the `facts` table.

```csharp
public class Fact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Content { get; set; }
    public required string Domain { get; set; }
    public float[]? Embedding { get; set; }
    public float Confidence { get; set; } = 1.0f;
    public Guid? SourceId { get; set; }
    public List<Guid> RelatedFactIds { get; set; } = [];
    public JsonElement Entities { get; set; }
    public Dictionary<string, JsonElement> Metadata { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsOutdated { get; set; }
    public string? OutdatedReason { get; set; }
}
```

### Source

A monitorable knowledge source. Maps to the `sources` table.

```csharp
public class Source
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Url { get; set; }
    public required string Domain { get; set; }
    public string? Name { get; set; }
    public SourceType Type { get; set; } = SourceType.WebPage;
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromDays(1);
    public DateTimeOffset? LastCheckedAt { get; set; }
    public DateTimeOffset? LastChangedAt { get; set; }
    public string? ContentHash { get; set; }
    public float Credibility { get; set; } = 0.5f;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public Dictionary<string, JsonElement> Metadata { get; set; } = new();
}
```

### FederationPeer

A known DEKE instance. Maps to the `federation_peers` table.

### FederationManifest

Response DTO returned by `GET /api/federation/manifest`. Contains instance identity, protocol version, domain list, and capabilities.

### SearchResult / FactSearchResult

Query result types returned by the search endpoints. FactSearchResult includes a similarity score and optional provenance metadata for federated results.

---

## API Contracts

### Search Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/search` | Semantic search across facts. Accepts domain filter, query text, minimum similarity, limit. Returns ranked results with similarity scores. Supports federation headers for cross-instance delegation. |
| POST | `/api/search/context` | Returns facts formatted as a markdown context block for LLM consumption. Accepts domain, query, and token budget. |

Federation headers (inbound): `X-Federation-Request-Id`, `X-Federation-Visited`, `X-Federation-Hop-Count`, `X-Federation-Max-Hops`.

### Fact Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/facts` | List facts by domain with pagination |
| GET | `/api/facts/{id}` | Get a single fact by ID |
| POST | `/api/facts` | Create a new fact (requires API key) |
| PUT | `/api/facts/{id}` | Update a fact (requires API key) |
| DELETE | `/api/facts/{id}` | Soft-delete (mark outdated) |

### Source Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/sources` | List sources by domain |
| GET | `/api/sources/{id}` | Get a single source |
| POST | `/api/sources` | Register a new source (requires API key) |
| PUT | `/api/sources/{id}` | Update source configuration |
| DELETE | `/api/sources/{id}` | Deactivate a source |

### Federation Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/federation/manifest` | Returns this instance's manifest (identity, domains, capabilities) |
| GET | `/api/federation/peers` | List known peers |
| POST | `/api/federation/peers` | Register a new peer (requires API key) |
| DELETE | `/api/federation/peers/{id}` | Remove a peer |

### Planned Endpoints

| Method | Path | Package | Description |
|--------|------|---------|-------------|
| POST | `/api/advisory` | P2 | Submit an advisory request, receive a grounded expert response |
| GET | `/api/domains/{domain}/status` | P2 | Domain activation status and metrics |
| POST | `/api/feedback` | P3 | Submit explicit feedback for an interaction |
| GET | `/api/interactions` | P3 | Query interaction log |
| GET | `/api/health/domain/{domain}` | P3 | Domain evolution health report |
| GET | `/api/domains/{domain}/trust-policy` | P1-Phase1 | Domain trust policy configuration |
| PUT | `/api/domains/{domain}/trust-policy` | P1-Phase1 | Update trust policy |
| GET | `/api/review-queue` | P1-Phase2 | Facts pending human review |

---

## MCP Tools

### Current (implemented)

| Tool | Description |
|------|-------------|
| `consult_domain_expert` | Semantic search with federation awareness. Accepts query, domain, optional minimum similarity and max results. Returns facts with similarity scores, source URLs, and federation provenance. |
| `get_context` | Returns facts formatted as markdown context for LLM consumption. Accepts query, domain, and token budget. |
| `list_available_domains` | Lists all domains available locally and across federated peers. Returns domain names with fact counts and peer source. |

### Planned (Package 2)

| Tool | Description |
|------|-------------|
| `GetDomainAdvice` | Full advisory pipeline call. Accepts query, domain, optional stakes hint and citation preference. Returns AdvisoryResponse with confidence band, cited facts, knowledge gaps, and escalation status. Formatted as markdown for Claude Code consumption. |

---

## Code Patterns

### Database Connection

All database access goes through `DbConnectionFactory`:

```csharp
await using var conn = await _db.CreateConnectionAsync(ct);
```

### Vector Similarity Search

Raw SQL with Pgvector.Dapper for vector operations:

```csharp
var embedding = new Vector(queryEmbedding);
var results = await conn.QueryAsync<FactSearchResult>(
    """
    SELECT f.id, f.content, f.domain, f.confidence,
           1 - (f.embedding <=> @embedding::vector) AS similarity
    FROM facts f
    WHERE f.domain = @domain
      AND f.is_outdated = FALSE
      AND 1 - (f.embedding <=> @embedding::vector) > @minSimilarity
    ORDER BY f.embedding <=> @embedding::vector
    LIMIT @limit
    """,
    new { embedding, domain, minSimilarity, limit });
```

### CRUD Operations

Use Dapper.FastCrud for simple single-entity operations:

```csharp
var fact = await conn.GetAsync(new Fact { Id = id });
await conn.InsertAsync(fact);
await conn.UpdateAsync(fact);
```

### Embedding Generation

```csharp
var embedding = _embeddingService.GenerateEmbedding("some text");
// Returns float[384], L2-normalized
```

Always normalize embeddings before storage.

### Type Handlers

Registered in `DapperConfig.Initialize()`:

- `JsonbTypeHandler<T>` -- serializes/deserializes JSONB columns via System.Text.Json
- `GuidArrayTypeHandler` -- maps PostgreSQL `uuid[]` to `List<Guid>`
- `EnumTypeHandler<T>` -- maps string columns to C# enums
- Pgvector.Dapper provides the vector type handler

### Dependency Injection

Services registered in `ServiceCollectionExtensions.AddDekeInfrastructure()`:

```csharp
services.AddSingleton<DbConnectionFactory>();
services.AddSingleton<IEmbeddingService, OnnxEmbeddingService>();
services.AddScoped<IFactRepository, FactRepository>();
services.AddScoped<ISourceRepository, SourceRepository>();
// ... other repositories
```

Federation services registered via `AddDekeFederation()`.

### Endpoint Pattern

Static classes with `MapXxxEndpoints(this WebApplication app)` extension methods:

```csharp
public static class FactEndpoints
{
    public static void MapFactEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/facts").WithTags("Facts");
        group.MapGet("/", SearchFacts).WithName("SearchFacts");
        // ...
    }

    private static async Task<IResult> SearchFacts(
        IFactRepository factRepo,
        [FromQuery] string? domain,
        CancellationToken ct)
    {
        // ...
    }
}
```

### Background Services

Extend `BackgroundService`, inject `IServiceProvider` for scoped dependencies:

```csharp
public class SourceMonitorService(
    IServiceProvider serviceProvider,
    ILogger<SourceMonitorService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            // ... do work with scoped services
            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}
```

---

## Package 2 Architecture

Package 2 (Knowledge Leverage) transforms accumulated knowledge into grounded advisory responses. For product context, see [product/roadmap.md](../product/roadmap.md).

### Three-Layer Stack

| Layer | Name | Responsibility |
|-------|------|----------------|
| Layer 1 | Fixed Interface | AdvisoryRequest in, AdvisoryResponse out. Never changes. REST API, MCP tool, future A2A -- all expose this contract. |
| Layer 2 | Shared Core | Context assembly, trust metadata interpretation, confidence banding, uncertainty expression, LLM call mechanics, graceful degradation, Package 3 event emission. |
| Layer 3 | Domain Adapter | Replaceable plugin: system prompt, fact weighting, context format, trust calibration, escalation rules. Domain-specific logic lives here only. |

### Advisory Pipeline (13 stages)

1. Request validation (domain exists and activated)
2. Niche classification (query_type x stakes_level)
3. Domain adapter resolution (IAdvisoryAdapter or DefaultAdvisoryAdapter)
4. Fact retrieval (Package 1 semantic search with trust filters)
5. Adapter-weighted ranking (domain-specific re-ranking)
6. Quality prediction (predicted_quality_score from fact metadata)
7. Context assembly (adapter.FormatContext)
8. Trust calibration (adapter.CalibrateTrust)
9. Prompt construction (system prompt + trust guidance + context + query)
10. LLM call (Anthropic API or Ollama)
11. Response assembly (wrap in AdvisoryResponse with ConfidenceBand)
12. Escalation check (adapter.ShouldEscalate)
13. Package 3 emission (AdvisoryInteractionEvent)

### IAdvisoryAdapter Interface

```csharp
public interface IAdvisoryAdapter
{
    string SystemPrompt();
    IReadOnlyList<KnowledgeFact> WeightFacts(IReadOnlyList<KnowledgeFact> facts, string query);
    string FormatContext(IReadOnlyList<KnowledgeFact> facts);
    string CalibrateTrust(TrustMetadata trustMetadata);
    bool ShouldEscalate(string query, string draftResponse);
    DomainActivationCriteria ActivationCriteria { get; }
}
```

New domains implement this interface. DefaultAdvisoryAdapter provides sensible defaults for domains without a custom adapter (~100-200 lines per custom adapter).

### Confidence Bands

| Band | Score Range | Meaning |
|------|-------------|---------|
| High | >= 0.75 | Multiple corroborated, recent facts cover the query well |
| Medium | 0.50-0.74 | Adequate coverage, some gaps or lower corroboration |
| Low | 0.25-0.49 | Sparse facts, aged sources, or partial coverage |
| Insufficient | < 0.25 | Knowledge base cannot support a grounded response |

The honesty constraint is enforced at the shared core level: no adapter can override uncertainty expression upward.

### LLM Selection Policy

| Backend | Condition |
|---------|-----------|
| claude-haiku-4-5 (Anthropic API) | Default. Selected when knowledge_depth_score >= 0.6 |
| claude-sonnet-4-6 (Anthropic API) | ShouldEscalate() returns true, or ConfidenceBand = Low with High stakes, or explicit caller override |
| Ollama (local) | domain.AllowLocalModel = true AND knowledge_depth_score >= 0.75 |

As the knowledge base deepens, the minimum capable model decreases -- the knowledge compensation principle.

### Fixed Contracts

```csharp
public record AdvisoryRequest
{
    public required string Query { get; init; }
    public required string Domain { get; init; }
    public string? SessionId { get; init; }
    public string[] PriorExchanges { get; init; } = [];
    public AdvisoryHints? Hints { get; init; }
}

public record AdvisoryResponse
{
    public required string Content { get; init; }
    public required string InteractionId { get; init; }
    public ConfidenceBand Confidence { get; init; }
    public string[] CitedFactIds { get; init; } = [];
    public string[] KnowledgeGaps { get; init; } = [];
    public bool WasEscalated { get; init; } = false;
    public string? EscalationReason { get; init; }
    public bool ContainsConflictingEvidence { get; init; } = false;
    public AdvisoryMetadata Metadata { get; init; } = new();
}
```

The `ContainsConflictingEvidence` field (from Guardrail G5) is set when Package 1 contradiction resolution is triggered, even when the served response is high-confidence.

---

## Package 3 Architecture

Package 3 (Evolution Engine) makes DEKE self-improving. It does not generate responses or call LLMs -- it observes, measures, and directs improvement. For the neuroscience and QD algorithm foundations, see [science/evolution-theory.md](../science/evolution-theory.md).

### Three-Signal Framework

Package 3's core defense against Goodhart's Law. No single signal track can be maximized at the expense of true quality without the others detecting divergence.

| Signal Track | Sources | Strength | Weakness |
|-------------|---------|----------|----------|
| Explicit feedback | User ratings, thumbs, corrections | Clear intent, direct measurement | Goodhart risk if over-weighted; low participation |
| Behavioral / implicit | Reformulation (negative), follow-up depth (positive), adoption signals | Passive capture, hard to game | Noisy, delayed |
| Veracity / objective | Fact corroboration over time, subsequent knowledge confirmation | Fully Goodhart-resistant, ground truth | Delayed, not always measurable |

Domain trust policies govern signal weighting: legal domains weight veracity highest; hobby domains can rely more on explicit and behavioral signals.

### Prediction-Error Engine

Before each advisory response, Package 3 computes `predicted_quality_score` from fact metadata (confidence scores, corroboration levels, domain coverage, contradiction density, recency, adapter niche match). After the response, three measurement windows collect actual quality signals:

- **Immediate** (0-5 min): Explicit rating, reformulation detection
- **Short-term** (1-24 hr): Follow-up depth, fact corrections
- **Delayed** (24-72 hr): Hindsight satisfaction probe (highest-value signal per research)

The learning signal is:

```
delta = actual_quality_signal - predicted_quality_score
```

Delta propagates backward through the causal chain: adapter configuration fitness, fact reliability scores, source credibility scores, and knowledge gap detection.

### Curiosity Service

Self-directed knowledge acquisition. Maintains a domain question corpus, continuously self-queries DEKE, measures answerability, and generates harvest directives for Package 1.

Gap taxonomy:

| Gap Type | Harvest Directive |
|----------|-------------------|
| Depth gap | Find more primary sources on the same topic |
| Breadth gap | Explore related topic areas |
| Recency gap | Re-verify existing sources; find recent sources |
| Contradiction gap | Find authoritative resolution; prioritize primary sources |
| Blind spot | Broad exploration harvest |

### Adapter Evolution via MAP-Elites

Rather than a single adapter per domain, Package 3 maintains a MAP-Elites archive: a grid of adapter variants indexed by behavioral dimensions (query_type x stakes_level x knowledge_depth).

Evolution mechanics:

| Operation | Mechanism |
|-----------|-----------|
| Mutation | GEPA-derived reflective mutation: feed full interaction trajectory to LLM, diagnose failure, propose adapter revision. One Haiku call per mutation attempt. |
| Competition | Variants run in shadow mode alongside incumbents. Promote if higher mean delta over N interactions. |
| Pruning | Variants with persistent negative deltas deprecated after grace period. |
| Preservation | One variant per niche always preserved as reference baseline. |

The honesty constraint safety gate (Guardrail G1) asserts that a variant's confidence ratio has not degraded before promotion. A variant that increases confident-but-wrong responses is disqualified even if its overall quality score is higher.

### GEPA Component Mapping to DEKE

| GEPA Component | DEKE Equivalent |
|----------------|-----------------|
| System with prompts | Domain adapter: SystemPrompt(), WeightFacts(), FormatContext() |
| Training dataset | AdvisoryInteractionEvent archive per niche |
| Evaluation metric | Predicted vs. actual quality delta |
| Feedback function | Three-signal framework |
| Rollout | One advisory interaction cycle |
| Pareto frontier | MAP-Elites archive grid |
| Reflective mutation | LLM-generated adapter variant from failure diagnosis |

### Package 3 Component Profile

| Component | Role |
|-----------|------|
| PostgreSQL | Interaction logs, delta history, adapter archive, curiosity queue, quality scores |
| Background .NET services | Curiosity loop, delta propagation, adapter fitness updates (all async) |
| No LLM API calls | Package 3 observes and learns; it does not generate |
| Lightweight ML (optional) | Prediction model can start as weighted average, upgrade to regression model |
| No embedding computation | Operates on structured metadata, not vectors |
| REST API | /api/feedback, /api/interactions, /api/health/domain |

---

## Configuration Reference

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Deke": "Host=localhost;Port=5432;Database=deke;Username=deke;Password=deke"
  },
  "Embeddings": {
    "ModelPath": "models/all-MiniLM-L6-v2/model.onnx",
    "VocabPath": "models/all-MiniLM-L6-v2/vocab.txt"
  },
  "Federation": {
    "InstanceId": "deke-primary",
    "InstanceName": "DEKE Primary",
    "ProtocolVersion": "1",
    "Domains": ["fishing", "software-product-advisor"],
    "Capabilities": ["search", "context"],
    "HealthCheckIntervalMinutes": 5,
    "MaxHops": 3,
    "TimeoutSeconds": 10,
    "LocalityWeights": {
      "Local": 1.0,
      "Peer": 0.8
    }
  }
}
```

### Environment Variable Overrides

| Variable | Default | Description |
|----------|---------|-------------|
| `ConnectionStrings__Deke` | localhost | PostgreSQL connection string |
| `Embeddings__ModelPath` | models/all-MiniLM-L6-v2/model.onnx | ONNX model file |
| `Embeddings__VocabPath` | models/all-MiniLM-L6-v2/vocab.txt | Vocabulary file |
| `Federation__InstanceId` | — | This instance's unique identity |
| `Federation__MaxHops` | 3 | Maximum federation delegation depth |
| `Federation__TimeoutSeconds` | 10 | Peer request timeout |

### API Authentication

Write endpoints require `X-Api-Key` header. Read endpoints and the manifest endpoint are open. Federated search uses shared-secret authentication between peers.
