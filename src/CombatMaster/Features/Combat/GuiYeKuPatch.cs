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
    /// 鬼夜哭补丁 - 静态上下文版本
    /// 使用CombatPatchBase提供的公共功能
    /// </summary>
    public static class GuiYeKuPatch
    {        
        /// <summary>
        /// 静态上下文版的鬼夜哭补丁 - 使用PatchBuilder框架
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("GuiYeKu")) return false;

            DebugLog.Info("[GuiYeKuPatch] 开始应用鬼夜哭反击特效触发概率补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuehoujiao.DefenseAndAssist.GuiYeKu),
                MethodName = "OnNormalAttackEnd",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext),
                    typeof(GameData.Domains.Combat.CombatCharacter),
                    typeof(GameData.Domains.Combat.CombatCharacter),
                    typeof(sbyte),
                    typeof(int),
                    typeof(bool),
                    typeof(bool)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "GuiYeKuOnNormalAttackEnd",
                OriginalMethod);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(GuiYeKuPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[GuiYeKuPatch] 添加静态上下文替换规则 - 替换第1次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用，期望结果为true
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[GuiYeKuPatch] 鬼夜哭补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuehoujiao.DefenseAndAssist.GuiYeKu), "OnNormalAttackEnd")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.SpecialEffect.CombatSkill.Xuehoujiao.DefenseAndAssist.GuiYeKu __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "GuiYeKu");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[GuiYeKuPatch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuehoujiao.DefenseAndAssist.GuiYeKu), "OnNormalAttackEnd")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("GuiYeKu");
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "GuiYeKu");
        }
    }
}
