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
        /// 获取功能显示名称
        /// </summary>
        /// <param name="featureKey">功能键</param>
        /// <returns>功能显示名称</returns>
        private static string GetFeatureName(string featureKey)
        {
            return featureKey ?? "未知";
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
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                int result;
                if (luck > 0)
                {
                    result = (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
                }
                else
                {
                    result = Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
                }
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, [{min},{max}), {randomValue} -> {result}");
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
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                int result;
                if (luck > 0)
                {
                    result = Math.Max(min, (int)(randomValue - (randomValue - min) * luck));
                }
                else
                {
                    result = Math.Min(max - 1, min + (int)((randomValue - min) * (1 - luck)));
                }
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, [{min},{max}), {randomValue} -> {result}");
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
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                int result;
                if (luck > 0)
                {
                    result = Math.Min(max - 1, randomValue + (int)((max - 1 - randomValue) * luck));
                }
                else
                {
                    result = Math.Max(0, (int)(randomValue * (1 + luck)));
                }
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, [0,{max}), {randomValue} -> {result}");
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
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                int result;
                if (luck > 0)
                {
                    result = Math.Max(0, randomValue - (int)(randomValue * luck));
                }
                else
                {
                    result = Math.Min(max - 1, (int)(randomValue * (1 - luck)));
                }
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, [0,{max}), {randomValue} -> {result}");
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
            var originalResult = randomValue <= percent;
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                double adjustedPercent;
                bool result;
                if (luck > 0)
                {
                    adjustedPercent = Math.Min(100, percent + (100 - percent) * luck);
                    result = randomValue <= adjustedPercent;
                }
                else
                {
                    adjustedPercent = Math.Max(0, percent * (1 + luck));
                    result = randomValue <= adjustedPercent;
                }
                string arrow = result ? "<=" : ">";
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, {randomValue} {arrow} {adjustedPercent:F2}%, 原始={originalResult} -> 气运后={result}");
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
            if (percent <= 0) return false;
            if (percent >= 100) return true;

            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, 100);
            var originalResult = randomValue <= percent;
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                double adjustedPercent;
                bool result;
                if (luck > 0)
                {
                    adjustedPercent = Math.Max(0, percent - percent * luck);
                    result = randomValue > adjustedPercent;
                }
                else
                {
                    adjustedPercent = Math.Min(100, percent + (100 - percent) * (-luck));
                    result = randomValue > adjustedPercent;
                }
                string arrow = result ? "<=" : ">";
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, {randomValue} {arrow} {adjustedPercent:F2}%, 原始={originalResult} -> 气运后={result}");
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
            var originalPercent = (float)chance / total * 100;
            var originalResult = randomValue <= chance;
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                double adjustedChance;
                bool result;
                if (luck > 0)
                {
                    adjustedChance = Math.Min(total, chance + (total - chance) * luck);
                }
                else
                {
                    adjustedChance = Math.Max(0, chance * (1 + luck));
                }
                result = randomValue <= adjustedChance;
                string arrow = result ? "<=" : ">";
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, {randomValue} {arrow} {adjustedChance:F2}, 原始={originalResult} -> 气运后={result}");
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
            if (chance <= 0) return false;
            if (chance >= total) return true;

            var luckyLevel = featureKey != null ? ConfigManager.GetFeatureLuckLevel(featureKey) : ConfigManager.LuckyLevel;
            var luck = QuantumMaster.LuckyLevelFactor[luckyLevel];
            var randomValue = QuantumMaster.Random.Next(0, total);
            var originalPercent = (float)chance / total * 100;
            var originalResult = randomValue <= chance;
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                double adjustedChance;
                bool result;
                if (luck > 0)
                {
                    adjustedChance = Math.Max(0, chance - chance * luck);
                }
                else
                {
                    adjustedChance = Math.Min(total, chance + (total - chance) * (-luck));
                }
                result = randomValue <= adjustedChance;
                string arrow = result ? "<=" : ">";
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, {randomValue} {arrow} {adjustedChance:F2}, 原始={originalResult} -> 气运后={result}");
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
            var featureName = GetFeatureName(featureKey);
            if (luck != 0)
            {
                int result;
                if (luck > 0)
                {
                    result = (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
                }
                else
                {
                    result = Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
                }
                DebugLog.Info($"[LR] {featureName}, {luckyLevel}, [{min},{max}), {randomValue} -> {result}");
                return result;
            }
            return randomValue;
        }
    }
}
