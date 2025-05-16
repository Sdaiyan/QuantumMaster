using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;
using Redzen.Random;
using GameData;
using GameData.Utilities;
using System.Reflection;

namespace QuantumMaster
{

    [PluginConfig("QuantumMaster", "dai", "0.0.1")]
    public class QuantumMaster : TaiwuRemakeHarmonyPlugin
    {
        Harmony harmony;
        public override void Initialize()
        {
            harmony = new Harmony("daige");
            registeBreakVisiblePatch();

        }

        public void registeBreakVisiblePatch()
        {
            // 检查扩展方法是否存在
            var targetMethodInfo = typeof(RedzenHelper).GetMethod(
                "CheckPercentProb",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new Type[] { typeof(IRandomSource), typeof(int) }, // 注意：扩展方法的第一个参数是被扩展的类型
                null);
                
            if (targetMethodInfo == null)
            {
                AdaptableLog.Info("找不到扩展方法 RedzenHelper.CheckPercentProb");
                
                // 列出所有可能的方法
                var methods = typeof(RedzenHelper).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (var method in methods)
                {
                    AdaptableLog.Info($"RedzenHelper 方法: {method.Name}, 参数: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}");
                }
                return;
            }
            
            AdaptableLog.Info($"找到扩展方法: {targetMethodInfo.Name}");

            var _patch = new TranspilerPatchDefinition
            {
                OriginalType = typeof(GameData.Domains.Taiwu.SkillBreakPlate),
                OriginalMethodName = "RandomGridData",
                OriginalMethodParameters = new Type[] { typeof(IRandomSource), typeof(sbyte) },
                Replacements = new List<MethodCallReplacement>
                {
                    new MethodCallReplacement
                    {
                        TargetMethodDeclaringType = typeof(RedzenHelper),
                        TargetMethodName = "CheckPercentProb",
                        // 注意：扩展方法在IL中是静态方法，第一个参数是被扩展的类型
                        TargetMethodParameters = new Type[] { typeof(IRandomSource), typeof(int) },
                        ReplacementMethodDeclaringType = typeof(RandomPath),
                        ReplacementMethodName = "Random_CheckPercentProb_True",
                        TargetOccurrence = 1 // Only replace the 2nd occurrence
                    }
                }
            };

            GenericTranspiler.RegisterPatch("breakVisible", _patch);
            GenericTranspiler.ApplyPatches(harmony);
        }
    }

    public class RandomPath
    {
        public static int Random_Next_2Args_Max(int min, int max)
        {
            return max - 1;
        }

        public static int Random_Next_2Args_Min(int min, int max)
        {
            return min;
        }

        public static int Random_Next_1Arg_Max(int max)
        {
            return max - 1;
        }
        
        public static int Random_Next_1Arg_0(int max)
        {
            return 0;
        }

        // Fix the rest of the methods similarly
        public static bool Random_CheckPercentProb_True(IRandomSource randomSource, int percent)
        {
            if (percent > 0)
            {
                return true;
            }
            return false;
        }

        public static bool Random_CheckPercentProb_False(int percent)
        {
            if (percent < 100)
            {
                return false;
            }
            return true;
        }
        
        public static bool Random_CheckProb_True(IRandomSource a, int b, int percent)
        {
            if (percent > 0)
            {
                return true;
            }
            return false;
        }

        public static bool Random_CheckProb_False(IRandomSource a, int b, int percent)
        {
            if (percent < 100)
            {
                return false;
            }
            return true;
        }
    }
}