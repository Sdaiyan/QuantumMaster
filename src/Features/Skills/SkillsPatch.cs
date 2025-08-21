/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Skills
{
    /// <summary>
    /// 技能功能补丁集合
    /// 使用 PatchBuilder 进行动态补丁，支持独立气运设置
    /// </summary>
    public static class SkillsPatch
    {
        /// <summary>
        /// 内功周天效果功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(SkillsPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };

            public static readonly ReplacementMethodInfo Next1Arg0 = new ReplacementMethodInfo
            {
                Type = typeof(SkillsPatch),
                MethodName = nameof(Next1Arg0_Method)
            };

            public static readonly ReplacementMethodInfo Next1ArgMax = new ReplacementMethodInfo
            {
                Type = typeof(SkillsPatch),
                MethodName = nameof(Next1ArgMax_Method)
            };
        }

        /// <summary>
        /// 内功周天效果功能专用的 Next2Args 替换方法（取最大值）
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "CalcNeigongLoopingEffect");
        }

        /// <summary>
        /// 内功周天效果功能专用的 Next1Arg 替换方法（取0值，减少走火入魔）
        /// </summary>
        public static int Next1Arg0_Method(this IRandomSource randomSource, int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_0_By_Luck(max, "CalcNeigongLoopingEffect");
        }

        /// <summary>
        /// 内功周天效果功能专用的 Next1Arg 替换方法（取最大值）
        /// </summary>
        public static int Next1ArgMax_Method(this IRandomSource randomSource, int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max, "CalcNeigongLoopingEffect");
        }

        /// <summary>
        /// 应用内功周天效果计算补丁
        /// 配置项: CalcNeigongLoopingEffect
        /// 功能: 修改内功周天效果计算，获得更多内力和更少走火入魔
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("CalcNeigongLoopingEffect")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.CombatSkill.CombatSkillDomain),
                MethodName = "CalcNeigongLoopingEffect",
                Parameters = new Type[] { typeof(IRandomSource), typeof(GameData.Domains.Character.Character), typeof(Config.CombatSkillItem), typeof(bool) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CalcNeigongLoopingEffect",
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
            // 1 neili = random.Next(neiliMin, neiliMax + 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    1);

            // 1 qiDisorder = (short)random.Next(qiDisorder + 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    Replacements.Next1Arg0,
                    1);

            // 2 qiDisorder = (short)(-random.Next(-qiDisorder + 1));
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    Replacements.Next1ArgMax,
                    2);
        }
    }
}
