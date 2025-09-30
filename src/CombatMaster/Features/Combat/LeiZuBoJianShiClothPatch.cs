/*
 * CombatMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;
using GameData.Domains.Item;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// 嫘祖剥茧式优先脱外观衣服补丁
    /// </summary>
    public static class LeiZuBoJianShiClothPatch
    {
        /// <summary>
        /// 替换RandomTargetSlot方法，实现优先脱外观衣服的逻辑
        /// </summary>
        /// <param name="__instance">实例对象</param>
        /// <param name="random">随机源</param>
        /// <param name="__result">返回结果</param>
        /// <returns>是否跳过原方法</returns>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi), "RandomTargetSlot")]
        [HarmonyPrefix]
        public static bool RandomTargetSlotPrefix(
            GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi __instance,
            IRandomSource random,
            ref sbyte __result)
        {
            // 检查功能是否启用
            if (!CombatConfigManager.LeiZuBoJianShiCloth)
            {
                // 功能未启用，使用原方法
                return true;
            }

            try
            {
                DebugLog.Info("[LeiZuBoJianShiClothPatch] 嫘祖剥茧式优先脱外观衣服功能已启用");

                // 获取敌方装备列表 - CurrEnemyEquipments 是私有属性
                var currEnemyEquipments = Traverse.Create(__instance).Property("CurrEnemyEquipments").GetValue<System.Collections.Generic.IReadOnlyList<ItemKey>>();
                
                if (currEnemyEquipments != null && currEnemyEquipments.Count > 4)
                {
                    ItemKey clothKey = currEnemyEquipments[4]; // 槽位4是外观衣服
                    
                    // 调用静态私有方法 IsDetachable 检查是否可脱卸
                    var isDetachableMethod = AccessTools.Method(typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi), "IsDetachable");
                    bool isDetachable = (bool)isDetachableMethod.Invoke(null, new object[] { clothKey });
                    
                    if (isDetachable)
                    {
                        // 如果外观衣服可脱卸，直接返回衣服槽位 4
                        DebugLog.Info("[LeiZuBoJianShiClothPatch] 外观衣服可脱卸，优先返回槽位 4");
                        __result = 4;
                        return false; // 跳过原方法
                    }
                }
                
                // 如果外观衣服不可脱卸或获取失败，走原逻辑
                DebugLog.Info("[LeiZuBoJianShiClothPatch] 外观衣服不可脱卸，使用原逻辑");
                return true; // 执行原方法
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[LeiZuBoJianShiClothPatch] RandomTargetSlot处理时发生错误: {ex.Message}");
                // 发生错误时使用原方法
                return true;
            }
        }
    }
}