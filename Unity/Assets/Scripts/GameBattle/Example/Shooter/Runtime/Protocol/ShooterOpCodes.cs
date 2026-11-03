namespace AbilityKit.Protocol.Shooter
{
    public static class ShooterOpCodes
    {
        public static class Input
        {
            public const int PlayerCommand = 5101;
        }

        public static class Snapshot
        {
            public const int StartGame = 5201;
            public const int State = 5202;
            public const int Events = 5203;
            public const int PackedState = 5204;
            public const int PackedStateDelta = 5205;
            public const int StateHash = 5206;
            public const int PureState = 5207;
            public const int PureStateDelta = 5208;
        }
    }
}
