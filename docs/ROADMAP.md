# Roadmap

DEKE's planned work, expressed as sized work packets in a dependency DAG.
Every sized packet lists its dependencies, context budget, tool budget, and
recommended tier (model classes per [TOOLING.md](TOOLING.md), Model & thinking
tiers). Work that cannot be honestly sized today is trigger-gated: it enters
the DAG through a sized research/sizing packet, or waits for an explicit
trigger condition.

Reading rules:

- ALWAYS-load for any packet: [GLOSSARY.md](GLOSSARY.md), this file, and the
  [PROJECT-MAP.md](PROJECT-MAP.md) entries named in the packet's context
  budget.
- Design or feature problems found mid-packet are escalated as proposed ADRs
  ([adr/](adr/)), never redesigned inline.
- Packet IDs reuse canonical phase shorthand where it exists (`P1-N`, `R2`-`R8`,
  `EE-N` — see [GLOSSARY.md](GLOSSARY.md)); work without a phase label uses
  `HYG-n` (hygiene), `RES-n` (research), `FED-n` (federation), `DOM-n`
  (domains).

## Current State (2026-07-14)

The MVP — DEKE answering domain questions better than a language model alone,
with cited and confidence-scored facts — is **delivered in full**. All eight
MVP work items (trust layer, semantic chunking, advisory contracts, advisory
pipeline, Software Product Advisor, GetDomainAdvice MCP tool, bootstrap
ingestion, interaction logging) are done.

| Capability | Status |
|---|---|
| **Knowledge Base (v1)** | Fact ingestion from RSS and web pages. Semantic chunking (`IChunker`/`SemanticChunkerAdapter`). Semantic search via 384-dim embeddings (pgvector, IVFFlat). Source monitoring, pattern discovery, relation mapping. URL + content-hash deduplication. Bootstrap self-ingestion (`--bootstrap`) into the `software-product` domain. |
| **Trust layer (simplified)** | `credibility` on sources, `confidence` on facts, `valid_from`/`valid_until` temporal validity. Five-factor trust scoring: similarity × confidence × credibility × recency decay × locality weight. |
| **REST API** | Facts, sources, search (POST), federation endpoints. API-key auth on write endpoints; fail-fast startup when `ApiKey` is unconfigured. |
| **MCP Tools** | `consult_domain_expert`, `get_context`, `list_available_domains` (source- and fact-derived domains), `add_fact`, `get_fact`, `get_domain_stats`, `GetDomainAdvice`. |
| **Federation (Phases 1–3)** | Manifest-driven peer discovery. Federated search with delegation, provenance, loop prevention, locality-weighted scoring. MCP tool surface. |
| **Advisory Pipeline (MVP)** | 7-stage knowledge-leverage pipeline: grounded, confidence-banded responses with cited facts and knowledge-gap disclosure. Software Product Advisor adapter. Model routing across Anthropic (haiku/sonnet) and Ollama via `IChatClient`. Append-only `advisory_interactions` audit table. |
| **Background Services** | Source monitor (15 min), pattern discovery (1 hr, `IChatClient`-backed), learning cycle (2 hr), peer health check (5 min). |
| **Infrastructure & CI** | .NET 9, PostgreSQL 16 + pgvector, ONNX embeddings (all-MiniLM-L6-v2), Dapper, Polly. CI: format check → glossary lint → build → tests. |

## The DAG at a Glance

```
NOW (no unmet dependencies, parallelizable)
  P1-1  trust & provenance schema      ← first priority
  RES-1 vectorless RAG research        ← parallel, read-only
  HYG-1 doc-sync sweep
  HYG-2 missing REST endpoints         ← done 2026-07-14
  HYG-3 Api/Mcp test coverage

NEXT (dependencies above)
  P1-1 ──→ R2   five-level deduplication
  P1-1 ──→ P1-2 quality pipeline
  RES-1 ─→ (spawned design packet: hybrid retrieval)
  R4   cross-encoder re-ranking        (R1 done; consult RES-1 findings)
  R5   PDF + Markdown ingestion        (R1 done)
  EE1-DESIGN  Evolution Engine entry: EE-1 design + sizing

LATER (sized, lower priority)
  R7 ──→ P1-4   embedding abstraction, then multilingual swap
  R1–R4 ──→ R8  evaluation harness
  P1-3 ──→ R3   terminology database, then query expansion
  FED-4, FED-5  selective replication; trust federation
  DOM-2         second domain adapter

TRIGGER-GATED (sized only when the trigger fires)
  R6, EE-2..EE-5 + Curiosity Service, OI-01..OI-10
```

## Sized Packets — Ready Now

### P1-1: Trust and provenance schema — first priority

- **Goal:** Implement the planned provenance/trust schema (planned tables plus
  source/fact column additions, [specification.md](architecture/specification.md)
  Planned Tables section) before more data accumulates.
- **Why first:** the 2026-03 decision log calls P1-1 the critical path —
  schema changes after data are expensive, and bootstrap ingestion has
  already run.
- **Depends on:** nothing. **Unblocks:** R2, P1-2.
- **Context budget:** specification.md Planned Tables / Planned Source & Fact
  Additions; `init.sql`; PROJECT-MAP: Data Access & Type Handlers,
  Repositories, Trust.
- **Tool budget:** serena, roslyn, postgres-mcp, dotnet SDK, git.
- **Tier:** strongest available, extended thinking (schema decisions are
  design work; implementation may hand off to Sonnet-class).
- **Done when:** `init.sql` and models/repositories updated and applied;
  build + tests pass; specification.md's planned-schema sections moved to
  current (via doc-maintainer).

### RES-1: Vectorless RAG and hybrid retrieval research (ADR-0010)

- **Goal:** Survey vectorless retrieval (including BM25/lexical hybrid — the
  old roadmap's separate "hybrid search" line is folded in here) and propose
  what a hybrid model means for DEKE's pgvector-based search. Research only;
  no code.
- **Depends on:** nothing; runs in parallel with anything. Findings should
  land before R4/R6 investment decisions.
- **Context budget:** [ADR-0010](adr/ADR-0010-vectorless-rag-hybrid-research-direction.md);
  [retrieval-pipeline.md](architecture/retrieval-pipeline.md); PROJECT-MAP:
  Search & Trust Contracts, Fact & Source Domain.
- **Tool budget:** web search/fetch, serena (read-only).
- **Tier:** strongest available, extended thinking.
- **Done when:** research memo at `docs/science/vectorless-rag-research.md`
  (via doc-maintainer); a spawned, sized design packet appended to this DAG
  (or a documented recommendation not to proceed).

### HYG-1: Doc-sync sweep

- **Goal:** Fix the six known doc/code mismatches: `FactTools`' three MCP
  tools missing from specification.md's MCP Tools table; bootstrap ingestion
  still absent from specification.md's project-structure tree; top-level
  CLAUDE.md's search example says GET where the endpoint is POST; README.md +
  CLAUDE.md never adopted the package model; dangling root SPECIFICATION.md
  reference in specification.md; CLAUDE.md's add-source example sends
  `"type":"Rss"` but `SourceType` has no string-enum JSON converter, so the
  documented call 400s (fix the doc or add the converter — decide at
  execution).
- **Depends on:** nothing.
- **Context budget:** the named sections only; PARKING-LOT.md entries dated
  2026-07-07/14 for each item.
- **Tool budget:** doc-maintainer (all edits), grep for verification.
- **Tier:** Sonnet-class, default.
- **Done when:** each item grep-verifiably fixed; glossary lint passes.

### HYG-2: Missing REST endpoints — done (2026-07-14)

- **Goal:** Implement the three endpoints specification.md documents as
  current scope but which do not exist: `PUT /api/facts/{id}`,
  `DELETE /api/facts/{id}` (soft delete), `PUT /api/sources/{id}`.
- **Depends on:** nothing.
- **Context budget:** `FactEndpoints.cs`, `SourceEndpoints.cs`, fact/source
  repositories; PROJECT-MAP: Fact Endpoints, Source Endpoints;
  specification.md API Contracts.
- **Tool budget:** serena, roslyn, dotnet SDK, git; run skill for live check.
- **Tier:** Sonnet-class, default.
- **Done when:** endpoints implemented with tests; live-verified; spec
  already documents them, so no doc change expected.
- **Status:** Done 2026-07-14. Served as the acceptance test for overhaul
  packet OP-012's exit criterion 8: a fresh subagent, given only `docs/` and
  this packet, shipped all three endpoints with 12 new tests (80/80
  passing), live-verified. Commit `150edb6`
  ("feat(api): add PUT/DELETE fact + PUT source endpoints").

### HYG-3: Test coverage — Deke.Api, Deke.Mcp, federation delegation

- **Goal:** Close the coverage gaps OP-004f mapped: test projects/references
  for `Deke.Api` + `Deke.Mcp`; `ApiKeyAuthHandler` (fail-fast and 401 paths,
  post-ADR-0008); `FederatedSearchService`'s actual delegation path
  (`ShouldFederate`, parallel peer queries, merging); extract the
  hand-copied test fakes into a shared helpers file.
- **Depends on:** nothing (HYG-2 lands more surface to cover; order is free).
- **Context budget:** `tests/Deke.Tests`; PROJECT-MAP: API Key
  Authentication, Federation (Infrastructure), the six test entries.
- **Tool budget:** serena, roslyn, dotnet SDK, git.
- **Tier:** Sonnet-class, default.
- **Done when:** named gaps have tests; `dotnet test` green; fakes
  deduplicated.

## Sized Packets — After Dependencies Land

### R2: Five-level deduplication

- **Goal:** Implement the five-level dedup pipeline (URL hash → content hash
  → normalized hash → similarity hash → semantic similarity ≥ 0.92); levels
  1–3 sync at ingest, 4–5 async background jobs.
- **Depends on:** P1-1 (`independence_fingerprint` on sources).
- **Context budget:** retrieval-pipeline.md Phase R2; specification.md dedup
  section; PROJECT-MAP: Harvesters, Repositories.
- **Tool budget:** serena, roslyn, postgres-mcp, dotnet SDK, git.
- **Tier:** Sonnet-class, default (design already specified).
- **Done when:** all five levels active; tests cover each level; background
  jobs scheduled.

### P1-2: Quality pipeline

- **Goal:** Review queue (`GET /api/review-queue`), temporal-validity
  handling, contradiction detection. Opens with the scheduled OI-07
  (version-aware contradiction) and OI-08 (fact retirement) design reviews
  per [decisions.md](architecture/decisions.md) Review Schedule.
- **Depends on:** P1-1.
- **Context budget:** decisions.md OI-07/OI-08; specification.md Planned
  Endpoints; PROJECT-MAP: Trust, Repositories, Fact Endpoints.
- **Tool budget:** serena, roslyn, postgres-mcp, dotnet SDK, git;
  AskUserQuestion for the OI reviews.
- **Tier:** strongest available, extended thinking (contains open design).
- **Done when:** review-queue endpoint live; contradiction detection behind a
  version-aware policy or an explicit deferral ADR; OI-07/OI-08 dispositioned.

### R4: Cross-encoder re-ranking

- **Goal:** Re-rank top-N ANN candidates with a local cross-encoder
  (ms-marco-MiniLM-L-6-v2 via ONNX); defaults top-50 in, top-12 out.
- **Depends on:** R1 (done). Consult RES-1 findings if available before
  committing to a pgvector-only pipeline shape.
- **Context budget:** retrieval-pipeline.md Phase R4; PROJECT-MAP: Embeddings,
  Search & Trust Contracts.
- **Tool budget:** serena, roslyn, dotnet SDK, git.
- **Tier:** Sonnet-class, default.
- **Done when:** re-rank stage wired into query pipeline behind config;
  measurable ranking change demonstrated on a sample query set.

### R5: PDF and Markdown ingestion

- **Goal:** `PdfHarvester` (PdfPig) and `MarkdownHarvester` (Markdig)
  implementing `IHarvester`, feeding page/section segments into the chunking
  pipeline.
- **Depends on:** R1 (done).
- **Context budget:** retrieval-pipeline.md Phase R5; PROJECT-MAP: Harvesters.
- **Tool budget:** serena, roslyn, nuget MCP, dotnet SDK, git.
- **Tier:** Sonnet-class, default.
- **Done when:** both harvesters registered and selected by `Source.Type`;
  ingestion live-verified on a real PDF and a real Markdown source.

### EE1-DESIGN: Evolution Engine entry — EE-1 design and sizing

- **Goal:** Produce the concrete EE-1 (trajectory logging) design: evaluate
  whether `advisory_interactions` already captures EE-1's needs or must be
  extended per the decisions.md action items (full trajectory capture,
  Response Audit Record, trajectory data governance). Re-evaluates
  [ADR-0011](adr/ADR-0011-advisory-pipeline-stages-vs-active-package-3.md)
  (7-stage pipeline sufficiency) — its revisit trigger fires exactly here.
- **Depends on:** none technically; scheduled after the NOW lane by priority.
- **Context budget:** [evolution-vision.md](science/evolution-vision.md);
  decisions.md Consolidated Action Items + Open Design Questions;
  ADR-0011; PROJECT-MAP: Advisory Pipeline Implementation.
- **Tool budget:** serena (read), postgres-mcp (audit-table inspection),
  AskUserQuestion.
- **Tier:** strongest available, extended thinking.
- **Done when:** EE-1 build packet (and, if ADR-0011 reopens, a
  pipeline-stage packet) appended to this DAG with full sizing; the EE phase
  inventory (EE-1..EE-5) confirmed and recorded.

## Sized Packets — Later

### R7: Embedding abstraction

- **Goal:** Wrap `OnnxEmbeddingService` behind the Microsoft.Extensions.AI
  embedding abstraction as the swap point for P1-4 and API/Ollama-hosted
  embedding models.
- **Depends on:** nothing; most valuable before P1-4. **Unblocks:** P1-4.
- **Context/tools/tier:** retrieval-pipeline.md Phase R7; PROJECT-MAP:
  Embeddings; serena, roslyn, dotnet SDK; Sonnet-class, default.

### R8: Evaluation harness

- **Goal:** DEKE-specific retrieval evaluation framework (metrics + method
  per retrieval-pipeline.md Evaluation Framework) so pipeline changes prove
  measurable improvement.
- **Depends on:** R1–R4.
- **Context/tools/tier:** retrieval-pipeline.md Phases R8 + Evaluation
  Framework; PROJECT-MAP: Embeddings, Search & Trust Contracts; serena,
  dotnet SDK, postgres-mcp; Sonnet-class, default.

### P1-3: Terminology database

- **Goal:** Structure-layer terminology store (canonical forms,
  abbreviations, cross-language variants). **Unblocks:** R3.
- **Depends on:** P1-1 recommended (shared schema work).
- **Context/tools/tier:** specification.md; PROJECT-MAP: Term & Pattern
  Domain; serena, postgres-mcp, dotnet SDK; strongest available (schema
  design), extended.

### R3: Query expansion

- **Goal:** Expand queries pre-embedding from the terminology database
  (canonical forms, cross-language variants, domain disambiguation).
- **Depends on:** P1-3.
- **Context/tools/tier:** retrieval-pipeline.md Phase R3; PROJECT-MAP:
  Search & Trust Contracts; serena, roslyn, dotnet SDK; Sonnet-class,
  default.

### FED-4: Selective replication (Federation Phase 4)

- **Goal:** Bulk replication for cross-instance knowledge sync per
  [federation.md](architecture/federation.md) Phase 4.
- **Depends on:** P1-1 (provenance travels with replicated facts).
- **Context/tools/tier:** federation.md; PROJECT-MAP: Federation,
  Federation (Infrastructure); serena, roslyn, postgres-mcp, dotnet SDK;
  strongest available, extended (protocol design).

### FED-5: Trust federation (Federation Phase 5)

- **Goal:** Security hardening — mutual TLS, per-peer authorization; ends
  the deliberate auth-deferred stance recorded in INTENT.md.
- **Depends on:** FED-4 (or explicit decision to harden before replication).
- **Context/tools/tier:** federation.md Phase 5; PROJECT-MAP: Federation
  Endpoints, API Key Authentication; serena, roslyn, dotnet SDK;
  strongest available, extended (security design).

### DOM-2: Second domain adapter

- **Goal:** Second `IAdvisoryAdapter` domain (proves multi-domain), domain
  activation auto-monitoring. Opens with the scheduled OI-02 (question
  corpus seeding) review; OI-03 (cross-domain transfer) becomes reviewable
  once this lands.
- **Depends on:** none hard; more valuable after P1-1/P1-2 raise fact
  quality.
- **Context/tools/tier:** PROJECT-MAP: Advisory Adapter Plugin, Advisory
  Pipeline Contract; decisions.md OI-02/OI-03; serena, roslyn, dotnet SDK,
  AskUserQuestion (domain choice is Mikael's); Sonnet-class, default.

### P1-4: Multilingual model swap

- **Goal:** Replace all-MiniLM-L6-v2 with a multilingual embedding model
  (Phase 4 of the retrieval plan; requires re-embedding stored facts).
- **Depends on:** R7.
- **Context/tools/tier:** retrieval-pipeline.md Phases R7 + Phase 4 (Query
  Stages); PROJECT-MAP: Embeddings; serena, dotnet SDK, postgres-mcp;
  Sonnet-class, default.

## Trigger-Gated Work

Not sized — each row names its trigger and the sizing path. Honest sizing
beats speculative sizing.

| Work | Trigger | Sizing path |
|---|---|---|
| R6: HNSW index option | Fact count > 10,000 per domain, or IVFFlat recall degrades | Size as a small retrieval packet when triggered |
| EE-2 (prediction model), EE-4 (hindsight loop), EE-5 (adapter evolution), Curiosity Service, and the rest of the EE phase inventory | EE1-DESIGN output | Spawned, sized, and sequenced by EE1-DESIGN |
| Hybrid retrieval build packet | RES-1 output | Spawned and sized by RES-1 |
| OI-01 … OI-10 open design items | Per the Review Schedule in [decisions.md](architecture/decisions.md) | Reviewed at their listed milestones; accepted items become packets here |
| Parked cleanup nits (DomainStats file location, shared ingestion helper, `GetDomainAdvice` naming) | Ride along with the next packet touching those files | overhaul/PARKING-LOT.md (archived) holds the details |

## Progress Log

| Date | Milestone | Notes |
|---|---|---|
| 2026-03 | Package 1 v1 | Core pipeline: ingest, search, serve. REST API + MCP. |
| 2026-04 | Federation Phase 1 | Peer discovery, manifest endpoint, health monitoring. |
| 2026-04 | Federation Phase 2 | Federated search, provenance, MCP tools. |
| 2026-04 | Documentation overhaul | Three-branch doc structure (product/architecture/science). |
| 2026-04 | Product checkpoint | Scope reduction: simplified pipeline, MVP focus. |
| 2026-07-03 | Advisory Pipeline MVP | Contracts, 7-stage pipeline, Software Product Advisor adapter, GetDomainAdvice MCP tool, `advisory_interactions` audit table. |
| 2026-07-14 | Overhaul design packets | `Llm/` retired onto `IChatClient` (ADR-0007); API-key fail-fast (ADR-0008); fact-only domains visible over MCP (ADR-0009); glossary ENFORCED via CI lint; MVP recognized as delivered in full. |
| 2026-07-14 | Roadmap rebuilt | This file: all planned work re-expressed as sized packets in a DAG (overhaul packet OP-011, exit criterion 7). |
| 2026-07-14 | Overhaul dissolved | All 8 exit criteria passed (OP-012). `overhaul/` archived in place — kept for history, excluded from agent context budgets. Repo tagged `overhaul-complete` (overhaul packet OP-013). |
