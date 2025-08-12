using System;
using Redzen.Random;
using Config;

namespace QuantumMaster
{
    /// <summary>
    /// 基于幸运等级的随机数辅助类
    /// 根据玩家的幸运等级调整各种随机结果，提供更有利或不利的结果
    /// </summary>
    public class LuckyRandomHelper
    {
        /// <summary>
        /// 根据幸运等级计算双参数随机数（倾向最大值）
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>调整后的随机数</returns>
        public int Calc_Random_Next_2Args_Max_By_Luck(int min, int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            // 先随机一个结果
            var randomValue = QuantumMaster.Random.Next(min, max);
            DebugLog.Info($"randomValue = {randomValue}");
            if (luck > 0)
            {
                // 获取的数量/概率 = Math.min(当前 + (最大-当前) * 因子, 最大)
                return (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
            }
            if (luck < 0)
            {
                // 获取的数量/概率 = 最小 + (当前-最小) * (1 + 因子)
                return Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
            }
            return randomValue;
        }

        /// <summary>
        /// 双参数随机数（倾向最大值）- 带日志输出
        /// </summary>
        public int Random_Next_2Args_Max(int min, int max)
        {
            var result = Calc_Random_Next_2Args_Max_By_Luck(min, max);
            DebugLog.Info($"Random_Next_2Args_Max min {min} max {max} result {result}");
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算双参数随机数（倾向最小值）
        /// </summary>
        public int Calc_Random_Next_2Args_Min_By_Luck(int min, int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            // 先随机一个结果
            var randomValue = QuantumMaster.Random.Next(min, max);
            DebugLog.Info($"randomValue = {randomValue}");
            if (luck > 0)
            {
                // 获取的数量/概率 = Math.max(当前 - (当前-最小) * 因子, 最小)
                return Math.Max(min, (int)(randomValue - (randomValue - min) * luck));
            }
            if (luck < 0)
            {
                // 获取的数量/概率 = 最小 + (当前-最小) * (1 + 因子)
                return Math.Min(max - 1, min + (int)((randomValue - min) * (1 - luck)));
            }
            return randomValue;
        }

        /// <summary>
        /// 双参数随机数（倾向最小值）- 带日志输出
        /// </summary>
        public int Random_Next_2Args_Min(int min, int max)
        {
            var result = Calc_Random_Next_2Args_Min_By_Luck(min, max);
            DebugLog.Info($"Random_Next_2Args_Min min {min} max {max} result {result}");
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算单参数随机数（倾向最大值）
        /// </summary>
        public int Calc_Random_Next_1Arg_Max_By_Luck(int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            // 先随机一个结果
            var randomValue = QuantumMaster.Random.Next(0, max);
            DebugLog.Info($"randomValue = {randomValue}");
            if (luck > 0)
            {
                // 获取的数量/概率 = Math.min(当前 + (最大-当前) * 因子, 最大-1)
                return Math.Min(max - 1, randomValue + (int)((max - 1 - randomValue) * luck));

                // max = 100 randomValue = 50 luck = 0.2
                // 50 + (99 - 50) * 0.2 = 50 + 9.8 = 59.8

                // max = 100 randomValue = 50 luck = 1
                // 50 + (99 - 50) * 1 = 50 + 49 = 99
            }
            if (luck < 0)
            {
                // 获取的数量/概率 = 0 + (当前-0) * (1 + 因子)
                return Math.Max(0, (int)(randomValue * (1 + luck)));

                // max = 100 randomValue = 50 luck = -0.33
                // 50 * (1 - 0.33) = 50 * 0.67 = 33.5
            }
            return randomValue;
        }

        /// <summary>
        /// 单参数随机数（倾向最大值）- 带日志输出
        /// </summary>
        public int Random_Next_1Arg_Max(int max)
        {
            var result = Calc_Random_Next_1Arg_Max_By_Luck(max);
            DebugLog.Info($"Random_Next_1Arg_Max max {max} result {result}");
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算单参数随机数（倾向最小值，趋向0）
        /// </summary>
        public int Calc_Random_Next_1Arg_0_By_Luck(int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            // 先随机一个结果
            var randomValue = QuantumMaster.Random.Next(0, max);
            DebugLog.Info($"randomValue = {randomValue}");
            if (luck > 0)
            {
                // 获取的数量/概率 = Math.max(当前 - 当前 * 因子, 0)
                return Math.Max(0, randomValue - (int)(randomValue * luck));

                // max = 100 randomValue = 50 luck = 0.2
                // 50 - 50 * 0.2 = 50 - 10 = 40

                // max = 100 randomValue = 50 luck = 1
                // 50 - 50 * 1 = 50 - 50 = 0
            }
            if (luck < 0)
            {
                // 获取的数量/概率 = 0 + (当前-0) * (1 - 因子)
                return Math.Min(max - 1, (int)(randomValue * (1 - luck)));

                // max = 100 randomValue = 50 luck = -0.33
                // 50 * (1 - (-0.33)) = 50 * 1.33 = 66.5
            }
            return randomValue;
        }

        /// <summary>
        /// 单参数随机数（倾向最小值）- 带日志输出
        /// </summary>
        public int Random_Next_1Arg_0(int max)
        {
            var result = Calc_Random_Next_1Arg_0_By_Luck(max);
            DebugLog.Info($"Random_Next_1Arg_0 max {max} result {result}");
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算百分比概率（倾向成功）
        /// </summary>
        public static bool Calc_Random_CheckPercentProb_True_By_Luck(IRandomSource randomSource, int percent)
        {
            if (percent <= 0) return false;
            if (percent >= 100) return true;

            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, 100);
            DebugLog.Info($"randomValue = {randomValue}");

            if (luck > 0)
            {
                // 提高成功概率
                var adjustedPercent = Math.Min(100, percent + (100 - percent) * luck);
                return randomValue < adjustedPercent;
            }
            if (luck < 0)
            {
                // 降低成功概率
                var adjustedPercent = Math.Max(0, percent * (1 + luck));
                return randomValue < adjustedPercent;
            }
            return randomValue < percent;
        }

        /// <summary>
        /// 百分比概率检查（倾向成功）- 带日志输出
        /// </summary>
        public static bool Random_CheckPercentProb_True(IRandomSource randomSource, int percent)
        {
            var result = Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent);
            DebugLog.Info($"Random_CheckPercentProb_True percent {percent} result {result}");
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算百分比概率（倾向失败）
        /// </summary>
        public static bool Calc_Random_CheckPercentProb_False_By_Luck(IRandomSource randomSource, int percent)
        {
            if (percent <= 0) return true;
            if (percent >= 100) return false;

            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, 100);
            DebugLog.Info($"randomValue = {randomValue}");

            if (luck > 0)
            {
                // 提高失败概率（降低成功概率）
                var adjustedPercent = Math.Max(0, percent - percent * luck);
                return randomValue >= adjustedPercent;
            }
            if (luck < 0)
            {
                // 降低失败概率（提高成功概率）
                var adjustedPercent = Math.Min(100, percent + (100 - percent) * (-luck));
                return randomValue >= adjustedPercent;
            }
            return randomValue >= percent;
        }

        /// <summary>
        /// 百分比概率检查（倾向失败）- 带日志输出
        /// </summary>
        public static bool Random_CheckPercentProb_False(IRandomSource randomSource, int percent)
        {
            var result = Calc_Random_CheckPercentProb_False_By_Luck(randomSource, percent);
            DebugLog.Info($"Random_CheckPercentProb_False percent {percent} result {result}");
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算概率检查（倾向成功）
        /// </summary>
        public static bool Calc_Random_CheckProb_True_By_Luck(IRandomSource randomSource, int chance, int total)
        {
            if (chance <= 0) return false;
            if (chance >= total) return true;

            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, total);
            DebugLog.Info($"randomValue = {randomValue}");

            if (luck > 0)
            {
                // 提高成功概率
                var adjustedChance = Math.Min(total, chance + (total - chance) * luck);
                return randomValue < adjustedChance;
            }
            if (luck < 0)
            {
                // 降低成功概率
                var adjustedChance = Math.Max(0, chance * (1 + luck));
                return randomValue < adjustedChance;
            }
            return randomValue < chance;
        }

        /// <summary>
        /// 概率检查（倾向成功）- 带日志输出
        /// </summary>
        public static bool Random_CheckProb_True(IRandomSource randomSource, int chance, int total)
        {
            var result = Calc_Random_CheckProb_True_By_Luck(randomSource, chance, total);
            DebugLog.Info($"Random_CheckProb_True chance {chance} total {total} result {result}");
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算概率检查（倾向失败）
        /// </summary>
        public static bool Calc_Random_CheckProb_False_By_Luck(IRandomSource randomSource, int chance, int total)
        {
            if (chance <= 0) return true;
            if (chance >= total) return false;

            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, total);
            DebugLog.Info($"randomValue = {randomValue}");

            if (luck > 0)
            {
                // 提高失败概率（降低成功概率）
                var adjustedChance = Math.Max(0, chance - chance * luck);
                return randomValue >= adjustedChance;
            }
            if (luck < 0)
            {
                // 降低失败概率（提高成功概率）
                var adjustedChance = Math.Min(total, chance + (total - chance) * (-luck));
                return randomValue >= adjustedChance;
            }
            return randomValue >= chance;
        }

        /// <summary>
        /// 概率检查（倾向失败）- 带日志输出
        /// </summary>
        public static bool Random_CheckProb_False(IRandomSource randomSource, int chance, int total)
        {
            var result = Calc_Random_CheckProb_False_By_Luck(randomSource, chance, total);
            DebugLog.Info($"Random_CheckProb_False chance {chance} total {total} result {result}");
            return result;
        }

        /// <summary>
        /// 静态方法：根据幸运等级计算双参数随机数（倾向最大值）
        /// </summary>
        public static int Calc_Random_Next_2Args_Max_By_Luck_Static(int min, int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            // 先随机一个结果
            var randomValue = QuantumMaster.Random.Next(min, max);
            DebugLog.Info($"randomValue = {randomValue}");
            if (luck > 0)
            {
                // 获取的数量/概率 = Math.min(当前 + (最大-当前) * 因子, 最大)
                return (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
            }
            if (luck < 0)
            {
                // 获取的数量/概率 = 最小 + (当前-最小) * (1 + 因子)
                return Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
            }
            return randomValue;
        }

        /// <summary>
        /// 根据建筑配置公式计算最大值
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
                    result = Calc_Random_Next_2Args_Max_By_Luck_Static(1, 6);
                }
                // 10 = 非太吾村资源等级
                else if (buildingFormulaItem.TemplateId == 10)
                {
                    // 10-20
                    result = Calc_Random_Next_2Args_Max_By_Luck_Static(10, 21);
                }
                // 其他的应该只有 8 = 太吾村杂草石头之类的等级
                else
                {
                    // 1-20
                    result = Calc_Random_Next_2Args_Max_By_Luck_Static(1, 21);
                }
            }
            
            DebugLog.Info($"Random_Calculate_5 result {result}");
            return result;
        }
    }
}
