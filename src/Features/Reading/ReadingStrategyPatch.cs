/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Extra;
using GameData.Utilities;
using QuantumMaster.Features.Core;

namespace QuantumMaster.Features.Reading
{
    /// <summary>
    /// 读书策略相关功能补丁
    /// 配置项: BookStrategiesSelect1-9
    /// 功能: 控制读书策略的选择，可指定特定策略而不是随机
    /// 注意: 当任何策略设置为非"使用原版"时，将应用指定的策略组合
    /// </summary>
    [HarmonyPatch(typeof(ExtraDomain), "SetAvailableReadingStrategies")]
    public class ReadingStrategyPatch
    {
        /// <summary>
        /// 设置可用读书策略补丁
        /// </summary>
        [HarmonyPrefix]
        public static void Prefix(ref SByteList strategyIds)
        {
            // 检查是否有任何策略被设置为非"使用原版"（即 > 0）
            bool hasEnabledStrategies = ConfigManager.BookStrategiesSelect1 > 0 || ConfigManager.BookStrategiesSelect2 > 0 || 
                                        ConfigManager.BookStrategiesSelect3 > 0 || ConfigManager.BookStrategiesSelect4 > 0 || 
                                        ConfigManager.BookStrategiesSelect5 > 0 || ConfigManager.BookStrategiesSelect6 > 0 || 
                                        ConfigManager.BookStrategiesSelect7 > 0 || ConfigManager.BookStrategiesSelect8 > 0 || 
                                        ConfigManager.BookStrategiesSelect9 > 0;

            if (!hasEnabledStrategies)
            {
                return; // 使用原版逻辑
            }

            DebugLog.Info("读书策略自定义功能已启用，正在应用指定策略");

            SByteList customIds = SByteList.Create();
            
            // 只添加非"使用原版"的策略（值 > 0），并且需要减 1 因为第一个选项是"使用原版"
            if (ConfigManager.BookStrategiesSelect1 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect1 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略1: {GetStrategyName(strategyId)}");
            }
            if (ConfigManager.BookStrategiesSelect2 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect2 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略2: {GetStrategyName(strategyId)}");
            }
            if (ConfigManager.BookStrategiesSelect3 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect3 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略3: {GetStrategyName(strategyId)}");
            }
            if (ConfigManager.BookStrategiesSelect4 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect4 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略4: {GetStrategyName(strategyId)}");
            }
            if (ConfigManager.BookStrategiesSelect5 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect5 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略5: {GetStrategyName(strategyId)}");
            }
            if (ConfigManager.BookStrategiesSelect6 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect6 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略6: {GetStrategyName(strategyId)}");
            }
            if (ConfigManager.BookStrategiesSelect7 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect7 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略7: {GetStrategyName(strategyId)}");
            }
            if (ConfigManager.BookStrategiesSelect8 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect8 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略8: {GetStrategyName(strategyId)}");
            }
            if (ConfigManager.BookStrategiesSelect9 > 0)
            {
                sbyte strategyId = (sbyte)(ConfigManager.BookStrategiesSelect9 - 1);
                customIds.Items.Add(strategyId);
                DebugLog.Info($"添加策略9: {GetStrategyName(strategyId)}");
            }
            
            strategyIds = customIds;
            DebugLog.Info($"读书策略设置完成，共应用{customIds.Items.Count}个自定义策略");
        }

        /// <summary>
        /// 获取策略名称（用于日志）
        /// </summary>
        private static string GetStrategyName(sbyte strategyId)
        {
            return strategyId switch
            {
                0 => "口诵心惟",
                1 => "行思坐忆", 
                2 => "独见独知",
                3 => "奇思妙想",
                4 => "照猫画虎",
                5 => "按图索骥",
                6 => "寻章摘句",
                7 => "含英咀华",
                8 => "温故知新",
                9 => "融会贯通",
                10 => "枕经席文",
                11 => "十行俱下",
                12 => "春诵夏弦",
                13 => "推敲化演",
                14 => "心领得间",
                15 => "迁思回虑",
                16 => "囫囵吞枣",
                17 => "大智若愚",
                18 => "义父注解",
                _ => $"未知策略({strategyId})"
            };
        }
    }
}
