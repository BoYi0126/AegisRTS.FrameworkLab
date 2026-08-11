using System;
using System.Collections.Generic;
using System.IO;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Serialization;
using AegisRTS.Gameplay.Content.Validation;
using UnityEditor;
using UnityEngine;

namespace AegisRTS.Tools
{
    public static class PackageContentValidatorMenu
    {
        [MenuItem("Tools/AegisRTS/Validate Content Pack...")]
        public static void ValidateContentPack()
        {
            string path = EditorUtility.OpenFilePanel("Validate AegisRTS Content Pack", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                ContentPack pack = new ContentPackJsonLoader().Load(File.ReadAllText(path));
                var prefabIds = new List<string>();
                foreach (var unit in pack.Units) prefabIds.Add(unit.PrefabId);
                foreach (var hero in pack.Heroes) prefabIds.Add(hero.PrefabId);
                foreach (var building in pack.Buildings) prefabIds.Add(building.PrefabId);
                foreach (var settlement in pack.Settlements) prefabIds.Add(settlement.PrefabId);
                foreach (var structure in pack.DefenseStructures) prefabIds.Add(structure.PrefabId);
                ContentValidationResult result = new ContentPackValidator().Validate(pack, new ContentAssetCatalog(prefabIds));
                if (result.IsValid) EditorUtility.DisplayDialog("AegisRTS", $"'{pack.DisplayName}' is valid.", "OK");
                else EditorUtility.DisplayDialog("AegisRTS", string.Join(Environment.NewLine, result.Issues), "OK");
            }
            catch (Exception exception)
            { EditorUtility.DisplayDialog("AegisRTS", exception.Message, "OK"); }
        }
    }
}
