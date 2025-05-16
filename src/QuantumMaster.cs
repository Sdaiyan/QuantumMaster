using HarmonyLib;
using GameData.Domains;
using GameData.Utilities;
using TaiwuModdingLib.Core.Plugin;
using Redzen.Random;

namespace QuantumMaster
{

    [PluginConfig("QuantumMaster", "dai", "0.0.1")]
    public class QuantumMaster : TaiwuRemakeHarmonyPlugin
    {
        Harmony harmony;

        public static bool BreakVisible;
        public static bool CreateBuildingArea;

        public override void OnModSettingUpdate()
        {
            DomainManager.Mod.GetSetting(ModIdStr, "steal", ref BreakVisible);
        }
        public override void Initialize()
        {
            harmony = new Harmony("daige");
            patchBreakVisible();
            patchCreateBuildingArea();
        }

        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
                harmony = null;
            }
        }

        // 突破时格子可见
        public void patchBreakVisible()
        {
            // if (!BreakVisible) return;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Taiwu.SkillBreakPlate),
                MethodName = "RandomGridData",
                Parameters = new Type[] { typeof(IRandomSource), typeof(sbyte) }
            };

            // 使用预设值创建补丁
            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
                "BreakVisible",
                OriginalMethod);

            // 添加扩展方法替换，使用预设值
            patchBuilder.AddExtensionMethodReplacement(
                PatchPresets.Extensions.CheckPercentProb,
                PatchPresets.Replacements.CheckPercentProbTrue,
                1); // 替换第1次出现

            // 应用补丁
            patchBuilder.Apply(harmony);
        }


        // 建筑生成时的数量/等级
        public void patchCreateBuildingArea()
        {
            // if (!CreateBuildingArea) return;

            var OriginalMethod = new OriginalMethodInfo
            {
                Type = typeof(GameData.Domains.Building.BuildingDomain),
                MethodName = "CreateBuildingArea",
                // DataContext context, short mapAreaId, short mapBlockId, short mapBlockTemplateId
                Parameters = new Type[] { typeof(GameData.Common.DataContext), typeof(short), typeof(short), typeof(short) }
            };

            var patchBuilder = GenericTranspiler.CreatePatchBuilder(
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
        }
    }

    public class RandomPath
    {
        public int Random_Next_2Args_Max(int min, int max)
        {
            AdaptableLog.Info($"Random_Next_2Args_Max min {min} max {max}");
            return Math.Max(min, max - 1);
        }

        public int Random_Next_2Args_Min(int min, int max)
        {
            AdaptableLog.Info($"Random_Next_2Args_Min min {min} max {max}");
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