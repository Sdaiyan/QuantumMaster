using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TaiwuModdingLib.Core.Plugin;
using QuantumMaster;  // 引用新的命名空间

// Define a structure to hold patch information
public class TranspilerPatchDefinition
{
    public Type OriginalType { get; set; }
    public string OriginalMethodName { get; set; }
    public Type[] OriginalMethodParameters { get; set; }
    
    public List<MethodCallReplacement> Replacements { get; set; } = new List<MethodCallReplacement>();
}

public class MethodCallReplacement
{
    public Type TargetMethodDeclaringType { get; set; }
    public string TargetMethodName { get; set; }
    public Type[] TargetMethodParameters { get; set; }
    
    public Type ReplacementMethodDeclaringType { get; set; }
    public string ReplacementMethodName { get; set; }
    
    // New property to target specific occurrences
    public int? TargetOccurrence { get; set; } = null; // null means replace all occurrences
    
    // Add support for argument conditions
    public List<ArgumentCondition> ArgumentConditions { get; set; } = new List<ArgumentCondition>();
}

public class ArgumentCondition
{
    public int ArgumentIndex { get; set; }
    public object ExpectedValue { get; set; }
}

// Universal transpiler generator
public static class GenericTranspiler
{
    private static Dictionary<string, TranspilerPatchDefinition> _patchDefinitions =
        new Dictionary<string, TranspilerPatchDefinition>();

    // Register a patch definition
    public static void RegisterPatch(string patchId, TranspilerPatchDefinition definition)
    {
        _patchDefinitions[patchId] = definition;
    }

    // Apply all registered patches
    public static void ApplyPatches(Harmony harmony)
    {
        foreach (var patchDef in _patchDefinitions.Values)
        {
            var originalMethod = patchDef.OriginalType.GetMethod(
                patchDef.OriginalMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                null,
                patchDef.OriginalMethodParameters,
                null);

            if (originalMethod == null)
            {
                continue;
            }

            harmony.Patch(
                originalMethod,
                transpiler: new HarmonyMethod(typeof(GenericTranspiler),
                    nameof(GenerateTranspilerAdapter)));
        }
    }

    // Adapter method to find the correct patch definition
    public static IEnumerable<CodeInstruction> GenerateTranspilerAdapter(
        IEnumerable<CodeInstruction> instructions,
        MethodBase original)
    {
        // Find the matching patch definition for this method
        foreach (var patchDef in _patchDefinitions.Values)
        {
            if (patchDef.OriginalType == original.DeclaringType &&
                patchDef.OriginalMethodName == original.Name)
            {
                return GenerateTranspiler(instructions, patchDef);
            }
        }

        // No matching patch found, return original instructions
        return instructions;
    }

    // Generate a transpiler for a given patch definition
    public static IEnumerable<CodeInstruction> GenerateTranspiler(
        IEnumerable<CodeInstruction> instructions,
        TranspilerPatchDefinition patchDef)
    {
        var codes = new List<CodeInstruction>(instructions);

        foreach (var replacement in patchDef.Replacements)
        {
            // 使用正确的绑定标志获取方法 - 对于扩展方法需要特殊处理
            MethodInfo targetMethod = null;

            // 尝试查找静态方法（扩展方法）
            targetMethod = replacement.TargetMethodDeclaringType.GetMethod(
                replacement.TargetMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                replacement.TargetMethodParameters,
                null);

            // 如果找不到，记录详细信息
            if (targetMethod == null)
            {
                AdaptableLog.Info($"找不到目标方法 {replacement.TargetMethodDeclaringType.Name}.{replacement.TargetMethodName}");

                // 输出所有方法
                var methods = replacement.TargetMethodDeclaringType.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                foreach (var method in methods)
                {
                    AdaptableLog.Info($"静态方法: {method.Name}, 参数数量: {method.GetParameters().Length}, " +
                        $"参数类型: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}, " +
                        $"返回类型: {method.ReturnType.Name}");
                }
                continue; // 跳过当前替换
            }

            MethodInfo replacementMethod = replacement.ReplacementMethodDeclaringType.GetMethod(
                replacement.ReplacementMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            // 添加空值检查
            if (replacementMethod == null)
            {
                // 记录日志或抛出异常
                AdaptableLog.Info($"找不到替换方法 {replacement.ReplacementMethodDeclaringType.Name}.{replacement.ReplacementMethodName}");
                // 输出所有方法
                foreach (var method in replacement.ReplacementMethodDeclaringType.GetMethods())
                {
                    AdaptableLog.Info($"方法: {method.Name}, 参数数量: {method.GetParameters().Length}, 参数类型: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}, 返回类型: {method.ReturnType.Name}");
                }
                continue; // 跳过当前替换
            }

            // 跟踪出现次数，如果我们针对特定的一个
            int currentOccurrence = 0;

            for (int i = 0; i < codes.Count; i++)
            {
                // 确保方法不为空
                if (codes[i].Calls(targetMethod))
                {
                    currentOccurrence++;

                    bool shouldReplace = true;

                    // Check if we're targeting a specific occurrence
                    if (replacement.TargetOccurrence.HasValue &&
                        currentOccurrence != replacement.TargetOccurrence.Value)
                    {
                        shouldReplace = false;
                    }

                    // Check argument conditions if any
                    if (shouldReplace && replacement.ArgumentConditions.Count > 0)
                    {
                        foreach (var condition in replacement.ArgumentConditions)
                        {
                            int argIndex = i - (replacement.TargetMethodParameters.Length - condition.ArgumentIndex);
                            if (argIndex < 0 || !LoadsConstant(codes[argIndex], condition.ExpectedValue))
                            {
                                shouldReplace = false;
                                break;
                            }
                        }
                    }

                    if (shouldReplace)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, replacementMethod);
                    }
                }
            }
        }

        return codes;
    }

    // Helper method to check if an instruction loads a specific constant
    private static bool LoadsConstant(CodeInstruction instruction, object expectedValue)
    {
        // Check for constant loading instructions
        if (instruction.opcode == OpCodes.Ldc_I4 ||
            instruction.opcode == OpCodes.Ldc_I4_S ||
            instruction.opcode == OpCodes.Ldc_I8 ||
            instruction.opcode == OpCodes.Ldc_R4 ||
            instruction.opcode == OpCodes.Ldc_R8 ||
            instruction.opcode == OpCodes.Ldstr)
        {
            if (instruction.operand == null && expectedValue == null)
                return true;

            if (instruction.operand != null && instruction.operand.Equals(expectedValue))
                return true;
        }

        // Handle fixed int constants
        if (instruction.opcode == OpCodes.Ldc_I4_0) return Convert.ToInt32(expectedValue) == 0;
        if (instruction.opcode == OpCodes.Ldc_I4_1) return Convert.ToInt32(expectedValue) == 1;
        if (instruction.opcode == OpCodes.Ldc_I4_2) return Convert.ToInt32(expectedValue) == 2;
        if (instruction.opcode == OpCodes.Ldc_I4_3) return Convert.ToInt32(expectedValue) == 3;
        if (instruction.opcode == OpCodes.Ldc_I4_4) return Convert.ToInt32(expectedValue) == 4;
        if (instruction.opcode == OpCodes.Ldc_I4_5) return Convert.ToInt32(expectedValue) == 5;
        if (instruction.opcode == OpCodes.Ldc_I4_6) return Convert.ToInt32(expectedValue) == 6;
        if (instruction.opcode == OpCodes.Ldc_I4_7) return Convert.ToInt32(expectedValue) == 7;
        if (instruction.opcode == OpCodes.Ldc_I4_8) return Convert.ToInt32(expectedValue) == 8;
        if (instruction.opcode == OpCodes.Ldc_I4_M1) return Convert.ToInt32(expectedValue) == -1;

        return false;
    }

    /// <summary>
    /// 创建一个补丁构建器
    /// </summary>
    public static PatchBuilder CreatePatchBuilder(
        string patchName,
        OriginalMethodInfo originalMethod)
    {
        return new PatchBuilder(patchName, originalMethod.Type, originalMethod.MethodName, originalMethod.Parameters);
    }

    // 为外部使用暴露补丁定义
    public static Dictionary<string, TranspilerPatchDefinition> GetPatchDefinitions()
    {
        return _patchDefinitions;
    }
}
