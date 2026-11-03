using AbilityKit.Demo.Moba.Components;
using AbilityKit.Demo.Moba.Config.BattleDemo.MO;
using AbilityKit.Demo.Moba.Services.Buffs.Core;
using AbilityKit.Demo.Moba.Share.Config;
using Xunit;

namespace AbilityKit.Demo.Moba.Tests.Buff;

public sealed class BuffStackingPolicyApplierTests
{
    [Fact]
    public void Replace_resets_stack_source_and_remaining()
    {
        var applier = new BuffStackingPolicyApplier();
        var runtime = new BuffRuntime
        {
            BuffId = 1001,
            SourceId = 10,
            StackCount = 3,
            Remaining = 1.5f,
            IntervalRemainingSeconds = 0.25f,
        };
        var buff = CreateBuff(BuffStackingPolicy.Replace, BuffRefreshPolicy.ResetRemaining, maxStacks: 2, intervalMs: 500);

        var result = applier.ApplyToExisting(runtime, buff, sourceActorId: 20, durationSeconds: 8f);

        Assert.True(result.Applied);
        Assert.Equal(BuffStackingApplyOutcome.Replaced, result.Outcome);
        Assert.True(result.IsReplace);
        Assert.Equal(20, runtime.SourceId);
        Assert.Equal(1, runtime.StackCount);
        Assert.Equal(8f, runtime.Remaining);
    }

    [Fact]
    public void AddStack_caps_at_max_and_adds_remaining_when_configured()
    {
        var applier = new BuffStackingPolicyApplier();
        var runtime = new BuffRuntime
        {
            BuffId = 1001,
            SourceId = 10,
            StackCount = 1,
            Remaining = 2f,
        };
        var buff = CreateBuff(BuffStackingPolicy.AddStack, BuffRefreshPolicy.AddRemaining, maxStacks: 2);

        var first = applier.ApplyToExisting(runtime, buff, sourceActorId: 20, durationSeconds: 3f);
        var second = applier.ApplyToExisting(runtime, buff, sourceActorId: 30, durationSeconds: 4f);

        Assert.True(first.Applied);
        Assert.Equal(BuffStackingApplyOutcome.StackAdded, first.Outcome);
        Assert.True(second.Applied);
        Assert.Equal(2, runtime.StackCount);
        Assert.Equal(30, runtime.SourceId);
        Assert.Equal(9f, runtime.Remaining);
    }

    [Fact]
    public void RefreshDuration_refreshes_remaining_without_changing_stack_count()
    {
        var applier = new BuffStackingPolicyApplier();
        var runtime = new BuffRuntime
        {
            BuffId = 1001,
            SourceId = 10,
            StackCount = 2,
            Remaining = 2f,
        };
        var buff = CreateBuff(BuffStackingPolicy.RefreshDuration, BuffRefreshPolicy.ResetRemaining, maxStacks: 5);

        var result = applier.ApplyToExisting(runtime, buff, sourceActorId: 20, durationSeconds: 6f);

        Assert.True(result.Applied);
        Assert.Equal(BuffStackingApplyOutcome.DurationRefreshed, result.Outcome);
        Assert.Equal(2, runtime.StackCount);
        Assert.Equal(20, runtime.SourceId);
        Assert.Equal(6f, runtime.Remaining);
    }

    [Fact]
    public void IgnoreIfExists_returns_ignored_without_mutating_runtime()
    {
        var applier = new BuffStackingPolicyApplier();
        var runtime = new BuffRuntime
        {
            BuffId = 1001,
            SourceId = 10,
            StackCount = 2,
            Remaining = 2f,
        };
        var buff = CreateBuff(BuffStackingPolicy.IgnoreIfExists, BuffRefreshPolicy.ResetRemaining, maxStacks: 5);

        var result = applier.ApplyToExisting(runtime, buff, sourceActorId: 20, durationSeconds: 6f);

        Assert.False(result.Applied);
        Assert.Equal(BuffStackingApplyOutcome.Ignored, result.Outcome);
        Assert.Equal(2, runtime.StackCount);
        Assert.Equal(10, runtime.SourceId);
        Assert.Equal(2f, runtime.Remaining);
    }

    [Fact]
    public void CreateNewRuntime_initializes_stack_duration_and_interval()
    {
        var applier = new BuffStackingPolicyApplier();
        var buff = CreateBuff(BuffStackingPolicy.AddStack, BuffRefreshPolicy.ResetRemaining, maxStacks: 3, intervalMs: 750);

        var runtime = applier.CreateNewRuntime(buff, sourceActorId: 12, durationSeconds: 4f);

        try
        {
            Assert.Equal(1001, runtime.BuffId);
            Assert.Equal(12, runtime.SourceId);
            Assert.Equal(1, runtime.StackCount);
            Assert.Equal(4f, runtime.Remaining);
            Assert.Equal(0.75f, runtime.IntervalRemainingSeconds);
        }
        finally
        {
            BuffRepository.ReleaseRuntime(runtime);
        }
    }

    private static BuffMO CreateBuff(BuffStackingPolicy stackingPolicy, BuffRefreshPolicy refreshPolicy, int maxStacks, int intervalMs = 0)
    {
        return new BuffMO(new BuffDTO
        {
            Id = 1001,
            Name = "test_buff",
            DurationMs = 4000,
            StackingPolicy = (int)stackingPolicy,
            RefreshPolicy = (int)refreshPolicy,
            MaxStacks = maxStacks,
            IntervalMs = intervalMs,
        });
    }
}
