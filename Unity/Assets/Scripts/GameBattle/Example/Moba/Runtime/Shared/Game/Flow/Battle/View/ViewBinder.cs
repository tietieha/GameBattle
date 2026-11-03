using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 视图绑定器实现
    /// 管理 ECS 实体与视图对象的绑定关系
    /// 参考 view.runtime 的 BattleViewBinder 实现
    /// </summary>
    public sealed class ViewBinder : IViewBinder
    {
        private readonly Dictionary<int, IViewHandle> _bindings = new Dictionary<int, IViewHandle>();
        private readonly Dictionary<int, BattleNetId> _reverseBindings = new Dictionary<int, BattleNetId>();
        private readonly object _lock = new object();
        private bool _isDisposed;

        /// <summary>
        /// 绑定实体到视图
        /// </summary>
        public void Bind(BattleNetId netId, IViewHandle view)
        {
            if (!netId.IsValid || view == null) return;

            lock (_lock)
            {
                var key = netId.Value;

                // 如果已有绑定，先解除
                if (_bindings.TryGetValue(key, out var oldView))
                {
                    oldView?.Dispose();
                }

                _bindings[key] = view;
                _reverseBindings[view.GetHashCode()] = netId;
            }
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        public void Unbind(BattleNetId netId)
        {
            if (!netId.IsValid) return;

            lock (_lock)
            {
                var key = netId.Value;
                if (_bindings.TryGetValue(key, out var view))
                {
                    _bindings.Remove(key);
                    view?.Dispose();

                    _reverseBindings.Remove(view?.GetHashCode() ?? 0);
                }
            }
        }

        /// <summary>
        /// 同步实体到视图
        /// 根据实体数据更新视图的变换信息
        /// </summary>
        public void Sync(IEntityHandle entity)
        {
            if (entity == null || !entity.IsValid) return;

            lock (_lock)
            {
                if (!_bindings.TryGetValue(entity.NetId.Value, out var view))
                {
                    return;
                }

                // 获取变换信息并更新视图
                if (TryGetEntityTransform(entity, out var transform))
                {
                    view.SetPosition(transform.Position);
                    view.SetRotation(new AbilityKit.Core.Mathematics.Vec3(0, transform.RotationY, 0));
                    view.SetScale(new AbilityKit.Core.Mathematics.Vec3(transform.Scale, transform.Scale, transform.Scale));
                }
            }
        }

        private static bool TryGetEntityTransform(IEntityHandle entity, out TransformComponent transform)
        {
            // 简化实现：尝试从实体获取变换信息
            transform = default;
            return false;
        }

        /// <summary>
        /// 尝试获取绑定的视图
        /// </summary>
        public bool TryGetView(BattleNetId netId, out IViewHandle view)
        {
            lock (_lock)
            {
                return _bindings.TryGetValue(netId.Value, out view);
            }
        }

        /// <summary>
        /// 尝试获取视图绑定的网络 ID
        /// </summary>
        public bool TryGetNetId(IViewHandle view, out BattleNetId netId)
        {
            lock (_lock)
            {
                if (view != null && _reverseBindings.TryGetValue(view.GetHashCode(), out netId))
                {
                    return true;
                }
                netId = BattleNetId.Invalid;
                return false;
            }
        }

        /// <summary>
        /// 清空所有绑定
        /// </summary>
        public void ClearAll()
        {
            lock (_lock)
            {
                foreach (var kvp in _bindings)
                {
                    kvp.Value?.Dispose();
                }
                _bindings.Clear();
                _reverseBindings.Clear();
            }
        }

        /// <summary>
        /// 获取绑定数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _bindings.Count;
                }
            }
        }

        /// <summary>
        /// 是否包含指定网络 ID 的绑定
        /// </summary>
        public bool Contains(BattleNetId netId)
        {
            lock (_lock)
            {
                return _bindings.ContainsKey(netId.Value);
            }
        }

        /// <summary>
        /// 获取所有绑定的网络 ID
        /// </summary>
        public IReadOnlyList<BattleNetId> GetAllNetIds()
        {
            lock (_lock)
            {
                var result = new List<BattleNetId>(_bindings.Keys.Count);
                foreach (var key in _bindings.Keys)
                {
                    result.Add(new BattleNetId(key));
                }
                return result;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            ClearAll();
        }

        private static AbilityKit.Core.Mathematics.Vec3 GetTransformPosition(IEntityHandle entity)
        {
            // 这里简化处理，实际实现需要从组件中获取
            return default;
        }
    }

    /// <summary>
    /// 变换组件数据
    /// 用于在视图绑定时传递变换信息
    /// </summary>
    public struct TransformComponent
    {
        public AbilityKit.Core.Mathematics.Vec3 Position;
        public float RotationY;
        public float Scale;
    }
}
