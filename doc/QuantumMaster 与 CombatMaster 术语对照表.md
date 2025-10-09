# QuantumMaster 与 CombatMaster 术语对照表

## 概述

本文档说明 QuantumMaster 和 CombatMaster 两个项目在术语使用上的差异。

**核心要点**：
- 两个项目的**底层运算机制完全相同**，都使用 `LuckyCalculator` 进行概率计算
- 差异仅在于**名称、描述和文化内涵**
- 这种区分是为了让功能描述更符合各自领域的特点

## 术语对照表

| 项目 | QuantumMaster | CombatMaster |
|------|---------------|--------------|
| **功能领域** | 非战斗功能（资源、建筑、角色、学习等） | 战斗相关功能（功法、招式等） |
| **核心术语** | 气运 | 武学境界 |
| **文档标签** | 【气运】 | 【武者】 |
| **英文标签** | 【Luck】 | 【Martial Arts】 |
| **配置文件** | Config.lua | Config.combat.lua |
| **配置管理器** | ConfigManager | CombatConfigManager |
| **选项变量名** | luckLevelOptions | combatMartialLevelOptions |
| **README 文件** | README.md, README_en.md | README.combat.md, README_en.combat.md |

## 等级描述对照

### QuantumMaster - 气运等级

| 等级 | 中文名称 | 含义 | 效果 |
|------|----------|------|------|
| 0 | 跟随全局 | 使用全局气运等级 | - |
| 1 | 命途多舛 | 运气极差 | 各种随机事件极度不利 |
| 2 | 时运不济 | 运气较差 | 各种随机事件比较不利 |
| 3 | 顺风顺水 | 正常运气 | 关闭MOD功能，默认游戏体验 |
| 4 | 左右逢源 | 运气较好 | 各种随机事件相对有利 |
| 5 | 心想事成 | 运气很好 | 各种随机事件非常有利 |
| 6 | 福星高照 | 运气极好 | 各种随机事件极度有利 |
| 7 | 洪福齐天 | 运气爆表 | 各种随机事件几乎必然有利 |
| 8 | 气运之子 | 运气达到理论极限 | 各种随机事件必然达到理论上限 |

### CombatMaster - 武学境界等级

| 等级 | 中文名称 | 含义 | 效果 |
|------|----------|------|------|
| 0 | 跟随全局 | 使用全局战斗武学境界 | - |
| 1 | 方寸大乱 | 心绪彻底失控，招式章法全无 | 战斗中各种随机事件极度不利 |
| 2 | 患得患失 | 心神不宁，疑虑丛生 | 战斗中各种随机事件比较不利 |
| 3 | 气定神闲 | 内心沉静，不起波澜 | 关闭MOD功能，默认游戏体验 |
| 4 | 游刃有余 | 心思通透，举止从容 | 战斗中各种随机事件相对有利 |
| 5 | 随心所欲 | 心之所向，身之所往 | 战斗中各种随机事件非常有利 |
| 6 | 如有神助 | 灵台空明，如有天启 | 战斗中各种随机事件极度有利 |
| 7 | 天人合一 | 神与天会，身与道合 | 战斗中各种随机事件几乎必然有利 |
| 8 | 万法归一 | 穷尽世间万法，终悟其源为一 | 战斗中各种随机事件必然达到理论上限 |

## 配置示例对比

### QuantumMaster 配置示例 (Config.lua)

```lua
{
    SettingType = "Dropdown",
    Key = "catchGrasshopper",
    DisplayName = "【气运】采集蛐蛐成功率",
    Description = "采集蛐蛐时，基础成功率根据气运增加",
    Options = luckLevelOptions,  -- 气运选项
    DefaultValue = 0,
}
```

### CombatMaster 配置示例 (Config.combat.lua)

```lua
{
    SettingType = "Dropdown",
    Key = "JieQingKuaiJian",
    DisplayName = "【武者】界青快剑重复触发概率增加",
    Description = "界青快剑重复触发概率增加",
    Options = combatMartialLevelOptions,  -- 武学境界选项
    DefaultValue = 0,
}
```

## README 文档格式对比

### QuantumMaster 文档格式

**README.md**:
```markdown
【气运】采集蛐蛐成功率: 采集蛐蛐时，基础成功率根据气运增加
```

**README_en.md**:
```markdown
【Luck】Grasshopper Collection Success Rate: Increase the base success rate when collecting grasshoppers based on luck
```

### CombatMaster 文档格式

**README.combat.md**:
```markdown
【武者】界青快剑重复触发概率增加: 界青快剑重复触发概率增加
```

**README_en.combat.md**:
```markdown
【Martial Arts】Jieqing Fast Sword Repeat Trigger Probability: Increase the repeat trigger probability of Jieqing Fast Sword
```

## 代码实现一致性

**重要**：虽然术语不同，但代码实现完全相同。

### 两个项目都使用相同的计算器

```csharp
// QuantumMaster 和 CombatMaster 都使用同样的代码
using QuantumMaster.Shared;

// 在补丁中调用
var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(
    min, 
    max, 
    "featureKey");
```

### 两个项目的配置管理器结构相同

```csharp
// QuantumMaster
ConfigManager.IsFeatureEnabled("featureKey")
ConfigManager.GetConfigValue("featureKey")

// CombatMaster
CombatConfigManager.IsFeatureEnabled("featureKey")
CombatConfigManager.GetConfigValue("featureKey")
```

## 选择指南

### 何时使用 QuantumMaster（气运）

- 资源采集相关
- 建筑和村庄管理
- 角色生成和属性
- 读书学习相关
- 世界生成相关
- 技能学习（非战斗）
- 奇遇事件
- 所有**非战斗**相关的游戏功能

### 何时使用 CombatMaster（武学境界）

- 功法效果修改
- 战斗招式相关
- 战斗中的特殊效果
- 所有**战斗**相关的游戏功能

## 文化内涵解释

### 为什么使用不同术语？

1. **QuantumMaster - 气运**
   - 气运是中国传统文化中的概念，强调命运和运气
   - 适合描述游戏中非战斗的各种随机事件
   - "命途多舛"、"福星高照"等词汇富有文化韵味

2. **CombatMaster - 武学境界**
   - 武学境界是武侠文化中的核心概念，强调内心修为
   - 更符合战斗、功法的主题
   - "方寸大乱"、"天人合一"等词汇体现武者的心境状态
   - 心境影响战斗表现，这在武侠文化中是很自然的逻辑

### 底层机制为何相同？

虽然术语不同，但本质上都是对随机事件的概率调整：
- 等级 1-2：降低成功率/倾向不利结果
- 等级 3：保持原始游戏逻辑（关闭MOD）
- 等级 4-8：提高成功率/倾向有利结果

这种设计既保持了代码的一致性和可维护性，又让玩家在不同场景下获得符合文化背景的体验。

## 开发注意事项

1. **添加新功能时**：
   - 先确定功能属于哪个项目
   - 使用对应项目的术语和配置
   - 在对应的 README 文件中添加描述

2. **修改现有功能时**：
   - 保持术语的一致性
   - 不要混用两种术语体系

3. **文档编写时**：
   - 清楚标注是针对哪个项目
   - 使用正确的标签（【气运】或【武者】）
   - 使用正确的配置变量名

4. **用户沟通时**：
   - 向用户明确说明两种术语的含义
   - 强调底层机制相同，只是描述不同
   - 帮助用户根据功能类型选择合适的模块

## 总结

- **QuantumMaster** = 气运系统 = 非战斗功能 = 【气运】标签
- **CombatMaster** = 武学境界系统 = 战斗功能 = 【武者】标签
- **共同点** = 底层使用 LuckyCalculator = 相同的概率调整机制
- **差异点** = 术语、描述、文化内涵不同

这种设计既统一了技术实现，又丰富了玩家的文化体验。
