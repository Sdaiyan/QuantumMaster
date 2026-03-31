---
mode: agent
description: '在 QuantumMaster 添加 ClassPatch（纯 Harmony Prefix/Postfix 补丁）'
---

# 在 QuantumMaster 添加 ClassPatch 功能

## 用户提供的参数

- **功能描述**：${{featureDescription}}
- **目标函数签名**：${{functionSignature}}
- **补丁类型**：${{patchType}}（prefix 或 postfix）
- **补丁逻辑描述**：${{patchLogicDescription}}

## 执行步骤

### 1. 自动生成配置字段

根据上面的功能描述和函数签名，推断并生成以下配置字段：

- `featureKey`：配置键名（英文 camelCase，如 `readingInspiration`）
- `displayName`：`【气运】` + 功能中文名称（如 `【气运】灵光一闪概率提升`）
- `description`：功能说明文字
- `settingType`：`Dropdown`（气运等级）或 `Toggle`（开关） — 根据功能描述推断

**请将生成的配置字段展示给用户确认或修改，确认后再继续执行。**

### 2. 执行 Skill 流程

参考 skill `add-quantummaster-classpatch` 的完整流程，依次完成：

1. 修改 `Config.lua` — 添加配置项（Dropdown 或 Toggle）
2. 更新 `src/QuantumMaster/ConfigManager.cs` — 添加字段和加载逻辑
3. 如有需要，更新 `src/Shared/LuckyCalculator.cs` — 添加 `FeatureDisplayNames` 映射
4. 创建补丁类文件 — 在 `src/QuantumMaster/Features/<Category>/` 下
5. 注册到 `src/QuantumMaster/QuantumMaster.cs` 的 `patchConfigMappings`
6. 更新 `README.md` 和 `README_en.md`

### 3. 补丁实现

- 补丁类型（Prefix/Postfix）：`${{patchType}}`
- 补丁逻辑：`${{patchLogicDescription}}`
- **默认只对太吾生效**
- **使用原始值作为范围**

### 4. 验证

完成后运行 `dotnet build QuantumMaster.csproj` 确认编译通过。
