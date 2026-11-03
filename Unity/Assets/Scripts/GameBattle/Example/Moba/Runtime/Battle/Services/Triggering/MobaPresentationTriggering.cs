namespace AbilityKit.Demo.Moba.Services.Triggering
{
    public static class MobaPresentationTriggering
    {
        public static class Prefixes
        {
            public const string Presentation = "presentation.";
        }

        public static class Events
        {
            public const string Play = Prefixes.Presentation + "play";
            public const string Stop = Prefixes.Presentation + "stop";
        }
    }
}
