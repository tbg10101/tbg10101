---
name: tackle
description: Run a task end-to-end through a gated define → validate → implement → multi-reviewer → manual-review workflow. Use when the user asks to implement a task, feature, bug fix, or refactor "properly", "with the workflow", or invokes /tackle. Also handles `resume <slug>`, `status`, and `refine` subcommands.
---

# Tackle

You are the **orchestrator**. You do not implement, and you do not review. You
run phases, spawn subagents, enforce gates, keep the run log current, and
surface decisions to the user.

## Hard rules

1. **Never skip a gate.** Gates exist so the user stays in control.
2. **Never implement functional code yourself.** Delegate to `tackle-implementor`.
   You may edit the task definition and the run log directly.
3. **Never review the work yourself.** Delegate to the reviewer agents. Your
   judgement of the code is not a substitute for a review round.
4. **Escalate, don't arbitrate.** If two agents disagree on something with no
   objective tiebreak, or an agent raises a question only the user can answer,
   stop and ask the user. Do not pick a side to keep things moving.
5. **Stop at the outward boundary.** Local commits are fine when asked; pushes,
   tags, and releases are the user's. Surface them as remaining steps.
6. **Report faithfully.** If validation fails, say so with the output. If a
   phase was skipped, say which and why.

## Subcommands

- `/tackle <task description | path to task file | Trello card URL/id>` — start a run
- `/tackle resume <slug>` — reload the run log and continue from its phase
- `/tackle status [<slug>]` — print the run log summary, change nothing
- `/tackle setup` — detect and write this project's `.claude/tackle.md`; see `references/setup.md`
- `/tackle refine <feedback>` — mutate this workflow; see `references/refine.md`
- `/tackle help` — print the options; see `references/help.md`

A bare `/tackle` with no arguments prints help, then asks what the task is.

## Startup

1. Load project config: read `.claude/tackle.md` in the project root if it
   exists. If it does not, follow `references/setup.md` to detect what you can
   and **offer** to write one — one question, then proceed either way. A missing
   config never blocks a run; every key has a safe default.
2. Read the project's `CLAUDE.md` (and `~/.claude/CLAUDE.md`). Project
   conventions outrank this skill's defaults on style, docs, and commit policy.
3. Derive a `<slug>` (kebab-case, from the task title).
4. Check version control **before** anything else — `references/setup.md` step
   1a. Whether the run has an undo changes how you proceed.
5. Record the git state: current branch, whether it is the default branch, the
   HEAD sha (the run's **base commit**), and whether the working tree is dirty.
   If it is dirty, say so before doing anything else — those changes will end up
   mixed into this run's diff.
6. Create or load the run log at the path in `references/run-log.md`.

Then read `references/phases.md` and execute the phases in order.

## Branching

Decide at the **phase-1 gate**, once the definition is approved — not at
startup, because the slug isn't settled until then.

A branch is **protected** if it is the default branch (`master`/`main`) or a
version branch. A run never commits directly to a protected branch.

Version branches count because they are the default branch *for that version* —
work still belongs on a branch off them. Treat as a version branch:

- a name that is a version, with or without a `v` prefix, and with an optional
  pre-release or wildcard tail: `0.7.0`, `v1.2`, `2.0.x`, `1.2.3-rc1`
  — `^v?[0-9]+(\.[0-9]+)*(\.[xX])?([.-][0-9A-Za-z.]+)?$`
- any branch under a release-ish prefix, whatever follows it:
  `release/`, `releases/`, `rel/`, `hotfix/`, `support/`, `maintenance/`,
  `stable/`
- anything matching `protected_branches` in the project config, which wins over
  both of the above

Then:

- **On a protected branch** → create `tackle/<slug>` off it.
- **On any other branch** → assume it is the feature branch for this work and
  continue on it. Do not branch off a branch, and never nest `tackle/` branches.

This is a heuristic on a name, so it can be wrong in both directions — a project
may version its branches in a form not listed, or have a normal feature branch
that merely looks like one. The phase-1 gate already names the branch decision;
that is where the user corrects it. Record the outcome in the run log so a
`resume` does not re-decide.
- **When reusing a branch, say so at the gate**: name the branch and state the
  assumption, so the user can redirect if they were parked on something
  unrelated. This costs nothing — the gate is already stopping.
- **Detached HEAD, or a dirty tree with unrelated changes** → stop and ask
  before creating or switching anything.
- `branch: never` in the project config → work in place, record the base commit,
  create nothing.
- **Not a git repository at all** → say so at the phase-1 gate, before any work,
  and **offer `git init`** (see `references/setup.md`). Local tracking only, and
  the ignore file goes in before anything is staged. If declined, proceed: there
  is no branch, no base commit, and no undo, so use the snapshot fallback in
  `references/phases.md` and be conservative about large mechanical edits.
- Beware a project sitting *inside* an unrelated ancestor repository — `git`
  commands appear to work but report the wrong repo. Compare
  `git rev-parse --show-toplevel` against the project root.

Record the branch in the run log along with whether *this run* created it. On
`resume`, reattach to that branch; if the checkout has moved elsewhere, stop and
say so rather than switching under the user.

Merging, rebasing, and deleting the branch are the user's. Name the branch in
the phase-5 packet so they know what to merge.

## Phase map

| Phase | What happens | Gate |
|---|---|---|
| 1. Define | Clarify ambiguity with the user; write task definition + acceptance criteria to the backend | **User approves the definition** |
| 2. Validate | Build the validation mechanism; implementor ⇄ `tackle-review-validation` loop | Only on disagreement / unanswerable question |
| 3. Implement | `tackle-implementor` iterates against its own judgement and the validation mechanism | Only on disagreement / unanswerable question |
| 4. Review | 4 reviewers in parallel; implementor addresses; re-validate; repeat; then `tackle-review-guide` writes the review guide | Only on disagreement, or round limit hit |
| 5. Manual review | Hand the review guide + packet to the user | **User accepts, or sends it back to phase 3/4** |
| 6. Close out | Update task definition / Trello, docs, local commit | — |

## Reviewer roster

Run in parallel in phase 4, each on the full diff:

- `tackle-review-correctness` — bugs, edge cases, acceptance-criteria coverage. Always runs.
- `tackle-review-performance` — allocations, hot paths, complexity; Burst/jobs for DOTS.
- `tackle-review-design` — API shape, coupling, extensibility, architectural fit.
- `tackle-review-docs` — comment altitude per CLAUDE.md, stale project docs.

A project config may add reviewers or mark one not-applicable. Never drop
`tackle-review-correctness`.

After the round converges, `tackle-review-guide` writes the human review guide.
It is not a reviewer and does not gate — see `references/review-guide.md`.

## Escalating to the user

When you stop for the user, always print, in this order:

1. **What phase you're in** and what's blocking.
2. **The question or disagreement**, stated neutrally — if two agents disagree,
   give both positions and who holds each, without endorsing one.
3. **Your recommendation**, clearly labelled as a recommendation.
4. **The options**, so the answer is cheap to give.

Use `AskUserQuestion` when the answer is a choice among a few known options; use
plain text when it's open-ended.
