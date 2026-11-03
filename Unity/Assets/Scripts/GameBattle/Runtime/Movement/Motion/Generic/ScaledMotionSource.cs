using System;
using AbilityKit.Combat.MotionSystem.Core;
using AbilityKit.Core.Mathematics;
using AbilityKit.Core.Pooling;

namespace AbilityKit.Combat.MotionSystem.Generic
{
    public sealed class ScaledMotionSource : IMotionSource, IMotionSnapshotSource
    {
        private static readonly ObjectPool<ScaledMotionSource> Pool = Pools.GetPool(
            createFunc: () => new ScaledMotionSource(),
            onRelease: s => s.Reset(),
            defaultCapacity: 64,
            maxSize: 2048,
            collectionCheck: false);

        private IMotionSource _inner;
        private float _scale;
        private int _groupId;
        private MotionStacking _stacking;
        private int _priority;

        private ScaledMotionSource()
        {
            Reset();
        }

        public ScaledMotionSource(IMotionSource inner, float scale, int groupId = 0, MotionStacking stacking = MotionStacking.Additive, int priority = int.MinValue)
        {
            Configure(inner, scale, groupId, stacking, priority);
        }

        public static ScaledMotionSource Rent(IMotionSource inner, float scale, int groupId = 0, MotionStacking stacking = MotionStacking.Additive, int priority = int.MinValue)
        {
            var source = Pool.Get();
            source.Configure(inner, scale, groupId, stacking, priority);
            return source;
        }

        public static void Release(ScaledMotionSource source)
        {
            if (source == null) return;
            Pool.Release(source);
        }

        public void Configure(IMotionSource inner, float scale, int groupId = 0, MotionStacking stacking = MotionStacking.Additive, int priority = int.MinValue)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _scale = scale;
            _groupId = groupId != 0 ? groupId : inner.GroupId;
            _stacking = stacking;
            _priority = priority != int.MinValue ? priority : inner.Priority;
        }

        public void Reset()
        {
            _inner = null;
            _scale = 1f;
            _groupId = 0;
            _stacking = MotionStacking.Additive;
            _priority = 0;
        }

        public int GroupId => _groupId;

        public MotionStacking Stacking => _stacking;

        public int Priority => _priority;

        public bool IsActive => _inner != null && _inner.IsActive;

        public float Scale
        {
            get => _scale;
            set => _scale = value;
        }

        public void Tick(int id, ref MotionState state, float dt, ref Vec3 outDesiredDelta)
        {
            if (_inner == null) return;
            if (_scale == 0f) return;

            var tmp = Vec3.Zero;
            _inner.Tick(id, ref state, dt, ref tmp);
            outDesiredDelta = outDesiredDelta + tmp * _scale;
        }

        public void Cancel()
        {
            _inner?.Cancel();
        }

        public bool ExportSnapshot(out MotionSourceSnapshot snapshot)
        {
            snapshot = new MotionSourceSnapshot
            {
                GroupId = _groupId,
                Priority = _priority,
                Stacking = _stacking,
                IsActive = IsActive,
                Float0 = _scale,
            };
            return true;
        }

        public bool ImportSnapshot(in MotionSourceSnapshot snapshot)
        {
            _groupId = snapshot.GroupId;
            _priority = snapshot.Priority;
            _stacking = snapshot.Stacking;
            _scale = snapshot.Float0;
            return _inner != null;
        }
    }
}
