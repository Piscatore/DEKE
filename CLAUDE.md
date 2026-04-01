# CLAUDE.md - DEKE Project Instructions

## Project Identity

**Name**: DEKE - Domain Expert Knowledge Engine
**Type**: .NET 9 Solution (Multi-project)
**Purpose**: Self-improving, domain-specific knowledge base with semantic search
**Status**: New Development

## Quick Start

```bash
# Start PostgreSQL with pgvector (if using container)
podman-compose up -d

# Build solution
dotnet build

# Database schema is applied via init.sql on first container start
# No migrations needed - schema managed via SQL scripts

# Download embedding model (one-time)
./scripts/download-model.sh

# Run API
dotnet run --project src/Deke.Api

# Run MCP Server (in separate terminal)
dotnet run --project src/Deke.Mcp
```

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    DOMAIN EXPERT ENGINE                         │
├─────────────────────────────────────────────────────────────────┤
│   INGEST          →        LEARN         →        SERVE         │
│   • Sources                • Patterns             • Search      │
│   • Extract                • Relations            • Context     │
│   • Monitor                • Structure            • API/MCP     │
└─────────────────────────────────────────────────────────────────┘
```

## Project Structure

| Project | Purpose |
|---------|---------|
| `Deke.Core` | Domain models, interfaces (no dependencies) |
| `Deke.Infrastructure` | Dapper + FastCrud, pgvector, ONNX embeddings, harvesters |
| `Deke.Api` | REST API endpoints |
| `Deke.Mcp` | MCP Server for Claude/LLM integration |
| `Deke.Worker` | Background services (source monitoring, learning) |
| `Deke.Tests` | Unit and integration tests |

## Key Technologies

- **Database**: PostgreSQL 16 + pgvector extension
- **Data Access**: Dapper + Dapper.FastCrud (CRUD), Pgvector.Dapper (vector ops)
- **Embeddings**: ONNX Runtime with all-MiniLM-L6-v2 (384 dimensions)
- **Vector Search**: pgvector with IVFFlat index
- **MCP**: ModelContextProtocol SDK

## Implementation Guidelines

### Database Access

All database access uses **Dapper** via `DbConnectionFactory`:
```csharp
await using var conn = await _db.CreateConnectionAsync(ct);

// Vector similarity search - raw SQL with Pgvector.Dapper
var embedding = new Vector(queryEmbedding);
var results = await conn.QueryAsync<FactSearchResult>(
    "SELECT * FROM facts WHERE 1 - (embedding <=> @embedding::vector) > 0.5",
    new { embedding });

// Regular CRUD - use Dapper.FastCrud
var fact = await conn.GetAsync(new Fact { Id = id });
await conn.InsertAsync(fact);
await conn.UpdateAsync(fact);
```

### Type Handlers

Custom Dapper type handlers are registered in `DapperConfig.Initialize()`:
- `JsonbTypeHandler<T>` - serializes/deserializes JSONB columns
- `GuidArrayTypeHandler` - maps PostgreSQL `uuid[]` to `List<Guid>`
- `VectorTypeHandler` - maps pgvector `vector` type (from Pgvector.Dapper)

### Schema Management

Schema is defined in `init.sql` (runs on first container start, or apply manually for local PostgreSQL).
For schema changes, update `init.sql` and apply manually or recreate the container.

### Embedding Generation

The `OnnxEmbeddingService` handles tokenization and embedding:
```csharp
var embedding = _embeddings.GenerateEmbedding("some text");
// Returns float[384]
```

Always normalize embeddings (L2 norm) before storage.

### Adding New MCP Tools

1. Create tool class in `Deke.Mcp/Tools/`
2. Add `[McpTool("tool_name")]` attribute
3. Add `[Description("...")]` to class and parameters
4. Register in `Program.cs` with `.WithTools<YourTool>()`

### Adding New Harvesters

1. Implement `IHarvester` interface
2. Set `SupportedType` property
3. Register in DI container
4. Harvesters are selected by `Source.Type`

## Configuration

Environment variables override appsettings.json:

| Variable | Default | Description |
|----------|---------|-------------|
| `ConnectionStrings__Deke` | localhost | PostgreSQL connection |
| `Embeddings__ModelPath` | models/all-MiniLM-L6-v2/model.onnx | ONNX model |
| `Embeddings__VocabPath` | models/all-MiniLM-L6-v2/vocab.txt | Vocabulary |

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Common Tasks

### Add a new domain
Just start adding facts with the domain name - no pre-configuration needed.

### Add a source to monitor
```bash
curl -X POST http://localhost:5000/api/sources \
  -H "Content-Type: application/json" \
  -d '{"url":"https://example.com/feed.rss","domain":"fishing","type":"Rss"}'
```

### Search facts
```bash
curl "http://localhost:5000/api/search?query=ice%20fishing&domain=fishing"
```

### Use MCP with Claude Code
```bash
claude mcp add deke -- dotnet run --project "C:/src/Life Projects/DEKE/src/Deke.Mcp"
```

## Code Style

- Use `required` keyword for mandatory properties
- Use records for DTOs and immutable models
- Use `CancellationToken` for all async methods
- Prefix private fields with `_`
- Use file-scoped namespaces
- **All `.cs` files must be UTF-8 with BOM** (editorconfig: `charset = utf-8-bom`). When creating new files or fully rewriting existing files, ensure the file starts with a UTF-8 BOM (`EF BB BF`). The CI format check will reject files missing the BOM.
- **Keep `using` directives sorted alphabetically** (editorconfig: `dotnet_sort_system_directives_first = true`). When adding a new `using`, insert it in alphabetical order.

## References

- [Technical Specification](./docs/architecture/specification.md) - How it is built
- [Product Overview](./docs/product/overview.md) - What DEKE is and why
- [Documentation Index](./docs/INDEX.md) - Full documentation map and conventions
- [pgvector docs](https://github.com/pgvector/pgvector)
- [pgvector-dotnet](https://github.com/pgvector/pgvector-dotnet) - Dapper + pgvector
- [Dapper.FastCrud](https://github.com/MoonStorm/FastCrud) - CRUD operations
- [MCP SDK](https://github.com/modelcontextprotocol)

## Documentation Governance

Documentation is managed by the doc-maintainer plugin (active mode).

- **All documentation changes** must go through doc-maintainer (delegate via Task tool)
- **Never edit .md files directly** - always delegate to doc-maintainer
- **After code changes**, notify doc-maintainer to assess documentation impact
- **Exceptions**: CLAUDE.md itself (managed by doc-maintainer for this section only)

### Configuration

```
Content Type:         Standard documentation
Operation:            Active maintenance
Scope:                Entire repo (root + docs/)
Project Type:         Multi-service backend (API + MCP + Worker + Core)
Versioning:           docs/architecture/decisions.md (decision log)
Style:                Formal tone, single H1, UPPERCASE root / lowercase-hyphen docs/
Authoritative Sources: Reference only (link to external docs)
Update Triggers:      Public API changes, configuration changes, dependency changes,
                      new project/component, interface changes (Deke.Core),
                      infrastructure changes, decision changes (what + why)
Forbidden Actions:    None
Cross-References:     Relative Markdown links, one-directional, documentation index maintained
```

### Versioned Documents

Only `docs/architecture/decisions.md` carries a version/decision log. The log records what changed and the reasoning behind it, especially when LLM-assisted development decisions diverge from earlier plans. Git handles version control; the in-document log serves as a decision journal.

To run a documentation audit: "Run doc-maintainer in audit mode"
To update docs: "Use doc-maintainer to update [description]"
