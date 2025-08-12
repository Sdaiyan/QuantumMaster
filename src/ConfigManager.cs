using TaiwuModdingLib.Core.Plugin;
using GameData.Domains;

namespace QuantumMaster
{
    /// <summary>
    /// 配置管理器，负责读取和管理所有 MOD 配置项
    /// </summary>
    public static class ConfigManager
    {
        // 通过 class patch 的方法迁移过来的各种功能开关
        public static bool steal; // 偷窃必定成功
        public static bool scam; // 唬骗必定成功
        public static bool rob; // 抢劫必定成功
        public static bool stealLifeSkill; // 偷学生活技能必定成功
        public static bool stealCombatSkill; // 偷学战斗技能必定成功
        public static bool poison; // 下毒必定成功
        public static bool plotHarm; // 暗害必定成功
        public static int genderControl; // 生成性别控制：0=关闭功能，1=修改为女，2=修改为男
        public static bool ropeOrSword; // 如果概率不为0，绳子绑架或者煎饼救人必定成功
        public static bool GetAskToTeachSkillRespondChance; // 如果概率不是0，则必定会指点别人
        public static bool GetTaughtNewSkillSuccessRate; // 如果概率不为0，接受指点的人必定能学习成功
        public static bool CatchCricket; // 抓蛐蛐必定成功
        public static bool InitResources; // 生成世界时，每个地块上的资源为浮动区间的最大值，受到难度的影响
        public static bool CheckCricketIsSmart; // 蛐蛐是否可以升级，如果符合条件必定升级
        public static bool GetCurrReadingEventBonusRate; // 灵光一闪概率不为0时，必定灵光一闪
        public static bool GeneratePageIncompleteState; // 生成书籍时，完整的书页为浮动区间的最大值，亡佚书页为浮动区间最小值，并且完整的书页会出现在书本的前篇位置
        public static bool FixedPagePos; // 位置固定在靠前
        public static bool CricketInitialize; // 生成蛐蛐时，必定生成耐久上限为理论上限值的，不受伤的蛐蛐
        public static bool TryAddLoopingEvent; // 如果概率不为0，尝试触发天人感应时必定成功
        public static bool SetSectMemberApproveTaiwu; // 送上拜帖时，必定是尽可能高阶的门派成员会认可太吾
        public static int BookStrategiesSelect1; // 第 1 个策略
        public static int BookStrategiesSelect2; // 第 2 个策略
        public static int BookStrategiesSelect3; // 第 3 个策略
        public static int BookStrategiesSelect4; // 第 4 个策略
        public static int BookStrategiesSelect5; // 第 5 个策略
        public static int BookStrategiesSelect6; // 第 6 个策略
        public static int BookStrategiesSelect7; // 第 7 个策略
        public static int BookStrategiesSelect8; // 第 8 个策略
        public static int BookStrategiesSelect9; // 第 9 个策略
        public static int sexualOrientationControl; // 性取向控制：0=关闭功能，1=全体双性恋，2=禁止双性恋
        public static bool GetQiArtStrategyDeltaNeiliBonus; // 周天内力策略收益最大

        // 通过 PatchBuilder 应用的补丁配置
        public static int LuckyLevel;
        public static bool CreateBuildingArea; // 生成世界时，产业中的建筑和资源点的初始等级，以及生成数量
        public static bool CalcNeigongLoopingEffect; // 周天运转时，获得的内力为浮动区间的最大值，内息恢复最大，内息紊乱最小
        public static bool collectResource; // 收获资源时必定获取引子，且是可能获取的最高级的引子
        public static bool GetCollectResourceAmount; // 采集数量必定为浮动区间的上限
        public static bool OfflineUpdateShopManagement; // 如果概率不为0，产业建筑经营、招募必然成功、村民技艺必定提升
        public static bool ApplyLifeSkillCombatResult; // 如果概率不为0，较艺读书&周天必定触发
        public static bool CalcReadInCombat; // 如果概率不为0，战斗读书必定触发
        public static bool CalcQiQrtInCombat; // 如果概率不为0，战斗周天运转必定触发
        public static bool CalcLootItem; // 如果概率不为0，战利品掉落判定必定通过（原本逻辑是对每个战利品进行判断是否掉落）
        public static bool InitPathContent; // 奇遇收获资源时，数量为浮动区间的上限
        public static bool GetStrategyProgressAddValue; // 读书策略进度增加为浮动区间的上限
        public static bool ApplyImmediateReadingStrategyEffectForLifeSkill; // 技艺读书策略进度增加为浮动区间的上限
        public static bool ChoosyGetMaterial; // 精挑细选，品质升级判定概率最大
        public static bool AddChoosyRemainUpgradeData; // 精挑细选过月累计最大
        public static bool ParallelUpdateOnMonthChange; // 地块每月资源恢复数量为浮动区间的上限

        /// <summary>
        /// 从游戏配置中读取所有 MOD 设置
        /// </summary>
        /// <param name="modIdStr">MOD ID 字符串</param>
        public static void LoadAllConfigs(string modIdStr)
        {
            DebugLog.Info("开始加载配置项...");

            // 通过 class 形式的补丁配置
            DomainManager.Mod.GetSetting(modIdStr, "steal", ref steal);
            DebugLog.Info($"配置加载: steal = {steal}");

            DomainManager.Mod.GetSetting(modIdStr, "scam", ref scam);
            DebugLog.Info($"配置加载: scam = {scam}");

            DomainManager.Mod.GetSetting(modIdStr, "rob", ref rob);
            DebugLog.Info($"配置加载: rob = {rob}");

            DomainManager.Mod.GetSetting(modIdStr, "stealLifeSkill", ref stealLifeSkill);
            DebugLog.Info($"配置加载: stealLifeSkill = {stealLifeSkill}");

            DomainManager.Mod.GetSetting(modIdStr, "stealCombatSkill", ref stealCombatSkill);
            DebugLog.Info($"配置加载: stealCombatSkill = {stealCombatSkill}");

            DomainManager.Mod.GetSetting(modIdStr, "poison", ref poison);
            DebugLog.Info($"配置加载: poison = {poison}");

            DomainManager.Mod.GetSetting(modIdStr, "plotHarm", ref plotHarm);
            DebugLog.Info($"配置加载: plotHarm = {plotHarm}");

            DomainManager.Mod.GetSetting(modIdStr, "genderControl", ref genderControl);
            DebugLog.Info($"配置加载: genderControl = {genderControl}");

            DomainManager.Mod.GetSetting(modIdStr, "ropeOrSword", ref ropeOrSword);
            DebugLog.Info($"配置加载: ropeOrSword = {ropeOrSword}");

            DomainManager.Mod.GetSetting(modIdStr, "GetAskToTeachSkillRespondChance", ref GetAskToTeachSkillRespondChance);
            DebugLog.Info($"配置加载: GetAskToTeachSkillRespondChance = {GetAskToTeachSkillRespondChance}");

            DomainManager.Mod.GetSetting(modIdStr, "GetTaughtNewSkillSuccessRate", ref GetTaughtNewSkillSuccessRate);
            DebugLog.Info($"配置加载: GetTaughtNewSkillSuccessRate = {GetTaughtNewSkillSuccessRate}");

            DomainManager.Mod.GetSetting(modIdStr, "CatchCricket", ref CatchCricket);
            DebugLog.Info($"配置加载: CatchCricket = {CatchCricket}");

            DomainManager.Mod.GetSetting(modIdStr, "InitResources", ref InitResources);
            DebugLog.Info($"配置加载: InitResources = {InitResources}");

            DomainManager.Mod.GetSetting(modIdStr, "CheckCricketIsSmart", ref CheckCricketIsSmart);
            DebugLog.Info($"配置加载: CheckCricketIsSmart = {CheckCricketIsSmart}");

            DomainManager.Mod.GetSetting(modIdStr, "GetCurrReadingEventBonusRate", ref GetCurrReadingEventBonusRate);
            DebugLog.Info($"配置加载: GetCurrReadingEventBonusRate = {GetCurrReadingEventBonusRate}");

            DomainManager.Mod.GetSetting(modIdStr, "GeneratePageIncompleteState", ref GeneratePageIncompleteState);
            DebugLog.Info($"配置加载: GeneratePageIncompleteState = {GeneratePageIncompleteState}");

            DomainManager.Mod.GetSetting(modIdStr, "FixedPagePos", ref FixedPagePos);
            DebugLog.Info($"配置加载: FixedPagePos = {FixedPagePos}");

            DomainManager.Mod.GetSetting(modIdStr, "CricketInitialize", ref CricketInitialize);
            DebugLog.Info($"配置加载: CricketInitialize = {CricketInitialize}");

            DomainManager.Mod.GetSetting(modIdStr, "TryAddLoopingEvent", ref TryAddLoopingEvent);
            DebugLog.Info($"配置加载: TryAddLoopingEvent = {TryAddLoopingEvent}");

            DomainManager.Mod.GetSetting(modIdStr, "SetSectMemberApproveTaiwu", ref SetSectMemberApproveTaiwu);
            DebugLog.Info($"配置加载: SetSectMemberApproveTaiwu = {SetSectMemberApproveTaiwu}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect1", ref BookStrategiesSelect1);
            DebugLog.Info($"配置加载: BookStrategiesSelect1 = {BookStrategiesSelect1}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect2", ref BookStrategiesSelect2);
            DebugLog.Info($"配置加载: BookStrategiesSelect2 = {BookStrategiesSelect2}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect3", ref BookStrategiesSelect3);
            DebugLog.Info($"配置加载: BookStrategiesSelect3 = {BookStrategiesSelect3}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect4", ref BookStrategiesSelect4);
            DebugLog.Info($"配置加载: BookStrategiesSelect4 = {BookStrategiesSelect4}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect5", ref BookStrategiesSelect5);
            DebugLog.Info($"配置加载: BookStrategiesSelect5 = {BookStrategiesSelect5}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect6", ref BookStrategiesSelect6);
            DebugLog.Info($"配置加载: BookStrategiesSelect6 = {BookStrategiesSelect6}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect7", ref BookStrategiesSelect7);
            DebugLog.Info($"配置加载: BookStrategiesSelect7 = {BookStrategiesSelect7}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect8", ref BookStrategiesSelect8);
            DebugLog.Info($"配置加载: BookStrategiesSelect8 = {BookStrategiesSelect8}");

            DomainManager.Mod.GetSetting(modIdStr, "BookStrategiesSelect9", ref BookStrategiesSelect9);
            DebugLog.Info($"配置加载: BookStrategiesSelect9 = {BookStrategiesSelect9}");

            DomainManager.Mod.GetSetting(modIdStr, "sexualOrientationControl", ref sexualOrientationControl);
            DebugLog.Info($"配置加载: sexualOrientationControl = {sexualOrientationControl}");

            DomainManager.Mod.GetSetting(modIdStr, "GetQiArtStrategyDeltaNeiliBonus", ref GetQiArtStrategyDeltaNeiliBonus);
            DebugLog.Info($"配置加载: GetQiArtStrategyDeltaNeiliBonus = {GetQiArtStrategyDeltaNeiliBonus}");

            // 通过 PatchBuilder 应用的补丁配置
            DomainManager.Mod.GetSetting(modIdStr, "LuckyLevel", ref LuckyLevel);
            DebugLog.Info($"配置加载: LuckyLevel = {LuckyLevel}");

            DomainManager.Mod.GetSetting(modIdStr, "collectResource", ref collectResource);
            DebugLog.Info($"配置加载: collectResource = {collectResource}");

            DomainManager.Mod.GetSetting(modIdStr, "GetCollectResourceAmount", ref GetCollectResourceAmount);
            DebugLog.Info($"配置加载: GetCollectResourceAmount = {GetCollectResourceAmount}");

            DomainManager.Mod.GetSetting(modIdStr, "CreateBuildingArea", ref CreateBuildingArea);
            DebugLog.Info($"配置加载: CreateBuildingArea = {CreateBuildingArea}");

            DomainManager.Mod.GetSetting(modIdStr, "CalcNeigongLoopingEffect", ref CalcNeigongLoopingEffect);
            DebugLog.Info($"配置加载: CalcNeigongLoopingEffect = {CalcNeigongLoopingEffect}");

            DomainManager.Mod.GetSetting(modIdStr, "OfflineUpdateShopManagement", ref OfflineUpdateShopManagement);
            DebugLog.Info($"配置加载: OfflineUpdateShopManagement = {OfflineUpdateShopManagement}");

            DomainManager.Mod.GetSetting(modIdStr, "ApplyLifeSkillCombatResult", ref ApplyLifeSkillCombatResult);
            DebugLog.Info($"配置加载: ApplyLifeSkillCombatResult = {ApplyLifeSkillCombatResult}");

            DomainManager.Mod.GetSetting(modIdStr, "CalcReadInCombat", ref CalcReadInCombat);
            DebugLog.Info($"配置加载: CalcReadInCombat = {CalcReadInCombat}");

            DomainManager.Mod.GetSetting(modIdStr, "CalcQiQrtInCombat", ref CalcQiQrtInCombat);
            DebugLog.Info($"配置加载: CalcQiQrtInCombat = {CalcQiQrtInCombat}");

            DomainManager.Mod.GetSetting(modIdStr, "CalcLootItem", ref CalcLootItem);
            DebugLog.Info($"配置加载: CalcLootItem = {CalcLootItem}");

            DomainManager.Mod.GetSetting(modIdStr, "InitPathContent", ref InitPathContent);
            DebugLog.Info($"配置加载: InitPathContent = {InitPathContent}");

            DomainManager.Mod.GetSetting(modIdStr, "GetStrategyProgressAddValue", ref GetStrategyProgressAddValue);
            DebugLog.Info($"配置加载: GetStrategyProgressAddValue = {GetStrategyProgressAddValue}");

            DomainManager.Mod.GetSetting(modIdStr, "ApplyImmediateReadingStrategyEffectForLifeSkill", ref ApplyImmediateReadingStrategyEffectForLifeSkill);
            DebugLog.Info($"配置加载: ApplyImmediateReadingStrategyEffectForLifeSkill = {ApplyImmediateReadingStrategyEffectForLifeSkill}");

            DomainManager.Mod.GetSetting(modIdStr, "ChoosyGetMaterial", ref ChoosyGetMaterial);
            DebugLog.Info($"配置加载: ChoosyGetMaterial = {ChoosyGetMaterial}");

            DomainManager.Mod.GetSetting(modIdStr, "ParallelUpdateOnMonthChange", ref ParallelUpdateOnMonthChange);
            DebugLog.Info($"配置加载: ParallelUpdateOnMonthChange = {ParallelUpdateOnMonthChange}");

            DomainManager.Mod.GetSetting(modIdStr, "AddChoosyRemainUpgradeData", ref AddChoosyRemainUpgradeData);
            DebugLog.Info($"配置加载: AddChoosyRemainUpgradeData = {AddChoosyRemainUpgradeData}");

            DebugLog.Info("所有配置项加载完成");
        }
    }
}
