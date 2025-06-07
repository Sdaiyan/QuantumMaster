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

		// 原Class1.cs中的各种功能开关
		public static bool steal; // 偷窃必定成功
		public static bool scam; // 唬骗必定成功
		public static bool rob; // 抢劫必定成功
		public static bool stealLifeSkill; // 偷学生活技能必定成功
		public static bool stealCombatSkill; // 偷学战斗技能必定成功
		public static bool poison; // 下毒必定成功
		public static bool plotHarm; // 暗害必定成功
		public static bool gender0; // 生成性别
		public static bool gender1; // 生成性别
		public static bool ropeOrSword; // 如果概率不为0，绳子绑架或者煎饼救人必定成功
		public static bool ApplyImmediateReadingStrategyEffectForCombatSkill; // 功法书籍的效率增加策略（奇思妙想）进度增加为浮动区间的上限值
		public static bool GetAskToTeachSkillRespondChance; // 如果概率不是0，则必定会指点别人
		public static bool GetTaughtNewSkillSuccessRate; // 如果概率不为0，接受指点的人必定能学习成功
		public static bool CatchCricket; // 抓蛐蛐必定成功
		public static bool InitResources; // 生成世界时，每个地块上的资源为浮动区间的最大值，受到难度的影响
		public static bool CheckCricketIsSmart; // 蛐蛐是否可以升级，如果符合条件必定升级
		public static bool GetCurrReadingEventBonusRate; // 灵光一闪概率不为0时，必定灵光一闪
		public static bool GeneratePageIncompleteState; // 生成书籍时，完整的书页为浮动区间的最大值，亡佚书页为浮动区间最小值，并且完整的书页会出现在书本的前篇位置
		public static bool FixedPagePos; // 位置固定在靠前
		public static bool CricketInitialize; // 生成蛐蛐时，必定生成耐久上限为理论上限值的，不受伤的蛐蛐
		public static bool TryAddLoopingEvent; // 如果概率不为0，尝试触发天人感应时必定成功
		public static bool BookStrategies; // 指定读书策略
		public static bool SetSectMemberApproveTaiwu; // 送上拜帖时，必定是尽可能高阶的门派成员会认可太吾
		public static int BookStrategiesSelect1; // 第 1 个策略
		public static int BookStrategiesSelect2; // 第 2 个策略
		public static int BookStrategiesSelect3; // 第 3 个策略
		public static int BookStrategiesSelect4; // 第 4 个策略
		public static int BookStrategiesSelect5; // 第 5 个策略
		public static int BookStrategiesSelect6; // 第 6 个策略
		public static int BookStrategiesSelect7; // 第 7 个策略
		public static int BookStrategiesSelect8; // 第 8 个策略
		public static int BookStrategiesSelect9; // 第 9 个策略
		public static bool GetBisexualTrue; // 强制所有人双性恋
		public static bool GetBisexualFalse; // 强制所有人单性恋

		// 原来QuantumMaster.cs中的功能开关
		public static bool CreateBuildingArea; // 生成世界时，产业中的建筑和资源点的初始等级，以及生成数量
		public static bool CalcNeigongLoopingEffect; // 周天运转时，获得的内力为浮动区间的最大值，内息恢复最大，内息紊乱最小
		public static bool GetQiArtStrategyDeltaNeiliBonus; // 周天内力策略收益最大
		public static bool collectResource; // 收获资源时必定获取引子，且是可能获取的最高级的引子
		public static bool GetCollectResourceAmount; // 采集数量必定为浮动区间的上限
		// public static bool UpdateResourceBlock; // 如果概率不为0，过月时，对应的产业资源必定升级与扩张
		public static bool OfflineUpdateShopManagement; // 如果概率不为0，产业建筑经营、招募必然成功、村民技艺必定提升
		public static bool ApplyLifeSkillCombatResult; // 如果概率不为0，较艺读书&周天必定触发
		public static bool CalcReadInCombat; // 如果概率不为0，战斗读书必定触发
		public static bool CalcQiQrtInCombat; // 如果概率不为0，战斗周天运转必定触发
		public static bool CalcLootItem; // 如果概率不为0，战利品掉落判定必定通过（原本逻辑是对每个战利品进行判断是否掉落）
		public static bool InitPathContent; // 奇遇收获资源时，数量为浮动区间的上限
		public static bool GetStrategyProgressAddValue; // 读书策略进度增加为浮动区间的上限
		public static bool ApplyImmediateReadingStrategyEffectForLifeSkill; // 技艺读书策略进度增加为浮动区间的上限
		public static bool ChoosyGetMaterial; // 精挑细选，品质升级判定概率最大
		public static bool AddChoosyRemainUpgradeData; // 精挑细选过月累计最大
		public static bool ParallelUpdateOnMonthChange; // 地块每月资源恢复数量为浮动区间的上限

		public override void OnModSettingUpdate()
		{
			UpdateConfig();
		}

		public void UpdateConfig()
		{

			// 从Class1.cs迁移的设置读取
			DomainManager.Mod.GetSetting(ModIdStr, "steal", ref steal);
			DebugLog.Info($"配置加载: steal = {steal}");

			DomainManager.Mod.GetSetting(ModIdStr, "scam", ref scam);
			DebugLog.Info($"配置加载: scam = {scam}");

			DomainManager.Mod.GetSetting(ModIdStr, "rob", ref rob);
			DebugLog.Info($"配置加载: rob = {rob}");

			DomainManager.Mod.GetSetting(ModIdStr, "stealLifeSkill", ref stealLifeSkill);
			DebugLog.Info($"配置加载: stealLifeSkill = {stealLifeSkill}");

			DomainManager.Mod.GetSetting(ModIdStr, "stealCombatSkill", ref stealCombatSkill);
			DebugLog.Info($"配置加载: stealCombatSkill = {stealCombatSkill}");

			DomainManager.Mod.GetSetting(ModIdStr, "poison", ref poison);
			DebugLog.Info($"配置加载: poison = {poison}");

			DomainManager.Mod.GetSetting(ModIdStr, "plotHarm", ref plotHarm);
			DebugLog.Info($"配置加载: plotHarm = {plotHarm}");

			DomainManager.Mod.GetSetting(ModIdStr, "gender0", ref gender0);
			DebugLog.Info($"配置加载: gender0 = {gender0}");

			DomainManager.Mod.GetSetting(ModIdStr, "gender1", ref gender1);
			DebugLog.Info($"配置加载: gender1 = {gender1}");

			DomainManager.Mod.GetSetting(ModIdStr, "ropeOrSword", ref ropeOrSword);
			DebugLog.Info($"配置加载: ropeOrSword = {ropeOrSword}");

			DomainManager.Mod.GetSetting(ModIdStr, "ApplyImmediateReadingStrategyEffectForCombatSkill", ref ApplyImmediateReadingStrategyEffectForCombatSkill);
			DebugLog.Info($"配置加载: ApplyImmediateReadingStrategyEffectForCombatSkill = {ApplyImmediateReadingStrategyEffectForCombatSkill}");

			DomainManager.Mod.GetSetting(ModIdStr, "GetAskToTeachSkillRespondChance", ref GetAskToTeachSkillRespondChance);
			DebugLog.Info($"配置加载: GetAskToTeachSkillRespondChance = {GetAskToTeachSkillRespondChance}");

			DomainManager.Mod.GetSetting(ModIdStr, "GetTaughtNewSkillSuccessRate", ref GetTaughtNewSkillSuccessRate);
			DebugLog.Info($"配置加载: GetTaughtNewSkillSuccessRate = {GetTaughtNewSkillSuccessRate}");

			DomainManager.Mod.GetSetting(ModIdStr, "CatchCricket", ref CatchCricket);
			DebugLog.Info($"配置加载: CatchCricket = {CatchCricket}");

			DomainManager.Mod.GetSetting(ModIdStr, "InitResources", ref InitResources);
			DebugLog.Info($"配置加载: InitResources = {InitResources}");

			DomainManager.Mod.GetSetting(ModIdStr, "CheckCricketIsSmart", ref CheckCricketIsSmart);
			DebugLog.Info($"配置加载: CheckCricketIsSmart = {CheckCricketIsSmart}");

			DomainManager.Mod.GetSetting(ModIdStr, "GetCurrReadingEventBonusRate", ref GetCurrReadingEventBonusRate);
			DebugLog.Info($"配置加载: GetCurrReadingEventBonusRate = {GetCurrReadingEventBonusRate}");

			DomainManager.Mod.GetSetting(ModIdStr, "GeneratePageIncompleteState", ref GeneratePageIncompleteState);
			DebugLog.Info($"配置加载: GeneratePageIncompleteState = {GeneratePageIncompleteState}");

			DomainManager.Mod.GetSetting(ModIdStr, "FixedPagePos", ref FixedPagePos);
			DebugLog.Info($"配置加载: FixedPagePos = {FixedPagePos}");

			DomainManager.Mod.GetSetting(ModIdStr, "CricketInitialize", ref CricketInitialize);
			DebugLog.Info($"配置加载: CricketInitialize = {CricketInitialize}");

			DomainManager.Mod.GetSetting(ModIdStr, "TryAddLoopingEvent", ref TryAddLoopingEvent);
			DebugLog.Info($"配置加载: TryAddLoopingEvent = {TryAddLoopingEvent}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategies", ref BookStrategies);
			DebugLog.Info($"配置加载: BookStrategies = {BookStrategies}");

			DomainManager.Mod.GetSetting(ModIdStr, "SetSectMemberApproveTaiwu", ref SetSectMemberApproveTaiwu);
			DebugLog.Info($"配置加载: SetSectMemberApproveTaiwu = {SetSectMemberApproveTaiwu}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect1", ref BookStrategiesSelect1);
			DebugLog.Info($"配置加载: BookStrategiesSelect1 = {BookStrategiesSelect1}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect2", ref BookStrategiesSelect2);
			DebugLog.Info($"配置加载: BookStrategiesSelect2 = {BookStrategiesSelect2}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect3", ref BookStrategiesSelect3);
			DebugLog.Info($"配置加载: BookStrategiesSelect3 = {BookStrategiesSelect3}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect4", ref BookStrategiesSelect4);
			DebugLog.Info($"配置加载: BookStrategiesSelect4 = {BookStrategiesSelect4}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect5", ref BookStrategiesSelect5);
			DebugLog.Info($"配置加载: BookStrategiesSelect5 = {BookStrategiesSelect5}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect6", ref BookStrategiesSelect6);
			DebugLog.Info($"配置加载: BookStrategiesSelect6 = {BookStrategiesSelect6}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect7", ref BookStrategiesSelect7);
			DebugLog.Info($"配置加载: BookStrategiesSelect7 = {BookStrategiesSelect7}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect8", ref BookStrategiesSelect8);
			DebugLog.Info($"配置加载: BookStrategiesSelect8 = {BookStrategiesSelect8}");

			DomainManager.Mod.GetSetting(ModIdStr, "BookStrategiesSelect9", ref BookStrategiesSelect9);
			DebugLog.Info($"配置加载: BookStrategiesSelect9 = {BookStrategiesSelect9}");

			// GetQiArtStrategyDeltaNeiliBonus
			DomainManager.Mod.GetSetting(ModIdStr, "GetQiArtStrategyDeltaNeiliBonus", ref GetQiArtStrategyDeltaNeiliBonus);
			DebugLog.Info($"配置加载: GetQiArtStrategyDeltaNeiliBonus = {GetQiArtStrategyDeltaNeiliBonus}");

			// 原QuantumMaster.cs中的设置读取
			DomainManager.Mod.GetSetting(ModIdStr, "collectResource", ref collectResource);
			DebugLog.Info($"配置加载: collectResource = {collectResource}");

			DomainManager.Mod.GetSetting(ModIdStr, "GetCollectResourceAmount", ref GetCollectResourceAmount);
			DebugLog.Info($"配置加载: GetCollectResourceAmount = {GetCollectResourceAmount}");

			DomainManager.Mod.GetSetting(ModIdStr, "CreateBuildingArea", ref CreateBuildingArea);
			DebugLog.Info($"配置加载: CreateBuildingArea = {CreateBuildingArea}");

			DomainManager.Mod.GetSetting(ModIdStr, "CalcNeigongLoopingEffect", ref CalcNeigongLoopingEffect);
			DebugLog.Info($"配置加载: CalcNeigongLoopingEffect = {CalcNeigongLoopingEffect}");

			// DomainManager.Mod.GetSetting(ModIdStr, "UpdateResourceBlock", ref UpdateResourceBlock);
			// DebugLog.Info($"配置加载: UpdateResourceBlock = {UpdateResourceBlock}");

			DomainManager.Mod.GetSetting(ModIdStr, "OfflineUpdateShopManagement", ref OfflineUpdateShopManagement);
			DebugLog.Info($"配置加载: OfflineUpdateShopManagement = {OfflineUpdateShopManagement}");

			DomainManager.Mod.GetSetting(ModIdStr, "ApplyLifeSkillCombatResult", ref ApplyLifeSkillCombatResult);
			DebugLog.Info($"配置加载: ApplyLifeSkillCombatResult = {ApplyLifeSkillCombatResult}");

			DomainManager.Mod.GetSetting(ModIdStr, "CalcReadInCombat", ref CalcReadInCombat);
			DebugLog.Info($"配置加载: CalcReadInCombat = {CalcReadInCombat}");

			DomainManager.Mod.GetSetting(ModIdStr, "CalcQiQrtInCombat", ref CalcQiQrtInCombat);
			DebugLog.Info($"配置加载: CalcQiQrtInCombat = {CalcQiQrtInCombat}");

			DomainManager.Mod.GetSetting(ModIdStr, "CalcLootItem", ref CalcLootItem);
			DebugLog.Info($"配置加载: CalcLootItem = {CalcLootItem}");

			DomainManager.Mod.GetSetting(ModIdStr, "InitPathContent", ref InitPathContent);
			DebugLog.Info($"配置加载: InitPathContent = {InitPathContent}");

			DomainManager.Mod.GetSetting(ModIdStr, "GetStrategyProgressAddValue", ref GetStrategyProgressAddValue);
			DebugLog.Info($"配置加载: GetStrategyProgressAddValue = {GetStrategyProgressAddValue}");

			DomainManager.Mod.GetSetting(ModIdStr, "ApplyImmediateReadingStrategyEffectForLifeSkill", ref ApplyImmediateReadingStrategyEffectForLifeSkill);
			DebugLog.Info($"配置加载: ApplyImmediateReadingStrategyEffectForLifeSkill = {ApplyImmediateReadingStrategyEffectForLifeSkill}");

			DomainManager.Mod.GetSetting(ModIdStr, "ChoosyGetMaterial", ref ChoosyGetMaterial);
			DebugLog.Info($"配置加载: ChoosyGetMaterial = {ChoosyGetMaterial}");

			DomainManager.Mod.GetSetting(ModIdStr, "ParallelUpdateOnMonthChange", ref ParallelUpdateOnMonthChange);
			DebugLog.Info($"配置加载: ParallelUpdateOnMonthChange = {ParallelUpdateOnMonthChange}");

			// AddChoosyRemainUpgradeData
			DomainManager.Mod.GetSetting(ModIdStr, "AddChoosyRemainUpgradeData", ref AddChoosyRemainUpgradeData);
			DebugLog.Info($"配置加载: AddChoosyRemainUpgradeData = {AddChoosyRemainUpgradeData}");

			DebugLog.Info("所有配置项加载完成");
		}

		public override void Initialize()
		{
			UpdateConfig();
			harmony = new Harmony("QuantumMaster");

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

		// 新增方法，根据配置选择性应用 class 形式的补丁
		private void ApplyClassPatches()
		{
			DebugLog.Info("开始应用类补丁...");
			int appliedClassPatches = 0;
			int skippedClassPatches = 0;

			// 获取所有带有 HarmonyPatch 特性的嵌套类
			var patchClasses = this.GetType().GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
				.Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0)
				.ToList();

			DebugLog.Info($"发现 {patchClasses.Count} 个补丁类");

			foreach (var patchClass in patchClasses)
			{
				try
				{
					string patchName = patchClass.Name;
					bool shouldApply = false;

					// 根据补丁类名称判断对应的开关
					if (patchName.Contains("GetStealActionPhase"))
						shouldApply = steal || openAll;
					else if (patchName.Contains("GetScamActionPhase"))
						shouldApply = scam || openAll;
					else if (patchName.Contains("GetRobActionPhase"))
						shouldApply = rob || openAll;
					else if (patchName.Contains("GetStealLifeSkillActionPhase"))
						shouldApply = stealLifeSkill || openAll;
					else if (patchName.Contains("GetStealCombatSkillActionPhase"))
						shouldApply = stealCombatSkill || openAll;
					else if (patchName.Contains("GetPoisonActionPhase"))
						shouldApply = poison || openAll;
					else if (patchName.Contains("GetPlotHarmActionPhase"))
						shouldApply = plotHarm || openAll;
					else if (patchName.Contains("GenderGetRandom"))
						shouldApply = (gender0 || gender1) || openAll;
					else if (patchName.Contains("CheckRopeOrSwordHit") || patchName.Contains("CheckRopeOrSwordHitOutofCombat"))
						shouldApply = ropeOrSword || openAll;
					else if (patchName.Contains("GetAskToTeachSkillRespondChance"))
						shouldApply = GetAskToTeachSkillRespondChance || openAll;
					else if (patchName.Contains("GetTaughtNewSkillSuccessRate"))
						shouldApply = GetTaughtNewSkillSuccessRate || openAll;
					else if (patchName.Contains("CatchCricket"))
						shouldApply = CatchCricket || openAll;
					else if (patchName.Contains("CheckCricketIsSmart"))
						shouldApply = CheckCricketIsSmart || openAll;
					else if (patchName.Contains("GetCurrReadingEventBonusRate"))
						shouldApply = GetCurrReadingEventBonusRate || openAll;
					// else if (patchName.Contains("GetDropRate"))
					// 	shouldApply = true; // 这个似乎总是应用的
					else if (patchName.Contains("GeneratePageIncompleteState"))
						shouldApply = GeneratePageIncompleteState || openAll;
					else if (patchName.Contains("Cricket_Initialize"))
						shouldApply = CricketInitialize || openAll;
					else if (patchName.Contains("TryAddLoopingEvent"))
						shouldApply = TryAddLoopingEvent || openAll;
					else if (patchName.Contains("SetAvailableReadingStrategies"))
						shouldApply = BookStrategies || openAll;
					else if (patchName.Contains("MapBlockData_InitResources"))
						shouldApply = InitResources || openAll;
					else if (patchName.Contains("SetSectMemberApproveTaiwu"))
						shouldApply = SetSectMemberApproveTaiwu || openAll;
					// GetBisexualTrue
					// GetBisexualFalse
					else if (patchName.Contains("GetBisexual"))
						shouldApply = GetBisexualTrue || GetBisexualFalse || openAll;
					// GetQiArtStrategyDeltaNeiliBonus
					else if (patchName.Contains("GetQiArtStrategyDeltaNeiliBonus") || patchName.Contains("GetQiArtStrategyExtraNeiliAllocationBonus"))
						shouldApply = GetQiArtStrategyDeltaNeiliBonus || openAll;
					else
						shouldApply = true; // 默认应用，如果没有明确的开关

					if (shouldApply)
					{
						DebugLog.Info($"应用补丁类: {patchName}");
						harmony.PatchAll(patchClass);
						appliedClassPatches++;
					}
					else
					{
						DebugLog.Info($"跳过补丁类: {patchName} (已禁用)");
						skippedClassPatches++;
					}
				}
				catch (Exception ex)
				{
					DebugLog.Warning($"应用补丁类 {patchClass.Name} 时出错: {ex.Message}");
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

		// 辅助方法 - 从Class1.cs迁移
		public static bool handleActionPhase(IRandomSource random, Character targetChar, int alertFactor, bool showCheckAnim, short templateId, ref sbyte __result, Character currentChar)
		{
			var targetId = targetChar.GetId();
			var taiwuid = DomainManager.Taiwu.GetTaiwuCharId();
			DomainManager.TaiwuEvent.ShowInteractCheckAnimation = false;
			if (showCheckAnim)
			{
				CombatSkillShorts selfCombatSkillAttainments = currentChar.GetCombatSkillAttainments();
				LifeSkillShorts selfLifeSkillAttainments = currentChar.GetLifeSkillAttainments();
				OuterAndInnerInts selfPenetrations = currentChar.GetPenetrations();
				OuterAndInnerInts selfPenetrationResists = currentChar.GetPenetrationResists();
				short selfAttackSpeed = currentChar.GetAttackSpeed();
				CombatSkillShorts targetCombatSkillAttainments = targetChar.GetCombatSkillAttainments();
				LifeSkillShorts targetLifeSkillAttainments = targetChar.GetLifeSkillAttainments();
				OuterAndInnerInts targetPenetrations = targetChar.GetPenetrations();
				OuterAndInnerInts targetPenetrationResists = targetChar.GetPenetrationResists();
				short targetAttackSpeed = targetChar.GetAttackSpeed();
				DomainManager.TaiwuEvent.InteractCheckData = new EventInteractCheckData(templateId)
				{
					SelfCombatSkillAttainments = selfCombatSkillAttainments,
					SelfLifeSkillAttainments = selfLifeSkillAttainments,
					SelfPenetrations = selfPenetrations,
					SelfPenetrationResists = selfPenetrationResists,
					SelfAttackSpeed = selfAttackSpeed,
					TargetCombatSkillAttainments = targetCombatSkillAttainments,
					TargetLifeSkillAttainments = targetLifeSkillAttainments,
					TargetPenetrations = targetPenetrations,
					TargetPenetrationResists = targetPenetrationResists,
					TargetAttackSpeed = targetAttackSpeed,
					TargetAlertFactor = alertFactor,
					SelfNameRelatedData = DomainManager.Character.GetNameRelatedData(currentChar.GetId()),
					TargetNameRelatedData = DomainManager.Character.GetNameRelatedData(targetChar.GetId())
				};
				DomainManager.TaiwuEvent.ShowInteractCheckAnimation = true;
			}
			if (targetId == taiwuid)
			{
				if (showCheckAnim)
				{
					DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(0);
					DomainManager.TaiwuEvent.InteractCheckData.FailPhase = 0;
				}
				__result = 0;
				return false;
			}
			else if (taiwuid == currentChar.GetId())
			{
				if (showCheckAnim)
				{
					DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
					DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
					DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
					DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
					DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
					DomainManager.TaiwuEvent.InteractCheckData.PhaseProbList.Add(100);
				}
				__result = 5;
				return false;
			}
			return true;
		}

		// 补丁类 - 偷窃补丁
		[HarmonyPatch(typeof(Character), "GetStealActionPhase")]
		public class Patch_GetStealActionPhase
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, Character targetChar, int alertFactor, bool showCheckAnim, ref sbyte __result, Character __instance)
			{
				if (steal)
				{
					return handleActionPhase(random, targetChar, alertFactor, showCheckAnim, 1, ref __result, __instance);
				}
				return true;
			}
		}

		// 补丁类 - 唬骗补丁
		[HarmonyPatch(typeof(Character), "GetScamActionPhase")]
		public class Patch_GetScamActionPhase
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, Character targetChar, int alertFactor, bool showCheckAnim, ref sbyte __result, Character __instance)
			{
				if (scam)
				{
					return handleActionPhase(random, targetChar, alertFactor, showCheckAnim, 0, ref __result, __instance);
				}
				return true;
			}
		}

		// 补丁类 - 抢劫补丁
		[HarmonyPatch(typeof(Character), "GetRobActionPhase")]
		public class Patch_GetRobActionPhase
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, Character targetChar, int alertFactor, bool showCheckAnim, ref sbyte __result, Character __instance)
			{
				if (rob)
				{
					return handleActionPhase(random, targetChar, alertFactor, showCheckAnim, 2, ref __result, __instance);
				}
				return true;
			}
		}

		// 补丁类 - 偷学生活技能补丁
		[HarmonyPatch(typeof(Character), "GetStealLifeSkillActionPhase")]
		public class Patch_GetStealLifeSkillActionPhase
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, Character targetChar, sbyte lifeSkillType, sbyte grade, bool showCheckAnim, ref sbyte __result, Character __instance)
			{
				if (stealLifeSkill)
				{
					return handleActionPhase(random, targetChar, targetChar.GetGradeAlertFactor(grade, 1), showCheckAnim, 6, ref __result, __instance);
				}
				return true;
			}
		}

		// 补丁类 - 偷学战斗技能补丁
		[HarmonyPatch(typeof(Character), "GetStealCombatSkillActionPhase")]
		public class Patch_GetStealCombatSkillActionPhase
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, Character targetChar, sbyte combatSkillType, sbyte grade, bool showCheckAnim, ref sbyte __result, Character __instance)
			{
				if (stealCombatSkill)
				{
					return handleActionPhase(random, targetChar, targetChar.GetGradeAlertFactor(grade, 1), showCheckAnim, 7, ref __result, __instance);
				}
				return true;
			}
		}

		// 补丁类 - 下毒补丁
		[HarmonyPatch(typeof(Character), "GetPoisonActionPhase")]
		public class Patch_GetPoisonActionPhase
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, Character targetChar, int alertFactor, bool showCheckAnim, ref sbyte __result, Character __instance)
			{
				if (poison)
				{
					return handleActionPhase(random, targetChar, alertFactor, showCheckAnim, 3, ref __result, __instance);
				}
				return true;
			}
		}

		// 补丁类 - 暗害补丁
		[HarmonyPatch(typeof(Character), "GetPlotHarmActionPhase")]
		public class Patch_GetPlotHarmActionPhase
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, Character targetChar, int alertFactor, bool showCheckAnim, ref sbyte __result, Character __instance)
			{
				if (plotHarm)
				{
					return handleActionPhase(random, targetChar, alertFactor, showCheckAnim, 4, ref __result, __instance);
				}
				return true;
			}
		}

		// 补丁类 - 性别生成补丁
		[HarmonyPatch(typeof(Gender), "GetRandom")]
		public class Patch_GenderGetRandom
		{
			[HarmonyPrefix]
			public static bool Prefix(ref sbyte __result)
			{
				if (gender0)
				{
					// 0 = 女 1 = 男
					__result = (sbyte)0;
					return false;
				}
				if (gender1)
				{
					__result = (sbyte)1;
					return false;
				}
				return true;
			}
		}

		// 补丁类 - 绳子命中补丁
		[HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHit")]
		public class Patch_CheckRopeOrSwordHit
		{
			[HarmonyPrefix]
			public static bool Prefix(ref bool __result)
			{
				if (ropeOrSword)
				{
					__result = true;
					return false;
				}
				return true;
			}
		}

		// 补丁类 - 战斗外绳子命中补丁
		[HarmonyPatch(typeof(CombatDomain), "CheckRopeOrSwordHitOutofCombat")]
		public class Patch_CheckRopeOrSwordHitOutofCombat
		{
			[HarmonyPrefix]
			public static bool Prefix(ref bool __result)
			{
				if (ropeOrSword)
				{
					__result = true;
					return false;
				}
				return true;
			}
		}

		// 补丁类 - 教学技能概率补丁
		[HarmonyPatch(typeof(GameData.Domains.Character.Ai.AiHelper.GeneralActionConstants), "GetAskToTeachSkillRespondChance")]
		public class Patch_GetAskToTeachSkillRespondChance
		{
			[HarmonyPostfix]
			public static void Postfix(ref int __result)
			{
				if (GetAskToTeachSkillRespondChance)
				{
					__result = (int)(__result > 0 ? 100 : 0);
				}
			}
		}

		// 补丁类 - 学习技能成功率补丁
		[HarmonyPatch(typeof(Character), "GetTaughtNewSkillSuccessRate")]
		public class Patch_GetTaughtNewSkillSuccessRate
		{
			[HarmonyPostfix]
			public static void Postfix(ref int __result)
			{
				if (GetTaughtNewSkillSuccessRate)
				{
					__result = (int)(__result > 0 ? 100 : 0);
				}
			}
		}

		// 补丁类 - 抓蛐蛐补丁
		[HarmonyPatch(typeof(GameData.Domains.Item.ItemDomain), "CatchCricket")]
		public class Patch_ItemDomain_CatchCricket
		{
			[HarmonyPrefix]
			public static void Prefix(ref short singLevel)
			{
				if (CatchCricket)
				{
					singLevel = (short)100;
				}
			}
		}

		// 补丁类 - 蛐蛐升级补丁
		[HarmonyPatch(typeof(GameData.Domains.Extra.ExtraDomain), "CheckCricketIsSmart")]
		public class Patch_CheckCricketIsSmart
		{
			[HarmonyPrefix]
			public static bool Prefix(ref bool __result, IRandomSource random, ItemKey cricketKey)
			{
				if (CheckCricketIsSmart)
				{
					// 获取Cricket对象
					var cricket = DomainManager.Item.GetElement_Crickets(cricketKey.Id);
					// 如果条件满足返回 false
					if (ItemTemplateHelper.GetCricketGrade(cricket.GetColorId(), cricket.GetPartId()) >= 7 ||
							CricketParts.Instance[DomainManager.Item.GetElement_Crickets(cricketKey.Id).GetColorId()].Type == ECricketPartsType.Trash)
					{
						__result = false;
						return false; // 跳过原始方法
					}
					// 如果不满足上面的条件，则直接返回 true
					__result = true;
					return false; // 跳过原始方法
				}
				return true;
			}
		}

		// 补丁类 - 灵光一闪概率补丁
		[HarmonyPatch(typeof(TaiwuDomain), "GetCurrReadingEventBonusRate")]
		public class Patch_GetCurrReadingEventBonusRate
		{
			[HarmonyPostfix]
			public static void Postfix(ref short __result)
			{
				if (GetCurrReadingEventBonusRate)
				{
					if (__result > 0)
					{
						__result = 100;
					}
				}
			}
		}

		// 补丁类 - 物品掉落率补丁
		// [HarmonyPatch(typeof(ItemTemplateHelper), "GetDropRate")]
		// public class Patch_GetDropRate
		// {
		// 	[HarmonyPostfix]
		// 	public static void Postfix(ref sbyte __result)
		// 	{
		// 		if (__result > 0)
		// 		{
		// 			__result = 100;
		// 		}
		// 	}
		// }

		// 补丁类 - 书籍页面状态生成补丁
		[HarmonyPatch(typeof(GameData.Domains.Item.SkillBook), "GeneratePageIncompleteState")]
		public class Patch_GeneratePageIncompleteState
		{
			[HarmonyPrefix]
			public static bool Prefix(ref ushort __result, IRandomSource random, sbyte skillGroup, sbyte grade, sbyte completePagesCount, sbyte lostPagesCount, bool outlineAlwaysComplete)
			{
				if (GeneratePageIncompleteState)
				{
					int normalPagesCount = ((skillGroup == 1) ? 5 : 5);
					if (completePagesCount < 0)
					{
						float mean = 3f - (float)grade / 4f;
						float min = Math.Max(0f, mean - 1f);
						float max = Math.Min(normalPagesCount, mean + 1f);
						completePagesCount = (sbyte)(max > min ? max : min);
					}
					if (lostPagesCount < 0)
					{
						float mean = -1f + (float)grade / 2.667f;
						float min = Math.Max(0f, mean - 1f);
						float max = Math.Min(normalPagesCount - completePagesCount, mean + 1f);
						lostPagesCount = (sbyte)Math.Round(max > min ? min : max);
					}
					int incompletePagesCount = normalPagesCount - completePagesCount - lostPagesCount;
					if (incompletePagesCount < 0)
					{
						throw new Exception($"IncompletePagesCount is less than zero: {incompletePagesCount}");
					}
					int i;
					int leftPagesCount = completePagesCount + incompletePagesCount + lostPagesCount;
					List<sbyte> pLeftStates = new List<sbyte>((int)leftPagesCount);
					for (i = 0; i < completePagesCount; i++)
					{
						pLeftStates.Add((sbyte)0);
					}
					for (i = completePagesCount; i < completePagesCount + incompletePagesCount; i++)
					{
						pLeftStates.Add((sbyte)1);
					}
					for (i = completePagesCount + incompletePagesCount; i < leftPagesCount; i++)
					{
						pLeftStates.Add((sbyte)2);
					}
					if (!FixedPagePos)
					{
						// 打乱 pLeftStates 顺序
						for (i = 0; i < leftPagesCount; i++)
						{
							Random _random = new Random();
							int rate = _random.Next(100);
							if (rate > 50)
							{
								int j = _random.Next(0, leftPagesCount);
								sbyte temp = pLeftStates[i];
								pLeftStates[i] = pLeftStates[j];
								pLeftStates[j] = temp;
							}
						}
					}
					i = 0;
					ushort states = 0;
					byte pageBeginId = 0;
					if (skillGroup == 1)
					{
						sbyte outlineState = (sbyte)((!outlineAlwaysComplete && random.CheckPercentProb(90)) ? 2 : 0);
						states = SkillBookStateHelper.SetPageIncompleteState(states, 0, outlineState);
						pageBeginId = 1;
					}
					for (i = 0; i < normalPagesCount; i++)
					{
						byte pageId = (byte)(pageBeginId + i);
						sbyte state = pLeftStates[i];
						states = SkillBookStateHelper.SetPageIncompleteState(states, pageId, state);
					}
					__result = states;
					return false;
				}
				return true;
			}
		}

		// 补丁类 - 蛐蛐初始化补丁
		[HarmonyPatch(typeof(GameData.Domains.Item.Cricket), "Initialize", new Type[] { typeof(IRandomSource), typeof(short), typeof(short), typeof(int) })]
		public class Patch_Cricket_Initialize
		{
			[HarmonyPostfix]
			public static void Postfix(ref GameData.Domains.Item.Cricket __instance, IRandomSource random, short colorId, short partId, int itemId)
			{
				if (CricketInitialize)
				{
					var trv = Traverse.Create(__instance);
					var TemplateId = trv.Field("TemplateId").GetValue<short>();
					short[] emptyArray = new short[5];
					sbyte grade = trv.Method("CalcGrade", colorId, partId).GetValue<sbyte>();
					int hp = trv.Method("CalcHp").GetValue<int>();
					int durability = grade + 1 + hp / 20;
					durability = Math.Max(durability * 135 / 100, 1);
					trv.Field("MaxDurability").SetValue((short)durability);
					trv.Field("CurrDurability").SetValue((short)durability);
					trv.Field("_injuries").SetValue(emptyArray);
					trv.Field("_age").SetValue((sbyte)0);
				}
			}
		}

		// 补丁类 - 天人感应补丁
		[HarmonyPatch(typeof(TaiwuDomain), "TryAddLoopingEvent")]
		public class Patch_TryAddLoopingEvent
		{
			[HarmonyPrefix]
			public static void Prefix(ref int basePercentProb)
			{
				if (TryAddLoopingEvent)
				{
					basePercentProb = 100;
				}
			}
		}

		// 补丁类 - 读书策略补丁
		[HarmonyPatch(typeof(GameData.Domains.Extra.ExtraDomain), "SetAvailableReadingStrategies")]
		public class Patch_SetAvailableReadingStrategies
		{
			[HarmonyPrefix]
			public static void Prefix(ref SByteList strategyIds)
			{
				if (BookStrategies)
				{
					SByteList ids = SByteList.Create();
					ids.Items.Add((sbyte)(BookStrategiesSelect1));
					ids.Items.Add((sbyte)(BookStrategiesSelect2));
					ids.Items.Add((sbyte)(BookStrategiesSelect3));
					ids.Items.Add((sbyte)(BookStrategiesSelect4));
					ids.Items.Add((sbyte)(BookStrategiesSelect5));
					ids.Items.Add((sbyte)(BookStrategiesSelect6));
					ids.Items.Add((sbyte)(BookStrategiesSelect7));
					ids.Items.Add((sbyte)(BookStrategiesSelect8));
					ids.Items.Add((sbyte)(BookStrategiesSelect9));
					strategyIds = ids;
				}
			}
		}

		// 补丁类 - 初始化地块资源补丁
		[HarmonyPatch(typeof(GameData.Domains.Map.MapBlockData), "InitResources")]
		public class Patch_MapBlockData_InitResources
		{
			[HarmonyPrefix]
			public unsafe static bool Prefix(MapBlockData __instance)
			{
				if (InitResources)
				{
					MapBlockItem configData = __instance.GetConfig();
					if (configData != null)
					{
						for (sbyte resourceType = 0; resourceType < 6; resourceType++)
						{
							short maxResource = configData.Resources[resourceType];
							if (maxResource < 0)
							{
								maxResource = (short)(Math.Abs(maxResource) * 5);
							}
							else if (maxResource != 0)
							{
								maxResource = (short)(maxResource + 25);
							}
							__instance.MaxResources.Items[resourceType] = maxResource;
							__instance.CurrResources.Items[resourceType] = (short)(maxResource * 50 / 100 * ItemTemplateHelper.GetGainResourcePercent(12) / 100);
						}
					}
					return false;
				}
				return true;
			}
		}

		// 补丁类 - 门派成员认可太吾补丁
		[HarmonyPatch(typeof(GameData.Domains.TaiwuEvent.EventHelper.EventHelper), "SetSectMemberApproveTaiwu")]
		public class Patch_SetSectMemberApproveTaiwu
		{
			[HarmonyPrefix]
			public static bool Prefix(sbyte sectId, byte countMax, ref sbyte gradeMin, sbyte gradeMax, ref List<GameData.Domains.Character.Character> __result)
			{
				if (!SetSectMemberApproveTaiwu)
				{
					return true;
				}
				List<GameData.Domains.Character.Character> allMembers = GameData.Domains.TaiwuEvent.EventHelper.EventHelper.GetSectCharList(sectId, 0, 8);
				for (int i = allMembers.Count - 1; i >= 0; i--)
				{
					OrganizationInfo orgInfo = allMembers[i].GetOrganizationInfo();
					if (orgInfo.Grade != gradeMax)
					{
						allMembers.RemoveAt(i);
						continue;
					}
					if (GameData.Domains.TaiwuEvent.EventHelper.EventHelper.IsSectCharApprovedTaiwu(allMembers[i].GetId()))
					{
						allMembers.RemoveAt(i);
						continue;
					}
				}
				var ids1 = new List<string>();
				foreach (var item in allMembers)
				{
					ids1.Add(item.GetId().ToString());
				}

				countMax = (byte)Math.Min(countMax, allMembers.Count);
				List<GameData.Domains.Character.Character> retList = new List<GameData.Domains.Character.Character>();
				for (int i = 0; i < countMax; i++)
				{
					GameData.Domains.TaiwuEvent.EventHelper.EventHelper.SetSectCharApprovedTaiwu(allMembers[i].GetId());
					retList.Add(allMembers[i]);
				}
				__result = retList;
				var ids = new List<string>();
				foreach (var item in __result)
				{
					ids.Add(item.GetId().ToString());
				}

				return false;
			}
		}

		// 补丁类 - 强制所有人双性恋
		// public bool GameData.Domains.Character.Character.GetBisexual()

		[HarmonyPatch(typeof(GameData.Domains.Character.Character), "GetBisexual")]
		public class Patch_Character_GetBisexual
		{
			[HarmonyPrefix]
			public static bool Prefix(ref bool __result)
			{
				if (GetBisexualTrue || GetBisexualFalse)
				{
					// 如果 GetBisexualTrue 为 true，则强制返回 true
					if (GetBisexualTrue)
					{
						__result = true;
					}
					// 如果 GetBisexualFalse 为 true，则强制返回 false
					else if (GetBisexualFalse)
					{
						__result = false;
					}
					return false; // 跳过原始方法
				}
				return true; // 默认行为
			}
		}

		// public int GameData.Domains.Taiwu.TaiwuDomain.GetQiArtStrategyDeltaNeiliBonus(Redzen.Random.IRandomSource random)
		[HarmonyPatch(typeof(GameData.Domains.Taiwu.TaiwuDomain), "GetQiArtStrategyDeltaNeiliBonus")]
		public class Patch_TaiwuDomain_GetQiArtStrategyDeltaNeiliBonus
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, ref int __result)
			{
				if (GetQiArtStrategyDeltaNeiliBonus)
				{
					GameData.Domains.Character.Character taiwuChar = DomainManager.Taiwu.GetTaiwu();
					short loopingNeigongTemplateId = taiwuChar.GetLoopingNeigong();
					if (DomainManager.Extra.TryGetElement_QiArtStrategyMap(loopingNeigongTemplateId, out var qiArtStrategyList))
					{
						if (qiArtStrategyList.Items.Count == 0)
						{
							__result = 0;
							return false;
						}
						int bonus = 0;
						foreach (sbyte id in qiArtStrategyList.Items)
						{
							if (id != -1)
							{
								QiArtStrategyItem config = QiArtStrategy.Instance[id];
								bonus += config.MaxExtraNeili;
							}
						}
						__result = bonus;
						return false;
					}
					__result = 0;
					return false; // 跳过原始方法
				}
				return true; // 默认行为
			}
		}

		// public int GameData.Domains.Taiwu.TaiwuDomain.GetQiArtStrategyExtraNeiliAllocationBonus(Redzen.Random.IRandomSource random)
		[HarmonyPatch(typeof(GameData.Domains.Taiwu.TaiwuDomain), "GetQiArtStrategyExtraNeiliAllocationBonus")]
		public class Patch_TaiwuDomain_GetQiArtStrategyExtraNeiliAllocationBonus
		{
			[HarmonyPrefix]
			public static bool Prefix(IRandomSource random, ref int __result)
			{
				if (GetQiArtStrategyDeltaNeiliBonus)
				{
					GameData.Domains.Character.Character taiwuChar = DomainManager.Taiwu.GetTaiwu();
					short loopingNeigongTemplateId = taiwuChar.GetLoopingNeigong();
					if (DomainManager.Extra.TryGetElement_QiArtStrategyMap(loopingNeigongTemplateId, out var qiArtStrategyList))
					{
						if (qiArtStrategyList.Items.Count == 0)
						{
							__result = 0;
							return false;
						}
						int bonus = 0;
						foreach (sbyte id in qiArtStrategyList.Items)
						{
							if (id != -1)
							{
								QiArtStrategyItem config = QiArtStrategy.Instance[id];
								bonus += config.MaxExtraNeiliAllocation;
							}
						}
						__result = bonus;
						return false;
					}
					__result = 0;
					return false; // 跳过原始方法
				}
				return true; // 默认行为
			}
		}




















		// 原QuantumMaster.cs中的方法保留
		public bool patchCreateBuildingArea()
		{
			if (!CreateBuildingArea && !openAll) return false;

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

			// 3 if (num11 >= 10 || random.CheckPercentProb(num11 * 10))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					3);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchCalcNeigongLoopingEffect()
		{
			if (!CalcNeigongLoopingEffect && !openAll) return false;

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
			if (!collectResource && !openAll) return false;

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

			// 2 if (random.CheckPercentProb(gradeUpOdds))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					2);

			// 3 if (random.CheckPercentProb(odds))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					3);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchGetCollectResourceAmount()
		{
			if (!GetCollectResourceAmount && !openAll) return false;

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

		// public bool patchUpdateResourceBlock()
		// {
		// 	if (!UpdateResourceBlock && !openAll) return false;

		// 	var OriginalMethod = new OriginalMethodInfo
		// 	{
		// 		Type = typeof(GameData.Domains.Building.BuildingDomain),
		// 		MethodName = "UpdateResourceBlock",
		// 		// DataContext context, short settlementId, BuildingBlockKey blockKey, BuildingBlockData blockData, List<short> neighborList, List<short> expandedResourceList, List<int> neighborDistanceList, List<short> neighborRangeOneList
		// 		Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(short), typeof(GameData.Domains.Building.BuildingBlockKey), typeof(GameData.Domains.Building.BuildingBlockData), typeof(List<short>), typeof(List<short>), typeof(List<int>), typeof(List<short>) }
		// 	};

		// 	patchBuilder = GenericTranspiler.CreatePatchBuilder(
		// 			"UpdateResourceBlock",
		// 			OriginalMethod);

		// 	// 1 if (random.CheckPercentProb(growOdds))
		// 	patchBuilder.AddExtensionMethodReplacement(
		// 			PatchPresets.Extensions.CheckPercentProb,
		// 			PatchPresets.Replacements.CheckPercentProbTrue,
		// 			1);

		// 	// 2 if (neighborBlock2.TemplateId == 0 && neighborBlock2.RootBlockIndex < 0 && random.CheckPercentProb(expandOdds))
		// 	patchBuilder.AddExtensionMethodReplacement(
		// 			PatchPresets.Extensions.CheckPercentProb,
		// 			PatchPresets.Replacements.CheckPercentProbTrue,
		// 			2);

		// 	patchBuilder.Apply(harmony);

		// 	return true;
		// }

		public bool patchOfflineUpdateShopManagement()
		{
			if (!OfflineUpdateShopManagement && !openAll) return false;
			
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
			if (!ApplyLifeSkillCombatResult && !openAll) return false;

			var OriginalMethod = new OriginalMethodInfo
			{
				Type = typeof(GameData.Domains.Taiwu.TaiwuDomain),
				MethodName = "DebateGameOver",
				// DataContext context, bool isTaiwuWin
				Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(bool) }
			};

			patchBuilder = GenericTranspiler.CreatePatchBuilder(
					"DebateGameOver",
					OriginalMethod);

			// CheckPercentProb
			// 1 if (context.Random.CheckPercentProb(bonusOdds))
			patchBuilder.AddExtensionMethodReplacement(
					PatchPresets.Extensions.CheckPercentProb,
					PatchPresets.Replacements.CheckPercentProbTrue,
					1);

			patchBuilder.Apply(harmony);

			return true;
		}

		public bool patchCalcReadInCombat()
		{
			if (!CalcReadInCombat && !openAll) return false;

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
			if (!CalcLootItem && !openAll) return false;

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
			if (!InitPathContent && !openAll) return false;

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
			if (!GetStrategyProgressAddValue && !openAll) return false;

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
			if (!ApplyImmediateReadingStrategyEffectForLifeSkill && !openAll) return false;

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
			if (!ChoosyGetMaterial && !openAll) return false;

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
			if (!ParallelUpdateOnMonthChange && !openAll) return false;

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
			if (!CalcQiQrtInCombat && !openAll) return false;

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
			if (!AddChoosyRemainUpgradeData && !openAll) return false;

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

	public class RandomPath
	{
		public int Random_Next_2Args_Max(int min, int max)
		{
			DebugLog.Info($"Random_Next_2Args_Max min {min} max {max}");
			return Math.Max(min, max - 1);
		}

		public int Random_Next_2Args_Min(int min, int max)
		{
			DebugLog.Info($"Random_Next_2Args_Min min {min} max {max}");
			return Math.Min(min, max - 1);
		}

		public int Random_Next_1Arg_Max(int max)
		{
			return Math.Max(0, max - 1);
		}

		public int Random_Next_1Arg_0(int max)
		{
			return 0;
		}

		// Fix the rest of the methods similarly
		public static bool Random_CheckPercentProb_True(IRandomSource randomSource, int percent)
		{
			if (percent > 0)
			{
				return true;
			}
			return false;
		}

		public static bool Random_CheckPercentProb_False(IRandomSource randomSource, int percent)
		{
			if (percent < 100)
			{
				return false;
			}
			return true;
		}

		public static bool Random_CheckProb_True(IRandomSource randomSource, int chance, int percent)
		{
			if (percent > 0)
			{
				return true;
			}
			return false;
		}

		public static bool Random_CheckProb_False(IRandomSource randomSource, int chance, int percent)
		{
			if (percent < 100)
			{
				return false;
			}
			return true;
		}
	}
}