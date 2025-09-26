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
        public static int steal; // 偷窃气运等级：0=跟随全局，1-8=独立气运等级
        public static int scam; // 唬骗气运等级：0=跟随全局，1-8=独立气运等级
        public static int rob; // 抢劫气运等级：0=跟随全局，1-8=独立气运等级
        public static int stealLifeSkill; // 偷学生活技能气运等级：0=跟随全局，1-8=独立气运等级
        public static int stealCombatSkill; // 偷学战斗技能气运等级：0=跟随全局，1-8=独立气运等级
        public static int poison; // 下毒气运等级：0=跟随全局，1-8=独立气运等级
        public static int plotHarm; // 暗害气运等级：0=跟随全局，1-8=独立气运等级
        public static int genderControl; // 生成性别控制：0=关闭功能，1=修改为女，2=修改为男
        public static int ropeOrSword = 0; // 绳子/剑柄救人的独立气运设置，0=跟随全局
        public static int OfflineCalcGeneralAction_TeachSkill = 0; // 太吾受到指点的独立气运设置，0=跟随全局
        public static int CatchCricket; // 抓蛐蛐必定成功
        public static int CatchCricketDouble = 0; // 抓蛐蛐时获得双只蛐蛐
        public static bool InitResources; // 生成世界时，每个地块上的资源为浮动区间的最大值，受到难度的影响
        public static int CheckCricketIsSmart; // 蛐蛐是否可以升级，如果符合条件必定升级
        public static int GetCurrReadingEventBonusRate = 0; // 灵光一闪概率不为0时，必定灵光一闪
        public static bool GeneratePageIncompleteState; // 生成书籍时，完整的书页为浮动区间的最大值，亡佚书页为浮动区间最小值，并且完整的书页会出现在书本的前篇位置
        public static bool FixedPagePos; // 位置固定在靠前
        public static bool CricketInitialize; // 生成蛐蛐时，必定生成耐久上限为理论上限值的，不受伤的蛐蛐
        public static int TryAddLoopingEvent; // 天人感应的独立气运设置，0=跟随全局
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
        public static int GetQiArtStrategyDeltaNeiliBonus; // 周天内力策略收益最大
        
        // 周天策略控制配置
        public static int QiArtStrategiesSelect1; // 第 1 个周天策略
        public static int QiArtStrategiesSelect2; // 第 2 个周天策略
        public static int QiArtStrategiesSelect3; // 第 3 个周天策略
        public static int QiArtStrategiesSelect4; // 第 4 个周天策略
        public static int QiArtStrategiesSelect5; // 第 5 个周天策略
        public static int QiArtStrategiesSelect6; // 第 6 个周天策略

        // 通过 PatchBuilder 应用的补丁配置
        public static int LuckyLevel;
        public static int CreateBuildingArea; // 【气运】世界初始建筑：0=跟随全局，1-8=独立气运等级
        public static int CalcNeigongLoopingEffect = 0; // 周天内力获取的独立气运设置，0=跟随全局
        public static int collectResource = 0; // 采集资源引子的独立气运设置，0=跟随全局
        public static int GetCollectResourceAmount = 0; // 采集资源数量的独立气运设置，0=跟随全局
        public static int OfflineUpdateShopManagement; // 【气运】太吾村经营成功率
        public static int ApplyLifeSkillCombatResult = 0; // 教艺读书&周天的独立气运设置，0=跟随全局
        public static int CalcReadInCombat = 0; // 战斗读书的独立气运设置，0=跟随全局
        public static int CalcQiQrtInCombat = 0; // 战斗周天运转的独立气运设置，0=跟随全局
        public static int CalcLootItem = 0; // 战利品概率的独立气运设置，0=跟随全局
        public static int CheckReduceWeaponDurability = 0; // 减少武器耐久消耗概率（强制扣除时无效）的独立气运设置，0=跟随全局
        public static int CheckReduceArmorDurability = 0; // 减少护甲耐久消耗概率（强制扣除时无效）的独立气运设置，0=跟随全局
        public static int InitPathContent = 0; // 奇遇收获资源时，数量为浮动区间的上限
        public static int GetStrategyProgressAddValue = 0; // 读书进度策略
        public static int SetReadingStrategy = 0; // 读书效率策略
        public static int ChoosyGetMaterial = 0; // 精挑细选品质升级的独立气运设置，0=跟随全局
        public static int AddChoosyRemainUpgradeData = 0; // 精挑细选过月累计的独立气运设置，0=跟随全局
        public static int ParallelUpdateOnMonthChange = 0; // 过月地块资源恢复的独立气运设置，0=跟随全局
        public static int BuildingRandomCorrection; // 【气运】太吾村经营收益
        public static int BuildingManageHarvestSpecialSuccessRate; // 【气运】赌坊与青楼基础暴击率
        public static int UpdateShopBuildingTeach; // 【气运】村民经营资质增加概率
        
        /// <summary>
        /// 村庄资源点获得心材概率气运设置
        /// </summary>
        public static int UpdateResourceBlockBuildingCoreProducing = 0;

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

            DomainManager.Mod.GetSetting(modIdStr, "OfflineCalcGeneralAction_TeachSkill", ref OfflineCalcGeneralAction_TeachSkill);
            DebugLog.Info($"配置加载: OfflineCalcGeneralAction_TeachSkill = {OfflineCalcGeneralAction_TeachSkill}");

            DomainManager.Mod.GetSetting(modIdStr, "CatchCricket", ref CatchCricket);
            DebugLog.Info($"配置加载: CatchCricket = {CatchCricket}");

            DomainManager.Mod.GetSetting(modIdStr, "CatchCricketDouble", ref CatchCricketDouble);
            DebugLog.Info($"配置加载: CatchCricketDouble = {CatchCricketDouble}");

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

            // 周天策略控制配置加载
            DomainManager.Mod.GetSetting(modIdStr, "QiArtStrategiesSelect1", ref QiArtStrategiesSelect1);
            DebugLog.Info($"配置加载: QiArtStrategiesSelect1 = {QiArtStrategiesSelect1}");

            DomainManager.Mod.GetSetting(modIdStr, "QiArtStrategiesSelect2", ref QiArtStrategiesSelect2);
            DebugLog.Info($"配置加载: QiArtStrategiesSelect2 = {QiArtStrategiesSelect2}");

            DomainManager.Mod.GetSetting(modIdStr, "QiArtStrategiesSelect3", ref QiArtStrategiesSelect3);
            DebugLog.Info($"配置加载: QiArtStrategiesSelect3 = {QiArtStrategiesSelect3}");

            DomainManager.Mod.GetSetting(modIdStr, "QiArtStrategiesSelect4", ref QiArtStrategiesSelect4);
            DebugLog.Info($"配置加载: QiArtStrategiesSelect4 = {QiArtStrategiesSelect4}");

            DomainManager.Mod.GetSetting(modIdStr, "QiArtStrategiesSelect5", ref QiArtStrategiesSelect5);
            DebugLog.Info($"配置加载: QiArtStrategiesSelect5 = {QiArtStrategiesSelect5}");

            DomainManager.Mod.GetSetting(modIdStr, "QiArtStrategiesSelect6", ref QiArtStrategiesSelect6);
            DebugLog.Info($"配置加载: QiArtStrategiesSelect6 = {QiArtStrategiesSelect6}");

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

            DomainManager.Mod.GetSetting(modIdStr, "CheckReduceWeaponDurability", ref CheckReduceWeaponDurability);
            DebugLog.Info($"配置加载: CheckReduceWeaponDurability = {CheckReduceWeaponDurability}");

            DomainManager.Mod.GetSetting(modIdStr, "CheckReduceArmorDurability", ref CheckReduceArmorDurability);
            DebugLog.Info($"配置加载: CheckReduceArmorDurability = {CheckReduceArmorDurability}");

            DomainManager.Mod.GetSetting(modIdStr, "InitPathContent", ref InitPathContent);
            DebugLog.Info($"配置加载: InitPathContent = {InitPathContent}");

            DomainManager.Mod.GetSetting(modIdStr, "GetStrategyProgressAddValue", ref GetStrategyProgressAddValue);
            DebugLog.Info($"配置加载: GetStrategyProgressAddValue = {GetStrategyProgressAddValue}");

            DomainManager.Mod.GetSetting(modIdStr, "SetReadingStrategy", ref SetReadingStrategy);
            DebugLog.Info($"配置加载: SetReadingStrategy = {SetReadingStrategy}");

            DomainManager.Mod.GetSetting(modIdStr, "ChoosyGetMaterial", ref ChoosyGetMaterial);
            DebugLog.Info($"配置加载: ChoosyGetMaterial = {ChoosyGetMaterial}");

            DomainManager.Mod.GetSetting(modIdStr, "ParallelUpdateOnMonthChange", ref ParallelUpdateOnMonthChange);
            DebugLog.Info($"配置加载: ParallelUpdateOnMonthChange = {ParallelUpdateOnMonthChange}");

            DomainManager.Mod.GetSetting(modIdStr, "AddChoosyRemainUpgradeData", ref AddChoosyRemainUpgradeData);
            DebugLog.Info($"配置加载: AddChoosyRemainUpgradeData = {AddChoosyRemainUpgradeData}");

            DomainManager.Mod.GetSetting(modIdStr, "BuildingRandomCorrection", ref BuildingRandomCorrection);
            DebugLog.Info($"配置加载: BuildingRandomCorrection = {BuildingRandomCorrection}");

            DomainManager.Mod.GetSetting(modIdStr, "BuildingManageHarvestSpecialSuccessRate", ref BuildingManageHarvestSpecialSuccessRate);
            DebugLog.Info($"配置加载: BuildingManageHarvestSpecialSuccessRate = {BuildingManageHarvestSpecialSuccessRate}");

            DomainManager.Mod.GetSetting(modIdStr, "UpdateShopBuildingTeach", ref UpdateShopBuildingTeach);
            DebugLog.Info($"配置加载: UpdateShopBuildingTeach = {UpdateShopBuildingTeach}");

            DomainManager.Mod.GetSetting(modIdStr, "UpdateResourceBlockBuildingCoreProducing", ref UpdateResourceBlockBuildingCoreProducing);
            DebugLog.Info($"配置加载: UpdateResourceBlockBuildingCoreProducing = {UpdateResourceBlockBuildingCoreProducing}");

            DebugLog.Info("所有配置项加载完成");
        }

        /// <summary>
        /// 获取指定功能的气运等级
        /// </summary>
        /// <param name="featureKey">功能键名</param>
        /// <returns>气运等级（0-7），如果功能设置为跟随全局则返回全局气运等级</returns>
        public static int GetFeatureLuckLevel(string featureKey)
        {
            try
            {
                // 使用反射获取对应的配置属性
                var field = typeof(ConfigManager).GetField(featureKey, 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                
                if (field == null)
                {
                    DebugLog.Warning($"未找到功能配置键: {featureKey}，使用全局气运等级");
                    return LuckyLevel;
                }

                // 如果是 int 类型，说明支持独立气运设置
                if (field.FieldType == typeof(int))
                {
                    int value = (int)field.GetValue(null);
                    return value == 0 ? LuckyLevel : value - 1; // 0=跟随全局，1-8映射到0-7
                }
                // 如果是 bool 类型，说明是传统的开关功能，使用全局气运
                else if (field.FieldType == typeof(bool))
                {
                    return LuckyLevel;
                }
                else
                {
                    DebugLog.Warning($"功能配置键 {featureKey} 的类型不支持气运设置: {field.FieldType}，使用全局气运等级");
                    return LuckyLevel;
                }
            }
            catch (System.Exception ex)
            {
                DebugLog.Error($"获取功能气运等级时发生错误: {featureKey}, 异常: {ex.Message}，使用全局气运等级");
                return LuckyLevel;
            }
        }

        /// <summary>
        /// 检查指定功能是否启用（不是顺风顺水状态）
        /// 统一处理功能激活逻辑，包含全局开关和气运等级判断
        /// </summary>
        /// <param name="featureKey">功能键名</param>
        /// <returns>true表示功能启用，false表示功能关闭（顺风顺水）</returns>
        public static bool IsFeatureEnabled(string featureKey)
        {
            // 如果全局开启，则所有功能都启用
            if (QuantumMaster.openAll) return true;
            
            int luckLevel = GetFeatureLuckLevel(featureKey);
            return luckLevel != 2; // 2 = 顺风顺水（关闭功能）
        }
    }
}
