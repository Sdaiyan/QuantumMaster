using HarmonyLib;
using GameData.Domains;
using GameData.Utilities;
using TaiwuModdingLib.Core.Plugin;
using Redzen.Random;
using System.Reflection;

namespace QuantumMaster
{

    [PluginConfig("QuantumMaster", "dai", "0.0.1")]
    public class QuantumMaster : TaiwuRemakeHarmonyPlugin
    {
        Harmony harmony;
        PatchBuilder patchBuilder;
        public static bool debug = true;
        public static bool openAll = true; // 是否开启所有补丁
        public static bool CreateBuildingArea; // 生成世界时，产业中的建筑和资源点的初始等级，以及生成数量
        public static bool CalcNeigongLoopingEffect; // 周天运转时，获得的内力为浮动区间的最大值，内息恢复最大，内息紊乱最小
        public static bool collectResource; // 收获资源时必定获取引子，且是可能获取的最高级的引子

        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "collectResource", ref collectResource);
            DomainManager.Mod.GetSetting(ModIdStr, "CreateBuildingArea", ref CreateBuildingArea);
            DomainManager.Mod.GetSetting(ModIdStr, "CalcNeigongLoopingEffect", ref CalcNeigongLoopingEffect);
        }
        public override void Initialize()
        {
            harmony = new Harmony("daige");
            int appliedPatches = 0;
            int skippedPatches = 0;

            // 自动调用所有以 patch 开头的方法
            var patchMethods = this.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(method => method.Name.StartsWith("patch", StringComparison.OrdinalIgnoreCase) &&
                       method.GetParameters().Length == 0 &&
                       (method.ReturnType == typeof(bool) || method.ReturnType == typeof(void)))
                .ToList();

            DebugLog.Info($"发现 {patchMethods.Count} 个补丁方法");

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

        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
                harmony = null;
            }
        }


        // 建筑生成时的数量/等级
        public bool patchCreateBuildingArea()
        {
            if (!CreateBuildingArea && !openAll) return false;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "CreateBuildingArea",
                // DataContext context, short mapAreaId, short mapBlockId, short mapBlockTemplateId
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(short), typeof(short), typeof(short) }
            };

            patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "CreateBuildingArea",
                OriginalMethod);

            // 2参数 NEXT 方法替换
            // 1 sbyte level = (sbyte)Math.Max(random.Next(maxLevel / 2, maxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                1);

            // 2 AddElement_BuildingBlocks(tempKey, new BuildingBlockData(blockId, buildingBlock.TemplateId, (sbyte)random.Next(1, buildingBlock.MaxLevel), -1), context);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                2);

            // 3 AddBuilding(context, mapAreaId, mapBlockId, blockId, buildingBlock.TemplateId, (sbyte)random.Next(1, buildingBlock.MaxLevel), areaWidth);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                3);

            // 4 blockIndex = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 5 level = (sbyte)Math.Clamp(random.Next(maxLevel / 2, maxLevel + 1), 1, currBuildingCfg.MaxLevel);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                5);

            // 6 level = (sbyte)Math.Max(random.Next(maxLevel / 2, maxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                6);

            // 7 int isBuild = random.Next(0, 2);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                7);

            // 8 blockIndex3 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 9 level = (sbyte)Math.Max(random.Next(maxLevel / 2, maxLevel + 1), 1);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                9);

            // 10 short blockIndex4 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 11 AddElement_BuildingBlocks(new BuildingBlockKey(mapAreaId, mapBlockId, blockIndex4), new BuildingBlockData(blockIndex4, buildingId3, (sbyte)random.Next(1, 11), -1), context);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                11);

            // 12 short blockIndex5 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 13 AddElement_BuildingBlocks(new BuildingBlockKey(mapAreaId, mapBlockId, blockIndex5), new BuildingBlockData(blockIndex5, buildingId4, (sbyte)random.Next(1, 6), -1), context);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                13);

            // 14 short blockIndex6 = canUseBlockList[random.Next(0, canUseBlockList.Count)];

            // 15 AddElement_BuildingBlocks(new BuildingBlockKey(mapAreaId, mapBlockId, blockIndex6), new BuildingBlockData(blockIndex6, buildingId5, (sbyte)random.Next(1, 16), -1), context);
            patchBuilder.AddInstanceMethodReplacement(
                PatchPresets.InstanceMethods.Next2Args,
                PatchPresets.Replacements.Next2ArgsMax,
                15);

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