/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace QuantumMaster.Features.Character
{
    /// <summary>
    /// 门派成员认可功能补丁
    /// 配置项: SetSectMemberApproveTaiwu
    /// 功能: 控制太吾第一次向门派发出拜帖时获取的支持度
    /// 注意: 会优先获取高品级人员的支持度，理论上获取的支持度更高
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.TaiwuEvent.EventHelper.EventHelper), "SetSectMemberApproveTaiwu")]
    public class SectApprovalPatch
    {
        /// <summary>
        /// 门派成员认可太吾补丁
        /// </summary>
        [HarmonyPrefix]
        public static bool Prefix(sbyte sectId, byte countMax, ref sbyte gradeMin, sbyte gradeMax, ref List<GameData.Domains.Character.Character> __result)
        {
            if (!ConfigManager.SetSectMemberApproveTaiwu)
            {
                return true; // 使用原版逻辑
            }

            // 获取门派所有成员
            List<GameData.Domains.Character.Character> allMembers = GameData.Domains.TaiwuEvent.EventHelper.EventHelper.GetSectCharList(sectId, 0, 8);
            
            // 过滤出最高品级且未认可太吾的成员
            for (int i = allMembers.Count - 1; i >= 0; i--)
            {
                var orgInfo = allMembers[i].GetOrganizationInfo();
                if (orgInfo.Grade != gradeMax)
                {
                    allMembers.RemoveAt(i);
                    continue;
                }
                if (GameData.Domains.TaiwuEvent.EventHelper.EventHelper.IsSectCharApprovedTaiwu(allMembers[i].GetId()))
                {
                    allMembers.RemoveAt(i);
                    continue;
                }
            }

            var memberIds = allMembers.Select(m => m.GetId().ToString()).ToList();
            DebugLog.Info($"门派认可: 找到{allMembers.Count}个可认可的最高品级成员");

            // 设置认可数量不超过可用成员数
            countMax = (byte)System.Math.Min(countMax, allMembers.Count);
            List<GameData.Domains.Character.Character> approvedList = new List<GameData.Domains.Character.Character>();
            
            // 让前countMax个成员认可太吾
            for (int i = 0; i < countMax; i++)
            {
                GameData.Domains.TaiwuEvent.EventHelper.EventHelper.SetSectCharApprovedTaiwu(allMembers[i].GetId());
                approvedList.Add(allMembers[i]);
            }

            __result = approvedList;
            
            var approvedIds = approvedList.Select(m => m.GetId().ToString()).ToList();
            DebugLog.Info($"门派认可完成: {approvedList.Count}个高品级成员已认可太吾");
            
            return false; // 跳过原方法
        }
    }
}
