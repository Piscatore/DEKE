# DEKE - Domain Expert Knowledge Engine

[![Build and Test](https://github.com/Piscatore/DEKE/actions/workflows/build.yml/badge.svg)](https://github.com/Piscatore/DEKE/actions/workflows/build.yml)

A self-improving, domain-specific knowledge base that serves as a semantic search backend for LLM services.

## What It Does

```
INGEST → LEARN → SERVE
```

1. **Ingest**: Collect facts from RSS feeds, web pages, and manual input
2. **Learn**: Discover patterns, build relations, improve structure over time
3. **Serve**: Provide semantic search via REST API and MCP for any LLM

## How It Works

DEKE runs a continuous learning loop:

- **Source Monitor** checks RSS feeds and web pages on a schedule, extracting new facts
- **Pattern Discovery** clusters similar facts using embedding similarity and identifies recurring themes
- **Relation Mapping** finds and links related facts across your knowledge base
- Each fact gets a 384-dimensional embedding (all-MiniLM-L6-v2) stored in pgvector for fast similarity search

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL 16 with [pgvector](https://github.com/pgvector/pgvector) extension — either installed locally or via container (see below)
- ~100 MB disk space for the embedding model

## Quick Start

```bash
# 1. Start PostgreSQL with pgvector (skip if you have a local PostgreSQL with pgvector)
podman-compose up -d
# Or: docker compose -f podman-compose.yml up -d

# 2. Download the embedding model
./scripts/download-model.sh
# On Windows PowerShell: pwsh scripts/download-model.ps1
# Manual download: https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2
#   Save model.onnx and vocab.txt to models/all-MiniLM-L6-v2/

# 3. Build and run
dotnet build
dotnet run --project src/Deke.Api
```

If you have PostgreSQL installed locally, ensure the `pgvector` extension is available and apply the schema from `init.sql`. Update the connection string in `appsettings.json` or via the `ConnectionStrings__Deke` environment variable.

## API Examples

```bash
# Add a fact
curl -X POST http://localhost:5000/api/facts \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: YOUR_KEY" \
  -d '{"content":"Walleye are most active during low-light conditions","domain":"fishing"}'

# Search facts
curl "http://localhost:5000/api/search?query=best%20time%20to%20fish&domain=fishing"

# Add a source to monitor
curl -X POST http://localhost:5000/api/sources \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: YOUR_KEY" \
  -d '{"url":"https://example.com/feed.rss","domain":"fishing","type":"Rss"}'
```

Write endpoints (POST, DELETE) require an API key via the `X-Api-Key` header. Read endpoints are open. Set the key via the `ApiKey` config value or `ApiKey` environment variable. When no key is configured, auth is disabled (development mode).

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

- [Product Overview](./docs/product/overview.md) - What DEKE is and why
- [Technical Specification](./docs/architecture/specification.md) - How it is built
- [CONTRIBUTING.md](./CONTRIBUTING.md) - How to contribute
- [Documentation Index](./docs/INDEX.md) - Full documentation map and conventions

## License

[MIT](./LICENSE)
