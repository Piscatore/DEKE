# DEKE - Domain Expert Knowledge Engine

## Executive Summary

DEKE is a self-improving, domain-specific knowledge engine that:
1. **Ingests** facts from configured sources (RSS, web pages, manual input)
2. **Learns** by finding patterns, relations, and structure in facts
3. **Serves** as a semantic search backend for any LLM service via REST API and MCP

**Core principle**: Store domain expertise, keep it current, make it searchable.

---

## Technology Stack

| Component | Technology | NuGet Package | Version |
|-----------|------------|---------------|---------|
| Runtime | .NET 9 | - | Latest |
| Database | PostgreSQL 16 + pgvector | Npgsql | 9.0.3 |
| Vector support | pgvector | Pgvector, Pgvector.Dapper | 0.3.0 |
| Data Access | Dapper + Dapper.FastCrud | Dapper, Dapper.FastCrud | 2.1.35, 3.3.2 |
| Embeddings | ONNX Runtime | Microsoft.ML.OnnxRuntime | 1.19.x |
| Tokenizer | BERTTokenizers | BERTTokenizers | 1.2.x |
| Web API | ASP.NET Core Minimal APIs | Built-in | - |
| MCP Server | ModelContextProtocol | ModelContextProtocol | Latest |
| HTTP Client | - | Microsoft.Extensions.Http | 9.x |
| HTML Parsing | AngleSharp | AngleSharp | 1.1.x |
| RSS Parsing | - | System.ServiceModel.Syndication | 9.x |
| JSON | System.Text.Json | Built-in | - |
| Logging | Serilog | Serilog.AspNetCore | 8.x |
| Scheduling | BackgroundService | Built-in | - |

### Embedding Model

**Model**: `all-MiniLM-L6-v2` (ONNX format)
- **Dimension**: 384
- **Download**: https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2
- **Files needed**: 
  - `model.onnx` (or `model_quantized.onnx` for smaller size)
  - `vocab.txt`

Store in: `models/all-MiniLM-L6-v2/`

---

## Project Structure

```
DEKE/
├── DEKE.sln
├── podman-compose.yml
├── .gitignore
├── README.md
├── SPECIFICATION.md                    # This file
├── models/
│   └── all-MiniLM-L6-v2/
│       ├── model.onnx
│       └── vocab.txt
├── src/
│   ├── Deke.Core/                      # Domain models, interfaces
│   │   ├── Models/
│   │   │   ├── Fact.cs
│   │   │   ├── Source.cs
│   │   │   ├── Term.cs
│   │   │   ├── Pattern.cs
│   │   │   └── SearchResult.cs
│   │   ├── Interfaces/
│   │   │   ├── IFactRepository.cs
│   │   │   ├── ISourceRepository.cs
│   │   │   ├── ITermRepository.cs
│   │   │   ├── IPatternRepository.cs
│   │   │   ├── IFactRelationRepository.cs
│   │   │   ├── ILearningLogRepository.cs
│   │   │   ├── IEmbeddingService.cs
│   │   │   ├── IExtractionService.cs
│   │   │   └── IHarvester.cs
│   │   └── Deke.Core.csproj
│   │
│   ├── Deke.Infrastructure/            # Implementations
│   │   ├── Data/
│   │   │   ├── DbConnectionFactory.cs
│   │   │   ├── DapperConfig.cs
│   │   │   └── TypeHandlers/
│   │   │       ├── JsonbTypeHandler.cs
│   │   │       ├── GuidArrayTypeHandler.cs
│   │   │       └── EnumTypeHandler.cs
│   │   ├── Repositories/
│   │   │   ├── FactRepository.cs
│   │   │   ├── SourceRepository.cs
│   │   │   ├── TermRepository.cs
│   │   │   ├── PatternRepository.cs
│   │   │   ├── FactRelationRepository.cs
│   │   │   └── LearningLogRepository.cs
│   │   ├── ServiceCollectionExtensions.cs
│   │   ├── Embeddings/
│   │   │   ├── OnnxEmbeddingService.cs
│   │   │   └── BertTokenizer.cs
│   │   ├── Extraction/
│   │   │   └── LlmExtractionService.cs
│   │   ├── Harvesters/
│   │   │   ├── RssHarvester.cs
│   │   │   ├── WebPageHarvester.cs
│   │   │   └── HarvesterFactory.cs
│   │   └── Deke.Infrastructure.csproj
│   │
│   ├── Deke.Api/                       # REST API
│   │   ├── Endpoints/
│   │   │   ├── SearchEndpoints.cs
│   │   │   ├── FactEndpoints.cs
│   │   │   ├── SourceEndpoints.cs
│   │   │   └── HealthEndpoints.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Deke.Api.csproj
│   │
│   ├── Deke.Mcp/                       # MCP Server (stdio)
│   │   ├── Tools/
│   │   │   ├── SearchTools.cs
│   │   │   ├── FactTools.cs
│   │   │   └── SourceTools.cs
│   │   ├── Program.cs
│   │   └── Deke.Mcp.csproj
│   │
│   └── Deke.Worker/                    # Background services
│       ├── Services/
│       │   ├── SourceMonitorService.cs
│       │   ├── LearningCycleService.cs
│       │   └── PatternDiscoveryService.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── Deke.Worker.csproj
│
└── tests/
    └── Deke.Tests/
        ├── EmbeddingServiceTests.cs
        ├── FactRepositoryTests.cs
        └── Deke.Tests.csproj
```

---

## Database Schema

### PostgreSQL Setup

```sql
-- Run these as superuser to set up the database

CREATE DATABASE deke;
\c deke

-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Enable UUID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

### Tables

```sql
-- Sources: Where facts come from
CREATE TABLE sources (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    url TEXT NOT NULL,
    domain VARCHAR(100) NOT NULL,
    name VARCHAR(200),
    type VARCHAR(20) NOT NULL DEFAULT 'WebPage',  -- WebPage, Rss, Api, Manual
    check_interval INTERVAL NOT NULL DEFAULT '1 day',
    last_checked_at TIMESTAMPTZ,
    last_changed_at TIMESTAMPTZ,
    content_hash VARCHAR(64),
    credibility REAL NOT NULL DEFAULT 0.5,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    metadata JSONB DEFAULT '{}',
    CONSTRAINT sources_url_unique UNIQUE (url)
);

CREATE INDEX idx_sources_domain ON sources(domain);
CREATE INDEX idx_sources_active ON sources(is_active) WHERE is_active = TRUE;
CREATE INDEX idx_sources_next_check ON sources(last_checked_at, check_interval) WHERE is_active = TRUE;

-- Facts: Individual pieces of knowledge
CREATE TABLE facts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    content TEXT NOT NULL,
    domain VARCHAR(100) NOT NULL,
    embedding vector(384),
    confidence REAL NOT NULL DEFAULT 1.0,
    source_id UUID REFERENCES sources(id) ON DELETE SET NULL,
    related_fact_ids UUID[] DEFAULT '{}',
    entities JSONB DEFAULT '[]',           -- Extracted entities [{type, value}]
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_outdated BOOLEAN NOT NULL DEFAULT FALSE,
    outdated_reason VARCHAR(200)
);

-- Vector similarity index (IVFFlat for good balance of speed/accuracy)
-- lists = sqrt(num_rows) is a good starting point, adjust as data grows
CREATE INDEX idx_facts_embedding ON facts 
USING ivfflat (embedding vector_cosine_ops) WITH (lists = 100);

CREATE INDEX idx_facts_domain ON facts(domain);
CREATE INDEX idx_facts_source ON facts(source_id);
CREATE INDEX idx_facts_created ON facts(created_at DESC);
CREATE INDEX idx_facts_active ON facts(domain, is_outdated) WHERE is_outdated = FALSE;

-- Terms: Domain-specific terminology
CREATE TABLE terms (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    canonical_form VARCHAR(200) NOT NULL,
    domain VARCHAR(100) NOT NULL,
    contexts JSONB NOT NULL DEFAULT '[]',      -- [{name, definition, signals[]}]
    translations JSONB NOT NULL DEFAULT '{}',  -- {lang_code: term}
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    CONSTRAINT terms_canonical_domain_unique UNIQUE (canonical_form, domain)
);

CREATE INDEX idx_terms_domain ON terms(domain);

-- Patterns: Discovered regularities in facts
CREATE TABLE patterns (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    description TEXT NOT NULL,
    domain VARCHAR(100) NOT NULL,
    pattern_type VARCHAR(50) NOT NULL DEFAULT 'observation',  -- observation, causal, temporal, structural
    evidence_fact_ids UUID[] NOT NULL DEFAULT '{}',
    confidence REAL NOT NULL,
    occurrence_count INT NOT NULL DEFAULT 1,
    discovered_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_validated_at TIMESTAMPTZ,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX idx_patterns_domain ON patterns(domain);
CREATE INDEX idx_patterns_confidence ON patterns(confidence DESC);

-- Fact relations: Explicit relationships between facts
CREATE TABLE fact_relations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    from_fact_id UUID NOT NULL REFERENCES facts(id) ON DELETE CASCADE,
    to_fact_id UUID NOT NULL REFERENCES facts(id) ON DELETE CASCADE,
    relation_type VARCHAR(50) NOT NULL,  -- causes, supports, contradicts, elaborates, etc.
    confidence REAL NOT NULL DEFAULT 0.5,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fact_relations_unique UNIQUE (from_fact_id, to_fact_id, relation_type)
);

CREATE INDEX idx_fact_relations_from ON fact_relations(from_fact_id);
CREATE INDEX idx_fact_relations_to ON fact_relations(to_fact_id);

-- Learning logs: Track system improvements
CREATE TABLE learning_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    domain VARCHAR(100) NOT NULL,
    cycle_type VARCHAR(50) NOT NULL,  -- source_check, pattern_discovery, relation_mapping, cleanup
    started_at TIMESTAMPTZ NOT NULL,
    completed_at TIMESTAMPTZ,
    facts_added INT DEFAULT 0,
    facts_updated INT DEFAULT 0,
    facts_outdated INT DEFAULT 0,
    patterns_discovered INT DEFAULT 0,
    relations_added INT DEFAULT 0,
    notes TEXT,
    error_message TEXT
);

CREATE INDEX idx_learning_logs_domain ON learning_logs(domain, started_at DESC);
```

---

## Core Models

### Deke.Core/Models/Fact.cs

```csharp
namespace Deke.Core.Models;

public class Fact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Content { get; set; }
    public required string Domain { get; set; }
    public float[]? Embedding { get; set; }
    public float Confidence { get; set; } = 1.0f;
    public Guid? SourceId { get; set; }
    public List<Guid> RelatedFactIds { get; set; } = [];
    public List<ExtractedEntity> Entities { get; set; } = [];
    public Dictionary<string, object> Metadata { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsOutdated { get; set; } = false;
    public string? OutdatedReason { get; set; }
    
    // Navigation
    public Source? Source { get; set; }
}

public class ExtractedEntity
{
    public required string Type { get; set; }   // Person, Event, Location, Organization, etc.
    public required string Value { get; set; }
}
```

### Deke.Core/Models/Source.cs

```csharp
namespace Deke.Core.Models;

public class Source
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Url { get; set; }
    public required string Domain { get; set; }
    public string? Name { get; set; }
    public SourceType Type { get; set; } = SourceType.WebPage;
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromDays(1);
    public DateTimeOffset? LastCheckedAt { get; set; }
    public DateTimeOffset? LastChangedAt { get; set; }
    public string? ContentHash { get; set; }
    public float Credibility { get; set; } = 0.5f;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = [];
    
    // Navigation
    public List<Fact> Facts { get; set; } = [];
    
    // Computed
    public DateTimeOffset? NextCheckAt => LastCheckedAt?.Add(CheckInterval);
    public bool IsDueForCheck => NextCheckAt == null || NextCheckAt <= DateTimeOffset.UtcNow;
}

public enum SourceType
{
    WebPage,
    Rss,
    Api,
    Manual
}
```

### Deke.Core/Models/Term.cs

```csharp
namespace Deke.Core.Models;

public class Term
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string CanonicalForm { get; set; }
    public required string Domain { get; set; }
    public List<TermContext> Contexts { get; set; } = [];
    public Dictionary<string, string> Translations { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class TermContext
{
    public required string Name { get; set; }
    public required string Definition { get; set; }
    public List<string> Signals { get; set; } = [];
}
```

### Deke.Core/Models/Pattern.cs

```csharp
namespace Deke.Core.Models;

public class Pattern
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Description { get; set; }
    public required string Domain { get; set; }
    public PatternType PatternType { get; set; } = PatternType.Observation;
    public List<Guid> EvidenceFactIds { get; set; } = [];
    public float Confidence { get; set; }
    public int OccurrenceCount { get; set; } = 1;
    public DateTimeOffset DiscoveredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastValidatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum PatternType
{
    Observation,    // General observation about the domain
    Causal,         // X tends to cause Y
    Temporal,       // X happens before/after Y
    Structural      // Things of type X have property Y
}
```

### Deke.Core/Models/SearchResult.cs

```csharp
namespace Deke.Core.Models;

public class FactSearchResult
{
    public required Guid Id { get; set; }
    public required string Content { get; set; }
    public required string Domain { get; set; }
    public float Confidence { get; set; }
    public float Similarity { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class SearchResponse
{
    public required string Query { get; set; }
    public required string Domain { get; set; }
    public List<FactSearchResult> Results { get; set; } = [];
    public int TotalCount { get; set; }
    public TimeSpan SearchDuration { get; set; }
}

public class ContextResponse
{
    public required string Topic { get; set; }
    public required string Domain { get; set; }
    public required string Context { get; set; }
    public int FactCount { get; set; }
    public int ApproximateTokens { get; set; }
}
```

---

## Core Interfaces

### Deke.Core/Interfaces/IFactRepository.cs

```csharp
namespace Deke.Core.Interfaces;

public interface IFactRepository
{
    Task<Fact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Fact>> GetByDomainAsync(string domain, int limit = 100, CancellationToken ct = default);
    Task<List<Fact>> GetBySourceAsync(Guid sourceId, CancellationToken ct = default);
    
    Task<List<FactSearchResult>> SearchAsync(
        float[] embedding, 
        string domain, 
        int limit = 10, 
        float minSimilarity = 0.5f,
        CancellationToken ct = default);
    
    Task<Guid> AddAsync(Fact fact, CancellationToken ct = default);
    Task UpdateAsync(Fact fact, CancellationToken ct = default);
    Task MarkOutdatedAsync(Guid id, string reason, CancellationToken ct = default);
    
    Task<int> GetCountAsync(string domain, CancellationToken ct = default);
    Task<List<Fact>> GetRecentAsync(string domain, int days, int limit = 100, CancellationToken ct = default);
    Task<List<Fact>> GetWithoutRelationsAsync(string domain, int limit = 50, CancellationToken ct = default);
}
```

### Deke.Core/Interfaces/IEmbeddingService.cs

```csharp
namespace Deke.Core.Interfaces;

public interface IEmbeddingService
{
    float[] GenerateEmbedding(string text);
    float[][] GenerateEmbeddings(IEnumerable<string> texts);
    float CosineSimilarity(float[] a, float[] b);
}
```

### Deke.Core/Interfaces/IExtractionService.cs

```csharp
namespace Deke.Core.Interfaces;

public interface IExtractionService
{
    Task<List<ExtractedFact>> ExtractFactsAsync(
        string content, 
        string domain, 
        string? sourceContext = null,
        CancellationToken ct = default);
}

public class ExtractedFact
{
    public required string Content { get; set; }
    public float Confidence { get; set; } = 0.8f;
    public List<ExtractedEntity> Entities { get; set; } = [];
}
```

### Deke.Core/Interfaces/IHarvester.cs

```csharp
namespace Deke.Core.Interfaces;

public interface IHarvester
{
    SourceType SupportedType { get; }
    Task<HarvestResult> HarvestAsync(Source source, CancellationToken ct = default);
}

public class HarvestResult
{
    public bool HasChanges { get; set; }
    public string? NewContentHash { get; set; }
    public List<string> ExtractedTexts { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
```

---

## Infrastructure Implementation

### Deke.Infrastructure/Data/DekaDbContext.cs

```csharp
using Deke.Core.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Deke.Infrastructure.Data;

public class DekaDbContext : DbContext
{
    public DbSet<Fact> Facts => Set<Fact>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<Pattern> Patterns => Set<Pattern>();
    
    public DekaDbContext(DbContextOptions<DekaDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fact configuration
        modelBuilder.Entity<Fact>(entity =>
        {
            entity.ToTable("facts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.Domain).HasColumnName("domain").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Embedding).HasColumnName("embedding").HasColumnType("vector(384)");
            entity.Property(e => e.Confidence).HasColumnName("confidence");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.RelatedFactIds).HasColumnName("related_fact_ids");
            entity.Property(e => e.Entities).HasColumnName("entities").HasColumnType("jsonb");
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.IsOutdated).HasColumnName("is_outdated");
            entity.Property(e => e.OutdatedReason).HasColumnName("outdated_reason").HasMaxLength(200);
            
            entity.HasOne(e => e.Source)
                  .WithMany(s => s.Facts)
                  .HasForeignKey(e => e.SourceId);
        });
        
        // Source configuration
        modelBuilder.Entity<Source>(entity =>
        {
            entity.ToTable("sources");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Url).HasColumnName("url").IsRequired();
            entity.Property(e => e.Domain).HasColumnName("domain").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(20)
                  .HasConversion<string>();
            entity.Property(e => e.CheckInterval).HasColumnName("check_interval");
            entity.Property(e => e.LastCheckedAt).HasColumnName("last_checked_at");
            entity.Property(e => e.LastChangedAt).HasColumnName("last_changed_at");
            entity.Property(e => e.ContentHash).HasColumnName("content_hash").HasMaxLength(64);
            entity.Property(e => e.Credibility).HasColumnName("credibility");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            
            entity.HasIndex(e => e.Url).IsUnique();
            entity.HasIndex(e => e.Domain);
        });
        
        // Term configuration
        modelBuilder.Entity<Term>(entity =>
        {
            entity.ToTable("terms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CanonicalForm).HasColumnName("canonical_form").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Domain).HasColumnName("domain").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Contexts).HasColumnName("contexts").HasColumnType("jsonb");
            entity.Property(e => e.Translations).HasColumnName("translations").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => new { e.CanonicalForm, e.Domain }).IsUnique();
        });
        
        // Pattern configuration
        modelBuilder.Entity<Pattern>(entity =>
        {
            entity.ToTable("patterns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.Domain).HasColumnName("domain").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasColumnName("pattern_type").HasMaxLength(50)
                  .HasConversion<string>();
            entity.Property(e => e.EvidenceFactIds).HasColumnName("evidence_fact_ids");
            entity.Property(e => e.Confidence).HasColumnName("confidence");
            entity.Property(e => e.OccurrenceCount).HasColumnName("occurrence_count");
            entity.Property(e => e.DiscoveredAt).HasColumnName("discovered_at");
            entity.Property(e => e.LastValidatedAt).HasColumnName("last_validated_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });
    }
}
```

### Deke.Infrastructure/Data/FactRepository.cs

```csharp
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;

namespace Deke.Infrastructure.Data;

public class FactRepository : IFactRepository
{
    private readonly DekaDbContext _context;
    private readonly string _connectionString;
    
    public FactRepository(DekaDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("Deke")!;
    }
    
    public async Task<List<FactSearchResult>> SearchAsync(
        float[] embedding, 
        string domain, 
        int limit = 10, 
        float minSimilarity = 0.5f,
        CancellationToken ct = default)
    {
        // Use raw SQL for vector search (EF Core doesn't support pgvector operators yet)
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        
        // Register pgvector
        await using (var cmd = new NpgsqlCommand("SELECT NULL::vector", conn))
            await cmd.ExecuteNonQueryAsync(ct);
        
        var sql = """
            SELECT 
                f.id, f.content, f.domain, f.confidence, f.source_id, f.created_at,
                s.url as source_url,
                1 - (f.embedding <=> $1::vector) as similarity
            FROM facts f
            LEFT JOIN sources s ON f.source_id = s.id
            WHERE f.domain = $2
              AND f.is_outdated = FALSE
              AND f.embedding IS NOT NULL
              AND 1 - (f.embedding <=> $1::vector) > $3
            ORDER BY f.embedding <=> $1::vector
            LIMIT $4
            """;
        
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(new Vector(embedding));
        cmd.Parameters.AddWithValue(domain);
        cmd.Parameters.AddWithValue(minSimilarity);
        cmd.Parameters.AddWithValue(limit);
        
        var results = new List<FactSearchResult>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        
        while (await reader.ReadAsync(ct))
        {
            results.Add(new FactSearchResult
            {
                Id = reader.GetGuid(0),
                Content = reader.GetString(1),
                Domain = reader.GetString(2),
                Confidence = reader.GetFloat(3),
                SourceId = reader.IsDBNull(4) ? null : reader.GetGuid(4),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(5),
                SourceUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                Similarity = reader.GetFloat(7)
            });
        }
        
        return results;
    }
    
    public async Task<Guid> AddAsync(Fact fact, CancellationToken ct = default)
    {
        // Use raw SQL for vector insert
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        
        var sql = """
            INSERT INTO facts (id, content, domain, embedding, confidence, source_id, 
                              related_fact_ids, entities, metadata, created_at)
            VALUES ($1, $2, $3, $4::vector, $5, $6, $7, $8::jsonb, $9::jsonb, $10)
            RETURNING id
            """;
        
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(fact.Id);
        cmd.Parameters.AddWithValue(fact.Content);
        cmd.Parameters.AddWithValue(fact.Domain);
        cmd.Parameters.AddWithValue(fact.Embedding != null ? new Vector(fact.Embedding) : DBNull.Value);
        cmd.Parameters.AddWithValue(fact.Confidence);
        cmd.Parameters.AddWithValue(fact.SourceId.HasValue ? fact.SourceId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue(fact.RelatedFactIds.ToArray());
        cmd.Parameters.AddWithValue(System.Text.Json.JsonSerializer.Serialize(fact.Entities));
        cmd.Parameters.AddWithValue(System.Text.Json.JsonSerializer.Serialize(fact.Metadata));
        cmd.Parameters.AddWithValue(fact.CreatedAt);
        
        return (Guid)(await cmd.ExecuteScalarAsync(ct))!;
    }
    
    public async Task<Fact?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Facts
            .Include(f => f.Source)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }
    
    public async Task<List<Fact>> GetByDomainAsync(string domain, int limit = 100, CancellationToken ct = default)
    {
        return await _context.Facts
            .Where(f => f.Domain == domain && !f.IsOutdated)
            .OrderByDescending(f => f.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }
    
    public async Task<List<Fact>> GetBySourceAsync(Guid sourceId, CancellationToken ct = default)
    {
        return await _context.Facts
            .Where(f => f.SourceId == sourceId)
            .ToListAsync(ct);
    }
    
    public async Task UpdateAsync(Fact fact, CancellationToken ct = default)
    {
        fact.UpdatedAt = DateTimeOffset.UtcNow;
        _context.Facts.Update(fact);
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task MarkOutdatedAsync(Guid id, string reason, CancellationToken ct = default)
    {
        await _context.Facts
            .Where(f => f.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(f => f.IsOutdated, true)
                .SetProperty(f => f.OutdatedReason, reason)
                .SetProperty(f => f.UpdatedAt, DateTimeOffset.UtcNow), ct);
    }
    
    public async Task<int> GetCountAsync(string domain, CancellationToken ct = default)
    {
        return await _context.Facts
            .Where(f => f.Domain == domain && !f.IsOutdated)
            .CountAsync(ct);
    }
    
    public async Task<List<Fact>> GetRecentAsync(string domain, int days, int limit = 100, CancellationToken ct = default)
    {
        var since = DateTimeOffset.UtcNow.AddDays(-days);
        return await _context.Facts
            .Where(f => f.Domain == domain && f.CreatedAt >= since && !f.IsOutdated)
            .OrderByDescending(f => f.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }
    
    public async Task<List<Fact>> GetWithoutRelationsAsync(string domain, int limit = 50, CancellationToken ct = default)
    {
        return await _context.Facts
            .Where(f => f.Domain == domain && !f.IsOutdated && f.RelatedFactIds.Count == 0)
            .OrderByDescending(f => f.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }
}
```

### Deke.Infrastructure/Embeddings/OnnxEmbeddingService.cs

```csharp
using Deke.Core.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Deke.Infrastructure.Embeddings;

public class OnnxEmbeddingService : IEmbeddingService, IDisposable
{
    private readonly InferenceSession _session;
    private readonly string[] _vocabulary;
    private readonly Dictionary<string, int> _vocabIndex;
    private const int MaxSequenceLength = 256;
    
    public OnnxEmbeddingService(string modelPath, string vocabPath)
    {
        _session = new InferenceSession(modelPath);
        _vocabulary = File.ReadAllLines(vocabPath);
        _vocabIndex = _vocabulary
            .Select((word, index) => (word, index))
            .ToDictionary(x => x.word, x => x.index);
    }
    
    public float[] GenerateEmbedding(string text)
    {
        var encoded = Tokenize(text);
        
        var inputIds = new DenseTensor<long>(
            encoded.InputIds.Select(x => (long)x).ToArray(),
            [1, encoded.InputIds.Length]);
        
        var attentionMask = new DenseTensor<long>(
            encoded.AttentionMask.Select(x => (long)x).ToArray(),
            [1, encoded.AttentionMask.Length]);
        
        var tokenTypeIds = new DenseTensor<long>(
            new long[encoded.InputIds.Length],
            [1, encoded.InputIds.Length]);
        
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
        };
        
        using var results = _session.Run(inputs);
        
        // Get the last_hidden_state output
        var output = results.First(r => r.Name == "last_hidden_state").AsTensor<float>();
        
        // Mean pooling over token dimension
        return MeanPool(output, encoded.AttentionMask);
    }
    
    public float[][] GenerateEmbeddings(IEnumerable<string> texts)
    {
        return texts.Select(GenerateEmbedding).ToArray();
    }
    
    public float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have same length");
        
        float dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        
        return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }
    
    private TokenizedInput Tokenize(string text)
    {
        // Simple WordPiece tokenization
        var tokens = new List<int> { GetTokenId("[CLS]") };
        
        var words = text.ToLowerInvariant()
            .Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            var wordTokens = TokenizeWord(word);
            
            if (tokens.Count + wordTokens.Count >= MaxSequenceLength - 1)
                break;
                
            tokens.AddRange(wordTokens);
        }
        
        tokens.Add(GetTokenId("[SEP]"));
        
        var attentionMask = Enumerable.Repeat(1, tokens.Count).ToArray();
        
        // Pad to MaxSequenceLength
        while (tokens.Count < MaxSequenceLength)
        {
            tokens.Add(0); // [PAD]
        }
        
        var mask = new int[MaxSequenceLength];
        Array.Copy(attentionMask, mask, attentionMask.Length);
        
        return new TokenizedInput
        {
            InputIds = tokens.Take(MaxSequenceLength).ToArray(),
            AttentionMask = mask
        };
    }
    
    private List<int> TokenizeWord(string word)
    {
        var tokens = new List<int>();
        
        // Try whole word first
        if (_vocabIndex.TryGetValue(word, out var id))
        {
            tokens.Add(id);
            return tokens;
        }
        
        // WordPiece: try to break into subwords
        var remaining = word;
        var isFirst = true;
        
        while (remaining.Length > 0)
        {
            var found = false;
            
            for (int end = remaining.Length; end > 0; end--)
            {
                var subword = remaining[..end];
                var lookup = isFirst ? subword : "##" + subword;
                
                if (_vocabIndex.TryGetValue(lookup, out var subId))
                {
                    tokens.Add(subId);
                    remaining = remaining[end..];
                    isFirst = false;
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                // Unknown token
                tokens.Add(GetTokenId("[UNK]"));
                break;
            }
        }
        
        return tokens;
    }
    
    private int GetTokenId(string token)
    {
        return _vocabIndex.TryGetValue(token, out var id) ? id : _vocabIndex["[UNK]"];
    }
    
    private float[] MeanPool(Tensor<float> embeddings, int[] attentionMask)
    {
        var seqLen = embeddings.Dimensions[1];
        var hiddenSize = embeddings.Dimensions[2];
        var result = new float[hiddenSize];
        
        var validTokens = attentionMask.Sum();
        
        for (int i = 0; i < seqLen; i++)
        {
            if (attentionMask[i] == 1)
            {
                for (int j = 0; j < hiddenSize; j++)
                {
                    result[j] += embeddings[0, i, j];
                }
            }
        }
        
        // Divide by valid token count
        for (int j = 0; j < hiddenSize; j++)
        {
            result[j] /= validTokens;
        }
        
        // L2 normalize
        var norm = MathF.Sqrt(result.Sum(x => x * x));
        for (int j = 0; j < hiddenSize; j++)
        {
            result[j] /= norm;
        }
        
        return result;
    }
    
    public void Dispose()
    {
        _session?.Dispose();
    }
    
    private class TokenizedInput
    {
        public required int[] InputIds { get; set; }
        public required int[] AttentionMask { get; set; }
    }
}
```

---

## API Endpoints

### Deke.Api/Endpoints/SearchEndpoints.cs

```csharp
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/search").WithTags("Search");
        
        group.MapGet("/", SearchFacts)
            .WithName("SearchFacts")
            .WithDescription("Search for facts using semantic similarity");
        
        group.MapGet("/context", GetContext)
            .WithName("GetContext")
            .WithDescription("Get relevant context for a topic, formatted for LLM consumption");
    }
    
    private static async Task<IResult> SearchFacts(
        string query,
        string domain,
        int limit = 10,
        float minSimilarity = 0.5f,
        IFactRepository factRepo,
        IEmbeddingService embeddings,
        CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        var embedding = embeddings.GenerateEmbedding(query);
        var results = await factRepo.SearchAsync(embedding, domain, limit, minSimilarity, ct);
        
        sw.Stop();
        
        return Results.Ok(new SearchResponse
        {
            Query = query,
            Domain = domain,
            Results = results,
            TotalCount = results.Count,
            SearchDuration = sw.Elapsed
        });
    }
    
    private static async Task<IResult> GetContext(
        string topic,
        string domain,
        int maxTokens = 2000,
        IFactRepository factRepo,
        IEmbeddingService embeddings,
        CancellationToken ct)
    {
        var embedding = embeddings.GenerateEmbedding(topic);
        var facts = await factRepo.SearchAsync(embedding, domain, limit: 30, minSimilarity: 0.4f, ct);
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"## Domain Knowledge: {domain}");
        sb.AppendLine($"### Topic: {topic}");
        sb.AppendLine();
        
        var tokenEstimate = 0;
        var factCount = 0;
        
        foreach (var fact in facts.OrderByDescending(f => f.Similarity))
        {
            var line = $"- {fact.Content}";
            var lineTokens = line.Length / 4; // rough estimate
            
            if (tokenEstimate + lineTokens > maxTokens)
                break;
            
            sb.AppendLine(line);
            tokenEstimate += lineTokens;
            factCount++;
        }
        
        return Results.Ok(new ContextResponse
        {
            Topic = topic,
            Domain = domain,
            Context = sb.ToString(),
            FactCount = factCount,
            ApproximateTokens = tokenEstimate
        });
    }
}
```

### Deke.Api/Endpoints/FactEndpoints.cs

```csharp
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class FactEndpoints
{
    public static void MapFactEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/facts").WithTags("Facts");
        
        group.MapGet("/{id:guid}", GetFact);
        group.MapGet("/domain/{domain}", GetFactsByDomain);
        group.MapPost("/", AddFact);
        group.MapGet("/stats/{domain}", GetStats);
    }
    
    private static async Task<IResult> GetFact(
        Guid id,
        IFactRepository repo,
        CancellationToken ct)
    {
        var fact = await repo.GetByIdAsync(id, ct);
        return fact is null ? Results.NotFound() : Results.Ok(fact);
    }
    
    private static async Task<IResult> GetFactsByDomain(
        string domain,
        int limit = 100,
        IFactRepository repo,
        CancellationToken ct)
    {
        var facts = await repo.GetByDomainAsync(domain, limit, ct);
        return Results.Ok(facts);
    }
    
    private static async Task<IResult> AddFact(
        AddFactRequest request,
        IFactRepository repo,
        IEmbeddingService embeddings,
        CancellationToken ct)
    {
        var embedding = embeddings.GenerateEmbedding(request.Content);
        
        var fact = new Fact
        {
            Content = request.Content,
            Domain = request.Domain,
            Embedding = embedding,
            Confidence = request.Confidence ?? 1.0f,
            SourceId = request.SourceId,
            Metadata = request.Metadata ?? []
        };
        
        var id = await repo.AddAsync(fact, ct);
        return Results.Created($"/api/facts/{id}", new { id });
    }
    
    private static async Task<IResult> GetStats(
        string domain,
        IFactRepository repo,
        CancellationToken ct)
    {
        var count = await repo.GetCountAsync(domain, ct);
        return Results.Ok(new { domain, factCount = count });
    }
}

public record AddFactRequest(
    string Content,
    string Domain,
    float? Confidence = null,
    Guid? SourceId = null,
    Dictionary<string, JsonElement>? Metadata = null
);
```

### Deke.Api/Endpoints/SourceEndpoints.cs

```csharp
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class SourceEndpoints
{
    public static void MapSourceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sources").WithTags("Sources");
        
        group.MapGet("/", GetSources);
        group.MapGet("/{id:guid}", GetSource);
        group.MapPost("/", AddSource);
        group.MapDelete("/{id:guid}", DeleteSource);
        group.MapPost("/{id:guid}/check", TriggerCheck);
    }
    
    private static async Task<IResult> GetSources(
        string? domain,
        ISourceRepository repo,
        CancellationToken ct)
    {
        var sources = domain is null 
            ? await repo.GetAllAsync(ct)
            : await repo.GetByDomainAsync(domain, ct);
        return Results.Ok(sources);
    }
    
    private static async Task<IResult> GetSource(
        Guid id,
        ISourceRepository repo,
        CancellationToken ct)
    {
        var source = await repo.GetByIdAsync(id, ct);
        return source is null ? Results.NotFound() : Results.Ok(source);
    }
    
    private static async Task<IResult> AddSource(
        AddSourceRequest request,
        ISourceRepository repo,
        CancellationToken ct)
    {
        var source = new Source
        {
            Url = request.Url,
            Domain = request.Domain,
            Name = request.Name,
            Type = request.Type ?? SourceType.WebPage,
            CheckInterval = request.CheckInterval ?? TimeSpan.FromDays(1),
            Credibility = request.Credibility ?? 0.5f
        };
        
        var id = await repo.AddAsync(source, ct);
        return Results.Created($"/api/sources/{id}", new { id });
    }
    
    private static async Task<IResult> DeleteSource(
        Guid id,
        ISourceRepository repo,
        CancellationToken ct)
    {
        await repo.DeleteAsync(id, ct);
        return Results.NoContent();
    }
    
    private static async Task<IResult> TriggerCheck(
        Guid id,
        ISourceRepository sourceRepo,
        IServiceProvider services,
        CancellationToken ct)
    {
        var source = await sourceRepo.GetByIdAsync(id, ct);
        if (source is null) return Results.NotFound();
        
        // Trigger immediate check
        // In production, this would queue a background job
        return Results.Accepted(new { message = "Check queued", sourceId = id });
    }
}

public record AddSourceRequest(
    string Url,
    string Domain,
    string? Name = null,
    SourceType? Type = null,
    TimeSpan? CheckInterval = null,
    float? Credibility = null
);
```

---

## MCP Server

### Deke.Mcp/Tools/SearchTools.cs

```csharp
using Deke.Core.Interfaces;
using Deke.Core.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Deke.Mcp.Tools;

[McpTool("search_knowledge")]
[Description("Search for facts in a domain using semantic similarity")]
public class SearchKnowledgeTool
{
    private readonly IFactRepository _factRepo;
    private readonly IEmbeddingService _embeddings;
    
    public SearchKnowledgeTool(IFactRepository factRepo, IEmbeddingService embeddings)
    {
        _factRepo = factRepo;
        _embeddings = embeddings;
    }
    
    public async Task<SearchResponse> ExecuteAsync(
        [Description("The search query")] string query,
        [Description("Knowledge domain to search in")] string domain,
        [Description("Maximum number of results (default: 10)")] int limit = 10,
        [Description("Minimum similarity threshold 0-1 (default: 0.5)")] float minSimilarity = 0.5f,
        CancellationToken ct = default)
    {
        var embedding = _embeddings.GenerateEmbedding(query);
        var results = await _factRepo.SearchAsync(embedding, domain, limit, minSimilarity, ct);
        
        return new SearchResponse
        {
            Query = query,
            Domain = domain,
            Results = results,
            TotalCount = results.Count
        };
    }
}

[McpTool("get_context")]
[Description("Get relevant knowledge context for a topic, formatted for LLM consumption")]
public class GetContextTool
{
    private readonly IFactRepository _factRepo;
    private readonly IEmbeddingService _embeddings;
    
    public GetContextTool(IFactRepository factRepo, IEmbeddingService embeddings)
    {
        _factRepo = factRepo;
        _embeddings = embeddings;
    }
    
    public async Task<ContextResponse> ExecuteAsync(
        [Description("Topic to get context for")] string topic,
        [Description("Knowledge domain")] string domain,
        [Description("Approximate maximum tokens in response (default: 2000)")] int maxTokens = 2000,
        CancellationToken ct = default)
    {
        var embedding = _embeddings.GenerateEmbedding(topic);
        var facts = await _factRepo.SearchAsync(embedding, domain, limit: 30, minSimilarity: 0.4f, ct);
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"## Domain Knowledge: {domain}");
        sb.AppendLine($"### Topic: {topic}");
        sb.AppendLine();
        
        var tokenEstimate = 0;
        var factCount = 0;
        
        foreach (var fact in facts.OrderByDescending(f => f.Similarity))
        {
            var line = $"- {fact.Content} (confidence: {fact.Confidence:P0})";
            var lineTokens = line.Length / 4;
            
            if (tokenEstimate + lineTokens > maxTokens) break;
            
            sb.AppendLine(line);
            tokenEstimate += lineTokens;
            factCount++;
        }
        
        return new ContextResponse
        {
            Topic = topic,
            Domain = domain,
            Context = sb.ToString(),
            FactCount = factCount,
            ApproximateTokens = tokenEstimate
        };
    }
}

[McpTool("list_domains")]
[Description("List all available knowledge domains")]
public class ListDomainsTool
{
    private readonly IFactRepository _factRepo;
    
    public ListDomainsTool(IFactRepository factRepo)
    {
        _factRepo = factRepo;
    }
    
    public async Task<DomainsResponse> ExecuteAsync(CancellationToken ct = default)
    {
        // This would need a method to get distinct domains
        // For now, return placeholder
        return new DomainsResponse
        {
            Domains = ["fishing", "technology"] // Replace with actual query
        };
    }
}

public class DomainsResponse
{
    public List<string> Domains { get; set; } = [];
}
```

### Deke.Mcp/Tools/FactTools.cs

```csharp
using Deke.Core.Interfaces;
using Deke.Core.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Deke.Mcp.Tools;

[McpTool("add_fact")]
[Description("Add a new fact to the knowledge base")]
public class AddFactTool
{
    private readonly IFactRepository _factRepo;
    private readonly IEmbeddingService _embeddings;
    
    public AddFactTool(IFactRepository factRepo, IEmbeddingService embeddings)
    {
        _factRepo = factRepo;
        _embeddings = embeddings;
    }
    
    public async Task<AddFactResponse> ExecuteAsync(
        [Description("The fact content to store")] string content,
        [Description("Knowledge domain this fact belongs to")] string domain,
        [Description("Confidence level 0-1 (default: 1.0)")] float confidence = 1.0f,
        CancellationToken ct = default)
    {
        var embedding = _embeddings.GenerateEmbedding(content);
        
        var fact = new Fact
        {
            Content = content,
            Domain = domain,
            Embedding = embedding,
            Confidence = confidence
        };
        
        var id = await _factRepo.AddAsync(fact, ct);
        
        return new AddFactResponse
        {
            Id = id,
            Message = $"Fact added to domain '{domain}'"
        };
    }
}

[McpTool("get_fact")]
[Description("Get a specific fact by ID")]
public class GetFactTool
{
    private readonly IFactRepository _factRepo;
    
    public GetFactTool(IFactRepository factRepo)
    {
        _factRepo = factRepo;
    }
    
    public async Task<Fact?> ExecuteAsync(
        [Description("The fact ID")] Guid id,
        CancellationToken ct = default)
    {
        return await _factRepo.GetByIdAsync(id, ct);
    }
}

[McpTool("get_domain_stats")]
[Description("Get statistics for a knowledge domain")]
public class GetDomainStatsTool
{
    private readonly IFactRepository _factRepo;
    
    public GetDomainStatsTool(IFactRepository factRepo)
    {
        _factRepo = factRepo;
    }
    
    public async Task<DomainStats> ExecuteAsync(
        [Description("Knowledge domain")] string domain,
        CancellationToken ct = default)
    {
        var count = await _factRepo.GetCountAsync(domain, ct);
        
        return new DomainStats
        {
            Domain = domain,
            FactCount = count
        };
    }
}

public class AddFactResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; } = "";
}

public class DomainStats
{
    public string Domain { get; set; } = "";
    public int FactCount { get; set; }
}
```

### Deke.Mcp/Program.cs

```csharp
using Deke.Core.Interfaces;
using Deke.Infrastructure;
using Deke.Infrastructure.Embeddings;
using Deke.Mcp.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("Deke")
    ?? "Host=localhost;Database=deke;Username=deke;Password=deke";
var modelPath = builder.Configuration["Embeddings:ModelPath"]
    ?? "models/all-MiniLM-L6-v2/model.onnx";
var vocabPath = builder.Configuration["Embeddings:VocabPath"]
    ?? "models/all-MiniLM-L6-v2/vocab.txt";

// Services — uses Dapper + DbConnectionFactory, registers all repositories
builder.Services.AddDekeInfrastructure(connectionString);

builder.Services.AddSingleton<IEmbeddingService>(sp =>
    new OnnxEmbeddingService(modelPath, vocabPath));

// MCP Server
builder.Services.AddMcpServer()
    .WithStdioTransport()
    .WithTools<SearchKnowledgeTool>()
    .WithTools<GetContextTool>()
    .WithTools<AddFactTool>()
    .WithTools<GetFactTool>()
    .WithTools<GetDomainStatsTool>()
    .WithTools<ListDomainsTool>();

var app = builder.Build();
await app.RunAsync();
```

---

## Background Worker

### Deke.Worker/Services/SourceMonitorService.cs

```csharp
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Worker.Services;

public class SourceMonitorService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SourceMonitorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15);
    
    public SourceMonitorService(IServiceProvider services, ILogger<SourceMonitorService> logger)
    {
        _services = services;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Source Monitor Service starting");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckSourcesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in source monitoring cycle");
            }
            
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
    
    private async Task CheckSourcesAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var sourceRepo = scope.ServiceProvider.GetRequiredService<ISourceRepository>();
        var factRepo = scope.ServiceProvider.GetRequiredService<IFactRepository>();
        var harvesters = scope.ServiceProvider.GetRequiredService<IEnumerable<IHarvester>>();
        var embeddings = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var extraction = scope.ServiceProvider.GetRequiredService<IExtractionService>();
        
        var dueSources = await sourceRepo.GetDueForCheckAsync(ct);
        _logger.LogInformation("Checking {Count} sources due for update", dueSources.Count);
        
        foreach (var source in dueSources)
        {
            try
            {
                var harvester = harvesters.FirstOrDefault(h => h.SupportedType == source.Type);
                if (harvester is null)
                {
                    _logger.LogWarning("No harvester for source type {Type}", source.Type);
                    continue;
                }
                
                var result = await harvester.HarvestAsync(source, ct);
                
                source.LastCheckedAt = DateTimeOffset.UtcNow;
                
                if (result.HasChanges)
                {
                    source.LastChangedAt = DateTimeOffset.UtcNow;
                    source.ContentHash = result.NewContentHash;
                    
                    // Extract facts from new content
                    foreach (var text in result.ExtractedTexts)
                    {
                        var extracted = await extraction.ExtractFactsAsync(text, source.Domain, source.Name, ct);
                        
                        foreach (var ef in extracted)
                        {
                            var embedding = embeddings.GenerateEmbedding(ef.Content);
                            var fact = new Fact
                            {
                                Content = ef.Content,
                                Domain = source.Domain,
                                Embedding = embedding,
                                Confidence = ef.Confidence * source.Credibility,
                                SourceId = source.Id,
                                Entities = ef.Entities
                            };
                            
                            await factRepo.AddAsync(fact, ct);
                        }
                    }
                    
                    _logger.LogInformation("Source {Name} updated, extracted {Count} texts", 
                        source.Name ?? source.Url, result.ExtractedTexts.Count);
                }
                
                await sourceRepo.UpdateAsync(source, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking source {Url}", source.Url);
            }
        }
    }
}
```

---

## Podman Compose

### podman-compose.yml

```yaml
version: '3.8'

services:
  postgres:
    image: pgvector/pgvector:pg16
    container_name: deke-postgres
    environment:
      POSTGRES_USER: deke
      POSTGRES_PASSWORD: deke
      POSTGRES_DB: deke
    ports:
      - "5432:5432"
    volumes:
      - deke-postgres-data:/var/lib/postgresql/data
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U deke -d deke"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  deke-postgres-data:
```

### init.sql

```sql
-- This runs automatically when the container first starts
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

---

## Configuration

### Deke.Api/appsettings.json

```json
{
  "ConnectionStrings": {
    "Deke": "Host=localhost;Database=deke;Username=deke;Password=deke"
  },
  "Embeddings": {
    "ModelPath": "models/all-MiniLM-L6-v2/model.onnx",
    "VocabPath": "models/all-MiniLM-L6-v2/vocab.txt"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## Getting Started (Implementation Order)

### Phase 1: Foundation — DONE

1. Create solution and project structure
2. Set up Podman Compose for PostgreSQL with pgvector (Docker Compose also works)
3. Implement Core models and interfaces (all 6 entity types)
4. Implement `DbConnectionFactory` + `DapperConfig` (replaces EF Core)
5. Schema managed via `init.sql` (runs on first container start)

### Phase 3: Storage — DONE

1. All 6 repositories implemented using raw Dapper SQL (not FastCrud) due to
   special column types: JSONB, vector, UUID[], INTERVAL, enum-as-varchar
2. Custom type handlers: `JsonbTypeHandler<T>`, `GuidArrayTypeHandler`,
   `EnumTypeHandler<T>` (for SourceType/PatternType stored as varchar)
3. `AddDekeInfrastructure()` extension registers all repositories via DI
4. Integration tests needed: repository tests against real PostgreSQL
   (Testcontainers or Podman/Docker Compose) covering vector similarity search,
   JSONB handling, and UUID array operations

### Phase 2: Embeddings — DONE

1. Create `download-model.ps1` / `download-model.sh` scripts for automated
   model download (referenced by README but not yet implemented)
2. Download ONNX model and vocab
3. Implement `OnnxEmbeddingService` with L2 normalization
4. Write unit tests for embedding generation
5. Test cosine similarity calculation

> **Critical-path blocker**: All search functionality (API, MCP, Worker)
> depends on embeddings being operational.

### Phase 4: API — DONE

1. Implement minimal API endpoints
2. Test search endpoint (requires working embeddings)
3. Test add fact endpoint
4. Add OpenAPI/Swagger

### Phase 5: MCP Server — DONE

1. Implement MCP tools
2. Test with Claude Desktop
3. Configure for claude.ai or Claude Code

### Phase 6: Background Services — DONE

1. Build RSS harvester end-to-end first (ingest → extract → embed → store)
   as the reference implementation for the harvest pipeline
2. Implement WebPageHarvester
3. Implement SourceMonitorService
4. Implement basic extraction service
5. Test end-to-end source monitoring

### Phase 7: Learning — DONE

1. Implement pattern discovery
2. Implement relation mapping
3. Add learning cycle service
4. Add confidence decay — reduce pattern confidence over time when not
   revalidated by new evidence

---

## Commands

```bash
# Create solution
dotnet new sln -n DEKE

# Create projects
dotnet new classlib -n Deke.Core -o src/Deke.Core
dotnet new classlib -n Deke.Infrastructure -o src/Deke.Infrastructure
dotnet new webapi -n Deke.Api -o src/Deke.Api
dotnet new console -n Deke.Mcp -o src/Deke.Mcp
dotnet new worker -n Deke.Worker -o src/Deke.Worker
dotnet new xunit -n Deke.Tests -o tests/Deke.Tests

# Add projects to solution
dotnet sln add src/Deke.Core
dotnet sln add src/Deke.Infrastructure
dotnet sln add src/Deke.Api
dotnet sln add src/Deke.Mcp
dotnet sln add src/Deke.Worker
dotnet sln add tests/Deke.Tests

# Add project references
dotnet add src/Deke.Infrastructure reference src/Deke.Core
dotnet add src/Deke.Api reference src/Deke.Core src/Deke.Infrastructure
dotnet add src/Deke.Mcp reference src/Deke.Core src/Deke.Infrastructure
dotnet add src/Deke.Worker reference src/Deke.Core src/Deke.Infrastructure
dotnet add tests/Deke.Tests reference src/Deke.Core src/Deke.Infrastructure

# Start PostgreSQL (or use docker-compose, or install PostgreSQL locally)
podman-compose up -d

# Run API
dotnet run --project src/Deke.Api

# Run MCP Server
dotnet run --project src/Deke.Mcp
```

---

## Model Download Instructions

```bash
# Create models directory
mkdir -p models/all-MiniLM-L6-v2
cd models/all-MiniLM-L6-v2

# Download from Hugging Face
# Option 1: Using wget
wget https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx
wget https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt

# Option 2: Using curl
curl -L -o model.onnx https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx
curl -L -o vocab.txt https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt
```

---

## References

- [pgvector](https://github.com/pgvector/pgvector) - Vector similarity search for PostgreSQL
- [Pgvector .NET](https://github.com/pgvector/pgvector-dotnet) - .NET client for pgvector
- [ONNX Runtime](https://onnxruntime.ai/) - High performance ML inference
- [all-MiniLM-L6-v2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2) - Sentence embedding model
- [ModelContextProtocol](https://github.com/modelcontextprotocol) - MCP SDK

---

## Decision Log

This log tracks significant decisions and specification changes throughout the project lifecycle. Each entry records what changed and the reasoning behind it. Git handles version control; this log serves as a decision journal.

| Version | Date | Summary |
|---------|------|---------|
| 1.0.0 | 2026-03 | Initial specification created. Defines full architecture: Core models and interfaces, Infrastructure (Dapper + pgvector + ONNX embeddings + harvesters), REST API, MCP server, background worker, and PostgreSQL schema. |
| 1.1.0 | 2026-03-11 | **Data access: EF Core replaced with Dapper + Dapper.FastCrud.** Original spec listed EF Core, but Dapper was chosen during implementation for better control over raw SQL vector operations (pgvector `<=>` operator), simpler mapping with `Pgvector.Dapper`, and lower overhead for a read-heavy semantic search workload. FastCrud handles CRUD generation. DbConnectionFactory wraps NpgsqlDataSource for connection pooling. |
| 1.1.1 | 2026-03-11 | **Review fixes.** Added missing `FactRelation` and `LearningLog` models to Core. Converted DTOs to records per style guide. Moved `ExtractedFact`/`HarvestResult` from interface files to Models/. Changed `Dictionary<string, object>` to `Dictionary<string, JsonElement>` for type-safe JSON round-trips. Made `DapperConfig.Initialize()` thread-safe. Extracted shared DI setup to `AddDekeInfrastructure()` extension. Moved connection strings to `appsettings.Development.json` only. Added `Directory.Build.props` for shared project properties. Pinned NuGet package versions. Completed `init.sql` with all 6 table definitions and indexes. |
| 1.2.0 | 2026-03-11 | **Phase 1 repositories complete. Plan merged with product review.** All 6 repositories implemented using raw Dapper SQL (not FastCrud) due to special column types (JSONB, vector, UUID[], INTERVAL, enum-as-varchar). Added `EnumTypeHandler<T>` for SourceType/PatternType. Renamed `Pattern.Type` to `Pattern.PatternType` for DB column mapping. Phase plan updated with status tracking and product review recommendations (model download automation, integration tests, confidence decay). |
| 1.3.0 | 2026-03-11 | **Phase 2: Embeddings Implementation complete.** `OnnxEmbeddingService` implements `IEmbeddingService` with manual WordPiece tokenizer — no BERTTokenizers NuGet dependency needed. `EmbeddingsConfig` is a simple POCO (not `IOptions<T>`). Separate DI extension `AddDekeEmbeddings(IConfiguration)` keeps embedding concerns isolated from `AddDekeInfrastructure`. `InferenceSession` registered as singleton (thread-safe for `Run()`). Added PowerShell download script (`download-model.ps1`) alongside existing bash script. |
| 1.4.0 | 2026-03-11 | **Phases 4-7 complete.** API endpoints implemented (search, facts, sources). MCP server uses `Host.CreateApplicationBuilder` with DI (consistent with Api/Worker), MCP SDK v1.1.0 uses `[McpServerToolType]`/`[McpServerTool]` attributes with method-level DI parameter injection. Background services: `RssHarvester` (`SyndicationFeed`), `WebPageHarvester` (AngleSharp), `SimpleExtractionService` (heuristic sentence splitting — no LLM dependency). Learning: `PatternDiscoveryService` (embedding similarity clustering), `LearningCycleService` (relation mapping). Pluggable `ILlmService` interface added with `NoOpLlmService` default — ready for Ollama/Claude integration later. `ExistsAsync` added to `IFactRelationRepository`. |
| 1.5.0 | 2026-03-12 | **Post-review hardening.** Broad pass fixing bugs, adding security, and improving operational readiness. **Bug fixes**: Worker services were silently no-ops — `GetRecentAsync` and `GetWithoutRelationsAsync` queries omitted the embedding column, so `PatternDiscoveryService` and `LearningCycleService` received null embeddings and skipped all facts. MCP `SearchTools` confidence formatting used `{confidence:P0}` (producing "1%" for 0.7) instead of `{confidence * 100:F0}%`. **Security**: Input validation and SSRF protection added to all API endpoints — URL scheme restricted to http/https, private/loopback IP ranges blocked, limit parameters clamped to safe ranges, empty string inputs rejected. `RssHarvester` XML parsing hardened (`DtdProcessing.Prohibit`, `XmlResolver = null`) to prevent XXE attacks. API key authentication added on write endpoints (POST, DELETE) via `X-Api-Key` header with constant-time comparison. **Reliability**: Background services now catch `OperationCanceledException` cleanly during shutdown instead of logging spurious errors. `PatternDiscoveryService` moved `GetActiveByDomainAsync` call outside the cluster loop to eliminate redundant DB queries per iteration. `NpgsqlDataSource` registered via factory in DI for proper disposal on shutdown. **Project infrastructure**: MIT LICENSE file added (was claimed in README but missing from repo). `.editorconfig` added for code style enforcement. CI/CD pipeline added via GitHub Actions (build + test on push/PR to main). README expanded with prerequisites, worked examples, and status badge. CONTRIBUTING.md added with development workflow and conventions. |
| 1.5.1 | 2026-03-12 | **Container tooling: Podman preferred, Docker alternative.** Renamed `docker-compose.yml` to `podman-compose.yml`. Podman is now the documented preferred container tool; Docker Compose remains a supported alternative. PostgreSQL can also be installed locally without any container tool. The compose file contents (pgvector image, volumes, healthcheck) are unchanged. |

---

**Created**: March 2026
**Purpose**: Complete implementation specification for Claude Code
