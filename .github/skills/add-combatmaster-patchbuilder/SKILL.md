---
name: add-combatmaster-patchbuilder
description: '在 CombatMaster 添加 PatchBuilder 功能（混合模式，含 CombatPatchBase 上下文注入）。适用于：战斗功法的概率修改，替换功法方法内部的随机函数调用。使用 CombatPatchBase 存储 _currentCharacterId。需要在 patchConfigMappings 和 patchBuilderMappings 两处注册。触发词：CombatMaster、PatchBuilder、武学境界、武者、战斗功法、transpiler、CombatPatchBase。'
---

# 在 CombatMaster 添加 PatchBuilder 功能

## 适用场景

战斗功法的概率修改，替换功法方法内部的随机函数调用。例如：
- 疯狗拳获得增伤概率（`FengGouQuanPatch`）
- 界青快剑重复触发概率（`JieQingKuaiJianPatch`）
- 百草雀啄指额外效果概率（`BaiCaoQueZhuoZhiPatch`）

**注意**：CombatMaster 的 PatchBuilder 天然是混合模式 — 通过 `CombatPatchBase` 注入角色上下文（CharacterId），用于判断是否太吾。

**如果是直接 Postfix 修改 `GetModifyValue` 返回值的场景，请使用 `add-combatmaster-classpatch` skill。**

## 参考实现

- `src/CombatMaster/Features/Combat/FengGouQuanPatch.cs`
- `src/CombatMaster/Features/Combat/CombatPatchBase.cs`

## 完整流程（6 步）

### 第 1 步：修改配置文件 `Config.combat.lua`

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

### 第 3 步：更新功能显示名称映射 `src/Shared/LuckyCalculator.cs`

```csharp
{ "FeatureKey", "功能中文名称" },
```

### 第 4 步：创建补丁类文件

在 `src/CombatMaster/Features/Combat/` 下创建 `<FeatureName>Patch.cs`。

代码模板见 [code-template.md](./references/code-template.md)。
替换方法类型参考见 [replacement-types.md](./references/replacement-types.md)。

补丁类包含 **3 个部分**：
1. **Apply 方法** — PatchBuilder transpiler 替换内部调用
2. **Prefix/Postfix** — 通过 `__instance.CharacterId` 设置/清理上下文到 `CombatPatchBase`
3. **替换方法** — 委托给 `CombatPatchBase.CheckPercentProbWithStaticContext`

关键要点：
- 使用 `CombatConfigManager.IsFeatureEnabled("FeatureKey")` 检查功能开关
- CharacterId 通过 `__instance.CharacterId` 获取
- `[HarmonyPatch]` 的目标类型是功法的完整类型路径

### 第 5 步：注册到主类 `src/CombatMaster/CombatMaster.cs`

**⚠️ 需要在两个字典中都注册！**

在 `patchConfigMappings` 中注册（Prefix/Postfix）：
```csharp
{ "FeatureKey", (typeof(Features.Combat.<FeatureName>Patch), () => CombatConfigManager.IsFeatureEnabled("FeatureKey")) },
```

在 `patchBuilderMappings` 中注册（PatchBuilder transpiler）：
```csharp
{ "FeatureKey", (Features.Combat.<FeatureName>Patch.Apply, () => CombatConfigManager.IsFeatureEnabled("FeatureKey")) },
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
