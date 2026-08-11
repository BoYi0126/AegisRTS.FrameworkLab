using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AegisRTS.Core.Events;
using AegisRTS.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace AegisRTS.Tests.EditMode
{
    public sealed class HudUiTests
    {
        [Test]
        public void ThemeData_LoadsThreeDistinctWorldThemes()
        {
            HudThemeDefinition[] themes = LoadThemes();
            Assert.That(themes.Select(item => item.Id), Is.EquivalentTo(new[] { "ui.neutral", "ui.three-kingdoms", "ui.fantasy" }));
            Assert.That(themes.Select(item => item.Panel).Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void ThemeData_RejectsInvalidColorAndScale()
        {
            Assert.Throws<ArgumentException>(() => new HudThemeDefinition("bad", "Bad", "red", "#000000", "#000000", "#000000", "#FFFFFF", "#AAAAAA"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HudThemeDefinition("bad", "Bad", "#000000", "#000000", "#000000", "#000000", "#FFFFFF", "#AAAAAA", 3));
        }

        [Test]
        public void Snapshot_RejectsDuplicatePanelIdentity()
        {
            var panel = new HudPanelViewModel(HudPanelId.ResourceBar, "Resources");
            Assert.Throws<ArgumentException>(() => new HudSnapshot(new[] { panel, panel }));
        }

        [Test]
        public void ViewModel_RefreshesOnlyThroughQueryAndInvalidationEvent()
        {
            var events = new EventBus(); var query = new FakeQuery(); var sink = new FakeSink();
            using (var viewModel = new RtsHudViewModel(query, sink, events))
            {
                Assert.That(viewModel.Snapshot.Revision, Is.EqualTo(1));
                Assert.That(viewModel.Snapshot.Revision, Is.EqualTo(1));
                Assert.That(query.Count, Is.EqualTo(1));
                query.Revision = 2; events.Publish(new HudInvalidatedEvent());
                Assert.That(viewModel.Snapshot.Revision, Is.EqualTo(2));
                Assert.That(query.Count, Is.EqualTo(2));
            }
        }

        [Test]
        public void Notifications_AreEventDrivenBoundedAndDismissible()
        {
            var events = new EventBus(); using (var viewModel = new RtsHudViewModel(new FakeQuery(), new FakeSink(), events, 2))
            {
                events.Publish(new HudNotificationEvent("one", "First"));
                events.Publish(new HudNotificationEvent("two", "Second"));
                events.Publish(new HudNotificationEvent("three", "Third"));
                viewModel.Snapshot.TryGetPanel(HudPanelId.Notification, out HudPanelViewModel panel);
                Assert.That(panel.Entries.Select(item => item.Id), Is.EqualTo(new[] { "two", "three" }));
                viewModel.DismissNotification("two");
                viewModel.Snapshot.TryGetPanel(HudPanelId.Notification, out panel);
                Assert.That(panel.Entries.Single().Id, Is.EqualTo("three"));
            }
        }

        [Test]
        public void Commands_AreDelegatedToSinkWithoutViewModelMutation()
        {
            var query = new FakeQuery(); var sink = new FakeSink(); using (var viewModel = new RtsHudViewModel(query, sink))
            {
                long revision = viewModel.Snapshot.Revision;
                Assert.That(viewModel.Execute(new HudCommand("unit.move", "42")).Succeeded, Is.True);
                Assert.That(sink.Commands.Single().CommandId, Is.EqualTo("unit.move"));
                Assert.That(viewModel.Snapshot.Revision, Is.EqualTo(revision));
            }
        }

        [Test]
        public void Presenter_UsesOneStableTenPanelLayoutAcrossThemes()
        {
            GameObject gameObject = new GameObject("HudPresenterTest");
            try
            {
                var presenter = gameObject.AddComponent<RtsHudPresenter>();
                using (var viewModel = new RtsHudViewModel(new CompleteQuery(), new FakeSink()))
                {
                    presenter.Configure(viewModel, LoadThemes(), "ui.neutral"); string layout = presenter.LayoutSignature;
                    Assert.That(presenter.PanelIds.Count, Is.EqualTo(10));
                    Assert.That(presenter.SwitchTheme("ui.three-kingdoms"), Is.True);
                    Assert.That(presenter.LayoutSignature, Is.EqualTo(layout));
                    Assert.That(presenter.SwitchTheme("ui.fantasy"), Is.True);
                    Assert.That(presenter.LayoutSignature, Is.EqualTo(layout));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(gameObject); }
        }

        [Test]
        public void Presenter_RejectsUnknownThemeWithoutChangingCurrentTheme()
        {
            GameObject gameObject = new GameObject("HudThemeTest");
            try
            {
                var presenter = gameObject.AddComponent<RtsHudPresenter>(); using (var viewModel = new RtsHudViewModel(new CompleteQuery(), new FakeSink()))
                {
                    presenter.Configure(viewModel, LoadThemes(), "ui.neutral");
                    Assert.That(presenter.SwitchTheme("missing"), Is.False);
                    Assert.That(presenter.CurrentThemeId, Is.EqualTo("ui.neutral"));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(gameObject); }
        }

        private static HudThemeDefinition[] LoadThemes() => new[] { "Neutral.json", "ThreeKingdoms.json", "Fantasy.json" }.Select(file =>
            new HudThemeJsonLoader().Load(File.ReadAllText(Path.Combine(ProjectRoot(), "Assets", "AegisRTS", "Content", "UIThemes", file)))).ToArray();

        private static string ProjectRoot()
        {
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null && !Directory.Exists(Path.Combine(current.FullName, "Assets", "AegisRTS"))) current = current.Parent;
            return current?.FullName ?? throw new DirectoryNotFoundException();
        }

        private sealed class FakeQuery : IHudQuery
        {
            public int Count; public long Revision = 1;
            public HudSnapshot Query() { Count++; return new HudSnapshot(new[] { new HudPanelViewModel(HudPanelId.ResourceBar, "Resources") }, Revision); }
        }
        private sealed class CompleteQuery : IHudQuery
        {
            public HudSnapshot Query() => new HudSnapshot(Enum.GetValues(typeof(HudPanelId)).Cast<HudPanelId>()
                .Where(id => id != HudPanelId.Notification).Select(id => new HudPanelViewModel(id, id.ToString())));
        }
        private sealed class FakeSink : IHudCommandSink
        {
            public readonly List<HudCommand> Commands = new List<HudCommand>();
            public HudCommandResult Dispatch(HudCommand command) { Commands.Add(command); return HudCommandResult.Success(); }
        }
    }
}
