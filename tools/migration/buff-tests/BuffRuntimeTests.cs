using AbilityKit.Buffs;
using AbilityKit.Context;
using AbilityKit.Continuous;
using AbilityKit.Deterministic;
using Xunit;

namespace GameBattle.Buff.Tests;

public sealed class BuffRuntimeTests
{
    [Fact]
    public void Context_fields_are_declared_on_shared_state_not_moba_extension()
    {
        foreach (var name in new[] { "RuntimeContextId", "RuntimeContextVersion", "RuntimeContextIdentity" })
            Assert.Equal(typeof(BuffState), typeof(AbilityKit.Demo.Moba.Components.BuffRuntime).GetField(name)!.DeclaringType);
    }

    [Fact]
    public void Storage_matches_first_buff_source_or_explicit_instance()
    {
        var first = new BuffState { BuffId = 1, SourceId = 2, SourceContextId = 10 };
        var second = new BuffState { BuffId = 1, SourceId = 3, SourceContextId = 20 };
        var list = new List<BuffState> { first, second };
        var key = BuffRuntimeKey.MatchBuff(1);
        Assert.Equal(0, BuffStorage<BuffState>.FindExistingBuffIndex(list, in key));
        key = BuffRuntimeKey.MatchBuffAndSource(1, 3);
        Assert.Equal(1, BuffStorage<BuffState>.FindExistingBuffIndex(list, in key));
        key = BuffRuntimeKey.MatchInstance(1, 0, 20);
        Assert.Equal(1, BuffStorage<BuffState>.FindExistingBuffIndex(list, in key));
        key = BuffRuntimeKey.MatchInstance(1, 2, 20);
        Assert.Equal(-1, BuffStorage<BuffState>.FindExistingBuffIndex(list, in key));
    }

    [Fact]
    public void Storage_rebuilds_after_identity_change_and_refuses_stale_commit()
    {
        var state = new BuffState { BuffId = 1, SourceId = 2 };
        var list = new List<BuffState> { state };
        var key = BuffRuntimeKey.MatchBuffAndSource(1, 2);
        Assert.Equal(0, BuffStorage<BuffState>.FindExistingBuffIndex(list, in key));
        state.SourceId = 3;
        BuffStorage<BuffState>.MarkDirty(list);
        Assert.Equal(-1, BuffStorage<BuffState>.FindExistingBuffIndex(list, in key));
        Assert.False(BuffStorage<BuffState>.ReplaceAt(list, 0, new BuffState(), new BuffState()));
        Assert.False(BuffStorage<BuffState>.RemoveAt(list, 0, new BuffState()));
        Assert.Same(state, Assert.Single(list));
        BuffStorage<BuffState>.Forget(list);
    }

    [Fact]
    public void New_apply_initializes_and_same_source_refreshes_instead_of_allocating()
    {
        var port = new TestPort();
        var processor = new BuffApplyProcessor<int, TestState, int>(port);
        var list = new List<TestState>();
        var app = Application(BuffStackingPolicy.AddStack);
        Assert.True(processor.Apply(7, list, in app));
        var state = Assert.Single(list);
        Assert.Equal(1, state.StackCount);
        Assert.Equal(4, state.Remaining);
        Assert.Equal(0.5f, state.IntervalRemainingSeconds);
        Assert.True(processor.Apply(7, list, in app));
        Assert.Same(state, Assert.Single(list));
        Assert.Equal(2, state.StackCount);
        Assert.Equal(1, port.Rented);
        Assert.Equal(1, port.Refreshed);
    }

    [Fact]
    public void Force_new_and_different_sources_are_separate_instances()
    {
        var port = new TestPort();
        var processor = new BuffApplyProcessor<int, TestState, int>(port);
        var list = new List<TestState>();
        var first = Application(BuffStackingPolicy.AddStack);
        var forced = Application(BuffStackingPolicy.AddStack, force: true);
        var otherSource = Application(BuffStackingPolicy.AddStack, source: 9);
        Assert.True(processor.Apply(7, list, in first));
        Assert.True(processor.Apply(7, list, in forced));
        Assert.True(processor.Apply(7, list, in otherSource));
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void Failed_replacement_preserves_old_instance_and_returns_candidate()
    {
        var old = new TestState { BuffId = 1, SourceId = 2, StackCount = 3 };
        var list = new List<TestState> { old };
        var port = new TestPort { AllowCandidate = false };
        var processor = new BuffApplyProcessor<int, TestState, int>(port);
        var app = Application(BuffStackingPolicy.Replace);
        Assert.False(processor.Apply(7, list, in app));
        Assert.Same(old, Assert.Single(list));
        Assert.Equal(1, port.CleanedCandidates);
        Assert.Equal(0, port.Replaced);
        Assert.Equal(0, port.Added);
    }

    [Fact]
    public void Replacement_commits_before_old_cleanup_and_still_notifies_if_cleanup_throws()
    {
        var old = new TestState { BuffId = 1, SourceId = 2 };
        var list = new List<TestState> { old };
        var port = new TestPort { OnReplace = () => { Assert.DoesNotContain(old, list); throw new InvalidOperationException("old cleanup"); } };
        var processor = new BuffApplyProcessor<int, TestState, int>(port);
        var app = Application(BuffStackingPolicy.Replace);
        Assert.Equal("old cleanup", Assert.Throws<InvalidOperationException>(() => processor.Apply(7, list, in app)).Message);
        Assert.NotSame(old, Assert.Single(list));
        Assert.Equal(1, port.Added);
        Assert.Equal(0, port.CleanedCandidates);
    }

    [Fact]
    public void Candidate_exception_cleans_once_and_preserves_original_failure()
    {
        var port = new TestPort
        {
            OnPrepare = () => throw new InvalidOperationException("activation"),
            OnCandidateCleanup = () => throw new InvalidOperationException("cleanup"),
        };
        var processor = new BuffApplyProcessor<int, TestState, int>(port);
        var list = new List<TestState>();
        var app = Application(BuffStackingPolicy.AddStack);
        Assert.Equal("activation", Assert.Throws<InvalidOperationException>(() => processor.Apply(7, list, in app)).Message);
        Assert.Empty(list);
        Assert.Equal(1, port.CleanedCandidates);
    }

    [Fact]
    public void Failed_refresh_still_invalidates_identity_index()
    {
        var old = new TestState { BuffId = 1, SourceId = 2, SourceContextId = 5 };
        var list = new List<TestState> { old };
        var key = BuffRuntimeKey.MatchInstance(1, 2, 5);
        Assert.Equal(0, BuffStorage<TestState>.FindExistingBuffIndex(list, in key));
        var port = new TestPort { OnPrepareExisting = runtime => { runtime.SourceContextId = 8; return false; } };
        var processor = new BuffApplyProcessor<int, TestState, int>(port);
        var app = Application(BuffStackingPolicy.AddStack, context: 5);
        Assert.False(processor.Apply(7, list, in app));
        Assert.Equal(-1, BuffStorage<TestState>.FindExistingBuffIndex(list, in key));
        key = BuffRuntimeKey.MatchInstance(1, 2, 8);
        Assert.Equal(0, BuffStorage<TestState>.FindExistingBuffIndex(list, in key));
    }

    [Fact]
    public void Committed_notification_failure_does_not_recycle_active_candidate()
    {
        var port = new TestPort { OnAdded = () => throw new InvalidOperationException("notify") };
        var processor = new BuffApplyProcessor<int, TestState, int>(port);
        var app = Application(BuffStackingPolicy.AddStack);
        var list = new List<TestState>();
        Assert.Throws<InvalidOperationException>(() => processor.Apply(7, list, in app));
        Assert.Single(list);
        Assert.Equal(0, port.CleanedCandidates);
    }

    [Fact]
    public void End_commits_removal_then_runs_every_cleanup_step_even_after_failure()
    {
        var state = new TestState { BuffId = 1, SourceContextId = 8 };
        var list = new List<TestState> { state };
        var port = new TestPort { ThrowOnStop = true, OnRemoved = () => Assert.Empty(list) };
        var processor = new BuffEndProcessor<int, TestState, string>(port);
        Assert.Equal("stop", Assert.Throws<InvalidOperationException>(() => processor.End(7, list, 0, state, 2, "expired")).Message);
        Assert.Equal(new[] { "stop", "detach", "context", "owner:8", "removed", "source", "ended", "clear", "return" }, port.EndOrder);
        Assert.Empty(list);
        Assert.Equal(0, state.RuntimeContextId);
        Assert.Equal(0, state.SourceContextId);
        Assert.False(processor.End(7, list, 0, state, 2, "expired"));
    }

    [Fact]
    public void Queue_append_and_recursive_immediate_follow_the_same_budgeted_path()
    {
        var seen = new List<int>();
        BuffCommandQueue<int>? queue = null;
        queue = new BuffCommandQueue<int>(value =>
        {
            seen.Add(value);
            if (value == 1)
            {
                var sequence = queue!.Enqueue(3);
                Assert.False(queue.DrainImmediate(sequence, 4));
                Assert.Equal(0, queue.Drain(4));
            }
            return true;
        });
        queue.Enqueue(1);
        queue.Enqueue(2);
        Assert.Equal(2, queue.Drain(2));
        Assert.Equal(new[] { 1, 2 }, seen);
        Assert.Equal(1, queue.PendingCount);
        Assert.Equal(1, queue.Drain(2));
        Assert.Equal(new[] { 1, 2, 3 }, seen);
    }

    [Fact]
    public void Immediate_budget_failure_cancels_unexecuted_request_and_duplicate_tickets_count_once()
    {
        var queue = new BuffCommandQueue<int>(_ => true);
        queue.Enqueue(1);
        var deferred = queue.Enqueue(2);
        Assert.False(queue.DrainImmediate(deferred, 1));
        Assert.Equal(0, queue.PendingCount);
        var ticket = queue.Enqueue(3);
        Assert.Equal(1, queue.DrainImmediate(new[] { ticket, ticket }, 1));
    }

    [Fact]
    public void Throwing_exception_policy_never_replays_executed_commands()
    {
        var queue = new BuffCommandQueue<int>(_ => throw new InvalidOperationException("execute"),
            (_, _) => throw new InvalidOperationException("policy"));
        var ticket = queue.Enqueue(1);
        Assert.Equal("policy", Assert.Throws<InvalidOperationException>(() => queue.DrainImmediate(ticket, 1)).Message);
        Assert.False(queue.IsDraining);
        Assert.Equal(0, queue.PendingCount);
        Assert.Equal(0, queue.Drain(1));
        queue.Clear();
    }

    [Fact]
    public void Unhandled_command_failure_is_visible_and_consumed()
    {
        var queue = new BuffCommandQueue<int>(_ => throw new InvalidOperationException("execute"));
        queue.Enqueue(1);
        Assert.Throws<InvalidOperationException>(() => queue.Drain(1));
        Assert.Equal(0, queue.PendingCount);
        Assert.False(queue.IsDraining);
    }

    [Fact]
    public void Duration_projects_from_logic_elapsed_and_zero_duration_is_infinite()
    {
        var duration = new BuffDuration(2);
        Assert.Equal(1.5f, duration.GetRemainingSeconds(Raw(0.5f)));
        Assert.False(duration.HasExpired(Raw(1.5f)));
        Assert.True(duration.HasExpired(Raw(2)));
        Assert.Equal(0, duration.GetRemainingRaw(Raw(3)));
        duration.RefreshRaw(Raw(1));
        Assert.Equal(1, duration.GetRemainingSeconds(0));
        var infinite = new BuffDuration(0);
        Assert.Null(infinite.GetRemainingRaw(Raw(100)));
        Assert.False(infinite.HasExpired(Raw(100)));
        Assert.Equal(float.PositiveInfinity, infinite.GetRemainingSeconds(0));
    }

    [Fact]
    public void Periodic_tick_uses_raw_time_and_preserves_backlog_after_budget()
    {
        var port = new IntervalPort { RemainingRaw = Raw(1) };
        Assert.Equal(32, FixedIntervalTicker.Tick(ref port, Raw(40), Raw(1)));
        Assert.Equal(Raw(-7), port.RemainingRaw);
        Assert.Equal(9, FixedIntervalTicker.Tick(ref port, Raw(1), Raw(1)));
        Assert.Equal(Raw(1), port.RemainingRaw);
        Assert.Equal(41, port.Count);
    }

    [Fact]
    public void Periodic_tick_stops_when_callback_pauses_or_ends_runtime()
    {
        var port = new IntervalPort { RemainingRaw = Raw(1), StopAfter = 1 };
        Assert.Equal(1, FixedIntervalTicker.Tick(ref port, Raw(4), Raw(1)));
        var retained = port.RemainingRaw;
        Assert.Equal(0, FixedIntervalTicker.Tick(ref port, Raw(1), Raw(1)));
        Assert.Equal(retained, port.RemainingRaw);
    }

    [Theory]
    [InlineData(-10L, 3L, 2L)]
    [InlineData(0L, 3L, 3L)]
    [InlineData(4L, 3L, 1L)]
    public void Missing_context_normalizes_interval_without_executing(long remaining, long period, long expected)
    {
        Assert.Equal(expected, FixedIntervalTicker.NormalizeRemainder(remaining, period));
    }

    [Fact]
    public void Context_identity_is_created_bound_destroyed_and_restored_with_same_persisted_id()
    {
        var registry = new ContextRegistry();
        var owners = new Dictionary<long, BuffState>();
        var binding = new BuffContextBinding(registry, (id, state) => owners.TryGetValue(id, out var owner) && ReferenceEquals(owner, state));
        var state = new BuffState();
        var id = binding.EnsureIdentity(state);
        owners.Add(id, state);
        binding.BindLocalIdentity(state);
        Assert.Equal(1, state.RuntimeContextVersion);
        var identity = state.RuntimeContextIdentity;
        Assert.True(identity.IsValid);
        Assert.Equal(id, binding.EnsureIdentity(state));
        Assert.Throws<InvalidOperationException>(() => binding.EnsureIdentity(new BuffState { RuntimeContextId = id }));
        binding.DestroyIdentity(state, key => owners.Remove(key), preserveReference: true);
        Assert.False(registry.Exists(id));
        Assert.Equal(id, state.RuntimeContextId);
        var restored = new BuffState { RuntimeContextId = id, RuntimeContextVersion = 7 };
        Assert.Equal(id, binding.RestoreIdentity(restored));
        owners[id] = restored;
        binding.BindLocalIdentity(restored);
        Assert.Equal(7, restored.RuntimeContextVersion);
        Assert.NotEqual(identity, restored.RuntimeContextIdentity);
        binding.DestroyIdentity(restored, key => owners.Remove(key));
        Assert.Equal(0, restored.RuntimeContextId);
        Assert.False(restored.RuntimeContextIdentity.IsValid);
    }

    [Theory]
    [InlineData(0L, 1L)]
    [InlineData(-1L, 1L)]
    [InlineData(1L, 0L)]
    [InlineData(long.MaxValue, 1L)]
    public void Invalid_restored_context_reference_is_rejected(long id, long version) =>
        Assert.Throws<InvalidOperationException>(() => BuffContextBinding.ValidateRestoredReference(id, version));

    [Fact]
    public void Ordinary_entity_buff_runs_command_apply_logic_tick_interval_and_cleanup_without_unity()
    {
        var list = new List<TestState>();
        var port = new TestPort();
        var apply = new BuffApplyProcessor<int, TestState, int>(port);
        var end = new BuffEndProcessor<int, TestState, string>(port);
        var app = Application(BuffStackingPolicy.AddStack);
        var queue = new BuffCommandQueue<int>(_ => apply.Apply(7, list, in app));
        queue.Enqueue(1);
        Assert.Equal(1, queue.Drain(32));
        var state = Assert.Single(list);
        var lifetime = new LifetimePort(end);
        for (var frame = 0; frame < 8; frame++)
        {
            state.ElapsedRaw += Raw(0.5f);
            if (!state.Duration.HasExpired(state.ElapsedRaw))
            {
                var interval = new StateIntervalPort(state);
                FixedIntervalTicker.Tick(ref interval, Raw(0.5f), Raw(0.5f));
            }
            BuffLifetimeReconciler.Reconcile(list, ref lifetime);
        }
        Assert.Empty(list);
        Assert.Equal(7, state.IntervalCount);
        Assert.Contains("return", port.EndOrder);
    }

    private static BuffApplication<int> Application(BuffStackingPolicy policy, int source = 2, long context = 0, bool force = false)
    {
        var settings = new BuffStackingSettings(policy, BuffRefreshPolicy.ResetRemaining, 3, 500);
        return new BuffApplication<int>(1, source, 4, context, force, in settings, 0);
    }

    private static long Raw(float seconds) => Fixed64.FromSingle(seconds).RawValue;

    private sealed class TestState : BuffState
    {
        public BuffDuration Duration;
        public long ElapsedRaw;
        public long IntervalRaw;
        public int IntervalCount;
    }

    private sealed class TestPort : IBuffApplyPort<int, TestState, int>, IBuffEndPort<int, TestState, string>
    {
        public int Rented, Refreshed, Replaced, Added, CleanedCandidates;
        public bool AllowCandidate = true, ThrowOnStop;
        public Action? OnReplace, OnPrepare, OnCandidateCleanup, OnAdded, OnRemoved;
        public Func<TestState, bool>? OnPrepareExisting;
        public readonly List<string> EndOrder = new();
        public TestState Rent() { Rented++; return new TestState(); }
        public bool PrepareExisting(int target, TestState runtime, in BuffApplication<int> application) => OnPrepareExisting?.Invoke(runtime) ?? true;
        public bool CompleteExisting(int target, TestState runtime, in BuffApplication<int> application, int oldStackCount, in BuffStackingApplyResult result)
        {
            Refreshed++;
            if (result.ShouldResetInterval)
            {
                var settings = application.Settings;
                BuffStackingPolicyApplier.ResetInterval(runtime, in settings);
            }
            return true;
        }
        public bool PrepareCandidate(int target, TestState runtime, in BuffApplication<int> application)
        {
            OnPrepare?.Invoke();
            runtime.Duration = new BuffDuration(application.DurationSeconds);
            runtime.IntervalRaw = Raw(runtime.IntervalRemainingSeconds);
            return AllowCandidate;
        }
        public void CleanupCandidate(int target, TestState runtime) { CleanedCandidates++; OnCandidateCleanup?.Invoke(); }
        public void EndReplaced(int target, TestState runtime) { Replaced++; OnReplace?.Invoke(); }
        public void NotifyAdded(int target, TestState runtime, in BuffApplication<int> application) { Added++; OnAdded?.Invoke(); }
        public void StopContinuous(int target, TestState runtime, string reason) { EndOrder.Add("stop"); if (ThrowOnStop) throw new InvalidOperationException("stop"); }
        public void DetachContinuous(int target, TestState runtime) => EndOrder.Add("detach");
        public void EndContext(TestState runtime, string reason) => EndOrder.Add("context");
        public void ClearOwnerBindings(int target, long sourceContextId) => EndOrder.Add("owner:" + sourceContextId);
        public void NotifyRemoved(int target, TestState runtime, int sourceActorId, string reason) { EndOrder.Add("removed"); OnRemoved?.Invoke(); }
        public void ReleaseSource(TestState runtime) => EndOrder.Add("source");
        public void NotifyEnded(TestState runtime) => EndOrder.Add("ended");
        public void Clear(TestState runtime) { EndOrder.Add("clear"); runtime.ClearRuntimeContext(); runtime.SourceContextId = 0; }
        public void Return(TestState runtime) => EndOrder.Add("return");
    }

    private struct IntervalPort : IFixedIntervalTickPort
    {
        public long RemainingRaw { get; set; }
        public int Count, StopAfter;
        public bool CanExecute => StopAfter == 0 || Count < StopAfter;
        public void OnInterval() => Count++;
    }

    private readonly struct StateIntervalPort : IFixedIntervalTickPort
    {
        private readonly TestState _state;
        public StateIntervalPort(TestState state) { _state = state; }
        public long RemainingRaw { get => _state.IntervalRaw; set => _state.IntervalRaw = value; }
        public bool CanExecute => !_state.Duration.HasExpired(_state.ElapsedRaw);
        public void OnInterval() => _state.IntervalCount++;
    }

    private readonly struct LifetimePort : IBuffLifetimePort<TestState>
    {
        private readonly BuffEndProcessor<int, TestState, string> _end;
        public LifetimePort(BuffEndProcessor<int, TestState, string> end) { _end = end; }
        public bool ShouldInterrupt(TestState runtime) => false;
        public void Synchronize(TestState runtime) => runtime.Remaining = runtime.Duration.GetRemainingSeconds(runtime.ElapsedRaw);
        public bool HasEnded(TestState runtime) => runtime.Duration.HasExpired(runtime.ElapsedRaw);
        public void End(List<TestState> list, int index, TestState runtime, bool interrupted) =>
            _end.End(7, list, index, runtime, runtime.SourceId, "expired");
    }
}
