# ADR-0011: Does the advisory pipeline need its removed stages back now that Package 3 is active?

- **Status:** accepted
- **Type:** design
- **Decision:** Not yet decided — this ADR exists to pose the question and
  its options, per OP-008a's escalation path. No pipeline or code change is
  made by this ADR.
- **Why:**
  - `docs/architecture/decisions.md`'s 2026-04 Product Checkpoint entry
    "Simplify advisory pipeline from 13 stages to 7" gives this rationale:
    "Niche classification, quality prediction, escalation check, and signal
    emission are premature without Package 3." That entry's precondition —
    Package 3 being absent/deferred — was true at the time it was written.
  - ADR-0002's 2026-07-07 resolution reverses that precondition: Package 3
    (Evolution Engine) is now an active package at full parity with Package 1
    and Package 2, not deferred research.
  - ADR-0002 explicitly scopes its own reconciliation work to documentation
    (`docs/product/overview.md`, `docs/science/evolution-vision.md`,
    `docs/architecture/decisions.md`, via packet OP-008a) and does not decide
    whether the 7-stage advisory pipeline itself needs to change. OP-008a is
    docs-only (no `src/` access) and cannot decide this either.
  - It is genuinely unclear whether the current pipeline is an adequate
    substrate for Package 3 as designed in `docs/science/evolution-vision.md`
    (the three-signal framework, prediction-error engine, adapter evolution).
    The 2026-07-03 Advisory Pipeline MVP Implementation entries in
    `decisions.md` already added an `advisory_interactions` audit table
    (query, domain, stakes, model, cited fact IDs + confidences, confidence
    band, knowledge gaps, raw output) — this may already capture what
    Package 3's P3-1 trajectory logging needs, making some or all of the four
    removed stages unnecessary. Or it may not be enough, and niche
    classification / quality prediction / escalation check / signal emission
    may need to return in some form. Nobody has evaluated this yet.
- **Options for Mikael:**
  - **Reintroduce the four removed stages** (niche classification, quality
    prediction, escalation check, signal emission) now that Package 3 is
    active, restoring some or all of the 13-stage pipeline.
  - **Keep the 7-stage pipeline as-is.** Treat the existing
    `advisory_interactions` audit table plus out-of-pipeline batch processing
    as sufficient for Package 3's needs; revisit only if a concrete P3 phase
    (e.g. P3-1 trajectory logging, P3-2 prediction model) proves the current
    instrumentation insufficient.
  - **Defer the decision to implementation time.** Treat this as an
    open design question (in the spirit of `decisions.md`'s Open Design
    Questions section) rather than something to resolve now, and revisit when
    Package 3's first implementation packet (P3-1) is actually sized.
- **Rejected alternatives:**
  - Reintroduce the four removed stages now — rejected: unverified need.
    Nobody has evaluated whether the `advisory_interactions` audit table
    already covers what those stages provided; reintroducing speculatively
    would repeat the over-engineering the 2026-04 simplification was meant
    to fix.
  - Defer as an open design question in `decisions.md` — superseded: a
    concrete decision with an explicit revisit trigger is more actionable
    than leaving it open-ended indefinitely.
- **Consequences:** If "reintroduce stages" is chosen, this spawns a
  code-capable packet (`src/Deke.Api`'s advisory pipeline) to add the stages
  back, scoped separately from OP-008a. If "keep as-is" or "defer" is chosen,
  no immediate packet is needed beyond noting the resolution in
  `decisions.md`. Routed through the standard adjudication path (OP-006/007
  pattern, or ad-hoc direct adjudication as recent ADRs in this series have
  used).
- **Resolution (2026-07-14, Mikael, direct adjudication — ad-hoc, ahead of
  OP-006/OP-007):** Keep the 7-stage pipeline as-is. The existing
  `advisory_interactions` audit table plus out-of-pipeline batch processing
  is treated as sufficient for Package 3's current needs. No packet is
  spawned by this ADR. Revisit trigger: when P3-1 (trajectory logging) is
  actually sized and its concrete instrumentation needs are known — if
  P3-1's needs exceed what `advisory_interactions` captures, reopen this
  question with a scoped code packet at that time.
