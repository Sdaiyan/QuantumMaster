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
    /// 获取收集资源数量功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class GetCollectResourceAmountPatch
    {
        /// <summary>
        /// 获取收集资源数量功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(GetCollectResourceAmountPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };
        }

        /// <summary>
        /// 获取收集资源数量功能专用的 Next2Args 替换方法（取最大值）
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "GetCollectResourceAmount");
        }

        /// <summary>
        /// 应用获取收集资源数量补丁
        /// 配置项: GetCollectResourceAmount
        /// 功能: 修改收集资源数量的随机计算，获得更多资源
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("GetCollectResourceAmount")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Map.MapDomain),
                MethodName = "GetCollectResourceAmount",
                Parameters = new Type[] { typeof(IRandomSource), typeof(GameData.Domains.Map.MapBlockData), typeof(sbyte) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "GetCollectResourceAmount",
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
            // 1 return currentResource * (((currentResource >= 100) ? 60 : 40) + random.Next(-20, 21)) / 100 * resourceMultiplier;
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    1);
        }
    }
}
