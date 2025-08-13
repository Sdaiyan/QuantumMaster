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
    /// 下毒功能补丁
    /// 配置项: poison
    /// 功能: 下毒必定成功，通过修改 CheckPercentProb 的第1-5次调用返回 true
    /// </summary>
    public static class PoisonPatch
    {
        /// <summary>
        /// 应用下毒补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool PatchGetPoisonActionPhase(Harmony harmony)
        {
            if (!ConfigManager.poison && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.Character),
                MethodName = "GetPoisonActionPhase",
                Parameters = new Type[] { typeof(Redzen.Random.IRandomSource), typeof(GameData.Domains.Character.Character), typeof(int), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "GetPoisonActionPhase",
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
