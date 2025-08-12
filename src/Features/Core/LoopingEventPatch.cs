/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Taiwu;

namespace QuantumMaster.Features.Core
{
    /// <summary>
    /// 天人感应功能补丁
    /// 配置项: TryAddLoopingEvent
    /// 功能: 控制天人感应的触发概率
    /// 注意: 当概率不为0时，必定成功触发天人感应
    /// </summary>
    [HarmonyPatch(typeof(TaiwuDomain), "TryAddLoopingEvent")]
    public class LoopingEventPatch
    {
        /// <summary>
        /// 天人感应触发补丁
        /// </summary>
        [HarmonyPrefix]
        public static void Prefix(ref int basePercentProb)
        {
            if (!ConfigManager.TryAddLoopingEvent)
            {
                return; // 使用原版逻辑
            }

            DebugLog.Info($"天人感应: 原概率{basePercentProb}% -> 强制触发100%");
            basePercentProb = 100;
        }
    }
}
