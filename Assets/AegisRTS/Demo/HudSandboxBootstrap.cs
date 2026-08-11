using System;
using System.Collections.Generic;
using AegisRTS.Core.Events;
using AegisRTS.Presentation.UI;
using UnityEngine;

namespace AegisRTS.Demo
{
    /// <summary>Phase 12 composition proving one HUD layout can swap world themes without gameplay mutation.</summary>
    [DisallowMultipleComponent]
    public sealed class HudSandboxBootstrap : MonoBehaviour, IHudQuery, IHudCommandSink
    {
        [SerializeField] private TextAsset[] themeAssets = Array.Empty<TextAsset>();
        private RtsHudViewModel _viewModel;
        private long _gameplayRevision = 12;

        public RtsHudPresenter Presenter { get; private set; }
        public int QueryCount { get; private set; }
        public int CommandCount { get; private set; }
        public int LoadedThemeCount { get; private set; }
        public long GameplayRevision => _gameplayRevision;
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            var loader = new HudThemeJsonLoader(); var themes = new List<HudThemeDefinition>();
            foreach (TextAsset asset in themeAssets)
                if (asset != null) { themes.Add(loader.Load(asset.text)); LoadedThemeCount++; }
            var events = new EventBus(); _viewModel = new RtsHudViewModel(this, this, events);
            events.Publish(new HudNotificationEvent("phase12", "HUD query/event/command boundary ready", HudNotificationSeverity.Success));
            Presenter = gameObject.AddComponent<RtsHudPresenter>(); Presenter.Configure(_viewModel, themes, "ui.neutral");
            long beforeRevision = _gameplayRevision; int beforeCommands = CommandCount; string layout = Presenter.LayoutSignature;
            bool switched = Presenter.SwitchTheme("ui.three-kingdoms") && Presenter.LayoutSignature == layout &&
                            Presenter.SwitchTheme("ui.fantasy") && Presenter.LayoutSignature == layout &&
                            Presenter.SwitchTheme("ui.neutral");
            AcceptancePassed = LoadedThemeCount == 3 && Presenter.PanelIds.Count == 10 && switched &&
                               beforeRevision == _gameplayRevision && beforeCommands == CommandCount;
        }

        public HudSnapshot Query()
        {
            QueryCount++;
            return new HudSnapshot(new[]
            {
                Panel(HudPanelId.ResourceBar, "Resources", Entry("resource.supplies", "Supplies", "240"), Entry("population", "Population", "18 / 30")),
                Panel(HudPanelId.SelectionPanel, "Selection", Entry("selected.hero", "Vanguard Hero", "HP 860 / 1000")),
                Panel(HudPanelId.CommandPanel, "Commands", Entry("unit.move", "Move"), Entry("unit.attack", "Attack"), Entry("unit.hold", "Hold")),
                Panel(HudPanelId.AbilityBar, "Abilities", Entry("ability.charge", "Charge", "Ready"), Entry("ability.rally", "Rally", "8s")),
                Panel(HudPanelId.ArmyPanel, "Army", Entry("army.main", "Main Army", "21 units"), Entry("morale", "Morale", "82")),
                Panel(HudPanelId.SettlementPanel, "Settlement", Entry("settlement.frontier", "Frontier", "Defense 120"), Entry("queue", "Recruitment", "2")),
                Panel(HudPanelId.Minimap, "Minimap", Entry("territories", "Territories", "2 / 3"), Entry("threat", "Threat", "East")),
                Panel(HudPanelId.Objective, "Objectives", Entry("capture-frontier", "Capture the frontier", "Active")),
                Panel(HudPanelId.Pause, "Pause", Entry("ui.pause.toggle", "Pause / Resume")),
            }, _gameplayRevision);
        }

        public HudCommandResult Dispatch(HudCommand command)
        {
            CommandCount++; return HudCommandResult.Success();
        }

        private void OnDestroy() => _viewModel?.Dispose();
        private static HudPanelViewModel Panel(HudPanelId id, string title, params HudEntry[] entries) => new HudPanelViewModel(id, title, entries);
        private static HudEntry Entry(string id, string label, string value = "") => new HudEntry(id, label, value);
    }
}
