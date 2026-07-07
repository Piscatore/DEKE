# ADR-0009: `list_available_domains` MCP tool misses domains created without a registered Source

- **Status:** accepted
- **Type:** design
- **Decision:** Merge fact-derived domains in. Change `ListAvailableDomains`
  in `src/Deke.Mcp/Tools/SearchTools.cs` to union `ISourceRepository`-derived
  domains with `IFactRepository.GetDomainStatsAsync()` results, so fact-only
  domains appear too (with fact count, no source count), mirroring what
  `Deke.Api`'s Federation manifest endpoint already does correctly.
- **Why:**
  - `src/Deke.Mcp/Tools/SearchTools.cs`'s `ListAvailableDomains` (MCP tool
    `list_available_domains`) builds its "Local Domains" list exclusively
    from `ISourceRepository.GetAllAsync()` → `Select(s => s.Domain)`. It
    never calls `IFactRepository` at all.
  - Top-level `CLAUDE.md`'s own documented workflow states: "Add a new
    domain: Just start adding facts with the domain name — no
    pre-configuration needed." The MCP tool that implements exactly this,
    `FactTools.AddFact` (`add_fact`, same `Tools/` directory), stores a
    `Fact` with an arbitrary `domain` string and no `SourceId` — confirmed
    nullable via this same file's sibling `FactTools.GetFact`'s
    `fact.SourceId.HasValue` check.
  - Net effect: a domain created via the documented zero-config path (facts
    only, no `Source` ever registered) is invisible to
    `list_available_domains` — an MCP client (e.g. Claude Code) has no way
    to discover the domain exists unless it already knows the domain name
    from elsewhere.
  - The fix ingredient already exists: `IFactRepository.GetDomainStatsAsync()`
    returns `List<DomainStats>` (domain name + fact count + last-updated)
    precisely for this purpose, and `Deke.Api`'s Federation manifest endpoint
    (`GET /api/federation/manifest`, see `docs/PROJECT-MAP.md`'s Federation
    Endpoints entry) already calls it correctly. `Deke.Mcp`'s
    `ListAvailableDomains` duplicates domain-listing logic instead of
    reusing it, and the duplicate is incomplete.
  - Not a naming/glossary issue — `docs/GLOSSARY.md`'s three terms (Evolution
    Engine, P1-N, IChunker/SemanticChunkerAdapter) don't appear anywhere in
    `src/Deke.Mcp`. This is a behavior gap between a documented workflow and
    its MCP-surfaced discoverability.
- **Options for Mikael:**
  - **Merge fact-derived domains in.** Change `ListAvailableDomains` to
    union `ISourceRepository`-derived domains with
    `IFactRepository.GetDomainStatsAsync()` results, so fact-only domains
    appear (with fact count, no source count). Small, local code fix —
    mirrors what the Federation manifest endpoint already does.
  - **Narrow the documented workflow instead.** Decide "no pre-configuration
    needed" was never meant to imply MCP-tool discoverability — a caller who
    adds facts under a new domain name is expected to already know that
    name. Update top-level `CLAUDE.md`'s wording (and/or
    `list_available_domains`'s tool description) to clarify the tool lists
    *registered-source* domains only, not "all available domains." No
    behavior change.
  - **Leave as-is, no doc change.** Decide the gap is inconsequential in
    practice (e.g. domains are always expected to have a source in normal
    use, or fact-only domains are a corner case not worth solving) and close
    this ADR with no further packet.
- **Rejected alternatives:**
  - Narrow the documented workflow instead — rejected; Mikael chose to close
    the discoverability gap in code rather than walk back top-level
    `CLAUDE.md`'s "no pre-configuration needed" promise.
  - Leave as-is, no doc change — rejected; the gap is real, cheap to fix, and
    not a corner case worth leaving unresolved.
- **Consequences:** spawns **OP-008e** (merge
  `IFactRepository.GetDomainStatsAsync()` results into `ListAvailableDomains`)
  — a small code-capable packet, `Deke.Mcp`-scoped, similar sizing to
  OP-008d, per the standard ADR→packet flow in `CHARTER.md`.
- **Resolution (2026-07-07, Mikael, direct adjudication):** Accepted as
  "merge fact-derived domains into `list_available_domains`." OP-008e
  drafted to implement it.
