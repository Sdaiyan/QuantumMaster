---
mode: agent
description: '在 QuantumMaster 添加 PatchBuilder 功能（纯 transpiler 替换）'
---

# 在 QuantumMaster 添加 PatchBuilder 功能

## 用户提供的参数

- **功能描述**：${{featureDescription}}
- **目标函数签名**：${{functionSignature}}
- **要替换的方法**：${{targetMethod}}
- **出现次数**：${{occurrence}}
- **期望结果**：${{expectedResult}}

## 执行步骤

### 1. 自动生成配置字段

根据上面的功能描述和函数签名，推断并生成以下配置字段：

- `featureKey`：配置键名（英文 camelCase，如 `catchGrasshopper`）
- `displayName`：`【气运】` + 功能中文名称（如 `【气运】捕虫概率提升`）
- `description`：功能说明文字

**请将生成的配置字段展示给用户确认或修改，确认后再继续执行。**

### 2. 执行 Skill 流程

参考 skill `add-quantummaster-patchbuilder` 的完整流程，依次完成：

1. 修改 `Config.lua` — 添加 Dropdown 配置项
2. 更新 `src/QuantumMaster/ConfigManager.cs` — 添加字段和加载逻辑
3. 更新 `src/Shared/LuckyCalculator.cs` — 添加 `FeatureDisplayNames` 映射
4. 创建补丁类文件 — 在 `src/QuantumMaster/Features/<Category>/` 下
5. 注册到 `src/QuantumMaster/QuantumMaster.cs` 的 `patchBuilderMappings`
6. 更新 `README.md` 和 `README_en.md`

### 3. 替换规则

在补丁类的 `Apply` 方法中，按用户指定的参数设置替换：
- 目标方法：`${{targetMethod}}`
- 出现次数：`${{occurrence}}`
- 期望结果：`${{expectedResult}}`

### 4. 验证

完成后运行 `dotnet build QuantumMaster.csproj` 确认编译通过。
