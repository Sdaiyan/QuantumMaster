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

            /// <summary>
            /// IRandomSource.CheckProb 扩展方法
            /// </summary>
            public static readonly ExtensionMethodInfo CheckProb = new ExtensionMethodInfo
            {
                Type = typeof(RedzenHelper),
                MethodName = "CheckProb",
                Parameters = new Type[] { typeof(IRandomSource), typeof(int), typeof(int) }
            };
        }
        
        /// <summary>
        /// 接口/实例方法预设
        /// </summary>
        public static class InstanceMethods
        {
            /// <summary>
            /// IRandomSource.Next(int, int) 方法
            /// </summary>
            public static readonly InstanceMethodInfo Next2Args = new InstanceMethodInfo
            {
                Type = typeof(IRandomSource),
                MethodName = "Next",
                Parameters = new Type[] { typeof(int), typeof(int) }
            };

            /// <summary>
            /// IRandomSource.Next(int) 方法
            /// </summary>
            public static readonly InstanceMethodInfo Next1Arg = new InstanceMethodInfo
            {
                Type = typeof(IRandomSource),
                MethodName = "Next",
                Parameters = new Type[] { typeof(int) }
            };
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

            /// <summary>
            /// 返回最大值的 Next(min, max) 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(RandomPath),
                MethodName = "Random_Next_2Args_Max"
            };

            /// <summary>
            /// 返回最小值的 Next(min, max) 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMin = new ReplacementMethodInfo
            {
                Type = typeof(RandomPath),
                MethodName = "Random_Next_2Args_Min"
            };

            /// <summary>
            /// 返回最大值的 Next(max) 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next1ArgMax = new ReplacementMethodInfo
            {
                Type = typeof(RandomPath),
                MethodName = "Random_Next_1Arg_Max"
            };

            /// <summary>
            /// 返回0的 Next(max) 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next1Arg0 = new ReplacementMethodInfo
            {
                Type = typeof(RandomPath),
                MethodName = "Random_Next_1Arg_0"
            };

            /// <summary>
            /// 总是返回 true 的 CheckProb 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(RandomPath),
                MethodName = "Random_CheckProb_True"
            };

            /// <summary>
            /// 总是返回 false 的 CheckProb 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckProbFalse = new ReplacementMethodInfo
            {
                Type = typeof(RandomPath),
                MethodName = "Random_CheckProb_False"
            };
        }

    }

}