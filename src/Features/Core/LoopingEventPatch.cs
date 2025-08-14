/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Taiwu;

namespace QuantumMaster.Features.Core
{
    /// <summary>
    /// 天人感应功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class LoopingEventPatch
    {
        /// <summary>
        /// 天人感应触发补丁
        /// 配置项: TryAddLoopingEvent
        /// 功能: 【气运】根据气运影响天人感应的触发概率
        /// </summary>
        public static bool PatchTryAddLoopingEvent(Harmony harmony)
        {
            if (!ConfigManager.TryAddLoopingEvent && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "TryAddLoopingEvent",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(int) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "TryAddLoopingEvent",
                    OriginalMethod);

            // CheckPercentProb 方法替换 - 期望成功，使用气运提高成功率
            // 第1个 CheckPercentProb 调用
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
