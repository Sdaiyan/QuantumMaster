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
    /// 多胞胎概率功能补丁
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class ParallelCreateNewbornChildrenPatch
    {
        /// <summary>
        /// 多胞胎概率功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next1Arg0 = new ReplacementMethodInfo
            {
                Type = typeof(ParallelCreateNewbornChildrenPatch),
                MethodName = nameof(Next1Arg0_Method)
            };
        }

        /// <summary>
        /// 多胞胎概率功能专用的 Next1Arg 替换方法（期望结果为0，增加多胞胎概率）
        /// </summary>
        public static int Next1Arg0_Method(this IRandomSource randomSource, int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_0_By_Luck(max, "ParallelCreateNewbornChildren");
        }

        /// <summary>
        /// 应用多胞胎概率补丁
        /// 配置项: ParallelCreateNewbornChildren
        /// 功能: 提高生育多胞胎的概率
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("ParallelCreateNewbornChildren")) return false;

            DebugLog.Info("[ParallelCreateNewbornChildrenPatch] 开始应用多胞胎概率补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.CharacterDomain),
                MethodName = "ParallelCreateNewbornChildren",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(GameData.Domains.Character.Character), 
                    typeof(bool), 
                    typeof(bool) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "ParallelCreateNewbornChildren",
                OriginalMethod);

            // 配置方法替换规则
            ConfigureReplacements(patchBuilder);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[ParallelCreateNewbornChildrenPatch] 多胞胎概率补丁应用完成");
            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // 替换第1次出现的 Next 单参数调用，期望结果为0（增加多胞胎概率）
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next1Arg,
                Replacements.Next1Arg0,
                1);
        }
    }
}
