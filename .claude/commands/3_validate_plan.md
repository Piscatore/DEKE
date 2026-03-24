# Validate Plan

Review and validate an implementation plan before execution.
This is an optional but recommended step of the RPI framework.

## Process

### 1. Load the Plan
Read the most recent plan from `thoughts/shared/plans/`.
If no plan exists, tell the user to run `/2_create_plan` first.

### 2. Structural Validation

Check the plan against DEKE architecture rules:

- [ ] **Dependency order**: Core changes before Infrastructure before Hosts
- [ ] **No circular deps**: Core never references Infrastructure or Hosts
- [ ] **Interface-first**: New Core interfaces defined before implementations
- [ ] **DI registration**: Every new service has a registration step
- [ ] **Schema sync**: Database changes in `init.sql` match model properties

### 3. Pattern Compliance

Verify each step follows established DEKE patterns:

- [ ] **Repositories**: Use `DbConnectionFactory`, Dapper + FastCrud pattern
- [ ] **Vector ops**: Use `Pgvector.Dapper`, cosine distance operator `<=>`
- [ ] **Embeddings**: Use `OnnxEmbeddingService`, L2-normalize before storage
- [ ] **API endpoints**: Static class, `MapGroup`, `TypedResults`
- [ ] **MCP tools**: `[McpTool]` + `[Description]` attributes, `.WithTools<T>()`
- [ ] **Workers**: Inherit `BackgroundService`, `PeriodicTimer` pattern
- [ ] **Type handlers**: Registered in `DapperConfig.Initialize()` if needed
- [ ] **Code style**: File-scoped namespaces, `_` prefix, `CancellationToken`, records for DTOs

### 4. Completeness Check

- [ ] All files listed in research are addressed in the plan
- [ ] Build verification steps included (at minimum after each phase)
- [ ] Test steps included for new functionality
- [ ] Configuration changes documented (appsettings, env vars)
- [ ] No orphan steps (every step has clear input and output)

### 5. Risk Assessment

- [ ] Breaking changes to existing interfaces identified
- [ ] Database migration path clear (manual apply or container recreate)
- [ ] Large steps broken into smaller, independently verifiable pieces
- [ ] Rollback approach defined for risky steps

### 6. Update Plan
If issues found, update the plan in `thoughts/shared/plans/` with fixes.
Add a validation log entry at the bottom of the plan document.

## Output
Report validation results: PASS / PASS WITH NOTES / NEEDS REVISION.
If passed, suggest proceeding with `/4_implement_plan`.
