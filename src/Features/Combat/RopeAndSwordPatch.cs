/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Combat;

namespace QuantumMaster.Features.Combat
{
    /// <summary>
    /// 绳子和剑柄相关功能补丁
    /// 配置项: ropeOrSword
    /// 功能: 控制绳子绑架和剑柄救人的成功率
    /// 注意: 当概率不为0时，必定成功
    /// </summary>
    public class RopeAndSwordPatch
    {
        /// <summary>
        /// 绳子和剑柄命中检查补丁
        /// </summary>
        [HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHit")]
        public class CheckRopeOrSwordHitPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result)
            {
                if (!ConfigManager.ropeOrSword)
                {
                    return true; // 使用原版逻辑
                }

                DebugLog.Info("绳子/剑柄命中检查: 强制成功");
                __result = true;
                return false; // 跳过原方法
            }
        }

        /// <summary>
        /// 战斗外绳子和剑柄命中检查补丁
        /// </summary>
        [HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHitOutofCombat")]
        public class CheckRopeOrSwordHitOutofCombatPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result)
            {
                if (!ConfigManager.ropeOrSword)
                {
                    return true; // 使用原版逻辑
                }

                DebugLog.Info("战斗外绳子/剑柄命中检查: 强制成功");
                __result = true;
                return false; // 跳过原方法
            }
        }
    }
}
