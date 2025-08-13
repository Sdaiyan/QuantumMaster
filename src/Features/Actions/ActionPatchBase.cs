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

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// Action补丁基类 - 提供静态上下文功能的公共实现
    /// </summary>
    public static class ActionPatchBase
    {
        // 静态字段存储当前执行的角色信息
        private static GameData.Domains.Character.Character _currentCharacter;
        private static GameData.Domains.Character.Character _targetCharacter;

        /// <summary>
        /// 设置角色上下文（通用方法）
        /// </summary>
        /// <param name="currentChar">当前角色</param>
        /// <param name="targetChar">目标角色</param>
        /// <param name="actionName">行动名称，用于日志</param>
        public static void SetCharacterContext(GameData.Domains.Character.Character currentChar, GameData.Domains.Character.Character targetChar, string actionName)
        {
            _currentCharacter = currentChar;
            _targetCharacter = targetChar;
            DebugLog.Info($"[{actionName}] 设置角色上下文 - 当前角色: {(_currentCharacter != null ? _currentCharacter.GetId().ToString() : "NULL")}, 目标角色: {(_targetCharacter != null ? _targetCharacter.GetId().ToString() : "NULL")}");
        }

        /// <summary>
        /// 清理角色上下文（通用方法）
        /// </summary>
        /// <param name="actionName">行动名称，用于日志</param>
        public static void ClearCharacterContext(string actionName)
        {
            DebugLog.Info($"[{actionName}] 清理角色上下文");
            _currentCharacter = null;
            _targetCharacter = null;
        }

        /// <summary>
        /// 带静态上下文的概率检查方法（通用实现）
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="probability">原始概率</param>
        /// <param name="actionName">行动名称，用于日志</param>
        /// <returns>是否成功</returns>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability, string actionName)
        {
            DebugLog.Info($"[{actionName}] === 静态上下文版方法被调用 ===");
            DebugLog.Info($"[{actionName}] 原始概率: {probability}");
            
            var currentChar = _currentCharacter;
            var targetChar = _targetCharacter;
            
            if (currentChar != null && targetChar != null)
            {
                var currentCharId = currentChar.GetId();
                var targetCharId = targetChar.GetId();
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                DebugLog.Info($"[{actionName}] 当前角色ID: {currentCharId}, 目标角色ID: {targetCharId}, 太吾ID: {taiwuId}");
                
                // 如果是太吾发起行动，使用气运加成
                if (currentCharId == taiwuId)
                {
                    DebugLog.Info($"[{actionName}] 太吾发起{actionName}（目标：{targetCharId}）- 使用倾向成功的气运函数");
                    var result = LuckyRandomHelper.Calc_Random_CheckPercentProb_True_By_Luck(random, probability);
                    DebugLog.Info($"[{actionName}] 太吾{actionName}结果: {result}");
                    return result;
                }
                // 如果目标是太吾，使用气运减成（对太吾不利）
                else if (targetCharId == taiwuId)
                {
                    DebugLog.Info($"[{actionName}] 针对太吾的{actionName}（{currentCharId} -> 太吾）- 使用倾向失败的气运函数（对太吾不利）");
                    var result = LuckyRandomHelper.Calc_Random_CheckPercentProb_False_By_Luck(random, probability);
                    DebugLog.Info($"[{actionName}] 针对太吾{actionName}结果: {result}");
                    return result;
                }
                else
                {
                    DebugLog.Info($"[{actionName}] 非太吾相关{actionName}（{currentCharId} -> {targetCharId}）- 使用原始概率逻辑");
                    var result = RedzenHelper.CheckPercentProb(random, probability);
                    DebugLog.Info($"[{actionName}] 非太吾相关{actionName}结果: {result}");
                    return result;
                }
            }
            else
            {
                DebugLog.Warning($"[{actionName}] 静态上下文中缺少角色信息 - 当前角色: {(currentChar != null ? "有效" : "NULL")}, 目标角色: {(targetChar != null ? "有效" : "NULL")}，使用原始概率");
                return RedzenHelper.CheckPercentProb(random, probability);
            }
        }
    }
}
