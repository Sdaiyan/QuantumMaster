---
mode: agent
description: '在 CombatMaster 添加 ClassPatch（纯 Harmony Postfix 补丁，修改 GetModifyValue 返回值）'
---

# 在 CombatMaster 添加 ClassPatch 功能

## 用户提供的参数

- **功能描述**：${{featureDescription}}
- **目标函数签名**：${{functionSignature}}
- **补丁类型**：${{patchType}}（prefix 或 postfix）
- **补丁逻辑描述**：${{patchLogicDescription}}

## 执行步骤

### 1. 自动生成配置字段

根据上面的功能描述和函数签名，推断并生成以下配置字段：

- `FeatureKey`：配置键名（英文 PascalCase，如 `YueNvJianFa`）
- `displayName`：`【武者】` + 功能中文名称（如 `【武者】越女剑法追击几率提升`）
- `description`：功能说明文字
- `settingType`：`Dropdown`（武学境界等级）或 `Toggle`（开关） — 根据功能描述推断

**请将生成的配置字段展示给用户确认或修改，确认后再继续执行。**

### 2. 执行 Skill 流程

参考 skill `add-combatmaster-classpatch` 的完整流程，依次完成：

1. 修改 `Config.combat.lua` — 添加配置项（使用 `combatMartialLevelOptions`）
2. 更新 `src/CombatMaster/CombatConfigManager.cs` — 添加字段、加载逻辑、`GetConfigValue` case、`FeatureMap` 条目
3. 创建补丁类文件 — 在 `src/CombatMaster/Features/Combat/` 下
4. 注册到 `src/CombatMaster/CombatMaster.cs` 的 `patchConfigMappings`
5. 如有需要，更新 `src/Shared/LuckyCalculator.cs` — 添加 `FeatureDisplayNames` 映射
6. 更新 `README.combat.md` 和 `README_en.combat.md`

### 3. 补丁实现

- 补丁类型（Prefix/Postfix）：`${{patchType}}`
- 补丁逻辑：`${{patchLogicDescription}}`
- **默认只对太吾生效**：通过 `__instance.CharacterId` 判断
- **使用原始值作为范围**：用 `__result` 作为起点

### 4. 验证

完成后运行 `dotnet build CombatMaster.csproj` 确认编译通过。
