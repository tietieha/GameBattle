#nullable enable

using System;

namespace AbilityKit.Ability.Host.Extensions.Client.FrameSync
{
    public readonly struct ClientPredictionReconciliationResult
    {
        public static readonly ClientPredictionReconciliationResult None = new ClientPredictionReconciliationResult(
            0,
            0u,
            0,
            0u,
            0u,
            false,
            0,
            0,
            0u,
            0,
            0,
            0);

        public readonly int PredictedFrameBeforeCorrection;
        public readonly uint PredictedHashBeforeCorrection;
        public readonly int AuthoritativeFrame;
        public readonly uint AuthoritativeStateHash;
        public readonly uint ImportedStateHash;
        public readonly bool AuthoritativeHashMatched;
        public readonly int ReplayTicks;
        public readonly int FinalFrame;
        public readonly uint FinalStateHash;
        public readonly int PendingInputFramesBeforeCorrection;
        public readonly int PendingInputFramesAfterTrim;
        public readonly int PendingInputFramesAfterReplay;

        public ClientPredictionReconciliationResult(
            int predictedFrameBeforeCorrection,
            uint predictedHashBeforeCorrection,
            int authoritativeFrame,
            uint authoritativeStateHash,
            uint importedStateHash,
            bool authoritativeHashMatched,
            int replayTicks,
            int finalFrame,
            uint finalStateHash,
            int pendingInputFramesBeforeCorrection,
            int pendingInputFramesAfterTrim,
            int pendingInputFramesAfterReplay)
        {
            PredictedFrameBeforeCorrection = predictedFrameBeforeCorrection;
            PredictedHashBeforeCorrection = predictedHashBeforeCorrection;
            AuthoritativeFrame = authoritativeFrame;
            AuthoritativeStateHash = authoritativeStateHash;
            ImportedStateHash = importedStateHash;
            AuthoritativeHashMatched = authoritativeHashMatched;
            ReplayTicks = replayTicks;
            FinalFrame = finalFrame;
            FinalStateHash = finalStateHash;
            PendingInputFramesBeforeCorrection = pendingInputFramesBeforeCorrection;
            PendingInputFramesAfterTrim = pendingInputFramesAfterTrim;
            PendingInputFramesAfterReplay = pendingInputFramesAfterReplay;
        }
    }

    public sealed class ClientPredictionReconciliationCoordinator<TInput>
    {
        private readonly ClientPredictionInputHistory<TInput> _inputHistory;

        public ClientPredictionReconciliationCoordinator()
            : this(new ClientPredictionInputHistory<TInput>())
        {
        }

        public ClientPredictionReconciliationCoordinator(ClientPredictionInputHistory<TInput> inputHistory)
        {
            _inputHistory = inputHistory ?? throw new ArgumentNullException(nameof(inputHistory));
        }

        public int PendingInputFrameCount => _inputHistory.Count;

        public void Clear()
        {
            _inputHistory.Clear();
        }

        public void RecordLocalInput(int frame, TInput[] inputs)
        {
            _inputHistory.Record(frame, inputs);
        }

        public ClientPredictionReconciliationResult ReconcileAfterAuthoritativeSnapshot(
            int replayTargetFrame,
            uint predictedHashBeforeCorrection,
            int authoritativeFrame,
            uint authoritativeStateHash,
            uint importedStateHash,
            int confirmedFrame,
            Func<int> getCurrentFrame,
            Func<uint> computeStateHash,
            Func<int, TInput[], int> submitInput,
            Func<bool> stepFrame)
        {
            if (getCurrentFrame == null) throw new ArgumentNullException(nameof(getCurrentFrame));
            if (computeStateHash == null) throw new ArgumentNullException(nameof(computeStateHash));
            if (submitInput == null) throw new ArgumentNullException(nameof(submitInput));
            if (stepFrame == null) throw new ArgumentNullException(nameof(stepFrame));

            var pendingBeforeCorrection = _inputHistory.Count;
            _inputHistory.TrimBefore(confirmedFrame);
            var pendingAfterTrim = _inputHistory.Count;

            var replay = _inputHistory.ReplayTo(
                replayTargetFrame,
                getCurrentFrame,
                submitInput,
                stepFrame);

            var finalStateHash = computeStateHash();
            return new ClientPredictionReconciliationResult(
                replayTargetFrame,
                predictedHashBeforeCorrection,
                authoritativeFrame,
                authoritativeStateHash,
                importedStateHash,
                authoritativeStateHash == importedStateHash,
                replay.ReplayTicks,
                replay.FinalFrame,
                finalStateHash,
                pendingBeforeCorrection,
                pendingAfterTrim,
                _inputHistory.Count);
        }
    }
}
