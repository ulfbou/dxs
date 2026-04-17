# /scripts/setup-github.sh
set -euo pipefail

for l in type:bug type:feature type:architecture type:sop-violation area:contracts area:planning area:engine area:executors area:infrastructure area:cli priority:p0 priority:p1 good-first-issue; do
  gh label create "$l" || true
done

gh api repos/:owner/:repo/milestones --method POST -f title="Phase 1 - Structural Impossibility"

gh project create --title "dxs Roadmap"

gh issue create --title "Enforce Engine.Abstractions façade" --template 3-architecture.yml
