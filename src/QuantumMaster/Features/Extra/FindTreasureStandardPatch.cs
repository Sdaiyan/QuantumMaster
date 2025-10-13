/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Extra
{
    /// <summary>
    /// 挖宝概率提升补丁
    /// 配置项: FindTreasureStandard
    /// 功能: 修改挖宝时的成功率，根据气运增加
    /// </summary>
    public static class FindTreasureStandardPatch
    {
        /// <summary>
        /// FindTreasureStandard 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// FindTreasureStandard 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(FindTreasureStandardPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };

            /// <summary>
            /// FindTreasureStandard 专用的 Next 2Args Max 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(FindTreasureStandardPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };
        }

        /// <summary>
        /// FindTreasureStandard 功能专用的 CheckPercentProb 替换方法
        /// 支持 FindTreasureStandard 功能的独立气运设置
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "FindTreasureStandard");
        }

        /// <summary>
        /// FindTreasureStandard 功能专用的 Next 2Args Max 替换方法
        /// 支持 FindTreasureStandard 功能的独立气运设置
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "FindTreasureStandard");
        }

        /// <summary>
        /// 应用挖宝概率提升补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("FindTreasureStandard")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Extra.ExtraDomain),
                MethodName = "FindTreasureStandard",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(GameData.Domains.Map.Location), 
                    typeof(GameData.Domains.Character.Character), 
                    typeof(GameData.Domains.Extra.TreasureFindResult).MakeByRefType()
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "FindTreasureStandard",
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
            // CheckPercentProb 方法替换
            // 替换第1次出现的 CheckPercentProb 调用，期望结果为 true
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);

            // 替换第2次出现的 CheckPercentProb 调用，期望结果为 true
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    2);

            // Next 方法替换（2参数版本）
            // 替换第1次出现的 Next(int, int) 调用，期望结果为 max
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    1);
        }
    }
}
