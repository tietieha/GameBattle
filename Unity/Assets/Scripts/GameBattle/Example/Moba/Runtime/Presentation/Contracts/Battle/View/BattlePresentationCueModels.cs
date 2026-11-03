using System;
using AbilityKit.Demo.Moba.View.Abstractions.Shared.Types;

namespace AbilityKit.Demo.Moba.View.Abstractions.Battle.View
{
    public readonly struct BattlePresentationCueRequestKey : IEquatable<BattlePresentationCueRequestKey>
    {
        private readonly string _externalKey;
        private readonly int _triggerKey;
        private readonly int _triggerEventId;
        private readonly int _actionIndex;
        private readonly int _order;
        private readonly int _sourceActorId;
        private readonly int _targetActorId;
        private readonly bool _hasExternalKey;
        private readonly bool _hasValue;

        public BattlePresentationCueRequestKey(string externalKey)
        {
            _externalKey = externalKey;
            _triggerKey = 0;
            _triggerEventId = 0;
            _actionIndex = 0;
            _order = 0;
            _sourceActorId = 0;
            _targetActorId = 0;
            _hasExternalKey = true;
            _hasValue = true;
        }

        private BattlePresentationCueRequestKey(
            int triggerKey,
            int triggerEventId,
            int actionIndex,
            int order,
            int sourceActorId,
            int targetActorId)
        {
            _externalKey = null;
            _triggerKey = triggerKey;
            _triggerEventId = triggerEventId;
            _actionIndex = actionIndex;
            _order = order;
            _sourceActorId = sourceActorId;
            _targetActorId = targetActorId;
            _hasExternalKey = false;
            _hasValue = true;
        }

        public bool IsEmpty => !_hasValue;

        public static BattlePresentationCueRequestKey From(in BattlePresentationCueData data)
        {
            if (!string.IsNullOrWhiteSpace(data.InstanceKey)) return FromExternal(data.InstanceKey);
            if (!string.IsNullOrWhiteSpace(data.RequestKey)) return FromExternal(data.RequestKey);

            return FromGenerated(
                data.TriggerId,
                data.TriggerEventId,
                data.ActionIndex,
                data.Order,
                data.SourceActorId,
                data.TargetActorId);
        }

        public static BattlePresentationCueRequestKey FromExternal(string externalKey)
        {
            return string.IsNullOrWhiteSpace(externalKey) ? default : new BattlePresentationCueRequestKey(externalKey);
        }

        public static BattlePresentationCueRequestKey FromGenerated(
            int triggerKey,
            int triggerEventId,
            int actionIndex,
            int order,
            int sourceActorId,
            int targetActorId)
        {
            return new BattlePresentationCueRequestKey(triggerKey, triggerEventId, actionIndex, order, sourceActorId, targetActorId);
        }

        public bool Equals(BattlePresentationCueRequestKey other)
        {
            if (_hasValue != other._hasValue) return false;
            if (!_hasValue) return true;
            if (_hasExternalKey != other._hasExternalKey) return false;
            if (_hasExternalKey) return string.Equals(_externalKey, other._externalKey, StringComparison.Ordinal);

            if (_triggerKey > 0 || other._triggerKey > 0)
            {
                return _triggerKey == other._triggerKey && _order == other._order;
            }

            if (_triggerEventId > 0 || other._triggerEventId > 0)
            {
                return _triggerEventId == other._triggerEventId && _order == other._order;
            }

            return _actionIndex == other._actionIndex
                && _order == other._order
                && _sourceActorId == other._sourceActorId
                && _targetActorId == other._targetActorId;
        }

        public override bool Equals(object obj)
        {
            return obj is BattlePresentationCueRequestKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            if (!_hasValue) return 0;
            if (_hasExternalKey) return _externalKey != null ? StringComparer.Ordinal.GetHashCode(_externalKey) : 0;

            unchecked
            {
                if (_triggerKey > 0)
                {
                    return (_triggerKey * 397) ^ _order;
                }

                if (_triggerEventId > 0)
                {
                    return (_triggerEventId * 397) ^ _order;
                }

                var hashCode = _actionIndex;
                hashCode = (hashCode * 397) ^ _order;
                hashCode = (hashCode * 397) ^ _sourceActorId;
                hashCode = (hashCode * 397) ^ _targetActorId;
                return hashCode;
            }
        }
    }

    public enum BattlePresentationCueDecisionKind
    {
        None = 0,
        Play = 1,
        Stop = 2,
    }

    public readonly struct BattlePresentationCueSpawnRequest
    {
        public BattlePresentationCueSpawnRequest(
            BattlePresentationCueRequestKey requestKey,
            int vfxId,
            int sourceActorId,
            int targetActorId,
            int firstTargetActorId,
            bool hasExplicitPosition,
            MobaFloat3 explicitPosition,
            MobaFloat3 offset)
        {
            RequestKey = requestKey;
            VfxId = vfxId;
            SourceActorId = sourceActorId;
            TargetActorId = targetActorId;
            FirstTargetActorId = firstTargetActorId;
            HasExplicitPosition = hasExplicitPosition;
            ExplicitPosition = explicitPosition;
            Offset = offset;
        }

        public BattlePresentationCueRequestKey RequestKey { get; }
        public int VfxId { get; }
        public int SourceActorId { get; }
        public int TargetActorId { get; }
        public int FirstTargetActorId { get; }
        public bool HasExplicitPosition { get; }
        public MobaFloat3 ExplicitPosition { get; }
        public MobaFloat3 Offset { get; }

        public bool IsEmpty => RequestKey.IsEmpty || VfxId <= 0;
    }

    public readonly struct BattlePresentationCueDecision
    {
        public BattlePresentationCueDecision(
            BattlePresentationCueDecisionKind kind,
            BattlePresentationCueRequestKey requestKey,
            BattlePresentationCueSpawnRequest spawnRequest)
        {
            Kind = kind;
            RequestKey = requestKey;
            SpawnRequest = spawnRequest;
        }

        public BattlePresentationCueDecisionKind Kind { get; }
        public BattlePresentationCueRequestKey RequestKey { get; }
        public BattlePresentationCueSpawnRequest SpawnRequest { get; }

        public bool IsNone => Kind == BattlePresentationCueDecisionKind.None || RequestKey.IsEmpty;

        public static BattlePresentationCueDecision None => default;
    }
}
