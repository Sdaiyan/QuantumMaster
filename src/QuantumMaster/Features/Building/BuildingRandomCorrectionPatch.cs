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
    /// 建筑随机纠正补丁
    /// 配置项: BuildingRandomCorrection
    /// 功能: 修改建筑相关的随机概率纠正机制
    /// </summary>
    public static class BuildingRandomCorrectionPatch
    {
        /// <summary>
        /// BuildingRandomCorrection 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// BuildingRandomCorrection 专用的 Next2Args 最大值替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(BuildingRandomCorrectionPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };
        }

        /// <summary>
        /// BuildingRandomCorrection 功能专用的 Next2Args 替换方法
        /// 支持 BuildingRandomCorrection 功能的独立气运设置
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "BuildingRandomCorrection");
        }

        /// <summary>
        /// 应用建筑随机纠正补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("BuildingRandomCorrection")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "BuildingRandomCorrection",
                Parameters = new Type[] { typeof(int), typeof(Redzen.Random.IRandomSource) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "BuildingRandomCorrection",
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
            // Next 2 args
            // correction = DataHelper.ArchitectureHelper.GetRandomOffsetValue(correction, totalCorrections[i], random.Next(-1, 2));
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    1);
        }
    }
}
