using GameData.Domains;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;
using Redzen.Random;
using System;
using System.Collections.Generic;
using QuantumMaster.Shared;

namespace CombatMaster
{
    [PluginConfig("CombatMaster", "dai", "0.0.1")]
    public class CombatMaster : TaiwuRemakeHarmonyPlugin
    {
        Harmony harmony;
        public static bool debug = false;
        public static bool openAll = false; // 是否开启所有补丁

        public static Random Random = new Random();

        // lucklevel的 因子 映射表 - 与QuantumMaster保持一致
        public static Dictionary<int, float> LuckyLevelFactor = new Dictionary<int, float>
        {
            { 0, -0.67f }, // 命途多舛
            { 1, -0.33f }, // 时运不济
            { 2, 0.0f },   // 顺风顺水
            { 3, 0.2f },   // 左右逢源
            { 4, 0.4f },   // 心想事成
            { 5, 0.6f },   // 福星高照
            { 6, 0.8f },   // 洪福齐天
            { 7, 1.0f }    // 气运之子
        };

        public override void OnModSettingUpdate()
        {
            UpdateConfig();
        }

        public void UpdateConfig()
        {
            // 使用配置管理器加载所有配置项
            CombatConfigManager.LoadAllConfigs(ModIdStr);
        }

        public override void Initialize()
        {
            // 设置配置提供者，供 Shared 代码使用
            ConfigProvider.SetProvider(new CombatConfigManagerAdapter());

            UpdateConfig();
            
            harmony = new Harmony("CombatMaster");

            if (CombatConfigManager.LuckyLevel == 2)
            {
                DebugLog.Info($"[CombatMaster] 选择了顺风顺水气运，MOD将不会生效");
                return;
            }

            // 应用战斗相关补丁
            // TODO: 在这里添加具体的补丁应用逻辑
            ApplyClassPatches();
            ApplyPatchBuilderPatches();

            // 重置已处理方法列表
            GenericTranspiler.ResetProcessedMethods();
        }

        // Class Patch 补丁配置映射表 - 目前为空，后续添加战斗相关补丁
        private readonly Dictionary<string, (System.Type patchType, System.Func<bool> condition)> patchConfigMappings = 
            new Dictionary<string, (System.Type, System.Func<bool>)>
        {
            // 战斗相关的Class补丁
            { "JieQingKuaiJian", (typeof(Features.Combat.JieQingKuaiJianPatch), () => CombatConfigManager.IsFeatureEnabled("JieQingKuaiJian")) },
            { "XinWuDingYi", (typeof(Features.Combat.XinWuDingYiPatch), () => CombatConfigManager.IsFeatureEnabled("XinWuDingYi")) },
            { "JiuSiLiHunShou", (typeof(Features.Combat.JiuSiLiHunShouPatch), () => CombatConfigManager.IsFeatureEnabled("JiuSiLiHunShou")) },
            { "LeiZuBoJianShi", (typeof(Features.Combat.LeiZuBoJianShiPatch), () => CombatConfigManager.IsFeatureEnabled("LeiZuBoJianShi")) },
            { "LeiZuBoJianShiCloth", (typeof(Features.Combat.LeiZuBoJianShiClothPatch), () => CombatConfigManager.LeiZuBoJianShiCloth) },
            { "YueNvJianFa", (typeof(Features.Combat.YueNvJianFaPatch), () => CombatConfigManager.IsFeatureEnabled("YueNvJianFa")) },
            { "JingHuaShuiYue", (typeof(Features.Combat.JingHuaShuiYuePatch), () => CombatConfigManager.IsFeatureEnabled("JingHuaShuiYue")) },
            { "QianNianZui", (typeof(Features.Combat.QianNianZuiPatch), () => CombatConfigManager.IsFeatureEnabled("QianNianZui")) },
            { "FengMoZuiQuan", (typeof(Features.Combat.FengMoZuiQuanPatch), () => CombatConfigManager.IsFeatureEnabled("FengMoZuiQuan")) },
            { "SuiSuoYu", (typeof(Features.Combat.SuiSuoYuPatch), () => CombatConfigManager.IsFeatureEnabled("SuiSuoYu")) },
            { "FengGouQuan", (typeof(Features.Combat.FengGouQuanPatch), () => CombatConfigManager.IsFeatureEnabled("FengGouQuan")) },
            { "LingLiuXu", (typeof(Features.Combat.LingLiuXuPatch), () => CombatConfigManager.IsFeatureEnabled("LingLiuXu")) },
            { "QingNvLvBing", (typeof(Features.Combat.QingNvLvBingPatch), () => CombatConfigManager.IsFeatureEnabled("QingNvLvBing")) },
            { "XiaoZongYueGong", (typeof(Features.Combat.XiaoZongYueGongPatch), () => CombatConfigManager.IsFeatureEnabled("XiaoZongYueGong")) },
            { "KuangDao", (typeof(Features.Combat.KuangDaoPatch), () => CombatConfigManager.IsFeatureEnabled("KuangDao")) },
        };

        // PatchBuilder 补丁配置映射表 - 目前为空，后续添加战斗相关补丁
        private readonly Dictionary<string, (System.Func<Harmony, bool> patchMethod, System.Func<bool> condition)> patchBuilderMappings = 
            new Dictionary<string, (System.Func<Harmony, bool>, System.Func<bool>)>
        {
            // 战斗相关的PatchBuilder补丁
            { "JieQingKuaiJianBuilder", (Features.Combat.JieQingKuaiJianPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("JieQingKuaiJian")) },
            { "XinWuDingYiBuilder", (Features.Combat.XinWuDingYiPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("XinWuDingYi")) },
            { "JiuSiLiHunShouBuilder", (Features.Combat.JiuSiLiHunShouPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("JiuSiLiHunShou")) },
            { "LeiZuBoJianShiBuilder", (Features.Combat.LeiZuBoJianShiPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("LeiZuBoJianShi")) },
            { "YaoJiYunYuShiBuilder", (Features.Combat.YaoJiYunYuShiPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("YaoJiYunYuShi")) },
            { "JingHuaShuiYueBuilder", (Features.Combat.JingHuaShuiYuePatch.Apply, () => CombatConfigManager.IsFeatureEnabled("JingHuaShuiYue")) },
            { "FengMoZuiQuanBuilder", (Features.Combat.FengMoZuiQuanPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("FengMoZuiQuan")) },
            { "SuiSuoYuBuilder", (Features.Combat.SuiSuoYuPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("SuiSuoYu")) },
            { "FengGouQuanBuilder", (Features.Combat.FengGouQuanPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("FengGouQuan")) },
            { "LingLiuXuBuilder", (Features.Combat.LingLiuXuPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("LingLiuXu")) },
            { "QingNvLvBingBuilder", (Features.Combat.QingNvLvBingPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("QingNvLvBing")) },
            { "XiaoZongYueGongBuilder", (Features.Combat.XiaoZongYueGongPatch.Apply, () => CombatConfigManager.IsFeatureEnabled("XiaoZongYueGong")) },
        };

        // 应用 class 形式的补丁
        private void ApplyClassPatches()
        {
            DebugLog.Info("[CombatMaster] 开始应用类补丁...");
            int appliedClassPatches = 0;
            int skippedClassPatches = 0;

            // 遍历补丁配置映射表
            foreach (var patchConfig in patchConfigMappings)
            {
                string patchName = patchConfig.Key;
                var (patchType, condition) = patchConfig.Value;

                // 检查是否应该应用此补丁
                if (condition() || openAll)
                {
                    try
                    {
                        harmony.PatchAll(patchType);
                        appliedClassPatches++;
                        DebugLog.Info($"[CombatMaster] 应用补丁类: {patchName}");
                    }
                    catch (Exception ex)
                    {
                        DebugLog.Warning($"[CombatMaster] 应用补丁类 {patchName} 时出错: {ex.Message}");
                        skippedClassPatches++;
                    }
                }
                else
                {
                    DebugLog.Info($"[CombatMaster] 跳过补丁类: {patchName} (已禁用)");
                    skippedClassPatches++;
                }
            }

            DebugLog.Info($"[CombatMaster] 类补丁应用完成: 成功 {appliedClassPatches} 个, 跳过 {skippedClassPatches} 个");
        }

        // 应用 PatchBuilder 形式的补丁
        private void ApplyPatchBuilderPatches()
        {
            DebugLog.Info("[CombatMaster] 开始应用 PatchBuilder 补丁...");
            int appliedBuilderPatches = 0;
            int skippedBuilderPatches = 0;

            // 遍历 PatchBuilder 补丁配置映射表
            foreach (var patchConfig in patchBuilderMappings)
            {
                string patchName = patchConfig.Key;
                var (patchMethod, condition) = patchConfig.Value;

                // 检查是否应该应用此补丁
                if (condition() || openAll)
                {
                    try
                    {
                        bool success = patchMethod(harmony);
                        if (success)
                        {
                            appliedBuilderPatches++;
                            DebugLog.Info($"[CombatMaster] 应用 PatchBuilder 补丁: {patchName}");
                        }
                        else
                        {
                            DebugLog.Info($"[CombatMaster] 跳过 PatchBuilder 补丁: {patchName} (条件不满足)");
                            skippedBuilderPatches++;
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLog.Warning($"[CombatMaster] 应用 PatchBuilder 补丁 {patchName} 时出错: {ex.Message}");
                        skippedBuilderPatches++;
                    }
                }
                else
                {
                    DebugLog.Info($"[CombatMaster] 跳过 PatchBuilder 补丁: {patchName} (已禁用)");
                    skippedBuilderPatches++;
                }
            }

            DebugLog.Info($"[CombatMaster] PatchBuilder 补丁应用完成: 成功 {appliedBuilderPatches} 个, 跳过 {skippedBuilderPatches} 个");
            
            // 应用所有已注册的 GenericTranspiler 补丁
            DebugLog.Info("[CombatMaster] 开始应用 GenericTranspiler 补丁...");
            GenericTranspiler.ApplyPatches(harmony);
            DebugLog.Info("[CombatMaster] GenericTranspiler 补丁应用完成");
        }

        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }
    }
}