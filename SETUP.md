# 开发环境配置说明

## 首次设置

1. **复制配置模板**：
   ```
   copy Directory.Build.props.template Directory.Build.props
   ```

2. **修改本地路径**：
   编辑 `Directory.Build.props` 文件，将 `<TaiwuPath>` 修改为您的太吾绘卷安装路径：
   ```xml
   <TaiwuPath>您的太吾绘卷安装路径</TaiwuPath>
   ```

3. **编译项目**：
   ```
   dotnet build QuantumMaster.sln
   ```

## 文件说明

- `Directory.Build.props.template` - 配置模板文件（会被提交到版本控制）
- `Directory.Build.props` - 本地配置文件（不会被提交到版本控制）

## 注意事项

- `Directory.Build.props` 文件已被添加到 `.gitignore`，不会被提交到版本控制系统
- 每个开发者都需要根据自己的环境创建和配置自己的 `Directory.Build.props` 文件
- 如果您的太吾绘卷路径发生变化，只需要修改 `Directory.Build.props` 中的路径即可