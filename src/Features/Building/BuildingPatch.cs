/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Building
{
    /// <summary>
    /// 建筑功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class BuildingPatch
    {
        /// <summary>
        /// 离线商店管理更新补丁
        /// 配置项: OfflineUpdateShopManagement
        /// 功能: 修改离线商店管理的随机概率，提高商品刷新和品质
        /// </summary>
        public static bool PatchOfflineUpdateShopManagement(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("OfflineUpdateShopManagement")) return false;

            // private void OfflineUpdateShopManagement(ParallelBuildingModification modification, short settlementId, BuildingBlockItem buildingBlockCfg, BuildingBlockKey blockKey, BuildingBlockData blockData, DataContext context)
            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "OfflineUpdateShopManagement",
                // ParallelBuildingModification modification, short settlementId, BuildingBlockItem buildingBlockCfg, BuildingBlockKey blockKey, BuildingBlockData blockData, DataContext context
                Parameters = new Type[] { typeof(GameData.Domains.Building.ParallelBuildingModification), typeof(short), typeof(Config.BuildingBlockItem), typeof(GameData.Domains.Building.BuildingBlockKey), typeof(GameData.Domains.Building.BuildingBlockData), typeof(GameData.Common.DataContext) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "OfflineUpdateShopManagement",
                    OriginalMethod);

            // CheckProb
            // 1 if (data3.ShopSoldItemList[k].TemplateId != -1 && random.CheckProb(50, 100))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckProb,
                    PatchPresets.Replacements.CheckProbTrue,
                    1);

            // Next 2 args
            // 1 itemTemplateId = itemProbList[random.Next(0, itemProbList.Count)]; 从任意成功的结果中 随机一个
            // 2 sbyte gradeLevel = itemProbList2[random.Next(0, itemProbList2.Count)]; 随机的品级，应该是越后面越大
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    2);

            // CheckPercentProb 我也不知道为什么 IL 最后只有 3个 CheckPercentProb，全部替换 
            // 1 if (hasManager && random.CheckPercentProb(prob))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            // 2 if (random.CheckPercentProb(prob2))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    2);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 建筑随机收益修正补丁
        /// 配置项: BuildingRandomCorrection
        /// 功能: 修改第一个出现的 random.next 2个参数版本，使其受到气运影响，期望为最大值
        /// 目标方法: internal int BuildingRandomCorrection(int value, IRandomSource randomSource)
        /// </summary>
        public static bool PatchBuildingRandomCorrection(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("BuildingRandomCorrection")) return false;

            // 目标方法：internal int BuildingRandomCorrection(int value, IRandomSource randomSource)
            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "BuildingRandomCorrection",
                Parameters = new Type[] { typeof(int), typeof(Redzen.Random.IRandomSource) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "BuildingRandomCorrection",
                    OriginalMethod);

            // 替换第一个出现的 random.Next(min, max) 为返回最大值的版本
            // 这样可以确保建筑收益（木材、金铁、金币、威望等）达到随机范围的上限
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 村民经营资质增加概率补丁
        /// 配置项: UpdateShopBuildingTeach
        /// 功能: 修改村民经营时资质增加的概率，替换第1和第2个CheckPercentProb调用
        /// 目标方法: private void UpdateShopBuildingTeach(DataContext context, ParallelBuildingModification modification, BuildingBlockKey blockKey, BuildingBlockData blockData)
        /// </summary>
        public static bool PatchUpdateShopBuildingTeach(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("UpdateShopBuildingTeach")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "UpdateShopBuildingTeach",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(GameData.Domains.Building.ParallelBuildingModification), 
                    typeof(GameData.Domains.Building.BuildingBlockKey), 
                    typeof(GameData.Domains.Building.BuildingBlockData) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "UpdateShopBuildingTeach",
                    OriginalMethod);

            // 替换第1个CheckPercentProb调用 - 村民经营资质增加概率
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            // 替换第2个CheckPercentProb调用 - 村民经营资质增加概率
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    2);

            patchBuilder.Apply(harmony);

            return true;
        }
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
            bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(null, originalResult);
            int newResult = success ? 100 : 0;
            
            DebugLog.Info($"【气运】赌坊与青楼基础暴击率: 原始概率{originalResult}% -> 气运判定{(success ? "成功" : "失败")} -> {newResult}%");
            __result = newResult;
        }
    }
}
