/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Combat
{
    /// <summary>
    /// 生活技能比试结果应用功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class ApplyLifeSkillCombatResultPatch
    {
        /// <summary>
        /// 生活技能比试结果功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(ApplyLifeSkillCombatResultPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// 生活技能比试结果功能专用的 CheckPercentProb 替换方法（期望成功）
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "ApplyLifeSkillCombatResult");
        }

        /// <summary>
        /// 应用生活技能比试结果补丁
        /// 配置项: ApplyLifeSkillCombatResult
        /// 功能: 修改生活技能比试后读书和练功的概率
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("ApplyLifeSkillCombatResult")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "DebateGameOver",
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(bool), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "DebateGameOver",
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
            // 1 if (readInLifeSkillCombatCount > 0 && currBook.IsValid() && GetTotalReadingProgress(currBook.Id) < 100 && bookCfg.LifeSkillTemplateId >= 0 && context.Random.CheckPercentProb(chanceReading))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);

            // 2 if (loopInLifeSkillCombatCount > 0 && loopingNeigongTemplateId >= 0 && DomainManager.CombatSkill.TryGetElement_CombatSkills(skillKey, out var skill) && skill.GetObtainedNeili() >= skill.GetTotalObtainableNeili() && context.Random.CheckPercentProb(chanceLooping))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    2);
        }
    }
}
