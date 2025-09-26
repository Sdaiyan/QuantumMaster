/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Map;
using QuantumMaster.Features.Core;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.World
{
    /// <summary>
    /// 地块资源初始化功能补丁
    /// 配置项: InitResources
    /// 功能: 控制世界生成时地块资源的初始数量
    /// 注意: 每个地块上的资源为浮动区间的最大值，受到难度影响
    /// </summary>
    [HarmonyPatch(typeof(MapBlockData), "InitResources")]
    public class MapResourceInitPatch
    {
        /// <summary>
        /// 地块资源初始化补丁
        /// </summary>
        [HarmonyPrefix]
        public static unsafe bool Prefix(MapBlockData __instance)
        {
            if (!ConfigManager.InitResources)
            {
                return true; // 使用原版逻辑
            }

            DebugLog.Info("地块资源初始化: 应用最大资源设置");

            var configData = __instance.GetConfig();
            if (configData != null)
            {
                for (sbyte resourceType = 0; resourceType < 6; resourceType++)
                {
                    short maxResource = configData.Resources[resourceType];
                    if (maxResource < 0)
                    {
                        maxResource = (short)(System.Math.Abs(maxResource) * 5);
                    }
                    else if (maxResource != 0)
                    {
                        maxResource = (short)(maxResource + 25);
                    }
                    __instance.MaxResources.Items[resourceType] = maxResource;
                    __instance.CurrResources.Items[resourceType] = (short)(maxResource * 50 / 100 * GameData.Domains.World.SharedMethods.GetGainResourcePercent(12) / 100);
                }
            }

            DebugLog.Info("地块资源初始化完成，已应用最大资源设置");
            return false; // 跳过原方法
        }
    }
}
