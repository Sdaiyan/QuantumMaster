**模板中 使用高亮标出的部分是需要你替换的部分**

---

参考这个函数定义，在 CombatMaster 添加一个新的 patchBuilder。添加流程参考 doc\添加PatchBuilder功能完整流程示例.md

`private void GameData.Domains.SpecialEffect.CombatSkill.Jieqingmen.Sword.JieQingKuaiJian.OnCastSkillEnd(GameData.Common.DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)`

要求替换第一次出现的 `CheckPercentProb` 期望结果为 `true`

`config.combat.lua` 定义如下

`key = JieQingKuaiJian`

`DisplayName = 【气运】界青快剑重复触发概率增加`

`Description = 界青快剑重复触发概率增加`

添加的逻辑需要参考 src\CombatMaster\Features\Combat\JieQingKuaiJianPatch.cs