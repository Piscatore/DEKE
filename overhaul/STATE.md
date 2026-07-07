# STATE.md — Overhaul Baton

> Read this first, every session. Append a new entry (newest on top) when
> you finish a packet. Keep entries ≤10 lines. This file plus the artifacts
> in `/docs` are the only cross-session memory — don't assume anything else
> carries over between sessions.

---

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
