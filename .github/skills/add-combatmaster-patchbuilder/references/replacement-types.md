# 常见替换方法类型

根据原方法中使用的随机数函数，选择对应的 LuckyCalculator 方法和 PatchPresets 预设。

## 1. CheckPercentProb(int percent) — 百分比概率

**PatchPresets**: `PatchPresets.Extensions.CheckPercentProb`

```csharp
// 倾向成功（true）
public static bool CheckPercentProbTrue_Method(this IRandomSource randomSource, int percent)
{
    return LuckyCalculator.Calc_Random_CheckPercentProb_True_By_Luck(randomSource, percent, "featureKey");
}

// 倾向失败（false）
public static bool CheckPercentProbFalse_Method(this IRandomSource randomSource, int percent)
{
    return LuckyCalculator.Calc_Random_CheckPercentProb_False_By_Luck(randomSource, percent, "featureKey");
}
```

## 2. CheckProb(int chance, int total) — 概率检查

**PatchPresets**: `PatchPresets.Extensions.CheckProb`

```csharp
// 倾向成功
public static bool CheckProbTrue_Method(this IRandomSource randomSource, int chance, int total)
{
    return LuckyCalculator.Calc_Random_CheckProb_True_By_Luck(randomSource, chance, total, "featureKey");
}

// 倾向失败
public static bool CheckProbFalse_Method(this IRandomSource randomSource, int chance, int total)
{
    return LuckyCalculator.Calc_Random_CheckProb_False_By_Luck(randomSource, chance, total, "featureKey");
}
```

## 3. Next(int min, int max) — 两参数随机数

**PatchPresets**: `PatchPresets.InstanceMethods.Next2Args`

```csharp
// 倾向最大值
public static int Next2ArgsMax_Method(this IRandomSource randomSource, int min, int max)
{
    return LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(min, max, "featureKey");
}

// 倾向最小值
public static int Next2ArgsMin_Method(this IRandomSource randomSource, int min, int max)
{
    return LuckyCalculator.Calc_Random_Next_2Args_Min_By_Luck(min, max, "featureKey");
}
```

## 4. Next(int max) — 单参数随机数

**PatchPresets**: `PatchPresets.InstanceMethods.Next1Arg`

```csharp
// 倾向最大值
public static int Next1ArgMax_Method(this IRandomSource randomSource, int max)
{
    return LuckyCalculator.Calc_Random_Next_1Arg_Max_By_Luck(max, "featureKey");
}

// 倾向 0
public static int Next1Arg0_Method(this IRandomSource randomSource, int max)
{
    return LuckyCalculator.Calc_Random_Next_1Arg_0_By_Luck(max, "featureKey");
}
```

## 5. CheckProbability(int value) — 奇遇事件概率（静态方法）

**适用场景**：`GameData.Domains.TaiwuEvent.EventHelper.EventHelper.CheckProbability(int)` 调用，常见于奇遇事件处理逻辑。

**注意**：此方法是**纯静态方法**，无 `IRandomSource` 参数，替换方法签名也只有 `int`。

**PatchPresets**: `PatchPresets.Extensions.CheckProbability`

```csharp
// 倾向成功（true）
public static bool CheckProbabilityTrue_Method(int percent)
{
    return LuckyCalculator.Calc_Random_CheckProbability_True_By_Luck(percent, "featureKey");
}

// 倾向失败（false）
public static bool CheckProbabilityFalse_Method(int percent)
{
    return LuckyCalculator.Calc_Random_CheckProbability_False_By_Luck(percent, "featureKey");
}
```

## 替换规则调用方式

```csharp
// 扩展方法（CheckPercentProb, CheckProb）
patchBuilder.AddExtensionMethodReplacement(
    PatchPresets.Extensions.CheckPercentProb,   // 目标方法预设
    Replacements.CheckPercentProbTrue,           // 替换方法
    1);                                          // 替换第几次出现

// CheckProbability（静态方法，同样用 AddExtensionMethodReplacement）
patchBuilder.AddExtensionMethodReplacement(
    PatchPresets.Extensions.CheckProbability,    // 目标方法预设
    Replacements.CheckProbabilityTrue,           // 替换方法
    1);                                          // 替换第几次出现

// 接口/实例方法（Next）
patchBuilder.AddInstanceMethodReplacement(
    PatchPresets.InstanceMethods.Next2Args,       // 目标方法预设
    Replacements.Next2ArgsMax,                    // 替换方法
    1);                                           // 替换第几次出现
```

## 确定替换次数 (targetOccurrence)

替换次数指目标方法中该函数调用出现的第几次（从 1 开始）。需查看反编译代码确认。
