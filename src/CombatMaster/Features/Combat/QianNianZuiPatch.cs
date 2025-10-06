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
    /// 千年醉获得的几率增益增加功能补丁 - Class 补丁形式
    /// 对 GetModifyValue 方法进行 postfix 处理
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Fulongtan.DefenseAndAssist.QianNianZui), "GetModifyValue")]
    public class QianNianZuiPatch
    {
        /// <summary>
        /// Postfix方法 - 在 GetModifyValue 执行后处理返回值
        /// </summary>
        /// <param name="__instance">实例对象</param>
        /// <param name="__result">原始返回值，会被修改</param>
        /// <param name="dataKey">数据键</param>
        /// <param name="currModifyValue">当前修改值</param>
        [HarmonyPostfix]
        public static void GetModifyValuePostfix(GameData.Domains.SpecialEffect.CombatSkill.Fulongtan.DefenseAndAssist.QianNianZui __instance, ref int __result, AffectedDataKey dataKey, int currModifyValue)        {
            try
            {
                // 检查功能是否启用
                if (!CombatConfigManager.IsFeatureEnabled("QianNianZui"))
                {
                    return;
                }

                // 获取当前角色ID和太吾ID
                var currentCharId = __instance.CharacterId;
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                // 只有太吾才进行处理
                if (currentCharId != taiwuId)
                {
                    DebugLog.Info($"[QianNianZuiPatch] 非太吾角色({currentCharId})执行千年醉，不进行处理");
                    return;
                }

                DebugLog.Info($"[QianNianZuiPatch] 太吾执行GetModifyValue - 原始返回值: {__result}, dataKey: {dataKey}, currModifyValue: {currModifyValue}");

                // 根据返回值进行处理
                if (__result > 0)
                {
                    // 大于0 则调用 Calc_Random_Next_2Args_Max_By_Luck，最大值为原值的2倍
                    var maxValue = __result * 2;
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(__result, maxValue, "QianNianZui");
                    DebugLog.Info($"[QianNianZuiPatch] 原始值>0，使用气运加成: {__result} -> {newValue} (范围: {__result} - {maxValue})");
                    __result = newValue;
                }
                else if (__result < 0)
                {
                    // 小于0 则调用 Calc_Random_Next_2Args_Min_By_Luck，最小值为原值的2倍
                    var minValue = __result * 2;
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(minValue, __result, "QianNianZui");
                    DebugLog.Info($"[QianNianZuiPatch] 原始值<0，使用气运加成: {__result} -> {newValue} (范围: {minValue} - {__result})");
                    __result = newValue;
                }
                else
                {
                    // 其他情况（等于0）什么都不做
                    DebugLog.Info($"[QianNianZuiPatch] 原始值为0，不进行处理");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[QianNianZuiPatch] Postfix处理时发生错误: {ex.Message}");
            }
        }
    }
}
