# 添加 PatchBuilder 功能完整流程示例

本文档展示如何完整地添加一个新的 PatchBuilder 功能，以"【气运】采集蛐蛐成功率"为例。

**重要提醒 - 项目和术语选择**: 
本项目已重构为双项目结构，请根据功能性质选择正确的项目：
- **QuantumMaster**: 核心游戏功能修改（资源、建筑、角色等）
  - 使用 **"气运"** 描述（命途多舛、时运不济、顺风顺水等）
  - 文档标签使用 **【气运】**
  - 配置文件：`Config.lua`
  - 配置管理器：`ConfigManager`
  
- **CombatMaster**: 战斗相关功能修改
  - 使用 **"武学境界"** 描述（方寸大乱、患得患失、气定神闲等）
  - 文档标签使用 **【武者】**
  - 配置文件：`Config.combat.lua`
  - 配置管理器：`CombatConfigManager`

**核心说明**：两个项目的底层运算机制完全相同，都使用 `LuckyCalculator` 进行计算，仅是名称、描述和文化内涵不同。

本示例以添加到 **QuantumMaster** 项目为例，如需添加到 CombatMaster 项目，请将对应路径中的 `QuantumMaster` 替换为 `CombatMaster`，并使用武学境界相关的术语。

## 项目选择指南

### QuantumMaster 项目适用于：
- 资源采集相关功能（采集蛐蛐、资源点等）
- 建筑管理功能（太吾村经营、建筑效果等）
- 角色生成功能（性别控制、性取向等）
- 读书学习功能（读书策略、灵光一闪等）
- 世界生成功能（地图资源初始化等）
- 技能学习功能（偷学、指点等）
- 奇遇相关功能
- **术语**：使用 **"气运"**（命途多舛、时运不济、顺风顺水等），标签 **【气运】**

### CombatMaster 项目适用于：
- 功法的修改
- 战斗相关的所有功能
- **术语**：使用 **"武学境界"**（方寸大乱、患得患失、气定神闲等），标签 **【武者】**

### 配置管理器对应关系：
- **QuantumMaster 项目** → `ConfigManager` → `Config.lua` → 使用 **"气运"** 术语
- **CombatMaster 项目** → `CombatConfigManager` → `Config.combat.lua` → 使用 **"武学境界"** 术语

**说明**：底层都使用 `LuckyCalculator` 进行计算，仅配置描述不同

## 功能信息

- **项目**: QuantumMaster（非战斗功能）
- **featureKey**: `catchGrasshopper`
- **功能名称**: 采集蛐蛐成功率
- **功能描述**: 采集蛐蛐时，基础成功率根据气运增加
- **术语**: 使用 **"气运"**，标签 **【气运】**
- **目标方法**: `GameData.Domains.World.ResourceDomain.CollectGrasshopper`
- **需要替换的函数**: `IRandomSource.CheckPercentProb(int percent)`
- **替换次数**: 1次（假设原方法中只有一个概率判断）

**注意**：如果是 CombatMaster 项目，则使用 **"武学境界"**，标签 **【武者】**

## 第一步：修改配置文件

**对于 QuantumMaster 项目**，在 `Config.lua` 的 `DefaultSettings` 数组中添加新配置项：

```lua
{
    SettingType = "Dropdown",
    Key = "catchGrasshopper",
    DisplayName = "【气运】采集蛐蛐成功率",
    Description = "采集蛐蛐时，基础成功率根据气运增加",
    Options = luckLevelOptions,  -- QuantumMaster 使用气运选项
    DefaultValue = 0,
},
```

**对于 CombatMaster 项目**，在 `Config.combat.lua` 的 `DefaultSettings` 数组中添加新配置项：

```lua
{
    SettingType = "Dropdown",
    Key = "featureKey",
    DisplayName = "【武者】功能名称",
    Description = "功能描述",
    Options = combatMartialLevelOptions,  -- CombatMaster 使用武学境界选项
    DefaultValue = 0,
},
```

**重要区别**：
- QuantumMaster: 标签用 **【气运】**，Options 用 `luckLevelOptions`
- CombatMaster: 标签用 **【武者】**，Options 用 `combatMartialLevelOptions`

## 第二步：更新配置管理器

**对于 QuantumMaster 项目**，在 `src/QuantumMaster/ConfigManager.cs` 中添加新的配置属性：

**对于 CombatMaster 项目**，在 `src/CombatMaster/CombatConfigManager.cs` 中添加新的配置属性：

```csharp
/// <summary>
/// 采集蛐蛐成功率气运设置
/// </summary>
public static int catchGrasshopper = 0;
```

然后在 `LoadAllConfigs` 方法中添加配置加载代码：

```csharp
DomainManager.Mod.GetSetting(modIdStr, "catchGrasshopper", ref catchGrasshopper);
DebugLog.Info($"配置加载: catchGrasshopper = {catchGrasshopper}");
```

## 第三步：更新功能显示名称映射

在 `src/Shared/LuckyCalculator.cs` 的 `FeatureDisplayNames` 字典中添加新映射：

```csharp
private static readonly Dictionary<string, string> FeatureDisplayNames = new Dictionary<string, string>
{
    // ... 现有映射
    { "catchGrasshopper", "采集蛐蛐成功率" },
    // ... 其他映射
};
```

**注意**: LuckyCalculator 现在位于 Shared 项目中，两个项目共享此配置。

## 关于 Shared 文件夹

`src/Shared/` 文件夹包含两个项目共享的代码：
- `LuckyCalculator.cs` - 气运计算逻辑
- `LuckyRandomHelper.cs` - 随机数辅助方法
- `PatchBuilder.cs` - 补丁构建器
- `PatchPresets.cs` - 预设的方法信息
- `DebugLog.cs` - 日志输出
- `IConfigProvider.cs` - 配置提供者接口
- `TranspilerHelper.cs` - Transpiler 辅助工具

这些文件会被两个项目同时编译，无需重复维护。

## 第四步：创建功能专用补丁类

**对于 QuantumMaster 项目**，创建新文件 `src/QuantumMaster/Features/Resources/CatchGrasshopperPatch.cs`：

**对于 CombatMaster 项目**，创建新文件 `src/CombatMaster/Features/Combat/SomeFeaturePatch.cs`：

```csharp
/*
 * QuantumMaster - 太吾绘卷MOD
 * Copyright (C) 2025
 * Licensed under GPL-3.0 - see LICENSE file for details
 */

using System;
using HarmonyLib;
using Redzen.Random;
using QuantumMaster.Shared;

namespace QuantumMaster.Features.Resources
{
    /// <summary>
    /// 采集蛐蛐补丁
    /// 配置项: catchGrasshopper
    /// 功能: 修改采集蛐蛐时的成功率，根据气运增加
    /// </summary>
    public static class CatchGrasshopperPatch
    {
        /// <summary>
        /// CatchGrasshopper 功能专用的替换方法信息
        /// </summary>
        private static class Replacements
        {
            /// <summary>
            /// CatchGrasshopper 专用的 CheckPercentProb True 替换
            /// </summary>
            public static readonly ReplacementMethodInfo CheckPercentProbTrue = new ReplacementMethodInfo
            {
                Type = typeof(CatchGrasshopperPatch),
                MethodName = nameof(CheckPercentProbTrue_Method)
            };
        }

        /// <summary>
        /// CatchGrasshopper 功能专用的 CheckPercentProb 替换方法
        /// 支持 CatchGrasshopper 功能的独立气运设置
        /// </summary>
        public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
        {
            return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "catchGrasshopper");
        }

        /// <summary>
        /// 应用采集蛐蛐补丁
        /// </summary>
        /// <param name="harmony">Harmony 实例</param>
        /// <returns>补丁应用是否成功</returns>
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("catchGrasshopper")) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.World.ResourceDomain),
                MethodName = "CollectGrasshopper",
                Parameters = new Type[] { 
                    typeof(GameData.Common.DataContext), 
                    typeof(short), 
                    typeof(short), 
                    typeof(int)
                }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                    "catchGrasshopper",
                    OriginalMethod);

            // 配置方法替换规则
            ConfigureReplacements(patchBuilder);
            
            patchBuilder.Apply(harmony);

            return true;
        }

        /// <summary>
        /// 配置方法替换规则
        /// </summary>
        /// <param name="patchBuilder">PatchBuilder 实例</param>
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // CheckPercentProb 方法替换
            // 1. if (random.CheckPercentProb(successRate))
            patchBuilder.AddExtensionMethodReplacement(
                    PatchPresets.Extensions.CheckPercentProb,
                    Replacements.CheckPercentProbTrue,
                    1);
        }
    }
}
```

**注意事项**：
- 确保添加了 `using QuantumMaster.Shared;` 引用
- 如果是 CombatMaster 项目，应使用 `CombatConfigManager.IsFeatureEnabled` 而不是 `ConfigManager.IsFeatureEnabled`
- 根据功能性质选择合适的命名空间（如 `Features.Combat` 用于战斗功能）
```

## 第五步：更新主类配置映射

**对于 QuantumMaster 项目**，在 `src/QuantumMaster/QuantumMaster.cs` 的 `patchBuilderMappings` 字典中添加新映射：

**对于 CombatMaster 项目**，在 `src/CombatMaster/CombatMaster.cs` 的 `patchBuilderMappings` 字典中添加新映射：

```csharp
private readonly Dictionary<string, (System.Func<Harmony, bool> patchMethod, System.Func<bool> condition)> patchBuilderMappings = new Dictionary<string, (System.Func<Harmony, bool>, System.Func<bool>)>
{
    // ... 现有映射
    
    // Resources 模块 (QuantumMaster)
    { "CollectResource", (Features.Resources.CollectResourcePatch.Apply, () => ConfigManager.IsFeatureEnabled("collectResource")) },
    { "UpgradeCollectMaterial", (Features.Resources.UpgradeCollectMaterialPatch.Apply, () => ConfigManager.IsFeatureEnabled("collectResource")) },
    { "GetCollectResourceAmount", (Features.Resources.GetCollectResourceAmountPatch.Apply, () => ConfigManager.IsFeatureEnabled("GetCollectResourceAmount")) },
    { "catchGrasshopper", (Features.Resources.CatchGrasshopperPatch.Apply, () => ConfigManager.IsFeatureEnabled("catchGrasshopper")) },
    
    // 或者 Combat 模块 (CombatMaster)
    // { "combatFeature", (Features.Combat.SomeFeaturePatch.Apply, () => CombatConfigManager.IsFeatureEnabled("combatFeature")) },
    
    // ... 其他映射
};
```

**注意**：
- QuantumMaster 项目使用 `ConfigManager.IsFeatureEnabled`
- CombatMaster 项目使用 `CombatConfigManager.IsFeatureEnabled`

## 第六步：【重要】更新功能描述文件 (README.md)

维护文档十分重要，不要遗漏这个步骤

**对于 QuantumMaster 项目**，在 `README.md` 文件中添加新功能的描述：

```markdown
【气运】采集蛐蛐成功率: 采集蛐蛐时，基础成功率根据气运增加
```

同时在 `README_en.md` 添加英文的描述，添加时需要特别注意格式

```markdown
【Luck】Grasshopper Collection Success Rate: Increase the base success rate when collecting grasshoppers based on luck
```

**对于 CombatMaster 项目**，在 `README.combat.md` 文件中添加新功能的描述：

```markdown
【武者】功能名称: 功能描述
```

同时在 `README_en.combat.md` 添加英文的描述

```markdown
【Martial Arts】Feature Name: Feature description
```

**重要区别**：
- QuantumMaster: 使用 **【气运】** / **【Luck】** 标签，文档在 `README.md` 和 `README_en.md`
- CombatMaster: 使用 **【武者】** / **【Martial Arts】** 标签，文档在 `README.combat.md` 和 `README_en.combat.md`

## 第七步：编译和测试

1. 编译对应的项目确保没有语法错误：

**编译 QuantumMaster 项目**：
```powershell
dotnet build QuantumMaster.csproj
```

**编译 CombatMaster 项目**：
```powershell
dotnet build CombatMaster.csproj
```

**编译两个项目**：
```powershell
dotnet build QuantumMaster.sln
```

2. 在游戏中测试功能是否正常工作

## 常见的替换方法类型

根据原方法中使用的随机数函数，选择对应的替换方法：

### 1. Next(int min, int max) - 两参数版本
```csharp
// 倾向最大值
public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
{
    return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "featureKey");
}

// 倾向最小值
public static int Next2ArgsMin_Method(this IRandomSource randomSource, int min, int max)
{
    return LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(min, max, "featureKey");
}
```

### 2. Next(int max) - 单参数版本
```csharp
// 倾向最大值
public static int Next1ArgMax_Method(this IRandomSource randomSource, int max)
{
    return LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max, "featureKey");
}

// 倾向0
public static int Next1Arg0_Method(this IRandomSource randomSource, int max)
{
    return LuckyCalculator.Calc_Random_Next_1Arg_0_By_Luck(max, "featureKey");
}
```

### 3. CheckPercentProb(int percent) - 百分比概率
```csharp
// 倾向成功
public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
{
    return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "featureKey");
}

// 倾向失败
public static bool CheckPercentProbFalse_Method(this IRandomSource randomSource, int percent)
{
    return LuckyCalculator.Calc_Random_CheckPercentProb_False_By_Luck(randomSource, percent, "featureKey");
}
```

### 4. CheckProb(int chance, int total) - 概率检查
```csharp
// 倾向成功
public static bool CheckProbTrue_Method(this IRandomSource randomSource, int chance, int total)
{
    return LuckyCalculator.Calc_Random_CheckProb_True_By_Luck(randomSource, chance, total, "featureKey");
}

// 倾向失败
public static bool CheckProbFalse_Method(this IRandomSource randomSource, int chance, int total)
{
    return LuckyCalculator.Calc_Random_CheckProb_False_By_Luck(randomSource, chance, total, "featureKey");
}
```

## 确定替换次数

替换次数 (targetOccurrence) 是指在目标方法中，该函数调用出现的第几次。例如：

```csharp
// 原方法示例
public void SomeMethod()
{
    var result1 = random.Next(1, 10);    // 第1次 Next(int, int) 调用
    var result2 = random.Next(5, 15);    // 第2次 Next(int, int) 调用
    var success = random.CheckPercentProb(50);  // 第1次 CheckPercentProb 调用
}
```

如果要替换第2次的 `Next(int, int)` 调用，则 `targetOccurrence` 应该设为 2。


## 调试技巧

1. **使用 DebugLog**: 在替换方法中添加日志输出，验证是否被正确调用
2. **检查原方法**: 使用反射或反编译工具确认原方法的确切签名
3. **逐步测试**: 先创建最简单的替换，确认流程正确后再添加复杂逻辑
4. **查看控制台**: 观察 Harmony 补丁应用的日志输出

## 注意事项

1. **保持原始逻辑**: 绝对不要改变原方法的核心逻辑
2. **参数类型匹配**: 确保替换方法的参数类型完全匹配
3. **替换次数准确**: 仔细计算目标方法中的调用次数
4. **功能独立性**: 每个功能应该有独立的补丁类
5. **向后兼容**: 确保新功能不会破坏现有功能
6. **项目选择**: 根据功能性质选择正确的项目（QuantumMaster vs CombatMaster）
7. **配置管理器**: 使用对应项目的配置管理器（ConfigManager vs CombatConfigManager）
8. **Shared 引用**: 补丁类中需要添加 `using QuantumMaster.Shared;` 引用
9. **配置文件**: 确保在正确的配置文件中添加设置项（Config.lua vs Config.combat.lua）
10. **术语一致性**: 
    - QuantumMaster 使用 **"气运"**（命途多舛等），标签 **【气运】**，Options 用 `luckLevelOptions`
    - CombatMaster 使用 **"武学境界"**（方寸大乱等），标签 **【武者】**，Options 用 `combatMartialLevelOptions`
    - 底层计算机制完全相同，仅是描述不同

## 架构说明

重构后的项目采用以下架构：

```
QuantumMaster/
├── QuantumMaster.csproj           # 主项目文件
├── CombatMaster.csproj            # 战斗项目文件
├── Config.lua                     # QuantumMaster 配置
├── Config.combat.lua              # CombatMaster 配置
└── src/
    ├── QuantumMaster/             # 主项目代码
    │   ├── QuantumMaster.cs
    │   ├── ConfigManager.cs
    │   ├── ConfigManagerAdapter.cs
    │   └── Features/
    ├── CombatMaster/              # 战斗项目代码
    │   ├── CombatMaster.cs
    │   ├── CombatConfigManager.cs
    │   ├── CombatConfigManagerAdapter.cs
    │   └── Features/
    └── Shared/                    # 共享代码
        ├── LuckyCalculator.cs
        ├── PatchBuilder.cs
        ├── IConfigProvider.cs
        └── ...
```

这种架构的优势：
- **代码复用**: 共享的逻辑（如气运/武学境界计算）不需要重复实现
- **功能分离**: 主功能和战斗功能分开管理，降低复杂度
- **独立配置**: 每个项目有独立的配置文件和配置管理器
- **术语区分**: QuantumMaster 用"气运"，CombatMaster 用"武学境界"，符合各自的文化内涵
- **灵活部署**: 用户可以选择只安装需要的功能模块

**核心原则**：
- 底层计算使用同一套 `LuckyCalculator`，机制完全相同
- 仅在配置描述、文档标签、选项名称上有所不同
- 保持两个项目的独立性和一致性

通过遵循这个流程，可以快速且正确地添加新的 PatchBuilder 功能。
