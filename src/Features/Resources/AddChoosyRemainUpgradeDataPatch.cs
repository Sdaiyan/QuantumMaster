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
    /// 添加挑剔剩余升级数据功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class AddChoosyRemainUpgradeDataPatch
    {
        /// <summary>
        /// 添加挑剔剩余升级数据功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next1ArgMax = new ReplacementMethodInfo
            {
                Type = typeof(AddChoosyRemainUpgradeDataPatch),
                MethodName = nameof(Next1ArgMax_Method)
            };
        }

        /// <summary>
        /// 添加挑剔剩余升级数据功能专用的 Next1Arg 替换方法（取最大值）
        /// </summary>
        public static int Next1ArgMax_Method(this IRandomSource randomSource, int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max, "AddChoosyRemainUpgradeData");
        }

        /// <summary>
        /// 应用添加挑剔剩余升级数据补丁
        /// 配置项: AddChoosyRemainUpgradeData
        /// 功能: 修改挑剔剩余升级数据的随机计算
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("AddChoosyRemainUpgradeData")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "AddChoosyRemainUpgradeData",
                Parameters = new Type[] { typeof(GameData.Common.DataContext) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "AddChoosyRemainUpgradeData",
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
            // 1 int randomAddCount = ((maxAddCount > 0) ? context.Random.Next(maxAddCount) : 0);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    Replacements.Next1ArgMax,
                    1);

            // 2 int randomAddRate = ((maxAddRate > 0) ? context.Random.Next(maxAddRate) : 0);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    Replacements.Next1ArgMax,
                    2);
        }
    }
}
