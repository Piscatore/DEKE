# DEKE - Domain Expert Knowledge Engine

A self-improving, domain-specific knowledge base that serves as a semantic search backend for LLM services.

## What It Does

```
INGEST → LEARN → SERVE
```

1. **Ingest**: Collect facts from RSS feeds, web pages, and manual input
2. **Learn**: Discover patterns, build relations, improve structure over time
3. **Serve**: Provide semantic search via REST API and MCP for any LLM

## Quick Start

```bash
# 1. Start PostgreSQL with pgvector
docker-compose up -d

# 2. Download the embedding model
./scripts/download-model.sh

# 3. Build and run
dotnet build
dotnet run --project src/Deke.Api
```

## Use with Claude

```bash
# Add as MCP server
claude mcp add deke -- dotnet run --project src/Deke.Mcp
```

Then in Claude:
```
Search my fishing knowledge base for "ice fishing techniques"
```

## Architecture

| Component | Technology |
|-----------|------------|
| Database | PostgreSQL 16 + pgvector |
| Embeddings | ONNX (all-MiniLM-L6-v2, 384 dim) |
| API | ASP.NET Core Minimal APIs |
| LLM Integration | Model Context Protocol (MCP) |
| Background Jobs | .NET BackgroundService |

## Documentation

- [SPECIFICATION.md](./SPECIFICATION.md) - Complete technical specification
- [CLAUDE.md](./CLAUDE.md) - Instructions for AI-assisted development
- [Documentation Index](./docs/INDEX.md) - Full documentation map and conventions

## License

MIT
