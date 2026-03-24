# Save Progress

Save the current implementation session state so work can be resumed later.
Use this when stopping mid-implementation or after completing a feature.

## Process

### 1. Assess Current State

Determine what has been done:
- Which plan is being executed (from `thoughts/shared/plans/`)
- Which steps are complete, in-progress, or pending
- Current build status (`dotnet build`)
- Current test status (`dotnet test`)
- Any uncommitted changes (`git status`, `git diff --stat`)

### 2. Save Session File

Write or update the session file at:
```
thoughts/shared/sessions/{feature-slug}-session.md
```

Include:
```markdown
# Session: {Feature Name}
**Plan**: thoughts/shared/plans/{feature-slug}-plan.md
**Started**: {timestamp}
**Last Updated**: {timestamp}
**Status**: In Progress / Complete / Blocked

## Completed Steps
- [x] Step 1: {description} — {commit hash if committed}
- [x] Step 2: {description}

## Current Step
- [ ] Step N: {description}
  - Progress: {what's been done so far}
  - Blockers: {any issues}

## Remaining Steps
- [ ] Step N+1: {description}
- [ ] Step N+2: {description}

## Uncommitted Changes
{output of git diff --stat, if any}

## Decisions & Deviations
- {any changes from the original plan and reasoning}

## Notes for Resumption
- {context needed to pick up where we left off}
- {any gotchas or things to remember}
```

### 3. Commit Progress

If there are uncommitted changes:
1. Stage relevant files
2. Commit with message: `wip: {feature} — {current step description}`
3. Push to the working branch

### 4. Summary

Report:
- How many steps complete out of total
- Current build/test status
- What's needed next to continue

## Output
Confirm session saved. Tell the user they can resume with `/6_resume_work`.
