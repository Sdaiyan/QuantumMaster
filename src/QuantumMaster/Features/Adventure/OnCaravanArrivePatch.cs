/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Adventure
{
    /// <summary>
    /// 商队收益暴击率补丁
    /// 配置项: OnCaravanArrive
    /// 功能: 修改商队到达时获得暴击收益的概率，根据气运提升暴击概率
    /// </summary>
    public static class OnCaravanArrivePatch
    {
        /// <summary>
        /// OnCaravanArrive 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// OnCaravanArrive 专用的 Next 0 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next1Arg0 = new ReplacementMethodInfo
            {
                Type = typeof(OnCaravanArrivePatch),
                MethodName = nameof(Next1Arg0_Method)
            };
        }

        /// <summary>
        /// OnCaravanArrive 功能专用的 Next 替换方法
        /// 支持 OnCaravanArrive 功能的独立气运设置，倾向返回0（提升暴击概率）
        /// </summary>
        public static int Next1Arg0_Method(this IRandomSource randomSource, int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_0_By_Luck(max, "OnCaravanArrive");
        }

        /// <summary>
        /// 应用商队收益暴击率补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("OnCaravanArrive")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Merchant.MerchantDomain),
                MethodName = "OnCaravanArrive",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext),
                    typeof(int),
                    typeof(GameData.Domains.Merchant.CaravanExtraData),
                    typeof(GameData.Domains.Merchant.MerchantData),
                    typeof(short)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "OnCaravanArrive",
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
            // Next 单参数方法替换 - 第1次出现，倾向0（提升暴击概率）
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next1Arg,
                    Replacements.Next1Arg0,
                    1);
        }
    }
}