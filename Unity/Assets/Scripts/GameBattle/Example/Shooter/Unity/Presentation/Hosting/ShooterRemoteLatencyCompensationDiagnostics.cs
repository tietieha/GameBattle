#nullable enable

namespace AbilityKit.Demo.Shooter.View.Hosting
{
    public readonly struct ShooterRemoteLatencyCompensationDiagnostics
    {
        public ShooterRemoteLatencyCompensationDiagnostics(
            bool hasResult,
            int requestedFrame,
            int acceptedFrame,
            int authoritativeFrame,
            int localAcceptedInputs,
            bool remoteSuccess,
            bool shouldResync,
            string status,
            long serverTicks,
            bool hasPendingInput,
            bool hasQueuedInput,
            long submittedCount,
            long queuedCount,
            long replacedCount,
            long completedCount,
            long failedCount,
            long resyncRequestedCount)
        {
            HasResult = hasResult;
            RequestedFrame = requestedFrame;
            AcceptedFrame = acceptedFrame;
            AuthoritativeFrame = authoritativeFrame;
            LocalAcceptedInputs = localAcceptedInputs;
            RemoteSuccess = remoteSuccess;
            ShouldResync = shouldResync;
            Status = status ?? string.Empty;
            ServerTicks = serverTicks;
            HasPendingInput = hasPendingInput;
            HasQueuedInput = hasQueuedInput;
            SubmittedCount = submittedCount;
            QueuedCount = queuedCount;
            ReplacedCount = replacedCount;
            CompletedCount = completedCount;
            FailedCount = failedCount;
            ResyncRequestedCount = resyncRequestedCount;
        }

        public bool HasResult { get; }
        public int RequestedFrame { get; }
        public int AcceptedFrame { get; }
        public int AuthoritativeFrame { get; }
        public int LocalAcceptedInputs { get; }
        public bool RemoteSuccess { get; }
        public bool ShouldResync { get; }
        public string Status { get; }
        public long ServerTicks { get; }
        public bool HasPendingInput { get; }
        public bool HasQueuedInput { get; }
        public long SubmittedCount { get; }
        public long QueuedCount { get; }
        public long ReplacedCount { get; }
        public long CompletedCount { get; }
        public long FailedCount { get; }
        public long ResyncRequestedCount { get; }
        public int InputDelayFrames => HasResult ? AcceptedFrame - RequestedFrame : 0;
        public int AuthoritativeFrameGap => HasResult ? AuthoritativeFrame - RequestedFrame : 0;
        public bool HasServerStamp => ServerTicks > 0L;

        public static ShooterRemoteLatencyCompensationDiagnostics FromGatewayInput(
            in ShooterClientGatewayInputSubmitResult result,
            bool hasPendingInput,
            bool hasQueuedInput,
            long submittedCount,
            long queuedCount,
            long replacedCount,
            long completedCount,
            long failedCount,
            long resyncRequestedCount)
        {
            var hasResult = result.Local.RequestedFrame != 0 || result.Local.AcceptedInputs != 0 ||
                result.Remote.ServerTicks > 0L || result.Remote.CurrentFrame != 0 ||
                result.Remote.AcceptedFrame != 0 || !string.IsNullOrWhiteSpace(result.Remote.Status);

            return new ShooterRemoteLatencyCompensationDiagnostics(
                hasResult,
                result.Local.RequestedFrame,
                result.Remote.AcceptedFrame,
                result.Remote.CurrentFrame,
                result.Local.AcceptedInputs,
                result.Remote.Success,
                result.Remote.ShouldResync,
                result.Remote.Status,
                result.Remote.ServerTicks,
                hasPendingInput,
                hasQueuedInput,
                submittedCount,
                queuedCount,
                replacedCount,
                completedCount,
                failedCount,
                resyncRequestedCount);
        }
    }
}
