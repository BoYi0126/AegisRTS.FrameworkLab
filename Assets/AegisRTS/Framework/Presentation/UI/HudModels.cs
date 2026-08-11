using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AegisRTS.Core.Events;

namespace AegisRTS.Presentation.UI
{
    public enum HudPanelId
    {
        ResourceBar, SelectionPanel, CommandPanel, AbilityBar, ArmyPanel,
        SettlementPanel, Minimap, Notification, Objective, Pause,
    }

    public enum HudNotificationSeverity { Info, Success, Warning, Error }

    public sealed class HudEntry
    {
        public HudEntry(string id, string label, string value = "", bool enabled = true)
        { Id = id ?? string.Empty; Label = label ?? string.Empty; Value = value ?? string.Empty; Enabled = enabled; }
        public string Id { get; }
        public string Label { get; }
        public string Value { get; }
        public bool Enabled { get; }
    }

    public sealed class HudPanelViewModel
    {
        public HudPanelViewModel(HudPanelId id, string title, IEnumerable<HudEntry> entries = null, bool visible = true)
        {
            Id = id; Title = title ?? string.Empty; Visible = visible;
            Entries = new ReadOnlyCollection<HudEntry>(new List<HudEntry>(entries ?? Array.Empty<HudEntry>()));
        }
        public HudPanelId Id { get; }
        public string Title { get; }
        public bool Visible { get; }
        public IReadOnlyList<HudEntry> Entries { get; }
    }

    public sealed class HudSnapshot
    {
        private readonly IReadOnlyDictionary<HudPanelId, HudPanelViewModel> _panels;
        public HudSnapshot(IEnumerable<HudPanelViewModel> panels, long revision = 0)
        {
            var map = new Dictionary<HudPanelId, HudPanelViewModel>();
            foreach (HudPanelViewModel panel in panels ?? Array.Empty<HudPanelViewModel>())
            {
                if (panel == null) continue;
                if (map.ContainsKey(panel.Id)) throw new ArgumentException($"Duplicate HUD panel '{panel.Id}'.", nameof(panels));
                map.Add(panel.Id, panel);
            }
            _panels = new ReadOnlyDictionary<HudPanelId, HudPanelViewModel>(map); Revision = revision;
        }
        public long Revision { get; }
        public IReadOnlyDictionary<HudPanelId, HudPanelViewModel> Panels => _panels;
        public bool TryGetPanel(HudPanelId id, out HudPanelViewModel panel) => _panels.TryGetValue(id, out panel);
    }

    public sealed class HudThemeDefinition
    {
        public HudThemeDefinition(string id, string displayName, string background, string panel,
            string primary, string accent, string text, string mutedText, double scale = 1d, double panelOpacity = 0.92d)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Theme ID is required.", nameof(id));
            if (!FiniteRange(scale, 0.5d, 2d)) throw new ArgumentOutOfRangeException(nameof(scale));
            if (!FiniteRange(panelOpacity, 0d, 1d)) throw new ArgumentOutOfRangeException(nameof(panelOpacity));
            Id = id.Trim(); DisplayName = displayName ?? string.Empty;
            Background = ValidateColor(background, nameof(background)); Panel = ValidateColor(panel, nameof(panel));
            Primary = ValidateColor(primary, nameof(primary)); Accent = ValidateColor(accent, nameof(accent));
            Text = ValidateColor(text, nameof(text)); MutedText = ValidateColor(mutedText, nameof(mutedText));
            Scale = scale; PanelOpacity = panelOpacity;
        }
        public string Id { get; }
        public string DisplayName { get; }
        public string Background { get; }
        public string Panel { get; }
        public string Primary { get; }
        public string Accent { get; }
        public string Text { get; }
        public string MutedText { get; }
        public double Scale { get; }
        public double PanelOpacity { get; }

        private static string ValidateColor(string value, string name)
        {
            string color = (value ?? string.Empty).Trim();
            if (color.Length != 7 && color.Length != 9) throw new ArgumentException("Theme color must be #RRGGBB or #RRGGBBAA.", name);
            if (color[0] != '#') throw new ArgumentException("Theme color must start with '#'.", name);
            for (int i = 1; i < color.Length; i++)
                if (!Uri.IsHexDigit(color[i])) throw new ArgumentException("Theme color contains invalid hex digits.", name);
            return color.ToUpperInvariant();
        }
        private static bool FiniteRange(double value, double min, double max) =>
            !double.IsNaN(value) && !double.IsInfinity(value) && value >= min && value <= max;
    }

    public readonly struct HudCommand
    {
        public HudCommand(string commandId, string targetId = null, string payload = null)
        {
            if (string.IsNullOrWhiteSpace(commandId)) throw new ArgumentException("HUD command ID is required.", nameof(commandId));
            CommandId = commandId.Trim(); TargetId = targetId?.Trim() ?? string.Empty; Payload = payload ?? string.Empty;
        }
        public string CommandId { get; }
        public string TargetId { get; }
        public string Payload { get; }
    }

    public readonly struct HudCommandResult
    {
        private HudCommandResult(bool succeeded, string error) { Succeeded = succeeded; Error = error ?? string.Empty; }
        public bool Succeeded { get; }
        public string Error { get; }
        public static HudCommandResult Success() => new HudCommandResult(true, string.Empty);
        public static HudCommandResult Failure(string error) => new HudCommandResult(false, string.IsNullOrWhiteSpace(error) ? "HUD command failed." : error);
    }

    public interface IHudQuery { HudSnapshot Query(); }
    public interface IHudCommandSink { HudCommandResult Dispatch(HudCommand command); }

    public sealed class HudInvalidatedEvent : IEvent { }
    public sealed class HudNotificationEvent : IEvent
    {
        public HudNotificationEvent(string id, string message, HudNotificationSeverity severity = HudNotificationSeverity.Info)
        { Id = id ?? string.Empty; Message = message ?? string.Empty; Severity = severity; }
        public string Id { get; }
        public string Message { get; }
        public HudNotificationSeverity Severity { get; }
    }

    /// <summary>Read-only UI state and command boundary; never mutates gameplay directly.</summary>
    public sealed class RtsHudViewModel : IDisposable
    {
        private readonly IHudQuery _query;
        private readonly IHudCommandSink _commands;
        private readonly int _notificationCapacity;
        private readonly List<HudEntry> _notifications = new List<HudEntry>();
        private readonly IDisposable[] _subscriptions;
        private HudSnapshot _snapshot;
        private bool _dirty = true;

        public RtsHudViewModel(IHudQuery query, IHudCommandSink commands, EventBus events = null, int notificationCapacity = 6)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            if (notificationCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(notificationCapacity));
            _notificationCapacity = notificationCapacity;
            _subscriptions = events == null ? Array.Empty<IDisposable>() : new[]
            {
                events.Subscribe<HudInvalidatedEvent>(_ => _dirty = true),
                events.Subscribe<HudNotificationEvent>(AddNotification),
            };
        }

        public HudSnapshot Snapshot
        {
            get { if (_dirty || _snapshot == null) Refresh(); return _snapshot; }
        }

        public void Refresh()
        {
            HudSnapshot source = _query.Query() ?? new HudSnapshot(Array.Empty<HudPanelViewModel>());
            var panels = source.Panels.Values.Where(item => item.Id != HudPanelId.Notification).ToList();
            panels.Add(new HudPanelViewModel(HudPanelId.Notification, "Notifications", _notifications));
            _snapshot = new HudSnapshot(panels, source.Revision); _dirty = false;
        }

        public HudCommandResult Execute(HudCommand command) => _commands.Dispatch(command);

        public void AddNotification(HudNotificationEvent notification)
        {
            if (notification == null || string.IsNullOrWhiteSpace(notification.Message)) return;
            _notifications.Add(new HudEntry(notification.Id, notification.Message, notification.Severity.ToString()));
            while (_notifications.Count > _notificationCapacity) _notifications.RemoveAt(0);
            _dirty = true;
        }

        public void DismissNotification(string id)
        {
            _notifications.RemoveAll(item => string.Equals(item.Id, id, StringComparison.Ordinal)); _dirty = true;
        }

        public void Dispose() { foreach (IDisposable subscription in _subscriptions) subscription.Dispose(); }
    }
}
