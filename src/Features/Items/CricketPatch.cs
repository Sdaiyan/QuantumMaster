/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Item;
using GameData.Domains;
using Redzen.Random;
using Config;
using System.Diagnostics;

namespace QuantumMaster.Features.Items
{
    /// <summary>
    /// 蛐蛐相关功能补丁集合
    /// 使用 PatchBuilder 和传统 Harmony 补丁
    /// 
    /// 注意：已迁移功能：
    /// - CatchCricketDouble -> CatchCricketDoublePatch.cs
    /// </summary>
    public static class CricketPatch
    {
        // CatchCricketDouble 功能已迁移到 CatchCricketDoublePatch.cs
    }

    /// <summary>
    /// 抓蛐蛐基础成功率补丁
    /// 配置项: CatchCricket
    /// 功能: 【气运】根据气运影响抓蛐蛐的成功率，成功返回100，失败返回0
    /// </summary>
    [HarmonyPatch(typeof(ItemDomain), "CatchCricket")]
    public static class CatchCricketSuccessRatePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref short singLevel)
        {
            if (!ConfigManager.CatchCricket && !QuantumMaster.openAll)
            {
                DebugLog.Info("【气运】抓蛐蛐基础成功率: 使用原版逻辑");
                return; // 使用原版逻辑
            }

            var originalSingLevel = singLevel;
            
            // 使用气运系统进行成功判断，基于原始唱级作为成功概率
            // 唱级范围通常是0-100，直接当作百分比概率使用
            bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(null, originalSingLevel);
            short newSingLevel = success ? (short)100 : (short)0;
            
            DebugLog.Info($"【气运】抓蛐蛐基础成功率: 原始唱级{originalSingLevel}% -> 气运判定{(success ? "成功" : "失败")} -> {newSingLevel}");
            singLevel = newSingLevel;
        }
    }

    /// <summary>
    /// 蛐蛐升级检查补丁
    /// 配置项: CheckCricketIsSmart
    /// 功能: 【气运】根据气运影响蛐蛐升级的成功率
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Extra.ExtraDomain), "CheckCricketIsSmart")]
    public static class CheckCricketIsSmartPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result, IRandomSource random, ItemKey cricketKey)
        {
            if (!ConfigManager.CheckCricketIsSmart && !QuantumMaster.openAll)
            {
                return true; // 使用原版逻辑
            }

            // 获取Cricket对象
            var cricket = DomainManager.Item.GetElement_Crickets(cricketKey.Id);
            
            // 检查是否满足不能升级的条件
            if (ItemTemplateHelper.GetCricketGrade(cricket.GetColorId(), cricket.GetPartId()) >= 7 ||
                CricketParts.Instance[cricket.GetColorId()].Type == ECricketPartsType.Trash)
            {
                DebugLog.Info("【气运】蛐蛐升级检查: 不满足升级条件，返回false");
                __result = false;
                return false; // 跳过原始方法
            }
            
            // 原版默认概率 20%，使用气运影响
            int originalPercent = 20;
            bool success = LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(random, originalPercent);
            
            DebugLog.Info($"【气运】蛐蛐升级检查: 满足升级条件，原始概率{originalPercent}%, 气运影响后结果={success}");
            __result = success;
            return false; // 跳过原始方法
        }
    }

    /// <summary>
    /// 蛐蛐初始化补丁
    /// 配置项: CricketInitialize
    /// 功能: 生成健康的蛐蛐（最大耐久、无伤势、年龄0）
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Item.Cricket), "Initialize", new System.Type[] { typeof(IRandomSource), typeof(short), typeof(short), typeof(int) })]
    public static class CricketInitializePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref GameData.Domains.Item.Cricket __instance, IRandomSource random, short colorId, short partId, int itemId)
        {
            if (!ConfigManager.CricketInitialize && !QuantumMaster.openAll)
            {
                DebugLog.Info("【气运】蛐蛐初始化: 使用原版逻辑");
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
