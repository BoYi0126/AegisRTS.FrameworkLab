using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Validation;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class ContentPackJsonLoaderTests
    {
        [TestCase("DemoNeutral", "demo.neutral")]
        [TestCase("DemoThreeKingdoms", "demo.three-kingdoms")]
        [TestCase("DemoFantasy", "demo.fantasy")]
        public void DemoPack_LoadsAndValidates(string folder, string expectedId)
        {
            ContentPack pack = ContentPackTestFactory.LoadDemoPack(folder);
            ContentValidationResult validation = new ContentPackValidator().Validate(
                pack,
                ContentPackTestFactory.Assets);

            Assert.That(pack.Id.Value, Is.EqualTo(expectedId));
            Assert.That(pack.Rules, Is.Not.Null);
            Assert.That(pack.Resources.Count, Is.EqualTo(1));
            Assert.That(pack.Units.Count, Is.EqualTo(1));
            Assert.That(pack.Heroes.Count, Is.EqualTo(1));
            Assert.That(pack.Heroes[0].Leadership, Is.GreaterThan(0));
            Assert.That(pack.Abilities.Count, Is.EqualTo(1));
            Assert.That(pack.Buildings.Count, Is.EqualTo(1));
            Assert.That(pack.Technologies.Count, Is.EqualTo(1));
            Assert.That(pack.Settlements.Count, Is.EqualTo(1));
            Assert.That(validation.IsValid, Is.True,
                string.Join(System.Environment.NewLine, validation.Issues));
        }
    }
}
