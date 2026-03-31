# ClassPatch 常见错误

## 1. 重复补丁

❌ 创建多个类互相调用：
```csharp
public static class FeatureHelper { ... }
public class FeatureClassPatch { /* 调用 FeatureHelper */ }
```

✅ 只创建一个带 `[HarmonyPatch]` 的类：
```csharp
[HarmonyPatch(typeof(TargetClass), "MethodName")]
public class FeaturePatch { /* 所有逻辑在这里 */ }
```

## 2. 使用固定值而非原始值

❌ 固定范围：
```csharp
var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(0, 100, "Feature");
```

✅ 使用原始返回值作为范围起点：
```csharp
var newValue = LuckyCalculator.Calc_Random_Next_2Args_Max_By_Luck(__result, __result * 2, "Feature");
```

## 3. 遗漏太吾角色判断

❌ 不检查角色，所有角色受影响：
```csharp
[HarmonyPostfix]
public static void Postfix(ref int __result)
{
    __result = newValue;  // 所有角色都被修改
}
```

✅ 默认只对太吾生效：
```csharp
[HarmonyPostfix]
public static void Postfix(SomeClass __instance, ref int __result)
{
    var taiwuId = GameData.Domains.DomainManager.Taiwu.GetTaiwuCharId();
    if (__instance.CharacterId != taiwuId)
        return;
    // 处理逻辑...
}
```

## 4. 配置类型不一致

❌ Lua 中是 Dropdown，代码中用 bool：
```csharp
public static bool YourFeature = true;  // 错误！
```

✅ Dropdown 用 int，Toggle 用 bool：
```csharp
public static int YourFeature = 0;    // Dropdown
public static bool YourToggle = true; // Toggle
```
