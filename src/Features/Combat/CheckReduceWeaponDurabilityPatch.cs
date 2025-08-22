/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Combat
{
    /// <summary>
    /// 减少武器耐久消耗概率（强制扣除时无效）补丁
    /// 配置项: CheckReduceWeaponDurability
    /// 功能: 武器耐久消耗判定时，根据气运减少消耗的概率
    /// </summary>
    public static class CheckReduceWeaponDurabilityPatch
    {
        /// <summary>
        /// CheckReduceWeaponDurability 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// CheckReduceWeaponDurability 专用的 CheckPercentProb False 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbFalse = new ReplacementMethodInfo
            {
                Type = typeof(CheckReduceWeaponDurabilityPatch),
                MethodName = nameof(CheckPercentProbFalse_Method)
            };
        }

        /// <summary>
        /// CheckReduceWeaponDurability 功能专用的 CheckPercentProb 替换方法
        /// 支持 CheckReduceWeaponDurability 功能的独立气运设置，倾向失败（减少耐久消耗）
        /// </summary>
        public static bool CheckPercentProbFalse_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_False_By_Luck(randomSource, percent, "CheckReduceWeaponDurability");
        }

        /// <summary>
        /// 应用减少武器耐久消耗概率（强制扣除时无效）补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("CheckReduceWeaponDurability")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Combat.CombatContext),
                MethodName = "CheckReduceWeaponDurability",
                Parameters = new Type[] { typeof(sbyte) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CheckReduceWeaponDurability",
                    OriginalMethod);

            // 配置方法替换规则
            ConfigureReplacements(patchBuilder);
            
            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        /// <param name="patchBuilder">PatchBuilder 实例</param>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // CheckPercentProb 方法替换，倾向失败（减少耐久消耗）
            // 1. if (random.CheckPercentProb(breakOdds))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbFalse,
                    1);
        }
    }
}
