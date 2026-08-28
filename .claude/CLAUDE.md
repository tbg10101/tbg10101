Keep commit messages short. Extended documentation, reasoning, and historical context belongs in documentation files, not commit messages.

Implementation comments should be as short as possible. If an extended explanation or historical context is required then a only few lines of summary is desired. Docstring comments on class members can be longer, up to a paragraph. Docstring comments on classes can be up to a few paragraphs. Extended documentation belongs in documentation files which the comments can reference. Documentation files' locations are project-dependent.

Prefer to explain resoning in sequences of logic formatted as a list instead of paragraphs of prose.

Leave outward actions to the user: git pushes, tags, and package releases are theirs to perform. Complete the local work (edits, local commits, version/changelog bumps), then stop at that boundary and surface the remaining publish steps.

Update project documentation as part of any change that affects it, without being asked. After a change, grep the docs for anything it touches (names, data model, workflow steps) and fix stale statements. Treat it as part of "done."

For visual or rendering validation, do not rely on screenshots or scene/game captures. Ask the user a specific, disambiguating question about what they observe and they will describe the behavior. Where useful, add temporary debug output that makes the answer unambiguous.

When a task has multiple distinct phases, validate or compile-check at the end of each phase before starting the next, rather than writing everything and validating once at the end. This keeps each phase's changes contained and isolates which phase introduced a problem.

GUI-launched applications (e.g. an editor or IDE started from a launcher rather than a terminal) inherit neither the shell environment nor a full PATH. For tooling that runs inside such an app, put configuration in a const with an env var as the override (never env-only), and resolve external executables by absolute path with a candidate list plus an override. Files read from the home directory (e.g. ~/.aws) are unaffected.

