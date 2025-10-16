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
    /// 金刀换掌功补丁 - 使用静态上下文
    /// 配置项: JinDaoHuanZhangGong
    /// 功能: 修改金刀换掌功必然化解几率
    /// </summary>
    public static class JinDaoHuanZhangGongPatch
    {
        /// <summary>
        /// 应用金刀换掌功补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("JinDaoHuanZhangGong")) return false;

            DebugLog.Info("[JinDaoHuanZhangGongPatch] 开始应用金刀换掌功补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Yuanshanpai.DefenseAndAssist.JinDaoHuanZhangGong),
                MethodName = "GetModifiedValue",
                Parameters = new Type[] { 
                    typeof(GameData.Domains.SpecialEffect.AffectedDataKey),
                    typeof(bool)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "JinDaoHuanZhangGongGetModifiedValue",
                OriginalMethod);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(JinDaoHuanZhangGongPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[JinDaoHuanZhangGongPatch] 添加静态上下文替换规则 - 替换第1次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用，期望结果为true
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[JinDaoHuanZhangGongPatch] 金刀换掌功补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Yuanshanpai.DefenseAndAssist.JinDaoHuanZhangGong), "GetModifiedValue")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.SpecialEffect.CombatSkill.Yuanshanpai.DefenseAndAssist.JinDaoHuanZhangGong __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "JinDaoHuanZhangGong");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[JinDaoHuanZhangGongPatch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Yuanshanpai.DefenseAndAssist.JinDaoHuanZhangGong), "GetModifiedValue")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("JinDaoHuanZhangGong");
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "JinDaoHuanZhangGong");
        }
    }
}
