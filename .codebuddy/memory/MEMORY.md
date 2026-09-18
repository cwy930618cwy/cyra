# 长期记忆（MEMORY）

## 工作区配置（2026-09-08）

用户的工作区 `cyra_company.code-workspace`（位于 `e:\ue5\cyra\workspace\company\`）包含三个根目录：

| 名称 | 路径 | 说明 |
|------|------|------|
| code (UE 项目) | `E:/ue5/cyra/code` | 用户的 UE 项目代码 |
| UE5.6 引擎 | `C:/Program Files/Epic Games/UE_5.6` | 引擎源码与插件 |
| （附加）Lyra 示例项目 | `e:\ue5\LyraStarterGame5.6\LyraStarterGame` | Lyra  starter 项目，用于学习参考 |

### 关键约定
- **引擎版本**：UE 5.6，安装在 `C:\Program Files\Epic Games\UE_5.6`。
- **学习笔记位置**：`e:\ue5\cyra\AI_Learning_Notes\`（按主题分目录，如 `Lyra_项目分析\06_Lyra功能总览\`）。
- 用户在学 Lyra / Modular Gameplay / GAS 等 UE 核心概念，偏好"新建 md + 画图 + 类比 + 源码证据"的讲解方式。

### 常用路径速查
- Lyra 项目根：`e:\ue5\LyraStarterGame5.6\LyraStarterGame`
- Lyra 插件目录：`e:\ue5\LyraStarterGame5.6\LyraStarterGame\Plugins\`
- Lyra 源码：`e:\ue5\LyraStarterGame5.6\LyraStarterGame\Source\LyraGame\`
- 引擎插件：`C:\Program Files\Epic Games\UE_5.6\Engine\Plugins\Runtime\`

## Lyra 启动流程"人物谱"体系（2026-09-14）

用户正在用**拟人化角色**讲 Lyra 启动/运行时全流程，笔记在 `e:\ue5\cyra\AI_Learning_Notes\人物介绍\`。世界观="游戏剧院"，每局游戏=一场演出。已建 3 篇，形成完整四层：

| 文件 | 层次 | 核心角色 |
|------|------|---------|
| `01_Lyra启动流程人物谱.md` | 幕后建造层 + 台上演出层 | 幕后：🏛️立项书(.uproject)/📐施工总工(Target.cs)/🔧装修队(LyraEditor)/🎭主演出团(LyraGame)/📖运营手册(DefaultEngine.ini)；台上：👔葛总管(GameInstance)/📜剧本小经(Experience)/📋场记小流(ExperienceManagerComponent)/🎪节目团(GameFeature)/🔌插座老王(ModularGameplay)/📦仓管老李(AssetManager)/🎬导演老规(GameMode) |
| `02_开演后_玩家与角色人物谱.md` | 开演后·静态结构 | 🎮观众本人(LocalPlayer)/🎯遥控器(PlayerController)/🎭场上演员(Pawn→Character→CharacterWithAbilities)/🧩装配组长(PawnExtensionComponent)/🎮操作台(HeroComponent)/🏃身法替身(MovementComponent)/🩺随队医生(HealthComponent)/📊记分牌(PlayerState,保管ASC)/📋角色设计稿(PawnData) |
| `03_动起来后_运行时链路人物谱.md` | 动起来后·运行时链路 | 🧠技能总管(ASC)/⚡技能(GameplayAbility)/💣武器实例(RangedWeaponInstance)/💥伤害效果(GE+DamageExecution)/📉血量属性(HealthSet)/💀死神(DeathAbility)/🏁比赛记录员(GameState)/🔄重生调度员(GameMode重生) |
| `04_技能配置长啥样_举例详解.md` | 配置层·数据驱动 | 用"突击步枪开火"讲配置五层套娃：PawnData→AbilitySet→GameplayAbility→WeaponInstance→GameplayEffect |
| `05_各资产配置项速查手册.md` | 配置层·字段字典 | 逐个列出 6 大资产(ULyraPawnData/AbilitySet/GameplayAbility/InputConfig/RangedWeaponInstance/TagRelationshipMapping)的每个 UPROPERTY 字段+类型+含义，均基于真实头文件 |
| `06_为什么技能是蓝图_事件图表在干嘛.md` | 蓝图层·理论 | 核心：C++写框架/骨架，蓝图写具体演法/血肉。C++留 K2_xxx 蓝图钩子；事件图表用 Ability Task 串流程；表现(Cue)与逻辑(Effect)分离 |
| `07_实战_拆解真实的GA_Weapon_Fire蓝图.md` | 蓝图层·实战 | 逐节点拆解真实开火蓝图(从编辑器复制节点文本得来)：激活→本地玩家射线检测+播动画→取命中→命中则播Cue特效+服务器HasAuthority算GE伤害→定时器连发 |

### 关键结论（讲 Lyra 时复用）
- **读蓝图的方法**：蓝图是 .uasset 二进制无法直接读；可在编辑器事件图表 Ctrl+A 全选→Ctrl+C→粘到 txt，文本里每个 `Begin Object`=一个节点、`LinkedTo`=连线、`MemberName`/`VariableReference`=函数/变量名，据此可还原流程图。
- **GA_Weapon_Fire 继承链**：ULyraGameplayAbility(C++)→ULyraGameplayAbility_RangedWeapon(远程基类,提供 StartRangedWeaponTargeting)→GA_Weapon_Fire(蓝图)。
- **开火蓝图套路**：几个 Ability Task(PlayMontageAndWait等异步) + 判断(IsLocallyControlled/HasAuthority/命中Branch) + 两个关键动作(ExecuteGameplayCue播表现、ApplyGameplayEffectToTarget算伤害)。
- **运行时链路**：操作台→技能总管(ASC)→技能→武器射线→伤害GE→血量扣减→(归0)死神→重生调度员→比赛记录员播报→闭环。
- **万物皆技能**：开火/跳跃/死亡/重生全是 GameplayAbility，ASC 统一调度。
- **ASC 挂在 PlayerState 上**（非 Character）：身体可换，技能/血量属性不丢。
- **改属性必须走 GameplayEffect**：HealthSet.Health 被 HideFromModifiers 保护，只有 DamageExecution 能改。
- **死亡是事件驱动技能**：血量归0→OnOutOfHealth→发 GameplayEvent.Death→死神技能自动激活。
