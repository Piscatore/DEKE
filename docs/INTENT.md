# Intent

Distilled design intentions behind DEKE, one section per major intention,
sourced from planning artifacts under `thoughts/shared/` rather than
re-interviewing Mikael from scratch. Produced by the `/overhaul` subproject
(see `/overhaul/OVERHAUL-SKETCH-v0.2.md` §3.3). Anything synthesized rather
than directly stated in a source is flagged `(inferred, unconfirmed)` for
confirmation at OP-006.

This file is additive across OP-005 sub-packets (a: Federation, b: Knowledge
Base Foundation, c: Advisory Pipeline MVP, d: standalone briefs/notes) — each
appends its own sections rather than overwriting.

---

## Intention: Federation is opt-in and additive, never disruptive to single-instance operation

- **What Mikael wants:** A DEKE instance works exactly as before with federation code merged in but turned off. No existing table gets schema changes; no existing endpoint or tool loses its non-federated behavior.
- **Why (origin/context):** Federation should extend, not undermine. DEKE's default deployment is a single instance; federation is a capability layered on top for instances that opt in.
- **Constraints it imposes:** `FederationConfig.Enabled` defaults to `false`; `federation_peers` is a new table, no existing tables touched (federation provenance in `facts.metadata` explicitly deferred to a not-yet-reached "Phase 4 (replication)", not Phase 1/2); `PeerHealthCheckService` no-ops entirely when disabled.
- **Source:** `thoughts/shared/research/federation-phase1-research.md` (Database Impact, "No changes to existing tables"); `docs/PROJECT-MAP.md` Peer Health Check entry (confirms built this way).

## Intention: Static peer configuration now, discovery mechanisms deferred

- **What Mikael wants:** Peers are named explicitly in config (URL only); DEKE does not try to auto-discover federation partners over the network.
- **Why (origin/context):** Keep the trust boundary explicit and manual — you decide who your peers are, DEKE doesn't go looking.
- **Constraints it imposes:** DNS-based or registry-based peer discovery pushed to "later phases" (research: "DNS and external registry are Phase 5+ concerns"); domain coverage per peer is still auto-discovered (via manifest polling) even though peer identity is not.
- **Source:** `thoughts/shared/research/federation-phase1-research.md` (Decisions Needed §3); `thoughts/shared/research/federation-phase2-search-research.md` (Design Decisions table, row 1).

## Intention: Prefer standard library patterns over hand-rolled equivalents, even where it means diverging from established project precedent

- **What Mikael wants:** Mature, well-tested library machinery (Polly resilience pipeline, `IOptions<T>` binding) over bespoke retry/failure-counting/manual-config-parsing code — accepted even where it contradicts the pattern the codebase had already established (`EmbeddingsConfig`'s manual binding).
- **Why (origin/context):** Plan rounds explicitly reconsidered the original hand-rolled failure counter and manual config parsing mid-flight and replaced both with library-backed equivalents, calling the divergence from precedent intentional rather than an oversight.
- **Constraints it imposes:** `FederationConfig` binds via `IOptions<T>`/`IOptionsMonitor<T>` (hot-reload of peer list) instead of `EmbeddingsConfig`'s `required` + manual-binding style; peer HTTP calls route through a named `"federation"` `HttpClient` with `AddStandardResilienceHandler` (retry, circuit breaker, timeout) instead of a manual failure counter.
- **Source:** `thoughts/shared/plans/federation-phase1-plan.md` (Decisions 4-5, "Round 2 (Library Review)" — "Diverges from EmbeddingsConfig precedent intentionally").

## Intention: Manifest-driven, polling-based peer/domain discovery — not push registration

- **What Mikael wants:** Peers don't announce themselves into a target instance's registry. A target instance polls each configured peer's `/api/federation/manifest` and self-updates its own view of that peer's health and domains.
- **Why (origin/context):** Keeps the write path single-owner (only the polling instance writes to its own `federation_peers` table) and makes the manifest endpoint the one source of truth for "what does this peer currently know."
- **Constraints it imposes:** Manifest endpoint is `AllowAnonymous()` by design (peers must be callable without pre-shared credentials in Phase 1/2); `PeerHealthCheckService` upserts by `instance_id` on a 5-minute cycle; a peer's advertised domains only reach the polling instance's registry after at least one successful probe (first-probe domain gap acknowledged as a known Phase 1 risk, not resolved).
- **Source:** `thoughts/shared/research/federation-phase1-research.md` (Risks §1, Decisions Needed §2); `docs/PROJECT-MAP.md` Peer Health Check + Federation Endpoints entries.

## Intention: Local-first search — federate only on a threshold miss or domain gap, not by default

- **What Mikael wants:** A search call stays entirely local whenever local results are good enough; peers are consulted only as a fallback/extension, controlled by one tunable number.
- **Why (origin/context):** Keeps the common case fast and avoids needless cross-instance traffic; a single `DelegationThreshold` (0.0 = only delegate when the domain isn't covered locally at all) is the one knob, not a maze of federation policy settings.
- **Constraints it imposes:** Delegation triggers only when (a) queried domain not covered locally, or (b) best local result score below `DelegationThreshold`; peer selection additionally filtered to only peers whose manifest advertises the queried domain.
- **Source:** `thoughts/shared/briefs/federation-phase2-search-brief.md` (Acceptance Criteria, delegation trigger bullet); `thoughts/shared/research/federation-phase2-search-research.md` (Federation Header Flow diagram).

## Intention: Loop prevention is a protocol-level concern, carried in headers — not just an internal guard

- **What Mikael wants:** Hop count, visited-instance set, and a request ID travel with the request itself (`X-Federation-Hop-Count`, `-Visited`, `-Request-Id`), not just tracked as private in-process state.
- **Why (origin/context):** A federated query can legitimately hop through more than one intermediate instance before terminating; loop-prevention state has to travel with the request across process boundaries to actually prevent loops — an in-process-only guard couldn't do that.
- **Constraints it imposes:** Hop count capped at `FederationConfig.MaxHops`; visited set causes self- and already-visited-peer skips; request ID is for tracing/dedup, not enforcement. Headers are unauthenticated in Phase 1/2 (peers are trusted, not verified) — explicitly deferred, see "Auth is deliberately deferred" below, not a fresh gap.
- **Source:** `thoughts/shared/briefs/federation-phase2-search-brief.md` (Acceptance Criteria, header list); `thoughts/shared/research/federation-phase2-search-research.md` ("Loop Prevention" subsection).

## Intention: Locality-weighted trust scoring — prefer near knowledge over far knowledge at equal similarity

- **What Mikael wants:** A result's final rank isn't pure semantic similarity; it's discounted by how many federation hops away it came from, on top of the fact's own confidence score.
- **Why (origin/context):** An instance's own knowledge (or its immediate peers') should generally be trusted more than knowledge relayed through several hops, all else equal.
- **Constraints it imposes:** `final_score = similarity * confidence * locality_weight`; default weights local=1.0, hop1=0.9, hop2=0.75, hop3=0.6, configurable via `FederationConfig.LocalityWeights`. This formula sits directly next to the ranking-formula documentation drift already tracked as ADR-0005 (proposed) — related background, not re-escalated here.
- **Source:** `thoughts/shared/research/federation-phase2-search-research.md` ("Locality Weight Scoring" subsection); `thoughts/shared/sessions/federation-phase2-search-session.md` (Step 1).

## Intention: MCP and REST are peers, not primary/secondary — both get federation awareness in the same phase

- **What Mikael wants:** The MCP tool surface (`consult_domain_expert`, `list_available_domains`) is not a thin follow-on wrapper bolted on after the HTTP API stabilizes; it ships bundled with the REST changes in the same phase.
- **Why (origin/context):** The brief explicitly folds the federation spec's "Phase 3" (MCP tools) into "Phase 2" (federated search) as one work unit rather than sequencing them — MCP is a first-class client of `IFederatedSearchService`, identical standing to the HTTP endpoints.
- **Constraints it imposes:** `SearchTools` and `SearchEndpoints` both call the same `IFederatedSearchService`; MCP calls always originate fresh (no inbound federation headers to relay, since MCP has no HTTP headers) — a structural difference from the REST path, not an oversight.
- **Source:** `thoughts/shared/briefs/federation-phase2-search-brief.md` (title, description); `docs/PROJECT-MAP.md` Search Tools entry.

## Intention: Breaking changes to the federation-touched API surface are acceptable pre-stabilization

- **What Mikael wants:** The GET→POST conversion of `/api/search`/`/api/search/context`, and the rename of `search_knowledge`→`consult_domain_expert` / `list_domains`→`list_available_domains`, were both taken as clean breaks rather than versioned/aliased.
- **Why (origin/context):** The brief/research both record this as a deliberate, resolved decision ("Breaking change OK", "Clean API surface, no legacy tool baggage") rather than something forced by a technical constraint.
- **Constraints it imposes:** No back-compat shim, no dual GET+POST support, no old-name aliases were added for either the REST or MCP surface.
- **Source:** `thoughts/shared/research/federation-phase2-search-research.md` (Design Decisions table, rows 4 and 6); `thoughts/shared/briefs/federation-phase2-search-brief.md` (Acceptance Criteria).
- **Confirmed general (2026-07-07, Mikael, direct answer during OP-006):** this is a DEKE-wide stance, not federation-specific — breaking changes to APIs/tool names are acceptable project-wide while pre-1.0, no back-compat shims required by default anywhere in the codebase.

## Intention: Auth/authorization hardening is a distinct later phase, not a hole in the current design

- **What Mikael wants:** Federation mechanics (discovery, search delegation, MCP tools) are built and proven functionally first; `X-Federation-Token`, mutual TLS, per-peer authorization policies, and rate limiting are explicitly named as "Phase 5" rather than left ambiguous.
- **Why (origin/context):** Keeps Phase 1/2 "focused on search mechanics" — an explicit scope boundary, not a deferred TODO discovered by accident.
- **Constraints it imposes:** Manifest and peer-search endpoints are unauthenticated by design in the current scope; this is a known, named boundary, not to be conflated with the unrelated `Deke.Api`-wide auth defect already tracked as ADR-0008 (`ApiKeyAuthHandler`'s unconfigured-key branch) — that finding is about general write-endpoint auth, not federation's peer-to-peer trust model.
- **Source:** `thoughts/shared/briefs/federation-phase2-search-brief.md` (Scope Boundaries, "Out of scope" list); `thoughts/shared/research/federation-phase2-search-research.md` (Design Decisions table, row 8).

## Intention: Trust scoring is one pure function, computed at merge-time, not precomputed or stored

- **What Mikael wants:** A single, pure, unit-testable scoring function decides a result's final rank at the moment results are merged — not a stored column, not a PLpgSQL function, not scattered inline arithmetic per call site.
- **Why (origin/context):** Keeps ranking logic in C# where it can be unit-tested in isolation, and keeps the formula in exactly one place (`ITrustScoringService.Score()`) for every search path — local, advisory, and federated alike — rather than each path growing its own copy.
- **Constraints it imposes:** `ITrustScoringService` is a pure function (similarity, confidence, sourceCredibility, validity window, localityWeight, now) → score, with no side effects and no DB access; the concrete formula (similarity × confidence × credibility × recency-decay × locality) is the same substantiation already tied to ADR-0005 (federation ranking formula doc/code drift) — related background, not re-escalated here.
- **Source:** `thoughts/shared/research/knowledge-base-foundation-research.md` (Resolved Design Decisions #1: "Merge-time in C#... No PLpgSQL function"); `docs/PROJECT-MAP.md` Trust + Search & Trust Contracts entries (confirms built exactly this way).

## Intention: A missing factor should fall back to neutral, not silently zero the result

- **What Mikael wants:** When one input to the trust formula is unavailable — specifically, source credibility for a federated result arriving over HTTP with no local `sources` row to join against — the formula treats it as neutral (0.5 default) rather than letting a missing value collapse the whole score to zero.
- **Why (origin/context):** Caught during plan validation as a real bug-in-waiting: federated results would have scored exactly 0 on every query, silently defeating the entire federation feature, because `credibility × everything-else` zeroes out when credibility defaults to 0. The fix was written into the plan before implementation, not discovered after.
- **Constraints it imposes:** `TrustScoringService` must treat `sourceCredibility <= 0` as neutral (0.5), never as a literal zero weight; a dedicated unit test asserts this specific case, not just the general scoring behavior.
- **Source:** `thoughts/shared/plans/knowledge-base-foundation-plan.md` (Step 4.2 "[GAP found in validation]"; Validation Log, Issue 2); `docs/PROJECT-MAP.md` Trust entry ("neutral 0.5 default when absent, e.g. federated results").

## Intention: Each trust factor is applied exactly once — no silent double-counting

- **What Mikael wants:** Source credibility must be applied to a fact's effective trust exactly one time, at merge/search time — never baked into the stored `confidence` value in addition to being applied again during ranking.
- **Why (origin/context):** The pre-existing ingestion code (`SourceMonitorService`) was already multiplying `extracted.Confidence × source.Credibility` at storage time; layering merge-time credibility scoring on top, without fixing that, would have silently squared credibility's effect on every fact's rank.
- **Constraints it imposes:** Ingestion now stores `extracted.Confidence` alone (extractor confidence only); credibility is applied exactly once, at merge-time, by `ITrustScoringService`. This was corrected within the same work thread that introduced merge-time scoring — not a live bug today, but the underlying principle ("one factor, one application point") generalizes to any future addition to the trust formula.
- **Source:** `thoughts/shared/plans/knowledge-base-foundation-plan.md` (Step 4.1, Validation Log Issue 4 "[CONFIRMED] Credibility double-count"); `thoughts/shared/research/knowledge-base-foundation-research.md` (Corrections to Brief Assumptions, confidence-computation note).

## Intention: Ingestion quality work reaches for an existing library before writing custom logic — reinforces the federation-thread pattern

- **What Mikael wants:** Replacing whole-blob, single-fact extraction with real semantic chunking used an existing chunking library (`SemanticChunker.NET`) plus a small adapter, rather than a hand-written paragraph/sentence splitter.
- **Why (origin/context):** Same standing preference already distilled from the Federation thread (see "Prefer standard library patterns..." above) — reused here for a different subsystem, not a one-off. The plan even caught and fixed a wrong assumption about the library's actual API (`CreateChunksAsync`, not the README's `ChunkAsync` shorthand) by verifying against source before coding, rather than trusting documentation at face value.
- **Constraints it imposes:** `IChunker`/`SemanticChunkerAdapter` wraps `SemanticChunker.NET` via a `Microsoft.Extensions.AI`-shaped embedding-generator adapter (`OnnxEmbeddingGenerator`) rather than reimplementing chunking logic; `SimpleExtractionService` becomes a 1:1 passthrough (one fact per chunk) instead of doing its own splitting.
- **Source:** `thoughts/shared/plans/knowledge-base-foundation-plan.md` (Confirmed Decisions table, "Chunker lib"; Validation Log Issue 1); `docs/PROJECT-MAP.md` Extraction + Embeddings entries.

## Intention: DEKE should know about itself first — bootstrap ingestion seeds a primary-source domain

- **What Mikael wants:** Before general-purpose ingestion proves itself, DEKE ingests its own `docs/` and `thoughts/` into a dedicated `software-product` domain, marked as elevated-confidence, primary-source facts.
- **Why (origin/context):** Self-referential grounding — DEKE's own documentation is the first real body of knowledge the system holds, and it should be trusted more than harvested third-party content by construction, not by accident.
- **Constraints it imposes:** Bootstrap facts carry confidence 0.95 (vs. the general extractor's 0.8) and a `Source` row with credibility 1.0 marking primary-source origin; ingestion is a one-shot CLI-triggered command (`--bootstrap`), not a recurring background job; re-runs must be idempotent (dedupe via content hash / `GetByUrlAsync` find-or-create), not additive garbage on every restart.
- **Source:** `thoughts/shared/work/knowledge-base-foundation-brief.md` (#7 Acceptance Criteria); `thoughts/shared/sessions/knowledge-base-foundation-session.md` (Acceptance criteria, #7 confirmed met).

## Intention: Interaction logging is data capture only — instrumentation before intelligence, and only for real top-level queries

- **What Mikael wants:** Every top-level search/context call gets logged (query, returned fact ids, scores, model, timing) purely as an audit trail — explicitly *not* wired into any learning or self-improvement loop yet. And only genuine top-level calls are logged, not the internal peer-relayed subqueries federation generates along the way.
- **Why (origin/context):** Separates "can we observe usage" from "should the system act on what it observes" — the former is safe to ship immediately, the latter is a distinct, larger decision deferred on purpose. The peer-relay exclusion was caught during plan validation: without it, every federated hop would double-log the same logical query as noise.
- **Constraints it imposes:** `interaction_logs` is insert-only, no read/aggregation path feeds back into ranking or ingestion; `FederatedSearchService.SearchAsync` logs only when `federation is null` (a genuine top-level call, not a relayed subquery or an internal `GetContextAsync` passthrough); duration is measured after the full response (including merge) is built, not before.
- **Source:** `thoughts/shared/work/knowledge-base-foundation-brief.md` (#8 Acceptance Criteria, "No learning loop — data capture only"); `thoughts/shared/plans/knowledge-base-foundation-plan.md` (Step 4.3, "[GAP found in validation]").

## Intention: Schema changes ship as hand-edited `init.sql`, no migration tooling — accepted trade-off, not an oversight

- **What Mikael wants:** New tables/columns are added directly to the single `init.sql` file; there is no migration framework, and an already-provisioned database needs a manual `ALTER`/recreate to pick up the change.
- **Why (origin/context):** Confirmed independently across two separate threads (Federation Phase 1, Knowledge Base Foundation) as the same deliberate simplicity trade-off, not something either thread introduced locally — DEKE prioritizes a single source of schema truth over migration machinery at this stage of the project.
- **Constraints it imposes:** Every schema-touching plan must include the literal `ALTER TABLE`/`CREATE TABLE` statements for already-provisioned databases in its rollout notes, since applying them is a manual, human step; rollback is similarly manual (`DROP TABLE`/`DROP COLUMN`), documented per plan rather than automated.
- **Source:** `thoughts/shared/plans/knowledge-base-foundation-plan.md` (Phase 2 notes, "no migrations — a provisioned DB needs manual ALTER"); `thoughts/shared/research/federation-phase1-research.md` (Risks §2, same constraint stated independently for the `federation_peers` table).

## Intention: Pure-unit tests with mocked repositories are an accepted substitute for a real-database integration harness, for now

- **What Mikael wants:** New DB-touching logic (bootstrap ingestion, interaction-log persistence) is tested with mocked repositories and in-memory fakes, not against a real PostgreSQL instance — accepted explicitly as a scoped trade-off, not silently skipped.
- **Why (origin/context):** `tests/Deke.Tests` has no Testcontainers or other DB fixture/harness today; standing up one was considered and consciously deferred rather than blocking this work on infrastructure investment.
- **Constraints it imposes:** Acceptance criteria for DB-touching items are satisfied by unit tests over mocked repos (e.g. `BootstrapIngestionTests` with a fake `IFactRepository`); the validation log itself flags this as an accepted gap ("no integration test exercises real Postgres"), so a future packet reintroducing that gap as a fresh finding would be re-discovering a known, already-decided trade-off, not a new smell.
- **Source:** `thoughts/shared/research/knowledge-base-foundation-research.md` (Risks, "DB-touching tests"); `thoughts/shared/plans/knowledge-base-foundation-plan.md` (Confirmed Decisions "Tests" row; Validation Log checklist, final `[~]` item).

## Intention: When code and docs disagree, the code — verified by direct reading, not by trusting a README or spec — wins, and the docs get corrected afterward

- **What Mikael wants:** Spec/brief assumptions are treated as hypotheses to verify against the real codebase and real library source, not facts to build on directly. Every stale assumption found gets corrected in the docs afterward, never silently worked around.
- **Why (origin/context):** This thread independently re-confirms a pattern already seen twice elsewhere (ADR-0004/0005's doc-catches-up-to-code adjudications; the Federation thread's chunker-API correction) — a third occurrence, now for Advisory: the brief assumed no LLM client existed (wrong — `ILlmService` already did, just insufficiently), spec's `KnowledgeFact`/`TrustMetadata` types don't exist (real types are `FactSearchResult` + a trust `double`), the spec's domain string (`software-product-advisor`) doesn't match the real one (`software-product`), and the spec's model ID (`claude-sonnet-4-6`) doesn't exist at all.
- **Constraints it imposes:** Contracts are defined against real, verified types, not spec-literal ones; a doc-correction pass (Phase 7, delegated to doc-maintainer per `CLAUDE.md` governance) is a standing, expected phase of this kind of work, not an afterthought; library API assumptions (e.g. `IChatClient` entry points) get verified against actual package docs/source before being coded against, the same discipline already applied to `SemanticChunker.NET` in the Knowledge Base Foundation thread.
- **Source:** `thoughts/shared/research/advisory-pipeline-mvp-research.md` (Key Findings §1-6); `thoughts/shared/plans/advisory-pipeline-mvp-plan.md` (Phase 7, "delegate to doc-maintainer").

## Intention: A second, richer LLM abstraction was deliberately introduced for Advisory rather than stretching the existing one

- **What Mikael wants:** `Microsoft.Extensions.AI`'s `IChatClient` (already in the codebase for embeddings) is adopted as Advisory's model-calling abstraction, explicitly *alongside* the pre-existing bespoke `ILlmService` — not a replacement, not a retrofit.
- **Why (origin/context):** `ILlmService` is `Task<string> GenerateAsync(prompt)` + `IsAvailable`, single-active-provider — it cannot express per-call haiku→sonnet→ollama routing, model IDs, or usage data the audit record needs. Rather than bend that abstraction to fit, Advisory reuses `IChatClient` (already present for `SemanticChunkerAdapter`'s embedding generator), keeping with the recurring reach-for-an-existing-abstraction pattern seen across both prior OP-005 threads. This split is exactly the subject of ADR-0006/ADR-0007 (relevant background, not re-litigated here).
- **Constraints it imposes:** Two keyed `IChatClient` instances (`anthropic`, `ollama` — collapsed from an original three-client design once research confirmed one Anthropic client serves both haiku and sonnet via per-call `ChatOptions.ModelId`); `ILlmService` and its Gemini/OpenAI backends are left completely untouched, still serving `PatternDiscoveryService` alone.
- **Source:** `thoughts/shared/research/advisory-pipeline-mvp-research.md` (Key Findings §1-2, Resolved Decisions #1); `thoughts/shared/plans/advisory-pipeline-mvp-plan.md` (Locked Decisions, "Model abstraction"; Validation Log Issue 3).

## Intention: Confidence must be earned by retrieval quality, never asserted upward by a model or an adapter

- **What Mikael wants:** An advisory answer's confidence band is a computed consequence of how good the retrieved evidence actually was — no adapter, and no downstream LLM output, is allowed to push that band higher than what retrieval earned.
- **Why (origin/context):** Stated as an explicit architectural invariant across the brief, research, and plan alike ("Honesty constraint enforced at shared-core (pipeline) level: no adapter can override uncertainty expression upward"), and specifically unit-tested as such rather than left as an aspiration.
- **Constraints it imposes:** The honesty cap is enforced inside `AdvisoryPipeline.AdviseAsync` itself (stage 6, response assembly), not delegated to any `IAdvisoryAdapter` — `CalibrateTrust()` can only describe or contextualize a score, never inflate it; a dedicated test asserts this cap holds even when a fake model response is confident-sounding.
- **Source:** `thoughts/shared/work/advisory-pipeline-mvp-brief.md` (#4 Acceptance Criteria, "Honesty constraint enforced at shared-core... level"); `thoughts/shared/plans/advisory-pipeline-mvp-plan.md` (Phase 4, stage 6 "honesty cap enforced here"); `docs/PROJECT-MAP.md` Advisory Pipeline Implementation entry (confirms enforced in `AdviseAsync`, "exactly per each Deke.Core interface's own doc comments").

## Intention: Model tier (haiku/sonnet/ollama) is chosen automatically from a computed retrieval-quality score, not a manual per-query dial

- **What Mikael wants:** A single heuristic — `knowledge_depth_score = mean(topK trust) × coverage × topSimilarity` — decides which model tier answers a query, escalating only when confidence is genuinely low and stakes are genuinely high (or an explicit override is given).
- **Why (origin/context):** Keeps the cost/quality tradeoff grounded in the same trust signal the rest of the system already computes, rather than adding a second, separate policy surface a caller would have to configure per query.
- **Constraints it imposes:** Default routing is haiku when `knowledge_depth_score >= 0.6`; escalate to sonnet only on Low confidence band + High stakes (or explicit override); Ollama requires both `AllowLocalModel` (an adapter-owned property on `DomainActivationCriteria`, not a separate config store) and `knowledge_depth_score >= 0.75`. This heuristic and its thresholds were an open item at research time and were locked as a concrete formula and threshold set by the plan.
- **Source:** `thoughts/shared/research/advisory-pipeline-mvp-research.md` (Open Items for Planning, `knowledge_depth_score`); `thoughts/shared/plans/advisory-pipeline-mvp-plan.md` (Locked Decisions, "Routing"; "AllowLocalModel"); `docs/PROJECT-MAP.md` Advisory Pipeline Implementation entry (confirms "mean top-5 trust × coverage × top-similarity" built as specified).

## Intention: Each feature gets its own audit record shaped for its own semantics, even when a similar-looking log table already exists

- **What Mikael wants:** `advisory_interactions` is a distinct, purpose-built append-only table — not a reuse or extension of Knowledge Base Foundation's `interaction_logs`, despite both being "log what happened" tables that a first glance might want to merge.
- **Why (origin/context):** `interaction_logs` is search-shaped (query, returned fact ids, scores); Advisory's audit record needs different, richer fields (stakes, model, confidence band, knowledge gaps, raw output, conflicting-evidence flag) that don't fit that shape. `docs/architecture/decisions.md` is cited as mandating this as an append-only Response Audit Record in its own right, not a schema Advisory should have bent to reuse.
- **Constraints it imposes:** `advisory_interactions` mirrors `interaction_logs`' *pattern* (append-only, plain `INSERT`, no `RETURNING`, indexed on `(domain, created_at DESC)`) without sharing its *schema*; `ConfidenceBand`/`Stakes` persist as real enums via new `EnumTypeHandler<T>` registrations, not as bare strings.
- **Source:** `thoughts/shared/research/advisory-pipeline-mvp-research.md` (Resolved Decisions #2, "decisions.md (2026-03-13) mandates an append-only Response Audit Record"); `thoughts/shared/plans/advisory-pipeline-mvp-plan.md` (Phase 2).

## Intention: The first domain adapter closes the loop — DEKE advises on itself, grounded in its own bootstrapped documentation

- **What Mikael wants:** The Software Product Advisor adapter isn't a generic demo domain; it specifically targets the `software-product` domain that Knowledge Base Foundation's bootstrap ingestion seeded from DEKE's own `docs/`/`thoughts/` — DEKE answering questions about itself, using its own primary-source facts.
- **Why (origin/context):** This is the MVP milestone the brief frames explicitly: "DEKE answering domain questions better than a language model alone, with cited and confidence-scored facts" — and the natural first proof of that is self-referential, connecting directly to the bootstrap-ingestion intention already distilled from the Knowledge Base Foundation thread (OP-005b).
- **Constraints it imposes:** `SoftwareProductAdvisorAdapter.ActivationCriteria.Domain == "software-product"` (the real bootstrap domain constant, not the spec's stale `software-product-advisor` string); its `WeightFacts` favors recent, high-credibility primary-source facts — the same facts bootstrap ingestion marks as elevated-confidence/credibility-1.0.
- **Source:** `thoughts/shared/work/advisory-pipeline-mvp-brief.md` (Description, "first domain adapter... for DEKE's own self-advisory over its bootstrapped design/architecture knowledge"); `thoughts/shared/research/advisory-pipeline-mvp-research.md` (Key Findings §4).

## Intention: A real, manual end-to-end pass against live services is required before shipping cross-cutting work — automated tests alone are not treated as sufficient proof

- **What Mikael wants:** Beyond the project's standing practice of pure-unit tests with fakes (no DB/network in CI), a feature this cross-cutting gets one real pass against actual Postgres, actual Anthropic API, and actual Ollama before being considered done.
- **Why (origin/context):** That live pass caught three runtime bugs the fake-backed unit tests had no way to see: an `M.E.AI`/`Anthropic.SDK` version incompatibility (fixed by pinning), an enum-serialized-as-integer persistence bug, and an advisory-to-federation trust-scoring coupling bug — all real, all invisible to mocks, all fixed same-thread. This doesn't contradict the "pure-unit tests over an integration harness" intention already distilled from Knowledge Base Foundation (OP-005b): that intention is about what ships in **automated CI**; this one is about a **manual, one-time verification gate** before a cross-cutting feature is called complete.
- **Constraints it imposes:** Live e2e verification is env-gated (real API key, running Postgres, optional local Ollama) and manual, not part of the automated test suite; when it surfaces a bug, the fix lands in the same work thread rather than being deferred.
- **Source:** `thoughts/shared/sessions/advisory-pipeline-mvp-session.md` ("Live e2e" section, "Caught 3 runtime bugs unit tests missed"); `thoughts/shared/plans/advisory-pipeline-mvp-plan.md` (Phase 6, "E2e (manual, env-gated)").

## Intention: Documentation follows a strict three-branch separation — what the system is, how it's built, and general background research never mix

- **What Mikael wants:** Every doc lives in exactly one of three branches: `product/` (what DEKE is — no SQL, no code, no library names), `architecture/` (how it's built — schema, endpoints, code patterns, both built and planned), or `science/` (general research that isn't DEKE-specific — reference material that ages gracefully). "How DEKE uses X" always goes in `architecture/`, never `science/`.
- **Why (origin/context):** The prior state was 8 docx files plus scattered markdown with heavy overlap and mixed concerns (vision/architecture/implementation/research all in the same document) — a hybrid of Diataxis and arc42-lite was adopted specifically to make "what is this" vs. "how does it work" vs. "background theory" separable and non-duplicative.
- **Constraints it imposes:** This structure is already fully executed — `docs/product/`, `docs/architecture/`, `docs/science/`, `docs/roadmap.md`, `docs/INDEX.md` match the brief's target structure exactly. Any future doc work (including this Overhaul's own artifacts and OP-009's spec refactors) should respect the same routing rule rather than reintroduce mixed-concern documents; filenames describe content, not package/phase sequence (`docs/INDEX.md` handles reading order).
- **Source:** `thoughts/shared/briefs/docs-overhaul-brief.md` (Content Routing Rules; Target Structure; Design Decisions #3).

## Intention: Product docs are a present-tense model of the *target* system; the roadmap — not the product model — is where "built vs. not-built" honesty lives

- **What Mikael wants:** `product/` files describe DEKE as if it were already complete, by design — that's what makes them useful as a model. Whether a described piece actually exists yet is the roadmap's job to track, not the product docs'. The failure mode this guards against is the opposite one: a roadmap or spec claiming something is done/current when it isn't.
- **Why (origin/context):** Explicit documentation philosophy adopted during the product checkpoint: "Product docs are a model of the target system, written in present tense... Progress is tracked separately in the roadmap." Also explicit: "No present-tense claims in roadmap for unbuilt features (roadmap is honest; product model is aspirational by design)."
- **Constraints it imposes:** A stale "Planned"/"not yet built" label in `roadmap.md` or `specification.md` for something that's actually shipped (already logged: bootstrap ingestion, `retrieval-pipeline.md`'s Chunk stage) is a violation of this exact principle — those are pre-existing `PARKING-LOT.md` items, not re-logged here, just now understood as instances of one general rule rather than isolated typos. Model claims are also expected to be honestly scoped as hypotheses where genuinely unproven — e.g. this thread's own acceptance criterion to reframe the "knowledge compensation principle" as a hypothesis to validate, not an invariant to assert.
- **Source:** `thoughts/shared/briefs/product-checkpoint-fixes-brief.md` (Description, "Documentation philosophy"; Acceptance Criteria, "Knowledge compensation principle..." and "No present-tense claims...").

## Intention: Package 3 (Evolution Engine)'s product-vs-research status has genuinely moved twice — this brief's "demote to research" decision was later reversed, not superseded quietly

- **What Mikael wants:** At the time of this brief, Package 3 was deliberately moved *out* of the core product model into a research/vision document, because it was judged "a research project masquerading as a product feature" with no clear MVP definition — the product model was trimmed to Package 1 + Package 2 + Federation only.
- **Why (origin/context):** This decision was later reversed: ADR-0002 (accepted, distilled in OP-003) promoted Evolution Engine back to full parity as an active Package 3 under a Three-Package Architecture — `docs/GLOSSARY.md`'s Evolution Engine row states this explicitly ("not deferred research"). `docs/product/overview.md`, however, was never updated after that reversal and still describes a Two-Package Architecture — logged fresh as a `PARKING-LOT.md` item by this packet (decision-ahead-of-doc gap, not a new design disagreement).
- **Constraints it imposes:** Anyone reading this brief in isolation would conclude Package 3 is currently out-of-product-scope — that conclusion is now stale. The current authoritative status is ADR-0002's, not this brief's. A future OP-009 packet correcting `overview.md` should treat ADR-0002 as the standing decision.
- **Source:** `thoughts/shared/briefs/product-checkpoint-fixes-brief.md` (Description; Change A.1); `docs/GLOSSARY.md` (Evolution Engine row, "APPROVED", ADR-0002); `overhaul/PARKING-LOT.md` (this packet's new entry).

## Intention: The Gemini/OpenAI LLM backend was purpose-built to unblock the advisory pipeline — a purpose later superseded, explaining why it now looks orphaned

- **What Mikael wants (at the time):** `GeminiLlmService`/`OpenAiLlmService` behind `ILlmService`, with one active provider selected via config, were built specifically because "this unblocks the advisory pipeline (MVP item #4) by providing the model call capability that the 7-stage pipeline requires."
- **Why (origin/context):** This explains the *origin* of a system that OP-004b/ADR-0006 later found to be unrelated to Advisory in the actual, built codebase — Advisory ended up calling `IChatClient` (Anthropic/Ollama) instead (see the "second LLM abstraction" intention distilled from the Advisory thread), leaving `Llm/`'s Gemini/OpenAI system with its only real consumer being `PatternDiscoveryService.cs`. This brief is the missing "why does this exist" context ADR-0006/ADR-0007 didn't have when they found the system undocumented and seemingly purposeless — it wasn't purposeless when built, its purpose was later fulfilled a different way.
- **Constraints it imposes:** None new — ADR-0006/ADR-0007 (accepted, spawning OP-008c to retire `Llm/` and migrate `PatternDiscoveryService` onto `IChatClient`) already cover this system's fate; this section is background explaining *why* the ADR's finding looks the way it does, not a re-escalation.
- **Source:** `thoughts/shared/work/llm-provider-config-brief.md` (Description, "This unblocks the advisory pipeline... by providing the model call capability that the 7-stage pipeline requires").

## Intention: A hybrid vectorless/vector RAG approach — promoted from a one-line spark to a real future roadmap item

- **What Mikael wants:** The note read in full: "Investigate and add vectorless rag and a very intelligent hybrid model that uses the best from both worlds." Asked directly (OP-006), Mikael confirmed this should become a real, sized packet rather than stay parked or get dropped.
- **Why (origin/context):** No rationale, motivating problem, or prior discussion was captured anywhere in `thoughts/` — the idea was genuinely context-free at distillation time. It still cannot be connected to any other distilled intention in this file with confidence; what's now resolved is only that it's worth pursuing, not what "vectorless RAG" or "hybrid" should concretely mean for DEKE.
- **Constraints it imposes:** Formalized as **ADR-0010** (accepted) rather than left as a bare inferred note. No design is decided yet — it does not spawn an immediate `OP-008` packet (that series is for already-decided redesigns); it becomes a roadmap entry to be sized during **OP-011** (roadmap rebuild), likely research-first given there's no existing code or pattern in this repo to anchor a design against yet.
- **Source:** `thoughts/adhoqnotes.md` (entire file, one line); `docs/adr/ADR-0010-vectorless-rag-hybrid-research-direction.md` (resolution, 2026-07-07).
