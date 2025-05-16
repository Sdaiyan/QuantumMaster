using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;
using Redzen.Random;
using GameData;
using GameData.Utilities;
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
                        TargetMethodParameters = new Type[] { typeof(int) },
                        ReplacementMethodDeclaringType = typeof(RandomPath),
                        ReplacementMethodName = "Random_CheckPercentProb_True",
                        TargetOccurrence = 1 // Only replace the 2nd occurrence
                    }
                }
            };

            GenericTranspiler.RegisterPatch("breakVisible", _patch);

            // Apply all patches
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
        public static bool Random_CheckPercentProb_True(int percent)
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