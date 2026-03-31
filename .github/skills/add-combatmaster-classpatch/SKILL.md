---
name: add-combatmaster-classpatch
description: '在 CombatMaster 添加 ClassPatch（纯 Harmony 属性补丁）。适用于：直接通过 Postfix 修改 GetModifyValue 返回值的战斗功法补丁。触发词：CombatMaster、ClassPatch、HarmonyPatch、武学境界、武者、Postfix、Prefix、GetModifyValue、返回值修改。'
---

# 在 CombatMaster 添加 ClassPatch 功能

## 适用场景

通过 `[HarmonyPostfix]` 直接修改 `GetModifyValue` 返回值。例如：
- 越女剑法追击几率提升（`YueNvJianFaPatch`）
- 千年醉增益增加（`QianNianZuiPatch`）

**如果需要替换方法内部的随机函数调用（如 `CheckPercentProb`），请使用 `add-combatmaster-patchbuilder` skill。**

## 参考实现

- `src/CombatMaster/Features/Combat/YueNvJianFaPatch.cs`

## 完整流程（6 步）

### 第 1 步：修改配置文件 `Config.combat.lua`

**Dropdown 类型**（武学境界等级）：
```lua
{
    SettingType = "Dropdown",
    Key = "FeatureKey",
    DisplayName = "【武者】功能显示名称",
    Description = "功能描述",
    Options = combatMartialLevelOptions,
    DefaultValue = 0,
},
```

**Toggle 类型**（开关功能）：
```lua
{
    SettingType = "Toggle",
    Key = "FeatureKey",
    DisplayName = "功能显示名称",
    Description = "功能描述",
    DefaultValue = true,
},
```

### 第 2 步：更新配置管理器 `src/CombatMaster/CombatConfigManager.cs`

1. 添加静态字段：
```csharp
public static int FeatureKey = 0;
```

2. 在 `LoadAllConfigs` 中添加：
```csharp
DomainManager.Mod.GetSetting(modIdStr, "FeatureKey", ref FeatureKey);
DebugLog.Info($"[CombatMaster] FeatureKey: {FeatureKey}");
```

3. 在 `GetConfigValue` 中添加：
```csharp
case "FeatureKey": return FeatureKey;
```

4. 在 `FeatureMap` 中添加：
```csharp
{ "FeatureKey", "FeatureKey" },
```

### 第 3 步：创建补丁类文件

在 `src/CombatMaster/Features/Combat/` 下创建 `<FeatureName>Patch.cs`。

代码模板见 [code-template.md](./references/code-template.md)。
常见错误见 [common-pitfalls.md](./references/common-pitfalls.md)。

关键要点：
- `[HarmonyPatch]` 目标是功法的完整类型路径
- **默认只对太吾生效**：通过 `__instance.CharacterId` 判断
- **使用原始值作为范围**：用 `__result` 作为起点
- 配置类型需与 Lua 一致（Dropdown → int，Toggle → bool）

### 第 4 步：注册到主类 `src/CombatMaster/CombatMaster.cs`

在 `patchConfigMappings` 字典中添加：
```csharp
{ "FeatureKey", (typeof(Features.Combat.<FeatureName>Patch), () => CombatConfigManager.IsFeatureEnabled("FeatureKey")) },
```

**注意**：纯 ClassPatch 只需注册到 `patchConfigMappings`，不需要注册到 `patchBuilderMappings`。

### 第 5 步：更新功能显示名称映射 `src/Shared/LuckyCalculator.cs`

在 `FeatureDisplayNames` 中添加：
```csharp
{ "FeatureKey", "功能中文名称" },
```

### 第 6 步：更新 README 文档

在 `README.combat.md` 中添加：
```markdown
【武者】功能名称: 功能描述
```

在 `README_en.combat.md` 中添加：
```markdown
【Martial Arts】Feature Name: Feature description in English
```

### 编译验证

```powershell
dotnet build CombatMaster.csproj
```
