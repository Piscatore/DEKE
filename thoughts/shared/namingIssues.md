1. “Package 3” has three incompatible framings
	•	docs/product/overview.md defines a “Two-Package Architecture” (P1 Knowledge Base, P2 Knowledge Leverage) — Package 3 isn’t mentioned at all, and Federation is explicitly called “cross-cutting,” not a package.
	•	docs/science/evolution-vision.md calls the same subsystem the “Evolution Engine” and states outright: “It is not part of DEKE’s active product model.” It never uses the word “Package.”
	•	docs/architecture/decisions.md still refers to it as “Package 3” throughout (P3-1, P3-5, guardrails G1/G2/G3, GEPA mapping, review schedule) as if it were a numbered package on par with P1/P2 — including a 2026-04 entry that says it was “deferred to research status,” yet the rest of the same document keeps assigning it active phase numbers and design targets.
Net effect: three docs disagree on whether this thing is a package, a deferred package, or not a package/product concept at all.
2. “Phase” is overloaded with no disambiguating convention
Four independent numbering tracks all just say “Phase N,” and even the shorthand for the same track is inconsistent:
	•	Federation: Phase 1–5 (federation.md — clean, has one canonical table).
	•	Retrieval Pipeline: Phase R1–R8 (retrieval-pipeline.md — prefixed, no collision).
	•	Package 2: P2-1 … P2-4 (roadmap.md, decisions.md — consistent).
	•	Package 1: written as Package 1 Phase 1 (decisions.md:27, retrieval-pipeline.md:130), P1-Phase1 (specification.md:378), P1-Phase 2 (decisions.md:260, 276), and P1-3 (decisions.md:165) — four different shorthand styles for the same concept, sometimes in the same file.
3. Package 1’s phase roadmap has no canonical source and a gap
Package 1 phases are referenced piecemeal with no table defining them (unlike Federation’s clean Phase 1–5 table):
	•	Phase 1 = provenance schema (specification.md:202, decisions.md:27)
	•	Phase 2 = quality pipeline (decisions.md:310)
	•	Phase 3 = terminology database (retrieval-pipeline.md:144)
	•	Phase 5 = multilingual model swap (retrieval-pipeline.md:190, “Phase 5 of Package 1”)
	•	Phase 4 is never defined anywhere.
4. LLM backend docs vs. actual code — real drift, not just naming
specification.md‘s tech stack and “LLM Selection Policy” table, and knowledge-leverage.md, describe Anthropic API (claude-haiku-4-5 / claude-sonnet-4-6) as the model backend and Ollama as the local option. The actual code (src/Deke.Infrastructure/Llm/) implements only GeminiLlmService, OpenAiLlmService, and NoOpLlmService — LlmProvider enum is { None, Gemini, OpenAi }. There is no Anthropic or Ollama code anywhere in src/. This is the most significant finding — the documented “default backend” doesn’t exist in the codebase at all.
5. Chunking: naming and status both wrong in docs
retrieval-pipeline.md‘s Phase R1 deliverables say build “IChunkingService interface in Deke.Core” and “SemanticChunkingService implementation in Deke.Infrastructure,” and its pipeline table lists Chunk stage as Current: (none). Actual code already has this built and wired into DI (ServiceCollectionExtensions.cs:64) and used in two Worker services — but named IChunker / SemanticChunkerAdapter, not what the doc says. Phase R1 reads as not-yet-started when it’s actually done, under different names.
6. Federation ranking formula: doc example doesn’t match the code
federation.md’s “Result Ranking” section documents final_score = similarity * locality_weight with a worked example using only those two factors. The actual implementation, TrustScoringService.Score() (used directly by FederatedSearchService), computes similarity * confidence * credibility * recencyDecay * localityWeight — three additional factors the doc never mentions.
7. Top-level docs never adopted the package model
README.md and CLAUDE.md’s architecture diagram both still describe DEKE as a three-stage Ingest → Learn → Serve pipeline. Nothing in either file mentions “packages,” “Knowledge Base,” or “Knowledge Leverage” — the terminology introduced in docs/product/overview.md. A newcomer reading the README gets a different mental model than one reading the product docs, with no cross-reference reconciling the two framings.
8. Dangling reference
specification.md‘s project-structure tree (line 37) lists a root SPECIFICATION.md file annotated “(deprecated — content moved here).” That file does not exist anywhere in the repo — it’s a leftover reference from a past migration.
9. Minor gap
roadmap.md‘s “Current State” table says Federation (Phase 1--2) is done, but federation.md separately marks Phase 3 (MCP Tools) Complete. The capability is listed in roadmap’s MCP Tools row, but the “Phase 3” label itself is dropped from the summary — small, but adds to the inconsistent phase-labeling pattern above.