# DEKE Overview

## What Is DEKE

DEKE --- Domain Expert Knowledge Engine --- is a self-improving knowledge engine that accumulates domain expertise over time and delivers it as grounded, trustworthy advisory responses. It is not a chatbot, not a search engine, and not a fine-tuned model. It is a system that learns what it knows, knows what it does not know, and gets better at both.

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

DEKE addresses all five. Its knowledge base tracks source credibility, corroboration, temporal validity, and contradictions. It knows what it does not know. And through the Evolution Engine, it learns from every interaction --- getting better without manual retraining.

## The Knowledge Compensation Principle

As DEKE's knowledge base deepens, the minimum capable language model decreases. A rich knowledge base compensates for a smaller, cheaper model. The cost curve bends favourably over time.

This is the economic foundation of DEKE's viability. Richer context compensates for a smaller model. Implementation decisions should always ask: does this improve grounding quality? Grounding improvement compounds over time; model upgrades do not.

## The Three-Package Architecture

DEKE is organised into three packages with clearly separated responsibilities. Each package has a distinct direction and scope.

| Package | Name | Responsibility | Direction |
|---|---|---|---|
| **P1** | Knowledge Integrity | What DEKE knows --- and how trustworthy it is. Ingests knowledge, tracks provenance, detects duplicates, scores confidence, identifies contradictions, maintains the knowledge store. | Inward-facing. Improves quality of what is stored. |
| **P2** | Knowledge Leverage | What DEKE can do with what it knows. Accepts advisory queries, returns grounded expert responses through a fixed external interface. Uses domain adapters for domain-specific behaviour. | Outward-facing. Converts stored knowledge into delivered value. |
| **P3** | Evolution Engine | How DEKE gets better over time. Predicts response quality, measures actual quality through three independent signal tracks, propagates learning signals, drives knowledge acquisition. | Cross-cutting. Makes Packages 1 and 2 continuously better. |

## How the Packages Interact

The three packages form a closed feedback loop:

| From | Signal / Data | To |
|---|---|---|
| Package 1 | Facts and trust metadata retrieved on each advisory query | Package 2 |
| Package 2 | Interaction event: interaction identifier, cited fact identifiers, predicted quality, adapter used | Package 3 |
| Package 3 | Delta-driven reliability score updates for facts and sources | Package 1 |
| Package 3 | Adapter fitness scores for niche selection | Package 2 |
| Package 3 | Knowledge gap harvest directives from the Curiosity Service | Package 1 |

All inter-package communication is asynchronous and loosely coupled. No package blocks on another. Package 2 emits and forgets; Package 3 reads lazily. This keeps advisory response latency independent of learning cycle latency.

## Cross-Cutting Design Principles

These principles span all three packages and are treated as architectural invariants --- decisions that override local convenience.

| Principle | What It Means |
|---|---|
| **Honesty constraint** | DEKE must never express more confidence than its knowledge justifies. Honest gap declaration is always preferable to a fluent but poorly-grounded response. No domain adapter may override uncertainty expression upward --- the confidence floor is enforced at the shared core level. |
| **Knowledge compensation** | As the knowledge base deepens, the advisory layer requires less from the language model. Richer context compensates for a smaller model. Grounding improvement compounds over time; model upgrades do not. |
| **Zero-cost priority** | All components default to zero marginal cost where possible. Local embeddings, local vector search, local language model when knowledge depth permits. External API calls are the only variable cost and should decrease as knowledge depth increases. |
| **Goodhart protection** | No single metric is ever the sole optimisation target. Every quality signal is triangulated across at least two independent tracks. When a metric becomes the target it ceases to be a good measure --- this is enforced architecturally, not just as a guideline. |
| **Delta over absolute** | Learning signals are differences, not raw outcomes. A response that exceeded its predicted quality is more informative than one predicted to be excellent that was. The prediction model is as important as the measurement model. |
| **Separation of domain logic** | Everything domain-specific lives in the domain adapter and nowhere else. The shared core, pipeline mechanics, trust interpretation, and uncertainty expression are domain-agnostic. New domains are adapters, not forks. |
| **Fixed external interface** | The advisory request/response contract is the public contract. It does not change in breaking ways. All consumers rely on this contract indefinitely. Internal evolution happens behind it. |

## Current Status

| Component | Status |
|---|---|
| **Package 1 v1** | Functional. Ingest, learn, and serve pipeline operational. Semantic search via embeddings. Source monitoring for feeds and web pages. Basic deduplication (URL and content hash). Pattern discovery via embedding similarity. |
| **Design documentation** | All companion documents complete: Knowledge Integrity plan, Knowledge Leverage, Evolution Engine, and this master reference. |
| **Known gaps** | Package 1 v1 was built before the provenance and trust framework was designed. No source credibility scoring, no temporal validity, no corroboration tracking, no contradiction detection, no domain trust policies, and only shallow deduplication. These gaps are addressed by the first implementation phase. |
| **Packages 2 and 3** | In design. Implementation sequence defined. |

See also: [architecture/specification.md](../architecture/specification.md) for implementation details.
