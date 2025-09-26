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
    /// 商店建筑教学更新补丁
    /// 配置项: UpdateShopBuildingTeach
    /// 功能: 修改商店建筑教学功能的随机概率，提高学习成功率
    /// </summary>
    public static class UpdateShopBuildingTeachPatch
    {
        /// <summary>
        /// UpdateShopBuildingTeach 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// UpdateShopBuildingTeach 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(UpdateShopBuildingTeachPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// UpdateShopBuildingTeach 功能专用的 CheckPercentProb 替换方法
        /// 支持 UpdateShopBuildingTeach 功能的独立气运设置
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "UpdateShopBuildingTeach");
        }

        /// <summary>
        /// 应用商店建筑教学更新补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
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
            // 替换第1个CheckPercentProb调用 - 村民经营资质增加概率
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);

            // 替换第2个CheckPercentProb调用 - 村民经营资质增加概率
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    2);
        }
    }
}
