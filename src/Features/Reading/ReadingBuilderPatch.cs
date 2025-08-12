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
    /// 读书相关 PatchBuilder 补丁集合
    /// 使用 PatchBuilder 进行动态补丁
    /// </summary>
    public static class ReadingBuilderPatch
    {
        /// <summary>
        /// 获取策略进度增加值补丁
        /// 配置项: GetStrategyProgressAddValue
        /// 功能: 修改读书策略进度增加值的随机计算，获得更多进度
        /// </summary>
        public static bool PatchGetStrategyProgressAddValue(Harmony harmony)
        {
            if (!ConfigManager.GetStrategyProgressAddValue && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "GetStrategyProgressAddValue",
                // IRandomSource random, ReadingStrategyItem strategyCfg
                Parameters = new Type[] { typeof(IRandomSource), typeof(Config.ReadingStrategyItem) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "GetStrategyProgressAddValue",
                    OriginalMethod);

            // 1 return (sbyte)((strategyCfg.MaxProgressAddValue > strategyCfg.MinProgressAddValue) ? random.Next(strategyCfg.MinProgressAddValue, strategyCfg.MaxProgressAddValue + 1) : strategyCfg.MaxProgressAddValue);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 应用生活技能立即读书策略效果补丁
        /// 配置项: ApplyImmediateReadingStrategyEffectForLifeSkill
        /// 功能: 修改生活技能读书策略效果的随机计算
        /// </summary>
        public static bool PatchApplyImmediateReadingStrategyEffectForLifeSkill(Harmony harmony)
        {
            if (!ConfigManager.ApplyImmediateReadingStrategyEffectForLifeSkill && !QuantumMaster.openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
                MethodName = "ApplyImmediateReadingStrategyEffectForLifeSkill",
                // DataContext context, GameData.Domains.Item.SkillBook book, byte pageIndex, ref ReadingBookStrategies strategies, sbyte strategyId
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(GameData.Domains.Item.SkillBook), typeof(byte), typeof(GameData.Domains.Taiwu.ReadingBookStrategies).MakeByRefType(), typeof(sbyte) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "ApplyImmediateReadingStrategyEffectForLifeSkill",
                    OriginalMethod);

            // 1 sbyte currPageAddValue = (sbyte)((strategyCfg.MaxProgressAddValue > strategyCfg.MinProgressAddValue) ? context.Random.Next(strategyCfg.MinProgressAddValue, strategyCfg.MaxProgressAddValue + 1) : strategyCfg.MaxProgressAddValue);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    PatchPresets.Replacements.Next2ArgsMax,
                    1);

            patchBuilder.Apply(harmony);

            return true;
        }
    }
}
