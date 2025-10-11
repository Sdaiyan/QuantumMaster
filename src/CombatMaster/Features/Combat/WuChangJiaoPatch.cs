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
    /// 无常脚补丁 - 静态上下文版本
    /// 使用CombatPatchBase提供的公共功能
    /// </summary>
    public static class WuChangJiaoPatch
    {        
        /// <summary>
        /// 静态上下文版的无常脚补丁 - 使用PatchBuilder框架
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("WuChangJiao")) return false;

            DebugLog.Info("[WuChangJiaoPatch] 开始应用无常脚威力提高概率补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuehoujiao.Leg.WuChangJiao),
                MethodName = "OnDistanceChanged",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext),
                    typeof(GameData.Domains.Combat.CombatCharacter),
                    typeof(short),
                    typeof(bool),
                    typeof(bool)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "WuChangJiaoOnDistanceChanged",
                OriginalMethod);

            // 替换方法：带静态上下文的Next 2参数版本，期望最大值
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(WuChangJiaoPatch),
                MethodName = nameof(Next2ArgsMaxWithStaticContext)
            };

            DebugLog.Info("[WuChangJiaoPatch] 添加静态上下文替换规则 - 替换第1次Next(int,int)调用");

            // 替换第1次Next(int,int)调用，期望结果为max
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                replacementMethod,
                1);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[WuChangJiaoPatch] 无常脚补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuehoujiao.Leg.WuChangJiao), "OnDistanceChanged")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.SpecialEffect.CombatSkill.Xuehoujiao.Leg.WuChangJiao __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "WuChangJiao");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[WuChangJiaoPatch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuehoujiao.Leg.WuChangJiao), "OnDistanceChanged")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("WuChangJiao");
        }

        /// <summary>
        /// 带静态上下文的Next 2参数版本，倾向最大值（调用基类方法）
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>随机数</returns>
        public static int Next2ArgsMaxWithStaticContext(IRandomSource random, int min, int max)
        {
            return CombatPatchBase.Next2ArgsWithStaticContext(random, min, max, "WuChangJiao", expectMax: true);
        }
    }
}