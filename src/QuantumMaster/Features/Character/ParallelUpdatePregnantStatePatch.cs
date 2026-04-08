/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Character
{
    /// <summary>
    /// 胎教触发概率提升补丁
    /// 配置项: ParallelUpdatePregnantState
    /// 功能: 提高过月时触发胎教的概率（原本概率为75%）
    /// </summary>
    public static class ParallelUpdatePregnantStatePatch
    {
        /// <summary>
        /// 胎教触发概率功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(ParallelUpdatePregnantStatePatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// 胎教触发概率功能专用的 CheckPercentProb 替换方法（期望结果为true，提高触发概率）
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "ParallelUpdatePregnantState");
        }

        /// <summary>
        /// 应用胎教触发概率补丁
        /// 配置项: ParallelUpdatePregnantState
        /// 功能: 提高过月时触发胎教的概率，原本概率为75%，期望概率为100%
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("ParallelUpdatePregnantState")) return false;

            DebugLog.Info("[ParallelUpdatePregnantStatePatch] 开始应用胎教触发概率补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.CharacterDomain),
                MethodName = "ParallelUpdatePregnantState",
                Parameters = new Type[]
                {
                    typeof(GameData.Common.DataContext),
                    typeof(GameData.Domains.Character.Character),
                    typeof(GameData.Domains.Character.ParallelModifications.PregnantStateModification)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "ParallelUpdatePregnantState",
                    OriginalMethod);

            ConfigureReplacements(patchBuilder);

            patchBuilder.Apply(harmony);
            return true;
        }

        /// <summary>
        /// 配置方法替换规则：替换第1次出现的 CheckPercentProb
        /// </summary>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);  // 替换第1次出现
        }
    }
}
