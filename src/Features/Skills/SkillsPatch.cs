/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Skills
{
    /// <summary>
    /// 技能功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class SkillsPatch
    {
        /// <summary>
        /// 内功周天效果计算补丁
        /// 配置项: CalcNeigongLoopingEffect
        /// 功能: 修改内功周天效果计算，获得更多内力和更少走火入魔
        /// </summary>
        public static bool PatchCalcNeigongLoopingEffect(Harmony harmony)
        {
            if (!ConfigManager.CalcNeigongLoopingEffect && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.CombatSkill.CombatSkillDomain),
                MethodName = "CalcNeigongLoopingEffect",
                // IRandomSource random, GameData.Domains.Character.Character character, CombatSkillItem skillCfg, bool includeReference = true
                Parameters = new Type[] { typeof(IRandomSource), typeof(GameData.Domains.Character.Character), typeof(Config.CombatSkillItem), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CalcNeigongLoopingEffect",
                    OriginalMethod);

            // 1 neili = random.Next(neiliMin, neiliMax + 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    1);

            // 1 qiDisorder = (short)random.Next(qiDisorder + 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    PatchPresets.Replacements.Next1Arg0,
                    1);

            // 2 qiDisorder = (short)(-random.Next(-qiDisorder + 1));
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    PatchPresets.Replacements.Next1ArgMax,
                    2);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
