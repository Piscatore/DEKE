# Evolution Engine — Research Vision

This document describes a research direction for self-improving knowledge systems. It is not part of DEKE's active product model. The concepts here inform future work if and when DEKE has sufficient user volume and query data to make learning mechanisms viable.

For DEKE's current product model, see [product/overview.md](../product/overview.md). For the decision to defer this work, see [architecture/decisions.md](../architecture/decisions.md).

---

## Purpose

The Evolution Engine is a cross-cutting component responsible for knowing what "good" means and improving toward it. It observes how advisory responses are received, learns from the difference between predicted and actual quality, and directs knowledge acquisition toward gaps revealed by those differences.

The Evolution Engine does not generate advisory responses. It does not change language model weights. It learns three things that sit entirely within the system's own architecture: which facts are reliable as evidenced by real-world outcomes, which adapter configurations work for which query types, and where knowledge gaps exist that the Knowledge Base should prioritise filling.

## Core Insight

The brain does not reward success. It rewards surprise at success. The dopaminergic teaching signal in biological learning systems is the difference between what was predicted and what actually happened --- the Temporal Difference error. A system that consistently predicts its own quality accurately learns nothing, even if it is consistently excellent. The Evolution Engine is built on this principle.

A response that was predicted to be low-quality but received strong positive feedback generates a larger learning signal than one predicted to be excellent that received excellent feedback. The prediction model must be maintained alongside the performance model, and the delta between them is the engine of improvement.

## The Three-Signal Framework

The Evolution Engine's core defense against metric gaming is using three independent, triangulated signal tracks. No single track can be maximised at the expense of true quality without the others detecting the divergence.

| Signal Track | What It Measures | Strength | Weakness |
|---|---|---|---|
| **Explicit feedback** | Direct user ratings, corrections, and satisfaction signals. | Clear intent, direct measurement. | Risk of gaming if over-weighted; effort barrier reduces participation. |
| **Behavioural / implicit** | Observable user actions: query reformulation (negative signal), follow-up depth (positive signal), response adoption. | Captured passively, difficult to game, reflects actual behaviour. | Noisy, difficult to interpret without context; delayed availability. |
| **Veracity / objective** | Independent corroboration or contradiction of cited facts discovered over time. | Fully resistant to gaming; objective ground truth. | Delayed by nature; not always measurable for abstract advice. |

The three-track design is the architectural guard against Goodhart's Law: when a proxy metric becomes the optimisation target, it ceases to be a good proxy. No single metric is ever the sole optimisation target.

## The Prediction-Error Engine

Before each advisory response is served, the Evolution Engine computes an expected quality estimate based on observable properties of the knowledge state: the confidence and corroboration levels of retrieved facts, domain coverage, contradiction density, recency, and how well the current adapter configuration matches the query type. This predicted quality score is stored with the interaction record before the response is served.

After the response is served, three measurement windows open:

- **Immediate (seconds to minutes).** Explicit rating if offered. Did the user immediately reformulate the query? (negative signal)
- **Short-term (hours).** Did the user follow up with a deeper question on the same topic? (positive signal) Did the user correct a fact? (strong negative signal)
- **Delayed (one to three days).** A hindsight satisfaction probe: did the advice turn out to be useful in practice? This is the highest-value signal.

The learning signal is the signed difference between actual and predicted quality. This delta propagates backward through the causal chain to adapter configurations, retrieved facts, and sources.

## The Curiosity Service

The Curiosity Service is an intrinsic motivation engine. It operates continuously in the background, independently of user interactions, driving proactive knowledge acquisition.

The service maintains a domain question corpus, samples questions, attempts to answer them, and assesses answerability. The curiosity signal is the inverse of answerability --- high curiosity means a knowledge gap worth filling. High-curiosity topics are queued as priority harvesting targets.

Gap types: depth gaps (shallow corroboration), breadth gaps (adjacent topics unrepresented), recency gaps (stale facts), contradiction gaps (contested state), and blind spots (no facts at all).

## Adapter Evolution

Rather than a single adapter per domain, the Evolution Engine maintains an archive of adapter variants indexed by behavioural dimensions: query type, stakes level, and knowledge depth. Each cell holds the currently best-performing adapter variant for that niche.

Evolution operates through mutation (successful adapters spawn variants), competition (variants run in shadow mode against incumbents), pruning (consistently poor variants are deprecated), and preservation (one baseline variant per niche is always preserved).

## Prerequisites for Viability

This research direction becomes viable when the following conditions are met:

- **Multiple active domains** (3+) to provide diverse query patterns
- **Sufficient query volume** (50+ queries per day) to produce statistically meaningful signals
- **A measurable quality problem** that manual curation cannot solve
- **Interaction logging** capturing data that future learning mechanisms can consume

Until these conditions are met, the most effective approach is manual curation of knowledge quality and adapter configuration.

## Novel Contributions

**Established foundations:** TD error as learning signal, RLHF failure modes, quality-diversity algorithms (MAP-Elites), curiosity as intrinsic reward, hindsight feedback superiority.

**Novel in this design:** Applying TD error as a quality prediction architecture to a retrieval-based knowledge system; a quality-diversity adapter archive for advisory behavioural niches; a Curiosity Service as a self-directed knowledge acquisition driver.

## See Also

- [reinforcement-learning.md](reinforcement-learning.md) for the theoretical foundations.
- [papers.md](papers.md) for supporting research references.
- [architecture/decisions.md](../architecture/decisions.md) for the decision to defer this to research status.
