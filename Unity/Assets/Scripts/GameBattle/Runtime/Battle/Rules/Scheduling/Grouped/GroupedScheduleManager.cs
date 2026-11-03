using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Pooling;
using AbilityKit.Triggering.Runtime.Schedule.Behavior;
using AbilityKit.Triggering.Runtime.Schedule.Data;

namespace AbilityKit.Triggering.Runtime.Schedule
{
    /// <summary>
    /// 分组调度管理器
    /// 实现 IGroupedScheduleManager 接口，提供分组管理能力
    /// 
    /// 使用场景：
    /// - Trigger 系统：按 TriggerId 分组管理调度项
    /// - 需要批量控制一组相关调度项
    /// 
    /// 如果不需要分组管理能力，请使用 SimpleScheduleManager
    /// 
    /// 性能优化：
    /// - 使用 LinkedList 存储分组索引，删除操作 O(1)
    /// - 使用字典缓存节点引用，避免遍历查找
    /// </summary>
    public sealed class GroupedScheduleManager : IGroupedScheduleManager
    {
        private static readonly TriggeringRuntimePools SharedPools = TriggeringRuntimePools.CreateDefault("triggering-grouped-schedule-temporary");

        private readonly GroupedScheduleStore _store;
        private readonly GroupedScheduleIndex _groupIndex;
        private readonly IScheduleStrategy _strategy;
        private readonly TriggeringRuntimePools _pools;

        public int ActiveCount { get; private set; }
        public int TotalCount => _store.Count;

        /// <summary>
        /// 创建调度管理器
        /// </summary>
        /// <param name="strategy">调度策略（可选，默认使用 DefaultScheduleStrategy）</param>
        public GroupedScheduleManager(IScheduleStrategy strategy = null, TriggeringRuntimePools pools = null)
        {
            _strategy = strategy ?? new DefaultScheduleStrategy();
            _pools = pools ?? SharedPools;
            _store = new GroupedScheduleStore(_pools);
            _groupIndex = new GroupedScheduleIndex(_pools);
        }

        #region 注册

        public ScheduleHandle Register(ScheduleRegisterRequest request, IScheduleEffect effect)
        {
            var handle = _store.Register(request, effect);
            _groupIndex.AddItem(request.TriggerId, handle.Index);
            ActiveCount++;
            return handle;
        }

        public ScheduleHandle RegisterPeriodic(float intervalMs, int maxExecutions, int businessId, IScheduleEffect effect)
        {
            var request = ScheduleRegisterRequest.Periodic(
                intervalMs: intervalMs,
                maxExecutions: maxExecutions,
                businessId: businessId
            );
            return Register(request, effect);
        }

        public ScheduleHandle RegisterContinuous(float intervalMs, int businessId, IScheduleEffect effect)
        {
            var request = ScheduleRegisterRequest.Continuous(
                intervalMs: intervalMs,
                businessId: businessId
            );
            return Register(request, effect);
        }

        public ScheduleHandle RegisterDelayed(float delayMs, int businessId, IScheduleEffect effect)
        {
            var request = ScheduleRegisterRequest.Delayed(
                delayMs: delayMs,
                businessId: businessId
            );
            return Register(request, effect);
        }

        #endregion

        #region 查询

        public bool TryGetItem(ScheduleHandle handle, out ScheduleItemData item)
        {
            return _store.TryGetItem(handle, out item);
        }

        public List<ScheduleItemData> FindByBusinessId(int businessId)
        {
            var result = new List<ScheduleItemData>();
            for (int i = 0; i < _store.Count; i++)
            {
                var item = _store[i];
                if (item.BusinessId == businessId && !item.IsTerminated)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public List<ScheduleHandle> FindHandlesByBusinessId(int businessId)
        {
            var result = new List<ScheduleHandle>();
            for (int i = 0; i < _store.Count; i++)
            {
                var item = _store[i];
                if (item.BusinessId == businessId && !item.IsTerminated)
                {
                    result.Add(item.Handle);
                }
            }
            return result;
        }

        #endregion

        #region 修改

        public bool Modify(ScheduleHandle handle, in ScheduleModifyRequest request)
        {
            if (!_store.IsValidHandle(handle))
                return false;

            var item = _store[handle.Index];
            if (item.IsTerminated)
                return false;

            if (request.HasSpeed) item.Speed = request.Speed;
            if (request.HasIntervalMs) item.IntervalMs = request.IntervalMs;
            if (request.HasMaxExecutions) item.MaxExecutions = request.MaxExecutions;
            if (request.HasDelayMs)
            {
                item.DelayMs = request.DelayMs;
                item.ElapsedMs = 0;
            }
            _store[handle.Index] = item;

            return true;
        }

        public bool SetSpeed(ScheduleHandle handle, float speed)
        {
            if (!_store.IsValidHandle(handle))
                return false;

            var item = _store[handle.Index];
            if (item.IsTerminated)
                return false;

            item.Speed = speed;
            _store[handle.Index] = item;
            return true;
        }

        public bool SetInterval(ScheduleHandle handle, float intervalMs)
        {
            if (!_store.IsValidHandle(handle))
                return false;

            var item = _store[handle.Index];
            if (item.IsTerminated)
                return false;

            item.IntervalMs = intervalMs;
            _store[handle.Index] = item;
            return true;
        }

        public bool AddExecutions(ScheduleHandle handle, int count)
        {
            if (!_store.IsValidHandle(handle))
                return false;

            var item = _store[handle.Index];
            if (item.IsTerminated)
                return false;

            if (item.MaxExecutions < 0)
                return true;

            item.MaxExecutions += count;
            _store[handle.Index] = item;
            return true;
        }

        #endregion

        #region 控制

        public bool Pause(ScheduleHandle handle)
        {
            if (!_store.IsValidHandle(handle))
                return false;

            var item = _store[handle.Index];
            if (item.IsTerminated)
                return false;

            item.State = EScheduleItemState.Paused;
            _store[handle.Index] = item;
            return true;
        }

        public bool Resume(ScheduleHandle handle)
        {
            if (!_store.IsValidHandle(handle))
                return false;

            var item = _store[handle.Index];
            if (item.IsTerminated || item.State != EScheduleItemState.Paused)
                return false;

            item.State = GetResumedState(item);
            _store[handle.Index] = item;
            return true;
        }

        public bool Interrupt(ScheduleHandle handle, string reason = null)
        {
            if (!_store.IsValidHandle(handle))
                return false;

            var item = _store[handle.Index];
            if (item.IsTerminated || !item.CanBeInterrupted)
                return false;

            item.State = EScheduleItemState.Interrupted;
            item.InterruptReason = reason;
            _store[handle.Index] = item;

            NotifyInterrupted(handle.Index, item, reason);

            ActiveCount--;
            return true;
        }

        public bool Cancel(ScheduleHandle handle)
        {
            if (!_store.IsValidHandle(handle))
                return false;

            var item = _store[handle.Index];
            if (item.IsTerminated)
                return false;

            item.State = EScheduleItemState.Terminated;
            _store[handle.Index] = item;

            ActiveCount--;
            return true;
        }

        public void PauseAll()
        {
            for (int i = 0; i < _store.Count; i++)
            {
                var item = _store[i];
                if (!item.IsTerminated && item.State != EScheduleItemState.Paused)
                {
                    item.State = EScheduleItemState.Paused;
                    _store[i] = item;
                }
            }
        }

        public void ResumeAll()
        {
            for (int i = 0; i < _store.Count; i++)
            {
                var item = _store[i];
                if (!item.IsTerminated && item.State == EScheduleItemState.Paused)
                {
                    item.State = GetResumedState(item);
                    _store[i] = item;
                }
            }
        }

        public int InterruptAll(string reason = null)
        {
            int count = 0;
            for (int i = 0; i < _store.Count; i++)
            {
                var item = _store[i];
                if (!item.IsTerminated && item.CanBeInterrupted)
                {
                    item.State = EScheduleItemState.Interrupted;
                    item.InterruptReason = reason;
                    _store[i] = item;

                    NotifyInterrupted(i, item, reason);
                    count++;
                }
            }
            ActiveCount -= count;
            return count;
        }

        #endregion

        #region 更新

        public void Update(float deltaTimeMs)
        {
            var indicesToRemove = _pools.RentIntList();
            try
            {
                for (int i = 0; i < _store.Count; i++)
                {
                    var item = _store[i];

                    if (item.IsTerminated)
                    {
                        indicesToRemove.Add(i);
                        continue;
                    }

                    var executor = new EffectExecutor(_store.GetEffect(i));
                    bool shouldRemove = _strategy.OnUpdate(ref item, deltaTimeMs, executor);
                    _store[i] = item;

                    if (shouldRemove)
                    {
                        item.State = item.State == EScheduleItemState.Interrupted
                            ? EScheduleItemState.Interrupted
                            : EScheduleItemState.Completed;
                        _store[i] = item;

                        NotifyCompleted(i, item);
                        indicesToRemove.Add(i);
                    }
                }

                CleanupItems(indicesToRemove);
            }
            finally
            {
                _pools.ReleaseIntList(indicesToRemove);
            }
        }

        #endregion

        #region 清理

        public void Clear()
        {
            _store.Clear();
            _groupIndex.Clear();
            ActiveCount = 0;
        }

        #endregion

        #region 分组属性

        public IReadOnlyList<int> GetActiveGroupIds()
        {
            return _groupIndex.GetActiveGroupIds();
        }

        public int GetItemCountByGroup(int groupId)
        {
            return _groupIndex.CountItems(groupId, index => _store.IsValidIndex(index) && !_store[index].IsTerminated);
        }

        #endregion

        #region 分组注册

        public ScheduleHandle RegisterForGroup(int groupId, ScheduleRegisterRequest request, IScheduleEffect effect)
        {
            request.TriggerId = groupId;
            return Register(request, effect);
        }

        public ScheduleHandle RegisterPeriodicForGroup(int groupId, float intervalMs, int maxExecutions, int businessId, IScheduleEffect effect)
        {
            var request = ScheduleRegisterRequest.Periodic(
                intervalMs: intervalMs,
                maxExecutions: maxExecutions,
                businessId: businessId,
                triggerId: groupId
            );
            return Register(request, effect);
        }

        public ScheduleHandle RegisterContinuousForGroup(int groupId, float intervalMs, int businessId, IScheduleEffect effect)
        {
            var request = ScheduleRegisterRequest.Continuous(
                intervalMs: intervalMs,
                businessId: businessId,
                triggerId: groupId
            );
            return Register(request, effect);
        }

        #endregion

        #region 分组查询

        public List<ScheduleItemData> FindByGroupId(int groupId)
        {
            var result = new List<ScheduleItemData>();
            if (_groupIndex.TryGetIndicesSnapshot(groupId, out var indices))
            {
                try
                {
                    foreach (var index in indices)
                    {
                        if (_store.IsValidIndex(index) && !_store[index].IsTerminated)
                        {
                            result.Add(_store[index]);
                        }
                    }
                }
                finally
                {
                    _pools.ReleaseIntList(indices);
                }
            }
            return result;
        }

        public List<ScheduleHandle> FindHandlesByGroupId(int groupId)
        {
            var result = new List<ScheduleHandle>();
            if (_groupIndex.TryGetIndicesSnapshot(groupId, out var indices))
            {
                try
                {
                    foreach (var index in indices)
                    {
                        if (_store.IsValidIndex(index) && !_store[index].IsTerminated)
                        {
                            result.Add(_store[index].Handle);
                        }
                    }
                }
                finally
                {
                    _pools.ReleaseIntList(indices);
                }
            }
            return result;
        }

        #endregion

        #region 分组控制

        public void PauseGroup(int groupId)
        {
            if (_groupIndex.TryGetIndicesSnapshot(groupId, out var indices))
            {
                try
                {
                    foreach (var index in indices)
                    {
                        if (!_store.IsValidIndex(index)) continue;
                        var item = _store[index];
                        if (!item.IsTerminated && item.State != EScheduleItemState.Paused)
                        {
                            item.State = EScheduleItemState.Paused;
                            _store[index] = item;
                        }
                    }
                }
                finally
                {
                    _pools.ReleaseIntList(indices);
                }
            }
        }

        public void ResumeGroup(int groupId)
        {
            if (_groupIndex.TryGetIndicesSnapshot(groupId, out var indices))
            {
                try
                {
                    foreach (var index in indices)
                    {
                        if (!_store.IsValidIndex(index)) continue;
                        var item = _store[index];
                        if (!item.IsTerminated && item.State == EScheduleItemState.Paused)
                        {
                            item.State = GetResumedState(item);
                            _store[index] = item;
                        }
                    }
                }
                finally
                {
                    _pools.ReleaseIntList(indices);
                }
            }
        }

        public int InterruptGroup(int groupId, string reason = null)
        {
            int count = 0;
            if (_groupIndex.TryGetIndicesSnapshot(groupId, out var indices))
            {
                try
                {
                    foreach (var index in indices)
                    {
                        if (!_store.IsValidIndex(index)) continue;
                        var item = _store[index];
                        if (!item.IsTerminated && item.CanBeInterrupted)
                        {
                            item.State = EScheduleItemState.Interrupted;
                            item.InterruptReason = reason;
                            _store[index] = item;

                            NotifyInterrupted(index, item, reason);
                            count++;
                        }
                    }
                }
                finally
                {
                    _pools.ReleaseIntList(indices);
                }
            }
            ActiveCount -= count;
            return count;
        }

        public int RemoveGroup(int groupId)
        {
            var count = 0;
            if (_groupIndex.TryGetIndicesSnapshot(groupId, out var indices))
            {
                count = indices.Count;
                _pools.ReleaseIntList(indices);
            }

            _groupIndex.RemoveGroup(groupId);
            ActiveCount -= count;
            return count;
        }

        #endregion

        #region 分组生命周期

        public void OnGroupActivated(int groupId)
        {
            _groupIndex.ActivateGroup(groupId);
        }

        public void OnGroupDeactivated(int groupId)
        {
            _groupIndex.DeactivateGroup(groupId);
        }

        #endregion

        private void CleanupItems(List<int> indices)
        {
            if (indices.Count == 0)
                return;

            var indicesToCleanup = _pools.RentIntHashSet();
            try
            {
                foreach (var index in indices)
                {
                    indicesToCleanup.Add(index);
                }

                _groupIndex.RemoveIndices(indicesToCleanup);

                var indexMapping = BuildCompactedIndexMapping(indicesToCleanup);
                _store.RebuildWithout(indicesToCleanup);
                _groupIndex.RebuildIndices(indexMapping);
                ActiveCount -= indices.Count;
            }
            finally
            {
                _pools.ReleaseIntHashSet(indicesToCleanup);
            }
        }

        private int[] BuildCompactedIndexMapping(HashSet<int> indicesToCleanup)
        {
            var indexMapping = new int[_store.Count];
            for (int i = 0; i < indexMapping.Length; i++)
            {
                indexMapping[i] = -1;
            }

            var writeIndex = 0;
            for (int i = 0; i < _store.Count; i++)
            {
                if (!indicesToCleanup.Contains(i))
                {
                    indexMapping[i] = writeIndex;
                    writeIndex++;
                }
            }

            return indexMapping;
        }

        private void NotifyCompleted(int index, ScheduleItemData item)
        {
            var callback = _store.GetCallback(index);
            if (callback != null)
            {
                var context = ScheduleContext.Create(item, 0, 0);
                callback.OnCompleted(context);
            }
        }

        private void NotifyInterrupted(int index, ScheduleItemData item, string reason)
        {
            var callback = _store.GetCallback(index);
            if (callback != null)
            {
                var context = ScheduleContext.Create(item, 0, 0);
                callback.OnInterrupted(context, reason ?? "Unknown");
            }
        }

        private static EScheduleItemState GetResumedState(ScheduleItemData item)
        {
            return item.ElapsedMs >= item.DelayMs
                ? EScheduleItemState.Running
                : EScheduleItemState.WaitingDelay;
        }
    }
}
