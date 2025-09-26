/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using GameData.Domains.Character;
using HarmonyLib;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Character
{
    /// <summary>
    /// 生成性别控制功能补丁
    /// 配置项: genderControl
    /// 功能: 生成角色时控制性别 - 0=关闭功能，1=修改为女，2=修改为男
    /// </summary>
    [HarmonyPatch(typeof(Gender), "GetRandom")]
    public class GenderControlPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref sbyte __result)
        {
            int maleProb = ConfigManager.genderControl;
            if (maleProb < 0 || maleProb > 100)
            {
                // 非法值，走原版
                return true;
            }
            if (maleProb == 0)
            {
                __result = (sbyte)0; // 全女
                DebugLog.Info("GenderControlPatch: 强制设置性别为女性 (0%)");
                return false;
            }
            if (maleProb == 100)
            {
                __result = (sbyte)1; // 全男
                DebugLog.Info("GenderControlPatch: 强制设置性别为男性 (100%)");
                return false;
            }
            // 1~99之间，按概率
            int rand = QuantumMaster.Random.Next(0, 100);
            if (rand < maleProb)
            {
                __result = (sbyte)1;
                DebugLog.Info($"GenderControlPatch: 按概率设置性别为男性 ({maleProb}%) rand={rand}");
            }
            else
            {
                __result = (sbyte)0;
                DebugLog.Info($"GenderControlPatch: 按概率设置性别为女性 ({100-maleProb}%) rand={rand}");
            }
            return false;
        }
    }
}
