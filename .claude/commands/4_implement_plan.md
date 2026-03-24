# Implement Plan

Execute the implementation plan step by step. This is the core execution
step of the RPI framework.

## Process

### 1. Load the Plan
Read the current plan from `thoughts/shared/plans/`.
If no plan exists, tell the user to run `/2_create_plan` first.
Check for a session file in `thoughts/shared/sessions/` to resume from.

### 2. Execute Steps In Order

For each step in the plan:

1. **Announce**: State which step you're executing and what it does
2. **Read context**: Read any files the step depends on
3. **Implement**: Make the code change following DEKE patterns:
   - Core models: `required` properties, records for DTOs
   - Repositories: `DbConnectionFactory` injection, `CreateConnectionAsync`,
     FastCrud for CRUD, raw Dapper for complex queries
   - Vector search: `Pgvector.Dapper`, `new Vector()`, `<=>` operator
   - Embeddings: `OnnxEmbeddingService.GenerateEmbedding()`, L2-normalize
   - API endpoints: Static class, `MapGroup`, `TypedResults`
   - MCP tools: `[McpTool]`, `[Description]`, `.WithTools<T>()`
   - Workers: `BackgroundService`, `ExecuteAsync`, `PeriodicTimer`
   - DI: Register in `ServiceCollectionExtensions` or host `Program.cs`
4. **Verify**: Run the verification step (`dotnet build` at minimum)
5. **Mark complete**: Update the session file with progress

### 3. Verification Gates

After each phase, run a full build:
```bash
dotnet build
```

After implementation is complete, run tests:
```bash
dotnet test
```

If a build or test fails:
1. Read the error carefully
2. Identify the root cause (don't guess)
3. Fix the issue following DEKE patterns
4. Re-verify before moving to the next step

### 4. Track Progress

Maintain a session file at:
```
thoughts/shared/sessions/{feature-slug}-session.md
```

Track:
- Which steps are complete / in-progress / pending
- Any deviations from the plan (and why)
- Build/test results at each checkpoint
- Decisions made during implementation

### 5. Commit Strategy

- Commit after each completed phase (Core, Schema, Infrastructure, Host, Tests)
- Use descriptive commit messages referencing the feature
- Keep commits atomic — one logical change per commit

## Output
After all steps complete, summarize what was implemented and any deviations
from the plan. Suggest running `/5_save_progress` to capture the session.
