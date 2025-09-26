/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Character;
using Redzen.Random;
using GameData.Utilities;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// 唬骗功能补丁 - 静态上下文版本
    /// 使用静态上下文获取角色信息，区分太吾和其他角色的唬骗成功率
    /// </summary>
    public static class ScamPatch
    {
        // 静态字段存储当前执行的角色信息
        private static GameData.Domains.Character.Character _currentCharacter;
        private static GameData.Domains.Character.Character _targetCharacter;
        
        /// <summary>
        /// 静态上下文版的唬骗补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool PatchGetScamActionPhase(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("scam")) return false;

            DebugLog.Info("[ScamPatch] 开始应用静态上下文版唬骗补丁");

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.Character),
                MethodName = "GetScamActionPhase",
                Parameters = new Type[] { typeof(IRandomSource), typeof(GameData.Domains.Character.Character), typeof(int), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "GetScamActionPhaseStatic",
                OriginalMethod);

            // 目标方法：CheckPercentProb
            var targetMethod = new PatchBuilder.TargetMethodInfo
            {
                Type = typeof(RedzenHelper),
                MethodName = "CheckPercentProb",
                Parameters = new Type[] { typeof(IRandomSource), typeof(int) }
            };

            // 替换方法：带静态上下文的概率检查
            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(ScamPatch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            DebugLog.Info("[ScamPatch] 添加静态上下文替换规则 - 替换所有5次调用");

            // 替换所有5次CheckPercentProb调用
            for (int i = 1; i <= 5; i++)
            {
                patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    replacementMethod,
                    i);
                
                DebugLog.Info($"[ScamPatch] 添加第{i}次调用替换规则");
            }

            patchBuilder.Apply(harmony);

            DebugLog.Info("[ScamPatch] 静态上下文版唬骗补丁应用完成");
            return true;
        }

        /// <summary>
        /// Prefix方法 - 设置当前角色和目标角色到静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetScamActionPhase")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(GameData.Domains.Character.Character __instance, GameData.Domains.Character.Character targetChar)
        {
            _currentCharacter = __instance;
            _targetCharacter = targetChar;
            DebugLog.Info($"[scam] 设置角色上下文 - 当前角色: {(_currentCharacter != null ? _currentCharacter.GetId().ToString() : "NULL")}, 目标角色: {(_targetCharacter != null ? _targetCharacter.GetId().ToString() : "NULL")}");
        }

        /// <summary>
        /// Postfix方法 - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetScamActionPhase")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            DebugLog.Info($"[scam] 清理角色上下文");
            _currentCharacter = null;
            _targetCharacter = null;
        }

        /// <summary>
        /// 带静态上下文的概率检查方法
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            DebugLog.Info($"[ScamPatch] === 静态上下文版方法被调用 ===");
            DebugLog.Info($"[ScamPatch] 原始概率: {probability}");
            
            var currentChar = _currentCharacter;
            var targetChar = _targetCharacter;
            
            if (currentChar != null && targetChar != null)
            {
                var currentCharId = currentChar.GetId();
                var targetCharId = targetChar.GetId();
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                DebugLog.Info($"[ScamPatch] 当前角色ID: {currentCharId}, 目标角色ID: {targetCharId}, 太吾ID: {taiwuId}");
                
                // 如果是太吾发起唬骗，使用气运加成
                if (currentCharId == taiwuId)
                {
                    DebugLog.Info($"[ScamPatch] 太吾发起唬骗（目标：{targetCharId}）- 使用倾向成功的气运函数");
                    var result = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(random, probability, "scam");
                    DebugLog.Info($"[ScamPatch] 太吾唬骗结果: {result}");
                    return result;
                }
                else
                {
                    DebugLog.Info($"[ScamPatch] 非太吾发起唬骗（{currentCharId} -> {targetCharId}）- 使用原始概率逻辑");
                    var result = RedzenHelper.CheckPercentProb(random, probability);
                    DebugLog.Info($"[ScamPatch] 非太吾唬骗结果: {result}");
                    return result;
                }
            }
            else
            {
                DebugLog.Warning($"[ScamPatch] 静态上下文中缺少角色信息 - 当前角色: {(currentChar != null ? "有效" : "NULL")}, 目标角色: {(targetChar != null ? "有效" : "NULL")}，使用原始概率");
                return RedzenHelper.CheckPercentProb(random, probability);
            }
        }
    }
}
