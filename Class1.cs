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
        public static int steal; // 偷窃
        public static int scam; // 唬骗
        public static int rob; // 抢劫
        public static int stealLifeSkill; // 偷学生活技能
        public static int stealCombatSkill; // 偷学战斗技能
        public static int poison; // 下毒
        public static int plotHarm; // 暗害
        public static int gender; // 生成性别
        public static int pregnant; // 怀孕概率
        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }

        public override void OnModSettingUpdate()
        {
            // DomainManager.Mod.GetSetting(ModIdStr, "skillGroup1", ref skillGroup1);
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
            if (CharacterHelper.IsTaiwu(character))
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
            {
                "CreateBuildingArea",
                "CollectResource",
                "UpdateResourceBlock",
                "OfflineUpdateShopManagement",
                "ApplyLifeSkillCombatResult", // 较艺读书
                "CalcReadInCombat", // 战斗读书
                "CalcLootItem" // 战斗掉落
            };

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
                "GetCollectResourceAmount", // 采集数量
                "CreateBuildingArea", // 创建村庄地块
                "InitPathContent", // 奇遇收获
                "TaiwuResourceGrow", // 太吾村资源生长
                "UpdateStoneRoomData", // 石室更新
                "GetStrategyProgressAddValue", // 读书策略进度增加
                "ApplyImmediateReadingStrategyEffectForLifeSkill" // 读书策略进度增加
            };

            var noZero = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CreateBuildingArea",
                "InitPathContent"
            };

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
                "ParallelUpdateOnMonthChange",
                "CollectResource"
            };

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
                "OfflineUpdateShopManagement",
            };

            var hitNames = GetCallerMethodNames();
            var hitName = IsMethodInList(hitNames, methodList);

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
            __result = (int)(__result > 0 ? 100 : 0);
        }

        // 教了 100% 学会
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetTaughtNewSkillSuccessRate")]
        public static void GetTaughtNewSkillSuccessRate_Postfix(ref int __result)
        {
            __result = (int)(__result > 0 ? 100 : 0);
        }

        // 100% 抓蛐蛐
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Item.ItemDomain), "CatchCricket")]
        public static bool ItemDomain_CatchCricket_HarmonyPrefix(ref short singLevel)
        {
            singLevel = 100;
            return true;
        }

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

        // 地块升级概率，有可能就是100
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingDomain), "GetResourceBlockGrowthChance")]
        public static void GetResourceBlockGrowthChance_HarmonyPostfix(ref sbyte __result)
        {
            if (__result > 0)
            {
                __result = 100;
            }
        }

        // 地块拓展概率，有可能就是100
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingDomain), "GetResourceBlockExpandChance")]
        public static void GetResourceBlockExpandChance_HarmonyPostfix(ref sbyte __result)
        {
            if (__result > 0)
            {
                __result = 100;
            }
        }

        // 初始化地块
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Map.MapBlockData), "InitResources")]
        public unsafe static bool MapBlockData_InitResources_HarmonyPrefix(MapBlockData __instance)
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
                    __instance.CurrResources.Items[resourceType] = maxResource;
                    // 地块诞生时的资源
                    // __instance.CurrResources.Items[resourceType] = (short)(maxResource * 50 / 100 * ItemTemplateHelper.GetGainResourcePercent(12) / 100);
                }
            }
            return false;
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

        // 生成读书策略
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData.Domains.Extra.ExtraDomain), "SetAvailableReadingStrategies")]
        public static void  SetAvailableReadingStrategies_Prefix(ref SByteList strategyIds)
        {
            SByteList ids = SByteList.Create();
            ids.Items.Add(14);
            ids.Items.Add(12);
            ids.Items.Add(12);
            ids.Items.Add(6);
            ids.Items.Add(4);
            ids.Items.Add(4);
            ids.Items.Add(2);
            ids.Items.Add(0);
            ids.Items.Add(0);
            strategyIds = ids;
        }

        // 灵光一闪概率
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaiwuDomain), "GetCurrReadingEventBonusRate")]
        public static void GetCurrReadingEventBonusRate_Postfix(ref short __result)
        {
            if (__result > 0)
            {
                __result = 100;
            }
        }

        // 不限制角色，找到宝物的概率是 100%
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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemTemplateHelper), "GetDropRate")]
        public static void GetDropRatePostfix(ref sbyte __result)
        {
            if (__result > 0)
            {
                __result = 100;
            }
        }



        // 不生成荒地或者毁坏地块
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(MapBlockData), "MapBlockData")]
        // public static void MapBlockData_Prefix(short areaId, short blockId, ref short templateId)
        // {
        //     List<short> list = new List<short>{
        //         109,
        //         110,
        //         111,
        //         112,
        //         113,
        //         114,
        //         115,
        //         116,
        //         117,
        //         124
        //     };
        // 
        //     // 如果 templateId 在 list 中，则设置从 39 - 108 随机一个
        //     if (list.Contains(templateId))
        //     {
        //         Random random = new Random();
        //         templateId = (short)random.Next(39, 109);
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

        // // CharacterDomain 还没看
    }
}