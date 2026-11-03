namespace AbilityKit.Context
{
    /// <summary>
    /// 上下文事件类型
    /// </summary>
    public enum ContextEventType
    {
        Created = 0,
        Updated = 1,
        Destroying = 2,
        Destroyed = 3,
        FlowCreated = 4,
        FlowPhaseChanged = 5
    }
}
