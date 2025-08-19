/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;

namespace QuantumMaster.Features.Building
{
    /// <summary>
    /// 建筑区域创建补丁
    /// 配置项: CreateBuildingArea
    /// 功能: 修改建筑区域创建时的随机数生成，使建筑等级更高
    /// </summary>
    public static class CreateBuildingAreaPatch
    {
        /// <summary>
        /// CreateBuildingArea 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// CreateBuildingArea 专用的 Next2Args 最大值替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(CreateBuildingAreaPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };

            /// <summary>
            /// CreateBuildingArea 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(CreateBuildingAreaPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };

            /// <summary>
            /// CreateBuildingArea 专用的 Calculate Max 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CalculateMax = new ReplacementMethodInfo
            {
                Type = typeof(CreateBuildingAreaPatch),
                MethodName = nameof(RandomCalculateMax_Method)
            };
        }

        /// <summary>
        /// CreateBuildingArea 功能专用的 Next2Args 替换方法
        /// 支持 CreateBuildingArea 功能的独立气运设置
        /// </summary>
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "CreateBuildingArea");
        }

        /// <summary>
        /// CreateBuildingArea 功能专用的 CheckPercentProb 替换方法
        /// 支持 CreateBuildingArea 功能的独立气运设置
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "CreateBuildingArea");
        }

        /// <summary>
        /// CreateBuildingArea 功能专用的 RandomCalculateMax 替换方法
        /// 支持 CreateBuildingArea 功能的独立气运设置
        /// </summary>
        public static int RandomCalculateMax_Method(Config.Common.IConfigFormula type)
        {
            var result = 20;
            var buildingFormulaItem = type as Config.BuildingFormulaItem;
            if (buildingFormulaItem != null)
            {
                // 9 = 太吾村 资源等级
                if (buildingFormulaItem.TemplateId == 9)
                {
                    // 1-5 -> 使用 CreateBuildingArea 功能的独立气运设置
                    result = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck_Static(1, 6, "CreateBuildingArea");
                }
                // 10 = 非太吾村资源等级
                else if (buildingFormulaItem.TemplateId == 10)
                {
                    // 10-20 -> 使用 CreateBuildingArea 功能的独立气运设置
                    result = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck_Static(10, 21, "CreateBuildingArea");
                }
                // 其他的应该只有 8 = 太吾村杂草石头之类的等级
                else
                {
                    // 1-20 -> 使用 CreateBuildingArea 功能的独立气运设置
                    result = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck_Static(1, 21, "CreateBuildingArea");
                }
            }
            return result;
        }

        /// <summary>
        /// 应用建筑区域创建补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("CreateBuildingArea")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "CreateBuildingArea",
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(short), typeof(short), typeof(short) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "CreateBuildingArea",
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
            // 2参数 NEXT 方法替换
            // 1 sbyte level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    1);

            // 2 AddElement_BuildingBlocks(buildingBlockKey, new BuildingBlockData(num3, buildingBlockItem.TemplateId, (sbyte)((buildingBlockItem.MaxLevel <= 1) ? 1 : random.Next(1, buildingBlockItem.MaxLevel)), -1), context);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    2);

            // 3 AddBuilding(context, mapAreaId, mapBlockId, num3, buildingBlockItem.TemplateId, (sbyte)((buildingBlockItem.MaxLevel <= 1) ? 1 : random.Next(1, buildingBlockItem.MaxLevel)), buildingAreaWidth);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    3);

            // 5 level = (sbyte)Math.Clamp(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1, buildingBlockItem2.MaxLevel);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    5);

            // 6 level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    6);

            // 7 int isBuild = random.Next(0, 2);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    7);

            // 9 level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                    PatchPresets.InstanceMethods.Next2Args,
                    Replacements.Next2ArgsMax,
                    9);

            // CheckPercentProb 方法替换
            // 1 if (num7 >= 5 || random.CheckPercentProb(num7 * 20))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);

            // 2 if (num9 >= 5 || random.CheckPercentProb(num9 * 20))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    2);

            // Calculate 方法替换
            // 1 sbyte level2 = (sbyte)formula.Calculate();
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CalculateFormula0Arg,
                    Replacements.CalculateMax,
                    1);

            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CalculateFormula0Arg,
                    Replacements.CalculateMax,
                    2);
                    
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CalculateFormula0Arg,
                    Replacements.CalculateMax,
                    3);
        }
    }
}
