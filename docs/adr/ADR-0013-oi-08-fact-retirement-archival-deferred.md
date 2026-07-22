# ADR-0013: OI-08 (fact retirement/archival) deferred beyond P1-2

- **Status:** accepted
- **Type:** design
- **Decision:** Defer fact retirement/archival beyond P1-2. This packet
  ships only the read-side half of OI-08's temporal-validity scope —
  `ITrustEvaluator.Evaluate` flags facts missing `ValidFrom` when a
  domain's policy sets `TemporalValidityRequired` — and builds none of
  `retired_at`, `retired_reason`, a query-time "exclude retired" filter, an
  "include historical" flag, or a retention policy.
- **Why:**
  - OI-08 (`decisions.md`) already framed this precisely: "design needed:
    retired_at timestamp, retired_reason field, query-time filter excluding
    retired facts by default, explicit 'include historical' flag, and a
    retention policy," with revisit trigger "...or P1-2 (temporal validity)
    is being implemented" — that trigger is this packet.
  - P1-2's brief acceptance criterion was "facts filtered/flagged by
    valid_from/valid_until." Investigation during implementation found
    `ITrustScoringService.Score` (`src/Deke.Infrastructure/Trust/
    TrustScoringService.cs`) — a pre-existing, separate mechanism from the
    `trust_state` classifier this packet adds — already zeroes a fact's
    composite trust score when `now` falls outside `[validFrom,
    validUntil]`. It is wired into `FederatedSearchService.MergeResults`
    (federated result ranking) and `KnowledgeDepth.Compute` (advisory-
    pipeline model routing), so `valid_until` expiry already demotes
    affected facts out of practical relevance at query time. Only the
    `valid_from`-missing half of temporal validity had no producer —
    nothing set or checked it — which is the gap `ITrustEvaluator`'s
    `TemporalValidityRequired` gate closes.
  - Building `retired_at`/`retired_reason`/historical-inclusion machinery
    now would answer a broader question ("what happens to a fact after it
    is retired, and how do we preserve the historical record for
    pattern-evolution and legacy-system queries") that OI-08 explicitly
    scopes as its own structural feature, separate from the
    temporal-validity read-side check P1-2 was sized for.
  - No facts have been retired in practice yet (OI-08's own deferral
    rationale), so there is no live pressure to design the retention policy
    now.
- **Rejected alternatives:**
  - Build `retired_at`/`retired_reason` plus query-time filtering now,
    since the packet was already touching trust/temporal code — rejected:
    OI-08 is a structural feature (schema + filter semantics + retention
    policy design), not a small extension of this packet's scope;
    conflating it here risks an under-designed retirement model shipped as
    a side effect.
  - Treat "facts filtered/flagged by valid_from/valid_until" as requiring
    new `valid_until`-expiry code in `ITrustEvaluator` — rejected:
    `ITrustScoringService` already does this at search-ranking time;
    duplicating it in the `trust_state` classifier would create two
    independent, potentially divergent expiry mechanisms for the same two
    columns.
  - Leave `valid_from`-missing facts unflagged (do nothing for OI-08 in
    this packet) — rejected: the brief's acceptance criterion explicitly
    covers `valid_from`, and `TemporalValidityRequired` was cheap to add
    alongside the other policy gates.
- **Consequences:** OI-08 remains open in `decisions.md`'s Open Design
  Questions, with its revisit trigger narrowed going forward to: the first
  domain's facts beginning to approach their `valid_until` dates in
  practice. No packet is spawned by this ADR. Future readers should note
  the two distinct trust mechanisms this packet leaves in place —
  `ITrustScoringService` (numeric, search-ranking, handles `valid_until`)
  and `ITrustEvaluator` (categorical `trust_state`, handles missing
  `valid_from`) — are both intentional, not duplicative drift.
- **Resolution (2026-07-22, Mikael, direct implementation-time scope
  decision, P1-2):** Deferred. Temporal-validity read-side flagging
  (missing `valid_from`) ships in P1-2; retirement/archival machinery is
  not built. Tracked as OI-08's continuing disposition in `decisions.md`.
