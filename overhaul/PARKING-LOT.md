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

## 2026-07-07 — Chunk stage status wrong in retrieval-pipeline.md
- Raised during: OP-003
- What: retrieval-pipeline.md's Phase R1 deliverables list "IChunkingService"/"SemanticChunkingService" as if not yet built, and its pipeline table marks the Chunk stage "Current: (none)." The code already implements this — as `IChunker`/`SemanticChunkerAdapter`, wired in `ServiceCollectionExtensions.cs:66` and used by two Worker services.
- Why deferred: plain doc-accuracy bug, not a naming conflict (the naming half is covered by the GLOSSARY.md row this packet added) or a design smell. Flagged for a future OP-009 spec-refactor packet to correct retrieval-pipeline.md's Phase R1 status.

## 2026-07-07 — Top-level docs (README/CLAUDE.md) never adopted the package model
- Raised during: OP-003
- What: README.md and CLAUDE.md's architecture diagram still describe DEKE as a three-stage Ingest → Learn → Serve pipeline. Neither mentions "packages," "Knowledge Base," or "Knowledge Leverage," the terminology introduced in docs/product/overview.md.
- Why deferred: cross-doc consistency gap, not a naming conflict to resolve via glossary nor a design smell. Flagged for a future top-level-docs refresh packet.

## 2026-07-07 — Dangling SPECIFICATION.md reference
- Raised during: OP-003
- What: specification.md's project-structure tree (line 37) lists a root SPECIFICATION.md file annotated "(deprecated — content moved here)." That file does not exist anywhere in the repo — a leftover from a past migration.
- Why deferred: trivial cleanup, not worth an ADR. Flagged for a later cleanup pass.

## 2026-07-07 — roadmap.md drops the Phase 3 label for Federation MCP Tools
- Raised during: OP-003
- What: roadmap.md's "Current State" table marks Federation (Phase 1–2) done and lists MCP Tools as a capability, but drops the "Phase 3" label that federation.md uses when marking that same capability Complete.
- Why deferred: small labeling gap, consistent with the broader phase-numbering pattern (see ADR-0003) but not itself worth a separate ADR. Flagged for a later cleanup pass.
