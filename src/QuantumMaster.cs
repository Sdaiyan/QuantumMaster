using HarmonyLib;
using GameData.Domains;
using GameData.Utilities;
using TaiwuModdingLib.Core.Plugin;
using Redzen.Random;

namespace QuantumMaster
{

    [PluginConfig("QuantumMaster", "dai", "0.0.1")]
    public class QuantumMaster : TaiwuRemakeHarmonyPlugin
    {
        Harmony harmony;

        public static bool BreakVisible;

        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "steal", ref BreakVisible);
        }
        public override void Initialize()
        {
            harmony = new Harmony("daige");
            patchBreakVisible();
        }

        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
                harmony = null;
            }
        }

        // 补丁方法
        public void patchBreakVisible()
        {
            if (!BreakVisible) return;
            // 使用预设值创建补丁
            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "BreakVisible",
                PatchPresets.OriginalMethods.SkillBreakPlateRandomGridData);

            // 添加扩展方法替换，使用预设值
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                PatchPresets.Replacements.CheckPercentProbTrue,
                1); // 替换第1次出现

            // 应用补丁
            patchBuilder.Apply(harmony);
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

        public static bool Random_CheckPercentProb_False(IRandomSource randomSource, int percent)
        {
            if (percent < 100)
            {
                return false;
            }
            return true;
        }

        public static bool Random_CheckProb_True(IRandomSource randomSource, int chance, int percent)
        {
            if (percent > 0)
            {
                return true;
            }
            return false;
        }

        public static bool Random_CheckProb_False(IRandomSource randomSource, int chance, int percent)
        {
            if (percent < 100)
            {
                return false;
            }
            return true;
        }
    }
}