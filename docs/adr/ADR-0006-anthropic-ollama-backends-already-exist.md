# ADR-0006: Anthropic/Ollama backends already exist — ADR-0004's premise was wrong

- **Status:** accepted
- **Type:** design
- **Decision:** Confirmed — Advisory pipeline's keyed `IChatClient` backends
  (`Anthropic`, `Ollama`) are the real, already-built implementation
  `specification.md` documents. ADR-0004's premise was wrong.
- **Why:**
  - ADR-0004 (accepted, resolved 2026-07-07) states "There is no Anthropic or
    Ollama implementation anywhere in `src/`" and its resolution scoped
    OP-008b to "implement the missing backends." This claim is false.
  - `src/Deke.Infrastructure/Advisory/ChatClientRegistration.cs` registers
    keyed `IChatClient` backends for **both**: Anthropic via
    `new AnthropicClient(config.AnthropicApiKey).Messages` (`Anthropic.SDK`
    package) and Ollama via `new OllamaApiClient(new Uri(config.OllamaBaseUrl))`
    (`OllamaSharp` package) — both packages are already referenced in
    `Deke.Infrastructure.csproj`.
  - `Advisory/AdvisoryConfig.cs` + `Advisory/LlmSelectionPolicy.cs` implement
    the exact routing policy `specification.md` documents: `claude-haiku-4-5`
    default, `claude-sonnet-5` escalation on low-confidence/high-stakes,
    Ollama for domains with `AllowLocalModel` above a depth threshold — model
    IDs match the spec verbatim.
  - `Advisory/AdvisoryPipeline.cs.AdviseAsync()` actually calls these clients
    (`_serviceProvider.GetRequiredKeyedService<IChatClient>(selection.ClientKey)`)
    — live, wired code, not a stub.
  - What ADR-0004 actually found is real, but different: `Llm/LlmConfig.cs`
    defines a **second, independent** `ILlmService` abstraction
    (`LlmProvider { None, Gemini, OpenAi }`, `GeminiLlmService`,
    `OpenAiLlmService`, `NoOpLlmService`) consumed only by
    `Deke.Worker/Services/PatternDiscoveryService.cs` (the background
    pattern-discovery learning cycle) — entirely separate from the Advisory
    pipeline's `IChatClient` system. OP-003's naming audit read only this
    file and concluded the documented backend didn't exist, without having
    scoped `Deke.Infrastructure/Advisory/` at all (that sweep is OP-004b,
    this packet).
- **Rejected alternatives:**
  - Re-scope OP-008b to docs-only instead of cancelling — rejected, Mikael
    chose outright cancellation (nothing left to build or document as a
    gap; the "two systems" fact is captured by this ADR and ADR-0007, not
    worth a separate spec-refactor packet).
  - Leave the `Llm/` (Gemini/OpenAI) system's undocumented status alone —
    rejected; Mikael chose to open a design question rather than accept the
    split as self-evidently intentional or merge it immediately.
- **Consequences:** Supersedes ADR-0004's *resolution* only (the doc-vs-code
  drift finding itself still stands — it was just misdiagnosed which side was
  missing).
  - **OP-008b is cancelled** — see its packet file for the cancellation note.
  - The `Llm/` (Gemini/OpenAI) system's undocumented status is escalated
    separately as **ADR-0007 (proposed)** — not decided here.
- **Resolution (2026-07-07, Mikael, direct adjudication — ad-hoc, ahead of
  OP-006/OP-007):** Accepted as written. OP-008b cancelled. ADR-0007 opened
  for the `Llm/` system question, left proposed pending further adjudication.
