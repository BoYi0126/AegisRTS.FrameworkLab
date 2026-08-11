using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Definitions;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class ContentPackServiceTests
    {
        [Test]
        public void Load_CanSwitchBetweenAllDemoPacks()
        {
            var service = new ContentPackService();

            Assert.That(service.Load(
                ContentPackTestFactory.LoadDemoPack("DemoNeutral"),
                ContentPackTestFactory.Assets).Succeeded, Is.True);
            Assert.That(service.ActiveCatalog.Pack.Id.Value, Is.EqualTo("demo.neutral"));

            Assert.That(service.Load(
                ContentPackTestFactory.LoadDemoPack("DemoThreeKingdoms"),
                ContentPackTestFactory.Assets).Succeeded, Is.True);
            Assert.That(service.ActiveCatalog.Pack.Id.Value, Is.EqualTo("demo.three-kingdoms"));

            Assert.That(service.Load(
                ContentPackTestFactory.LoadDemoPack("DemoFantasy"),
                ContentPackTestFactory.Assets).Succeeded, Is.True);
            Assert.That(service.ActiveCatalog.Pack.Id.Value, Is.EqualTo("demo.fantasy"));
            Assert.That(service.ActiveCatalog.TryGet(
                new DefinitionId("fantasy.golem"),
                out UnitDefinition unit), Is.True);
            Assert.That(unit.DisplayName, Is.EqualTo("Golem"));
        }

        [Test]
        public void Load_InvalidPack_DoesNotReplaceActiveCatalog()
        {
            var service = new ContentPackService();
            service.Load(ContentPackTestFactory.CreateValidParts().Build(), ContentPackTestFactory.Assets);
            ContentCatalog previous = service.ActiveCatalog;

            PackParts invalidParts = ContentPackTestFactory.CreateValidParts();
            invalidParts.Units.Clear();
            invalidParts.Units.Add(new UnitDefinition(
                new DefinitionId("test.invalid"), "Invalid", -1d, 1d, "PF_Missing",
                System.Array.Empty<ResourceCost>(), System.Array.Empty<DefinitionId>(),
                ContentPackTestFactory.Tags("unit")));

            ContentPackLoadResult result = service.Load(invalidParts.Build(), ContentPackTestFactory.Assets);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(service.ActiveCatalog, Is.SameAs(previous));
        }
    }
}
