﻿using GameData.Domains;
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
using GameData.Domains.Map;
using Config;
using GameData.Domains.Item;
using static QuantumMaster.QuantumMaster;
using GameData.Domains.Taiwu;
using GameData.Domains.Extra;
using GameData.Domains.Building;
using System.Drawing;

namespace QuantumMaster
{

    [PluginConfig("QuantumMaster", "dai", "0.0.1")]
    public class QuantumMaster : TaiwuRemakeHarmonyPlugin
    {
        Harmony harmony;
        // public static int pregnant; // 怀孕概率
        public static bool steal; // 偷窃必定成功
        public static bool scam; // 唬骗必定成功
        public static bool rob; // 抢劫必定成功
        public static bool stealLifeSkill; // 偷学生活技能必定成功
        public static bool stealCombatSkill; // 偷学战斗技能必定成功
        public static bool poison; // 下毒必定成功
        public static bool plotHarm; // 暗害必定成功
        public static bool gender0; // 生成性别
        public static bool gender1; // 生成性别
        public static bool ropeOrSword; // 如果概率不为0，绳子绑架或者煎饼救人必定成功
        public static bool industryBlockLevel; // 初始建筑等级生成时必定为浮动区间的上限
        public static bool collectResource; // 收获资源时必定获取引子，且是可能获取的最高级的引子
        public static bool GetCollectResourceAmount; // 采集数量必定为浮动区间的上限
        public static bool UpdateResourceBlock; // 如果概率不为0，过月时，对应的产业资源必定升级与扩张
        public static bool OfflineUpdateShopManagement; // 如果概率不为0，产业建筑经营、招募必然成功、村民技艺必定提升
        public static bool ApplyLifeSkillCombatResult; // 如果概率不为0，较艺读书必定触发
        public static bool CalcReadInCombat; // 如果概率不为0，战斗读书必定触发
        public static bool CalcLootItem; // 如果概率不为0，战利品掉落判定必定通过（原本逻辑是对每个战利品进行判断是否掉落）
        public static bool InitPathContent; // 奇遇收获资源时，数量为浮动区间的上限
        public static bool TaiwuResourceGrow; // 峨眉恩义太吾村资源生长时，数量为浮动区间的上限
        public static bool GetResourceBlockGrowthChance; // 峨眉恩义太吾村资源升级判定时，如果概率不是0，那么这个资源点必定升级
        public static bool GetStrategyProgressAddValue; // 读书策略（独见独知）进度增加为浮动区间的上限
        public static bool ApplyImmediateReadingStrategyEffectForLifeSkill; // 技艺书籍的效率增加策略（奇思妙想）进度增加为浮动区间的上限值
        public static bool ApplyImmediateReadingStrategyEffectForCombatSkill; // 功法书籍的效率增加策略（奇思妙想）进度增加为浮动区间的上限值
        public static bool ParallelUpdateOnMonthChange; // 地块每月资源恢复数量为浮动区间的上限
        public static bool GetAskToTeachSkillRespondChance; // 如果概率不是0，则必定会指点别人
        public static bool GetTaughtNewSkillSuccessRate; // 如果概率不为0，接受指点的人必定能学习成功
        public static bool CatchCricket; // 抓蛐蛐必定成功
        public static bool InitResources; // 生成世界时，每个地块上的资源为浮动区间的最大值，受到难度的影响
        public static bool CheckCricketIsSmart; // 蛐蛐是否可以升级，如果符合条件必定升级
        public static bool GetCurrReadingEventBonusRate; // 灵光一闪概率不为0时，必定灵光一闪
        public static bool GeneratePageIncompleteState; // 生成书籍时，完整的书页为浮动区间的最大值，亡佚书页为浮动区间最小值，并且完整的书页会出现在书本的前篇位置
        public static bool CricketInitialize; // 生成蛐蛐时，必定生成耐久上限为理论上限值的，不受伤的蛐蛐
        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }
        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "steal", ref steal);
            DomainManager.Mod.GetSetting(ModIdStr, "scam", ref scam);
            DomainManager.Mod.GetSetting(ModIdStr, "rob", ref rob);
            DomainManager.Mod.GetSetting(ModIdStr, "stealLifeSkill", ref stealLifeSkill);
            DomainManager.Mod.GetSetting(ModIdStr, "stealCombatSkill", ref stealCombatSkill);
            DomainManager.Mod.GetSetting(ModIdStr, "poison", ref poison);
            DomainManager.Mod.GetSetting(ModIdStr, "plotHarm", ref plotHarm);
            DomainManager.Mod.GetSetting(ModIdStr, "gender0", ref gender0);
            DomainManager.Mod.GetSetting(ModIdStr, "gender1", ref gender1);
            DomainManager.Mod.GetSetting(ModIdStr, "ropeOrSword", ref ropeOrSword);
            DomainManager.Mod.GetSetting(ModIdStr, "industryBlockLevel", ref industryBlockLevel);
            DomainManager.Mod.GetSetting(ModIdStr, "collectResource", ref collectResource);
            DomainManager.Mod.GetSetting(ModIdStr, "GetCollectResourceAmount", ref GetCollectResourceAmount);
            DomainManager.Mod.GetSetting(ModIdStr, "UpdateResourceBlock", ref UpdateResourceBlock);
            DomainManager.Mod.GetSetting(ModIdStr, "OfflineUpdateShopManagement", ref OfflineUpdateShopManagement);
            DomainManager.Mod.GetSetting(ModIdStr, "ApplyLifeSkillCombatResult", ref ApplyLifeSkillCombatResult);
            DomainManager.Mod.GetSetting(ModIdStr, "CalcReadInCombat", ref CalcReadInCombat);
            DomainManager.Mod.GetSetting(ModIdStr, "CalcLootItem", ref CalcLootItem);
            DomainManager.Mod.GetSetting(ModIdStr, "InitPathContent", ref InitPathContent);
            DomainManager.Mod.GetSetting(ModIdStr, "TaiwuResourceGrow", ref TaiwuResourceGrow);
            DomainManager.Mod.GetSetting(ModIdStr, "GetResourceBlockGrowthChance", ref GetResourceBlockGrowthChance);
            DomainManager.Mod.GetSetting(ModIdStr, "GetStrategyProgressAddValue", ref GetStrategyProgressAddValue);
            DomainManager.Mod.GetSetting(ModIdStr, "ApplyImmediateReadingStrategyEffectForLifeSkill", ref ApplyImmediateReadingStrategyEffectForLifeSkill);
            DomainManager.Mod.GetSetting(ModIdStr, "ApplyImmediateReadingStrategyEffectForCombatSkill", ref ApplyImmediateReadingStrategyEffectForCombatSkill);
            DomainManager.Mod.GetSetting(ModIdStr, "ParallelUpdateOnMonthChange", ref ParallelUpdateOnMonthChange);
            DomainManager.Mod.GetSetting(ModIdStr, "GetAskToTeachSkillRespondChance", ref GetAskToTeachSkillRespondChance);
            DomainManager.Mod.GetSetting(ModIdStr, "GetTaughtNewSkillSuccessRate", ref GetTaughtNewSkillSuccessRate);
            DomainManager.Mod.GetSetting(ModIdStr, "CatchCricket", ref CatchCricket);
            DomainManager.Mod.GetSetting(ModIdStr, "InitResources", ref InitResources);
            DomainManager.Mod.GetSetting(ModIdStr, "CheckCricketIsSmart", ref CheckCricketIsSmart);
            DomainManager.Mod.GetSetting(ModIdStr, "GetCurrReadingEventBonusRate", ref GetCurrReadingEventBonusRate);
            DomainManager.Mod.GetSetting(ModIdStr, "GeneratePageIncompleteState", ref GeneratePageIncompleteState);
            DomainManager.Mod.GetSetting(ModIdStr, "CricketInitialize", ref CricketInitialize);
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
            if (steal) {
                handleActionPhase(random, targetChar, ref __result);
                return false;
            }
            return true;
        }
        // 唬骗
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetScamActionPhase")]
        public static bool GetScamActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            if (scam) {
                handleActionPhase(random, targetChar, ref __result);
                return false;
            }
            return true;
        }
        // 抢劫
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetRobActionPhase")]
        public static bool GetRobActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            if (rob) {
                handleActionPhase(random, targetChar, ref __result);
                return false;
            }
            return true;
        }
        // 偷学生活技能
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetStealLifeSkillActionPhase")]
        public static bool GetStealLifeSkillActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            if (stealLifeSkill) {
                handleActionPhase(random, targetChar, ref __result);
                return false;
            }
            return true;
        }
        // 偷学战斗技能
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetStealCombatSkillActionPhase")]
        public static bool GetStealCombatSkillActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            if (stealCombatSkill) {
                handleActionPhase(random, targetChar, ref __result);
                return false;
            }
            return true;
        }
        // 下毒
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetPoisonActionPhase")]
        public static bool GetPoisonActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            if (poison) {
                handleActionPhase(random, targetChar, ref __result);
                return false;
            }
            return true;
        }
        // 暗害
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetPlotHarmActionPhase")]
        public static bool GetPlotHarmActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result)
        {
            if (plotHarm) {
                handleActionPhase(random, targetChar, ref __result);
                return false;
            }
            return true;
        }
        // 覆盖生成性别
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Gender), "GetRandom")]
        public static bool GenderGetRandom__prefix(ref sbyte __result)
        {
            if (gender0) {
                // 0 = 女 1 = 男
                __result = (sbyte)0
                return false;
            }
            if (gender1) {
                __result = (sbyte)1
                return false;
            }
            return true;
        }
        // 怀孕概率
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(PregnantState), "CheckPregnant")]
        // public static bool CheckPregnant__prefix(IRandomSource random, Character father, Character mother, bool isRape, ref bool __result)
        // {
        //     var taiwuid = DomainManager.Taiwu.GetTaiwuCharId();
        //     if (father.GetId() == taiwuid)
        //     {
        //         __result = true;
        //         return false;
        //     }
        //     else if (mother.GetId() == taiwuid)
        //     {
        //         __result = true;
        //         return false;
        //     }
        //     else if (isRape)
        //     {
        //         __result = true;
        //         return false;
        //     }
        //     else
        //     {
        //         return true;
        //     }
        // }
        // 绳子或者煎饼成功率
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHit")]
        public static bool CheckRopeOrSwordHit__prefix(ref bool __result)
        {
            if (ropeOrSword) {
                __result = true;
                return false;
            }
            return true;
        }
        // 绳子或者煎饼成功率 TODO: 需要检查
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHitOutofCombat")]
        // public static bool CheckRopeOrSwordHitOutofCombat__prefix(ref bool __result)
        // {
        //     __result = true;
        //     return false;
        // }
        // CheckEscape


        public class myRandom : IRandomSource
        {
            public int Next()
            {
                throw new NotImplementedException();
            }
            public int Next(int maxValue)
            {
                // AdaptableLog.Info($"Next max {maxValue}");
                return maxValue - 1;
            }
            public int Next(int minValue, int maxValue)
            {
                // AdaptableLog.Info($"Next min {minValue} max {maxValue}");
                return maxValue - 1;
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
            var taiwuid = DomainManager.Taiwu.GetTaiwuCharId();
            var id = character.GetId();
            if (taiwuid == id)
            {
                random = new myRandom();
            }
        }


        private static List<string> GetCallerMethodNames()
        {
            const int frameCount = 3;
            var stack = new StackTrace(frameCount);
            var methodNames = new List<string>();
            foreach (var frame in stack.GetFrames())
            {
                var method = frame.GetMethod();
                if (method != null)
                {
                    methodNames.Add(method.Name);
                }
            }
            return methodNames;
        }
        private static string IsMethodInList(List<string> methodNames, HashSet<string> methodList)
        {
            // AdaptableLog.Info($"Method list: {string.Join(", ", methodList)}");
            foreach (var methodName in methodNames)
            {
                var isInList = methodList.Contains(methodName);
                // 打 log，同时打印 methodList 和 methodName
                if (isInList)
                {
                    // AdaptableLog.Info($"Method {methodName} is in list");
                    // AdaptableLog.Info($"Method list: {string.Join(", ", methodList)}");
                    return methodName;
                }
            }
            return null;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RedzenHelper), "CheckPercentProb")]
        public static bool RedzenHelper_CheckPercentProb_Prefix(ref bool __result, int percentProb)
        {
            var methodList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {};
            // 初始建筑等级
            if (industryBlockLevel)
            {
                methodList.Add("CreateBuildingArea");
            }
            if (collectResource)
            {
                methodList.Add("CollectResource");
            }
            if (UpdateResourceBlock)
            {
                methodList.Add("UpdateResourceBlock");
            }
            if (OfflineUpdateShopManagement)
            {
                methodList.Add("OfflineUpdateShopManagement");
            }
            if (ApplyLifeSkillCombatResult)
            {
                methodList.Add("ApplyLifeSkillCombatResult");
            }
            if (CalcReadInCombat)
            {
                methodList.Add("CalcReadInCombat");
            }
            if (CalcLootItem)
            {
                methodList.Add("CalcLootItem");
            }
            
            var hitNames = GetCallerMethodNames();
            var hitName = IsMethodInList(hitNames, methodList);
            if (hitName != null && percentProb > 0)
            {
                // AdaptableLog.Info($"Method CheckPercentProb {hitName}");
                __result = true;
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RandomSourceBase), "Next", new Type[] { typeof(int), typeof(int) })]
        public static bool RedzenHelper_Next_Prefix(ref int __result, int minValue, int maxValue)
        {
            var methodList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // "UpdateStoneRoomData", // 石室更新
            };
            var noZero = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "OfflineUpdateShopManagement", // 招募成功率
                "CreateBuildingArea",
                "InitPathContent"
            };
            if (OfflineUpdateShopManagement) {
                methodList.Add("OfflineUpdateShopManagement");
            }
            if (GetCollectResourceAmount) {
                methodList.Add("GetCollectResourceAmount");
            }
            if (CreateBuildingArea) {
                methodList.Add("CreateBuildingArea");
            }
            if (InitPathContent) {
                methodList.Add("InitPathContent");
            }
            if (TaiwuResourceGrow) {
                methodList.Add("TaiwuResourceGrow");
            }
            if (GetStrategyProgressAddValue) {
                methodList.Add("GetStrategyProgressAddValue");
            }
            if (ApplyImmediateReadingStrategyEffectForLifeSkill) {
                methodList.Add("ApplyImmediateReadingStrategyEffectForLifeSkill");
            }
            if (ApplyImmediateReadingStrategyEffectForCombatSkill) {
                methodList.Add("ApplyImmediateReadingStrategyEffectForCombatSkill");
            }
            var hitNames = GetCallerMethodNames();
            var hitName = IsMethodInList(hitNames, methodList);
            if (hitName != null)
            {
                // AdaptableLog.Info($"Next 2 args {hitName}, minValue {minValue}, maxValue {maxValue}");
                if (noZero.Contains(hitName) && minValue > 0)
                {
                    __result = maxValue - 1;
                    return false;
                }
                else
                {
                    __result = maxValue - 1;
                    return false;
                }
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RandomSourceBase), "Next", new Type[] { typeof(int) })]
        public static bool RedzenHelper_Next1_Prefix(ref int __result, int maxValue)
        {
            var methodList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            };
            if (ParallelUpdateOnMonthChange) {
                methodList.Add("ParallelUpdateOnMonthChange");
            }
            var hitNames = GetCallerMethodNames();
            var hitName = IsMethodInList(hitNames, methodList);
            if (hitName != null)
            {
                // AdaptableLog.Info($"Next 1 args {hitName}, maxValue {maxValue}");
                __result = maxValue - 1;
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RedzenHelper), "CheckProb", new Type[] { typeof(IRandomSource), typeof(int), typeof(int) })]
        public static bool RedzenHelper_CheckProb_Prefix(ref bool __result)
        {
            var methodList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            };
            var hitNames = GetCallerMethodNames();
            var hitName = IsMethodInList(hitNames, methodList);
            if (OfflineUpdateShopManagement) {
                methodList.Add("OfflineUpdateShopManagement");
            }
            if (hitName != null)
            {
                // AdaptableLog.Info($"CheckProb args {hitName}");
                __result = true;
                return false;
            }
            return true;
        }
        // 100% 教技能
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Character.Ai.AiHelper.GeneralActionConstants), "GetAskToTeachSkillRespondChance")]
        public static void GetAskToTeachSkillRespondChance_Postfix(ref int __result)
        {
            if (GetAskToTeachSkillRespondChance) {
                __result = (int)(__result > 0 ? 100 : 0);
            }
        }
        // 如果概率不为0，接受指点的人必定能学习成功
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetTaughtNewSkillSuccessRate")]
        public static void GetTaughtNewSkillSuccessRate_Postfix(ref int __result)
        {
            if (GetTaughtNewSkillSuccessRate)
            {
                __result = 100;
            }
        }
        // 100% 抓蛐蛐
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Item.ItemDomain), "CatchCricket")]
        public static void ItemDomain_CatchCricket_HarmonyPrefix(ref short singLevel)
        {
            if (CatchCricket)
            {
                singLevel = 100;
            }
        }
        // 教人不消耗能量
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(GameData.Domains.Character.Ai.ActionEnergySbytes), "SpendEnergyOnAction")]
        // public static bool SpendEnergyOnAction_HarmonyPrefix(sbyte actionEnergyType)
        // {
        //     if (actionEnergyType == 4)
        //     {
        //         return false;
        //     }
        //     return true;
        // }
        // 教人能量检查永远足够
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(GameData.Domains.Character.Ai.ActionEnergySbytes), "HasEnoughForAction")]
        // public static bool HasEnoughForAction_HarmonyPrefix(ref bool __result, sbyte actionEnergyType)
        // {
        //     if (actionEnergyType == 4)
        //     {
        //         __result = true;
        //         return false;
        //     }
        //     return true;
        // }
        // 地块升级概率，有可能就是100
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingDomain), "GetResourceBlockGrowthChance")]
        public static void GetResourceBlockGrowthChance_HarmonyPostfix(ref sbyte __result)
        {
            if (GetResourceBlockGrowthChance)
            {
                if (__result > 0)
                {
                    __result = 100;
                }
            }
        }
        // 地块拓展概率，有可能就是100
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(BuildingDomain), "GetResourceBlockExpandChance")]
        // public static void GetResourceBlockExpandChance_HarmonyPostfix(ref sbyte __result)
        // {
        //     if (__result > 0)
        //     {
        //         __result = 100;
        //     }
        // }
        // 初始化地块
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Map.MapBlockData), "InitResources")]
        public unsafe static bool MapBlockData_InitResources_HarmonyPrefix(MapBlockData __instance)
        {
            if (InitResources) {
                MapBlockItem configData = __instance.GetConfig();
                if (configData != null)
                {
                    for (sbyte resourceType = 0; resourceType < 6; resourceType++)
                    {
                        short maxResource = configData.Resources[resourceType];
                        if (maxResource < 0)
                        {
                            maxResource = (short)(Math.Abs(maxResource) * 5);
                        }
                        else if (maxResource != 0)
                        {
                            maxResource = (short)(maxResource + 25);
                        }
                        __instance.MaxResources.Items[resourceType] = maxResource;
                        // __instance.CurrResources.Items[resourceType] = maxResource;
                        // 地块诞生时的资源
                        __instance.CurrResources.Items[resourceType] = (short)(maxResource * 50 / 100 * ItemTemplateHelper.GetGainResourcePercent(12) / 100);
                    }
                }
                return false;
            }
        }
        // // 读书策略进度增加最小值取最大值
        // [HarmonyPostfix]
        // [HarmonyPatch(nameof(ReadingStrategyItem.MinProgressAddValue), MethodType.Getter)]
        // public static void PatchMinProgressAddValue(ref sbyte __result, ReadingStrategyItem __instance)
        // {
        //     __result = __instance.MaxProgressAddValue;
        // }
        // // 读书策略当前页效率增加最小值取最大值
        // [HarmonyPostfix]
        // [HarmonyPatch(nameof(ReadingStrategyItem.MinCurrPageEfficiencyChange), MethodType.Getter)]
        // public static void PatchMinCurrPageEfficiencyChange(ref sbyte __result, ReadingStrategyItem __instance)
        // {
        //     __result = __instance.MaxCurrPageEfficiencyChange;
        // }
        // 蛐蛐是否可以升级，如果符合条件必成
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Extra.ExtraDomain), "CheckCricketIsSmart")]
        public static bool Prefix(ref bool __result, IRandomSource random, ItemKey cricketKey)
        {
            if (CheckCricketIsSmart) {
                // 获取Cricket对象
                var cricket = DomainManager.Item.GetElement_Crickets(cricketKey.Id);
                // 如果条件满足返回 false
                if (ItemTemplateHelper.GetCricketGrade(cricket.GetColorId(), cricket.GetPartId()) >= 7 ||
                    CricketParts.Instance[DomainManager.Item.GetElement_Crickets(cricketKey.Id).GetColorId()].Type == ECricketPartsType.Trash)
                {
                    __result = false;
                    return false; // 跳过原始方法
                }
                // 如果不满足上面的条件，则直接返回 true
                __result = true;
                return false; // 跳过原始方法
            }
            return true;
        }
        // 生成读书策略
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(GameData.Domains.Extra.ExtraDomain), "SetAvailableReadingStrategies")]
        // public static void  SetAvailableReadingStrategies_Prefix(ref SByteList strategyIds)
        // {
        //     SByteList ids = SByteList.Create();
        //     ids.Items.Add(14);
        //     ids.Items.Add(12);
        //     ids.Items.Add(12);
        //     ids.Items.Add(6);
        //     ids.Items.Add(4);
        //     ids.Items.Add(4);
        //     ids.Items.Add(2);
        //     ids.Items.Add(0);
        //     ids.Items.Add(0);
        //     strategyIds = ids;
        // }
        // 灵光一闪概率
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "GetCurrReadingEventBonusRate")]
        public static void GetCurrReadingEventBonusRate_Postfix(ref short __result)
        {
            if (GetCurrReadingEventBonusRate) {
                if (__result > 0)
                {
                    __result = 100;
                }
            }
        }
        // 不限制角色，找到宝物的概率是 100%
        // TODO: 测试
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtraDomain), "FindTreasureChance", typeof(MapBlockData), typeof(Character))]
        public static void FindTreasureChancePostfix(ref int __result)
        {
            if (__result > 0)
            {
                __result = 100;
            }
        }
        // 不限制角色，找到宝物的概率是 100%
        // TODO: 测试
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtraDomain), "FindTreasureChance", typeof(MapBlockData), typeof(Character), typeof(int))]
        public static void FindTreasureChanceWithItemsCountPostfix(ref int __result)
        {
            if (__result > 0)
            {
                __result = 100;
            }
        }

        // 物品掉率
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(ItemTemplateHelper), "GetDropRate")]
        // public static void GetDropRatePostfix(ref sbyte __result)
        // {
        //     if (__result > 0)
        //     {
        //         __result = 100;
        //     }
        // }
        // 生成读书策略
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Item.SkillBook), "GeneratePageIncompleteState")]
        public unsafe static bool GeneratePageIncompleteState_HarmonyPrefix(ref ushort __result, IRandomSource random, sbyte skillGroup, sbyte grade, sbyte completePagesCount, sbyte lostPagesCount, bool outlineAlwaysComplete)
        {
            int normalPagesCount = ((skillGroup == 1) ? 5 : 5);
            if (completePagesCount < 0)
            {
                float mean = 3f - (float)grade / 4f;
                float min = Math.Max(0f, mean - 1f);
                float max = Math.Min(normalPagesCount, mean + 1f);
                completePagesCount = (sbyte)(max > min ? max : min);
            }
            if (lostPagesCount < 0)
            {
                float mean = -1f + (float)grade / 2.667f;
                float min = Math.Max(0f, mean - 1f);
                float max = Math.Min(normalPagesCount - completePagesCount, mean + 1f);
                lostPagesCount = (sbyte)Math.Round(max > min ? min : max);
            }
            int incompletePagesCount = normalPagesCount - completePagesCount - lostPagesCount;
            if (incompletePagesCount < 0)
            {
                throw new Exception($"IncompletePagesCount is less than zero: {incompletePagesCount}");
            }
            sbyte* pStates = stackalloc sbyte[(int)(uint)normalPagesCount];
            int i;
            for (i = 0; i < normalPagesCount; i++)
            {
                pStates[i] = -1;
            }
            for (i = 0; i < normalPagesCount; i++)
            {
                if (completePagesCount <= 0)
                {
                    break;
                }
                int completeProb = 70 - i * 10;
                if (completeProb > 0)
                {
                    pStates[i] = 0;
                    completePagesCount--;
                }
            }
            int leftPagesCount = completePagesCount + incompletePagesCount + lostPagesCount;
            sbyte* pLeftStates = stackalloc sbyte[(int)(uint)leftPagesCount];
            for (i = 0; i < completePagesCount; i++)
            {
                pLeftStates[i] = 0;
            }
            for (i = completePagesCount; i < completePagesCount + incompletePagesCount; i++)
            {
                pLeftStates[i] = 1;
            }
            for (i = completePagesCount + incompletePagesCount; i < leftPagesCount; i++)
            {
                pLeftStates[i] = 2;
            }
		        // CollectionUtils.Shuffle(random, pLeftStates, leftPagesCount);
            i = 0;
            int leftPageId = 0;
            for (; i < normalPagesCount; i++)
            {
                if (pStates[i] == -1)
                {
                    pStates[i] = pLeftStates[leftPageId++];
                }
            }
            ushort states = 0;
            byte pageBeginId = 0;
            if (skillGroup == 1)
            {
                sbyte outlineState = (sbyte)((!outlineAlwaysComplete && random.CheckPercentProb(90)) ? 2 : 0);
                states = SkillBookStateHelper.SetPageIncompleteState(states, 0, outlineState);
                pageBeginId = 1;
            }
            for (i = 0; i < normalPagesCount; i++)
            {
                byte pageId = (byte)(pageBeginId + i);
                sbyte state = pStates[i];
                states = SkillBookStateHelper.SetPageIncompleteState(states, pageId, state);
            }
            __result = states;
            return false;
        }

        // 使蛐蛐初始化时是一只满耐久无伤口的
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Item.Cricket), "Initialize", typeof(IRandomSource), typeof(short), typeof(short), typeof(int))]
        public static void Cricket_Initialize_postfix(ref GameData.Domains.Item.Cricket __instance, IRandomSource random, short colorId, short partId, int itemId)
        {
            if (CricketInitialize) {
                var trv = Traverse.Create(__instance);
                var TemplateId = trv.Field("TemplateId").GetValue<short>();
                short[] emptyArray = new short[5];
                sbyte grade = trv.Method("CalcGrade", colorId, partId).GetValue<sbyte>();
                int hp = trv.Method("CalcHp").GetValue<int>();
                int durability = grade + 1 + hp / 20;
                durability = Math.Max(durability * 135 / 100, 1);
                trv.Field("MaxDurability").SetValue((short)durability);
                trv.Field("CurrDurability").SetValue((short)durability);
                trv.Field("_injuries").SetValue(emptyArray);
                trv.Field("_age").SetValue((sbyte)0);
            }
        }
        // // CharacterDomain 还没看
    }
}