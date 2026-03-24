# Research Codebase

Conduct deep research on the DEKE codebase to understand how to implement a
requested feature or fix. This is the first step of the RPI framework.

## Process

### 1. Understand the Request
Read the user's description of what they want to build or fix.
Identify which DEKE subsystems are involved:
- **Core models/interfaces**: New domain types or interface changes
- **Infrastructure/Repositories**: New data access or query changes
- **Infrastructure/Embeddings**: Embedding pipeline changes
- **Infrastructure/Harvesters**: New or modified content harvesters
- **Api/Endpoints**: New or modified REST endpoints
- **Mcp/Tools**: New or modified MCP tools for Claude integration
- **Worker/Services**: New or modified background services
- **Database schema**: Changes to `init.sql`

### 2. Launch Parallel Research Agents
Use the Agent tool to run these concurrently:

**Agent 1 — Codebase Locator** (`.claude/agents/codebase-locator.md`):
Find all files related to the feature area. Map which files exist, which
need modification, and where new files should be placed.

**Agent 2 — Codebase Analyzer** (`.claude/agents/codebase-analyzer.md`):
Trace the data flow for similar existing features. Understand how the
current code handles analogous operations end-to-end.

**Agent 3 — Pattern Finder** (`.claude/agents/codebase-pattern-finder.md`):
Find established patterns for the type of code being added. Collect
templates and conventions that the new code must follow.

### 3. Synthesize Research
Combine findings into a research document covering:
- **Affected files**: Every file that needs creation or modification
- **Dependency chain**: How the change flows through Core → Infrastructure → Hosts
- **Existing patterns**: Code templates to follow (with file:line references)
- **Database impact**: Schema changes needed in `init.sql`
- **DI wiring**: Services to register and where
- **Test coverage**: What tests exist for similar features
- **Risks**: Breaking changes, migration concerns, performance implications

### 4. Save Research
Write the research document to:
```
thoughts/shared/research/{feature-slug}-research.md
```

Include:
- Timestamp and feature description
- All file references with line numbers
- Code snippets showing existing patterns to follow
- Clear list of unknowns or decisions needed

## Output
Confirm research is complete and summarize the key findings.
Ask the user to proceed with `/2_create_plan` when ready.
