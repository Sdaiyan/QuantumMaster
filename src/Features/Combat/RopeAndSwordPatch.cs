/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Combat;
using Redzen.Random;

namespace QuantumMaster.Features.Combat
{
    /// <summary>
    /// 绳子和剑柄相关功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class RopeAndSwordPatch
    {
        /// <summary>
        /// 绳子和剑柄功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(RopeAndSwordPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// 绳子和剑柄功能专用的 CheckPercentProb 替换方法（期望成功）
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "ropeOrSword");
        }

        /// <summary>
        /// 应用绳子和剑柄命中检查补丁
        /// 配置项: ropeOrSword
        /// 功能: 【气运】根据气运影响绳子绑架和剑柄救人的成功率
        /// </summary>
        public static bool PatchCheckRopeHit(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("ropeOrSword")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Combat.CombatDomain),
                MethodName = "CheckRopeHit",
                Parameters = new Type[] { 
                    typeof(Redzen.Random.IRandomSource), 
                    typeof(int) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CheckRopeHit",
                    OriginalMethod);

            // CheckPercentProb 方法替换 - 期望成功，使用气运提高成功率
            // 第1个 CheckPercentProb 调用
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 应用战斗外绳子和剑柄命中检查补丁
        /// 配置项: ropeOrSword
        /// 功能: 【气运】根据气运影响战斗外绳子和剑柄的成功率
        /// </summary>
        public static bool PatchCheckRopeHitOutOfCombat(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("ropeOrSword")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Combat.CombatDomain),
                MethodName = "CheckRopeHitOutOfCombat",
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
                    "CheckRopeHitOutOfCombat",
                    OriginalMethod);

            // CheckPercentProb 方法替换 - 期望成功，使用气运提高成功率
            // 第1个 CheckPercentProb 调用
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
