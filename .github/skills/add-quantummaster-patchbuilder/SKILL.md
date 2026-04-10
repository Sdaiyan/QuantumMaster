---
name: add-quantummaster-patchbuilder
description: '在 QuantumMaster 添加 PatchBuilder 功能。适用于：纯 transpiler 替换方法内部调用，无需角色上下文的场景（资源采集、建筑、技能学习、读书等非战斗功能）。触发词：QuantumMaster、PatchBuilder、气运、添加功能、transpiler、替换方法调用。'
---

# 在 QuantumMaster 添加 PatchBuilder 功能

## 适用场景

纯 transpiler 替换，无需角色上下文的场景。例如：
- 资源采集（蛐蛐、资源点）
- 建筑管理（经营、建筑效果）
- 读书学习（读书策略、灵光一闪）
- 技能学习（偷学、指点）
- 奇遇相关

**如果需要区分太吾/目标角色（如偷抢骗等行动类），请使用 `add-quantummaster-hybrid-patch` skill。**

## 参考实现

参考 `src/QuantumMaster/Features/Resources/CollectResourcePatch.cs` 或同目录下其他 PatchBuilder 文件。

## 完整流程（6 步）

### 第 1 步：修改配置文件 `Config.lua`

在 `DefaultSettings` 数组中添加：

```lua
{
    SettingType = "Dropdown",
    Key = "featureKey",
    DisplayName = "【气运】功能显示名称",
    Description = "功能描述",
    Options = luckLevelOptions,
    DefaultValue = 0,
},
```

### 第 2 步：更新配置管理器 `src/QuantumMaster/ConfigManager.cs`

1. 添加静态字段：
```csharp
public static int featureKey = 0;
```

2. 在 `LoadAllConfigs` 方法中添加：
```csharp
DomainManager.Mod.GetSetting(modIdStr, "featureKey", ref featureKey);
DebugLog.Info($"配置加载: featureKey = {featureKey}");
```

### 第 3 步：更新功能显示名称映射 `src/Shared/LuckyCalculator.cs`

在 `FeatureDisplayNames` 字典中添加：
```csharp
{ "featureKey", "功能中文名称" },
```

### 第 4 步：创建补丁类文件

在 `src/QuantumMaster/Features/<Category>/` 下创建 `<FeatureName>Patch.cs`。

代码模板见 [code-template.md](./references/code-template.md)。
替换方法类型参考见 [replacement-types.md](./references/replacement-types.md)。

关键要点：
- 使用 `ConfigManager.IsFeatureEnabled("featureKey")` 检查功能开关
- 使用 `GenericTranspiler.CreatePatchBuilder()` 创建构建器
- 使用 `patchBuilder.AddExtensionMethodReplacement()` 或 `AddInstanceMethodReplacement()` 添加替换规则
- 每个功能创建独立的替换方法（含 `featureKey` 参数）

### 第 5 步：注册到主类 `src/QuantumMaster/QuantumMaster.cs`

在 `patchBuilderMappings` 字典中添加：
```csharp
{ "featureKey", (Features.<Category>.<FeatureName>Patch.Apply, () => ConfigManager.IsFeatureEnabled("featureKey")) },
```

**注意**：纯 PatchBuilder 只需注册到 `patchBuilderMappings`，不需要注册到 `patchConfigMappings`。

### 第 6 步：更新 README 文档

在 `README.md` 中添加：
```markdown
【气运】功能名称: 功能描述
```

在 `README_en.md` 中添加：
```markdown
【Luck】Feature Name: Feature description in English
```

### 编译验证

```powershell
dotnet build QuantumMaster.csproj
```
