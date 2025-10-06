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
            // 原方法代码参考（已注释）
            // private sbyte RandomTargetSlot(IRandomSource random)
            // {
            //     ItemKey clothKey = CurrEnemyEquipments[4];
            //     List<sbyte> slotPool = ObjectPool<List<sbyte>>.Instance.Get();
            //     slotPool.Clear();
            //     if (IsDetachable(clothKey) && random.CheckPercentProb(50))
            //     {
            //         slotPool.Add(4);
            //     }
            //     else
            //     {
            //         slotPool.AddRange(CanRemoveSlots.Where(IsDetachable));
            //     }
            //     if (slotPool.Count == 0 && IsDetachable(clothKey))
            //     {
            //         slotPool.Add(4);
            //     }
            //     sbyte slot = (sbyte)((slotPool.Count > 0) ? slotPool.GetRandom(random) : (-1));
            //     ObjectPool<List<sbyte>>.Instance.Return(slotPool);
            //     return slot;
            // }

            // 检查功能是否启用
            if (!CombatConfigManager.LeiZuBoJianShiCloth)
            {
                // 功能未启用，使用原方法
                return true;
            }            try
            {
                DebugLog.Info("[LeiZuBoJianShiClothPatch] 嫘祖剥茧式优先脱外观衣服功能已启用");

                // 获取敌方装备列表 - CurrEnemyEquipments 是私有属性
                var currEnemyEquipments = Traverse.Create(__instance).Property("CurrEnemyEquipments").GetValue<System.Collections.Generic.IReadOnlyList<ItemKey>>();
                
                if (currEnemyEquipments != null && currEnemyEquipments.Count > 4)
                {
                    ItemKey clothKey = currEnemyEquipments[4]; // 槽位4是外观衣服
                    
                    // 获取静态私有方法 IsDetachable 和实例方法 IsDetachable
                    var isDetachableStaticMethod = AccessTools.Method(
                        typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi), 
                        "IsDetachable", 
                        new[] { typeof(ItemKey) });
                    
                    var isDetachableInstanceMethod = AccessTools.Method(
                        typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi), 
                        "IsDetachable", 
                        new[] { typeof(sbyte) });
                    
                    // 检查外观衣服是否可脱卸（使用静态方法）
                    bool clothDetachable = (bool)isDetachableStaticMethod.Invoke(null, new object[] { clothKey });
                    
                    if (clothDetachable)
                    {
                        // 外观衣服可脱卸，直接返回槽位 4（100% 优先脱外观）
                        DebugLog.Info("[LeiZuBoJianShiClothPatch] 外观衣服可脱卸，优先返回槽位 4");
                        __result = 4;
                        return false; // 跳过原方法
                    }
                    else
                    {
                        // 外观衣服不可脱卸，检查其他槽位
                        var canRemoveSlotsField = AccessTools.Field(
                            typeof(GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.Finger.LeiZuBoJianShi), 
                            "CanRemoveSlots");
                        
                        sbyte[] canRemoveSlots = (sbyte[])canRemoveSlotsField.GetValue(null);
                        
                        // 找到所有可脱卸的槽位
                        var availableSlots = new System.Collections.Generic.List<sbyte>();
                        foreach (sbyte slot in canRemoveSlots)
                        {
                            bool slotDetachable = (bool)isDetachableInstanceMethod.Invoke(__instance, new object[] { slot });
                            if (slotDetachable)
                            {
                                availableSlots.Add(slot);
                            }
                        }
                        
                        if (availableSlots.Count > 0)
                        {
                            // 从可脱卸槽位中随机选择
                            int randomIndex = random.Next(0, availableSlots.Count);
                            __result = availableSlots[randomIndex];
                            DebugLog.Info($"[LeiZuBoJianShiClothPatch] 外观衣服不可脱卸，从其他槽位中随机选择: {__result}");
                            return false;
                        }
                        else
                        {
                            // 没有可脱卸的装备
                            __result = -1;
                            DebugLog.Info("[LeiZuBoJianShiClothPatch] 没有可脱卸的装备");
                            return false;
                        }
                    }
                }
                
                // 获取失败，走原逻辑
                DebugLog.Info("[LeiZuBoJianShiClothPatch] 获取装备列表失败，使用原逻辑");
                return true; // 执行原方法
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[LeiZuBoJianShiClothPatch] RandomTargetSlot处理时发生错误: {ex.Message}\n{ex.StackTrace}");
                // 发生错误时使用原方法
                return true;
            }
        }
    }
}