# ADR-0004: LLM backend documentation doesn't match implementation

- **Status:** accepted
- **Type:** design
- **Decision:** Not decided here — this ADR records a real doc-vs-code drift
  for Mikael to adjudicate. No fix is chosen or implied by this packet.
- **Why:**
  - `specification.md`'s tech stack table documents `Anthropic API (via
    IChatClient)` — `claude-haiku-4-5, claude-sonnet-5` — as the LLM backend,
    and `Ollama (via IChatClient)` as the local option (lines 24–25).
  - `specification.md`'s "LLM Selection Policy" table (lines 568–576) further
    specifies `claude-haiku-4-5` as the default backend, `claude-sonnet-5`
    for low-confidence/high-stakes cases, and Ollama for domains that allow
    local models — all served through a single keyed `IChatClient`
    (`anthropic`).
  - `docs/product/knowledge-leverage.md`'s "LLM Backend Selection" section
    (lines 82–86) describes the same Anthropic-default / Ollama-fallback
    policy in product terms.
  - The actual code, `src/Deke.Infrastructure/Llm/LlmConfig.cs`, defines
    `LlmProvider { None, Gemini, OpenAi }`. There is no Anthropic or Ollama
    implementation anywhere in `src/`. The documented default backend does
    not exist in the codebase at all — this is the most significant of the
    nine naming/framing findings.
- **Rejected alternatives:** (presented as options for Mikael, none chosen)
  - Update the docs to match the code (document Gemini/OpenAI/NoOp as the
    real backends) — plausible, but discards the documented Anthropic/Ollama
    selection policy without a decision on whether that policy is still
    wanted.
  - Implement the missing Anthropic/Ollama backends to match the docs —
    plausible, but a nontrivial feature-build, not a doc fix.
  - Hybrid: keep Anthropic/Ollama as the documented target architecture and
    add a status note that current code implements Gemini/OpenAI as an
    interim/dev-mode default — avoids relitigating the target design but
    leaves the drift formally acknowledged rather than resolved.
- **Consequences:** Feeds the OP-006 interview packet. Whichever option
  Mikael picks becomes an OP-008 design packet (if code changes) or is
  scoped directly into an OP-009 spec-refactor packet (if only docs change).
- **Resolution (2026-07-07, Mikael, direct adjudication — ad-hoc, ahead of
  OP-006/OP-007):** Implement the missing backends. The documented
  Anthropic/Ollama `IChatClient` backends are the target architecture; the
  code needs to catch up to the docs, not the other way around. This is real,
  nontrivial feature work — new backend implementations in
  `src/Deke.Infrastructure/Llm/` — not a doc fix, and is scoped to packet
  OP-008b, which requires a code-capable agent/session.
