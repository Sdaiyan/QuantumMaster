/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Taiwu;
using GameData.Domains;
using Redzen.Random;
using Config;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Core
{
    /// <summary>
    /// 周天内力策略功能补丁集合
    /// 配置项: GetQiArtStrategyDeltaNeiliBonus
    /// 功能: 控制周天运转时通过策略获得的内力收益
    /// 注意: 内力收益为理论最大值
    /// </summary>
    public static class QiArtStrategyPatch
    {
        // 这个类现在只作为补丁集合的容器，实际的补丁类已移到顶层
    }

    /// <summary>
    /// 周天内力策略增量收益补丁
    /// 配置项: GetQiArtStrategyDeltaNeiliBonus
    /// 功能: 控制周天运转时通过策略获得的内力增量收益
    /// </summary>
    [HarmonyPatch(typeof(TaiwuDomain), "GetQiArtStrategyDeltaNeiliBonus")]
    public static class GetQiArtStrategyDeltaNeiliBonusPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(IRandomSource random, ref int __result)
        {
            if (!ConfigManager.IsFeatureEnabled("GetQiArtStrategyDeltaNeiliBonus"))
            {
                return true; // 使用原版逻辑
            }

            var taiwuChar = DomainManager.Taiwu.GetTaiwu();
            short loopingNeigongTemplateId = taiwuChar.GetLoopingNeigong();
            
            if (DomainManager.Extra.TryGetElement_QiArtStrategyMap(loopingNeigongTemplateId, out var qiArtStrategyList))
            {
                if (qiArtStrategyList.Items.Count == 0)
                {
                    __result = 0;
                    return false;
                }
                
                int bonus = 0;
                foreach (sbyte id in qiArtStrategyList.Items)
                {
                    if (id != -1)
                    {
                        QiArtStrategyItem config = QiArtStrategy.Instance[id];
                        // 使用气运计算，在最小值到最大值之间随机
                        int minValue = config.MinExtraNeili;
                        int maxValue = config.MaxExtraNeili;
                        int actualValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(minValue, maxValue, "GetQiArtStrategyDeltaNeiliBonus");
                        bonus += actualValue;
                    }
                }
                
                __result = bonus;
                DebugLog.Info($"【气运】周天内力策略增量收益: {bonus}");
                return false;
            }
            
            __result = 0;
            return false; // 跳过原方法
        }
    }

    /// <summary>
    /// 周天内力策略额外分配收益补丁
    /// 配置项: GetQiArtStrategyDeltaNeiliBonus
    /// 功能: 控制周天运转时通过策略获得的内力额外分配收益
    /// </summary>
    [HarmonyPatch(typeof(TaiwuDomain), "GetQiArtStrategyExtraNeiliAllocationBonus")]
    public static class GetQiArtStrategyExtraNeiliAllocationBonusPatch    {
        [HarmonyPrefix]
        public static bool Prefix(IRandomSource random, ref int __result)
        {
            if (!ConfigManager.IsFeatureEnabled("GetQiArtStrategyDeltaNeiliBonus"))
            {
                return true; // 使用原版逻辑
            }

            var taiwuChar = DomainManager.Taiwu.GetTaiwu();
            short loopingNeigongTemplateId = taiwuChar.GetLoopingNeigong();
            
            if (DomainManager.Extra.TryGetElement_QiArtStrategyMap(loopingNeigongTemplateId, out var qiArtStrategyList))
            {
                if (qiArtStrategyList.Items.Count == 0)
                {
                    __result = 0;
                    return false;
                }
                
                int bonus = 0;
                foreach (sbyte id in qiArtStrategyList.Items)
                {
                    if (id != -1)
                    {
                        QiArtStrategyItem config = QiArtStrategy.Instance[id];
                        // 使用气运计算，在最小值到最大值之间随机
                        int minValue = config.MinExtraNeiliAllocation;
                        int maxValue = config.MaxExtraNeiliAllocation;
                        int actualValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(minValue, maxValue, "GetQiArtStrategyDeltaNeiliBonus");
                        bonus += actualValue;
                    }
                }
                
                __result = bonus;
                DebugLog.Info($"【气运】周天内力策略额外分配收益: {bonus}");
                return false;
            }
            
            __result = 0;
            return false; // 跳过原方法
        }
    }
}
