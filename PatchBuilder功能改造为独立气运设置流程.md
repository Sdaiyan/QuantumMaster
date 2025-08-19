# PatchBuilder 功能改造为独立气运设置的操作流程

## 概述
本文档记录了将 QuantumMaster MOD 中的 PatchBuilder 功能改造为支持独立气运设置的完整流程。以 `CreateBuildingArea` 功能为例，展示了从简单开关到可自定义气运级别的转换过程。

## 背景
- **原始状态**: `CreateBuildingArea` 是一个简单的布尔开关，要么启用要么禁用
- **目标状态**: 支持独立的气运级别设置，允许用户为每个功能单独配置气运强度
- **核心思想**: 每个功能都能有自己的气运设置，而不是全局统一的气运级别

## 操作步骤

### 第一步：修改配置文件 (Config.lua)
将原来的 Toggle 配置项改为 Dropdown 下拉选择：

```lua
-- 原来的配置
{
    SettingType = "Toggle",
    Key = "CreateBuildingArea",
    DisplayName = "建筑区域创建增强",
    Description = "修改建筑区域创建时的随机数生成，使建筑等级更高",
    DefaultValue = false,
}

-- 改为支持气运级别的配置
{
    SettingType = "Dropdown",
    Key = "CreateBuildingArea",
    DisplayName = "【气运】世界初始建筑",
    Description = "【气运】生成世界时，产业中的建筑和资源点的初始等级，以及生成数量",
    Options = luckLevelOptions,  -- 引用预定义的气运级别选项
    DefaultValue = 0,  -- 0 = "跟随全局"
}
```

注意：`luckLevelOptions` 已在配置文件顶部定义：
```lua
local luckLevelOptions = {
    "跟随全局",
    "命途多舛", 
    "时运不济",
    "顺风顺水(关闭功能)",
    "左右逢源",
    "心想事成",
    "福星高照",
    "洪福齐天",
    "气运之子"
}
```

### 第二步：更新配置管理器 (ConfigManager.cs)
将配置项从布尔类型改为整数类型：

```csharp
// 原来的配置
public static bool CreateBuildingArea = false;

// 改为气运级别配置（默认值 0 = 跟随全局）
public static int CreateBuildingArea = 0;
```

### 第三步：创建功能专用的独立补丁类
**判断是否需要创建新文件**：
- 如果原补丁文件中只有一个 `IsFeatureEnabled` 调用，且传入的 featureKey 与当前功能一致，则说明已经是专用文件，可以原地修改
- 如果原补丁文件中有多个不同的 `IsFeatureEnabled` 调用（传入不同的 featureKey），则说明是混合文件，需要创建新的专用文件

**情况一：原地修改（已经是专用文件）**
如果检查发现原文件已经是专用的，直接修改现有文件即可，无需创建新文件。

**情况二：创建新的专用文件**
创建新文件 `src/Features/Building/CreateBuildingAreaPatch.cs`：

```csharp
namespace QuantumMaster.Features.Building
{
    public static class CreateBuildingAreaPatch
    {
        // 功能专用的替换方法信息
        private static class Replacements
        {
            public static readonly ReplacementMethodInfo Next2ArgsMax = new ReplacementMethodInfo
            {
                Type = typeof(CreateBuildingAreaPatch),
                MethodName = nameof(Next2ArgsMax_Method)
            };
            // ... 其他替换方法
        }

        // 功能专用的替换方法实现
        public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
        {
            return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "CreateBuildingArea");
        }

        // 主要的补丁应用方法
        public static bool Apply(Harmony harmony)
        {
            if (!ConfigManager.IsFeatureEnabled("CreateBuildingArea")) return false;
            
            // 创建和配置 PatchBuilder
            var patchBuilder = GenericTranspiler.CreatePatchBuilder("CreateBuildingArea", OriginalMethod);
            ConfigureReplacements(patchBuilder);
            patchBuilder.Apply(harmony);
            
            return true;
        }

        // 配置方法替换规则
        private static void ConfigureReplacements(PatchBuilder patchBuilder)
        {
            // 添加所有需要的方法替换
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                Replacements.Next2ArgsMax,
                1);
            // ... 其他替换规则
        }
    }
}
```

### 第四步：更新 QuantumMaster.cs 配置映射
修改主文件中的补丁映射，指向新的独立补丁类：

```csharp
// PatchBuilder 补丁配置映射表
private readonly Dictionary<string, (System.Func<Harmony, bool> patchMethod, System.Func<bool> condition)> patchBuilderMappings = new Dictionary<string, (System.Func<Harmony, bool>, System.Func<bool>)>
{
    // 原来的配置
    { "CreateBuildingArea", (Features.Building.BuildingPatch.PatchCreateBuildingArea, () => ConfigManager.IsFeatureEnabled("CreateBuildingArea")) },
    
    // 改为新的独立补丁类
    { "CreateBuildingArea", (Features.Building.CreateBuildingAreaPatch.Apply, () => ConfigManager.IsFeatureEnabled("CreateBuildingArea")) },
};
```

### 第四步：清理原有代码
从原来的 `BuildingPatch.cs` 中删除 `CreateBuildingArea` 相关的代码：
- 删除 `CreateBuildingAreaReplacements` 嵌套类
- 删除相关的替换方法实现
- 删除 `PatchCreateBuildingArea` 方法
- 从 `Apply` 方法中移除对应的调用

## 关键技术要点

### 1. 气运系统架构
- **LuckyCalculator**: 核心计算逻辑，支持 featureKey 参数
- **LuckyRandomHelper**: Harmony 兼容包装器
- **ConfigManager**: 统一配置管理，支持独立气运级别

### 2. 方法替换策略
- 每个功能创建专用的替换方法
- 替换方法调用 LuckyCalculator 并传入对应的 featureKey
- 通过 featureKey 获取该功能的独立气运设置

### 3. 代码组织原则
- 每个功能一个独立的补丁类
- 清晰的命名空间和文件结构
- 统一的 Apply 方法接口

## 验证清单

完成改造后，请检查以下项目：

1. **配置文件**: Config.lua 中的配置项类型正确
2. **配置管理**: ConfigManager.cs 中的属性类型匹配
3. **气运计算**: LuckyCalculator 支持 featureKey 参数
4. **独立补丁类**: 新建的补丁类功能完整
5. **主配置映射**: QuantumMaster.cs 中的映射指向正确
6. **代码清理**: 原有冗余代码已移除
7. **编译测试**: 项目能够成功编译
8. **功能测试**: 游戏中功能按预期工作

## 扩展应用

此操作流程可以应用于任何需要独立气运设置的 PatchBuilder 功能：

1. 按照上述步骤修改配置
2. 创建功能专用的补丁类
3. 更新主配置映射
4. 清理原有代码

这样就能实现每个功能都有独立的气运控制，提供更精细的用户体验。

## 注意事项

1. **气运函数查找**: 需要调用相关的气运函数时，请到 `LuckyCalculator.cs` 文件中查找支持 featureKey 参数的方法，如：
   - `Calc_Random_Next_2Args_Max_By_Luck(int min, int max, string featureKey)`
   - `Calc_Random_CheckPercentProb_True_By_Luck(IRandomSource randomSource, int percent, string featureKey)`
   - `Calc_Random_Next_2Args_Max_By_Luck_Static(int min, int max, string featureKey)`
   - 等其他已实现的方法
2. **文件检查**: 改造前先检查原补丁文件中 `IsFeatureEnabled` 调用的数量和 featureKey，判断是否需要创建新文件
3. **向后兼容**: 确保现有存档和配置能正常工作
4. **默认值**: 新配置项的默认值应该是 0（跟随全局气运）
5. **配置格式**: 使用 `luckLevelOptions` 预定义选项列表，保持与现有配置的一致性
6. **错误处理**: 添加适当的错误处理和日志记录
7. **性能考虑**: 避免在热路径中进行复杂计算
8. **代码质量**: 保持代码风格一致性和良好的注释
