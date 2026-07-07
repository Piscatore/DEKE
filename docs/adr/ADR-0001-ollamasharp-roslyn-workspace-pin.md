# ADR-0001: Fix the OllamaSharp exact-version pin blocking Roslyn MCP workspace loads

- **Status:** accepted, resolved
- **Type:** tooling
- **Decision:** Replace the exact `Version="5.3.0"` pin on `OllamaSharp` in `src/Deke.Infrastructure/Deke.Infrastructure.csproj` with `5.3.1` (the version NuGet actually resolves and has been building against all along), after confirming it keeps the same `Microsoft.Extensions.AI` floor documented in the 2026-07-07 decision. Separately, adopt Central Package Management (`Directory.Packages.props`) repo-wide, as `nuget` MCP's `review_supply_chain_security` independently recommends.
- **Why:**
  - `dotnet build DEKE.sln` succeeds today (0 errors, 15 warnings) despite the NU1603 warning — the pin is not a real build break.
  - `mcp__roslyn__search_symbols` hard-fails (workspace load abort) on that same warning, blocking every future mapping/spec packet that needs symbol-level Roslyn tooling (OP-004 mapping sweeps depend on this). (Correction: the original audit also logged `cwm-roslyn-navigator`'s `find_symbol` as failing for the same reason — that was a testing mistake, an invalid tool call with the wrong parameter name, not a real NU1603 block; it worked correctly both before and after this fix once called with valid parameters.)
  - `~/.nuget/packages/ollamasharp/` only has `5.3.1` and `5.4.25` cached locally — `5.3.0` itself isn't available, so the "pin" was already a fiction; NuGet has silently floated to 5.3.1 on every restore.
  - `5.3.1` is pre-5.4.x, so per the 2026-07-07 decision's own logic ("OllamaSharp 5.4.x floors at M.E.AI 10.4.1 ... downgraded to 5.3.0, whose floor is <= 10.3.0") it should carry the same M.E.AI floor as 5.3.0 — this needs a one-line confirmation, not a re-litigation of the pin decision.
  - `nuget` MCP's `review_supply_chain_security` independently flagged the repo for lacking Central Package Management, which would have caught this exact kind of pin/resolution drift structurally.
- **Rejected alternatives:**
  - Leave as-is — rejected: silently blocks two of the three C#-analysis MCP tools for the rest of the Overhaul and the permanent roadmap.
  - Force-restore exact 5.3.0 from nuget.org — rejected: unclear if 5.3.0 is still listed upstream; 5.3.1 is already proven to resolve and run correctly.
- **Consequences:** spawns a small tooling packet (not sized here) to: (1) bump the `OllamaSharp` csproj reference to `5.3.1` and confirm `Microsoft.Extensions.AI` compatibility still holds at 10.3.0, (2) optionally add `Directory.Packages.props` for Central Package Management repo-wide, (3) re-run `docs/TOOLING.md` verification-suite item 6 to confirm `roslyn`/`cwm-roslyn-navigator` symbol search now PASS.
- **Resolution (2026-07-07):** Applied. Bumped `OllamaSharp` to `5.3.1` in `src/Deke.Infrastructure/Deke.Infrastructure.csproj`. `dotnet build DEKE.sln` now has zero OllamaSharp-related warnings. `mcp__roslyn__search_symbols` on DEKE.sln confirmed working post-fix. Central Package Management (`Directory.Packages.props`) was NOT done as part of this resolution — left for a future tooling packet if wanted.
