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

			// 原QuantumMaster.cs中的自动补丁应用逻辑
			int appliedPatches = 0;
			int skippedPatches = 0;

			// 自动调用所有以 patch 开头的方法
			var patchMethods = this.GetType()
					.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Where(method => method.Name.StartsWith("patch", StringComparison.OrdinalIgnoreCase) &&
								 method.GetParameters().Length == 0 &&
								 (method.ReturnType == typeof(bool) || method.ReturnType == typeof(void)))
					.ToList();

			DebugLog.Info($"发现 {patchMethods.Count} 个补丁方法: {string.Join(", ", patchMethods.Select(m => m.Name))}");

			foreach (var method in patchMethods)
			{
				try
				{
					DebugLog.Info($"尝试应用补丁: {method.Name}");
					object result = method.Invoke(this, null);

					// 检查返回值
					if (method.ReturnType == typeof(bool))
					{
						bool success = (bool)result;
						if (success)
						{
							DebugLog.Info($"补丁 {method.Name} 应用成功");
							appliedPatches++;
						}
						else
						{
							DebugLog.Info($"补丁 {method.Name} 未应用（跳过）");
							skippedPatches++;
						}
					}
					else
					{
						// void 返回类型视为成功
						DebugLog.Info($"补丁 {method.Name} 应用成功");
						appliedPatches++;
					}
				}
				catch (Exception ex)
				{
					DebugLog.Warning($"应用补丁 {method.Name} 时出错: {ex.Message}");
					if (debug && ex.InnerException != null)
					{
						DebugLog.Warning($"详细错误: {ex.InnerException.Message}");
						DebugLog.Warning($"堆栈跟踪: {ex.InnerException.StackTrace}");
					}
					skippedPatches++;
				}
			}

			patchBuilder?.ApplyPatches(harmony);

			DebugLog.Info($"补丁应用完成: 成功 {appliedPatches} 个, 跳过 {skippedPatches} 个");

			// 重置已处理方法列表
			GenericTranspiler.ResetProcessedMethods();
		}

		// 补丁配置映射表 - 定义哪些配置项对应哪些补丁类
		private readonly Dictionary<string, (System.Type patchType, System.Func<bool> condition)> patchConfigMappings = 
			new Dictionary<string, (System.Type, System.Func<bool>)>
		{
			// Actions 模块
			{ "StealPatch", (typeof(Features.Actions.StealPatch), () => ConfigManager.steal) },
			{ "ScamPatch", (typeof(Features.Actions.ScamPatch), () => ConfigManager.scam) },
			{ "RobPatch", (typeof(Features.Actions.RobPatch), () => ConfigManager.rob) },
			{ "StealLifeSkillPatch", (typeof(Features.Actions.StealLifeSkillPatch), () => ConfigManager.stealLifeSkill) },
			{ "StealCombatSkillPatch", (typeof(Features.Actions.StealCombatSkillPatch), () => ConfigManager.stealCombatSkill) },
			{ "PoisonPatch", (typeof(Features.Actions.PoisonPatch), () => ConfigManager.poison) },
			{ "PlotHarmPatch", (typeof(Features.Actions.PlotHarmPatch), () => ConfigManager.plotHarm) },
			
			// Character 模块
			{ "GenderControlPatch", (typeof(Features.Character.GenderControlPatch), () => ConfigManager.genderControl > 0) },
			{ "NPCTeachingPatch", (typeof(Features.Character.NPCTeachingPatch), () => ConfigManager.GetAskToTeachSkillRespondChance || ConfigManager.GetTaughtNewSkillSuccessRate) },
			{ "SectApprovalPatch", (typeof(Features.Character.SectApprovalPatch), () => ConfigManager.SetSectMemberApproveTaiwu) },
			{ "SexualOrientationControlPatch", (typeof(Features.Character.SexualOrientationControlPatch), () => ConfigManager.sexualOrientationControl > 0) },
			
			// Combat 模块
			{ "RopeAndSwordPatch", (typeof(Features.Combat.RopeAndSwordPatch), () => ConfigManager.ropeOrSword) },
			
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

		public override void Dispose()
		{
			if (harmony != null)
			{
				harmony.UnpatchSelf();
			}
		}

		// 原QuantumMaster.cs中的方法保留
		public bool patchCreateBuildingArea()
		{
			if (!ConfigManager.CreateBuildingArea && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Building.BuildingDomain),
				MethodName = "CreateBuildingArea",
				Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(short), typeof(short), typeof(short) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"CreateBuildingArea",
					OriginalMethod);

			// 2参数 NEXT 方法替换
			// 1 sbyte level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					1);

			// 2 AddElement_BuildingBlocks(buildingBlockKey, new BuildingBlockData(num3, buildingBlockItem.TemplateId, (sbyte)((buildingBlockItem.MaxLevel <= 1) ? 1 : random.Next(1, buildingBlockItem.MaxLevel)), -1), context);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					2);

			// 3 AddBuilding(context, mapAreaId, mapBlockId, num3, buildingBlockItem.TemplateId, (sbyte)((buildingBlockItem.MaxLevel <= 1) ? 1 : random.Next(1, buildingBlockItem.MaxLevel)), buildingAreaWidth);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					3);

			// 4 num5 = list2[random.Next(0, list2.Count)];

			// 5 level = (sbyte)Math.Clamp(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1, buildingBlockItem2.MaxLevel);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					5);

			// 6 level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					6);

			// 7 int isBuild = random.Next(0, 2);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					7);

			// 8 blockIndex = canUseBlockList[random.Next(0, canUseBlockList.Count)];

			// 9 level = (sbyte)Math.Max(random.Next(centerBuildingMaxLevel / 2, centerBuildingMaxLevel + 1), 1);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					9);

			// 10 short blockIndex4 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

			// 11 short blockIndex5 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

			// 12 short blockIndex6 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

			// 1 参数 NEXT 方法替换
			// 1 short merchantBuildingId = (short)(274 + context.Random.Next(7));

			// CheckPercentProb 方法替换
			// 1 if (num7 >= 5 || random.CheckPercentProb(num7 * 20))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			// 2 if (num9 >= 5 || random.CheckPercentProb(num9 * 20))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					2);

			// 1 sbyte level2 = (sbyte)formula.Calculate();
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CalculateFormula0Arg,
					PatchPresets.Replacements.RandomCalculateMax,
					1);

			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CalculateFormula0Arg,
					PatchPresets.Replacements.RandomCalculateMax,
					2);
					
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CalculateFormula0Arg,
					PatchPresets.Replacements.RandomCalculateMax,
					3);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchCalcNeigongLoopingEffect()
		{
			if (!ConfigManager.CalcNeigongLoopingEffect && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.CombatSkill.CombatSkillDomain),
				MethodName = "CalcNeigongLoopingEffect",
				// IRandomSource random, GameData.Domains.Character.Character character, CombatSkillItem skillCfg, bool includeReference = true
				Parameters = new Type[] { typeof(IRandomSource), typeof(GameData.Domains.Character.Character), typeof(Config.CombatSkillItem), typeof(bool) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"CalcNeigongLoopingEffect",
					OriginalMethod);

			// 1 neili = random.Next(neiliMin, neiliMax + 1);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					1);

			// 1 qiDisorder = (short)random.Next(qiDisorder + 1);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next1Arg,
					PatchPresets.Replacements.Next1Arg0,
					1);

			// 2 qiDisorder = (short)(-random.Next(-qiDisorder + 1));
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next1Arg,
					PatchPresets.Replacements.Next1ArgMax,
					2);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchCollectResource()
		{
			if (!ConfigManager.collectResource && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Map.MapDomain),
				MethodName = "ApplyCollectResourceResult",
				// DataContext ctx, GameData.Domains.Character.Character character, MapBlockData blockData, short currentResource, short maxResource, bool costResource, ref CollectResourceResult result
				Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(GameData.Domains.Character.Character), typeof(GameData.Domains.Map.MapBlockData), typeof(short), typeof(short), typeof(bool), typeof(GameData.Domains.Map.CollectResourceResult).MakeByRefType() }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"ApplyCollectResourceResult",
					OriginalMethod);

			// 1 else if (itemTemplateId >= 0 && random.CheckPercentProb(blockData.GetCollectItemChance(resourceType)))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			// // 2 if (random.CheckPercentProb(gradeUpOdds))
			// patchBuilder.AddExtensionMethodReplacement(
			// 		PatchPresets.Extensions.CheckPercentProb,
			// 		PatchPresets.Replacements.CheckPercentProbTrue,
			// 		2);

			// // 3 if (random.CheckPercentProb(odds))
			// patchBuilder.AddExtensionMethodReplacement(
			// 		PatchPresets.Extensions.CheckPercentProb,
			// 		PatchPresets.Replacements.CheckPercentProbTrue,
			// 		3);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchUpgradeCollectMaterial()
		{
			if (!ConfigManager.collectResource && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Map.MapDomain),
				MethodName = "UpgradeCollectMaterial",
				// public void UpgradeCollectMaterial(IRandomSource random, ResourceCollectionItem collectionConfig, sbyte resourceType, short maxResource, short currentResource, int neighborOddsMultiplier, ref short itemTemplateId)
				Parameters = new Type[] { typeof(IRandomSource), typeof(ResourceCollectionItem), typeof(sbyte), typeof(short), typeof(short), typeof(int), typeof(short).MakeByRefType() }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"UpgradeCollectMaterial",
					OriginalMethod);


			// if (!random.CheckPercentProb(gradeUpOdds))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			// 2 if (random.CheckPercentProb(odds))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					2);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchGetCollectResourceAmount()
		{
			if (!ConfigManager.GetCollectResourceAmount && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Map.MapDomain),
				MethodName = "GetCollectResourceAmount",
				// IRandomSource random, MapBlockData blockData, sbyte resourceType
				Parameters = new Type[] { typeof(IRandomSource), typeof(GameData.Domains.Map.MapBlockData), typeof(sbyte) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"GetCollectResourceAmount",
					OriginalMethod);

			// 1 return currentResource * (((currentResource >= 100) ? 60 : 40) + random.Next(-20, 21)) / 100 * resourceMultiplier;
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchOfflineUpdateShopManagement()
		{
			if (!ConfigManager.OfflineUpdateShopManagement && !openAll) return false;

			// private void OfflineUpdateShopManagement(ParallelBuildingModification modification, short settlementId, BuildingBlockItem buildingBlockCfg, BuildingBlockKey blockKey, BuildingBlockData blockData, DataContext context)
			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Building.BuildingDomain),
				MethodName = "OfflineUpdateShopManagement",
				// ParallelBuildingModification modification, short settlementId, BuildingBlockItem buildingBlockCfg, BuildingBlockKey blockKey, BuildingBlockData blockData, DataContext context
				Parameters = new Type[] { typeof(GameData.Domains.Building.ParallelBuildingModification), typeof(short), typeof(Config.BuildingBlockItem), typeof(GameData.Domains.Building.BuildingBlockKey), typeof(GameData.Domains.Building.BuildingBlockData), typeof(GameData.Common.DataContext) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"OfflineUpdateShopManagement",
					OriginalMethod);

			// CheckProb
			// 1 if (data3.ShopSoldItemList[k].TemplateId != -1 && random.CheckProb(50, 100))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckProb,
					PatchPresets.Replacements.CheckProbTrue,
					1);

			// Next 2 args
			// 1 itemTemplateId = itemProbList[random.Next(0, itemProbList.Count)]; 从任意成功的结果中 随机一个
			// 2 sbyte gradeLevel = itemProbList2[random.Next(0, itemProbList2.Count)]; 随机的品级，应该是越后面越大
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					2);

			// CheckPercentProb 我也不知道为什么 IL 最后只有 3个 CheckPercentProb，全部替换 
			// 1 if (hasManager && random.CheckPercentProb(prob))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			// 2 if (random.CheckPercentProb(prob2))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					2);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchApplyLifeSkillCombatResult()
		{
			if (!ConfigManager.ApplyLifeSkillCombatResult && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
				MethodName = "DebateGameOver",
				// DataContext context, bool isTaiwuWin, bool isSurrender
				Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(bool), typeof(bool) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"DebateGameOver",
					OriginalMethod);

			// CheckPercentProb
			// 1 if (readInLifeSkillCombatCount > 0 && currBook.IsValid() && GetTotalReadingProgress(currBook.Id) < 100 && bookCfg.LifeSkillTemplateId >= 0 && context.Random.CheckPercentProb(chanceReading))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			// 2 if (loopInLifeSkillCombatCount > 0 && loopingNeigongTemplateId >= 0 && DomainManager.CombatSkill.TryGetElement_CombatSkills(skillKey, out var skill) && skill.GetObtainedNeili() >= skill.GetTotalObtainableNeili() && context.Random.CheckPercentProb(chanceLooping))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					2);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchCalcReadInCombat()
		{
			if (!ConfigManager.CalcReadInCombat && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Combat.CombatDomain),
				MethodName = "CalcReadInCombat",
				// DataContext context
				Parameters = new Type[] { typeof(GameData.Common.DataContext) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"CalcReadInCombat",
					OriginalMethod);

			// CheckPercentProb
			// 1 if (context.Random.CheckPercentProb(odds))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchCalcLootItem()
		{
			if (!ConfigManager.CalcLootItem && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Combat.CombatDomain),
				MethodName = "CalcLootItem",
				// DataContext context
				Parameters = new Type[] { typeof(GameData.Common.DataContext) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"CalcLootItem",
					OriginalMethod);

			// CheckPercentProb
			// 1 if (context.Random.CheckPercentProb(dropRate))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			// 2 if (context.Random.CheckPercentProb(dropRate2))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					2);

			// 3 if (context.Random.CheckPercentProb(100 - 20 * j))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					3);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchInitPathContent()
		{
			if (!ConfigManager.InitPathContent && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Adventure.AdventureDomain),
				MethodName = "InitPathContent",
				// DataContext context
				Parameters = new Type[] { typeof(GameData.Common.DataContext) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"InitPathContent",
					OriginalMethod);

			// 1 resAmount = (short)(context.Random.Next(minAmount, maxAmount) * DomainManager.World.GetGainResourcePercent(4) / 100);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchGetStrategyProgressAddValue()
		{
			if (!ConfigManager.GetStrategyProgressAddValue && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
				MethodName = "GetStrategyProgressAddValue",
				// IRandomSource random, ReadingStrategyItem strategyCfg
				Parameters = new Type[] { typeof(IRandomSource), typeof(Config.ReadingStrategyItem) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"GetStrategyProgressAddValue",
					OriginalMethod);

			// 1 return (sbyte)((strategyCfg.MaxProgressAddValue > strategyCfg.MinProgressAddValue) ? random.Next(strategyCfg.MinProgressAddValue, strategyCfg.MaxProgressAddValue + 1) : strategyCfg.MaxProgressAddValue);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchApplyImmediateReadingStrategyEffectForLifeSkill()
		{
			if (!ConfigManager.ApplyImmediateReadingStrategyEffectForLifeSkill && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
				MethodName = "ApplyImmediateReadingStrategyEffectForLifeSkill",
				// DataContext context, GameData.Domains.Item.SkillBook book, byte pageIndex, ref ReadingBookStrategies strategies, sbyte strategyId
				Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(GameData.Domains.Item.SkillBook), typeof(byte), typeof(GameData.Domains.Taiwu.ReadingBookStrategies).MakeByRefType(), typeof(sbyte) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"ApplyImmediateReadingStrategyEffectForLifeSkill",
					OriginalMethod);

			// 1 sbyte currPageAddValue = (sbyte)((strategyCfg.MaxProgressAddValue > strategyCfg.MinProgressAddValue) ? context.Random.Next(strategyCfg.MinProgressAddValue, strategyCfg.MaxProgressAddValue + 1) : strategyCfg.MaxProgressAddValue);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next2Args,
					PatchPresets.Replacements.Next2ArgsMax,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchChoosyGetMaterial()
		{
			if (!ConfigManager.ChoosyGetMaterial && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
				MethodName = "ChoosyGetMaterial",
				// DataContext context, sbyte resourceType, int count
				Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(sbyte), typeof(int) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"ChoosyGetMaterial",
					OriginalMethod);

			// 1 int random = context.Random.Next(10000);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next1Arg,
					PatchPresets.Replacements.Next1ArgMax,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchParallelUpdateOnMonthChange()
		{
			if (!ConfigManager.ParallelUpdateOnMonthChange && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Map.MapDomain),
				MethodName = "ParallelUpdateOnMonthChange",
				// DataContext context, int areaIdInt
				Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(int) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"ParallelUpdateOnMonthChange",
					OriginalMethod);

			// 1 int addValue = context.Random.Next(maxAddValue + 1);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next1Arg,
					PatchPresets.Replacements.Next1ArgMax,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchCalcQiQrtInCombat()
		{
			if (!ConfigManager.CalcQiQrtInCombat && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Combat.CombatDomain),
				MethodName = "CalcQiQrtInCombat",
				// DataContext context
				Parameters = new Type[] { typeof(GameData.Common.DataContext) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"CalcQiQrtInCombat",
					OriginalMethod);

			// CheckPercentProb
			// 1 if (context.Random.CheckPercentProb(odds))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchAddChoosyRemainUpgradeData()
		{
			if (!ConfigManager.AddChoosyRemainUpgradeData && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
				MethodName = "AddChoosyRemainUpgradeData",
				// DataContext context
				Parameters = new Type[] { typeof(GameData.Common.DataContext) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"AddChoosyRemainUpgradeData",
					OriginalMethod);

			// 1 int randomAddCount = ((maxAddCount > 0) ? context.Random.Next(maxAddCount) : 0);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next1Arg,
					PatchPresets.Replacements.Next1ArgMax,
					1);

			// 2 int randomAddRate = ((maxAddRate > 0) ? context.Random.Next(maxAddRate) : 0);
			patchBuilder.AddInstanceMethodReplacement(
					PatchPresets.InstanceMethods.Next1Arg,
					PatchPresets.Replacements.Next1ArgMax,
					2);

			patchBuilder.Apply(harmony);

			return true;
		}












	}
}