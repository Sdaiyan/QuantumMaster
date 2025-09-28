using System;
using System.Collections.Generic;
using QuantumMaster.Shared;

namespace CombatMaster
{
    /// <summary>
    /// CombatConfigManager 的适配器，实现 IConfigProvider 接口
    /// 用于为 Shared 代码提供配置访问能力
    /// </summary>
    public class CombatConfigManagerAdapter : IConfigProvider
    {
        /// <summary>
        /// 获取全局气运等级
        /// </summary>
        public int GlobalLuckyLevel => CombatConfigManager.LuckyLevel;

        /// <summary>
        /// 获取气运等级因子映射表
        /// </summary>
        public Dictionary<int, float> LuckyLevelFactor => CombatMaster.LuckyLevelFactor;

        /// <summary>
        /// 获取随机数生成器
        /// </summary>
        public Random Random => CombatMaster.Random;

        /// <summary>
        /// 获取指定功能的气运等级
        /// </summary>
        /// <param name="featureKey">功能键名</param>
        /// <returns>气运等级（0-7），如果功能设置为跟随全局则返回全局气运等级</returns>
        public int GetFeatureLuckLevel(string featureKey)
        {
            return CombatConfigManager.GetFeatureLuckLevel(featureKey);
        }

        /// <summary>
        /// 检查指定功能是否启用（不是顺风顺水状态）
        /// </summary>
        /// <param name="featureKey">功能键名</param>
        /// <returns>true表示功能启用，false表示功能关闭（顺风顺水）</returns>
        public bool IsFeatureEnabled(string featureKey)
        {
            return CombatConfigManager.IsFeatureEnabled(featureKey);
        }

        /// <summary>
        /// 检查是否开启所有补丁
        /// </summary>
        public bool OpenAll => CombatMaster.openAll;

        /// <summary>
        /// 获取调试模式状态
        /// </summary>
        public bool Debug => CombatMaster.debug;
    }
}