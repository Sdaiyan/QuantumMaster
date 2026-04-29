---
name: patch-baseline-workflow
description: '在 QuantumMaster / CombatMaster 维护 patch 基线、版本对比与更新流程。适用于：按 ConfigManager/CombatConfigManager 配置项生成 patch registry、保存目标函数完整代码快照、保留版本化游戏源码、比较两个版本基线、识别 InternalConsumption 内部消费配置项、生成更新审查模板、在补丁适配后回写新基线。触发词：patch baseline、补丁基线、版本对比、更新流程、函数快照、游戏源码快照、registry、compare、update review、internal consumption。'
---

# Patch Baseline Workflow

## 适用场景

当你需要处理以下任一任务时，使用这个 skill：

1. 为 QuantumMaster / CombatMaster 建立配置项级别的 patch 基线。
2. 为当前版本保存目标函数的完整代码快照。
3. 为当前版本保留完整反编译游戏源码。
4. 对比两个游戏版本下的 patch 基线。
5. 生成更新审查模板，判断哪些 patch 需要改。
6. 在 patch 适配完成后回写新版本基线。

## 输入前提

默认约定：

1. 仓库根目录是 `d:\code\QuantumMaster`。
2. 脚本入口是 `tools/patch-baseline/PatchBaseline.ps1`。
3. 如果没有显式传入 `-GameSourceRoot`，脚本会优先读取 `Directory.Build.props` 中的 `<PatchBaselineGameSourceRoot>`。
4. 如果配置文件里也没有该属性，才回退到 `D:\code\Data`。

如果用户给了新的反编译源码目录或输出目录，优先使用用户提供的值。

## 工作流

### A. 收集基线

1. 以 `src/QuantumMaster/ConfigManager.cs` 和 `src/CombatMaster/CombatConfigManager.cs` 的单个配置 Key 作为 unit。
2. 优先从 `src/QuantumMaster/QuantumMaster.cs` 和 `src/CombatMaster/CombatMaster.cs` 找到直接注册到每个 key 的 patch。
3. 如果某个 key 没有直接注册，但在已注册 patch class 内部被消费，则把它归类为 `InternalConsumption`，而不是 `Unmapped`。
4. 下钻到 patch 文件，提取 patch 类型、目标函数、replacement、occurrence、上下文注入方式，以及 `ConfigBinding`（`DirectRegistration` / `InternalConsumption`）。
5. 到反编译游戏源码中定位目标函数，保存完整函数代码正文。
6. 如果用户要求保留版本源码，执行 `Collect` 时启用 `-CopyGameSource`。

### B. 对比版本

1. 读取两个版本的 `registry.json`。
2. 比较 patch 定义是否变化。
3. 比较目标函数是否存在、签名是否变化、代码 hash 是否变化、occurrence 画像是否变化。
4. 输出 `comparison.json`、`comparison.md`、`update-review-template.json`。

### C. 更新处置

1. 优先查看 `comparison.md` 摘要。
2. 再按 `update-review-template.json` 中的推荐动作分流：
   1. `patch-redesign-needed`
   2. `patch-change-needed`
   3. `doc-only`
   4. `no-update-needed`
3. 如果需要改 patch，完成代码修改后，重新对目标版本跑一次 `Collect`，刷新基线。

### D. 回写基线

patch 更新完成后，要确保以下产物都被重新生成并保留下来：

1. 新版本完整反编译游戏源码
2. 新版本函数代码快照
3. 新版本 registry
4. 更新审查模板中的最终结论

## 常用命令模板

收集基线：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\patch-baseline\PatchBaseline.ps1 `
  -Action Collect `
  -Version <version-tag> `
  -CopyGameSource
```

小范围试跑：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\patch-baseline\PatchBaseline.ps1 `
  -Action Collect `
  -Version pilot-<date> `
  -IncludeConfigKey collectResource,steal,QianNianZui,XinWuDingYi
```

版本对比：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\patch-baseline\PatchBaseline.ps1 `
  -Action Compare `
  -BaseVersion <old-version> `
  -TargetVersion <new-version>
```

## 产物要求

收集时至少生成：

1. `manifest.json`
2. `registry.json`
3. `registry.csv`
4. `inventory.md`
5. `function-snapshots/`
6. `game-source/`，如果用户要求保留完整版本源码
7. `registry.json` / `inventory.md` 中的每个 patch entry 应保留 `ConfigBinding`，至少区分 `DirectRegistration` 和 `InternalConsumption`

对比时至少生成：

1. `comparison.json`
2. `comparison.md`
3. `update-review-template.json`

## 异常处理

1. 如果函数代码快照定位失败，不要静默跳过，必须在 registry / inventory 中留下 warning。
2. 如果 patch 文件注册关系与实际内容不一致，不要自动修正语义，先保留 warning。
3. 如果配置项没有直接注册，但能确认是在某个已注册 patch class 内部消费，不要继续标记为 `Unmapped`，应归类为 `InternalConsumption`。
4. 如果用户只给了新版本源码，没有旧版本基线，先提示需要补采旧版本或选择一个现有版本作为 base。
5. 如果 `-CopyGameSource` 体积过大，优先提醒输出目录空间风险，但不要擅自关闭完整源码保留。

## 验收清单

1. 配置项级别的条目数是否符合预期。
2. 是否真的保存了目标函数完整代码正文，而不是只存位置。
3. 是否在需要时保留了完整反编译游戏源码。
4. 仅在真正找不到 patch 关联时才标记为 `Unmapped`，内部消费配置项应显示为 `InternalConsumption`。
5. `comparison.md` 在 `同版本对比` 时是否能给出全量 unchanged。
6. `update-review-template.json` 是否适合人工继续填写最终更新决定。

## 参考

详细工作流见：

`doc/补丁基线收集、对比与更新工作流.md`