# Knowledge Leverage (Package 2)

## Purpose

Knowledge Leverage is the outward-facing package of DEKE. Where Package 1 builds and maintains a trustworthy knowledge base, Package 2 transforms that knowledge into grounded expert advisory responses. It is the interface between DEKE's accumulated domain understanding and its consumers --- whether human users, AI coding sessions, or other agents.

Package 2's base reasoning capability comes from a general-purpose language model. Its domain competence comes from DEKE's accumulated knowledge. As the knowledge base deepens, the minimum capable model decreases --- the cost curve bends favourably over time.

## The Three-Layer Stack

Package 2 is organised into three layers with strictly defined responsibilities:

| Layer | Name | Responsibility |
|---|---|---|
| **Layer 1** | Fixed Interface | The public contract: an advisory request goes in, an advisory response comes out. This contract never changes. Every consumer --- whether API client, tool integration, or peer agent --- relies on this contract indefinitely. |
| **Layer 2** | Shared Core | Context assembly from DEKE facts, trust metadata interpretation, confidence banding, uncertainty expression language, model call mechanics, graceful degradation, and signal emission for the Evolution Engine. Built once, improves once. All domains benefit automatically. |
| **Layer 3** | Domain Adapter | The minimal, replaceable plugin: domain-specific prompt strategy, fact weighting, context formatting, trust calibration overrides, and escalation rules. Domain-specific logic lives here and only here. |

This separation is the most important design decision in Package 2. Without it, every new domain requires rebuilding the entire advisory pipeline. With it, a new domain is a small adapter with a handful of domain-specific behaviours. When the language model improves, the shared core is updated and all adapters benefit. When the trust model evolves, the shared core changes and all domains inherit the improvement.

## Advisory Pipeline

Every advisory call passes through thirteen stages in sequence:

1. **Request validation.** Confirm the domain exists and is activated (knowledge threshold met). Validate the query.
2. **Niche classification.** Classify the query by type and stakes level for adapter selection and quality tracking.
3. **Domain adapter resolution.** Resolve the appropriate domain adapter. If none is registered, use the default adapter.
4. **Fact retrieval.** Retrieve the most relevant facts from the Knowledge Base by semantic similarity, applying trust filters (minimum confidence, minimum corroboration).
5. **Adapter-weighted ranking.** The domain adapter re-ranks retrieved facts using domain-specific relevance signals beyond semantic similarity.
6. **Quality prediction.** Compute a predicted quality score from the metadata of the retrieved facts. Store this prediction for later comparison against actual quality.
7. **Context assembly.** The domain adapter formats the ranked facts into context for injection into the prompt.
8. **Trust calibration.** The domain adapter translates trust metadata into explicit guidance that the language model can act on --- for example, "multiple independent sources confirm this" or "sources disagree on this point."
9. **Prompt construction.** Combine the adapter's system prompt, trust guidance, assembled context, and the original query into the final prompt.
10. **Model call.** Dispatch the prompt to the selected language model backend. Capture token counts.
11. **Response assembly.** Wrap the model output in a structured response including confidence band, cited fact references, and identified knowledge gaps.
12. **Escalation check.** The domain adapter evaluates whether the query and draft response should be escalated (flagged for human review or routed to a more capable model).
13. **Signal emission.** Emit an interaction record to the Evolution Engine containing the interaction identifier, predicted quality, cited facts, and model used.

## Confidence Expression

DEKE expresses uncertainty in four discrete bands rather than raw decimal scores. This prevents false precision and gives consumers a clear, actionable signal.

| Band | Meaning | Consumer Guidance |
|---|---|---|
| **High** | Multiple corroborated, recent facts cover the query well. | Proceed with confidence. |
| **Medium** | Adequate coverage with some gaps or lower corroboration. | Useful, but verify for high-stakes decisions. |
| **Low** | Sparse facts, aged sources, or partial coverage. | Treat as a starting point; seek corroboration. |
| **Insufficient** | The knowledge base cannot support a grounded response. | Domain adapter policy applies --- see graceful degradation. |

## Uncertainty and Honesty

DEKE maintains a consistent vocabulary for expressing uncertainty across all domains. High-confidence knowledge is presented as direct assertion. Medium confidence is qualified. Low confidence carries explicit caveats. Insufficient knowledge produces an honest gap declaration: DEKE states what it does not know and suggests alternatives.

The honesty constraint is enforced at the shared core level. Domain adapters can make uncertainty language more conservative --- a high-stakes domain can add stronger disclaimers --- but no adapter can override uncertainty expression upward. The confidence floor is architecturally enforced, not merely a guideline.

## Domain Adapters

A domain adapter is the minimal, replaceable plugin that supplies everything domain-specific to the advisory pipeline. Each adapter provides:

- **A system prompt** establishing DEKE's role and behavioural rules within the domain.
- **A fact weighting strategy** that re-ranks retrieved facts using domain-specific relevance signals (for example, boosting recent facts in a fast-moving domain, or penalising contested facts in a high-stakes domain).
- **A context formatting strategy** that controls how facts are presented to the language model.
- **Trust calibration overrides** that adjust how trust metadata is translated into prompt guidance for this domain.
- **Escalation rules** that determine when a query or response should be flagged for human review or routed to a more capable model.
- **Activation criteria** defining the minimum viable knowledge state before the domain begins serving responses.

A default adapter is provided that delivers sensible behaviour for any domain without customisation. It is not a placeholder --- it is designed to be genuinely useful for low-stakes domains that do not warrant a custom adapter.

New domains are new adapters, not forks of the system. Everything that is not domain-specific is inherited from the shared core.

## Graceful Degradation

Package 2 handles three classes of "cannot answer" scenarios:

| Class | Scenario | Response Strategy |
|---|---|---|
| **Knowledge gap** | DEKE has some knowledge but not enough to ground a confident response. | Deliver a low-confidence response with explicit gap declaration. Flag the gap to the Evolution Engine's Curiosity Service as a priority harvest target. Never fabricate to fill the gap. |
| **Cold start** | The domain exists but has not yet met its activation criteria. | Return a structured status response showing current knowledge state versus the activation threshold. Suggest alternative resources. Log as a demand signal for the Knowledge Base. |
| **Unknown domain** | No adapter and no knowledge exist for this domain. | Offer to answer from general model knowledge (clearly flagged as ungrounded) if the caller permits, or return an explicit "outside DEKE scope" response. |

Domain policy governs degradation behaviour. A high-stakes domain adapter triggers escalation when confidence is low, preventing any low-confidence response from reaching the user without review. A casual domain permits low-confidence responses with appropriate hedging.

## Domain Activation

DEKE will not serve advisory responses for a domain that has not met its activation criteria. A system that answers poorly is worse than one that honestly says it cannot answer yet.

Each domain adapter specifies its own activation criteria: minimum fact count, minimum mean confidence, minimum number of distinct sources, and minimum coverage score (the fraction of domain topics with at least one relevant fact). Conservative defaults are provided --- better to delay activation than to serve poor-quality responses that erode trust.

An activation status endpoint exposes the current state of each domain, allowing monitoring and the Knowledge Base to understand how far each domain is from readiness.

## LLM Backend Selection

Package 2 supports multiple language model backends. The selection policy is driven by stakes level, domain adapter preference, and knowledge depth:

- **Default backend.** A fast, cost-effective model suitable for most interactions. Selected automatically when the knowledge base is rich enough to provide strong context.
- **Escalation backend.** A more capable model used when the domain adapter triggers escalation, when confidence is low and stakes are high, or when the caller explicitly requests it.
- **Local backend.** A zero-cost local model option for non-sensitive domains. Available only when the domain permits it and knowledge depth exceeds a high threshold --- the rich context compensates for the smaller model's reduced general capability.

As the Knowledge Base accumulates knowledge, the local model threshold becomes reachable for more domains. This is the knowledge compensation principle in action: a compounding cost reduction driven by knowledge depth.

## First Domain: Software Product Advisor

DEKE's first production domain is a software product advisor --- a self-referential domain where DEKE uses its own accumulated knowledge about software product design to advise on its own development. The bootstrap source is the design session history that produced DEKE's own specifications --- primary-source, human-curated knowledge with high initial confidence.

This domain has specific characteristics: knowledge evolves quickly (short validity windows), primary authoritative sources are available, contradictions are version-aware (a pattern correct for one version is not necessarily contradicted by a different recommendation for a later version), and the mix of opinions and objective facts requires careful confidence scoring.

## See Also

- [architecture/specification.md](../architecture/specification.md) for Package 2 architecture details, interface definitions, and phased delivery plan.
