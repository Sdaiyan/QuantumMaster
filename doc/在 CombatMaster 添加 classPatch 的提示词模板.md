**模板中 使用高亮标出的部分是需要你替换的部分**

---

帮我在 CombatMaster 添加一个 class Patch。你需要做好 config.combat.lua、CombatConfigManager 以及 README.combat.md 的修改工作，并且按照要求完成 patch。

**重要说明**：
- CombatMaster 使用 **"武学境界"** 描述（方寸大乱、患得患失、气定神闲等），配置使用 Dropdown 类型
- 文档标签使用 **【武者】**，而不是【气运】
- 底层仍使用 `LuckyCalculator` 进行计算，机制与 QuantumMaster 的气运系统相同

函数的定义是
`public override int GameData.Domains.SpecialEffect.CombatSkill.Fulongtan.DefenseAndAssist.QianNianZui.GetModifyValue(GameData.Domains.SpecialEffect.AffectedDataKey dataKey, int currModifyValue)`

需要对这个函数进行 postfix。首先需要判断角色是太吾。然后判断返回值，如果
等于大于0 则调用 Calc_Random_Next_2Args_Max_By_Luck ，最大值为原值的2倍
等于小于0 则调用 Calc_Random_Next_2Args_Min_By_Luck ，最小值也为原值的2倍
其他情况什么都不做

你可以参考这个文件的写法 `src\CombatMaster\Features\Combat\YueNvJianFaPatch.cs`

`config.combat.lua` 定义如下，注意配置类型是 Dropdown（武学境界等级）：

`key = QianNianZui`

`DisplayName = 【武者】千年醉获得的几率增益增加`

`Description = 千年醉获得的几率增益增加`