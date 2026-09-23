# Run log

Path: `.claude/tackle/runs/<slug>.md`, created at run start.

- Lets `/tackle resume <slug>` continue without re-deriving anything, and makes
  `status` cheap.
- Written after every phase transition, gate, and escalation.
- State, not prose — keep it terse.

```markdown
# Run: <slug>

- **Task definition:** <path or Trello card url>
- **Backend:** markdown | trello
- **Base commit:** <sha at run start>
- **Branch:** <branch> (created by this run: yes | no)
- **Phase:** 1–6
- **Blocked on:** none | <what the user was asked>

## Validation
- Command: `<how to run it>`
- Last result: pass | fail (<date>)
- Criteria coverage: 1→`TestFoo`, 3→`TestBar`, 2→manual, 4→review

## Phase history
- P1 approved <date> — decisions: <one line each>
- P2 rounds: 2, converged
- P3 <date> — validation green
- P4 round 1: correctness 2 must-fix, perf 1 should-fix, design 0, docs 1 → addressed
- P4 round 2: all clear
- P5 <date> — packet delivered, awaiting manual review

## Open items
- <finding or question, who raised it, current state>
```

**On `resume`:** read this file and the task definition, re-run validation to
confirm the recorded state, then continue from `Phase:`. If the working tree
changed since, say so first.

`.claude/tackle/runs/` holds everything a run produces: this log, findings, the
review guide and its `<slug>.assets/`, and the `<slug>.base/` snapshot for
non-git projects. Suggest gitignoring it unless the user wants the history.
Mention its size at close-out if captures have piled up.
