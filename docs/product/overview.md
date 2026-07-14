# DEKE Overview

## What Is DEKE

DEKE --- Domain Expert Knowledge Engine --- is a knowledge engine that accumulates domain expertise over time and delivers it as grounded, trustworthy advisory responses. It is not a chatbot, not a search engine, and not a fine-tuned model. It is a system that knows what it knows, knows what it does not know, and exposes both through a consistent interface.

The core insight driving DEKE is that a language model's general reasoning capability is not the bottleneck in most advisory contexts. The bottleneck is grounded, domain-specific knowledge --- facts that are trustworthy, current, well-sourced, and organised. DEKE provides that grounding layer. The language model provides the reasoning and expression layer on top of it.

## The Problem DEKE Solves

AI-assisted knowledge work has a trust problem: language models are fluent but unreliable. They hallucinate, they express false confidence, and their knowledge is static. Retrieval-augmented generation helps, but basic retrieval has no concept of how trustworthy a retrieved fact is, whether it is current, whether it is corroborated, or whether it contradicts other facts in the same corpus.

DEKE addresses five specific problems:

| Problem | Description |
|---|---|
| **Static knowledge** | Knowledge frozen at training time. Cannot reflect recent developments. |
| **Hallucination risk** | The model invents plausible-sounding but incorrect facts, especially at domain edges. |
| **No trust differentiation** | A fact from a primary authoritative source is treated identically to speculation scraped from a forum. |
| **No self-awareness of gaps** | The system cannot tell you what it does not know. It will answer confidently regardless. |
| **No learning from outcomes** | Each interaction is independent. The system does not improve from feedback. |

DEKE addresses all five problems. Its knowledge base (Package 1) tracks source credibility, corroboration, temporal validity, and contradictions. It knows what it does not know. The fifth problem --- learning from outcomes --- is addressed by the Evolution Engine, which observes advisory outcomes and directs both knowledge acquisition and adapter behaviour toward the gaps that harm quality (see [Evolution Engine](#evolution-engine-in-depth) below).

## The Knowledge Compensation Hypothesis

As DEKE's knowledge base deepens, the minimum capable language model decreases. A rich knowledge base compensates for a smaller, cheaper model. The cost curve bends favourably over time.

This hypothesis is directionally sound and guides design decisions, but it is unproven at scale. Richer context should compensate for a smaller model; grounding improvement should compound over time while model upgrades do not. Implementation decisions should ask: does this improve grounding quality? But the principle is treated as a design hypothesis to validate, not as immutable law.

## The Three-Package Architecture

DEKE is organised into three packages with clearly separated responsibilities.

| Package | Name | Responsibility | Direction |
|---|---|---|---|
| **P1** | Knowledge Base | What DEKE knows --- and how trustworthy it is. Ingests knowledge, tracks provenance, detects duplicates, scores confidence, identifies contradictions, maintains the knowledge store. | Inward-facing. Improves quality of what is stored. |
| **P2** | Knowledge Leverage | What DEKE can do with what it knows. Accepts advisory queries, returns grounded expert responses through a fixed external interface. Uses domain adapters for domain-specific behaviour. | Outward-facing. Converts stored knowledge into delivered value. |
| **EE** | Evolution Engine | What DEKE learns from what it does. Observes advisory outcomes, computes prediction-error deltas across three triangulated signal tracks (explicit, behavioural, veracity), evolves domain adapter configurations, and directs knowledge acquisition toward the gaps that most harm advisory quality. | Feedback-facing. Closes the loop from delivered value back into stored knowledge and delivery behaviour. |

**Federation** is a cross-cutting capability that spans all three packages. It enables DEKE instances to discover each other, share knowledge, and route queries across domains without centralised coordination.

## How the Packages Interact

Package 1 provides facts and trust metadata to Package 2 on each advisory query. Package 2 uses that grounding to assemble and deliver advisory responses. On the per-query path, this flow is one-directional: knowledge flows outward from storage to delivery, and advisory response latency does not wait on anything downstream.

The Evolution Engine closes the loop on a slower cycle, outside the per-query path. It observes delivered responses and their real-world outcomes, then feeds two things back: knowledge-gap signals into Package 1, directing what to harvest next, and evolved adapter variants into Package 2, directing how to respond next. This is the one feedback loop in the architecture, and it runs over accumulated interaction history rather than per query.

All inter-package communication is asynchronous and loosely coupled. Advisory response latency is independent of knowledge acquisition latency and of the Evolution Engine's evolution cycle.

## Cross-Cutting Design Principles

These principles span all three packages and guide architectural decisions.

| Principle | What It Means |
|---|---|
| **Honesty constraint** | DEKE must never express more confidence than its knowledge justifies. Honest gap declaration is always preferable to a fluent but poorly-grounded response. No domain adapter may override uncertainty expression upward --- the confidence floor is enforced at the shared core level. |
| **Knowledge compensation** | A design hypothesis: as the knowledge base deepens, the advisory layer requires less from the language model. Richer context compensates for a smaller model. This guides decisions toward grounding investment, but remains subject to validation. |
| **Zero-cost priority** | All components default to zero marginal cost where possible. Local embeddings, local vector search, local language model when knowledge depth permits. External API calls are the only variable cost and should decrease as knowledge depth increases. |
| **Separation of domain logic** | Everything domain-specific lives in the domain adapter and nowhere else. The shared core, pipeline mechanics, trust interpretation, and uncertainty expression are domain-agnostic. New domains are adapters, not forks. |
| **Fixed external interface** | The advisory request/response contract is the public contract. It does not change in breaking ways. All consumers rely on this contract indefinitely. Internal evolution happens behind it. |

## Current Status

For detailed progress tracking, see [roadmap.md](../roadmap.md).

See also: [architecture/specification.md](../architecture/specification.md) for implementation details.

## Evolution Engine In Depth

The Evolution Engine's full design --- the three-signal framework, prediction-error engine, Curiosity Service, and adapter evolution mechanics --- is detailed in [science/evolution-vision.md](../science/evolution-vision.md).
