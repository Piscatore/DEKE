# ADR-0007: Gemini/OpenAI backend (Llm/) — intentional second system or drift?

- **Status:** accepted
- **Type:** design
- **Decision:** Merge onto the Advisory pipeline's `IChatClient` backend.
  Retire `Llm/` (`LlmProvider`, `GeminiLlmService`, `OpenAiLlmService`,
  `NoOpLlmService`, `ILlmService`); switch `PatternDiscoveryService` to a
  keyed `IChatClient` via `LlmSelectionPolicy`-style routing.
- **Why:**
  - `src/Deke.Infrastructure/Llm/` defines `LlmProvider { None, Gemini,
    OpenAi }`, `GeminiLlmService`, `OpenAiLlmService`, `NoOpLlmService` — a
    complete, working `ILlmService` implementation.
  - Its only consumer in the entire solution is
    `Deke.Worker/Services/PatternDiscoveryService.cs` (the background
    pattern-discovery learning cycle), confirmed via repo-wide grep.
  - This is architecturally independent from the Advisory pipeline's
    `IChatClient`-based Anthropic/Ollama system (`Advisory/
    ChatClientRegistration.cs`, `LlmSelectionPolicy.cs`) — two separate LLM
    call paths exist side by side.
  - Neither `docs/architecture/specification.md` nor
    `docs/product/knowledge-leverage.md` mentions Gemini or OpenAI anywhere.
    Only the Anthropic/Ollama system is documented as DEKE's LLM backend.
  - This is not necessarily a bug: background pattern-discovery (hourly
    batch cycle, one call per active domain) and interactive advisory
    (per-request, latency-sensitive) have different cost/latency profiles, so
    a cheaper/different provider for the former could be a deliberate,
    just-undocumented choice rather than drift.
- **Options for Mikael:**
  - **Keep both, document the split.** Add a short note to
    `specification.md`/`knowledge-leverage.md` that `Deke.Worker`'s
    pattern-discovery cycle intentionally uses a separate, cheaper
    Gemini/OpenAI backend, distinct from Advisory's Anthropic/Ollama. No code
    change — a docs-only OP-009-style packet.
  - **Merge onto the Advisory pipeline's `IChatClient` backend.** Retire
    `Llm/` (`LlmProvider`, `GeminiLlmService`, `OpenAiLlmService`,
    `NoOpLlmService`, `ILlmService`); switch `PatternDiscoveryService` to a
    keyed `IChatClient` (likely `AdvisoryClientKeys.Ollama` or a new
    cheap/local key) via `LlmSelectionPolicy`-style routing. Real code
    change, a sized packet of its own.
  - **Keep both, no doc change.** Decide the split doesn't need documenting
    (e.g. it's obvious enough, or subject to change soon) — closes this ADR
    with no further packet.
- **Rejected alternatives:**
  - Keep both, document the split — rejected; Mikael chose consolidation
    over documenting two parallel LLM systems.
  - Keep both, no doc change — rejected for the same reason.
- **Consequences:** spawns **OP-008c** (retire `Llm/`, switch
  `PatternDiscoveryService` to a keyed `IChatClient`) — a code-capable
  packet, per the standard ADR→packet flow in `CHARTER.md`.
- **Resolution (2026-07-07, Mikael, direct adjudication):** Accepted as
  "merge onto Advisory's `IChatClient`." OP-008c drafted to implement it.
