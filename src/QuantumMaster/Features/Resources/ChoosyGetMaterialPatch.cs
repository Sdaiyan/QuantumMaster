/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Resources
{
    /// <summary>
    /// 挑剔获取材料功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class ChoosyGetMaterialPatch
    {
        /// <summary>
        /// 挑剔获取材料功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next1ArgMax = new ReplacementMethodInfo
            {
                Type = typeof(ChoosyGetMaterialPatch),
                MethodName = nameof(Next1ArgMax_Method)
            };
        }

        /// <summary>
        /// 挑剔获取材料功能专用的 Next1Arg 替换方法（取最大值）
        /// </summary>
        public static int Next1ArgMax_Method(this IRandomSource randomSource, int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max, "ChoosyGetMaterial");
        }

        /// <summary>
        /// 应用挑剔获取材料补丁
        /// 配置项: ChoosyGetMaterial
        /// 功能: 修改挑剔获取材料的随机计算，获得更好的材料
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("ChoosyGetMaterial")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "ChoosyGetMaterial",
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(sbyte), typeof(int) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "ChoosyGetMaterial",
                    OriginalMethod);

            // 配置方法替换规则
            ConfigureReplacements(patchBuilder);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // 1 int random = context.Random.Next(10000);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    Replacements.Next1ArgMax,
                    1);
        }
    }
}
