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
    /// NPC指点教学功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class OfflineCalcGeneralActionTeachSkillPatch
    {
        /// <summary>
        /// NPC指点教学功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(OfflineCalcGeneralActionTeachSkillPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// NPC指点教学功能专用的 CheckPercentProb 替换方法（期望成功）
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "OfflineCalcGeneralAction_TeachSkill");
        }

        /// <summary>
        /// 应用NPC离线指点技能补丁
        /// 配置项: OfflineCalcGeneralAction_TeachSkill
        /// 功能: 修改NPC离线指点技能时的随机概率检查，使用气运影响成功率
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("OfflineCalcGeneralAction_TeachSkill")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.Character),
                MethodName = "OfflineCalcGeneralAction_TeachSkill",
                Parameters = new Type[] {
                    typeof(GameData.Common.DataContext),
                    typeof(GameData.Domains.Character.ParallelModifications.PeriAdvanceMonthGeneralActionModification),
                    typeof(System.Collections.Generic.HashSet<int>),
                    typeof(System.Collections.Generic.HashSet<int>)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "OfflineCalcGeneralAction_TeachSkill",
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
            // 1-3次 CheckPercentProb 调用都替换为有气运影响的版本
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    2);

            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    3);
        }
    }
}
