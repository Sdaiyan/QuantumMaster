**模板中 使用高亮标出的部分是需要你替换的部分**

---

参考这个函数定义，添加一个新的 QuantumMaster patchBuilder。添加流程参考 doc\添加PatchBuilder功能完整流程示例.md

**重要说明 - 项目选择**：
- **QuantumMaster** 使用 **"气运"** 描述（命途多舛、时运不济、顺风顺水等），文档标签使用 **【气运】**
- **CombatMaster** 使用 **"武学境界"** 描述（方寸大乱、患得患失、气定神闲等），文档标签使用 **【武者】**
- 两者底层运算机制完全相同，仅是名称和文化内涵不同

`public void GameData.Domains.Extra.ExtraDomain.UpdateResourceBlockBuildingCoreProducing(GameData.Common.DataContext context)`

要求替换第`一`次出现的 `CheckPercentProb`，期望结果为 `true`

config.lua 定义如下

`key = UpdateResourceBlockBuildingCoreProducing`

`DisplayName = 【气运】村庄资源点获得心材概率`

`Description = 过月时，采集到资源点心材的概率（有内置冷却时间，低品3个月，高品6个月）`

不要忘记按照 流程示例中的要求维护 README.md 和 README_en.md