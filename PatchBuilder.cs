using HarmonyLib;
using GameData.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        Type extensionType,
        string extensionMethodName,
        Type[] extensionMethodParams,
        Type replacementType,
        string replacementMethodName,
        int? targetOccurrence = null)
    {
        // 检查扩展方法是否存在
        var extensionMethodInfo = extensionType.GetMethod(
            extensionMethodName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            null,
            extensionMethodParams,
            null);
            
        if (extensionMethodInfo == null)
        {
            AdaptableLog.Info($"找不到扩展方法 {extensionType.Name}.{extensionMethodName}");
            
            // 列出所有可能的方法
            var methods = extensionType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                AdaptableLog.Info($"{extensionType.Name} 方法: {method.Name}, 参数: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}");
            }
            return this;
        }
        
        AdaptableLog.Info($"找到扩展方法: {extensionMethodInfo.Name}");

        // 添加替换定义
        _patchDefinition.Replacements.Add(new MethodCallReplacement
        {
            TargetMethodDeclaringType = extensionType,
            TargetMethodName = extensionMethodName,
            TargetMethodParameters = extensionMethodParams,
            ReplacementMethodDeclaringType = replacementType,
            ReplacementMethodName = replacementMethodName,
            TargetOccurrence = targetOccurrence
        });

        return this;
    }

    /// <summary>
    /// 添加普通方法替换
    /// </summary>
    public PatchBuilder AddMethodReplacement(
        Type targetType,
        string targetMethodName,
        Type[] targetMethodParams,
        Type replacementType,
        string replacementMethodName,
        int? targetOccurrence = null)
    {
        // 添加替换定义
        _patchDefinition.Replacements.Add(new MethodCallReplacement
        {
            TargetMethodDeclaringType = targetType,
            TargetMethodName = targetMethodName,
            TargetMethodParameters = targetMethodParams,
            ReplacementMethodDeclaringType = replacementType,
            ReplacementMethodName = replacementMethodName,
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