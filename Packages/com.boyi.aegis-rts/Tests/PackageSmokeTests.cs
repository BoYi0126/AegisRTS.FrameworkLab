using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Serialization;
using NUnit.Framework;

namespace AegisRTS.Package.Tests
{
    public sealed class PackageSmokeTests
    {
        [Test]
        public void CoreAndGameplayAssemblies_AreConsumableFromPackage()
        {
            var commands = new CommandBus();
            Assert.That(new EntityId(1).IsValid, Is.True);
            Assert.That(commands.GetDebugSummary(), Does.Contain("Handlers=0"));
        }

        [Test]
        public void ConsumerContentPack_CanBeLoaded()
        {
            const string json = "{\"id\":\"consumer.pack\",\"displayName\":\"Consumer\",\"declaredTags\":[],\"resources\":[],\"units\":[],\"heroes\":[],\"abilities\":[],\"buildings\":[],\"technologies\":[],\"settlements\":[],\"defenseStructures\":[],\"aiProfiles\":[],\"rules\":{}}";
            ContentPack pack = new ContentPackJsonLoader().Load(json);
            Assert.That(pack.Id.Value, Is.EqualTo("consumer.pack"));
        }
    }
}
