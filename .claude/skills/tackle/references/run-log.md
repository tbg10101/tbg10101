# Run log

Path: `<run>/run.md`, created at run start. `<run>` is the run's own folder,
`.claude/tackle/runs/<slug>/`; every file the run writes goes inside it.

Nothing outside `<run>` may cite it: not the task definition, project docs,
code, comments, or commit messages. Run folders are local, gitignored and
disposable, and anything committed or shared outlives them. Carry the content
itself, or leave it out.

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

Layout of `<run>/`:

- `run.md` — this log
- `findings/<reviewer>-r<N>.md` — reviewer output
- `review-guide.html`, `review-guide.md`, `assets/` — the phase-4 guide
- `base/` — the snapshot for non-git projects
- anything else the run writes (diffs handed to reviewers, playtest scripts) —
  loose in `<run>/`, never in `runs/` itself

Suggest gitignoring `.claude/tackle/runs/` unless the user wants the history.
Mention its size at close-out if captures have piled up.
