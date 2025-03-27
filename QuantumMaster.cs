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
using GameData.Domains.Map;
using Config;
using GameData.Domains.Item;
using static QuantumMaster.QuantumMaster;
using GameData.Domains.Taiwu;
using GameData.Domains.Extra;
using GameData.Domains.Building;
using System.Drawing;
using GameData.Common;
using GameData.Domains.CombatSkill;
using GameData.Domains.Character.Ai;
using GameData.Domains.Character.ParallelModifications;
using GameData.Domains.Character.Relation;
using GameData.Domains.SpecialEffect;
using GameData.DomainEvents;
using GameData.Domains.Information.Collection;
using GameData.Domains.LifeRecord;
using GameData.Domains.World.Notification;

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
        public static bool CreateBuildingArea; // 生成世界时，产业中的建筑和资源点的初始等级，以及生成数量
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
        public static bool FixedPagePos; // 位置固定在靠前
        public static bool CricketInitialize; // 生成蛐蛐时，必定生成耐久上限为理论上限值的，不受伤的蛐蛐
        public static bool CalcNeigongLoopingEffect; // 周天运转时，获得的内力为浮动区间的最大值，但现在有个副作用，就是真气获取的时候会全部加在第一种真气上，暂时没有办法解决
        public static bool TryAddLoopingEvent; // 如果概率不为0，尝试触发天人感应时必定成功
        public static bool ChoosyGetMaterial; // 精挑细选，品质升级判定概率最大
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
            DomainManager.Mod.GetSetting(ModIdStr, "CreateBuildingArea", ref CreateBuildingArea);
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
            DomainManager.Mod.GetSetting(ModIdStr, "FixedPagePos", ref FixedPagePos);
            DomainManager.Mod.GetSetting(ModIdStr, "CricketInitialize", ref CricketInitialize);
            DomainManager.Mod.GetSetting(ModIdStr, "CalcNeigongLoopingEffect", ref CalcNeigongLoopingEffect);
            DomainManager.Mod.GetSetting(ModIdStr, "TryAddLoopingEvent", ref TryAddLoopingEvent);
            DomainManager.Mod.GetSetting(ModIdStr, "ChoosyGetMaterial", ref ChoosyGetMaterial);
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
            AdaptableLog.Info("QuantumMaster path");
        }
        public static bool handleActionPhase(IRandomSource random, Character targetChar, ref sbyte __result, Character currentChar)
        {
            var targetId = targetChar.GetId();
            var taiwuid = DomainManager.Taiwu.GetTaiwuCharId();
            if (targetId == taiwuid) {
                __result = 0;
                return false;
            } else if (taiwuid == currentChar.GetId()) {
                __result = 5;
                return false;
            }
            return true;
        }
        // 目标为太吾或者村民时，偷窃必败，否则必成
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetStealActionPhase")]
        public static bool GetStealActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result, Character __instance)
        {
            if (steal)
            {
                return handleActionPhase(random, targetChar, ref __result, __instance);
            }
            return true;
        }
        // 唬骗
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetScamActionPhase")]
        public static bool GetScamActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result, Character __instance)
        {
            if (scam)
            {
                return handleActionPhase(random, targetChar, ref __result, __instance);
            }
            return true;
        }
        // 抢劫
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetRobActionPhase")]
        public static bool GetRobActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result, Character __instance)
        {
            if (rob)
            {
                return handleActionPhase(random, targetChar, ref __result, __instance);
            }
            return true;
        }
        // 偷学生活技能
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetStealLifeSkillActionPhase")]
        public static bool GetStealLifeSkillActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result, Character __instance)
        {
            if (stealLifeSkill)
            {
                return handleActionPhase(random, targetChar, ref __result, __instance);
            }
            return true;
        }
        // 偷学战斗技能
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetStealCombatSkillActionPhase")]
        public static bool GetStealCombatSkillActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result, Character __instance)
        {
            if (stealCombatSkill)
            {
                return handleActionPhase(random, targetChar, ref __result, __instance);
            }
            return true;
        }
        // 下毒
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetPoisonActionPhase")]
        public static bool GetPoisonActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result, Character __instance)
        {
            if (poison)
            {
                return handleActionPhase(random, targetChar, ref __result, __instance);
            }
            return true;
        }
        // 暗害
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "GetPlotHarmActionPhase")]
        public static bool GetPlotHarmActionPhase__prefix(IRandomSource random, Character targetChar, ref sbyte __result, Character __instance)
        {
            if (plotHarm)
            {
                return handleActionPhase(random, targetChar, ref __result, __instance);
            }
            return true;
        }
        // 覆盖生成性别
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Gender), "GetRandom")]
        public static bool GenderGetRandom__prefix(ref sbyte __result)
        {
            if (gender0)
            {
                // 0 = 女 1 = 男
                __result = (sbyte)0;
                return false;
            }
            if (gender1)
            {
                __result = (sbyte)1;
                return false;
            }
            return true;
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
            if (ropeOrSword)
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHitOutofCombat")]
        public static bool CheckRopeOrSwordHitOutofCombat__prefix(ref bool __result)
        {
            if (ropeOrSword)
            {
                __result = true;
                return false;
            }
            return true;
        }
        // CheckEscape

        private static List<string> GetCallerMethodNames()
        {
            const int frameCount = 1;
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
            { };
            // 初始建筑等级
            if (CreateBuildingArea)
            {
                methodList.Add("CreateBuildingArea");
            }
            if (collectResource)
            {
                methodList.Add("ApplyCollectResourceResult"); // 改名了 collectResource -> ApplyCollectResourceResult
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
                "CalcNeigongLoopingEffect",
                "OfflineUpdateShopManagement", // 招募成功率
                "CreateBuildingArea",
                "OfflineUpdateShopManagement" // 虽然会导致品级随机，但是可以避免每次都搞到一样的东西
            };
            if (CalcNeigongLoopingEffect)
            {
                methodList.Add("CalcNeigongLoopingEffect");
            }
            if (OfflineUpdateShopManagement)
            {
                methodList.Add("OfflineUpdateShopManagement");
            }
            if (GetCollectResourceAmount)
            {
                methodList.Add("GetCollectResourceAmount");
            }
            if (CreateBuildingArea)
            {
                methodList.Add("CreateBuildingArea");
            }
            if (InitPathContent)
            {
                methodList.Add("InitPathContent");
            }
            if (TaiwuResourceGrow)
            {
                methodList.Add("TaiwuResourceGrow");
            }
            if (GetStrategyProgressAddValue)
            {
                methodList.Add("GetStrategyProgressAddValue");
            }
            if (ApplyImmediateReadingStrategyEffectForLifeSkill)
            {
                methodList.Add("ApplyImmediateReadingStrategyEffectForLifeSkill");
            }
            if (ApplyImmediateReadingStrategyEffectForCombatSkill)
            {
                methodList.Add("GetStrategyProgressAddValue");
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

            var zeroList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            };
            if (ParallelUpdateOnMonthChange)
            {
                methodList.Add("ParallelUpdateOnMonthChange");
            }
            if (ChoosyGetMaterial)
            {
                methodList.Add("ChoosyGetMaterial"); // 精挑细选
            }
            var hitNames = GetCallerMethodNames();
            var hitName = IsMethodInList(hitNames, methodList);
            if (hitName != null)
            {
                // AdaptableLog.Info($"Next 1 args {hitName}, maxValue {maxValue}");
                if (zeroList.Contains(hitName))
                {
                    __result = 0;
                } else
                {

                    __result = maxValue - 1;
                }
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
            if (OfflineUpdateShopManagement)
            {
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
            if (GetAskToTeachSkillRespondChance)
            {
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
                __result = (int)(__result > 0 ? 100 : 0);
            }
        }
        // 100% 抓蛐蛐
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Item.ItemDomain), "CatchCricket")]
        public static void ItemDomain_CatchCricket_HarmonyPrefix(ref short singLevel)
        {
            if (CatchCricket)
            {
                singLevel = (short)100;
            }
        }
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
            if (CheckCricketIsSmart)
            {
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
        // 灵光一闪概率
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "GetCurrReadingEventBonusRate")]
        public static void GetCurrReadingEventBonusRate_Postfix(ref short __result)
        {
            if (GetCurrReadingEventBonusRate)
            {
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
            if (GeneratePageIncompleteState)
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
                int i;
                int leftPagesCount = completePagesCount + incompletePagesCount + lostPagesCount;
                List<sbyte> pLeftStates = new List<sbyte>((int)leftPagesCount);
                for (i = 0; i < completePagesCount; i++)
                {
                    pLeftStates.Add((sbyte)0);
                }
                for (i = completePagesCount; i < completePagesCount + incompletePagesCount; i++)
                {
                    pLeftStates.Add((sbyte)1);
                }
                for (i = completePagesCount + incompletePagesCount; i < leftPagesCount; i++)
                {
                    pLeftStates.Add((sbyte)2);
                }
                if (!FixedPagePos)
                {
                    // 打乱 pLeftStates 顺序
                    for (i = 0; i < leftPagesCount; i++)
                    {
                        Random _random = new Random();
                        int rate = _random.Next(100);
                        if (rate > 50)
                        {
                            int j = _random.Next(0, leftPagesCount);
                            sbyte temp = pLeftStates[i];
                            pLeftStates[i] = pLeftStates[j];
                            pLeftStates[j] = temp;
                        }
                    }
                }
                i = 0;
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
                    sbyte state = pLeftStates[i];
                    states = SkillBookStateHelper.SetPageIncompleteState(states, pageId, state);
                }
                __result = states;
                return false;
            }
            return true;
        }

        // 使蛐蛐初始化时是一只满耐久无伤口的
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Item.Cricket), "Initialize", typeof(IRandomSource), typeof(short), typeof(short), typeof(int))]
        public static void Cricket_Initialize_postfix(ref GameData.Domains.Item.Cricket __instance, IRandomSource random, short colorId, short partId, int itemId)
        {
            if (CricketInitialize)
            {
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



        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(GameData.Domains.CombatSkill.CombatSkillDomain), "")]
        // public static void CalcNeigongLoopingEffect_Prefix(ref IRandomSource random, GameData.Domains.Character.Character character)
        // {
        //     var taiwuid = DomainManager.Taiwu.GetTaiwuCharId();
        //     var id = character.GetId();
        //     if (taiwuid == id)
        //     {
        //         random = new myRandom();
        //     }
        // }


        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(CombatSkillDomain), "CalcTaiwuExtraDeltaNeiliPerLoop")]
        // public static void CalcTaiwuExtraDeltaNeiliPerLoop_Postfix(DataContext context, ref (int minNeili, int maxNeili) __result)
        // {
        //     var bigger = __result.maxNeili > __result.minNeili ? __result.maxNeili : __result.minNeili;
        //     __result = (minNeili: bigger, maxNeili: bigger);
        // }
        // 
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(TaiwuDomain), "GetQiArtStrategyExtraNeiliAllocationBonusRange")]
        // public static void GetQiArtStrategyExtraNeiliAllocationBonusRange_Postfix(ref (int min, int max) __result)
        // {
        //     var bigger = __result.max > __result.min ? __result.max : __result.min;
        //     __result = (min: bigger, max: bigger);
        // }
        // 
        // 灵光一闪？
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaiwuDomain), "TryAddLoopingEvent")]
        public static void TryAddLoopingEvent_Postfix(ref int basePercentProb)
        {
            if (TryAddLoopingEvent)
            {
                basePercentProb = 100;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ExtraDomain), "TryGetChoosyRemainUpgradeRate")]
        public static void TryGetChoosyRemainUpgradeRate_Postfix(ref int value)
        {
            if (ChoosyGetMaterial)
            {
                value = 2000483647;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ExtraDomain), "TryGetChoosyRemainUpgradeCount")]
        public static void TryGetChoosyRemainUpgradeCount_Postfix(ref int value)
        {
            if (ChoosyGetMaterial)
            {
                value = 2000483647;
            }
        }
        // CharacterDomain 还没看
        // 教人不消耗能量
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Character.Ai.ActionEnergySbytes), "SpendEnergyOnAction")]
        public static bool SpendEnergyOnAction_HarmonyPrefix(sbyte actionEnergyType)
        {
            if (actionEnergyType == 4)
            {
                return false;
            }
            return true;
        }
        // 教人能量检查永远足够
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Character.Ai.ActionEnergySbytes), "HasEnoughForAction")]
        public static bool HasEnoughForAction_HarmonyPrefix(ref bool __result, sbyte actionEnergyType)
        {
            if (actionEnergyType == 4)
            {
                __result = true;
                return false;
            }
            return true;
        }
        // 生成读书策略
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Extra.ExtraDomain), "SetAvailableReadingStrategies")]
        public static void  SetAvailableReadingStrategies_Prefix(ref SByteList strategyIds)
        {
            SByteList ids = SByteList.Create();
            ids.Items.Add(0);
            ids.Items.Add(0);
            ids.Items.Add(2);
            ids.Items.Add(2);
            ids.Items.Add(6);
            ids.Items.Add(6);
            ids.Items.Add(12);
            ids.Items.Add(12);
            ids.Items.Add(12);
            strategyIds = ids;
        }
        // 初始化地块
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Map.MapBlockData), "InitResources")]
        public unsafe static bool MapBlockData_InitResources_HarmonyPrefix(MapBlockData __instance)
        {
            if (InitResources)
            {
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
            return true;
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

        // 随机的威力上限, 改为最大上限
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData.Domains.Taiwu.SkillBreakPlate), "RandomGridPower", typeof(IRandomSource), typeof(int), typeof(int), typeof(int))]
        public static void SkillBreakPlate_RandomGridPower_HarmonyPostfix(IRandomSource random, ref int power, ref int chance, int powerPerGrid, ref short __result)
        {
            if (BreakValue) {
                __result = (short)powerPerGrid;
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