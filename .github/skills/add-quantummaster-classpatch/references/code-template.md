# ClassPatch 代码模板（QuantumMaster）

## Postfix 示例（修改返回值）

基于 `ReadingInspirationPatch.cs`：

```csharp
/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.<Category>
{
    /// <summary>
    /// <功能描述>补丁
    /// 配置项: <featureKey>
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.<Domain>.<ClassName>), "<MethodName>")]
    public class <FeatureName>Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref <ReturnType> __result)
        {
            if (!ConfigManager.IsFeatureEnabled("<featureKey>"))
            {
                return;
            }

            // 使用原始值作为范围起点
            if (__result > 0)
            {
                bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(
                    null, __result, "<featureKey>");
                <ReturnType> newResult = success ? (<ReturnType>)100 : (<ReturnType>)0;
                DebugLog.Info($"【气运】<功能名>: 原值{__result} -> {newResult}");
                __result = newResult;
            }
        }
    }
}
```

## Postfix 示例（需要角色判断）

```csharp
[HarmonyPatch(typeof(GameData.Domains.<Domain>.<ClassName>), "<MethodName>")]
public class <FeatureName>Patch
{
    [HarmonyPostfix]
    public static void Postfix(<ClassName> __instance, ref int __result)
    {
        if (!ConfigManager.IsFeatureEnabled("<featureKey>"))
            return;

        // 默认只对太吾生效
        var currentCharId = __instance.CharacterId;
        var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
        if (currentCharId != taiwuId)
            return;

        // 使用原始值作为范围
        var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(
            __result, __result * 2, "<featureKey>");
        DebugLog.Info($"【气运】<功能名>: {__result} -> {newValue}");
        __result = newValue;
    }
}
```

## Prefix 示例（替换原方法）

基于 `GenderControlPatch.cs`：

```csharp
[HarmonyPatch(typeof(GameData.Domains.<Domain>.<ClassName>), "<MethodName>")]
public class <FeatureName>Patch
{
    /// <summary>
    /// 返回 false 跳过原方法
    /// </summary>
    [HarmonyPrefix]
    public static bool Prefix(ref <ReturnType> __result, <ParamType> param)
    {
        // 读取配置值
        var configValue = ConfigManager.<featureKey>;
        
        // 特定值表示不修改
        if (configValue == <defaultValue>)
            return true;  // 继续执行原方法

        // 设置返回值并跳过原方法
        __result = (<ReturnType>)configValue;
        return false;  // 跳过原方法
    }
}
```

## 常用 LuckyCalculator 方法

```csharp
// 概率判定（倾向成功/失败）
LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(random, percent, featureKey)
LuckyCalculator.Calc_Random_CheckPercentProb_False_By_Luck(random, percent, featureKey)

// 数值范围（倾向最大/最小）
LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, featureKey)
LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(min, max, featureKey)
```
