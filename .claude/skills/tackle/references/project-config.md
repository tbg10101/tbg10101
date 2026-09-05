# Project config — `.claude/tackle.md`

Optional, project root. Absent is fine — every key has a safe default and the
workflow detects what it can. Run `/tackle setup` to generate it; see
`setup.md` for the detection procedure. When you find yourself asking the same project question
twice, offer to write it here.

```markdown
---
task_backend: markdown        # markdown | trello
task_path: docs/tasks         # markdown backend: directory for task files
trello_board: <board id>      # trello backend
trello_done_list: <list id>
validate: dotnet test         # command that runs the automated checks
build: dotnet build           # compile-check command, run at phase boundaries
reviewers_add: [security]     # extra reviewer agent names (tackle-review-<name>)
reviewers_skip: [performance] # reviewers that don't apply here; correctness can never be skipped
commit: ask                   # ask | never | always — the single phase-6 commit
branch: auto                  # auto | never — auto branches off protected branches only
protected_branches: []        # extra regexes treated as protected, e.g. ['^stable-.*']
ide: rider                    # none (default) | rider | jetbrains-<product> | vscode
ide_project: <sln basename>   # e.g. capsulecolliders64-test  (IDE-driven links only)
ide_project_root: <dir>       # repo-relative dir containing the .sln
---

## Notes for agents

Free-form. Anything an implementor or reviewer should know that isn't in
CLAUDE.md: how to run the app, where the docs live that must be kept in sync,
known-flaky checks, the definition of "hot path" in this codebase.
```

## Unity / DOTS projects

`validate` is usually not a shell command — it runs through the Unity MCP
server. Put the real procedure in the Notes section and set `validate:` to a
short label the orchestrator can echo. The saved Unity MCP workflow memory
covers the timing and the pitfalls (busy-vs-broken bridge responses, play-mode
delays, silent Burst fallback to managed); implementor and reviewers should
follow it rather than inventing their own polling.
