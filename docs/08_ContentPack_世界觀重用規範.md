# Content Pack 世界觀重用規範

Framework 不理解「劉備、騎士、獸人、魔法、黃金」；它理解 Hero、Unit、Faction、Resource、Ability、Tag。

三國：Hero=武將、Settlement=城池、SiegeUnit=衝車/投石車。

中世紀：Hero=Lord、Settlement=Castle、SiegeUnit=Trebuchet。

奇幻：Hero=Archmage、Settlement=Arcane Fortress、SiegeUnit=Golem。

特殊差異優先用 Tags / Ability / Modifier / Rule：

```text
Cavalry
Flying
Undead
Mechanical
Siege
Hero
Structure
Magic
```

禁止 `if (unit.Name == "Dragon")` 這種內容硬編碼。

## Pack 資料與載入

每個 pack 的入口固定為：

```text
Assets/AegisRTS/Content/<PackName>/ContentPack.json
```

JSON 只存 Definition、通用 Tag、Definition ID reference、Prefab ID 與 `GameRuleSet`。世界觀 display name 可以不同，但 Framework source 不得依 pack ID、display name 或特定角色名稱分支。

載入順序：

```text
JSON → ContentPackJsonLoader → immutable ContentPack
     → ContentPackValidator → ContentCatalog → Active Pack
```

驗證失敗的 pack 不得替換目前的 active catalog。三個基準 pack 為 `DemoNeutral`、`DemoThreeKingdoms`、`DemoFantasy`，必須能以同一套 loader、validator 與 query API 載入。
