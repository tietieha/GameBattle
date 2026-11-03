using System;
using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Demo.Moba.Components;
using AbilityKit.Demo.Moba;
using AbilityKit.Demo.Moba.Services;
using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World;

namespace AbilityKit.Demo.Moba.Systems
{
    [WorldSystem(order: MobaSystemOrder.SkillPipelines + 1, Phase = WorldSystemPhase.Execute)]
    public sealed class MobaSkillCastInstanceSyncSystem : WorldSystemBase
    {
        private int _retainCompletedFrames = 30;
        private int _destroyConfirmGateFrames = 10;

        private SkillCastCoordinator _skills;
        private IFrameTime _time;
        private MobaAuthorityFrameService _authority;
        private MobaWorldSystemServices _systemServices;

        private global::Entitas.IGroup<global::ActorEntity> _actors;
        private global::ActorContext _actorContext;

        private readonly List<SkillPipelineRunner.RunningSnapshot> _buffer = new List<SkillPipelineRunner.RunningSnapshot>(8);
        private readonly List<SkillPipelineRunner.RunningSnapshot> _endedBuffer = new List<SkillPipelineRunner.RunningSnapshot>(4);
        private readonly HashSet<long> _seenThisFrame = new HashSet<long>();
        private readonly Dictionary<long, global::ActorEntity> _byInstanceId = new Dictionary<long, global::ActorEntity>();
        private readonly Dictionary<long, int> _lastSeenFrame = new Dictionary<long, int>();

        public MobaSkillCastInstanceSyncSystem(global::Entitas.IContexts contexts, IWorldResolver services)
            : base(contexts, services)
        {
        }

        protected override void OnInit()
        {
            Services.TryResolve(out _skills);
            Services.TryResolve(out _time);
            Services.TryResolve(out _authority);
            _systemServices = MobaWorldSystemExecution.Resolve(Services);

            if (Services.TryResolve<MobaSkillCastInstanceSyncSettings>(out var settings) && settings != null)
            {
                _retainCompletedFrames = settings.RetainCompletedFrames;
                _destroyConfirmGateFrames = settings.DestroyConfirmGateFrames;
            }

            _actorContext = Contexts.Actor();
            _actors = _actorContext.GetGroup(ActorMatcher.AllOf(ActorComponentsLookup.ActorId));
        }

        protected override void OnExecute()
        {
            MobaWorldSystemExecution.Require(
                _skills != null && _actors != null && _actorContext != null,
                Services,
                nameof(MobaSkillCastInstanceSyncSystem),
                "skill.cast.instance.sync",
                "SkillCastCoordinator, actor group and actor context",
                $"hasSkills={_skills != null}, hasActors={_actors != null}, hasActorContext={_actorContext != null}");

            var frame = ResolveFrame("skill.cast.instance.resolveFrame");

            var entities = _actors.GetEntities();
            if (entities == null || entities.Length == 0) return;

            _seenThisFrame.Clear();

            for (int i = 0; i < entities.Length; i++)
            {
                var actor = entities[i];
                if (actor == null || !actor.hasActorId) continue;

                _skills.FillRunningSnapshots(actor.actorId.Value, _buffer);
                UpsertSkillCastInstances(_buffer, frame, isRunning: true);
            }

            for (int i = 0; i < entities.Length; i++)
            {
                var actor = entities[i];
                if (actor == null || !actor.hasActorId) continue;

                _skills.FillEndedSnapshots(actor.actorId.Value, _endedBuffer);
                UpsertSkillCastInstances(_endedBuffer, frame, isRunning: false);
            }

            CleanupNotSeenRunningInstances();
        }

        private void UpsertSkillCastInstances(List<SkillPipelineRunner.RunningSnapshot> snapshots, int frame, bool isRunning)
        {
            if (snapshots == null || snapshots.Count == 0) return;

            for (int i = 0; i < snapshots.Count; i++)
            {
                UpsertSkillCastInstance(snapshots[i], frame, isRunning);
            }
        }

        private void UpsertSkillCastInstance(in SkillPipelineRunner.RunningSnapshot snapshot, int frame, bool isRunning)
        {
            if (snapshot.InstanceId == 0) return;

            _seenThisFrame.Add(snapshot.InstanceId);
            _lastSeenFrame[snapshot.InstanceId] = frame;

            if (!_byInstanceId.TryGetValue(snapshot.InstanceId, out var inst) || inst == null)
            {
                inst = _actorContext.CreateEntity();
                inst.AddSkillCastInstanceId(snapshot.InstanceId);
                _byInstanceId[snapshot.InstanceId] = inst;
            }

            if (!inst.hasSkillCastOwnerActorId) inst.AddSkillCastOwnerActorId(snapshot.OwnerActorId);
            else inst.ReplaceSkillCastOwnerActorId(snapshot.OwnerActorId);

            if (!inst.hasSkillCastSkillId) inst.AddSkillCastSkillId(snapshot.SkillId);
            else inst.ReplaceSkillCastSkillId(snapshot.SkillId);

            if (!inst.hasSkillCastSlot) inst.AddSkillCastSlot(snapshot.SkillSlot);
            else inst.ReplaceSkillCastSlot(snapshot.SkillSlot);

            if (!inst.hasSkillCastSkillLevel) inst.AddSkillCastSkillLevel(snapshot.SkillLevel);
            else inst.ReplaceSkillCastSkillLevel(snapshot.SkillLevel);

            if (!inst.hasSkillCastSequence) inst.AddSkillCastSequence(snapshot.Sequence);
            else inst.ReplaceSkillCastSequence(snapshot.Sequence);

            if (!inst.hasSkillCastStartFrame) inst.AddSkillCastStartFrame(snapshot.StartFrame);
            else if (inst.skillCastStartFrame.Value == 0 && snapshot.StartFrame != 0) inst.ReplaceSkillCastStartFrame(snapshot.StartFrame);

            if (!inst.hasSkillCastStage) inst.AddSkillCastStage(snapshot.Stage);
            else inst.ReplaceSkillCastStage(snapshot.Stage);

            if (!inst.hasSkillCastTargetActorId) inst.AddSkillCastTargetActorId(snapshot.TargetActorId);
            else inst.ReplaceSkillCastTargetActorId(snapshot.TargetActorId);

            if (!inst.hasSkillCastAim) inst.AddSkillCastAim(snapshot.AimPos, snapshot.AimDir);
            else inst.ReplaceSkillCastAim(snapshot.AimPos, snapshot.AimDir);

            if (!inst.hasSkillCastTimelineRuntime)
            {
                inst.AddSkillCastTimelineRuntime(snapshot.ElapsedMs, snapshot.NextEventIndex);
            }
            else
            {
                inst.ReplaceSkillCastTimelineRuntime(snapshot.ElapsedMs, snapshot.NextEventIndex);
            }

            inst.isSkillCastRunningTag = isRunning;

            if (isRunning && inst.hasSkillCastCancelRequest)
            {
                inst.RemoveSkillCastCancelRequest();
            }
        }

        private void CleanupNotSeenRunningInstances()
        {
            if (_byInstanceId.Count == 0) return;

            var frame = ResolveFrame("skill.cast.instance.cleanup.resolveFrame");

            List<long> stale = null;

            foreach (var kv in _byInstanceId)
            {
                var id = kv.Key;
                var e = kv.Value;
                if (e == null || !e.isEnabled)
                {
                    stale ??= new List<long>(4);
                    stale.Add(id);
                    continue;
                }

                if (_seenThisFrame.Contains(id)) continue;

                if (e.isSkillCastRunningTag)
                {
                    e.isSkillCastRunningTag = false;
                    if (!e.hasSkillCastStage)
                    {
                        e.AddSkillCastStage(SkillCastStage.Completed);
                    }
                    else
                    {
                        var stage = e.skillCastStage.Value;
                        if (stage == SkillCastStage.PreCast || stage == SkillCastStage.Cast || stage == SkillCastStage.Channeling)
                        {
                            e.ReplaceSkillCastStage(SkillCastStage.Completed);
                        }
                    }
                }

                var destroyReason = SkillDestroyReason.Unknown;
                if (e.hasSkillCastStage)
                {
                    switch (e.skillCastStage.Value)
                    {
                        case SkillCastStage.Failed:
                            destroyReason = SkillDestroyReason.FailedRetainExpired;
                            break;
                        case SkillCastStage.Cancelled:
                            destroyReason = SkillDestroyReason.CancelledRetainExpired;
                            break;
                        case SkillCastStage.Completed:
                        default:
                            destroyReason = SkillDestroyReason.CompletedRetainExpired;
                            break;
                    }
                }
                if (e.hasSkillCastCancelRequest && destroyReason != SkillDestroyReason.FailedRetainExpired)
                {
                    destroyReason = SkillDestroyReason.CancelledRetainExpired;
                }

                if (e.hasSkillCastCancelRequest) e.RemoveSkillCastCancelRequest();

                if (_retainCompletedFrames > 0 && frame > 0 && _lastSeenFrame.TryGetValue(id, out var last))
                {
                    if (frame - last > _retainCompletedFrames)
                    {
                        var requestFrame = frame;
                        var minConfirmed = requestFrame + _destroyConfirmGateFrames;
                        if (!e.hasSkillCastDestroyRequest)
                        {
                            e.AddSkillCastDestroyRequest(requestFrame, minConfirmed, destroyReason);
                        }
                        else
                        {
                            e.ReplaceSkillCastDestroyRequest(requestFrame, minConfirmed, destroyReason);
                        }
                    }
                }
            }

            if (stale != null)
            {
                for (int i = 0; i < stale.Count; i++)
                {
                    var id = stale[i];
                    _lastSeenFrame.Remove(id);
                    _seenThisFrame.Remove(id);
                    _byInstanceId.Remove(id);
                }
            }

            // Note: actual entity destruction is handled by MobaSkillCastDestroyCleanupSystem,
            // gated by authoritative confirmed frames.
        }

        private int ResolveFrame(string operation)
        {
            MobaWorldSystemExecution.Require(
                _authority != null || _time != null,
                Services,
                nameof(MobaSkillCastInstanceSyncSystem),
                operation,
                "MobaAuthorityFrameService or IFrameTime",
                $"hasAuthority={_authority != null}, hasFrameTime={_time != null}");

            if (_authority != null) return _authority.PredictedFrame.Value;
            return _time.Frame.Value;
        }

        private void ReportException(Exception ex, string operation, int actorId = 0, int skillId = 0, long runtimeId = 0L)
        {
            MobaWorldSystemExecution.HandleException(
                in _systemServices,
                ex,
                nameof(MobaSkillCastInstanceSyncSystem),
                operation,
                actorId: actorId,
                skillId: skillId,
                runtimeId: runtimeId);
        }
    }
}

