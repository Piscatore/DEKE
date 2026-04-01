# Knowledge Base

## Purpose

The Knowledge Base is the inward-facing core of DEKE. Its sole concern is the quality of what is stored. It ingests knowledge from external sources, organises it, tracks its provenance, assesses its trustworthiness, and makes it retrievable. Its direction is always inward: making the stored knowledge more accurate, better sourced, and more richly connected.

## Core Capabilities

The Knowledge Base provides five foundational capabilities:

- **Fact ingestion.** Knowledge enters DEKE as individual facts extracted from feeds, web pages, documents, and curated conversations. Each fact carries provenance metadata from the moment of ingestion: where it came from, when it was collected, and how it was extracted.

- **Semantic search.** Facts are stored with embedding vectors that capture their meaning, not just their words. Search queries are matched against these vectors, enabling retrieval by semantic similarity rather than keyword matching.

- **Source monitoring.** DEKE continuously monitors registered sources --- feeds, web pages, and other endpoints --- on a configurable schedule. New content is automatically ingested, extracted into facts, and scored.

- **Pattern discovery.** Recurring themes, clusters of related facts, and emergent topic groupings are identified automatically through embedding-space analysis.

- **Relation mapping.** Facts are linked to other facts based on semantic proximity. These connections form the foundation for the typed relation graph introduced in later phases.

## Knowledge Quality Framework

What distinguishes DEKE from a simple fact store is its commitment to knowledge quality. Every fact carries metadata that describes not just what it says, but how much it should be trusted.

### Source Credibility

Every source registered in DEKE carries a credibility score. This score is tracked independently from any individual fact --- it reflects the overall reliability of the source based on its history. A high-credibility source produces facts that start with higher baseline confidence; a low-credibility or unverified source produces facts that require additional signals before they are fully trusted.

### Fact Confidence

Each fact has a confidence score computed from two inputs: the credibility of the source it came from and its recency. A fact from a highly credible source that was recently verified has high confidence. A fact from an unverified source or one that has not been checked in a long time has lower confidence. Confidence is a continuous value, not a binary state.

### Temporal Validity

Facts carry optional validity windows: when they became true and when they ceased to be true. This is critical for domains where knowledge evolves --- a regulatory requirement that changed, a software pattern that was superseded, or a seasonal condition that is time-bounded. Temporal validity enables DEKE to answer "what was true on date X" queries and to flag stale facts for re-verification.

### Deduplication

DEKE applies deduplication at three levels during ingestion:

1. **URL-level.** The same source URL, after normalisation, produces only one fact.
2. **Content hash.** Byte-identical content from different URLs is recognised as the same fact.
3. **Normalised hash.** Whitespace, encoding, and punctuation variations of identical content are collapsed.

These three levels run immediately at ingestion, preventing duplicates from entering the store.

### Planned Enhancements

The following capabilities extend the quality framework and are planned for future phases:

- **Corroboration tracking.** The number of independent sources asserting the same or equivalent claim is tracked automatically. When a new fact is ingested, DEKE searches for semantically equivalent existing facts and increments the corroboration count for genuinely independent sources. Higher corroboration means higher confidence.

- **Contradiction detection.** When a new fact closely matches an existing fact in meaning but asserts an opposing claim, both facts are flagged as contested. Neither is deleted. The conflict enters a review queue with resolution options including accepting the newer fact, accepting the one with higher corroboration, or producing a synthesised fact that acknowledges both positions.

- **Trust states lifecycle.** Every fact passes through a lifecycle of trust states: unscored, accepted, flagged, contested, or rejected. These states are independent of ingestion order and are governed by domain trust policies.

- **Domain trust policies.** Per-domain configuration governing which source tiers are auto-accepted, minimum corroboration counts, whether temporal validity metadata is required, minimum confidence thresholds, and whether only primary sources are accepted without review.

- **Near-duplicate detection (dedup level 4).** Content with approximately eighty percent or greater textual overlap --- such as wire-service reprints --- is identified using techniques like MinHash or SimHash. This level runs asynchronously to avoid slowing the ingestion pipeline.

- **Semantic deduplication (dedup level 5).** Content that conveys the same meaning in different words is identified through embedding similarity. This level also runs asynchronously.

## Planned Capabilities

### Structure Layer

The current Knowledge Base stores facts as individual items linked by semantic proximity. The planned structure layer adds richer organisation:

- **Typed relations.** Facts are connected by named relationship types (causes, contradicts, supports, requires, supersedes, instance-of) rather than unnamed similarity links. Each relation carries a confidence score and a source attribution.
- **Dynamic taxonomy.** Facts are classified into a hierarchical category structure that adapts as the knowledge base grows. Categories emerge from clustering patterns, split when they become too broad, merge when they become too sparse, and restructure when coherence drops.
- **Terminology management.** A domain terminology database maps canonical terms to their variants, abbreviations, regional forms, and cross-language equivalents. Queries are expanded using this vocabulary before search, improving recall for both domain experts and novices.
- **Entity awareness.** Named entities are extracted from facts and resolved to canonical records. Facts about the same entity expressed in different ways are linked. Entity-scoped queries become direct lookups rather than similarity searches.

### Expertise Layer

The expertise layer transforms structured knowledge into reasoning capability:

- **Inference engine.** DEKE can draw conclusions beyond simple retrieval: deductive (certain conclusions from premises), inductive (probable conclusions from patterns), abductive (best explanations for observations), and analogical (transfer from similar domains).
- **Gap analysis.** DEKE identifies what it does not know: unanswerable questions, structural gaps between entities, temporal staleness, and single-source risk areas.
- **Health monitoring.** The knowledge base monitors its own quality and triggers remediation when thresholds are crossed: rising contradiction rates, declining corroboration, stale sources, or high rates of unclassified facts.

### Advanced Capabilities

- **Curator workflows.** Roles, batch review workflows, and resolution tracking for human curation of knowledge quality.
- **Emergent schema.** When facts about a specific entity type accumulate beyond a threshold, DEKE recognises the pattern and proposes structured storage for that entity type, enabling direct queries alongside semantic search.
- **Multilingual support.** A single embedding space across languages, with language detection, cross-language terminology variants, and query expansion across languages.

## See Also

- [architecture/specification.md](../architecture/specification.md) for implementation details, schema definitions, and phased delivery plan.
