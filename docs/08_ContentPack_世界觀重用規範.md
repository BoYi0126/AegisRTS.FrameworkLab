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
