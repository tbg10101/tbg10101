# Run log

Path: `.claude/tackle/runs/<slug>.md`, created on run start.

It exists so a fresh session can pick the run up with
`/tackle resume <slug>` without re-deriving anything, and so `status` is
cheap. Write it after every phase transition, every gate, and every escalation.
Keep it terse — it is state, not prose.

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

On `resume`: read this file, re-read the task definition, re-run validation to
confirm the recorded state is still true, then continue from `Phase:`. If the
working tree has changed since the recorded state, say so before continuing.

`.claude/tackle/runs/` holds everything a run produces: this log, the review
guide, its `<slug>.assets/` images, and the `<slug>.base/` snapshot for non-git
projects. It is run state, not project source — suggest gitignoring it unless
the user wants the history committed. Captures accumulate, so mention the
directory at close-out if it has grown.
