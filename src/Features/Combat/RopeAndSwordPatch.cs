/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Combat;

namespace QuantumMaster.Features.Combat
{
    /// <summary>
    /// 绳子和剑柄相关功能补丁集合
    /// 使用 PatchBuilder 和传统 Harmony 补丁
    /// </summary>
    public static class RopeAndSwordPatch
    {
        /// <summary>
        /// 绳子和剑柄命中检查补丁
        /// 配置项: ropeOrSword
        /// 功能: 【气运】根据气运影响绳子绑架和剑柄救人的成功率
        /// </summary>
        public static bool PatchCheckRopeOrSwordHit(Harmony harmony)
        {
            if (!ConfigManager.ropeOrSword && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Combat.CombatDomain),
                MethodName = "CheckRopeOrSwordHit",
                Parameters = new Type[] { 
                    typeof(Redzen.Random.IRandomSource), 
                    typeof(int) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CheckRopeOrSwordHit",
                    OriginalMethod);

            // CheckPercentProb 方法替换 - 期望成功，使用气运提高成功率
            // 第1个 CheckPercentProb 调用
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 战斗外绳子和剑柄命中检查补丁
        /// 配置项: ropeOrSword
        /// 功能: 【气运】根据气运影响战斗外绳子和剑柄的成功率
        /// </summary>
        public static bool PatchCheckRopeOrSwordHitOutofCombat(Harmony harmony)
        {
            if (!ConfigManager.ropeOrSword && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Combat.CombatDomain),
                MethodName = "CheckRopeOrSwordHitOutofCombat",
                Parameters = new Type[] { 
                    typeof(Redzen.Random.IRandomSource), 
                    typeof(GameData.Domains.Character.Character),
                    typeof(GameData.Domains.Character.Character),
                    typeof(sbyte),
                    typeof(bool),
                    typeof(int)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CheckRopeOrSwordHitOutofCombat",
                    OriginalMethod);

            // CheckPercentProb 方法替换 - 期望成功，使用气运提高成功率
            // 第1个 CheckPercentProb 调用
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
