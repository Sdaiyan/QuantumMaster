using System;

namespace QuantumMaster.Shared
{
    /// <summary>
    /// 原始方法信息结构体
    /// </summary>
    public struct OriginalMethodInfo
    {
        public Type Type { get; set; }
        public string MethodName { get; set; }
        public Type[] Parameters { get; set; }
    }

    /// <summary>
    /// 扩展方法信息结构体
    /// </summary>
    public struct ExtensionMethodInfo
    {
        public Type Type { get; set; }
        public string MethodName { get; set; }
        public Type[] Parameters { get; set; }
    }

    /// <summary>
    /// 替换方法信息结构体
    /// </summary>
    public struct ReplacementMethodInfo
    {
        public Type Type { get; set; }
        public string MethodName { get; set; }
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
    /// 参数条件结构体
    /// </summary>
    public struct ArgumentCondition
    {
        public int ArgumentIndex { get; set; }
        public object ExpectedValue { get; set; }
    }

    
    /// <summary>
    /// 接口/实例方法信息结构体
    /// </summary>
    public struct InstanceMethodInfo
    {
        public Type Type { get; set; }
        public string MethodName { get; set; }
        public Type[] Parameters { get; set; }
    }

    /// <summary>
    /// 本地函数信息结构体
    /// </summary>
    public struct LocalFunctionInfo
    {
        /// <summary>
        /// 本地函数部分名称（用于匹配）
        /// </summary>
        public string PartialName { get; set; }
        /// <summary>
        /// 本地函数参数类型
        /// </summary>
        public Type[] Parameters { get; set; }
        /// <summary>
        /// 本地函数返回类型
        /// </summary>
        public Type ReturnType { get; set; }
    }
}