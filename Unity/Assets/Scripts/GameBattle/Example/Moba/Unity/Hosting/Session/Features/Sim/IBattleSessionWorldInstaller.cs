namespace AbilityKit.Game.Flow
{
    internal interface IBattleSessionWorldInstaller
    {
        void EnsureRemoteDrivenStarted(RemoteDrivenWorldInstallOptions options);
        void EnsureConfirmedAuthorityStarted(ConfirmedAuthorityWorldInstallOptions options);
    }

    internal sealed class DefaultBattleSessionWorldInstaller : IBattleSessionWorldInstaller
    {
        public void EnsureRemoteDrivenStarted(RemoteDrivenWorldInstallOptions options)
        {
            RemoteDrivenWorldInstaller.EnsureStarted(options);
        }

        public void EnsureConfirmedAuthorityStarted(ConfirmedAuthorityWorldInstallOptions options)
        {
            ConfirmedAuthorityWorldInstaller.EnsureStarted(options);
        }
    }
}
