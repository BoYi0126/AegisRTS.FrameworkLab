using UnityEngine;

namespace AegisRTS.Demo.PlayablePrototype
{
    /// <summary>Maps content prefab IDs to presentation prefabs without leaking Unity objects into gameplay definitions.</summary>
    public static class PrototypeUnitArtCatalog
    {
        public const string InfantryPrefabId = "PF_Unit_Infantry";
        public const string InfantryResourcePath = "AegisRTS/Units/Infantry/PF_Unit_Infantry";
        public const string ArcherPrefabId = "PF_Unit_Archer";
        public const string ArcherResourcePath = "AegisRTS/Units/Archer/PF_Unit_Archer";

        public static GameObject Load(string prefabId)
        {
            switch (prefabId)
            {
                case InfantryPrefabId: return Resources.Load<GameObject>(InfantryResourcePath);
                case ArcherPrefabId: return Resources.Load<GameObject>(ArcherResourcePath);
                default: return null;
            }
        }

        public static bool TryInstantiate(string prefabId, Transform parent, Color teamColor,
            out GameObject instance, out PrototypeUnitArtView artView)
        {
            GameObject prefab = Load(prefabId);
            if (prefab == null)
            {
                instance = null;
                artView = null;
                return false;
            }

            instance = Object.Instantiate(prefab, parent, false);
            instance.name = "Visual";
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.transform.localScale = Vector3.one;
            artView = instance.GetComponent<PrototypeUnitArtView>();
            if (artView == null)
            {
                Object.Destroy(instance);
                instance = null;
                return false;
            }
            artView.ApplyTeamColor(teamColor);
            return true;
        }
    }
}
