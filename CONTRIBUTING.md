# Contributing to DEKE

Thanks for your interest in contributing! Here's how to get started.

## Local Setup

1. **Prerequisites**: .NET 9 SDK, Podman (for PostgreSQL container)
2. **Start the database**: `podman machine start && podman compose -f podman-compose.yml up -d`
3. **Download the embedding model**: `./scripts/download-model.sh` (~100 MB)
4. **Configure local settings** (see below)
5. **Build**: `dotnet build`
6. **Run tests**: `dotnet test`
7. **Run the API**: `dotnet run --project src/Deke.Api`

### Database Options

**Container** (recommended): `podman compose -f podman-compose.yml up -d` starts PostgreSQL 16 with pgvector on port **5433**. The default `appsettings.json` connection strings match the container credentials (`deke`/`deke`). Data is persisted in a named Podman volume (`deke-postgres-data`).

If this is your first time, initialize the Podman machine first: `podman machine init && podman machine start`.

**Local PostgreSQL**: If you have PostgreSQL installed locally, ensure the `pgvector` extension is available and apply the schema:

```bash
psql -U your_user -d your_db -f init.sql
```

### Local Configuration Overrides

If your database credentials differ from the container defaults, create a local override file. Files matching `*.local.json` are gitignored and will never be committed.

Create `src/Deke.Api/appsettings.Development.local.json`:

```json
{
  "ConnectionStrings": {
    "Deke": "Host=localhost;Database=your_db;Username=your_user;Password=your_password"
  }
}
```

This file is loaded last and overrides all other configuration. You can also use environment variables:

```bash
export ConnectionStrings__Deke="Host=localhost;Database=mydb;Username=myuser;Password=mypass"
```

### LLM Provider Secrets

DEKE supports Gemini and OpenAI as interchangeable LLM backends. One is active at a time, selected via configuration. API keys are stored using .NET User Secrets (never in code or committed files).

All three host projects share a single secret store, so you set secrets once:

```bash
# Set your API keys (one or both)
dotnet user-secrets set "Llm:GeminiApiKey" "your-gemini-key" --project src/Deke.Api
dotnet user-secrets set "Llm:OpenAiApiKey" "your-openai-key" --project src/Deke.Api

# Choose the active provider
dotnet user-secrets set "Llm:Provider" "Gemini" --project src/Deke.Api
```

To switch providers, just change the `Provider` value — both keys stay stored:

```bash
dotnet user-secrets set "Llm:Provider" "OpenAi" --project src/Deke.Api
```

To verify what's stored:

```bash
dotnet user-secrets list --project src/Deke.Api
```

You can optionally override the default models:

```bash
dotnet user-secrets set "Llm:GeminiModel" "gemini-2.0-flash" --project src/Deke.Api
dotnet user-secrets set "Llm:OpenAiModel" "gpt-4o-mini" --project src/Deke.Api
```

**For Claude Code users**, you can also set keys via environment variables in `.claude/settings.local.json` (git-ignored):

```json
{
  "env": {
    "Llm__Provider": "Gemini",
    "Llm__GeminiApiKey": "your-gemini-key",
    "Llm__OpenAiApiKey": "your-openai-key"
  }
}
```

### API Key (Optional)

Write endpoints (POST, DELETE) require an API key via the `X-Api-Key` header. For local development, leave `ApiKey` empty in `appsettings.json` to disable auth. To test with auth enabled, add to your local override:

```json
{
  "ApiKey": "any-secret-key-you-choose"
}
```

## Coding Conventions

- File-scoped namespaces (`namespace Foo;`)
- Private fields prefixed with `_` (`private readonly Foo _foo;`)
- Records for DTOs and immutable models
- `CancellationToken` on all async methods
- 4-space indentation (see `.editorconfig`)
- New endpoints require auth by default (fallback policy). Add `.AllowAnonymous()` for read endpoints or `.RequireAuthorization()` for write endpoints explicitly.

## Documentation Guide

Documentation is organized into three branches under `docs/`. See [docs/INDEX.md](docs/INDEX.md) for the full map.

| Branch | Purpose | Content rule |
|--------|---------|-------------|
| `product/` | What the system does | No code, no library names, no SQL — conceptual only |
| `architecture/` | How it is built | Tech stack, schemas, API contracts, code patterns |
| `science/` | Background research | General theory — no DEKE-specific implementation details |

**File naming**: root = UPPERCASE (`README.md`), docs/ = lowercase-with-hyphens (`retrieval-pipeline.md`). Single H1 per file. Formal tone.

**Adding documentation**: Check `docs/INDEX.md` first. If a document for your topic exists, update it. If not, create a new file following naming conventions and add it to the index. Architecture decisions go in `docs/architecture/decisions.md`.

**Governance**: Documentation maintenance is managed by the doc-maintainer plugin. See the Documentation Governance section in [CLAUDE.md](CLAUDE.md) for details.

## PR Process

1. Fork the repo and create a feature branch from `main`
2. Make your changes and ensure `dotnet build` and `dotnet test` pass
3. Write a clear PR description explaining what and why
4. CI will run build + tests automatically

## Areas That Could Use Help

- **Harvesters**: New source types (GitHub Issues, PDFs, YouTube transcripts)
- **Tests**: Unit and integration test coverage
- **Documentation**: Tutorials, example use cases
- **Performance**: Embedding generation caching, query optimization

## Claude Code Setup (Recommended)

DEKE is developed with [Claude Code](https://claude.com/claude-code). If you use it, here's the tooling we've found most useful.

### MCP Servers

| Server | What it does | Install |
|--------|-------------|---------|
| [Postgres MCP](https://github.com/crystaldba/postgres-mcp) | Schema introspection, query analysis, index suggestions | `pip install postgres-mcp` |
| [Microsoft Learn](https://learn.microsoft.com/api/mcp) | Real-time .NET / ASP.NET / EF Core docs | HTTP MCP endpoint |
| [NuGet MCP](https://devblogs.microsoft.com/dotnet/nuget-mcp-server-preview) | Package discovery, vulnerability scanning | `dnx NuGet.Mcp.Server` |
| [Docker MCP](https://github.com/ckreiling/mcp-server-docker) | Container lifecycle management | `uvx mcp-server-docker` |

### Plugins

| Plugin | What it does |
|--------|-------------|
| [.NET Claude Kit](https://codewithmukesh.com/resources/dotnet-claude-kit) | 47 skills, Roslyn analysis, slash commands (`/build-fix`, `/tdd`, `/code-review`) |
| [Serena](https://github.com/maks-ivanov/serena) | Semantic code navigation and symbol-level editing |

### Adding DEKE as MCP Server

To use DEKE's own knowledge base from Claude Code:

```bash
claude mcp add deke -- dotnet run --project src/Deke.Mcp
```

## Questions?

Open an issue — happy to help!
