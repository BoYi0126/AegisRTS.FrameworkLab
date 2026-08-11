using AegisRTS.Core.Entities;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class EntityIdTests
    {
        [Test]
        public void EqualValues_AreEqualAndHaveSameHashCode()
        {
            var first = new EntityId(42);
            var second = new EntityId(42);
            var different = new EntityId(43);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(different));
            Assert.That(EntityId.Invalid.IsValid, Is.False);
        }

        [Test]
        public void Generator_IssuesMonotonicNonZeroIds()
        {
            var generator = new EntityIdGenerator(10);

            Assert.That(generator.Next(), Is.EqualTo(new EntityId(10)));
            Assert.That(generator.Next(), Is.EqualTo(new EntityId(11)));

            generator.Reset();
            Assert.That(generator.Next(), Is.EqualTo(new EntityId(1)));
        }
    }
}
