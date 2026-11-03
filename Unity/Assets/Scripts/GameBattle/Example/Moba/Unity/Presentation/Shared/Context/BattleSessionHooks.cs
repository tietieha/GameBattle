using System;
using System.Collections.Generic;
using AbilityKit.Ability.Host.Hooks;
using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Game.Flow.Battle.Modules
{
    public readonly struct ViewBinderReadyEvent
    {
        public readonly bool IsConfirmed;
        public readonly WorldId WorldId;

        public ViewBinderReadyEvent(bool isConfirmed, WorldId worldId)
        {
            IsConfirmed = isConfirmed;
            WorldId = worldId;
        }
    }

    public readonly struct ViewsReboundEvent
    {
        public readonly bool IsConfirmed;
        public readonly WorldId WorldId;
        public readonly int Frame;

        public ViewsReboundEvent(bool isConfirmed, WorldId worldId, int frame)
        {
            IsConfirmed = isConfirmed;
            WorldId = worldId;
            Frame = frame;
        }
    }

    public readonly struct ViewFrameAlignedEvent
    {
        public readonly bool IsConfirmed;
        public readonly WorldId WorldId;
        public readonly int Frame;

        public ViewFrameAlignedEvent(bool isConfirmed, WorldId worldId, int frame)
        {
            IsConfirmed = isConfirmed;
            WorldId = worldId;
            Frame = frame;
        }
    }

    public sealed class BattleSessionHooks
    {
        public readonly Hook<float> PreTick = new Hook<float>();
        public readonly Hook<float> PostTick = new Hook<float>();

        public readonly InterceptHook<BattleStartPlan> PlanBuilt = new InterceptHook<BattleStartPlan>();
        public readonly Hook<BattleStartPlan> SessionStarted = new Hook<BattleStartPlan>();
        public readonly Hook<Exception> SessionFailed = new Hook<Exception>();
        public readonly Hook FirstFrameReceived = new Hook();

        public readonly Hook SessionStarting = new Hook();
        public readonly Hook SessionStopping = new Hook();

        public readonly Hook<ViewBinderReadyEvent> ViewBinderReady = new Hook<ViewBinderReadyEvent>();
        public readonly Hook<ViewsReboundEvent> ViewsRebound = new Hook<ViewsReboundEvent>();
        public readonly Hook<ViewFrameAlignedEvent> ViewFrameAligned = new Hook<ViewFrameAlignedEvent>();
    }

    public sealed class InterceptHook<T>
    {
        private readonly List<Func<T, bool>> _handlers = new List<Func<T, bool>>(4);

        public void Add(Func<T, bool> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _handlers.Add(handler);
        }

        public bool Remove(Func<T, bool> handler)
        {
            if (handler == null) return false;
            return _handlers.Remove(handler);
        }

        public bool Invoke(T arg)
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i]?.Invoke(arg) == true) return true;
            }

            return false;
        }
    }
}
