# Project config — `.claude/tackle.md`

Optional, in the project root. Every key has a safe default. Generate with
`/tackle setup` (`setup.md`). When you ask the same project question twice,
offer to record the answer here.

```markdown
---
task_backend: markdown        # markdown | trello
task_path: docs/tasks         # markdown: directory for task files
trello_board: <board id>      # trello
trello_done_list: <list id>
validate: dotnet test         # runs the automated checks
build: dotnet build           # compile-check, run at phase boundaries
reviewers_add: [security]     # extra agents, named tackle-review-<name>
reviewers_skip: [performance] # never correctness
models: {performance: opus}   # per-agent model override: sonnet | opus | haiku | fable
commit: ask                   # ask | never | always — the single phase-6 commit
branch: auto                  # auto | never — auto branches off protected branches only
protected_branches: []        # extra regexes, e.g. ['^stable-.*']
ide: rider                    # none (default) | rider | jetbrains-<product> | vscode
ide_project: <sln basename>   # IDE links only
ide_project_root: <dir>       # repo-relative dir containing the .sln
---

## Notes for agents

What an implementor or reviewer needs that CLAUDE.md lacks: how to run the app,
docs that must stay in sync, flaky checks, what "hot path" means here.
```

`models:` keys are agent names without the `tackle-`/`tackle-review-` prefix
(`implementor`, `correctness`, `guide`, …). Effort can't be overridden per
project; it is fixed in each agent's frontmatter.

## Unity / DOTS projects

`validate` usually runs through the Unity MCP server, not a shell. Put the
procedure in Notes and set `validate:` to a short label. Agents follow the saved
Unity MCP workflow memory for timing and pitfalls (busy-vs-broken bridge
responses, play-mode delays, silent Burst fallback) rather than inventing their
own polling.
