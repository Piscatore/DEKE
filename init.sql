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
    source_tier VARCHAR(20) NOT NULL DEFAULT 'Unverified',
    independence_fingerprint VARCHAR(64),
    last_verified_at TIMESTAMPTZ,
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
    outdated_reason VARCHAR(200),
    valid_from TIMESTAMPTZ,
    valid_until TIMESTAMPTZ,
    corroboration_count INT NOT NULL DEFAULT 0,
    last_verified_at TIMESTAMPTZ,
    contradiction_flag BOOLEAN NOT NULL DEFAULT FALSE,
    trust_state VARCHAR(20) NOT NULL DEFAULT 'Unscored',
    -- R2 deduplication (levels 1-5)
    content_hash VARCHAR(64),
    normalized_hash VARCHAR(64),
    similarity_hash BIGINT,
    duplicate_of UUID REFERENCES facts(id) ON DELETE SET NULL
);

CREATE INDEX idx_facts_embedding ON facts
USING ivfflat (embedding vector_cosine_ops) WITH (lists = 100);

CREATE INDEX idx_facts_domain ON facts(domain);
CREATE INDEX idx_facts_source ON facts(source_id);
CREATE INDEX idx_facts_created ON facts(created_at DESC);
CREATE INDEX idx_facts_active ON facts(domain, is_outdated) WHERE is_outdated = FALSE;

-- R2 dedup: exact-match lookup (levels 2-3) and per-domain uniqueness guard
CREATE INDEX idx_facts_content_hash ON facts(content_hash);
CREATE UNIQUE INDEX idx_facts_domain_normhash ON facts(domain, normalized_hash);
-- R2 dedup: async level-4 candidate scan (facts without a similarity hash yet)
CREATE INDEX idx_facts_pending_simhash ON facts(id) WHERE similarity_hash IS NULL;
-- P1-2 quality pipeline: review-queue selection and pending-evaluation/contradiction scans
CREATE INDEX idx_facts_trust_state ON facts(domain, trust_state) WHERE trust_state <> 'Accepted';

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

-- Interaction logs: Per-query search/context interaction capture
CREATE TABLE interaction_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    domain VARCHAR(100),
    query TEXT NOT NULL,
    model VARCHAR(200),
    returned_fact_ids UUID[] DEFAULT '{}',
    scores REAL[] DEFAULT '{}',
    min_similarity REAL,
    result_count INT NOT NULL DEFAULT 0,
    duration_ms INT NOT NULL DEFAULT 0,
    federation JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_interaction_logs_created ON interaction_logs(created_at DESC);
CREATE INDEX idx_interaction_logs_domain ON interaction_logs(domain, created_at DESC);

-- Advisory interactions: Append-only audit record of advisory pipeline responses
CREATE TABLE advisory_interactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    domain VARCHAR(100),
    query TEXT NOT NULL,
    stakes VARCHAR(20) NOT NULL DEFAULT 'Low',
    model VARCHAR(200) NOT NULL,
    cited_fact_ids UUID[] DEFAULT '{}',
    fact_confidences REAL[] DEFAULT '{}',
    confidence_band VARCHAR(20) NOT NULL,
    knowledge_gaps JSONB NOT NULL DEFAULT '[]',
    raw_output TEXT,
    contains_conflicting BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_advisory_interactions_created ON advisory_interactions(created_at DESC);
CREATE INDEX idx_advisory_interactions_domain ON advisory_interactions(domain, created_at DESC);

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

-- Fact provenance: one-to-many link between facts and the sources that assert them
CREATE TABLE fact_provenance (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    fact_id UUID NOT NULL REFERENCES facts(id) ON DELETE CASCADE,
    source_id UUID NOT NULL REFERENCES sources(id) ON DELETE CASCADE,
    extracted_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    extraction_method VARCHAR(30) NOT NULL,
    extraction_confidence REAL NOT NULL DEFAULT 1.0,
    CONSTRAINT fact_provenance_unique UNIQUE (fact_id, source_id)
);

CREATE INDEX idx_fact_provenance_fact ON fact_provenance(fact_id);
CREATE INDEX idx_fact_provenance_source ON fact_provenance(source_id);

-- Fact version: immutable change history, "what did we believe on date X"
CREATE TABLE fact_version (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    fact_id UUID NOT NULL REFERENCES facts(id) ON DELETE CASCADE,
    content_snapshot TEXT NOT NULL,
    embedding_snapshot vector(384),
    changed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    change_reason VARCHAR(30) NOT NULL
);

CREATE INDEX idx_fact_version_fact ON fact_version(fact_id, changed_at DESC);

-- Domain trust policy: per-domain configuration governing provenance strictness
CREATE TABLE domain_trust_policy (
    domain VARCHAR(100) PRIMARY KEY,
    require_primary_source BOOLEAN NOT NULL DEFAULT FALSE,
    min_corroboration INT NOT NULL DEFAULT 0,
    auto_accept_tiers JSONB NOT NULL DEFAULT '[]',
    flag_for_review_tiers JSONB NOT NULL DEFAULT '[]',
    temporal_validity_required BOOLEAN NOT NULL DEFAULT FALSE,
    min_confidence_score REAL NOT NULL DEFAULT 0.0
);
