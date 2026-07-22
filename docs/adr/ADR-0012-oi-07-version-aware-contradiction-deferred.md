# ADR-0012: OI-07 (version-aware contradiction resolution) deferred beyond P1-2

- **Status:** accepted
- **Type:** design
- **Decision:** Defer version-aware contradiction resolution beyond P1-2.
  `ContradictionDetectionService` ships only a basic, non-version-aware
  embedding-similarity heuristic (candidate band 0.75-0.90, kept below L5
  dedup's 0.92 threshold); the version tagging schema
  (`applies_to_version`, `deprecated_after_version`), version-scoped
  detection, and query-time version resolver described in OI-07 are not
  built.
- **Why:**
  - OI-07 (`decisions.md`) already framed this precisely: "requires version
    tagging schema (P1-1). Detection logic can be added in P1-2," with
    revisit trigger "...or P1-2 is being implemented" — that trigger is
    this packet.
  - P1-1 shipped schema-only (per its 2026-07-21 `decisions.md` entry) and
    did not include a version-tagging schema — `applies_to_version`/
    `deprecated_after_version` do not exist on `facts`. Building
    version-scoped detection now would require adding that schema
    mid-packet, which is schema design work beyond P1-2's scope as sized in
    [ROADMAP.md](../ROADMAP.md) ("review queue, temporal-validity handling,
    contradiction detection").
  - The Self-Evolving AI Agents Survey (arXiv:2508.07407), cited in OI-07,
    treats version-aware contradiction resolution without ground truth as
    an open research problem, not an implementation detail — not something
    to build speculatively inside a quality-pipeline activation packet.
  - The basic heuristic's similarity band (0.75 lower bound, 0.90 upper
    bound) is deliberately kept below L5 semantic dedup's 0.92 threshold so
    the two systems — deduplication and contradiction detection — never
    compete for the same fact pair.
- **Rejected alternatives:**
  - Build the full version tagging schema and version-scoped detection now,
    since the packet was already touching trust/contradiction code —
    rejected: this is unsized schema-design work (a P1-1-shaped change)
    folded into a P1-2 activation packet; OI-07 explicitly separates schema
    (P1-1) from detection logic (P1-2), and the schema half was never done.
  - Ship contradiction detection with no similarity band at all (flag every
    candidate above L5's dedup threshold) — rejected: collapses into
    deduplication's territory and would re-flag the exact near-duplicates
    L4/L5 already resolve, rather than the semantically-similar-but-
    conflicting facts contradiction detection is meant to catch.
  - Skip contradiction detection entirely until version-awareness is
    designed — rejected: leaves P1-2's `Contested` trust state and the
    review queue with no producer. The basic heuristic delivers real value
    now (catching same-version contradictions) even though it cannot yet
    distinguish version-progressive facts from genuine conflicts.
- **Consequences:** OI-07 remains open in `decisions.md`'s Open Design
  Questions, with its revisit trigger narrowed going forward to: an
  observed false-positive contradiction rate on version-progressive facts
  in practice (e.g. a .NET 6 fact wrongly flagged against a .NET 9 fact),
  or a future packet explicitly sized to build the version-tagging schema.
  No packet is spawned by this ADR.
- **Resolution (2026-07-22, Mikael, direct implementation-time scope
  decision, P1-2):** Deferred. The basic similarity-band heuristic ships in
  P1-2; version-aware resolution is not built. Tracked as OI-07's
  continuing disposition in `decisions.md`.
