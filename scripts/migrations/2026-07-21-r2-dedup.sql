-- R2 five-level deduplication schema.
-- Idempotent upgrade for the live container (init.sql covers fresh installs).
-- Additive only: nullable columns + indexes. Safe to re-run.

ALTER TABLE facts ADD COLUMN IF NOT EXISTS content_hash VARCHAR(64);
ALTER TABLE facts ADD COLUMN IF NOT EXISTS normalized_hash VARCHAR(64);
ALTER TABLE facts ADD COLUMN IF NOT EXISTS similarity_hash BIGINT;
ALTER TABLE facts ADD COLUMN IF NOT EXISTS duplicate_of UUID REFERENCES facts(id) ON DELETE SET NULL;

-- Exact-match lookup (levels 2-3) and per-domain uniqueness guard.
-- NULL normalized_hash rows (legacy / pre-backfill) are exempt: Postgres
-- treats NULLs as distinct, so multiple NULLs coexist under the unique index.
CREATE INDEX IF NOT EXISTS idx_facts_content_hash ON facts(content_hash);
CREATE UNIQUE INDEX IF NOT EXISTS idx_facts_domain_normhash ON facts(domain, normalized_hash);

-- Async level-4 candidate scan (facts without a similarity hash yet).
CREATE INDEX IF NOT EXISTS idx_facts_pending_simhash ON facts(id) WHERE similarity_hash IS NULL;

-- rollback:
--   DROP INDEX IF EXISTS idx_facts_pending_simhash;
--   DROP INDEX IF EXISTS idx_facts_domain_normhash;
--   DROP INDEX IF EXISTS idx_facts_content_hash;
--   ALTER TABLE facts DROP COLUMN IF EXISTS duplicate_of;
--   ALTER TABLE facts DROP COLUMN IF EXISTS similarity_hash;
--   ALTER TABLE facts DROP COLUMN IF EXISTS normalized_hash;
--   ALTER TABLE facts DROP COLUMN IF EXISTS content_hash;
