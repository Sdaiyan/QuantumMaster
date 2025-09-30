/*
 * CombatMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Character;
using Redzen.Random;
using GameData.Utilities;
using System.Collections.Generic;
using System.Reflection.Emit;
using QuantumMaster.Shared;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// 九死离魂手触发秘闻概率增加补丁 - 静态上下文版本
    /// 使用CombatPatchBase提供的公共功能
    /// </summary>
    public static class JiuSiLiHunShouPatch
    {        
        /// <summary>
        /// 静态上下文版的九死离魂手补丁 - 使用PatchBuilder框架
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("JiuSiLiHunShou")) return false;

            DebugLog.Info("[JiuSiLiHunShouPatch] 开始应用九死离魂手触发秘闻概率补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Wuxianjiao.FistAndPalm.JiuSiLiHunShou),
                MethodName = "OnCastMaxPower",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "JiuSiLiHunShouOnCastMaxPower",
                OriginalMethod);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(JiuSiLiHunShouPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[JiuSiLiHunShouPatch] 添加静态上下文替换规则 - 替换第1次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用，期望结果为true
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[JiuSiLiHunShouPatch] 九死离魂手补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Wuxianjiao.FistAndPalm.JiuSiLiHunShou), "OnCastMaxPower")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.SpecialEffect.CombatSkill.Wuxianjiao.FistAndPalm.JiuSiLiHunShou __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "JiuSiLiHunShou");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[JiuSiLiHunShouPatch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Wuxianjiao.FistAndPalm.JiuSiLiHunShou), "OnCastMaxPower")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("JiuSiLiHunShou");
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "JiuSiLiHunShou");
        }
    }
}