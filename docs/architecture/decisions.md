# Architecture Decisions

Consolidated record of architecture decisions, guardrails, risk analysis, and open design questions for the DEKE project.

For the technical specification these decisions shaped, see [specification.md](specification.md). For theoretical foundations behind Package 3 choices, see [science/evolution-theory.md](../science/evolution-theory.md).

---

## Architecture Decision Records

Decisions captured during design sessions, recorded with date, decision, and rationale.

### 2026-03 Design Session Series

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-03 | Separate source credibility from fact confidence as orthogonal concerns | A fact from a high-credibility source can still be ambiguous or incorrectly extracted. A fact from an unverified source can be corroborated by five independent sources. Conflating them produces misleading trust signals. |
| 2026-03 | Track corroboration via independence fingerprint, not raw source count | Two articles from AP Wire appearing on 200 news sites are not 200 independent sources. The independence_fingerprint (registered domain + publisher identity) prevents single-source amplification. |
| 2026-03 | Use four confidence bands (High/Medium/Low/Insufficient) rather than raw decimal scores | Prevents false precision. Gives consumers a clear actionable signal. Raw scores are internal; bands are the external contract. |
| 2026-03 | Enforce honesty constraint at shared core level, not adapter level | No domain adapter may override uncertainty expression upward. A system that erodes user trust through overconfident responses undermines its own existence. This is the engineering implementation of epistemic integrity as self-preservation. |
| 2026-03 | Three-layer Package 2 architecture (fixed interface / shared core / domain adapter) | Without separation, every new domain requires rebuilding the entire advisory pipeline. With it, a new domain is a small adapter class (~100-200 lines). The fixed interface never changes; internal evolution happens behind it. |
| 2026-03 | Use TD error (prediction-error delta) as Package 3's learning signal, not raw outcome scores | A response that exceeded its predicted quality is more informative than one predicted to be excellent that was. The prediction model is as important as the measurement model. Delta over absolute is an architectural invariant. |
| 2026-03 | MAP-Elites archive for adapter configurations rather than single-best optimization | Quality-Diversity algorithms maintain diverse high-performing solutions per behavioral niche. The best adapter for exploratory design questions may be mediocre for factual lookup. Collapsing to a single "average best" adapter serves neither well. |
| 2026-03 | Three independent signal tracks for Package 3 (explicit + behavioral + veracity) | Core defense against Goodhart's Law. No single track can be maximized at the expense of true quality without the others detecting divergence. Triangulation is enforced architecturally, not as a guideline. |
| 2026-03 | Hindsight feedback (48-hour delayed probe) prioritized over immediate feedback | Hindsight feedback is systematically more accurate than foresight feedback. A user who has tried to apply advice knows whether it was actually useful, not just whether it sounded useful. |
| 2026-03 | AdvisoryRequest/AdvisoryResponse as the permanent public contract | All consumers (MCP tools, REST clients, future A2A agents) rely on this contract indefinitely. Breaking changes are prohibited. New fields are additive. |
| 2026-03 | Package 1 Phase 1 (provenance schema) is the critical path for all subsequent work | Schema changes before data are free; schema changes after data are expensive. Must be completed before significant data accumulates or bootstrap ingestion runs. |
| 2026-03 | Zero-cost priority: default to local inference everywhere | ONNX embeddings (local), pgvector (no hosted vector DB), Ollama (local LLM when knowledge depth permits). Anthropic API calls are the only variable cost and should decrease as knowledge depth increases. |

### 2026-03-13 Research Session

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-03-13 | Replace parameter perturbation with GEPA-derived reflective mutation for P3-5 adapter evolution | GEPA (Genetic-Pareto prompt optimizer) outperforms GRPO by 6% average using 35x fewer rollouts. Its reflective mutation (feed trajectory to LLM, diagnose failure, propose revision) is directly applicable to DEKE's adapter evolution. One Haiku call per mutation attempt is affordable under zero-cost constraints. |
| 2026-03-13 | Extend P3-1 interaction log to capture full trajectory (not just outcome metadata) | GEPA-style reflective mutation requires full interaction trajectory as input: retrieved fact content, full context window, LLM reasoning trace, intermediate retrieval scores. Outcome metadata alone is insufficient. This is a critical path dependency. |
| 2026-03-13 | Add Response Audit Record to P3-1 persistence design | Every advisory response persisted with: adapter variant ID, fact IDs and confidence scores at retrieval time, LLM model used, stakes classification, raw LLM output. Append-only, never modified by evolution processes. Minimum 90-day retention. Prevents the "moral crumple zone" (attributing bad outcomes to the system without traceability). |
| 2026-03-13 | DEKE's architecture maps to the MASE paradigm (Multi-Agent Self-Evolving) | The Self-Evolving AI Agents Survey (arXiv:2508.07407) establishes a unified framework. DEKE sits at the MAO-to-MASE transition. The Three Laws (Endure > Excel > Evolve) are the academic expression of DEKE's own priority ordering. |
| 2026-03-13 | Package 1's provenance schema is a genuine academic differentiator | The survey treats structured knowledge with trust metadata as an underexplored problem. DEKE's provenance and confidence schema is a direct solution to this gap. |
| 2026-03-13 | ES paper validates MAP-Elites as anti-Goodhart mechanism | Evolution Strategies at Scale (arXiv:2509.24372) demonstrates that optimizing a solution distribution rather than a single point provides reward hacking resistance. MAP-Elites achieves this at the adapter level. |
| 2026-03-13 | Niche competition suffices for credit assignment in P3-5 | The survey confirms credit assignment via niche competition is state-of-the-art. Explicit causal tracing is not required; niche fitness is sufficient. |

### 2026-04 Federation Design

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-04 | Locality-weighted scoring for federated results (local 1.0, peer 0.8) | Local facts are managed by this instance with full provenance tracking. Peer facts are outside this instance's control. A slight discount ensures local knowledge is preferred when available. |
| 2026-04 | Loop prevention via visited set + hop limit + request ID + timeout | Four complementary mechanisms make infinite loops structurally impossible. Any single mechanism could fail; the combination is robust. |
| 2026-04 | Federation is read-only (no federated writes) | Facts are only created locally. This preserves provenance integrity and prevents distributed consistency problems. Query-time delegation is sufficient for Phase 1-3. |
| 2026-04 | Bundle Phase 3 MCP tools with Phase 2 federated search | MCP tools are the primary consumer interface. Deploying federated search without updating MCP tools creates a capability gap. Bundling ensures consumers benefit immediately. |

---

## Guardrails and Risk Analysis

### Risk Surface

DEKE gives advisory responses grounded in a knowledge base with confidence banding, gap disclosure, and cited fact IDs. The following risks emerge from that behavior:

| Risk | Description |
|------|-------------|
| Outdated facts presented confidently | A fact accurate at ingestion may degrade over time. Confidence score reflects corroboration quality, not recency. Temporal validity catches this eventually but not immediately. |
| Corroborated but incorrect facts | Multiple sources can all be wrong. High confidence + high corroboration does not guarantee truth. The veracity signal track catches drift but not initial state. |
| Scope creep on misconfigured adapters | If ActivationCriteria or domain scope is poorly configured, adapter may attempt answers outside its knowledge. |
| LLM hallucination above retrieved facts | Even with grounded context, the LLM can introduce content not present in retrieved facts. Classic RAG failure mode. |
| Stakes misclassification | If consumer passes incorrect stakes hint and no override mechanism exists, wrong model is used and curator escalation may not trigger. |
| Adapter evolution introduces overconfidence | An evolved adapter variant that scores higher on quality metrics may simultaneously be more prone to overconfident responses on borderline-knowledge queries. |
| Response accountability gap | Without a full audit trail, bad advisory responses cannot be traced to the specific adapter variant, retrieved facts, or evolution history that produced them. |
| Trajectory data governance | P3-1 interaction logs will contain sensitive query intent signals. No retention, access, or usage policy currently specified. |

### Existing Mitigations

These risks are addressed in the current design:

1. Honesty constraint enforced at shared core -- adapters cannot override it.
2. ConfidenceBand.Insufficient produces explicit refusal rather than low-quality response.
3. KnowledgeGaps field is mandatory in AdvisoryResponse -- consumer always sees what system did not know.
4. CitedFactIds always returned -- consumer can inspect source facts.
5. Activation criteria prevent domain from serving responses below knowledge thresholds.
6. AdvisoryRequest accepts stakes hint from consumer -- consumer can escalate manually.
7. WasEscalated flag in response contract.

### Proposed Guardrails (G1-G5)

#### G1: Adapter Evolution Safety Gate

**Target**: Package 3, Phase P3-5.

Before promoting a MAP-Elites variant from shadow to live, assert that the honesty constraint score has not degraded. Define honesty constraint score as the ratio of Insufficient or Low confidence responses on borderline-knowledge queries. A variant that increases confident-but-wrong responses is disqualified even if overall quality score is higher. This gate is separate from and runs before the MAP-Elites fitness comparison.

#### G2: Response Audit Record

**Target**: Package 3, Phase P3-1 (alongside trajectory logging).

Every advisory response persisted with: adapter variant ID, fact IDs and their confidence scores at time of retrieval, LLM model used, stakes classification, raw LLM output before post-processing. The audit record is append-only and must not be modified by adapter evolution processes. Retention policy: minimum 90 days; longer for curator-reviewed responses.

#### G3: Trajectory Data Governance Policy

**Target**: Package 3, Phase P3-1 (new subsection).

Specify for P3-1 trajectory log: what is retained (query intent, retrieved fact content, context window, reasoning trace), for how long, at what granularity. Distinguish data eligible for adapter evolution (interactions with outcome signals) from data that should be discarded (queries with no signal or containing sensitive intent). User query content should not be stored in raw form beyond the interaction window -- distill to signals before persistence. Access policy: trajectory data used only for adapter evolution, never for knowledge base population.

#### G4: Stakes Floor

**Target**: Open item, deferred until P2-1 adapter configuration design is complete.

If a query contains domain signals indicating production systems, financial decisions, or safety-adjacent topics, stakes should be floored at High regardless of the consumer-supplied hint. This is adapter-level domain knowledge and belongs in ShouldEscalate() logic. Specific trigger signals are domain-dependent and should be specified per adapter at configuration time.

#### G5: Contradiction Visibility

**Target**: Package 2, Phase P2-2.

When Package 1 detects a contradiction between facts and selects the higher-confidence one, the advisory response must flag "conflicting information exists on this topic." This applies even when the served response is high-confidence. The ContainsConflictingEvidence field is added to AdvisoryResponse. Consumer can use this flag to surface uncertainty to the end user or trigger manual review.

### GEPA Component Mapping to DEKE

This mapping documents how the GEPA framework (arXiv:2507.19457) translates to DEKE's adapter evolution mechanism:

| GEPA Component | DEKE Equivalent |
|----------------|-----------------|
| System with prompts | Domain adapter: SystemPrompt(), WeightFacts(), FormatContext() |
| Training dataset | AdvisoryInteractionEvent archive per niche |
| Evaluation metric | Predicted vs. actual quality delta |
| Feedback function | Three-signal framework (explicit + behavioral + veracity) |
| Rollout | One advisory interaction cycle |
| Pareto frontier | MAP-Elites archive grid |
| Reflective mutation | LLM-generated adapter variant from failure diagnosis |

### Consolidated Action Items from Guardrails Session

#### Critical Path (must resolve before first advisory call)

| Action | Target |
|--------|--------|
| Extend P3-1 interaction log to capture full trajectory (retrieved fact content, full context window, LLM reasoning trace, intermediate retrieval scores) | Package 3 P3-1 specification |
| Add Response Audit Record to P3-1 persistence design (adapter variant ID, fact confidence scores at retrieval time, model used, stakes, raw output) | Package 3 P3-1 specification |
| Add Trajectory Data Governance subsection to P3-1 (retention, access policy, distillation rules) | Package 3 P3-1 specification |

#### Design Amendments (incorporate at next document edit)

| Action | Target |
|--------|--------|
| Replace parameter perturbation with GEPA-derived reflective mutation mechanism in P3-5 | Package 3 P3-5 |
| Add honesty constraint safety gate to P3-5 promotion criteria (G1) | Package 3 P3-5 |
| Add ContainsConflictingEvidence field to AdvisoryResponse (G5) | Package 2 P2-2 |
| Add Three Laws framing (Endure > Excel > Evolve as MASE paradigm) | Master documentation |
| Add honesty constraint framing as epistemic integrity / self-preservation | Master documentation |
| Add ES paper citation as validation of MAP-Elites population approach | Package 3 P3-5 rationale |
| Cite survey as validation of three-signal feedback design and niche competition credit assignment | Package 3 P3-5 |

#### Open Items (add to backlog)

| Item | Notes |
|------|-------|
| ES-style update rules as alternative to delta-propagation for adapter evolution | Investigate at P3-5 design time |
| Expand OI-07: version-aware contradiction resolution without ground truth is an unsolved research problem | Needs deeper design before P1-3 |
| Elevate OI-09: Curator Workflow is an accountability mechanism, not just a quality gate | EvoAgentX confirms HITL is necessary in production |
| Stakes Floor: domain-signal-triggered escalation floor at High | Deferred until P2-1 |
| "optimize_anything" generalization: evolve fact weighting and retrieval strategies, not just prompts | Long-range consideration |

---

## Open Design Questions

Ten deferred design items organized by group. Each has a description, why it was deferred, and a trigger condition for revisiting. None block current implementation -- they are opportunities, not debts.

### Group A: Within-Package Questions

These are gaps within already-specified packages requiring design work before implementation is complete.

#### OI-01: Curiosity Blockade

**Category**: Package 3, Curiosity Service. **Priority**: Medium.

The Curiosity Service drives self-directed knowledge acquisition by self-querying DEKE and measuring answerability. This works once the domain has enough knowledge to generate meaningful self-test questions. At absolute cold start, or when DEKE encounters a topic so unknown it cannot form useful questions, the curiosity loop can stall: DEKE asks itself a question, retrieves nothing, scores answerability as zero, flags it as high-priority -- but harvesting returns only tangentially related low-confidence facts, and the curiosity score barely moves. This is directly analogous to the curiosity blockade observed in game-playing RL agents (Pathak et al.).

Candidate approaches: seeding with expert-authored starter questions, broader web search fallback when retrieval returns nothing, or a minimum exploration budget that forces harvesting even when the curiosity signal is weak.

**Why deferred**: No production domain has hit this problem yet. It is a theoretical gap.

**Revisit when**: First domain reaches a topic cluster where three consecutive curiosity-driven harvest cycles return near-zero improvement in answerability.

#### OI-02: Domain Question Corpus Seeding

**Category**: Package 3, Curiosity Service. **Priority**: Medium.

The Curiosity Service requires a domain question corpus to run its self-query loop. The Package 3 design notes this corpus can be seeded manually and extended automatically. What is missing is a concrete seeding strategy: who authors initial questions, how many are needed, can an LLM generate them from a domain description, and should the corpus be versioned alongside the knowledge base.

For the Software Product Advisor, the bootstrap design sessions provide a natural source. For future domains this is not guaranteed.

**Why deferred**: The Software Product Advisor bootstraps naturally. The general case is not urgent until a second domain is activated.

**Revisit when**: Second domain is planned for activation and has no natural question corpus source.

#### OI-03: Cross-Domain Adapter Transfer

**Category**: Package 3, Adapter Evolution. **Priority**: Low (now) / Medium (later).

Related domains likely share adapter patterns. A good fact-weighting strategy for software architecture may transfer to a DevOps domain. Cross-domain transfer would allow a new domain to start with battle-tested adapter variants rather than evolving from the default adapter from scratch. The MAP-Elites structure supports this mechanically -- variants can be copied across archives. What is not designed is the transfer policy: which domains are "related" enough, how transferred variants are marked, and how their fitness is initialized.

**Why deferred**: Only one domain exists. Requires at least two domains with overlapping behavioral niches.

**Revisit when**: Two or more activated domains show correlated positive deltas on similar query types.

#### OI-04: Multi-Turn Conversation Continuity

**Category**: Package 2, Advisory Pipeline. **Priority**: Medium.

AdvisoryRequest includes SessionId and PriorExchanges for multi-turn continuity. The contract exists but the mechanics are not designed: how many prior exchanges to include (context window limits), how cited facts from earlier turns are carried forward, where session state is stored, and how fact citations are deduplicated across a session.

**Why deferred**: MCP tool usage is single-turn. Multi-turn conversational interface is not the first consumer.

**Revisit when**: A conversational UI or chat interface calls Package 2 in a session context, or a consumer passes PriorExchanges in API calls.

### Group B: Research Threads

These surfaced during the Package 3 research phase but were not developed into concrete design.

#### OI-05: Debate-and-Critique Feedback

**Category**: Package 3, Veracity Signal. **Priority**: Low (research interest).

When a contested fact is identified (contradictions detected, low corroboration), DEKE could instantiate a mini-debate: one model instance argues for the fact, another against. An evaluator judges the debate. The outcome updates the fact's corroboration and contested status. This is more expensive than passive feedback (multiple LLM calls per debate) but applicable to exactly the cases where passive feedback is weakest -- highly contested facts where user signals are noisy and biased.

**Why deferred**: The veracity signal track is not yet implemented. Debate-and-critique enhances a mechanism that does not yet exist.

**Revisit when**: P3-4 (Hindsight Loop) is running and identifies persistently contested facts that passive signals cannot resolve.

#### OI-06: Social Proof as Corroboration Signal

**Category**: Package 3, Feedback Framework. **Priority**: Low.

When multiple independent users receive the same advisory response and provide consistent feedback, that consistency is itself a form of corroboration of the response as a whole. If 10 independent users ask similar questions and 8 provide positive signals, that convergence is stronger than any single user's feedback. This requires identifying "substantially similar" queries, grouping interactions, and computing consensus signals.

The main risk is that social proof can encode majority biases. Must be weighted against the objective veracity track.

**Why deferred**: DEKE does not yet have enough user interactions to form meaningful consensus groups.

**Revisit when**: A domain accumulates 50+ interactions on similar query clusters.

#### OI-07: Version-Aware Contradiction Resolution

**Category**: Package 1, Quality Pipeline. **Priority**: Medium.

A .NET 6 recommendation is not contradicted by a .NET 9 recommendation -- they are versioned facts applicable to different contexts. Current contradiction detection operates on semantic similarity without version awareness, producing false-positive contradictions in version-rich domains. What is needed: a version tagging schema (applies_to_version, deprecated_after_version), version-scoped contradiction detection, and a query-time version resolver. Version relationships are domain-specific (SemVer for software, jurisdiction+date for legal, guideline versions for medicine).

The Self-Evolving AI Agents Survey (arXiv:2508.07407) confirms that version-aware contradiction resolution without ground truth is an active unsolved research problem, not merely a deferred detail.

**Why deferred**: Requires version tagging schema (Package 1 Phase 1). Detection logic can be added in Phase 2.

**Revisit when**: The Software Product Advisor returns false-positive contradiction warnings on version-progressive patterns, or P1-Phase 2 is being implemented.

### Group C: Structural Features

Larger features requiring their own design sessions.

#### OI-08: Fact Retirement vs Archival

**Category**: Package 1, Knowledge Integrity. **Priority**: Medium.

When a fact ages past valid_until, is superseded, or is marked incorrect -- what happens? The historical record of what was believed at a given time is itself valuable: understanding pattern evolution requires retaining history, past advisory responses were grounded in those facts, and old recommendations may still apply to legacy systems.

Design needed: retired_at timestamp, retired_reason field, query-time filter excluding retired facts by default, explicit "include historical" flag, and a retention policy.

**Why deferred**: No facts have yet been retired.

**Revisit when**: The first domain's facts begin approaching their valid_until dates, or P1-Phase 2 (temporal validity) is being implemented.

#### OI-09: Curator Workflow

**Category**: Package 2 + Package 3, Human-in-the-Loop. **Priority**: Medium.

ShouldEscalate() flags responses for human review, but there is no queue, UI, notification, or workflow for a curator to review, approve/modify, and release escalated responses. For the Software Product Advisor at Medium stakes with a single user, this is not critical. For high-stakes domains, a genuine curator workflow is required: review queue, curator interface, approval/modification/rejection actions, curator decisions fed back to Package 3 as high-weight explicit feedback, and response delivery only after curator approval.

The curator workflow is an accountability mechanism, not merely a quality gate. EvoAgentX confirms HITL checkpoints are necessary in production. The responsibility chain must be explicitly assigned: adapter variant, curator, domain owner. This prevents the "moral crumple zone" described by Bozkurt (2025).

**Why deferred**: No high-stakes domain is currently planned.

**Revisit when**: A high-stakes domain (legal, medical, financial) is planned, or multi-user deployment is considered.

#### OI-10: A2A Exposure

**Category**: Package 2, External Interface. **Priority**: Low (now) / Medium (later).

The A2A (Agent-to-Agent) protocol would make DEKE discoverable as a peer agent in multi-agent ecosystems. Package 2's fixed interface contract is designed to be A2A-compatible without structural changes -- the addition would be a thin protocol wrapper. However, A2A has real operational overhead: agent card, discovery registration, protocol version compatibility.

Precise trigger conditions:

- A second external consumer (not Claude Code) wants to call DEKE in an automated multi-agent pipeline, OR
- DEKE needs to discover and call other specialist agents as part of its own advisory pipeline, OR
- The A2A spec reaches a stable 1.0 release and operational overhead reduces to acceptable levels.

**Why deferred**: MCP covers all current consumers. A2A adds overhead without a concrete consumer.

**Revisit when**: Any of the three trigger conditions above is met.

### Review Schedule

| Milestone | Items to Review |
|-----------|-----------------|
| After Package 1 Phase 2 (Quality Pipeline) | OI-07 (Version-Aware Contradiction), OI-08 (Fact Retirement) |
| After Package 2 Phase 3 (Software Product Advisor activated) | OI-04 (Multi-Turn), OI-02 (Corpus Seeding) |
| After Package 3 Phase P3-2 (Prediction Model running) | OI-01 (Curiosity Blockade), OI-03 (Cross-Domain Transfer) |
| After 50+ user interactions on a single domain | OI-06 (Social Proof), OI-05 (Debate-and-Critique) |
| Before any high-stakes domain activation | OI-09 (Curator Workflow) -- mandatory |
| When second external consumer requests integration | OI-10 (A2A Trigger Conditions) |
