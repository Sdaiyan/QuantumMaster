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

        // 瑶姬云雨式爱慕概率气运设置
        public static int YaoJiYunYuShi = 0;

        // 鬼夜哭反击成功后的特效触发概率气运设置
        public static int GuiYeKu = 0;

        // 越女剑法追击几率提升气运设置
        public static int YueNvJianFa = 0;
        
        // 镜花水月绝对化解概率气运设置
        public static int JingHuaShuiYue = 0;

        // 疯魔醉拳复读概率气运设置
        public static int FengMoZuiQuan = 0;
          // 随所欲获得真气概率气运设置
        public static int SuiSuoYu = 0;

        // 千年醉获得的几率增益增加气运设置
        public static int QianNianZui = 0;

        // 疯狗拳获得增伤的概率气运设置
        public static int FengGouQuan = 0;

        // 凌六虚命中要害概率气运设置
        public static int LingLiuXu = 0;

        // 青女履冰复读概率气运设置
        public static int QingNvLvBing = 0;

        // 小纵跃功不消耗脚力概率气运设置
        public static int XiaoZongYueGong = 0;

        // 狂刀命中DEBUFF减少气运设置
        public static int KuangDao = 0;

        // 惊鬼符敌人逃跑概率气运设置
        public static int JingGuiFu = 0;

        // 木公咒打断功法概率气运设置
        public static int MuGongZhou = 0;

        // 九痴香功法打断概率气运设置
        public static int JiuChiXiang = 0;

        // 离合指反噬概率气运设置
        public static int LiHeZhi = 0;

        // 伏阴指转化旧伤概率气运设置
        public static int FuYinZhi = 0;

        // 太素绝手打断施展功法概率气运设置
        public static int TaiSuJueShou = 0;

        // 漫天花雨式额外封穴/破绽概率气运设置
        public static int ManTianHuaYuShi = 0;
        
        // 功能开关映射表 - 将功能名映射到对应的配置字段
        private static readonly Dictionary<string, string> FeatureMap = new Dictionary<string, string>
        {
            { "JieQingKuaiJian", "JieQingKuaiJian" },
            { "XinWuDingYi", "XinWuDingYi" },
            { "JiuSiLiHunShou", "JiuSiLiHunShou" },
            { "LeiZuBoJianShi", "LeiZuBoJianShi" },
            { "YaoJiYunYuShi", "YaoJiYunYuShi" },
            { "GuiYeKu", "GuiYeKu" },
            { "YueNvJianFa", "YueNvJianFa" },
            { "JingHuaShuiYue", "JingHuaShuiYue" },
            { "FengMoZuiQuan", "FengMoZuiQuan" },
            { "SuiSuoYu", "SuiSuoYu" },
            { "QianNianZui", "QianNianZui" },
            { "FengGouQuan", "FengGouQuan" },
            { "LingLiuXu", "LingLiuXu" },
            { "QingNvLvBing", "QingNvLvBing" },
            { "XiaoZongYueGong", "XiaoZongYueGong" },
            { "KuangDao", "KuangDao" },
            { "JingGuiFu", "JingGuiFu" },
            { "MuGongZhou", "MuGongZhou" },
            { "JiuChiXiang", "JiuChiXiang" },
            { "LiHeZhi", "LiHeZhi" },
            { "FuYinZhi", "FuYinZhi" },
            { "TaiSuJueShou", "TaiSuJueShou" },
            { "ManTianHuaYuShi", "ManTianHuaYuShi" }
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

            // 加载瑶姬云雨式配置
            DomainManager.Mod.GetSetting(modIdStr, "YaoJiYunYuShi", ref YaoJiYunYuShi);
            DebugLog.Info($"[CombatMaster] 瑶姬云雨式爱慕概率: {YaoJiYunYuShi}");

            // 加载鬼夜哭配置
            DomainManager.Mod.GetSetting(modIdStr, "GuiYeKu", ref GuiYeKu);
            DebugLog.Info($"[CombatMaster] 鬼夜哭反击成功后的特效触发概率: {GuiYeKu}");

            // 加载越女剑法配置
            DomainManager.Mod.GetSetting(modIdStr, "YueNvJianFa", ref YueNvJianFa);
            DebugLog.Info($"[CombatMaster] 越女剑法追击几率提升: {YueNvJianFa}");            // 加载镜花水月配置
            DomainManager.Mod.GetSetting(modIdStr, "JingHuaShuiYue", ref JingHuaShuiYue);
            DebugLog.Info($"[CombatMaster] 镜花水月绝对化解概率: {JingHuaShuiYue}");

            // 加载疯魔醉拳配置
            DomainManager.Mod.GetSetting(modIdStr, "FengMoZuiQuan", ref FengMoZuiQuan);
            DebugLog.Info($"[CombatMaster] 疯魔醉拳复读概率: {FengMoZuiQuan}");            // 加载随所欲配置
            DomainManager.Mod.GetSetting(modIdStr, "SuiSuoYu", ref SuiSuoYu);
            DebugLog.Info($"[CombatMaster] 随所欲获得真气概率: {SuiSuoYu}");

            // 加载千年醉配置
            DomainManager.Mod.GetSetting(modIdStr, "QianNianZui", ref QianNianZui);
            DebugLog.Info($"[CombatMaster] 千年醉获得的几率增益增加: {QianNianZui}");

            // 加载疯狗拳配置
            DomainManager.Mod.GetSetting(modIdStr, "FengGouQuan", ref FengGouQuan);
            DebugLog.Info($"[CombatMaster] 疯狗拳获得增伤的概率: {FengGouQuan}");

            // 加载凌六虚配置
            DomainManager.Mod.GetSetting(modIdStr, "LingLiuXu", ref LingLiuXu);
            DebugLog.Info($"[CombatMaster] 凌六虚命中要害概率: {LingLiuXu}");

            // 加载青女履冰配置
            DomainManager.Mod.GetSetting(modIdStr, "QingNvLvBing", ref QingNvLvBing);
            DebugLog.Info($"[CombatMaster] 青女履冰复读概率: {QingNvLvBing}");

            // 加载小纵跃功配置
            DomainManager.Mod.GetSetting(modIdStr, "XiaoZongYueGong", ref XiaoZongYueGong);
            DebugLog.Info($"[CombatMaster] 小纵跃功不消耗脚力概率: {XiaoZongYueGong}");

            // 加载狂刀配置
            DomainManager.Mod.GetSetting(modIdStr, "KuangDao", ref KuangDao);
            DebugLog.Info($"[CombatMaster] 狂刀命中DEBUFF减少: {KuangDao}");

            // 加载惊鬼符配置
            DomainManager.Mod.GetSetting(modIdStr, "JingGuiFu", ref JingGuiFu);
            DebugLog.Info($"[CombatMaster] 惊鬼符敌人逃跑概率: {JingGuiFu}");

            // 加载木公咒配置
            DomainManager.Mod.GetSetting(modIdStr, "MuGongZhou", ref MuGongZhou);
            DebugLog.Info($"[CombatMaster] 木公咒打断功法概率: {MuGongZhou}");

            // 加载九痴香配置
            DomainManager.Mod.GetSetting(modIdStr, "JiuChiXiang", ref JiuChiXiang);
            DebugLog.Info($"[CombatMaster] 九痴香功法打断概率: {JiuChiXiang}");

            // 加载离合指配置
            DomainManager.Mod.GetSetting(modIdStr, "LiHeZhi", ref LiHeZhi);
            DebugLog.Info($"[CombatMaster] 离合指反噬概率: {LiHeZhi}");

            // 加载伏阴指配置
            DomainManager.Mod.GetSetting(modIdStr, "FuYinZhi", ref FuYinZhi);
            DebugLog.Info($"[CombatMaster] 伏阴指转化旧伤概率: {FuYinZhi}");

            // 加载太素绝手配置
            DomainManager.Mod.GetSetting(modIdStr, "TaiSuJueShou", ref TaiSuJueShou);
            DebugLog.Info($"[CombatMaster] 太素绝手打断施展功法概率: {TaiSuJueShou}");

            // 加载漫天花雨式配置
            DomainManager.Mod.GetSetting(modIdStr, "ManTianHuaYuShi", ref ManTianHuaYuShi);
            DebugLog.Info($"[CombatMaster] 漫天花雨式额外封穴/破绽概率: {ManTianHuaYuShi}");

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
        }        /// <summary>
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
                case "YaoJiYunYuShi": return YaoJiYunYuShi;
                case "GuiYeKu": return GuiYeKu;
                case "YueNvJianFa": return YueNvJianFa;
                case "JingHuaShuiYue": return JingHuaShuiYue;
                case "FengMoZuiQuan": return FengMoZuiQuan;
                case "SuiSuoYu": return SuiSuoYu;
                case "QianNianZui": return QianNianZui;
                case "FengGouQuan": return FengGouQuan;
                case "LingLiuXu": return LingLiuXu;
                case "QingNvLvBing": return QingNvLvBing;
                case "XiaoZongYueGong": return XiaoZongYueGong;
                case "KuangDao": return KuangDao;
                case "JingGuiFu": return JingGuiFu;
                case "MuGongZhou": return MuGongZhou;
                case "JiuChiXiang": return JiuChiXiang;
                case "LiHeZhi": return LiHeZhi;
                case "FuYinZhi": return FuYinZhi;
                case "TaiSuJueShou": return TaiSuJueShou;
                case "ManTianHuaYuShi": return ManTianHuaYuShi;
                default: 
                    DebugLog.Warning($"[CombatMaster] 未知的配置键: {configKey}");
                    return 0;
            }
        }
    }
}