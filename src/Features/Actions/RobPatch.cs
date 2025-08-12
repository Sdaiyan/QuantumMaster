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
    /// 抢劫功能补丁
    /// 配置项: rob
    /// 功能: 抢劫必定成功，假如目标是太吾必定失败，如果发起者是太吾则必定成功
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetRobActionPhase")]
    public class RobPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IRandomSource random, GameData.Domains.Character.Character targetChar, int alertFactor, bool showCheckAnim, ref sbyte __result, GameData.Domains.Character.Character __instance)
        {
            if (!ConfigManager.rob) return true;

            return ActionPatchHelper.HandleActionPhase(random, targetChar, alertFactor, showCheckAnim, 2, ref __result, __instance);
        }
    }
}
