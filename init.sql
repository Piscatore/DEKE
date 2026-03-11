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
