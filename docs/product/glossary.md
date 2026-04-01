# Glossary

**Adapter.**
See *Domain Adapter*.

**Cold Start Threshold.**
The minimum knowledge state a domain must reach before DEKE will serve advisory responses. Defined per domain through activation criteria including minimum fact count, minimum mean confidence, minimum distinct sources, and minimum topic coverage. A domain below its cold start threshold returns an honest status response rather than a poorly grounded answer.

**Confidence Band.**
One of four discrete levels of uncertainty that DEKE expresses in advisory responses: High, Medium, Low, or Insufficient. Derived from predicted quality scores based on retrieved fact metadata. Confidence bands replace raw decimal scores to prevent false precision and give consumers a clear, actionable signal.

**Corroboration.**
The number of independent sources that assert the same or semantically equivalent claim. Higher corroboration increases confidence. Source independence is tracked to prevent single-source amplification --- multiple reprints of the same original do not count as independent corroboration.

**Curiosity Service.**
The Evolution Engine component that continuously self-queries DEKE's knowledge base with representative domain questions, measures answerability, and generates harvest directives for the Knowledge Base to fill identified gaps. The curiosity signal is the inverse of answerability --- high curiosity means a knowledge gap worth filling.

**Delta / TD Error.**
Temporal Difference error --- the signed difference between predicted and actual quality for a given advisory interaction. The core learning signal in the Evolution Engine. A positive delta means the response was better than expected; a negative delta means it was worse. Borrowed from the neuroscience of dopaminergic reward signalling.

**Domain.**
A bounded knowledge area within DEKE. Each domain has its own trust policy, adapter configuration, activation criteria, and knowledge corpus. Examples include a software product advisor domain or a fishing knowledge domain. Domains are independent --- new domains do not require changes to the core system.

**Domain Adapter.**
The replaceable, domain-specific plugin that supplies custom prompt strategy, fact weighting, context formatting, trust calibration overrides, and escalation rules to the advisory pipeline. Everything domain-specific lives in the adapter and nowhere else. New domains are new adapters, not forks.

**Domain Trust Policy.**
A per-domain configuration that governs how strictly provenance is enforced. Controls which source tiers are auto-accepted, the minimum corroboration count for auto-acceptance, whether temporal validity is required, and the minimum confidence score for entry into the review queue. Ranges from permissive (casual hobby domain) to strict (legally critical domain).

**Fact.**
The atomic unit of knowledge in DEKE. A single claim with provenance metadata: source, confidence score, corroboration count, temporal validity window, contradiction flag, and trust state. Facts are immutable once created; changes produce new versions with the full history preserved.

**Federation.**
The planned capability for multiple DEKE instances to discover each other, exchange knowledge, and serve cross-instance advisory queries. Enables distributed knowledge networks where each instance maintains sovereignty over its own domains while participating in a broader knowledge ecosystem.

**Goodhart's Law.**
The principle that when a proxy metric becomes the optimisation target, it ceases to be a good proxy. The Evolution Engine guards against this by triangulating quality signals across three independent tracks (explicit, behavioural, veracity). No single metric is ever the sole optimisation target.

**Harvest Directive.**
An instruction from the Evolution Engine's Curiosity Service to the Knowledge Base, directing it to acquire knowledge in a specific area. Harvest directives are typed by gap classification: depth gaps, breadth gaps, recency gaps, contradiction gaps, and blind spots each require different acquisition strategies.

**InteractionId.**
A unique identifier generated before each advisory call and returned in the response. The thread that connects the initial quality prediction, the served response, and all subsequent feedback signals into a single traceable interaction record. Generated before the model call so that the record exists even if the call fails.

**Knowledge Compensation.**
The principle that a richer knowledge base compensates for a less capable language model. As DEKE's knowledge deepens, smaller and cheaper models become viable for more domains. This is the economic foundation of DEKE's long-term cost structure --- grounding improvement compounds over time; model upgrades do not.

**Knowledge Gap.**
An area where DEKE's knowledge base lacks sufficient facts to support a confident advisory response. Identified through low coverage scores, low answerability in Curiosity Service self-queries, or high negative deltas on advisory interactions. Gaps are classified by type (depth, breadth, recency, contradiction, blind spot) to guide appropriate acquisition strategies.

**MAP-Elites.**
A quality-diversity algorithm used by the Evolution Engine to maintain a diverse archive of adapter configurations, one per behavioural niche. Rather than optimising for a single best adapter, MAP-Elites maintains the best-performing variant for each combination of query type, stakes level, and knowledge depth. Evolution operates through mutation, competition, pruning, and preservation.

**Prediction-Error Engine.**
The core mechanism of the Evolution Engine that transforms outcome measurement into a structured learning signal. Predicts expected response quality before each advisory call, measures actual quality after through three signal tracks, and computes the delta. The delta propagates backward to credit or penalise contributing facts, sources, and adapter configurations.

**Provenance.**
The full lineage of a fact: where it came from, when it was collected, how credible the source is, how it was extracted, and how many independent sources corroborate it. Provenance metadata is tracked from the moment of ingestion and maintained throughout the fact's lifecycle.

**Quality-Diversity.**
A family of algorithms that maintain a diverse archive of high-performing solutions rather than converging on a single optimum. The insight is that the best solution for one behavioural niche may be mediocre for another. In DEKE, quality-diversity principles govern adapter evolution through the MAP-Elites archive.

**Three-Signal Framework.**
The Evolution Engine's approach to quality measurement using three independent, triangulated signal tracks: explicit feedback (direct user signals), behavioural/implicit feedback (observable user actions), and veracity/objective feedback (independent fact verification over time). The three-track design is the architectural guard against Goodhart's Law.
