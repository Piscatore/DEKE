# Roadmap

This document summarizes the implementation phases across all three packages. For detailed design, see the [architecture](architecture/) branch. For what each package delivers, see the [product](product/) branch.

## Current Status

| Package | Status | Details |
|---------|--------|---------|
| Package 1: Knowledge Base | v1 complete | Core INGEST-LEARN-SERVE pipeline operational |
| Federation | Phase 1-2 complete | Discovery, manifest, federated search, MCP tools |
| Package 2: Knowledge Leverage | In design | Advisory pipeline, domain adapters |
| Package 3: Evolution Engine | In design | Three-signal framework, prediction-error learning |

## Implementation Sequence

Phases are ordered by dependency and value delivery. Package 1 Phase 1 (provenance schema) is the critical path for everything downstream.

### Package 1: Knowledge Base

| Phase | Name | Scope | Status |
|-------|------|-------|--------|
| P1-0 | Foundation | Core pipeline: ingest, search, serve | Complete |
| P1-1 | Provenance Core | Source credibility, temporal validity, corroboration, contradiction flag, fact versioning | Planned |
| P1-2 | Quality Pipeline | 5-level deduplication, trust scoring, domain trust policies, review queue | Planned |
| P1-3 | Structure | Typed relations, dynamic taxonomy, terminology, entity awareness | Planned |
| P1-4 | Expertise | Inference engine, gap analysis, health monitoring, self-assessment | Planned |
| P1-5 | Advanced | Curator workflows, emergent schema, methodology learning, multilingual | Planned |

### Package 2: Knowledge Leverage

| Phase | Name | Scope | Prerequisites |
|-------|------|-------|---------------|
| P2-1 | Core Contracts | AdvisoryRequest, AdvisoryResponse, IAdvisoryAdapter, DefaultAdapter | None |
| P2-2 | Advisory Pipeline | Full 13-stage pipeline, LLM integration, Package 3 event emission | P2-1, P1-1 |
| P2-3 | Software Product Advisor | First domain adapter, bootstrap ingestion | P2-2 |
| P2-4 | MCP Tool | GetDomainAdvice tool for Claude Code | P2-3 |
| P2-5 | Package 3 Integration | Live adapter fitness reads, domain health signals | P2-2, P3-1, P3-2 |
| P2-6 | Ollama + Multi-domain | Local model backend, second domain | P2-5 |

### Package 3: Evolution Engine

| Phase | Name | Scope | Prerequisites |
|-------|------|-------|---------------|
| P3-1 | Signal Infrastructure | Interaction logging, feedback capture, trajectory data | P2 v1 |
| P3-2 | Prediction Model | Quality prediction, delta computation, backward propagation | P3-1, P1-1 |
| P3-3 | Curiosity Service | Self-query loop, gap taxonomy, harvest directives | P3-2, P1-2 |
| P3-4 | Hindsight Loop | Delayed probes, three-signal triangulation | P3-2 |
| P3-5 | Adapter Evolution | MAP-Elites archive, GEPA-derived mutation, shadow mode | P3-2, P2 adapters |
| P3-6 | Cross-Domain Transfer | Delta sharing, adapter transfer, compound prediction | P3-5, multiple domains |

### Federation

| Phase | Name | Status |
|-------|------|--------|
| Phase 1 | Discovery and Manifest | Complete |
| Phase 2 | Federated Search + MCP Tools | Complete |
| Phase 4 | Bulk Replication | Planned |
| Phase 5 | Security and Hardening | Planned |

### Retrieval Pipeline

| Phase | Name | Scope |
|-------|------|-------|
| R1 | Chunking Strategy | SemanticChunker.NET integration |
| R2 | Hybrid Search | BM25 alongside vector search |
| R3 | Re-Ranking | Cross-encoder re-ranking |
| R4 | Query Transformation | Decomposition, HyDE |
| R5 | Context Assembly | Coherent ordering, deduplication |
| R6 | Evaluation | Automated quality metrics |
| R7 | Ingestion Formats | PDF, Markdown, HTML |
| R8 | Semantic Caching | Query result caching |

## First Milestone

The first meaningful milestone is DEKE giving grounded software architecture advice via Claude Code MCP:

- P1-1 complete (provenance schema)
- P2-1 through P2-4 complete (advisory pipeline + MCP tool)
- P3-1 complete (interaction logging from first advisory call)
- Bootstrap ingestion of DEKE design session history
