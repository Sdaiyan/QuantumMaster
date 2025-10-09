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
    /// 漫天花雨式额外封穴/破绽功能补丁 - 静态上下文版本
    /// 使用CombatPatchBase提供的公共功能
    /// </summary>
    public static class ManTianHuaYuShiPatch
    {        
        /// <summary>
        /// 静态上下文版的漫天花雨式补丁 - 使用PatchBuilder框架
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("ManTianHuaYuShi")) return false;

            DebugLog.Info("[ManTianHuaYuShiPatch] 开始应用漫天花雨式额外封穴/破绽补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Baihuagu.Finger.ManTianHuaYuShi),
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
                "ManTianHuaYuShiOnCastSkillEnd",
                OriginalMethod);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(ManTianHuaYuShiPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[ManTianHuaYuShiPatch] 添加静态上下文替换规则 - 替换第1次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用，期望结果为true（额外封穴/破绽）
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[ManTianHuaYuShiPatch] 漫天花雨式补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色到静态上下文
        /// 通过__instance.CharacterId获取角色ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Baihuagu.Finger.ManTianHuaYuShi), "OnCastSkillEnd")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.SpecialEffect.CombatSkill.Baihuagu.Finger.ManTianHuaYuShi __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "ManTianHuaYuShi");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[ManTianHuaYuShiPatch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Baihuagu.Finger.ManTianHuaYuShi), "OnCastSkillEnd")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("ManTianHuaYuShi");
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "ManTianHuaYuShi");
        }
    }
}
