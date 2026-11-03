using AbilityKit.Battle.Commands;
using AbilityKit.Ability.Host.Extensions.Server.BattleHost;
using AbilityKit.Buffs;
using AbilityKit.Core.Mathematics;
using AbilityKit.Combat.Navigation;
using Xunit;

namespace GameBattle.Extraction.Tests;

public sealed class IntegrationContractTests
{
    [Theory]
    [InlineData(BuffStackingPolicy.None)]
    [InlineData(BuffStackingPolicy.IgnoreIfExists)]
    [InlineData((BuffStackingPolicy)99)]
    public void Ignored_policy_keeps_all_shared_state(BuffStackingPolicy policy)
    {
        var state = new BuffState { StackCount = 2, Remaining = 4, SourceId = 3, IntervalRemainingSeconds = 1 };
        var settings = new BuffStackingSettings(policy, BuffRefreshPolicy.ResetRemaining, 5, 500);
        var result = new BuffStackingPolicyApplier().ApplyToExisting(state, in settings, 9, 10);
        Assert.False(result.Applied);
        Assert.False(result.ShouldResetInterval);
        Assert.Equal(2, state.StackCount);
        Assert.Equal(4, state.Remaining);
        Assert.Equal(3, state.SourceId);
        Assert.Equal(1, state.IntervalRemainingSeconds);
    }

    [Theory]
    [InlineData(BuffRefreshPolicy.None, 4)]
    [InlineData(BuffRefreshPolicy.KeepRemaining, 4)]
    [InlineData(BuffRefreshPolicy.ResetRemaining, 10)]
    [InlineData(BuffRefreshPolicy.AddRemaining, 14)]
    public void Add_stack_uses_refresh_policy_and_caps_stack(BuffRefreshPolicy refresh, float expected)
    {
        var state = new BuffState { StackCount = 2, Remaining = 4 };
        var settings = new BuffStackingSettings(BuffStackingPolicy.AddStack, refresh, 2, 500);
        var result = new BuffStackingPolicyApplier().ApplyToExisting(state, in settings, 9, 10);
        Assert.Equal(BuffStackingApplyOutcome.StackAdded, result.Outcome);
        Assert.Equal(2, state.StackCount);
        Assert.Equal(expected, state.Remaining);
        Assert.Equal(9, state.SourceId);
    }

    [Fact]
    public void Replacement_resets_stack_source_duration_but_not_interval_until_requested()
    {
        var state = new BuffState { StackCount = 5, Remaining = 1, IntervalRemainingSeconds = 2 };
        var settings = new BuffStackingSettings(BuffStackingPolicy.Replace, BuffRefreshPolicy.KeepRemaining, 3, 500);
        var result = new BuffStackingPolicyApplier().ApplyToExisting(state, in settings, 7, 10);
        Assert.True(result.IsReplace);
        Assert.Equal(1, state.StackCount);
        Assert.Equal(10, state.Remaining);
        Assert.Equal(7, state.SourceId);
        Assert.Equal(2, state.IntervalRemainingSeconds);
        BuffStackingPolicyApplier.ResetInterval(state, in settings);
        Assert.Equal(0.5f, state.IntervalRemainingSeconds);
    }

    [Fact]
    public void Initialization_supports_caller_owned_state_and_unlimited_stacks()
    {
        var state = new BuffState { SourceContextId = 99 };
        var settings = new BuffStackingSettings(BuffStackingPolicy.AddStack, BuffRefreshPolicy.KeepRemaining, 0, 500);
        BuffStackingPolicyApplier.Initialize(state, 123, in settings, 0, 6);
        Assert.Equal(123, state.BuffId);
        Assert.Equal(0, state.SourceContextId);
        Assert.Equal(1, state.StackCount);
        Assert.Equal(6, state.Remaining);
        Assert.Equal(0.5f, state.IntervalRemainingSeconds);
        state.StackCount = int.MaxValue;
        new BuffStackingPolicyApplier().ApplyToExisting(state, in settings, 0, 6);
        Assert.Equal(int.MaxValue, state.StackCount);
    }

    [Fact]
    public void Refresh_does_not_add_stacks_and_null_state_is_ignored()
    {
        var settings = new BuffStackingSettings(BuffStackingPolicy.RefreshDuration, BuffRefreshPolicy.ResetRemaining, 3, 0);
        var policy = new BuffStackingPolicyApplier();
        Assert.False(policy.ApplyToExisting(null, in settings, 1, 2).Applied);
        var state = new BuffState { StackCount = 2 };
        Assert.Equal(BuffStackingApplyOutcome.DurationRefreshed, policy.ApplyToExisting(state, in settings, 1, 2).Outcome);
        Assert.Equal(2, state.StackCount);
        Assert.Equal(2, state.Remaining);
    }

    [Fact]
    public void Program_generated_commands_are_drained_before_simulation_and_future_frames_wait()
    {
        var buffer = new BattleInputBuffer<AttackCommand>();
        IBattleCommandSource<AttackCommand> source = new AutoChessCommandSource();
        source.CollectCommands(0, buffer);
        Assert.True(buffer.TrySubmit(1, new AttackCommand(3, 4)));
        var state = new BattleHostState();
        state.Initialize(1, "autochess", 30);
        var order = new List<string>();
        var driver = new BattleTickDriver<AttackCommand>(
            (frame, commands) => { order.Add("commands"); Assert.Single(commands); Assert.Equal(1, commands[0].ActorId); return commands.Count; },
            (frame, rate, delta) => { order.Add("tick"); return true; });
        var result = driver.Tick(state, buffer);
        Assert.Equal(new[] { "commands", "tick" }, order);
        Assert.True(result.WorldTicked);
        Assert.Equal(1, state.Frame);
        Assert.Single(buffer.Drain(1).Inputs);
    }

    [Fact]
    public void Command_sink_reports_invalid_frame_and_capacity_rejection()
    {
        IBattleCommandSink<int> sink = new BattleInputBuffer<int>(maxPendingFrames: 1);
        Assert.False(sink.TrySubmit(-1, 1));
        Assert.True(sink.TrySubmit(0, 1));
        Assert.True(sink.TrySubmit(0, 2));
        Assert.False(sink.TrySubmit(1, 3));
    }

    [Fact]
    public void Caller_can_supply_navigation_without_unity_or_default_grid()
    {
        INavigationWorld world = new ExternalNavigationWorld();
        using INavigationService service = new NavigationService(world);
        Assert.Same(world, service.World);
        var start = new Vec3(0, 0, 0);
        var target = new Vec3(2, 0, 3);
        Assert.Equal(PathStatus.Found, world.FindPath(in start, in target, 0.5f, out var path));
        Assert.Equal(2, path.Length);
        Assert.True(world.IsWalkable(in target, 0.5f));
        Assert.True(world.TryProjectToWalkable(in target, 0.5f, out _));
    }

    private readonly record struct AttackCommand(int ActorId, int TargetId);

    private sealed class AutoChessCommandSource : IBattleCommandSource<AttackCommand>
    {
        public void CollectCommands(int targetFrame, IBattleCommandSink<AttackCommand> sink)
        {
            if (!sink.TrySubmit(targetFrame, new AttackCommand(1, 2)))
                throw new InvalidOperationException("Generated command rejected.");
        }
    }

    private sealed class ExternalNavigationWorld : INavigationWorld
    {
        public PathStatus FindPath(in Vec3 start, in Vec3 target, float agentRadius, out NavigationPath path)
        {
            path = new NavigationPath(new[] { start, target }, PathStatus.Found);
            return path.Status;
        }

        public bool IsWalkable(in Vec3 position, float radius) => true;
        public bool TryProjectToWalkable(in Vec3 position, float radius, out Vec3 projected)
        {
            projected = position;
            return true;
        }
    }
}
