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
    /// 读书进度策略独立补丁
    /// 配置项: GetStrategyProgressAddValue
    /// 功能: 【气运】读书策略进度增加值根据设置的气运在浮动区间内增加或者减少
    /// </summary>
    public static class GetStrategyProgressAddValuePatch
    {
        /// <summary>
        /// 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(GetStrategyProgressAddValuePatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };
        }

        /// <summary>
        /// GetStrategyProgressAddValue 功能专用的 Next2Args 替换方法实现
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "GetStrategyProgressAddValue");
        }

        /// <summary>
        /// 主要的补丁应用方法
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("GetStrategyProgressAddValue")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "GetStrategyProgressAddValue",
                Parameters = new Type[] { typeof(IRandomSource), typeof(Config.ReadingStrategyItem) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "GetStrategyProgressAddValue",
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
            // 1 return (sbyte)((strategyCfg.MaxProgressAddValue > strategyCfg.MinProgressAddValue) ? random.Next(strategyCfg.MinProgressAddValue, strategyCfg.MaxProgressAddValue + 1) : strategyCfg.MaxProgressAddValue);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                Replacements.Next2ArgsMax,
                1);
        }
    }
}
