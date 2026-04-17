This is the green light. We are moving from the "Constitutional" phase to the "Implementation" phase. By treating these changes as a disciplined evolution of the existing `vNext` branch, we ensure the project gains the high-integrity substrate it requires without the catastrophic velocity loss of a rewrite.

Below are the three authoritative deliverables required to execute this migration.

---

## 1. SQL Migration Scripts (SQLite / vNext)

These scripts transition the existing schema to the **Layered Authority** model. Legacy data is preserved by defaulting to `optimistic` trust and `accepted` status.

```sql
-- Migration: 20260411_AddAuthorityAndTrust
-- Description: Adds semantic authority and trust levels to snapshots and logs.

BEGIN TRANSACTION;

-- 1. Update snap_handles with constitutional columns
ALTER TABLE snap_handles ADD COLUMN trust_level TEXT NOT NULL DEFAULT 'optimistic';
ALTER TABLE snap_handles ADD COLUMN status TEXT NOT NULL DEFAULT 'accepted';
ALTER TABLE snap_handles ADD COLUMN causal_id TEXT;

-- 2. Add Constraints (Note: SQLite requires a table recreation for complex CHECK constraints 
-- on existing tables, but for Phase 1, we enforce these via the Dispatcher/Code.)

-- 3. Extend session_log op_types
-- If session_log.op_type is a simple TEXT column:
-- No action needed other than documentation.
-- If it's a constrained ENUM (simulated), we ensure the Dispatcher 
-- acknowledges the new types: 'authority_update', 'veto', 'checkpoint'.

-- 4. Create an index for causal lookups to prevent performance regression
CREATE INDEX idx_snap_causal ON snap_handles(causal_id);

COMMIT;
```

---

## 2. Dispatcher Logic Checklist (The State Machine)

The Dispatcher must be updated to handle the transition from **Implicit Authority** (latest is best) to **Explicit Authority** (events decide truth).

### Flow A: Propose Snapshot (`dxs run`)
- [ ] Calculate `InputsHash` and `Command`.
- [ ] Create `session_log` entry of type `task_end`. Capture its `LogId`.
- [ ] Insert `snap_handles` record:
    - `status`: **proposed** (not accepted yet).
    - `causal_id`: The `LogId` from the previous step.
    - `trust_level`: **optimistic**.
- [ ] **Important:** Do *not* update the global `current_snap_id` handle yet.

### Flow B: Accept Snapshot (`dxs accept` / Default Success)
- [ ] Emit `session_log` entry of type `authority_update` referencing the `snap_id`.
- [ ] Update `snap_handles` set `status = 'accepted'` where `snap_id = X`.
- [ ] Update global state: Set `current_snap_id = X`.

### Flow C: Veto Snapshot (`dxs veto`)
- [ ] Emit `session_log` entry of type `veto` with `metadata.reason`.
- [ ] Update `snap_handles` set `status = 'rejected'` where `snap_id = X`.
- [ ] **Constraint:** The `current_snap_id` must remain at the last `accepted` snapshot.

---

## 3. C# Invariant Test Harness (xUnit)

This harness enforces the "Internal Constitution" at the code level. It targets the `IStateStore` and `IDispatcher` interfaces.

```csharp
public class ConstitutionalInvariantsTests
{
    private readonly IDispatcher _dispatcher;
    private readonly IStateStore _store;

    public ConstitutionalInvariantsTests(IDispatcher dispatcher, IStateStore store)
    {
        _dispatcher = dispatcher;
        _store = store;
    }

    [Fact]
    public async Task I1_Snapshot_Immutability_Check()
    {
        // GIVEN an existing snapshot
        var snapId = "snap_123";
        
        // WHEN attempting to write to the same snap_id with different data
        // THEN the store should throw a Concurrency or Mutability exception
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _store.InsertSnapshotAsync(new Snapshot { SnapId = snapId, Content = "new_data" }));
    }

    [Fact]
    public async Task I3_Causal_Link_Required()
    {
        // GIVEN a new snapshot proposal
        // WHEN the causal_id is missing or invalid
        var snap = new Snapshot { SnapId = "snap_456", CausalId = null };

        // THEN the dispatcher must reject the proposal
        var result = await _dispatcher.ProposeAsync(snap);
        Assert.False(result.Success);
        Assert.Contains("Causal integrity violation", result.Message);
    }

    [Fact]
    public async Task I4_Authority_Requires_Event()
    {
        // GIVEN a proposed snapshot S2 (parent S1)
        var s1 = _store.GetCurrentAuthority();
        var s2 = await _dispatcher.ProposeAsync(new TaskContext());

        // WHEN we check authority WITHOUT an authority_update event
        var current = _store.GetCurrentAuthority();

        // THEN authority must still be S1
        Assert.Equal(s1.SnapId, current.SnapId);
    }

    [Fact]
    public async Task I6_Rejected_Snapshots_Persist()
    {
        // GIVEN a proposed snapshot that is subsequently vetoed
        var snap = await _dispatcher.ProposeAsync(new TaskContext());
        await _dispatcher.VetoAsync(snap.Id, "Semantic error");

        // WHEN querying the store for that ID
        var retrieved = await _store.GetSnapshotAsync(snap.Id);

        // THEN it must still exist but carry the 'rejected' status
        Assert.NotNull(retrieved);
        Assert.Equal("rejected", retrieved.Status);
    }
}
```

---

### Final Implementation Note
By landing these three artifacts, `dxs` moves from a tool that *records history* to a protocol that *verifies authority*. The migration is safe because it uses standard SQL patterns, and the tests ensure that the new logic cannot be bypassed by "shortcuts" in the command-line implementation.

**The migration is ready for the dev branch.**

---

This is tight, executable, and—critically—coherent across storage, dispatcher, and tests. You’ve crossed the boundary from “architecture” into a **closed operational loop**.

What follows is a final hardening pass: resolving edge conditions, tightening contracts, and eliminating the last sources of ambiguity before this hits the dev branch.

---

# 1. SQL Migration — Final Adjustments

Your migration is sound, but there are two subtle integrity risks in SQLite that should be addressed.

## 1.1 Enforce Allowed Values (Without Table Rebuild)

Since SQLite CHECK constraints aren’t retrofitted cleanly, enforce via **partial indexes**:

```sql
-- Enforce valid trust levels
CREATE INDEX idx_snap_trust_valid 
ON snap_handles(trust_level)
WHERE trust_level NOT IN ('optimistic', 'hermetic');

-- Enforce valid status
CREATE INDEX idx_snap_status_valid 
ON snap_handles(status)
WHERE status NOT IN ('proposed', 'accepted', 'rejected');
```

Then in dispatcher:

* Any row returned by these indexes → **hard fail**

This gives you **constraint detection without table rebuild**.

---

## 1.2 Strengthen Causal Integrity (Soft FK)

SQLite FK enforcement is often disabled; assume worst case.

Add:

```sql
CREATE INDEX idx_log_id ON session_log(log_id);
```

Then enforce in code:

> `snap_handles.causal_id MUST resolve to session_log.log_id`

---

## 1.3 Authority Pointer Location (Clarify Now)

You referenced:

> “global current_snap_id handle”

Make this explicit **now**, or it will fragment later.

### Recommended:

```sql
CREATE TABLE authority_state (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    current_snap_id TEXT
);

INSERT INTO authority_state (id, current_snap_id) VALUES (1, NULL);
```

This avoids:

* hidden globals
* multi-writer ambiguity
* test instability

---

# 2. Dispatcher — Critical Ordering Guarantees

Your flows are correct, but **ordering is everything** in an event-backed authority model.

## 2.1 Flow A — Fix Event Semantics

Right now:

> Create `task_end` → use as causal_id

This is slightly off semantically.

### Correction:

Split into **two conceptual events**:

* `task_end` → execution finished
* `snapshot_proposed` → state candidate created

### Minimal fix (no new op_type required):

Keep `task_end`, but define:

> **The causal_id of a snapshot is the log entry that *first makes it observable***

So your current approach is acceptable if:

* `task_end` is treated as “snapshot materialization event”

Just document this explicitly.

---

## 2.2 Flow B — Atomicity Requirement (Non-Negotiable)

This must be a **single transaction**:

```text
BEGIN
  insert authority_update log
  update snap_handles.status = accepted
  update authority_state.current_snap_id
COMMIT
```

Otherwise you risk:

* authority drift
* partial acceptance
* invariant violations under crash

---

## 2.3 Flow C — Veto Guardrail

Add this invariant:

> You cannot veto a snapshot that is already `accepted` **and current authority**

Unless you explicitly support rollback.

So enforce:

```text
IF snap_id == current_snap_id → reject veto
```

(Phase 1 keeps authority monotonic)

---

## 2.4 Hidden Edge Case — Duplicate Proposals

You need one rule:

> Same `(inputs_hash, command, parent_snap_id)` → same snap_id

Otherwise:

* causal graph forks artificially
* cache explodes

---

# 3. Invariant Tests — Strengthening

Your tests are well targeted. Add three that close real gaps.

---

## 3.1 I2 — Parent Must Exist

```csharp
[Fact]
public async Task I2_Parent_Must_Exist()
{
    var snap = new Snapshot {
        SnapId = "snap_orphan",
        ParentSnapId = "non_existent",
        CausalId = "log_1"
    };

    var result = await _dispatcher.ProposeAsync(snap);

    Assert.False(result.Success);
    Assert.Contains("Parent snapshot not found", result.Message);
}
```

---

## 3.2 I5 — Trust Level Must Be Present

```csharp
[Fact]
public async Task I5_Trust_Level_Required()
{
    var snap = new Snapshot {
        SnapId = "snap_no_trust",
        CausalId = "log_1",
        TrustLevel = null
    };

    var result = await _dispatcher.ProposeAsync(snap);

    Assert.False(result.Success);
}
```

---

## 3.3 I4 (Extended) — Authority Must Match Log

```csharp
[Fact]
public async Task I4b_Authority_Must_Be_Log_Derived()
{
    var snap = await _dispatcher.ProposeAsync(new TaskContext());
    await _dispatcher.AcceptAsync(snap.Id);

    var authority = _store.GetCurrentAuthority();
    var log = await _store.GetLogEntriesForSnap(snap.Id);

    Assert.Contains(log, e => e.OpType == "authority_update");
}
```

---

# 4. One Missing Concept (Important but Small)

## “Accepted but Not Current” Snapshots

Your model implicitly allows:

* accepted snapshot
* but not current authority

This is **good** (branching), but must be explicit.

### Add invariant:

> There is exactly **one current authority**, but many accepted snapshots may exist.

---

# 5. Migration Safety Check

This is where your approach really holds up.

### Why this migration is safe:

* Defaults preserve legacy behavior:

  * `trust_level = optimistic`
  * `status = accepted`
* Existing flows map to:

  * implicit → explicit authority without breaking reads
* No destructive schema changes
* No re-keying required

---

# 6. What You Have Now (Systemically)

After landing this:

You have transformed dxs into:

### Before

* Append-only history
* Implicit “latest = truth”

### After

* Append-only **causal graph**
* Explicit **authority selection layer**
* Verifiable **state transitions**

That’s a categorical shift.

---

# 7. Final Go/No-Go

**Go. With the following conditions:**

* [ ] Authority updates are transactional
* [ ] Causal_id is always resolvable
* [ ] Authority pointer is explicitly stored
* [ ] Dispatcher enforces all invariants (not DB)

If those are satisfied:

> This is not just safe to land — it is foundational.

---

If you want the next step, I can produce:

* A concrete `IDispatcher` implementation skeleton (C#)
* A SQLite-backed `IStateStore` with all invariants enforced
* Or a migration verification script that asserts pre/post correctness on real data

That would take this from **design-complete → integration-ready** immediately.
