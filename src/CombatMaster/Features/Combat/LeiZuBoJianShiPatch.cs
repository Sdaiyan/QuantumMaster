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
    /// 嫘祖剥茧式脱装备概率增加补丁 - 静态上下文版本
    /// 使用CombatPatchBase提供的公共功能
    /// </summary>
    public static class LeiZuBoJianShiPatch
    {        
        /// <summary>
        /// 静态上下文版的嫘祖剥茧式补丁 - 使用PatchBuilder框架
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("LeiZuBoJianShi")) return false;

            DebugLog.Info("[LeiZuBoJianShiPatch] 开始应用嫘祖剥茧式脱装备概率补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi),
                MethodName = "OnCastSkillEnd",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(int), 
                    typeof(bool), 
                    typeof(short), 
                    typeof(sbyte), 
                    typeof(bool) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "LeiZuBoJianShiOnCastSkillEnd",
                OriginalMethod);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(LeiZuBoJianShiPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[LeiZuBoJianShiPatch] 添加静态上下文替换规则 - 替换第1次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用，期望结果为true
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[LeiZuBoJianShiPatch] 嫘祖剥茧式补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi), "OnCastSkillEnd")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "LeiZuBoJianShi");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[LeiZuBoJianShiPatch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi), "OnCastSkillEnd")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("LeiZuBoJianShi");
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "LeiZuBoJianShi");
        }
    }
}