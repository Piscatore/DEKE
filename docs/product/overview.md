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

DEKE addresses the first four problems directly. Its knowledge base tracks source credibility, corroboration, temporal validity, and contradictions. It knows what it does not know. The fifth problem --- learning from outcomes --- is a research direction explored separately (see [Research Vision](#research-vision) below).

## The Knowledge Compensation Hypothesis

As DEKE's knowledge base deepens, the minimum capable language model decreases. A rich knowledge base compensates for a smaller, cheaper model. The cost curve bends favourably over time.

This hypothesis is directionally sound and guides design decisions, but it is unproven at scale. Richer context should compensate for a smaller model; grounding improvement should compound over time while model upgrades do not. Implementation decisions should ask: does this improve grounding quality? But the principle is treated as a design hypothesis to validate, not as immutable law.

## The Two-Package Architecture

DEKE is organised into two packages with clearly separated responsibilities.

| Package | Name | Responsibility | Direction |
|---|---|---|---|
| **P1** | Knowledge Base | What DEKE knows --- and how trustworthy it is. Ingests knowledge, tracks provenance, detects duplicates, scores confidence, identifies contradictions, maintains the knowledge store. | Inward-facing. Improves quality of what is stored. |
| **P2** | Knowledge Leverage | What DEKE can do with what it knows. Accepts advisory queries, returns grounded expert responses through a fixed external interface. Uses domain adapters for domain-specific behaviour. | Outward-facing. Converts stored knowledge into delivered value. |

**Federation** is a cross-cutting capability that spans both packages. It enables DEKE instances to discover each other, share knowledge, and route queries across domains without centralised coordination.

## How the Packages Interact

Package 1 provides facts and trust metadata to Package 2 on each advisory query. Package 2 uses that grounding to assemble and deliver advisory responses. The flow is one-directional: knowledge flows outward from storage to delivery. There is no feedback loop back from Package 2 to Package 1.

All inter-package communication is asynchronous and loosely coupled. Advisory response latency is independent of knowledge acquisition latency.

## Cross-Cutting Design Principles

These principles span both packages and guide architectural decisions.

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

## Research Vision

For research on self-improvement and evolution mechanisms, see [science/evolution-vision.md](../science/evolution-vision.md).
