#nullable enable

namespace AbilityKit.Demo.Shooter.Runtime
{
    public readonly struct ShooterSveltoGameplayScenarioResult
    {
        public ShooterSveltoGameplayScenarioResult(
            string scenarioId,
            int frames,
            int shooters,
            int targets,
            int projectilesSpawned,
            int projectilesExpired,
            int hits,
            int defeatedTargets,
            int activeProjectiles,
            int remainingTargetHp,
            int enemyHits,
            uint stateHash)
        {
            ScenarioId = scenarioId;
            Frames = frames;
            Shooters = shooters;
            Targets = targets;
            ProjectilesSpawned = projectilesSpawned;
            ProjectilesExpired = projectilesExpired;
            Hits = hits;
            DefeatedTargets = defeatedTargets;
            ActiveProjectiles = activeProjectiles;
            RemainingTargetHp = remainingTargetHp;
            EnemyHits = enemyHits;
            StateHash = stateHash;
        }

        public string ScenarioId { get; }

        public int Frames { get; }

        public int Shooters { get; }

        public int Targets { get; }

        public int ProjectilesSpawned { get; }

        public int ProjectilesExpired { get; }

        public int Hits { get; }

        public int DefeatedTargets { get; }

        public int ActiveProjectiles { get; }

        public int RemainingTargetHp { get; }

        public int EnemyHits { get; }

        public uint StateHash { get; }
    }
}
