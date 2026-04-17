# Standard Operating Procedures (SOP)

## SOP-REF 2.1 Plan-First Execution
Execution is based solely on ExecutionPlan. No runtime inference.

## SOP-REF 2.2 Explicit Modes
Mode must be explicit (Apply, DryRun, Request).

## SOP-REF 2.4 Constrained TDD
All structural guarantees enforced at build-time.

## SOP-REF 3.1 Audit-Commit Gate
Begin → Intent → Execute → Result → Commit (atomic).

## SOP-REF 3.3 Snapshot Enforcement
SnapshotId + SnapshotExpected persisted with result.

## SOP-REF 4.1 Assembly Map
contracts → planning → engine.abstractions → engine → infrastructure.abstractions → infrastructure

## SOP-REF 4.2 Reference Rules
Forbidden:
- Executors → Infrastructure
- Executors → Engine
- CLI → Engine implementation
- Planning → Infrastructure

## Enforcement

| SOP | Enforcement |
|-----|------------|
| 2.1 | ExecutionPlan.cs hash validation |
| 2.4 | ValidateExecutors.targets |
| 3.1 | Engine transaction structure |
| 4.1 | Folder topology |
| 4.2 | NamespaceGuard.targets + Dx.Architecture.Tests |
