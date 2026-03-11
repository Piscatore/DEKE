# Documentation Index

This index provides a navigable overview of all DEKE project documentation.

## Root Documents

| Document | Type | Description |
|----------|------|-------------|
| [README.md](../README.md) | Living | Project overview, quick start guide, architecture summary |
| [SPECIFICATION.md](../SPECIFICATION.md) | Living (versioned) | Complete technical specification with decision log |
| [CLAUDE.md](../CLAUDE.md) | Living | AI development instructions, project conventions, code style |

## docs/ Directory

| Document | Type | Description |
|----------|------|-------------|
| [INDEX.md](./INDEX.md) | Living | This file -- documentation index and map |
| [mcp-plugins-research.md](./mcp-plugins-research.md) | Reference | Research notes on MCP server registries and plugin ecosystem |

## Documentation Map

```
DEKE/
├── README.md                       Project entry point, quick start
├── SPECIFICATION.md                Technical specification (versioned, decision log)
├── CLAUDE.md                       AI-assisted development instructions
└── docs/
    ├── INDEX.md                    Documentation index (this file)
    └── mcp-plugins-research.md     MCP plugin ecosystem research
```

## Document Classifications

### Living Documents

These documents are actively maintained and updated as the project evolves.

- `README.md` -- Updated when project capabilities, setup steps, or architecture change.
- `SPECIFICATION.md` -- Updated when technical decisions are made or plans change. Contains a decision log tracking the reasoning behind changes.
- `CLAUDE.md` -- Updated when development conventions, project structure, or tooling change.
- `docs/INDEX.md` -- Updated whenever documentation is added, removed, or reorganized.

### Reference Documents

These documents capture research or analysis and are not expected to change frequently.

- `docs/mcp-plugins-research.md` -- Snapshot of MCP plugin ecosystem research.

## Cross-Reference Summary

| Source | Links To |
|--------|----------|
| `README.md` | `SPECIFICATION.md`, `CLAUDE.md` |
| `CLAUDE.md` | `SPECIFICATION.md`, external dependency docs |
| `SPECIFICATION.md` | External dependency docs |
| `docs/INDEX.md` | All documentation files |

## Conventions

- **Root documents**: UPPERCASE filenames (e.g., `README.md`, `SPECIFICATION.md`)
- **docs/ files**: lowercase-with-hyphens (e.g., `mcp-plugins-research.md`)
- **Tone**: Formal
- **Headings**: Single H1 per file
- **Code**: Inline backticks for short codes and commands, fenced code blocks with language tags otherwise
- **Links**: Relative Markdown links
- **External references**: Link to official documentation rather than restating content
