# 01 — Core 模块深度剖析

> **路径**：`Engine/Source/Runtime/Core/`
> 
> **定位**：整个 UE5 引擎的"地基"。没有 Core，其他一切（UObject、渲染、物理、网络）都无法运行。
> 
> **核心原则**：Core 不依赖任何 UE 模块，只依赖 C++ 标准库 + 平台 API。它是唯一可以 `#include <iostream>` 的地方。

---

## 一、目录结构

```
Runtime/Core/
├── Public/                  ← 头文件（你 #include 的东西）
│   ├── Containers/          ← ★ 容器：TArray、TMap、TSet、FString、TSharedPtr
│   ├── Math/                ← ★ 数学：FVector、FRotator、FMath、FTransform
│   ├── Delegates/           ← ★ 委托：单播/多播/动态委托
│   ├── Templates/           ← 模板工具：类型特征、移动语义、Tuple
│   ├── HAL/                 ← 硬件抽象层：线程、锁、文件、内存分配
│   ├── Misc/                ← 杂项：日志、断言、配置文件、命令行
│   ├── Serialization/       ← 序列化底层：FArchive、内存布局
│   ├── UObject/             ← NameTypes.h（FName）、UnrealNames.cpp
│   ├── Async/               ← 异步任务：Future、Promise、TaskGraph
│   ├── Compression/         ← 压缩：LZ4、Oodle
│   ├── Experimental/        ← 实验性：ConcurrentLinearAllocator
│   └── ...
├── Private/                 ← 实现文件（.cpp）
└── Core.Build.cs            ← 模块构建配置
```

---

## 二、六大核心子系统

### 2.1 TArray — 动态数组（最常用）

**文件**：`Public/Containers/Array.h`（约 4000 行）

#### 内存布局

```cpp
template<typename InElementType, typename InAllocatorType>
class TArray {
private:
    ElementAllocatorType AllocatorInstance;  // 内存分配器（管理实际数据指针）
    SizeType             ArrayNum;           // 当前元素个数
    SizeType             ArrayMax;           // 已分配容量（>= ArrayNum）
};
```

**图解：**

```
AllocatorInstance → [Elem0][Elem1][Elem2][  ][  ][  ]
                     ↑      ↑      ↑     ↑    ↑    ↑
                   已用元素              空闲空间（Slack）
                   
ArrayNum = 3（有 3 个元素）
ArrayMax = 6（总共能装 6 个）
```

#### 关键方法源码解析

**① 构造函数**

```cpp
FORCEINLINE TArray()
    : ArrayNum(0)                              // 初始 0 个元素
    , ArrayMax(AllocatorInstance.GetInitialCapacity())  // 容量由分配器决定
{
}
```

**② operator[] — 下标访问**

```cpp
FORCEINLINE ElementType& operator[](SizeType Index) {
    RangeCheck(Index);       // 越界检查（Debug 模式下会断言）
    return GetData()[Index]; // 直接返回引用，O(1)
}
```

**③ Add — 添加元素**

```cpp
SizeType Add(const ElementType& Item) {
    CheckAddress(&Item);     // 防止自我引用导致的数据损坏
    return Emplace(Item);    // 转发到 Emplace
}

// Emplace 内部逻辑（简化）：
SizeType Emplace(ElementType&& Item) {
    if (ArrayNum == ArrayMax) {
        // 容量满了 → 重新分配更大的内存（通常 1.5x 增长）
        Grow();
    }
    // 在末尾构造新元素（placement new）
    ::new((void*)GetAllocation() + ArrayNum * sizeof(ElementType)) 
        ElementType(Forward<Item>(Item));
    ArrayNum++;
    return ArrayNum - 1;
}
```

**④ Empty — 清空**

```cpp
void Empty(SizeType Slack = 0) {
    DestructItems(GetData(), ArrayNum);  // 调用每个元素的析构函数
    ArrayNum = 0;
    // 如果 Slack > 0，保留一定容量；否则释放全部内存
}
```

#### 性能要点

| 操作 | 时间复杂度 | 说明 |
|------|-----------|------|
| `operator[]` | O(1) | 直接指针偏移 |
| `Add()` | O(1)* | *均摊，偶尔需要 realloc |
| `RemoveAt(i)` | O(n) | 后面所有元素前移 |
| `RemoveAtSwap(i)` | O(1) | 交换到末尾再删（破坏顺序） |
| `Find(Item)` | O(n) | 线性查找 |
| `Empty()` | O(n) | 逐个析构 |

#### TArray vs std::vector

| 特性 | TArray | std::vector |
|------|--------|-------------|
| 内存分配器 | 可定制（FDefaultAllocator / FInlineAllocator） | std::allocator |
| 元素类型限制 | 必须支持 `GetTypeLayout()`（反射友好） | 无限制 |
| 调试支持 | UE_LOG 集成、内存标记 | 无 |
| 序列化 | 内置 Serialize() 支持 | 无 |
| 迭代器失效 | 和 vector 一样 | 一样 |

---

### 2.2 FString — 字符串

**文件**：`Public/Containers/UnrealString.h`

#### 为什么不用 std::string？

1. **Unicode 优先**：FString 内部用 `TCHAR`（Windows 上是 `wchar_t`，即 UTF-16）
2. **反射集成**：可以作为 UPROPERTY 参与 GC 和序列化
3. **UE 生态一致**：所有 UE API 都返回 FString

#### 核心结构

```cpp
class FString {
protected:
    TArray<TCHAR> Data;  // 实际字符存储（含结尾 \0）
};
```

**是的，FString 就是 TArray<TCHAR> 的封装！**

#### 常见操作

```cpp
FString Name = TEXT("Hello World");  // TEXT() 宏确保宽字符
Name.Len();                           // 长度（不含 \0）
Name.IsEmpty();                       // 是否为空
Name.Contains(TEXT("World"));         // 包含子串
Name.StartsWith(TEXT("Hello"));       // 前缀匹配
Name.Split(TEXT(" "), &Before, &After); // 分割
Name.Replace(TEXT("World"), TEXT("UE5")); // 替换
Name.TrimStartAndEnd();               // 去首尾空白
FString Num = FString::FromInt(42);   // int → FString
int32 Val = FCString::Atoi(*Name);    // FString → int（通过 * 取原始指针）
```

#### FName vs FString vs FText

| 类型 | 用途 | 特点 |
|------|------|------|
| **FName** | 标识符（资源名、Tag、属性名） | 不可变、哈希比较、全局唯一、不能改 |
| **FString** | 通用字符串操作 | 可变、支持拼接/查找/替换 |
| **FText** | 显示给玩家的文本 | 支持本地化翻译、格式化 |

**转换关系：**
```
FName ──ToString()──→ FString ──ToText()──→ FText
FText ──ToString()──→ FString ──ToString()─→ FName
```

---

### 2.3 TMap — 键值对映射

**文件**：`Public/Containers/Map.h`

#### 底层实现

```cpp
template<typename KeyType, typename ValueType, ...>
class TMap {
private:
    TPairSortedMap<KeyType, ValueType, ...> SortedMap;
    // 内部基于 TSet<TPair<Key, Value>> 实现
    // 使用哈希表 + 有序遍历
};
```

#### 常用操作

```cpp
TMap<FName, int32> HealthMap;
HealthMap.Add(TEXT("MaxHealth"), 100);     // 插入
int32* Found = HealthMap.Find(TEXT("MaxHealth")); // 查找（返回指针，未找到返回 nullptr）
HealthMap.Remove(TEXT("MaxHealth"));       // 删除
HealthMap.Contains(TEXT("MaxHealth"));     // 是否包含
for (auto& Pair : HealthMap) {             // 遍历
    UE_LOG(LogTemp, Log, TEXT("%s = %d"), *Pair.Key.ToString(), Pair.Value);
}
```

---

### 2.4 委托系统（Delegate）

**文件**：`Public/Delegates/Delegate.h` + `MulticastDelegate.h` + `DynamicDelegate.h`

#### 四种委托对比

| 类型 | 声明宏 | 特点 | 使用场景 |
|------|--------|------|---------|
| **单播委托** | `DECLARE_DELEGATE` | 只能绑一个函数 | 回调、事件处理 |
| **多播委托** | `DECLARE_MULTICAST_DELEGATE` | 能绑多个函数 | 广播通知 |
| **动态单播** | `DECLARE_DYNAMIC_DELEGATE` | 可序列化、蓝图可绑定 | 需要在编辑器中配置 |
| **动态多播** | `DECLARE_DYNAMIC_MULTICAST_DELEGATE` | 上面两者的结合 | 蓝图事件分发器 |

#### 单播委托源码结构

```cpp
// 声明
DECLARE_DELEGATE_OneParam(FOnHealthChanged, float);
// 展开后等价于：
class FOnHealthChanged : public FDelegate {
    // 存储：对象指针 + 成员函数指针
    // 调用：Execute(NewValue)
};
```

#### 使用示例

```cpp
// 声明
DECLARE_MULTICAST_DELEGATE_TwoParams(FOnDamageTaken, float, FVector);

// 类中使用
class AMyCharacter : public ACharacter {
    FOnDamageTaken OnDamageTaken;
    
    void TakeDamage(float Amount, FVector Location) {
        OnDamageTaken.Broadcast(Amount, Location);  // 广播给所有绑定者
    }
};

// 外部绑定
Character->OnDamageTaken.AddLambda([](float Amount, FVector Loc) {
    UE_LOG(LogTemp, Warning, TEXT("受伤 %.1f 位置 %s"), Amount, *Loc.ToString());
});
```

#### 委托的执行模型

```
单播委托：
  Delegate.Bind(this, &AMyClass::OnEvent);
  Delegate.Execute(Param);  → 调用 AMyClass::OnEvent(Param)

多播委托：
  Delegate.AddUObject(this, &AMyClass::OnEvent);
  Delegate.AddLambda(...);
  Delegate.Broadcast(Param); → 依次调用所有绑定的函数
```

---

### 2.5 FMath — 数学工具箱

**文件**：`Public/Math/UnrealMathUtility.h`

#### 常用常量

```cpp
PI                          // 3.14159265358979323846
UE_SMALL_NUMBER             // 1.e-8（浮点比较的容差）
UE_KINDA_SMALL_NUMBER       // 1.e-4
UE_BIG_NUMBER               // 3.4e+98
UE_DOUBLE_PI                // PI * 2
UE_HALF_PI                  // PI / 2
UE_INV_PI                   // 1 / PI
```

#### 常用函数

```cpp
FMath::Abs(x)               // 绝对值
FMath::Clamp(x, Min, Max)   // 限制范围
FMath::Lerp(A, B, Alpha)    // 线性插值：A + (B-A)*Alpha
FMath::Sqrt(x)              // 平方根
FMath::Square(x)            // x²
FMath::Sin/Cos/Tan(x)       // 三角函数（弧度）
FMath::DegreesToRadians(d)  // 角度 → 弧度
FMath::RandRange(Min, Max)  // 随机浮点数
FMath::RandStream()         // 伪随机数流（可复现）
```

#### 向量运算（FVector）

```cpp
FVector A(1.f, 2.f, 3.f);
FVector B(4.f, 5.f, 6.f);

A + B                       // 加法
A - B                       // 减法
FVector::DotProduct(A, B)   // 点积
FVector::CrossProduct(A, B) // 叉积
A.Size()                    // 长度
A.SizeSquared()             // 长度的平方（更快，不用开方）
A.GetSafeNormal()           // 归一化（安全，零向量返回 ZeroVector）
A.Distance(B)               // 两点距离
```

---

### 2.6 HAL — 硬件抽象层

**文件**：`Public/HAL/`

这是引擎和操作系统之间的"翻译层"，让你用同一套代码操作不同平台。

| 文件 | 作用 |
|------|------|
| `Platform.h` | 平台检测宏（PLATFORM_WINDOWS / PLATFORM_LINUX / PLATFORM_MAC） |
| `Thread.h` | 线程抽象（FRunnable、FRunnableThread） |
| `CriticalSection.h` | 临界区/互斥锁（FCriticalSection） |
| `FileManager.h` | 文件操作（IFileManager::Get().Copy/Move/Delete） |
| `PlatformFile.h` | 平台文件系统抽象 |
| `MemoryBase.h` | 内存分配接口 |
| `UnrealMemory.h` | FMemory::Malloc/Free/Memcpy/Memzero |
| `PlatformProcess.h` | 进程操作（创建子进程、获取 PID） |
| `Event.h` | 事件信号（线程间同步） |
| `Runnable.h` | 可运行对象基类（多线程入口） |

#### 示例：跨平台文件操作

```cpp
// 不需要 #ifdef _WIN32，UE 帮你处理了
IFileManager& FM = IFileManager::Get();
FM.Copy(TEXT("Dest.txt"), TEXT("Src.txt"), false, true);
FM.Delete(TEXT("Temp.txt"));
```

---

## 三、其他重要子系统速查

### 3.1 模板工具（Templates/）

| 文件 | 作用 |
|------|------|
| `TypeTraits.h` | 编译期类型判断（`TIsPointer_V<T>`、`TIsSame_V<A,B>`） |
| `UnrealTemplate.h` | UE 特有模板（`TUniquePtr`、`MakeShared`） |
| `Tuple.h` | 元组实现 |
| `Function.h` | 函数包装器（类似 std::function） |
| `Sorting.h` | 排序算法（`Algo::Sort`、`Algo::BinarySearch`） |
| `Invoke.h` | 调用转发（`Invoke`、`Forward`） |

### 3.2 序列化（Serialization/）

| 文件 | 作用 |
|------|------|
| `Archive.h` | FArchive 基类（所有读写的抽象） |
| `MemoryReader.h` | 从内存读取 |
| `MemoryWriter.h` | 写入内存 |
| `BufferArchive.h` | 缓冲区读写 |
| `StructuredArchive.h` | 结构化序列化（JSON-like） |

### 3.3 异步（Async/）

| 文件 | 作用 |
|------|------|
| `Future.h` | Future/Promise 模式 |
| `TaskGraphInterfaces.h` | 任务图（UE 的线程池调度系统） |
| `ParallelFor.h` | 并行 for 循环 |

### 3.4 日志系统（Misc/）

```cpp
// 日志级别
UE_LOG(LogTemp, Log, TEXT("普通信息"));
UE_LOG(LogTemp, Warning, TEXT("警告"));
UE_LOG(LogTemp, Error, TEXT("错误"));
UE_LOG(LogTemp, Fatal, TEXT("致命错误"));  // 会崩溃

// 自定义日志类别
DECLARE_LOG_CATEGORY_EXTERN(LogMyGame, Log, All);
DEFINE_LOG_CATEGORY(LogMyGame);
```

---

## 四、Core 在整个引擎中的位置

```
┌─────────────────────────────────────────────┐
│              你的游戏代码                     │
├─────────────────────────────────────────────┤
│  Engine (AActor/UWorld/GameMode...)          │
├─────────────────────────────────────────────┤
│  CoreUObject (UObject/GC/反射/序列化)         │
├─────────────────────────────────────────────┤
│  Renderer/Slate/AIModule/Network...          │
├─────────────────────────────────────────────┤
│  ★ Core ★                                    │
│  ┌─────────────────────────────────────────┐│
│  │ TArray/FString/TMap/Delegate/FMath/HAL  ││
│  └─────────────────────────────────────────┘│
├─────────────────────────────────────────────┤
│  C++ 标准库 (<memory>/<vector>/<string>)     │
├─────────────────────────────────────────────┤
│  操作系统 (Windows/Linux/macOS/iOS/Android)  │
└─────────────────────────────────────────────┘
```

**Core 是唯一被所有其他模块依赖的模块。** 它不依赖任何 UE 模块，只依赖 C++ 标准库和平台 API。

---

## 五、新手必须掌握的 10 个 API

| 优先级 | API | 所属 | 原因 |
|--------|-----|------|------|
| ⭐⭐⭐ | `TArray<T>` | Containers | 到处都在用，替代 std::vector |
| ⭐⭐⭐ | `FString` | Containers | 字符串操作，替代 std::string |
| ⭐⭐⭐ | `FName` | UObject/NameTypes | 标识符，比 FString 快得多 |
| ⭐⭐⭐ | `UE_LOG` | Misc/Logging | 日志输出 |
| ⭐⭐⭐ | `FMath::Clamp/Lerp` | Math | 游戏数学必备 |
| ⭐⭐ | `TMap<K,V>` | Containers | 键值对查找 |
| ⭐⭐ | `TSharedPtr<T>` | Templates | 引用计数智能指针 |
| ⭐⭐ | `FVector` | Math | 3D 向量 |
| ⭐⭐ | `TEXT()` 宏 | CoreTypes | 宽字符字符串字面量 |
| ⭐ | `FCriticalSection` | HAL | 多线程互斥锁 |

---

## 六、常见陷阱

### 6.1 TArray 迭代器失效

```cpp
// ❌ 错误：遍历时删除元素
for (int32 i = 0; i < Arr.Num(); i++) {
    if (ShouldRemove(Arr[i])) {
        Arr.RemoveAt(i);  // 后面的元素前移，i 跳过了下一个！
    }
}

// ✅ 正确：倒序遍历删除
for (int32 i = Arr.Num() - 1; i >= 0; i--) {
    if (ShouldRemove(Arr[i])) {
        Arr.RemoveAt(i);
    }
}

// ✅ 更好：使用 RemoveAllSwap（如果不关心顺序）
Arr.RemoveAllSwap([&](const auto& Item) { return ShouldRemove(Item); });
```

### 6.2 FString 转 const char*

```cpp
FString Str = TEXT("Hello");

// ❌ 错误：直接转可能乱码
const char* Bad = TCHAR_TO_ANSI(*Str);  // 非 ASCII 字符会丢失

// ✅ 正确：保持宽字符
const TCHAR* Wide = *Str;  // 或 Str.GetData()

// ✅ 需要 ANSI 时用
FTCHARToANSI AnsiConverter(*Str);
const char* Ansi = AnsiConverter.Get();
```

### 6.3 FName 不要用 == 频繁比较

```cpp
// FName 的 == 比较的是哈希值，不是字符串内容
// 对于大量比较场景，先转 FString 再比较
FName A(TEXT("LongName123"));
FName B(TEXT("LongName123"));
// A == B  → 比较哈希（快但理论上有碰撞风险）
// 如果需要绝对精确，用 A.IsEqual(B, ENameCompareFlags::CaseSensitive)
```
