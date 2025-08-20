using System;
using Redzen.Random;
using Config;

namespace QuantumMaster
{
    /// <summary>
    /// 气运计算器 - 专门处理基于气运等级的随机数计算
    /// 支持全局气运和功能专属气运设置
    /// </summary>
    public class LuckyCalculator
    {
        /// <summary>
        /// 气运等级名称映射表，对应 Config.lua 中的 LuckyLevel 选项
        /// </summary>
        private static readonly string[] LuckyLevelNames = {
            "命途多舛",
            "时运不济", 
            "顺风顺水",
            "左右逢源",
            "心想事成",
            "福星高照",
            "洪福齐天",
            "气运之子"
        };

        /// <summary>
        /// 获取气运等级的显示名称
        /// </summary>
        /// <param name="luckyLevel">气运等级索引 (0-7)</param>
        /// <returns>气运等级名称</returns>
        private static string GetLuckyLevelName(int luckyLevel)
        {
            if (luckyLevel >= 0 && luckyLevel < LuckyLevelNames.Length)
            {
                return LuckyLevelNames[luckyLevel];
            }
            return "未知";
        }
        /// <summary>
        /// 根据幸运等级计算双参数随机数（倾向最大值）
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>调整后的随机数</returns>
        public static int Calc_Random_Next_2Args_Max_By_Luck(int min, int max, string featureKey = null)
        {
            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(min, max);
            
            if (luck > 0)
            {
                var result = (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            
            return randomValue;
        }

        /// <summary>
        /// 根据幸运等级计算双参数随机数（倾向最小值）
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>调整后的随机数</returns>
        public static int Calc_Random_Next_2Args_Min_By_Luck(int min, int max, string featureKey = null)
        {
            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(min, max);
            
            if (luck > 0)
            {
                var result = Math.Max(min, (int)(randomValue - (randomValue - min) * luck));
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Min(max - 1, min + (int)((randomValue - min) * (1 - luck)));
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            
            return randomValue;
        }

        /// <summary>
        /// 根据幸运等级计算单参数随机数（倾向最大值）
        /// </summary>
        /// <param name="max">最大值</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>调整后的随机数</returns>
        public static int Calc_Random_Next_1Arg_Max_By_Luck(int max, string featureKey = null)
        {
            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, max);
            
            if (luck > 0)
            {
                var result = Math.Min(max - 1, randomValue + (int)((max - 1 - randomValue) * luck));
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Max(0, (int)(randomValue * (1 + luck)));
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            
            return randomValue;
        }

        /// <summary>
        /// 根据幸运等级计算单参数随机数（倾向最小值，趋向0）
        /// </summary>
        /// <param name="max">最大值</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>调整后的随机数</returns>
        public static int Calc_Random_Next_1Arg_0_By_Luck(int max, string featureKey = null)
        {
            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, max);
            
            if (luck > 0)
            {
                var result = Math.Max(0, randomValue - (int)(randomValue * luck));
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Min(max - 1, (int)(randomValue * (1 - luck)));
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            
            return randomValue;
        }

        /// <summary>
        /// 根据幸运等级计算百分比概率（倾向成功）
        /// </summary>
        /// <param name="randomSource">随机数源</param>
        /// <param name="percent">成功概率百分比</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>是否成功</returns>
        public static bool Calc_Random_CheckPercentProb_True_By_Luck(IRandomSource randomSource, int percent, string featureKey = null)
        {
            if (percent <= 0) return false;
            if (percent >= 100) return true;

            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, 100);
            var originalResult = randomValue < percent;

            if (luck > 0)
            {
                // 提高成功概率
                var adjustedPercent = Math.Min(100, percent + (100 - percent) * luck);
                var result = randomValue < adjustedPercent;
                
                // 无气运影响时，不输出日志
                if (originalResult != result)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {percent}% -> {adjustedPercent:F1}% < {randomValue} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                // 降低成功概率
                var adjustedPercent = Math.Max(0, percent * (1 + luck));
                var result = randomValue < adjustedPercent;
                
                // 无气运影响时，不输出日志
                if (originalResult != result)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {percent}% -> {adjustedPercent:F1}% < {randomValue} ***");
                }
                return result;
            }
            
            return originalResult;
        }

        /// <summary>
        /// 根据幸运等级计算百分比概率（倾向失败）
        /// </summary>
        /// <param name="randomSource">随机数源</param>
        /// <param name="percent">失败概率百分比</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>是否失败</returns>
        public static bool Calc_Random_CheckPercentProb_False_By_Luck(IRandomSource randomSource, int percent, string featureKey = null)
        {
            if (percent <= 0) return true;
            if (percent >= 100) return false;

            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, 100);
            var originalResult = randomValue >= percent;

            if (luck > 0)
            {
                // 提高失败概率（降低成功概率）
                var adjustedPercent = Math.Max(0, percent - percent * luck);
                var result = randomValue >= adjustedPercent;
                
                // 无气运影响时，不输出日志
                if (originalResult != result)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {percent}% -> {adjustedPercent:F1}% > {randomValue} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                // 降低失败概率（提高成功概率）
                var adjustedPercent = Math.Min(100, percent + (100 - percent) * (-luck));
                var result = randomValue >= adjustedPercent;
                
                // 无气运影响时，不输出日志
                if (originalResult != result)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {percent}% -> {adjustedPercent:F1}% > {randomValue} ***");
                }
                return result;
            }
            
            return originalResult;
        }

        /// <summary>
        /// 根据幸运等级计算概率检查（倾向成功）
        /// </summary>
        /// <param name="randomSource">随机数源</param>
        /// <param name="chance">成功机会数</param>
        /// <param name="total">总机会数</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>是否成功</returns>
        public static bool Calc_Random_CheckProb_True_By_Luck(IRandomSource randomSource, int chance, int total, string featureKey = null)
        {
            if (chance <= 0) return false;
            if (chance >= total) return true;

            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, total);
            var originalResult = randomValue < chance;

            if (luck > 0)
            {
                // 提高成功概率
                var adjustedChance = Math.Min(total, chance + (total - chance) * luck);
                var result = randomValue < adjustedChance;
                
                // 无气运影响时，不输出日志
                if (originalResult != result)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {chance} -> {adjustedChance:F0} < {randomValue} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                // 降低成功概率
                var adjustedChance = Math.Max(0, chance * (1 + luck));
                var result = randomValue < adjustedChance;
                
                // 无气运影响时，不输出日志
                if (originalResult != result)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {chance} -> {adjustedChance:F0} < {randomValue} ***");
                }
                return result;
            }
            
            return originalResult;
        }

        /// <summary>
        /// 根据幸运等级计算概率检查（倾向失败）
        /// </summary>
        /// <param name="randomSource">随机数源</param>
        /// <param name="chance">失败机会数</param>
        /// <param name="total">总机会数</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>是否失败</returns>
        public static bool Calc_Random_CheckProb_False_By_Luck(IRandomSource randomSource, int chance, int total, string featureKey = null)
        {
            if (chance <= 0) return true;
            if (chance >= total) return false;

            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, total);
            var originalResult = randomValue >= chance;

            if (luck > 0)
            {
                // 提高失败概率（降低成功概率）
                var adjustedChance = Math.Max(0, chance - chance * luck);
                var result = randomValue >= adjustedChance;
                
                // 无气运影响时，不输出日志
                if (originalResult != result)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {chance} -> {adjustedChance:F0} > {randomValue} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                // 降低失败概率（提高成功概率）
                var adjustedChance = Math.Min(total, chance + (total - chance) * (-luck));
                var result = randomValue >= adjustedChance;
                
                // 无气运影响时，不输出日志
                if (originalResult != result)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运改写结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {chance} -> {adjustedChance:F0} > {randomValue} ***");
                }
                return result;
            }
            
            return originalResult;
        }

        /// <summary>
        /// 静态方法：根据幸运等级计算双参数随机数（倾向最大值）
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="featureKey">功能键，用于获取该功能的专属气运等级</param>
        /// <returns>调整后的随机数</returns>
        public static int Calc_Random_Next_2Args_Max_By_Luck_Static(int min, int max, string featureKey = null)
        {
            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(min, max);
            
            if (luck > 0)
            {
                var result = (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            if (luck < 0)
            {
                var result = Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
                // 无气运影响时，不输出日志
                if (result != randomValue)
                {
                    var luckyLevelName = GetLuckyLevelName(luckyLevel);
                    DebugLog.Info($"[LuckyRandom] *** 气运调整结果: 气运等级={luckyLevelName}, 功能={featureKey ?? "全局"}, {randomValue} -> {result} ***");
                }
                return result;
            }
            
            return randomValue;
        }
    }
}
