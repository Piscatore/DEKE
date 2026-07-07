# ADR-0010: Vectorless RAG + hybrid retrieval — promote from ad-hoc note to a real roadmap item

- **Status:** accepted
- **Type:** design
- **Decision:** Promote `thoughts/adhoqnotes.md`'s one-line idea — "Investigate
  and add vectorless rag and a very intelligent hybrid model that uses the
  best from both worlds" — from an unexplained ad-hoc note to a real,
  future-roadmap research direction. No design is decided here; this ADR
  only decides that the idea is worth a sized slot on the permanent roadmap
  rather than staying an orphaned one-liner.
- **Why:**
  - The note carried zero surrounding context — no motivating problem, no
    prior discussion, no connection to any other distilled intention in
    `docs/INTENT.md` (OP-005d flagged it entirely `(inferred, unconfirmed)`
    for exactly this reason).
  - Asked directly (ad-hoc adjudication, same pattern as ADR-0002..0009):
    Mikael confirmed the idea should become a real packet rather than stay
    parked indefinitely or be dropped.
  - The idea is genuinely unresearched — unlike OP-008a-e's design ADRs
    (which fix an already-understood gap), there is no existing code,
    pattern, or prior art in this repo to point a code-capable packet at
    yet. It needs a research pass before it can become a sized design/build
    packet.
- **Options for Mikael:** (posed via `AskUserQuestion`, 2026-07-07)
  - Still interesting, park it — keep as a `PARKING-LOT.md` entry, no packet.
  - **Promote to a real packet** — size it as an actual future `OP-0xx`
    research/design packet on the roadmap. **← chosen.**
  - Drop it — no longer relevant, remove from active consideration.
- **Rejected alternatives:**
  - Park indefinitely — rejected; Mikael wants it actively tracked as
    roadmap work, not left as a note that could be lost.
  - Drop — rejected; the idea is still of interest.
- **Consequences:** Does **not** spawn an immediate `OP-008` packet (that
  series is for already-decided redesigns with a clear code target; this
  isn't one). Instead, it becomes a roadmap entry to be sized during
  **OP-011** (Roadmap rebuild) — likely as a research packet first
  (survey what "vectorless RAG" and a hybrid vector/non-vector retrieval
  approach would mean for DEKE's existing pgvector-based search, per
  `docs/PROJECT-MAP.md`'s Fact & Source Domain / Search & Trust Contracts
  entries), followed by a design packet once that research resolves what
  "hybrid" should mean concretely. `docs/INTENT.md`'s adhoqnotes.md section
  (OP-005d) is updated to reflect this resolution rather than left as a bare
  unconfirmed flag.
- **Resolution (2026-07-07, Mikael, direct adjudication via `AskUserQuestion`
  during OP-006):** Accepted as "promote to a real packet." Sizing deferred
  to OP-011's roadmap rebuild — no packet number assigned yet.
