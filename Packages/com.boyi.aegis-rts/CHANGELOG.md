# Changelog

## [Unreleased]

- Added four unit engagement modes, deterministic proactive target acquisition, retaliation targeting, pursuit leashes, return-to-origin state, and a stateless Combat/Movement coordinator.
- Added selection revisions and descriptor-driven command contexts for automatic Domestic, Unit Settings, and Siege panel routing; world commands now filter out selectable structures and settlements.

- Added the packaged Framework API operation map and public contract tests.
- Added immutable snapshots and no-double-charge restore APIs for active building, technology, and recruitment queues.
- Added movement order, combat target/cooldown, army order/morale/supply, AI cadence/decision, deterministic production, and army-member lifecycle restoration support.
- Recruitment spawn failures now atomically refund resources and population before reporting the failure.
- Added world-neutral fortified-settlement rule metadata to `GameRuleSet`.
- Added defender-only `RepairDefenseStructureCommand`, repair events, and gate breach sealing when a destroyed repairable gate is restored.

## [1.0.0] - 2026-08-11

- Initial SemVer release.
- Includes data-driven content, RTS input and camera adapters, movement, combat, hero armies, factions and territory, economy, siege, AI, scenarios, HUD, persistence, replay, diagnostics, and performance utilities.
- Includes Basic RTS, Basic Combat, and Basic Siege samples.
