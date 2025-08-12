/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.TaiwuEvent.DisplayEvent;
using Redzen.Random;

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// 行动阶段补丁的共享帮助类
    /// 提供通用的行动阶段处理逻辑
    /// </summary>
    public static class ActionPatchHelper
    {
        /// <summary>
        /// 处理行动阶段的通用逻辑
        /// </summary>
        /// <param name="random">随机源</param>
        /// <param name="targetChar">目标角色</param>
        /// <param name="alertFactor">警戒因子</param>
        /// <param name="showCheckAnim">显示检查动画</param>
        /// <param name="templateId">模板ID</param>
        /// <param name="__result">结果</param>
        /// <param name="currentChar">当前角色</param>
        /// <returns>是否继续执行原始方法</returns>
        public static bool HandleActionPhase(IRandomSource random, GameData.Domains.Character.Character targetChar, int alertFactor, bool showCheckAnim, short templateId, ref sbyte __result, GameData.Domains.Character.Character currentChar)
        {
            var targetId = targetChar.GetId();
            var taiwuid = DomainManager.Taiwu.GetTaiwuCharId();
            DomainManager.TaiwuEvent.ShowInteractCheckAnimation = false;

            if (showCheckAnim)
            {
                CombatSkillShorts selfCombatSkillAttainments = currentChar.GetCombatSkillAttainments();
                LifeSkillShorts selfLifeSkillAttainments = currentChar.GetLifeSkillAttainments();
                OuterAndInnerInts selfPenetrations = currentChar.GetPenetrations();
                OuterAndInnerInts selfPenetrationResists = currentChar.GetPenetrationResists();
                short selfAttackSpeed = currentChar.GetAttackSpeed();
                CombatSkillShorts targetCombatSkillAttainments = targetChar.GetCombatSkillAttainments();
                LifeSkillShorts targetLifeSkillAttainments = targetChar.GetLifeSkillAttainments();
                OuterAndInnerInts targetPenetrations = targetChar.GetPenetrations();
                OuterAndInnerInts targetPenetrationResists = targetChar.GetPenetrationResists();
                short targetAttackSpeed = targetChar.GetAttackSpeed();
                DomainManager.TaiwuEvent.InteractCheckData = new EventInteractCheckData(templateId)
                {
                    SelfCombatSkillAttainments = selfCombatSkillAttainments,
                    SelfLifeSkillAttainments = selfLifeSkillAttainments,
                    SelfPenetrations = selfPenetrations,
                    SelfPenetrationResists = selfPenetrationResists,
                    SelfAttackSpeed = selfAttackSpeed,
                    TargetCombatSkillAttainments = targetCombatSkillAttainments,
                    TargetLifeSkillAttainments = targetLifeSkillAttainments,
                    TargetPenetrations = targetPenetrations,
                    TargetPenetrationResists = targetPenetrationResists,
                    TargetAttackSpeed = targetAttackSpeed,
                    TargetAlertFactor = alertFactor,
                    SelfNameRelatedData = DomainManager.Character.GetNameRelatedData(currentChar.GetId()),
                    TargetNameRelatedData = DomainManager.Character.GetNameRelatedData(targetChar.GetId())
                };
                DomainManager.TaiwuEvent.ShowInteractCheckAnimation = true;
            }

            // 如果目标是太吾，必定失败
            if (targetId == taiwuid)
            {
                if (showCheckAnim)
                {
                    DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(0);
                    DomainManager.TaiwuEvent.InteractCheckData.FailPhase = 0;
                }
                __result = 0;
                DebugLog.Info($"ActionPatchHelper: 目标是太吾，行动失败 (templateId={templateId})");
                return false;
            }
            // 如果发起者是太吾，必定成功
            else if (taiwuid == currentChar.GetId())
            {
                if (showCheckAnim)
                {
                    DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
                    DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
                    DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
                    DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
                    DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
                    DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
                }
                __result = 5;
                DebugLog.Info($"ActionPatchHelper: 发起者是太吾，行动成功 (templateId={templateId})");
                return false;
            }

            return true; // 其他情况使用原始逻辑
        }
    }
}
