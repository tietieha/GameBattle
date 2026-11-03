using Svelto.ECS;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public static class ShooterSveltoGroups
    {
        public static readonly ExclusiveGroup Players = new ExclusiveGroup("shooter.players");

        public static readonly ExclusiveGroup Projectiles = new ExclusiveGroup("shooter.projectiles");

        public static readonly ExclusiveGroup GameplayShooters = new ExclusiveGroup("shooter.gameplay.shooters");

        public static readonly ExclusiveGroup GameplayTargets = new ExclusiveGroup("shooter.gameplay.targets");

        public static readonly ExclusiveGroup GameplayProjectiles = new ExclusiveGroup("shooter.gameplay.projectiles");
    }
}
