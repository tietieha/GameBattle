using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Testing
{
    /// <summary>
    /// Platform-neutral battle input step kind used by MOBA view/runtime smoke scenarios.
    /// </summary>
    public enum BattleTestStepKind
    {
        Move,
        Skill,
        Wait,
        Idle
    }

/// <summary>
/// Immutable platform-neutral battle input step. Console, Unity EditMode and future headless view-runtime
/// harnesses should adapt from this model instead of maintaining separate scenario definitions.
/// </summary>
public sealed class BattleTestStep
{
    public BattleTestStep(BattleTestStepKind kind, int durationTicks, float dx = 0f, float dz = 0f, int slot = 0)
    {
        if (durationTicks <= 0) throw new ArgumentOutOfRangeException(nameof(durationTicks), durationTicks, "Duration ticks must be positive.");

        Kind = kind;
        DurationTicks = durationTicks;
        Dx = dx;
        Dz = dz;
        Slot = slot;
    }

    public BattleTestStepKind Kind { get; }
    public int DurationTicks { get; }
    public float Dx { get; }
    public float Dz { get; }
    public int Slot { get; }

    public static BattleTestStep Move(float dx, float dz, int durationTicks)
    {
        return new BattleTestStep(BattleTestStepKind.Move, durationTicks, dx, dz);
    }

    public static BattleTestStep Skill(int slot, int durationTicks = 1)
    {
        if (slot <= 0) throw new ArgumentOutOfRangeException(nameof(slot), slot, "Skill slot must be positive.");
        return new BattleTestStep(BattleTestStepKind.Skill, durationTicks, slot: slot);
    }

    public static BattleTestStep Wait(int durationTicks)
    {
        return new BattleTestStep(BattleTestStepKind.Wait, durationTicks);
    }

    public static BattleTestStep Idle(int durationTicks)
    {
        return new BattleTestStep(BattleTestStepKind.Idle, durationTicks);
    }
}

/// <summary>
/// Named platform-neutral battle test script.
/// </summary>
public sealed class BattleTestScript
{
    private readonly List<BattleTestStep> _steps;
    private readonly List<string> _riskTags;

    public BattleTestScript(string name, IEnumerable<BattleTestStep> steps)
        : this(name, steps, Array.Empty<string>())
    {
    }

    public BattleTestScript(string name, IEnumerable<BattleTestStep> steps, IEnumerable<string> riskTags)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Script name is required.", nameof(name));
        if (steps == null) throw new ArgumentNullException(nameof(steps));
        if (riskTags == null) throw new ArgumentNullException(nameof(riskTags));

        Name = name;
        _steps = new List<BattleTestStep>(steps);
        _riskTags = new List<string>(riskTags);
    }

    public string Name { get; }
    public IReadOnlyList<BattleTestStep> Steps => _steps;
    public IReadOnlyList<string> RiskTags => _riskTags;

    public int TotalDurationTicks
    {
        get
        {
            var total = 0;
            for (var i = 0; i < _steps.Count; i++) total += _steps[i].DurationTicks;
            return total;
        }
    }
}

/// <summary>
/// Mutable builder used by scenario definitions.
/// </summary>
public sealed class BattleTestScriptBuilder
{
    private readonly List<BattleTestStep> _steps = new();
    private readonly List<string> _riskTags = new();

    public IReadOnlyList<BattleTestStep> Steps => _steps;
    public IReadOnlyList<string> RiskTags => _riskTags;

    public BattleTestScriptBuilder Add(BattleTestStep step)
    {
        _steps.Add(step ?? throw new ArgumentNullException(nameof(step)));
        return this;
    }

    public BattleTestScriptBuilder AddRiskTag(string riskTag)
    {
        if (string.IsNullOrWhiteSpace(riskTag)) throw new ArgumentException("Risk tag is required.", nameof(riskTag));
        if (!_riskTags.Contains(riskTag)) _riskTags.Add(riskTag);
        return this;
    }

    public BattleTestScriptBuilder Move(float dx, float dz, int durationTicks)
    {
        return Add(BattleTestStep.Move(dx, dz, durationTicks));
    }

    public BattleTestScriptBuilder Skill(int slot, int durationTicks = 1)
    {
        return Add(BattleTestStep.Skill(slot, durationTicks));
    }

    public BattleTestScriptBuilder Wait(int durationTicks)
    {
        return Add(BattleTestStep.Wait(durationTicks));
    }

    public BattleTestScriptBuilder Idle(int durationTicks)
    {
        return Add(BattleTestStep.Idle(durationTicks));
    }

    public BattleTestScript Build(string name)
    {
        return new BattleTestScript(name, _steps, _riskTags);
    }
}

/// <summary>
/// Shared MOBA battle smoke scenarios. Keep scenario semantics here so Console, Unity and .NET tests
/// consume the same input assets.
/// </summary>
public static class BattleTestScenarioLibrary
{
    public const string SimpleMovementName = "SimpleMovement";
    public const string RandomMovementName = "RandomMovement";
    public const string SkillCastName = "SkillCast";
    public const string MoveAndCastName = "MoveAndCast";
    public const string FullBattleName = "FullBattle";
    public const string StressTestName = "StressTest";
    public const string ViewPresentationRiskName = "ViewPresentationRisk";

    public const string EntityRiskTag = "entity";
    public const string FloatingTextRiskTag = "floating-text";
    public const string ProjectileRiskTag = "projectile";
    public const string VfxRiskTag = "vfx";
    public const string SnapshotEventRiskTag = "snapshot-event";

    public static BattleTestScript CreateSimpleMovement(int cycles = 3)
    {
        var builder = new BattleTestScriptBuilder();
        AddSimpleMovement(builder, cycles);
        return builder.Build(SimpleMovementName);
    }

    public static BattleTestScript CreateRandomMovement(int steps = 20, int seed = 1337)
    {
        var builder = new BattleTestScriptBuilder();
        AddRandomMovement(builder, steps, seed);
        return builder.Build(RandomMovementName);
    }

    public static BattleTestScript CreateSkillCast(int skillSlot = 1, int repeats = 3)
    {
        var builder = new BattleTestScriptBuilder();
        AddSkillCast(builder, skillSlot, repeats);
        return builder.Build(SkillCastName);
    }

    public static BattleTestScript CreateMoveAndCast(int cycles = 5)
    {
        var builder = new BattleTestScriptBuilder();
        AddMoveAndCast(builder, cycles);
        return builder.Build(MoveAndCastName);
    }

    public static BattleTestScript CreateFullBattle(int randomSeed = 1337)
    {
        var builder = new BattleTestScriptBuilder();
        AddFullBattleTest(builder, randomSeed);
        return builder.Build(FullBattleName);
    }

    public static BattleTestScript CreateStressTest(int durationTicks = 300, int seed = 1337)
    {
        var builder = new BattleTestScriptBuilder();
        AddStressTest(builder, durationTicks, seed);
        return builder.Build(StressTestName);
    }

    public static BattleTestScript CreateViewPresentationRisk(int cycles = 2)
    {
        var builder = new BattleTestScriptBuilder();
        AddViewPresentationRisk(builder, cycles);
        return builder.Build(ViewPresentationRiskName);
    }

    public static void AddSimpleMovement(BattleTestScriptBuilder builder, int cycles = 3)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (cycles < 0) throw new ArgumentOutOfRangeException(nameof(cycles), cycles, "Cycles cannot be negative.");

        builder.AddRiskTag(EntityRiskTag);

        for (var i = 0; i < cycles; i++)
        {
            builder.Move(1f, 0f, 10);
            builder.Wait(5);
            builder.Move(0f, 1f, 10);
            builder.Wait(5);
            builder.Move(-1f, 0f, 10);
            builder.Wait(5);
            builder.Move(0f, -1f, 10);
            builder.Idle(10);
        }
    }

    public static void AddRandomMovement(BattleTestScriptBuilder builder, int steps = 20, int seed = 1337)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (steps < 0) throw new ArgumentOutOfRangeException(nameof(steps), steps, "Steps cannot be negative.");

        builder.AddRiskTag(EntityRiskTag);

        var random = new Random(seed);
        for (var i = 0; i < steps; i++)
        {
            var dx = (float)(random.NextDouble() * 2 - 1);
            var dz = (float)(random.NextDouble() * 2 - 1);
            var duration = random.Next(5, 15);
            builder.Move(dx, dz, duration);
        }

        builder.Idle(10);
    }

    public static void AddSkillCast(BattleTestScriptBuilder builder, int skillSlot = 1, int repeats = 3)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (repeats < 0) throw new ArgumentOutOfRangeException(nameof(repeats), repeats, "Repeats cannot be negative.");

        builder.AddRiskTag(FloatingTextRiskTag);
        builder.AddRiskTag(ProjectileRiskTag);
        builder.AddRiskTag(VfxRiskTag);
        builder.AddRiskTag(SnapshotEventRiskTag);

        for (var i = 0; i < repeats; i++)
        {
            builder.Skill(skillSlot, 1);
            builder.Wait(30);
        }
    }

    public static void AddMoveAndCast(BattleTestScriptBuilder builder, int cycles = 5)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (cycles < 0) throw new ArgumentOutOfRangeException(nameof(cycles), cycles, "Cycles cannot be negative.");

        builder.AddRiskTag(EntityRiskTag);
        builder.AddRiskTag(FloatingTextRiskTag);
        builder.AddRiskTag(ProjectileRiskTag);
        builder.AddRiskTag(VfxRiskTag);
        builder.AddRiskTag(SnapshotEventRiskTag);

        for (var i = 0; i < cycles; i++)
        {
            builder.Move(1f, 0f, 10);
            builder.Wait(5);
            builder.Skill(1, 1);
            builder.Wait(30);
            builder.Move(0f, 1f, 10);
            builder.Wait(5);
            builder.Skill(2, 1);
            builder.Wait(30);
        }

        builder.Idle(10);
    }

    public static void AddFullBattleTest(BattleTestScriptBuilder builder, int randomSeed = 1337)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        AddMoveAndCast(builder, 3);
        AddRandomMovement(builder, 10, randomSeed);

        for (var slot = 1; slot <= 3; slot++)
        {
            AddSkillCast(builder, slot, 2);
        }
    }

    public static void AddStressTest(BattleTestScriptBuilder builder, int durationTicks = 300, int seed = 1337)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (durationTicks < 0) throw new ArgumentOutOfRangeException(nameof(durationTicks), durationTicks, "Duration ticks cannot be negative.");

        builder.AddRiskTag(EntityRiskTag);
        builder.AddRiskTag(FloatingTextRiskTag);
        builder.AddRiskTag(ProjectileRiskTag);
        builder.AddRiskTag(VfxRiskTag);
        builder.AddRiskTag(SnapshotEventRiskTag);

        var random = new Random(seed);
        var tick = 0;
        while (tick < durationTicks)
        {
            var action = random.Next(4);
            switch (action)
            {
                case 0:
                    builder.Move(
                        (float)(random.NextDouble() * 2 - 1),
                        (float)(random.NextDouble() * 2 - 1),
                        3);
                    break;
                case 1:
                    builder.Skill(random.Next(1, 4), 1);
                    break;
                default:
                    builder.Idle(1);
                    break;
            }

            tick += 3;
        }

        builder.Idle(10);
    }

    public static void AddViewPresentationRisk(BattleTestScriptBuilder builder, int cycles = 2)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (cycles < 0) throw new ArgumentOutOfRangeException(nameof(cycles), cycles, "Cycles cannot be negative.");

        builder.AddRiskTag(EntityRiskTag);
        builder.AddRiskTag(FloatingTextRiskTag);
        builder.AddRiskTag(ProjectileRiskTag);
        builder.AddRiskTag(VfxRiskTag);
        builder.AddRiskTag(SnapshotEventRiskTag);

        for (var i = 0; i < cycles; i++)
        {
            builder.Move(1f, 0f, 4);
            builder.Skill(1, 1);
            builder.Wait(2);
            builder.Move(0f, 1f, 4);
            builder.Skill(2, 1);
            builder.Wait(2);
            builder.Move(-1f, 0f, 4);
            builder.Skill(3, 1);
            builder.Idle(3);
        }
    }
}
}
