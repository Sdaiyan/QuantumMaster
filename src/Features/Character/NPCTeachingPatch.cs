/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;

namespace QuantumMaster.Features.Character
{
    /// <summary>
    /// NPC指点教学功能补丁
    /// 配置项: GetAskToTeachSkillRespondChance, GetTaughtNewSkillSuccessRate
    /// 功能: 控制NPC过月指点概率和接受指点成功率
    /// 注意: 当概率不为0时，必定成功
    /// </summary>
    public class NPCTeachingPatch
    {
        /// <summary>
        /// NPC指点技能响应概率补丁
        /// 配置项: GetAskToTeachSkillRespondChance
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Ai.AiHelper.GeneralActionConstants), "GetAskToTeachSkillRespondChance")]
        public class TeachSkillRespondChancePatch
        {
            [HarmonyPostfix]
            public static void Postfix(ref int __result)
            {
                if (!ConfigManager.GetAskToTeachSkillRespondChance)
                {
                    return; // 使用原版逻辑
                }

                // 如果原概率大于0，则设为100%
                if (__result > 0)
                {
                    DebugLog.Info($"NPC指点概率: 原值{__result}% -> 强制成功100%");
                    __result = 100;
                }
            }
        }

        /// <summary>
        /// NPC学习技能成功率补丁
        /// 配置项: GetTaughtNewSkillSuccessRate
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetTaughtNewSkillSuccessRate")]
        public class TaughtSkillSuccessRatePatch
        {
            [HarmonyPostfix]
            public static void Postfix(ref int __result)
            {
                if (!ConfigManager.GetTaughtNewSkillSuccessRate)
                {
                    return; // 使用原版逻辑
                }

                // 如果原概率大于0，则设为100%
                if (__result > 0)
                {
                    DebugLog.Info($"接受指点成功率: 原值{__result}% -> 强制成功100%");
                    __result = 100;
                }
            }
        }
    }
}
