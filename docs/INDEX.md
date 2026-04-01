# Documentation Index

This index provides a navigable overview of all DEKE project documentation.

## Root Documents

| Document | Type | Description |
|----------|------|-------------|
| [README.md](../README.md) | Living | Project overview, quick start guide, architecture summary |
| [CONTRIBUTING.md](../CONTRIBUTING.md) | Living | Setup guide, coding conventions, documentation guide, tooling |
| [CLAUDE.md](../CLAUDE.md) | Living | AI development instructions, project conventions, code style |
| [LICENSE](../LICENSE) | Static | MIT License |

## Product — What the System Does

| Document | Type | Description |
|----------|------|-------------|
| [overview.md](product/overview.md) | Living | Vision, problem statement, three-package architecture, design principles |
| [knowledge-base.md](product/knowledge-base.md) | Living | Package 1: knowledge ingestion, search, quality, trust framework |
| [knowledge-leverage.md](product/knowledge-leverage.md) | Living | Package 2: advisory pipeline, domain adapters, confidence expression |
| [evolution-engine.md](product/evolution-engine.md) | Living | Package 3: prediction-error learning, curiosity, adapter evolution |
| [glossary.md](product/glossary.md) | Living | Domain terms and definitions |

## Architecture — How It Is Built

| Document | Type | Description |
|----------|------|-------------|
| [specification.md](architecture/specification.md) | Living | Tech stack, database schema, API contracts, code patterns |
| [federation.md](architecture/federation.md) | Living | Federation protocol: discovery, delegation, provenance, loop prevention |
| [retrieval-pipeline.md](architecture/retrieval-pipeline.md) | Living | Retrieval pipeline design: chunking, hybrid search, re-ranking, phases |
| [decisions.md](architecture/decisions.md) | Living | Architecture decision records, guardrails, open design questions |

## Science — Background Research

| Document | Type | Description |
|----------|------|-------------|
| [neuroevolution.md](science/neuroevolution.md) | Reference | Evolution strategies, NEAT, NAS, population-based training |
| [reinforcement-learning.md](science/reinforcement-learning.md) | Reference | TD learning, Goodhart's Law, curiosity, quality-diversity algorithms |
| [retrieval-theory.md](science/retrieval-theory.md) | Reference | Chunking, hybrid search, re-ranking, query transformation theory |
| [papers.md](science/papers.md) | Reference | Curated bibliography organized by topic |

## Planning

| Document | Type | Description |
|----------|------|-------------|
| [roadmap.md](roadmap.md) | Living | Phase summary across all packages |

## Documentation Map

```
DEKE/
├── README.md                           Project entry point, quick start
├── CONTRIBUTING.md                     Developer setup, documentation guide
├── CLAUDE.md                           AI-assisted development instructions
├── archive.zip                         Original documentation (docx files)
└── docs/
    ├── INDEX.md                        This file
    ├── roadmap.md                      Implementation phases and milestones
    ├── product/                        "What" — system model and behavior
    │   ├── overview.md                 Vision and three-package architecture
    │   ├── knowledge-base.md           Package 1: knowledge integrity
    │   ├── knowledge-leverage.md       Package 2: advisory responses
    │   ├── evolution-engine.md         Package 3: self-improvement
    │   └── glossary.md                 Domain terms
    ├── architecture/                   "How" — design and implementation
    │   ├── specification.md            Technical specification
    │   ├── federation.md               Federation protocol
    │   ├── retrieval-pipeline.md       Retrieval pipeline design
    │   └── decisions.md                ADRs, guardrails, open questions
    └── science/                        Background research
        ├── neuroevolution.md           Evolution strategies and frameworks
        ├── reinforcement-learning.md   TD learning, Goodhart's Law, curiosity
        ├── retrieval-theory.md         Retrieval science and techniques
        └── papers.md                   Paper bibliography
```

## Document Classifications

### Living Documents

Actively maintained as the project evolves.

**Product**: Updated when system capabilities, package scope, or design principles change.

**Architecture**: Updated when technical decisions are made, implementations change, or new design questions arise. `decisions.md` carries the architecture decision log.

**Planning**: `roadmap.md` updated at each milestone.

### Reference Documents

The `science/` branch contains curated research that ages gracefully. Updated when new relevant research is reviewed, not on every code change.

## Conventions

- **Root documents**: UPPERCASE filenames (e.g., `README.md`, `CONTRIBUTING.md`)
- **docs/ files**: lowercase-with-hyphens (e.g., `retrieval-pipeline.md`)
- **Tone**: Formal
- **Headings**: Single H1 per file
- **Code**: Inline backticks for short codes, fenced blocks with language tags
- **Links**: Relative Markdown links between documents
- **External references**: Link to official documentation rather than restating content
- **Content routing**: product/ = what (no code), architecture/ = how (tech details), science/ = general research (no DEKE specifics)
