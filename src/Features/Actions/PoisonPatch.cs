/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using GameData.Domains.Character;
using GameData.Domains.Taiwu;

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// 下毒功能补丁
    /// 配置项: poison
    /// 功能: 下毒必定成功，针对太吾角色进行概率修改
    /// </summary>
    public static class PoisonPatch
    {
        /// <summary>
        /// 下毒方法前缀：设置角色上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetPoisonActionPhase")]
        [HarmonyPrefix]
        public static void GetPoisonActionPhase_Prefix(GameData.Domains.Character.Character __instance, GameData.Domains.Character.Character targetChar)
        {
            if (!ConfigManager.poison && !QuantumMaster.openAll) return;
            ActionPatchBase.SetCharacterContext(__instance, targetChar, "PoisonPatch");
        }

        /// <summary>
        /// 下毒方法后缀：清理角色上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetPoisonActionPhase")]
        [HarmonyPostfix]
        public static void GetPoisonActionPhase_Postfix()
        {
            if (!ConfigManager.poison && !QuantumMaster.openAll) return;
            ActionPatchBase.ClearCharacterContext("PoisonPatch");
        }

        /// <summary>
        /// 检查概率，使用静态上下文判断
        /// </summary>
        public static bool CheckPercentProbWithStaticContext(Redzen.Random.IRandomSource random, int probability)
        {
            return ActionPatchBase.CheckPercentProbWithStaticContext(random, probability, "PoisonPatch");
        }

        /// <summary>
        /// 应用下毒补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool PatchGetPoisonActionPhase(Harmony harmony)
        {
            if (!ConfigManager.poison && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.Character),
                MethodName = "GetPoisonActionPhase",
                Parameters = new Type[] { typeof(Redzen.Random.IRandomSource), typeof(GameData.Domains.Character.Character), typeof(int), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "GetPoisonActionPhase",
                OriginalMethod);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(PoisonPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            // CheckPercentProb 方法替换 - 第1-5次调用都使用静态上下文判断
            for (int i = 1; i <= 5; i++)
            {
                patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    replacementMethod,
                    i);
            }

            patchBuilder.Apply(harmony);
            return true;
        }
    }
}
