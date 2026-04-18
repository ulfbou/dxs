# Contributing to dxs

This document defines how changes are proposed, reviewed, and merged in the dxs repository.

dxs is correctness‑first. Contributions are evaluated not only on behavior, but on whether they preserve architectural invariants and execution authority.

---

## Prerequisites

- .NET SDK (see `global.json`)
- Git
- PowerShell or Bash

---

## Getting Started

1. Clone the repository
2. Build the solution:
   ```powershell
   dotnet build
````

3.  Run architecture tests:
    ```powershell
    dotnet test tests/Dx.Architecture.Tests
    ```

A clean build and green architecture tests are the baseline for all work.

---

## Contribution Model

dxs follows a **plan‑first, authority‑driven model**. All changes must respect:

*   Explicit execution authority (Engine only)
*   Deterministic planning
*   Enforced layering via build rules and tests

If a change weakens these properties, it will not be merged.

When in doubt, open a discussion before writing code.

---

## Branch Naming

Use intent‑based prefixes:

*   `feat/` — new capabilities
*   `fix/` — defect corrections
*   `refactor/` — structural change with no semantic change
*   `chore/` — tooling and maintenance
*   `docs/` — documentation only
*   `test/` — test additions

Example:

    feat/engine-abstractions

---

## Commits

Use Conventional Commits:

    <type>(<scope>): <description>

Types:

*   `feat`
*   `fix`
*   `refactor`
*   `docs`
*   `test`
*   `chore`

Guidelines:

*   Subject line under 72 characters
*   Explain *why* the change exists, not what the diff does
*   Keep commits semantically coherent

---

## Pull Requests

Before opening a PR:

*   Rebase on the latest `main`
*   `dotnet build` succeeds with no warnings
*   `dotnet test tests/Dx.Architecture.Tests` passes
*   Documentation is updated if behavior or contracts change

PR titles must follow the same convention as commits.

Each PR description must include:

*   Summary of the change
*   Affected assemblies
*   SOP references (for any invariant‑relevant change)

The `ci` workflow must be green before review.

---

## Architecture Governance

Changes touching the following areas require special scrutiny and may require an ADR:

*   `src/contracts`
*   `src/engine`
*   `src/infrastructure`

Relevant SOP references:

*   SOP‑REF 4.1 — Infrastructure Abstraction
*   SOP‑REF 4.2 — Dependency and Executor Isolation

These rules are enforced by:

*   `NamespaceGuard.targets`
*   `ValidateExecutors.targets`
*   `Dx.Architecture.Tests`

If the build fails due to an architecture rule, the fix is to correct the structure, not to disable the check.

---

## Code Standards

*   Prefer explicit types where clarity matters
*   Keep public APIs minimal and stable
*   Executors must not reference Infrastructure
*   CLI must not reference Engine implementations
*   Planning must remain pure and deterministic

Refactors must include proof that semantics are unchanged (tests, invariants, or both).

---

## Documentation

Documentation lives under `/docs`.

Update documentation when:

*   Execution behavior changes
*   Contracts are added or modified
*   Authority or boundaries shift

Keep documentation concise and task‑oriented. Avoid restating implementation details that are enforced by build or tests.

---

## Asking Questions

For questions about direction or correctness, open a discussion before starting work. Early alignment is preferred over late rework.

dxs values structural clarity over speed. Contributions are reviewed accordingly.
