using System;
using UnityEngine;

// JsonUtility matches JSON keys to field names, so fields carry the lowercase keys and properties expose them.

namespace Software10101.Tackle.Editor {
    /// <summary>
    /// A tackle run's <c>playtest.json</c>: a title, an optional browser guide, and the ordered manual
    /// playtest steps. <c>guide</c> and step images are relative to the JSON file.
    /// </summary>
    [Serializable]
    public sealed class PlaytestDocument {
        [SerializeField]
        private string title = "";

        [SerializeField]
        private string guide = "";

        [SerializeField]
        private PlaytestStep[] steps = Array.Empty<PlaytestStep>();

        public string Title => title;
        public string Guide => guide;
        public PlaytestStep[] Steps => steps ?? Array.Empty<PlaytestStep>();
    }

    /// <summary>
    /// One playtest step: what to do, what to look for, code links, and an optional image.
    /// </summary>
    [Serializable]
    public sealed class PlaytestStep {
        [SerializeField]
        private string text = "";

        [SerializeField]
        private string observe = "";

        [SerializeField]
        private PlaytestLink[] links = Array.Empty<PlaytestLink>();

        [SerializeField]
        private string image = "";

        public string Text => text;
        public string Observe => observe;
        public PlaytestLink[] Links => links ?? Array.Empty<PlaytestLink>();
        public string Image => image;
    }

    /// <summary>
    /// A link to a source line. <c>path</c> is relative to the Unity project root; <c>line</c> is 1-based.
    /// </summary>
    [Serializable]
    public sealed class PlaytestLink {
        [SerializeField]
        private string label = "";

        [SerializeField]
        private string path = "";

        [SerializeField]
        private int line = 0;

        public string Label => label;
        public string Path => path;
        public int Line => line;
    }
}
