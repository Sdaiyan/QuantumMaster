/*
 * CombatMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using QuantumMaster.Shared;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// 太素绝手打断施展功法概率提升功能补丁 - Class 补丁形式
    /// 对 CalcInterruptOdds 方法进行 postfix 处理
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Jieqingmen.Finger.TaiSuJueShou), "CalcInterruptOdds")]
    public class TaiSuJueShouPatch
    {
        /// <summary>
        /// Postfix方法 - 在 CalcInterruptOdds 执行后处理返回值
        /// </summary>
        /// <param name="__instance">实例对象</param>
        /// <param name="__result">原始返回值，会被修改</param>
        /// <param name="selfSkill">自身功法</param>
        /// <param name="isDirect">是否直接</param>
        /// <param name="enemySkill">敌方功法</param>
        [HarmonyPostfix]
        public static void CalcInterruptOddsPostfix(
            GameData.Domains.SpecialEffect.CombatSkill.Jieqingmen.Finger.TaiSuJueShou __instance, 
            ref int __result, 
            GameData.Domains.CombatSkill.CombatSkillKey selfSkill, 
            bool isDirect, 
            GameData.Domains.CombatSkill.CombatSkillKey enemySkill)
        {
            try
            {
                // 检查功能是否启用
                if (!CombatConfigManager.IsFeatureEnabled("TaiSuJueShou"))
                {
                    return;
                }

                // 获取当前角色ID和太吾ID
                var currentCharId = __instance.CharacterId;
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                // 只有太吾才进行处理
                if (currentCharId != taiwuId)
                {
                    DebugLog.Info($"[TaiSuJueShouPatch] 非太吾角色({currentCharId})执行太素绝手，不进行处理");
                    return;
                }

                DebugLog.Info($"[TaiSuJueShouPatch] 太吾执行CalcInterruptOdds - 原始返回值: {__result}, isDirect: {isDirect}");

                // 如果返回值大于等于0，则进行处理
                if (__result >= 0)
                {
                    // 计算最大值：原值的2倍，但最多100
                    int maxValue = Math.Min(__result * 2, 100);
                    
                    // 调用 Calc_Random_Next_2Args_Max_By_Luck，范围从result到maxValue
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(__result, maxValue, "TaiSuJueShou");
                    DebugLog.Info($"[TaiSuJueShouPatch] 原始值{__result}，使用气运加成: {__result} -> {newValue} (最大值: {maxValue})");
                    __result = newValue;
                }
                else
                {
                    // 其他情况什么都不做
                    DebugLog.Info($"[TaiSuJueShouPatch] 原始值{__result} < 0，不进行处理");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[TaiSuJueShouPatch] Postfix处理时发生错误: {ex.Message}");
            }
        }
    }
}
