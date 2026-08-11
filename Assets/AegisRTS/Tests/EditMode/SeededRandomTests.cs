using AegisRTS.Core.Random;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class SeededRandomTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var first = new SeededRandom(123456);
            var second = new SeededRandom(123456);

            for (int index = 0; index < 128; index++)
            {
                Assert.That(first.NextUInt(), Is.EqualTo(second.NextUInt()));
            }

            Assert.That(first.DrawCount, Is.EqualTo(128));
            Assert.That(second.DrawCount, Is.EqualTo(128));
        }

        [Test]
        public void KnownSeed_ProducesStableReferenceVector()
        {
            var random = new SeededRandom(123456);
            uint[] expected =
            {
                566038743u,
                3318627358u,
                2343776568u,
                1669880209u,
                1144641032u,
            };

            foreach (uint expectedValue in expected)
            {
                Assert.That(random.NextUInt(), Is.EqualTo(expectedValue));
            }
        }

        [Test]
        public void RangeMethods_StayWithinRequestedBounds()
        {
            var random = new SeededRandom(-17);

            for (int index = 0; index < 256; index++)
            {
                Assert.That(random.NextInt(-10, 25), Is.InRange(-10, 24));
                Assert.That(random.NextFloat(), Is.InRange(0f, 0.99999994f));
                Assert.That(random.NextDouble(), Is.GreaterThanOrEqualTo(0d).And.LessThan(1d));
            }

            Assert.That(random.NextBool(0d), Is.False);
            Assert.That(random.NextBool(1d), Is.True);
        }
    }
}
