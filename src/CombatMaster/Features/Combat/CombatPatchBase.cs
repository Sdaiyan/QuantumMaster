/*
 * CombatMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using GameData.Domains.Character;
using Redzen.Random;
using GameData.Utilities;
using QuantumMaster.Shared;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// Combat补丁基类 - 提供静态上下文功能的公共实现
    /// 专用于战斗相关功能，只处理单一角色上下文
    /// </summary>
    public static class CombatPatchBase
    {
        // 静态字段存储当前执行的角色ID
        private static int _currentCharacterId;

        /// <summary>
        /// 设置角色上下文（单角色版本）
        /// </summary>
        /// <param name="currentCharId">当前角色ID</param>
        /// <param name="featureKey">功能键，用于日志</param>
        public static void SetCharacterContext(int currentCharId, string featureKey)
        {
            _currentCharacterId = currentCharId;
            DebugLog.Info($"[{featureKey}] 设置角色上下文 - 当前角色ID: {_currentCharacterId}");
        }

        /// <summary>
        /// 清理角色上下文（单角色版本）
        /// </summary>
        /// <param name="featureKey">功能键，用于日志</param>
        public static void ClearCharacterContext(string featureKey)
        {
            DebugLog.Info($"[{featureKey}] 清理角色上下文");
            _currentCharacterId = 0;
        }

        /// <summary>
        /// 带静态上下文的概率检查方法（单角色版本）
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级和日志输出</param>
        /// <param name="expectSuccess">期望成功还是失败，默认为 true（期望成功）</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability, string featureKey, bool expectSuccess = true)
        {
            if (_currentCharacterId != 0)
            {
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                // 如果是太吾，使用气运加成
                if (_currentCharacterId == taiwuId)
                {
                    bool result;
                    if (expectSuccess)
                    {
                        DebugLog.Info($"[{featureKey}] 太吾执行{featureKey} - 使用倾向成功的气运函数");
                        result = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(random, probability, featureKey);
                    }
                    else
                    {
                        DebugLog.Info($"[{featureKey}] 太吾执行{featureKey} - 使用倾向失败的气运函数");
                        result = LuckyCalculator.Calc_Random_CheckPercentProb_False_By_Luck(random, probability, featureKey);
                    }
                    DebugLog.Info($"[{featureKey}] 太吾{featureKey}结果: {result} (期望{(expectSuccess ? "成功" : "失败")})");
                    return result;
                }
                else
                {
                    DebugLog.Info($"[{featureKey}] 非太吾角色({_currentCharacterId})执行{featureKey} - 使用原始概率");
                    var result = RedzenHelper.CheckPercentProb(random, probability);
                    return result;
                }
            }
            else
            {
                DebugLog.Warning($"[{featureKey}] 静态上下文中缺少角色信息 - 当前角色ID: {_currentCharacterId}，使用原始概率");
                return RedzenHelper.CheckPercentProb(random, probability);
            }
        }
    }
}