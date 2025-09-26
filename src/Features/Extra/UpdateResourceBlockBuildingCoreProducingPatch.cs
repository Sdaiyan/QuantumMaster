/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Extra
{
    /// <summary>
    /// 村庄资源点获得心材概率补丁
    /// 配置项: UpdateResourceBlockBuildingCoreProducing
    /// 功能: 修改过月时采集到资源点心材的概率，根据气运增加
    /// </summary>
    public static class UpdateResourceBlockBuildingCoreProducingPatch
    {
        /// <summary>
        /// UpdateResourceBlockBuildingCoreProducing 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// UpdateResourceBlockBuildingCoreProducing 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(UpdateResourceBlockBuildingCoreProducingPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// UpdateResourceBlockBuildingCoreProducing 功能专用的 CheckPercentProb 替换方法
        /// 支持 UpdateResourceBlockBuildingCoreProducing 功能的独立气运设置
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "UpdateResourceBlockBuildingCoreProducing");
        }

        /// <summary>
        /// 应用村庄资源点获得心材概率补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("UpdateResourceBlockBuildingCoreProducing")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Extra.ExtraDomain),
                MethodName = "UpdateResourceBlockBuildingCoreProducing",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "UpdateResourceBlockBuildingCoreProducing",
                    OriginalMethod);

            // 配置方法替换规则
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
            // CheckPercentProb 方法替换
            // 替换第一次出现的 CheckPercentProb 调用，期望结果为 true
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);
        }
    }
}