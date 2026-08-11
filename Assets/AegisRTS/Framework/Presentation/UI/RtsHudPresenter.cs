using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace AegisRTS.Presentation.UI
{
    /// <summary>Immediate-mode Unity adapter for the reusable RTS HUD layout.</summary>
    [DisallowMultipleComponent]
    public sealed class RtsHudPresenter : MonoBehaviour
    {
        private static readonly HudPanelId[] LayoutPanels =
        {
            HudPanelId.ResourceBar, HudPanelId.SelectionPanel, HudPanelId.CommandPanel,
            HudPanelId.AbilityBar, HudPanelId.ArmyPanel, HudPanelId.SettlementPanel,
            HudPanelId.Minimap, HudPanelId.Notification, HudPanelId.Objective, HudPanelId.Pause,
        };
        private readonly Dictionary<string, HudThemeDefinition> _themes =
            new Dictionary<string, HudThemeDefinition>(StringComparer.Ordinal);
        private RtsHudViewModel _viewModel;
        private HudThemeDefinition _theme;
        private GUIStyle _titleStyle, _entryStyle, _buttonStyle;
        private double _nextRefresh;

        public IReadOnlyList<HudPanelId> PanelIds => new ReadOnlyCollection<HudPanelId>(LayoutPanels);
        public IReadOnlyList<string> ThemeIds => new ReadOnlyCollection<string>(_themes.Keys.OrderBy(value => value).ToList());
        public string CurrentThemeId => _theme?.Id ?? string.Empty;
        public string LayoutSignature => string.Join("|", LayoutPanels.Select(id => $"{id}:{NormalizedRect(id)}"));

        public void Configure(RtsHudViewModel viewModel, IEnumerable<HudThemeDefinition> themes, string initialThemeId = null)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel)); _themes.Clear();
            foreach (HudThemeDefinition theme in themes ?? Array.Empty<HudThemeDefinition>())
                if (theme != null) _themes[theme.Id] = theme;
            if (_themes.Count == 0) throw new ArgumentException("At least one HUD theme is required.", nameof(themes));
            string selected = !string.IsNullOrWhiteSpace(initialThemeId) && _themes.ContainsKey(initialThemeId)
                ? initialThemeId : _themes.Keys.OrderBy(value => value).First();
            SwitchTheme(selected); _viewModel.Refresh();
        }

        public bool SwitchTheme(string themeId)
        {
            if (!_themes.TryGetValue(themeId ?? string.Empty, out HudThemeDefinition theme)) return false;
            _theme = theme; _titleStyle = null; _entryStyle = null; _buttonStyle = null; return true;
        }

        public Rect GetPanelRect(HudPanelId id, float width, float height)
        {
            Rect normalized = NormalizedRect(id); float scale = (float)(_theme?.Scale ?? 1d);
            return new Rect(normalized.x * width, normalized.y * height,
                normalized.width * width * scale, normalized.height * height * scale);
        }

        private void Update()
        {
            if (_viewModel == null || Time.unscaledTimeAsDouble < _nextRefresh) return;
            _viewModel.Refresh(); _nextRefresh = Time.unscaledTimeAsDouble + 0.2d;
        }

        private void OnGUI()
        {
            if (_viewModel == null || _theme == null) return;
            EnsureStyles(); HudSnapshot snapshot = _viewModel.Snapshot;
            foreach (HudPanelId id in LayoutPanels)
                if (snapshot.TryGetPanel(id, out HudPanelViewModel panel) && panel.Visible)
                    DrawPanel(GetPanelRect(id, Screen.width, Screen.height), panel);
        }

        private void DrawPanel(Rect rect, HudPanelViewModel panel)
        {
            Color previousBackground = GUI.backgroundColor; Color previousContent = GUI.contentColor;
            GUI.backgroundColor = Parse(_theme.Panel, Color.black) * new Color(1, 1, 1, (float)_theme.PanelOpacity);
            GUI.contentColor = Parse(_theme.Text, Color.white); GUI.Box(rect, GUIContent.none);
            float padding = 8f; float y = rect.y + padding;
            GUI.Label(new Rect(rect.x + padding, y, rect.width - padding * 2f, 22f), panel.Title, _titleStyle); y += 24f;
            bool clickable = panel.Id == HudPanelId.CommandPanel || panel.Id == HudPanelId.AbilityBar || panel.Id == HudPanelId.Pause;
            foreach (HudEntry entry in panel.Entries)
            {
                if (y + 20f > rect.yMax) break;
                string text = string.IsNullOrEmpty(entry.Value) ? entry.Label : $"{entry.Label}  {entry.Value}";
                Rect row = new Rect(rect.x + padding, y, rect.width - padding * 2f, 20f);
                if (clickable)
                {
                    GUI.enabled = entry.Enabled;
                    if (GUI.Button(row, text, _buttonStyle)) _viewModel.Execute(new HudCommand(entry.Id));
                    GUI.enabled = true;
                }
                else GUI.Label(row, text, _entryStyle);
                y += 21f;
            }
            GUI.backgroundColor = previousBackground; GUI.contentColor = previousContent;
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;
            Color text = Parse(_theme.Text, Color.white), muted = Parse(_theme.MutedText, Color.gray);
            _titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13, normal = { textColor = text } };
            _entryStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = muted } };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 11, alignment = TextAnchor.MiddleLeft, normal = { textColor = text } };
        }

        private static Color Parse(string value, Color fallback) =>
            ColorUtility.TryParseHtmlString(value, out Color color) ? color : fallback;

        private static Rect NormalizedRect(HudPanelId id)
        {
            switch (id)
            {
                case HudPanelId.ResourceBar: return new Rect(0.25f, 0.01f, 0.50f, 0.08f);
                case HudPanelId.SelectionPanel: return new Rect(0.01f, 0.76f, 0.22f, 0.22f);
                case HudPanelId.CommandPanel: return new Rect(0.24f, 0.78f, 0.22f, 0.20f);
                case HudPanelId.AbilityBar: return new Rect(0.47f, 0.84f, 0.24f, 0.14f);
                case HudPanelId.ArmyPanel: return new Rect(0.01f, 0.32f, 0.20f, 0.25f);
                case HudPanelId.SettlementPanel: return new Rect(0.79f, 0.30f, 0.20f, 0.27f);
                case HudPanelId.Minimap: return new Rect(0.78f, 0.72f, 0.21f, 0.26f);
                case HudPanelId.Notification: return new Rect(0.75f, 0.02f, 0.24f, 0.18f);
                case HudPanelId.Objective: return new Rect(0.01f, 0.02f, 0.25f, 0.20f);
                case HudPanelId.Pause: return new Rect(0.42f, 0.30f, 0.16f, 0.16f);
                default: return new Rect(0, 0, 0.1f, 0.1f);
            }
        }
    }
}
