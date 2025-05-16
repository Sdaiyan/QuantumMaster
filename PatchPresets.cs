using GameData.Utilities;
using Redzen.Random;
using System;
using QuantumMaster;

namespace QuantumMaster
{
    /// <summary>
    /// 包含常用补丁预设的静态类
    /// </summary>
    public static class PatchPresets
    {
        /// <summary>
        /// 常用扩展方法预设
        /// </summary>
        public static class Extensions
        {
            /// <summary>
            /// RedzenHelper.CheckPercentProb 扩展方法
            /// </summary>
            public static readonly ExtensionMethodInfo CheckPercentProb = new ExtensionMethodInfo
            {
                Type = typeof(RedzenHelper),
                MethodName = "CheckPercentProb",
                Parameters = new Type[] { typeof(IRandomSource), typeof(int) }
            };

            // 可以添加更多常用扩展方法...
        }

        /// <summary>
        /// 常用替换方法预设
        /// </summary>
        public static class Replacements
        {
            /// <summary>
            /// 总是返回 true 的 CheckPercentProb 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(RandomPath),
                MethodName = "Random_CheckPercentProb_True"
            };
            
            /// <summary>
            /// 总是返回 false 的 CheckPercentProb 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbFalse = new ReplacementMethodInfo
            {
                Type = typeof(RandomPath),
                MethodName = "Random_CheckPercentProb_False"
            };

            // 可以添加更多常用替换方法...
        }

        /// <summary>
        /// 常用原始方法预设
        /// </summary>
        public static class OriginalMethods
        {
            /// <summary>
            /// SkillBreakPlate.RandomGridData 方法
            /// </summary>
            public static readonly OriginalMethodInfo SkillBreakPlateRandomGridData = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.SkillBreakPlate),
                MethodName = "RandomGridData",
                Parameters = new Type[] { typeof(IRandomSource), typeof(sbyte) }
            };

            // 可以添加更多常用原始方法...
        }
    }
}