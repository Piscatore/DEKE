# Retrieval Theory

Modern information retrieval combines classical text-matching techniques with neural methods to locate, rank, and assemble relevant passages from large corpora. This document covers the core concepts underlying retrieval-augmented generation (RAG) pipelines: how documents are segmented, how search is performed, how results are refined, and how quality is measured.

## Chunking Strategies

Chunking is the process of dividing source documents into segments suitable for embedding and retrieval. The choice of chunking strategy directly affects recall (can the system find the answer?) and precision (does the retrieved context contain noise?).

### Semantic Chunking

Semantic chunking splits text at natural topic boundaries rather than at fixed token counts. Approaches include:

- **Embedding-based boundary detection.** Compute sentence embeddings for consecutive sentences and split where cosine similarity between adjacent sentences drops below a threshold.
- **LLM-assisted segmentation.** Use a language model to identify topic shifts and produce semantically coherent chunks.

Semantic chunking preserves the internal coherence of each chunk, improving embedding quality, but is more computationally expensive than fixed-size methods.

### Sliding Window

A sliding window produces overlapping chunks of fixed token length. For example, a window of 512 tokens with a stride of 256 tokens produces chunks that overlap by 50%. The overlap ensures that information near chunk boundaries is not lost. This is the simplest and most widely used strategy, and serves as a strong baseline.

### Recursive Chunking

Recursive chunking attempts to split text at progressively finer structural boundaries: first by section, then by paragraph, then by sentence, until each chunk is within the target size. This preserves document structure better than a flat sliding window, as chunks tend to align with the author's organizational intent.

### Document-Structure-Aware Chunking

For structured documents (HTML, Markdown, PDF with headings, code files), chunking can respect the document's own hierarchy. Headings, function boundaries, table rows, and list items serve as natural split points. Metadata (the heading path, file name, function signature) is attached to each chunk to provide context that the chunk text alone may lack.

## Hybrid Search

No single retrieval method dominates across all query types. Hybrid search combines the complementary strengths of sparse and dense retrieval.

### Vector Similarity

Dense retrieval encodes both queries and documents as vectors in a learned embedding space and retrieves by nearest-neighbor search (typically cosine similarity or inner product). Dense retrieval excels at semantic matching — finding passages that are conceptually related to the query even when they share few lexical terms. However, it can struggle with precise keyword matching, rare terms, and exact-match requirements.

### BM25

BM25 is a probabilistic sparse retrieval function based on term frequency, inverse document frequency, and document length normalization. It excels at exact keyword matching and handles rare terms well. BM25 requires no training data and is fast to index and query. Its weakness is the vocabulary mismatch problem: it cannot match synonyms, paraphrases, or semantically equivalent expressions that use different words.

### Reciprocal Rank Fusion (RRF)

RRF is a simple, effective method for combining ranked lists from multiple retrieval systems. For each document, the RRF score is:

```
RRF(d) = Σ 1 / (k + rank_i(d))
```

where `k` is a constant (typically 60) and `rank_i(d)` is the rank of document `d` in the i-th retrieval system's results. RRF does not require score normalization across systems, making it practical for combining BM25 and vector search outputs. Documents that rank highly in both systems receive the highest fused scores.

## Re-Ranking

Initial retrieval (whether sparse, dense, or hybrid) is optimized for recall at moderate precision. Re-ranking applies a more expensive model to a smaller candidate set to improve precision.

### Cross-Encoder Re-Ranking

A cross-encoder takes the query and a candidate passage as a single concatenated input and produces a relevance score. Unlike bi-encoders (which encode query and passage independently), cross-encoders attend jointly to both, capturing fine-grained interactions. This makes them substantially more accurate but too slow to apply to the full corpus. The standard pattern is: retrieve the top-N candidates with a fast method, then re-rank with a cross-encoder.

### The Lost-in-the-Middle Problem

Liu et al. (2023) demonstrated that large language models are sensitive to the position of relevant information within their context window. Information at the beginning and end of the context is used more effectively than information in the middle. This finding has direct implications for context assembly: the ordering of retrieved passages matters, not just their selection. Placing the most relevant passages at the start or end of the context, rather than burying them in the middle, improves downstream task performance.

## Query Transformation

Raw user queries are often underspecified, ambiguous, or poorly suited for direct retrieval. Query transformation techniques reformulate the query to improve retrieval quality.

### Query Decomposition

Complex questions are broken into simpler sub-questions, each of which is retrieved independently. The results are then merged. For example, "How does X compare to Y for use case Z?" can be decomposed into: "What is X?", "What is Y?", "How is X used for Z?", "How is Y used for Z?" Decomposition improves recall for multi-faceted queries.

### HyDE (Hypothetical Document Embeddings)

Gao et al. (2022) proposed generating a hypothetical answer to the query using an LLM, then using the embedding of that hypothetical answer as the retrieval query. The intuition is that a generated answer, even if imperfect, is closer in embedding space to real relevant documents than the original short query. HyDE improves retrieval performance on zero-shot benchmarks where no training data is available.

### Step-Back Prompting

Zheng et al. (2023) introduced step-back prompting, which asks the LLM to generate a more abstract or general version of the query before retrieval. For instance, "What is the melting point of iron?" might step back to "What are the physical properties of iron?" The broader query retrieves a richer set of passages that is more likely to contain the specific answer.

## Context Assembly

After retrieval and re-ranking, the selected passages must be assembled into a coherent context for the downstream model (typically an LLM). Context assembly is a constrained optimization problem: maximize the relevance and coverage of the context within a fixed token budget.

### Token Budget Management

Each LLM has a finite context window. The context assembly stage must allocate tokens across: the system prompt, the user query, the retrieved passages, and space reserved for the model's response. Strategies include:

- **Truncation.** Include passages in rank order until the budget is exhausted.
- **Compression.** Summarize or extract key sentences from lower-ranked passages to fit more information within the budget.
- **Adaptive allocation.** Reserve more budget for complex queries and less for simple ones.

### Deduplication

When multiple retrieval paths (decomposed sub-queries, hybrid search) return overlapping results, deduplication prevents the context from containing redundant passages. Deduplication can operate at the chunk level (exact or near-exact match) or at the information level (semantic deduplication using embedding similarity).

### Coherence Ordering

The order in which passages appear in the context affects both LLM comprehension (see the lost-in-the-middle problem above) and the logical flow of the assembled context. Strategies include:

- **Relevance-first.** Place the most relevant passages at the start.
- **Chronological.** For time-sensitive content, order passages by date.
- **Source grouping.** Group passages from the same document together to preserve local context.
- **Sandwich ordering.** Place high-relevance passages at the start and end, with lower-relevance passages in the middle, to mitigate the lost-in-the-middle effect.

## Evaluation Metrics

Retrieval quality is measured at two levels: the retrieval stage itself (did we find the right passages?) and the end-to-end system (did the final answer satisfy the user?).

### Mean Reciprocal Rank (MRR)

MRR measures how early the first relevant result appears in the ranked list:

```
MRR = (1/|Q|) Σ 1/rank_i
```

where `rank_i` is the position of the first relevant document for query `i`. MRR is appropriate when users care primarily about the top result.

### Normalized Discounted Cumulative Gain (nDCG)

nDCG accounts for graded relevance (not just binary) and discounts the contribution of results at lower ranks:

```
DCG_k = Σ (2^{rel_i} − 1) / log_2(i + 1)
nDCG_k = DCG_k / IDCG_k
```

where `IDCG_k` is the ideal DCG achievable with perfect ranking. nDCG is the standard metric for evaluating ranked retrieval systems with graded relevance judgments.

### Answer Relevance

In RAG systems, retrieval is a means to an end. Answer relevance measures whether the final generated answer addresses the user's query. This is typically evaluated by an LLM judge or human annotators on a Likert scale. Answer relevance captures the full pipeline quality, including retrieval, context assembly, and generation.

### Faithfulness

Faithfulness measures whether the generated answer is grounded in the retrieved context — that is, whether the answer makes claims that are supported by the provided passages. Low faithfulness indicates hallucination: the model is generating information not present in the context. Faithfulness is typically measured by decomposing the answer into atomic claims and verifying each against the retrieved passages.

## See Also

- [papers.md](papers.md) — Consolidated bibliography with full citations
