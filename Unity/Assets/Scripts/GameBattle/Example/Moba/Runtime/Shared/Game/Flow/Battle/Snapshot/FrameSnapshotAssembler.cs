using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 帧快照组装器
    /// 负责组装完整的帧快照数据
    /// 参考 view.runtime 的实现模式
    /// </summary>
    public sealed class FrameSnapshotAssembler : IFrameSnapshotAssembler
    {
        private int _currentFrame;
        private double _timestamp;

        private EnterGameData _enterGame;
        private bool _hasEnterGame;

        private readonly List<ActorTransformData> _actorTransforms = new List<ActorTransformData>();
        private readonly List<ProjectileEventData> _projectileEvents = new List<ProjectileEventData>();
        private readonly List<AreaEventData> _areaEvents = new List<AreaEventData>();
        private readonly List<DamageEventData> _damageEvents = new List<DamageEventData>();

        private StateHashData _stateHash;
        private bool _hasStateHash;

        private SnapshotType _snapshotType = SnapshotType.Full;

        /// <summary>
        /// 开始组装新帧
        /// </summary>
        public void BeginFrame(int frameIndex)
        {
            _currentFrame = frameIndex;
            _timestamp = Environment.TickCount / 1000.0;
            _snapshotType = SnapshotType.Delta;

            _hasEnterGame = false;
            _hasStateHash = false;

            _actorTransforms.Clear();
            _projectileEvents.Clear();
            _areaEvents.Clear();
            _damageEvents.Clear();
        }

        /// <summary>
        /// 设置进入游戏数据
        /// </summary>
        public void SetEnterGame(in EnterGameData data)
        {
            _enterGame = data;
            _hasEnterGame = true;
            _snapshotType = SnapshotType.Full;
        }

        /// <summary>
        /// 添加角色变换数据
        /// </summary>
        public void AddActorTransform(in ActorTransformData data)
        {
            _actorTransforms.Add(data);
        }

        /// <summary>
        /// 添加弹道事件数据
        /// </summary>
        public void AddProjectileEvent(in ProjectileEventData data)
        {
            _projectileEvents.Add(data);
        }

        /// <summary>
        /// 添加区域事件数据
        /// </summary>
        public void AddAreaEvent(in AreaEventData data)
        {
            _areaEvents.Add(data);
        }

        /// <summary>
        /// 添加伤害事件数据
        /// </summary>
        public void AddDamageEvent(in DamageEventData data)
        {
            _damageEvents.Add(data);
        }

        /// <summary>
        /// 设置状态哈希
        /// </summary>
        public void SetStateHash(in StateHashData data)
        {
            _stateHash = data;
            _hasStateHash = true;
        }

        /// <summary>
        /// 完成组装，获取完整快照
        /// </summary>
        public bool TryFinishFrame(out FrameSnapshotData snapshot)
        {
            var hasData = _hasEnterGame ||
                          _actorTransforms.Count > 0 ||
                          _projectileEvents.Count > 0 ||
                          _areaEvents.Count > 0 ||
                          _damageEvents.Count > 0 ||
                          _hasStateHash;

            if (!hasData)
            {
                snapshot = default;
                return false;
            }

            snapshot = new FrameSnapshotData(
                _currentFrame,
                _timestamp,
                _snapshotType,
                _hasEnterGame ? _enterGame : default,
                _actorTransforms.Count > 0 ? _actorTransforms : null,
                _projectileEvents.Count > 0 ? _projectileEvents : null,
                _areaEvents.Count > 0 ? _areaEvents : null,
                _damageEvents.Count > 0 ? _damageEvents : null,
                _hasStateHash ? _stateHash : default
            );

            return true;
        }

        /// <summary>
        /// 重置组装器
        /// </summary>
        public void Reset()
        {
            _currentFrame = 0;
            _timestamp = 0;
            _hasEnterGame = false;
            _hasStateHash = false;
            _actorTransforms.Clear();
            _projectileEvents.Clear();
            _areaEvents.Clear();
            _damageEvents.Clear();
            _snapshotType = SnapshotType.Full;
        }

        /// <summary>
        /// 获取当前帧索引
        /// </summary>
        public int CurrentFrame => _currentFrame;

        /// <summary>
        /// 获取已添加的角色变换数量
        /// </summary>
        public int ActorTransformCount => _actorTransforms.Count;

        /// <summary>
        /// 获取已添加的弹道事件数量
        /// </summary>
        public int ProjectileEventCount => _projectileEvents.Count;

        /// <summary>
        /// 获取已添加的区域事件数量
        /// </summary>
        public int AreaEventCount => _areaEvents.Count;

        /// <summary>
        /// 获取已添加的伤害事件数量
        /// </summary>
        public int DamageEventCount => _damageEvents.Count;
    }
}
