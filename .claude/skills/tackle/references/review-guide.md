# Review guide — links and settings

Written by `tackle-review-guide` at the end of phase 4, to
`.claude/tackle/runs/<slug>.review-guide.html` — **HTML is the deliverable**,
opened in a browser. A plain `<slug>.review-guide.md` is written alongside from
the same content, for reading in a terminal or on a phone. Run state, not
project source — not committed.

## Why HTML, and why a browser

Established by testing on 2026-09-04, in this order:

1. `jetbrains://` links work when opened by the OS (`open`), and from **a real
   browser**, which hands unknown schemes to the OS.
2. They do **not** work from Rider's own Markdown preview, or from an HTML file
   opened inside Rider. Its embedded browser refuses the scheme — including
   JetBrains' own. Getting the guide *into* Rider is not the same as getting
   clickable links there.

So the review workflow is a **browser | IDE split**: guide in a browser window,
code in Rider. Do not suggest opening the guide inside the IDE for navigation;
it silently loses every link.

Images and charts go beside it in `.claude/tackle/runs/<slug>.assets/`, embedded
with a **relative** path (`![...](<slug>.assets/before.png)`) so the guide and
its assets move together and render anywhere. Never embed an absolute path or
link an image out of a temp directory that will be cleared.

## Link format by IDE

Set by `ide:` in `.claude/tackle.md`. **Default is `none`** — plain relative
markdown links plus `path:line`, which work everywhere and need no handler.
Only opt into an IDE scheme once it has been verified on that machine.

| `ide:` | Status | Path form | Line base |
|---|---|---|---|
| `none` (default) | — | plain relative link | n/a, shown in text |
| `rider` | **verified** 2026-09-04, Rider 2026.2 | relative to project root, `../` allowed | **0-based** |
| `jetbrains-<product>` | unverified | assume as Rider | assume as Rider |
| `vscode` | unverified | `vscode://file/<absolute>:<line>:<col>`, documented as absolute | documented 1-based |

Unverified means exactly that: do not emit those links until `references/setup.md`
step 3 has been run and this table updated with the result. Verify by clicking
**from a browser**, not from inside the IDE.

## Rider / JetBrains

Verified working (Rider 2026.2, installed via JetBrains Toolbox, which registers
and routes the `jetbrains:` URL scheme):

```
[BvhBuilder.cs:88](jetbrains://rider/navigate/reference?project=<name>&path=<relative>:<line-1>)
```

Three rules, each established by testing — do not "fix" them:

1. **`path` must be relative to the IDE project root.** Absolute paths are
   rejected with "The URI could not be opened". Paths may escape the root with
   `../`, which is required whenever the code under review sits outside it.
2. **The line number is 0-based.** Requesting `:88` puts the caret on line 89.
   Emit `line - 1`, and keep the *displayed* text 1-based so it matches the
   editor gutter.
3. **`project` is the `.sln` basename**, not the repository name.

Intended use: guide open in one Rider split, code in the other.

## Per-project settings

In `.claude/tackle.md`:

```yaml
ide: rider                    # none (default) | rider | jetbrains-<product> | vscode
ide_project: capsulecolliders64-test
ide_project_root: Examples~/capsulecolliders64-test
```

If unset, try to derive them once and offer to record them:

- Rider's recent projects list at
  `~/Library/Application Support/Rider*/options/recentSolutions.xml` holds the
  `.sln` path. Its basename is `ide_project`; its directory is `ide_project_root`.
- Unity packages are the awkward case: Rider opens the *example* project inside
  `Examples~/`, while the reviewable source is in `Runtime/` above it. The root
  is the example project; links to library source need `../../`.

`ide: none` → a repo-relative markdown link plus `path:line`. This is the
default and is never wrong; prefer it over a guessed scheme.

## Degradation

Always render both forms:

```markdown
1. [BvhBuilder.cs:88](jetbrains://...) — `Runtime/Scripts/Bvh/BvhBuilder.cs:88`
   The pruning pass. The whole change lives here.
```

The link is for Rider; the plain path is for reading in a terminal, on a phone,
or after the guide outlives the checkout. If the scheme ever stops resolving,
the guide degrades to still-useful instead of unusable.

## Visuals

A guide is for a human. Where an image or a chart answers a question faster than
prose, produce one and embed it.

**The distinction that matters:** these are for *the user to look at and judge*.
They are not evidence for an agent to draw conclusions from. The standing rule
in `~/.claude/CLAUDE.md` — never validate rendering or visual behaviour from a
capture, ask the user a specific disambiguating question instead — is unchanged.
A capture in the guide is there so the user has something concrete to answer
*about*; it is not the guide author deciding the visuals are correct.

### Unity render captures

Where a change affects anything visible, the Unity MCP server can capture the
game or scene view (`mcp__unity-editor-mcp__capture_game_view`,
`capture_scene_view`, `screenshot`). Embed the image next to the playtest step
it belongs to, and caption it with **what to look at**, not what you concluded.

- Good: "Scene view after the change, 5000 colliders. Look at the gizmo density
  near the top-right cluster — is that the pruning you expected?"
- Bad: "Rendering is correct."

A before/after pair is worth far more than either alone. Capture the same view
from the same camera position on the base commit and after the change; say
explicitly if you could not, so the user doesn't read a difference into two
shots that aren't comparable.

Capturing costs a play-mode round trip on the Unity main thread — see the
project's config Notes for the timing. Do it once, deliberately, for the views
that matter.

### Data visualizations

Benchmark results, allocation counts, complexity across input sizes, and
before/after timings are all easier to judge as a chart than a table of numbers.
When the change touches performance and there are numbers to show, plot them.

**Invoke the `dataviz` skill before writing any chart code** — it covers palette,
form, and accessibility, and produces something consistent rather than ad hoc.

Rules specific to a review guide:

- Always plot **before and after together**. A single bar is not reviewable.
- Label the axis units and say how many runs the numbers came from.
- Benchmark numbers are only comparable on the same machine in the same session
  — state that on the chart, so a number is never read as an absolute claim.
- If the change is not about performance, skip this. A chart of irrelevant
  numbers is noise that costs review attention, which is the one thing the
  guide exists to protect.

## Writing the HTML

One **self-contained** file: inline CSS, no external stylesheets, fonts, or
scripts. It must render correctly with no network and after being moved.

- **Escape `&` as `&amp;` in every href.** The query string joins `project` and
  `path` with `&`; an unescaped one truncates the URL and the link silently
  opens the wrong thing. This is the easiest bug to introduce here.
- **Line numbers are 0-based in the href, 1-based in the link text.** Emit
  `line - 1` in the URL; show the real number to the reader.
- Alongside every link, show the plain `path:line` in muted text, so the guide
  is still usable if a link fails or is read as source.
- Respect `prefers-color-scheme` — light and dark both legible. Reviewers run
  dark IDEs.
- Constrain body width (~44em) and use the system font stack. This is a reading
  document, not a dashboard.
- Give the playtest steps real `<input type="checkbox">` elements. State is not
  persisted; they are for keeping your place during a manual pass.
- Images from `<slug>.assets/` via relative `src`. Never absolute paths.
- No JavaScript. It buys nothing here and makes the file harder to trust.

The markdown copy carries the same content with plain links, and does not try to
reproduce the styling.
