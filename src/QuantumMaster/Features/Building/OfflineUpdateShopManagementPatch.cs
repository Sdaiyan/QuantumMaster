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
    /// 离线商店管理更新补丁
    /// 配置项: OfflineUpdateShopManagement
    /// 功能: 修改离线商店管理的随机概率，提高商品刷新和品质
    /// </summary>
    public static class OfflineUpdateShopManagementPatch
    {
        /// <summary>
        /// OfflineUpdateShopManagement 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// OfflineUpdateShopManagement 专用的 CheckProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(OfflineUpdateShopManagementPatch),
                MethodName = nameof(CheckProbTrue_Method)
            };

            /// <summary>
            /// OfflineUpdateShopManagement 专用的 Next2Args 最大值替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(OfflineUpdateShopManagementPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };

            /// <summary>
            /// OfflineUpdateShopManagement 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(OfflineUpdateShopManagementPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// OfflineUpdateShopManagement 功能专用的 CheckProb 替换方法
        /// 支持 OfflineUpdateShopManagement 功能的独立气运设置
        /// </summary>
        public static bool CheckProbTrue_Method(this IRandomSource randomSource, int numerator, int denominator)
        {
            return LuckyCalculator.Calc_Random_CheckProb_True_By_Luck(randomSource, numerator, denominator, "OfflineUpdateShopManagement");
        }

        /// <summary>
        /// OfflineUpdateShopManagement 功能专用的 Next2Args 替换方法
        /// 支持 OfflineUpdateShopManagement 功能的独立气运设置
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "OfflineUpdateShopManagement");
        }

        /// <summary>
        /// OfflineUpdateShopManagement 功能专用的 CheckPercentProb 替换方法
        /// 支持 OfflineUpdateShopManagement 功能的独立气运设置
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "OfflineUpdateShopManagement");
        }

        /// <summary>
        /// 应用离线商店管理更新补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("OfflineUpdateShopManagement")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "OfflineUpdateShopManagement",
                Parameters = new Type[] { typeof(GameData.Domains.Building.ParallelBuildingModification), typeof(short), typeof(Config.BuildingBlockItem), typeof(GameData.Domains.Building.BuildingBlockKey), typeof(GameData.Domains.Building.BuildingBlockData), typeof(GameData.Common.DataContext) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "OfflineUpdateShopManagement",
                    OriginalMethod);

            ConfigureReplacements(patchBuilder);
            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        /// <param name="patchBuilder">PatchBuilder 实例</param>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // CheckProb
            // 1 if (data3.ShopSoldItemList[k].TemplateId != -1 && random.CheckProb(50, 100))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckProb,
                    Replacements.CheckProbTrue,
                    1);

            // Next 2 args
            // 1 itemTemplateId = itemProbList[random.Next(0, itemProbList.Count)]; 从任意成功的结果中 随机一个
            // 2 sbyte gradeLevel = itemProbList2[random.Next(0, itemProbList2.Count)]; 随机的品级，应该是越后面越大
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    2);

            // CheckPercentProb 我也不知道为什么 IL 最后只有 3个 CheckPercentProb，全部替换 
            // 1 if (hasManager && random.CheckPercentProb(prob))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);

            // 2 if (random.CheckPercentProb(prob2))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    2);
        }
    }
}
