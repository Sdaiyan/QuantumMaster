/*
 * CombatMaster - 太吾绘卷战斗MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using QuantumMaster.Shared;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// 示例战斗功能补丁 - 展示如何添加新的战斗相关功能
    /// 这是一个空的示例文件，展示CombatMaster功能的基本结构
    /// </summary>
    public static class ExampleCombatPatch
    {
        /* 
         * Class形式补丁示例：
         * 
         * [HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
         * public static class SomeMethodPatch
         * {
         *     static bool Prefix(...)
         *     {
         *         if (!CombatConfigManager.IsFeatureEnabled("someFeature"))
         *             return true;
         *         
         *         // 补丁逻辑
         *         return true;
         *     }
         * }
         */

        /* 
         * PatchBuilder形式补丁示例：
         * 
         * public static bool Apply(Harmony harmony)
         * {
         *     if (!CombatConfigManager.IsFeatureEnabled("someFeature"))
         *         return false;
         *         
         *     var patchBuilder = GenericTranspiler.CreatePatchBuilder(
         *         "SomeFeaturePatch",
         *         typeof(SomeGameClass),
         *         "SomeMethod",
         *         new Type[] { typeof(int), typeof(string) }
         *     );
         *     
         *     // 配置替换逻辑
         *     patchBuilder.AddExtensionMethodReplacement(
         *         new ExtensionMethodInfo
         *         {
         *             Type = typeof(Random),
         *             MethodName = "Next",
         *             Parameters = new Type[] { typeof(int), typeof(int) }
         *         },
         *         new ReplacementMethodInfo
         *         {
         *             Type = typeof(LuckyRandomHelper),
         *             MethodName = "Next_2Args_Max_By_Luck"
         *         },
         *         1 // 替换第1次出现
         *     );
         *     
         *     patchBuilder.Apply(harmony);
         *     return true;
         * }
         */

        /// <summary>
        /// 示例：如何在CombatMaster.cs中注册这个补丁
        /// 
        /// 1. Class补丁注册（在patchConfigMappings中添加）：
        /// { "exampleCombat", (typeof(ExampleCombatPatch.SomeMethodPatch), () => CombatConfigManager.IsFeatureEnabled("exampleCombat")) },
        /// 
        /// 2. PatchBuilder补丁注册（在patchBuilderMappings中添加）：
        /// { "exampleCombat", (ExampleCombatPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("exampleCombat")) },
        /// 
        /// 3. 在CombatConfigManager.cs中添加对应的配置项：
        /// public static int ExampleCombat = 0;
        /// 
        /// 4. 在FeatureMap中添加映射：
        /// { "exampleCombat", "ExampleCombat" },
        /// 
        /// 5. 在LoadAllConfigs方法中添加配置加载：
        /// DomainManager.Mod.GetSetting(modIdStr, "exampleCombat", ref ExampleCombat);
        /// </summary>
        public static void RegistrationExample()
        {
            // 这个方法仅用于展示注册流程，不会被实际调用
        }
    }
}