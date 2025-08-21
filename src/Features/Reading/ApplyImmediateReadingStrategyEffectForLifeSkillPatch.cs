/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Reading
{
    /// <summary>
    /// 生活技能立即读书策略效果独立补丁
    /// 配置项: ApplyImmediateReadingStrategyEffectForLifeSkill
    /// 功能: 【气运】生活技能读书策略效果的随机计算，使用与GetStrategyProgressAddValue相同的气运设置
    /// </summary>
    public static class ApplyImmediateReadingStrategyEffectForLifeSkillPatch
    {
        /// <summary>
        /// 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(ApplyImmediateReadingStrategyEffectForLifeSkillPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };
        }

        /// <summary>
        /// ApplyImmediateReadingStrategyEffectForLifeSkill 功能专用的 Next2Args 替换方法实现
        /// 使用与GetStrategyProgressAddValue相同的气运设置
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "GetStrategyProgressAddValue");
        }

        /// <summary>
        /// 主要的补丁应用方法
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("GetStrategyProgressAddValue")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "ApplyImmediateReadingStrategyEffectForLifeSkill",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(GameData.Domains.Item.SkillBook), 
                    typeof(byte), 
                    typeof(GameData.Domains.Taiwu.ReadingBookStrategies).MakeByRefType(), 
                    typeof(sbyte) 
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "ApplyImmediateReadingStrategyEffectForLifeSkill",
                OriginalMethod);

            ConfigureReplacements(patchBuilder);
            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // 1 sbyte currPageAddValue = (sbyte)((strategyCfg.MaxProgressAddValue > strategyCfg.MinProgressAddValue) ? context.Random.Next(strategyCfg.MinProgressAddValue, strategyCfg.MaxProgressAddValue + 1) : strategyCfg.MaxProgressAddValue);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                Replacements.Next2ArgsMax,
                1);
        }
    }
}
