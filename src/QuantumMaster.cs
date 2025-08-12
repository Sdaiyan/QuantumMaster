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
						shouldApply = ConfigManager.steal || openAll;
					else if (patchName.Contains("GetScamActionPhase"))
						shouldApply = ConfigManager.scam || openAll;
					else if (patchName.Contains("GetRobActionPhase"))
						shouldApply = ConfigManager.rob || openAll;
					else if (patchName.Contains("GetStealLifeSkillActionPhase"))
						shouldApply = ConfigManager.stealLifeSkill || openAll;
					else if (patchName.Contains("GetStealCombatSkillActionPhase"))
						shouldApply = ConfigManager.stealCombatSkill || openAll;
					else if (patchName.Contains("GetPoisonActionPhase"))
						shouldApply = ConfigManager.poison || openAll;
					else if (patchName.Contains("GetPlotHarmActionPhase"))
						shouldApply = ConfigManager.plotHarm || openAll;
					else if (patchName.Contains("GenderGetRandom"))
						shouldApply = (ConfigManager.gender0 || ConfigManager.gender1) || openAll;
					else if (patchName.Contains("CheckRopeOrSwordHit") || patchName.Contains("CheckRopeOrSwordHitOutofCombat"))
						shouldApply = ConfigManager.ropeOrSword || openAll;
					else if (patchName.Contains("GetAskToTeachSkillRespondChance"))
						shouldApply = ConfigManager.GetAskToTeachSkillRespondChance || openAll;
					else if (patchName.Contains("GetTaughtNewSkillSuccessRate"))
						shouldApply = ConfigManager.GetTaughtNewSkillSuccessRate || openAll;
					else if (patchName.Contains("CatchCricket"))
						shouldApply = ConfigManager.CatchCricket || openAll;
					else if (patchName.Contains("CheckCricketIsSmart"))
						shouldApply = ConfigManager.CheckCricketIsSmart || openAll;
					else if (patchName.Contains("GetCurrReadingEventBonusRate"))
						shouldApply = ConfigManager.GetCurrReadingEventBonusRate || openAll;
					// else if (patchName.Contains("GetDropRate"))
					// 	shouldApply = true; // 这个似乎总是应用的
					else if (patchName.Contains("GeneratePageIncompleteState"))
						shouldApply = ConfigManager.GeneratePageIncompleteState || openAll;
					else if (patchName.Contains("Cricket_Initialize"))
						shouldApply = ConfigManager.CricketInitialize || openAll;
					else if (patchName.Contains("TryAddLoopingEvent"))
						shouldApply = ConfigManager.TryAddLoopingEvent || openAll;
					else if (patchName.Contains("SetAvailableReadingStrategies"))
						shouldApply = ConfigManager.BookStrategies || openAll;
					else if (patchName.Contains("MapBlockData_InitResources"))
						shouldApply = ConfigManager.InitResources || openAll;
					else if (patchName.Contains("SetSectMemberApproveTaiwu"))
						shouldApply = ConfigManager.SetSectMemberApproveTaiwu || openAll;
					// GetBisexualTrue
					// GetBisexualFalse
					else if (patchName.Contains("GetBisexual"))
						shouldApply = ConfigManager.GetBisexualTrue || ConfigManager.GetBisexualFalse || openAll;
					// GetQiArtStrategyDeltaNeiliBonus
					else if (patchName.Contains("GetQiArtStrategyDeltaNeiliBonus") || patchName.Contains("GetQiArtStrategyExtraNeiliAllocationBonus"))
						shouldApply = ConfigManager.GetQiArtStrategyDeltaNeiliBonus || openAll;
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
				if (ConfigManager.steal)
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
				if (ConfigManager.scam)
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
				if (ConfigManager.rob)
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
				if (ConfigManager.stealLifeSkill)
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
				if (ConfigManager.stealCombatSkill)
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
				if (ConfigManager.poison)
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
				if (ConfigManager.plotHarm)
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
				if (ConfigManager.gender0)
				{
					// 0 = 女 1 = 男
					__result = (sbyte)0;
					return false;
				}
				if (ConfigManager.gender1)
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
				if (ConfigManager.ropeOrSword)
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
				if (ConfigManager.ropeOrSword)
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
				if (ConfigManager.GetAskToTeachSkillRespondChance)
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
				if (ConfigManager.GetTaughtNewSkillSuccessRate)
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
				if (ConfigManager.CatchCricket)
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
				if (ConfigManager.CheckCricketIsSmart)
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
				if (ConfigManager.GetCurrReadingEventBonusRate)
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
				if (ConfigManager.GeneratePageIncompleteState)
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
					if (!ConfigManager.FixedPagePos)
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
				if (ConfigManager.CricketInitialize)
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
				if (ConfigManager.TryAddLoopingEvent)
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
				if (ConfigManager.BookStrategies)
				{
					SByteList ids = SByteList.Create();
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect1));
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect2));
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect3));
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect4));
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect5));
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect6));
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect7));
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect8));
					ids.Items.Add((sbyte)(ConfigManager.BookStrategiesSelect9));
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
				if (ConfigManager.InitResources)
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
							__instance.CurrResources.Items[resourceType] = (short)(maxResource * 50 / 100 * GameData.Domains.World.SharedMethods.GetGainResourcePercent(12) / 100);
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
				if (!ConfigManager.SetSectMemberApproveTaiwu)
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
				if (ConfigManager.GetBisexualTrue || ConfigManager.GetBisexualFalse)
				{
					// 如果 GetBisexualTrue 为 true，则强制返回 true
					if (ConfigManager.GetBisexualTrue)
					{
						__result = true;
					}
					// 如果 GetBisexualFalse 为 true，则强制返回 false
					else if (ConfigManager.GetBisexualFalse)
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
				if (ConfigManager.GetQiArtStrategyDeltaNeiliBonus)
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
				if (ConfigManager.GetQiArtStrategyDeltaNeiliBonus)
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

	public class RandomPath
	{
		public int Calc_Random_Next_2Args_Max_By_Luck(int min, int max)
		{
			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			// 先随机一个结果
			var randomValue = QuantumMaster.Random.Next(min, max);
			DebugLog.Info($"randomValue = {randomValue}");
			if (luck > 0)
			{
				// 获取的数量/概率 = Math.min(当前 + (最大-当前) * 因子, 最大)
				return (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
			}
			if (luck < 0)
			{
				// 获取的数量/概率 = 最小 + (当前-最小) * (1 + 因子)
				return Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
			}
			return randomValue;
		}

		public int Random_Next_2Args_Max(int min, int max)
		{
			var result = Calc_Random_Next_2Args_Max_By_Luck(min, max);
			DebugLog.Info($"Random_Next_2Args_Max min {min} max {max} result {result}");
			return result;
		}

		public int Calc_Random_Next_2Args_Min_By_Luck(int min, int max)
		{
			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			// 先随机一个结果
			var randomValue = QuantumMaster.Random.Next(min, max);
			DebugLog.Info($"randomValue = {randomValue}");
			if (luck > 0)
			{
				// 获取的数量/概率 = Math.max(当前 - (当前-最小) * 因子, 最小)
				return Math.Max(min, (int)(randomValue - (randomValue - min) * luck));
			}
			if (luck < 0)
			{
				// 获取的数量/概率 = 最小 + (当前-最小) * (1 + 因子)
				return Math.Min(max - 1, min + (int)((randomValue - min) * (1 - luck)));
			}
			return randomValue;
		}

		public int Random_Next_2Args_Min(int min, int max)
		{
			var result = Calc_Random_Next_2Args_Min_By_Luck(min, max);
			DebugLog.Info($"Random_Next_2Args_Min min {min} max {max} result {result}");
			return result;
		}

		public int Calc_Random_Next_1Arg_Max_By_Luck(int max)
		{
			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			// 先随机一个结果
			var randomValue = QuantumMaster.Random.Next(0, max);
			DebugLog.Info($"randomValue = {randomValue}");
			if (luck > 0)
			{
				// 获取的数量/概率 = Math.min(当前 + (最大-当前) * 因子, 最大-1)
				return Math.Min(max - 1, randomValue + (int)((max - 1 - randomValue) * luck));

				// max = 100 randomValue = 50 luck = 0.2
				// 50 + (99 - 50) * 0.2 = 50 + 9.8 = 59.8

				// max = 100 randomValue = 50 luck = 1
				// 50 + (99 - 50) * 1 = 50 + 49 = 99
			}
			if (luck < 0)
			{
				// 获取的数量/概率 = 0 + (当前-0) * (1 + 因子)
				return Math.Max(0, (int)(randomValue * (1 + luck)));

				// max = 100 randomValue = 50 luck = -0.33
				// 50 * (1 - 0.33) = 50 * 0.67 = 33.5
			}
			return randomValue;
		}

		public int Random_Next_1Arg_Max(int max)
		{
			var result = Calc_Random_Next_1Arg_Max_By_Luck(max);
			DebugLog.Info($"Random_Next_1Arg_Max max {max} result {result}");
			return result;
		}

		public int Calc_Random_Next_1Arg_0_By_Luck(int max)
		{
			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			// 先随机一个结果
			var randomValue = QuantumMaster.Random.Next(0, max);
			DebugLog.Info($"randomValue = {randomValue}");
			if (luck > 0)
			{
				// 获取的数量/概率 = Math.max(当前 - 当前 * 因子, 0)
				return Math.Max(0, randomValue - (int)(randomValue * luck));

				// max = 100 randomValue = 50 luck = 0.2
				// 50 - 50 * 0.2 = 50 - 10 = 40

				// max = 100 randomValue = 50 luck = 1
				// 50 - 50 * 1 = 50 - 50 = 0
			}
			if (luck < 0)
			{
				// 获取的数量/概率 = 0 + (当前-0) * (1 - 因子)
				return Math.Min(max - 1, (int)(randomValue * (1 - luck)));

				// max = 100 randomValue = 50 luck = -0.33
				// 50 * (1 - (-0.33)) = 50 * 1.33 = 66.5
			}
			return randomValue;
		}

		public int Random_Next_1Arg_0(int max)
		{
			var result = Calc_Random_Next_1Arg_0_By_Luck(max);
			DebugLog.Info($"Random_Next_1Arg_0 max {max} result {result}");
			return result;
		}

		public static bool Calc_Random_CheckPercentProb_True_By_Luck(IRandomSource randomSource, int percent)
		{
			if (percent <= 0) return false;
			if (percent >= 100) return true;

			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			var randomValue = QuantumMaster.Random.Next(0, 100);
			DebugLog.Info($"randomValue = {randomValue}");

			if (luck > 0)
			{
				// 提高成功概率
				var adjustedPercent = Math.Min(100, percent + (100 - percent) * luck);
				return randomValue < adjustedPercent;
			}
			if (luck < 0)
			{
				// 降低成功概率
				var adjustedPercent = Math.Max(0, percent * (1 + luck));
				return randomValue < adjustedPercent;
			}
			return randomValue < percent;
		}

		public static bool Random_CheckPercentProb_True(IRandomSource randomSource, int percent)
		{
			var result = Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent);
			DebugLog.Info($"Random_CheckPercentProb_True percent {percent} result {result}");
			return result;
		}

		public static bool Calc_Random_CheckPercentProb_False_By_Luck(IRandomSource randomSource, int percent)
		{
			if (percent <= 0) return true;
			if (percent >= 100) return false;

			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			var randomValue = QuantumMaster.Random.Next(0, 100);
			DebugLog.Info($"randomValue = {randomValue}");

			if (luck > 0)
			{
				// 提高失败概率（降低成功概率）
				var adjustedPercent = Math.Max(0, percent - percent * luck);
				return randomValue >= adjustedPercent;
			}
			if (luck < 0)
			{
				// 降低失败概率（提高成功概率）
				var adjustedPercent = Math.Min(100, percent + (100 - percent) * (-luck));
				return randomValue >= adjustedPercent;
			}
			return randomValue >= percent;
		}

		public static bool Random_CheckPercentProb_False(IRandomSource randomSource, int percent)
		{
			var result = Calc_Random_CheckPercentProb_False_By_Luck(randomSource, percent);
			DebugLog.Info($"Random_CheckPercentProb_False percent {percent} result {result}");
			return result;
		}

		public static bool Calc_Random_CheckProb_True_By_Luck(IRandomSource randomSource, int chance, int total)
		{
			if (chance <= 0) return false;
			if (chance >= total) return true;

			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			var randomValue = QuantumMaster.Random.Next(0, total);
			DebugLog.Info($"randomValue = {randomValue}");

			if (luck > 0)
			{
				// 提高成功概率
				var adjustedChance = Math.Min(total, chance + (total - chance) * luck);
				return randomValue < adjustedChance;
			}
			if (luck < 0)
			{
				// 降低成功概率
				var adjustedChance = Math.Max(0, chance * (1 + luck));
				return randomValue < adjustedChance;
			}
			return randomValue < chance;
		}

		public static bool Random_CheckProb_True(IRandomSource randomSource, int chance, int total)
		{
			var result = Calc_Random_CheckProb_True_By_Luck(randomSource, chance, total);
			DebugLog.Info($"Random_CheckProb_True chance {chance} total {total} result {result}");
			return result;
		}

		public static bool Calc_Random_CheckProb_False_By_Luck(IRandomSource randomSource, int chance, int total)
		{
			if (chance <= 0) return true;
			if (chance >= total) return false;

			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			var randomValue = QuantumMaster.Random.Next(0, total);
			DebugLog.Info($"randomValue = {randomValue}");

			if (luck > 0)
			{
				// 提高失败概率（降低成功概率）
				var adjustedChance = Math.Max(0, chance - chance * luck);
				return randomValue >= adjustedChance;
			}
			if (luck < 0)
			{
				// 降低失败概率（提高成功概率）
				var adjustedChance = Math.Min(total, chance + (total - chance) * (-luck));
				return randomValue >= adjustedChance;
			}
			return randomValue >= chance;
		}

		public static bool Random_CheckProb_False(IRandomSource randomSource, int chance, int total)
		{
			var result = Calc_Random_CheckProb_False_By_Luck(randomSource, chance, total);
			DebugLog.Info($"Random_CheckProb_False chance {chance} total {total} result {result}");
			return result;
		}

		public static int Calc_Random_Next_2Args_Max_By_Luck_Static(int min, int max)
		{
			var luck = QuantumMaster.LuckyLevelFactor[ConfigManager.LuckyLevel];
			// 先随机一个结果
			var randomValue = QuantumMaster.Random.Next(min, max);
			DebugLog.Info($"randomValue = {randomValue}");
			if (luck > 0)
			{
				// 获取的数量/概率 = Math.min(当前 + (最大-当前) * 因子, 最大)
				return (int)Math.Min(max - 1, randomValue + (max - 1 - randomValue) * luck);
			}
			if (luck < 0)
			{
				// 获取的数量/概率 = 最小 + (当前-最小) * (1 + 因子)
				return Math.Max(min, min + (int)((randomValue - min) * (1 + luck)));
			}
			return randomValue;
		}

		public static int Random_Calculate_Max(Config.Common.IConfigFormula type)
		{
			var result = 20;
			var buildingFormulaItem = type as Config.BuildingFormulaItem;
			if (buildingFormulaItem != null)
			{
				// 9 = 太吾村 资源等级
				if (buildingFormulaItem.TemplateId == 9)
				{
					// 1-5
					result = Calc_Random_Next_2Args_Max_By_Luck_Static(1, 6);
				}
				// 10 = 非太吾村资源等级
				else if (buildingFormulaItem.TemplateId == 10)
				{
					// 10-20
					result = Calc_Random_Next_2Args_Max_By_Luck_Static(10, 21);
				}
				// 其他的应该只有 8 = 太吾村杂草石头之类的等级
				else
				{
					// 1-20
					result = Calc_Random_Next_2Args_Max_By_Luck_Static(1, 21);
				}
			}
			
			DebugLog.Info($"Random_Calculate_5 result {result}");
			return result;
		}

	}
}