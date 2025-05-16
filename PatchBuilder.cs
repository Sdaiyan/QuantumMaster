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
        int? targetOccurrence = null)
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
            AdaptableLog.Info($"找不到扩展方法 {extensionMethod.Type.Name}.{extensionMethod.MethodName}");
            
            // 列出所有可能的方法
            var methods = extensionMethod.Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                AdaptableLog.Info($"{extensionMethod.Type.Name} 方法: {method.Name}, 参数: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}");
            }
            return this;
        }
        
        AdaptableLog.Info($"找到扩展方法: {extensionMethodInfo.Name}");

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
    /// 添加普通方法替换
    /// </summary>
    public PatchBuilder AddMethodReplacement(
        TargetMethodInfo targetMethod,
        ReplacementMethodInfo replacementMethod,
        int? targetOccurrence = null)
    {
        // 添加替换定义
        _patchDefinition.Replacements.Add(new MethodCallReplacement
        {
            TargetMethodDeclaringType = targetMethod.Type,
            TargetMethodName = targetMethod.MethodName,
            TargetMethodParameters = targetMethod.Parameters,
            ReplacementMethodDeclaringType = replacementMethod.Type,
            ReplacementMethodName = replacementMethod.MethodName,
            TargetOccurrence = targetOccurrence
        });

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
            AdaptableLog.Info($"替换索引 {replacementIndex} 超出范围");
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
        GenericTranspiler.ApplyPatches(harmony);
    }
}