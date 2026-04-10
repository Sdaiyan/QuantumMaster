# PatchBuilder 代码模板（CombatMaster）

基于 `FengGouQuanPatch.cs` 的完整模板。

```csharp
/*
 * CombatMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// <功能描述>补丁 - 使用 CombatPatchBase 静态上下文
    /// </summary>
    public static class <FeatureName>Patch
    {
        // ========== PatchBuilder Apply ==========

        /// <summary>
        /// 使用 PatchBuilder 框架替换方法内部调用
        /// </summary>
        public static bool Apply(Harmony harmony)
        {
            if (!CombatConfigManager.IsFeatureEnabled("<FeatureKey>")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.SpecialEffect.CombatSkill.<门派>.<类型>.<功法类名>),
                MethodName = "<MethodName>",
                Parameters = new Type[] { 
                    // 填入目标方法的参数类型
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "<FeatureKey><MethodName>",
                OriginalMethod);

            var replacementMethod = new ReplacementMethodInfo
            {
                Type = typeof(<FeatureName>Patch),
                MethodName = nameof(CheckPercentProbWithStaticContext)
            };

            // 替换第 N 次 CheckPercentProb 调用
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                replacementMethod,
                1);

            patchBuilder.Apply(harmony);
            return true;
        }

        // ========== Prefix/Postfix 上下文注入 ==========

        /// <summary>
        /// Prefix - 通过 __instance.CharacterId 获取角色 ID
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.<门派>.<类型>.<功法类名>), "<MethodName>")]
        [HarmonyPrefix]
        public static void SetCurrentCharacterPrefix(
            GameData.Domains.SpecialEffect.CombatSkill.<门派>.<类型>.<功法类名> __instance)
        {
            try
            {
                var charId = __instance.CharacterId;
                CombatPatchBase.SetCharacterContext(charId, "<FeatureKey>");
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[<FeatureName>Patch] 设置角色上下文时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Postfix - 清理静态上下文
        /// </summary>
        [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.<门派>.<类型>.<功法类名>), "<MethodName>")]
        [HarmonyPostfix]
        public static void ClearCurrentCharacterPostfix()
        {
            CombatPatchBase.ClearCharacterContext("<FeatureKey>");
        }

        // ========== 替换方法 ==========

        /// <summary>
        /// 带静态上下文的概率检查 - 委托给 CombatPatchBase
        /// </summary>
        public static bool CheckPercentProbWithStaticContext(IRandomSource random, int probability)
        {
            return CombatPatchBase.CheckPercentProbWithStaticContext(random, probability, "<FeatureKey>");
        }
    }
}
```

## CombatPatchBase 其他替换方法

如果需要替换 `Next` 而非 `CheckPercentProb`：

```csharp
// 替换 Next(int, int) 倾向最大值
public static int Next2ArgsMaxWithStaticContext(IRandomSource random, int min, int max)
{
    return CombatPatchBase.Next2ArgsWithStaticContext(random, min, max, "<FeatureKey>", expectMax: true);
}

// 替换 Next(int) 倾向最大值
public static int Next1ArgMaxWithStaticContext(IRandomSource random, int max)
{
    return CombatPatchBase.Next1ArgWithStaticContext(random, max, "<FeatureKey>", expectMax: true);
}
```

## 注册示例

```csharp
// CombatMaster.cs 中两处注册：

// 1. patchConfigMappings
{ "<FeatureKey>", (typeof(Features.Combat.<FeatureName>Patch), () => CombatConfigManager.IsFeatureEnabled("<FeatureKey>")) },

// 2. patchBuilderMappings
{ "<FeatureKey>", (Features.Combat.<FeatureName>Patch.Apply, () => CombatConfigManager.IsFeatureEnabled("<FeatureKey>")) },
```
