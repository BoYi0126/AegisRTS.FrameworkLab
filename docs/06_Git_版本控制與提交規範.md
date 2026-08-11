# Git 規範

Repository Root 就是 Unity Project Root。

提交：

```text
Assets/
Packages/
ProjectSettings/
docs/
DevelopmentProgress.md
.gitignore
README.md
```

Unity `.meta` 與資產一起提交。

任何會修改 repository 的開發工作都必須同步更新根目錄 `DevelopmentProgress.md`，並與對應變更放在同一個 commit 或 PR。詳細格式見 `docs/09_DevelopmentProgress_開發進度紀錄規範.md`。

不提交：

```text
Library/
Temp/
Logs/
obj/
Build/
Builds/
.vs/
```

Commit 例：

```text
feat(core): add command bus
feat(siege): add destructible gate
test(combat): add damage tests
fix(ai): prevent attack deadlock
refactor(core): split runtime state and view
```
