# Resume Work

Resume a previously saved implementation session. Use this to continue
work from where you left off.

## Process

### 1. Find Sessions
List all session files in `thoughts/shared/sessions/`.
If multiple sessions exist, show them and ask the user which to resume.
If only one exists, load it automatically.

### 2. Restore Context

From the session file:
1. Read the linked plan from `thoughts/shared/plans/`
2. Read the linked research from `thoughts/shared/research/`
3. Identify the current step and its status
4. Check for any noted blockers or decisions

### 3. Verify Current State

Confirm the codebase matches the session state:
```bash
git status
git log --oneline -5
dotnet build
```

Check:
- Are we on the correct branch?
- Do the completed steps match what's actually in the code?
- Does the project still build?
- Are there any unexpected uncommitted changes?

### 4. Reconcile Differences

If the codebase state doesn't match the session:
- Someone may have made changes outside the session
- Identify what changed and whether it affects the plan
- Update the session file with the reconciled state
- Ask the user before overwriting any external changes

### 5. Resume Execution

Once context is restored:
1. Announce the current step
2. Continue implementation following `/4_implement_plan` process
3. Update the session file as steps complete

## Output
Report the restored context: which feature, which step, what's remaining.
Then proceed with implementation unless there are blockers to discuss.
