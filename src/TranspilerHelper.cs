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

    private static Dictionary<string, TranspilerPatchDefinition> _methodKeyToPatchDef = 
        new Dictionary<string, TranspilerPatchDefinition>();

    // Register a patch definition
    public static void RegisterPatch(string patchId, TranspilerPatchDefinition definition)
    {
        _patchDefinitions[patchId] = definition;
        
        // 同时添加到方法查找字典
        string methodKey = $"{definition.OriginalType.FullName}.{definition.OriginalMethodName}";
        _methodKeyToPatchDef[methodKey] = definition;
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
    private static HashSet<MethodBase> _processedMethods = new HashSet<MethodBase>();

    public static IEnumerable<CodeInstruction> GenerateTranspilerAdapter(
        IEnumerable<CodeInstruction> instructions,
        MethodBase original)
    {
        // 检查这个方法是否已经被处理过
        if (_processedMethods.Contains(original))
        {
            AdaptableLog.Info($"方法 {original.DeclaringType.Name}.{original.Name} 已经被处理过，跳过重复处理");
            return instructions;
        }

        // 查找匹配的补丁定义
        foreach (var patchDef in _patchDefinitions.Values)
        {
            if (patchDef.OriginalType == original.DeclaringType &&
                patchDef.OriginalMethodName == original.Name)
            {
                // 标记为已处理
                _processedMethods.Add(original);
                AdaptableLog.Info($"开始处理方法 {original.DeclaringType.Name}.{original.Name}");
                return GenerateTranspiler(instructions, patchDef);
            }
        }

        // 没有找到匹配的补丁，返回原始指令
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
            // 使用正确的绑定标志获取方法
            MethodInfo targetMethod = null;

            // 1. 首先尝试查找静态方法（扩展方法）
            targetMethod = replacement.TargetMethodDeclaringType.GetMethod(
                replacement.TargetMethodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                replacement.TargetMethodParameters,
                null);

            // 2. 如果找不到静态方法，尝试查找实例方法
            if (targetMethod == null)
            {
                targetMethod = replacement.TargetMethodDeclaringType.GetMethod(
                    replacement.TargetMethodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    replacement.TargetMethodParameters,
                    null);
            }

            // 如果仍然找不到，记录详细信息
            if (targetMethod == null)
            {
                AdaptableLog.Info($"找不到目标方法 {replacement.TargetMethodDeclaringType.Name}.{replacement.TargetMethodName}");

                // 输出所有方法
                var staticMethods = replacement.TargetMethodDeclaringType.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                
                AdaptableLog.Info("静态方法:");
                foreach (var method in staticMethods)
                {
                    AdaptableLog.Info($"  {method.Name}, 参数数量: {method.GetParameters().Length}, " +
                        $"参数类型: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}, " +
                        $"返回类型: {method.ReturnType.Name}");
                }
                
                var instanceMethods = replacement.TargetMethodDeclaringType.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                AdaptableLog.Info("实例方法:");
                foreach (var method in instanceMethods)
                {
                    AdaptableLog.Info($"  {method.Name}, 参数数量: {method.GetParameters().Length}, " +
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
                if (targetMethod != null && codes[i].Calls(targetMethod))
                {
                    currentOccurrence++;
                    
                    // 添加日志记录每次找到的方法调用
                    int prevInstr = Math.Max(0, i-3);
                    int nextInstr = Math.Min(codes.Count-1, i+3);
                    AdaptableLog.Info($"找到第 {currentOccurrence} 次 {targetMethod.Name} 调用，" +
                        $"位置: {i}, 上下文: {codes[prevInstr].opcode} ... {codes[i].opcode} ... {codes[nextInstr].opcode}");
                    
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
                            if (argIndex < 0)
                            {
                                AdaptableLog.Info($"参数索引 {argIndex} 超出范围，不替换");
                                shouldReplace = false;
                                break;
                            }
                            
                            bool conditionMet = LoadsConstant(codes[argIndex], condition.ExpectedValue);
                            AdaptableLog.Info($"条件检查: 参数[{condition.ArgumentIndex}]={condition.ExpectedValue}, " +
                                $"指令: {codes[argIndex].opcode}, 结果: {conditionMet}");
                                
                            if (!conditionMet)
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

        AdaptableLog.Info($"已应用 {patchDef.Replacements.Count} 个替换到 {patchDef.OriginalType.Name}.{patchDef.OriginalMethodName}");

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

    public static void ResetProcessedMethods()
    {
        _processedMethods.Clear();
    }
}
