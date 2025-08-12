/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using GameData.Domains.Character;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// 偷学生活技能功能补丁
    /// 配置项: stealLifeSkill
    /// 功能: 偷学生活技能必定成功，假如目标是太吾必定失败，如果发起者是太吾则必定成功
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetStealLifeSkillActionPhase")]
    public class StealLifeSkillPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IRandomSource random, GameData.Domains.Character.Character targetChar, sbyte lifeSkillType, sbyte grade, bool showCheckAnim, ref sbyte __result, GameData.Domains.Character.Character __instance)
        {
            if (!ConfigManager.stealLifeSkill) return true;

            return ActionPatchHelper.HandleActionPhase(random, targetChar, targetChar.GetGradeAlertFactor(grade, 1), showCheckAnim, 6, ref __result, __instance);
        }
    }
}
