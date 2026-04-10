# ActionPatchBase API

`src/QuantumMaster/Features/Actions/ActionPatchBase.cs` 提供混合模式补丁的公共基础设施。

## 静态字段

```csharp
private static Character _currentCharacter;   // 发起行动的角色
private static Character _targetCharacter;     // 行动目标角色
```

## 方法

### SetCharacterContext

在 Prefix 中调用，存储角色引用到静态字段。

```csharp
public static void SetCharacterContext(
    Character currentChar,    // __instance（发起行动的角色）
    Character targetChar,     // targetChar 参数（行动目标）
    string featureKey)        // 功能键，用于日志
```

### ClearCharacterContext

在 Postfix 中调用，清理静态字段。

```csharp
public static void ClearCharacterContext(string featureKey)
```

### CheckPercentProbWithStaticContext

替换方法中调用，自动根据角色身份选择不同的气运策略。

```csharp
public static bool CheckPercentProbWithStaticContext(
    IRandomSource random,
    int probability,
    string featureKey)
```

**内部逻辑**：
1. 读取 `_currentCharacter` 和 `_targetCharacter`
2. 获取太吾角色 ID
3. 条件判断：
   - `currentCharId == taiwuId` → 调用 `Calc_Random_CheckPercentProb_True_By_Luck`（倾向成功）
   - `targetCharId == taiwuId` → 调用 `Calc_Random_CheckPercentProb_False_By_Luck`（倾向失败，保护太吾）
   - 其他 → 使用原始 `CheckPercentProb`（不修改）

## 执行流程

```
方法调用 GetStealActionPhase(__instance, targetChar, ...)
  │
  ├─ [Prefix] SetCharacterContext(__instance, targetChar, "steal")
  │    └─ 存储到静态字段
  │
  ├─ 方法体执行（IL 已被 PatchBuilder 修改）
  │    ├─ 原: random.CheckPercentProb(prob)
  │    └─ 现: StealPatch.CheckPercentProbWithStaticContext(random, prob)
  │           └─ 委托给 ActionPatchBase.CheckPercentProbWithStaticContext
  │                └─ 读取静态字段判断太吾身份
  │
  └─ [Postfix] ClearCharacterContext("steal")
       └─ 清理静态字段
```

## 与 CombatPatchBase 的区别

| | ActionPatchBase (QM) | CombatPatchBase (CM) |
|---|---|---|
| 存储内容 | `Character` 对象（当前+目标） | `int` 角色 ID（仅当前） |
| 双向判断 | 是（太吾发起/太吾被攻击） | 否（仅判断是否太吾） |
| 获取方式 | `__instance` + 方法参数 | `__instance.CharacterId` |
