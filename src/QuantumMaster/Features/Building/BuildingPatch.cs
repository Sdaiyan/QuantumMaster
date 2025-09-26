/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Building
{
    /// <summary>
    /// 建筑功能补丁集合
    /// </summary>
    public static class BuildingPatch
    {
        // 注意：OfflineUpdateShopManagement、BuildingRandomCorrection、UpdateShopBuildingTeach 
        // 已转移到各自的独立补丁类中，支持独立气运设置
    }

    /// <summary>
    /// 赌坊与青楼暴击率补丁
    /// 配置项: BuildingManageHarvestSpecialSuccessRate
    /// 功能: 【气运】根据气运影响赌坊与青楼的暴击概率，成功返回100，失败返回0
    /// 目标方法: internal int BuildingManageHarvestSpecialSuccessRate(BuildingBlockKey blockKey, int charId)
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Building.BuildingDomain), "BuildingManageHarvestSpecialSuccessRate")]
    public static class BuildingManageHarvestSpecialSuccessRatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref int __result, GameData.Domains.Building.BuildingBlockKey blockKey, int charId)
        {
            if (!ConfigManager.IsFeatureEnabled("BuildingManageHarvestSpecialSuccessRate"))
            {
                DebugLog.Info("【气运】赌坊与青楼基础暴击率: 使用原版逻辑");
                return; // 使用原版逻辑
            }

            var originalResult = __result;
            
            // 使用气运系统进行成功判断，基于原始概率值
            // 原始返回值通常是0-100的百分比概率
            bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(null, originalResult, "BuildingManageHarvestSpecialSuccessRate");
            int newResult = success ? 100 : 0;
            
            DebugLog.Info($"【气运】赌坊与青楼基础暴击率: 原始概率{originalResult}% -> 气运判定{(success ? "成功" : "失败")} -> {newResult}%");
            __result = newResult;
        }
    }
}
