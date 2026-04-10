# QuantumMaster 项目指南

## 项目概述

太吾绘卷 HarmonyLib MOD，双模块架构编译为两个独立 DLL。

## 双模块架构

| 项目 | 用途 | 术语 | 标签 | 配置文件 | 配置管理器 |
|------|------|------|------|----------|-----------|
| **QuantumMaster** | 非战斗功能（资源、建筑、角色、学习、技能等） | 气运 | 【气运】/【Luck】 | `Config.lua` | `ConfigManager` |
| **CombatMaster** | 战斗相关功能（功法、招式等） | 武学境界 | 【武者】/【Martial Arts】 | `Config.combat.lua` | `CombatConfigManager` |

**核心要点**：两者底层运算机制完全相同，都使用 `LuckyCalculator` 进行概率计算，仅名称和描述不同。

## 气运/武学境界 等级对照

| 等级 | QuantumMaster（气运） | CombatMaster（武学境界） | 效果 |
|------|----------------------|------------------------|------|
| 0 | 跟随全局 | 跟随全局 | 使用全局设置 |
| 1 | 命途多舛 | 方寸大乱 | 极度不利 |
| 2 | 时运不济 | 患得患失 | 比较不利 |
| 3 | 顺风顺水 | 气定神闲 | **关闭功能**（默认体验） |
| 4 | 左右逢源 | 游刃有余 | 相对有利 |
| 5 | 心想事成 | 随心所欲 | 非常有利 |
| 6 | 福星高照 | 如有神助 | 极度有利 |
| 7 | 洪福齐天 | 天人合一 | 几乎必然有利 |
| 8 | 气运之子 | 万法归一 | 达到理论上限 |

## 配置选项变量

- QuantumMaster：`Options = luckLevelOptions`
- CombatMaster：`Options = combatMartialLevelOptions`

## 补丁模式

项目中有 3 种补丁模式：

1. **PatchBuilder**（纯 transpiler）— 替换方法内部调用，无需角色上下文。注册到 `patchBuilderMappings`
2. **ClassPatch**（纯 Harmony 属性）— `[HarmonyPrefix]`/`[HarmonyPostfix]` 直接修改返回值。注册到 `patchConfigMappings`
3. **混合模式**（ClassPatch + PatchBuilder）— Prefix/Postfix 注入角色上下文 + PatchBuilder 替换内部调用。**需在 `patchConfigMappings` 和 `patchBuilderMappings` 两处注册**
   - QM 使用 `ActionPatchBase`（存储 `_currentCharacter` + `_targetCharacter`）
   - CM 使用 `CombatPatchBase`（存储 `_currentCharacterId`）

## Shared 文件夹

`src/Shared/` 包含两个项目共享的代码：
- `LuckyCalculator.cs` — 气运计算逻辑（含 `FeatureDisplayNames` 映射表）
- `LuckyRandomHelper.cs` — 随机数辅助方法
- `PatchBuilder.cs` — 补丁构建器
- `PatchPresets.cs` — 预设的方法信息
- `PatchDefinitions.cs` — 数据结构定义
- `TranspilerHelper.cs` — Transpiler 辅助工具
- `DebugLog.cs` — 日志输出
- `IConfigProvider.cs` — 配置提供者接口

## 默认约束

- **默认只对太吾生效**：除非明确指出，所有补丁默认只对太吾角色生效
- **等级 3 关闭功能**：`IsFeatureEnabled()` 检查 luck level ≠ 2（内部索引，UI 上是第 3 档）
- **使用原始值作为范围**：数值修改时用 `__result` 作为范围起点，不用固定值

## 构建命令

```powershell
dotnet build QuantumMaster.sln    # 编译两个项目
dotnet build QuantumMaster.csproj  # 仅编译 QuantumMaster
dotnet build CombatMaster.csproj   # 仅编译 CombatMaster
```

## README 文件对应

| 项目 | 中文 README | 英文 README |
|------|------------|------------|
| QuantumMaster | `README.md` | `README_en.md` |
| CombatMaster | `README.combat.md` | `README_en.combat.md` |
