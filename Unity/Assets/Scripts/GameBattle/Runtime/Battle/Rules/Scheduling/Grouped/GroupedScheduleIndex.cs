using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Pooling;

namespace AbilityKit.Triggering.Runtime.Schedule
{
    /// <summary>
    /// 维护分组调度器的分组索引，隔离 LinkedList/活跃分组集合等索引细节。
    /// </summary>
    internal sealed class GroupedScheduleIndex
    {
        private readonly Dictionary<int, LinkedList<int>> _itemsByGroup = new();
        private readonly HashSet<int> _activeGroups = new();
        private readonly TriggeringRuntimePools _pools;

        public GroupedScheduleIndex(TriggeringRuntimePools pools)
        {
            _pools = pools ?? throw new ArgumentNullException(nameof(pools));
        }

        public void AddItem(int groupId, int itemIndex)
        {
            if (!_itemsByGroup.TryGetValue(groupId, out var linkedList))
            {
                linkedList = new LinkedList<int>();
                _itemsByGroup[groupId] = linkedList;
            }

            linkedList.AddLast(itemIndex);
        }

        public IReadOnlyList<int> GetActiveGroupIds()
        {
            return new List<int>(_activeGroups);
        }

        public int CountItems(int groupId, Predicate<int> predicate)
        {
            if (!_itemsByGroup.TryGetValue(groupId, out var linkedList))
                return 0;

            int count = 0;
            foreach (var index in linkedList)
            {
                if (predicate == null || predicate(index))
                    count++;
            }
            return count;
        }

        public bool TryGetIndicesSnapshot(int groupId, out List<int> indices)
        {
            if (_itemsByGroup.TryGetValue(groupId, out var linkedList))
            {
                indices = _pools.RentIntList();
                foreach (var index in linkedList)
                {
                    indices.Add(index);
                }
                return true;
            }

            indices = null;
            return false;
        }

        public void ActivateGroup(int groupId)
        {
            _activeGroups.Add(groupId);
        }

        public void DeactivateGroup(int groupId)
        {
            _activeGroups.Remove(groupId);
        }

        public bool RemoveGroup(int groupId)
        {
            var removed = _itemsByGroup.Remove(groupId);
            _activeGroups.Remove(groupId);
            return removed;
        }

        public void RemoveIndices(ISet<int> indicesToRemove)
        {
            if (indicesToRemove == null || indicesToRemove.Count == 0)
                return;

            var groupsToRemove = _pools.RentIntList();
            try
            {
                foreach (var kvp in _itemsByGroup)
                {
                    int groupId = kvp.Key;
                    var linkedList = kvp.Value;
                    var node = linkedList.First;

                    while (node != null)
                    {
                        int itemIndex = node.Value;
                        if (indicesToRemove.Contains(itemIndex))
                        {
                            var nextNode = node.Next;
                            linkedList.Remove(node);
                            node = nextNode;
                        }
                        else
                        {
                            node = node.Next;
                        }
                    }

                    if (linkedList.Count == 0)
                    {
                        groupsToRemove.Add(groupId);
                    }
                }

                foreach (var groupId in groupsToRemove)
                {
                    RemoveGroup(groupId);
                }
            }
            finally
            {
                _pools.ReleaseIntList(groupsToRemove);
            }
        }

        public void RebuildIndices(int[] indexMapping)
        {
            if (indexMapping == null)
                return;

            foreach (var linkedList in _itemsByGroup.Values)
            {
                var node = linkedList.First;
                while (node != null)
                {
                    int oldIndex = node.Value;
                    node.Value = oldIndex >= 0 && oldIndex < indexMapping.Length
                        ? indexMapping[oldIndex]
                        : -1;
                    node = node.Next;
                }
            }
        }

        public void Clear()
        {
            _itemsByGroup.Clear();
            _activeGroups.Clear();
        }
    }
}
