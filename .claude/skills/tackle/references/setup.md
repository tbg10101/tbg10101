# Setup — writing `.claude/tackle.md`

Run by `/tackle setup`, and offered (never forced) at the start of a run in a
project that has no config. A missing config is not an error; every key has a
safe default.

Principle: **detect first, ask only what you couldn't detect.** Show what you
found and let the user correct it — that is faster to check than to answer.

## 1. Detect

Work through these and record what you find, with confidence:

- **Task backend** — a `docs/tasks/` or `tasks/` directory → `markdown` with
  that path. No local task files but the Trello MCP server is connected → ask.
  Default `markdown` at `docs/tasks`.
- **Validation command** — from the project's own docs and manifests:
  `dotnet test`, `npm test`, `cargo test`, a `Makefile` target, a CI workflow.
  Unity projects usually have no shell command; see below.
- **Build / compile-check command** — same sources.
- **Default branch** — `git symbolic-ref refs/remotes/origin/HEAD`, else
  whichever of `master`/`main` exists.
- **IDE** — see below.
- **Reviewers** — a project with no runtime hot path may not need
  `performance`; a project with no public API may not need `design`. Suggest,
  don't decide. `correctness` can never be skipped.

## 1a. Version control

Check this **before** anything else, because it determines whether the run has
an undo.

```
git rev-parse --show-toplevel
```

Three outcomes:

- **Resolves to the project root** → normal git project, nothing to do.
- **Fails** → not a repository. Offer `git init`, below.
- **Resolves to an _ancestor_ directory** → the project is not its own repo; it
  has been swallowed by a repo further up (a dotfiles repo in `$HOME` does this
  silently). Treat as "not a repository" — commits would land in the wrong repo
  and its ignore rules may hide the project entirely. Say this explicitly, since
  `git status` inside the project looks like it works.

### Offering `git init`

Offer once. Local tracking only — **never add a remote, never push.** Frame it
as what it buys: an undo for the run, and a real diff for phase 4 instead of the
snapshot fallback.

If accepted, in this order:

1. **Write `.gitignore` first.** Do not run `git add` before this exists. A
   Unity project carries a `Library/` directory that is routinely **gigabytes**;
   staging it is slow, may exhaust memory, and is painful to undo.
   Copy the ignore file from a sibling project of the same shape rather than
   inventing one — a Unity *package* repo (source at the root, a test project
   under `Examples~/`) needs `/Examples~/*/Library`, `Logs`, `obj`, `Temp`,
   `UserSettings`, plus `*.sln` and `*.csproj`; a Unity *application* repo needs
   the same paths at the root instead.
2. `git init` — match the branch name the user's other projects use
   (`git config --get init.defaultBranch`, and check sibling repos, which may
   disagree with each other).
3. **Show the damage before doing it.** `git status --short | wc -l` and the
   size of what would be staged. If it is thousands of files or hundreds of
   megabytes, the ignore file is wrong — stop and fix it, don't commit.
4. Commit only with explicit approval. Short message.
5. Update `.claude/tackle.md`: `branch: auto`, `commit: ask`, and delete any
   note recording that the project was untracked.

If declined, record `branch: never` / `commit: never` and note in the config
that the project is deliberately untracked, so no later run re-asks.

## 2. Detect the IDE

Only worth configuring if the user reviews in an IDE that can be driven by URL.

- **JetBrains** — `~/Library/Application Support/JetBrains/<Product><Version>/options/recentSolutions.xml`
  (Rider) or `recentProjects.xml` (IDEA and others) lists recently opened
  projects. If this repo appears there, that's the IDE, and the entry gives you
  both `ide_project` (the `.sln`/project basename) and `ide_project_root` (its
  directory). Confirm the `jetbrains:` scheme is actually registered:
  `lsregister -dump | grep -i "jetbrains:"`.
- **VS Code** — `.vscode/` in the repo, or the repo in
  `~/Library/Application Support/Code/User/globalStorage`.
- **Neither, or unsure** → `ide: none`. Plain relative links still work
  everywhere, including on GitHub and in a terminal.

## 3. Verify before trusting

Do not write an IDE link format into the config on the strength of a table.
**Only `rider` has been verified on this machine.** For anything else, verify
once, with the user watching, exactly as Rider was verified:

1. Construct a link to a known file and line.
2. `open` it.
3. Ask the user what they see — which file, and *which line number*. Do not
   infer this from a screenshot or from the shell; the caret position is only
   observable to them.
4. Check both things that bit on Rider: whether the path must be
   project-relative or absolute, and whether the line is 0- or 1-based.
5. Record the answer in `references/review-guide.md` so the next project
   inherits it.

If the user doesn't want to do this now, write `ide: none` and move on. A guide
with plain paths is useful; a guide with links that silently open the wrong line
is worse than one with none.

## 4. Confirm and write

Show the proposed config as a single block, marking each value **detected** or
**assumed**. Ask once, then write `.claude/tackle.md`.

Then add the Notes section — anything an agent would otherwise rediscover every
run. Ask the user directly: "what will an implementor get wrong here that isn't
written down anywhere?"

## 5. Keep it current

When a run wastes a step on something the config could have answered — a wrong
test command, a doc directory nobody found, a re-asked question — offer to add
it at close-out. The config should get better with use.
