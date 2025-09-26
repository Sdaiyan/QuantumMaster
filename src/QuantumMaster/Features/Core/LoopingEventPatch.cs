/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Taiwu;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Core
{
    /// <summary>
    /// 天人感应功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class LoopingEventPatch
    {
        /// <summary>
        /// 天人感应功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(LoopingEventPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// 天人感应功能专用的 CheckPercentProb 替换方法（期望成功）
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "TryAddLoopingEvent");
        }

        /// <summary>
        /// 应用天人感应触发补丁
        /// 配置项: TryAddLoopingEvent
        /// 功能: 【气运】根据气运影响天人感应的触发概率
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("TryAddLoopingEvent")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "TryAddLoopingEvent",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(int) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "TryAddLoopingEvent",
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
            // CheckPercentProb 方法替换 - 期望成功，使用气运提高成功率
            // 第1个 CheckPercentProb 调用
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);
        }
    }
}
