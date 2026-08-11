using System;
using AegisRTS.Gameplay.Content.Definitions;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class DefinitionValueTests
    {
        [Test]
        public void DefinitionId_NormalizesCaseAndWhitespace()
        {
            var first = new DefinitionId("  Demo.Unit-One ");
            var second = new DefinitionId("demo.unit-one");

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.Value, Is.EqualTo("demo.unit-one"));
        }

        [Test]
        public void ContentTag_RejectsWorldDisplayTextAndSpaces()
        {
            Assert.Throws<ArgumentException>(() => new ContentTag("Flying Unit"));
        }
    }
}
