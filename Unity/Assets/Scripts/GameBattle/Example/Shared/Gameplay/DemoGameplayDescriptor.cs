namespace AbilityKit.Demo.Common
{
    public readonly struct DemoGameplayDescriptor
    {
        public DemoGameplayDescriptor(string roomType, int gameplayId, string displayName, string defaultWorldType)
        {
            RoomType = roomType ?? string.Empty;
            GameplayId = gameplayId;
            DisplayName = displayName ?? string.Empty;
            DefaultWorldType = defaultWorldType ?? string.Empty;
        }

        public string RoomType { get; }

        public int GameplayId { get; }

        public string DisplayName { get; }

        public string DefaultWorldType { get; }
    }
}
