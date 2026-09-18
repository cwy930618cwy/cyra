# 长期记忆（MEMORY）

## 【最高铁律】教学必须基于真实源码，禁止瞎想（2026-09-18）

教任何 Lyra / UE5 知识点前，**必须先到下面两个权威来源查真实代码/实现，再据此讲解**，绝不凭记忆或想象编造：
- 🎯 Lyra 示例项目源码：`e:\ue5\LyraStarterGame5.6\LyraStarterGame`（讲 Lyra 架构/写法必须引用 `Source/LyraGame/...`、`Plugins/...` 真实文件）
- 🎯 UE 5.6 引擎源码：`c:\Program Files\Epic Games\UE_5.6`（讲引擎机制/API 必须引用 `Engine/Source/...`、`Engine/Plugins/...` 真实实现）

执行要求：(1)先查后教，用工具搜索/读取真实文件确认写法；(2)引用到具体文件+函数+行，给真实代码证据；(3)两来源里找不到的，明确说"未找到源码依据"，不编"应该是这样"；(4)自己的类比/推断要显式标注"这是我的类比，非源码"。经验文件入口：`e:\ue5\cyra\AI_Learning_Notes\教学经验新对话必看\00_主要经验.md`（新对话教学前必读）。

### 【教学铁律】一比一还原 Lyra + 只教不写代码（2026-09-18）
本项目目标是**一比一还原 Lyra 真实写法**（不是参考思路自己发挥），每处做法都要在 `LyraStarterGame5.6` 源码找到一一对应实现，按 Lyra 的类结构/命名/流程讲。**我只负责教（讲概念+对照源码+说明 Lyra 怎么做/为什么），绝不主动在用户 `e:\ue5\cyra\code` 工程里写/改代码**——代码由用户自己写。可贴 Lyra 真实源码片段作教学示例，但不创建/修改用户工程文件；用户遇问题可引导答疑，仍以教为主。详见独立经验文件 `教学经验新对话必看\01_一比一还原Lyra_只教不写代码.md`。

### 【教学铁律】必须按教程目录步骤推进（2026-09-18）
每次教学都要**严格按 `e:\ue5\cyra\AI_Learning_Notes\Lyra_从零开发教学\` 目录里既定的课次步骤顺序**推进（00→00a→01总纲→02/03/04各课），不跳步、不自创路线。当前进度：00/00a/01总纲已完成，下一步=第02课（给角色挂ASC+属性集）。详见 `教学经验新对话必看\02_按教程目录步骤教学.md`。

### 【教学铁律】遇到问题要自建md总结（2026-09-18）
教学中**每次遇到问题**（用户提问/踩坑/报错/易混点），我都要**主动自己新建 md 文件总结沉淀**，不等用户提醒。内容含：问题/背景/原因(结合真实源码)/解决办法/源码依据/一句话结论。放合适目录（该课相关→Lyra_从零开发教学，通用→教学经验新对话必看）。详见 `教学经验新对话必看\03_教学中遇到问题要自建md总结.md`。

### 【教学铁律】分步骤教学，不要一次丢一堆代码（2026-09-18）
教学要**小步慢走**：一课先给步骤全景清单，然后**一次只教一步**（讲透该步目标+概念+该步Lyra源码片段+在code落地位置）就**停下等反馈**，用户"懂了/做完/继续"才进下一步。代码"够用就好"，只贴当前步必需片段并标注步序（如"【第2步/共5步】"），不整文件/多文件一次性砸。卡住就在那一步深入拆，不跳过。详见 `教学经验新对话必看\04_教学分步骤不要一次丢一堆代码.md`。

### 【教学铁律】课时md要拆小，每课3~5知识点（2026-09-18）
每个教学步骤都要在 `e:\ue5\cyra\AI_Learning_Notes\Lyra_从零开发教学\` **落地成独立小 md**；**md不要太大**（用户明确"很讨厌文件太大"），一个md只放**3~5个知识点**，一课内容多就拆成多个md（如02a/02b），宁可多建几个小文件不堆大文件。命名延续编号+见名知意，md内部用小标题/表格/代码块分块。详见 `教学经验新对话必看\05_课时md要拆小每课3到5知识点.md`。

## 工作区配置（2026-09-08）

用户的工作区 `cyra_company.code-workspace`（位于 `e:\ue5\cyra\workspace\company\`）包含三个根目录：

| 名称 | 路径 | 说明 |
|------|------|------|
| code (UE 项目) | `E:/ue5/cyra/code` | 用户的 UE 项目代码。**2026-09-18 已重置为干净空白工程**：UE5.6 空白 C++ Game 工程，无任何 GAS 痕迹（Build.cs 已去掉 GAS 三依赖、AttributeSet 目录已删），ASC/属性集/Character 全未建，第02课从"真正加 GAS 依赖"重教 |
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
