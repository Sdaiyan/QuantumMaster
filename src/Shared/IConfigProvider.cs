using System;
using System.Collections.Generic;

namespace QuantumMaster.Shared
{
    /// <summary>
    /// 配置提供者接口，用于解耦 Shared 代码对具体配置实现的依赖
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// 获取全局气运等级
        /// </summary>
        int GlobalLuckyLevel { get; }

        /// <summary>
        /// 获取气运等级因子映射表
        /// </summary>
        Dictionary<int, float> LuckyLevelFactor { get; }

        /// <summary>
        /// 获取随机数生成器
        /// </summary>
        Random Random { get; }

        /// <summary>
        /// 获取指定功能的气运等级
        /// </summary>
        /// <param name="featureKey">功能键名</param>
        /// <returns>气运等级（0-7），如果功能设置为跟随全局则返回全局气运等级</returns>
        int GetFeatureLuckLevel(string featureKey);

        /// <summary>
        /// 检查指定功能是否启用（不是顺风顺水状态）
        /// </summary>
        /// <param name="featureKey">功能键名</param>
        /// <returns>true表示功能启用，false表示功能关闭（顺风顺水）</returns>
        bool IsFeatureEnabled(string featureKey);

        /// <summary>
        /// 检查是否开启所有补丁
        /// </summary>
        bool OpenAll { get; }

        /// <summary>
        /// 获取调试模式状态
        /// </summary>
        bool Debug { get; }
    }

    /// <summary>
    /// 配置提供者静态访问器
    /// </summary>
    public static class ConfigProvider
    {
        private static IConfigProvider _instance;

        /// <summary>
        /// 设置配置提供者实例
        /// </summary>
        /// <param name="provider">配置提供者实例</param>
        public static void SetProvider(IConfigProvider provider)
        {
            _instance = provider;
        }

        /// <summary>
        /// 获取当前配置提供者实例
        /// </summary>
        public static IConfigProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("ConfigProvider 未初始化。请在使用前调用 SetProvider 方法。");
                }
                return _instance;
            }
        }
    }
}