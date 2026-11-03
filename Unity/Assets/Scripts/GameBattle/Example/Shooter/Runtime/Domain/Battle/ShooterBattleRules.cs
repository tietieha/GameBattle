using AbilityKit.Ability.World.DI;
using AbilityKit.Ability.World.Services.Attributes;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public interface IShooterBattleRules
    {
        ShooterRuleSet RuleSet { get; }

        int RuleSetId { get; }

        int ConfigVersion { get; }

        float PlayerSpeed { get; }

        float BulletSpeed { get; }

        int BulletLifeFrames { get; }

        float HitRadius { get; }

        int HitDamage { get; }
    }

    [WorldService(typeof(ShooterBattleRules), WorldLifetime.Singleton)]
    [WorldService(typeof(IShooterBattleRules), WorldLifetime.Singleton)]
    public sealed class ShooterBattleRules : IShooterBattleRules
    {
        public static ShooterBattleRules Default { get; } = new ShooterBattleRules();

        public ShooterBattleRules()
            : this(ShooterRuleSet.Default)
        {
        }

        public ShooterBattleRules(float playerSpeed, float bulletSpeed, int bulletLifeFrames, float hitRadius, int hitDamage)
            : this(new ShooterRuleSet(
                ShooterRuleSet.DefaultRuleSetId,
                ShooterRuleSet.DefaultConfigVersion,
                playerSpeed,
                bulletSpeed,
                bulletLifeFrames,
                hitRadius,
                hitDamage))
        {
        }

        public ShooterBattleRules(ShooterRuleSet ruleSet)
        {
            RuleSet = ruleSet;
        }

        public ShooterRuleSet RuleSet { get; }

        public int RuleSetId => RuleSet.RuleSetId;

        public int ConfigVersion => RuleSet.ConfigVersion;

        public float PlayerSpeed => RuleSet.PlayerSpeed;

        public float BulletSpeed => RuleSet.BulletSpeed;

        public int BulletLifeFrames => RuleSet.BulletLifeFrames;

        public float HitRadius => RuleSet.HitRadius;

        public int HitDamage => RuleSet.HitDamage;
    }
}
