/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Skills
{
    /// <summary>
    /// 较艺迫使认输成功率补丁
    /// 配置项: DebateGameTryForceWin
    /// 功能: 修改较艺时迫使对方认输的成功率，根据气运提升成功概率
    /// </summary>
    public static class DebateGameTryForceWinPatch
    {
        /// <summary>
        /// DebateGameTryForceWin 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// DebateGameTryForceWin 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(DebateGameTryForceWinPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// DebateGameTryForceWin 功能专用的 CheckPercentProb 替换方法
        /// 支持 DebateGameTryForceWin 功能的独立气运设置，倾向返回true（提升成功率）
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "DebateGameTryForceWin");
        }

        /// <summary>
        /// 应用较艺迫使认输成功率补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("DebateGameTryForceWin")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "DebateGameTryForceWin",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "DebateGameTryForceWin",
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
            // CheckPercentProb 方法替换 - 第1次出现，倾向true（提升迫使认输成功率）
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);
        }
    }
}