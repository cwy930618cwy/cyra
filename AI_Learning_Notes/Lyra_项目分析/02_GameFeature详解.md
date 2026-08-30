# 02 — GameFeature 详解（游戏特性/玩法插件）

> **定位**：GameFeature 是 **UE5 引擎的能力**（不是 Lyra 独有的），但 Lyra 大量用它把玩法拆成模块。它是 Lyra 模块化的核心。
>
> **一句话**：GameFeature = **把"一块玩法/内容"封装成可插拔插件**，能按需加载/卸载。这样游戏可以像"拼积木"一样，按不同玩法加载不同模块。
>
> **文件**：`Engine/Plugins/GameFeatures/`（引擎机制）、Lyra `LyraGame/Plugins/`（用法）

---

## 一、GameFeature 是什么

### 一句话

**GameFeature（游戏特性）= 一个"玩法插件"**。它把一整块玩法（比如"团队死斗"、"枪械系统"）打包成插件，可以**按需加载、按需卸载**。

```
GameFeature：GF_团队死斗（一个玩法插件）
  ├─ 代码（GameMode、技能、规则）
  ├─ 资产（蓝图、数据、地图）
  └─ 能：加载它 → 这个玩法生效
           卸载它 → 这个玩法移除
```

### 类比

```
GameFeature = "可插拔的游戏模块"
  就像手机的 APP：
    装个"相机APP" → 有拍照功能
    装个"地图APP" → 有导航功能
    不需要了 → 卸载

GameFeature 同理：
    加载"团队死斗模块" → 有团队死斗玩法
    加载"大逃杀模块" → 有大逃杀玩法
    换玩法 → 换加载的模块
```

---

## 二、GameFeature 解决什么问题（为什么需要）

### 痛点：传统项目玩法全混在一起

```
❌ 传统项目：所有玩法写在一个游戏里
  一个大包里塞了：死斗 + 大逃杀 + 单机 + 合作
  → 代码臃肿、内存占用高、难维护
```

### GameFeature 解决：玩法拆成模块，按需加载

```
✅ GameFeature：玩法拆成独立模块
  GF_死斗 / GF_大逃杀 / GF_合作
  每个模块独立，用哪个加载哪个
  → 代码清晰、内存省、可维护
```

**核心价值**：
| 好处 | 说明 |
|------|------|
| **模块化** | 玩法拆成独立插件 |
| **按需加载** | 用哪个加载哪个，省内存 |
| **可组合** | 自由组合玩法模块 |
| **热更新** | 可动态加载/卸载（打包补丁） |

---

## 三、GameFeature 和普通插件的区别

| | 普通 Plugin | GameFeature |
|---|---|---|
| 加载时机 | 引擎启动就加载 | **运行时按需加载** |
| 卸载 | 基本不卸载 | **可卸载** |
| 用途 | 引擎/编辑器工具 | **玩法内容** |
| 打包 | 全打进去 | **可单独打包/补丁** |

**关键区别**：GameFeature 能**在运行时动态加载/卸载**，普通插件不能。这正是玩法模块化的关键。

---

## 四、GameFeature 怎么工作（核心概念）

### GameFeature 的三个核心：

```
GameFeature
  ├─ 内容（代码 + 资产 + 数据）
  ├─ 状态机（加载流程：安装→激活→开始）
  └─ 和 Experience 配合（谁加载它）
```

### 加载流程（状态机）：

```
GameFeature 加载流程：
  Registered（注册）
    → Downloaded（下载）
      → Installed（安装）
        → Loaded（加载）
          → Active（激活）
            → Started（开始）
```

### 和 Experience 配合：

```
Experience（规则剧本）
  └─ 指定加载哪些 GameFeature（玩法模块）
       ├─ GF_团队系统
       ├─ GF_枪械
       └─ GF_阶段

加载 Experience → 加载它指定的 GameFeature → 玩法生效
```

---

## 五、具体场景：Lyra 怎么用 GameFeature

**场景：Lyra 拆了多个 GameFeature，一个玩法一个模块**

```
Lyra 的 GameFeatures（Plugins/GameFeatures/）：
  ├─ GameFeatures_TeamDeathmatch（团队死斗玩法）
  ├─ GameFeatures_Control（占点玩法）
  ├─ GameFeatures_TopDownArena（俯视角玩法）
  └─ ...（各种玩法模块）

每个模式 = 一个 GameFeature
玩家选模式 → 加载对应 GameFeature → 那个玩法生效
```

**这就是 Lyra 支持"三种游戏模式"的原因**——每种模式是一个 GameFeature，切换就是加载不同的 GameFeature。

---

## 六、GameFeature 和 Experience 的关系（关键）

这是 Lyra 里最容易混的两个概念，要分清：

| | Experience | GameFeature |
|---|---|---|
| 是什么 | **规则数据资产**（剧本） | **玩法插件**（模块） |
| 作用 | 决定"这局怎么玩" | 提供"玩法内容" |
| 关系 | **指定**加载哪些 GameFeature | **被** Experience 加载 |
| 类比 | 剧本 | 演员/道具 |

```
Experience（剧本）
  └─ 加载 → GameFeature（演员/道具）
       └─ 演员按剧本演出 = 游戏按 Experience 规则跑
```

**一句话**：Experience 是"**剧本**"（定规则），GameFeature 是"**演员道具**"（玩法模块）。Experience 决定加载哪些 GameFeature。

---

## 七、总结速查

```
GameFeature = 玩法插件（可插拔，按需加载）
  ├─ 内容：代码 + 资产 + 数据
  ├─ 加载：运行时动态加载/卸载
  └─ 和普通插件区别：能运行时加载/卸载

价值：
  模块化（玩法拆开）
  按需加载（省内存）
  可组合（自由拼）
  热更新（可补丁）

和 Experience：
  Experience = 剧本（定规则）
  GameFeature = 演员道具（玩法模块）
  Experience 加载 GameFeature
```

**一句话**：GameFeature 是 **UE5 把"一块玩法"封装成可插拔插件的能力**，能按需加载/卸载，实现玩法模块化。**Experience（剧本）决定加载哪些 GameFeature（玩法模块）**。Lyra 大量用它把三种游戏模式拆成独立模块。

---

## 八、下一步

理解了 GameFeature，下一步可以深入 **GameFeature 的加载流程/状态机**，或看 **Lyra 具体怎么用 GameFeature 拆玩法**。
