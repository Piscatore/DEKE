# Parking Lot

Ideas, issues, or scope questions raised during the Overhaul that are
deliberately deferred. This is a holding pen, not a backlog — not every
entry needs action. Review periodically (e.g. at each adjudication packet)
and either promote to a proposed ADR / packet, or cross out as declined.

Format:

```
## <date> — <short title>
- Raised during: OP-00n
- What: ...
- Why deferred: ...
```

---

## 2026-07-07 — cwm-roslyn-navigator TargetFramework misreport
- Raised during: OP-002
- What: `get_project_graph` reports `TargetFramework: netcoreapp1.0` for every DEKE project; actual is `net9.0` (confirmed via obj/ output and a successful `dotnet build`).
- Why deferred: cosmetic tool bug, doesn't block use of the project-graph tool itself; not worth an ADR. Noted here in case it causes confusion in a future mapping packet.
