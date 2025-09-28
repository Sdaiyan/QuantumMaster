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
    /// 心无定意获得式或者减少式消耗的效果生效概率增加补丁 - 静态上下文版本
    /// 使用CombatPatchBase提供的公共功能
    /// </summary>
    public static class XinWuDingYiPatch
    {        
        /// <summary>
        /// 静态上下文版的心无定意补丁 - 使用PatchBuilder框架
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("XinWuDingYi")) return false;

            DebugLog.Info("[XinWuDingYiPatch] 开始应用心无定意效果生效概率补丁");

            // 第一个方法：OnGetTrick
            var originalMethodOnGetTrick = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist.XinWuDingYi),
                MethodName = "OnGetTrick",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(int), 
                    typeof(bool), 
                    typeof(sbyte), 
                    typeof(bool) 
                }
            };

            var patchBuilderOnGetTrick = GenericTranspiler.CreatePatchBuilder(
                "XinWuDingYiOnGetTrick",
                originalMethodOnGetTrick);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(XinWuDingYiPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[XinWuDingYiPatch] 添加OnGetTrick静态上下文替换规则 - 替换第1次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用，期望结果为true
            patchBuilderOnGetTrick.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilderOnGetTrick.Apply(harmony);

            // 第二个方法：GetModifiedValue
            var originalMethodGetModifiedValue = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist.XinWuDingYi),
                MethodName = "GetModifiedValue",
                Parameters = new Type[] { 
                    typeof(GameData.Domains.SpecialEffect.AffectedDataKey), 
                    typeof(System.Collections.Generic.List<Config.NeedTrick>) 
                }
            };

            var patchBuilderGetModifiedValue = GenericTranspiler.CreatePatchBuilder(
                "XinWuDingYiGetModifiedValue",
                originalMethodGetModifiedValue);

            DebugLog.Info("[XinWuDingYiPatch] 添加GetModifiedValue静态上下文替换规则 - 替换第1次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用，期望结果为true
            patchBuilderGetModifiedValue.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilderGetModifiedValue.Apply(harmony);

            DebugLog.Info("[XinWuDingYiPatch] 心无定意补丁应用完成");
            return true;
        }

        /// <summary>
        /// OnGetTrick Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist.XinWuDingYi), "OnGetTrick")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefixOnGetTrick(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist.XinWuDingYi __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "XinWuDingYi");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[XinWuDingYiPatch] OnGetTrick设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// OnGetTrick Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist.XinWuDingYi), "OnGetTrick")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfixOnGetTrick()
        {
            CombatPatchBase.ClearCharacterContext("XinWuDingYi");
        }

        /// <summary>
        /// GetModifiedValue Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist.XinWuDingYi), "GetModifiedValue")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefixGetModifiedValue(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist.XinWuDingYi __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "XinWuDingYi");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[XinWuDingYiPatch] GetModifiedValue设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// GetModifiedValue Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist.XinWuDingYi), "GetModifiedValue")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfixGetModifiedValue()
        {
            CombatPatchBase.ClearCharacterContext("XinWuDingYi");
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "XinWuDingYi");
        }
    }
}