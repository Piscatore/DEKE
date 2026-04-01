# Evolution Engine (Package 3)

## Purpose

The Evolution Engine is the cross-cutting package responsible for DEKE knowing what "good" means and improving toward it. It observes how advisory responses from Package 2 are received, learns from the difference between predicted and actual quality, and directs Package 1's knowledge acquisition toward gaps revealed by those differences.

The Evolution Engine does not generate advisory responses. It does not change language model weights. It learns three things that sit entirely within DEKE's own architecture: which facts are reliable as evidenced by real-world outcomes, which adapter configurations work for which query types, and where knowledge gaps exist that the Knowledge Base should prioritise filling.

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

Domain trust policies govern signal weighting. A high-stakes domain applies maximum weight to the veracity track and minimal weight to explicit approval. A casual domain can rely more heavily on explicit and behavioural signals.

The three-track design is DEKE's architectural guard against Goodhart's Law: when a proxy metric becomes the optimisation target, it ceases to be a good proxy. No single metric is ever the sole optimisation target.

## The Prediction-Error Engine

Before each advisory response is served, the Evolution Engine computes an expected quality estimate based on observable properties of the knowledge state: the confidence and corroboration levels of retrieved facts, domain coverage, contradiction density, recency, and how well the current adapter configuration matches the query type. This predicted quality score is stored with the interaction record before the response is served.

After the response is served, three measurement windows open:

- **Immediate (seconds to minutes).** Explicit rating if offered. Did the user immediately reformulate the query? (negative signal)
- **Short-term (hours).** Did the user follow up with a deeper question on the same topic? (positive signal) Did the user correct a fact? (strong negative signal)
- **Delayed (one to three days).** A hindsight satisfaction probe: did the advice turn out to be useful in practice? This is the highest-value signal --- a user who has tried to apply advice knows whether it was actually useful, not just whether it sounded useful.

The learning signal is the signed difference between actual and predicted quality. This delta propagates backward through the causal chain:

- **Adapter configuration.** Positive delta increases the adapter variant's fitness in its niche. Negative delta decreases fitness and spawns mutant variants for exploration.
- **Retrieved facts.** Positive delta increments each contributing fact's reliability score. Negative delta flags facts for re-verification and decrements reliability.
- **Sources.** Positive delta increments source credibility. Negative delta flags the source for re-assessment.
- **Knowledge gaps.** High negative delta with low domain coverage generates a curiosity signal directing the Knowledge Base to acquire more knowledge in the affected area.

## The Curiosity Service

The Curiosity Service is DEKE's intrinsic motivation engine. It operates continuously in the background, independently of user interactions, driving proactive knowledge acquisition.

The service maintains a domain question corpus --- a set of representative questions that a knowledgeable user of the domain might ask. It samples questions from this corpus, attempts to answer them using DEKE's knowledge base, and assesses answerability: fact count, mean confidence, corroboration, contradiction density, and coverage. The curiosity signal is the inverse of answerability --- high curiosity means low answerability means a knowledge gap worth filling.

High-curiosity topics are queued to the Knowledge Base as priority harvesting targets. The service tracks whether recent harvesting has reduced the curiosity signal for a topic, closing the feedback loop.

The Curiosity Service classifies gaps before dispatching harvest directives, because different gap types require different acquisition strategies:

| Gap Type | Description |
|---|---|
| **Depth gap** | Facts exist but are shallow or poorly corroborated. Directive: find more primary sources on the same topic. |
| **Breadth gap** | Good depth on known sub-topics but adjacent sub-topics are unrepresented. Directive: explore related areas. |
| **Recency gap** | Facts exist but are stale. Directive: re-verify existing sources; find recent sources. |
| **Contradiction gap** | Multiple facts exist but are in contested state. Directive: find authoritative resolution. |
| **Blind spot** | No facts exist at all. Highest curiosity signal. Directive: broad exploration harvest. |

## Adapter Evolution

Package 2 introduced the domain adapter as a replaceable plugin. The Evolution Engine adds the evolutionary mechanism that allows adapters to improve over time without manual intervention.

Rather than a single adapter per domain, the Evolution Engine maintains an archive of adapter variants indexed by behavioural dimensions: query type (factual lookup, exploratory design, verification, analogical reasoning, prediction), stakes level (low, medium, high), and knowledge depth (sparse, rich). Each cell in this grid holds the currently best-performing adapter variant for that niche.

When a query arrives, the Evolution Engine classifies it into a niche and selects the appropriate adapter variant. This replaces the "one adapter for everything" approach with a behavioural repertoire.

Evolution operates through four mechanisms:

- **Mutation.** Successful adapters spawn variants with small perturbations to prompt emphasis, fact weighting strategy, and context assembly order.
- **Competition.** New variants run in shadow mode --- their output is generated but not served to the user --- alongside incumbents. Variants that achieve higher mean delta over a sufficient number of interactions replace incumbents.
- **Pruning.** Variants that consistently produce negative deltas are deprecated after a grace period.
- **Preservation.** Regardless of performance, one variant per niche is always preserved as a reference baseline. The system cannot optimise itself into a local minimum by discarding the starting point.

Shadow mode is critical. New adapter variants must not serve users until they have demonstrated improvement. This prevents deploying degraded configurations during the optimisation process.

## The Complete Evolution Loop

The Evolution Engine integrates all components into a continuous self-improvement cycle operating across three time horizons:

### Interaction Loop (seconds to minutes)

Classify the query into a behavioural niche. Select the best adapter variant for that niche. Retrieve facts and assemble context. Predict quality from fact metadata. Serve the response. Capture immediate feedback signals.

### Learning Loop (hours to days)

Collect delayed signals including hindsight probes for high-uncertainty interactions. Compute the delta between actual and predicted quality. Propagate the delta to fact reliability scores, source credibility, and adapter fitness. Evolve adapters: spawn variants from high-fitness incumbents, deprecate low-fitness variants. Recalibrate the prediction model on recent delta history.

### Growth Loop (days to weeks)

Run the Curiosity Service's self-query loop to produce a knowledge gap taxonomy. Issue harvest directives to the Knowledge Base for priority knowledge acquisition. Ingest, deduplicate, and score new facts. Re-assess curiosity on harvested topics to measure gap closure. Produce a domain health report: answerability percentage, delta trends, adapter fitness distribution, and top curiosity gaps.

## Novel Contributions

The Evolution Engine's design draws on established research --- temporal difference learning, reinforcement learning from feedback, quality-diversity algorithms, and curiosity as intrinsic reward --- but applies them in novel combinations:

**Established foundations:**
- Temporal difference error as a learning signal (well-established neuroscience).
- Failure modes of reinforcement learning from human feedback (extensively documented).
- Quality-diversity algorithms such as MAP-Elites (active research, well-understood algorithmics).
- Curiosity as intrinsic reward (established in reinforcement learning research).
- Superiority of hindsight feedback over foresight feedback (demonstrated empirically).

**Novel in this design:**
- Applying temporal difference error as a quality prediction architecture to a retrieval-based knowledge system, where predicted quality is computed from fact metadata and the delta drives backward credit assignment through the retrieval chain.
- A quality-diversity adapter archive for advisory behavioural niches, where adapter configurations are evolved per niche rather than optimised globally.
- A Curiosity Service as a self-directed knowledge acquisition driver, using answerability prediction error as an intrinsic curiosity signal to generate harvest directives without user demand.

## See Also

- [science/reinforcement-learning.md](../science/reinforcement-learning.md) for the theoretical foundations of temporal difference learning, Goodhart's Law, and quality-diversity algorithms.
- [architecture/specification.md](../architecture/specification.md) for Evolution Engine design details and phased delivery plan.
