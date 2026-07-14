# Project Map

Component inventory for DEKE's codebase — one entry per module/major
component, built up region by region ("mapping sweeps"). Produced by the
`/overhaul` subproject (see `/overhaul/OVERHAUL-SKETCH-v0.2.md` §3.1). Each
entry follows a fixed template: what it is, what it owns, its key
dependencies, any naming issues (cross-referencing `docs/GLOSSARY.md`), any
design smells (escalated as ADRs, never fixed here), a confidence rating,
and the actual sources read.

This file is additive: each OP-004 sub-packet appends the entries for one
repo region and never edits another region's entries. See `overhaul/STATE.md`
for which regions are done.

## Regions covered so far

- **Deke.Core** (OP-004a, complete) — entries below.
- **Deke.Infrastructure** (OP-004b, complete) — entries below.
- **Deke.Api** (OP-004c, complete) — entries below.
- **Deke.Mcp** (OP-004d, complete) — entries below.
- **Deke.Worker** (OP-004e, complete) — entries below.
- **tests/Deke.Tests** (OP-004f, complete) — entries below.

---

## Deke.Core

> `src/Deke.Core` — domain models and interfaces, zero project/package
> dependencies (`Deke.Core.csproj` has no `<PackageReference>`/`<ProjectReference>`
> items, confirming the claim in top-level `CLAUDE.md`). Physically flat:
> everything lives directly under `Interfaces/` or `Models/`, no further
> subdirectories. The six entries below are analytical groupings by domain
> concept, not existing folder names — hence all marked `KEEP` rather than
> proposing a rename: the grouping is new documentation, not a claim that the
> code should be physically reorganized to match.

## Fact & Source Domain  [KEEP]
- **What it is:** The core knowledge unit (`Fact`) and its origin (`Source`),
  plus the harvest → extract → chunk → embed pipeline that turns raw source
  content into stored facts.
- **Responsibility:** Owns the shape and lifecycle of a fact, from harvested
  raw text through extraction, chunking, embedding, storage, and retrieval.
- **Key dependencies:** →Search & Trust Contracts (`IFactRepository.SearchAsync`
  returns `FactSearchResult`); →Federation (`IFactRepository.GetDomainStatsAsync`
  returns `DomainStats`, which is defined in `Models/FederationManifest.cs` —
  see Naming issues).
- **Naming issues:** `IChunker`/`SemanticChunkerAdapter` — references
  `docs/GLOSSARY.md`'s PROPOSED row (namingissues.md #5); the interface here
  is the real, already-implemented code the glossary row is about. Also:
  `DomainStats` is filed under `Models/FederationManifest.cs` despite being a
  Fact-domain return type — not a naming conflict, logged in
  `overhaul/PARKING-LOT.md` as a file-location nit.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `src/Deke.Core/Interfaces/IFactRepository.cs`,
  `ISourceRepository.cs`, `IFactRelationRepository.cs`, `IHarvester.cs`,
  `IExtractionService.cs`, `IChunker.cs`, `IEmbeddingService.cs`;
  `src/Deke.Core/Models/Fact.cs`, `ExtractedFact.cs`, `HarvestResult.cs`,
  `Source.cs`, `FactRelation.cs`.

## Term & Pattern Domain  [KEEP]
- **What it is:** Canonical terminology (`Term`, with multilingual/context
  variants) and discovered behavioral regularities (`Pattern`), plus a
  per-domain audit trail (`LearningLog`) of learning cycles.
- **Responsibility:** Owns the knowledge base's emergent structure —
  vocabulary and patterns inferred from facts — and the bookkeeping for each
  learning cycle that produced them.
- **Key dependencies:** →Fact & Source Domain (`Pattern.EvidenceFactIds`
  references `Fact` by `Guid` only — a loose, non-compile-time dependency).
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `src/Deke.Core/Interfaces/ITermRepository.cs`,
  `IPatternRepository.cs`, `ILearningLogRepository.cs`;
  `src/Deke.Core/Models/Term.cs`, `Pattern.cs`, `LearningLog.cs`.

## Search & Trust Contracts  [KEEP]
- **What it is:** The shared result/response shapes returned by any search
  (`FactSearchResult`, `SearchResponse`, `ContextResponse`), the scoring
  service that ranks results (`ITrustScoringService`), and the query audit
  log (`InteractionLog`).
- **Responsibility:** Owns the single "what a search returns" contract,
  shared by local fact search, the advisory pipeline, and federated search
  alike — plus how a raw similarity score becomes a trust score.
- **Key dependencies:** →Federation (`SearchResponse`/`ContextResponse` each
  carry an optional `FederationMetadata`).
- **Naming issues:** none.
- **Design smells:** none newly found here. `ITrustScoringService.Score()`'s
  signature (similarity, confidence, sourceCredibility, a validity/recency
  window, localityWeight) substantiates the doc/code drift already escalated
  as **ADR-0005** (proposed, from OP-003, re: `federation.md`'s
  under-documented ranking formula) — not re-escalated here.
- **Confidence:** HIGH
- **Sources read:** `src/Deke.Core/Interfaces/ITrustScoringService.cs`,
  `IInteractionLogRepository.cs`; `src/Deke.Core/Models/SearchResult.cs`,
  `InteractionLog.cs`.

## Advisory Pipeline Contract (Layer 1+2)  [KEEP]
- **What it is:** The fixed, additive-only request/response contract for an
  advisory call (`AdvisoryRequest` → `AdvisoryResponse`), the pipeline
  interface that fulfills it, and the append-only interaction audit record.
- **Responsibility:** Owns the stable "ask DEKE a question, get a grounded,
  cited, confidence-scored answer" contract and its audit trail, deliberately
  insulated from adapter/backend churn (per each model's own XML doc
  comments: "Never changes; new fields are additive").
- **Key dependencies:** →Fact & Source Domain (`AdvisoryResponse.CitedFactIds`
  and `AdvisoryInteraction.CitedFactIds` reference `Fact` by `Guid` only —
  loose).
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `src/Deke.Core/Interfaces/IAdvisoryPipeline.cs`,
  `IAdvisoryInteractionRepository.cs`, `ILlmService.cs`,
  `ILlmSelectionPolicy.cs`; `src/Deke.Core/Models/AdvisoryRequest.cs`,
  `AdvisoryResponse.cs`, `ConfidenceBand.cs`, `Stakes.cs`,
  `AdvisoryInteraction.cs`.

## Advisory Adapter Plugin (Layer 3)  [KEEP]
- **What it is:** The extensibility point domain packages implement to
  customize an advisory: system prompt, fact weighting, context formatting,
  and trust-score calibration, gated by activation criteria.
- **Responsibility:** Owns the one seam where domain-specific behavior plugs
  into the shared advisory pipeline — the interface's own doc comment notes
  adapters "cannot raise confidence" above what the shared pipeline computes.
- **Key dependencies:** →Search & Trust Contracts (`WeightFacts`/
  `FormatContext` operate on `FactSearchResult`; `CalibrateTrust` consumes an
  `ITrustScoringService.Score()` output value).
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `src/Deke.Core/Interfaces/IAdvisoryAdapter.cs`;
  `src/Deke.Core/Models/DomainActivationCriteria.cs`.

## Federation  [KEEP]
- **What it is:** Cross-instance search delegation — the peer registry
  (`FederationPeer`), instance manifest/runtime config, and the
  request/response/provenance shapes for a federated search or context call.
- **Responsibility:** Owns how DEKE instances discover each other, delegate
  searches, and track hop provenance and loop prevention (`FederationContext`,
  `MaxHops`).
- **Key dependencies:** →Search & Trust Contracts (`IFederatedSearchService`
  returns `SearchResponse`/`ContextResponse`).
- **Naming issues:** `DomainStats` (a Fact-domain return type — see Fact &
  Source Domain entry above) is physically defined here, in
  `Models/FederationManifest.cs`, rather than colocated with `Fact.cs` —
  logged in `overhaul/PARKING-LOT.md` as a file-location nit, not a naming
  conflict (the type's own name is fine).
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `src/Deke.Core/Interfaces/IFederationPeerRepository.cs`,
  `IFederatedSearchService.cs`; `src/Deke.Core/Models/FederationManifest.cs`,
  `FederationPeer.cs`, `FederationConfig.cs`, `FederationDtos.cs`.

---

## Deke.Infrastructure

> `src/Deke.Infrastructure` — Dapper/pgvector/ONNX/harvester implementation
> project, one `<ProjectReference>` to `Deke.Core`. Physically organized into
> real subdirectories (`Data/`, `Embeddings/`, `Harvesters/`, `Extraction/`,
> `Repositories/`, `Llm/`, `Trust/`, `Federation/`, `Advisory/`) plus a root
> `ServiceCollectionExtensions.cs` composition root — entries below follow
> these existing folders rather than inventing new groupings.

## Data Access & Type Handlers  [KEEP]
- **What it is:** `DbConnectionFactory` (opens an `NpgsqlConnection` from a
  shared `NpgsqlDataSource`), `DapperConfig.Initialize()` (one-time Dapper
  type-handler + dialect registration), and four custom `SqlMapper.TypeHandler`
  implementations (Jsonb, GuidArray, FloatArray, Enum).
- **Responsibility:** Owns how every repository gets a PostgreSQL connection
  and how Dapper maps PostgreSQL-specific column types (`jsonb`, `uuid[]`,
  `real[]`, native enums) to/from .NET types. The pgvector `vector` type
  itself is handled by `Pgvector.Dapper`'s own `VectorTypeHandler`, just
  registered here — matching top-level `CLAUDE.md`'s description exactly.
- **Key dependencies:** consumed by every entry in Repositories below.
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `Data/DbConnectionFactory.cs`, `Data/DapperConfig.cs`,
  `Data/TypeHandlers/JsonbTypeHandler.cs`, `GuidArrayTypeHandler.cs`
  (partial), `FloatArrayTypeHandler.cs` (name/registration only),
  `EnumTypeHandler.cs` (name/registration only).

## Embeddings  [KEEP]
- **What it is:** `OnnxEmbeddingService` (owns the ONNX `InferenceSession` +
  vocabulary, tokenizes and runs all-MiniLM-L6-v2 inference, implements
  `IEmbeddingService`) and `OnnxEmbeddingGenerator` (a thin adapter wrapping
  the former to satisfy `Microsoft.Extensions.AI`'s
  `IEmbeddingGenerator<string, Embedding<float>>`, consumed by
  `SemanticChunkerAdapter`).
- **Responsibility:** Owns turning text into a normalized 384-dim vector, and
  exposing that capability under both DEKE's own `IEmbeddingService` contract
  and the `Microsoft.Extensions.AI` abstraction the chunker library expects.
- **Key dependencies:** →Extraction (`SemanticChunkerAdapter` depends on
  `OnnxEmbeddingGenerator`, not `OnnxEmbeddingService`, directly).
- **Naming issues:** none — the two classes look similar but are a real
  service + a real adapter, not duplication (confirmed by reading both
  bodies).
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `Embeddings/EmbeddingsConfig.cs`,
  `Embeddings/OnnxEmbeddingService.cs`, `Embeddings/OnnxEmbeddingGenerator.cs`.

## Harvesters  [KEEP]
- **What it is:** Three `IHarvester` implementations, one per `SourceType`:
  `RssHarvester` (via `System.ServiceModel.Syndication`), `WebPageHarvester`
  (via `AngleSharp`), `FileSystemHarvester` (`.md`/`.markdown`/`.txt` files).
- **Responsibility:** Owns turning a `Source` (RSS feed, web page, or local
  file tree) into raw `HarvestResult` content for the extraction stage.
- **Key dependencies:** →Extraction (`SimpleExtractionService` runs on
  `HarvestResult.Content`).
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `Harvesters/RssHarvester.cs` (full),
  `WebPageHarvester.cs` (partial), `FileSystemHarvester.cs` (partial).

## Extraction  [KEEP]
- **What it is:** `SimpleExtractionService` (`IExtractionService` — currently
  a deliberately naive placeholder: whole trimmed content becomes one
  `ExtractedFact` at a fixed 0.8 confidence, no real NLP) and
  `SemanticChunkerAdapter` (`IChunker`, wraps the `SemanticChunker.NET`
  package at a 384-token limit).
- **Responsibility:** Owns turning harvested raw content into extracted facts
  and, separately, chunking long text into semantically coherent pieces
  before embedding.
- **Key dependencies:** →Embeddings (`SemanticChunkerAdapter` takes an
  `IEmbeddingGenerator<string, Embedding<float>>` constructor dependency).
- **Naming issues:** `IChunker`/`SemanticChunkerAdapter` — this is the
  concrete implementation behind `docs/GLOSSARY.md`'s APPROVED row; also the
  concrete fix target for the `overhaul/PARKING-LOT.md` "Chunk stage status
  wrong in retrieval-pipeline.md" item (OP-003) — both already tracked, not
  re-escalated here.
- **Design smells:** none. `SimpleExtractionService`'s naivety is honestly
  named ("Simple"), not a doc/code mismatch.
- **Confidence:** HIGH
- **Sources read:** `Extraction/SimpleExtractionService.cs`,
  `Extraction/SemanticChunkerAdapter.cs`.

## Repositories  [KEEP]
- **What it is:** Nine Dapper-based repository implementations — one per
  `Deke.Core` repository interface: `FactRepository`, `SourceRepository`,
  `TermRepository`, `PatternRepository`, `FactRelationRepository`,
  `LearningLogRepository`, `InteractionLogRepository`,
  `FederationPeerRepository`, `AdvisoryInteractionRepository`.
- **Responsibility:** Owns all SQL for CRUD + the pgvector similarity search
  (`FactRepository.SearchAsync`, `1 - (embedding <=> @vector::vector)`).
  Every repository follows the same shape: constructor-injected
  `DbConnectionFactory`, raw SQL strings, no query builder.
- **Key dependencies:** →Data Access & Type Handlers (`DbConnectionFactory`,
  registered type handlers).
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH (`FactRepository`, `FederationPeerRepository` read in
  full; remaining seven inferred from consistent naming, 1:1 interface match
  confirmed in `ServiceCollectionExtensions.cs`, and the two read in full
  sharing an identical structural pattern — not independently read line by
  line).
- **Sources read:** `Repositories/FactRepository.cs` (full),
  `FederationPeerRepository.cs` (partial); interface list cross-checked
  against `ServiceCollectionExtensions.cs:32-38,75`.

## Trust  [KEEP]
- **What it is:** `TrustScoringService`, the sole `ITrustScoringService`
  implementation — a pure function combining similarity, confidence, source
  credibility (neutral 0.5 default when absent, e.g. federated results),
  exponential recency decay (180-day half-life), and a caller-supplied
  locality weight.
- **Responsibility:** Owns the concrete trust/ranking formula every search
  path (local, advisory, federated) calls through `ITrustScoringService`.
- **Key dependencies:** consumed by Advisory (`KnowledgeDepth.Compute`) and
  Federation (`FederatedSearchService.MergeResults`) below.
- **Naming issues:** none.
- **Design smells:** none newly found. This is the concrete implementation
  substantiating **ADR-0005** (federation ranking formula doc/code drift,
  already proposed from OP-003/OP-004a) — the exact 5-factor formula
  (similarity × confidence × credibility × recencyDecay × localityWeight) is
  visible here, not re-escalated.
- **Confidence:** HIGH
- **Sources read:** `Trust/TrustScoringService.cs`.

## Federation (Infrastructure)  [KEEP]
- **What it is:** `FederatedSearchService` (`IFederatedSearchService` — runs
  local search, decides whether to delegate via `ShouldFederate`, queries
  peers in parallel, merges + trust-scores results, logs one interaction per
  top-level call) and `FederationClient` (raw HTTP POST to a peer's
  `/api/search`, with hop-count/origin/visited/request-id headers).
- **Responsibility:** Owns the runtime mechanics of cross-instance search —
  the peer-selection, hop-limiting, and result-merging logic that
  `Deke.Core`'s Federation contracts (`FederationPeer`, `FederationContext`)
  describe the shape of.
- **Key dependencies:** →Trust (`ITrustScoringService.Score()` for both local
  and federated results); →Repositories (`FactRepository`,
  `FederationPeerRepository`, `InteractionLogRepository`).
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `Federation/FederatedSearchService.cs`,
  `Federation/FederationClient.cs`.

## Llm — Gemini/OpenAI Backend (ILlmService)  [RETIRED — see OP-008c]
- **Status:** Retired. `src/Deke.Infrastructure/Llm/` (`LlmConfig`,
  `GeminiLlmService`, `OpenAiLlmService`, `NoOpLlmService`) and
  `Deke.Core`'s `ILlmService` interface have been deleted entirely, and the
  `AddDekeLlm` DI registration removed. This was a second, independent LLM
  abstraction that coexisted with the Advisory pipeline's `IChatClient`-based
  system (see Advisory Pipeline Implementation entry below) — flagged as a
  design smell in ADR-0006 (proposed), resolved by ADR-0007 (accepted), and
  the retirement carried out by `overhaul/packets/OP-008c.md`. Confirmed via
  repo-wide grep: zero remaining references to `ILlmService`, `LlmProvider`,
  `GeminiLlmService`, `OpenAiLlmService`, `NoOpLlmService`, or `LlmConfig`
  anywhere in `src/`.
- **What replaced it:** `PatternDiscoveryService.cs` (see Pattern Discovery
  entry, Deke.Worker region) was this system's sole consumer; it now resolves
  the keyed `ollama` `IChatClient` (`AdvisoryClientKeys.Ollama`) through the
  same `ChatClientRegistration`/`AddAdvisoryChatClients` infrastructure the
  Advisory Pipeline Implementation entry describes, falling back to the old
  templated pattern description on failure.
- **Confidence:** HIGH
- **Sources read:** `overhaul/packets/OP-008c.md`;
  `docs/adr/ADR-0007-gemini-openai-backend-undocumented-status.md`; grep
  confirming zero remaining references in `src/`.

## Advisory Pipeline Implementation (Layer 2/3 concrete)  [KEEP]
- **What it is:** The concrete fulfillment of `Deke.Core`'s Advisory Pipeline
  Contract and Adapter Plugin interfaces: `AdvisoryPipeline` (the 7-stage
  pipeline: validate → retrieve → assemble context → construct prompt → call
  model → assemble response → log), `ChatClientRegistration` (registers
  keyed `IChatClient` backends — `anthropic` via `Anthropic.SDK`'s
  `AnthropicClient`, `ollama` via `OllamaSharp`'s `OllamaApiClient`),
  `LlmSelectionPolicy` (routes haiku/sonnet/Ollama per knowledge depth,
  confidence band, and stakes), `KnowledgeDepth` (the
  `knowledge_depth_score` formula: mean top-5 trust × coverage ×
  top-similarity), and two `IAdvisoryAdapter`s (`DefaultAdvisoryAdapter`,
  `SoftwareProductAdvisorAdapter`).
- **Responsibility:** Owns turning a validated `AdvisoryRequest` into a
  grounded, cited, confidence-banded `AdvisoryResponse` — the honesty
  constraint (confidence band derives from retrieval, adapters cannot raise
  it) is enforced in `AdvisoryPipeline.AdviseAsync`, exactly per each
  `Deke.Core` interface's own doc comments.
- **Key dependencies:** →Trust (`ITrustScoringService` via `KnowledgeDepth`
  and directly); →Repositories (`FactRepository`,
  `AdvisoryInteractionRepository`); →Llm entry above (a sibling, unrelated
  LLM system — see that entry's Design smells).
- **Naming issues:** none.
- **Design smells:** none directly in this code — see ADR-0006 above, which
  is about this system's *relationship* to the Llm/ entry, not a flaw here.
  The `claude-haiku-4-5`/`claude-sonnet-5` model IDs in `AdvisoryConfig.cs`
  match `specification.md`'s LLM Selection Policy table verbatim — this
  system is the documented target architecture, already built.
- **Confidence:** HIGH
- **Sources read:** `Advisory/AdvisoryPipeline.cs`, `AdvisoryConfig.cs`,
  `ChatClientRegistration.cs`, `LlmSelectionPolicy.cs`, `KnowledgeDepth.cs`,
  `DefaultAdvisoryAdapter.cs`, `SoftwareProductAdvisorAdapter.cs`.

## DI Composition Root  [KEEP]
- **What it is:** `ServiceCollectionExtensions` — five extension methods
  (`AddDekeInfrastructure`, `AddDekeEmbeddings`, `AddDekeHarvesters`,
  `AddDekeFederation`, `AddDekeAdvisory`) wiring every component above into
  `IServiceCollection`. (`AddDekeLlm` was the sixth method; retired by
  OP-008c per ADR-0007 — see the Llm — Gemini/OpenAI Backend entry above.)
- **Responsibility:** Owns the one place that maps every `Deke.Core`
  interface to its Infrastructure implementation — the ground truth for
  "what's actually registered," used throughout this region's entries to
  confirm 1:1 interface↔implementation mapping.
- **Key dependencies:** →every entry above.
- **Naming issues:** none.
- **Design smells:** none. Note: `ITrustScoringService` is registered twice
  (`AddDekeFederation` via `AddSingleton`, `AddDekeAdvisory` via
  `TryAddSingleton`) — deliberate, per its own inline comment ("Advisory
  depends on trust scoring; register it here too so the pipeline does not
  require AddDekeFederation to have run first"), not a duplication bug.
- **Confidence:** HIGH
- **Sources read:** `ServiceCollectionExtensions.cs` (full).

---

## Deke.Api

> `src/Deke.Api` — minimal-API REST host, one `<ProjectReference>` to
> `Deke.Infrastructure`. Small and flat: `Endpoints/` (four files, one per
> REST resource), `Auth/` (one handler), `Program.cs` (composition root).
> Entries below follow that structure 1:1.

## Fact Endpoints  [KEEP, see Design smells]
- **What it is:** `FactEndpoints.MapFactEndpoints` — `GET /api/facts/{id}`,
  `GET /api/facts/domain/{domain}` (paginated, limit clamped 1-500),
  `POST /api/facts` (generates the embedding inline via `IEmbeddingService`
  before storage, requires authorization), `GET /api/facts/stats/{domain}`.
- **Responsibility:** Owns the REST surface for `Deke.Core`'s Fact & Source
  Domain `Fact` type — create/read plus per-domain fact-count stats.
- **Key dependencies:** →Deke.Core Fact & Source Domain (`IFactRepository`,
  `Fact`); →Deke.Infrastructure Embeddings (`IEmbeddingService`, called
  directly here, not through a service layer).
- **Naming issues:** none.
- **Design smells:** none in the code itself, but a spec/code gap:
  `specification.md`'s Fact Endpoints table (the current-scope table, not the
  separately phase-gated "Planned Endpoints" table) documents
  `PUT /api/facts/{id}` (update) and `DELETE /api/facts/{id}` (soft-delete) —
  neither exists in this file. Logged to `PARKING-LOT.md` as an
  implementation gap, not escalated as an ADR (no design disagreement, just
  unbuilt).
- **Confidence:** HIGH
- **Sources read:** `Endpoints/FactEndpoints.cs` (full).

## Source Endpoints  [KEEP, see Design smells]
- **What it is:** `SourceEndpoints.MapSourceEndpoints` — `GET /api/sources`
  (optional domain filter), `GET /api/sources/{id}`, `POST /api/sources`
  (requires authorization, SSRF-guarded via a private `IsValidPublicUrl`
  check rejecting loopback/private-range/link-local hosts),
  `DELETE /api/sources/{id}`.
- **Responsibility:** Owns the REST surface for `Source` registration, plus
  the one input-validation guard in this project defending against a
  malicious source URL targeting internal infrastructure.
- **Key dependencies:** →Deke.Core Fact & Source Domain (`ISourceRepository`,
  `Source`, `SourceType`).
- **Naming issues:** none.
- **Design smells:** none in the code itself; same spec/code gap pattern as
  Fact Endpoints — `specification.md`'s current-scope Source Endpoints table
  documents `PUT /api/sources/{id}` (update configuration), not implemented
  here. Logged to `PARKING-LOT.md` alongside the Fact Endpoints gap (one
  entry, both gaps share the same root cause).
- **Confidence:** HIGH
- **Sources read:** `Endpoints/SourceEndpoints.cs` (full).

## Search Endpoints  [KEEP]
- **What it is:** `SearchEndpoints.MapSearchEndpoints` — `POST /api/search`,
  `POST /api/search/context`; both anonymous, both delegate to
  `IFederatedSearchService`, both parse optional inbound federation headers
  (`X-Federation-Hop-Count`/`-Query-Origin`/`-Visited`/`-Request-Id`) via a
  shared private `ParseFederationContext` helper.
- **Responsibility:** Owns the one search surface that is simultaneously
  anonymous and federation-aware — every request can carry or trigger
  cross-instance delegation.
- **Key dependencies:** →Deke.Core Federation (`IFederatedSearchService`,
  `FederationContext`) — the same interface
  `Deke.Infrastructure/Federation/FederatedSearchService.cs` implements.
- **Naming issues:** none.
- **Design smells:** none. Matches `specification.md`'s Search Endpoints
  table and federation header names verbatim.
- **Confidence:** HIGH
- **Sources read:** `Endpoints/SearchEndpoints.cs` (full).

## Federation Endpoints  [KEEP]
- **What it is:** `FederationEndpoints.MapFederationEndpoints` —
  `GET /api/federation/manifest` (builds a `FederationManifest` live from
  `FederationConfig` + `IFactRepository.GetDomainStatsAsync`),
  `GET`/`POST`/`DELETE /api/federation/peers`.
- **Responsibility:** Owns the peer-discovery/registration REST surface; the
  manifest endpoint is the one place `DomainStats` round-trips into an HTTP
  response.
- **Key dependencies:** →Deke.Core Federation (`IFederationPeerRepository`,
  `FederationManifest`, `FederationConfig`); →Deke.Core Fact & Source Domain
  (`IFactRepository.GetDomainStatsAsync`).
- **Naming issues:** none new — touches the `DomainStats` file-location nit
  already logged in `PARKING-LOT.md` (OP-004a); not re-logged.
- **Design smells:** none. Hardcoded `Version = "1.0.0"` / `Capabilities =
  ["search"]` match a fixed single-capability instance, consistent with
  current scope.
- **Confidence:** HIGH
- **Sources read:** `Endpoints/FederationEndpoints.cs` (full).

## API Key Authentication  [KEEP]
- **What it is:** `ApiKeyAuthHandler` — the sole `AuthenticationHandler`,
  checking an `X-Api-Key` request header against configuration key
  `"ApiKey"` under scheme name `"ApiKey"`, registered as the app's only
  authentication scheme with an `AuthenticatedOnly` fallback policy
  (`RequireAuthenticatedUser`).
- **Responsibility:** Owns the one gate in front of every
  `RequireAuthorization()` endpoint across Fact/Source/Federation Endpoints.
- **Key dependencies:** consumed by `Program.cs`'s `AddAuthentication`
  registration; gates the write endpoints in the three entries above.
- **Naming issues:** none.
- **Design smells:** none — **resolved by OP-008d**, implementing ADR-0008's
  accepted resolution. `Program.cs` now throws `InvalidOperationException`
  at startup when configuration key `"ApiKey"` is empty or missing (checked
  right after the connection-string check, before `AddDekeInfrastructure`
  runs), so an unconfigured key can no longer reach request time in any
  environment. `HandleAuthenticateAsync`'s `NoResult()` "allow all
  (development mode)" branch — previously misleading, since `NoResult()`
  actually left every `RequireAuthorization()` endpoint rejected rather than
  allowed — has been removed; the handler now goes straight to its
  header-check/Fail/Success logic, since the startup check guarantees
  `configuredKey` is always set. Verified (not just code-reviewed): `dotnet
  run` with no `"ApiKey"` configured fails fast with a clear error naming the
  missing key; with `ApiKey` set, `/health` (anonymous) returns 200 and
  `POST /api/sources` without/with a wrong `X-Api-Key` header returns 401.
  Top-level `CLAUDE.md`'s Quick Start and curl examples updated to match.
- **Confidence:** HIGH
- **Sources read:** `Auth/ApiKeyAuthHandler.cs` (full); `appsettings.json`
  (`ApiKey` key only); `Program.cs` (auth registration + new startup check).

## Host Composition (Program.cs)  [KEEP]
- **What it is:** The `WebApplication` builder/pipeline — Serilog, a
  gitignored local `appsettings.{env}.local.json` override, the `Deke`
  connection string, three of `Deke.Infrastructure`'s five DI extension
  methods (`AddDekeInfrastructure`, `AddDekeEmbeddings`, `AddDekeFederation`
  — not `AddDekeHarvesters` or `AddDekeAdvisory`), the `ApiKey` auth scheme +
  `AuthenticatedOnly` fallback policy, OpenAPI, an anonymous `/health` check,
  and the four `MapXEndpoints()` calls.
- **Responsibility:** Owns the process entry point and this host's specific
  slice of the DI Composition Root — which of Infrastructure's five extension
  methods a REST-API process actually needs.
- **Key dependencies:** →all four Endpoints entries above; →API Key
  Authentication; →Deke.Infrastructure DI Composition Root (calls 3 of its 5
  methods).
- **Naming issues:** none.
- **Design smells:** none. `AddDekeHarvesters` and `AddDekeAdvisory` are
  deliberately absent — harvesting is `Deke.Worker`'s job (background
  service) and Advisory is `Deke.Mcp`'s job (confirmed via repo-wide grep:
  `IAdvisoryPipeline`/`AdviseAsync` have zero references anywhere under
  `src/Deke.Api`). This matches top-level `CLAUDE.md`'s REST-API-vs-MCP-Server
  project split; not a gap.
- **Confidence:** HIGH
- **Sources read:** `Program.cs` (full).

---

## Deke.Mcp

> `src/Deke.Mcp` — MCP server host (stdio transport), one
> `<ProjectReference>` to `Deke.Core` and one to `Deke.Infrastructure`
> (`Deke.Mcp.csproj`). Small and flat: `Tools/` (three files, one
> `[McpServerToolType]` class each — `FactTools`, `SearchTools`,
> `AdvisoryTools`), `Program.cs` (composition root). No `Auth/` directory —
> stdio has no HTTP surface to gate, unlike `Deke.Api`. Entries below follow
> that structure 1:1.

## Fact Tools  [KEEP, see Design smells]
- **What it is:** `FactTools` — three MCP tools: `add_fact` (embeds content
  via `IEmbeddingService`, stores via `IFactRepository.AddAsync`), `get_fact`
  (by GUID string), `get_domain_stats` (fact count for a domain via
  `IFactRepository.GetCountAsync`).
- **Responsibility:** Owns the write/read-by-id MCP surface for `Deke.Core`'s
  Fact & Source Domain — the tool-calling equivalent of `Deke.Api`'s Fact
  Endpoints, for an MCP client (e.g. Claude Code) rather than REST.
- **Key dependencies:** →Deke.Core Fact & Source Domain (`IFactRepository`,
  `Fact`); →Deke.Infrastructure Embeddings (`IEmbeddingService`, called
  directly here, same pattern as `Deke.Api/Endpoints/FactEndpoints.cs`).
- **Naming issues:** none against `docs/GLOSSARY.md`'s canonical terms — none
  of Evolution Engine / P1-N / IChunker-SemanticChunkerAdapter appear
  anywhere in `src/Deke.Mcp`.
- **Design smells:** none in the code itself, but a spec/code gap: none of
  `add_fact`, `get_fact`, `get_domain_stats` appear in `specification.md`'s
  "MCP Tools — Current (implemented)" table (`specification.md:384-393`
  lists only `consult_domain_expert`, `get_context`, `list_available_domains`,
  `GetDomainAdvice`), and no "Planned" MCP Tools section exists to hold them
  either. Logged to `PARKING-LOT.md` as an implementation-ahead-of-spec gap,
  not escalated as an ADR (no design disagreement, just undocumented).
- **Confidence:** HIGH
- **Sources read:** `Tools/FactTools.cs` (full); `docs/architecture/
  specification.md:369-395` (MCP Tools section, cross-check only).

## Search Tools  [KEEP]
- **What it is:** `SearchTools` — three MCP tools: `consult_domain_expert`
  (semantic search via `IFederatedSearchService.SearchAsync`, `federation`
  argument always `null`), `get_context` (same service's `GetContextAsync`,
  LLM-formatted context block), `list_available_domains` (lists local +
  federated-peer domains).
- **Responsibility:** Owns the MCP-native search/discovery surface — the
  tool-calling equivalent of `Deke.Api`'s Search Endpoints, minus inbound
  federation-header parsing (not applicable; MCP has no HTTP headers, so
  these calls always originate fresh, never relay an in-flight federated
  request).
- **Key dependencies:** →Deke.Core Federation (`IFederatedSearchService`,
  same interface `Deke.Infrastructure/Federation/FederatedSearchService.cs`
  implements; `IFederationPeerRepository.GetHealthyAsync`); →Deke.Core Fact &
  Source Domain (`ISourceRepository.GetAllAsync` and
  `IFactRepository.GetDomainStatsAsync`, both for local domain discovery —
  see Design smells).
- **Naming issues:** none against `docs/GLOSSARY.md`'s canonical terms. Tool
  names here are consistently snake_case (`consult_domain_expert`,
  `get_context`, `list_available_domains`) — the sole exception across this
  whole project is `AdvisoryTools.GetDomainAdvice` (see that entry); noted
  here for completeness, not re-logged.
- **Design smells:** none — **resolved by OP-008e**, implementing ADR-0009's
  accepted resolution. `ListAvailableDomains` now takes an added
  `IFactRepository factRepository` parameter and builds its "Local Domains"
  list from a union of `ISourceRepository.GetAllAsync()`-derived domains and
  `IFactRepository.GetDomainStatsAsync()`-derived domains, mirroring the
  pattern `Deke.Api`'s Federation manifest endpoint already used (per this
  file's Federation Endpoints entry). Each domain line now shows source
  count and/or fact count depending on what's available; a fact-only domain
  (created via top-level `CLAUDE.md`'s documented zero-config `add_fact`
  workflow, no `SourceId` ever registered) is labeled "no registered
  source" instead of being omitted. Verified (not just code-reviewed):
  a fact inserted under a brand-new domain with `SourceId = null` did not
  appear in `list_available_domains`'s output before the fix, and appeared
  afterward as `**domain** (1 fact(s), no registered source)`, against the
  live Postgres instance.
- **Confidence:** HIGH
- **Sources read:** `Tools/SearchTools.cs` (full, post-fix);
  `src/Deke.Core/Interfaces/IFactRepository.cs` (`GetDomainStatsAsync`
  signature, re-confirmed); cross-referenced against this file's own
  Federation Endpoints entry (`Deke.Api` region, above); ADR-0009
  (accepted, resolution recorded 2026-07-07).

## Advisory Tools  [KEEP, see Naming issues]
- **What it is:** `AdvisoryTools` — one MCP tool, `GetDomainAdvice`, the
  MCP-native entry point to `Deke.Core`'s Advisory Pipeline Contract
  (`IAdvisoryPipeline.AdviseAsync`).
- **Responsibility:** Owns exposing a grounded, cited, confidence-banded
  advisory answer (citations, knowledge gaps, conflicting-evidence flag,
  interaction id) as markdown for an MCP client.
- **Key dependencies:** →Deke.Core Advisory Pipeline Contract
  (`IAdvisoryPipeline`, `AdvisoryRequest`/`AdvisoryResponse`, `Stakes`) — the
  same contract `Deke.Infrastructure`'s Advisory Pipeline Implementation
  entry fulfills.
- **Naming issues:** `GetDomainAdvice` is PascalCase; every other MCP tool
  name in this project (`add_fact`, `get_fact`, `get_domain_stats`,
  `consult_domain_expert`, `get_context`, `list_available_domains`) is
  snake_case. Not a `docs/GLOSSARY.md` tie (no canonical-term collision) — a
  bare naming-convention inconsistency within the MCP tool surface itself.
  `specification.md` documents it as `GetDomainAdvice` too, so this isn't a
  doc/code mismatch. Logged to `PARKING-LOT.md` as a style nit.
- **Design smells:** none. The `sessionId` parameter is honestly documented
  as "currently unused" — not a doc/code mismatch.
- **Confidence:** HIGH
- **Sources read:** `Tools/AdvisoryTools.cs` (full); `docs/architecture/
  specification.md:393` (cross-check only).

## Host Composition (Program.cs)  [KEEP]
- **What it is:** The `Host` builder — user-secrets loaded unconditionally
  (not gated to Development, so an Anthropic API key set via `dotnet
  user-secrets` is picked up when run as an MCP server), Serilog, the `Deke`
  connection string, four of `Deke.Infrastructure`'s five DI extension
  methods (`AddDekeInfrastructure`, `AddDekeEmbeddings`, `AddDekeFederation`,
  `AddDekeAdvisory` — not `AddDekeHarvesters`), and
  `AddMcpServer().WithStdioServerTransport()` registering all three `Tools/`
  classes.
- **Responsibility:** Owns the process entry point and this host's specific
  slice of the DI Composition Root — an MCP server process reachable over
  stdio, not HTTP, so no authentication scheme (unlike `Deke.Api`'s API Key
  Authentication) is needed or present.
- **Key dependencies:** →all three Tools entries above; →Deke.Infrastructure
  DI Composition Root (calls 4 of its 5 methods).
- **Naming issues:** none.
- **Design smells:** none. Previously this entry logged a pre-existing
  cross-project DI-hygiene nit — `AddDekeLlm` registered here but unused by
  any `src/Deke.Mcp` code, the Gemini/OpenAI backend actually being consumed
  solely by `Deke.Worker/PatternDiscoveryService.cs`. That nit is resolved:
  OP-008c (per ADR-0007, accepted) deleted `AddDekeLlm` and the `Llm/`
  system it registered entirely — see the Llm — Gemini/OpenAI Backend entry
  in the `Deke.Infrastructure` region above. Nothing remains to log.
- **Confidence:** HIGH
- **Sources read:** `Program.cs` (full); `Deke.Mcp.csproj` (full);
  `appsettings.json`, `appsettings.Development.json` (full, both small).

---

## Deke.Worker

> `src/Deke.Worker` — background-services host (`Microsoft.NET.Sdk.Worker`),
> `<ProjectReference>`s to `Deke.Core` and `Deke.Infrastructure`. Small and
> flat: `Services/` (five files — four `BackgroundService` subclasses plus one
> plain injectable class), `Program.cs` (composition root, also doubles as a
> one-shot CLI entry point via `--bootstrap`). Entries below follow that
> structure 1:1.

## Source Monitoring  [KEEP]
- **What it is:** `SourceMonitorService` — a `BackgroundService` that polls
  every 15 minutes for `Source`s due for a check, harvests each via the
  matching `IHarvester`, and on content change runs the full
  chunk → extract → embed → store pipeline.
- **Responsibility:** Owns the recurring "keep registered sources fresh" job —
  the only place `ISourceRepository.GetDueForCheckAsync` is called.
- **Key dependencies:** →Deke.Core Fact & Source Domain (`ISourceRepository`,
  `IFactRepository`, `Fact`); →Deke.Infrastructure Harvesters (`IHarvester`,
  keyed by `SourceType`); →Deke.Infrastructure Extraction (`IChunker`,
  `IExtractionService`); →Deke.Infrastructure Embeddings (`IEmbeddingService`).
- **Naming issues:** confirms `docs/GLOSSARY.md`'s `IChunker`/
  `SemanticChunkerAdapter` row ("consumed by two Worker services",
  namingissues.md #5) — this is one of the two; see Bootstrap Ingestion below
  for the other. Both now positively identified, not re-escalated.
- **Design smells:** none directly. Its harvest→chunk→extract→embed→store
  inner loop (`CheckSourcesAsync`) is near-duplicated in Bootstrap Ingestion's
  `IngestPathAsync` — logged once, in that entry, to `PARKING-LOT.md`.
- **Confidence:** HIGH
- **Sources read:** `Services/SourceMonitorService.cs` (full).

## Bootstrap Ingestion  [KEEP, see Design smells]
- **What it is:** `BootstrapIngestionService` — not a `BackgroundService`; a
  plain class run once via `Program.cs`'s `--bootstrap` CLI branch. Ingests
  `docs/` and `thoughts/` from a given repo root through the file harvester
  into a fixed `"software-product"` domain at a fixed 0.95 confidence, then
  exits (`host.Run()` is never reached on this path).
- **Responsibility:** Owns DEKE's self-referential seed: turning its own
  design docs and RPI-workflow artifacts into the first facts for the
  Software Product Advisor domain, per `docs/product/knowledge-leverage.md`'s
  "DEKE advises on its own development" framing.
- **Key dependencies:** →Deke.Infrastructure Harvesters (`IHarvester`,
  `SourceType.File` only); same Extraction/Embeddings dependencies as Source
  Monitoring.
- **Naming issues:** confirms the `IChunker`/`SemanticChunkerAdapter`
  GLOSSARY.md row's "two Worker services" — this is the second, alongside
  Source Monitoring above.
- **Design smells:** none rising to ADR level — behavior is correct and
  intentional (a one-shot CLI seed step, not a background job, hence the
  bare-class-not-`BackgroundService` shape). Two cross-doc gaps logged to
  `PARKING-LOT.md` instead: (1) `docs/ROADMAP.md`'s "Not yet built" list and
  MVP table (lines 17, 31) both mark bootstrap ingestion "Planned" — it is
  fully implemented; `specification.md`'s project-structure tree
  (`specification.md:65-66`) lists only 2 of `Deke.Worker`'s 4
  `BackgroundService`s and omits this class entirely. (2) This class's
  harvest→chunk→extract→embed→store loop (`IngestPathAsync`) duplicates
  Source Monitoring's `CheckSourcesAsync` almost line-for-line — no shared
  helper between the two.
- **Confidence:** HIGH
- **Sources read:** `Services/BootstrapIngestionService.cs` (full);
  `Program.cs` (`--bootstrap` branch); `docs/product/knowledge-leverage.md`
  (bootstrap framing, cross-check only); `docs/ROADMAP.md`,
  `docs/architecture/specification.md:55-69` (cross-check only).

## Pattern Discovery  [KEEP]
- **What it is:** `PatternDiscoveryService` — hourly `BackgroundService`
  cycle: per active domain, pulls the last 7 days of facts, clusters them by
  pairwise cosine similarity (>0.8, simple greedy grouping, not true
  union-find despite the method's framing), skips clusters an existing
  `Pattern` already covers, and persists a new `Pattern` per novel cluster —
  optionally LLM-summarized via `ILlmService` when available, else a
  templated description. Always writes one `LearningLog` row per domain per
  cycle.
- **Responsibility:** Owns turning recent fact clusters into `Pattern`
  records — the Term & Pattern Domain's write path.
- **Key dependencies:** →Deke.Core Term & Pattern Domain (`IPatternRepository`,
  `Pattern`, `ILearningLogRepository`); →Deke.Core Fact & Source Domain
  (`IFactRepository.GetRecentAsync`); →Deke.Infrastructure Advisory Pipeline
  Implementation (keyed `IChatClient`, resolved via `AdvisoryClientKeys.Ollama`)
  — replaces the retired Llm — Gemini/OpenAI Backend entry above, per
  ADR-0007/OP-008c.
- **Naming issues:** none.
- **Design smells:** none newly found — the `ILlmService` dependency is
  already covered by ADR-0006 (proposed → accepted) and ADR-0007 (accepted,
  spawns OP-008c), not re-escalated here.
- **Confidence:** HIGH
- **Sources read:** `Services/PatternDiscoveryService.cs` (full).

## Relation Mapping (Learning Cycle)  [KEEP]
- **What it is:** `LearningCycleService` — a 2-hour `BackgroundService` cycle:
  per active domain, pulls up to 50 facts lacking relations, vector-searches
  each for similar facts (similarity > 0.7), and adds a `"related"`
  `FactRelation` edge in both directions' absence (checks both directions
  before inserting, so edges are effectively undirected despite the
  `FromFactId`/`ToFactId` shape). Writes one `LearningLog` row per domain per
  cycle, same pattern as Pattern Discovery.
- **Responsibility:** Owns turning embedding similarity into persisted
  `FactRelation` edges — the Fact & Source Domain's relation-graph write path.
- **Key dependencies:** →Deke.Core Fact & Source Domain (`IFactRepository`,
  `IFactRelationRepository`, `FactRelation`).
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `Services/LearningCycleService.cs` (full).

## Peer Health Check  [KEEP]
- **What it is:** `PeerHealthCheckService` — a 5-minute `BackgroundService`
  cycle, no-ops entirely when `FederationConfig.Enabled` is false. Fetches
  each configured peer's `/api/federation/manifest` via a named `"federation"`
  `HttpClient`, upserts a healthy `FederationPeer` row on success or an
  unhealthy one (on null manifest or any exception) — never throws out of the
  per-peer loop.
- **Responsibility:** Owns keeping `federation_peers` health/manifest data
  fresh — the write path `docs/architecture/federation.md` describes as
  populating that table on startup and thereafter.
- **Key dependencies:** →Deke.Core Federation (`IFederationPeerRepository`,
  `FederationPeer`, `FederationConfig`) — the named `"federation"` HttpClient
  itself is registered elsewhere (`Deke.Infrastructure`'s `AddDekeFederation`,
  not in this project).
- **Naming issues:** none.
- **Design smells:** none. Matches `docs/architecture/federation.md`'s
  description verbatim (peer-manifest fetch on startup and on interval,
  sustained-failure-based unhealthy marking, not single-timeout-based).
- **Confidence:** HIGH
- **Sources read:** `Services/PeerHealthCheckService.cs` (full);
  `docs/architecture/federation.md` (PeerHealthCheckService mentions,
  cross-check only).

## Host Composition (Program.cs)  [KEEP]
- **What it is:** A dual-mode entry point. Normal mode: Serilog, the `Deke`
  connection string, four of `Deke.Infrastructure`'s five DI extension methods
  (`AddDekeInfrastructure`, `AddDekeEmbeddings`, `AddDekeHarvesters`,
  `AddDekeFederation` — not the full `AddDekeAdvisory`), plus a direct call to
  `AddAdvisoryChatClients` (`Advisory/ChatClientRegistration.cs` — the same
  keyed-`IChatClient` registration `AddDekeAdvisory` itself calls internally,
  registering both the `anthropic` and `ollama` keyed backends) so
  `PatternDiscoveryService` can resolve the keyed `ollama` `IChatClient`
  without pulling in the rest of `AddDekeAdvisory`'s registrations
  (`ITrustScoringService`, `ILlmSelectionPolicy`,
  `IAdvisoryInteractionRepository`, `IAdvisoryAdapter`, `IAdvisoryPipeline`) —
  this narrower wiring replaces the retired `AddDekeLlm` call, per
  OP-008c/ADR-0007. It then registers all four `BackgroundService`s as hosted
  services and calls `host.Run()`. `--bootstrap <path>` mode: builds the
  host, resolves `BootstrapIngestionService` from a fresh DI scope, runs it
  once against the given (or current) directory, and returns — no hosted
  services ever start.
- **Responsibility:** Owns the process entry point and this host's specific
  slice of the DI Composition Root, including the branch that decides whether
  this run is a long-lived worker or a one-shot bootstrap CLI invocation.
- **Key dependencies:** →all five `Services/` entries above; →Deke.Infrastructure
  DI Composition Root (calls 4 of its 5 methods, plus a direct call to
  `AddAdvisoryChatClients`, a piece of `AddDekeAdvisory`'s own registration,
  not one of the Composition Root's five methods itself).
- **Naming issues:** none.
- **Design smells:** none. The full `AddDekeAdvisory` is deliberately absent
  — no file under `src/Deke.Worker` references `IAdvisoryPipeline` (confirmed
  by direct read of all five `Services/` files); only the keyed-`IChatClient`
  slice it depends on (`AddAdvisoryChatClients`) is pulled in directly, for
  `PatternDiscoveryService`'s Ollama-backed summarization (see that entry,
  above). This is a narrower, partial wiring — not simply "absent" the way
  `Deke.Api`'s and `Deke.Mcp`'s Host Composition entries' advisory-
  registration split is a plain present/absent call to the full method.
- **Confidence:** HIGH
- **Sources read:** `Program.cs` (full); `Deke.Worker.csproj` (full).

---

## tests/Deke.Tests

> `tests/Deke.Tests` — xUnit 2.9.2 project (`Deke.Tests.csproj`), `<ProjectReference>`
> to `Deke.Core`, `Deke.Infrastructure`, `Deke.Worker` only — no reference to
> `Deke.Api` or `Deke.Mcp` (see Design smells below). 9 test files, no
> subdirectories, no shared test-helpers/fixtures file — every fake
> dependency is a private nested class inside the test file that uses it.
> Entries below group the 9 files by which production region they exercise,
> matching this map's existing region boundaries rather than 1:1 by file.

## Federation & Search Contract Model Tests  [KEEP]
- **What it is:** `FederationTests.cs` + `FederatedSearchTests.cs` — pure
  construction/default-value tests for `Deke.Core`'s federation and search
  DTOs (`FederationPeer`, `FederationConfig`, `FederationManifest`,
  `DomainStats`, `FederatedSearchRequest`, `FederatedContextRequest`,
  `FederationContext`, `ResultProvenance`, `FederationMetadata`,
  `FactSearchResult`, `SearchResponse`, `ContextResponse`), plus
  `FederationConfig.GetLocalityWeight`'s hop-decay lookup.
- **Responsibility:** Owns regression coverage for every federation/search
  DTO's default values and the locality-weight formula that
  `ITrustScoringService.Score()` consumes as its `localityWeight` argument.
- **Key dependencies:** →Deke.Core Federation (models under test); →Deke.Core
  Search & Trust Contracts (`FactSearchResult`/`SearchResponse`/
  `ContextResponse` extension fields under test).
- **Naming issues:** none.
- **Design smells:** none. `LocalityWeightTests`'s fallback-hop assertion
  (0.5 beyond configured hops) reconfirms **ADR-0005** (federation ranking
  formula doc/code drift, already proposed) — not re-escalated.
- **Confidence:** HIGH
- **Sources read:** `FederationTests.cs` (full), `FederatedSearchTests.cs`
  (full).

## Semantic Chunking Tests  [KEEP]
- **What it is:** `SemanticChunkerAdapterTests.cs` — 4 tests against
  `SemanticChunkerAdapter` (`IChunker`), using a hand-written
  `TopicEmbeddingGenerator` fake that clusters embeddings by keyword count so
  a real topic-transition breakpoint fires deterministically without a live
  embedding model.
- **Responsibility:** Owns regression coverage for chunk-boundary behavior:
  empty/whitespace input, single-sentence input, and multi-topic text
  producing more than one coherent chunk.
- **Key dependencies:** →Deke.Infrastructure Extraction
  (`SemanticChunkerAdapter` under test).
- **Naming issues:** none — confirms `docs/GLOSSARY.md`'s
  `IChunker`/`SemanticChunkerAdapter` row is testing the real, approved class;
  not re-logged.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `SemanticChunkerAdapterTests.cs` (full).

## Trust Scoring Tests  [KEEP]
- **What it is:** `TrustScoringTests.cs` — 9 tests directly against
  `TrustScoringService.Score()`, covering each of its 5 factors (similarity,
  confidence, source credibility, validity window, recency decay) plus the
  neutral-fallback branch for zero/negative source credibility (the
  federated-result case).
- **Responsibility:** Owns regression coverage for the concrete ranking
  formula every search path calls through `ITrustScoringService` — the same
  formula **ADR-0005** is about.
- **Key dependencies:** →Deke.Infrastructure Trust (`TrustScoringService`
  under test).
- **Naming issues:** none.
- **Design smells:** none newly found — `Score()`'s exact parameter list
  (similarity, confidence, sourceCredibility, validFrom/validUntil,
  localityWeight, now) substantiates ADR-0005 yet again; not re-escalated.
- **Confidence:** HIGH
- **Sources read:** `TrustScoringTests.cs` (full).

## Federated Search Interaction Logging Tests  [KEEP, see Design smells]
- **What it is:** `InteractionLoggingTests.cs` — 2 tests against
  `FederatedSearchService.SearchAsync`, verifying an `InteractionLog` is
  written for a top-level (non-relayed) query and specifically *not* written
  when the call carries a `FederationContext` with a non-empty `QueryOrigin`
  (a relayed peer query).
- **Responsibility:** Owns regression coverage for the audit-log write/skip
  branch — the one place `FederatedSearchService` decides whether a search is
  this instance answering a fresh question versus relaying a peer's
  in-flight federated search.
- **Key dependencies:** →Deke.Infrastructure Federation (Infrastructure)
  (`FederatedSearchService` under test); →Deke.Infrastructure Trust (real
  `TrustScoringService` instance used, not faked).
- **Naming issues:** none.
- **Design smells:** none in the code under test. The test file itself never
  exercises the actual peer-delegation path — `FakeFederationPeerRepository.
  GetHealthyAsync` throws `NotImplementedException`, so both tests only cover
  the local-only branch of `SearchAsync`, never `ShouldFederate`, parallel
  peer querying, or result merging. Logged to `PARKING-LOT.md` as a coverage
  gap, not a design smell.
- **Confidence:** HIGH
- **Sources read:** `InteractionLoggingTests.cs` (full).

## Bootstrap Ingestion Tests  [KEEP]
- **What it is:** `BootstrapIngestionTests.cs` — 2 tests against
  `BootstrapIngestionService.RunAsync`, using a real `FileSystemHarvester`
  and `SimpleExtractionService` against a temp directory, an identity
  `IChunker` fake (returns input unchunked), and fake source/fact
  repositories.
- **Responsibility:** Owns regression coverage for the "ingest `docs/` and
  `thoughts/` into the `software-product` domain at high confidence" seed
  behavior, plus a same-content-twice idempotency check.
- **Key dependencies:** →Deke.Worker Bootstrap Ingestion
  (`BootstrapIngestionService` under test); →Deke.Infrastructure Harvesters
  (real `FileSystemHarvester`, not faked).
- **Naming issues:** none.
- **Design smells:** none.
- **Confidence:** HIGH
- **Sources read:** `BootstrapIngestionTests.cs` (full).

## Advisory Pipeline & Model Selection Tests  [KEEP]
- **What it is:** Four files together: `AdvisoryPipelineTests.cs` (5 tests,
  full `AdvisoryPipeline.AdviseAsync` orchestration via a `FakeChatClient`
  registered under both the Anthropic and Ollama keyed `IChatClient` slots),
  `KnowledgeDepthTests.cs` (`KnowledgeDepth.Compute`/`Band` — the depth
  formula and its 4-band cutoffs), `LlmSelectionPolicyTests.cs`
  (`LlmSelectionPolicy.Select`'s model-override/Ollama-eligibility/
  stakes-escalation branches), `SoftwareProductAdvisorAdapterTests.cs`
  (`SoftwareProductAdvisorAdapter`'s activation criteria, system prompt,
  fact weighting, and context formatting).
- **Responsibility:** Owns regression coverage for the whole Advisory
  Pipeline Implementation region: the 7-stage pipeline, the knowledge-depth
  formula that feeds model selection, the selection policy itself, and the
  one built-in domain adapter.
- **Key dependencies:** →Deke.Infrastructure Advisory Pipeline Implementation
  (all four classes under test); →Deke.Infrastructure Trust (real
  `TrustScoringService`, not faked, used by both `AdvisoryPipelineTests` and
  `KnowledgeDepthTests`).
- **Naming issues:** none.
- **Design smells:** none. `AdvisoryPipelineTests.BuildPipeline` registers
  one `FakeChatClient` under both `AdvisoryClientKeys.Anthropic` and
  `AdvisoryClientKeys.Ollama` — deliberate: these tests exercise pipeline
  orchestration, not backend routing; `LlmSelectionPolicyTests` separately
  and directly tests which key gets selected. Not a test bug.
- **Confidence:** HIGH
- **Sources read:** `AdvisoryPipelineTests.cs` (full), `KnowledgeDepthTests.cs`
  (full), `LlmSelectionPolicyTests.cs` (full),
  `SoftwareProductAdvisorAdapterTests.cs` (full).
