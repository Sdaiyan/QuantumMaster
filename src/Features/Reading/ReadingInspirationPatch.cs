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
    /// 功能: 控制灵光一闪的触发概率
    /// 注意: 当概率不为0时，必定灵光一闪
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
            if (!ConfigManager.GetCurrReadingEventBonusRate)
            {
                return; // 使用原版逻辑
            }

            // 如果原概率大于0，则设为100%
            if (__result > 0)
            {
                DebugLog.Info($"灵光一闪概率: 原值{__result}% -> 强制触发100%");
                __result = 100;
            }
        }
    }
}
