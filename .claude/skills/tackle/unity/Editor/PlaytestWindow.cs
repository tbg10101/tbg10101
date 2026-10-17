using System;
using System.Collections.Generic;
using System.IO;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Software10101.Tackle.Editor {
    /// <summary>
    /// Shows a tackle run's <c>playtest.json</c> as a checklist inside the Editor, so a playtest can run
    /// without leaving a full-screen Editor. The format is documented in the tackle skill's
    /// <c>references/playtest-window.md</c>.
    /// <para>
    /// The open path is kept in a serialized field and <see cref="SessionState"/>, and check state and
    /// scroll position in <see cref="SessionState"/>, so all three survive rebuilds and domain reloads. The file is polled about once a second
    /// and the window rebuilds when its last-write time changes.
    /// </para>
    /// </summary>
    public sealed class PlaytestWindow : EditorWindow {
        private const string PathKey = "Software10101.Tackle.Playtest.Path";
        private const string CheckKeyPrefix = "Software10101.Tackle.Playtest.Check:";
        private const string ScrollKeyPrefix = "Software10101.Tackle.Playtest.Scroll:";
        private const long ScrollRestoreTimeoutMs = 1000;
        private const double PollIntervalSeconds = 1.0;

        [SerializeField]
        private string _path = "";

        private readonly List<Texture2D> _textures = new();
        private DateTime _loadedWriteTime = DateTime.MinValue;
        private double _nextPollTime = 0.0;

        /// <summary>
        /// Opens the window with no file change; it shows whichever file it last had, if any.
        /// </summary>
        [MenuItem("Window/Tackle/Playtest")]
        public static void ShowWindow() {
            GetWindow<PlaytestWindow>("Playtest");
        }

        /// <summary>
        /// Opens the window on the playtest file at <paramref name="path"/> (absolute) and focuses it.
        /// Deferred to the next Editor tick, so it is safe to call from any thread.
        /// </summary>
        public static void Open(string path) {
            EditorApplication.delayCall += () => {
                PlaytestWindow window = GetWindow<PlaytestWindow>("Playtest");
                window.SetPath(path);
                window.Focus();
            };
        }

        private void OnEnable() {
            if (string.IsNullOrEmpty(_path)) {
                _path = SessionState.GetString(PathKey, "");
            }

            EditorApplication.update += Poll;
        }

        private void OnDisable() {
            EditorApplication.update -= Poll;
            DestroyTextures();
        }

        private void CreateGUI() {
            Rebuild();
        }

        private void SetPath(string path) {
            _path = string.IsNullOrEmpty(path) ? "" : Path.GetFullPath(path);
            SessionState.SetString(PathKey, _path);
            Rebuild();
        }

        private void Poll() {
            if (EditorApplication.timeSinceStartup < _nextPollTime) {
                return;
            }

            _nextPollTime = EditorApplication.timeSinceStartup + PollIntervalSeconds;

            if (!string.IsNullOrEmpty(_path) && GetWriteTime(_path) != _loadedWriteTime) {
                Rebuild();
            }
        }

        private void Rebuild() {
            VisualElement root = rootVisualElement;
            root.Clear();
            DestroyTextures();

            root.Add(BuildToolbar());

            ScrollView scroll = new(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.contentContainer.style.paddingLeft = 8;
            scroll.contentContainer.style.paddingRight = 8;
            scroll.contentContainer.style.paddingTop = 6;
            scroll.contentContainer.style.paddingBottom = 6;
            root.Add(scroll);
            PreserveScroll(scroll);

            _loadedWriteTime = string.IsNullOrEmpty(_path) ? DateTime.MinValue : GetWriteTime(_path);
            PlaytestDocument document = TryLoad(out string error);

            if (document == null) {
                scroll.Add(new HelpBox(error, HelpBoxMessageType.Warning));
                return;
            }

            BuildDocument(scroll, document);
        }

        /// <summary>
        /// Restores the saved scroll offset once the content is tall enough to hold it, then saves later
        /// changes. Saving waits for the restore, so the offset clamped to 0 before layout is not saved.
        /// Content that never gets that tall stops waiting after a timeout.
        /// </summary>
        private void PreserveScroll(ScrollView scroll) {
            string key = ScrollKeyPrefix + _path;
            float saved = SessionState.GetFloat(key, 0f);
            bool restored = saved <= 0f;

            scroll.verticalScroller.valueChanged += value => {
                if (restored) {
                    SessionState.SetFloat(key, value);
                }
            };

            if (restored) {
                return;
            }

            EventCallback<GeometryChangedEvent> onLayout = null;
            onLayout = _ => {
                scroll.scrollOffset = new Vector2(scroll.scrollOffset.x, saved);

                if (scroll.verticalScroller.highValue >= saved) {
                    restored = true;
                    scroll.contentContainer.UnregisterCallback(onLayout);
                }
            };
            scroll.contentContainer.RegisterCallback(onLayout);
            scroll.schedule.Execute(() => {
                restored = true;
                scroll.contentContainer.UnregisterCallback(onLayout);
            }).ExecuteLater(ScrollRestoreTimeoutMs);
        }

        private Toolbar BuildToolbar() {
            Toolbar toolbar = new();
            toolbar.Add(new ToolbarButton(Browse) { text = "Open…" });
            toolbar.Add(new ToolbarButton(Rebuild) { text = "Reload" });

            Label pathLabel = new(string.IsNullOrEmpty(_path) ? "(no file)" : _path) { tooltip = _path };
            pathLabel.style.flexGrow = 1;
            pathLabel.style.flexShrink = 1;
            pathLabel.style.overflow = Overflow.Hidden;
            pathLabel.style.textOverflow = TextOverflow.Ellipsis;
            pathLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            pathLabel.style.marginLeft = 4;
            toolbar.Add(pathLabel);

            toolbar.Add(new ToolbarButton(ResetChecks) { text = "Reset checks" });
            return toolbar;
        }

        private void BuildDocument(VisualElement container, PlaytestDocument document) {
            Label title = new(string.IsNullOrEmpty(document.Title) ? "Playtest" : document.Title);
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            title.style.marginBottom = 6;
            container.Add(title);

            if (!string.IsNullOrEmpty(document.Guide)) {
                string guidePath = ResolveBesideFile(document.Guide);
                Button guideButton = new(() => OpenGuide(guidePath)) { text = "Open full guide in browser" };
                guideButton.style.alignSelf = Align.FlexStart;
                guideButton.style.marginBottom = 8;
                container.Add(guideButton);
            }

            PlaytestStep[] steps = document.Steps;

            if (steps.Length == 0) {
                container.Add(new HelpBox("This playtest has no steps.", HelpBoxMessageType.Info));
            }

            for (int i = 0; i < steps.Length; i++) {
                container.Add(BuildStep(i, steps[i] ?? new PlaytestStep()));
            }
        }

        private VisualElement BuildStep(int index, PlaytestStep step) {
            VisualElement row = new();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 10;

            VisualElement body = new();
            body.style.flexGrow = 1;
            body.style.flexShrink = 1;

            string checkKey = CheckKey(index);
            Toggle toggle = new() { value = SessionState.GetBool(checkKey, false) };
            toggle.style.alignSelf = Align.FlexStart;
            toggle.RegisterValueChangedCallback(evt => {
                SessionState.SetBool(checkKey, evt.newValue);
                body.style.opacity = evt.newValue ? 0.5f : 1f;
            });
            body.style.opacity = toggle.value ? 0.5f : 1f;
            row.Add(toggle);
            row.Add(body);

            Label text = new($"{index + 1}. {step.Text}");
            text.style.whiteSpace = WhiteSpace.Normal;
            body.Add(text);

            if (!string.IsNullOrEmpty(step.Observe)) {
                Label observe = new($"Observe: {step.Observe}");
                observe.style.whiteSpace = WhiteSpace.Normal;
                observe.style.unityFontStyleAndWeight = FontStyle.Italic;
                observe.style.color = new Color(0.55f, 0.8f, 1f);
                observe.style.borderLeftWidth = 3;
                observe.style.borderLeftColor = new Color(0.55f, 0.8f, 1f);
                observe.style.paddingLeft = 6;
                observe.style.marginTop = 3;
                body.Add(observe);
            }

            if (step.Links.Length > 0) {
                VisualElement links = new();
                links.style.flexDirection = FlexDirection.Row;
                links.style.flexWrap = Wrap.Wrap;
                links.style.marginTop = 3;

                foreach (PlaytestLink link in step.Links) {
                    if (link == null) {
                        continue;
                    }

                    string label = string.IsNullOrEmpty(link.Label) ? $"{link.Path}:{link.Line}" : link.Label;
                    links.Add(new Button(() => OpenLink(link)) { text = label, tooltip = link.Path });
                }

                body.Add(links);
            }

            if (!string.IsNullOrEmpty(step.Image)) {
                body.Add(BuildImage(step.Image));
            }

            return row;
        }

        private VisualElement BuildImage(string relativePath) {
            string imagePath = ResolveBesideFile(relativePath);

            if (!File.Exists(imagePath)) {
                return new HelpBox($"Image not found: {imagePath}", HelpBoxMessageType.Warning);
            }

            byte[] bytes;

            try {
                bytes = File.ReadAllBytes(imagePath);
            } catch (Exception e) {
                return new HelpBox($"Image could not be read: {imagePath}\n{e.Message}", HelpBoxMessageType.Warning);
            }

            Texture2D texture = new(2, 2) { hideFlags = HideFlags.HideAndDontSave };

            if (!texture.LoadImage(bytes)) {
                DestroyImmediate(texture);
                return new HelpBox($"Image could not be decoded: {imagePath}", HelpBoxMessageType.Warning);
            }

            _textures.Add(texture);

            Image image = new() { image = texture, scaleMode = ScaleMode.ScaleToFit };
            image.style.marginTop = 4;
            image.style.maxWidth = texture.width;
            image.RegisterCallback<GeometryChangedEvent>(evt => {
                float height = evt.newRect.width * texture.height / texture.width;

                if (!Mathf.Approximately(image.resolvedStyle.height, height)) {
                    image.style.height = height;
                }
            });
            return image;
        }

        private PlaytestDocument TryLoad(out string error) {
            if (string.IsNullOrEmpty(_path)) {
                error = "No playtest file open. Use Open… or PlaytestWindow.Open(path).";
                return null;
            }

            if (!File.Exists(_path)) {
                error = $"Playtest file not found: {_path}";
                return null;
            }

            try {
                PlaytestDocument document = JsonUtility.FromJson<PlaytestDocument>(File.ReadAllText(_path));
                error = document == null ? $"Playtest file is empty: {_path}" : null;
                return document;
            } catch (Exception e) {
                error = $"Playtest file could not be read: {_path}\n{e.Message}";
                return null;
            }
        }

        private void Browse() {
            string directory = string.IsNullOrEmpty(_path) ? ProjectRoot : Path.GetDirectoryName(_path);
            string chosen = EditorUtility.OpenFilePanel("Open playtest", directory, "json");

            if (!string.IsNullOrEmpty(chosen)) {
                SetPath(chosen);
            }
        }

        private void ResetChecks() {
            PlaytestDocument document = TryLoad(out _);
            int count = document?.Steps.Length ?? 0;

            for (int i = 0; i < count; i++) {
                SessionState.EraseBool(CheckKey(i));
            }

            Rebuild();
        }

        private static void OpenLink(PlaytestLink link) {
            string fullPath = Path.GetFullPath(Path.Combine(ProjectRoot, link.Path ?? ""));

            if (!File.Exists(fullPath)) {
                Debug.LogWarning($"Playtest link target not found: {fullPath}");
                return;
            }

            int line = link.Line >= 1 ? link.Line : -1;

            // the code editor may refuse file types it does not handle
            if (CodeEditor.CurrentEditor.OpenProject(fullPath, line, 1)
                || InternalEditorUtility.OpenFileAtLineExternal(fullPath, line)) {
                return;
            }

            Debug.LogWarning($"Code editor declined {fullPath}; opening it with the default app.");
            EditorUtility.OpenWithDefaultApp(fullPath);
        }

        private static void OpenGuide(string guidePath) {
            if (!File.Exists(guidePath)) {
                Debug.LogWarning($"Playtest guide not found: {guidePath}");
                return;
            }

            Application.OpenURL(new Uri(guidePath).AbsoluteUri);
        }

        private string ResolveBesideFile(string relativePath) {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(_path) ?? "", relativePath));
        }

        private string CheckKey(int index) {
            return $"{CheckKeyPrefix}{_path}:{index}";
        }

        private void DestroyTextures() {
            foreach (Texture2D texture in _textures) {
                DestroyImmediate(texture);
            }

            _textures.Clear();
        }

        private static DateTime GetWriteTime(string path) {
            return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        }

        private static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);
    }
}
