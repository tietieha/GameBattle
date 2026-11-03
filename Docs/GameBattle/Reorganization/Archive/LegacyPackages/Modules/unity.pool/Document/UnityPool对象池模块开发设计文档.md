# Ability-Kit Unity Pool 对象池模块开发设计文档

> **阅读对象**：需要在 Unity 环境中复用 GameObject、Component 的客户端开发者。
>
> **文档目标**：说明 Unity Pool 包如何基于 Core 对象池封装 Unity 对象生命周期，以及使用时需要注意的主线程和归还规则。

---

## 一、设计理念

`com.abilitykit.unity.pool` 是 Core 对象池在 Unity 对象上的适配层。它不重新实现通用池算法，而是使用 `AbilityKit.Core.Pooling.ObjectPool<T>`，只补充 GameObject/Component 的 Instantiate、SetActive、Destroy、Parent 处理。

适用场景：

- 技能特效、命中特效、飘字。
- 临时碰撞体或可视化 Debug 对象。
- UI Item、列表元素。
- 需要统一查看 `PoolStats` 的 Unity 对象复用场景。

---

## 二、模块边界

负责：

- 从 prefab 创建 GameObject 或 Component 实例。
- Get 时激活对象。
- Release 时隐藏对象，并可重新挂到 root。
- 超过池上限或销毁池对象时调用 Unity `Object.Destroy`。
- 暴露 Core 池统计信息。

不负责：

- 不负责异步加载 prefab。
- 不负责 Addressables、AssetBundle 生命周期。
- 不负责跨场景 root 管理。
- 不负责自动清理业务状态，调用方需要在归还前或 OnGet/OnRelease 中处理。
- 不允许后台线程访问 Unity 对象。

---

## 三、目录结构

| 路径 | 职责 |
|------|------|
| `Runtime/Pool/UnityGameObjectPool.cs` | GameObject 池 |
| `Runtime/Pool/UnityComponentPool.cs` | Component 池 |
| `Runtime/Pool/UnityPoolHandle.cs` | 租借句柄，便于 using/Dispose 归还 |
| `Runtime/Pool/UnityPools.cs` | 静态创建/辅助入口 |
| `Runtime/com.abilitykit.unity.pool.asmdef` | 引用 `AbilityKit.Core`，允许 UnityEngine 引用 |

---

## 四、核心类型

### 4.1 UnityGameObjectPool

构造时接收 prefab、root、defaultCapacity、maxSize、collectionCheck，并创建 `ObjectPool<GameObject>`。

生命周期策略：

- `Create`：`UnityEngine.Object.Instantiate(prefab, root)`。
- `OnGet`：`go.SetActive(true)`。
- `OnRelease`：`go.SetActive(false)`，如果 root 不为空则 `SetParent(root, false)`。
- `OnDestroy`：`UnityEngine.Object.Destroy(go)`。

### 4.2 UnityComponentPool

Component 池通常围绕某个组件 prefab 工作，内部仍需要通过 GameObject 实例化获得组件。使用时应确保 prefab 上存在目标组件类型。

### 4.3 UnityPoolHandle

句柄用于把“租借后必须归还”的约束显式化。推荐临时使用场景通过句柄包装，降低忘记 Release 的概率。

---

## 五、使用流程

```csharp
var pool = new UnityGameObjectPool(prefab, poolRoot, defaultCapacity: 8, maxSize: 128);

var go = pool.Get();
go.transform.position = hitPoint;

// 使用结束后归还
pool.Release(go);
```

如果对象内部有粒子、动画、脚本状态，调用方应在归还前重置，或在自定义包装中统一处理。

---

## 六、注意事项

- 该包代码使用 `#if UNITY_5_3_OR_NEWER`，只在 Unity 环境下生效。
- `SetActive(false)` 不等于业务状态清理，脚本字段、订阅事件、协程都需要自行处理。
- collectionCheck 打开时能检查重复归还，但会增加一定开销。
- 如果 root 随场景销毁，池里对象也会失效，跨场景池需要单独管理 root。
- `Object.Destroy` 是 Unity 生命周期调用，不应在非主线程触发。

---

*文档版本：1.0*  
*最后更新：2026-06-05*
