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
        /// 带静态上下文的Next 2参数版本
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级和日志输出</param>
        /// <param name="expectMax">期望最大值还是最小值，默认为 true（期望最大值）</param>
        /// <returns>随机数</returns>
        public static int Next2ArgsWithStaticContext(IRandomSource random, int min, int max, string featureKey, bool expectMax = true)
        {
            if (_currentCharacterId != 0)
            {
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                // 如果是太吾，使用气运加成
                if (_currentCharacterId == taiwuId)
                {
                    int result;
                    if (expectMax)
                    {
                        DebugLog.Info($"[{featureKey}] 太吾执行{featureKey} - 使用倾向最大值的气运函数");
                        result = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, featureKey);
                    }
                    else
                    {
                        DebugLog.Info($"[{featureKey}] 太吾执行{featureKey} - 使用倾向最小值的气运函数");
                        result = LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(min, max, featureKey);
                    }
                    DebugLog.Info($"[{featureKey}] 太吾{featureKey}结果: {result} (范围{min}-{max}, 期望{(expectMax ? "最大值" : "最小值")})");
                    return result;
                }
                else
                {
                    DebugLog.Info($"[{featureKey}] 非太吾角色({_currentCharacterId})执行{featureKey} - 使用原始随机数");
                    return random.Next(min, max);
                }
            }
            else
            {
                DebugLog.Warning($"[{featureKey}] 静态上下文中缺少角色信息 - 当前角色ID: {_currentCharacterId}，使用原始随机数");
                return random.Next(min, max);
            }
        }

        /// <summary>
        /// 带静态上下文的Next 1参数版本
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="max">最大值</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级和日志输出</param>
        /// <param name="expectMax">期望最大值还是0，默认为 true（期望最大值）</param>
        /// <returns>随机数</returns>
        public static int Next1ArgWithStaticContext(IRandomSource random, int max, string featureKey, bool expectMax = true)
        {
            if (_currentCharacterId != 0)
            {
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                // 如果是太吾，使用气运加成
                if (_currentCharacterId == taiwuId)
                {
                    int result;
                    if (expectMax)
                    {
                        DebugLog.Info($"[{featureKey}] 太吾执行{featureKey} - 使用倾向最大值的气运函数");
                        result = LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max, featureKey);
                    }
                    else
                    {
                        DebugLog.Info($"[{featureKey}] 太吾执行{featureKey} - 使用倾向0的气运函数");
                        result = LuckyCalculator.Calc_Random_Next_1Arg_0_By_Luck(max, featureKey);
                    }
                    DebugLog.Info($"[{featureKey}] 太吾{featureKey}结果: {result} (范围0-{max}, 期望{(expectMax ? "最大值" : "0")})");
                    return result;
                }
                else
                {
                    DebugLog.Info($"[{featureKey}] 非太吾角色({_currentCharacterId})执行{featureKey} - 使用原始随机数");
                    return random.Next(max);
                }
            }
            else
            {
                DebugLog.Warning($"[{featureKey}] 静态上下文中缺少角色信息 - 当前角色ID: {_currentCharacterId}，使用原始随机数");
                return random.Next(max);
            }
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