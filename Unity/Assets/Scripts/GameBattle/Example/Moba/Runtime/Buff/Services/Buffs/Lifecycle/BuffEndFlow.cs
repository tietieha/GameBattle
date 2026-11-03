using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using AbilityKit.Core.Logging;
using AbilityKit.Demo.Moba.Components;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Demo.Moba.Services.Buffs.Core;
using AbilityKit.Demo.Moba.Services.Buffs.Presentation;
using AbilityKit.Demo.Moba.Services.Buffs.Runtime;
using AbilityKit.Demo.Moba.Services.Buffs.Triggering;

namespace AbilityKit.Demo.Moba.Services.Buffs.Lifecycle
{
    /// <summary>
    /// Buff 结束流程：集中维护持续行为停止、上下文收尾、事件表现派发、技能运行时释放和对象池回收顺序。
    /// </summary>
    internal sealed class BuffEndFlow : AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>
    {
        private readonly MobaConfigDatabase _configs;
        private readonly BuffContextRegistry _ctx;
        private readonly BuffLifecycleNotifier _notifier;
        private readonly BuffRuntimeBindingCoordinator _bindings;
        private readonly AbilityKit.Buffs.BuffEndProcessor<global::ActorEntity, BuffRuntime, MobaExecutionEndReason> _processor;

        public BuffEndFlow(
            MobaConfigDatabase configs,
            BuffContextRegistry ctx,
            BuffLifecycleNotifier notifier,
            BuffRuntimeBindingCoordinator bindings)
        {
            _configs = configs;
            _ctx = ctx;
            _notifier = notifier;
            _bindings = bindings;
            _processor = new AbilityKit.Buffs.BuffEndProcessor<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>(this);
        }

        public bool EndRuntime(global::ActorEntity target, List<BuffRuntime> list, int index, BuffRuntime runtime, int sourceActorId, MobaExecutionEndReason reason)
        {
            if (target == null || !target.hasActorId || runtime == null) return false;
            if (!BuffRepository.RemoveAt(list, index, runtime)) return false;

            EndCommittedRuntime(target, runtime, sourceActorId, reason);
            return true;
        }

        /// <summary>
        /// 清理已从仓库移除或已被替换的运行时。调用前必须完成容器提交。
        /// </summary>
        public void EndCommittedRuntime(global::ActorEntity target, BuffRuntime runtime, int sourceActorId, MobaExecutionEndReason reason)
        {
            if (target == null || !target.hasActorId || runtime == null) return;

            var targetActorId = target.actorId.Value;
            var normalizedReason = reason == MobaExecutionEndReason.None ? MobaExecutionEndReason.Expired : reason;
            var buffId = runtime.BuffId;
            var sourceContextId = runtime.SourceContextId;
            var hadContinuous = runtime.Continuous != null;
            var hadSkillRuntimeRetain = runtime.SkillRuntimeRetainHandle.IsValid;
            var hadModifierBindings = runtime.ModifierBindings != null && runtime.ModifierBindings.Count > 0;
            try
            {
                _processor.EndCommitted(target, runtime, sourceActorId, normalizedReason);
            }
            finally
            {
                LogBuffCleanup(buffId, targetActorId, sourceActorId, sourceContextId, normalizedReason,
                    hadContinuous, true, hadSkillRuntimeRetain, true, hadModifierBindings, true, true);
            }
        }

        public void CleanupOwnerBindings(global::ActorEntity target, long ownerKey)
        {
            if (target == null) return;
            if (ownerKey == 0) return;
 
            RemoveEffectListeners(target, ownerKey);
        }

        public void ReleaseSkillRuntime(BuffRuntime runtime)
        {
            _bindings?.ReleaseSkillRuntime(runtime);
        }

        public void NotifyLifecycle(BuffRuntime runtime, MobaRuntimeLifecycleEventKind kind, string reason)
        {
            _bindings?.NotifyLifecycle(runtime, kind, reason);
        }

        private void NotifyRemoved(int targetActorId, int sourceActorId, BuffRuntime runtime, MobaExecutionEndReason reason)
        {
            if (_configs == null) return;
            if (runtime == null) return;
            if (!_configs.TryGetBuff(runtime.BuffId, out var buff) || buff == null) return;

            _notifier?.Removed(buff, sourceActorId, targetActorId, runtime, reason);
        }

        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.StopContinuous(
            global::ActorEntity target, BuffRuntime runtime, MobaExecutionEndReason reason) => _bindings?.EndContinuous(runtime, reason);
        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.DetachContinuous(
            global::ActorEntity target, BuffRuntime runtime) => _bindings?.CleanupContinuous(target, target.actorId.Value, runtime, invalidateTags: true);
        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.EndContext(
            BuffRuntime runtime, MobaExecutionEndReason reason) => _ctx?.EndByRuntimeNoClear(runtime, reason);
        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.ClearOwnerBindings(
            global::ActorEntity target, long sourceContextId) => CleanupOwnerBindings(target, sourceContextId);
        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.NotifyRemoved(
            global::ActorEntity target, BuffRuntime runtime, int sourceActorId, MobaExecutionEndReason reason) =>
            NotifyRemoved(target.actorId.Value, sourceActorId, runtime, reason);
        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.ReleaseSource(
            BuffRuntime runtime) => ReleaseSkillRuntime(runtime);
        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.NotifyEnded(
            BuffRuntime runtime) => NotifyLifecycle(runtime, MobaRuntimeLifecycleEventKind.Ended, "buff.lifecycle.ended");
        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.Clear(
            BuffRuntime runtime) => new BuffRuntimeView(runtime).ClearRuntimeBindings();
        void AbilityKit.Buffs.IBuffEndPort<global::ActorEntity, BuffRuntime, MobaExecutionEndReason>.Return(
            BuffRuntime runtime) => BuffRepository.ReleaseRuntime(runtime);

        private static void LogBuffCleanup(int buffId, int targetActorId, int sourceActorId, long sourceContextId, MobaExecutionEndReason reason, bool hadContinuous, bool continuousCleared, bool hadSkillRuntimeRetain, bool skillRuntimeCleared, bool hadModifierBindings, bool modifierBindingsCleared, bool removedFromList)
        {
            if (IsExpectedLifecycleEnd(reason)) return;

            Log.Warning($"[MobaBuffCleanup] buff ended unexpectedly. buffId={buffId}, target={targetActorId}, source={sourceActorId}, sourceContextId={sourceContextId}, reason={reason}, hadContinuous={hadContinuous}, continuousCleared={continuousCleared}, hadSkillRuntimeRetain={hadSkillRuntimeRetain}, skillRuntimeCleared={skillRuntimeCleared}, hadModifierBindings={hadModifierBindings}, modifierBindingsCleared={modifierBindingsCleared}, removedFromList={removedFromList}");
        }

        private static bool IsExpectedLifecycleEnd(MobaExecutionEndReason reason)
        {
            return reason == MobaExecutionEndReason.Expired || reason == MobaExecutionEndReason.Completed;
        }

        private static void RemoveEffectListeners(global::ActorEntity e, long ownerKey)
        {
            if (e == null) return;
            if (ownerKey == 0) return;
            if (!e.hasEffectListeners) return;

            var listeners = e.effectListeners.Active;
            if (listeners == null || listeners.Count == 0) return;

            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                var listener = listeners[i];
                if (listener == null) continue;
                if (listener.SourceContextId != ownerKey) continue;
                listeners.RemoveAt(i);
            }
        }
    }
}
