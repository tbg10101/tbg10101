# Playtest window — Unity

With `playtest_window: unity`, the playtest runs from an Editor window instead
of the browser guide, so the user never leaves a full-screen Editor.

## Install

- Develop: `"com.10101software.tackle": "file:/Users/<you>/.claude/skills/tackle/unity"`
- Consume: `https://github.com/tbg10101/tbg10101.git?path=/.claude/skills/tackle/unity#tackle-unity-<version>`
- Commit the package's `.meta` files; git installs are immutable.

## `<run>/playtest.json`

Written by `tackle-review-guide` beside the guide. Same steps, same order.

```json
{
  "title": "...",
  "guide": "review-guide.html",
  "steps": [
    {
      "text": "...",
      "observe": "...",
      "links": [{ "label": "Foo.cs:88", "path": "<project-relative>", "line": 88 }],
      "image": "assets/<file>.png"
    }
  ]
}
```

- `line` is 1-based. The window opens it in the configured code editor.
- `guide` and `image` are relative to the JSON file.
- Plain text only. No markdown, no HTML.

## Opening it

Phase 5: `mcp__unity-editor-mcp__eval` running
`Software10101.Tackle.Editor.PlaytestWindow.Open("<absolute path>")`.
If that fails (no package, no bridge), say so and fall back to the browser guide.
