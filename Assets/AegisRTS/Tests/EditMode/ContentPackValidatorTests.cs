using System;
using System.Linq;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Content.Validation;
using NUnit.Framework;

namespace AegisRTS.Tests.EditMode
{
    public sealed class ContentPackValidatorTests
    {
        private readonly ContentPackValidator _validator = new ContentPackValidator();

        [Test]
        public void Validate_DetectsDuplicateDefinitionIdAcrossTypes()
        {
            PackParts parts = ContentPackTestFactory.CreateValidParts();
            parts.Abilities.Add(new AbilityDefinition(
                parts.Resources[0].Id, "Duplicate", 1d, 1d, ContentPackTestFactory.Tags("ability")));

            ContentValidationResult result = _validator.Validate(parts.Build(), ContentPackTestFactory.Assets);

            Assert.That(result.Issues.Any(issue =>
                issue.Code == ContentValidationIssueCode.DuplicateDefinitionId), Is.True);
        }

        [Test]
        public void Validate_DetectsMissingTypedReferences()
        {
            PackParts parts = ContentPackTestFactory.CreateValidParts();
            parts.Units.Clear();
            parts.Units.Add(new UnitDefinition(
                new DefinitionId("test.bad-unit"), "Bad Unit", 10d, 1d, "PF_Unit_Placeholder",
                new[] { new ResourceCost(new DefinitionId("missing.resource"), 1d) },
                new[] { new DefinitionId("missing.ability") },
                ContentPackTestFactory.Tags("unit")));

            ContentValidationResult result = _validator.Validate(parts.Build(), ContentPackTestFactory.Assets);

            Assert.That(result.Issues.Count(issue =>
                issue.Code == ContentValidationIssueCode.MissingReference), Is.EqualTo(2));
        }

        [Test]
        public void Validate_DetectsInvalidStatsAndCosts()
        {
            PackParts parts = ContentPackTestFactory.CreateValidParts();
            parts.Units.Clear();
            parts.Units.Add(new UnitDefinition(
                new DefinitionId("test.bad-unit"), "Bad Unit", 0d, -1d, "PF_Unit_Placeholder",
                new[] { new ResourceCost(parts.Resources[0].Id, -5d) },
                Array.Empty<DefinitionId>(),
                ContentPackTestFactory.Tags("unit")));

            ContentValidationResult result = _validator.Validate(parts.Build(), ContentPackTestFactory.Assets);

            Assert.That(result.Issues.Any(issue => issue.Code == ContentValidationIssueCode.InvalidStat), Is.True);
            Assert.That(result.Issues.Any(issue => issue.Code == ContentValidationIssueCode.InvalidCost), Is.True);
        }

        [Test]
        public void Validate_DetectsInvalidHeroLeadership()
        {
            PackParts parts = ContentPackTestFactory.CreateValidParts();
            parts.Heroes.Clear();
            parts.Heroes.Add(new HeroDefinition(
                new DefinitionId("test.bad-hero"), "Bad Hero", 100d, 4d, "PF_Hero_Placeholder",
                Array.Empty<ResourceCost>(), Array.Empty<DefinitionId>(),
                ContentPackTestFactory.Tags("hero"), leadership: -1d));

            ContentValidationResult result = _validator.Validate(parts.Build(), ContentPackTestFactory.Assets);

            Assert.That(result.Issues.Any(issue =>
                issue.Code == ContentValidationIssueCode.InvalidStat && issue.Message.Contains("leadership")), Is.True);
        }

        [Test]
        public void Validate_DetectsTechnologyCycle()
        {
            PackParts parts = ContentPackTestFactory.CreateValidParts();
            DefinitionId firstId = new DefinitionId("test.tech-a");
            DefinitionId secondId = new DefinitionId("test.tech-b");
            parts.Technologies.Clear();
            parts.Technologies.Add(new TechnologyDefinition(
                firstId, "A", Array.Empty<ResourceCost>(), new[] { secondId },
                ContentPackTestFactory.Tags("technology")));
            parts.Technologies.Add(new TechnologyDefinition(
                secondId, "B", Array.Empty<ResourceCost>(), new[] { firstId },
                ContentPackTestFactory.Tags("technology")));

            ContentValidationResult result = _validator.Validate(parts.Build(), ContentPackTestFactory.Assets);

            Assert.That(result.Issues.Any(issue => issue.Code == ContentValidationIssueCode.TechnologyCycle), Is.True);
        }

        [Test]
        public void Validate_DetectsMissingPrefabAndUndeclaredTag()
        {
            PackParts parts = ContentPackTestFactory.CreateValidParts();
            parts.Units.Clear();
            parts.Units.Add(new UnitDefinition(
                new DefinitionId("test.bad-unit"), "Bad Unit", 10d, 1d, "PF_Missing",
                Array.Empty<ResourceCost>(), Array.Empty<DefinitionId>(),
                ContentPackTestFactory.Tags("flying")));

            ContentValidationResult result = _validator.Validate(parts.Build(), ContentPackTestFactory.Assets);

            Assert.That(result.Issues.Any(issue => issue.Code == ContentValidationIssueCode.MissingPrefab), Is.True);
            Assert.That(result.Issues.Any(issue => issue.Code == ContentValidationIssueCode.MissingTag), Is.True);
        }
    }
}
