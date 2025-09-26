**模板中 使用高亮标出的部分是需要你替换的部分**

---

参考这个函数定义，添加一个新的 patchBuilder。添加流程参考 doc\添加PatchBuilder功能完整流程示例.md

`public void GameData.Domains.Extra.ExtraDomain.UpdateResourceBlockBuildingCoreProducing(GameData.Common.DataContext context)`

要求替换第`一`次出现的 `CheckPercentProb`，期望结果为 `true`

config.lua 定义如下

`key = UpdateResourceBlockBuildingCoreProducing`

`DisplayName = 【气运】村庄资源点获得心材概率`

`Description = 过月时，采集到资源点心材的概率（有内置冷却时间，低品3个月，高品6个月）`