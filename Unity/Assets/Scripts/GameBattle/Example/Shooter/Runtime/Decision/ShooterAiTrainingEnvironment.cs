using System;
using AbilityKit.AI.Abstractions;
using AbilityKit.Demo.Shooter.Runtime;
using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.AI
{
    public readonly struct ShooterAiEnvironmentOptions
    {
        public ShooterAiEnvironmentOptions(
            int controlledPlayerId,
            int maxObservedEnemies = 8,
            int maxObservedProjectiles = 8,
            int tickRate = 30,
            float arenaExtent = 24f,
            bool enableEnemyWaves = true)
        {
            ControlledPlayerId = controlledPlayerId <= 0 ? 1 : controlledPlayerId;
            MaxObservedEnemies = maxObservedEnemies < 0 ? 0 : maxObservedEnemies;
            MaxObservedProjectiles = maxObservedProjectiles < 0 ? 0 : maxObservedProjectiles;
            TickRate = tickRate < 1 ? 30 : tickRate;
            ArenaExtent = arenaExtent > 0f ? arenaExtent : 24f;
            EnableEnemyWaves = enableEnemyWaves;
        }

        public int ControlledPlayerId { get; }

        public int MaxObservedEnemies { get; }

        public int MaxObservedProjectiles { get; }

        public int TickRate { get; }

        public float ArenaExtent { get; }

        public bool EnableEnemyWaves { get; }

        public static ShooterAiEnvironmentOptions Default => new ShooterAiEnvironmentOptions(1);
    }

    public sealed class ShooterAiTrainingEnvironment : IAiEnvironment
    {
        private readonly ShooterAiEnvironmentOptions _environmentOptions;
        private readonly AiObservationBuffer _observation;
        private readonly ShooterAiObservationBuilder _observationBuilder;
        private readonly ShooterAiActionMapper _actionMapper;
        private readonly ShooterAiRewardEvaluator _rewardEvaluator;
        private ShooterBattleRuntimePort _runtime;
        private AiEpisodeOptions _episodeOptions;
        private int _stepIndex;

        public ShooterAiTrainingEnvironment()
            : this(ShooterAiEnvironmentOptions.Default)
        {
        }

        public ShooterAiTrainingEnvironment(ShooterAiEnvironmentOptions environmentOptions)
        {
            _environmentOptions = environmentOptions;
            ObservationSpec = ShooterAiObservationBuilder.CreateSpec(environmentOptions);
            ActionSpec = ShooterAiActionMapper.ActionSpec;
            _observation = new AiObservationBuffer(ObservationSpec);
            _observationBuilder = new ShooterAiObservationBuilder(environmentOptions);
            _actionMapper = new ShooterAiActionMapper(environmentOptions.ControlledPlayerId);
            _rewardEvaluator = new ShooterAiRewardEvaluator(environmentOptions.ControlledPlayerId);
            _runtime = CreateRuntime(environmentOptions);
            _episodeOptions = new AiEpisodeOptions(1, 1, 1f / environmentOptions.TickRate);
        }

        public AiObservationSpec ObservationSpec { get; }

        public AiActionSpec ActionSpec { get; }

        public ShooterBattleRuntimePort Runtime => _runtime;

        public AiStepResult Reset(in AiEpisodeOptions options)
        {
            _runtime = CreateRuntime(_environmentOptions);
            _episodeOptions = options;
            _stepIndex = 0;

            var start = CreateStartPayload(options.Seed);
            if (!_runtime.StartGame(in start))
            {
                throw new InvalidOperationException("Shooter AI environment failed to start runtime episode.");
            }

            var snapshot = _runtime.GetSnapshot();
            _rewardEvaluator.Reset(in snapshot);
            _observationBuilder.Write(in snapshot, _observation);
            return new AiStepResult(_observation, 0f, done: false, truncated: false, _stepIndex, _runtime.ComputeStateHash());
        }

        public AiStepResult Step(in AiActionBuffer action)
        {
            if (!_runtime.IsStarted)
            {
                throw new InvalidOperationException("Shooter AI environment must be reset before stepping.");
            }

            var command = _actionMapper.ToCommand(action);
            _runtime.SubmitInput(_runtime.CurrentFrame, new[] { command });
            _runtime.Tick(_episodeOptions.FixedDeltaSeconds);
            _stepIndex++;

            var snapshot = _runtime.GetSnapshot();
            var reward = _rewardEvaluator.Evaluate(in snapshot, _runtime.MatchState);
            _observationBuilder.Write(in snapshot, _observation);

            var done = _runtime.MatchState == ShooterBattleMatchState.Victory ||
                       _runtime.MatchState == ShooterBattleMatchState.Defeat ||
                       _runtime.MatchState == ShooterBattleMatchState.Ended;
            var truncated = !done && _stepIndex >= _episodeOptions.MaxSteps;
            return new AiStepResult(_observation, reward, done, truncated, _stepIndex, _runtime.ComputeStateHash());
        }

        private ShooterStartGamePayload CreateStartPayload(int seed)
        {
            return new ShooterStartGamePayload(
                $"shooter-ai-training-{seed}",
                _environmentOptions.TickRate,
                seed,
                new[] { new ShooterStartPlayer(_environmentOptions.ControlledPlayerId, "Learner", 0f, 0f) });
        }

        private static ShooterBattleRuntimePort CreateRuntime(in ShooterAiEnvironmentOptions options)
        {
            var waveOptions = options.EnableEnemyWaves
                ? ShooterEnemyWaveOptions.DefaultEnabled
                : ShooterEnemyWaveOptions.Disabled;
            return new ShooterBattleRuntimePort(ShooterEntityLimitOptions.Default, waveOptions);
        }
    }

    public sealed class ShooterAiObservationBuilder
    {
        private const int PlayerValueCount = 8;
        private const int EnemyValueCount = 5;
        private const int ProjectileValueCount = 5;
        private readonly ShooterAiEnvironmentOptions _options;

        public ShooterAiObservationBuilder(ShooterAiEnvironmentOptions options)
        {
            _options = options;
        }

        public static AiObservationSpec CreateSpec(ShooterAiEnvironmentOptions options)
        {
            var length = PlayerValueCount +
                         options.MaxObservedEnemies * EnemyValueCount +
                         options.MaxObservedProjectiles * ProjectileValueCount +
                         4;
            return new AiObservationSpec("shooter.snapshot.v1", length);
        }

        public void Write(in ShooterStateSnapshotPayload snapshot, AiObservationBuffer buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            buffer.Clear();

            var index = 0;
            var player = FindPlayer(snapshot.Players, _options.ControlledPlayerId);
            var extent = _options.ArenaExtent;
            if (player.PlayerId != 0)
            {
                buffer[index++] = NormalizePosition(player.X, extent);
                buffer[index++] = NormalizePosition(player.Y, extent);
                buffer[index++] = ClampUnit(player.AimX);
                buffer[index++] = ClampUnit(player.AimY);
                buffer[index++] = NormalizePositive(player.Hp, 100f);
                buffer[index++] = NormalizePositive(player.Score, 32f);
                buffer[index++] = player.Alive ? 1f : 0f;
                buffer[index++] = 1f;
            }
            else
            {
                index += PlayerValueCount;
            }

            WriteEnemies(snapshot.Enemies, buffer, ref index, player, extent);
            WriteProjectiles(snapshot.Bullets, buffer, ref index, player, extent);

            buffer[index++] = NormalizePositive(snapshot.Frame, 3600f);
            buffer[index++] = NormalizePositive(snapshot.RemainingTimeFrames, 3600f);
            buffer[index++] = snapshot.MatchState == (int)ShooterBattleMatchState.Running ? 1f : 0f;
            buffer[index] = NormalizePositive(snapshot.Enemies?.Length ?? 0, 32f);
        }

        private void WriteEnemies(ShooterEnemySnapshot[]? enemies, AiObservationBuffer buffer, ref int index, ShooterPlayerSnapshot player, float extent)
        {
            var written = 0;
            enemies ??= Array.Empty<ShooterEnemySnapshot>();
            for (int i = 0; i < enemies.Length && written < _options.MaxObservedEnemies; i++)
            {
                var enemy = enemies[i];
                if (!enemy.Alive) continue;

                buffer[index++] = NormalizeDelta(enemy.X - player.X, extent);
                buffer[index++] = NormalizeDelta(enemy.Y - player.Y, extent);
                buffer[index++] = NormalizePositive(enemy.Hp, Math.Max(enemy.MaxHp, 1));
                buffer[index++] = NormalizePositive(Distance(enemy.X - player.X, enemy.Y - player.Y), extent);
                buffer[index++] = 1f;
                written++;
            }

            index += (_options.MaxObservedEnemies - written) * EnemyValueCount;
        }

        private void WriteProjectiles(ShooterBulletSnapshot[]? bullets, AiObservationBuffer buffer, ref int index, ShooterPlayerSnapshot player, float extent)
        {
            var written = 0;
            bullets ??= Array.Empty<ShooterBulletSnapshot>();
            for (int i = 0; i < bullets.Length && written < _options.MaxObservedProjectiles; i++)
            {
                var bullet = bullets[i];
                buffer[index++] = NormalizeDelta(bullet.X - player.X, extent);
                buffer[index++] = NormalizeDelta(bullet.Y - player.Y, extent);
                buffer[index++] = ClampUnit(bullet.VelocityX);
                buffer[index++] = ClampUnit(bullet.VelocityY);
                buffer[index++] = bullet.OwnerPlayerId == _options.ControlledPlayerId ? 1f : -1f;
                written++;
            }

            index += (_options.MaxObservedProjectiles - written) * ProjectileValueCount;
        }

        private static ShooterPlayerSnapshot FindPlayer(ShooterPlayerSnapshot[]? players, int playerId)
        {
            players ??= Array.Empty<ShooterPlayerSnapshot>();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].PlayerId == playerId)
                {
                    return players[i];
                }
            }

            return default;
        }

        private static float NormalizePosition(float value, float extent) => ClampUnit(value / extent);

        private static float NormalizeDelta(float value, float extent) => ClampUnit(value / extent);

        private static float NormalizePositive(float value, float max) => max <= 0f ? 0f : Clamp01(value / max);

        private static float ClampUnit(float value) => value < -1f ? -1f : value > 1f ? 1f : value;

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;

        private static float Distance(float x, float y) => MathF.Sqrt(x * x + y * y);
    }

    public sealed class ShooterAiActionMapper
    {
        private readonly int _playerId;

        public ShooterAiActionMapper(int playerId)
        {
            _playerId = playerId <= 0 ? 1 : playerId;
        }

        public static AiActionSpec ActionSpec { get; } = new AiActionSpec("shooter.command.v1", continuousLength: 4, discreteLength: 2);

        public ShooterPlayerCommand ToCommand(in AiActionBuffer action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var continuous = action.Continuous;
            var discrete = action.Discrete;
            var moveX = continuous.Length > 0 ? ClampUnit(continuous[0]) : 0f;
            var moveY = continuous.Length > 1 ? ClampUnit(continuous[1]) : 0f;
            var aimX = continuous.Length > 2 ? ClampUnit(continuous[2]) : moveX;
            var aimY = continuous.Length > 3 ? ClampUnit(continuous[3]) : moveY;
            if (aimX * aimX + aimY * aimY <= 0.000001f)
            {
                aimX = 1f;
                aimY = 0f;
            }

            var fire = discrete.Length > 0 && discrete[0] != 0;
            var attackSlot = discrete.Length > 1 ? ShooterPlayerAttackSlots.Normalize(discrete[1]) : ShooterPlayerAttackSlots.Primary;
            return new ShooterPlayerCommand(_playerId, moveX, moveY, aimX, aimY, fire, attackSlot);
        }

        private static float ClampUnit(float value) => value < -1f ? -1f : value > 1f ? 1f : value;
    }

    public sealed class ShooterAiRewardEvaluator
    {
        private readonly int _playerId;
        private int _lastScore;
        private int _lastHp;
        private bool _hasBaseline;

        public ShooterAiRewardEvaluator(int playerId)
        {
            _playerId = playerId <= 0 ? 1 : playerId;
        }

        public void Reset(in ShooterStateSnapshotPayload snapshot)
        {
            var player = FindPlayer(snapshot.Players, _playerId);
            _lastScore = player.Score;
            _lastHp = player.Hp;
            _hasBaseline = true;
        }

        public float Evaluate(in ShooterStateSnapshotPayload snapshot, ShooterBattleMatchState matchState)
        {
            var player = FindPlayer(snapshot.Players, _playerId);
            if (!_hasBaseline)
            {
                Reset(in snapshot);
                return 0f;
            }

            var reward = -0.001f;
            reward += (player.Score - _lastScore) * 1.0f;
            reward += (player.Hp - _lastHp) * 0.05f;
            if (!player.Alive)
            {
                reward -= 2f;
            }

            if (matchState == ShooterBattleMatchState.Victory)
            {
                reward += 5f;
            }
            else if (matchState == ShooterBattleMatchState.Defeat)
            {
                reward -= 5f;
            }

            _lastScore = player.Score;
            _lastHp = player.Hp;
            return reward;
        }

        private static ShooterPlayerSnapshot FindPlayer(ShooterPlayerSnapshot[]? players, int playerId)
        {
            players ??= Array.Empty<ShooterPlayerSnapshot>();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].PlayerId == playerId)
                {
                    return players[i];
                }
            }

            return default;
        }
    }

    public sealed class ShooterAiForwardFirePolicy : IAiPolicy
    {
        public AiActionSpec ActionSpec => ShooterAiActionMapper.ActionSpec;

        public void Decide(in AiObservationBuffer observation, AiActionBuffer action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action.Clear();
            if (action.Continuous.Length >= 4)
            {
                action.Continuous[2] = 1f;
                action.Continuous[3] = 0f;
            }

            if (action.Discrete.Length >= 2)
            {
                action.Discrete[0] = 1;
                action.Discrete[1] = ShooterPlayerAttackSlots.Primary;
            }
        }
    }
}
