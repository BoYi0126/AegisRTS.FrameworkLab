using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AegisRTS.Core.Entities;
using AegisRTS.Demo.PlayablePrototype;
using AegisRTS.Gameplay.AI;
using AegisRTS.Gameplay.Armies;
using AegisRTS.Gameplay.Combat;
using AegisRTS.Gameplay.Content;
using AegisRTS.Gameplay.Content.Definitions;
using AegisRTS.Gameplay.Content.Serialization;
using AegisRTS.Gameplay.Content.Validation;
using AegisRTS.Gameplay.Economy;
using AegisRTS.Gameplay.Objectives;
using AegisRTS.Gameplay.Siege;
using AegisRTS.Gameplay.VerticalSlice;
using AegisRTS.Presentation.Camera;
using AegisRTS.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using EntityId = AegisRTS.Core.Entities.EntityId;

namespace AegisRTS.Tests.PlayMode
{
    public sealed class PlayablePrototypePlayModeTests
    {
        [Test]
        public void PP00_ContentAndComposition_BootWithAllRequiredSystems()
        {
            ContentPack pack = new ContentPackJsonLoader().Load(Read("ContentPack.json"));
            ContentValidationResult validation = new ContentPackValidator().Validate(pack,
                new ContentAssetCatalog(new[] { "PF_Unit_Infantry", "PF_Unit_Placeholder", "PF_Hero_Placeholder", "PF_Structure_Placeholder", "PF_Settlement_Placeholder" }));
            Assert.That(validation.IsValid, Is.True, string.Join("\n", validation.Issues));
            Assert.That(pack.Rules.SettlementArchetypeId, Is.EqualTo("fortified-city"));
            Assert.That(pack.Rules.DestructibleWalls, Is.False);
            Assert.That(pack.Rules.GateRepairEnabled, Is.True);
            Assert.That(pack.Rules.StrongholdRecruitmentEnabled, Is.True);
            Assert.That(pack.Rules.CaptureStrongholdInsteadOfDestroy, Is.True);
            Assert.That(pack.DefenseStructures.Any(value => value.Id.Value == "structure.stronghold-core"), Is.True);
            Assert.That(pack.Units.Single(value => value.Id.Value == "unit.infantry").PrefabId,
                Is.EqualTo(PrototypeUnitArtCatalog.InfantryPrefabId));
            using (PrototypeSystemComposition value = Create())
            {
                Assert.That(value.ContentPackId, Is.EqualTo("prototype.neutral"));
                Assert.That(value.ScenarioId, Is.EqualTo("scenario.prototype-conquest"));
                Assert.That(value.Registry.Count, Is.EqualTo(8));
                Assert.That(value.Registry.Snapshot().Where(record => record.DefinitionId == "unit.infantry")
                    .All(record => record.PrefabId == PrototypeUnitArtCatalog.InfantryPrefabId), Is.True);
                Assert.That(value.Factions.FactionCount, Is.EqualTo(2));
                Assert.That(value.Settlements.SettlementCount, Is.EqualTo(3));
                Assert.That(value.AI.AgentCount, Is.EqualTo(1));
                Assert.That(value.Commands.RegisteredHandlerCount, Is.GreaterThanOrEqualTo(12));
                Assert.That(value.Events.SubscriberCount, Is.GreaterThanOrEqualTo(5));
            }
        }

        [Test]
        public void PP01_ManualMoveAttackDamageDeathAndCleanup_UsesOneEntityIdentity()
        {
            using (PrototypeSystemComposition value = Create())
            {
                EntityId target = value.Registry.GetFactionEntities(PrototypeSystemComposition.EnemyFactionId)
                    .First(id => id.Value >= 2002UL);
                CombatantSnapshot before;
                Assert.That(value.Combat.TryGetState(target, out before), Is.True);
                EntityId[] actors = value.Registry.GetFactionEntities(PrototypeSystemComposition.PlayerFactionId).ToArray();
                EntityId actor = actors[0];
                Assert.That(value.Move(new[] { actor }, new AegisRTS.Gameplay.Units.WorldPoint(-6d, 0d, 0d)).WasHandled, Is.True);
                Assert.That(value.Move(new[] { actor }, new AegisRTS.Gameplay.Units.WorldPoint(-3d, 0d, 0d), true).WasHandled, Is.True);
                Assert.That(value.Movement.SnapshotOrders(actor).Count, Is.EqualTo(2));
                Assert.That(value.Commands.Dispatch(new AegisRTS.Gameplay.Units.StopUnitsCommand(new[] { actor })).WasHandled, Is.True);
                Assert.That(value.Movement.SnapshotOrders(actor), Is.Empty);
                Assert.That(value.Commands.Dispatch(new AegisRTS.Gameplay.Units.HoldUnitsCommand(new[] { actor })).WasHandled, Is.True);
                Assert.That(value.Movement.TryGetState(actor, out var holding), Is.True);
                Assert.That(holding.Status, Is.EqualTo(AegisRTS.Gameplay.Movement.MovementStatus.Holding));
                Assert.That(value.Commands.Dispatch(new AegisRTS.Gameplay.Units.MoveUnitsCommand(actors,
                    new AegisRTS.Gameplay.Units.WorldPoint(-2d, 0d, 0d), formation: AegisRTS.Gameplay.Formation.FormationType.Line)).WasHandled, Is.True);
                Assert.That(actors.Select(id =>
                {
                    value.Movement.TryGetState(id, out var state);
                    return state.FormationSlotIndex;
                }).Distinct().Count(), Is.EqualTo(actors.Length));
                Assert.That(value.Move(actors, before.Position).WasHandled, Is.True);
                Tick(value, 4d);
                int projectiles = 0;
                using (value.Events.Subscribe<ProjectileLaunchedEvent>(_ => projectiles++))
                {
                    Assert.That(value.Attack(actors, target).WasHandled, Is.True);
                    TickUntil(value, () => !value.Registry.TryGet(target, out _), 20d);
                }
                Assert.That(projectiles, Is.GreaterThan(0), "The ranged unit must launch an observable projectile event.");
                Assert.That(value.Registry.TryGet(target, out _), Is.False, "Dead entity must leave the prototype registry.");
                Assert.That(value.Movement.TryGetState(target, out _), Is.False, "Dead entity must leave movement.");
                Assert.That(value.Combat.TryGetState(target, out _), Is.False, "Dead entity must leave combat target state.");
                Assert.That(value.Armies.TryGetArmyForUnit(target, out _), Is.False, "Dead entity must leave army membership.");
            }
        }

        [Test]
        public void PP02_EconomyBuildResearchRecruit_ValidatesPrerequisitesAndSpawnsAtomically()
        {
            using (PrototypeSystemComposition value = Create())
            {
                Assert.That(value.Recruit(new DefinitionId("unit.siege")).WasHandled, Is.False);
                int directRecruitBefore = value.Registry.Count;
                Assert.That(value.Recruit(new DefinitionId("unit.infantry")).WasHandled, Is.True,
                    "Fortified-city strongholds recruit ordinary units without a barracks.");
                value.Tick(1d);
                Assert.That(value.Registry.Count, Is.EqualTo(directRecruitBefore + 1));
                Assert.That(value.Construct(new DefinitionId("building.economy")).WasHandled, Is.True);
                Assert.That(value.Research(new DefinitionId("technology.siege")).WasHandled, Is.True);
                value.Tick(1d);
                int before = value.Registry.Count;
                Assert.That(value.Recruit(new DefinitionId("unit.siege")).WasHandled, Is.True);
                value.Tick(1d);
                Assert.That(value.Registry.Count, Is.EqualTo(before + 1));
                EntityId siege = value.FindPlayerSiegeUnit();
                Assert.That(siege.IsValid, Is.True);
                Assert.That(value.Movement.TryGetState(siege, out _), Is.True);
                Assert.That(value.Combat.TryGetState(siege, out _), Is.True);
                Assert.That(value.TryGetPlayerEconomy(out EconomyAccountSnapshot economy), Is.True);
                Assert.That(economy.PopulationUsed, Is.EqualTo(3d));
            }
            var navigation = new FailNextNavigationRuntime();
            using (var failed = new PrototypeSystemComposition(Read("ContentPack.json"), Read("Scenario.json"), navigation))
            {
                Assert.That(failed.TryGetPlayerEconomy(out EconomyAccountSnapshot before), Is.True);
                int entitiesBefore = failed.Registry.Count;
                ulong randomDrawsBefore = failed.RandomSource.DrawCount;
                Assert.That(failed.Recruit(new DefinitionId("unit.infantry")).WasHandled, Is.True);
                navigation.FailNextRegister = true;
                Assert.Throws<InvalidOperationException>(() => failed.Tick(1d));
                Assert.That(failed.TryGetPlayerEconomy(out EconomyAccountSnapshot after), Is.True);
                foreach (var resource in before.Resources)
                {
                    before.Production.TryGetValue(resource.Key, out double rate);
                    Assert.That(after.Resources[resource.Key], Is.EqualTo(resource.Value + rate).Within(0.000001d),
                        $"A failed spawn must refund {resource.Key}; only the one-second production tick may remain.");
                }
                Assert.That(after.PopulationUsed, Is.EqualTo(before.PopulationUsed),
                    "A failed spawn must release reserved population.");
                Assert.That(failed.Registry.Count, Is.EqualTo(entitiesBefore));
                Assert.That(failed.Recruitment.QueuedCount, Is.Zero);
                Assert.That(failed.RandomSource.DrawCount, Is.EqualTo(randomDrawsBefore));
                Assert.That(navigation.AgentCount, Is.EqualTo(entitiesBefore));
            }
        }

        [Test]
        public void PP03_HeroArmyCommands_KeepHeroCombatAndMembershipAligned()
        {
            using (PrototypeSystemComposition value = Create())
            {
                Assert.That(value.CreatePlayerArmy().WasHandled, Is.True);
                Assert.That(value.Armies.TryGetState(PrototypeSystemComposition.PlayerArmyId, out ArmySnapshot army), Is.True);
                Assert.That(army.CommanderId, Is.EqualTo(PrototypeSystemComposition.PlayerHeroId));
                Assert.That(army.UnitCount, Is.EqualTo(4));
                Assert.That(value.Heroes.TryGetState(PrototypeSystemComposition.PlayerHeroId, out var hero), Is.True);
                Assert.That(hero.ArmyId, Is.EqualTo(PrototypeSystemComposition.PlayerArmyId));
                Assert.That(value.Combat.TryGetState(PrototypeSystemComposition.PlayerHeroId, out CombatantSnapshot combat), Is.True);
                Assert.That(combat.ArmyId, Is.EqualTo(PrototypeSystemComposition.PlayerArmyId));
                Assert.That(value.MovePlayerArmy(new AegisRTS.Gameplay.Units.WorldPoint(2d, 0d, 2d)).WasHandled, Is.True);
                Assert.That(value.SplitSelectedFromPlayerArmy(new[] { PrototypeSystemComposition.PlayerLieutenantId }).WasHandled, Is.True);
                Assert.That(value.Armies.TryGetState(new EntityId(1303), out ArmySnapshot detachment), Is.True);
                Assert.That(detachment.CommanderId, Is.EqualTo(PrototypeSystemComposition.PlayerLieutenantId));
                Assert.That(value.MergePlayerDetachment().WasHandled, Is.True);
                Assert.That(value.AssignPlayerLieutenantCommander().WasHandled, Is.True);
                Assert.That(value.Armies.TryGetState(PrototypeSystemComposition.PlayerArmyId, out army), Is.True);
                Assert.That(army.CommanderId, Is.EqualTo(PrototypeSystemComposition.PlayerLieutenantId));
                int beforeSpawn = value.Registry.Count;
                ulong previousMax = value.Registry.Snapshot().Max(record => record.EntityId.Value);
                value.SpawnUnit(PrototypeSystemComposition.PlayerCityId, PrototypeSystemComposition.PlayerFactionId,
                    new DefinitionId("unit.infantry"));
                EntityId reinforcement = value.Registry.Snapshot().Single(record => record.EntityId.Value > previousMax).EntityId;
                Assert.That(value.Registry.Count, Is.EqualTo(beforeSpawn + 1));
                Assert.That(value.AddSelectedToPlayerArmy(new[] { reinforcement }).WasHandled, Is.True);
                Assert.That(value.Armies.TryGetArmyForUnit(reinforcement, out EntityId joinedArmy), Is.True);
                Assert.That(joinedArmy, Is.EqualTo(PrototypeSystemComposition.PlayerArmyId));
                Assert.That(value.DefendPlayerArmy(new AegisRTS.Gameplay.Units.WorldPoint(-4d, 0d, 0d)).WasHandled, Is.True);
                Assert.That(value.Armies.TryGetState(PrototypeSystemComposition.PlayerArmyId, out army), Is.True);
                Assert.That(army.Order.Type, Is.EqualTo(ArmyOrderType.Defend));
                Assert.That(value.RetreatPlayerArmy(new AegisRTS.Gameplay.Units.WorldPoint(-12d, 0d, 0d)).WasHandled, Is.True);
                Assert.That(value.Armies.TryGetState(PrototypeSystemComposition.PlayerArmyId, out army), Is.True);
                Assert.That(army.Order.Type, Is.EqualTo(ArmyOrderType.Retreat));
            }
        }

        [Test]
        public void PP04_AI_ObservesRealStateAndIssuesSharedArmyCommandsOnCadence()
        {
            using (PrototypeSystemComposition value = Create())
            {
                int initialEnemyCount = value.Registry.GetFactionEntities(PrototypeSystemComposition.EnemyFactionId).Count;
                TickUntil(value, () => value.AiCounterattackIssued, 105d);
                Assert.That(value.AiCounterattackIssued, Is.True);
                Assert.That(value.Registry.GetFactionEntities(PrototypeSystemComposition.EnemyFactionId).Count,
                    Is.GreaterThan(initialEnemyCount), "AI must recruit through the shared recruitment command pipeline.");
                Assert.That(value.Armies.TryGetState(PrototypeSystemComposition.EnemyArmyId, out ArmySnapshot enemyArmy), Is.True);
                Assert.That(enemyArmy.UnitCount, Is.GreaterThan(initialEnemyCount),
                    "The recruited AI unit must join the real army through shared create/merge commands.");
                Assert.That(value.Notifications.Any(message => message.Contains("Recruit complete")), Is.True);
                Assert.That(value.AI.TryGetState(PrototypeSystemComposition.EnemyFactionId, out AiAgentSnapshot ai), Is.True);
                Assert.That(ai.DecisionCount, Is.GreaterThanOrEqualTo(2));
                Assert.That(new[] { AiActionType.StartSiege, AiActionType.Breach, AiActionType.ProtectSiege }, Does.Contain(ai.Action));
                Assert.That(value.Commands.RegisteredHandlerCount, Is.GreaterThan(0));
            }
        }

        [Test]
        public void PP05_SiegeBreachNavigationCaptureVictoryAndDefeat_AreAuthoritative()
        {
            using (PrototypeSystemComposition value = Create())
            {
                PrepareSiege(value);
                Assert.That(value.StartPlayerSiege().WasHandled, Is.True);
                Assert.That(value.EnterFortress().WasHandled, Is.False, "Closed gates must be rejected by the shared command validator.");
                Assert.That(value.Sieges.TryGetState(PrototypeSystemComposition.PlayerSiegeId, out SiegeSnapshot before), Is.True);
                Assert.That(before.CurrentArea, Is.EqualTo(SiegeArea.OuterArea));
                Assert.That(value.BreachGate().WasHandled, Is.True);
                Assert.That(value.BreachGate().WasHandled, Is.True);
                Assert.That(value.GateNavigationRefreshed, Is.True);
                Assert.That(value.TryGetSiegeStructure(PrototypeSystemComposition.FortressStrongholdId, out DefenseStructureSnapshot coreBefore), Is.True);
                Assert.That(coreBefore.IsDestroyed, Is.False, "The main stronghold remains intact until attackers reach it.");
                Assert.That(value.EnterFortress().WasHandled, Is.True);
                Assert.That(value.CaptureFortress().WasHandled, Is.True);
                Assert.That(value.Scenario.Status, Is.EqualTo(ScenarioStatus.Victory));
                Assert.That(value.Settlements.TryGetState(PrototypeSystemComposition.EnemyFortressId, out var fortress), Is.True);
                Assert.That(fortress.OwnerId, Is.EqualTo(PrototypeSystemComposition.PlayerFactionId));
                Assert.That(value.TryGetSiegeStructure(PrototypeSystemComposition.FortressStrongholdId, out DefenseStructureSnapshot coreAfter), Is.True);
                Assert.That(coreAfter.IsDestroyed, Is.True, "Zero core HP represents subdued defenses; the settlement is captured rather than removed.");
            }
            using (PrototypeSystemComposition defeat = Create())
            {
                TickUntil(defeat, () => defeat.IsDefeat, 140d);
                Assert.That(defeat.Scenario.Status, Is.EqualTo(ScenarioStatus.Defeat),
                    "The AI counterattack must be able to kill the player commander and trigger a normal defeat path.");
            }
        }

        [UnityTest]
        public IEnumerator PP06_SceneBootsHUDInputViewsPauseRestartAndNoMissingReferences()
        {
            SceneManager.LoadScene("PlayablePrototype_01", LoadSceneMode.Single);
            yield return null;
            yield return null;
            PlayablePrototypeBootstrap bootstrap = UnityEngine.Object.FindAnyObjectByType<PlayablePrototypeBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.BootSucceeded, Is.True, bootstrap.LastUiMessage);
            Assert.That(bootstrap.TutorialVisible, Is.True, "A new player must see the onboarding guide before simulation starts.");
            bootstrap.DismissTutorialNow();
            Assert.That(bootstrap.NavigationReady, Is.True);
            Assert.That(bootstrap.UsesUnityNavMesh, Is.True, "The playable scene must inject the Unity NavMesh product adapter.");
            Assert.That(bootstrap.Composition, Is.Not.Null);
            Assert.That(bootstrap.ViewCount, Is.EqualTo(bootstrap.Composition.Registry.Count));
            PrototypeUnitArtView[] infantryArt = UnityEngine.Object.FindObjectsByType<PrototypeUnitArtView>(FindObjectsInactive.Exclude);
            Assert.That(infantryArt, Has.Length.EqualTo(2), "Player and enemy infantry must use the imported art prefab.");
            Assert.That(infantryArt.All(value => value.TeamColorRenderers.Length >= 2), Is.True);
            Assert.That(infantryArt.All(value => value.HealthBarAnchor != null && value.SelectionAnchor != null), Is.True);
            Assert.That(bootstrap.HudQuery, Is.Not.Null);
            Assert.That(bootstrap.HudCommandSink, Is.Not.Null);
            Assert.That(bootstrap.Selection.RegisteredCount, Is.EqualTo(bootstrap.ViewCount + 4),
                "Player city, neutral village, fortress gate, and enemy stronghold must be real selectable views.");
            bootstrap.Selection.Select(PrototypeSystemComposition.PlayerCityId);
            yield return null;
            Assert.That(bootstrap.ActiveCommandTab, Is.EqualTo(PrototypeCommandTab.Domestic));
            bootstrap.Selection.Select(PrototypeSystemComposition.EnemyFortressId);
            yield return null;
            Assert.That(bootstrap.ActiveCommandTab, Is.EqualTo(PrototypeCommandTab.Siege));
            bootstrap.Selection.Select(PrototypeSystemComposition.PlayerHeroId);
            yield return null;
            Assert.That(bootstrap.ActiveCommandTab, Is.EqualTo(PrototypeCommandTab.UnitSettings));
            Assert.That(bootstrap.Selection.IsSelected(PrototypeSystemComposition.PlayerHeroId), Is.True);
            Assert.That(bootstrap.HudCommandSink.Dispatch(new HudCommand("engagement.aggressive")).Succeeded, Is.True);
            Assert.That(bootstrap.Selection.IsSelected(PrototypeSystemComposition.PlayerHeroId), Is.True,
                "Clicking a HUD stance command must not clear world selection.");
            Assert.That(bootstrap.Composition.Combat.TryGetState(
                PrototypeSystemComposition.PlayerHeroId, out CombatantSnapshot stancedHero), Is.True);
            Assert.That(stancedHero.EngagementMode, Is.EqualTo(UnitEngagementMode.Aggressive));
            bootstrap.Selection.SelectMany(new[]
            {
                PrototypeSystemComposition.PlayerCityId,
                PrototypeSystemComposition.PlayerHeroId,
            });
            yield return null;
            Assert.That(bootstrap.ActiveCommandTab, Is.EqualTo(PrototypeCommandTab.UnitSettings),
                "Mixed building and unit box selection must prioritize the unit settings panel.");
            HudSnapshot hud = bootstrap.HudQuery.Query();
            Assert.That(hud.TryGetPanel(HudPanelId.ResourceBar, out _), Is.True);
            Assert.That(hud.TryGetPanel(HudPanelId.SelectionPanel, out _), Is.True);
            Assert.That(hud.TryGetPanel(HudPanelId.ArmyPanel, out _), Is.True);
            Assert.That(hud.TryGetPanel(HudPanelId.SettlementPanel, out _), Is.True);
            Assert.That(hud.TryGetPanel(HudPanelId.AbilityBar, out _), Is.True);
            Assert.That(hud.TryGetPanel(HudPanelId.Objective, out _), Is.True);
            string themeBefore = bootstrap.ActiveThemeName;
            bootstrap.ToggleThemeNow();
            Assert.That(bootstrap.ActiveThemeName, Is.Not.EqualTo(themeBefore));
            Assert.That(bootstrap.Session.Pause(), Is.True);
            double elapsed = bootstrap.Composition.ElapsedSeconds;
            yield return null;
            Assert.That(bootstrap.Composition.ElapsedSeconds, Is.EqualTo(elapsed));
            Assert.That(bootstrap.Session.Resume(), Is.True);
            PrepareSiege(bootstrap.Composition);
            Assert.That(bootstrap.Composition.StartPlayerSiege().WasHandled, Is.True);
            Assert.That(bootstrap.Composition.BreachGate().WasHandled, Is.True);
            Assert.That(bootstrap.Composition.BreachGate().WasHandled, Is.True);
            EntityId selectedVictim = bootstrap.Composition.FindFirstEnemyTarget();
            bootstrap.Selection.Select(selectedVictim);
            Assert.That(bootstrap.Selection.IsSelected(selectedVictim), Is.True);
            Assert.That(bootstrap.Composition.Attack(
                bootstrap.Composition.Registry.GetFactionEntities(PrototypeSystemComposition.PlayerFactionId), selectedVictim).WasHandled, Is.True);
            float previousScale = Time.timeScale;
            Time.timeScale = 10f;
            yield return new WaitForSeconds(25f);
            Time.timeScale = previousScale;
            Assert.That(bootstrap.Composition.Registry.TryGet(selectedVictim, out _), Is.False);
            Assert.That(bootstrap.Selection.IsSelected(selectedVictim), Is.False,
                "Death cleanup must remove the entity from selection before its view is destroyed.");
            bootstrap.Composition.TriggerDefeat();
            yield return null;
            Assert.That(bootstrap.Session.State, Is.EqualTo(GameSessionState.Defeat));
            Assert.That(bootstrap.RestartNow(), Is.True);
            Assert.That(bootstrap.Session.State, Is.EqualTo(GameSessionState.Playing),
                "Restarting from an outcome overlay must clear the terminal session state.");
            Assert.That(bootstrap.ViewCount, Is.EqualTo(8));
            bootstrap.DismissTutorialNow();
            Assert.That(bootstrap.SaveNow(), Is.True);
            Assert.That(bootstrap.LoadNow(), Is.True);
            yield return null;
            yield return null;
            bootstrap = UnityEngine.Object.FindAnyObjectByType<PlayablePrototypeBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.NavigationReady, Is.True);
            Assert.That(bootstrap.ViewCount, Is.EqualTo(bootstrap.Composition.Registry.Count),
                "Loading through the visible HUD path must rebuild every world view.");
            GameObject rebuiltWorld = GameObject.Find("PlayablePrototype_World");
            Assert.That(rebuiltWorld, Is.Not.Null);
            Assert.That(rebuiltWorld.activeInHierarchy, Is.True,
                "The replacement world must remain active after deferred cleanup destroys the old session.");
            GameObject rebuiltGround = GameObject.Find("Ground");
            Assert.That(rebuiltGround, Is.Not.Null);
            Renderer groundRenderer = rebuiltGround.GetComponent<Renderer>();
            Assert.That(groundRenderer, Is.Not.Null);
            Assert.That(groundRenderer.enabled, Is.True);
            Assert.That(GeometryUtility.TestPlanesAABB(
                    GeometryUtility.CalculateFrustumPlanes(Camera.main), groundRenderer.bounds), Is.True,
                $"Loaded world must remain inside the camera frustum. Camera={Camera.main.transform.position}, " +
                $"rotation={Camera.main.transform.rotation.eulerAngles}, ground={groundRenderer.bounds}");
            RtsCameraController rebuiltCamera = Camera.main.GetComponent<RtsCameraController>();
            Assert.That(rebuiltCamera.Model.PivotX, Is.EqualTo(0d).Within(0.001d));
            Assert.That(rebuiltCamera.Model.PivotZ, Is.EqualTo(0d).Within(0.001d),
                "A load must reset stale edge-scroll movement before accepting new pointer input.");
        }

        [UnityTest]
        public IEnumerator PP05_UnityNavMeshGate_BlocksFortressUntilBreachThenProvidesCompletePath()
        {
            SceneManager.LoadScene("PlayablePrototype_01", LoadSceneMode.Single);
            yield return null;
            yield return null;
            PlayablePrototypeBootstrap bootstrap = UnityEngine.Object.FindAnyObjectByType<PlayablePrototypeBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            bootstrap.DismissTutorialNow();
            Assert.That(bootstrap.NavigationReady, Is.True);
            var interior = new AegisRTS.Gameplay.Units.WorldPoint(16d, 0d, 3d);
            var closed = bootstrap.Composition.Navigation.SetDestination(
                PrototypeSystemComposition.PlayerHeroId, interior, 0);
            Assert.That(closed.Accepted, Is.False, "The closed gate must make the enclosed courtyard unreachable.");

            PrepareSiege(bootstrap.Composition);
            Assert.That(bootstrap.Composition.StartPlayerSiege().WasHandled, Is.True);
            Assert.That(bootstrap.Composition.BreachGate().WasHandled, Is.True);
            Assert.That(bootstrap.Composition.BreachGate().WasHandled, Is.True);
            yield return null;
            Assert.That(bootstrap.Composition.GateNavigationRefreshed, Is.True);
            var open = bootstrap.Composition.Navigation.SetDestination(
                PrototypeSystemComposition.PlayerHeroId, interior, 0);
            Assert.That(open.Accepted, Is.True, open.Error);
            bootstrap.Composition.Navigation.Stop(PrototypeSystemComposition.PlayerHeroId);
            Assert.That(bootstrap.Composition.Move(new[] { PrototypeSystemComposition.PlayerHeroId }, interior).WasHandled, Is.True);

            float previousTimeScale = Time.timeScale;
            Time.timeScale = 10f;
            yield return new WaitForSeconds(8f);
            Time.timeScale = previousTimeScale;
            Assert.That(bootstrap.Composition.Movement.TryGetState(PrototypeSystemComposition.PlayerHeroId, out var arrived), Is.True);
            Assert.That(arrived.Position.X, Is.GreaterThan(13d), "The unit must enter the courtyard through the breached gate.");
        }

        [Test]
        public void PP07_EngagementModeCommand_AcquiresPursuesLeashesAndReturnsThroughSharedSystems()
        {
            using (PrototypeSystemComposition value = Create())
            {
                EntityId actor = PrototypeSystemComposition.PlayerHeroId;
                EntityId enemy = value.FindFirstEnemyTarget();
                Assert.That(value.Combat.TryGetState(actor, out CombatantSnapshot initial), Is.True);
                Assert.That(value.Combat.TryGetProfile(actor, out CombatantProfile profile), Is.True);
                var pursuitPoint = new AegisRTS.Gameplay.Units.WorldPoint(
                    initial.Position.X + profile.Attack.Range * 1.25d,
                    initial.Position.Y,
                    initial.Position.Z);
                Assert.That(value.Navigation.SetPosition(enemy, pursuitPoint), Is.True);
                Assert.That(value.SetEngagementMode(new[] { actor }, UnitEngagementMode.Aggressive).WasHandled, Is.True);

                value.Tick(0.01d);

                Assert.That(value.Combat.TryGetState(actor, out CombatantSnapshot pursuing), Is.True);
                Assert.That(pursuing.TargetId, Is.EqualTo(enemy));
                Assert.That(pursuing.TargetReason, Is.EqualTo(EngagementTargetReason.Proactive));
                Assert.That(value.Movement.TryGetState(actor, out var movement), Is.True);
                Assert.That(movement.Status, Is.EqualTo(AegisRTS.Gameplay.Movement.MovementStatus.Moving));
                value.Tick(0.2d);

                var outsideLeash = new AegisRTS.Gameplay.Units.WorldPoint(
                    initial.Position.X + profile.Attack.Range * 1.6d,
                    initial.Position.Y,
                    initial.Position.Z);
                Assert.That(value.Navigation.SetPosition(enemy, outsideLeash), Is.True);
                value.Tick(0.01d);

                Assert.That(value.Combat.TryGetState(actor, out CombatantSnapshot returning), Is.True);
                Assert.That(returning.TargetId.IsValid, Is.False);
                Assert.That(returning.ShouldReturnToOrigin, Is.True);
                Assert.That(value.Movement.TryGetState(actor, out movement), Is.True);
                Assert.That(movement.Destination, Is.EqualTo(initial.Position));
            }
        }

        [Test]
        public void PP07_SaveLoad_RoundTripsWholePrototypeFingerprintAndRejectsBadData()
        {
            var adapter = new PrototypeGameStateAdapter();
            string queuedJson;
            using (PrototypeSystemComposition queued = Create())
            {
                Assert.That(queued.Construct(new DefinitionId("building.economy")).WasHandled, Is.True);
                Assert.That(queued.Research(new DefinitionId("technology.siege")).WasHandled, Is.True);
                queued.Tick(0.2d);
                queuedJson = adapter.CaptureJson(queued);
                Assert.That(queued.Buildings.QueuedCount, Is.EqualTo(1),
                    "The fortified-city AI can recruit from its stronghold and no longer queues a barracks.");
                Assert.That(queued.Technologies.QueuedCount, Is.EqualTo(1));
            }
            PrototypeSaveData queuedData = adapter.ParseAndValidate(queuedJson, "prototype.neutral", "scenario.prototype-conquest");
            using (PrototypeSystemComposition queuedRestored = Create())
            {
                queuedRestored.RestoreState(queuedData);
                Assert.That(adapter.CaptureJson(queuedRestored), Is.EqualTo(queuedJson),
                    "Already-paid construction and technology jobs must round-trip without duplicate costs.");
            }

            string json;
            string fingerprint;
            using (PrototypeSystemComposition source = Create())
            {
                PrepareSiege(source);
                Assert.That(source.Recruit(new DefinitionId("unit.infantry")).WasHandled, Is.True);
                EntityId target = source.FindFirstEnemyTarget();
                Assert.That(source.AttackWithPlayerArmy(target).WasHandled, Is.True);
                Assert.That(source.SetEngagementMode(
                    new[] { PrototypeSystemComposition.PlayerHeroId }, UnitEngagementMode.Aggressive).WasHandled, Is.True);
                Assert.That(source.Move(new[] { PrototypeSystemComposition.PlayerHeroId },
                    new AegisRTS.Gameplay.Units.WorldPoint(4d, 0d, 3d), true).WasHandled, Is.True);
                source.StartPlayerSiege();
                source.BreachGate();
                json = adapter.CaptureJson(source);
                fingerprint = PrototypeGameStateAdapter.Fingerprint(json);
                Assert.That(source.Recruitment.QueuedCount, Is.EqualTo(1));
                Assert.That(source.RandomSource.DrawCount, Is.GreaterThan(0UL));
            }
            PrototypeSaveData data = adapter.ParseAndValidate(json, "prototype.neutral", "scenario.prototype-conquest");
            using (PrototypeSystemComposition restored = Create())
            {
                restored.RestoreState(data);
                string restoredJson = adapter.CaptureJson(restored);
                Assert.That(restoredJson, Is.EqualTo(json), $"Source={json}\nRestored={restoredJson}");
                Assert.That(PrototypeGameStateAdapter.Fingerprint(restoredJson), Is.EqualTo(fingerprint));
                Assert.That(restored.FindPlayerSiegeUnit().IsValid, Is.True);
                Assert.That(restored.Armies.TryGetState(PrototypeSystemComposition.PlayerArmyId, out _), Is.True);
                Assert.That(restored.Recruitment.QueuedCount, Is.EqualTo(1));
                Assert.That(restored.Movement.SnapshotOrders(PrototypeSystemComposition.PlayerHeroId).Count, Is.GreaterThanOrEqualTo(2));
                Assert.That(restored.Combat.TryGetState(PrototypeSystemComposition.PlayerHeroId, out CombatantSnapshot restoredHero), Is.True);
                Assert.That(restoredHero.TargetId.IsValid, Is.True);
                Assert.That(restoredHero.EngagementMode, Is.EqualTo(UnitEngagementMode.Aggressive));
            }
            using (PrototypeSystemComposition continuationSource = Create())
            {
                continuationSource.Tick(3d);
                string continuationJson = adapter.CaptureJson(continuationSource);
                PrototypeSaveData continuationData = adapter.ParseAndValidate(
                    continuationJson, "prototype.neutral", "scenario.prototype-conquest");
                using (PrototypeSystemComposition continuationRestored = Create())
                {
                    continuationRestored.RestoreState(continuationData);
                    continuationSource.Tick(0.25d);
                    continuationRestored.Tick(0.25d);
                    Assert.That(adapter.CaptureJson(continuationRestored),
                        Is.EqualTo(adapter.CaptureJson(continuationSource)),
                        "A loaded AI economy must continue deterministically instead of rebuilding completed production.");
                }
            }
            Assert.Throws<InvalidOperationException>(() => adapter.ParseAndValidate("{broken", "prototype.neutral", "scenario.prototype-conquest"));
            Assert.Throws<InvalidOperationException>(() => adapter.ParseAndValidate(json, "another.pack", "scenario.prototype-conquest"));
        }

        [Test]
        public void PP08_EndToEndAndDeterministicLongRun_ReachVictoryWithoutException()
        {
            using (PrototypeSystemComposition value = Create())
            {
                PrepareSiege(value);
                EntityId enemy = value.Registry.GetFactionEntities(PrototypeSystemComposition.EnemyFactionId)
                    .First(id => id.Value >= 2002UL);
                value.Attack(value.Registry.GetFactionEntities(PrototypeSystemComposition.PlayerFactionId), enemy);
                TickUntil(value, () => !value.Registry.TryGet(enemy, out _), 20d);
                value.StartPlayerSiege();
                value.BreachGate();
                value.BreachGate();
                value.EnterFortress();
                value.CaptureFortress();
                Assert.That(value.IsVictory, Is.True);
            }
            using (PrototypeSystemComposition soak = Create())
            {
                for (int index = 0; index < 1800; index++) soak.Tick(1d);
                Assert.That(soak.ElapsedSeconds, Is.EqualTo(1800d));
                Assert.That(soak.AI.TryGetState(PrototypeSystemComposition.EnemyFactionId, out AiAgentSnapshot ai), Is.True);
                Assert.That(ai.DecisionCount, Is.GreaterThan(100));
            }
        }

        [Test]
        public void PP08_ThreeHundredActiveUnits_PerformanceSmokeCompletesWithinBudget()
        {
            using (PrototypeSystemComposition value = Create())
            {
                for (int index = value.Registry.Count; index < 300; index++)
                    value.SpawnUnit(PrototypeSystemComposition.PlayerCityId, PrototypeSystemComposition.PlayerFactionId,
                        new DefinitionId(index % 2 == 0 ? "unit.infantry" : "unit.archer"));
                var watch = Stopwatch.StartNew();
                for (int index = 0; index < 120; index++) value.Tick(0.05d);
                watch.Stop();
                Assert.That(value.Registry.Count, Is.GreaterThanOrEqualTo(290));
                Assert.That(watch.Elapsed.TotalSeconds, Is.LessThan(5d), "300-unit deterministic simulation smoke exceeded the CI budget.");
            }
        }

        private static void PrepareSiege(PrototypeSystemComposition value)
        {
            Assert.That(value.Research(new DefinitionId("technology.siege")).WasHandled, Is.True);
            value.Tick(1d);
            Assert.That(value.Recruit(new DefinitionId("unit.siege")).WasHandled, Is.True);
            value.Tick(1d);
            Assert.That(value.CreatePlayerArmy().WasHandled, Is.True);
        }

        private static PrototypeSystemComposition Create() => new PrototypeSystemComposition(Read("ContentPack.json"), Read("Scenario.json"));
        private static string Read(string file) => File.ReadAllText(Path.Combine("Assets", "AegisRTS", "Content", "PrototypeNeutral", file));

        private static void Tick(PrototypeSystemComposition value, double seconds)
        {
            int steps = (int)Math.Ceiling(seconds / 0.05d);
            for (int index = 0; index < steps; index++) value.Tick(0.05d);
        }

        private static void TickUntil(PrototypeSystemComposition value, Func<bool> condition, double timeout)
        {
            int steps = (int)Math.Ceiling(timeout / 0.05d);
            for (int index = 0; index < steps && !condition(); index++) value.Tick(0.05d);
        }

        private sealed class FailNextNavigationRuntime : IPrototypeNavigationRuntime
        {
            private readonly PrototypeNavigationAdapter _inner = new PrototypeNavigationAdapter();
            public bool FailNextRegister { get; set; }
            public int AgentCount => _inner.AgentCount;
            public bool UsesUnityNavMesh => false;
            public void Register(EntityId entityId, AegisRTS.Gameplay.Units.WorldPoint position, double speed)
            {
                if (FailNextRegister) { FailNextRegister = false; throw new InvalidOperationException("Injected navigation registration failure."); }
                _inner.Register(entityId, position, speed);
            }
            public bool Unregister(EntityId entityId) => _inner.Unregister(entityId);
            public bool SetPosition(EntityId entityId, AegisRTS.Gameplay.Units.WorldPoint position) => _inner.SetPosition(entityId, position);
            public AegisRTS.Gameplay.Movement.NavigationDestinationResult SetDestination(EntityId entityId,
                AegisRTS.Gameplay.Units.WorldPoint destination, int formationSlotIndex) =>
                _inner.SetDestination(entityId, destination, formationSlotIndex);
            public void Stop(EntityId entityId) => _inner.Stop(entityId);
            public bool TryGetSnapshot(EntityId entityId, out AegisRTS.Gameplay.Movement.NavigationAgentSnapshot snapshot) =>
                _inner.TryGetSnapshot(entityId, out snapshot);
            public void Tick(double deltaSeconds) => _inner.Tick(deltaSeconds);
            public string GetDebugSummary() => _inner.GetDebugSummary();
        }
    }
}
