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
            Key = "combatLucky",
            DisplayName = "【战斗气运】战斗幸运加成",
            Description = "战斗中的各种幸运判定根据气运增加成功率",
            Options = {
                "跟随全局",
                "命途多舛",
                "时运不济",
                "顺风顺水(关闭功能)",
                "左右逢源",
                "心想事成",
                "福星高照",
                "洪福齐天",
                "气运之子"
            },
            DefaultValue = 0,
        },
        {
            SettingType = "Dropdown",
            Key = "weaponDurability",
            DisplayName = "【战斗气运】减少武器耐久消耗概率",
            Description = "武器耐久消耗判定时，根据气运减少消耗的概率（强制扣除时无效）",
            Options = {
                "跟随全局",
                "命途多舛",
                "时运不济",
                "顺风顺水(关闭功能)",
                "左右逢源",
                "心想事成",
                "福星高照",
                "洪福齐天",
                "气运之子"
            },
            DefaultValue = 3,
        },
        {
            SettingType = "Dropdown",
            Key = "armorDurability",
            DisplayName = "【战斗气运】减少护甲耐久消耗概率",
            Description = "护甲耐久消耗判定时，根据气运减少消耗的概率（强制扣除时无效）",
            Options = {
                "跟随全局",
                "命途多舛",
                "时运不济",
                "顺风顺水(关闭功能)",
                "左右逢源",
                "心想事成",
                "福星高照",
                "洪福齐天",
                "气运之子"
            },
            DefaultValue = 3,
        },
        {
            SettingType = "Dropdown",
            Key = "combatRead",
            DisplayName = "【战斗气运】战斗读书",
            Description = "如果战斗读书的概率大于0，那么触发几率根据气运增加（如果评价过低概率可能为0）",
            Options = {
                "跟随全局",
                "命途多舛",
                "时运不济",
                "顺风顺水(关闭功能)",
                "左右逢源",
                "心想事成",
                "福星高照",
                "洪福齐天",
                "气运之子"
            },
            DefaultValue = 0,
        },
        {
            SettingType = "Dropdown",
            Key = "combatQiQrt",
            DisplayName = "【战斗气运】战斗周天运转",
            Description = "如果战斗周天运转的概率大于0，那么触发几率根据气运增加（如果评价过低概率可能为0）",
            Options = {
                "跟随全局",
                "命途多舛",
                "时运不济",
                "顺风顺水(关闭功能)",
                "左右逢源",
                "心想事成",
                "福星高照",
                "洪福齐天",
                "气运之子"
            },
            DefaultValue = 0,
        },
        {
            SettingType = "Dropdown",
            Key = "lootDrop",
            DisplayName = "【战斗气运】战利品掉落概率",
            Description = "战利品掉落概率不为0，那么掉落的概率根据气运增加（原本逻辑是对每个战利品进行判断是否掉落）",
            Options = {
                "跟随全局",
                "命途多舛",
                "时运不济",
                "顺风顺水(关闭功能)",
                "左右逢源",
                "心想事成",
                "福星高照",
                "洪福齐天",
                "气运之子"
            },
            DefaultValue = 0,
        },
        {
            SettingType = "Dropdown",
            Key = "lifeSkillCombat",
            DisplayName = "【战斗气运】教艺读书&周天",
            Description = "如果较艺读书&周天的概率大于0，那么触发几率根据气运增加（如果评价过低概率可能为0）",
            Options = {
                "跟随全局",
                "命途多舛",
                "时运不济",
                "顺风顺水(关闭功能)",
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