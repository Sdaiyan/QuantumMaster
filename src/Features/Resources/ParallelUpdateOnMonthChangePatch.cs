/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Resources
{
    /// <summary>
    /// 每月地图更新功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class ParallelUpdateOnMonthChangePatch
    {
        /// <summary>
        /// 每月地图更新功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next1ArgMax = new ReplacementMethodInfo
            {
                Type = typeof(ParallelUpdateOnMonthChangePatch),
                MethodName = nameof(Next1ArgMax_Method)
            };
        }

        /// <summary>
        /// 每月地图更新功能专用的 Next1Arg 替换方法（取最大值）
        /// </summary>
        public static int Next1ArgMax_Method(this IRandomSource randomSource, int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max, "ParallelUpdateOnMonthChange");
        }

        /// <summary>
        /// 应用每月地图更新补丁
        /// 配置项: ParallelUpdateOnMonthChange
        /// 功能: 修改每月地图资源恢复的随机计算，获得更多资源恢复
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("ParallelUpdateOnMonthChange")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Map.MapDomain),
                MethodName = "ParallelUpdateOnMonthChange",
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(int) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "ParallelUpdateOnMonthChange",
                    OriginalMethod);

            // 配置方法替换规则
            ConfigureReplacements(patchBuilder);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // 1 int addValue = context.Random.Next(maxAddValue + 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    Replacements.Next1ArgMax,
                    1);
        }
    }
}
