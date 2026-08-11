using System.Collections.Generic;

namespace AegisRTS.Gameplay.Content.Definitions
{
    /// <summary>Data-authored siege structure; custom type IDs allow world-specific extensions.</summary>
    public sealed class DefenseStructureDefinition : DefinitionBase
    {
        public DefenseStructureDefinition(DefinitionId id, string displayName, string structureTypeId,
            string siegeAreaId, double maxHealth, double armor, string prefabId, IEnumerable<ContentTag> tags)
            : base(id, displayName, tags)
        {
            StructureTypeId = Normalize(structureTypeId);
            SiegeAreaId = Normalize(siegeAreaId);
            MaxHealth = maxHealth;
            Armor = armor;
            PrefabId = prefabId ?? string.Empty;
        }

        public string StructureTypeId { get; }
        public string SiegeAreaId { get; }
        public double MaxHealth { get; }
        public double Armor { get; }
        public string PrefabId { get; }

        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value)
            ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
