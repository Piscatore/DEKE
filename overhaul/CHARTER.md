# DEKE Overhaul — Charter

> **Status: ARCHIVED (2026-07-14, OP-013).** The subproject dissolved after
> all 8 exit criteria passed (OP-012, see `STATE.md`). This charter is kept
> for history per the original plan — `/overhaul` was scaffolding, not
> architecture. Live docs: `/docs`.

## Goal

Establish a canonical ontology (naming + concepts) and a modular,
LLM-executable roadmap for DEKE. Where bad naming or planning turns out to be
a symptom of an underlying design or feature problem, fixing that underlying
problem is **in scope** — the Overhaul may reshape design and features, not
just labels.

## Non-goals

No **undecided** redesign. Design and feature changes are welcome, but they
enter only through the front door: a mapping/glossary/spec packet that
uncovers a design smell raises it as a proposed ADR; Mikael adjudicates; if
approved, it becomes its own sized packet in the DAG. No packet quietly
redesigns inline while doing something else. Ideas that don't warrant an ADR
go to `PARKING-LOT.md`.

## Work model

Mikael produces ideas and decisions; LLM agents perform the work. Every step
must be executable by an agent that has *only*:

1. This subproject's artifacts (small, dense, always loadable)
2. The specific files and tools named in its work packet
3. Mikael available for bounded decisions (approve/veto/pick-one, not
   open-ended design conversations)

## Prime directive for agents

Never require "understanding the whole project" as a precondition. If a task
seems to require it, the task is mis-sized — split it and update the packet
plan.

## Escalation path (applies to every packet)

If a packet uncovers a design or feature problem:

1. Do **not** fix it inline.
2. Write a `proposed` ADR (type: design) with the finding and options.
3. Note it in `STATE.md` and in the packet's output.
4. The adjudication packet routes it to Mikael; if accepted, the ADR's
   `Consequences` field defines the new packet(s).

## Exit criteria

See `OVERHAUL-SKETCH-v0.2.md` §8 for the full list. In short: no unresolved
glossary proposals, no unescalated design smells, no unconfirmed inferred
intentions, no unresolved tooling verdicts, all specs pass the glossary
lint, the roadmap is fully expressed as sized packets — and a fresh agent
session, given only `/docs` plus one roadmap packet, can complete it without
asking "what is this project?"
