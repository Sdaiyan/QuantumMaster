/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;

namespace QuantumMaster.Features.Adventure
{
    /// <summary>
    /// 冒险功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class AdventurePatch
    {
        /// <summary>
        /// 初始化路径内容补丁
        /// 配置项: InitPathContent
        /// 功能: 修改冒险路径内容初始化的随机计算，获得更多资源
        /// </summary>
        public static bool PatchInitPathContent(Harmony harmony)
        {
            if (!ConfigManager.InitPathContent && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Adventure.AdventureDomain),
                MethodName = "InitPathContent",
                // DataContext context
                Parameters = new Type[] { typeof(GameData.Common.DataContext) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "InitPathContent",
                    OriginalMethod);

            // 1 resAmount = (short)(context.Random.Next(minAmount, maxAmount) * DomainManager.World.GetGainResourcePercent(4) / 100);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
