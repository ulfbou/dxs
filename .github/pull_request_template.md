## Summary
[One precise sentence]

## Links
Closes: #
Milestone: vX.Y.Z

## Type
- [ ] bug
- [ ] refactor
- [ ] behavior
- [ ] test-gap
- [ ] docs-gap

## Contracts / Invariants
- Touched: EXECUTION_CONTRACT §__, SESSION_LOG_MODEL §__
- Enforced: [list concrete guarantees]
- Preserved: [list invariants unchanged]

## Scope Boundary (Does NOT)
- 
-

## Verification (deterministic)
Commands run:
```
```
Expected:
Observed:

## Risk
- [ ] safe (refactor/docs/test only)
- [ ] contract-impacting (requires ADR)
- [ ] breaking (requires migration notes)

## Merge-Gate Checklist
- [ ] Builds cleanly with warnings as errors
- [ ] All CI jobs pass
- [ ] Links to exactly one issue of valid type
- [ ] Contracts / Invariants section completed with section numbers
- [ ] Scope Boundary completed
- [ ] No new project references violating layering
- [ ] PlanCompletenessHarness passes
- [ ] Dx.Architecture.Tests passes
- [ ] Invariant tests pass (dual-log, snapshot coupling, genesis)

### For behavior PRs additionally
- [ ] ADR created/updated in /docs/adr/
- [ ] EXECUTION_CONTRACT or SESSION_LOG_MODEL updated
- [ ] Breaking change migration notes provided if risk=breaking
