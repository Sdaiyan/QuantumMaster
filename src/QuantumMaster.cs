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
		public static bool debug = true;
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
			{ "StealPatch", (typeof(Features.Actions.StealPatch), () => ConfigManager.steal) },
			{ "RobPatch", (typeof(Features.Actions.RobPatch), () => ConfigManager.rob) },
			{ "ScamPatch", (typeof(Features.Actions.ScamPatch), () => ConfigManager.scam) },
			{ "PoisonPatch", (typeof(Features.Actions.PoisonPatch), () => ConfigManager.poison) },
			{ "PlotHarmPatch", (typeof(Features.Actions.PlotHarmPatch), () => ConfigManager.plotHarm) },
			{ "StealLifeSkillPatch", (typeof(Features.Actions.StealLifeSkillPatch), () => ConfigManager.stealLifeSkill) },
			{ "StealCombatSkillPatch", (typeof(Features.Actions.StealCombatSkillPatch), () => ConfigManager.stealCombatSkill) },
			
			// Character 模块
			{ "GenderControlPatch", (typeof(Features.Character.GenderControlPatch), () => ConfigManager.genderControl != 50) },
			{ "SectApprovalPatch", (typeof(Features.Character.SectApprovalPatch), () => ConfigManager.SetSectMemberApproveTaiwu) },
			{ "SexualOrientationControlPatch", (typeof(Features.Character.SexualOrientationControlPatch), () => ConfigManager.sexualOrientationControl > 0) },
			
			// Core 模块
			{ "LoopingEventPatch", (typeof(Features.Core.LoopingEventPatch), () => ConfigManager.TryAddLoopingEvent) },
			{ "QiArtStrategyPatch", (typeof(Features.Core.QiArtStrategyPatch), () => ConfigManager.GetQiArtStrategyDeltaNeiliBonus) },
			
			// Items 模块
			{ "CricketPatch", (typeof(Features.Items.CricketPatch), () => ConfigManager.CatchCricket || ConfigManager.CheckCricketIsSmart || ConfigManager.CricketInitialize) },
			{ "BookGenerationPatch", (typeof(Features.Items.BookGenerationPatch), () => ConfigManager.GeneratePageIncompleteState) },
			
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
			{ "GetStealActionPhase", (Features.Actions.StealPatch.PatchGetStealActionPhase, () => ConfigManager.steal) },
			{ "GetScamActionPhase", (Features.Actions.ScamPatch.PatchGetScamActionPhase, () => ConfigManager.scam) },
			{ "GetRobActionPhase", (Features.Actions.RobPatch.PatchGetRobActionPhase, () => ConfigManager.rob) },
			{ "GetStealLifeSkillActionPhase", (Features.Actions.StealLifeSkillPatch.PatchGetStealLifeSkillActionPhase, () => ConfigManager.stealLifeSkill) },
			{ "GetStealCombatSkillActionPhase", (Features.Actions.StealCombatSkillPatch.PatchGetStealCombatSkillActionPhase, () => ConfigManager.stealCombatSkill) },
			{ "GetPoisonActionPhase", (Features.Actions.PoisonPatch.PatchGetPoisonActionPhase, () => ConfigManager.poison) },
			{ "GetPlotHarmActionPhase", (Features.Actions.PlotHarmPatch.PatchGetPlotHarmActionPhase, () => ConfigManager.plotHarm) },
			
			// Character 模块
			{ "OfflineCalcGeneralActionTeachSkill", (Features.Character.OfflineCalcGeneralActionTeachSkillPatch.PatchOfflineCalcGeneralActionTeachSkill, () => ConfigManager.OfflineCalcGeneralAction_TeachSkill) },
			
			// Building 模块
			{ "CreateBuildingArea", (Features.Building.BuildingPatch.PatchCreateBuildingArea, () => ConfigManager.CreateBuildingArea) },
			{ "OfflineUpdateShopManagement", (Features.Building.BuildingPatch.PatchOfflineUpdateShopManagement, () => ConfigManager.OfflineUpdateShopManagement) },
			
			// Skills 模块
			{ "CalcNeigongLoopingEffect", (Features.Skills.SkillsPatch.PatchCalcNeigongLoopingEffect, () => ConfigManager.CalcNeigongLoopingEffect) },
			
			// Resources 模块
			{ "CollectResource", (Features.Resources.ResourcesPatch.PatchCollectResource, () => ConfigManager.collectResource) },
			{ "UpgradeCollectMaterial", (Features.Resources.ResourcesPatch.PatchUpgradeCollectMaterial, () => ConfigManager.collectResource) },
			{ "GetCollectResourceAmount", (Features.Resources.ResourcesPatch.PatchGetCollectResourceAmount, () => ConfigManager.GetCollectResourceAmount) },
			{ "ParallelUpdateOnMonthChange", (Features.Resources.ResourcesPatch.PatchParallelUpdateOnMonthChange, () => ConfigManager.ParallelUpdateOnMonthChange) },
			{ "ChoosyGetMaterial", (Features.Resources.ResourcesPatch.PatchChoosyGetMaterial, () => ConfigManager.ChoosyGetMaterial) },
			{ "AddChoosyRemainUpgradeData", (Features.Resources.ResourcesPatch.PatchAddChoosyRemainUpgradeData, () => ConfigManager.AddChoosyRemainUpgradeData) },
			
			// Combat 模块
			{ "ApplyLifeSkillCombatResult", (Features.Combat.CombatPatch.PatchApplyLifeSkillCombatResult, () => ConfigManager.ApplyLifeSkillCombatResult) },
			{ "CalcReadInCombat", (Features.Combat.CombatPatch.PatchCalcReadInCombat, () => ConfigManager.CalcReadInCombat) },
			{ "CalcLootItem", (Features.Combat.CombatPatch.PatchCalcLootItem, () => ConfigManager.CalcLootItem) },
			{ "CalcQiQrtInCombat", (Features.Combat.CombatPatch.PatchCalcQiQrtInCombat, () => ConfigManager.CalcQiQrtInCombat) },
			{ "CheckRopeOrSwordHit", (Features.Combat.RopeAndSwordPatch.PatchCheckRopeOrSwordHit, () => ConfigManager.ropeOrSword) },
			{ "CheckRopeOrSwordHitOutofCombat", (Features.Combat.RopeAndSwordPatch.PatchCheckRopeOrSwordHitOutofCombat, () => ConfigManager.ropeOrSword) },
			
			// Adventure 模块
			{ "InitPathContent", (Features.Adventure.AdventurePatch.PatchInitPathContent, () => ConfigManager.InitPathContent) },
			
			// Reading 模块
			{ "GetStrategyProgressAddValue", (Features.Reading.ReadingBuilderPatch.PatchGetStrategyProgressAddValue, () => ConfigManager.GetStrategyProgressAddValue) },
			{ "ApplyImmediateReadingStrategyEffectForLifeSkill", (Features.Reading.ReadingBuilderPatch.PatchApplyImmediateReadingStrategyEffectForLifeSkill, () => ConfigManager.ApplyImmediateReadingStrategyEffectForLifeSkill) },
			
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