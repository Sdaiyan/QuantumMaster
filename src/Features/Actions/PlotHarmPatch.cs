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
    /// 暗害功能补丁
    /// 配置项: plotHarm
    /// 功能: 暗害必定成功，通过修改 CheckPercentProb 的第1-5次调用返回 true
    /// </summary>
    public static class PlotHarmPatch
    {
        /// <summary>
        /// 应用暗害补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool PatchGetPlotHarmActionPhase(Harmony harmony)
        {
            if (!ConfigManager.plotHarm && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.Character),
                MethodName = "GetPlotHarmActionPhase",
                Parameters = new Type[] { typeof(Redzen.Random.IRandomSource), typeof(GameData.Domains.Character.Character), typeof(int), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "GetPlotHarmActionPhase",
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
