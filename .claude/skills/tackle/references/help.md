# Help output

Printed by `/tackle help`, and by a bare `/tackle` with no arguments (then ask
what the task is). Keep it to one screen — it is a reminder, not documentation.
Adapt the middle sections to the current project: show the real config values if
`.claude/tackle.md` exists, and say so if it doesn't.

---

**tackle** — run a task through define → validate → implement → review → manual review.

**Commands**

| | |
|---|---|
| `/tackle <task>` | Start a run. Takes a description, a path to a task file, or a Trello card URL. |
| `/tackle resume <slug>` | Continue a run from its recorded phase. |
| `/tackle status [<slug>]` | Print a run's state. Changes nothing. |
| `/tackle setup` | Detect and write this project's `.claude/tackle.md`. |
| `/tackle refine <feedback>` | Change how the workflow itself behaves. |
| `/tackle help` | This. |

**Phases** — two gates, both marked

1. **Define** — clarify, write acceptance criteria, pick the branch → **you approve**
2. **Validate** — build the checks, confirm they fail for the right reason, review them
3. **Implement** — iterate until validation passes
4. **Review** — 4 reviewers in parallel, findings addressed, re-validated; review guide written
5. **Manual review** — guide + packet handed over → **you accept or send it back**
6. **Close out** — update task and docs, one local commit if asked

Anything unresolvable — a disagreement between agents, a question only you can
answer — stops the run and comes to you, at any phase.

**Reviewers** — correctness (never skipped), performance, design, docs.
Configurable per project except correctness.

**Where things go**

- Task definition: `docs/tasks/<slug>.md` or a Trello card
- Run state, guide, captures: `.claude/tackle/runs/`
- Review guide: `<slug>.review-guide.html` — **open in a browser**, not the IDE

**Not done for you:** pushes, tags, releases, merges. Those stay yours.
