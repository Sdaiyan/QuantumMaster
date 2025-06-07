using HarmonyLib;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuantumMaster;

/// <summary>
/// 补丁构建器 - 用于流畅地构建补丁
/// </summary>
public class PatchBuilder
{
    private string _patchName;
    private TranspilerPatchDefinition _patchDefinition;

    public PatchBuilder(string patchName, Type originalType, string originalMethodName, Type[] originalMethodParams)
    {
        _patchName = patchName;
        _patchDefinition = new TranspilerPatchDefinition
        {
            OriginalType = originalType,
            OriginalMethodName = originalMethodName,
            OriginalMethodParameters = originalMethodParams,
            Replacements = new List<MethodCallReplacement>()
        };
    }

    /// <summary>
    /// 添加扩展方法替换
    /// </summary>
    public PatchBuilder AddExtensionMethodReplacement(
        ExtensionMethodInfo extensionMethod,
        ReplacementMethodInfo replacementMethod,
        int targetOccurrence) // 必传参数
    {
        // 检查扩展方法是否存在
        var extensionMethodInfo = extensionMethod.Type.GetMethod(
            extensionMethod.MethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            null,
            extensionMethod.Parameters,
            null);
            
        if (extensionMethodInfo == null)
        {
            DebugLog.Info($"找不到扩展方法 {extensionMethod.Type.Name}.{extensionMethod.MethodName}");
            
            // 列出所有可能的方法
            var methods = extensionMethod.Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                DebugLog.Info($"{extensionMethod.Type.Name} 方法: {method.Name}, 参数: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}");
            }
            return this;
        }
        
        DebugLog.Info($"找到扩展方法: {extensionMethodInfo.Name}");

        // 添加替换定义
        _patchDefinition.Replacements.Add(new MethodCallReplacement
        {
            TargetMethodDeclaringType = extensionMethod.Type,
            TargetMethodName = extensionMethod.MethodName,
            TargetMethodParameters = extensionMethod.Parameters,
            ReplacementMethodDeclaringType = replacementMethod.Type,
            ReplacementMethodName = replacementMethod.MethodName,
            TargetOccurrence = targetOccurrence
        });

        return this;
    }

    /// <summary>
    /// 目标方法信息结构体
    /// </summary>
    public struct TargetMethodInfo
    {
        public Type Type { get; set; }
        public string MethodName { get; set; }
        public Type[] Parameters { get; set; }
    }

    /// <summary>
    /// 添加接口/实例方法替换
    /// </summary>
    public PatchBuilder AddInstanceMethodReplacement(
        InstanceMethodInfo instanceMethod,
        ReplacementMethodInfo replacementMethod,
        int targetOccurrence) // 必传参数
    {
        // 检查实例方法是否存在
        var instanceMethodInfo = instanceMethod.Type.GetMethod(
            instanceMethod.MethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            instanceMethod.Parameters,
            null);
            
        if (instanceMethodInfo == null)
        {
            DebugLog.Info($"找不到实例方法 {instanceMethod.Type.Name}.{instanceMethod.MethodName}");
            
            // 列出所有可能的方法
            var methods = instanceMethod.Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                DebugLog.Info($"{instanceMethod.Type.Name} 方法: {method.Name}, 参数: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}");
            }
            return this;
        }
        
        DebugLog.Info($"找到实例方法: {instanceMethodInfo.Name}");

        // 添加替换定义
        _patchDefinition.Replacements.Add(new MethodCallReplacement
        {
            TargetMethodDeclaringType = instanceMethod.Type,
            TargetMethodName = instanceMethod.MethodName,
            TargetMethodParameters = instanceMethod.Parameters,
            ReplacementMethodDeclaringType = replacementMethod.Type,
            ReplacementMethodName = replacementMethod.MethodName,
            TargetOccurrence = targetOccurrence
        });

        return this;
    }

    /// <summary>
    /// 添加本地函数替换
    /// </summary>
    /// <param name="localFunction">本地函数信息</param>
    /// <param name="replacementMethod">替换方法信息</param>
    /// <param name="targetOccurrence">目标出现次数</param>
    /// <returns>补丁构建器实例，支持链式调用</returns>
    public PatchBuilder AddLocalFunctionReplacement(
        LocalFunctionInfo localFunction,
        ReplacementMethodInfo replacementMethod,
        int targetOccurrence) // 必传参数
    {
        DebugLog.Info($"添加本地函数替换: 部分名称={localFunction.PartialName}, " +
            $"参数=[{string.Join(", ", localFunction.Parameters?.Select(p => p.Name) ?? new string[0])}], " +
            $"返回类型={localFunction.ReturnType?.Name ?? "未指定"}, " +
            $"替换方法={replacementMethod.Type.Name}.{replacementMethod.MethodName}, " +
            $"目标出现次数={targetOccurrence}");

        // 检查替换方法是否存在
        var replacementMethodInfo = replacementMethod.Type.GetMethod(
            replacementMethod.MethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            
        if (replacementMethodInfo == null)
        {
            DebugLog.Warning($"找不到替换方法 {replacementMethod.Type.Name}.{replacementMethod.MethodName}");
            
            // 列出所有可能的方法
            var methods = replacementMethod.Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            DebugLog.Info($"{replacementMethod.Type.Name} 可用方法列表:");
            foreach (var method in methods)
            {
                DebugLog.Info($"  - {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}) : {method.ReturnType.Name}");
            }
            return this;
        }
        
        DebugLog.Info($"找到替换方法: {replacementMethodInfo.Name}, 参数: [{string.Join(", ", replacementMethodInfo.GetParameters().Select(p => p.ParameterType.Name))}], " +
            $"返回类型: {replacementMethodInfo.ReturnType.Name}");

        // 添加本地函数替换定义
        _patchDefinition.Replacements.Add(new MethodCallReplacement
        {
            IsLocalFunction = true,
            LocalFunctionPartialName = localFunction.PartialName,
            TargetMethodParameters = localFunction.Parameters,
            LocalFunctionReturnType = localFunction.ReturnType,
            ReplacementMethodDeclaringType = replacementMethod.Type,
            ReplacementMethodName = replacementMethod.MethodName,
            TargetOccurrence = targetOccurrence
        });

        DebugLog.Info($"成功添加本地函数替换定义");
        return this;
    }

    /// <summary>
    /// 添加参数条件
    /// </summary>
    public PatchBuilder AddArgumentCondition(
        int replacementIndex,
        int argumentIndex,
        object expectedValue)
    {
        if (replacementIndex < 0 || replacementIndex >= _patchDefinition.Replacements.Count)
        {
            DebugLog.Info($"替换索引 {replacementIndex} 超出范围");
            return this;
        }
        
        _patchDefinition.Replacements[replacementIndex].ArgumentConditions.Add(
            new ArgumentCondition
            {
                ArgumentIndex = argumentIndex,
                ExpectedValue = expectedValue
            }
        );
        
        return this;
    }

    /// <summary>
    /// 应用补丁
    /// </summary>
    public void Apply(Harmony harmony)
    {
        GenericTranspiler.RegisterPatch(_patchName, _patchDefinition);
    }
    
    public void ApplyPatches(Harmony harmony)
    {
        GenericTranspiler.ApplyPatches(harmony);
    }
}