# DXS Internal Constitution & Invariant Test Suite

**Status:** Normative (Phase 0/1)  
**Description:** This document complements the DXS Layered Authority Protocol Specification by defining the Internal Constitution (non-negotiable principles every change must respect) and the Invariant Test Suite (machine-checkable assertions that enforce the Constitution in code).

---

## Part A — Internal Constitution

### A.1 Constitutional Hierarchy (Four Pillars)

| Pillar | Principle | Operational Reality |
| :--- | :--- | :--- |
| **I. Content Authority** | The Snapshot is the only Truth. | If it is not in a Snapshot Manifest, it did not happen. |
| **II. Causal Integrity** | No state without a parent. | Every Snapshot must name its parent snapshot and causal event. |
| **III. Epistemic Humility** | Trust is declared, not assumed. | Optimistic ≠ Proven; Hermetic ⇒ Cryptographic. |
| **IV. Explicit Agency** | Authority is a Pointer, not a Path. | Vetoes re-orient the future; history remains. |

> **Constitutional Rule:** Any implementation violating a pillar **MUST** be rejected regardless of performance or UX gains.

### A.2 Authority Model (Normative)
* **Authoritative Truth:** Snapshot Manifests only.
* **Explanatory Records:** Event Logs only.
* **Derived Views:** State files, indexes, and projections.
* **Authority Changes:** Only via explicit `AuthorityUpdate` events.
* **Prohibition:** No command may implicitly change authority.

### A.3 Intent vs Outcome
* Intent is recorded as events (`TaskStarted`, `SnapshotProposed`).
* Outcome is recorded as snapshots (accepted or rejected).
* Acceptance is orthogonal to existence.
* **Rule:** A Snapshot **MAY** exist without being authoritative.

### A.4 Trust Levels
* **Optimistic:** Best-effort determinism; cache is advisory.
* **Hermetic:** Enforced containment; cache is sound.
* **Rule:** Trust level **MUST** be recorded in Snapshot Manifests and hash outputs.

### A.5 Governance (Phase 1 Minimalism)
* Governance is event-based, not implicit.
* **Phase 1 supports:** Single-actor authority; explicit veto with reason.
* **Phase 2 MAY add:** Quorums, weighted actors, policy rules.
* **Rule:** Governance extensions **MUST NOT** rewrite history.

---

## Part B — Minimum Constitutional Compliance (MCC)

Every PR, feature, or refactor **MUST** pass the MCC checklist:

1.  **Snapshot Mutation:** Does this change mutate a Snapshot? → **Reject**.
2.  **Hidden Veto:** Does this change hide or collapse a veto? → **Reject**.
3.  **Log Bypass:** Does this command bypass the Event Log? → **Reject**.
4.  **OS/Shell Coupling:** Does this rely on a specific OS/shell? → **Mark Optimistic**.
5.  **Implicit Authority:** Does this infer authority without an event? → **Reject**.

---

## Part C — Invariant Test Suite (Normative)

The following invariants **MUST** be enforced by automated tests. Failure of any test is a release blocker.

### C.1 Snapshot Immutability
* **Invariant:** Snapshot Manifests are immutable.
* **Tests:**
    * Attempt to modify an existing Snapshot Manifest → **FAIL**.
    * Hash(manifest) changes after write → **FAIL**.

### C.2 Authority Exclusivity
* **Invariant:** Authority changes only via `AuthorityUpdate` events.
* **Tests:**
    * Create Snapshot without `AuthorityUpdate` → Authority unchanged.
    * Attempt to change authority implicitly → **FAIL**.

### C.3 Causal Completeness
* **Invariant:** No Snapshot without a parent snapshot and causal event.
* **Tests:**
    * `SnapshotProposed` without `parent_snapshot_id` → **FAIL**.
    * Snapshot without `causal_link` → **FAIL**.

### C.4 Veto Preservation
* **Invariant:** Rejected snapshots remain queryable.
* **Tests:**
    * Reject Snapshot *S*.
    * Query index for *S* → **FOUND** with `status=rejected`.
    * Authority points elsewhere.

### C.5 Trust Declaration Integrity
* **Invariant:** Trust level is explicit and propagated.
* **Tests:**
    * Optimistic run produces `trust_level=optimistic`.
    * Hermetic run produces `trust_level=hermetic`.
    * Cache reuse across trust levels → **FAIL**.

### C.6 Determinism Honesty
* **Invariant:** Determinism claims match execution mode.
* **Tests:**
    * Optimistic run **MAY** differ across machines.
    * Hermetic run **MUST** produce identical snapshot IDs.

### C.7 Log Non-Authority
* **Invariant:** Event Logs never decide truth.
* **Tests:**
    * Delete log entries → Snapshot authority unchanged.
    * Corrupt logs → Rebuild index from snapshots succeeds.

### C.8 Redaction Safety
* **Invariant:** Redaction never rewrites authoritative snapshots.
* **Tests:**
    * Redact blob via new snapshot → Old snapshot remains.
    * Authority moves to redacted snapshot.
    * No authoritative snapshot references redacted blob.

### C.9 Bundle Integrity (Protocol Interchange)
* **Invariant:** Exported bundles are self-verifying.
* **Tests:**
    * Export bundle → Import elsewhere → Same snapshot IDs.
    * Missing blob in bundle → **FAIL** verification.

---

## Part D — Operational Guardrails

* **Fail Closed:** If invariants cannot be verified, abort execution.
* **Explain Failures:** All invariant failures **MUST** emit structured diagnostics.
* **No Silent Repair:** Auto-healing is forbidden without events.

---

## Part E — Adoption Rule

Any feature that cannot be expressed as: **Snapshot + Event(s) + AuthorityUpdate** does not belong in dxs.

---

## Part F — North Star (Reaffirmed)

> **dxs is not a tool that changes files. It is a protocol that assigns meaning, trust, and authority to workspace states.**
