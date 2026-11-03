using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Pooling;
using AbilityKit.Triggering.Runtime.Schedule.Behavior;
using AbilityKit.Triggering.Runtime.Schedule.Data;

namespace AbilityKit.Triggering.Runtime.Schedule
{
    /// <summary>
    /// 持有分组调度项存储，并保持 item/effect/callback 数组对齐。
    /// </summary>
    internal sealed class GroupedScheduleStore
    {
        private readonly List<ScheduleItemData> _items = new();
        private readonly List<IScheduleEffect> _effects = new();
        private readonly List<IScheduleEffectCallbacks> _callbacks = new();
        private readonly TriggeringRuntimePools _pools;

        public GroupedScheduleStore(TriggeringRuntimePools pools)
        {
            _pools = pools ?? throw new ArgumentNullException(nameof(pools));
        }

        public int Count => _items.Count;

        public ScheduleItemData this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public IScheduleEffect GetEffect(int index)
        {
            return _effects[index];
        }

        public IScheduleEffectCallbacks GetCallback(int index)
        {
            return _callbacks[index];
        }

        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < _items.Count;
        }

        public bool IsValidHandle(ScheduleHandle handle)
        {
            return handle.IsValid && IsValidIndex(handle.Index);
        }

        public ScheduleHandle Register(ScheduleRegisterRequest request, IScheduleEffect effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            int index = _items.Count;
            var item = CreateItem(index, request);

            _items.Add(item);
            _effects.Add(effect);
            _callbacks.Add(effect as IScheduleEffectCallbacks);

            return item.Handle;
        }

        public bool TryGetItem(ScheduleHandle handle, out ScheduleItemData item)
        {
            if (IsValidHandle(handle))
            {
                item = _items[handle.Index];
                return true;
            }

            item = default;
            return false;
        }

        public void Clear()
        {
            _items.Clear();
            _effects.Clear();
            _callbacks.Clear();
        }

        public void RebuildWithout(HashSet<int> indicesToRemove)
        {
            if (indicesToRemove == null || indicesToRemove.Count == 0)
                return;

            var newItems = _pools.RentScheduleItemList();
            var newEffects = _pools.RentScheduleEffectList();
            var newCallbacks = _pools.RentScheduleEffectCallbackList();
            try
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    if (indicesToRemove.Contains(i))
                        continue;

                    int newIndex = newItems.Count;
                    var item = _items[i];
                    item.Handle = new ScheduleHandle(item.Handle.HandleId, newIndex);
                    newItems.Add(item);
                    newEffects.Add(_effects[i]);
                    newCallbacks.Add(_callbacks[i]);
                }

                _items.Clear();
                _effects.Clear();
                _callbacks.Clear();
                _items.AddRange(newItems);
                _effects.AddRange(newEffects);
                _callbacks.AddRange(newCallbacks);
            }
            finally
            {
                _pools.ReleaseScheduleItemList(newItems);
                _pools.ReleaseScheduleEffectList(newEffects);
                _pools.ReleaseScheduleEffectCallbackList(newCallbacks);
            }
        }

        private static ScheduleItemData CreateItem(int index, ScheduleRegisterRequest request)
        {
            return new ScheduleItemData
            {
                Handle = new ScheduleHandle(index + 1, index),
                BusinessId = request.BusinessId,
                TriggerId = request.TriggerId,
                State = EScheduleItemState.Registered,
                Mode = request.Mode,
                IntervalMs = request.IntervalMs,
                DelayMs = request.DelayMs,
                MaxExecutions = request.MaxExecutions,
                Speed = request.Speed,
                ElapsedMs = 0,
                LastExecuteMs = 0,
                ExecutionCount = 0,
                CanBeInterrupted = request.CanBeInterrupted,
                InterruptReason = null
            };
        }
    }
}
