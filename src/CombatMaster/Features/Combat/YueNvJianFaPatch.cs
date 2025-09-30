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
    /// 越女剑法追击几率提升功能补丁 - Class 补丁形式
    /// 对 GetModifyValue 方法进行 postfix 处理
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.Sword.YueNvJianFa), "GetModifyValue")]
    public class YueNvJianFaPatch
    {
        /// <summary>
        /// Postfix方法 - 在 GetModifyValue 执行后处理返回值
        /// </summary>
        /// <param name="__instance">实例对象</param>
        /// <param name="__result">原始返回值，会被修改</param>
        /// <param name="dataKey">数据键</param>
        /// <param name="currModifyValue">当前修改值</param>
        [HarmonyPostfix]
        public static void GetModifyValuePostfix(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.Sword.YueNvJianFa __instance, ref int __result, AffectedDataKey dataKey, int currModifyValue)
        {
            try
            {
                // 检查功能是否启用
                if (!CombatConfigManager.IsFeatureEnabled("YueNvJianFa"))
                {
                    return;
                }

                // 获取当前角色ID和太吾ID
                var currentCharId = __instance.CharacterId;
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                // 只有太吾才进行处理
                if (currentCharId != taiwuId)
                {
                    DebugLog.Info($"[YueNvJianFaPatch] 非太吾角色({currentCharId})执行越女剑法，不进行处理");
                    return;
                }

                DebugLog.Info($"[YueNvJianFaPatch] 太吾执行GetModifyValue - 原始返回值: {__result}, dataKey: {dataKey}, currModifyValue: {currModifyValue}");

                // 根据返回值进行处理
                if (__result == 50)
                {
                    // 等于50 则调用 Calc_Random_Next_2Args_Max_By_Luck，范围从result到100
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(__result, 100, "YueNvJianFa");
                    DebugLog.Info($"[YueNvJianFaPatch] 原始值50，使用气运加成: {__result} -> {newValue}");
                    __result = newValue;
                }
                else if (__result == -50)
                {
                    // 等于-50 则调用 Calc_Random_Next_2Args_Min_By_Luck，范围从-100到result
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(-100, __result, "YueNvJianFa");
                    DebugLog.Info($"[YueNvJianFaPatch] 原始值-50，使用气运加成: {__result} -> {newValue}");
                    __result = newValue;
                }
                else
                {
                    // 其他情况什么都不做
                    DebugLog.Info($"[YueNvJianFaPatch] 原始值{__result}，不进行处理");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[YueNvJianFaPatch] Postfix处理时发生错误: {ex.Message}");
            }
        }
    }
}