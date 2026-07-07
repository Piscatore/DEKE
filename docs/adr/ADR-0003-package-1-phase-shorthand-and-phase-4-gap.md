# ADR-0003: Canonicalize Package 1's phase shorthand and resolve the Phase 4 gap

- **Status:** proposed
- **Type:** naming
- **Decision:** Adopt "P1-N" as the canonical shorthand for Package 1 phases
  (glossary row proposed in `docs/GLOSSARY.md`), matching the pattern already
  used cleanly by Package 2's `P2-N`. Additionally flag that Phase 4 is
  referenced nowhere — this ADR bundles that gap in as the same
  Package-1-phase-table topic, for Mikael to resolve.
- **Why:**
  - Package 1 phases are written four different ways across the docs, some
    in the same file: "Package 1 Phase 1" (`decisions.md:27`), "P1-Phase1"
    (`specification.md:378`), "P1-Phase 2" (`decisions.md:260`, `276`), and
    "P1-3" (`decisions.md:165`).
  - Federation's `Phase 1–5` (`federation.md`, one canonical table) and
    Package 2's `P2-N` (`roadmap.md`, `decisions.md`) are already internally
    consistent and don't need a glossary row — only Package 1's shorthand is
    conflicting.
  - Package 1 phases have no canonical table, unlike Federation: Phase 1 =
    provenance schema (`specification.md:202`, `decisions.md:27`), Phase 2 =
    quality pipeline (`decisions.md:310`), Phase 3 = terminology database
    (`retrieval-pipeline.md:144`), Phase 5 = multilingual model swap
    (`retrieval-pipeline.md:190`, "Phase 5 of Package 1"). Phase 4 is never
    defined anywhere in the repo.
- **Rejected alternatives:**
  - Leave the shorthand as-is and rely on context — rejected: already causing
    inconsistency within single files, not just across documents.
  - Have this packet invent a Phase 4 definition unilaterally — rejected:
    defining what Phase 4 *is* would be a design decision, not a naming fix;
    it must go to Mikael, not be assumed by an ingestion packet.
- **Consequences:** Feeds the OP-006 interview packet as two closed
  questions: confirm "P1-N" as the canonical shorthand, and define what
  Package 1 Phase 4 is (or confirm it is intentionally reserved/absent). If
  accepted, spawns an OP-009 spec-refactor packet to normalize the shorthand
  across `specification.md` and `decisions.md`.
