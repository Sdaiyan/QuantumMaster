/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.TaiwuEvent.DisplayEvent;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// 偷窃功能补丁
    /// 配置项: steal
    /// 功能: 偷窃必定成功，假如目标是太吾必定失败，如果发起者是太吾则必定成功
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetStealActionPhase")]
    public class StealPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IRandomSource random, GameData.Domains.Character.Character targetChar, int alertFactor, bool showCheckAnim, ref sbyte __result, GameData.Domains.Character.Character __instance)
        {
            if (!ConfigManager.steal) return true;

            return ActionPatchHelper.HandleActionPhase(random, targetChar, alertFactor, showCheckAnim, 1, ref __result, __instance);
        }
    }
}
