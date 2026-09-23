## Git and publishing

- Keep commit messages short. Extended documentation, reasoning, and historical context belong in documentation files, not commit messages.
- Outward actions are the user's: git pushes, tags, and package releases. Local commits and version/changelog bumps are fine when asked or when the task is a release. Stop at that boundary and surface the remaining publish steps.

## Comments and documentation

- Implementation comments: as short as possible. Where extended explanation or historical context is needed, only a few lines of summary.
- Docstrings on class members: up to a paragraph.
- Docstrings on classes: up to a few paragraphs.
- Extended documentation belongs in documentation files, which comments can reference. Their location is project-dependent.
- Update project documentation as part of any change that affects it, without being asked. After a change, grep the docs for anything it touches (names, data model, workflow steps) and fix stale statements. Treat it as part of "done."

## Chat messages

- Explain reasoning as a list of logical steps rather than paragraphs of prose.

## Validation

- For visual or rendering validation, do not rely on screenshots or scene/game captures. Ask the user a specific, disambiguating question about what they observe and they will describe the behavior. Where useful, add temporary debug output that makes the answer unambiguous.
- When a task has multiple distinct phases, validate or compile-check at the end of each phase before starting the next, rather than writing everything and validating once at the end. This keeps each phase's changes contained and isolates which phase introduced a problem.

## Tooling

- GUI-launched applications (e.g. an editor or IDE started from a launcher rather than a terminal) inherit neither the shell environment nor a full PATH. For tooling that runs inside such an app, put configuration in a const with an env var as the override (never env-only), and resolve external executables by absolute path with a candidate list plus an override. Files read from the home directory (e.g. ~/.aws) are unaffected.
- Two `unity-cli` skills are installed. Always use the bare `unity-cli` (`~/.claude/skills/unity-cli`), never the Unity plugin's `unity:unity-cli` — as of 2026-09-09 the plugin's copy lags the CLI and gets test exit codes wrong. Refresh the bare one with `unity skill install claude-code --yes --force`, which tracks the installed binary. The rest of the `unity:*` plugin skills are fine.
