namespace AbilityKit.Ability.Host.Extensions.Server.BattleHost
{
    public enum BattleHostLifecycleErrorCode
    {
        None = 0,
        AlreadyStarted = 1,
        CreateHostFailed = 2,
        RuntimeNotResolved = 3,
        RuntimeNotReadyForStart = 4,
        StartRuntimeRejected = 5,
        SnapshotProviderNotResolved = 6,
        TimerStartFailed = 7,
        StopFailed = 8,
        InvalidContext = 9
    }

    public readonly struct BattleHostLifecycleResult
    {
        public readonly bool Succeeded;
        public readonly BattleHostLifecycleErrorCode ErrorCode;
        public readonly string Message;

        private BattleHostLifecycleResult(bool succeeded, BattleHostLifecycleErrorCode errorCode, string message)
        {
            Succeeded = succeeded;
            ErrorCode = errorCode;
            Message = message ?? string.Empty;
        }

        public static BattleHostLifecycleResult Success(string message = "")
        {
            return new BattleHostLifecycleResult(true, BattleHostLifecycleErrorCode.None, message);
        }

        public static BattleHostLifecycleResult Fail(BattleHostLifecycleErrorCode errorCode, string message = "")
        {
            return new BattleHostLifecycleResult(false, errorCode, message);
        }

        public override string ToString()
        {
            return Succeeded ? "Success" : $"{ErrorCode}: {Message}";
        }
    }
}
