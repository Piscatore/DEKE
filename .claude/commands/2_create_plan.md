# Create Implementation Plan

Create a detailed, step-by-step implementation plan based on the research
from step 1. This is the second step of the RPI framework.

## Process

### 1. Load Research
Read the most recent research document from `thoughts/shared/research/`.
If no research exists, tell the user to run `/1_research_codebase` first.

### 2. Design the Implementation

Organize work into ordered steps that respect DEKE's dependency flow:

**Phase 1 — Core Layer** (if needed):
- New models in `src/Deke.Core/Models/`
- New or modified interfaces in `src/Deke.Core/Interfaces/`
- Core has zero dependencies — changes here affect everything downstream

**Phase 2 — Database Schema** (if needed):
- Table/column/index changes in `init.sql`
- New type handlers in `src/Deke.Infrastructure/Data/TypeHandlers/`
- Register handlers in `DapperConfig.Initialize()`

**Phase 3 — Infrastructure Layer**:
- Repository implementations in `src/Deke.Infrastructure/Repositories/`
- Use Dapper + FastCrud for CRUD, raw Dapper for complex queries
- Embedding operations via `OnnxEmbeddingService`
- New harvesters in `src/Deke.Infrastructure/Harvesters/`
- DI registration in `ServiceCollectionExtensions.cs`

**Phase 4 — Host Layer** (one or more):
- API endpoints: `src/Deke.Api/Endpoints/` + register in `Program.cs`
- MCP tools: `src/Deke.Mcp/Tools/` + register in `Program.cs`
- Worker services: `src/Deke.Worker/Services/` + register in `Program.cs`

**Phase 5 — Tests**:
- Unit tests for new Core types
- Unit tests for Infrastructure services (mock DbConnectionFactory)
- Integration tests for repositories (if test DB available)
- Endpoint tests for API changes

### 3. Write the Plan

For each step, specify:
- **File**: Exact path to create or modify
- **Action**: Create / Modify / Delete
- **What**: Precise description of the change
- **Pattern**: Reference to existing code that demonstrates the pattern
- **Dependencies**: Which prior steps must complete first
- **Verification**: How to confirm the step is correct (`dotnet build`, test, etc.)

### 4. Save the Plan
Write to:
```
thoughts/shared/plans/{feature-slug}-plan.md
```

Include:
- Link back to the research document
- Ordered step list with all details above
- Estimated complexity (S/M/L per step)
- Checkpoint markers: where to verify partial progress
- Rollback notes: how to undo each step if needed

## Output
Present the plan summary and ask the user to validate with
`/3_validate_plan` or proceed directly with `/4_implement_plan`.
