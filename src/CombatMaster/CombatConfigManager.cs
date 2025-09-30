using TaiwuModdingLib.Core.Plugin;
using GameData.Domains;
using System.Collections.Generic;
using QuantumMaster.Shared;

namespace CombatMaster
{
    /// <summary>
    /// CombatMaster配置管理器，负责读取和管理所有战斗相关的MOD配置项
    /// </summary>
    public static class CombatConfigManager
    {
        // 气运等级
        public static int LuckyLevel = 7;

        // 界青快剑重复触发概率气运设置
        public static int JieQingKuaiJian = 0;

        // 心无定意获得式或者减少式消耗的效果生效概率气运设置
        public static int XinWuDingYi = 0;

        // 九死离魂手触发秘闻概率气运设置
        public static int JiuSiLiHunShou = 0;

        // 嫘祖剝茧式脱装备概率气运设置
        public static int LeiZuBoJianShi = 0;

        // 嫘祖剝茧式优先脱外观衣服设置
        public static bool LeiZuBoJianShiCloth = true;

        // 功能开关映射表 - 将功能名映射到对应的配置字段
        private static readonly Dictionary<string, string> FeatureMap = new Dictionary<string, string>
        {
            { "JieQingKuaiJian", "JieQingKuaiJian" },
            { "XinWuDingYi", "XinWuDingYi" },
            { "JiuSiLiHunShou", "JiuSiLiHunShou" },
            { "LeiZuBoJianShi", "LeiZuBoJianShi" }
        };

        /// <summary>
        /// 从游戏配置中读取所有 MOD 设置
        /// </summary>
        /// <param name="modIdStr">MOD ID 字符串</param>
        public static void LoadAllConfigs(string modIdStr)
        {
            DebugLog.Info("[CombatMaster] 开始加载配置项...");
            
            // 加载气运等级
            DomainManager.Mod.GetSetting(modIdStr, "CombatLuckyLevel", ref LuckyLevel);
            DebugLog.Info($"[CombatMaster] 气运等级: {LuckyLevel}");

            // 加载界青快剑配置
            DomainManager.Mod.GetSetting(modIdStr, "JieQingKuaiJian", ref JieQingKuaiJian);
            DebugLog.Info($"[CombatMaster] 界青快剑重复触发概率: {JieQingKuaiJian}");

            // 加载心无定意配置
            DomainManager.Mod.GetSetting(modIdStr, "XinWuDingYi", ref XinWuDingYi);
            DebugLog.Info($"[CombatMaster] 心无定意效果生效概率: {XinWuDingYi}");

            // 加载九死离魂手配置
            DomainManager.Mod.GetSetting(modIdStr, "JiuSiLiHunShou", ref JiuSiLiHunShou);
            DebugLog.Info($"[CombatMaster] 九死离魂手触发秘闻概率: {JiuSiLiHunShou}");

            // 加载嫘祖剝茧式配置
            DomainManager.Mod.GetSetting(modIdStr, "LeiZuBoJianShi", ref LeiZuBoJianShi);
            DebugLog.Info($"[CombatMaster] 嫘祖剝茧式脱装备概率: {LeiZuBoJianShi}");

            // 加载嫘祖剝茧式优先脱衣服配置
            DomainManager.Mod.GetSetting(modIdStr, "LeiZuBoJianShiCloth", ref LeiZuBoJianShiCloth);
            DebugLog.Info($"[CombatMaster] 嫘祖剝茧式优先脱外观衣服: {LeiZuBoJianShiCloth}");

            DebugLog.Info("[CombatMaster] 所有配置项加载完成");
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
                if (!FeatureMap.ContainsKey(featureKey))
                {
                    DebugLog.Warning($"[CombatMaster] 未找到功能配置键: {featureKey}，使用全局气运等级");
                    return LuckyLevel;
                }

                var configValue = GetConfigValue(FeatureMap[featureKey]);
                return configValue == 0 ? LuckyLevel : configValue - 1; // 0=跟随全局，1-8映射到0-7
            }
            catch (System.Exception ex)
            {
                DebugLog.Error($"[CombatMaster] 获取功能气运等级时发生错误: {featureKey}, 异常: {ex.Message}，使用全局气运等级");
                return LuckyLevel;
            }
        }

        /// <summary>
        /// 检查指定功能是否启用（不是顺风顺水状态）
        /// </summary>
        /// <param name="featureKey">功能键名</param>
        /// <returns>true表示功能启用，false表示功能关闭（顺风顺水）</returns>
        public static bool IsFeatureEnabled(string featureKey)
        {
            // 如果全局开启，则所有功能都启用
            if (CombatMaster.openAll) return true;
            
            int luckLevel = GetFeatureLuckLevel(featureKey);
            return luckLevel != 2; // 2 = 顺风顺水（关闭功能）
        }

        /// <summary>
        /// 根据配置键名获取配置值
        /// </summary>
        private static int GetConfigValue(string configKey)
        {
            switch (configKey)
            {
                case "JieQingKuaiJian": return JieQingKuaiJian;
                case "XinWuDingYi": return XinWuDingYi;
                case "JiuSiLiHunShou": return JiuSiLiHunShou;
                case "LeiZuBoJianShi": return LeiZuBoJianShi;
                default: 
                    DebugLog.Warning($"[CombatMaster] 未知的配置键: {configKey}");
                    return 0;
            }
        }
    }
}