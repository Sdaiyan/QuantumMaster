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
    
    // 修改为必传参数 - 不再使用可空类型
    public int TargetOccurrence { get; set; } // 必须指定具体的出现次数
    
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
            DebugLog.Info($"方法 {original.DeclaringType.Name}.{original.Name} 已经被处理过，跳过重复处理");
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
                DebugLog.Info($"开始处理方法 {original.DeclaringType.Name}.{original.Name}");
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
        int totalReplacements = 0;
        
        // 第一阶段：准备数据 - 获取所有目标方法
        var targetMethods = new Dictionary<MethodInfo, List<MethodCallReplacement>>();
        
        foreach (var replacement in patchDef.Replacements)
        {
            // 获取目标方法
            MethodInfo targetMethod = null;
            
            // 使用 AccessTools 查找方法，它能处理所有访问级别包括 internal
            targetMethod = AccessTools.Method(
                replacement.TargetMethodDeclaringType,
                replacement.TargetMethodName,
                replacement.TargetMethodParameters);
    
            // 如果找不到方法，记录日志并跳过
            if (targetMethod == null)
            {
                DebugLog.Info($"找不到目标方法 {replacement.TargetMethodDeclaringType.Name}.{replacement.TargetMethodName}，参数: {string.Join(", ", replacement.TargetMethodParameters.Select(p => p?.Name ?? "null"))}");
                
                // 输出类型中所有可能的方法
                DebugLog.Info($"可用方法列表:");
                var allMethods = AccessTools.GetDeclaredMethods(replacement.TargetMethodDeclaringType);
                foreach (var method in allMethods)
                {
                    DebugLog.Info($"  - {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
                }
                continue;
            }
            
            // 将替换添加到目标方法的列表中
            if (!targetMethods.ContainsKey(targetMethod))
            {
                targetMethods[targetMethod] = new List<MethodCallReplacement>();
            }
            targetMethods[targetMethod].Add(replacement);
        }
        
        DebugLog.Info($"找到 {targetMethods.Count} 个需要处理的目标方法");
        
        // 第二阶段：扫描代码，找出所有目标方法调用的位置
        var methodCalls = new Dictionary<MethodInfo, List<(int Index, int Occurrence)>>();
        
        foreach (var entry in targetMethods)
        {
            var targetMethod = entry.Key;
            methodCalls[targetMethod] = new List<(int, int)>();
            
            // 扫描代码中的所有调用
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(targetMethod))
                {
                    int occurrence = methodCalls[targetMethod].Count + 1;
                    methodCalls[targetMethod].Add((i, occurrence));
                    
                    // 记录日志
                    int prevInstr = Math.Max(0, i-3);
                    int nextInstr = Math.Min(codes.Count-1, i+3);
                    DebugLog.Info($"找到第 {occurrence} 次 {targetMethod.DeclaringType.Name}.{targetMethod.Name} 调用，" +
                        $"位置: {i}, 上下文: {codes[prevInstr].opcode} ... {codes[i].opcode} ... {codes[nextInstr].opcode}");
                }
            }
            
            DebugLog.Info($"方法 {targetMethod.DeclaringType.Name}.{targetMethod.Name} 共有 {methodCalls[targetMethod].Count} 处调用");
        }
        
        // 第三阶段：确定需要替换的位置和替换方法
        var replacements = new List<(int Index, MethodInfo ReplacementMethod)>();
        int expectedReplacements = 0; // 跟踪预期的替换数量
        bool hasFailedReplacement = false; // 标记是否有替换失败

        foreach (var methodEntry in methodCalls)
        {
            var targetMethod = methodEntry.Key;
            var callPositions = methodEntry.Value;
            var replacementList = targetMethods[targetMethod];
            
            // 计算这个方法预期的替换数量
            foreach (var replacement in replacementList)
            {
                // 每个替换规则都必须指定目标出现次数
                expectedReplacements++;
            }
            
            foreach (var position in callPositions)
            {
                int index = position.Index;
                int occurrence = position.Occurrence;
                
                // 找到所有可能的替换
                var applicableReplacements = replacementList
                    .Where(r => r.TargetOccurrence == occurrence)
                    .ToList();
                
                if (applicableReplacements.Count == 0)
                {
                    DebugLog.Info($"第 {occurrence} 次出现的 {targetMethod.Name} 调用没有被要求替换，跳过");
                    continue;
                }
                // 尝试查找满足参数条件的替换
                var matchingReplacement = FindMatchingReplacement(codes, index, applicableReplacements);
                
                if (matchingReplacement != null)
                {
                    // 获取替换方法 - 使用AccessTools以支持所有访问级别
                    MethodInfo replacementMethod = AccessTools.Method(
                        matchingReplacement.ReplacementMethodDeclaringType,
                        matchingReplacement.ReplacementMethodName);
                        
                    if (replacementMethod == null)
                    {
                        DebugLog.Warning($"找不到替换方法 {matchingReplacement.ReplacementMethodDeclaringType.Name}.{matchingReplacement.ReplacementMethodName}");
                        hasFailedReplacement = true;
                        continue;
                    }
                    
                    replacements.Add((index, replacementMethod));
                    DebugLog.Info($"将替换 {targetMethod.Name}({string.Join(", ", targetMethod.GetParameters().Select(p => p.ParameterType.Name))}) 在位置 {index} 的第 {occurrence} 次调用，" +
                        $"替换方法: {replacementMethod.DeclaringType.Name}.{replacementMethod.Name}({string.Join(", ", replacementMethod.GetParameters().Select(p => p.ParameterType.Name))})");
                }
                else
                {
                    // 有适用的替换规则但没有匹配的参数条件
                    DebugLog.Warning($"第 {occurrence} 次出现的 {targetMethod.Name} 调用有替换规则，但参数条件不匹配");
                    hasFailedReplacement = true;
                }
            }
        }
        
        // 如果有任何替换失败，放弃所有替换
        if (hasFailedReplacement)
        {
            DebugLog.Warning($"存在替换失败的情况，预期替换 {expectedReplacements} 个方法调用，但有匹配失败。放弃所有替换！");
            return instructions; // 返回原始指令，不进行任何替换
        }

        // 如果预期替换数量与实际找到的不符
        if (expectedReplacements != replacements.Count)
        {
            DebugLog.Warning($"替换数量不匹配：预期替换 {expectedReplacements} 个方法调用，找到 {replacements.Count} 个匹配。放弃所有替换！");
            return instructions; // 返回原始指令，不进行任何替换
        }

        // 所有替换都成功匹配，可以继续执行替换操作
        DebugLog.Info($"成功匹配 {replacements.Count} 个替换方法调用，继续执行替换操作");
        
        // 第四阶段：执行替换
        // 按索引倒序排列，从后向前替换，避免索引变化
        replacements.Sort((a, b) => b.Index.CompareTo(a.Index));
        
        foreach (var (index, replacementMethod) in replacements)
        {
            if (index < 0 || index >= codes.Count)
            {
                DebugLog.Warning($"替换位置 {index} 超出范围 [0, {codes.Count - 1}]，跳过");
                continue;
            }
            
            codes[index] = new CodeInstruction(OpCodes.Call, replacementMethod);
            totalReplacements++;
            DebugLog.Info($"[{totalReplacements}/{replacements.Count}]替换位置 {index} 的方法调用为 {replacementMethod.DeclaringType.Name}.{replacementMethod.Name}");
        }
        
        DebugLog.Info($"已对 {patchDef.OriginalType.Name}.{patchDef.OriginalMethodName} 执行了 {totalReplacements} 次方法替换");
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

    // 辅助方法：查找满足参数条件的替换
    private static MethodCallReplacement FindMatchingReplacement(List<CodeInstruction> codes, int index, List<MethodCallReplacement> replacements)
    {
        foreach (var replacement in replacements)
        {
            bool isMatch = true;
            
            // 检查参数条件
            if (replacement.ArgumentConditions.Count > 0)
            {
                foreach (var condition in replacement.ArgumentConditions)
                {
                    int argIndex = index - (replacement.TargetMethodParameters.Length - condition.ArgumentIndex);
                    
                    if (argIndex < 0 || argIndex >= codes.Count)
                    {
                        DebugLog.Info($"参数索引 {argIndex} 超出范围 [0, {codes.Count - 1}]，不匹配");
                        isMatch = false;
                        break;
                    }
                    
                    bool conditionMet = LoadsConstant(codes[argIndex], condition.ExpectedValue);
                    DebugLog.Info($"条件检查: 参数[{condition.ArgumentIndex}]={condition.ExpectedValue}, " +
                        $"指令: {codes[argIndex].opcode}, 结果: {conditionMet}");
                        
                    if (!conditionMet)
                    {
                        isMatch = false;
                        break;
                    }
                }
            }
            
            if (isMatch)
            {
                return replacement;
            }
        }
        
        return null;
    }
}
