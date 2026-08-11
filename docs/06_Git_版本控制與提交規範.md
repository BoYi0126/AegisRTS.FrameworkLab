# Git 規範

Repository Root 就是 Unity Project Root。

提交：

```text
Assets/
Packages/
ProjectSettings/
docs/
.gitignore
README.md
```

Unity `.meta` 與資產一起提交。

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
