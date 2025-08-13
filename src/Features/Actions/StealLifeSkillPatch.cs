/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// 偷学生活技能功能补丁
    /// 配置项: stealLifeSkill
    /// 功能: 偷学生活技能必定成功，通过修改 CheckPercentProb 的第1-5次调用返回 true
    /// </summary>
    public static class StealLifeSkillPatch
    {
        /// <summary>
        /// 应用偷学生活技能补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool PatchGetStealLifeSkillActionPhase(Harmony harmony)
        {
            if (!ConfigManager.stealLifeSkill && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.Character),
                MethodName = "GetStealLifeSkillActionPhase",
                Parameters = new Type[] { typeof(Redzen.Random.IRandomSource), typeof(GameData.Domains.Character.Character), typeof(sbyte), typeof(sbyte), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "GetStealLifeSkillActionPhase",
                OriginalMethod);

            // CheckPercentProb 方法替换 - 第1-5次调用都返回 true
            // 1 if (random.CheckPercentProb(...))
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                PatchPresets.Replacements.CheckPercentProbTrue,
                1);

            // 2 if (random.CheckPercentProb(...))
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                PatchPresets.Replacements.CheckPercentProbTrue,
                2);

            // 3 if (random.CheckPercentProb(...))
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                PatchPresets.Replacements.CheckPercentProbTrue,
                3);

            // 4 if (random.CheckPercentProb(...))
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                PatchPresets.Replacements.CheckPercentProbTrue,
                4);

            // 5 if (random.CheckPercentProb(...))
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                PatchPresets.Replacements.CheckPercentProbTrue,
                5);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
