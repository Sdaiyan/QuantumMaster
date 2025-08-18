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
            var randomValue = QuantumMaster.Random.Next(min, max);
            
            if (luck > 0)
            {
                var result = (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[{min},{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[{min},{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            
            return randomValue;
        }

        /// <summary>
        /// 双参数随机数（倾向最大值）- 带日志输出
        /// </summary>
        public int Random_Next_2Args_Max(int min, int max)
        {
            var result = Calc_Random_Next_2Args_Max_By_Luck(min, max);
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算双参数随机数（倾向最小值）
        /// </summary>
        public int Calc_Random_Next_2Args_Min_By_Luck(int min, int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            var randomValue = QuantumMaster.Random.Next(min, max);
            
            if (luck > 0)
            {
                var result = Math.Max(min, (int)(randomValue - (randomValue - min) * luck));
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[{min},{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Min(max - 1, min + (int)((randomValue - min) * (1 - luck)));
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[{min},{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            
            return randomValue;
        }

        /// <summary>
        /// 双参数随机数（倾向最小值）- 带日志输出
        /// </summary>
        public int Random_Next_2Args_Min(int min, int max)
        {
            var result = Calc_Random_Next_2Args_Min_By_Luck(min, max);
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算单参数随机数（倾向最大值）
        /// </summary>
        public int Calc_Random_Next_1Arg_Max_By_Luck(int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, max);
            
            if (luck > 0)
            {
                var result = Math.Min(max - 1, randomValue + (int)((max - 1 - randomValue) * luck));
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[0,{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Max(0, (int)(randomValue * (1 + luck)));
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[0,{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            
            return randomValue;
        }

        /// <summary>
        /// 单参数随机数（倾向最大值）- 带日志输出
        /// </summary>
        public int Random_Next_1Arg_Max(int max)
        {
            var result = Calc_Random_Next_1Arg_Max_By_Luck(max);
            return result;
        }

        /// <summary>
        /// 根据幸运等级计算单参数随机数（倾向最小值，趋向0）
        /// </summary>
        public int Calc_Random_Next_1Arg_0_By_Luck(int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, max);
            
            if (luck > 0)
            {
                var result = Math.Max(0, randomValue - (int)(randomValue * luck));
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[0,{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Min(max - 1, (int)(randomValue * (1 - luck)));
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[0,{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            
            return randomValue;
        }

        /// <summary>
        /// 单参数随机数（倾向最小值）- 带日志输出
        /// </summary>
        public int Random_Next_1Arg_0(int max)
        {
            var result = Calc_Random_Next_1Arg_0_By_Luck(max);
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
            var originalResult = randomValue < percent;

            if (luck > 0)
            {
                // 提高成功概率
                var adjustedPercent = Math.Min(100, percent + (100 - percent) * luck);
                var result = randomValue < adjustedPercent;
                
                if (originalResult != result)
                {
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 原始概率={percent}%, 判定值={randomValue}, 新概率={adjustedPercent:F2}%, luck={luck:F2}, 原始判定={originalResult} -> 气运调整后={result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                // 降低成功概率
                var adjustedPercent = Math.Max(0, percent * (1 + luck));
                var result = randomValue < adjustedPercent;
                
                if (originalResult != result)
                {
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 原始概率={percent}%, 判定值={randomValue}, 新概率={adjustedPercent:F2}%, luck={luck:F2}, 原始判定={originalResult} -> 气运调整后={result} ***");
                }
                return result;
            }
            
            return originalResult;
        }

        /// <summary>
        /// 百分比概率检查（倾向成功）- 带日志输出
        /// </summary>
        public static bool Random_CheckPercentProb_True(IRandomSource randomSource, int percent)
        {
            var result = Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent);
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
            var originalResult = randomValue >= percent;

            if (luck > 0)
            {
                // 提高失败概率（降低成功概率）
                var adjustedPercent = Math.Max(0, percent - percent * luck);
                var result = randomValue >= adjustedPercent;
                
                if (originalResult != result)
                {
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 原始概率={percent}%, 判定值={randomValue}, 新概率={adjustedPercent:F2}%, luck={luck:F2}, 原始判定={originalResult} -> 气运调整后={result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                // 降低失败概率（提高成功概率）
                var adjustedPercent = Math.Min(100, percent + (100 - percent) * (-luck));
                var result = randomValue >= adjustedPercent;
                
                if (originalResult != result)
                {
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 原始概率={percent}%, 判定值={randomValue}, 新概率={adjustedPercent:F2}%, luck={luck:F2}, 原始判定={originalResult} -> 气运调整后={result} ***");
                }
                return result;
            }
            
            return originalResult;
        }

        /// <summary>
        /// 百分比概率检查（倾向失败）- 带日志输出
        /// </summary>
        public static bool Random_CheckPercentProb_False(IRandomSource randomSource, int percent)
        {
            var result = Calc_Random_CheckPercentProb_False_By_Luck(randomSource, percent);
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
            var originalPercent = (float)chance / total * 100;
            var originalResult = randomValue < chance;

            if (luck > 0)
            {
                // 提高成功概率
                var adjustedChance = Math.Min(total, chance + (total - chance) * luck);
                var result = randomValue < adjustedChance;
                var adjustedPercent = (float)adjustedChance / total * 100;
                
                if (originalResult != result)
                {
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 原始概率={originalPercent:F1}%({chance}/{total}), 判定值={randomValue}, 新概率={adjustedPercent:F1}%({adjustedChance:F2}/{total}), luck={luck:F2}, 原始判定={originalResult} -> 气运调整后={result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                // 降低成功概率
                var adjustedChance = Math.Max(0, chance * (1 + luck));
                var result = randomValue < adjustedChance;
                var adjustedPercent = (float)adjustedChance / total * 100;
                
                if (originalResult != result)
                {
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 原始概率={originalPercent:F1}%({chance}/{total}), 判定值={randomValue}, 新概率={adjustedPercent:F1}%({adjustedChance:F2}/{total}), luck={luck:F2}, 原始判定={originalResult} -> 气运调整后={result} ***");
                }
                return result;
            }
            
            return originalResult;
        }

        /// <summary>
        /// 概率检查（倾向成功）- 带日志输出
        /// </summary>
        public static bool Random_CheckProb_True(IRandomSource randomSource, int chance, int total)
        {
            var result = Calc_Random_CheckProb_True_By_Luck(randomSource, chance, total);
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
            var originalPercent = (float)chance / total * 100;
            var originalResult = randomValue >= chance;

            if (luck > 0)
            {
                // 提高失败概率（降低成功概率）
                var adjustedChance = Math.Max(0, chance - chance * luck);
                var result = randomValue >= adjustedChance;
                var adjustedPercent = (float)adjustedChance / total * 100;
                
                if (originalResult != result)
                {
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 原始概率={originalPercent:F1}%({chance}/{total}), 判定值={randomValue}, 新概率={adjustedPercent:F1}%({adjustedChance:F2}/{total}), luck={luck:F2}, 原始判定={originalResult} -> 气运调整后={result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                // 降低失败概率（提高成功概率）
                var adjustedChance = Math.Min(total, chance + (total - chance) * (-luck));
                var result = randomValue >= adjustedChance;
                var adjustedPercent = (float)adjustedChance / total * 100;
                
                if (originalResult != result)
                {
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 原始概率={originalPercent:F1}%({chance}/{total}), 判定值={randomValue}, 新概率={adjustedPercent:F1}%({adjustedChance:F2}/{total}), luck={luck:F2}, 原始判定={originalResult} -> 气运调整后={result} ***");
                }
                return result;
            }
            
            return originalResult;
        }

        /// <summary>
        /// 概率检查（倾向失败）- 带日志输出
        /// </summary>
        public static bool Random_CheckProb_False(IRandomSource randomSource, int chance, int total)
        {
            var result = Calc_Random_CheckProb_False_By_Luck(randomSource, chance, total);
            return result;
        }

        /// <summary>
        /// 静态方法：根据幸运等级计算双参数随机数（倾向最大值）
        /// </summary>
        public static int Calc_Random_Next_2Args_Max_By_Luck_Static(int min, int max)
        {
            var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
            var randomValue = QuantumMaster.Random.Next(min, max);
            
            if (luck > 0)
            {
                var result = (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[{min},{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
                DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 范围=[{min},{max}), 原始值={randomValue}, 新值={result}, luck={luck:F2} ***");
                return result;
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
            return result;
        }
    }
}
