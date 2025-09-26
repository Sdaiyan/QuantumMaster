/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Items
{
    /// <summary>
    /// 抓到双蛐蛐概率独立补丁
    /// 配置项: CatchCricketDouble
    /// 功能: 【气运】抓蛐蛐时，抓到2只蛐蛐的概率根据气运增加
    /// </summary>
    public static class CatchCricketDoublePatch
    {
        /// <summary>
        /// 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(CatchCricketDoublePatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// CatchCricketDouble 功能专用的 CheckPercentProb 替换方法实现
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "CatchCricketDouble");
        }

        /// <summary>
        /// 主要的补丁应用方法
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("CatchCricketDouble")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Item.ItemDomain),
                MethodName = "CatchCricket",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(short), 
                    typeof(short), 
                    typeof(short), 
                    typeof(sbyte) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "CatchCricketDouble",
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
            // CheckPercentProb 方法替换 - 第3个调用是抓到2只蛐蛐的概率
            // 3 第3个 CheckPercentProb 调用 - 抓到额外蛐蛐的概率
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                Replacements.CheckPercentProbTrue,
                3);
        }
    }
}
