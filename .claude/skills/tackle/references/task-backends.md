# Task backends

The backend is set by `.claude/tackle.md` (`task_backend:`). If absent,
detect: a Trello card URL/id in the invocation → `trello`; otherwise `markdown`.

## markdown

- Path: `task_path` from config, else `docs/tasks/<slug>.md`.
- The file *is* the source of truth. Edit it in place across phases — check off
  criteria, empty the open-questions section, append the playtest script.
- It is committed with the work unless the project config says otherwise.

Template:

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

- Card **description** holds Summary / Context / Scope / Out of scope /
  Playtest script, in the same markdown structure as above.
- Acceptance criteria go in a **checklist named "Acceptance criteria"**
  (`create_checklist`, `add_checkitem`), one item per criterion, each prefixed
  with its `[auto]` / `[manual]` / `[review]` tag. Tick with `update_checkitem`
  in phases 4–6 as each is satisfied.
- Open questions and decisions go in **comments** (`add_comment`), so the
  history is preserved. Post the phase-1 answers as a "Decisions" comment.
- Phase 6 moves the card to the done list (`update_card` with the list id) —
  ask which list once and record it in `.claude/tackle.md`.
- Do not delete or archive cards. Do not create boards.
- Trello is outward-facing to anyone else on the board: post the definition and
  the close-out comment, but do not narrate every intermediate round there.

If a Trello call fails or the MCP server is unavailable, do not silently fall
back to markdown — tell the user and ask.
