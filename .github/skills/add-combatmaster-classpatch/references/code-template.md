# ClassPatch 代码模板（CombatMaster）

## Postfix 示例（修改 GetModifyValue 返回值）

基于 `YueNvJianFaPatch.cs`：

```csharp
/*
 * CombatMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.SpecialEffect;
using QuantumMaster.Shared;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// <功能描述>补丁 - Class 补丁形式
    /// 对 GetModifyValue 方法进行 postfix 处理
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.<门派>.<类型>.<功法类名>), "GetModifyValue")]
    public class <FeatureName>Patch
    {
        [HarmonyPostfix]
        public static void GetModifyValuePostfix(
            GameData.Domains.SpecialEffect.CombatSkill.<门派>.<类型>.<功法类名> __instance,
            ref int __result,
            AffectedDataKey dataKey,
            int currModifyValue)
        {
            try
            {
                if (!CombatConfigManager.IsFeatureEnabled("<FeatureKey>"))
                    return;

                // 默认只对太吾生效
                var currentCharId = __instance.CharacterId;
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                if (currentCharId != taiwuId)
                    return;

                DebugLog.Info($"[<FeatureName>Patch] 太吾执行GetModifyValue - 原始返回值: {__result}");

                // 根据返回值范围处理
                if (__result > 0)
                {
                    // 正值：使用倾向最大值的气运函数，范围从 result 到 result*2
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(
                        __result, __result * 2, "<FeatureKey>");
                    DebugLog.Info($"[<FeatureName>Patch] 正值气运加成: {__result} -> {newValue}");
                    __result = newValue;
                }
                else if (__result < 0)
                {
                    // 负值：使用倾向最小值的气运函数，范围从 result*2 到 result
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(
                        __result * 2, __result, "<FeatureKey>");
                    DebugLog.Info($"[<FeatureName>Patch] 负值气运加成: {__result} -> {newValue}");
                    __result = newValue;
                }
                // __result == 0 时不处理
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[<FeatureName>Patch] Postfix处理时发生错误: {ex.Message}");
            }
        }
    }
}
```

## 注册示例

```csharp
// CombatMaster.cs - 只需注册到 patchConfigMappings
{ "<FeatureKey>", (typeof(Features.Combat.<FeatureName>Patch), () => CombatConfigManager.IsFeatureEnabled("<FeatureKey>")) },
```

## 常用 LuckyCalculator 方法

```csharp
// 数值范围（倾向最大/最小）
LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, featureKey)
LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(min, max, featureKey)

// 概率判定（倾向成功/失败）
LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(random, percent, featureKey)
LuckyCalculator.Calc_Random_CheckPercentProb_False_By_Luck(random, percent, featureKey)
```
