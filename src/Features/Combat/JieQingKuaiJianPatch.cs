/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Combat
{
    /// <summary>
    /// 界青快剑补丁
    /// 配置项: JieQingKuaiJian
    /// 功能: 【气运】界青快剑重复触发概率增加
    /// </summary>
    public static class JieQingKuaiJianPatch
    {
        /// <summary>
        /// JieQingKuaiJian 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// JieQingKuaiJian 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(JieQingKuaiJianPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// JieQingKuaiJian 功能专用的 CheckPercentProb 替换方法
        /// 支持 JieQingKuaiJian 功能的独立气运设置
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "JieQingKuaiJian");
        }

        /// <summary>
        /// 应用界青快剑补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("JieQingKuaiJian")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.Jieqingmen.Sword.JieQingKuaiJian),
                MethodName = "OnCastSkillEnd",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(int), 
                    typeof(bool), 
                    typeof(short), 
                    typeof(sbyte), 
                    typeof(bool)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "JieQingKuaiJian",
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
            // CheckPercentProb 方法替换 - 替换第1次出现，期望结果为 true
            // 1. if (context.Random.CheckPercentProb(odds))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);
        }
    }
}