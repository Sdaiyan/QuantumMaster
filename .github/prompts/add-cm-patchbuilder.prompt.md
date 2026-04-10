---
mode: agent
description: '在 CombatMaster 添加 PatchBuilder 功能（含 CombatPatchBase 上下文注入）'
---

# 在 CombatMaster 添加 PatchBuilder 功能

## 用户提供的参数

- **功能描述**：${{featureDescription}}
- **目标函数签名**：${{functionSignature}}
- **要替换的方法**：${{targetMethod}}
- **出现次数**：${{occurrence}}
- **期望结果**：${{expectedResult}}

## 执行步骤

### 1. 自动生成配置字段

根据上面的功能描述和函数签名，推断并生成以下配置字段：

- `FeatureKey`：配置键名（英文 PascalCase，如 `FengGouQuan`）
- `displayName`：`【武者】` + 功能中文名称（如 `【武者】疯狗拳增伤概率提升`）
- `description`：功能说明文字

**请将生成的配置字段展示给用户确认或修改，确认后再继续执行。**

### 2. 执行 Skill 流程

参考 skill `add-combatmaster-patchbuilder` 的完整流程，依次完成：

1. 修改 `Config.combat.lua` — 添加 Dropdown 配置项（使用 `combatMartialLevelOptions`）
2. 更新 `src/CombatMaster/CombatConfigManager.cs` — 添加字段、加载逻辑、`GetConfigValue` case、`FeatureMap` 条目
3. 更新 `src/Shared/LuckyCalculator.cs` — 添加 `FeatureDisplayNames` 映射
4. 创建补丁类文件 — 在 `src/CombatMaster/Features/Combat/` 下，包含 3 部分：
   - Apply 方法（PatchBuilder transpiler 替换）
   - Prefix/Postfix（通过 `__instance.CharacterId` 设置/清理 `CombatPatchBase` 上下文）
   - 替换方法（委托给 `CombatPatchBase.CheckPercentProbWithStaticContext`）
5. 注册到 `src/CombatMaster/CombatMaster.cs` 的 **`patchConfigMappings`** 和 **`patchBuilderMappings`**（**两处都要注册**）
6. 更新 `README.combat.md` 和 `README_en.combat.md`

### 3. 替换规则

- 目标方法：`${{targetMethod}}`
- 出现次数：`${{occurrence}}`
- 期望结果：`${{expectedResult}}`

### 4. 验证

完成后运行 `dotnet build CombatMaster.csproj` 确认编译通过。
