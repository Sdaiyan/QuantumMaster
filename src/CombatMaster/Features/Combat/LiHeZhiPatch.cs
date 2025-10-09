/*
 * CombatMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// 离合指反噬功能补丁 - 静态上下文版本
    /// 使用CombatPatchBase提供的公共功能
    /// </summary>
    public static class LiHeZhiPatch
    {        
        /// <summary>
        /// 静态上下文版的离合指补丁 - 使用PatchBuilder框架
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("LiHeZhi")) return false;

            DebugLog.Info("[LiHeZhiPatch] 开始应用离合指反噬补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Ranshanpai.Finger.LiHeZhi),
                MethodName = "DoAffect",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "LiHeZhiDoAffect",
                OriginalMethod);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(LiHeZhiPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[LiHeZhiPatch] 添加静态上下文替换规则 - 替换第1次和第2次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用，期望结果为true（反噬）
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            // 替换第2次CheckPercentProb调用，期望结果为true（反噬）
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                2);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[LiHeZhiPatch] 离合指补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Ranshanpai.Finger.LiHeZhi), "DoAffect")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.SpecialEffect.CombatSkill.Ranshanpai.Finger.LiHeZhi __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "LiHeZhi");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[LiHeZhiPatch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Ranshanpai.Finger.LiHeZhi), "DoAffect")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("LiHeZhi");
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "LiHeZhi");
        }
    }
}
