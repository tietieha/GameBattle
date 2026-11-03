using AbilityKit.AI.Abstractions;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace AbilityKit.AI.MLAgents.Bridge;

public abstract class AbilityKitMlAgentsEnvironmentAgent : Agent
{
    private IAiEnvironment? _environment;
    private AiActionBuffer? _actionBuffer;
    private AiStepResult _lastStep;
    private bool _hasObservation;

    public IAiEnvironment? Environment => _environment;

    public override void Initialize()
    {
        _environment = CreateEnvironment();
        _actionBuffer = new AiActionBuffer(_environment.ActionSpec);
    }

    public override void OnEpisodeBegin()
    {
        EnsureInitialized();
        _lastStep = _environment!.Reset(CreateEpisodeOptions());
        _hasObservation = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        EnsureInitialized();
        if (!_hasObservation)
        {
            _lastStep = _environment!.Reset(CreateEpisodeOptions());
            _hasObservation = true;
        }

        var values = _lastStep.Observation.Values;
        for (int i = 0; i < values.Length; i++)
        {
            sensor.AddObservation(values[i]);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        EnsureInitialized();
        _actionBuffer!.Clear();
        CopyContinuousActions(actions.ContinuousActions, _actionBuffer.Continuous);
        CopyDiscreteActions(actions.DiscreteActions, _actionBuffer.Discrete);

        _lastStep = _environment!.Step(_actionBuffer);
        _hasObservation = true;
        AddReward(_lastStep.Reward);

        if (_lastStep.Done || _lastStep.Truncated)
        {
            EndEpisode();
        }
    }

    protected abstract IAiEnvironment CreateEnvironment();

    protected virtual AiEpisodeOptions CreateEpisodeOptions()
    {
        var seed = unchecked(System.Environment.TickCount ^ CompletedEpisodes);
        return new AiEpisodeOptions(seed, MaxStep <= 0 ? 1024 : MaxStep, Time.fixedDeltaTime > 0f ? Time.fixedDeltaTime : 1f / 30f);
    }

    private void EnsureInitialized()
    {
        if (_environment != null && _actionBuffer != null) return;

        _environment = CreateEnvironment();
        _actionBuffer = new AiActionBuffer(_environment.ActionSpec);
    }

    private static void CopyContinuousActions(ActionSegment<float> source, float[] target)
    {
        var count = source.Length < target.Length ? source.Length : target.Length;
        for (int i = 0; i < count; i++)
        {
            target[i] = source[i];
        }
    }

    private static void CopyDiscreteActions(ActionSegment<int> source, int[] target)
    {
        var count = source.Length < target.Length ? source.Length : target.Length;
        for (int i = 0; i < count; i++)
        {
            target[i] = source[i];
        }
    }
}
