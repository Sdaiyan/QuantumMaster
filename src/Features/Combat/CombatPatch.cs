/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;

namespace QuantumMaster.Features.Combat
{
    /// <summary>
    /// 战斗功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class CombatPatch
    {
        /// <summary>
        /// 生活技能比试结果应用补丁
        /// 配置项: ApplyLifeSkillCombatResult
        /// 功能: 修改生活技能比试后读书和练功的概率
        /// </summary>
        public static bool PatchApplyLifeSkillCombatResult(Harmony harmony)
        {
            if (!ConfigManager.ApplyLifeSkillCombatResult && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "DebateGameOver",
                // DataContext context, bool isTaiwuWin, bool isSurrender
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(bool), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "DebateGameOver",
                    OriginalMethod);

            // CheckPercentProb
            // 1 if (readInLifeSkillCombatCount > 0 && currBook.IsValid() && GetTotalReadingProgress(currBook.Id) < 100 && bookCfg.LifeSkillTemplateId >= 0 && context.Random.CheckPercentProb(chanceReading))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            // 2 if (loopInLifeSkillCombatCount > 0 && loopingNeigongTemplateId >= 0 && DomainManager.CombatSkill.TryGetElement_CombatSkills(skillKey, out var skill) && skill.GetObtainedNeili() >= skill.GetTotalObtainableNeili() && context.Random.CheckPercentProb(chanceLooping))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    2);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 战斗中读书计算补丁
        /// 配置项: CalcReadInCombat
        /// 功能: 修改战斗中读书的概率计算
        /// </summary>
        public static bool PatchCalcReadInCombat(Harmony harmony)
        {
            if (!ConfigManager.CalcReadInCombat && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Combat.CombatDomain),
                MethodName = "CalcReadInCombat",
                // DataContext context
                Parameters = new Type[] { typeof(GameData.Common.DataContext) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CalcReadInCombat",
                    OriginalMethod);

            // CheckPercentProb
            // 1 if (context.Random.CheckPercentProb(odds))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 计算战利品补丁
        /// 配置项: CalcLootItem
        /// 功能: 修改战斗后物品掉落的概率计算
        /// </summary>
        public static bool PatchCalcLootItem(Harmony harmony)
        {
            if (!ConfigManager.CalcLootItem && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Combat.CombatDomain),
                MethodName = "CalcLootItem",
                // DataContext context
                Parameters = new Type[] { typeof(GameData.Common.DataContext) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CalcLootItem",
                    OriginalMethod);

            // CheckPercentProb
            // 1 if (context.Random.CheckPercentProb(dropRate))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            // 2 if (context.Random.CheckPercentProb(dropRate2))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    2);

            // 3 if (context.Random.CheckPercentProb(100 - 20 * j))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    3);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 战斗中运气计算补丁
        /// 配置项: CalcQiQrtInCombat
        /// 功能: 修改战斗中运气相关的概率计算
        /// </summary>
        public static bool PatchCalcQiQrtInCombat(Harmony harmony)
        {
            if (!ConfigManager.CalcQiQrtInCombat && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Combat.CombatDomain),
                MethodName = "CalcQiQrtInCombat",
                // DataContext context
                Parameters = new Type[] { typeof(GameData.Common.DataContext) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CalcQiQrtInCombat",
                    OriginalMethod);

            // CheckPercentProb
            // 1 if (context.Random.CheckPercentProb(odds))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
