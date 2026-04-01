-- DEKE Database Initialization
-- This runs automatically when the PostgreSQL container first starts

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Verify extensions are loaded
DO $$
BEGIN
    RAISE NOTICE 'pgvector extension loaded: %', (SELECT extversion FROM pg_extension WHERE extname = 'vector');
    RAISE NOTICE 'uuid-ossp extension loaded: %', (SELECT extversion FROM pg_extension WHERE extname = 'uuid-ossp');
END $$;

-- Sources: Where facts come from
CREATE TABLE sources (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    url TEXT NOT NULL,
    domain VARCHAR(100) NOT NULL,
    name VARCHAR(200),
    type VARCHAR(20) NOT NULL DEFAULT 'WebPage',
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
    entities JSONB DEFAULT '[]',
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_outdated BOOLEAN NOT NULL DEFAULT FALSE,
    outdated_reason VARCHAR(200)
);

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
    contexts JSONB NOT NULL DEFAULT '[]',
    translations JSONB NOT NULL DEFAULT '{}',
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
    pattern_type VARCHAR(50) NOT NULL DEFAULT 'observation',
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
    relation_type VARCHAR(50) NOT NULL,
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
    cycle_type VARCHAR(50) NOT NULL,
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

-- Federation peers: Known DEKE instances for cross-domain queries
CREATE TABLE federation_peers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    instance_id VARCHAR(100) NOT NULL UNIQUE,
    base_url TEXT NOT NULL,
    domains JSONB NOT NULL DEFAULT '[]',
    capabilities JSONB NOT NULL DEFAULT '[]',
    protocol_version VARCHAR(10) NOT NULL DEFAULT '1',
    last_seen_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_healthy BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_federation_peers_healthy ON federation_peers(is_healthy) WHERE is_healthy = TRUE;
