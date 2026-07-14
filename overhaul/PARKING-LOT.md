# Parking Lot

Ideas, issues, or scope questions raised during the Overhaul that are
deliberately deferred. This is a holding pen, not a backlog — not every
entry needs action. Review periodically (e.g. at each adjudication packet)
and either promote to a proposed ADR / packet, or cross out as declined.

Format:

```
## <date> — <short title>
- Raised during: OP-00n
- What: ...
- Why deferred: ...
```

---

## 2026-07-07 — cwm-roslyn-navigator TargetFramework misreport
- Raised during: OP-002
- What: `get_project_graph` reports `TargetFramework: netcoreapp1.0` for every DEKE project; actual is `net9.0` (confirmed via obj/ output and a successful `dotnet build`).
- Why deferred: cosmetic tool bug, doesn't block use of the project-graph tool itself; not worth an ADR. Noted here in case it causes confusion in a future mapping packet.
- **Reviewed 2026-07-14 (OP-011)**: stays parked — external tool bug, not DEKE roadmap work.

## 2026-07-07 — Chunk stage status wrong in retrieval-pipeline.md
- Raised during: OP-003
- What: retrieval-pipeline.md's Phase R1 deliverables list "IChunkingService"/"SemanticChunkingService" as if not yet built, and its pipeline table marks the Chunk stage "Current: (none)." The code already implements this — as `IChunker`/`SemanticChunkerAdapter`, wired in `ServiceCollectionExtensions.cs:66` and used by two Worker services.
- Why deferred: plain doc-accuracy bug, not a naming conflict (the naming half is covered by the GLOSSARY.md row this packet added) or a design smell. Flagged for a future OP-009 spec-refactor packet to correct retrieval-pipeline.md's Phase R1 status.
- **Resolved 2026-07-14 by OP-010**: Phase R1 Deliverables list corrected to `IChunker`/`SemanticChunkerAdapter` with "Done" status framing; Ingestion Stages table's Chunk row "Current" cell fixed from "(none)" to the real implemented names.

## 2026-07-07 — Top-level docs (README/CLAUDE.md) never adopted the package model
- Raised during: OP-003
- What: README.md and CLAUDE.md's architecture diagram still describe DEKE as a three-stage Ingest → Learn → Serve pipeline. Neither mentions "packages," "Knowledge Base," or "Knowledge Leverage," the terminology introduced in docs/product/overview.md.
- Why deferred: cross-doc consistency gap, not a naming conflict to resolve via glossary nor a design smell. Flagged for a future top-level-docs refresh packet.
- **Promoted 2026-07-14 by OP-011**: → HYG-1 (doc-sync sweep) in `docs/ROADMAP.md`.

## 2026-07-07 — Dangling SPECIFICATION.md reference
- Raised during: OP-003
- What: specification.md's project-structure tree (line 37) lists a root SPECIFICATION.md file annotated "(deprecated — content moved here)." That file does not exist anywhere in the repo — a leftover from a past migration.
- Why deferred: trivial cleanup, not worth an ADR. Flagged for a later cleanup pass.
- **Promoted 2026-07-14 by OP-011**: → HYG-1 (doc-sync sweep) in `docs/ROADMAP.md`.

## 2026-07-07 — roadmap.md drops the Phase 3 label for Federation MCP Tools
- Raised during: OP-003
- What: roadmap.md's "Current State" table marks Federation (Phase 1–2) done and lists MCP Tools as a capability, but drops the "Phase 3" label that federation.md uses when marking that same capability Complete.
- Why deferred: small labeling gap, consistent with the broader phase-numbering pattern (see ADR-0003) but not itself worth a separate ADR. Flagged for a later cleanup pass.
- **Resolved 2026-07-14 by OP-011**: roadmap rebuilt as `docs/ROADMAP.md`; its Current State table now labels Federation Phases 1–3 explicitly.

## 2026-07-07 — DomainStats model filed under Federation, used by Fact domain
- Raised during: OP-004a
- What: `DomainStats` (domain name + fact count + last-updated) is defined in `src/Deke.Core/Models/FederationManifest.cs`, but it's returned by `IFactRepository.GetDomainStatsAsync()` — a Fact & Source Domain method, not a Federation one.
- Why deferred: file-organization nit, not a naming conflict (the type's own name is fine) or a design smell (behavior is correct). `src/` is out of scope for OP-004a (read-only). Flagged for a later code-cleanup pass.
- **Reviewed 2026-07-14 (OP-011)**: stays parked — ride-along row in `docs/ROADMAP.md`'s Trigger-Gated table (fix with the next packet touching the file).

## 2026-07-07 — Deke.Api missing PUT/DELETE endpoints specification.md documents as current scope
- Raised during: OP-004c
- What: `specification.md`'s Fact Endpoints and Source Endpoints tables (not the separately phase-gated "Planned Endpoints" table) document `PUT /api/facts/{id}`, `DELETE /api/facts/{id}` (soft-delete), and `PUT /api/sources/{id}` — none of the three exist in `FactEndpoints.cs`/`SourceEndpoints.cs`. `FactEndpoints` has no delete at all (fact "soft-delete/mark outdated" is undocumented as unbuilt).
- Why deferred: implementation-behind-spec gap, not a design disagreement (no one disputes what these endpoints should do) — doesn't warrant an ADR. Flagged as a small, concrete future packet: add the three endpoints to match the already-documented contract.
- **Promoted 2026-07-14 by OP-011**: → HYG-2 (missing REST endpoints) in `docs/ROADMAP.md`.

## 2026-07-07 — Deke.Mcp's add_fact/get_fact/get_domain_stats undocumented in specification.md
- Raised during: OP-004d
- What: `specification.md`'s "MCP Tools — Current (implemented)" table (`specification.md:384-393`) lists only `consult_domain_expert`, `get_context`, `list_available_domains`, `GetDomainAdvice`. `Tools/FactTools.cs`'s three MCP tools — `add_fact`, `get_fact`, `get_domain_stats` — are fully implemented and registered (`Program.cs`'s `.WithTools<FactTools>()`) but appear nowhere in the spec, not even in a "Planned" section.
- Why deferred: implementation-ahead-of-spec gap (the mirror image of the OP-004c item above), not a design disagreement — doesn't warrant an ADR. Flagged as a future small packet: add a `FactTools` row group to `specification.md`'s MCP Tools table.
- **Promoted 2026-07-14 by OP-011**: → HYG-1 (doc-sync sweep) in `docs/ROADMAP.md`.

## 2026-07-07 — Deke.Mcp/Program.cs registers AddDekeLlm with no consumer in this project
- Raised during: OP-004d
- What: `Program.cs` calls `builder.Services.AddDekeLlm(builder.Configuration)`, but no code under `src/Deke.Mcp` references `ILlmService` — its only solution-wide consumer is `Deke.Worker/Services/PatternDiscoveryService.cs` (per ADR-0006/ADR-0007). `Deke.Api`'s Host Composition entry (`docs/PROJECT-MAP.md`) shows the identical unused-registration pattern, so this isn't new to `Deke.Mcp`.
- Why deferred: dead DI registration, not a behavior bug — no design disagreement, doesn't warrant its own ADR. ADR-0007 (accepted: retire `Llm/`, spawns OP-008c) already covers this system's fate; flagged here so OP-008c's implementer also removes this call site (and `Deke.Api`'s) when retiring `Llm/`, not just `PatternDiscoveryService.cs`'s.
- **Resolved 2026-07-14 by OP-008c**: `AddDekeLlm` and the whole `Llm/` system deleted; call sites removed from `Deke.Api`, `Deke.Mcp`, and `Deke.Worker`.

## 2026-07-07 — Bootstrap ingestion is built but docs still say "Planned"/"not yet built"
- Raised during: OP-004e
- What: `src/Deke.Worker/Services/BootstrapIngestionService.cs` is fully implemented and wired via `Program.cs`'s `--bootstrap` CLI branch — it ingests `docs/` and `thoughts/` into the `"software-product"` domain at 0.95 confidence. `docs/roadmap.md` lists "bootstrap ingestion into the Software Product Advisor domain" under "Not yet built" (line 17) and as MVP work item #7, status "Planned" (line 31). `docs/architecture/specification.md`'s project-structure tree (`specification.md:65-66`) lists only 2 of `Deke.Worker`'s 4 `BackgroundService`s (`SourceMonitorService`, `PeerHealthCheckService`) and omits `BootstrapIngestionService` entirely.
- Why deferred: implementation-ahead-of-spec gap (same pattern as OP-004d's `FactTools` finding), not a design disagreement — doesn't warrant an ADR. Flagged for a future doc-sync packet: flip roadmap.md item #7 to Done and update the specification.md tree.
- **Split 2026-07-14 by OP-011**: roadmap half resolved directly (rebuilt `docs/ROADMAP.md` records bootstrap as delivered); specification.md project-tree half promoted → HYG-1 (doc-sync sweep).

## 2026-07-07 — SourceMonitorService and BootstrapIngestionService duplicate the ingestion pipeline
- Raised during: OP-004e
- What: `SourceMonitorService.CheckSourcesAsync`'s inner loop and `BootstrapIngestionService.IngestPathAsync` both independently implement the same chunk → extract → embed → build-`Fact` → store sequence (~30 lines each), with no shared helper.
- Why deferred: code-quality/DRY nit, not a design smell (both produce correct, intentional behavior) — doesn't warrant an ADR. Flagged for a later extract-a-shared-ingestion-helper cleanup pass.
- **Reviewed 2026-07-14 (OP-011)**: stays parked — ride-along row in `docs/ROADMAP.md`'s Trigger-Gated table.

## 2026-07-07 — tests/Deke.Tests has no shared test-helpers file; fakes duplicated verbatim
- Raised during: OP-004f
- What: `tests/Deke.Tests` has no subdirectory or shared fixtures file — every
  fake dependency is a private nested class in the file that uses it.
  `FakeEmbeddingService` (identical body: `GenerateEmbedding` returns
  `new float[8]`, everything else throws `NotImplementedException`) is
  hand-copied verbatim into `BootstrapIngestionTests.cs`,
  `InteractionLoggingTests.cs`, and `AdvisoryPipelineTests.cs`.
  `FakeFactRepository(List<FactSearchResult> results) : IFactRepository`
  (identical body) is hand-copied verbatim into `InteractionLoggingTests.cs`
  and `AdvisoryPipelineTests.cs`.
- Why deferred: code-quality/DRY nit, not a design smell (all copies are
  correct and consistent) — doesn't warrant an ADR. Same pattern as the
  OP-004e Source/Bootstrap ingestion-pipeline duplication finding. Flagged
  for a later "extract shared test fakes" cleanup pass.
- **Promoted 2026-07-14 by OP-011**: → HYG-3 (test coverage) in
  `docs/ROADMAP.md` (folded into the same packet as the coverage gaps).

## 2026-07-07 — tests/Deke.Tests has no coverage for Deke.Api or Deke.Mcp
- Raised during: OP-004f
- What: `Deke.Tests.csproj` references only `Deke.Core`, `Deke.Infrastructure`,
  `Deke.Worker` — it cannot reference `Deke.Api` or `Deke.Mcp` types at all.
  Notably, `ApiKeyAuthHandler` (the exact class under **ADR-0008**, currently
  proposed, re: its unconfigured-key `NoResult()` branch) has zero unit
  tests either asserting or guarding its current behavior. Also untested:
  all MCP tools (`Tools/FactTools.cs`, `SearchTools.cs`, `AdvisoryTools.cs`),
  all REST endpoints, `RssHarvester`/`WebPageHarvester` (only
  `FileSystemHarvester` gets exercised, indirectly, via
  `BootstrapIngestionTests.cs`), every `Repositories/` implementation
  (DB-backed, no test doubles for real Postgres), and
  `SourceMonitorService`/`PatternDiscoveryService`/`LearningCycleService`/
  `PeerHealthCheckService`. `FederatedSearchService`'s actual cross-instance
  delegation path (`ShouldFederate`, parallel peer queries, result merging)
  is also untested — see the Federated Search Interaction Logging Tests
  entry in `docs/PROJECT-MAP.md`.
- Why deferred: a systematic test-coverage audit is explicitly out of scope
  for OP-004f (a mapping sweep, not a health-check) — this is what
  incidentally surfaced while mapping, not the product of a coverage audit.
  No design disagreement, so not an ADR. Flagged for a future dedicated
  test-coverage packet; the `ApiKeyAuthHandler` gap specifically worth
  picking up together with whichever packet implements ADR-0008's decision.
- **Promoted 2026-07-14 by OP-011**: → HYG-3 (test coverage) in
  `docs/ROADMAP.md`. ADR-0008's implementation (OP-008d) landed without
  tests, so HYG-3 explicitly covers the fail-fast + 401 paths.

## 2026-07-07 — docs/product/overview.md still says "Two-Package Architecture" after ADR-0002 promoted Evolution Engine back to a third package
- Raised during: OP-005d
- What: `docs/product/overview.md:31` heads its architecture section "## The Two-Package Architecture" and its package table (`:35`) lists only two packages — a leftover from the `product-checkpoint-fixes` work item, which deliberately demoted Package 3 (Evolution Engine) out of the product model into a research/vision document. ADR-0002 (accepted, OP-003) later reversed that: `docs/GLOSSARY.md`'s Evolution Engine row states it is "Promoted to full parity with Package 1 and Package 2 under DEKE's Three-Package Architecture — an active package, not deferred research." `overview.md` was never updated to reflect the reversal.
- Why deferred: decision-ahead-of-doc gap (ADR-0002 already adjudicated the substance; only the doc text is stale), not a fresh design disagreement — doesn't warrant a new ADR. Flagged for a future OP-009 spec-refactor packet: rewrite `overview.md`'s architecture section for three packages, consistent with the already-approved glossary entry.
- **Resolved 2026-07-14 by OP-008a**: `overview.md`, `evolution-vision.md`, and `decisions.md` reconciled to the Three-Package Architecture. See ADR-0011 (proposed) for a design question this reconciliation surfaced.

## 2026-07-07 — GetDomainAdvice is the sole PascalCase MCP tool name
- Raised during: OP-004d
- What: `AdvisoryTools.cs`'s MCP tool is named `GetDomainAdvice` (PascalCase, via `[McpServerTool(Name = "GetDomainAdvice")]`). Every other MCP tool across `Tools/FactTools.cs` and `Tools/SearchTools.cs` uses snake_case (`add_fact`, `get_fact`, `get_domain_stats`, `consult_domain_expert`, `get_context`, `list_available_domains`). `specification.md` documents it as `GetDomainAdvice` too, so this isn't a doc/code mismatch — just an internal naming-convention inconsistency.
- Why deferred: cosmetic, not a naming conflict for `docs/GLOSSARY.md` (no canonical-term collision) or a design smell (tool works correctly). Flagged for a later cleanup pass — rename to `get_domain_advice` for consistency, would require updating `specification.md` too.
- **Reviewed 2026-07-14 (OP-011)**: stays parked — ride-along row in `docs/ROADMAP.md`'s Trigger-Gated table.

## 2026-07-14 — CLAUDE.md's "Search facts" example uses GET, real endpoint is POST
- Raised during: OP-008d
- What: top-level `CLAUDE.md`'s "Search facts" quick-start example shows `GET /api/search?query=...&domain=...`, but `src/Deke.Api/Endpoints/SearchEndpoints.cs` implements `/api/search` as `POST` with a JSON body. Found incidentally by the OP-008d doc-maintainer pass while updating the neighboring "Add a source to monitor" example for the new `ApiKey` requirement; out of that packet's scope to fix.
- Why deferred: doc/code mismatch, not a design disagreement — doesn't warrant an ADR. Flagged for a later doc-sync pass: either fix the example to `POST` with a body, or confirm a `GET` overload was intended and add it to the endpoint.
- **Promoted 2026-07-14 by OP-011**: → HYG-1 (doc-sync sweep) in `docs/ROADMAP.md`.
