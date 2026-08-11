using System;
using System.Collections.Generic;
using AegisRTS.Core.Commands;
using AegisRTS.Core.Entities;
using AegisRTS.Core.Events;
using AegisRTS.Gameplay.Buildings;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Recruitment;
using AegisRTS.Gameplay.Technology;
using UnityEngine;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Demo
{
    /// <summary>Phase 08 end-to-end economy, construction, research, and recruitment acceptance sandbox.</summary>
    [DisallowMultipleComponent]
    public sealed class EconomyProductionSandboxBootstrap : MonoBehaviour, IBuildingCompletionSink,
        ITechnologyCompletionSink, IUnitSpawnSink
    {
        private readonly List<IDisposable> _routers = new List<IDisposable>();
        private CommandBus _commands;
        private BuildingSystem _buildings;
        private TechnologySystem _technologies;
        private RecruitmentSystem _recruitment;
        private int _stage;

        public EconomySystem Economy { get; private set; }
        public EntityId SettlementId { get; private set; }
        public EntityId FactionId { get; private set; }
        public DefinitionId ResourceId { get; private set; }
        public int CompletedBuildings { get; private set; }
        public int CompletedTechnologies { get; private set; }
        public int SpawnedUnits { get; private set; }
        public bool AcceptancePassed { get; private set; }

        private void Awake()
        {
            var events = new EventBus(); _commands = new CommandBus();
            SettlementId = new EntityId(8101); FactionId = new EntityId(8102);
            ResourceId = new DefinitionId("neutral.supplies");
            DefinitionId buildingId = new DefinitionId("neutral.outpost");
            DefinitionId technologyId = new DefinitionId("neutral.training");
            DefinitionId unitId = new DefinitionId("neutral.infantry");

            var building = new BuildingDefinition(buildingId, "Debug Outpost", 800, "PF_Structure_Placeholder",
                new[] { new ResourceCost(ResourceId, 200) }, Tags("structure"), 0.2, null, null,
                new[] { new ResourceProduction(ResourceId, 4) }, 5);
            var technology = new TechnologyDefinition(technologyId, "Basic Training",
                new[] { new ResourceCost(ResourceId, 100) }, Array.Empty<DefinitionId>(), Tags("technology"), 0.2,
                new[] { new TechnologyModifier("unit.damage", 2, 1.05) });
            var unit = new UnitDefinition(unitId, "Debug Infantry", 100, 4, "PF_Unit_Placeholder",
                new[] { new ResourceCost(ResourceId, 50) }, Array.Empty<DefinitionId>(), Tags("unit"), 0.2, 1,
                new[] { buildingId }, new[] { technologyId });

            Economy = new EconomySystem(true, events);
            Economy.RegisterAccount(SettlementId, new[] { new ResourceCost(ResourceId, 500) }, 0, 2);
            Economy.RegisterAccount(FactionId, new[] { new ResourceCost(ResourceId, 200) });
            _technologies = new TechnologySystem(new[] { technology }, Economy, sink: this, eventBus: events);
            _buildings = new BuildingSystem(new[] { building }, Economy, _technologies, this, events);
            _recruitment = new RecruitmentSystem(new[] { unit }, Economy, _buildings, _technologies, this, events);
            _routers.Add(new TechnologyCommandRouter(_commands, _technologies));
            _routers.Add(new BuildingCommandRouter(_commands, _buildings));
            _routers.Add(new RecruitmentCommandRouter(_commands, _recruitment));
            _commands.Dispatch(new ConstructBuildingCommand(SettlementId, FactionId, buildingId));
        }

        private void Update()
        {
            double delta = Time.deltaTime;
            Economy.Tick(delta); _buildings.Tick(delta); _technologies.Tick(delta); _recruitment.Tick(delta);
            if (_stage == 0 && CompletedBuildings == 1)
            { _stage = 1; _commands.Dispatch(new ResearchTechnologyCommand(SettlementId, FactionId, new DefinitionId("neutral.training"))); }
            if (_stage == 1 && CompletedTechnologies == 1)
            { _stage = 2; _commands.Dispatch(new RecruitUnitCommand(SettlementId, FactionId, new DefinitionId("neutral.infantry"))); }
            if (_stage == 2 && SpawnedUnits == 1) { _stage = 3; AcceptancePassed = true; }
        }

        public void BuildingCompleted(EntityId settlementId, DefinitionId buildingId) => CompletedBuildings++;
        public void TechnologyCompleted(EntityId factionId, DefinitionId technologyId) => CompletedTechnologies++;
        public void SpawnUnit(EntityId settlementId, EntityId factionId, DefinitionId unitId) => SpawnedUnits++;

        private static ContentTag[] Tags(params string[] values)
        { var result = new ContentTag[values.Length]; for (int i = 0; i < values.Length; i++) result[i] = new ContentTag(values[i]); return result; }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 180, 620, 125), GUI.skin.box);
            GUILayout.Label("Phase 08 Economy / Recruit / Build / Tech");
            GUILayout.Label(Economy?.GetDebugSummary() ?? "Economy unavailable");
            GUILayout.Label($"Buildings: {CompletedBuildings} | Technologies: {CompletedTechnologies} | Units: {SpawnedUnits}");
            GUILayout.Label($"Acceptance: {(AcceptancePassed ? "PASS" : "RUNNING")}");
            GUILayout.EndArea();
        }

        private void OnDestroy()
        { foreach (IDisposable router in _routers) router.Dispose(); _routers.Clear(); }
    }
}
