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
| Evolution Engine | The cross-cutting subsystem that learns which facts, adapter configurations, and knowledge gaps improve advisory quality over time, using a three-signal prediction-error framework. Deferred to research status (`docs/architecture/decisions.md`, 2026-04 entry) and explicitly described as not part of DEKE's active product model. | Package 3, P3 | — | PROPOSED |
| P1-N | Canonical shorthand for phase numbers within Package 1 (Knowledge Base), mirroring the `P2-N` pattern already used consistently for Package 2. Only phases 1, 2, 3, and 5 are documented anywhere; Phase 4 is currently undefined. | Package 1 Phase N, P1-PhaseN | — | PROPOSED |
| IChunker / SemanticChunkerAdapter | The interface and adapter that chunk extracted content into semantically coherent pieces before embedding; registered in DI (`ServiceCollectionExtensions.cs:66`) and consumed by two Worker services. This is the real, already-implemented code — distinct from the names `retrieval-pipeline.md` documents as not-yet-built. | IChunkingService, SemanticChunkingService | — | PROPOSED |

## Related escalations

Two of the three rows above surface an open naming *and* status question that
this glossary cannot resolve alone; both are escalated as proposed ADRs:

- **Evolution Engine / Package 3** — naming pick plus a product-status
  disagreement across docs. See [ADR-0002](adr/ADR-0002-evolution-engine-package-3-naming-status.md).
- **P1-N shorthand** — naming pick plus an undefined Phase 4. See
  [ADR-0003](adr/ADR-0003-package-1-phase-shorthand-and-phase-4-gap.md).
