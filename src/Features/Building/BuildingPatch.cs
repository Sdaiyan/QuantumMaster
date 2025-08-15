/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;

namespace QuantumMaster.Features.Building
{
    /// <summary>
    /// 建筑功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class BuildingPatch
    {
        /// <summary>
        /// 建筑区域创建补丁
        /// 配置项: CreateBuildingArea
        /// 功能: 修改建筑区域创建时的随机数生成，使建筑等级更高
        /// </summary>
        public static bool PatchCreateBuildingArea(Harmony harmony)
        {
            if (!ConfigManager.CreateBuildingArea && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "CreateBuildingArea",
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(short), typeof(short), typeof(short) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CreateBuildingArea",
                    OriginalMethod);

            // 2参数 NEXT 方法替换
            // 1 sbyte level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    1);

            // 2 AddElement_BuildingBlocks(buildingBlockKey, new BuildingBlockData(num3, buildingBlockItem.TemplateId, (sbyte)((buildingBlockItem.MaxLevel <= 1) ? 1 : random.Next(1, buildingBlockItem.MaxLevel)), -1), context);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    2);

            // 3 AddBuilding(context, mapAreaId, mapBlockId, num3, buildingBlockItem.TemplateId, (sbyte)((buildingBlockItem.MaxLevel <= 1) ? 1 : random.Next(1, buildingBlockItem.MaxLevel)), buildingAreaWidth);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    3);

            // 4 num5 = list2[random.Next(0, list2.Count)];

            // 5 level = (sbyte)Math.Clamp(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1, buildingBlockItem2.MaxLevel);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    5);

            // 6 level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    6);

            // 7 int isBuild = random.Next(0, 2);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    7);

            // 8 blockIndex = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 9 level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    9);

            // 10 short blockIndex4 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 11 short blockIndex5 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 12 short blockIndex6 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 1 参数 NEXT 方法替换
            // 1 short merchantBuildingId = (short)(274 + context.Random.Next(7));

            // CheckPercentProb 方法替换
            // 1 if (num7 >= 5 || random.CheckPercentProb(num7 * 20))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            // 2 if (num9 >= 5 || random.CheckPercentProb(num9 * 20))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    2);

            // 1 sbyte level2 = (sbyte)formula.Calculate();
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CalculateFormula0Arg,
                    PatchPresets.Replacements.RandomCalculateMax,
                    1);

            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CalculateFormula0Arg,
                    PatchPresets.Replacements.RandomCalculateMax,
                    2);
                    
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CalculateFormula0Arg,
                    PatchPresets.Replacements.RandomCalculateMax,
                    3);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 离线商店管理更新补丁
        /// 配置项: OfflineUpdateShopManagement
        /// 功能: 修改离线商店管理的随机概率，提高商品刷新和品质
        /// </summary>
        public static bool PatchOfflineUpdateShopManagement(Harmony harmony)
        {
            if (!ConfigManager.OfflineUpdateShopManagement && !QuantumMaster.openAll) return false;

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
            if (!ConfigManager.BuildingRandomCorrection && !QuantumMaster.openAll) return false;

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
            if (!ConfigManager.BuildingManageHarvestSpecialSuccessRate && !QuantumMaster.openAll)
            {
                DebugLog.Info("【气运】赌坊与青楼基础暴击率: 使用原版逻辑");
                return; // 使用原版逻辑
            }

            var originalResult = __result;
            
            // 使用气运系统进行成功判断，基于原始概率值
            // 原始返回值通常是0-100的百分比概率
            bool success = LuckyRandomHelper.Calc_Random_CheckPercentProb_True_By_Luck(null, originalResult);
            int newResult = success ? 100 : 0;
            
            DebugLog.Info($"【气运】赌坊与青楼基础暴击率: 原始概率{originalResult}% -> 气运判定{(success ? "成功" : "失败")} -> {newResult}%");
            __result = newResult;
        }
    }
}
