# Review guide — format, links, and visuals

Written by `tackle-review-guide` at the end of phase 4:

- `<run>/review-guide.html` — the deliverable, opened in a
  browser
- `<run>/review-guide.md` — same content, for a terminal or phone
- `<run>/assets/` — images and charts, embedded by relative path

Run state, not project source; not committed.

## Why HTML in a browser

Tested 2026-09-04:

1. `jetbrains://` links work from `open` and from a real browser, which hands
   unknown schemes to the OS.
2. They fail from Rider's Markdown preview and from HTML opened inside Rider —
   its embedded browser refuses the scheme.

So: guide in a browser window, code in the IDE. Never suggest opening the guide
in the IDE; every link silently dies.

## Link format by IDE

Set by `ide:` in `.claude/tackle.md`. Default `none`: a plain relative link plus
`path:line`, which works everywhere.

| `ide:` | Status | Path form | Line base |
|---|---|---|---|
| `none` (default) | — | plain relative link | n/a, shown in text |
| `rider` | **verified** 2026-09-04, Rider 2026.2 | relative to project root, `../` allowed | **0-based** |
| `jetbrains-<product>` | unverified | assume as Rider | assume as Rider |
| `vscode` | unverified | `vscode://file/<absolute>:<line>:<col>` per docs | 1-based per docs |

Never emit an unverified scheme. Verify first via `setup.md` §3 — clicking from a
browser, not the IDE — and update this table.

## Rider / JetBrains

Verified on Rider 2026.2 via JetBrains Toolbox, which registers the scheme:

```
[BvhBuilder.cs:88](jetbrains://rider/navigate/reference?project=<name>&path=<relative>:<line-1>)
```

Each rule was established by testing; don't "fix" them:

1. **`path` is relative to the IDE project root.** Absolute paths fail with "The
   URI could not be opened". Use `../` for code outside the root.
2. **The line is 0-based.** `:88` lands on line 89. Emit `line - 1`; display the
   1-based number.
3. **`project` is the `.sln` basename**, not the repo name.

## Per-project settings

```yaml
ide: rider                    # none (default) | rider | jetbrains-<product> | vscode
ide_project: capsulecolliders64-test
ide_project_root: Examples~/capsulecolliders64-test
```

If unset, derive once and offer to record:

- Rider's `~/Library/Application Support/Rider*/options/recentSolutions.xml`
  holds the `.sln` path: basename → `ide_project`, directory →
  `ide_project_root`.
- Unity packages: Rider opens the example project under `Examples~/`, while the
  source is in `Runtime/` above it. The root is the example project; library
  links need `../../`.

## Degradation

Always render both forms, so the guide survives a dead scheme, a terminal, or a
phone:

```markdown
1. [BvhBuilder.cs:88](jetbrains://...) — `Runtime/Scripts/Bvh/BvhBuilder.cs:88`
   The pruning pass. The whole change lives here.
```

## Visuals

Produce an image or chart where it answers a question faster than prose.

They are for **the user to judge**, not evidence for an agent. The
`~/.claude/CLAUDE.md` rule stands: never validate rendering from a capture. A
capture gives the user something concrete to answer about.

### Unity render captures

- Tools: `mcp__unity-editor-mcp__capture_game_view`, `capture_scene_view`,
  `screenshot`.
- Embed next to the playtest step it belongs to. Caption with **what to look
  at**, never a verdict.
  - Good: "Scene view, 5000 colliders. Is the gizmo density near the top-right
    cluster the pruning you expected?"
  - Bad: "Rendering is correct."
- Before/after pairs from the same camera, base commit vs. change. If you
  couldn't match them, say so.
- Each capture costs a play-mode round trip (timing in the project config
  Notes). Capture only the views that matter.

### Charts

For benchmarks, allocation counts, complexity across sizes, before/after
timings. **Invoke the `dataviz` skill before writing chart code.**

- Always plot before and after together.
- Label axis units and the number of runs.
- State that numbers are comparable only on the same machine and session.
- Skip charts when the change isn't about performance; irrelevant numbers cost
  review attention.

## Writing the HTML

One self-contained file: inline CSS, no external stylesheets, fonts, scripts,
or network.

**Copy `assets/review-guide.css` verbatim into `<style>`.** Don't re-derive,
restyle, or `<link>` it. If a class is missing, add it to that file so later
runs inherit it.

- **Escape `&` as `&amp;` in every href.** An unescaped `&` truncates the query
  and the link silently opens the wrong thing.
- Line numbers: 0-based in the href, 1-based in the text.
- Show plain `path:line` (class `path`) beside every link.
- **Never type item numbers in HTML.** `<ol>` numbers its items; a typed "1."
  inside an `<li>` renders as "1. 1.". Use `<ol start="N">` to continue a
  sequence. Only the markdown copy types numbers.
- Playtest steps get real `<input type="checkbox">` elements (state not
  persisted) inside an `<ol>`, which keeps its numbers.
- Depth goes in `<details>`, closed by default. Each `<summary>` names its
  contents ("why positional matching", "the reviewer exchange"), never "more".
  The fully collapsed page is the guide.
- Images from `assets/` by relative `src`. Never absolute paths or temp
  directories.
- No JavaScript.

The markdown copy has the same content with plain links and no styling. Keep
`<details>`/`<summary>` — they render on GitHub and degrade to visible text.
Keep one-line entries one line; it is read on a phone.
