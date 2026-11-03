using AbilityKit.Combat.Projectile;

namespace AbilityKit.Demo.Moba.Services.Projectile.Launch
{
    public readonly struct MobaProjectileLaunchResult
    {
        public MobaProjectileLaunchResult(
            bool success,
            int launcherActorId,
            ProjectileScheduleId scheduleId,
            int intervalFrames,
            int totalCount,
            int startFrame,
            long endTimeMs,
            global::ActorEntity launcherEntity,
            IMobaProjectileLaunchSequence sequence,
            IProjectileService projectileService,
            IMobaProjectileLaunchRuntime sequenceRuntime,
            string error)
        {
            Success = success;
            LauncherActorId = launcherActorId;
            ScheduleId = scheduleId;
            IntervalFrames = intervalFrames;
            TotalCount = totalCount;
            StartFrame = startFrame;
            EndTimeMs = endTimeMs;
            LauncherEntity = launcherEntity;
            Sequence = sequence;
            ProjectileService = projectileService;
            SequenceRuntime = sequenceRuntime;
            Error = error;
        }

        public bool Success { get; }
        public int LauncherActorId { get; }
        public ProjectileScheduleId ScheduleId { get; }
        public int IntervalFrames { get; }
        public int TotalCount { get; }
        public int StartFrame { get; }
        public long EndTimeMs { get; }
        public global::ActorEntity LauncherEntity { get; }
        public IMobaProjectileLaunchSequence Sequence { get; }
        public IProjectileService ProjectileService { get; }
        public IMobaProjectileLaunchRuntime SequenceRuntime { get; }
        public string Error { get; }

        public static MobaProjectileLaunchResult Failed(string error)
        {
            return new MobaProjectileLaunchResult(false, 0, default, 0, 0, 0, 0L, null, null, null, null, error);
        }
    }
}
