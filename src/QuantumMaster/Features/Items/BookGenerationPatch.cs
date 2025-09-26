/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Item;
using Redzen.Random;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using QuantumMaster.Features.Core;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Items
{
    /// <summary>
    /// 书籍生成功能补丁
    /// 配置项: GeneratePageIncompleteState, FixedPagePos
    /// 功能: 控制书籍生成时的页面状态
    /// 注意: 完整书页为最大值，亡佚书页为最小值，可选择固定完整页面位置
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Item.SkillBook), "GeneratePageIncompleteState")]
    public class BookGenerationPatch
    {
        /// <summary>
        /// 书籍页面不完整状态生成补丁
        /// </summary>
        [HarmonyPrefix]
        public static bool Prefix(ref ushort __result, IRandomSource random, sbyte skillGroup, sbyte grade, sbyte completePagesCount, sbyte lostPagesCount, bool outlineAlwaysComplete)
        {
            if (!ConfigManager.GeneratePageIncompleteState)
            {
                return true; // 使用原版逻辑
            }

            DebugLog.Info($"书籍生成: 技能组{skillGroup}, 品级{grade}, 应用最优页面配置");

            int normalPagesCount = (skillGroup == 1) ? 5 : 5;
            
            if (completePagesCount < 0)
            {
                float mean = 3f - (float)grade / 4f;
                float min = Math.Max(0f, mean - 1f);
                float max = Math.Min(normalPagesCount, mean + 1f);
                completePagesCount = (sbyte)(max > min ? max : min);
            }
            
            if (lostPagesCount < 0)
            {
                float mean = -1f + (float)grade / 2.667f;
                float min = Math.Max(0f, mean - 1f);
                float max = Math.Min(normalPagesCount - completePagesCount, mean + 1f);
                lostPagesCount = (sbyte)Math.Round(max > min ? min : max);
            }
            
            int incompletePagesCount = normalPagesCount - completePagesCount - lostPagesCount;
            if (incompletePagesCount < 0)
            {
                throw new Exception($"IncompletePagesCount is less than zero: {incompletePagesCount}");
            }
            
            int leftPagesCount = completePagesCount + incompletePagesCount + lostPagesCount;
            List<sbyte> pLeftStates = new List<sbyte>(leftPagesCount);
            
            // 添加完整页面 (状态0)
            for (int i = 0; i < completePagesCount; i++)
            {
                pLeftStates.Add((sbyte)0);
            }
            
            // 添加不完整页面 (状态1)
            for (int i = completePagesCount; i < completePagesCount + incompletePagesCount; i++)
            {
                pLeftStates.Add((sbyte)1);
            }
            
            // 添加亡佚页面 (状态2)
            for (int i = completePagesCount + incompletePagesCount; i < leftPagesCount; i++)
            {
                pLeftStates.Add((sbyte)2);
            }
            
            // 如果没有启用固定页面位置，则打乱顺序
            if (!ConfigManager.FixedPagePos)
            {
                Random _random = new Random();
                for (int i = 0; i < leftPagesCount; i++)
                {
                    int rate = _random.Next(100);
                    if (rate > 50)
                    {
                        int j = _random.Next(0, leftPagesCount);
                        sbyte temp = pLeftStates[i];
                        pLeftStates[i] = pLeftStates[j];
                        pLeftStates[j] = temp;
                    }
                }
            }
            
            ushort states = 0;
            byte pageBeginId = 0;
            
            if (skillGroup == 1)
            {
                sbyte outlineState = (sbyte)((!outlineAlwaysComplete && random.CheckPercentProb(90)) ? 2 : 0);
                states = SkillBookStateHelper.SetPageIncompleteState(states, 0, outlineState);
                pageBeginId = 1;
            }
            
            for (int i = 0; i < normalPagesCount; i++)
            {
                byte pageId = (byte)(pageBeginId + i);
                sbyte state = pLeftStates[i];
                states = SkillBookStateHelper.SetPageIncompleteState(states, pageId, state);
            }
            
            __result = states;
            DebugLog.Info($"书籍生成完成: 完整页{completePagesCount}, 不完整页{incompletePagesCount}, 亡佚页{lostPagesCount}");
            return false; // 跳过原方法
        }
    }
}
