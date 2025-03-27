using GameData.Domains;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;
using Redzen.Random;
using GameData.Domains.Taiwu;

namespace QuantumMaster
{

    [PluginConfig("QuantumMaster", "dai", "0.0.1")]
    public class QuantumMaster : TaiwuRemakeHarmonyPlugin
    {
        Harmony harmony;
        public static bool BreakBaseChane; // 突破基础概率
        public static bool BreakVisible; // 突破格可见性
        public static bool BreakValue; // 突破格威力最大
        public static bool BreakFinnalChance; // 突破格最终概率
        public static bool SpecBreak; // 修改突破格子的类型，使其必定是逆行或者嫁衣，逆行的占比可以在下方调整
        public static int SpecBreakRate; // 逆行格子的占比，必须开启突破格子类型修改功能才能生效
        public static Random _qmrd = new Random();
        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }
        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "BreakBaseChane", ref BreakBaseChane);
            DomainManager.Mod.GetSetting(ModIdStr, "BreakVisible", ref BreakVisible);
            DomainManager.Mod.GetSetting(ModIdStr, "BreakValue", ref BreakValue);
            DomainManager.Mod.GetSetting(ModIdStr, "BreakFinnalChance", ref BreakFinnalChance);
            DomainManager.Mod.GetSetting(ModIdStr, "SpecBreak", ref SpecBreak);
            DomainManager.Mod.GetSetting(ModIdStr, "SpecBreakRate", ref SpecBreakRate);
        }
        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(QuantumMaster));
            harmony.PatchAll(typeof(SkillBreakPlateRandomGridPowerPatch));
        }


        // 突破，基础成功率以及可见性
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Taiwu.SkillBreakPlate), "RandomGridData", typeof(IRandomSource), typeof(sbyte))]
        public static void SkillBreakPlate_RandomGridData_HarmonyPostfix(ref SkillBreakPlateGrid __result)
        {
            if (BreakBaseChane) {
                if (__result.SuccessRateFix > 0)
                {
                    __result.SuccessRateFix = 100;
                }
            }
            if (BreakVisible) {
                Traverse.Create(__result).Field("_internalState").SetValue((sbyte)0);
            }
        }

        [HarmonyPatch(typeof(GameData.Domains.Taiwu.SkillBreakPlate), "RandomGridPower")]
        public static class SkillBreakPlateRandomGridPowerPatch
        {
            public static void Postfix(ref short __result, IRandomSource random, ref int power, ref int chance, int powerPerGrid)
            {
                if (BreakValue)
                {
                    __result = (short)powerPerGrid;
                }
            }
        }

        // 最终成功率
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Taiwu.SkillBreakPlate), "CalcSuccessRate")]
        public static void SkillBreakPlate_CalcSuccessRate_HarmonyPostfix(ref short __result)
        {
            if (BreakFinnalChance) {
                if (__result > 0)
                {
                    __result = 100;
                }
            }
        }

        // 生成特殊格子
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Taiwu.SkillBreakPlate), "RandomGridType", typeof(IRandomSource))]
        public static void SkillBreakPlate_RandomGridType_HarmonyPostfix(ref sbyte __result)
        {
            if (SpecBreak) {
                // 17 = 嫁衣 11 = 逆行
                __result = (sbyte)((_qmrd.Next(0, 100) > SpecBreakRate) ? 11 : 17);
            }
        }
    }
}