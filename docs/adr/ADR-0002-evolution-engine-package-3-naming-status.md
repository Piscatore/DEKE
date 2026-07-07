# ADR-0002: Canonicalize "Evolution Engine" and adjudicate its product status

- **Status:** proposed
- **Type:** naming
- **Decision:** Adopt "Evolution Engine" as the canonical term for this
  subsystem, deprecating "Package 3" and "P3" (glossary row proposed in
  `docs/GLOSSARY.md`). This ADR additionally flags that the three source
  documents disagree not just on spelling but on the subsystem's actual
  product status — active package, deferred research, or not a product
  concept at all — which naming alone cannot settle.
- **Why:**
  - `docs/product/overview.md` defines a "Two-Package Architecture" (P1
    Knowledge Base, P2 Knowledge Leverage). Package 3 is never mentioned, and
    Federation is explicitly called cross-cutting rather than a package.
  - `docs/science/evolution-vision.md` calls the same subsystem the
    "Evolution Engine" and states outright: "It is not part of DEKE's active
    product model." It never uses the word "Package."
  - `docs/architecture/decisions.md` still refers to it as "Package 3"
    throughout (P3-1, P3-5, guardrails, GEPA mapping, review schedule) as if
    it were a numbered package on par with P1/P2 — including a 2026-04 entry
    that defers it "to research status," while other entries in the same
    document keep assigning it active phase numbers and design targets.
  - This is not a spelling disagreement: three documents disagree on whether
    the thing exists as a product concept at all, which is a bigger question
    than picking a name.
- **Rejected alternatives:**
  - Keep "Package 3" as canonical — rejected: implies numbered-package parity
    with P1/P2 that `overview.md` explicitly does not grant it.
  - Pick "Evolution Engine" as canonical and stop there — rejected: a naming
    fix alone doesn't resolve whether `decisions.md`'s ongoing P3-* entries
    should still be tracked as live design targets or archived as research
    history.
- **Consequences:** Feeds the OP-006 interview packet as a closed question:
  is the Evolution Engine active, deferred, or out of scope, and should
  `decisions.md`'s P3-* entries be re-labeled accordingly? If accepted, may
  spawn an OP-009 spec-refactor packet to reconcile `decisions.md`'s status
  framing with `evolution-vision.md`.
