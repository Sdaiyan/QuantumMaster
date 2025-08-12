/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Resources
{
    /// <summary>
    /// 资源功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class ResourcesPatch
    {
        /// <summary>
        /// 收集资源补丁
        /// 配置项: collectResource
        /// 功能: 修改资源收集概率，提高稀有物品获得率
        /// </summary>
        public static bool PatchCollectResource(Harmony harmony)
        {
            if (!ConfigManager.collectResource && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Map.MapDomain),
                MethodName = "ApplyCollectResourceResult",
                // DataContext ctx, GameData.Domains.Character.Character character, MapBlockData blockData, short currentResource, short maxResource, bool costResource, ref CollectResourceResult result
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(GameData.Domains.Character.Character), typeof(GameData.Domains.Map.MapBlockData), typeof(short), typeof(short), typeof(bool), typeof(GameData.Domains.Map.CollectResourceResult).MakeByRefType() }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "ApplyCollectResourceResult",
                    OriginalMethod);

            // 1 else if (itemTemplateId >= 0 && random.CheckPercentProb(blockData.GetCollectItemChance(resourceType)))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 升级收集材料补丁
        /// 配置项: collectResource
        /// 功能: 修改材料升级概率，提高材料品质
        /// </summary>
        public static bool PatchUpgradeCollectMaterial(Harmony harmony)
        {
            if (!ConfigManager.collectResource && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Map.MapDomain),
                MethodName = "UpgradeCollectMaterial",
                // public void UpgradeCollectMaterial(IRandomSource random, ResourceCollectionItem collectionConfig, sbyte resourceType, short maxResource, short currentResource, int neighborOddsMultiplier, ref short itemTemplateId)
                Parameters = new Type[] { typeof(IRandomSource), typeof(Config.ResourceCollectionItem), typeof(sbyte), typeof(short), typeof(short), typeof(int), typeof(short).MakeByRefType() }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "UpgradeCollectMaterial",
                    OriginalMethod);

            // if (!random.CheckPercentProb(gradeUpOdds))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    1);

            // 2 if (random.CheckPercentProb(odds))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    PatchPresets.Replacements.CheckPercentProbTrue,
                    2);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 获取收集资源数量补丁
        /// 配置项: GetCollectResourceAmount
        /// 功能: 修改收集资源数量的随机计算，获得更多资源
        /// </summary>
        public static bool PatchGetCollectResourceAmount(Harmony harmony)
        {
            if (!ConfigManager.GetCollectResourceAmount && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Map.MapDomain),
                MethodName = "GetCollectResourceAmount",
                // IRandomSource random, MapBlockData blockData, sbyte resourceType
                Parameters = new Type[] { typeof(IRandomSource), typeof(GameData.Domains.Map.MapBlockData), typeof(sbyte) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "GetCollectResourceAmount",
                    OriginalMethod);

            // 1 return currentResource * (((currentResource >= 100) ? 60 : 40) + random.Next(-20, 21)) / 100 * resourceMultiplier;
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 每月地图更新补丁
        /// 配置项: ParallelUpdateOnMonthChange
        /// 功能: 修改每月地图资源恢复的随机计算，获得更多资源恢复
        /// </summary>
        public static bool PatchParallelUpdateOnMonthChange(Harmony harmony)
        {
            if (!ConfigManager.ParallelUpdateOnMonthChange && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Map.MapDomain),
                MethodName = "ParallelUpdateOnMonthChange",
                // DataContext context, int areaIdInt
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(int) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "ParallelUpdateOnMonthChange",
                    OriginalMethod);

            // 1 int addValue = context.Random.Next(maxAddValue + 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    PatchPresets.Replacements.Next1ArgMax,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 挑剔获取材料补丁
        /// 配置项: ChoosyGetMaterial
        /// 功能: 修改挑剔获取材料的随机计算，获得更好的材料
        /// </summary>
        public static bool PatchChoosyGetMaterial(Harmony harmony)
        {
            if (!ConfigManager.ChoosyGetMaterial && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "ChoosyGetMaterial",
                // DataContext context, sbyte resourceType, int count
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(sbyte), typeof(int) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "ChoosyGetMaterial",
                    OriginalMethod);

            // 1 int random = context.Random.Next(10000);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    PatchPresets.Replacements.Next1ArgMax,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 添加挑剔剩余升级数据补丁
        /// 配置项: AddChoosyRemainUpgradeData
        /// 功能: 修改挑剔剩余升级数据的随机计算
        /// </summary>
        public static bool PatchAddChoosyRemainUpgradeData(Harmony harmony)
        {
            if (!ConfigManager.AddChoosyRemainUpgradeData && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "AddChoosyRemainUpgradeData",
                // DataContext context
                Parameters = new Type[] { typeof(GameData.Common.DataContext) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "AddChoosyRemainUpgradeData",
                    OriginalMethod);

            // 1 int randomAddCount = ((maxAddCount > 0) ? context.Random.Next(maxAddCount) : 0);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    PatchPresets.Replacements.Next1ArgMax,
                    1);

            // 2 int randomAddRate = ((maxAddRate > 0) ? context.Random.Next(maxAddRate) : 0);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    PatchPresets.Replacements.Next1ArgMax,
                    2);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
