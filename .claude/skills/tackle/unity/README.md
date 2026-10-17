# Tackle (Unity)

Editor-only package with a window that shows a tackle run's manual playtest steps
(`<run>/playtest.json`) inside the Unity Editor, so a playtest can run from a full-screen
Editor without switching apps.

- **Window/Tackle/Playtest** opens the window; **Open…** picks a file.
- `Software10101.Tackle.Editor.PlaytestWindow.Open("<absolute path>")` opens it on a file.
  Tackle calls this through the Unity Editor MCP `eval` tool.
- Each step has a checkbox (kept for the Editor session), its text, what to observe, buttons that
  open linked source lines in the configured code editor, and an optional image.
- The window reloads itself when the file changes, keeping its scroll position.

The file format and how tackle opens the window are in
[`../references/playtest-window.md`](../references/playtest-window.md).

## Install

Add one of these to the project's `Packages/manifest.json`:

- Development, from a local checkout:
  `"com.10101software.tackle": "file:/Users/<you>/.claude/skills/tackle/unity"`
- Consumers, pinned to a release tag:
  `"com.10101software.tackle": "https://github.com/tbg10101/tbg10101.git?path=/.claude/skills/tackle/unity#tackle-unity-<version>"`

Commit the package's `.meta` files: git installs are immutable, so Unity cannot generate them there.
