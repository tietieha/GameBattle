namespace AbilityKit.Demo.Shooter.Runtime
{
    public enum ShooterBotAiProfile
    {
        None = 0,
        SimpleBattle = 1
    }

    public readonly struct ShooterBotAiMountOptions
    {
        public ShooterBotAiMountOptions(int playerId, ShooterBotAiProfile profile = ShooterBotAiProfile.SimpleBattle, string? profileId = null)
        {
            PlayerId = playerId;
            Profile = profile;
            ProfileId = profileId ?? string.Empty;
        }

        public int PlayerId { get; }

        public ShooterBotAiProfile Profile { get; }

        public string ProfileId { get; }
    }

    public interface IShooterBotAiPort
    {
        int BotAiCount { get; }

        bool MountBotAi(in ShooterBotAiMountOptions options);

        bool UnmountBotAi(int playerId);

        void ClearBotAi();
    }
}
