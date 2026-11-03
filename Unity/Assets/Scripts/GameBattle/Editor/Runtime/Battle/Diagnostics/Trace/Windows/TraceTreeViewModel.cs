using System.Collections.Generic;
using System.Linq;
using AbilityKit.Trace;

namespace AbilityKit.Trace.Editor.Windows
{
    /// <summary>
    /// 树节点视图数据
    /// </summary>
    public class TraceNodeViewData
    {
        public long ContextId { get; set; }
        public long RootId { get; set; }
        public long ParentId { get; set; }
        public int Kind { get; set; }
        public string KindName { get; set; }
        public int Level { get; set; }
        public int OrderInLevel { get; set; }
        public int ChildCount { get; set; }
        public bool IsEnded { get; set; }
        public bool IsRoot => ContextId == RootId;
        public object Metadata { get; set; }
    }

    /// <summary>
    /// 根节点视图数据
    /// </summary>
    public class TraceRootViewData
    {
        public long RootId { get; set; }
        public int Kind { get; set; }
        public string KindName { get; set; }
        public bool IsActive { get; set; }
        public int ActiveCount { get; set; }
        public int ExternalRefCount { get; set; }
        public int NodeCount { get; set; }
    }

    /// <summary>
    /// 溯源树窗口视图模型
    /// </summary>
    public class TraceTreeViewModel
    {
        private ITraceRegistryProvider _registryProvider;

        /// <summary>
        /// 活跃根节点列表
        /// </summary>
        public List<TraceRootViewData> ActiveRoots { get; private set; } = new List<TraceRootViewData>();

        /// <summary>
        /// 当前选中树的节点列表
        /// </summary>
        public List<TraceNodeViewData> CurrentNodes { get; private set; } = new List<TraceNodeViewData>();

        /// <summary>
        /// 当前选中的根节点ID
        /// </summary>
        public long SelectedRootId { get; private set; }

        /// <summary>
        /// 当前选中的节点
        /// </summary>
        public TraceNodeViewData SelectedNode { get; private set; }

        /// <summary>
        /// 总节点数
        /// </summary>
        public int TotalNodeCount { get; private set; }

        /// <summary>
        /// 设置注册表提供者
        /// </summary>
        public void SetRegistryProvider(ITraceRegistryProvider provider)
        {
            _registryProvider = provider;
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        public void Refresh()
        {
            ActiveRoots.Clear();
            TotalNodeCount = 0;

            var registries = GetRegistries();

            foreach (var registry in registries)
            {
                RefreshFromRegistry(registry);
            }

            // 如果当前选中的根节点不再活跃，清除选择
            if (SelectedRootId != 0 && !ActiveRoots.Any(r => r.RootId == SelectedRootId))
            {
                SelectedRootId = 0;
                SelectedNode = null;
                CurrentNodes.Clear();
            }

            // 如果选中了根节点但节点列表为空，刷新节点
            if (SelectedRootId != 0 && CurrentNodes.Count == 0)
            {
                RefreshCurrentTree();
            }
        }

        private IEnumerable<TraceTreeRegistryBase> GetRegistries()
        {
            if (_registryProvider != null)
            {
                return _registryProvider.GetRegistries();
            }

            return DefaultTraceRegistryProvider.Instance.GetRegistries();
        }

        private void RefreshFromRegistry(TraceTreeRegistryBase registry)
        {
            foreach (var root in registry.GetActiveRoots())
            {
                var kind = 0;
                if (registry.TryGetNodeSnapshot(root.RootId, out var rootSnapshot) && rootSnapshot.IsValid)
                    kind = rootSnapshot.Kind;

                var nodeCount = registry.GetNodeSnapshotsByRoot(root.RootId).Count();
                var rootData = new TraceRootViewData
                {
                    RootId = root.RootId,
                    Kind = kind,
                    KindName = GetKindName(kind, registry),
                    IsActive = root.ActiveCount > 0,
                    ActiveCount = root.ActiveCount,
                    ExternalRefCount = root.ExternalRefCount,
                    NodeCount = nodeCount
                };

                ActiveRoots.Add(rootData);
                TotalNodeCount += nodeCount;
            }
        }

        /// <summary>
        /// 选择根节点
        /// </summary>
        public void SelectRoot(long rootId)
        {
            if (SelectedRootId == rootId) return;

            SelectedRootId = rootId;
            SelectedNode = null;
            RefreshCurrentTree();
        }

        /// <summary>
        /// 选择节点
        /// </summary>
        public void SelectNode(long contextId)
        {
            SelectedNode = CurrentNodes.FirstOrDefault(n => n.ContextId == contextId);
        }

        /// <summary>
        /// 获取节点ID对应的视图数据
        /// </summary>
        public TraceNodeViewData GetNodeById(long contextId)
        {
            return CurrentNodes.FirstOrDefault(n => n.ContextId == contextId);
        }

        /// <summary>
        /// 刷新当前选中树的节点
        /// </summary>
        private void RefreshCurrentTree()
        {
            CurrentNodes.Clear();

            if (SelectedRootId == 0) return;

            var registries = GetRegistries();
            foreach (var registry in registries)
            {
                var snapshots = registry.GetNodeSnapshotsByRoot(SelectedRootId);
                var nodes = new List<TraceNodeViewData>();

                foreach (var snapshot in snapshots)
                {
                    var viewData = new TraceNodeViewData
                    {
                        ContextId = snapshot.ContextId,
                        RootId = snapshot.RootId,
                        ParentId = snapshot.ParentId,
                        Kind = snapshot.Kind,
                        KindName = GetKindName(snapshot.Kind, registry),
                        ChildCount = snapshot.ChildCount,
                        IsEnded = snapshot.IsEnded,
                        Metadata = snapshot.Metadata
                    };
                    nodes.Add(viewData);
                }

                if (nodes.Count > 0)
                {
                    BuildNodeHierarchy(nodes);
                    CurrentNodes = nodes;
                    return;
                }
            }
        }

        private void BuildNodeHierarchy(List<TraceNodeViewData> nodes)
        {
            // 创建节点映射
            var nodeMap = nodes.ToDictionary(n => n.ContextId);

            // 查找根节点
            var rootNode = nodes.FirstOrDefault(n => n.IsRoot);
            if (rootNode == null) return;

            // 使用 BFS 计算层级和同层顺序
            var queue = new Queue<long>();
            var levelMap = new Dictionary<long, int>();
            var orderMap = new Dictionary<long, int>();
            var levelCounters = new Dictionary<int, int>();

            levelMap[rootNode.ContextId] = 0;
            orderMap[rootNode.ContextId] = 0;
            levelCounters[0] = 1;

            // 获取子节点
            var childrenMap = new Dictionary<long, List<long>>();
            foreach (var node in nodes)
            {
                if (node.ParentId != 0)
                {
                    if (!childrenMap.TryGetValue(node.ParentId, out var children))
                    {
                        children = new List<long>();
                        childrenMap[node.ParentId] = children;
                    }
                    children.Add(node.ContextId);
                }
            }

            queue.Enqueue(rootNode.ContextId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var currentLevel = levelMap[currentId];
                var currentOrder = orderMap[currentId];

                if (nodeMap.TryGetValue(currentId, out var nodeView))
                {
                    nodeView.Level = currentLevel;
                    nodeView.OrderInLevel = currentOrder;
                }

                // 获取子节点
                if (childrenMap.TryGetValue(currentId, out var children))
                {
                    var nextLevel = currentLevel + 1;
                    for (int i = 0; i < children.Count; i++)
                    {
                        var childId = children[i];
                        if (!levelMap.ContainsKey(childId))
                        {
                            levelMap[childId] = nextLevel;

                            if (!levelCounters.TryGetValue(nextLevel, out var counter))
                            {
                                counter = 0;
                            }
                            orderMap[childId] = counter;
                            levelCounters[nextLevel] = counter + 1;

                            queue.Enqueue(childId);
                        }
                    }
                }
            }

            // 更新节点的子节点数量
            foreach (var node in nodes)
            {
                if (childrenMap.TryGetValue(node.ContextId, out var children))
                {
                    node.ChildCount = children.Count;
                }
            }
        }

        /// <summary>
        /// 获取节点类型的名称
        /// </summary>
        private string GetKindName(int kind, TraceTreeRegistryBase registry)
        {
            var name = registry?.GetKindName(kind);
            return string.IsNullOrEmpty(name) ? $"Kind_{kind}" : name;
        }
    }
}
