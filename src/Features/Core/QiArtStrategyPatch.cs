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

namespace QuantumMaster.Features.Core
{
    /// <summary>
    /// 周天内力策略功能补丁
    /// 配置项: GetQiArtStrategyDeltaNeiliBonus
    /// 功能: 控制周天运转时通过策略获得的内力收益
    /// 注意: 内力收益为理论最大值
    /// </summary>
    public class QiArtStrategyPatch
    {
        /// <summary>
        /// 周天内力策略增量收益补丁
        /// </summary>
        [HarmonyPatch(typeof(TaiwuDomain), "GetQiArtStrategyDeltaNeiliBonus")]
        public class GetQiArtStrategyDeltaNeiliBonusPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(IRandomSource random, ref int __result)
            {
                if (!ConfigManager.GetQiArtStrategyDeltaNeiliBonus)
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
                            bonus += config.MaxExtraNeili;
                        }
                    }
                    
                    __result = bonus;
                    DebugLog.Info($"周天内力策略增量收益: {bonus}");
                    return false;
                }
                
                __result = 0;
                return false; // 跳过原方法
            }
        }

        /// <summary>
        /// 周天内力策略额外分配收益补丁
        /// </summary>
        [HarmonyPatch(typeof(TaiwuDomain), "GetQiArtStrategyExtraNeiliAllocationBonus")]
        public class GetQiArtStrategyExtraNeiliAllocationBonusPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(IRandomSource random, ref int __result)
            {
                if (!ConfigManager.GetQiArtStrategyDeltaNeiliBonus)
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
                            bonus += config.MaxExtraNeiliAllocation;
                        }
                    }
                    
                    __result = bonus;
                    DebugLog.Info($"周天内力策略额外分配收益: {bonus}");
                    return false;
                }
                
                __result = 0;
                return false; // 跳过原方法
            }
        }
    }
}
