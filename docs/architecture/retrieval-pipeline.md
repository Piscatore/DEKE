# Retrieval Pipeline

Technical design for DEKE's retrieval pipeline: how facts are ingested, chunked, embedded, indexed, searched, and ranked. This document covers the DEKE-specific implementation plan and library choices.

For the underlying concepts of semantic chunking, re-ranking, and retrieval evaluation, see [science/retrieval-theory.md](../science/retrieval-theory.md). For the product roadmap context, see [product/roadmap.md](../product/roadmap.md).

---

## Design Principles

- **Local-first inference.** All embedding and chunking operations run locally via ONNX Runtime. No external API calls for retrieval pipeline operations.
- **Progressive enhancement.** Each phase adds capability without breaking existing functionality. The current v1 pipeline (embed + cosine search) is the baseline that later phases extend.
- **Domain-agnostic infrastructure.** The pipeline mechanics are the same for all domains. Domain-specific behavior is limited to trust policy configuration and adapter-level fact weighting.
- **Measurable improvement.** Every pipeline change must be evaluated against the DEKE-specific metrics framework. No changes are adopted based on theoretical superiority alone.

---

## Pipeline Architecture

### Current Pipeline (v1)

```
Source → Harvest → Extract → Embed (ONNX) → Store (pgvector) → Search (cosine) → Rank → Return
```

This is functional but has known limitations:

- No semantic chunking (extraction produces variable-length text)
- No re-ranking (cosine similarity is the only ranking signal)
- No cross-encoder scoring
- No query expansion
- Basic deduplication (URL and content hash only)

### Target Pipeline

```
Source → Harvest → Extract → Chunk → Embed → Dedup → Store → Index
                                                          ↓
Query → Expand → Embed → ANN Search → Re-rank → Filter → Assemble → Return
```

Each stage is implemented as an independent, composable service with a defined interface. Stages can be upgraded individually without affecting the rest of the pipeline.

### Ingestion Stages

| Stage | Current | Target | Description |
|-------|---------|--------|-------------|
| Harvest | RssHarvester, WebPageHarvester | + PDF, Markdown, API harvesters | Fetch raw content from sources |
| Extract | SimpleExtractionService | LLM-assisted extraction | Extract discrete claims from raw content |
| Chunk | (none) | SemanticChunker.NET | Split content into semantically coherent segments |
| Embed | OnnxEmbeddingService (384-dim) | Same, with model upgrade path | Generate embeddings |
| Dedup | URL + content hash | 5-level pipeline (see below) | Prevent duplicate facts |
| Store | Dapper INSERT + pgvector | Same | Persist to PostgreSQL |

### Query Stages

| Stage | Current | Target | Description |
|-------|---------|--------|-------------|
| Expand | (none) | Terminology database + query rewriting | Augment query with domain synonyms and variants |
| Embed | OnnxEmbeddingService | Same | Embed the expanded query |
| ANN Search | pgvector IVFFlat | HNSW index option | Approximate nearest neighbor search |
| Re-rank | (none) | Cross-encoder re-ranker | Re-score top-N candidates with higher-fidelity model |
| Filter | Domain + is_outdated | + trust_state, min_confidence, min_corroboration | Apply provenance-based filters |
| Assemble | Raw similarity ranking | Adapter-weighted ranking + locality weighting | Produce final ranked result set |

---

## Library Ecosystem

These are the candidate libraries for pipeline enhancement. All are .NET-compatible and support local-only operation (zero external API cost).

### SemanticChunker.NET

Semantic chunking library that splits text based on meaning boundaries rather than fixed token counts.

- **Role in DEKE**: Replace the current extraction-produces-a-single-fact approach with semantically coherent chunks that better fit the 384-dimensional embedding space.
- **Integration point**: Between Extract and Embed stages.
- **Configuration**: Chunk size target, overlap percentage, boundary detection sensitivity. Domain-configurable via trust policy or adapter settings.

### Semantic Kernel Rankers

Microsoft Semantic Kernel provides re-ranking abstractions that can wrap cross-encoder models.

- **Role in DEKE**: Re-rank top-N results from pgvector's cosine search using a cross-encoder model for higher-fidelity relevance scoring.
- **Integration point**: Between ANN Search and Filter stages.
- **Model options**: ms-marco-MiniLM-L-6-v2 (local, ONNX) or API-backed cross-encoders.

### Microsoft.Extensions.AI

Unified abstraction layer for embedding and chat completions across providers.

- **Role in DEKE**: Provide a stable interface for embedding generation that can switch between ONNX, Ollama, and API-backed models without changing calling code.
- **Integration point**: Wraps OnnxEmbeddingService and future embedding providers.
- **Benefit**: Enables the multilingual model swap (Phase 5) without pipeline code changes.

### PdfPig

PDF text extraction library for .NET.

- **Role in DEKE**: Enable ingestion from PDF sources (documentation, specifications, academic papers).
- **Integration point**: New PdfHarvester implementing IHarvester.
- **Capabilities**: Text extraction, page segmentation, table detection.

### Markdig

Markdown processing library for .NET.

- **Role in DEKE**: Parse and extract structured content from Markdown sources (README files, documentation, design documents).
- **Integration point**: New MarkdownHarvester or enhancement to WebPageHarvester for Markdown content.

---

## Implementation Phases

### Phase R1: Semantic Chunking

**Dependency**: None (can proceed immediately).

Add SemanticChunker.NET to the ingestion pipeline. Replace the current approach where extraction produces a single fact per source with a chunking step that produces multiple semantically coherent facts.

Deliverables:

- IChunkingService interface in Deke.Core
- SemanticChunkingService implementation in Deke.Infrastructure
- Integration with existing harvester pipeline (chunk after extract, before embed)
- Chunk size and overlap configuration per domain

### Phase R2: Five-Level Deduplication

**Dependency**: Package 1 Phase 1 (independence_fingerprint on sources).

Implement the dedup pipeline described in [specification.md](specification.md):

1. URL hash (normalized)
2. Content hash (SHA-256)
3. Normalized hash (whitespace/encoding/punctuation)
4. Similarity hash (MinHash or SimHash for ~80%+ textual overlap)
5. Semantic similarity (pgvector cosine threshold >= 0.92)

Levels 1-3 run synchronously at ingest. Levels 4-5 run asynchronously as background jobs.

### Phase R3: Query Expansion

**Dependency**: Package 1 Phase 3 (terminology database).

Use the terminology database to expand queries before embedding:

- Canonical form expansion (abbreviations, synonyms)
- Cross-language variant expansion
- Domain-specific disambiguation

### Phase R4: Cross-Encoder Re-Ranking

**Dependency**: Phase R1 (semantic chunking must be in place to benefit from re-ranking).

Add a re-ranking stage using a local cross-encoder model (ms-marco-MiniLM-L-6-v2 via ONNX). The cross-encoder scores query-document pairs jointly, producing higher-fidelity relevance scores than cosine similarity of independent embeddings.

Configuration:

- Top-N candidates from ANN search to re-rank (default: 50)
- Final result set size after re-ranking (default: 12)
- Re-ranker model path and ONNX configuration

### Phase R5: PDF and Markdown Ingestion

**Dependency**: Phase R1 (chunking should be available for multi-page documents).

Add PdfPig and Markdig harvesters:

- PdfHarvester implementing IHarvester (SupportedType = Pdf)
- MarkdownHarvester implementing IHarvester (SupportedType = Markdown)
- Page/section segmentation feeding into the chunking pipeline

### Phase R6: HNSW Index Option

**Dependency**: None, but should be evaluated after fact count exceeds 10,000.

pgvector supports both IVFFlat and HNSW indexes. HNSW provides better recall at the cost of higher memory usage and slower index build time. Evaluate switching when:

- Fact count exceeds 10,000 per domain
- IVFFlat recall drops below acceptable threshold at current list count
- Memory budget allows HNSW's higher footprint

### Phase R7: Microsoft.Extensions.AI Abstraction

**Dependency**: None, but most valuable before the multilingual model swap.

Wrap OnnxEmbeddingService behind the Microsoft.Extensions.AI embedding abstraction. This provides a clean swap point for:

- Multilingual model replacement (Phase 5 of Package 1)
- API-backed embedding for evaluation or fallback
- Ollama-hosted embedding models

### Phase R8: Evaluation Harness

**Dependency**: Phases R1-R4 (must have pipeline stages to evaluate).

Build the DEKE-specific evaluation framework for measuring pipeline quality. This is the mechanism that ensures pipeline changes produce measurable improvement.

---

## Evaluation Framework

### DEKE-Specific Metrics

These metrics are tailored to DEKE's use case (grounded advisory responses) rather than generic IR benchmarks.

| Metric | Description | Target |
|--------|-------------|--------|
| Retrieval precision@k | Of the top-k facts retrieved, what fraction are relevant to the query? | >= 0.80 at k=12 |
| Grounding coverage | What fraction of the advisory response's claims are supported by at least one retrieved fact? | >= 0.90 |
| Provenance accuracy | Of retrieved facts, what fraction have correct and current provenance metadata? | >= 0.95 |
| Contradiction detection recall | What fraction of known contradictions are surfaced when relevant facts are retrieved? | >= 0.85 |
| Corroboration accuracy | When corroboration_count > 1, are the corroborating sources genuinely independent? | >= 0.90 |
| Latency (p95) | 95th percentile query-to-result time for a single-domain local search | < 200ms |
| Latency with federation (p95) | 95th percentile including one hop of federation delegation | < 1000ms |

### Evaluation Method

1. **Golden dataset**: Curated set of (query, expected_relevant_facts) pairs per domain. Maintained alongside the domain question corpus (Package 3 Curiosity Service).
2. **A/B pipeline comparison**: Run the same query set through old and new pipeline configurations. Compare metrics.
3. **Regression detection**: Any pipeline change that reduces a metric below its target is rejected.
4. **Integration with Package 3**: The evaluation framework feeds into Package 3's quality prediction model. Pipeline improvements that increase retrieval precision should produce corresponding improvements in advisory response quality.
