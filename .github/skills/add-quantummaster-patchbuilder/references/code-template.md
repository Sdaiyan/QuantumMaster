# PatchBuilder 代码模板（QuantumMaster）

基于 `CollectResourcePatch.cs` 等现有实现的模板。

```csharp
/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.<Category>
{
    /// <summary>
    /// <功能描述>补丁
    /// 配置项: <featureKey>
    /// 功能: <详细功能说明>
    /// </summary>
    public static class <FeatureName>Patch
    {
        /// <summary>
        /// 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(<FeatureName>Patch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// 功能专用的 CheckPercentProb 替换方法
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "<featureKey>");
        }

        /// <summary>
        /// 应用补丁
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("<featureKey>")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.<Domain>.<ClassName>),
                MethodName = "<MethodName>",
                Parameters = new Type[] { 
                    // 填入目标方法的参数类型
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "<featureKey>",
                    OriginalMethod);

            ConfigureReplacements(patchBuilder);
            
            patchBuilder.Apply(harmony);
            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // 替换第 N 次出现的目标方法调用
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);  // 替换第1次出现
        }
    }
}
```

## 替换方法命名规范

根据替换目标和期望结果命名：
- `CheckPercentProbTrue_Method` — CheckPercentProb 倾向 true
- `CheckPercentProbFalse_Method` — CheckPercentProb 倾向 false
- `Next2ArgsMax_Method` — Next(min, max) 倾向最大值
- `Next2ArgsMin_Method` — Next(min, max) 倾向最小值
- `Next1ArgMax_Method` — Next(max) 倾向最大值
- `Next1Arg0_Method` — Next(max) 倾向 0

## 多次替换示例

如果目标方法中多次调用同一随机函数，且需要不同处理：

```csharp
private static class Replacements
{
    // 第1次调用倾向成功
    public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
    {
        Type = typeof(<FeatureName>Patch),
        MethodName = nameof(CheckPercentProbTrue_Method)
    };

    // 第2次调用倾向失败
    public static readonly ReplacementMethodInfo CheckPercentProbFalse = new ReplacementMethodInfo
    {
        Type = typeof(<FeatureName>Patch),
        MethodName = nameof(CheckPercentProbFalse_Method)
    };
}

private static void ConfigureReplacements(PatchBuilder patchBuilder)
{
    patchBuilder.AddExtensionMethodReplacement(
            PatchPresets.Extensions.CheckPercentProb,
            Replacements.CheckPercentProbTrue,
            1);  // 第1次

    patchBuilder.AddExtensionMethodReplacement(
            PatchPresets.Extensions.CheckPercentProb,
            Replacements.CheckPercentProbFalse,
            2);  // 第2次
}
```

## 接口方法替换示例（Next）

```csharp
// 替换 IRandomSource.Next(int, int)
patchBuilder.AddInstanceMethodReplacement(
        PatchPresets.InstanceMethods.Next2Args,
        Replacements.Next2ArgsMax,
        1);

// 替换 IRandomSource.Next(int)
patchBuilder.AddInstanceMethodReplacement(
        PatchPresets.InstanceMethods.Next1Arg,
        Replacements.Next1ArgMax,
        1);
```
