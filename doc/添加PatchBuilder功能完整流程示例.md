# 添加 PatchBuilder 功能完整流程示例

本文档展示如何完整地添加一个新的 PatchBuilder 功能，以"【气运】采集蛐蛐成功率"为例。

## 功能信息

- **featureKey**: `catchGrasshopper`
- **功能名称**: 采集蛐蛐成功率
- **功能描述**: 采集蛐蛐时，基础成功率根据气运增加
- **目标方法**: `GameData.Domains.World.ResourceDomain.CollectGrasshopper`
- **需要替换的函数**: `IRandomSource.CheckPercentProb(int percent)`
- **替换次数**: 1次（假设原方法中只有一个概率判断）

## 第一步：修改配置文件 (Config.lua)

在 `Config.lua` 的 `DefaultSettings` 数组中添加新配置项：

```lua
{
    SettingType = "Dropdown",
    Key = "catchGrasshopper",
    DisplayName = "【气运】采集蛐蛐成功率",
    Description = "采集蛐蛐时，基础成功率根据气运增加",
    Options = luckLevelOptions,
    DefaultValue = 0,
},
```

## 第二步：更新配置管理器 (ConfigManager.cs)

在 `ConfigManager.cs` 中添加新的配置属性：

```csharp
/// <summary>
/// 采集蛐蛐成功率气运设置
/// </summary>
public static int catchGrasshopper = 0;
```

## 第三步：更新功能显示名称映射 (LuckyCalculator.cs)

在 `LuckyCalculator.cs` 的 `FeatureDisplayNames` 字典中添加新映射：

```csharp
private static readonly Dictionary<string, string> FeatureDisplayNames = new Dictionary<string, string>
{
    // ... 现有映射
    { "catchGrasshopper", "采集蛐蛐成功率" },
    // ... 其他映射
};
```

## 第四步：创建功能专用补丁类

创建新文件 `src/Features/Resources/CatchGrasshopperPatch.cs`：

```csharp
/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Resources
{
    /// <summary>
    /// 采集蛐蛐补丁
    /// 配置项: catchGrasshopper
    /// 功能: 修改采集蛐蛐时的成功率，根据气运增加
    /// </summary>
    public static class CatchGrasshopperPatch
    {
        /// <summary>
        /// CatchGrasshopper 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// CatchGrasshopper 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(CatchGrasshopperPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// CatchGrasshopper 功能专用的 CheckPercentProb 替换方法
        /// 支持 CatchGrasshopper 功能的独立气运设置
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "catchGrasshopper");
        }

        /// <summary>
        /// 应用采集蛐蛐补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("catchGrasshopper")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.World.ResourceDomain),
                MethodName = "CollectGrasshopper",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(short), 
                    typeof(short), 
                    typeof(int)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "catchGrasshopper",
                    OriginalMethod);

            // 配置方法替换规则
            ConfigureReplacements(patchBuilder);
            
            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        /// <param name="patchBuilder">PatchBuilder 实例</param>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // CheckPercentProb 方法替换
            // 1. if (random.CheckPercentProb(successRate))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);
        }
    }
}
```

## 第五步：更新 QuantumMaster.cs 配置映射

在 `QuantumMaster.cs` 的 `patchBuilderMappings` 字典中添加新映射：

```csharp
private readonly Dictionary<string, (System.Func<Harmony, bool> patchMethod, System.Func<bool> condition)> patchBuilderMappings = new Dictionary<string, (System.Func<Harmony, bool>, System.Func<bool>)>
{
    // ... 现有映射
    
    // Resources 模块
    { "CollectResource", (Features.Resources.CollectResourcePatch.Apply, () => ConfigManager.IsFeatureEnabled("collectResource")) },
    { "UpgradeCollectMaterial", (Features.Resources.UpgradeCollectMaterialPatch.Apply, () => ConfigManager.IsFeatureEnabled("collectResource")) },
    { "GetCollectResourceAmount", (Features.Resources.GetCollectResourceAmountPatch.Apply, () => ConfigManager.IsFeatureEnabled("GetCollectResourceAmount")) },
    { "catchGrasshopper", (Features.Resources.CatchGrasshopperPatch.Apply, () => ConfigManager.IsFeatureEnabled("catchGrasshopper")) },
    
    // ... 其他映射
};
```

## 第六步：更新功能描述文件 (readme.md)

在 `README.md` 文件中添加新功能的描述，让用户了解功能的作用。找到合适的分类位置添加：

同时在 `README_en.md` 添加英文的描述，添加时需要特别注意格式

```plaintext
【气运】采集蛐蛐成功率: 采集蛐蛐时，基础成功率根据气运增加
```

## 第七步：编译和测试

1. 编译项目确保没有语法错误：
```powershell
dotnet build
```

2. 在游戏中测试功能是否正常工作

## 常见的替换方法类型

根据原方法中使用的随机数函数，选择对应的替换方法：

### 1. Next(int min, int max) - 两参数版本
```csharp
// 倾向最大值
public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
{
    return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "featureKey");
}

// 倾向最小值
public static int Next2ArgsMin_Method(this IRandomSource randomSource, int min, int max)
{
    return LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(min, max, "featureKey");
}
```

### 2. Next(int max) - 单参数版本
```csharp
// 倾向最大值
public static int Next1ArgMax_Method(this IRandomSource randomSource, int max)
{
    return LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max, "featureKey");
}

// 倾向0
public static int Next1Arg0_Method(this IRandomSource randomSource, int max)
{
    return LuckyCalculator.Calc_Random_Next_1Arg_0_By_Luck(max, "featureKey");
}
```

### 3. CheckPercentProb(int percent) - 百分比概率
```csharp
// 倾向成功
public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
{
    return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "featureKey");
}

// 倾向失败
public static bool CheckPercentProbFalse_Method(this IRandomSource randomSource, int percent)
{
    return LuckyCalculator.Calc_Random_CheckPercentProb_False_By_Luck(randomSource, percent, "featureKey");
}
```

### 4. CheckProb(int chance, int total) - 概率检查
```csharp
// 倾向成功
public static bool CheckProbTrue_Method(this IRandomSource randomSource, int chance, int total)
{
    return LuckyCalculator.Calc_Random_CheckProb_True_By_Luck(randomSource, chance, total, "featureKey");
}

// 倾向失败
public static bool CheckProbFalse_Method(this IRandomSource randomSource, int chance, int total)
{
    return LuckyCalculator.Calc_Random_CheckProb_False_By_Luck(randomSource, chance, total, "featureKey");
}
```

## 确定替换次数

替换次数 (targetOccurrence) 是指在目标方法中，该函数调用出现的第几次。例如：

```csharp
// 原方法示例
public void SomeMethod()
{
    var result1 = random.Next(1, 10);    // 第1次 Next(int, int) 调用
    var result2 = random.Next(5, 15);    // 第2次 Next(int, int) 调用
    var success = random.CheckPercentProb(50);  // 第1次 CheckPercentProb 调用
}
```

如果要替换第2次的 `Next(int, int)` 调用，则 `targetOccurrence` 应该设为 2。


## 调试技巧

1. **使用 DebugLog**: 在替换方法中添加日志输出，验证是否被正确调用
2. **检查原方法**: 使用反射或反编译工具确认原方法的确切签名
3. **逐步测试**: 先创建最简单的替换，确认流程正确后再添加复杂逻辑
4. **查看控制台**: 观察 Harmony 补丁应用的日志输出

## 注意事项

1. **保持原始逻辑**: 绝对不要改变原方法的核心逻辑
2. **参数类型匹配**: 确保替换方法的参数类型完全匹配
3. **替换次数准确**: 仔细计算目标方法中的调用次数
4. **功能独立性**: 每个功能应该有独立的补丁类
5. **向后兼容**: 确保新功能不会破坏现有功能

通过遵循这个流程，可以快速且正确地添加新的 PatchBuilder 功能。
