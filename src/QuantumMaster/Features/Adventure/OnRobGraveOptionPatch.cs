/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Adventure
{
    /// <summary>
    /// 盗墓事件概率补丁
    /// 配置项: OnRobGraveOptionResource / OnRobGraveOptionItem / OnRobGraveOptionNothingHappen / OnRobGraveOptionMeetSkeleton
    /// 功能: 修改盗墓事件内第 1/2/3/4 次 CheckProbability 调用，分别影响资源、物品、无事发生、遇到骷髅的概率
    /// </summary>
    public static class OnRobGraveOptionPatch
    {
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckProbabilityResourceTrue = new ReplacementMethodInfo
            {
                Type = typeof(OnRobGraveOptionPatch),
                MethodName = nameof(CheckProbabilityResourceTrue_Method)
            };

            public static readonly ReplacementMethodInfo CheckProbabilityItemTrue = new ReplacementMethodInfo
            {
                Type = typeof(OnRobGraveOptionPatch),
                MethodName = nameof(CheckProbabilityItemTrue_Method)
            };

            public static readonly ReplacementMethodInfo CheckProbabilityNothingHappenFalse = new ReplacementMethodInfo
            {
                Type = typeof(OnRobGraveOptionPatch),
                MethodName = nameof(CheckProbabilityNothingHappenFalse_Method)
            };

            public static readonly ReplacementMethodInfo CheckProbabilityMeetSkeletonFalse = new ReplacementMethodInfo
            {
                Type = typeof(OnRobGraveOptionPatch),
                MethodName = nameof(CheckProbabilityMeetSkeletonFalse_Method)
            };
        }

        public static bool CheckProbabilityResourceTrue_Method(int percent)
        {
            return LuckyCalculator.Calc_Random_CheckProbability_True_By_Luck(percent, "OnRobGraveOptionResource");
        }

        public static bool CheckProbabilityItemTrue_Method(int percent)
        {
            return LuckyCalculator.Calc_Random_CheckProbability_True_By_Luck(percent, "OnRobGraveOptionItem");
        }

        public static bool CheckProbabilityNothingHappenFalse_Method(int percent)
        {
            return LuckyCalculator.Calc_Random_CheckProbability_False_By_Luck(percent, "OnRobGraveOptionNothingHappen");
        }

        public static bool CheckProbabilityMeetSkeletonFalse_Method(int percent)
        {
            return LuckyCalculator.Calc_Random_CheckProbability_False_By_Luck(percent, "OnRobGraveOptionMeetSkeleton");
        }

        public static bool Apply(Harmony harmony)
        {
            if (!ShouldApply()) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(ConchShip.EventConfig.Taiwu.TaiwuEvent_e357bf8175dc48ccb47e98217d622954),
                MethodName = "OnRobGraveOption",
                Parameters = Type.EmptyTypes
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "OnRobGraveOption",
                OriginalMethod);

            ConfigureReplacements(patchBuilder);
            patchBuilder.Apply(harmony);

            return true;
        }

        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckProbability,
                Replacements.CheckProbabilityResourceTrue,
                1);

            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckProbability,
                Replacements.CheckProbabilityItemTrue,
                2);

            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckProbability,
                Replacements.CheckProbabilityNothingHappenFalse,
                3);

            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckProbability,
                Replacements.CheckProbabilityMeetSkeletonFalse,
                4);
        }

        private static bool ShouldApply()
        {
            return ConfigManager.IsFeatureEnabled("OnRobGraveOptionResource")
                || ConfigManager.IsFeatureEnabled("OnRobGraveOptionItem")
                || ConfigManager.IsFeatureEnabled("OnRobGraveOptionNothingHappen")
                || ConfigManager.IsFeatureEnabled("OnRobGraveOptionMeetSkeleton");
        }
    }
}