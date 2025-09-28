return {
    Title = "[测试]战斗大师-气运之子(Reborn)",
    Author = "dai",
    Cover = "cover2.jpg",
    BackendPlugins = {
        "CombatMaster.dll",
    },
    DefaultSettings = {
        {
            SettingType = "Dropdown",
            Key = "CombatLuckyLevel",
            DisplayName = "战斗气运",
            Description = [[选择战斗相关功能的气运等级。
命途多舛: 战斗中各种随机事件极度不利
时运不济：战斗中各种随机事件比较不利
顺风顺水：关闭本MOD的战斗功能
左右逢源：战斗中各种随机事件相对有利
心想事成：战斗中各种随机事件非常有利
福星高照：战斗中各种随机事件极度有利
洪福齐天：战斗中各种随机事件几乎必然有利
气运之子：战斗中各种随机事件必然达到理论上限]],
            Options = {
                "命途多舛",
                "时运不济",
                "顺风顺水",
                "左右逢源",
                "心想事成",
                "福星高照",
                "洪福齐天",
                "气运之子"
            },
            DefaultValue = 7,
        },
        {
            SettingType = "Dropdown",
            Key = "JieQingKuaiJian",
            DisplayName = "【气运】界青快剑重复触发概率增加",
            Description = "界青快剑重复触发概率增加（理论上最多连发3次，加上主动释放的话一共4次）",
            Options = {
                "跟随全局",
                "命途多舛",
                "时运不济",
                "顺风顺水",
                "左右逢源",
                "心想事成",
                "福星高照",
                "洪福齐天",
                "气运之子"
            },
            DefaultValue = 0,
        }
    },
}