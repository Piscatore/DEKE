# Knowledge Base (Package 1 --- Knowledge Integrity)

## Purpose

The Knowledge Base is the inward-facing package of DEKE. Its sole concern is the quality of what is stored. It ingests knowledge from external sources, organises it, tracks its provenance, assesses its trustworthiness, and makes it retrievable. It does not serve advisory responses (that is Package 2) and it does not learn from outcomes (that is Package 3). Its direction is always inward: making the stored knowledge more accurate, better sourced, and more richly connected.

## Core Capabilities

The Knowledge Base provides five foundational capabilities:

- **Fact ingestion.** Knowledge enters DEKE as individual facts extracted from feeds, web pages, documents, and curated conversations. Each fact carries provenance metadata from the moment of ingestion: where it came from, when it was collected, and how it was extracted.

- **Semantic search.** Facts are stored with embedding vectors that capture their meaning, not just their words. Search queries are matched against these vectors, enabling retrieval by semantic similarity rather than keyword matching.

- **Source monitoring.** DEKE continuously monitors registered sources --- feeds, web pages, and other endpoints --- on a configurable schedule. New content is automatically ingested, extracted into facts, and scored.

- **Pattern discovery.** Recurring themes, clusters of related facts, and emergent topic groupings are identified automatically through embedding-space analysis.

- **Relation mapping.** Facts are linked to other facts based on semantic proximity. These connections form the foundation for the typed relation graph introduced in later phases.

## Knowledge Quality Framework

What distinguishes DEKE from a simple fact store is its commitment to knowledge quality. Every fact carries metadata that describes not just what it says, but how much it should be trusted.

### Provenance Tracking

Every fact in DEKE maintains a complete lineage: the source it came from, the credibility of that source, when the fact was collected, how it was extracted, and when it was last verified. Source credibility and fact confidence are tracked independently --- a fact extracted from a high-credibility source can still be ambiguous, and a fact from an unverified source can be corroborated by multiple independent sources.

### Corroboration

The number of independent sources asserting the same or equivalent claim is tracked automatically. When a new fact is ingested, DEKE searches for semantically equivalent existing facts. If the new fact comes from a genuinely independent source (not a reprint or syndication), the corroboration count increments. Higher corroboration means higher confidence. Source independence is tracked to prevent single-source amplification.

### Contradiction Detection

When a new fact closely matches an existing fact in meaning but asserts an opposing claim, both facts are flagged as contested. Neither is deleted. The conflict enters a review queue. Resolution options include accepting the newer fact, accepting the one with higher corroboration, or producing a synthesised fact that acknowledges both positions.

### Temporal Validity

Facts carry optional validity windows: when they became true and when they ceased to be true. This is critical for domains where knowledge evolves --- a regulatory requirement that changed, a software pattern that was superseded, or a seasonal condition that is time-bounded. Temporal validity enables DEKE to answer "what was true on date X" queries and to flag stale facts for re-verification.

### Trust States

Every fact passes through a lifecycle of trust states: unscored, accepted, flagged, contested, or rejected. These states are independent of ingestion order and are governed by domain trust policies.

## Deduplication

DEKE applies deduplication at five progressively sophisticated levels:

1. **URL-level.** The same source URL, after normalisation, produces only one fact.
2. **Content hash.** Byte-identical content from different URLs is recognised as the same fact.
3. **Normalised hash.** Whitespace, encoding, and punctuation variations of identical content are collapsed.
4. **Near-duplicate.** Content with approximately eighty percent or greater textual overlap --- such as wire-service reprints --- is identified as a near-duplicate.
5. **Semantic.** Content that conveys the same meaning in different words is identified through embedding similarity as a semantic duplicate.

The first three levels run immediately at ingestion. The fourth and fifth levels run asynchronously to avoid slowing the ingestion pipeline. A fact can enter the store in a pending state and transition to accepted once all deduplication levels have cleared.

## Domain Trust Policies

Different domains have different standards for what constitutes trustworthy knowledge. A casual hobby domain can auto-accept facts from any source. A legally critical domain may require multiple independent primary sources and human curation before a fact is accepted.

Domain trust policies are configurable per domain and govern:

- Which source tiers are auto-accepted and which require review.
- The minimum corroboration count before auto-acceptance.
- Whether temporal validity metadata is required.
- The minimum confidence score below which facts enter the review queue.
- Whether only primary sources are accepted without review.

This trust gradient allows the same infrastructure to serve both casual and high-stakes domains without compromise in either direction.

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
