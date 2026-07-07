# ADR-0008: ApiKeyAuthHandler's "unconfigured = allow all" comment doesn't match its actual behavior

- **Status:** accepted
- **Type:** design
- **Decision:** Require key everywhere, fail-fast variant. `Deke.Api` refuses
  to start (or hard-fails early) unless `"ApiKey"` is configured, in every
  environment including local dev — no more silent "allow all" branch.
- **Why:**
  - `src/Deke.Api/Auth/ApiKeyAuthHandler.cs`'s `HandleAuthenticateAsync`:
    when configuration key `"ApiKey"` is empty, it returns
    `AuthenticateResult.NoResult()` behind the comment `// No API key
    configured — allow all requests (development mode)`.
  - `NoResult()` does not authenticate the request. It leaves
    `HttpContext.User` unauthenticated, so ASP.NET Core's
    `AuthenticatedOnly` fallback policy (`RequireAuthenticatedUser`) — and
    every endpoint's own `.RequireAuthorization()` call, which uses the same
    default policy — rejects the request. The actual effect is the opposite
    of the comment: write endpoints become *unusable*, not open.
  - `src/Deke.Api/appsettings.json` ships `"ApiKey": ""` by default, so this
    is the out-of-the-box behavior on a fresh clone, not an edge case.
  - Top-level `CLAUDE.md`'s own documented examples (`POST
    http://localhost:5000/api/sources`) show no `X-Api-Key` header, implying
    the author expected these calls to work by default — consistent with
    the comment's stated intent, not the code's actual behavior.
  - `specification.md:655` ("Write endpoints require `X-Api-Key` header.
    Read endpoints and the manifest endpoint are open.") doesn't address the
    unconfigured-key case either way, so this isn't a spec/code disagreement
    — it's the code disagreeing with its own inline comment.
- **Options for Mikael:**
  - **Make the comment true.** When `"ApiKey"` is unconfigured, synthesize a
    successful `AuthenticationTicket` (mirroring the configured-key success
    branch) instead of `NoResult()`, so write endpoints genuinely work
    without a key in dev. Small, local code fix.
  - **Make the code match a stricter reading.** Drop the "allow all" idea
    entirely — require `"ApiKey"` to be configured in every environment
    (fail startup if missing, or leave `NoResult()` as-is and fix the
    comment/docs instead to say write endpoints are unusable until an API
    key is set). Changes the documented quick-start experience.
  - **Leave as-is, comment only.** Decide the current behavior (locked out
    by default) is actually fine/intentional and just delete or correct the
    misleading comment. No behavior change.
- **Rejected alternatives:**
  - Make the comment true (synthesize a success ticket when unconfigured) —
    rejected; Mikael chose not to have write endpoints ever silently open by
    default, in any environment.
  - Leave behavior, fix comment only — rejected; leaves write endpoints
    permanently unusable without ever telling the operator why until they
    read the source.
- **Consequences:** spawns **OP-008d** (`Deke.Api` startup fails fast with a
  clear error if `"ApiKey"` is unconfigured; `CLAUDE.md`'s quick-start curl
  examples updated to set an API key first) — a small code-capable packet,
  per the standard ADR→packet flow in `CHARTER.md`.
- **Resolution (2026-07-07, Mikael, direct adjudication):** Accepted as
  "require key everywhere, fail startup if `ApiKey` missing." OP-008d
  drafted to implement it.
