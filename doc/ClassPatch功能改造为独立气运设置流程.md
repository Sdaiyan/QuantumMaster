# Class Patch 独立气运设置改造流程

## 概述
本文档记录了将 QuantumMaster MOD 中的 Class Patch 功能改造为支持独立气运设置的完整流程。以 `BuildingManageHarvestSpecialSuccessRate` 功能为例，展示了从简单开关到可自定义气运级别的转换过程。

## 背景
- **原始状态**: `BuildingManageHarvestSpecialSuccessRate` 是一个简单的布尔开关，要么启用要么禁用
- **目标状态**: 支持独立的气运级别设置，允许用户为每个功能单独配置气运强度
- **核心思想**: 每个功能都能有自己的气运设置，而不是全局统一的气运级别
- **补丁类型**: Class Patch (使用 HarmonyPostfix)

## Class Patch 与 PatchBuilder 的关键差异

### Class Patch 特点：
- 使用传统 Harmony 注解 (`[HarmonyPatch]`, `[HarmonyPostfix]` 等)
- 直接在补丁方法中调用 LuckyCalculator 的对应方法，需要传入 FeatureKey
- 无需创建独立的补丁类或重构架构
- 保持原有的 Class 结构

### PatchBuilder 特点：
- 使用动态 IL 代码注入
- 需要创建独立补丁类和方法替换逻辑
- 需要统一 Apply 方法名
- 涉及更复杂的架构重构

## 操作步骤

### 第一步：修改配置文件 (Config.lua)
将原来的 Toggle 配置项改为 Dropdown 下拉选择：

```lua
-- 原来的配置
{
    SettingType = "Toggle",
    Key = "BuildingManageHarvestSpecialSuccessRate",
    DisplayName = "【气运】赌坊与青楼基础暴击率",
    Description = "【气运】赌坊与青楼根据气运增加或减少",
    DefaultValue = true,
}

-- 改为支持气运级别的配置
{
    SettingType = "Dropdown",
    Key = "BuildingManageHarvestSpecialSuccessRate",
    DisplayName = "【气运】赌坊与青楼基础暴击率",
    Description = "【气运】赌坊与青楼根据气运增加或减少",
    Options = luckLevelOptions,  -- 引用预定义的气运级别选项
    DefaultValue = 0,  -- 0 = "跟随全局"
}
```

### 第二步：更新配置管理器 (ConfigManager.cs)
将配置项从布尔类型改为整数类型：

```csharp
// 原来的配置
public static bool BuildingManageHarvestSpecialSuccessRate; // 【气运】赌坊与青楼基础暴击率

// 改为气运级别配置（默认值 0 = 跟随全局）
public static int BuildingManageHarvestSpecialSuccessRate = 0; // 【气运】赌坊与青楼基础暴击率
```

### 第三步：更新 Class Patch 中的气运调用
在 `BuildingManageHarvestSpecialSuccessRatePatch.cs` 中添加 featureKey 参数：

```csharp
[HarmonyPostfix]
public static void Postfix(ref int __result, GameData.Domains.Building.BuildingBlockKey blockKey, int charId)
{
    if (!ConfigManager.IsFeatureEnabled("BuildingManageHarvestSpecialSuccessRate"))
    {
        DebugLog.Info("【气运】赌坊与青楼基础暴击率: 使用原版逻辑");
        return; // 使用原版逻辑
    }

    var originalResult = __result;
    
    // 关键修改：添加 featureKey 参数支持独立气运
    bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(null, originalResult, "BuildingManageHarvestSpecialSuccessRate");
    int newResult = success ? 100 : 0;
    
    DebugLog.Info($"【气运】赌坊与青楼基础暴击率: 原始概率{originalResult}% -> 气运判定{(success ? "成功" : "失败")} -> {newResult}%");
    __result = newResult;
}
```

**核心修改**：
```csharp
// 原来：
bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(null, originalResult);

// 改为：
bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(null, originalResult, "BuildingManageHarvestSpecialSuccessRate");
```

### 第四步：更新 QuantumMaster.cs 配置映射
修改主文件中的补丁条件检查：

```csharp
// Class Patch 补丁配置映射表
private readonly Dictionary<string, (System.Type patchType, System.Func<bool> condition)> patchConfigMappings = new Dictionary<string, (System.Type, System.Func<bool>)>
{
    // 原来的配置
    { "BuildingManageHarvestSpecialSuccessRatePatch", (typeof(Features.Building.BuildingManageHarvestSpecialSuccessRatePatch), () => ConfigManager.BuildingManageHarvestSpecialSuccessRate) },
    
    // 改为使用 IsFeatureEnabled
    { "BuildingManageHarvestSpecialSuccessRatePatch", (typeof(Features.Building.BuildingManageHarvestSpecialSuccessRatePatch), () => ConfigManager.IsFeatureEnabled("BuildingManageHarvestSpecialSuccessRate")) },
};
```

## 关键技术要点

### 1. 独立气运架构
- **LuckyCalculator**: 核心计算逻辑，支持 featureKey 参数
- **ConfigManager.IsFeatureEnabled()**: 统一的功能启用检查
- **ConfigManager.GetFeatureLuckLevel()**: 获取功能专属气运级别

### 2. Class Patch 特有优势
- **简单直接**: 无需重构现有补丁架构
- **保持兼容**: 继续使用 Harmony 注解系统
- **最小改动**: 只需添加 featureKey 参数

### 3. 气运传递机制
```csharp
// LuckyCalculator 内部逻辑
var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
```

### 4. 条件检查统一
```csharp
// 统一使用 IsFeatureEnabled 方法
if (!ConfigManager.IsFeatureEnabled("BuildingManageHarvestSpecialSuccessRate"))
{
    return; // 使用原版逻辑
}
```

## 验证清单

完成改造后，请检查以下项目：

1. **配置文件**: Config.lua 中的配置项类型从 Toggle 改为 Dropdown
2. **配置管理**: ConfigManager.cs 中的属性类型从 bool 改为 int
3. **独立气运**: LuckyCalculator 调用中添加了 featureKey 参数
4. **条件检查**: 使用 ConfigManager.IsFeatureEnabled() 进行功能启用检查
5. **主配置映射**: QuantumMaster.cs 中的条件检查使用 IsFeatureEnabled
6. **编译测试**: 项目能够成功编译
7. **功能测试**: 游戏中功能按预期工作

## 与 PatchBuilder 流程的对比

| 项目 | Class Patch | PatchBuilder |
|------|-------------|--------------|
| 复杂度 | 简单 | 复杂 |
| 架构改动 | 最小 | 重构 |
| 新建文件 | 无需 | 需要独立补丁类 |
| 方法统一 | 无需 Apply | 需要 Apply 方法 |
| 核心修改 | 添加 featureKey | 创建替换方法 |
| 适用场景 | Harmony 补丁 | 动态 IL 注入 |

## 扩展应用

此操作流程可以应用于任何需要独立气运设置的 Class Patch 功能：

1. 按照上述步骤修改配置
2. 在 LuckyCalculator 调用中添加 featureKey 参数
3. 更新条件检查为 IsFeatureEnabled
4. 更新主配置映射

这样就能实现每个功能都有独立的气运控制，提供更精细的用户体验。

## 注意事项

1. **⚠️ 保持原始逻辑**: 绝对不可以篡改原本的补丁逻辑和方法签名
2. **featureKey 一致性**: 确保在所有地方使用相同的 featureKey 字符串
3. **默认值**: 新配置项的默认值应该是 0（跟随全局气运）
4. **向后兼容**: 确保现有存档和配置能正常工作
5. **错误处理**: 添加适当的错误处理和日志记录
6. **性能考虑**: Class Patch 的性能影响通常小于 PatchBuilder
