/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using GameData.Domains.Item;
using GameData.Domains;
using Redzen.Random;
using Config;

namespace QuantumMaster.Features.Items
{
    /// <summary>
    /// 蛐蛐相关功能补丁
    /// 配置项: CatchCricket, CheckCricketIsSmart, CricketInitialize
    /// 功能: 控制蛐蛐捕捉、升级和初始化
    /// 注意: 确保蛐蛐相关操作必定成功且生成最优属性
    /// </summary>
    public class CricketPatch
    {
        /// <summary>
        /// 抓蛐蛐成功率补丁
        /// 配置项: CatchCricket
        /// </summary>
        [HarmonyPatch(typeof(ItemDomain), "CatchCricket")]
        public class CatchCricketPatch
        {
            [HarmonyPrefix]
            public static void Prefix(ref short singLevel)
            {
                if (!ConfigManager.CatchCricket)
                {
                    return; // 使用原版逻辑
                }

                DebugLog.Info($"抓蛐蛐: 原唱级{singLevel} -> 强制成功100");
                singLevel = 100; // 设置为100确保必定成功
            }
        }

        /// <summary>
        /// 蛐蛐升级检查补丁
        /// 配置项: CheckCricketIsSmart
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Extra.ExtraDomain), "CheckCricketIsSmart")]
        public class CheckCricketIsSmartPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, IRandomSource random, ItemKey cricketKey)
            {
                if (!ConfigManager.CheckCricketIsSmart)
                {
                    return true; // 使用原版逻辑
                }

                // 获取Cricket对象
                var cricket = DomainManager.Item.GetElement_Crickets(cricketKey.Id);
                
                // 检查是否满足不能升级的条件
                if (ItemTemplateHelper.GetCricketGrade(cricket.GetColorId(), cricket.GetPartId()) >= 7 ||
                    CricketParts.Instance[cricket.GetColorId()].Type == ECricketPartsType.Trash)
                {
                    DebugLog.Info("蛐蛐升级检查: 不满足升级条件，返回false");
                    __result = false;
                    return false; // 跳过原始方法
                }
                
                // 如果满足升级条件，则必定升级
                DebugLog.Info("蛐蛐升级检查: 满足升级条件，强制升级成功");
                __result = true;
                return false; // 跳过原始方法
            }
        }

        /// <summary>
        /// 蛐蛐初始化补丁
        /// 配置项: CricketInitialize
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Item.Cricket), "Initialize", new System.Type[] { typeof(IRandomSource), typeof(short), typeof(short), typeof(int) })]
        public class CricketInitializePatch
        {
            [HarmonyPostfix]
            public static void Postfix(ref GameData.Domains.Item.Cricket __instance, IRandomSource random, short colorId, short partId, int itemId)
            {
                if (!ConfigManager.CricketInitialize)
                {
                    return; // 使用原版逻辑
                }

                var trv = HarmonyLib.Traverse.Create(__instance);
                var templateId = trv.Field("TemplateId").GetValue<short>();
                short[] emptyArray = new short[5];
                sbyte grade = trv.Method("CalcGrade", colorId, partId).GetValue<sbyte>();
                int hp = trv.Method("CalcHp").GetValue<int>();
                int durability = grade + 1 + hp / 20;
                durability = System.Math.Max(durability * 135 / 100, 1);
                
                // 设置最大耐久和当前耐久
                trv.Field("MaxDurability").SetValue((short)durability);
                trv.Field("CurrDurability").SetValue((short)durability);
                // 清除伤势
                trv.Field("_injuries").SetValue(emptyArray);
                // 设置年龄为0
                trv.Field("_age").SetValue((sbyte)0);
                
                DebugLog.Info($"蛐蛐初始化: 设置最优属性 - 耐久{durability}, 无伤势, 年龄0");
            }
        }
    }
}
