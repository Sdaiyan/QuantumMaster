using GameData.Utilities;

namespace QuantumMaster.Shared
{
    /// <summary>
    /// 日志辅助类，只有在debug模式下才输出日志
    /// </summary>
    public static class DebugLog
    {
        public static void Info(string message)
        {
            if (ConfigProvider.Instance.Debug)
            {
                AdaptableLog.Info(message);
            }
        }

        public static void Warning(string message)
        {
            if (ConfigProvider.Instance.Debug)
            {
                AdaptableLog.Warning(message);
            }
        }

        public static void Error(string message)
        {
            // 错误日志始终输出，不受debug模式影响
            AdaptableLog.Error(message);
        }
    }
}