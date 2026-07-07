# Glossary

Canonical ontology for DEKE naming — one row per *concept*, not per name.
Produced by the `/overhaul` subproject (see `/overhaul/OVERHAUL-SKETCH-v0.2.md`
§3.2) from a naming-and-framing audit of DEKE's docs and code
(`thoughts/shared/namingissues.md`).

## Status legend

- **PROPOSED** — agent suggestion, not yet reviewed.
- **APPROVED** / **REJECTED** — Mikael has adjudicated.
- **ENFORCED** — an approved term backed by an active CI lint rule.

A concept that resists clean naming because the underlying design itself is
unsettled is not resolved here — it is escalated as a proposed ADR instead
(see `docs/adr/`).

## Terms

| Canonical term | Definition (≤2 sentences) | Deprecated aliases | Decided in | Status |
|---|---|---|---|---|
| Evolution Engine | The cross-cutting subsystem that learns which facts, adapter configurations, and knowledge gaps improve advisory quality over time, using a three-signal prediction-error framework. Promoted to full parity with Package 1 and Package 2 under DEKE's Three-Package Architecture — an active package, not deferred research. | Package 3, P3 | ADR-0002 | APPROVED |
| P1-N | Canonical shorthand for phase numbers within Package 1 (Knowledge Base), mirroring the `P2-N` pattern already used consistently for Package 2. Phases 1 through 4 are now fully defined (Phase 4 = multilingual model swap, renumbered from the former Phase 5); no gap remains. | Package 1 Phase N, P1-PhaseN | ADR-0003 | APPROVED |
| IChunker / SemanticChunkerAdapter | The interface and adapter that chunk extracted content into semantically coherent pieces before embedding; registered in DI (`ServiceCollectionExtensions.cs:66`) and consumed by two Worker services. This is the real, already-implemented code — distinct from the names `retrieval-pipeline.md` documents as not-yet-built. | IChunkingService, SemanticChunkingService | Mikael, 2026-07-07 (direct adjudication, no ADR) | APPROVED |
