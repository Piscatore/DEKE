# ADR-0005: Federation ranking formula — documentation doesn't match implementation

- **Status:** accepted
- **Type:** design
- **Decision:** Not decided here — this ADR records a real doc-vs-code drift
  for Mikael to adjudicate. No fix is chosen or implied by this packet.
- **Why:**
  - `federation.md`'s "Result Ranking" section (lines 231–248) documents
    `final_score = similarity * locality_weight`, with a worked example using
    only those two factors.
  - The actual implementation, `TrustScoringService.Score()`
    (`src/Deke.Infrastructure/Trust/TrustScoringService.cs`), used directly
    by `FederatedSearchService`, computes
    `similarity * confidence * credibility * recencyDecay * localityWeight`
    — three additional factors (confidence, source credibility, recency
    decay) that `federation.md` never mentions.
  - The doc's two-factor worked example (0.80 local vs. 0.85 peer) would
    produce a different ranking than the real five-factor formula once
    confidence, credibility, and recency are non-neutral — the documented
    behavior is not just incomplete, it can be numerically wrong for readers
    relying on it.
- **Rejected alternatives:** (presented as options for Mikael, none chosen)
  - Update `federation.md` to document the actual five-factor formula —
    plausible, most direct fix, but should be confirmed rather than assumed
    since the doc's simplicity may have been a deliberate public-facing
    simplification.
  - Simplify `TrustScoringService` to match the documented two-factor formula
    — plausible, but discards confidence/credibility/recency weighting
    without knowing whether that was intentional or an oversight.
  - Treat the doc's formula as a deliberately simplified illustrative example
    and add a note pointing to the real implementation for full detail —
    avoids a code change but leaves the "why simplified" question unanswered.
- **Consequences:** Feeds the OP-006 interview packet. Whichever option
  Mikael picks becomes an OP-009 spec-refactor packet (if only docs change)
  or an OP-008 design packet (if the scoring logic itself changes).
- **Resolution (2026-07-07, Mikael, direct adjudication — ad-hoc, ahead of
  OP-006/OP-007):** Update the doc to the real formula. `federation.md`'s
  "Result Ranking" section is to be rewritten to document the actual
  `similarity * confidence * credibility * recencyDecay * localityWeight`
  formula from `TrustScoringService.Score()`, replacing the stale two-factor
  `similarity * locality_weight` example. This is a pure doc fix, scoped to
  packet OP-009b — the scoring logic itself is not changing.
