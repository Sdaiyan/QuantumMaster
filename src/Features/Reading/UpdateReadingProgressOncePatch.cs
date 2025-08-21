/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Reading
{
    /// <summary>
    /// 地图捡书灵光一闪独立补丁
    /// 配置项: GetCurrReadingEventBonusRate
    /// 功能: 【气运】地图上捡起书籍时的灵光一闪触发概率根据设置的气运影响
    /// </summary>
    public static class UpdateReadingProgressOncePatch
    {
        /// <summary>
        /// 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(UpdateReadingProgressOncePatch),
                MethodName = nameof(CheckProbTrue_Method)
            };
        }

        /// <summary>
        /// UpdateReadingProgressOnce 功能专用的 CheckProb 替换方法实现
        /// </summary>
        public static bool CheckProbTrue_Method(this IRandomSource randomSource, int chance, int total)
        {
            return LuckyCalculator.Calc_Random_CheckProb_True_By_Luck(randomSource, chance, total, "GetCurrReadingEventBonusRate");
        }

        /// <summary>
        /// 主要的补丁应用方法
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("GetCurrReadingEventBonusRate")) return false;

            DebugLog.Info("[UpdateReadingProgressOncePatch] 开始应用地图上捡起书籍的灵光一闪补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "UpdateReadingProgressOnce",
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(bool), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "UpdateReadingProgressOnce",
                OriginalMethod);

            ConfigureReplacements(patchBuilder);
            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // 1 替换地图捡书时的灵光一闪检查
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckProb,
                Replacements.CheckProbTrue,
                1);
        }
    }
}
