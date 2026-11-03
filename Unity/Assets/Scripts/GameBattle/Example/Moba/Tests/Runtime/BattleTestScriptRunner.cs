using System;

namespace AbilityKit.Demo.Moba.Testing
{
/// <summary>
/// Platform-neutral driver that adapts a battle test script tick to Console, Unity or a headless harness.
/// </summary>
public interface IBattleTestScriptDriver
{
    /// <summary>
    /// Applies the input represented by the current step for the current tick.
    /// </summary>
    void Apply(BattleTestStep step);

    /// <summary>
    /// Advances the adapted runtime by one logical test tick.
    /// </summary>
    void Tick();
}

/// <summary>
/// Optional lifecycle callbacks for drivers that need setup/teardown around a script run.
/// </summary>
public interface IBattleTestScriptDriverLifecycle
{
    void BeginScript(BattleTestScript script);
    void EndScript(BattleTestScript script, BattleTestScriptRunResult result);
}

/// <summary>
/// Immutable result returned by <see cref="BattleTestScriptRunner"/>.
/// </summary>
public sealed class BattleTestScriptRunResult
{
    public BattleTestScriptRunResult(string scriptName, int stepCount, int tickCount, bool completed, string errorMessage = null)
    {
        ScriptName = scriptName ?? throw new ArgumentNullException(nameof(scriptName));
        StepCount = stepCount;
        TickCount = tickCount;
        Completed = completed;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public string ScriptName { get; }
    public int StepCount { get; }
    public int TickCount { get; }
    public bool Completed { get; }
    public string ErrorMessage { get; }
}

/// <summary>
/// Shared deterministic runner for platform-neutral MOBA battle test scripts.
/// Console, Unity EditMode and pure headless harnesses should share this tick semantics.
/// </summary>
public sealed class BattleTestScriptRunner
{
    public BattleTestScriptRunResult Run(BattleTestScript script, IBattleTestScriptDriver driver)
    {
        if (script == null) throw new ArgumentNullException(nameof(script));
        if (driver == null) throw new ArgumentNullException(nameof(driver));

        var tickCount = 0;
        var lifecycle = driver as IBattleTestScriptDriverLifecycle;

        try
        {
            lifecycle?.BeginScript(script);

            for (var stepIndex = 0; stepIndex < script.Steps.Count; stepIndex++)
            {
                var step = script.Steps[stepIndex];
                for (var tick = 0; tick < step.DurationTicks; tick++)
                {
                    driver.Apply(step);
                    driver.Tick();
                    tickCount++;
                }
            }

            var result = new BattleTestScriptRunResult(script.Name, script.Steps.Count, tickCount, completed: true);
            lifecycle?.EndScript(script, result);
            return result;
        }
        catch (Exception ex)
        {
            var result = new BattleTestScriptRunResult(script.Name, script.Steps.Count, tickCount, completed: false, ex.Message);
            lifecycle?.EndScript(script, result);
            return result;
        }
    }
}

/// <summary>
/// Shared scenario contract that produces a reusable platform-neutral script.
/// </summary>
public interface IBattleTestScenario
{
    string Name { get; }
    BattleTestScript CreateScript();
}

public sealed class SimpleMovementScenario : IBattleTestScenario
{
    public string Name => BattleTestScenarioLibrary.SimpleMovementName;
    public int Cycles { get; set; } = 3;

    public BattleTestScript CreateScript()
    {
        return BattleTestScenarioLibrary.CreateSimpleMovement(Cycles);
    }
}

public sealed class SkillCastScenario : IBattleTestScenario
{
    public string Name => BattleTestScenarioLibrary.SkillCastName;
    public int SkillSlot { get; set; } = 1;
    public int Repeats { get; set; } = 3;

    public BattleTestScript CreateScript()
    {
        return BattleTestScenarioLibrary.CreateSkillCast(SkillSlot, Repeats);
    }
}

public sealed class MoveAndCastScenario : IBattleTestScenario
{
    public string Name => BattleTestScenarioLibrary.MoveAndCastName;
    public int Cycles { get; set; } = 5;

    public BattleTestScript CreateScript()
    {
        return BattleTestScenarioLibrary.CreateMoveAndCast(Cycles);
    }
}

public sealed class FullBattleScenario : IBattleTestScenario
{
    public string Name => BattleTestScenarioLibrary.FullBattleName;
    public int RandomSeed { get; set; } = 1337;

    public BattleTestScript CreateScript()
    {
        return BattleTestScenarioLibrary.CreateFullBattle(RandomSeed);
    }
}

public sealed class StressTestScenario : IBattleTestScenario
{
    public string Name => BattleTestScenarioLibrary.StressTestName;
    public int DurationTicks { get; set; } = 300;
    public int Seed { get; set; } = 1337;

    public BattleTestScript CreateScript()
    {
        return BattleTestScenarioLibrary.CreateStressTest(DurationTicks, Seed);
    }
}

public sealed class ViewPresentationRiskScenario : IBattleTestScenario
{
    public string Name => BattleTestScenarioLibrary.ViewPresentationRiskName;
    public int Cycles { get; set; } = 2;

    public BattleTestScript CreateScript()
    {
        return BattleTestScenarioLibrary.CreateViewPresentationRisk(Cycles);
    }
}
}
