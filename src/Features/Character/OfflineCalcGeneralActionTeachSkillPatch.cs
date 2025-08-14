/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;

namespace QuantumMaster.Features.Character
{
    /// <summary>
    /// NPC指点教学功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class OfflineCalcGeneralActionTeachSkillPatch
    {
        /// <summary>
        /// NPC离线指点技能补丁
        /// 配置项: OfflineCalcGeneralAction_TeachSkill
        /// 功能: 修改NPC离线指点技能时的随机概率检查，使用气运影响成功率
        /// </summary>
        public static bool PatchOfflineCalcGeneralActionTeachSkill(Harmony harmony)
        {
            if (!ConfigManager.OfflineCalcGeneralAction_TeachSkill && !QuantumMaster.openAll) return false;

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

            // CheckPercentProb 方法替换 - 期望成功，使用气运提高成功率
            // 1-3次 CheckPercentProb 调用都替换为有气运影响的版本
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    2);

            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    3);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
