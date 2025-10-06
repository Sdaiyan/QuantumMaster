**模板中 使用高亮标出的部分是需要你替换的部分**

---

帮我在 CombatMaster 添加一个 class Patch。你需要做好 config.lua configManager 以及 README 的修改工作，并且按照要求完成 patch。


函数的定义是
`public override int GameData.Domains.SpecialEffect.CombatSkill.Fulongtan.DefenseAndAssist.QianNianZui.GetModifyValue(GameData.Domains.SpecialEffect.AffectedDataKey dataKey, int currModifyValue)`

需要对这个函数进行 postfix。首先需要判断角色是太吾。然后判断返回值，如果
等于大于0 则调用 Calc_Random_Next_2Args_Max_By_Luck ，最大值为原值的2倍
等于小于0 则调用 Calc_Random_Next_2Args_Min_By_Luck ，最小值也为原值的2倍
其他情况什么都不做

你可以参考这个文件的写法 `src\CombatMaster\Features\Combat\YueNvJianFaPatch.cs`

`config.combat.lua` 定义如下，注意他的类型不再是 option 而是 Toggle，默认是开启的。

`key = QianNianZui`

`DisplayName = 【气运】千年醉获得的几率增益增加`

`Description = 千年醉获得的几率增益增加`