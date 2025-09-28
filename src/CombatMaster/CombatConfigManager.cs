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

        // 战斗相关功能开关 - 示例配置项，可根据需要添加更多
        public static int CombatLucky = 0;          // 战斗幸运加成
        public static int WeaponDurability = 3;    // 武器耐久控制
        public static int ArmorDurability = 3;     // 护甲耐久控制
        public static int CombatRead = 0;          // 战斗中读书
        public static int CombatQiQrt = 0;         // 战斗中周天运转
        public static int LootDrop = 0;            // 战利品掉落
        public static int LifeSkillCombat = 0;     // 生活技能战斗应用

        // 功能开关映射表 - 将功能名映射到对应的配置字段
        private static readonly Dictionary<string, string> FeatureMap = new Dictionary<string, string>
        {
            { "combatLucky", "CombatLucky" },
            { "weaponDurability", "WeaponDurability" },
            { "armorDurability", "ArmorDurability" },
            { "combatRead", "CombatRead" },
            { "combatQiQrt", "CombatQiQrt" },
            { "lootDrop", "LootDrop" },
            { "lifeSkillCombat", "LifeSkillCombat" }
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

            // 加载战斗相关配置
            DomainManager.Mod.GetSetting(modIdStr, "combatLucky", ref CombatLucky);
            DebugLog.Info($"[CombatMaster] 战斗幸运加成: {CombatLucky}");

            DomainManager.Mod.GetSetting(modIdStr, "weaponDurability", ref WeaponDurability);
            DebugLog.Info($"[CombatMaster] 武器耐久控制: {WeaponDurability}");

            DomainManager.Mod.GetSetting(modIdStr, "armorDurability", ref ArmorDurability);
            DebugLog.Info($"[CombatMaster] 护甲耐久控制: {ArmorDurability}");

            DomainManager.Mod.GetSetting(modIdStr, "combatRead", ref CombatRead);
            DebugLog.Info($"[CombatMaster] 战斗中读书: {CombatRead}");

            DomainManager.Mod.GetSetting(modIdStr, "combatQiQrt", ref CombatQiQrt);
            DebugLog.Info($"[CombatMaster] 战斗中周天运转: {CombatQiQrt}");

            DomainManager.Mod.GetSetting(modIdStr, "lootDrop", ref LootDrop);
            DebugLog.Info($"[CombatMaster] 战利品掉落: {LootDrop}");

            DomainManager.Mod.GetSetting(modIdStr, "lifeSkillCombat", ref LifeSkillCombat);
            DebugLog.Info($"[CombatMaster] 生活技能战斗应用: {LifeSkillCombat}");

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
                case "CombatLucky": return CombatLucky;
                case "WeaponDurability": return WeaponDurability;
                case "ArmorDurability": return ArmorDurability;
                case "CombatRead": return CombatRead;
                case "CombatQiQrt": return CombatQiQrt;
                case "LootDrop": return LootDrop;
                case "LifeSkillCombat": return LifeSkillCombat;
                default: 
                    DebugLog.Warning($"[CombatMaster] 未知的配置键: {configKey}");
                    return 0;
            }
        }
    }
}