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
    /// 偷学战斗技能功能补丁
    /// 配置项: stealCombatSkill
    /// 功能: 偷学战斗技能必定成功，假如目标是太吾必定失败，如果发起者是太吾则必定成功
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetStealCombatSkillActionPhase")]
    public class StealCombatSkillPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IRandomSource random, GameData.Domains.Character.Character targetChar, sbyte combatSkillType, sbyte grade, bool showCheckAnim, ref sbyte __result, GameData.Domains.Character.Character __instance)
        {
            if (!ConfigManager.stealCombatSkill) return true;

            return ActionPatchHelper.HandleActionPhase(random, targetChar, targetChar.GetGradeAlertFactor(grade, 1), showCheckAnim, 7, ref __result, __instance);
        }
    }
}
