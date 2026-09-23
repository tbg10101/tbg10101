# Setup — writing `.claude/tackle.md`

Run by `/tackle setup`, and offered (never forced) when a run starts without a
config. Every key has a safe default.

**Detect first; ask only what detection couldn't settle.** Showing findings for
correction is faster than asking.

## 1. Detect

Record each with its confidence:

- **Task backend** — `docs/tasks/` or `tasks/` exists → `markdown` there. No
  task files but Trello MCP connected → ask. Default `markdown` at
  `docs/tasks`.
- **Validate command** — from project docs, manifests, Makefile, CI:
  `dotnet test`, `npm test`, `cargo test`. Unity usually has none; see
  `project-config.md`.
- **Build command** — same sources.
- **Default branch** — `git symbolic-ref refs/remotes/origin/HEAD`, else
  whichever of `master`/`main` exists.
- **IDE** — §2.
- **Reviewers** — suggest skipping `performance` with no runtime hot path, or
  `design` with no public API. Suggest, don't decide. `correctness` is never
  skipped.

## 1a. Version control

Check before anything else — it decides whether the run has an undo.

```
git rev-parse --show-toplevel
```

- **Project root** → normal.
- **Fails** → not a repo; offer `git init`.
- **An ancestor directory** → swallowed by an outer repo (e.g. a dotfiles repo in
  `$HOME`). Treat as not a repo: commits would land in the wrong place and outer
  ignore rules may hide the project. Say so explicitly — `git status` looks
  like it works.

### Offering `git init`

Offer once, framed as what it buys: an undo, and a real phase-4 diff instead of
the snapshot fallback. Local only — never add a remote or push.

If accepted, in order:

1. **Write `.gitignore` before any `git add`.** A Unity `Library/` is routinely
   gigabytes. Copy from a sibling project of the same shape:
   - Unity package (source at root, test project under `Examples~/`):
     `/Examples~/*/Library`, `Logs`, `obj`, `Temp`, `UserSettings`, `*.sln`,
     `*.csproj`
   - Unity application: the same paths at the root
2. `git init` with the branch name the user's other projects use
   (`git config --get init.defaultBranch`, and sibling repos — they may differ).
3. Check `git status --short | wc -l` and the staged size. Thousands of files or
   hundreds of MB means the ignore file is wrong — fix it, don't commit.
4. Commit only with explicit approval. Short message.
5. Set `branch: auto`, `commit: ask`, and remove any "untracked" note from the
   config.

If declined: set `branch: never`, `commit: never`, and note that the project is
deliberately untracked so no run re-asks.

## 2. Detect the IDE

Only worth it for an IDE that can be driven by URL.

- **JetBrains** — `~/Library/Application Support/JetBrains/<Product><Version>/options/recentSolutions.xml`
  (Rider) or `recentProjects.xml` (others). If this repo is listed, the entry
  gives `ide_project` (basename) and `ide_project_root` (directory). Confirm the
  scheme is registered: `lsregister -dump | grep -i "jetbrains:"`.
- **VS Code** — `.vscode/` in the repo, or the repo in
  `~/Library/Application Support/Code/User/globalStorage`.
- **Neither or unsure** → `ide: none`.

## 3. Verify before trusting

Only `rider` is verified on this machine. For anything else, verify once with
the user:

1. Build a link to a known file and line.
2. `open` it.
3. Ask the user which file opened and which line the caret is on. Only they can
   observe it.
4. Check what bit on Rider: relative vs. absolute path, 0- vs. 1-based line.
5. Record the result in `review-guide.md`.

If they'd rather not, write `ide: none`. Links that open the wrong line are
worse than none.

## 4. Confirm and write

Show the proposed config as one block, each value marked **detected** or
**assumed**. Ask once, then write it.

Add the Notes section by asking: "what will an implementor get wrong here that
isn't written down anywhere?"

## 5. Keep it current

When a run wastes a step the config could have saved — a wrong command, an
undiscovered doc directory, a re-asked question — offer to record it at
close-out.
