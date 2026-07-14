# STATE.md — Overhaul Baton

> Read this first, every session. Append a new entry (newest on top) when
> you finish a packet. Keep entries ≤10 lines. This file plus the artifacts
> in `/docs` are the only cross-session memory — don't assume anything else
> carries over between sessions.

---

## 2026-07-14 — OP-012: review packet — all 8 exit criteria PASS

- Criteria 1-7 verified fresh (evidence table in `packets/OP-012.md`):
  glossary 4/4 ENFORCED, map 38/38 HIGH, INTENT clean, 11/11 ADRs accepted,
  tooling suite 8/8, lint exit 0, ROADMAP DAG shape confirmed.
- Criterion 8 (Mikael: real run): fresh subagent given only /docs + HYG-2
  completed it — 3 endpoints, 12 tests (80/80), live-verified, **zero
  orientation questions**. HYG-2 thereby done early; ROADMAP status update
  deferred to OP-013's doc pass. Ride-along: CLAUDE.md `"type":"Rss"`
  example 400s (no string-enum converter) → appended to HYG-1 list.
- Remaining 4 DISCONNECTs executed (Mikael's call): LSP+DesignSync denied,
  playwright-skill override re-keyed (`.claude/settings.json`).
- **Next: OP-013 (dissolution)** — nothing blocks it.

## 2026-07-14 — OP-011: roadmap rebuilt as sized packet DAG (docs/ROADMAP.md)

- Prior OP-008/009/010 work batch-committed first (`aed86bb`, Mikael's call).
- `docs/roadmap.md` → `docs/ROADMAP.md` (renamed per sketch §2; fixed 3
  *pre-existing broken* `product/roadmap.md` inbound links + 4 more sites).
  Old roadmap's Current State was badly stale — all three "Not yet built"
  items (trust layer, chunking, bootstrap) were in fact built; **MVP is
  delivered in full**; rebuilt roadmap opens post-MVP.
- DAG: **P1-1 first priority** (critical path, data already accumulating);
  RES-1 (ADR-0010 research, old "BM25 hybrid" line folded in) early+parallel;
  HYG-1/2/3 promoted from PARKING-LOT; R2/P1-2/R4/R5/EE1-DESIGN
  dependency-gated; R7/R8/P1-3/R3/FED-4/FED-5/DOM-2/P1-4 later; R6 +
  EE-2..5/Curiosity + OI-01..10 trigger-gated (tiered sizing, Mikael's call).
  EE1-DESIGN carries ADR-0011's revisit trigger. All 12 PARKING-LOT items
  dispositioned (6 promoted, 1 resolved, 1 split, 4 stay parked).
- Verified: glossary lint exit 0 (run post-install by doc-maintainer; later
  edits touched only `overhaul/`, outside scan scope); build untouched (zero
  `.cs` changes this packet — a fresh `dotnet build` re-run was blocked by a
  transient harness outage, re-run at commit time); `roadmap.md` refs remain
  only in the 2 exempt historical files. Exit criterion 7 satisfied.
- **Next: OP-012** (review packet — verify exit criteria 1-8, re-run
  TOOLING.md verification suite), then OP-013 (dissolution).

## 2026-07-14 — OP-010: glossary lint script + CI hook, terms move to ENFORCED

- Interviewed Mikael in two rounds. Round 1 (procedural): lint scope
  docs/+src/, hardcoded path exclusions (no inline-suppress syntax), hard-fail
  gate, all 3 `APPROVED` rows to `ENFORCED` together. Round 2 (substantive,
  surfaced by this packet's own pre-build scan): ADR-0002 bans "Package
  3"/"P3" as deprecated aliases of "Evolution Engine", but ADR-0002's own
  resolution granted the subsystem "full parity with Package 1 and Package
  2" under a Three-Package Architecture — and OP-008a's same-day
  reconciliation had, correctly per that resolution, reintroduced "Package
  3"/"P3-N" across 8 files. Building the lint as literally specified would
  have failed CI against ~80 lines of correct, deliberate work. Mikael's
  call: **keep the ban** (don't un-deprecate); new shorthand **EE-N**
  replaces P3-N (new `GLOSSARY.md` row); `decisions.md`'s live Open Design
  Questions sections get rewritten, its dated historical table stays
  verbatim (same treatment OP-008a already gave it).
- `docs/adr/` and `docs/INTENT.md`'s historical-narrative section were
  given the same exemption as `decisions.md` without a third interview
  round — mechanical extension of the same principle, flagged in
  `OP-010.md` for Mikael to correct if wrong.
- Also fixed, incidentally: `retrieval-pipeline.md`'s Phase R1 used the
  real deprecated code names (`IChunkingService`/`SemanticChunkingService`)
  as if unbuilt, and its Ingestion Stages table still said Chunk stage
  "(none)" — an unresolved OP-003 `PARKING-LOT.md` item, now resolved.
- `scripts/glossary-lint.sh`/`.ps1` written and wired into `build.yml` as a
  hard-fail step. Found and fixed a real bug while testing locally: this
  environment's GNU grep 3.0 (Git-for-Windows) silently drops an
  `--exclude`/`--exclude-dir` flag placed before a later `--include` flag —
  reordered so all `--include`s precede all `--exclude`s.
- Verified: `bash scripts/glossary-lint.sh` exits 0 against current repo
  state; `dotnet build` clean (0 errors, pre-existing ICU4N warnings only —
  no `.cs` files were touched this packet). All 4 `GLOSSARY.md` rows now
  `ENFORCED`.
- **Next: OP-011** (roadmap rebuild) is next in the DAG.

## 2026-07-14 — OP-008a/OP-008c/OP-008d/OP-008e/OP-009a/OP-009b: all six code-capable + doc-reconciliation packets closed

- All 6 packets OP-006/OP-007's handover pointed to (see prior entry below) are
  now done in one session. Doc-only packets (OP-009b, OP-009a, OP-008a) were
  executed via `doc-maintainer` subagents per top-level `CLAUDE.md`'s
  Documentation Governance section; code packets (OP-008d, OP-008e, OP-008c)
  were implemented directly and each build+test+live-verified against the
  real running Postgres/Ollama instances, not just code-reviewed.
- **OP-009b** (`federation.md` ranking formula): rewritten to the real
  five-factor formula (`similarity * confidence * credibility * recencyDecay
  * localityWeight`), re-verified against `TrustScoringService.Score()` live
  source rather than paraphrased from ADR-0005. Bonus: also caught and fixed
  an adjacent, previously-unflagged drift in the same section — the
  locality-weight table said flat "Local 1.0 / Peer 0.8" but real
  `FederationConfig` uses a per-hop schedule (0.9/0.75/0.6, 0.5 beyond
  `MaxHops`); folded into this fix rather than escalated separately, since
  it's the same factor within the same section this packet was chartered to
  correct.
- **OP-008d** (`Deke.Api` ApiKey fail-fast): `Program.cs` now throws
  `InvalidOperationException` at startup if `"ApiKey"` is unconfigured;
  `ApiKeyAuthHandler`'s misleading `NoResult()` "allow all" branch removed
  entirely. Verified live: no-key run throws immediately with a clear
  message naming the missing config key; keyed run (`ApiKey=test-key-123`)
  starts cleanly, `/health` returns 200 anonymous, `POST /api/sources`
  returns 401 without/with-wrong key. `CLAUDE.md` quick-start and
  `docs/PROJECT-MAP.md` updated via doc-maintainer.
- **OP-008e** (`list_available_domains` fact-only fix): `SearchTools.
  ListAvailableDomains` now unions `ISourceRepository`- and `IFactRepository.
  GetDomainStatsAsync()`-derived domains, mirroring the pattern the
  Federation manifest endpoint already used. Verified live against the real
  Postgres instance: inserted a fact under a brand-new domain with
  `SourceId = null`, confirmed the domain was absent from the tool's output
  before the fix and present afterward (labeled "no registered source"),
  then cleaned up the test row. `docs/PROJECT-MAP.md`'s Search Tools entry
  updated via doc-maintainer.
- **OP-009a** (P1-N normalize + Phase 5→4 renumber): `specification.md` and
  `decisions.md` normalized to canonical `P1-N` shorthand;
  `retrieval-pipeline.md`'s Phase 5 (multilingual model swap) renumbered to
  Phase 4 in all three places it appeared. Note: the doc-maintainer subagent
  running this packet hit an account-wide session-limit error mid-run,
  right as it started editing `retrieval-pipeline.md` — verified afterward
  via repo-wide grep that all three target files were nonetheless fully and
  correctly normalized before it died (only Federation's separate, in-scope
  "Phase 5" and the ADR-0003 historical record itself remain, both
  correctly out of this packet's scope); nothing left dangling or
  half-edited.
- **OP-008a** (Three-Package Architecture doc reconciliation):
  `docs/product/overview.md`, `docs/science/evolution-vision.md`, and
  `docs/architecture/decisions.md` reconciled to describe the Evolution
  Engine as active Package 3, per ADR-0002. Historical `decisions.md`
  entries annotated in place (2026-04 deferral + `P3-*` entries), not
  deleted or rewritten. **New escalation — ADR-0011 (proposed, design)**:
  the 2026-04 "13-stage → 7-stage" advisory pipeline simplification was
  partly justified by Package 3 being deferred; that precondition is now
  reversed, so whether the removed stages (niche classification, quality
  prediction, escalation, signal-emission) need to return is an open
  `src/`-affecting design question the doc-maintainer subagent correctly did
  not decide unilaterally.
- **OP-008c** (retire `Llm/`, switch `PatternDiscoveryService` to
  `IChatClient`): deleted `src/Deke.Infrastructure/Llm/` (`LlmConfig.cs`,
  `GeminiLlmService.cs`, `OpenAiLlmService.cs`, `NoOpLlmService.cs`) and
  `src/Deke.Core/Interfaces/ILlmService.cs` entirely — auto-mode's
  destructive-action classifier required explicit user confirmation before
  the delete, which Mikael gave. `PatternDiscoveryService` now resolves the
  keyed `"ollama"` `IChatClient` (`AdvisoryClientKeys.Ollama`) instead,
  wrapped in a try/catch that falls back to the old templated pattern
  description if the call fails (mirrors the old `IsAvailable`-gated
  fallback behavior, just against a real backend now instead of an inert
  `NoOpLlmService`). `Deke.Worker/Program.cs` now calls
  `AddAdvisoryChatClients` directly (not the full `AddDekeAdvisory`) —
  narrower wiring, avoids pulling in the unrelated Advisory pipeline,
  adapters, and interaction repository Worker doesn't need. Verified:
  `dotnet build`/`dotnet test` (68/68) pass; repo-wide grep for
  `ILlmService|LlmProvider|GeminiLlmService|OpenAiLlmService|
  NoOpLlmService|LlmConfig` returns zero matches in any `.cs` file;
  live-verified the actual keyed-client call path against a real local
  Ollama instance (`qwen2.5:7b`) via a throwaway harness, got a real
  summarization response back. `docs/PROJECT-MAP.md`'s Llm entry and all
  four Host Composition entries (Api/Mcp/Worker/DI Composition Root)
  updated via doc-maintainer to drop every stale `AddDekeLlm` reference,
  not just the obvious one.
- 2 PARKING-LOT items resolved in place (not deleted, annotated): the
  `AddDekeLlm`-with-no-consumer item (OP-004d, resolved by OP-008c) and the
  "Two-Package Architecture" stale-doc item (OP-005d, resolved by OP-008a).
  1 new PARKING-LOT item logged: `CLAUDE.md`'s "Search facts" example uses
  `GET /api/search`, but the real endpoint is `POST` with a JSON body —
  found incidentally by the OP-008d doc-maintainer pass, out of scope for
  that packet.
- **Exit-criteria check** (§8, `OVERHAUL-SKETCH-v0.2.md`): criteria 1-4 were
  already satisfied per the prior entry below. This session's work doesn't
  touch criteria 5-8 (tooling re-verification, spec lint, roadmap rebuild,
  fresh-agent acceptance test) — those remain OP-010..013, not yet started.
- **Next:** ADR-0011 needs adjudication (advisory pipeline stage
  restoration, spawned by OP-008a) before any packet it would spawn can be
  sized. Otherwise OP-010 (tooling re-verification) is next in the DAG.
- Open questions for Mikael: approve/reject/amend ADR-0011.

## 2026-07-07 — OP-006 + OP-007 run lightweight (done, both packets closed)

- By execution time, **all 9 ADRs were already `accepted` and all 3
  glossary rows already `APPROVED`** (everything adjudicated ad-hoc, out of
  DAG order, across OP-002..005) — OP-006/007's original scope (collect+
  adjudicate a backlog of open ADR/glossary questions) was almost entirely
  pre-empted, same pattern as OP-008b's cancellation. Asked Mikael directly
  (3 closed questions via `AskUserQuestion`) whether to skip both packets
  entirely or still run a lightweight pass — **chose to run lightweight**.
- **3 questions answered:** (1) OP-005a's "breaking changes OK" aside is a
  **general DEKE-wide stance**, not federation-specific — `docs/INTENT.md`
  updated in place, no ADR needed. (2) OP-005d's `adhoqnotes.md` vectorless-
  RAG-hybrid idea is **promoted to a real packet** — recorded as new
  **ADR-0010** (accepted), deferred to OP-011 (roadmap rebuild) for sizing
  since it needs research first, not an immediate OP-008 slot;
  `docs/INTENT.md`'s adhoqnotes.md section updated to cite the resolution
  instead of sitting as a bare inferred flag. (3) confirmed: run OP-006/007
  lightweight rather than skip.
- Wrote `overhaul/packets/OP-006.md` and `OP-007.md` (both `Status: done`,
  documenting the lightweight run rather than left as unused DAG
  placeholders).
- **Exit-criteria check** (§8, `OVERHAUL-SKETCH-v0.2.md`): criteria 1
  (no PROPOSED glossary rows), 2 (no unescalated design smells /
  LOW-confidence map entries), 3 (no unconfirmed INTENT sections), and 4
  (no proposed ADRs) are now **all satisfied**. Criteria 5-8 (tooling
  re-verification, spec lint, roadmap rebuild, fresh-agent acceptance test)
  remain for OP-010..013.
- 0 new parking-lot items this round (the one from OP-005d already logged).
- **Next:** code-capable work can now proceed in parallel — **OP-008a**
  (Three-Package Architecture doc reconciliation, ADR-0002), **OP-008c**
  (merge `Llm/` onto `IChatClient`, ADR-0007), **OP-008d** (Api key
  fail-fast, ADR-0008), **OP-008e** (merge fact-derived domains into
  `list_available_domains`, ADR-0009) — all `pending`, all code-capable,
  `OP-008b` stays `CANCELLED` (ADR-0006 mooted it). **OP-009a/b** (spec
  refactor: P1-N/Phase renumber, federation.md ranking formula) can run in
  parallel too. Whoever picks up next should read the specific `OP-008x`/
  `OP-009x` packet file for its own context budget — this STATE.md entry is
  not itself a packet to execute.
- Open questions for Mikael: none new.

## 2026-07-07 — OP-005d standalone briefs + ad-hoc notes intent distillation (done, OP-005 series complete)

- Appended 5 sections to `docs/INTENT.md` from the 4 remaining `thoughts/`
  files (~314 lines): docs' three-branch Diataxis/arc42-lite separation
  (already fully executed, matches `docs/INDEX.md` verbatim), the
  present-tense-product-model/honest-roadmap documentation philosophy
  (explains *why* several existing PARKING-LOT doc-drift items are drift, as
  instances of one general rule), Package 3's demote-then-repromote history
  (`product-checkpoint-fixes` demoted it, ADR-0002 later reversed that —
  flagged explicitly so the reversal isn't missed), the Llm/ Gemini-OpenAI
  system's original purpose (unblock Advisory — later superseded when
  Advisory adopted `IChatClient` instead, explaining ADR-0006/0007's "looks
  orphaned" finding), and `adhoqnotes.md`'s one-line vectorless-RAG idea
  (entirely `(inferred, unconfirmed)` per the packet's own instruction, no
  surrounding context exists to ground it further).
- **New PARKING-LOT item logged** (not an ADR — decision-ahead-of-doc, not a
  fresh design disagreement): `docs/product/overview.md` still reads "The
  Two-Package Architecture" and lists only 2 packages, even though ADR-0002
  (accepted, OP-003) already promoted Evolution Engine back to a full third
  package — `overview.md` was never updated after that reversal. Flagged for
  a future OP-009 packet.
- No new ADR escalated. `llm-provider-config-brief.md`'s tie to ADR-0006/
  ADR-0007 was read for background only, per the packet's own instruction —
  not re-escalated (it explains the *origin* of the already-adjudicated
  finding, doesn't change it). `docs-overhaul-brief.md`'s tie to the
  existing "top-level docs never adopted the package model" PARKING-LOT item
  (OP-003) was cross-checked and confirmed as the same finding — not
  re-logged.
- 1 parking-lot item (new, above). 1 section (`adhoqnotes.md`) is entirely
  `(inferred, unconfirmed)` per the packet's explicit instruction; all other
  4 sections cite direct statements.
- **The OP-005 intent-distillation series (a/b/c/d) is now fully complete.**
  `docs/INTENT.md` holds 30 sections total across Federation (10),
  Knowledge Base Foundation (8), Advisory Pipeline MVP (7), and standalone
  threads (5). Across all four sub-packets: 0 new ADRs, 1 new parking-lot
  item (this entry), 1 section fully inferred/unconfirmed (flagged for
  OP-006), and no plan-vs-built drift found anywhere in the a/b/c threads.
- **Next:** OP-006 — interview packet (closed questions from OP-003..005,
  including all escalated design ADRs) → Mikael. Read `packets/OP-006.md`
  (create if it doesn't exist yet, per the packet DAG in
  `OVERHAUL-SKETCH-v0.2.md` §6).
- Open questions for Mikael: the accumulated set from OP-003..005 — this is
  exactly what OP-006 exists to collect and front-load as closed questions,
  not to resolve ad-hoc here.

## 2026-07-07 — OP-005c Advisory Pipeline MVP intent distillation (done)

- Appended 7 sections to `docs/INTENT.md` from the 4 Advisory Pipeline MVP
  `thoughts/` files (~436 lines): code-over-docs-when-conflicting (3rd
  independent confirmation of the pattern), `IChatClient` as a deliberate
  second LLM abstraction alongside `ILlmService` (ties to ADR-0006/0007, not
  re-litigated), honesty cap as a pipeline-enforced architectural invariant,
  `knowledge_depth_score`-driven automatic model-tier routing, per-feature
  audit tables (`advisory_interactions` distinct from `interaction_logs`
  despite surface similarity), Software Product Advisor as DEKE's
  self-advisory closing the loop with OP-005b's bootstrap intent, and a
  manual live-e2e verification gate for cross-cutting features (complements,
  doesn't contradict, OP-005b's pure-unit-tests-in-CI intention).
- Cross-checked against `docs/PROJECT-MAP.md`'s Advisory Pipeline Contract,
  Advisory Adapter Plugin, Advisory Pipeline Implementation, Advisory Tools
  entries: **no plan-vs-built drift found** — third consecutive clean OP-005
  packet. The session's 3 live-e2e runtime bugs (M.E.AI/Anthropic.SDK pin,
  enum-as-int, advisory→federation trust coupling) were all found *and
  fixed* within the same thread — not live issues, not re-flagged.
- No new ADR escalated: the two smell-adjacent findings (ILlmService/
  IChatClient duality; `GetDomainAdvice` PascalCase naming) already
  substantiate ADR-0006/0007 and an existing PARKING-LOT style-nit
  respectively — neither re-escalated.
- 0 parking-lot items. 0 sections flagged `(inferred, unconfirmed)`.
- **Next:** OP-005d — misc standalone threads (docs-overhaul,
  product-checkpoint-fixes, llm-provider-config, adhoqnotes) intent
  distillation, the last OP-005 sub-packet. Read `packets/OP-005d.md`.
- Open questions for Mikael: none new.

## 2026-07-07 — OP-005b Knowledge Base Foundation intent distillation (done)

- Appended 8 sections to `docs/INTENT.md` from the 4 KB Foundation
  `thoughts/` files (~407 lines): merge-time pure trust scoring, neutral
  fallback for missing credibility (federated-result zeroing bug caught in
  plan validation), no-double-counting-credibility principle, chunking via
  library (SemanticChunker.NET, reinforces OP-005a's library-over-hand-rolled
  theme), bootstrap self-ingestion as primary-source domain, interaction
  logging as capture-only + top-level-only, hand-edited `init.sql` with no
  migration tooling (cross-thread confirmed with federation-phase1), unit
  tests over mocked repos accepted in lieu of a DB integration harness.
- Cross-checked against `docs/PROJECT-MAP.md`'s Fact & Source Domain, Term &
  Pattern Domain, Search & Trust Contracts, Data Access & Type Handlers,
  Embeddings, Harvesters, Extraction, Repositories, Trust entries: **no
  plan-vs-built drift found** — matches OP-005a's pattern of a clean packet.
- No new ADR escalated: the only design-smell-adjacent finding
  (`ITrustScoringService`'s 5-factor formula) already substantiates ADR-0005
  (not re-escalated, same as OP-004a/OP-004b's prior notes).
- 0 parking-lot items. 0 sections flagged `(inferred, unconfirmed)` — all 8
  cite direct statements from the source files.
- **Next:** OP-005c — Advisory Pipeline MVP intent distillation. Read
  `packets/OP-005c.md`.
- Open questions for Mikael: none new.

## 2026-07-07 — OP-005a Federation intent distillation (done)

- Created `docs/INTENT.md` (new file) with 10 sections distilled from the 7
  Federation phase1+phase2 `thoughts/` files (~1334 lines): opt-in/additive
  federation, static peer config now, library-over-hand-rolled patterns
  (Polly/`IOptions<T>`), manifest-driven polling discovery, local-first
  delegation threshold, protocol-level loop prevention, locality-weighted
  scoring, MCP-and-REST-as-peers, breaking-changes-acceptable, auth-deferred.
- Cross-checked against `docs/PROJECT-MAP.md`'s Federation, Federation
  (Infrastructure), Search Endpoints, Federation Endpoints, Search Tools, and
  Peer Health Check entries: **no plan-vs-built drift found** — Phase 1 and
  Phase 2 shipped essentially as planned (unusually clean packet).
- No new ADR escalated: nothing found was a fresh design smell. Two sections
  cross-reference already-known items without re-escalating (ADR-0005
  ranking-formula drift; ADR-0008 unrelated Api auth defect, explicitly
  distinguished from federation's separately-deferred Phase 5 auth).
- 0 parking-lot items (no drift to log). 1 section carries a partial
  `(inferred, unconfirmed)` aside (whether "breaking changes OK" generalizes
  DEKE-wide beyond federation) — flagged for OP-006, not presented as fact.
- **Next:** OP-005b — Knowledge Base Foundation intent distillation. Read
  `packets/OP-005b.md`.
- Open questions for Mikael: none new (same open set as prior entry — none
  block OP-005b).

## 2026-07-07 — OP-004f tests/Deke.Tests mapping sweep (done, OP-004 series complete)

- Added 6 `docs/PROJECT-MAP.md` entries for `tests/Deke.Tests`, grouping its 9
  test files by production region: Federation & Search Contract Model Tests,
  Semantic Chunking Tests, Trust Scoring Tests, Federated Search Interaction
  Logging Tests, Bootstrap Ingestion Tests, Advisory Pipeline & Model
  Selection Tests. All `KEEP`, all Confidence HIGH (all 9 files read in
  full).
- No new ADR escalated: nothing found was a design disagreement. Two factual
  findings — `Deke.Tests.csproj` has no `<ProjectReference>` to `Deke.Api` or
  `Deke.Mcp`, and `TrustScoringTests`/`LocalityWeightTests` re-confirm
  ADR-0005's ranking-formula drift — neither warranted a new ADR.
- 2 parking-lot items: no shared test-helpers file (`FakeEmbeddingService`
  copied verbatim into 3 files, `FakeFactRepository` into 2); no test
  coverage anywhere for `Deke.Api`/`Deke.Mcp` — notably `ApiKeyAuthHandler`
  (ADR-0008's subject) has zero tests either way, and
  `FederatedSearchService`'s actual peer-delegation path is untested (only
  the local-only branch is covered).
- **OP-004 mapping-sweep series is now fully complete** (OP-004a..f, all
  regions of `src/` + `tests/`).
- Drafted `overhaul/packets/OP-005a.md` through `OP-005d.md` (all
  `Status: pending`) — split intent distillation by feature thread, sized
  against `thoughts/` line counts (Federation 7 files/~1334 lines;
  Knowledge Base Foundation 4 files/~407 lines; Advisory Pipeline MVP 4
  files/~436 lines; standalone briefs + `adhoqnotes.md` 4 files/~314 lines).
- **Next:** OP-005a — Federation intent distillation. Read
  `packets/OP-005a.md`.
- Open questions for Mikael: none new. Still open from prior entries:
  ADR-0007-spawned OP-008c, ADR-0008 (proposed, re: `ApiKeyAuthHandler` —
  now also tied to the coverage-gap item above), ADR-0009-spawned OP-008e —
  none block OP-005.

## 2026-07-07 — OP-004e Deke.Worker mapping sweep (done)

- Added 6 `docs/PROJECT-MAP.md` entries for `src/Deke.Worker`: Source
  Monitoring, Bootstrap Ingestion, Pattern Discovery, Relation Mapping
  (Learning Cycle), Peer Health Check, Host Composition (Program.cs). All
  `KEEP`, all Confidence HIGH (every `Services/` file + `Program.cs` read in
  full).
- No new ADR escalated: nothing found rose to a design disagreement needing
  Mikael's adjudication. `PatternDiscoveryService`'s `ILlmService` dependency
  re-confirms (by direct read, not just grep) ADR-0006/ADR-0007's existing
  finding — not re-escalated.
- Resolved the packet's named citation check: `docs/GLOSSARY.md`'s
  `IChunker`/`SemanticChunkerAdapter` row's "two Worker services" are
  `SourceMonitorService` and `BootstrapIngestionService` — both confirmed by
  direct read, no glossary edit needed (row already APPROVED and accurate).
- 2 parking-lot items: bootstrap ingestion is fully built but `roadmap.md`
  (lines 17, 31) and `specification.md`'s project-structure tree
  (`specification.md:65-66`) both still describe it as unbuilt/"Planned";
  `SourceMonitorService`/`BootstrapIngestionService` duplicate the same
  chunk→extract→embed→store pipeline with no shared helper.
- **Next:** OP-004f — tests/Deke.Tests mapping sweep (last OP-004 sub-packet).
- Open questions for Mikael: none new.

## 2026-07-07 — Ad-hoc adjudication: ADR-0009 accepted

- **ADR-0009 accepted:** `list_available_domains` unions `ISourceRepository`-
  derived domains with `IFactRepository.GetDomainStatsAsync()` results, so
  fact-only domains appear too. Spawns **OP-008e** (drafted, pending,
  code-capable, Sonnet-tier — small well-scoped fix, design already decided).
- Adjudicated ad-hoc, out of DAG order (same pattern as the ADR-0002..0005,
  ADR-0006, and ADR-0007/ADR-0008 rounds) — OP-004d's next-packet pointer
  (OP-004e, Deke.Worker sweep) is unaffected and still next in the
  mapping-sweep sequence, running in parallel with OP-008e.
- Open questions for Mikael: none new.

## 2026-07-07 — OP-004d Deke.Mcp mapping sweep (done)

- Added 4 `docs/PROJECT-MAP.md` entries for `src/Deke.Mcp`: Fact Tools,
  Search Tools, Advisory Tools, Host Composition (Program.cs). All `KEEP`,
  all Confidence HIGH (every source file read in full).
- **New finding — ADR-0009 (proposed, design):** `SearchTools.
  ListAvailableDomains` (`list_available_domains`) derives domains solely
  from `ISourceRepository`, never `IFactRepository.GetDomainStatsAsync()` —
  so a domain created via the documented zero-config `add_fact`-only
  workflow (top-level `CLAUDE.md`) is invisible to MCP clients. 3 options
  put to Mikael (merge in fact-derived domains / narrow the documented
  workflow / leave as-is).
- 3 parking-lot items: `FactTools`' three tools undocumented in
  `specification.md`'s MCP Tools table; `Deke.Mcp/Program.cs` registers
  unused `AddDekeLlm` (same pattern as `Deke.Api`, relevant to OP-008c);
  `GetDomainAdvice` is the sole PascalCase MCP tool name.
- No `docs/GLOSSARY.md` ties found — none of its 3 canonical terms appear
  under `src/Deke.Mcp`.
- **Next:** OP-004e — Deke.Worker mapping sweep.
- Open questions for Mikael: approve/reject/amend ADR-0009 (no other new
  ones — ADR-0006/0007/0008 already resolved per the entries below).

## 2026-07-07 — Ad-hoc adjudication: ADR-0007 + ADR-0008 accepted

- **ADR-0007 accepted:** merge `Llm/` (Gemini/OpenAI) onto Advisory's keyed
  `IChatClient` system — retire `ILlmService`/`LlmProvider`/`GeminiLlmService`/
  `OpenAiLlmService`/`NoOpLlmService`, switch `PatternDiscoveryService` to a
  keyed `IChatClient`. Spawns **OP-008c** (drafted, pending, code-capable).
- **ADR-0008 accepted:** `Deke.Api` must fail fast at startup if `"ApiKey"`
  is unconfigured, in every environment — no more silent `NoResult()`
  "allow all" branch. Spawns **OP-008d** (drafted, pending, code-capable,
  Sonnet-tier — small well-scoped fix, design already decided).
- Both adjudicated ad-hoc via interview-style AskUserQuestion, out of DAG
  order (same pattern as the ADR-0002..0005 and ADR-0006 ad-hoc rounds) —
  OP-004d (Deke.Mcp sweep) is unaffected and still next in the mapping-sweep
  sequence.
- **Next:** OP-004d (Deke.Mcp mapping sweep) continues unblocked, in
  parallel with — not blocked by — OP-008c/OP-008d, which need a
  code-capable session rather than a mapping-sweep session.
- Open questions for Mikael: none new. No proposed ADRs remain open as of
  this entry.

## 2026-07-07 — OP-004c Deke.Api mapping sweep (done)

- Added 6 `docs/PROJECT-MAP.md` entries for `src/Deke.Api`: Fact Endpoints,
  Source Endpoints, Search Endpoints, Federation Endpoints, API Key
  Authentication, Host Composition (Program.cs). All `KEEP`, all Confidence
  HIGH (every source file read in full).
- **New finding — ADR-0008 (proposed, design):** `ApiKeyAuthHandler`'s
  unconfigured-key branch returns `AuthenticateResult.NoResult()` behind a
  comment claiming this "allow[s] all requests (development mode)" — it
  actually does the opposite: `NoResult()` leaves the request
  unauthenticated, so every `RequireAuthorization()` write endpoint is
  rejected, not allowed. `appsettings.json` ships `"ApiKey": ""` by default,
  so this is out-of-the-box behavior, not an edge case, and contradicts
  top-level `CLAUDE.md`'s own no-API-key curl examples. 3 options put to
  Mikael (make comment true / require key everywhere / leave behavior,
  fix comment).
- 1 parking-lot item: `specification.md`'s Fact/Source Endpoints tables
  (current-scope, not phase-gated) document `PUT /api/facts/{id}`,
  `DELETE /api/facts/{id}`, `PUT /api/sources/{id}` — none implemented.
  Implementation-behind-spec gap, not a design smell; flagged as a future
  small packet, not escalated as an ADR.
- **Next:** OP-004d — Deke.Mcp mapping sweep. Read `packets/OP-004d.md`.
- Open questions for Mikael: approve/reject/amend ADR-0008 (also still open:
  ADR-0006 accepted; ADR-0007 open per the entry below, unrelated).

## 2026-07-07 — Ad-hoc adjudication: ADR-0006 accepted, OP-008b cancelled, ADR-0007 opened

- ADR-0006 accepted: confirmed Advisory pipeline's `IChatClient` backends
  (Anthropic/Ollama) are real and already match `specification.md` —
  ADR-0004's premise was wrong.
- **OP-008b cancelled** (packet file updated with cancellation note) — no
  backends to implement.
- **ADR-0007 opened (proposed):** is `Llm/`'s Gemini/OpenAI system (used
  only by `Deke.Worker/PatternDiscoveryService.cs`) an intentional
  cost-isolated second backend, or should it be merged onto Advisory's
  `IChatClient`/Anthropic-Ollama system? Left proposed, not decided —
  Mikael chose to open the question rather than resolve it immediately.
- **Next:** OP-004c (Deke.Api sweep) continues unblocked, in parallel with —
  not blocked by — ADR-0007 awaiting adjudication.
- Open questions for Mikael: approve/reject/amend ADR-0007, whenever the
  next adjudication round happens (ad-hoc or OP-006/OP-007).

## 2026-07-07 — OP-004b Deke.Infrastructure mapping sweep (done)

- Added 10 `docs/PROJECT-MAP.md` entries for `src/Deke.Infrastructure`: Data
  Access & Type Handlers, Embeddings, Harvesters, Extraction, Repositories,
  Trust, Federation (Infrastructure), Llm — Gemini/OpenAI Backend, Advisory
  Pipeline Implementation, DI Composition Root. All KEEP, all Confidence
  HIGH.
- **Major finding — ADR-0006 (proposed, design):** ADR-0004's premise was
  wrong. Anthropic/Ollama backends already exist and are fully wired —
  `Advisory/ChatClientRegistration.cs` + `LlmSelectionPolicy.cs` implement
  exactly the routing policy `specification.md` documents (model IDs match
  verbatim), called live from `AdvisoryPipeline.AdviseAsync`. ADR-0004's
  audit only ever read `Llm/LlmConfig.cs` — a second, independent
  `ILlmService` system (Gemini/OpenAI/NoOp) whose only consumer is
  `Deke.Worker/PatternDiscoveryService.cs`, unrelated to Advisory. Did **not**
  edit ADR-0004 (out of scope, additive only) — ADR-0006 corrects the record
  and asks Mikael to adjudicate OP-008b's fate (likely moot/cancel) plus
  whether the Gemini/OpenAI system's undocumented status needs its own
  design decision.
- No parking-lot items logged this packet (nothing minor surfaced —
  everything found was either clean or ADR-0006-worthy).
- **Next:** OP-004c — Deke.Api mapping sweep. Read `packets/OP-004c.md`.
- Open questions for Mikael: approve/reject/amend ADR-0006 (also still
  pending: ADR-0001 tooling item already resolved; ADR-0002..0005 already
  adjudicated per the entry below — ADR-0006 is the only new open one).

## 2026-07-07 — Ad-hoc adjudication: ADR-0002..0005 + glossary (out of DAG order)

- ADR-0002 accepted: Evolution Engine → active package; Three-Package
  Architecture (was Two-Package). ADR-0003 accepted: renumber Phase 5→4,
  closes gap; P1-N shorthand confirmed. ADR-0004 accepted: implement missing
  Anthropic/Ollama LLM backends (code catches up to docs). ADR-0005 accepted:
  rewrite `federation.md` to the real 5-factor formula (doc catches up to
  code).
- Glossary: Evolution Engine, P1-N, IChunker/SemanticChunkerAdapter all →
  APPROVED; "Related escalations" section removed (nothing open remains).
- Drafted 4 pending packets: OP-008a (Three-Package Architecture doc
  reconciliation), OP-008b (implement Anthropic/Ollama backends — needs a
  code-capable session), OP-009a (P1-N normalization + Phase 5→4 renumber),
  OP-009b (federation.md ranking formula rewrite).
- **Ordering note:** this happened ad-hoc, out of DAG sequence — OP-004's
  mapping sweeps are only partway done (OP-004a complete, OP-004b next).
  OP-006/OP-007 should treat these 4 ADRs + 3 glossary rows as already
  resolved and focus only on what OP-004b-f and OP-005 additionally surface.
- **Next:** OP-004b (Deke.Infrastructure sweep) continues unblocked, in
  parallel with — not blocked by — the new OP-008/009 packets.
- Open questions for Mikael: none new.

## 2026-07-07 — OP-004a Deke.Core mapping sweep (done)

- Created `docs/PROJECT-MAP.md` with 6 entries for `src/Deke.Core`: Fact & Source Domain, Term & Pattern Domain, Search & Trust Contracts, Advisory Pipeline Contract, Advisory Adapter Plugin (Layer 3), Federation. All `KEEP`, all Confidence HIGH (every source file read in full).
- No new ADR escalated: no genuine design smell found beyond what OP-003 already raised. `ITrustScoringService`'s signature re-confirms ADR-0005 (federation ranking formula drift) rather than warranting a new one.
- 1 parking-lot item: `DomainStats` (a Fact-domain type) is defined in `Models/FederationManifest.cs` — file-location nit, not a naming conflict.
- Drafted `overhaul/packets/OP-004b.md` through `OP-004f.md` (all `Status: pending`) for the remaining regions: Deke.Infrastructure, Deke.Api, Deke.Mcp, Deke.Worker, tests/Deke.Tests.
- **Next:** OP-004b — Deke.Infrastructure mapping sweep. Read `packets/OP-004b.md`.
- Open questions for Mikael: none new; ADR-0002 through ADR-0005 and the 3 PROPOSED glossary rows still await adjudication (see prior entries).

## 2026-07-07 — OP-003 Glossary ingestion (done)

- Ingested `thoughts/shared/namingissues.md` (9 findings) → `docs/GLOSSARY.md`, 3 PROPOSED rows: Evolution Engine (was "Package 3"/"P3"), P1-N (Package 1 phase shorthand), IChunker/SemanticChunkerAdapter (was IChunkingService/SemanticChunkingService).
- Escalated 4 proposed ADRs: ADR-0002 (naming: Evolution Engine/Package 3 status disagreement), ADR-0003 (naming: Package 1 phase shorthand + undefined Phase 4), ADR-0004 (design: LLM backend docs vs. code drift), ADR-0005 (design: federation ranking formula doc vs. code drift).
- Logged 4 items in PARKING-LOT.md: chunking status doc bug, top-level-docs package-model gap, dangling SPECIFICATION.md reference, roadmap.md Phase 3 label drop.
- **Next:** OP-004a..n — mapping sweeps (PROJECT-MAP.md entries).
- Open questions for Mikael: approve/reject/amend ADR-0002 through ADR-0005; approve/reject/amend the 3 PROPOSED glossary rows.

## 2026-07-07 — OP-002 Tooling audit (done)

- Inventoried CLI/MCP/plugin tooling actually available in DEKE sessions; wrote `/docs/TOOLING.md` (inventory, verification suite, model/thinking tiers).
- KEEP: git, gh (Piscatore), dotnet SDK, podman, postgres-mcp, nuget MCP, serena, roslyn + cwm-roslyn-navigator (blocked, see below), dotnet-claude-kit + several skills.
- DISCONNECT (pending Mikael confirm): docker-mcp (Docker Desktop not running; DEKE uses Podman), LSP (csharp-ls missing, redundant), sqlcl + windows-desktop (Avient bleed-through), system-monitor, Google Calendar/Matrixify/Mermaid Chart/Google Drive/DesignSync, graphify, smithery-ai-cli, qf-*/verbose-* skills, dataviz/artifact-design, playwright-skill.
- Escalated ADR-0001 (type: tooling, proposed): OllamaSharp exact-pin (`5.3.0`, not actually cached — NuGet floats to `5.3.1`) causes NU1603 that aborts both Roslyn MCP servers' symbol-level ops, though `dotnet build` succeeds fine. Fix suggested: retarget pin to `5.3.1`.
- **Next:** OP-003 — ingest `namingissues.md` → GLOSSARY.md rows (all PROPOSED).
- Open questions for Mikael: confirm the DISCONNECT list above; approve/reject ADR-0001.
- ADR-0001 accepted and resolved same session: OllamaSharp bumped to 5.3.1, both Roslyn MCP tools confirmed working. (Also: original audit's "cwm-roslyn-navigator find_symbol FAIL" was a false negative from an invalid tool call, corrected in TOOLING.md.)

## 2026-07-03 — OP-001 Bootstrap (seed)

- Created `/overhaul` (CHARTER.md, STATE.md, PARKING-LOT.md, packets/,
  OVERHAUL-SKETCH-v0.2.md). Deliberately did NOT touch `/docs` — it already
  exists in the repo; later packets add individual named files there
  (starting with `docs/TOOLING.md` in OP-002) rather than pre-scaffolding it.
- Plan agreed with Mikael across a planning conversation on 2026-07-03;
  full rationale in `OVERHAUL-SKETCH-v0.2.md`.
- Design/feature redesign is in scope, via the ADR escalation path in
  CHARTER.md — not frozen, but gated.
- **Next:** OP-002 — tooling audit. Read `packets/OP-002.md`. Produces
  `/docs/TOOLING.md`.
- Open questions for Mikael: none yet.
