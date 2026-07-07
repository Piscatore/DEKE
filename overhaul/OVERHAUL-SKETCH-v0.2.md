# DEKE Overhaul — Subproject Sketch (v0.2)

> **Status: TEMPORARY SUBPROJECT.** The Overhaul is scaffolding, not architecture.
> It exists to produce a small set of permanent artifacts and then dissolve.
> When the Exit Criteria (§8) are met, this directory's *process* documents are
> archived; only the artifacts it produced (glossary, project map, intent,
> tooling doc, ADRs, roadmap) remain live.

**Changes from v0.1:** design/feature redesign now explicitly in scope via a
controlled escalation path (§1, §4, §5); tooling audit added as OP-002 with
tool budgets per packet and model-tier guidance (§3.5, §4); packet DAG
renumbered (§6); exit criteria extended (§8).

---

## 1. Charter

**Goal.** Establish a canonical ontology (naming + concepts) and a modular,
LLM-executable roadmap for DEKE. Where bad naming or planning turns out to be
a symptom of an underlying design or feature problem, fixing that underlying
problem is *in scope* — the Overhaul may reshape design and features, not just
labels.

**Non-goals.** No **undecided** redesign. Design and feature changes are
welcome, but they enter only through the front door: a mapping/glossary/spec
packet that uncovers a design smell raises it as a proposed ADR; Mikael
adjudicates; if approved, it becomes its own sized packet in the DAG. No
packet quietly redesigns inline while doing something else. Ideas that don't
warrant an ADR go to `PARKING-LOT.md`.

**Work model.** Mikael produces ideas and decisions; LLM agents perform the
work. Therefore every step must be executable by an agent that has *only*:

1. This subproject's artifacts (small, dense, always loadable)
2. The specific files and tools named in its work packet
3. Mikael available for bounded decisions (approve/veto/pick-one, not
   open-ended design conversations)

**Prime directive for agents.** Never require "understanding the whole
project" as a precondition. If a task seems to require it, the task is
mis-sized — split it and update the packet plan.

---

## 2. Repo layout for the subproject

```
/overhaul/                      ← temporary; archived at exit
  CHARTER.md                    ← §1, standalone
  STATE.md                      ← the handover baton (§7)
  PARKING-LOT.md                ← ideas/issues deferred out of scope
  packets/
    OP-001.md ... OP-nnn.md     ← work packet definitions
/docs/                          ← permanent artifacts produced by the overhaul
  GLOSSARY.md
  PROJECT-MAP.md
  INTENT.md
  TOOLING.md
  adr/
    ADR-0001-....md
  ROADMAP.md                    ← rebuilt as a packet DAG
```

Rationale: hard separation between *temporary process* (`/overhaul`) and
*permanent product* (`/docs`) makes the "this is scaffolding" promise
structural, not just rhetorical.

---

## 3. Phase 1 artifact templates

### 3.1 PROJECT-MAP.md — the component inventory

One entry per module/project/major directory. Hard cap: ~8 lines per entry.
The map must stay small enough to load in full, always.

```markdown
## <CurrentName>  [proposed: <CanonicalName> | KEEP | TBD]
- **What it is:** one sentence, plain language.
- **Responsibility:** the one thing it owns.
- **Key dependencies:** →<Module>, →<Module>
- **Naming issues:** references namingissues.md items #n, #m (or "none")
- **Design smells:** none | escalated as ADR-000n (proposed)
- **Confidence:** HIGH | MEDIUM | LOW  ← how sure the mapping agent is
- **Sources read:** paths the agent actually looked at
```

`Confidence` + `Sources read` enable cheap targeted review later. The
`Design smells` field is the escalation hook: a smell is *recorded and
escalated*, never fixed inline.

### 3.2 GLOSSARY.md — the ubiquitous language

One row per *concept*, not per name. This is the single most important
artifact; everything else enforces it.

```markdown
| Canonical term | Definition (≤2 sentences) | Deprecated aliases | Decided in | Status |
|---|---|---|---|---|
| KnowledgeBlock | The atomic unit of ... | Chunk, KnowledgeItem, KBEntry | ADR-0003 | APPROVED |
| ExpertAdapter  | ...                     | Adapter, Expert, Persona     | —        | PROPOSED |
```

Statuses: `PROPOSED` (agent suggestion) → `APPROVED` / `REJECTED` (Mikael) →
`ENFORCED` (lint rule active). An agent may only *propose*; only Mikael
approves. A concept that resists clean naming is treated as a possible design
smell and escalated per §4.

### 3.3 INTENT.md — the distilled why

Replaces "study earlier LLM chat sessions" with a one-time distillation.
Structure: one section per major design intention, each ≤ half a page:

```markdown
## Intention: <name>
- **What Mikael wants:** ...
- **Why (origin/context):** ...
- **Constraints it imposes:** ...
- **Source:** interview 2026-07-xx / chat export <file> / inferred (flagged)
```

Anything *inferred* rather than sourced gets flagged for confirmation in the
interview packet. This file is how future agents "interview Mikael" without
interviewing him.

### 3.4 ADRs — naming & design decisions

Minimal template, ≤1 page each:

```markdown
# ADR-000n: <decision title>
- **Status:** proposed | accepted | rejected | superseded by ADR-...
- **Type:** naming | design | tooling
- **Decision:** what is decided, in glossary terms.
- **Why:** 2–5 bullet rationale.
- **Rejected alternatives:** name + one-line reason each.
- **Consequences:** packets this creates or reshapes (for design ADRs)
```

Purpose: future sessions never relitigate. They read, they comply. Design
ADRs additionally spawn packets via their `Consequences` field.

### 3.5 TOOLING.md — the agent environment

The agents' environment is part of the system being overhauled. A bloated or
misconfigured toolchain taxes every packet: each connected MCP server injects
its tool definitions into context whether used or not. Token efficiency and
quality control are favored over convenience.

```markdown
## Inventory
| Tool / MCP / plugin | Purpose for DEKE | Needed by packet types | Verified check | Verdict |
|---|---|---|---|---|
| RoslynMcp.Server | C# solution analysis | mapping, spec, design | resolves DEKE.sln: PASS | KEEP |
| <pgvector access> | KB store queries    | design, roadmap        | test query: PASS       | KEEP |
| mssql-rw          | (Avient/OrfPIM)     | none                   | —                      | DISCONNECT for DEKE sessions |

## Verification suite
Small, cheap checks (one command or one tool call each) proving each KEPT
tool actually works against DEKE. Reviewer packets re-run the suite to catch
config drift.

## Model & thinking tiers
| Packet type | Model | Thinking |
|---|---|---|
| Adjudication, ADRs, design escalations, roadmap rebuild | strongest available (Opus-class) | extended |
| Mapping sweeps, spec refactors against approved glossary, lint work | Sonnet-class | default |
| Reviewer packets | Sonnet-class | default (escalate on findings) |
```

The inventory rows above are illustrative guesses; OP-002 verifies rather
than assumes, and its explicit mandate includes recommending
**disconnections**, not just additions.

---

## 4. Work packet contract (the core format)

Every unit of work — during the overhaul *and* on the permanent roadmap
after it — is a packet:

```markdown
# OP-00n: <imperative title, e.g. "Map the ingestion pipeline modules">

## Contract
- **Goal:** one sentence. If it needs two, split the packet.
- **Context budget:** files an agent must load, listed explicitly:
  - ALWAYS: /docs/GLOSSARY.md, /overhaul/STATE.md
  - THIS PACKET: <specific paths, with line ranges if large>
- **Tool budget:** tools/MCPs this packet needs (per TOOLING.md);
  everything else is presumed off/disconnected for the session.
- **Recommended tier:** model + thinking level (per TOOLING.md §tiers).
- **Inputs:** artifacts/decisions this depends on (packet IDs, ADR IDs)
- **Outputs:** exact files created/modified
- **Done when:** checkable criteria (a reviewer agent could verify)
- **Human touchpoints:** decisions Mikael must make, phrased as
  closed questions (approve/veto/pick-one), listed up front
- **Out of scope:** explicit, to prevent drift

## Escalation path (standard, applies to every packet)
If this packet uncovers a design or feature problem:
1. Do NOT fix it inline.
2. Write a `proposed` ADR (type: design) with the finding and options.
3. Note it in STATE.md and in this packet's output.
4. The adjudication packet routes it to Mikael; if accepted, the ADR's
   Consequences field defines the new packet(s).

## Sizing rule
Glossary + STATE + this packet's files must fit one session with ≥50%
context left for actual work. If not: split, and record the split here.

## Handover
On completion, the agent appends 5–10 lines to STATE.md:
what was done, what was decided/escalated, what the next packet should know.
```

Design notes, specifically for LLM efficiency and quality:

- **Closed human questions.** Agents work best when Mikael's input is a
  decision, not a conversation. Each packet front-loads its questions so they
  can be answered in one message.
- **Reviewer packets.** Every few production packets, a fresh session loads
  only the outputs + done-criteria and verifies (and re-runs the tooling
  verification suite). Fresh eyes are free with LLMs; use them as QA.
- **Deterministic enforcement over vigilance.** Once GLOSSARY terms hit
  ENFORCED, a lint script fails CI on deprecated terms. Agents don't have to
  *remember* naming; the build remembers.

---

## 5. How redesign flows through the system

```
mapping/glossary/spec packet
        │ finds design smell
        ▼
 proposed ADR (type: design)     ← never fixed inline
        │
        ▼
 interview/adjudication packet   ← Mikael decides (closed questions)
        │ accepted
        ▼
 ADR.Consequences → new sized design packet(s) in the DAG
        │
        ▼
 spec refactor reflects the *decided* design
```

This is why spec refactoring (OP-009) comes **after** design adjudication:
approved redesigns reshape what the specs even describe.

---

## 6. Packet plan (v0.2 draft of the DAG)

```
OP-001   Bootstrap: create /overhaul + /docs skeletons, STATE.md, charter
OP-002   Tooling audit: inventory, verify, recommend keep/disconnect,
         write TOOLING.md incl. verification suite + tier table
OP-003   Ingest namingissues.md → normalize into GLOSSARY rows (all PROPOSED)
OP-004a..n  Mapping sweeps: PROJECT-MAP entries, one repo region per packet;
         design smells escalated as proposed ADRs
OP-005   Intent distillation: from available chat exports/docs → INTENT.md
OP-006   Interview packet: closed questions from OP-003..005, including all
         escalated design ADRs → Mikael answers
OP-007   Adjudication: apply answers; glossary rows APPROVED/REJECTED;
         design ADRs accepted/rejected; accepted ones spawn OP-008 packets
OP-008a..n  Design packets: approved redesigns, each its own sized packet
OP-009a..n  Spec refactor packets: one per spec doc, against approved
         glossary + decided design (parallelizable)
OP-010   Glossary lint script + CI hook → terms move to ENFORCED
OP-011   Roadmap rebuild: all future work re-expressed as packets in a DAG
OP-012   Review packet: verify exit criteria; re-run tooling verification
OP-013   Dissolution: archive /overhaul, tag repo, close subproject
```

OP-004 is deliberately *sweeps* — nobody maps the whole repo; each sweep maps
one region against the same template, and the map is the union. OP-008 may be
empty if no redesigns are approved; the slot exists so redesign has a lane.

---

## 7. STATE.md — the baton

A single append-only file, newest entries on top, each ≤10 lines. It is the
only cross-session memory besides the artifacts themselves. Every packet
starts by reading it, ends by appending to it. (Same philosophy as the
rpi-workflow handover documents — one baton, always small, always current.)

---

## 8. Exit criteria (when the Overhaul dissolves)

1. GLOSSARY.md: no PROPOSED rows remain; core terms ENFORCED via CI lint.
2. PROJECT-MAP.md: no LOW-confidence entries; no unescalated design smells.
3. INTENT.md: no unconfirmed inferred intentions remain.
4. ADRs: no `proposed` design ADRs remain — each is accepted (with its
   packets completed or placed on the permanent roadmap) or rejected.
5. TOOLING.md: verification suite passes; no unresolved KEEP/DISCONNECT
   verdicts.
6. All spec documents pass the glossary lint.
7. ROADMAP.md expresses all planned future work as sized packets in a DAG,
   each with context budget, tool budget, and recommended tier.
8. A fresh agent session, given only /docs + one roadmap packet, completes
   that packet without asking "what is this project?" — the acceptance test.

After exit: /overhaul is archived (kept for history, excluded from agent
context budgets), and DEKE's own /docs become the first knowledge base DEKE
should learn to serve about itself.
