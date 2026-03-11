# MCP Servers & Plugins Research for DEKE

## MCP Server Registries
| Registry | URL |
|----------|-----|
| PulseMCP | pulsemcp.com/servers (8,590+ servers) |
| Smithery.ai | smithery.ai |
| Glama.ai | glama.ai/mcp/servers |
| Docker MCP Catalog | hub.docker.com/mcp (270+ curated) |
| Official MCP Servers | github.com/modelcontextprotocol/servers |
| Awesome MCP (punkpeye) | github.com/punkpeye/awesome-mcp-servers |
| Awesome .NET MCP | github.com/SciSharp/Awesome-DotNET-MCP |

## Tier 1: Install First

### .NET Claude Kit (codewithmukesh)
- 47 skills, 10 specialist agents (EF Core specialist, API designer, test engineer, security auditor)
- 16 slash commands (/scaffold, /tdd, /code-review, /build-fix)
- Roslyn MCP tools for semantic analysis
- Source: codewithmukesh.com/resources/dotnet-claude-kit

### dotnet-skills (Aaronontheweb)
- 30 skills, 5 specialist agents
- EF Core patterns, Testcontainers, concurrency, benchmarking
- Source: github.com/Aaronontheweb/dotnet-skills

### Postgres MCP Pro (Crystal DBA)
- Schema introspection, execution plan analysis, index simulation, pg_stat_statements
- Source: github.com/crystaldba/postgres-mcp

## Tier 2: High Value

### PGVector Semantic Search Server
- Similarity searches, metadata filtering, embedding insertion
- Source: smithery.ai/server/@yusufferdogan/mcp-pgvector-server

### Docker MCP Server (ckreiling)
- Container lifecycle, docker-compose workflows, resource stats, logs
- Source: github.com/ckreiling/mcp-server-docker

### Microsoft Learn MCP Server
- Real-time .NET 9 / ASP.NET / EF Core docs lookup
- Source: github.com/MicrosoftDocs/mcp (endpoint: https://learn.microsoft.com/api/mcp)

### NuGet MCP Server (Microsoft)
- Package discovery, vulnerability scanning, dependency conflict resolution
- Source: devblogs.microsoft.com/dotnet/nuget-mcp-server-preview

## Tier 3: Relevant to Harvesting Pipeline

### Firecrawl MCP Server
- Scrape, batch scrape, URL discovery, crawl. Markdown/JSON output.
- Source: github.com/firecrawl/firecrawl-mcp-server

### mcp-rss-crawler (mshk)
- RSS feed parsing, storage, keyword search, article summarization
- Source: github.com/mshk/mcp-rss-crawler

### Playwright MCP (Microsoft)
- Browser automation for JS-rendered content
- Source: github.com/microsoft/playwright-mcp

## Tier 4: Nice to Have

| Tool | Purpose | Source |
|------|---------|--------|
| GitHub Official MCP | PR reviews, CI/CD, issues | github.com/github/github-mcp-server |
| AI Log Analyzer MCP | Serilog analysis | (Medium article by aaronlu5) |
| DotNetMetadataMcpServer | .NET type info for AI | github.com/V0v1kkk/DotNetMetadataMcpServer |
| QuickMCP | Auto-gen MCP from OpenAPI spec | github.com/gunpal5/QuickMCP |
| HenkDz PostgreSQL MCP | 14 DB management tools | github.com/HenkDz/postgresql-mcp-server |
| MCP Memory Server | pgvector + BERT memory (reference arch) | github.com/sdimitrov/mcp-memory |
