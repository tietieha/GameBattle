namespace AbilityKit.Demo.Moba
{
    public enum BuffStackingPolicy
    {
        None = 0,
        IgnoreIfExists = 1,
        Replace = 2,
        AddStack = 3,
        RefreshDuration = 4,
    }

    public enum BuffRefreshPolicy
    {
        None = 0,
        KeepRemaining = 1,
        ResetRemaining = 2,
        AddRemaining = 3,
    }
}
