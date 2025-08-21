/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Taiwu;

namespace QuantumMaster.Features.Reading
{
    /// <summary>
    /// 灵光一闪功能补丁
    /// 配置项: GetCurrReadingEventBonusRate
    /// 功能: 【气运】控制灵光一闪的触发概率，根据气运影响成功率
    /// 
    /// 注意：地图捡书的灵光一闪功能已迁移到 UpdateReadingProgressOncePatch.cs
    /// </summary>
    [HarmonyPatch(typeof(TaiwuDomain), "GetCurrReadingEventBonusRate")]
    public class ReadingInspirationPatch
    {
        /// <summary>
        /// 当前读书事件奖励率补丁
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(ref short __result)
        {
            if (!ConfigManager.IsFeatureEnabled("GetCurrReadingEventBonusRate"))
            {
                return; // 使用原版逻辑
            }

            // 如果原概率大于0，则使用气运系统进行判断
            if (__result > 0)
            {
                bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(null, __result, "GetCurrReadingEventBonusRate");
                short newResult = success ? (short)100 : (short)0;
                DebugLog.Info($"【气运】灵光一闪: 原概率{__result}% -> 气运判定{(success ? "成功" : "失败")} -> {newResult}%");
                __result = newResult;
            }
        }
    }
}
