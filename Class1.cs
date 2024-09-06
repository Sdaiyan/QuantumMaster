using GameData.Domains;
using GameData.Utilities;
using HarmonyLib;
using System.Reflection;
using TaiwuModdingLib.Core.Plugin;
using Character = GameData.Domains.Character.Character;
using Redzen.Random;
using System;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using System.Diagnostics;

namespace QuantumMaster
{
    public static class CharacterHelper
    {
        public static bool IsTaiwu(this Character character)
        {
            return character != null ? character.GetId() == DomainManager.Taiwu.GetTaiwuCharId() : false;
        }

        public static bool IsTaiwuVillagers(this Character character)
        {
            var villagersStatus = DomainManager.Taiwu.GetAllVillagersStatus();
            return character != null ? villagersStatus.Exists(v => v.CharacterId == character.GetId()) : false;
        }

        public static T GetValue<T>(this Character character, string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return character != null ? (T)character.GetType().GetField(fieldName, flags).GetValue(character) : default(T);
        }

        public static void SetValue<T>(this Character character, string fieldName, T value, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            character.GetType().GetField(fieldName, flags).SetValue(character, value);
        }

        public static bool HasAdoredRelaltionWithTaiwu(this Character character)
        {
            return DomainManager.Character.HasRelation(character.GetId(), DomainManager.Taiwu.GetTaiwu().GetId(), 16384)
                || DomainManager.Character.HasRelation(DomainManager.Taiwu.GetTaiwu().GetId(), character.GetId(), 16384);
        }
    }

    [PluginConfig("QuantumMaster", "dai", "0.0.1")]
    public class QuantumMaster : TaiwuRemakeHarmonyPlugin
    {
        Harmony harmony;
        public static int skillGroup1;
        public static int skillGroup2;
        public static int skillType1;
        public static int skillType2;
        public static int skillType3;
        public static int skillType4;
        public static int value1;
        public static int value2;
        public static int count = 0;
        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }

        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "skillGroup1", ref skillGroup1);
            DomainManager.Mod.GetSetting(ModIdStr, "skillGroup2", ref skillGroup2);
            DomainManager.Mod.GetSetting(ModIdStr, "skillType1", ref skillType1);
            DomainManager.Mod.GetSetting(ModIdStr, "skillType2", ref skillType2);
            DomainManager.Mod.GetSetting(ModIdStr, "skillType3", ref skillType3);
            DomainManager.Mod.GetSetting(ModIdStr, "skillType4", ref skillType4);
            DomainManager.Mod.GetSetting(ModIdStr, "value1", ref value1);
            DomainManager.Mod.GetSetting(ModIdStr, "value2", ref value2);
            DomainManager.Mod.GetSetting(ModIdStr, "count ", ref count);
        }
        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(QuantumMaster));
            AdaptableLog.Info("QuantumMaster path");
        }

        public static void handleActionPhase(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            var targetId = targetChar.GetId();
            var allowSuccess = !DomainManager.Character.IsTaiwuPeople(targetId);
            if (allowSuccess)
            {
                __result = 5;
            }
            else
            {
                __result = 0;
            }
        }

        // 目标为太吾或者村民时，偷窃必败，否则必成
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetStealActionPhase")]
        public static bool GetStealActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            handleActionPhase(random, targetChar, ref __result);
            return false;
        }

        // 唬骗
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetScamActionPhase")]
        public static bool GetScamActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            handleActionPhase(random, targetChar, ref __result);
            return false;
        }

        // 抢劫
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetRobActionPhase")]
        public static bool GetRobActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            handleActionPhase(random, targetChar, ref __result);
            return false;
        }

        // 偷学生活技能
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetStealLifeSkillActionPhase")]
        public static bool GetStealLifeSkillActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            handleActionPhase(random, targetChar, ref __result);
            return false;
        }

        // 偷学战斗技能
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetStealCombatSkillActionPhase")]
        public static bool GetStealCombatSkillActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            handleActionPhase(random, targetChar, ref __result);
            return false;
        }

        // 下毒
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetPoisonActionPhase")]
        public static bool GetPoisonActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            handleActionPhase(random, targetChar, ref __result);
            return false;
        }

        // 暗害
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetPlotHarmActionPhase")]
        public static bool GetPlotHarmActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            handleActionPhase(random, targetChar, ref __result);
            return false;
        }

        // 覆盖生成性别
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Gender), "GetRandom")]
        public static bool GenderGetRandom__prefix(ref sbyte __result)
        {
            // 0 = 女 1 = 男
            __result = 0;
            return false;
        }

        // 怀孕概率
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PregnantState), "CheckPregnant")]
        public static bool CheckPregnant__prefix(IRandomSource random, Character father, Character mother, bool isRape, ref bool __result)
        {
            var taiwuid = DomainManager.Taiwu.GetTaiwuCharId();
            if (father.GetId() == taiwuid)
            {
                __result = true;
                return false;
            }
            else if (mother.GetId() == taiwuid)
            {
                __result = true;
                return false;
            }
            else if (isRape)
            {
                __result = true;
                return false;
            }
            else
            {
                return true;
            }
        }

        // 绳子或者煎饼成功率
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHit")]
        public static bool CheckRopeOrSwordHit__prefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        // 绳子或者煎饼成功率 TODO: 需要检查
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHitOutofCombat")]
        public static bool CheckRopeOrSwordHitOutofCombat__prefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        // CheckEscape

        

        public class myRandom : IRandomSource
        {
            public int Next()
            {
                throw new NotImplementedException();
            }
            public int Next(int maxValue)
            {
                AdaptableLog.Info($"Next max {maxValue}");
                return maxValue;
            }
            public int Next(int minValue, int maxValue)
            {
                AdaptableLog.Info($"Next min {minValue} max {maxValue}");
                return maxValue;
            }

            public bool NextBool()
            {
                throw new NotImplementedException();
            }

            public byte NextByte()
            {
                throw new NotImplementedException();
            }

            public void NextBytes(Span<byte> span)
            {
                throw new NotImplementedException();
            }

            public double NextDouble()
            {
                throw new NotImplementedException();
            }

            public double NextDoubleHighRes()
            {
                throw new NotImplementedException();
            }

            public double NextDoubleNonZero()
            {
                throw new NotImplementedException();
            }

            public float NextFloat()
            {
                throw new NotImplementedException();
            }

            public float NextFloatNonZero()
            {
                throw new NotImplementedException();
            }

            public int NextInt()
            {
                throw new NotImplementedException();
            }

            public uint NextUInt()
            {
                throw new NotImplementedException();
            }

            public ulong NextULong()
            {
                throw new NotImplementedException();
            }

            public void Reinitialise(ulong seed)
            {
                throw new NotImplementedException();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.CombatSkill.CombatSkillDomain), "CalcNeigongLoopingEffect")]
        public static void CalcNeigongLoopingEffect_Prefix(ref IRandomSource random, GameData.Domains.Character.Character character)
        {
            if (CharacterHelper.IsTaiwu(character))
            {
                random = new myRandom();
            }
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(RedzenHelper), "CheckPercentProb")]
        public static bool RedzenHelper_CheckPercentProb_Prefix(ref bool __result)
        {
            var stack = new StackTrace();
            if (stack.GetFrames().Exist(f => f.GetMethod()?.Name == "CreateBuildingArea"))
            {
                __result = true;
                return false;
            }
            return true;
        }

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(RedzenHelper), "Next", new Type[] { typeof(int), typeof(int) })]
        // public static bool RedzenHelper_Next_Prefix(ref int __result, int minValue, int MaxValue)
        // {
        //     var stack = new StackTrace();
        //     if (stack.GetFrames().Exist(f => f.GetMethod()?.Name == "CreateBuildingArea"))
        //     {
        //         if (minValue > 0)
        //         {
        //             __result = MaxValue - 1;
        //             return false;
        //         }
        //     }
        //     return true;
        // }


        // 100% 教技能
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(GameData.Domains.Character.Ai.AiHelper.GeneralActionConstants), "GetAskToTeachSkillRespondChance")]
        // public static void GetAskToTeachSkillRespondChance_Postfix(ref sbyte __result)
        // {
        //     __result = (sbyte)(__result > 0 ? 100 : 0);
        // }

        // 教了 100% 学会
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(Character), "GetTaughtNewSkillSuccessRate")]
        // public static void GetTaughtNewSkillSuccessRate_Postfix(ref int __result)
        // {
        //     __result = (int)(__result > 0 ? 100 : 0);
        // }

        // 100% 抓蛐蛐
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Item.ItemDomain), "CatchCricket")]
        public static bool ItemDomain_CatchCricket_HarmonyPrefix(ref short singLevel)
        {
            singLevel = 100;
            return true;
        }
    }
}
