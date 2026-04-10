---
mode: agent
description: '在 QuantumMaster 添加混合模式补丁（ClassPatch + PatchBuilder，含 ActionPatchBase 角色上下文注入）'
---

# 在 QuantumMaster 添加混合模式补丁

## 用户提供的参数

- **功能描述**：${{featureDescription}}
- **目标函数签名**：${{functionSignature}}
- **要替换的方法**：${{targetMethod}}
- **出现次数**：${{occurrence}}
- **期望结果**：${{expectedResult}}
- **上下文获取方式**：${{contextDescription}}（如何从方法参数获取 currentChar / targetChar）

## 执行步骤

### 1. 自动生成配置字段

根据上面的功能描述和函数签名，推断并生成以下配置字段：

- `featureKey`：配置键名（英文 camelCase，如 `stealSuccess`）
- `displayName`：`【气运】` + 功能中文名称（如 `【气运】偷窃成功概率提升`）
- `description`：功能说明文字

**请将生成的配置字段展示给用户确认或修改，确认后再继续执行。**

### 2. 执行 Skill 流程

参考 skill `add-quantummaster-hybrid-patch` 的完整流程，依次完成：

1. 修改 `Config.lua` — 添加 Dropdown 配置项
2. 更新 `src/QuantumMaster/ConfigManager.cs` — 添加字段和加载逻辑
3. 更新 `src/Shared/LuckyCalculator.cs` — 添加 `FeatureDisplayNames` 映射
4. 创建补丁类文件 — 在 `src/QuantumMaster/Features/Actions/` 下，包含 3 部分：
   - Prefix/Postfix（设置/清理角色上下文到 `ActionPatchBase`）
   - 替换方法（从静态上下文读取角色信息做条件判断）
   - PatchBuilder Apply 方法
5. 注册到 `src/QuantumMaster/QuantumMaster.cs` 的 **`patchConfigMappings`** 和 **`patchBuilderMappings`**（**两处都要注册**）
6. 更新 `README.md` 和 `README_en.md`

### 3. 角色上下文注入

- 上下文获取方式：`${{contextDescription}}`
- 使用 `ActionPatchBase.SetCharacterContext` 在 Prefix 中设置
- 使用 `ActionPatchBase.ClearCharacterContext` 在 Postfix 中清理
- 替换方法中根据角色身份区分逻辑：
  - 太吾发起 → 倾向成功
  - 目标是太吾 → 倾向失败（保护太吾）
  - 其他 → 使用原始概率

### 4. 替换规则

- 目标方法：`${{targetMethod}}`
- 出现次数：`${{occurrence}}`
- 期望结果：`${{expectedResult}}`

### 5. 验证

完成后运行 `dotnet build QuantumMaster.csproj` 确认编译通过。
