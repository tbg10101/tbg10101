# Phases

Validate or compile-check at the end of every phase before starting the next.
Write the run log after every phase transition and every gate.

---

## Phase 1 — Define

**Goal:** a task definition nobody has to guess at, with acceptance criteria
that can actually be checked.

1. Read the source material: the user's prompt, the existing task file, or the
   Trello card (`mcp__claude_ai_MCP_for_Trello__get_card` + `get_comments` +
   `get_card_checklists`).
2. Explore the codebase enough to ask *informed* questions. Ambiguity you can
   resolve by reading the code is not a question — resolve it.
3. Ask the user about everything left. Batch the questions into one
   `AskUserQuestion` call (up to 4) where the answers are bounded choices; use
   plain text for open-ended ones.

   **Every question leads with a recommended option, marked as such, with the
   trade-off stated.** That is what makes asking cheap: a question with a
   defensible default costs a glance, so ask the extra one rather than assuming.
   A question you cannot recommend an answer to is usually one you have not
   researched enough yet — go read the code first. Never pad the list with
   questions that have an obvious answer.

   Ask about at minimum, where unclear:
   - scope boundaries — what is explicitly *not* in this task
   - behaviour under edge cases and failure
   - how it will be verified, including anything only a human can check
   - performance / compatibility constraints
   - **lifecycle** — what happens when the thing this depends on changes *after*
     it is built. Most work produces an artifact derived from something else;
     ask what should happen when that something else is edited, removed or
     reordered later.
   - **who initiates** — for anything that mutates authored or user-owned data:
     automatic, or on demand? This is a workflow preference, never inferable
     from the code, and expensive to change once built.
   - **limits and sizing** — the concrete numbers the user already knows and you
     would otherwise guess: how big, how many, how often.

   **Then draft the phase-5 "Your call" list now, and ask everything on it that
   does not need code to answer.** Those are the decisions tests and reviewers
   cannot settle, and every one that survives to phase 5 is a decision made
   without the only person qualified to make it — often after the code that
   assumes an answer already exists. Questions about the artifact are the easy
   ones; the costly misses are about its lifecycle.
4. Write the definition to the backend (see `task-backends.md`). It must contain:
   - **Summary** — one paragraph
   - **Context / why**
   - **Scope** and **Out of scope**
   - **Acceptance criteria** — a numbered list. Each criterion states how it is
     checked, and is tagged exactly one of:
     - `[auto]` — a unit/integration test, benchmark, or command asserts it
     - `[manual]` — the user must observe it (playtesting, visual, feel)
     - `[review]` — satisfied by reviewer judgement, not a runnable check
   - **Open questions** — empty by the time the gate passes
5. **GATE:** show the definition to the user and ask for approval, together
   with the **branch decision** (see `Branching` in SKILL.md): either the
   `tackle/<slug>` branch you will create, or the existing feature branch you
   will continue on, named explicitly. Do not proceed on silence or on "looks
   fine, but…" — resolve the "but" first.
6. Once approved, create or switch to the branch and record it in the run log.

If every criterion is `[manual]` or `[review]`, say so explicitly at the gate —
phase 2 will have little to build, and the user may want to add an `[auto]` one.

---

## Phase 2 — Build the validation mechanism

**Goal:** the checks exist and are trustworthy *before* the implementation
exists, so they can't be shaped to fit a bug.

1. If there are no `[auto]` criteria, skip to phase 3 and record the skip and
   its reason in the run log.
2. Spawn `tackle-implementor` to write the validation mechanism only — tests,
   fixtures, benchmark harness, scripted checks. Tell it explicitly: **do not
   write the functional implementation.**
3. Verify the checks are meaningfully **red**: run them and confirm they fail
   for the right reason (the behaviour is missing), not for a setup error. A
   test that passes before the implementation exists is not a test.
4. Spawn `tackle-review-validation` on the validation code.
5. Loop: implementor addresses findings → reviewer re-reviews → repeat until the
   reviewer reports no must-fix findings and the implementor has no outstanding
   objections. **Cap: 3 rounds.** On round 3 without convergence, escalate the
   remaining disagreement to the user. Batch findings and re-check with a fresh
   narrow agent, per phase 4 steps 4-5 — the same cost rules apply here.
6. Record in the run log: which criteria are covered by which check, and which
   criteria remain `[manual]`/`[review]`.

For `[manual]` criteria, do not write code — write a **playtest script**: the
exact steps for the user to perform and what they should observe. It goes in the
task definition and is reused verbatim in phase 5.

---

## Phase 3 — Implement

1. Spawn `tackle-implementor` with: the approved task definition, the validation
   mechanism and how to run it, the project config, and the project CLAUDE.md
   conventions.
2. The implementor iterates on its own until validation passes *and* it is
   satisfied by its own judgement. It reports back with: what it changed and
   why, validation output, assumptions made, and anything it deliberately left
   out.
3. Run the validation yourself to confirm the reported result. Do not take the
   implementor's word for a green run.
4. If validation cannot be made to pass because a criterion turns out to be
   wrong or infeasible: **stop and escalate.** Never edit an acceptance
   criterion to match the implementation without the user agreeing.

---

## Phase 4 — Multi-reviewer round

1. Produce the diff once (`git diff <base commit>`, recorded in the run log) and
   give every reviewer the same diff, the task definition, and the acceptance
   criteria. A run makes no intermediate commits, so this is a working-tree
   diff — if the tree was dirty at run start, those changes are in it, and you
   must say so to the reviewers.
   **If the project is not a git repository**, there is no diff to take. Before
   phase 3 begins, copy the files the task will touch into
   `.claude/tackle/runs/<slug>.base/`, and produce the review diff with
   `diff -ru` against that snapshot. Record in the run log that the diff is
   snapshot-based and which files were snapshotted — anything edited that wasn't
   snapshotted will be invisible to the reviewers, so widen the snapshot if the
   implementor's scope grows.
2. Spawn all applicable reviewers **in parallel, in a single message**.
3. Collect findings. Each finding has a severity: `must-fix`, `should-fix`,
   `consider`. Merge duplicates across reviewers, keeping the highest severity.
   - **Findings go to a file, not through your context twice.** Each reviewer
     writes `.claude/tackle/runs/<slug>.findings/<reviewer>-r<N>.md` and returns
     only a digest: counts by severity, and one line per `must-fix`. Read a full
     file when you need to judge or merge it; hand the implementor the paths.
     Otherwise every finding is paid for three times — the reviewer writing it,
     you reading it, you restating it.
4. Spawn `tackle-implementor` to address them. It must respond to every `must-fix`
   and `should-fix` with either a change or a reasoned rebuttal — silently
   dropping a finding is not allowed.
   - **Send one batch per round, never one finding at a time.** Every resume
     re-sends the agent's whole transcript, so a round costs what the agent has
     accumulated, not what you asked. Hold `consider` items and stragglers for
     the next batch.
   - Apply prose-only findings yourself (SKILL.md rule 2) and tell the
     implementor you did, so it does not redo them.
   - **Recycle a fat implementor.** Past roughly 150k tokens of accumulated
     transcript, spawn a fresh one with a written handoff instead of resuming.
5. Re-run validation. Then re-check the addressed findings — **with a fresh,
   narrowly-scoped agent given the fix diff and the original finding, not by
   resuming the reviewer.** A resumed reviewer re-sends its whole history to
   answer a question about twenty lines. Resume only when the check genuinely
   needs the reviewer's own prior reasoning, and say why in the run log.
6. Repeat until no `must-fix` findings remain and the implementor's rebuttals
   are accepted. **Cap: 3 rounds.**
   - Implementor rebuts and reviewer accepts → resolved, log it.
   - Implementor rebuts and reviewer holds → **escalate to the user**, with both
     positions stated neutrally.
   - Round 3 ends with open `must-fix` items → escalate.
7. `consider` findings that nobody acts on are not failures. Carry them into the
   phase 5 packet as "noted, not addressed" so the user can decide.
   - **Re-entering phase 4 after a phase-5 send-back** runs a *delta* review:
     only the reviewers whose domain the change actually touches, on the delta
     rather than the whole diff. Say in the run log and the packet which
     reviewers did not re-run and why. A send-back is where a run's cost
     doubles, and it is also where regressions land — scope it, do not skip it.
8. Once findings are resolved and validation is green, spawn
   `tackle-review-guide` to write the review guide. Give it the final diff, the
   task definition, every reviewer's findings and their resolutions, the
   validation output, and the project config. It writes
   `.claude/tackle/runs/<slug>.review-guide.html` (plus a markdown copy). Do not
   write this yourself —
   tiering the diff requires having read the code, which you have not.

---

## Phase 5 — Manual review handoff

Deliver the **review guide** written at the end of phase 4. Send the HTML file
and give the `open` command for it; say plainly that it must be opened **in a
browser**, not in the IDE, or the links stop working. The intended setup is a
browser window beside the IDE. Alongside it, a short **review packet** in the
conversation:

- **Review guide** — the HTML path, sent as a file, with the browser caveat.
- **What changed** — a short prose summary, then the file list with one line each.
- **Acceptance criteria table** — each criterion, its tag, and its status:
  `[auto]` → the check and its result; `[review]` → which reviewer signed off;
  `[manual]` → **awaiting you**, with the playtest script from phase 2.
- **Validation output** — the actual run, not a paraphrase.
- **Review summary** — findings raised and resolved, per reviewer; plus anything
  noted-but-not-addressed and why.
- **Assumptions and deviations** — every assumption the implementor made, and
  anything deliberately left out of scope.
- **Branch** — the branch the work is on, and whether this run created it.
- **Remaining outward steps** — merge / push / tag / release, listed for the
  user to do.

**GATE.** The user either accepts, or sends it back with feedback. Feedback that
changes behaviour re-enters at phase 3 (or phase 1 if it changes the definition);
feedback about code quality re-enters at phase 4.

For visual or rendering behaviour, do not attempt to verify with screenshots or
scene captures. Ask the user a specific, disambiguating question about what they
observe, and add temporary debug output where it makes the answer unambiguous.

---

## Phase 6 — Close out

1. Update the task definition / Trello card: check off acceptance criteria, move
   the card to its done list, add a comment summarising the outcome.
2. Update project documentation affected by the change — grep the docs for names,
   data-model statements, and workflow steps the change touched, and fix stale
   ones. This is part of "done", not an optional extra.
3. **One** local commit for the whole run, if the user asked for one or the
   project config says to. Short message per `~/.claude/CLAUDE.md` — reasoning
   belongs in documentation files, not the commit message. This is the run's
   only commit; there are no per-phase checkpoints.
4. Finalise the run log and tell the user what remains for them to do.
