---
name: add-quantummaster-hybrid-patch
description: '在 QuantumMaster 添加混合模式补丁（ClassPatch + PatchBuilder）。适用于：需要注入角色上下文并替换方法内部调用的场景（偷窃、抢夺、欺骗、下毒、暗害等行动类功能）。使用 ActionPatchBase 存储 _currentCharacter 和 _targetCharacter。需要在 patchConfigMappings 和 patchBuilderMappings 两处注册。触发词：QuantumMaster、混合模式、hybrid、ActionPatchBase、偷抢骗、上下文注入、双注册。'
---

# 在 QuantumMaster 添加混合模式补丁

## 适用场景

需要**注入角色上下文** + **transpiler 替换方法内部调用**的场景。典型用例：
- 偷窃（`StealPatch`）
- 抢夺（`RobPatch`）
- 欺骗（`ScamPatch`）
- 下毒（`PoisonPatch`）
- 暗害（`PlotHarmPatch`）
- 偷学武功/生活技艺（`StealCombatSkillPatch` / `StealLifeSkillPatch`）

## 为什么需要混合模式？

这类方法接收 `currentChar` 和 `targetChar` 参数，需要区分：
- **太吾发起行动** → 使用倾向成功的气运加成
- **目标是太吾** → 使用倾向失败的气运加成（保护太吾）
- **其他情况** → 使用原始概率

但 PatchBuilder transpiler 只能替换方法调用签名，无法传递额外参数。因此：
1. **Prefix** 将角色信息存入 `ActionPatchBase` 的静态字段
2. **PatchBuilder** 替换内部调用为自定义方法
3. 自定义方法从静态字段读取角色信息做条件判断
4. **Postfix** 清理静态字段

## 参考实现

- `src/QuantumMaster/Features/Actions/StealPatch.cs`
- `src/QuantumMaster/Features/Actions/ActionPatchBase.cs`

ActionPatchBase API 详见 [action-patch-base.md](./references/action-patch-base.md)。

## 完整流程（7 步）

### 第 1 步：修改配置文件 `Config.lua`

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

```csharp
public static int featureKey = 0;
```

在 `LoadAllConfigs` 中添加：
```csharp
DomainManager.Mod.GetSetting(modIdStr, "featureKey", ref featureKey);
```

### 第 3 步：更新功能显示名称映射 `src/Shared/LuckyCalculator.cs`

```csharp
{ "featureKey", "功能中文名称" },
```

### 第 4 步：创建补丁类文件

在 `src/QuantumMaster/Features/Actions/` 下创建 `<FeatureName>Patch.cs`。

代码模板见 [code-template.md](./references/code-template.md)。

补丁类包含 **3 个部分**：
1. **Prefix/Postfix 方法**（带 `[HarmonyPatch]` 属性）— 设置/清理角色上下文
2. **替换方法** — 从静态上下文读取角色信息做条件判断
3. **PatchBuilder Apply 方法** — 使用 transpiler 替换内部调用

### 第 5 步：注册到主类 `src/QuantumMaster/QuantumMaster.cs`

**⚠️ 关键：需要在两个字典中都注册！**

在 `patchConfigMappings` 中注册（用于 Prefix/Postfix）：
```csharp
{ "featureKey", (typeof(Features.Actions.<FeatureName>Patch), () => ConfigManager.IsFeatureEnabled("featureKey")) },
```

在 `patchBuilderMappings` 中注册（用于 PatchBuilder transpiler）：
```csharp
{ "featureKey", (Features.Actions.<FeatureName>Patch.Patch<MethodName>, () => ConfigManager.IsFeatureEnabled("featureKey")) },
```

### 第 6 步：更新 README 文档

在 `README.md` 中添加：
```markdown
【气运】功能名称: 功能描述
```

在 `README_en.md` 中添加：
```markdown
【Luck】Feature Name: Feature description in English
```

### 第 7 步：编译验证

```powershell
dotnet build QuantumMaster.csproj
```
