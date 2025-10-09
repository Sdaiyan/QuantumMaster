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
    /// 狂刀命中DEBUFF减少功能补丁 - Class 补丁形式
    /// 对 GetModifyValue 方法进行 postfix 处理
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Shixiangmen.Blade.KuangDao), "GetModifyValue")]
    public class KuangDaoPatch
    {
        /// <summary>
        /// Postfix方法 - 在 GetModifyValue 执行后处理返回值
        /// </summary>
        /// <param name="__instance">实例对象</param>
        /// <param name="__result">原始返回值，会被修改</param>
        /// <param name="dataKey">数据键</param>
        /// <param name="currModifyValue">当前修改值</param>
        [HarmonyPostfix]
        public static void GetModifyValuePostfix(GameData.Domains.SpecialEffect.CombatSkill.Shixiangmen.Blade.KuangDao __instance, ref int __result, AffectedDataKey dataKey, int currModifyValue)
        {
            try
            {
                // 检查功能是否启用
                if (!CombatConfigManager.IsFeatureEnabled("KuangDao"))
                {
                    return;
                }

                // 获取当前角色ID和太吾ID
                var currentCharId = __instance.CharacterId;
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                // 只有太吾才进行处理
                if (currentCharId != taiwuId)
                {
                    DebugLog.Info($"[KuangDaoPatch] 非太吾角色({currentCharId})执行狂刀，不进行处理");
                    return;
                }

                DebugLog.Info($"[KuangDaoPatch] 太吾执行GetModifyValue - 原始返回值: {__result}, dataKey: {dataKey}, currModifyValue: {currModifyValue}");

                // 根据返回值进行处理
                if (__result < 0)
                {
                    // 返回值小于0，调用 Calc_Random_Next_2Args_Max_By_Luck，范围从result到0
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(__result, 0, "KuangDao");
                    DebugLog.Info($"[KuangDaoPatch] 原始值{__result}（负数），使用气运加成减少DEBUFF: {__result} -> {newValue}");
                    __result = newValue;
                }
                else
                {
                    // 其他情况（非负数）什么都不做
                    DebugLog.Info($"[KuangDaoPatch] 原始值{__result}（非负数），不进行处理");
                }
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[KuangDaoPatch] Postfix处理时发生错误: {ex.Message}");
            }
        }
    }
}
