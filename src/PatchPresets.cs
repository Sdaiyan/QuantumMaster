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

            /// <summary>
            /// call int32 Config.Common.ConfigFormulaExtensions::Calculate(class Config.Common.IConfigFormula)
            /// </summary>
            public static readonly ExtensionMethodInfo CalculateFormula0Arg = new ExtensionMethodInfo
            {
                Type = typeof(Config.Common.ConfigFormulaExtensions),
                MethodName = "Calculate",
                Parameters = new Type[] { typeof(Config.Common.IConfigFormula) }
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
        /// 本地函数预设
        /// </summary>
        public static class LocalFunctions
        {
            // // 这里可以添加常用的本地函数预设
            // // 由于本地函数通常是动态生成的，预设可能不如其他类型的方法那么固定
            // // 用户主要通过 AddLocalFunctionReplacement 方法传入具体的本地函数信息
            // // sbyte GetRandomResourceLevel()
            // // {
            // //     sbyte b = (sbyte)random.Next(100);
            // //     if (1 == 0)
            // //     {
            // //     }
            // //     sbyte result = (sbyte)((b < 50) ? 1 : ((sbyte)random.Next(2, 6)));
            // //     if (1 == 0)
            // //     {
            // //     }
            // //     return result;
            // // }
            // public static readonly LocalFunctionInfo GetRandomResourceLevel = new LocalFunctionInfo
            // {
            //     PartialName = "GetRandomResourceLevel",
            //     ReturnType = typeof(sbyte)
            // };

            
            // // sbyte GetRandomUselessResourceLevel()
            // // {
            // //     return (sbyte)random.Next(1, 20);
            // // }
            // public static readonly LocalFunctionInfo GetRandomUselessResourceLevel = new LocalFunctionInfo
            // {
            //     PartialName = "GetRandomUselessResourceLevel",
            //     ReturnType = typeof(sbyte)
            // };
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
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_CheckPercentProb_True"
            };

            /// <summary>
            /// 总是返回 false 的 CheckPercentProb 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbFalse = new ReplacementMethodInfo
            {
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_CheckPercentProb_False"
            };

            /// <summary>
            /// 返回最大值的 Next(min, max) 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_Next_2Args_Max"
            };

            /// <summary>
            /// 返回最小值的 Next(min, max) 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next2ArgsMin = new ReplacementMethodInfo
            {
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_Next_2Args_Min"
            };

            /// <summary>
            /// 返回最大值的 Next(max) 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next1ArgMax = new ReplacementMethodInfo
            {
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_Next_1Arg_Max"
            };

            /// <summary>
            /// 返回0的 Next(max) 替换
            /// </summary>
            public static readonly ReplacementMethodInfo Next1Arg0 = new ReplacementMethodInfo
            {
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_Next_1Arg_0"
            };

            /// <summary>
            /// 总是返回 true 的 CheckProb 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_CheckProb_True"
            };

            /// <summary>
            /// 总是返回 false 的 CheckProb 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckProbFalse = new ReplacementMethodInfo
            {
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_CheckProb_False"
            };

            // public static readonly ReplacementMethodInfo GetRandomResourceLevel5 = new ReplacementMethodInfo
            // {
            //     Type = typeof(LuckyRandomHelper),
            //     MethodName = "Custom_GetRandomResourceLevel"
            // };
            // public static readonly ReplacementMethodInfo GetRandomUselessResourceLevel20 = new ReplacementMethodInfo
            // {
            //     Type = typeof(LuckyRandomHelper),
            //     MethodName = "Custom_GetRandomUselessResourceLevel"
            // };

            // public static int Config.Common.ConfigFormulaExtensions.Calculate(this Config.Common.IConfigFormula formula)
            // Random_Calculate_Max
            public static readonly ReplacementMethodInfo RandomCalculateMax = new ReplacementMethodInfo
            {
                Type = typeof(LuckyRandomHelper),
                MethodName = "Random_Calculate_Max"
            };

        }

    }

}