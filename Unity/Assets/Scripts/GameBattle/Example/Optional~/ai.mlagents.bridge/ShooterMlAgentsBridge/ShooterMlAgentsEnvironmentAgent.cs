using AbilityKit.AI.Abstractions;
using AbilityKit.AI.MLAgents.Bridge;
using AbilityKit.Demo.Shooter.AI;
using UnityEngine;

namespace AbilityKit.AI.MLAgents.Bridge.Samples;

public sealed class ShooterMlAgentsEnvironmentAgent : AbilityKitMlAgentsEnvironmentAgent
{
    [SerializeField]
    private int _controlledPlayerId = 1;

    [SerializeField]
    private int _maxObservedEnemies = 8;

    [SerializeField]
    private int _maxObservedProjectiles = 8;

    [SerializeField]
    private bool _enableEnemyWaves = true;

    [SerializeField]
    private int _maxEpisodeSteps = 1200;

    protected override IAiEnvironment CreateEnvironment()
    {
        return new ShooterAiTrainingEnvironment(new ShooterAiEnvironmentOptions(
            _controlledPlayerId,
            _maxObservedEnemies,
            _maxObservedProjectiles,
            enableEnemyWaves: _enableEnemyWaves));
    }

    protected override AiEpisodeOptions CreateEpisodeOptions()
    {
        var seed = unchecked(System.Environment.TickCount ^ CompletedEpisodes);
        return new AiEpisodeOptions(seed, _maxEpisodeSteps, Time.fixedDeltaTime > 0f ? Time.fixedDeltaTime : 1f / 30f);
    }
}
