using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Definitions;

namespace AegisRTS.Gameplay.VerticalSlice
{
    public sealed class VerticalSliceUnitRole
    {
        public VerticalSliceUnitRole(string role, DefinitionId unitId)
        {
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("Unit role is required.", nameof(role));
            if (!unitId.IsValid) throw new ArgumentException("Unit ID is required.", nameof(unitId));
            Role = role.Trim().ToLowerInvariant(); UnitId = unitId;
        }
        public string Role { get; }
        public DefinitionId UnitId { get; }
    }

    /// <summary>World-agnostic semantic bindings for the framework vertical slice.</summary>
    public sealed class VerticalSliceDefinition
    {
        public VerticalSliceDefinition(DefinitionId id, DefinitionId contentPackId, string worldId,
            IEnumerable<DefinitionId> resourceIds, IEnumerable<VerticalSliceUnitRole> unitRoles,
            IEnumerable<DefinitionId> heroIds, DefinitionId economyBuildingId, DefinitionId recruitmentBuildingId,
            DefinitionId playerCityId, DefinitionId villageId, DefinitionId enemyFortressId,
            DefinitionId gateId, DefinitionId aiProfileId)
        {
            if (!id.IsValid || !contentPackId.IsValid) throw new ArgumentException("Scenario and content pack IDs are required.");
            if (string.IsNullOrWhiteSpace(worldId)) throw new ArgumentException("World ID is required.", nameof(worldId));
            Id = id; ContentPackId = contentPackId; WorldId = worldId.Trim();
            ResourceIds = Copy(resourceIds); UnitRoles = Copy(unitRoles); HeroIds = Copy(heroIds);
            EconomyBuildingId = Required(economyBuildingId); RecruitmentBuildingId = Required(recruitmentBuildingId);
            PlayerCityId = Required(playerCityId); VillageId = Required(villageId); EnemyFortressId = Required(enemyFortressId);
            GateId = Required(gateId); AiProfileId = Required(aiProfileId);
        }

        public DefinitionId Id { get; }
        public DefinitionId ContentPackId { get; }
        public string WorldId { get; }
        public IReadOnlyList<DefinitionId> ResourceIds { get; }
        public IReadOnlyList<VerticalSliceUnitRole> UnitRoles { get; }
        public IReadOnlyList<DefinitionId> HeroIds { get; }
        public DefinitionId EconomyBuildingId { get; }
        public DefinitionId RecruitmentBuildingId { get; }
        public DefinitionId PlayerCityId { get; }
        public DefinitionId VillageId { get; }
        public DefinitionId EnemyFortressId { get; }
        public DefinitionId GateId { get; }
        public DefinitionId AiProfileId { get; }

        private static DefinitionId Required(DefinitionId id)
        { if (!id.IsValid) throw new ArgumentException("A referenced definition ID is required."); return id; }
        private static IReadOnlyList<T> Copy<T>(IEnumerable<T> values) =>
            new ReadOnlyCollection<T>(new List<T>(values ?? Array.Empty<T>()));
    }

    public sealed class VerticalSliceJsonLoader
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip };

        public VerticalSliceDefinition Load(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Vertical slice JSON is required.", nameof(json));
            try
            {
                Document value = JsonSerializer.Deserialize<Document>(json, Options);
                if (value == null) throw new FormatException("Vertical slice JSON produced no document.");
                var resources = new List<DefinitionId>();
                foreach (string id in value.ResourceIds ?? Array.Empty<string>()) resources.Add(new DefinitionId(id));
                var units = new List<VerticalSliceUnitRole>();
                foreach (UnitRoleDocument unit in value.UnitRoles ?? Array.Empty<UnitRoleDocument>())
                    units.Add(new VerticalSliceUnitRole(unit.Role, new DefinitionId(unit.UnitId)));
                var heroes = new List<DefinitionId>();
                foreach (string id in value.HeroIds ?? Array.Empty<string>()) heroes.Add(new DefinitionId(id));
                return new VerticalSliceDefinition(new DefinitionId(value.Id), new DefinitionId(value.ContentPackId), value.WorldId,
                    resources, units, heroes, new DefinitionId(value.EconomyBuildingId), new DefinitionId(value.RecruitmentBuildingId),
                    new DefinitionId(value.PlayerCityId), new DefinitionId(value.VillageId), new DefinitionId(value.EnemyFortressId),
                    new DefinitionId(value.GateId), new DefinitionId(value.AiProfileId));
            }
            catch (Exception exception) when (exception is JsonException || exception is ArgumentException)
            { throw new FormatException("Vertical slice JSON is invalid.", exception); }
        }

        private sealed class Document
        {
            public string Id { get; set; }
            public string ContentPackId { get; set; }
            public string WorldId { get; set; }
            public string[] ResourceIds { get; set; }
            public UnitRoleDocument[] UnitRoles { get; set; }
            public string[] HeroIds { get; set; }
            public string EconomyBuildingId { get; set; }
            public string RecruitmentBuildingId { get; set; }
            public string PlayerCityId { get; set; }
            public string VillageId { get; set; }
            public string EnemyFortressId { get; set; }
            public string GateId { get; set; }
            public string AiProfileId { get; set; }
        }
        private sealed class UnitRoleDocument { public string Role { get; set; } public string UnitId { get; set; } }
    }

    public sealed class VerticalSliceValidationResult
    {
        public VerticalSliceValidationResult(IEnumerable<string> issues)
        { Issues = new ReadOnlyCollection<string>(new List<string>(issues)); }
        public IReadOnlyList<string> Issues { get; }
        public bool IsValid => Issues.Count == 0;
    }

    public sealed class VerticalSliceValidator
    {
        private static readonly string[] RequiredRoles = { "infantry", "archer", "cavalry", "siege-unit" };

        public VerticalSliceValidationResult Validate(VerticalSliceDefinition definition, ContentPack pack)
        {
            var issues = new List<string>();
            if (definition == null || pack == null) { issues.Add("Definition and content pack are required."); return new VerticalSliceValidationResult(issues); }
            if (definition.ContentPackId != pack.Id) issues.Add("Content pack ID does not match the scenario binding.");
            if (definition.ResourceIds.Count != 2) issues.Add("Exactly two resource bindings are required.");
            if (definition.HeroIds.Count != 2) issues.Add("Exactly two hero bindings are required.");
            if (definition.UnitRoles.Count != RequiredRoles.Length) issues.Add("Exactly four unit role bindings are required.");
            var roles = new HashSet<string>(StringComparer.Ordinal);
            foreach (VerticalSliceUnitRole role in definition.UnitRoles) roles.Add(role.Role);
            foreach (string role in RequiredRoles) if (!roles.Contains(role)) issues.Add($"Missing unit role '{role}'.");
            foreach (DefinitionId id in definition.ResourceIds) if (!Contains(pack.Resources, id)) issues.Add($"Missing resource '{id}'.");
            foreach (VerticalSliceUnitRole role in definition.UnitRoles) if (!Contains(pack.Units, role.UnitId)) issues.Add($"Missing unit '{role.UnitId}'.");
            foreach (DefinitionId id in definition.HeroIds) if (!Contains(pack.Heroes, id)) issues.Add($"Missing hero '{id}'.");
            if (!Contains(pack.Buildings, definition.EconomyBuildingId)) issues.Add("Economy building is missing.");
            if (!Contains(pack.Buildings, definition.RecruitmentBuildingId)) issues.Add("Recruitment building is missing.");
            if (!Contains(pack.Settlements, definition.PlayerCityId) || !Contains(pack.Settlements, definition.VillageId) ||
                !Contains(pack.Settlements, definition.EnemyFortressId)) issues.Add("Player city, village, and enemy fortress are required.");
            if (!Contains(pack.DefenseStructures, definition.GateId)) issues.Add("Fortress gate is missing.");
            if (!Contains(pack.AiProfiles, definition.AiProfileId)) issues.Add("AI profile is missing.");
            return new VerticalSliceValidationResult(issues);
        }

        private static bool Contains<T>(IReadOnlyList<T> values, DefinitionId id) where T : IDefinition
        { foreach (T value in values) if (value.Id == id) return true; return false; }
    }
}
