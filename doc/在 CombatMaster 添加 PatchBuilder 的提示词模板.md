**模板中 使用高亮标出的部分是需要你替换的部分**

---

参考这个函数定义，在 CombatMaster 添加一个新的 patchBuilder。添加流程参考 doc\添加PatchBuilder功能完整流程示例.md

**重要说明**：
- CombatMaster 使用 **"武学境界"** 描述（方寸大乱、患得患失、气定神闲等）
- 文档标签使用 **【武者】**，而不是【气运】
- 配置文件是 `config.combat.lua`，使用 `CombatConfigManager`
- 底层仍使用 `LuckyCalculator` 进行计算，机制与 QuantumMaster 相同

`private void GameData.Domains.SpecialEffect.CombatSkill.Jieqingmen.Sword.JieQingKuaiJian.OnCastSkillEnd(GameData.Common.DataContext context, int charId, bool isAlly, short skillId, sbyte power, bool interrupted)`

要求替换第一次出现的 `CheckPercentProb` 期望结果为 `true`

`config.combat.lua` 定义如下

`key = JieQingKuaiJian`

`DisplayName = 【武者】界青快剑重复触发概率增加`

`Description = 界青快剑重复触发概率增加`

添加的逻辑需要参考 src\CombatMaster\Features\Combat\JieQingKuaiJianPatch.cs

注意需要在 patch 的 prefix 获取 CharacterId 上下文，然后在 postfix 清空。

CharacterId 的获取方式是直接通过 harmony 提供的 __instance.CharacterId 可以拿到