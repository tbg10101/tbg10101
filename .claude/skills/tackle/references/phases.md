# Phases

- Compile-check or validate at the end of every phase before starting the next.
- Write the run log after every phase transition, gate, and escalation.

---

## Phase 1 — Define

**Goal:** a definition nobody has to guess at, with checkable acceptance
criteria.

1. Read the source: the prompt, the task file, or the Trello card
   (`get_card`, `get_comments`, `get_card_checklists`).
2. Read enough code to ask informed questions. Anything the code answers is not
   a question.
3. Ask about everything left, batched — one `AskUserQuestion` (up to 4) for
   bounded choices, plain text for open-ended ones.
   - Lead every question with a recommended option and its trade-off. A
     defensible default makes a question cheap, so ask rather than assume.
   - No recommendation possible → research more first.
   - Never pad with questions that have an obvious answer.

   Cover, where unclear:
   - **scope** — what is explicitly out
   - **edge cases and failure** behaviour
   - **verification** — including what only a human can check
   - **performance / compatibility** constraints
   - **lifecycle** — what happens when the thing this derives from is edited,
     removed, or reordered after it's built
   - **who initiates** — for mutations of authored or user-owned data: automatic
     or on demand? Never inferable from code; expensive to change later.
   - **limits and sizing** — how big, how many, how often

   Then draft the phase-5 "Your call" list and ask everything on it that doesn't
   need code to answer. Each item that survives to phase 5 is a decision made
   without the user, usually after code already assumes an answer. Lifecycle
   questions are the usual misses.
4. Write the definition to the backend (`task-backends.md`):
   - **Summary** — one paragraph
   - **Context / why**
   - **Scope** and **Out of scope**
   - **Acceptance criteria** — numbered; each says how it is checked and carries
     exactly one tag:
     - `[auto]` — a test, benchmark, or command asserts it
     - `[manual]` — the user must observe it
     - `[review]` — reviewer judgement
   - **Open questions** — empty before the gate passes
5. **GATE:** present the definition and the branch decision (SKILL.md
   `Branching`) — the branch to create or the one to continue on, by name.
   - If no criterion is `[auto]`, say so; the user may want to add one.
   - Don't proceed on silence or "looks fine, but…" — resolve the "but".
6. Once approved, create or switch to the branch and record it.

---

## Phase 2 — Build the validation mechanism

**Goal:** trustworthy checks that exist before the implementation, so they
can't be shaped to fit a bug.

1. No `[auto]` criteria → skip to phase 3; log the skip and reason.
2. Spawn `tackle-implementor` for the validation mechanism only: tests,
   fixtures, harnesses, scripted checks. State explicitly: no functional code.
3. Run the checks and confirm they are red for the right reason — the behaviour
   is missing, not a setup error. A check green before implementation is not a
   check.
4. Spawn `tackle-review-validation` on the validation code.
5. Loop until no `must-fix` remains and the implementor has no objections.
   - **Cap: 3 rounds**, then escalate what's left.
   - Batching and re-check rules as phase 4 steps 4–5.
6. Log which check covers which criterion, and which remain
   `[manual]`/`[review]`.

For `[manual]` criteria, write a **playtest script** instead of code: exact
steps and what to observe. It goes in the task definition and is reused
verbatim in phase 5.

---

## Phase 3 — Implement

1. Spawn `tackle-implementor` with the approved definition, the validation
   mechanism and how to run it, the project config, and CLAUDE.md conventions.
2. It iterates until validation passes and it is satisfied, then reports
   changes, validation output, assumptions, and omissions.
3. Re-run validation yourself. Don't take a reported green on trust.
4. A criterion that proves wrong or infeasible → escalate. Never edit a
   criterion to match the implementation without the user.

---

## Phase 4 — Multi-reviewer round

1. Produce the diff once (`git diff <base commit>`) and give every reviewer the
   same diff, task definition, and criteria.
   - It is a working-tree diff; runs make no intermediate commits. If the tree
     was dirty at start, tell the reviewers.
   - **No git:** before phase 3, copy the files the task will touch to
     `.claude/tackle/runs/<slug>.base/` and diff with `diff -ru`. Log which
     files were snapshotted; widen the snapshot if scope grows, or edits
     outside it are invisible to reviewers.
2. Spawn all applicable reviewers **in parallel, in one message**.
3. Collect findings — severity `must-fix`, `should-fix`, or `consider`. Merge
   duplicates, keeping the highest severity.
   - Each reviewer writes `.claude/tackle/runs/<slug>.findings/<reviewer>-r<N>.md`
     and returns a digest only. Read a file when you need to judge or merge it;
     hand the implementor paths, not restatements.
4. Spawn `tackle-implementor` to address them. Every `must-fix` and `should-fix`
   gets a change or a reasoned rebuttal — never silence.
   - One batch per round. Every resume re-sends the agent's whole transcript,
     so hold `consider` items and stragglers for the next batch.
   - Apply prose-only findings yourself (SKILL.md rule 2) and tell the
     implementor.
   - Past ~150k tokens of transcript, spawn a fresh implementor with a written
     handoff instead of resuming.
5. Re-run validation. Re-check addressed findings with a **fresh instance of the
   same reviewer type**, given only the fix diff and the original finding.
   Resume the original reviewer only when its prior reasoning is genuinely
   needed, and log why.
6. Repeat until no `must-fix` remains and rebuttals are accepted. **Cap: 3
   rounds.**
   - Rebuttal accepted → resolved; log it.
   - Rebuttal rejected → escalate with both positions.
   - Round 3 ends with open `must-fix` → escalate.
7. Unaddressed `consider` findings are not failures; carry them into the packet
   as "noted, not addressed".
8. **Re-entry after a phase-5 send-back** is a delta review: only reviewers whose
   domain the change touches, on the delta only. Log and report which reviewers
   didn't re-run and why. Scope it; don't skip it — regressions land here.
9. Once findings are resolved and validation is green, spawn
   `tackle-review-guide` with the final diff, task definition, all findings and
   resolutions, validation output, and project config. It writes
   `.claude/tackle/runs/<slug>.review-guide.html` plus a markdown copy. Don't
   write it yourself; tiering needs the code read, and you haven't.

---

## Phase 5 — Manual review handoff

Send the review guide HTML as a file with its `open` command. It must be opened
in a **browser**, not the IDE, or the links break; the intended setup is a
browser window beside the IDE.

Alongside it, a short packet:

- **What changed** — a short summary, then files with one line each.
- **Acceptance criteria** — each with tag and status: `[auto]` → check and
  result; `[review]` → which reviewer signed off; `[manual]` → awaiting the
  user, with the playtest script.
- **Validation output** — the actual run.
- **Review summary** — findings raised and resolved per reviewer; anything
  noted but not addressed, and why.
- **Assumptions and deviations** — every implementor assumption and deliberate
  omission.
- **Branch** — name, and whether this run created it.
- **Remaining outward steps** — merge, push, tag, release.

**GATE.** The user accepts or sends it back:

- behaviour change → phase 3 (phase 1 if the definition changes)
- code quality → phase 4

Never verify visual or rendering behaviour from screenshots. Ask the user a
specific, disambiguating question; add temporary debug output where it makes the
answer unambiguous.

---

## Phase 6 — Close out

1. Update the task: check off criteria, move a Trello card to its done list,
   add an outcome comment.
2. Grep the project docs for names, data-model statements, and workflow steps
   the change touched; fix stale ones.
3. **One** local commit for the whole run, if the user asked or the config
   says to. Short message per `~/.claude/CLAUDE.md`.
4. Finalise the run log and list what remains for the user.
