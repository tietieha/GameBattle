#nullable enable

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal sealed class ShooterEnemyWaveProgress
    {
        private readonly int[] _waveSpawned;

        public ShooterEnemyWaveProgress(ShooterSveltoGameplayWaveConfig[] waves)
        {
            _waveSpawned = waves is { Length: > 0 } ? new int[waves.Length] : new int[0];
        }

        public int[] WaveSpawned => _waveSpawned;

        public void Reset()
        {
            for (var i = 0; i < _waveSpawned.Length; i++)
            {
                _waveSpawned[i] = 0;
            }
        }

        public void RestoreFromSpawnCount(int importedSpawnCount, ShooterSveltoGameplayWaveConfig[] waves)
        {
            Reset();
            var remaining = importedSpawnCount;
            for (var i = 0; i < _waveSpawned.Length && i < waves.Length; i++)
            {
                var spawnedInWave = remaining > 0 ? System.Math.Min(remaining, waves[i].EnemyCount) : 0;
                _waveSpawned[i] = spawnedInWave;
                remaining -= spawnedInWave;
                if (remaining <= 0)
                {
                    break;
                }
            }
        }

        public int GetSpawned(int index)
        {
            return index < 0 || index >= _waveSpawned.Length ? 0 : _waveSpawned[index];
        }

        public void Increment(int index)
        {
            if (index < 0 || index >= _waveSpawned.Length)
            {
                return;
            }

            _waveSpawned[index]++;
        }
    }
}
