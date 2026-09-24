# Task backends

Set by `task_backend:` in `.claude/tackle.md`. If absent: a Trello card URL/id in
the invocation → `trello`, otherwise `markdown`.

The definition must stand alone: no run-folder paths (`run-log.md`). Put the
content itself in it (playtest script, contracts, noted-not-addressed findings).

## markdown

- Path: `task_path` from config, else `docs/tasks/<slug>.md`.
- The file is the source of truth. Edit it in place across phases: check off
  criteria, empty open questions, add the playtest script.
- Committed with the work unless the config says otherwise.

```markdown
# <Title>

- **Status:** define | validate | implement | review | manual-review | done
- **Slug:** <slug>
- **Base commit:** <sha>

## Summary
## Context
## Scope
## Out of scope

## Acceptance criteria
1. `[auto]` … — checked by `<test name / command>`
2. `[manual]` … — checked by playtest script below
3. `[review]` … — checked by `<reviewer>`

## Playtest script
## Open questions
## Decisions
<!-- user answers from phase 1, dated, so later sessions don't re-ask -->
```

## trello

Tools: `mcp__claude_ai_MCP_for_Trello__*`.

- **Description**: Summary, Context, Scope, Out of scope, Playtest script — same
  structure as above.
- **Acceptance criteria**: a checklist named "Acceptance criteria"
  (`create_checklist`, `add_checkitem`), one tagged item per criterion. Tick with
  `update_checkitem` in phases 4–6.
- **Questions and decisions**: comments (`add_comment`), preserving history.
  Post the phase-1 answers as a "Decisions" comment.
- **Close-out**: move the card to the **top** of the done list (`update_card`
  with the list id and `pos: "top"`) so the list reads newest-first. Ask for the
  list once; record it in `.claude/tackle.md`.
- Never delete or archive cards, or create boards.
- The board is visible to others: post the definition and the close-out
  comment, not every intermediate round.

If a Trello call fails or the server is unavailable, tell the user and ask.
Never silently fall back to markdown.
