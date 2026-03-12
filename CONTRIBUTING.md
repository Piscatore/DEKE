# Contributing to DEKE

Thanks for your interest in contributing! Here's how to get started.

## Local Setup

1. **Prerequisites**: .NET 9 SDK, PostgreSQL 16 with pgvector (local install or via container)
2. **Start the database** (if using a container): `podman-compose up -d`
3. **Download the embedding model**: `./scripts/download-model.sh` (~100 MB)
4. **Configure local settings** (see below)
5. **Build**: `dotnet build`
6. **Run tests**: `dotnet test`
7. **Run the API**: `dotnet run --project src/Deke.Api`

### Database Options

**Container** (easiest): `podman-compose up -d` starts PostgreSQL with pgvector using default credentials. No extra configuration needed — `appsettings.Development.json` matches the container defaults.

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
