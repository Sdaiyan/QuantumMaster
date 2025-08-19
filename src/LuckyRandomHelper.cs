using System;
using Redzen.Random;
using Config;

namespace QuantumMaster
{
    /// <summary>
    /// 基于幸运等级的随机数辅助类
    /// 这个类的函数专门用于被 Harmony 补丁替换，不能修改函数签名
    /// 实际的气运计算逻辑在 LuckyCalculator 中
    /// </summary>
    public class LuckyRandomHelper
    {
        /// <summary>
        /// 双参数随机数（倾向最大值）- 用于被 patch 替换
        /// </summary>
        public int Random_Next_2Args_Max(int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max);
        }

        /// <summary>
        /// 双参数随机数（倾向最小值）- 用于被 patch 替换
        /// </summary>
        public int Random_Next_2Args_Min(int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(min, max);
        }

        /// <summary>
        /// 单参数随机数（倾向最大值）- 用于被 patch 替换
        /// </summary>
        public int Random_Next_1Arg_Max(int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max);
        }

        /// <summary>
        /// 单参数随机数（倾向最小值）- 用于被 patch 替换
        /// </summary>
        public int Random_Next_1Arg_0(int max)
        {
            return LuckyCalculator.Calc_Random_Next_1Arg_0_By_Luck(max);
        }

        /// <summary>
        /// 百分比概率检查（倾向成功）- 用于被 patch 替换
        /// </summary>
        public static bool Random_CheckPercentProb_True(IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent);
        }

        /// <summary>
        /// 百分比概率检查（倾向失败）- 用于被 patch 替换
        /// </summary>
        public static bool Random_CheckPercentProb_False(IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_False_By_Luck(randomSource, percent);
        }

        /// <summary>
        /// 概率检查（倾向成功）- 用于被 patch 替换
        /// </summary>
        public static bool Random_CheckProb_True(IRandomSource randomSource, int chance, int total)
        {
            return LuckyCalculator.Calc_Random_CheckProb_True_By_Luck(randomSource, chance, total);
        }

        /// <summary>
        /// 概率检查（倾向失败）- 用于被 patch 替换
        /// </summary>
        public static bool Random_CheckProb_False(IRandomSource randomSource, int chance, int total)
        {
            return LuckyCalculator.Calc_Random_CheckProb_False_By_Luck(randomSource, chance, total);
        }

        /// <summary>
        /// 根据建筑配置公式计算最大值 - 用于被 patch 替换
        /// </summary>
        public static int Random_Calculate_Max(Config.Common.IConfigFormula type)
        {
            var result = 20;
            var buildingFormulaItem = type as Config.BuildingFormulaItem;
            if (buildingFormulaItem != null)
            {
                // 9 = 太吾村 资源等级
                if (buildingFormulaItem.TemplateId == 9)
                {
                    // 1-5
                    result = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck_Static(1, 6);
                }
                // 10 = 非太吾村资源等级
                else if (buildingFormulaItem.TemplateId == 10)
                {
                    // 10-20
                    result = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck_Static(10, 21);
                }
                // 其他的应该只有 8 = 太吾村杂草石头之类的等级
                else
                {
                    // 1-20
                    result = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck_Static(1, 21);
                }
            }
            return result;
        }
    }
}
