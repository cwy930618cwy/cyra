# 01 - GameplayAbilities（GAS 技能系统）

> 路径：`c:\Program Files\Epic Games\UE_5.6\Engine\Plugins\Runtime\GameplayAbilities\`
> 规模：**621 文件**（437 .h + 161 .cpp）—— Runtime 里最大的单个游戏逻辑插件
> Lyra 使用度：⭐⭐⭐ **核心中的核心**

---

## 一、这是什么？

**GAS (Gameplay Ability System)** 是 UE 官方的**技能/属性/效果框架**，专门用于：
- RPG 技能系统
- MOBA 英雄技能
- 射击游戏武器/道具
- 任何需要"属性 + 技能 + Buff/Debuff"的游戏

**Lyra 用它实现了**：跳跃、冲刺、近战、远程射击、生命值、伤害计算……几乎所有战斗相关逻辑。

---

## 二、四大核心概念（必须记住）

### 2.1 ASC — AbilitySystemComponent（能力系统组件）
**技能的"管理器"**，挂在 Actor 上（Lyra 挂在 PlayerState）。

```cpp
UAbilitySystemComponent* ASC = GetASC();
ASC->TryActivateAbility(AbilityHandle);   // 激活技能
ASC->ApplyGameplayEffect(GEClass, ...);   // 施加效果
ASC->GetGameplayAttributeValue(HealthAttr); // 读取属性
```

### 2.2 GA — GameplayAbility（技能）
**一个可激活的能力**。继承 `UGameplayAbility`。

```cpp
// Lyra 里的例子
ULyraGameplayAbility_Jump      // 跳跃
ULyraGameplayAbility_Dash      // 冲刺
ULyraGameplayAbility_Melee     // 近战
ULyraGameplayAbility_RangedWeapon // 远程射击
```

每个 GA 有：
- **ActivationPolicy** — 何时激活（OnInputTriggered / OnGivenTrigger）
- **ActivationGroup** — 激活组（Independent / Exclusive_Replaceable）
- **Cost** — 消耗（GameplayEffect）
- **Cooldown** — 冷却（GameplayEffect）

### 2.3 GE — GameplayEffect（效果）
**对属性的修改器**，分三类：
- **Instantaneous** — 瞬时（立即扣血）
- **Duration** — 持续（中毒 10 秒）
- **Infinite** — 无限（装备加成）

```cpp
// Lyra 里的 GE
GE_LyraDamage      // 伤害
GE_LyraHeal        // 治疗
GE_LyraCooldown    // 冷却
GE_LyraCost        // 消耗
```

### 2.4 AttributeSet — 属性集
**一组数值属性**。继承 `UAttributeSet`。

```cpp
// Lyra 里的两个属性集
ULyraHealthSet     // 生命值、最大生命值、死亡状态
ULyraCombatSet     // 攻击力、防御力、暴击率
```

属性用 `FGameplayAttributeData` 包装，支持：
- 基础值 (BaseValue)
- 当前值 (CurrentValue)
- 网络复制
- 变化回调 (PostGameplayEffectExecute)

---

## 三、目录结构

```
GameplayAbilities/
├── Source/
│   ├── GameplayAbilities/       ← 主模块（C++ 核心）
│   │   ├── Public/
│   │   │   ├── AbilitySystemComponent.h      ← ASC
│   │   │   ├── GameplayAbility.h             ← GA 基类
│   │   │   ├── GameplayEffect.h              ← GE
│   │   │   ├── GameplayEffectTypes.h         ← GE 类型
│   │   │   ├── AttributeSet.h                ← 属性集基类
│   │   │   ├── GameplayTagContainer.h        ← Tag 容器
│   │   │   └── ...
│   │   └── Private/
│   ├── GameplayAbilitiesEditor/ ← 编辑器扩展
│   └── GameplayAbilityTests/    ← 测试
├── Content/                     ← 蓝图资产
└── GameplayAbilities.uplugin
```

---

## 四、关键设计思想

### 4.1 GameplayTag 驱动一切
每个技能/效果/属性都用 **GameplayTag** 标识：

```cpp
// 技能标签
Ability.Attack.Melee
Ability.Attack.Ranged
Ability.Movement.Jump

// 状态标签
State.Debuff.Stun
State.Buff.Shield

// 事件标签
Event.Damage.Dealt
Event.Health.Death
```

Tag 用于：
- 技能的激活条件（需要/阻止某些 Tag）
- 效果的过滤（只对特定 Tag 生效）
- 动画/特效的触发（GameplayCue）

### 4.2 预测系统 (Prediction)
GAS 内置**客户端预测**，让技能在服务器确认前就能本地表现：

```cpp
FPredictionKey PredictionKey;
ASC->TryActivateAbilityByClass(AbilityClass, true, &PredictionKey);
```

这对射击/动作游戏至关重要（否则会有明显延迟感）。

### 4.3 GameplayCue（表现层）
把"逻辑"和"表现"分离：
- **GA/GE** 负责逻辑（扣血、加 Buff）
- **GameplayCue** 负责表现（特效、音效、震动）

```cpp
// 触发一个 Cue（如击中爆炸特效）
ASC->ExecuteGameplayCue(HitCueTag, params);
```

### 4.4 ExecutionCalculation（伤害公式）
自定义伤害计算逻辑：

```cpp
// Lyra 的 LyraDamageExecution
// 读取攻击力、防御力、暴击率 → 算出最终伤害
class ULyraDamageExecution : public UGameplayEffectExecutionCalculation;
```

---

## 五、Lyra 中的用法示例

### 5.1 生命值组件如何与 GAS 交互

```cpp
// ULyraHealthComponent 监听 GAS 属性变化
void ULyraHealthComponent::PostInitializeComponents()
{
    // 拿到 ASC 的生命值属性
    HealthAttribute = ULyraAttributeSet::GetHealthAttribute();
    
    // 监听变化
    ASC->GetGameplayAttributeValueChangeDelegate(HealthAttribute)
       ->AddUObject(this, &ThisClass::HandleHealthChanged);
}

void ULyraHealthComponent::HandleHealthChanged(const FOnAttributeChangeData& Data)
{
    // 血量变化 → 更新 UI / 判断死亡
    if (NewValue <= 0) { OnDeath(); }
}
```

### 5.2 技能激活流程

```
1. 玩家按攻击键
2. InputConfig 映射到 AbilityInputTag
3. ASC 收到输入 → 查找对应 GA
4. CanActivateAbility? (检查冷却/消耗/Tag)
5. ActivateAbility → 播放动画/Montage
6. AnimNotify 触发 → 生成子弹/判定伤害
7. ApplyGameplayEffect → 扣血
8. GameplayCue → 播放特效/音效
9. CommitAbility → 扣除消耗、进入冷却
```

---

## 六、学习建议

1. **先理解四大概念** — ASC/GA/GE/AttributeSet 的关系
2. **看懂 LyraHealthSet** — 最简单的属性集示例
3. **跟踪一个完整技能** — 如 LyraGameplayAbility_Jump
4. **动手实践** — 跟着教程做一个新 GA（如护盾）

## 七、下一步

- [02_CommonUI与UMG](./02_CommonUI与UMG.md) — UI 框架
- [03_ModularGameplay组件化](./03_ModularGameplay组件化.md) — 角色组件化
- [00_插件体系总览](./00_插件体系总览.md) — 回到总览
