---
name: add-quantummaster-classpatch
description: '在 QuantumMaster 添加 ClassPatch（纯 Harmony 属性补丁）。适用于：直接通过 Prefix/Postfix 修改方法返回值或参数的场景（性别控制、灵光一闪、门派好感度等）。触发词：QuantumMaster、ClassPatch、HarmonyPatch、气运、Prefix、Postfix、返回值修改。'
---

# 在 QuantumMaster 添加 ClassPatch 功能

## 适用场景

通过 `[HarmonyPrefix]` / `[HarmonyPostfix]` 直接修改方法的返回值或参数。例如：
- 直接修改概率/数值返回值（灵光一闪 `ReadingInspirationPatch`）
- 控制随机生成结果（性别控制 `GenderControlPatch`）
- 修改好感度计算（门派好感 `SectApprovalPatch`）

**如果需要替换方法内部的随机函数调用，请使用 `add-quantummaster-patchbuilder` skill。**
**如果需要注入角色上下文 + transpiler 替换，请使用 `add-quantummaster-hybrid-patch` skill。**

## 参考实现

- `src/QuantumMaster/Features/Reading/ReadingInspirationPatch.cs`（Postfix 示例）
- `src/QuantumMaster/Features/Character/GenderControlPatch.cs`（Prefix 示例）

## 完整流程（6 步）

### 第 1 步：修改配置文件 `Config.lua`

**Dropdown 类型**（气运等级）：
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

**Toggle 类型**（开关功能）：
```lua
{
    SettingType = "Toggle",
    Key = "featureKey",
    DisplayName = "功能显示名称",
    Description = "功能描述",
    DefaultValue = true,
},
```

### 第 2 步：更新配置管理器 `src/QuantumMaster/ConfigManager.cs`

1. 添加静态字段：
```csharp
public static int featureKey = 0;       // Dropdown
// 或
public static bool featureKey = true;   // Toggle
```

2. 在 `LoadAllConfigs` 方法中添加：
```csharp
DomainManager.Mod.GetSetting(modIdStr, "featureKey", ref featureKey);
DebugLog.Info($"配置加载: featureKey = {featureKey}");
```

### 第 3 步：更新功能显示名称映射 `src/Shared/LuckyCalculator.cs`

如果使用了 `LuckyCalculator` 的计算方法，在 `FeatureDisplayNames` 中添加：
```csharp
{ "featureKey", "功能中文名称" },
```

### 第 4 步：创建补丁类文件

在 `src/QuantumMaster/Features/<Category>/` 下创建 `<FeatureName>Patch.cs`。

代码模板见 [code-template.md](./references/code-template.md)。
常见错误见 [common-pitfalls.md](./references/common-pitfalls.md)。

关键要点：
- 类上添加 `[HarmonyPatch(typeof(TargetClass), "MethodName")]`
- **默认只对太吾生效**：检查角色 ID 是否为太吾
- **使用原始值作为范围**：用 `__result` 作为起点，不用固定值
- 配置类型需与 Lua 一致（Dropdown → int，Toggle → bool）

### 第 5 步：注册到主类 `src/QuantumMaster/QuantumMaster.cs`

在 `patchConfigMappings` 字典中添加：
```csharp
{ "featureKey", (typeof(Features.<Category>.<FeatureName>Patch), () => ConfigManager.IsFeatureEnabled("featureKey")) },
```

**注意**：纯 ClassPatch 只需注册到 `patchConfigMappings`，不需要注册到 `patchBuilderMappings`。

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
