/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using GameData.Domains.Character;
using HarmonyLib;

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
            switch (ConfigManager.genderControl)
            {
                case 1: // 修改为女
                    // 0 = 女 1 = 男
                    __result = (sbyte)0;
                    DebugLog.Info("GenderControlPatch: 强制设置性别为女性");
                    return false;
                case 2: // 修改为男
                    __result = (sbyte)1;
                    DebugLog.Info("GenderControlPatch: 强制设置性别为男性");
                    return false;
                default: // 关闭功能
                    return true;
            }
        }
    }
}
