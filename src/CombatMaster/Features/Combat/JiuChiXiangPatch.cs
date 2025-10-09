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
    /// 九痴香功法打断功能补丁 - 静态上下文版本
    /// 使用CombatPatchBase提供的公共功能
    /// 根据 IsDirect 决定期望结果：true 期望成功（打断敌人），false 期望失败（不被打断）
    /// </summary>
    public static class JiuChiXiangPatch
    {
        // 静态字段存储 IsDirect 状态
        private static bool _isDirect;

        /// <summary>
        /// 静态上下文版的九痴香补丁 - 使用PatchBuilder框架
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("JiuChiXiang")) return false;

            DebugLog.Info("[JiuChiXiangPatch] 开始应用九痴香功法打断补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Kongsangpai.Throw.JiuChiXiang),
                MethodName = "OnPrepareSkillProgressChange",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(int), 
                    typeof(bool), 
                    typeof(short), 
                    typeof(sbyte)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "JiuChiXiangOnPrepareSkillProgressChange",
                OriginalMethod);

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(JiuChiXiangPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[JiuChiXiangPatch] 添加静态上下文替换规则 - 替换第1次CheckPercentProb调用");

            // 替换第1次CheckPercentProb调用
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilder.Apply(harmony);

            DebugLog.Info("[JiuChiXiangPatch] 九痴香补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色和 IsDirect 到静态上下文
        /// 通过__instance.CharacterId和__instance.IsDirect获取
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Kongsangpai.Throw.JiuChiXiang), "OnPrepareSkillProgressChange")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.SpecialEffect.CombatSkill.Kongsangpai.Throw.JiuChiXiang __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                _isDirect = __instance.IsDirect;
                CombatPatchBase.SetCharacterContext(charId, "JiuChiXiang");
                DebugLog.Info($"[JiuChiXiang] 设置上下文 - CharId: {charId}, IsDirect: {_isDirect}");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[JiuChiXiangPatch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Kongsangpai.Throw.JiuChiXiang), "OnPrepareSkillProgressChange")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("JiuChiXiang");
            _isDirect = false;
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// 根据 IsDirect 决定期望结果：
        /// - IsDirect = true: 期望成功（true），打断敌人功法
        /// - IsDirect = false: 期望失败（false），避免自己被打断
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            // 根据 IsDirect 决定期望结果
            bool expectSuccess = _isDirect;
            
            DebugLog.Info($"[JiuChiXiang] CheckPercentProb调用 - IsDirect: {_isDirect}, 期望结果: {(expectSuccess ? "成功" : "失败")}");
            
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "JiuChiXiang", expectSuccess);
        }
    }
}
