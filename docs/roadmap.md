# Roadmap

## Current State

What DEKE delivers today:

| Capability | Status |
|---|---|
| **Knowledge Base (v1)** | Fact ingestion from RSS feeds and web pages. Semantic search via 384-dim embeddings (pgvector). Source monitoring on schedule. Pattern discovery. Relation mapping. Basic deduplication (URL + content hash). |
| **REST API** | Facts CRUD, source CRUD, semantic search (POST), federation endpoints. |
| **MCP Tools** | `consult_domain_expert` (federated search), `get_context` (LLM-formatted context), `list_available_domains` (local + peer domains). |
| **Federation (Phase 1--2)** | Peer discovery via manifest. Federated search with delegation, provenance tracking, loop prevention, and locality-weighted scoring. |
| **Background Services** | Source monitor (15 min), pattern discovery (1 hr), learning cycle (2 hr), peer health check (5 min). |
| **Infrastructure** | .NET 9, PostgreSQL 16 + pgvector, ONNX embeddings (all-MiniLM-L6-v2), Dapper, Polly resilience. |

**Not yet built**: advisory pipeline, domain adapters, trust layer, semantic chunking, LLM-generated responses.

## MVP Target

The first meaningful milestone: DEKE answering domain questions better than a language model alone, with cited and confidence-scored facts.

| # | Work Item | Scope | Label |
|---|-----------|-------|-------|
| 1 | **Simplified trust layer** | Add `credibility_score` to sources, `confidence_score` to facts, optional `valid_from`/`valid_until` on facts. Schema migration + scoring function. | knowledge-base |
| 2 | **Semantic chunking (R1)** | Integrate SemanticChunker.NET. Replace single-fact extraction with semantically coherent chunks. | knowledge-base |
| 3 | **Advisory contracts (P2-1)** | AdvisoryRequest, AdvisoryResponse, IAdvisoryAdapter, ConfidenceBand, DefaultAdvisoryAdapter. Compile-only milestone. | knowledge-leverage |
| 4 | **Advisory pipeline (P2-2)** | 7-stage pipeline: validate, retrieve, assemble context, construct prompt, call model, assemble response, log. Anthropic API integration. | knowledge-leverage |
| 5 | **Software Product Advisor (P2-3)** | First domain adapter. Custom system prompt, fact weighting, version-aware context. Domain activation. | knowledge-leverage |
| 6 | **GetDomainAdvice MCP tool (P2-4)** | MCP tool exposing advisory pipeline to Claude Code. | knowledge-leverage |
| 7 | **Bootstrap ingestion** | Ingest DEKE design session history into Software Product Advisor domain. Primary-source, high-confidence seed content. | knowledge-base |
| 8 | **Interaction logging** | Log every advisory interaction (query, cited facts, model used, confidence). Data capture for future analysis. No learning loop yet. | knowledge-leverage |

## Next Priorities

After MVP, in rough priority order:

- **Hybrid search (R2)** — Add BM25 alongside vector search for keyword-heavy queries.
- **Cross-encoder re-ranking (R4)** — Re-score top-N candidates with a local cross-encoder model.
- **Full trust framework** — Corroboration tracking, contradiction detection, trust states lifecycle, domain trust policies, 5-level deduplication.
- **PDF and Markdown ingestion (R5)** — Extend harvesters to PDF and Markdown sources.
- **Multi-domain** — Second domain adapter, domain activation auto-monitoring.

## Future

- **Federation Phase 4** — Bulk replication for cross-instance knowledge sync.
- **Federation Phase 5** — Security hardening (mutual TLS, per-peer authorization).
- **Local LLM backend** — Ollama integration for zero-cost advisory in mature domains.
- **Structure layer** — Typed relations, dynamic taxonomy, terminology database, entity awareness.
- **Expertise layer** — Inference engine, gap analysis, health monitoring, self-assessment.

## Research Directions

These are intellectually interesting but require user volume and query data that do not currently exist. They are preserved as research for future exploration, not active development targets.

- **Evolution Engine** — Self-improving advisory quality through prediction-error learning, three-signal feedback, and adapter evolution. See [science/evolution-vision.md](science/evolution-vision.md).
- **Curiosity Service** — Self-directed knowledge acquisition driven by answerability prediction.
- **Advanced retrieval** — Query expansion (R3), HNSW indexing (R6), embedding abstraction (R7), evaluation harness (R8).

## Progress Log

| Date | Milestone | Notes |
|---|---|---|
| 2026-03 | Package 1 v1 | Core pipeline: ingest, search, serve. REST API + MCP. |
| 2026-04 | Federation Phase 1 | Peer discovery, manifest endpoint, health monitoring. |
| 2026-04 | Federation Phase 2 | Federated search, provenance, MCP tools. |
| 2026-04 | Documentation overhaul | Three-branch doc structure (product/architecture/science). |
| 2026-04 | Product checkpoint | Scope reduction: two-package model, simplified pipeline, MVP focus. |
