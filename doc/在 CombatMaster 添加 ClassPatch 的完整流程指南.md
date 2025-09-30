# 在 CombatMaster 添加 Class Patch 的完整流程指南

## 概述
本指南提供了在 CombatMaster 项目中添加新的 Class 形式补丁的完整流程。Class Patch 是使用 Harmony 框架的标准补丁形式，通过 `[HarmonyPatch]` 属性自动发现和应用补丁。

## 💡 核心原则

### ⚠️ 重要注意事项
1. **默认只对太吾生效**：除非明确指出，所有补丁默认只对 `CharacterId` 为太吾的角色生效
2. **避免重复补丁**：一个补丁只需要一个类，避免创建多个调用关系的补丁类
3. **使用原始值作为范围**：在处理数值修改时，应使用原始返回值 `__result` 作为范围的起点，而不是固定值
4. **配置类型一致性**：气运功能使用 Dropdown 类型，开关功能使用 Toggle 类型

## 📋 完整流程

### 1. 配置文件修改 (`Config.combat.lua`)

在 `DefaultSettings` 数组中添加新的配置项：

```lua
{
    SettingType = "Dropdown",  -- 气运功能使用 Dropdown
    Key = "YourFeatureName",   -- 功能键名，与代码中保持一致
    DisplayName = "【气运】功能显示名称",
    Description = "功能描述说明",
    Options = {
        "跟随全局",    -- 0: 使用全局战斗气运等级
        "命途多舛",    -- 1: 气运等级0
        "时运不济",    -- 2: 气运等级1  
        "顺风顺水",    -- 3: 气运等级2 (功能关闭)
        "左右逢源",    -- 4: 气运等级3
        "心想事成",    -- 5: 气运等级4
        "福星高照",    -- 6: 气运等级5
        "洪福齐天",    -- 7: 气运等级6
        "气运之子"     -- 8: 气运等级7
    },
    DefaultValue = 0,  -- 默认跟随全局
}
```

**对于开关类功能（非气运）：**
```lua
{
    SettingType = "Toggle",
    Key = "YourToggleFeature",
    DisplayName = "开关功能显示名称",
    Description = "开关功能描述",
    DefaultValue = true,  -- 或 false
}
```

### 2. 配置管理器更新 (`CombatConfigManager.cs`)

#### 2.1 添加静态字段
```csharp
// 在类的顶部添加配置字段
public static int YourFeatureName = 0;  // 气运功能
// 或
public static bool YourToggleFeature = true;  // 开关功能
```

#### 2.2 更新功能映射表
```csharp
private static readonly Dictionary<string, string> FeatureMap = new Dictionary<string, string>
{
    // 现有映射...
    { "YourFeatureName", "YourFeatureName" }  // 添加新的映射
};
```

#### 2.3 添加配置加载逻辑
在 `LoadAllConfigs` 方法中添加：
```csharp
// 加载新功能配置
DomainManager.Mod.GetSetting(modIdStr, "YourFeatureName", ref YourFeatureName);
DebugLog.Info($"[CombatMaster] 功能名称: {YourFeatureName}");
```

#### 2.4 更新配置值获取方法
在 `GetConfigValue` 方法中添加：
```csharp
case "YourFeatureName": return YourFeatureName;  // 气运功能
// 或
case "YourToggleFeature": return YourToggleFeature ? 1 : 0;  // 开关功能
```

### 3. 创建补丁类文件

在 `src/CombatMaster/Features/Combat/` 目录下创建 `YourFeatureNamePatch.cs`：

```csharp
/*
 * CombatMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using GameData.Domains.SpecialEffect;
using QuantumMaster.Shared;

namespace CombatMaster.Features.Combat
{
    /// <summary>
    /// 功能描述补丁 - Class 补丁形式
    /// 对 TargetMethod 方法进行 postfix/prefix 处理
    /// </summary>
    [HarmonyPatch(typeof(GameData.Domains.Target.Class), "MethodName")]
    public class YourFeatureNamePatch
    {
        /// <summary>
        /// Postfix方法 - 在目标方法执行后处理
        /// </summary>
        /// <param name="__instance">实例对象</param>
        /// <param name="__result">原始返回值（如果需要修改）</param>
        /// <param name="param1">方法参数1</param>
        /// <param name="param2">方法参数2</param>
        [HarmonyPostfix]
        public static void MethodNamePostfix(
            GameData.Domains.Target.Class __instance, 
            ref ReturnType __result,  // 只有需要修改返回值时才加 ref
            ParamType1 param1, 
            ParamType2 param2)
        {
            try
            {
                // 1. 检查功能是否启用
                if (!CombatConfigManager.IsFeatureEnabled("YourFeatureName"))
                {
                    return;
                }

                // 2. ⚠️ 重要：默认只对太吾角色生效
                var currentCharId = __instance.CharacterId;
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                if (currentCharId != taiwuId)
                {
                    DebugLog.Info($"[YourFeatureNamePatch] 非太吾角色({currentCharId})执行功能，不进行处理");
                    return;
                }

                DebugLog.Info($"[YourFeatureNamePatch] 太吾执行方法 - 原始返回值: {__result}, 参数: {param1}, {param2}");

                // 3. 具体的补丁逻辑
                // ⚠️ 重要：使用原始值作为范围起点
                if (__result == 特定值)
                {
                    // 使用 LuckyCalculator 进行气运计算
                    var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(
                        __result,  // ⚠️ 使用原始值，不是固定的0
                        最大值, 
                        "YourFeatureName");
                    
                    DebugLog.Info($"[YourFeatureNamePatch] 原始值{__result}，使用气运加成: {__result} -> {newValue}");
                    __result = newValue;
                }
                // 其他处理逻辑...
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[YourFeatureNamePatch] 处理时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// Prefix方法示例 - 在目标方法执行前处理
        /// </summary>
        [HarmonyPrefix]
        public static bool MethodNamePrefix(
            GameData.Domains.Target.Class __instance,
            ParamType1 param1)
        {
            try
            {
                // 检查功能和太吾角色（同上）
                if (!CombatConfigManager.IsFeatureEnabled("YourFeatureName"))
                    return true;  // 继续执行原方法

                var currentCharId = __instance.CharacterId;
                var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
                
                if (currentCharId != taiwuId)
                    return true;  // 继续执行原方法

                // Prefix 逻辑...
                return true;  // true=继续执行原方法，false=跳过原方法
            }
            catch (Exception ex)
            {
                DebugLog.Error($"[YourFeatureNamePatch] Prefix处理时发生错误: {ex.Message}");
                return true;
            }
        }
    }
}
```

### 4. 注册补丁 (`CombatMaster.cs`)

在 `patchConfigMappings` 字典中添加补丁注册：

```csharp
private readonly Dictionary<string, (System.Type patchType, System.Func<bool> condition)> patchConfigMappings = 
    new Dictionary<string, (System.Type, System.Func<bool>)>
{
    // 现有补丁...
    { "YourFeatureName", (typeof(Features.Combat.YourFeatureNamePatch), () => CombatConfigManager.IsFeatureEnabled("YourFeatureName")) },
};
```

### 5. 更新气运计算器 (`LuckyCalculator.cs`)

在功能显示名称映射表中添加：

```csharp
private static readonly Dictionary<string, string> FeatureDisplayNames = new Dictionary<string, string>
{
    // 现有映射...
    { "YourFeatureName", "功能中文显示名称" }
};
```

### 6. 更新文档

#### 6.1 更新 `README.combat.md`
在"战斗功法增强"部分添加：
```markdown
【气运】功能中文名称: 功能描述说明
```

#### 6.2 更新 `README_en.combat.md`
在"Combat Skill Enhancement"部分添加：
```markdown
【Luck】Feature English Name: Feature description in English
```

## ⚠️ 常见错误和注意事项

### 1. 重复补丁问题
❌ **错误做法**：
```csharp
// 错误：创建多个补丁类相互调用
public static class YourFeaturePatch { ... }  // 静态类
public class YourFeatureClassPatch { ... }    // 又一个类调用上面的
```

✅ **正确做法**：
```csharp
// 正确：只创建一个带 [HarmonyPatch] 的类
[HarmonyPatch(typeof(TargetClass), "MethodName")]
public class YourFeaturePatch { ... }
```

### 2. 数值范围问题
❌ **错误做法**：
```csharp
// 错误：使用固定范围
var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(0, 100, "Feature");
```

✅ **正确做法**：
```csharp
// 正确：使用原始值作为起点
var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(__result, 100, "Feature");
```

### 3. 太吾角色判断遗漏
❌ **错误做法**：
```csharp
// 错误：没有检查角色，所有角色都会受影响
[HarmonyPostfix]
public static void SomePostfix(SomeClass __instance, ref int __result)
{
    // 直接处理，没有角色检查
    __result = newValue;
}
```

✅ **正确做法**：
```csharp
// 正确：默认只对太吾生效
[HarmonyPostfix] 
public static void SomePostfix(SomeClass __instance, ref int __result)
{
    var currentCharId = __instance.CharacterId;
    var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
    
    if (currentCharId != taiwuId)
        return;  // 只有太吾才处理
        
    // 处理逻辑...
}
```

### 4. 配置类型不一致
❌ **错误做法**：
```csharp
// Lua 中是 Dropdown，但代码中当作 bool 处理
public static bool YourFeature = true;  // 错误
```

✅ **正确做法**：
```csharp
// Lua 中是 Dropdown，代码中使用 int
public static int YourFeature = 0;  // 正确
```

## 🔍 调试和测试

### 1. 编译检查
```bash
cd /d "项目根目录"
dotnet build CombatMaster.csproj
```

### 2. 日志检查
查看游戏日志，确认：
- 配置加载成功
- 补丁应用成功
- 功能触发时的日志输出

### 3. 功能测试
- 测试配置开关是否生效
- 测试气运等级是否影响结果
- 测试非太吾角色是否被正确忽略

## 📝 总结

遵循本指南可以确保：
1. ✅ 配置系统完整性
2. ✅ 补丁结构清晰
3. ✅ 太吾角色限制正确实施
4. ✅ 气运系统正确集成
5. ✅ 避免常见错误

记住核心原则：**简洁、准确、只对太吾生效**。