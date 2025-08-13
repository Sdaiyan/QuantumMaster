/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Character;
using Redzen.Random;
using GameData.Utilities;

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// 抢劫功能补丁 - 静态上下文版本
    /// 使用ActionPatchBase提供的公共功能
    /// </summary>
    public static class RobPatch
    {        
        /// <summary>
        /// 静态上下文版的抢劫补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool PatchGetRobActionPhase(Harmony harmony)
        {
            if (!ConfigManager.rob && !QuantumMaster.openAll) return false;

            DebugLog.Info("[RobPatch] 开始应用静态上下文版抢劫补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.Character),
                MethodName = "GetRobActionPhase",
                Parameters = new Type[] { typeof(IRandomSource), typeof(GameData.Domains.Character.Character), typeof(int), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "GetRobActionPhaseStatic",
                OriginalMethod);

            // 目标方法：CheckPercentProb
            var targetMethod = new PatchBuilder.TargetMethodInfo
            {
                Type = typeof(RedzenHelper),
                MethodName = "CheckPercentProb",
                Parameters = new Type[] { typeof(IRandomSource), typeof(int) }
            };

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(RobPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[RobPatch] 添加静态上下文替换规则 - 替换所有5次调用");

            // 替换所有5次CheckPercentProb调用
            for (int i = 1; i <= 5; i++)
            {
                patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    replacementMethod,
                    i);
                
                DebugLog.Info($"[RobPatch] 添加第{i}次调用替换规则");
            }

            patchBuilder.Apply(harmony);

            DebugLog.Info("[RobPatch] 静态上下文版抢劫补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色和目标角色到静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetRobActionPhase")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.Character.Character __instance, GameData.Domains.Character.Character targetChar)
        {
            ActionPatchBase.SetCharacterContext(__instance, targetChar, "RobPatch");
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetRobActionPhase")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            ActionPatchBase.ClearCharacterContext("RobPatch");
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return ActionPatchBase.CheckPercentProbWithStaticContext(random, probability, "RobPatch");
        }
    }
}
