namespace AbilityKit.Demo.Shooter.Runtime
{
    public readonly struct ShooterRuleSet
    {
        public const int DefaultRuleSetId = 1;
        public const int DefaultConfigVersion = 1;

        public static ShooterRuleSet Default { get; } = new ShooterRuleSet(
            DefaultRuleSetId,
            DefaultConfigVersion,
            ShooterBattleTuning.PlayerSpeed,
            ShooterBattleTuning.BulletSpeed,
            ShooterBattleTuning.BulletLifeFrames,
            ShooterBattleTuning.HitRadius,
            ShooterBattleTuning.HitDamage);

        public ShooterRuleSet(
            int ruleSetId,
            int configVersion,
            float playerSpeed,
            float bulletSpeed,
            int bulletLifeFrames,
            float hitRadius,
            int hitDamage)
        {
            RuleSetId = ruleSetId;
            ConfigVersion = configVersion;
            PlayerSpeed = playerSpeed;
            BulletSpeed = bulletSpeed;
            BulletLifeFrames = bulletLifeFrames;
            HitRadius = hitRadius;
            HitDamage = hitDamage;
        }

        public int RuleSetId { get; }

        public int ConfigVersion { get; }

        public float PlayerSpeed { get; }

        public float BulletSpeed { get; }

        public int BulletLifeFrames { get; }

        public float HitRadius { get; }

        public int HitDamage { get; }
    }
}
