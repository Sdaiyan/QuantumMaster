using GameData.Domains;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;
using Character = GameData.Domains.Character.Character;
using Redzen.Random;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using System.Diagnostics;
using GameData.Domains.Map;
using Config;
using GameData.Domains.Item;
using GameData.Utilities;
using GameData.Domains.Taiwu;
using GameData.Domains.Extra;
using GameData.Domains.Building;
using GameData.Domains.TaiwuEvent.DisplayEvent;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace QuantumMaster
{
	[PluginConfig("QuantumMaster", "dai", "0.0.1")]
	public class QuantumMaster : TaiwuRemakeHarmonyPlugin
	{
		Harmony harmony;
		PatchBuilder patchBuilder;
		public static bool debug = false;
		public static bool openAll = false; // 是否开启所有补丁

		public static Random Random = new Random();


		// 配置项已迁移到 ConfigManager 类中进行统一管理

		// lucklevel的 因子 映射表
		public static Dictionary<int, float> LuckyLevelFactor = new Dictionary<int, float>
		{
			{ 0, -0.67f }, // 命途多舛
			{ 1, -0.33f }, // 时运不济
			{ 2, 0.0f }, // 顺风顺水
			{ 3, 0.2f }, // 左右逢源
			{ 4, 0.4f }, // 心想事成
			{ 5, 0.6f }, // 福星高照
			{ 6, 0.8f }, // 洪福齐天
			{ 7, 1.0f }  // 气运之子
		};

		public override void OnModSettingUpdate()
		{
			UpdateConfig();
		}

		public void UpdateConfig()
		{
			// 使用配置管理器加载所有配置项
			ConfigManager.LoadAllConfigs(ModIdStr);
		}

		public override void Initialize()
		{
			UpdateConfig();
			harmony = new Harmony("QuantumMaster");

			if (ConfigManager.LuckyLevel == 2)
			{
				DebugLog.Info($"选择了顺风顺水气运，MOD将不会生效");
				return;
			}

			// 不再使用 harmony.PatchAll() 直接应用所有补丁
			// 而是根据配置选择性应用
			ApplyClassPatches();
			ApplyPatchBuilderPatches();

			// 重置已处理方法列表
			GenericTranspiler.ResetProcessedMethods();
		}

		// Class Patch 补丁配置映射表 - 定义哪些配置项对应哪些补丁类
		private readonly Dictionary<string, (System.Type patchType, System.Func<bool> condition)> patchConfigMappings = new Dictionary<string, (System.Type, System.Func<bool>)>
		{
			// Actions 模块 - 静态上下文支持的Prefix/Postfix补丁
			{ "steal", (typeof(Features.Actions.StealPatch), () => ConfigManager.IsFeatureEnabled("steal")) },
			{ "rob", (typeof(Features.Actions.RobPatch), () => ConfigManager.IsFeatureEnabled("rob")) },
			{ "scam", (typeof(Features.Actions.ScamPatch), () => ConfigManager.IsFeatureEnabled("scam")) },
			{ "poison", (typeof(Features.Actions.PoisonPatch), () => ConfigManager.IsFeatureEnabled("poison")) },
			{ "plotHarm", (typeof(Features.Actions.PlotHarmPatch), () => ConfigManager.IsFeatureEnabled("plotHarm")) },
			{ "stealLifeSkill", (typeof(Features.Actions.StealLifeSkillPatch), () => ConfigManager.IsFeatureEnabled("stealLifeSkill")) },
			{ "stealCombatSkill", (typeof(Features.Actions.StealCombatSkillPatch), () => ConfigManager.IsFeatureEnabled("stealCombatSkill")) },
			
			// Character 模块
			{ "GenderControlPatch", (typeof(Features.Character.GenderControlPatch), () => ConfigManager.genderControl != 50) },
			{ "SectApprovalPatch", (typeof(Features.Character.SectApprovalPatch), () => ConfigManager.SetSectMemberApproveTaiwu) },
			{ "SexualOrientationControlPatch", (typeof(Features.Character.SexualOrientationControlPatch), () => ConfigManager.sexualOrientationControl > 0) },
			// Core 模块
			{ "GetQiArtStrategyDeltaNeiliBonusPatch", (typeof(Features.Core.GetQiArtStrategyDeltaNeiliBonusPatch), () => ConfigManager.GetQiArtStrategyDeltaNeiliBonus) },
			{ "GetQiArtStrategyExtraNeiliAllocationBonusPatch", (typeof(Features.Core.GetQiArtStrategyExtraNeiliAllocationBonusPatch), () => ConfigManager.GetQiArtStrategyDeltaNeiliBonus) },
			{ "QiArtStrategyControlPatch", (typeof(Features.Core.QiArtStrategyControlPatch), () => ConfigManager.QiArtStrategiesSelect1 > 0 || ConfigManager.QiArtStrategiesSelect2 > 0 || ConfigManager.QiArtStrategiesSelect3 > 0 || ConfigManager.QiArtStrategiesSelect4 > 0 || ConfigManager.QiArtStrategiesSelect5 > 0 || ConfigManager.QiArtStrategiesSelect6 > 0) },

			// Items 模块 - 蛐蛐相关补丁
			{ "CatchCricketSuccessRatePatch", (typeof(Features.Items.CatchCricketSuccessRatePatch), () => ConfigManager.CatchCricket) },
			{ "CheckCricketIsSmartPatch", (typeof(Features.Items.CheckCricketIsSmartPatch), () => ConfigManager.CheckCricketIsSmart) },
			{ "CricketInitializePatch", (typeof(Features.Items.CricketInitializePatch), () => ConfigManager.CricketInitialize) },
			{ "BookGenerationPatch", (typeof(Features.Items.BookGenerationPatch), () => ConfigManager.GeneratePageIncompleteState) },
			
			// Building 模块 - 建筑相关补丁
			{ "BuildingManageHarvestSpecialSuccessRatePatch", (typeof(Features.Building.BuildingManageHarvestSpecialSuccessRatePatch), () => ConfigManager.BuildingManageHarvestSpecialSuccessRate) },
			
			// Reading 模块
			{ "ReadingInspirationPatch", (typeof(Features.Reading.ReadingInspirationPatch), () => ConfigManager.GetCurrReadingEventBonusRate) },
			{ "ReadingStrategyPatch", (typeof(Features.Reading.ReadingStrategyPatch), () => 
				ConfigManager.BookStrategiesSelect1 > 0 || ConfigManager.BookStrategiesSelect2 > 0 || 
				ConfigManager.BookStrategiesSelect3 > 0 || ConfigManager.BookStrategiesSelect4 > 0 || 
				ConfigManager.BookStrategiesSelect5 > 0 || ConfigManager.BookStrategiesSelect6 > 0 || 
				ConfigManager.BookStrategiesSelect7 > 0 || ConfigManager.BookStrategiesSelect8 > 0 || 
				ConfigManager.BookStrategiesSelect9 > 0) },
			
			// World 模块
			{ "MapResourceInitPatch", (typeof(Features.World.MapResourceInitPatch), () => ConfigManager.InitResources) }
		};

		// PatchBuilder 补丁配置映射表 - 定义哪些配置项对应哪些方法调用
		private readonly Dictionary<string, (System.Func<Harmony, bool> patchMethod, System.Func<bool> condition)> patchBuilderMappings = new Dictionary<string, (System.Func<Harmony, bool>, System.Func<bool>)>
		{
			// Actions 模块 - 使用静态上下文版本测试所有5次替换和双角色判断
			{ "steal", (Features.Actions.StealPatch.PatchGetStealActionPhase, () => ConfigManager.IsFeatureEnabled("steal")) },
			{ "rob", (Features.Actions.ScamPatch.PatchGetScamActionPhase, () => ConfigManager.IsFeatureEnabled("scam")) },
			{ "scam", (Features.Actions.RobPatch.PatchGetRobActionPhase, () => ConfigManager.IsFeatureEnabled("rob")) },
			{ "poison", (Features.Actions.StealLifeSkillPatch.PatchGetStealLifeSkillActionPhase, () => ConfigManager.IsFeatureEnabled("stealLifeSkill")) },
			{ "plotHarm", (Features.Actions.StealCombatSkillPatch.PatchGetStealCombatSkillActionPhase, () => ConfigManager.IsFeatureEnabled("stealCombatSkill")) },
			{ "stealLifeSkill", (Features.Actions.PoisonPatch.PatchGetPoisonActionPhase, () => ConfigManager.IsFeatureEnabled("poison")) },
			{ "stealCombatSkill", (Features.Actions.PlotHarmPatch.PatchGetPlotHarmActionPhase, () => ConfigManager.IsFeatureEnabled("plotHarm")) },
			
			// Character 模块
			{ "OfflineCalcGeneralActionTeachSkill", (Features.Character.OfflineCalcGeneralActionTeachSkillPatch.Apply, () => ConfigManager.IsFeatureEnabled("OfflineCalcGeneralAction_TeachSkill")) },
			
			// Core 模块
			{ "TryAddLoopingEvent", (Features.Core.LoopingEventPatch.Apply, () => ConfigManager.IsFeatureEnabled("TryAddLoopingEvent")) },
			
			// Building 模块
			{ "CreateBuildingArea", (Features.Building.CreateBuildingAreaPatch.Apply, () => ConfigManager.IsFeatureEnabled("CreateBuildingArea")) },
			{ "OfflineUpdateShopManagement", (Features.Building.OfflineUpdateShopManagementPatch.Apply, () => ConfigManager.IsFeatureEnabled("OfflineUpdateShopManagement")) },
			{ "BuildingRandomCorrection", (Features.Building.BuildingRandomCorrectionPatch.Apply, () => ConfigManager.IsFeatureEnabled("BuildingRandomCorrection")) },
			{ "UpdateShopBuildingTeach", (Features.Building.UpdateShopBuildingTeachPatch.Apply, () => ConfigManager.IsFeatureEnabled("UpdateShopBuildingTeach")) },
			
			// Skills 模块
			{ "CalcNeigongLoopingEffect", (Features.Skills.SkillsPatch.Apply, () => ConfigManager.IsFeatureEnabled("CalcNeigongLoopingEffect")) },
			
			// Resources 模块
			{ "CollectResource", (Features.Resources.CollectResourcePatch.Apply, () => ConfigManager.IsFeatureEnabled("collectResource")) },
			{ "UpgradeCollectMaterial", (Features.Resources.UpgradeCollectMaterialPatch.Apply, () => ConfigManager.IsFeatureEnabled("collectResource")) },
			{ "GetCollectResourceAmount", (Features.Resources.GetCollectResourceAmountPatch.Apply, () => ConfigManager.IsFeatureEnabled("GetCollectResourceAmount")) },
			{ "ParallelUpdateOnMonthChange", (Features.Resources.ParallelUpdateOnMonthChangePatch.Apply, () => ConfigManager.IsFeatureEnabled("ParallelUpdateOnMonthChange")) },
			{ "ChoosyGetMaterial", (Features.Resources.ChoosyGetMaterialPatch.Apply, () => ConfigManager.IsFeatureEnabled("ChoosyGetMaterial")) },
			{ "AddChoosyRemainUpgradeData", (Features.Resources.AddChoosyRemainUpgradeDataPatch.Apply, () => ConfigManager.IsFeatureEnabled("AddChoosyRemainUpgradeData")) },
			
			// Combat 模块
			{ "ApplyLifeSkillCombatResult", (Features.Combat.ApplyLifeSkillCombatResultPatch.Apply, () => ConfigManager.IsFeatureEnabled("ApplyLifeSkillCombatResult")) },
			{ "CalcReadInCombat", (Features.Combat.CalcReadInCombatPatch.Apply, () => ConfigManager.IsFeatureEnabled("CalcReadInCombat")) },
			{ "CalcLootItem", (Features.Combat.CalcLootItemPatch.Apply, () => ConfigManager.IsFeatureEnabled("CalcLootItem")) },
			{ "CalcQiQrtInCombat", (Features.Combat.CalcQiQrtInCombatPatch.Apply, () => ConfigManager.IsFeatureEnabled("CalcQiQrtInCombat")) },
			{ "CheckRopeOrSwordHit", (Features.Combat.RopeAndSwordPatch.PatchCheckRopeOrSwordHit, () => ConfigManager.IsFeatureEnabled("ropeOrSword")) },
			{ "CheckRopeOrSwordHitOutofCombat", (Features.Combat.RopeAndSwordPatch.PatchCheckRopeOrSwordHitOutofCombat, () => ConfigManager.IsFeatureEnabled("ropeOrSword")) },
			
			// Adventure 模块
			{ "InitPathContent", (Features.Adventure.AdventurePatch.PatchInitPathContent, () => ConfigManager.InitPathContent) },
			
			// Reading 模块
			{ "ReadingInspirationPatch", (Features.Reading.ReadingInspirationPatch.PatchUpdateReadingProgressOnce, () => ConfigManager.GetCurrReadingEventBonusRate) },
			
			// 下面这两项都是书籍进度进度增加策略
			{ "GetStrategyProgressAddValue", (Features.Reading.ReadingBuilderPatch.PatchGetStrategyProgressAddValue, () => ConfigManager.GetStrategyProgressAddValue) },
			{ "ApplyImmediateReadingStrategyEffectForLifeSkill", (Features.Reading.ReadingBuilderPatch.PatchApplyImmediateReadingStrategyEffectForLifeSkill, () => ConfigManager.GetStrategyProgressAddValue) },
			{ "SetReadingStrategy", (Features.Reading.ReadingBuilderPatch.PatchSetReadingStrategy, () => ConfigManager.SetReadingStrategy) },
			
			// Items 模块
			{ "CatchCricketDouble", (Features.Items.CricketPatch.PatchCatchCricketDouble, () => ConfigManager.CatchCricketDouble) }
		};

		// 新增方法，根据配置选择性应用 class 形式的补丁
		private void ApplyClassPatches()
		{
			DebugLog.Info("开始应用类补丁...");
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
						DebugLog.Info($"应用补丁类: {patchName}");
					}
					catch (Exception ex)
					{
						DebugLog.Warning($"应用补丁类 {patchName} 时出错: {ex.Message}");
						skippedClassPatches++;
					}
				}
				else
				{
					DebugLog.Info($"跳过补丁类: {patchName} (已禁用)");
					skippedClassPatches++;
				}
			}

			DebugLog.Info($"类补丁应用完成: 成功 {appliedClassPatches} 个, 跳过 {skippedClassPatches} 个");
		}

		// 新增方法，根据配置选择性应用 PatchBuilder 形式的补丁
		private void ApplyPatchBuilderPatches()
		{
			DebugLog.Info("开始应用 PatchBuilder 补丁...");
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
							DebugLog.Info($"应用 PatchBuilder 补丁: {patchName}");
						}
						else
						{
							DebugLog.Info($"跳过 PatchBuilder 补丁: {patchName} (条件不满足)");
							skippedBuilderPatches++;
						}
					}
					catch (Exception ex)
					{
						DebugLog.Warning($"应用 PatchBuilder 补丁 {patchName} 时出错: {ex.Message}");
						skippedBuilderPatches++;
					}
				}
				else
				{
					DebugLog.Info($"跳过 PatchBuilder 补丁: {patchName} (已禁用)");
					skippedBuilderPatches++;
				}
			}

			DebugLog.Info($"PatchBuilder 补丁应用完成: 成功 {appliedBuilderPatches} 个, 跳过 {skippedBuilderPatches} 个");
			
			// 应用所有已注册的 GenericTranspiler 补丁
			DebugLog.Info("开始应用 GenericTranspiler 补丁...");
			GenericTranspiler.ApplyPatches(harmony);
			DebugLog.Info("GenericTranspiler 补丁应用完成");
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