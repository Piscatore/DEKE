# STATE.md — Overhaul Baton

> Read this first, every session. Append a new entry (newest on top) when
> you finish a packet. Keep entries ≤10 lines. This file plus the artifacts
> in `/docs` are the only cross-session memory — don't assume anything else
> carries over between sessions.

---

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
