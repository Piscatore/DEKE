# Documentation Index

This index provides a navigable overview of all DEKE project documentation.

## Root Documents

| Document | Type | Description |
|----------|------|-------------|
| [README.md](../README.md) | Living | Project overview, quick start guide, architecture summary |
| [SPECIFICATION.md](../SPECIFICATION.md) | Living (versioned) | Complete technical specification with decision log |
| [CLAUDE.md](../CLAUDE.md) | Living | AI development instructions, project conventions, code style |
| [CONTRIBUTING.md](../CONTRIBUTING.md) | Living | Setup guide, coding conventions, tooling recommendations |
| [LICENSE](../LICENSE) | Static | MIT License |

## docs/ Directory

| Document | Type | Description |
|----------|------|-------------|
| [INDEX.md](./INDEX.md) | Living | This file -- documentation index and map |

## Documentation Map

```
DEKE/
├── README.md                       Project entry point, quick start
├── SPECIFICATION.md                Technical specification (versioned, decision log)
├── CLAUDE.md                       AI-assisted development instructions
└── docs/
    └── INDEX.md                    Documentation index (this file)
```

## Document Classifications

### Living Documents

These documents are actively maintained and updated as the project evolves.

- `README.md` -- Updated when project capabilities, setup steps, or architecture change.
- `SPECIFICATION.md` -- Updated when technical decisions are made or plans change. Contains a decision log tracking the reasoning behind changes.
- `CLAUDE.md` -- Updated when development conventions, project structure, or tooling change.
- `docs/INDEX.md` -- Updated whenever documentation is added, removed, or reorganized.

## Cross-Reference Summary

| Source | Links To |
|--------|----------|
| `README.md` | `SPECIFICATION.md`, `CLAUDE.md` |
| `CLAUDE.md` | `SPECIFICATION.md`, external dependency docs |
| `SPECIFICATION.md` | External dependency docs |
| `docs/INDEX.md` | All documentation files |

## Conventions

- **Root documents**: UPPERCASE filenames (e.g., `README.md`, `SPECIFICATION.md`)
- **docs/ files**: lowercase-with-hyphens
- **Tone**: Formal
- **Headings**: Single H1 per file
- **Code**: Inline backticks for short codes and commands, fenced code blocks with language tags otherwise
- **Links**: Relative Markdown links
- **External references**: Link to official documentation rather than restating content
