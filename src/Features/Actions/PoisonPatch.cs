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
    /// 下毒功能补丁
    /// 配置项: poison
    /// 功能: 下毒必定成功，假如目标是太吾必定失败，如果发起者是太吾则必定成功
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetPoisonActionPhase")]
    public class PoisonPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IRandomSource random, GameData.Domains.Character.Character targetChar, int alertFactor, bool showCheckAnim, ref sbyte __result, GameData.Domains.Character.Character __instance)
        {
            if (!ConfigManager.poison) return true;

            return ActionPatchHelper.HandleActionPhase(random, targetChar, alertFactor, showCheckAnim, 3, ref __result, __instance);
        }
    }
}
