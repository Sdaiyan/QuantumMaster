# Steam BBCode 转换工具使用说明

## 功能说明

这个脚本可以将 Markdown 格式的 README 文件转换为 Steam 创意工坊的 BBCode 格式。

## 使用方法

### 方法 1: 使用默认参数

```bash
node convert-to-steam.js
```

这将读取 `README.combat.md` 并输出到 `README.combat.steam.txt`

### 方法 2: 指定输入和输出文件

```bash
node convert-to-steam.js <输入文件> <输出文件>
```

例如：
```bash
node convert-to-steam.js README.combat.md output.txt
```

### 在 PowerShell 中运行

```powershell
cd d:\code\QuantumMaster
node convert-to-steam.js
```

## 转换规则

| Markdown 格式 | Steam BBCode 格式 | 说明 |
|--------------|------------------|------|
| `# 标题` | `[h1]标题[/h1]` | 一级标题 |
| `## 标题` | `[hr][/hr]\n[h2]标题[/h2]` | 二级标题（前面加分隔线） |
| `### 标题` | `[h3]标题[/h3]` | 三级标题 |
| `**粗体**` | `[b]粗体[/b]` | 粗体文本 |
| `*斜体*` | `[i]斜体[/i]` | 斜体文本 |
| `- 列表项` | `[list]\n[*] 列表项\n[/list]` | 无序列表 |
| `1. 列表项` | `[olist]\n[*] 列表项\n[/olist]` | 有序列表 |
| `` `代码` `` | `[code]代码[/code]` | 行内代码 |
| ` ```代码块``` ` | `[code]代码块[/code]` | 代码块 |
| `[链接](URL)` | `[url=URL]链接[/url]` | 超链接 |

## 特殊处理

1. **二级标题前自动添加分隔线**：每个 `## 标题` 会自动在前面加上 `[hr][/hr]`
2. **第一个标题前不加分隔线**：如果第一行是标题，不会在前面添加分隔线
3. **列表自动包裹**：连续的列表项会被自动包裹在 `[list]` 或 `[olist]` 标签中
4. **粗体格式保留冒号**：`**文本**: 描述` 会被转换为 `[b]文本[/b]: 描述`

## 输出示例

输入 (Markdown):
```markdown
# 天机秘术-十强武者

## 简介
这是一个MOD

### 战斗功法增强
**【武者】界青快剑**: 界青快剑重复触发概率增加

## 安装说明
1. 确保游戏版本兼容
2. 将MOD文件放入游戏MOD目录
```

输出 (Steam BBCode):
```
[h1]天机秘术-十强武者[/h1]

[hr][/hr]
[h2]简介[/h2]
这是一个MOD

[h3]战斗功法增强[/h3]
[b]【武者】界青快剑[/b]: 界青快剑重复触发概率增加

[hr][/hr]
[h2]安装说明[/h2]
[olist]
[*] 确保游戏版本兼容
[*] 将MOD文件放入游戏MOD目录
[/olist]
```

## 注意事项

1. 确保安装了 Node.js 环境
2. 脚本会覆盖输出文件（如果已存在）
3. 建议先备份原文件
4. 转换后请在 Steam 创意工坊中预览效果

## 故障排除

### 找不到 Node.js
请安装 Node.js: https://nodejs.org/

### 文件读取失败
检查文件路径是否正确，确保文件存在

### 输出格式不正确
Steam BBCode 有其特定的限制，某些复杂的 Markdown 格式可能无法完美转换
