using System;
using System.Collections.Generic;
using System.Linq;
using AegisRTS.Gameplay.Objectives;
using UnityEngine;

namespace AegisRTS.Demo
{
    /// <summary>Loads and completes authored scenarios through one generic Phase 11 runtime.</summary>
    [DisallowMultipleComponent]
    public sealed class ScenarioSandboxBootstrap : MonoBehaviour
    {
        [SerializeField] private TextAsset[] scenarioAssets = Array.Empty<TextAsset>();
        private readonly List<GameModeType> _completedModes = new List<GameModeType>();

        public int LoadedScenarioCount { get; private set; }
        public int CompletedScenarioCount => _completedModes.Count;
        public IReadOnlyList<GameModeType> CompletedModes => _completedModes;
        public bool AcceptancePassed => LoadedScenarioCount >= 4 && CompletedScenarioCount == LoadedScenarioCount;
        public string LastDebugSummary { get; private set; } = string.Empty;

        private void Awake()
        {
            var loader = new ScenarioJsonLoader();
            foreach (TextAsset asset in scenarioAssets.Where(item => item != null))
            {
                ScenarioDefinition definition = loader.Load(asset.text); LoadedScenarioCount++;
                var scenario = new ScenarioSystem(); scenario.Start(definition);
                CompleteWithGenericDriver(scenario);
                LastDebugSummary = scenario.GetDebugSummary();
                if (scenario.Status == ScenarioStatus.Victory) _completedModes.Add(definition.GameMode.Type);
            }
        }

        private static void CompleteWithGenericDriver(ScenarioSystem scenario)
        {
            for (int pass = 0; pass < 32 && scenario.Status == ScenarioStatus.Running; pass++)
            {
                scenario.TryGetSnapshot(out ScenarioSnapshot snapshot);
                ObjectiveSnapshot objective = snapshot.Objectives.FirstOrDefault(item => item.Status == ObjectiveStatus.Active);
                if (objective == null) break;
                if (objective.Definition.FactId == ScenarioSystem.ElapsedSecondsFact)
                    scenario.Update(Math.Max(0d, objective.Definition.TargetValue - snapshot.ElapsedSeconds));
                else
                    scenario.SetFact(objective.Definition.FactId, objective.Definition.TargetValue);
                if (objective.Definition.RequiredDurationSeconds > 0d)
                    scenario.Update(objective.Definition.RequiredDurationSeconds);
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(12, 640, 620, 78), "Phase 11 — Data-Driven Scenarios");
            GUI.Label(new Rect(24, 668, 590, 20), $"Loaded={LoadedScenarioCount}, Completed={CompletedScenarioCount}, PASS={AcceptancePassed}");
            GUI.Label(new Rect(24, 690, 590, 20), string.Join(" | ", _completedModes));
        }
    }
}
