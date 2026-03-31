# 混合模式代码模板（QuantumMaster）

基于 `StealPatch.cs` 的完整模板。

```csharp
/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.Character;
using Redzen.Random;
using GameData.Utilities;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Actions
{
    /// <summary>
    /// <功能描述>补丁 - 混合模式（ClassPatch + PatchBuilder）
    /// 使用 ActionPatchBase 提供的公共功能
    /// </summary>
    public static class <FeatureName>Patch
    {
        // ========== 第1部分：PatchBuilder Apply 方法 ==========

        /// <summary>
        /// 使用 PatchBuilder 框架替换方法内部调用
        /// </summary>
        public static bool Patch<MethodName>(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("<featureKey>")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Character.Character),
                MethodName = "<MethodName>",
                Parameters = new Type[] {
                    typeof(IRandomSource),
                    typeof(GameData.Domains.Character.Character),
                    // ... 其他参数类型
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "<MethodName>",
                OriginalMethod);

            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(<FeatureName>Patch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            // 替换所有 N 次 CheckPercentProb 调用
            for (int i = 1; i <= N; i++)
            {
                patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    replacementMethod,
                    i);
            }

            patchBuilder.Apply(harmony);
            return true;
        }

        // ========== 第2部分：Prefix/Postfix 上下文注入 ==========

        /// <summary>
        /// Prefix - 设置角色上下文到 ActionPatchBase 静态字段
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "<MethodName>")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(
            GameData.Domains.Character.Character __instance,
            GameData.Domains.Character.Character targetChar)
        {
            ActionPatchBase.SetCharacterContext(__instance, targetChar, "<featureKey>");
        }

        /// <summary>
        /// Postfix - 清理角色上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "<MethodName>")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            ActionPatchBase.ClearCharacterContext("<featureKey>");
        }

        // ========== 第3部分：替换方法 ==========

        /// <summary>
        /// 带静态上下文的概率检查 - 委托给 ActionPatchBase
        /// ActionPatchBase 内部自动判断：
        /// - 太吾发起 → 倾向成功
        /// - 目标是太吾 → 倾向失败
        /// - 其他 → 原始概率
        /// </summary>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return ActionPatchBase.CheckPercentProbWithStaticContext(random, probability, "<featureKey>");
        }
    }
}
```

## 注册示例

```csharp
// QuantumMaster.cs 中需要两处注册：

// 1. patchConfigMappings（Prefix/Postfix）
{ "<featureKey>", (typeof(Features.Actions.<FeatureName>Patch), () => ConfigManager.IsFeatureEnabled("<featureKey>")) },

// 2. patchBuilderMappings（PatchBuilder transpiler）
{ "<featureKey>", (Features.Actions.<FeatureName>Patch.Patch<MethodName>, () => ConfigManager.IsFeatureEnabled("<featureKey>")) },
```
