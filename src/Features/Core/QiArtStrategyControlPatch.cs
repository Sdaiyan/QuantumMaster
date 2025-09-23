/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Extra;
using GameData.Utilities;

namespace QuantumMaster.Features.Core
{
    /// <summary>
    /// 周天策略控制功能补丁
    /// 配置项: QiArtStrategiesSelect1-6
    /// 功能: 控制周天策略的选择，可指定特定策略而不是随机
    /// 注意: 当任何策略设置为非"使用原版"时，将应用指定的策略组合
    /// </summary>
    [HarmonyPatch(typeof(ExtraDomain), "SetAvailableQiArtStrategiesForNeigong")]
    public static class QiArtStrategyControlPatch
    {
        /// <summary>
        /// 设置可用周天策略补丁
        /// </summary>
        [HarmonyPrefix]
        public static void Prefix(ref SByteList strategyIds)
        {
            // 检查是否有任何策略被设置为非"使用原版"（即 > 0）
            bool hasEnabledStrategies = ConfigManager.QiArtStrategiesSelect1 > 0 || ConfigManager.QiArtStrategiesSelect2 > 0 || 
                                        ConfigManager.QiArtStrategiesSelect3 > 0 || ConfigManager.QiArtStrategiesSelect4 > 0 || 
                                        ConfigManager.QiArtStrategiesSelect5 > 0 || ConfigManager.QiArtStrategiesSelect6 > 0;

            if (!hasEnabledStrategies)
            {
                return; // 使用原版逻辑
            }

            DebugLog.Info("周天策略自定义功能已启用，正在应用指定策略");

            // 直接按位置替换对应的策略（值 > 0），并且需要减 1 因为第一个选项是"使用原版"
            if (ConfigManager.QiArtStrategiesSelect1 > 0 && 0 < strategyIds.Items.Count)
            {
                sbyte strategyId = (sbyte)(ConfigManager.QiArtStrategiesSelect1 - 1);
                strategyIds.Items[0] = strategyId;
                DebugLog.Info($"替换周天策略位置0: {GetQiArtStrategyName(strategyId)}");
            }
            if (ConfigManager.QiArtStrategiesSelect2 > 0 && 1 < strategyIds.Items.Count)
            {
                sbyte strategyId = (sbyte)(ConfigManager.QiArtStrategiesSelect2 - 1);
                strategyIds.Items[1] = strategyId;
                DebugLog.Info($"替换周天策略位置1: {GetQiArtStrategyName(strategyId)}");
            }
            if (ConfigManager.QiArtStrategiesSelect3 > 0 && 2 < strategyIds.Items.Count)
            {
                sbyte strategyId = (sbyte)(ConfigManager.QiArtStrategiesSelect3 - 1);
                strategyIds.Items[2] = strategyId;
                DebugLog.Info($"替换周天策略位置2: {GetQiArtStrategyName(strategyId)}");
            }
            if (ConfigManager.QiArtStrategiesSelect4 > 0 && 3 < strategyIds.Items.Count)
            {
                sbyte strategyId = (sbyte)(ConfigManager.QiArtStrategiesSelect4 - 1);
                strategyIds.Items[3] = strategyId;
                DebugLog.Info($"替换周天策略位置3: {GetQiArtStrategyName(strategyId)}");
            }
            if (ConfigManager.QiArtStrategiesSelect5 > 0 && 4 < strategyIds.Items.Count)
            {
                sbyte strategyId = (sbyte)(ConfigManager.QiArtStrategiesSelect5 - 1);
                strategyIds.Items[4] = strategyId;
                DebugLog.Info($"替换周天策略位置4: {GetQiArtStrategyName(strategyId)}");
            }
            if (ConfigManager.QiArtStrategiesSelect6 > 0 && 5 < strategyIds.Items.Count)
            {
                sbyte strategyId = (sbyte)(ConfigManager.QiArtStrategiesSelect6 - 1);
                strategyIds.Items[5] = strategyId;
                DebugLog.Info($"替换周天策略位置5: {GetQiArtStrategyName(strategyId)}");
            }
            
            DebugLog.Info("周天策略设置完成");
        }

        /// <summary>
        /// 获取周天策略名称（用于日志）
        /// </summary>
        private static string GetQiArtStrategyName(sbyte strategyId)
        {
            return strategyId switch
            {
                0 => "气沉丹田",
                1 => "五气调营",
                2 => "通经达脉",
                3 => "洞开气海",
                4 => "调形启气",
                5 => "贯气于顶",
                6 => "八脉充盈",
                7 => "洗筋伐髓",
                8 => "真力澎湃",
                9 => "川流不息",
                10 => "窍通络活",
                11 => "炼气调神",
                12 => "内息绵绵",
                13 => "修息生长",
                14 => "运转奇经",
                15 => "云蒸泉涌",
                16 => "天人感应",
                17 => "意明神秀",
                18 => "金刚气体",
                19 => "紫霞气体",
                20 => "玄阴气体",
                21 => "纯阳气体",
                22 => "归元气体",
                23 => "混元气体",
                24 => "化土生金",
                25 => "生水还金",
                26 => "克木成金",
                27 => "化水生木",
                28 => "生火还木",
                29 => "克土成木",
                30 => "化金生水",
                31 => "生木还水",
                32 => "克火成水",
                33 => "化木生火",
                34 => "生土还火",
                35 => "克金成火",
                36 => "化火生土",
                37 => "生金还土",
                38 => "克水还土",
                39 => "祛虚还实",
                40 => "返本归元",
                _ => $"未知周天策略({strategyId})"
            };
        }
    }
}