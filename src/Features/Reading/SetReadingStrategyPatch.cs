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
    /// 读书效率策略独立补丁
    /// 配置项: SetReadingStrategy
    /// 功能: 【气运】技艺书籍的效率增加策略进度增加值根据设置的气运在浮动区间内增加或者减少
    /// </summary>
    public static class SetReadingStrategyPatch
    {
        /// <summary>
        /// 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(SetReadingStrategyPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };
        }

        /// <summary>
        /// SetReadingStrategy 功能专用的 Next2Args 替换方法实现
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "SetReadingStrategy");
        }

        /// <summary>
        /// 主要的补丁应用方法
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("SetReadingStrategy")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "SetReadingStrategy",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(byte), 
                    typeof(int), 
                    typeof(sbyte) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "SetReadingStrategy",
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
            // 替换第1个 random.Next 2参数版本调用 - 期望好结果，使用气运提升效果
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                Replacements.Next2ArgsMax,
                1);
        }
    }
}
