# TOOLING

> Produced by OP-002 (DEKE Overhaul). Inventories every MCP server, plugin, and CLI tool available to a DEKE session; records a KEEP/DISCONNECT verdict per tool with the check that produced it. Re-run the verification suite in any reviewer packet to catch config drift.

## Inventory

### CLI (always available, not MCP-mediated)
| Tool | Purpose for DEKE | Needed by packet types | Verified check | Verdict |
|---|---|---|---|---|
| git | version control | all | `git remote -v` / `git status`: PASS | KEEP |
| gh (GitHub CLI) | PR/issue ops, authed as Piscatore | roadmap, adjudication, dissolution | `gh auth status`: PASS, active account Piscatore | KEEP |
| dotnet SDK 10.0.301 (targets net9.0 projects) | build/test/run | all | `dotnet build DEKE.sln`: PASS, 0 errors / 15 warnings | KEEP |
| podman | actual container runtime behind `docker-compose.yml` (podman-compose, not Docker Desktop) | mapping, design, roadmap (DB access) | `podman ps --filter name=deke-postgres`: PASS, container healthy | KEEP |

### MCP servers
| Tool / MCP | Purpose for DEKE | Needed by packet types | Verified check | Verdict |
|---|---|---|---|---|
| postgres-mcp | KB store queries (facts/terms/patterns/etc.) | mapping, design, spec, roadmap | `list_schemas` + `list_objects`: PASS — confirmed `deke` schema, all 8 expected tables (facts, terms, patterns, fact_relations, sources, learning_logs, advisory_interactions, federation_peers) | KEEP |
| nuget MCP | package docs, version checks, supply-chain review | mapping (dependency review), design | `get_package_context`(Dapper): PASS; `review_supply_chain_security`: PASS — flags no Central Package Management / no Package Source Mapping (relevant to the pin drift in ADR-0001) | KEEP |
| serena | semantic code nav + persistent project memory | all (primary code tool) | onboarding already performed, 5 project memories present | KEEP |
| roslyn ("roslyn" server, 40+ refactor/analysis tools) | deep C# refactor/analysis | mapping, spec | `search_symbols` on DEKE.sln: **PASS** (confirmed after OllamaSharp bumped to 5.3.1 in ADR-0001 — originally FAILED, MSBuildWorkspace aborted on the NU1603 restore warning; fix resolved it, re-verified working) | KEEP |
| cwm-roslyn-navigator (dotnet-claude-kit plugin) | C# project graph, antipattern/dead-code detection | mapping | `get_project_graph`: PASS, but TargetFramework field wrong (reports `netcoreapp1.0`, actual `net9.0`, see PARKING-LOT). `find_symbol`: **PASS** — correction: the original audit's apparent FAIL was a testing mistake (wrong parameter name, `query` instead of the tool's actual `name` parameter), not a real NU1603 block; re-tested with correct parameters both before and after the OllamaSharp fix and it works both times. | KEEP |
| docker-mcp | Docker container management | none — DEKE runs on Podman, not Docker Desktop | `list_containers`: **FAIL** — `RemoteDisconnected`, Docker Desktop daemon not running | DISCONNECT |
| LSP (harness-native, csharp-ls backend) | generic C# language-server analysis | none — redundant with the two Roslyn MCPs above even if fixed | `documentSymbol`: **FAIL** — `csharp-ls` not installed; MCP server disconnected immediately after | DISCONNECT |
| sqlcl | Oracle SQLcl (Avient/OrfPIM tool) | none — DEKE is Postgres-only | `connections_list`: returned Oracle connection `topsdev` — no DEKE relevance, confirms Avient bleed-through | DISCONNECT |
| windows-desktop | Windows GUI/AutoIt automation (Avient tool) | none — DEKE is a backend API/MCP/Worker solution, no GUI | inspected only, not invoked (no GUI surface to automate) | DISCONNECT |
| system-monitor | generic OS health metrics (CPU/disk/battery/network) | none | `get_cpu_usage`: PASS, functional but not DEKE-specific — no packet type consumes host metrics | DISCONNECT |
| claude.ai Google Calendar | calendar CRUD | none | inspected only | DISCONNECT |
| claude.ai Matrixify | Shopify import/export | none | inspected only | DISCONNECT |
| claude.ai Mermaid Chart | diagrams + Jira/Notion integration | none now; possible future use visualizing the OP-011 packet DAG | inspected only | DISCONNECT for now — revisit at OP-011 |
| claude.ai Google Drive | file access | none | requires OAuth, not authorized in this environment | DISCONNECT |
| DesignSync | claude.ai design-system sync | none — no UI/design system in DEKE | inspected only | DISCONNECT |

### Plugins / skill-sets
| Plugin | Purpose for DEKE | Needed by packet types | Verified check | Verdict |
|---|---|---|---|---|
| dotnet-claude-kit | build-fix, code-review, ef-core, minimal-api, testing, etc. — matches DEKE's .NET 9/Dapper/ASP.NET stack directly | all packets touching code | skill catalog matches stack | KEEP |
| claude-api (skill) | Claude/Anthropic API reference (models, pricing, IChatClient, MCP) | design/spec packets touching Advisory pipeline (`Anthropic.SDK` `IChatClient`, per decisions.md 2026-07-03/07) | directly relevant, confirmed against decisions.md | KEEP |
| verify, code-review, simplify, fewer-permission-prompts, review, security-review (skills) | general code-quality gates | mapping, spec, design, roadmap review | general-purpose, no DEKE-specific blocker found | KEEP |
| run (skill) | launch/exercise Deke.Api / Deke.Mcp / Deke.Worker | design, spec (manual verification) | no DEKE-specific blocker found | KEEP |
| rpi-workflow | general Research-Plan-Implement dev workflow, separate from the Overhaul's own STATE.md baton | regular DEKE feature work outside the Overhaul | not used by the Overhaul itself, harmless alongside it | KEEP (post-dissolution use) |
| update-config, keybindings-help, init, loop, schedule (skills) | Claude Code harness config, one-off bootstrap, automation | none DEKE-specific — harness-level, not per-project | n/a | KEEP-NEUTRAL, out of tool-budget scope |
| caveman / cavecrew, workflow-guard (plugins) | communication compression, hook management — user session preference | none — not DEKE tools | n/a | KEEP-NEUTRAL, out of tool-budget scope |
| graphify | generic "any input → knowledge graph" tool, user global default | none for DEKE — overlaps with serena for code nav and with the Overhaul's own purpose-built GLOSSARY/PROJECT-MAP artifacts | inspected only | DISCONNECT for DEKE/Overhaul sessions |
| smithery-ai-cli | MCP/tool discovery | none once tooling is settled | inspected only | DISCONNECT |
| qf-check / qf-complete / qf-decide / qf-migrate-to-raid / qf-new / qf-resume / qf-switch | quality-framework / RAID session tracking — duplicates the STATE.md baton mechanism the Overhaul already chose | none | inspected only | DISCONNECT |
| verbose-basic / -detailed / -debug / -off / -status | verbosity control for the same RAID/Memory-Agent ecosystem as qf-* | none | inspected only | DISCONNECT |
| dataviz, artifact-design | chart/artifact design guidance | none currently — DEKE is backend-only, no charts; revisit if OP-011 wants a visual packet DAG | inspected only | DISCONNECT for now |
| playwright-skill | browser automation | none — DEKE has no web frontend | inspected only | DISCONNECT |

### Harness-native tools (out of scope)
`WebFetch`, `WebSearch`, `TodoWrite`, `Monitor`, `NotebookEdit`, `EnterPlanMode`/`ExitPlanMode`, `EnterWorktree`/`ExitWorktree`, `CronCreate`/`CronDelete`/`CronList`, `PushNotification`, `RemoteTrigger`, `SendMessage`, `TaskOutput`/`TaskStop`, `ListMcpResourcesTool`, `ReadMcpResourceDirTool`/`ReadMcpResourceTool` — baseline Claude Code capabilities, not something a DEKE session config can independently disconnect. Not scored here.

## Verification suite
Cheap, one-call checks a reviewer packet can re-run to catch config drift:
1. `git remote -v` — expect `origin` -> `github.com/Piscatore/DEKE.git`
2. `gh auth status` — expect active account `Piscatore`
3. `dotnet build DEKE.sln` — expect `Build succeeded`, 0 errors
4. `podman ps --filter name=deke-postgres` — expect container `Up ... (healthy)`
5. postgres-mcp `list_objects(schema_name="public")` — expect 8 tables incl. `facts`, `terms`, `patterns`
6. roslyn `search_symbols` / cwm-roslyn-navigator `find_symbol` on DEKE.sln — expect PASS (ADR-0001 resolved: OllamaSharp bumped to 5.3.1). A FAIL here would be a regression worth investigating.
7. docker-mcp `list_containers` — expected FAIL (Docker Desktop not in use); a PASS here would mean Docker Desktop is now running and should prompt re-evaluating podman vs Docker Desktop, not silent acceptance
8. sqlcl `connections_list` — expected to show no DEKE-relevant connection; any DEKE/Postgres connection appearing here would be a scope leak worth investigating

## Model & thinking tiers
| Packet type | Model | Thinking |
|---|---|---|
| Adjudication, ADRs, design escalations, roadmap rebuild | strongest available (Opus-class) | extended |
| Mapping sweeps, spec refactors against approved glossary, lint work | Sonnet-class | default |
| Reviewer packets | Sonnet-class | default (escalate on findings) |

(Confirmed appropriate for OP-002 itself: default Sonnet-class, default thinking — no ambiguity required escalation.)

## Human touchpoints — confirm before disconnecting anything
Recommend disconnecting: **docker-mcp, LSP, sqlcl, windows-desktop, system-monitor, claude.ai Google Calendar, claude.ai Matrixify, claude.ai Mermaid Chart, claude.ai Google Drive, DesignSync, graphify, smithery-ai-cli, qf-* (7 skills), verbose-* (5 skills), dataviz, artifact-design, playwright-skill** — confirm?
