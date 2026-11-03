using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using AbilityKit.Demo.Moba.Config.BattleDemo.MO;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Demo.Moba.Components;
using AbilityKit.Demo.Moba.Services.Buffs.Core;
using AbilityKit.Demo.Moba.Services.Buffs.Runtime;
using AbilityKit.Demo.Moba.Services.Buffs.Tagging;
using AbilityKit.Demo.Moba.Services;
using AbilityKit.GameplayTags;
using SharedApplyProcessor = AbilityKit.Buffs.BuffApplyProcessor<global::ActorEntity, AbilityKit.Demo.Moba.Components.BuffRuntime, AbilityKit.Demo.Moba.Services.Buffs.Core.BuffOperationContext>;
using SharedApplication = AbilityKit.Buffs.BuffApplication<AbilityKit.Demo.Moba.Services.Buffs.Core.BuffOperationContext>;

namespace AbilityKit.Demo.Moba.Services.Buffs.Lifecycle
{
    /// <summary>
    /// Buff 应用流程：负责 Apply 阶段的配置校验、运行时选择、叠层执行、上下文/持续行为/触发计划绑定。
    /// </summary>
    internal sealed class BuffApplyFlow
    {
        private readonly MobaConfigDatabase _configs;
        private readonly MobaActorLookupService _actors;
        private readonly IMobaEffectiveTagQueryService _tags;
        private readonly IMobaContinuousTagTemplateRegistry _tagTemplates;
        private readonly BuffRepository _repo;
        private readonly BuffContextRegistry _ctx;
        private readonly BuffRuntimeBindingCoordinator _bindings;
        private readonly BuffEndFlow _endFlow;
        private readonly BuffLifecycleNotifier _notifier;
        private readonly MobaSkillParamModifierService _paramModifiers;
        private readonly MobaSkillCastRuntimeService _skillRuntimes;
        private readonly SharedApplyProcessor _processor;

        public BuffApplyFlow(
            MobaConfigDatabase configs,
            MobaActorLookupService actors,
            IMobaEffectiveTagQueryService tags,
            IMobaContinuousTagTemplateRegistry tagTemplates,
            BuffRepository repo,
            BuffContextRegistry ctx,
            BuffStackingPolicyApplier stacking,
            BuffRuntimeBindingCoordinator bindings,
            BuffEndFlow endFlow,
            BuffLifecycleNotifier notifier,
            MobaSkillParamModifierService paramModifiers = null,
            MobaSkillCastRuntimeService skillRuntimes = null)
        {
            _configs = configs;
            _actors = actors;
            _tags = tags;
            _tagTemplates = tagTemplates;
            _repo = repo ?? new BuffRepository();
            _ctx = ctx;
            _bindings = bindings;
            _endFlow = endFlow;
            _notifier = notifier;
            _paramModifiers = paramModifiers;
            _skillRuntimes = skillRuntimes;
            _processor = new SharedApplyProcessor(new ApplyPort(this));
        }

        public BuffLifecycleRejectResult LastReject { get; private set; }

        [MobaModifierConsumes(MobaSkillParamModifierKeys.Buff.Ids.DurationMs,
            MobaParameterStage.BuffApply, MobaModifierSampleTiming.CreateSnapshot)]
        public bool Apply(in BuffApplyRequest request)
        {
            LastReject = BuffLifecycleRejectResult.None;
            if (!request.IsValid) return Reject(BuffLifecycleRejectCode.ApplyInvalidRequest, $"apply request is invalid. target={request.TargetActorId} buffId={request.BuffId} source={request.SourceActorId}.");
            if (_configs == null) return Reject(BuffLifecycleRejectCode.ApplyConfigDatabaseMissing, $"config database is missing. target={request.TargetActorId} buffId={request.BuffId} source={request.SourceActorId}.");
            if (!_configs.TryGetBuff(request.BuffId, out var buff) || buff == null) return Reject(BuffLifecycleRejectCode.ApplyConfigNotFound, $"buff config not found. target={request.TargetActorId} buffId={request.BuffId} source={request.SourceActorId}.");

            if (!TryGetTarget(request.TargetActorId, out var target)) return Reject(BuffLifecycleRejectCode.ApplyTargetNotFound, $"target actor not found. target={request.TargetActorId} buffId={request.BuffId} source={request.SourceActorId}.");

            var targetActorId = request.TargetActorId;
            var requirements = BuffTagLifecycle.ResolveRequirements(buff, _tagTemplates);
            if (!BuffTagLifecycle.CanActivate(_tags, targetActorId, requirements)) return Reject(BuffLifecycleRejectCode.ApplyTagRequirementsBlocked, $"tag requirements blocked activation. target={targetActorId} buffId={request.BuffId} source={request.SourceActorId}.");

            var list = _repo.GetOrCreateList(target);
            if (list == null) return Reject(BuffLifecycleRejectCode.ApplyRuntimeListUnavailable, $"buff runtime list is unavailable. target={targetActorId} buffId={request.BuffId} source={request.SourceActorId}.");

            var duration = request.DurationOverrideMs > 0 ? request.DurationOverrideMs : buff.DurationMs;
            if (_paramModifiers != null && request.SourceActorId > 0)
            {
                var skillHandle = request.SkillRuntimeHandle;
                var record = MobaParameterRecord.FromSkillRuntime(MobaParameterStage.BuffApply,
                    _skillRuntimes, in skillHandle,
                    projectileTemplateId: request.Origin.ProjectileTemplateId,
                    buffTemplateId: request.BuffId);
                duration = _paramModifiers.Buff.ResolveDurationMs(request.SourceActorId, duration, in record);
            }
            if (duration < 0)
                return Reject(BuffLifecycleRejectCode.ApplyInvalidRequest,
                    $"resolved buff duration is negative. buffId={request.BuffId} durationMs={duration}.");
            var context = new BuffOperationContext
            {
                ApplyRequest = request,
                Buff = buff,
                TargetActorId = targetActorId,
                DurationSeconds = duration > 0 ? duration / 1000f : 0f,
                Requirements = requirements,
            };

            var settings = new AbilityKit.Buffs.BuffStackingSettings(
                (AbilityKit.Buffs.BuffStackingPolicy)(int)buff.StackingPolicy,
                (AbilityKit.Buffs.BuffRefreshPolicy)(int)buff.RefreshPolicy, buff.MaxStacks, buff.IntervalMs);
            var application = new SharedApplication(buff.Id, request.SourceActorId, context.DurationSeconds,
                request.SourceContextId, request.ForceNewInstance, in settings, context);
            var applied = _processor.Apply(target, list, in application);
            if (!applied && !LastReject.HasValue)
                return Reject(BuffLifecycleRejectCode.ApplyReplacementCommitFailed, "replacement commit failed.");
            return applied;
        }

        private bool PrepareExisting(BuffRuntime runtime, in SharedApplication application)
        {
            var context = application.Payload;
            var buff = context.Buff;
            var request = context.ApplyRequest;
            _ctx?.EnsureBuffContext(runtime, buff.Id, request.SourceActorId, context.TargetActorId, request.Origin);
            if (!_bindings.BindSkillRuntime(runtime, in request))
            {
                return Reject(BuffLifecycleRejectCode.ApplySkillRuntimeBindingFailed, $"skill runtime binding failed for existing buff. target={context.TargetActorId} buffId={buff.Id} source={request.SourceActorId} sourceContextId={runtime.SourceContextId}.");
            }
            return true;
        }

        private bool CompleteExisting(BuffRuntime runtime, in SharedApplication application, int oldStackCount,
            in AbilityKit.Buffs.BuffStackingApplyResult stackingResult)
        {
            var context = application.Payload;
            var buff = context.Buff;
            var request = context.ApplyRequest;
            var runtimeState = new BuffRuntimeView(runtime);
            var applied = stackingResult.Applied;
            _endFlow.NotifyLifecycle(runtime, MobaRuntimeLifecycleEventKind.Activated, "buff.lifecycle.active");
            if (applied || runtime.TagRequirements == null)
            {
                runtimeState.SetTagRequirements(context.Requirements);
            }

            if (applied && !EnsureContinuousRuntime(runtime, buff, request.SourceActorId, context.TargetActorId, context.DurationSeconds, context.Requirements))
            {
                return Reject(BuffLifecycleRejectCode.ApplyContinuousActivationFailed, $"continuous runtime activation failed for existing buff. target={context.TargetActorId} buffId={buff.Id} source={request.SourceActorId} sourceContextId={runtime.SourceContextId}.");
            }

            if (stackingResult.ShouldResetInterval)
            {
                BuffStackingPolicyApplier.ResetInterval(runtime, buff);
            }

            _ctx?.BindRuntimeContext(runtime, context.TargetActorId, MobaRuntimeContextLifecycleState.Refreshed);
            _notifier.AppliedExisting(buff, request.SourceActorId, context.TargetActorId, context.DurationSeconds, runtime, oldStackCount, applied);
            return true;
        }

        private bool PrepareCandidate(BuffRuntime runtime, in SharedApplication application)
        {
            var context = application.Payload;
            var buff = context.Buff;
            var request = context.ApplyRequest;
            _ctx?.EnsureBuffContext(runtime, buff.Id, request.SourceActorId, context.TargetActorId, request.Origin);
            if (!_bindings.BindSkillRuntime(runtime, in request))
            {
                return Reject(BuffLifecycleRejectCode.ApplySkillRuntimeBindingFailed, $"skill runtime binding failed for buff candidate. target={context.TargetActorId} buffId={buff.Id} source={request.SourceActorId} sourceContextId={runtime.SourceContextId}.");
            }

            new BuffRuntimeView(runtime).SetTagRequirements(context.Requirements);
            if (!EnsureContinuousRuntime(runtime, buff, request.SourceActorId, context.TargetActorId, context.DurationSeconds, context.Requirements))
            {
                return Reject(BuffLifecycleRejectCode.ApplyContinuousActivationFailed, $"continuous runtime activation failed for buff candidate. target={context.TargetActorId} buffId={buff.Id} source={request.SourceActorId} sourceContextId={runtime.SourceContextId}.");
            }

            _ctx?.BindRuntimeContext(runtime, context.TargetActorId, MobaRuntimeContextLifecycleState.Active);
            return true;
        }

        private void CleanupFailedCandidate(global::ActorEntity target, int targetActorId, BuffRuntime runtime)
        {
            if (runtime == null) return;

            AbilityKit.Buffs.BuffCleanupPipeline.Run(
                () => _bindings?.EndContinuous(runtime, MobaExecutionEndReason.Failed),
                () => _bindings?.CleanupContinuous(target, targetActorId, runtime, invalidateTags: false),
                () => _ctx?.CancelAndEnd(runtime),
                () => _endFlow.ReleaseSkillRuntime(runtime),
                () => _endFlow.NotifyLifecycle(runtime, MobaRuntimeLifecycleEventKind.Failed, "buff.lifecycle.activateFailed"),
                () => new BuffRuntimeView(runtime).ClearRuntimeBindings(),
                () => BuffRepository.ReleaseRuntime(runtime));
        }

        private bool EnsureContinuousRuntime(BuffRuntime runtime, BuffMO buff, int sourceActorId, int targetActorId, float remainingSeconds, ContinuousTagRequirements requirements)
        {
            return _bindings == null || _bindings.EnsureContinuousRuntime(runtime, buff, sourceActorId, targetActorId, remainingSeconds, requirements);
        }

        private bool TryGetTarget(int actorId, out global::ActorEntity target)
        {
            target = null;
            if (actorId <= 0) return false;
            if (_actors == null) return false;
            return _actors.TryGetActorEntity(actorId, out target) && target != null && target.hasActorId;
        }

        private sealed class ApplyPort : AbilityKit.Buffs.IBuffApplyPort<global::ActorEntity, BuffRuntime, BuffOperationContext>
        {
            private readonly BuffApplyFlow _owner;
            public ApplyPort(BuffApplyFlow owner) { _owner = owner; }
            public BuffRuntime Rent() => BuffRepository.RentRuntime();
            public bool PrepareExisting(global::ActorEntity target, BuffRuntime runtime, in SharedApplication application) =>
                _owner.PrepareExisting(runtime, in application);
            public bool CompleteExisting(global::ActorEntity target, BuffRuntime runtime, in SharedApplication application,
                int oldStackCount, in AbilityKit.Buffs.BuffStackingApplyResult result) =>
                _owner.CompleteExisting(runtime, in application, oldStackCount, in result);
            public bool PrepareCandidate(global::ActorEntity target, BuffRuntime runtime, in SharedApplication application) =>
                _owner.PrepareCandidate(runtime, in application);
            public void CleanupCandidate(global::ActorEntity target, BuffRuntime runtime) =>
                _owner.CleanupFailedCandidate(target, target.actorId.Value, runtime);
            public void EndReplaced(global::ActorEntity target, BuffRuntime runtime) =>
                _owner._endFlow.EndCommittedRuntime(target, runtime, runtime.SourceId, MobaExecutionEndReason.Replaced);
            public void NotifyAdded(global::ActorEntity target, BuffRuntime runtime, in SharedApplication application)
            {
                var context = application.Payload;
                AbilityKit.Buffs.BuffCleanupPipeline.Run(
                    () => _owner._endFlow.NotifyLifecycle(runtime, MobaRuntimeLifecycleEventKind.Activated, "buff.lifecycle.active"),
                    () => _owner._notifier.AppliedNew(context.Buff, context.ApplyRequest.SourceActorId,
                        context.TargetActorId, context.DurationSeconds, runtime));
            }
        }

        private bool Reject(BuffLifecycleRejectCode code, string message)
        {
            LastReject = new BuffLifecycleRejectResult(code, message);
            return false;
        }
    }
}
