using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AegisRTS.Core.Entities;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Movement;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Persistence.Save;
using UnityEngine;

namespace AegisRTS.Demo.PlayablePrototype
{
    [Serializable]
    public sealed class PrototypeSaveData
    {
        public int formatVersion = 3;
        public string contentPackId;
        public string scenarioId;
        public double elapsedSeconds;
        public ulong nextEntityId;
        public int randomSeed;
        public ulong randomDrawCount;
        public ulong randomState;
        public double playerMaterial;
        public double playerSupply;
        public double enemyMaterial;
        public double enemySupply;
        public double playerPopulationUsed;
        public double playerPopulationCapacity;
        public double enemyPopulationUsed;
        public double enemyPopulationCapacity;
        public bool economyBuildingBuilt;
        public bool recruitmentBuildingBuilt;
        public bool enemyEconomyBuildingBuilt;
        public bool enemyRecruitmentBuildingBuilt;
        public bool siegeTechnologyResearched;
        public bool fortressCaptured;
        public bool playerCityLost;
        public bool aiDeployed;
        public bool aiCounterattackIssued;
        public double aiDecisionRemaining;
        public int aiDecisionCount;
        public int aiStalledDecisionCount;
        public string aiGoal;
        public string aiLayer;
        public string aiAction;
        public string aiLastError;
        public PrototypeEntitySaveData[] entities = Array.Empty<PrototypeEntitySaveData>();
        public PrototypeArmySaveData[] armies = Array.Empty<PrototypeArmySaveData>();
        public PrototypeBuildingQueueSaveData[] buildingQueue = Array.Empty<PrototypeBuildingQueueSaveData>();
        public PrototypeTechnologyQueueSaveData[] technologyQueue = Array.Empty<PrototypeTechnologyQueueSaveData>();
        public PrototypeRecruitmentQueueSaveData[] recruitmentQueue = Array.Empty<PrototypeRecruitmentQueueSaveData>();
        public PrototypeSiegeSaveData siege;
    }

    [Serializable]
    public sealed class PrototypeEntitySaveData
    {
        public ulong entityId;
        public string definitionId;
        public ulong factionId;
        public bool isHero;
        public double x;
        public double y;
        public double z;
        public double health;
        public string movementStatus;
        public PrototypeMovementOrderSaveData[] movementOrders = Array.Empty<PrototypeMovementOrderSaveData>();
        public ulong combatTargetId;
        public double attackCooldownRemaining;
        public string engagementMode;
        public string engagementTargetReason;
        public double engagementOriginX;
        public double engagementOriginY;
        public double engagementOriginZ;
    }

    [Serializable]
    public sealed class PrototypeMovementOrderSaveData
    {
        public double x;
        public double y;
        public double z;
        public int formationSlotIndex;
    }

    [Serializable]
    public sealed class PrototypeArmySaveData
    {
        public ulong armyId;
        public ulong factionId;
        public ulong commanderId;
        public ulong[] members = Array.Empty<ulong>();
        public string formation;
        public double morale;
        public double supply;
        public string orderType;
        public double orderX;
        public double orderY;
        public double orderZ;
        public ulong orderTargetId;
    }

    [Serializable]
    public sealed class PrototypeBuildingQueueSaveData
    {
        public ulong settlementId;
        public string buildingId;
        public double remainingSeconds;
    }

    [Serializable]
    public sealed class PrototypeTechnologyQueueSaveData
    {
        public ulong factionId;
        public string technologyId;
        public double remainingSeconds;
    }

    [Serializable]
    public sealed class PrototypeRecruitmentQueueSaveData
    {
        public ulong settlementId;
        public ulong factionId;
        public string unitId;
        public double remainingSeconds;
    }

    [Serializable]
    public sealed class PrototypeSiegeSaveData
    {
        public bool exists;
        public string state;
        public string area;
        public double gateHealth;
        public double strongholdHealth;
        public double gateRepairRemainingSeconds;
    }

    /// <summary>Serializes only prototype DTOs. No UnityEngine.Object reference enters the save payload.</summary>
    public sealed class PrototypeGameStateAdapter
    {
        public const string SlotKey = "AegisRTS.PlayablePrototype.01";
        private const string FrameworkVersion = "1.0.0";
        private const string ExtensionId = "playable-prototype-v3";
        private readonly ISaveStore _store = new PlayerPrefsSaveStore();

        public string CaptureJson(PrototypeSystemComposition composition)
        {
            if (composition == null) throw new ArgumentNullException(nameof(composition));
            PrototypeSaveData data = composition.CaptureState();
            GameStateSaveService service = CreateService(data.contentPackId);
            var coordinator = new GameStateCoordinator(service);
            SaveMetadata metadata = service.CreateMetadata(data.scenarioId,
                DateTimeOffset.UnixEpoch.AddMilliseconds(Math.Max(0d, data.elapsedSeconds) * 1000d));
            return coordinator.Save(new PrototypeCaptureSource(data), metadata);
        }

        public void SaveToSlot(PrototypeSystemComposition composition)
        {
            _store.Write(SlotKey, CaptureJson(composition));
        }

        public bool HasSlot => _store.TryRead(SlotKey, out string json) && !string.IsNullOrWhiteSpace(json);

        public string ReadSlot() => _store.TryRead(SlotKey, out string json) ? json : string.Empty;

        public PrototypeSaveData ParseAndValidate(string json, string contentPackId, string scenarioId)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new InvalidOperationException("Save slot is empty.");
            PrototypeSaveData data;
            try
            {
                var sink = new PrototypeRestoreSink();
                SaveEnvelope envelope = new GameStateCoordinator(CreateService(contentPackId)).Load(json, sink);
                if (!string.Equals(envelope.Metadata.ScenarioId, scenarioId, StringComparison.Ordinal))
                    throw new InvalidOperationException("Save content or scenario is incompatible.");
                data = sink.Data;
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception exception) { throw new InvalidOperationException("Save data is corrupted or incompatible.", exception); }
            if (data == null || data.formatVersion != 3) throw new InvalidOperationException("Save format is incompatible.");
            if (!string.Equals(data.contentPackId, contentPackId, StringComparison.Ordinal) ||
                !string.Equals(data.scenarioId, scenarioId, StringComparison.Ordinal))
                throw new InvalidOperationException("Save content or scenario is incompatible.");
            if (data.entities == null || data.armies == null || data.buildingQueue == null ||
                data.technologyQueue == null || data.recruitmentQueue == null)
                throw new InvalidOperationException("Save data is incomplete.");
            return data;
        }

        public string Fingerprint(PrototypeSystemComposition composition) => Fingerprint(CaptureJson(composition));

        public static string Fingerprint(string json)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(json ?? string.Empty));
                var result = new StringBuilder(bytes.Length * 2);
                foreach (byte value in bytes) result.Append(value.ToString("x2"));
                return result.ToString();
            }
        }

        private static GameStateSaveService CreateService(string contentPackId) =>
            new GameStateSaveService("3", FrameworkVersion,
                string.IsNullOrWhiteSpace(contentPackId) ? "missing-content" : contentPackId);

        private sealed class PrototypeCaptureSource : IGameStateCaptureSource
        {
            private readonly PrototypeSaveData _data;
            public PrototypeCaptureSource(PrototypeSaveData data) => _data = data ?? throw new ArgumentNullException(nameof(data));

            public GameStateDocument CaptureGameState()
            {
                return new GameStateDocument
                {
                    Clock = new ClockSaveState { TotalSeconds = _data.elapsedSeconds },
                    Random = new RandomSaveState
                    {
                        Seed = _data.randomSeed,
                        DrawCount = _data.randomDrawCount,
                        InternalState = _data.randomState,
                    },
                    Extensions = new[]
                    {
                        new ExtensionSaveState { Id = ExtensionId, Json = JsonUtility.ToJson(_data, false) },
                    },
                };
            }
        }

        private sealed class PrototypeRestoreSink : IGameStateRestoreSink
        {
            public PrototypeSaveData Data { get; private set; }

            public void RestoreGameState(GameStateDocument state)
            {
                if (state == null) throw new InvalidOperationException("Save state is missing.");
                ExtensionSaveState extension = state.Extensions?.FirstOrDefault(value =>
                    string.Equals(value?.Id, ExtensionId, StringComparison.Ordinal));
                if (extension == null || string.IsNullOrWhiteSpace(extension.Json))
                    throw new InvalidOperationException("Prototype save extension is missing.");
                try { Data = JsonUtility.FromJson<PrototypeSaveData>(extension.Json); }
                catch (Exception exception) { throw new InvalidOperationException("Prototype save extension is corrupted.", exception); }
            }
        }

        private sealed class PlayerPrefsSaveStore : ISaveStore
        {
            public void Write(string slotId, string json)
            {
                PlayerPrefs.SetString(Required(slotId), json ?? throw new ArgumentNullException(nameof(json)));
                PlayerPrefs.Save();
            }

            public bool TryRead(string slotId, out string json)
            {
                string key = Required(slotId);
                if (!PlayerPrefs.HasKey(key)) { json = null; return false; }
                json = PlayerPrefs.GetString(key);
                return true;
            }

            private static string Required(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Slot ID is required.", nameof(value));
                return value.Trim();
            }
        }
    }
}
