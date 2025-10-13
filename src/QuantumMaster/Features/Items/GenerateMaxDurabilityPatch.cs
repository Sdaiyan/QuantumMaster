/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Items
{
    /// <summary>
    /// 物品耐久上限补丁
    /// 配置项: GenerateMaxDurability
    /// 功能: 修改物品生成时耐久上限的计算，根据气运增加
    /// </summary>
    public static class GenerateMaxDurabilityPatch
    {
        /// <summary>
        /// GenerateMaxDurability 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// GenerateMaxDurability 专用的 Next2Args 替换，倾向最大值
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(GenerateMaxDurabilityPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };
        }

        /// <summary>
        /// GenerateMaxDurability 功能专用的 Next(int, int) 替换方法
        /// 支持 GenerateMaxDurability 功能的独立气运设置
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "GenerateMaxDurability");
        }

        /// <summary>
        /// 应用物品耐久上限补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("GenerateMaxDurability")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Item.ItemBase),
                MethodName = "GenerateMaxDurability",
                Parameters = new Type[] { 
                    typeof(IRandomSource), 
                    typeof(short)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "GenerateMaxDurability",
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
            // 替换第1次出现的 Next(int, int) 调用
            // 原方法逻辑: return (short)random.Next(value / 2, value + 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    1);
        }
    }
}
