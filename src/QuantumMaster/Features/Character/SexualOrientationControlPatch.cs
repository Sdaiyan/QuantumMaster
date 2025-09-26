/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using HarmonyLib;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Character
{
    /// <summary>
    /// 性取向控制功能补丁
    /// 配置项: sexualOrientationControl
    /// 功能: 控制所有人的性取向 - 0=关闭功能，1=全体双性恋，2=禁止双性恋
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetBisexual")]
    public class SexualOrientationControlPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            switch (ConfigManager.sexualOrientationControl)
            {
                case 1: // 全体双性恋
                    __result = true;
                    DebugLog.Info("SexualOrientationControlPatch: 设置为双性恋");
                    return false; // 跳过原始方法
                case 2: // 禁止双性恋
                    __result = false;
                    DebugLog.Info("SexualOrientationControlPatch: 设置为单性恋");
                    return false; // 跳过原始方法
                default: // 关闭功能
                    return true; // 默认行为
            }
        }
    }
}
