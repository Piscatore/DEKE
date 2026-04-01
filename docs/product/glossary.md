# Glossary

**Adapter.**
See *Domain Adapter*.

**Cold Start Threshold.**
The minimum knowledge state a domain must reach before DEKE serves advisory responses. Defined per domain through activation criteria including minimum fact count, minimum mean confidence, minimum distinct sources, and minimum topic coverage. A domain below its cold start threshold returns an honest status response rather than a poorly grounded answer.

**Confidence Band.**
One of four discrete levels of uncertainty that DEKE expresses in advisory responses: High, Medium, Low, or Insufficient. Derived from quality assessment based on retrieved fact metadata. Confidence bands replace raw decimal scores to prevent false precision and give consumers a clear, actionable signal.

**Corroboration.**
The number of independent sources that assert the same or semantically equivalent claim. Higher corroboration increases confidence. Source independence is tracked to prevent single-source amplification --- multiple reprints of the same original do not count as independent corroboration. See [knowledge-base.md](knowledge-base.md) planned enhancements.

**Domain.**
A bounded knowledge area within DEKE. Each domain has its own adapter configuration, activation criteria, and knowledge corpus. Examples include a software product advisor domain or a fishing knowledge domain. Domains are independent --- new domains do not require changes to the core system.

**Domain Adapter.**
The replaceable, domain-specific plugin that supplies custom prompt strategy, fact weighting, context formatting, and trust calibration to the advisory pipeline. Everything domain-specific lives in the adapter and nowhere else. New domains are new adapters, not forks.

**Fact.**
The atomic unit of knowledge in DEKE. A single claim with provenance metadata: source, confidence score, and optional temporal validity window. Facts carry the credibility of their source and an independently assessed confidence level.

**Federation.**
The capability for multiple DEKE instances to discover each other and serve cross-instance queries. Enables distributed knowledge networks where each instance maintains sovereignty over its own domains while participating in a broader knowledge ecosystem. Federation Phases 1--2 (discovery and search) are implemented; later phases are planned.

**InteractionId.**
A unique identifier generated before each advisory call and returned in the response. Used for interaction logging and traceability. Generated before the model call so that the record exists even if the call fails.

**Knowledge Compensation.**
The hypothesis that a richer knowledge base compensates for a less capable language model. As DEKE's knowledge deepens, smaller and cheaper models may become viable for more domains. This guides design decisions but requires empirical validation. See [overview.md](overview.md).

**Knowledge Gap.**
An area where DEKE's knowledge base lacks sufficient facts to support a confident advisory response. Identified through low coverage scores or low fact confidence in a topic area. Gaps are logged as demand signals for future knowledge acquisition.

**Provenance.**
The lineage of a fact: where it came from, when it was collected, and how credible the source is. Provenance metadata is tracked from the moment of ingestion.

**Source Credibility.**
A score (0.0--1.0) assigned to each knowledge source, representing how trustworthy content from that source is expected to be. Tracked independently from fact confidence --- a high-credibility source can still produce ambiguous facts, and facts from lower-credibility sources can be valuable.

**Temporal Validity.**
Optional timestamps on a fact indicating when it became true (valid_from) and when it ceased to be true (valid_until). Critical for domains where knowledge evolves --- regulatory changes, software version differences, seasonal conditions.

---

*For research terms related to self-improving systems (Curiosity Service, MAP-Elites, Prediction-Error Engine, Three-Signal Framework, TD Error, Goodhart's Law, Quality-Diversity), see [science/evolution-vision.md](../science/evolution-vision.md).*
