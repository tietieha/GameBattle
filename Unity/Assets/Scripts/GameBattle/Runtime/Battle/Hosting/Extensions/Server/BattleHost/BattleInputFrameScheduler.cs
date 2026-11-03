namespace AbilityKit.Ability.Host.Extensions.Server.BattleHost
{
    public enum BattleInputAcceptStatus
    {
        Accepted = 0,
        RemappedTooEarly = 1,
        RemappedLate = 2,
        RejectedInvalidFrame = 3,
        RejectedTooFarFuture = 4
    }

    public readonly struct BattleInputFrameScheduleResult
    {
        public readonly bool Accepted;
        public readonly int RequestedFrame;
        public readonly int AcceptedFrame;
        public readonly int CurrentFrame;
        public readonly int InputDelayFrames;
        public readonly BattleInputAcceptStatus Status;

        public BattleInputFrameScheduleResult(
            bool accepted,
            int requestedFrame,
            int acceptedFrame,
            int currentFrame,
            int inputDelayFrames,
            BattleInputAcceptStatus status)
        {
            Accepted = accepted;
            RequestedFrame = requestedFrame;
            AcceptedFrame = acceptedFrame;
            CurrentFrame = currentFrame;
            InputDelayFrames = inputDelayFrames;
            Status = status;
        }
    }

    public readonly struct BattleInputFrameSchedulerOptions
    {
        public static readonly BattleInputFrameSchedulerOptions Default = new BattleInputFrameSchedulerOptions(
            remapLateInputs: true,
            remapTooEarlyInputs: true,
            maxFutureLeadFrames: 120);

        public readonly bool RemapLateInputs;
        public readonly bool RemapTooEarlyInputs;
        public readonly int MaxFutureLeadFrames;

        public BattleInputFrameSchedulerOptions(bool remapLateInputs, bool remapTooEarlyInputs, int maxFutureLeadFrames)
        {
            RemapLateInputs = remapLateInputs;
            RemapTooEarlyInputs = remapTooEarlyInputs;
            MaxFutureLeadFrames = maxFutureLeadFrames > 0 ? maxFutureLeadFrames : 120;
        }
    }

    public static class BattleInputFrameScheduler
    {
        public static BattleInputFrameScheduleResult Schedule(
            int requestedFrame,
            int currentFrame,
            int inputDelayFrames,
            BattleInputFrameSchedulerOptions options)
        {
            if (requestedFrame < 0 || currentFrame < 0)
            {
                return new BattleInputFrameScheduleResult(
                    false,
                    requestedFrame,
                    requestedFrame,
                    currentFrame,
                    inputDelayFrames,
                    BattleInputAcceptStatus.RejectedInvalidFrame);
            }

            var delayFrames = inputDelayFrames > 0 ? inputDelayFrames : 0;
            var earliestFrame = currentFrame + delayFrames;
            var latestFrame = currentFrame + options.MaxFutureLeadFrames;

            if (requestedFrame > latestFrame)
            {
                return new BattleInputFrameScheduleResult(
                    false,
                    requestedFrame,
                    requestedFrame,
                    currentFrame,
                    delayFrames,
                    BattleInputAcceptStatus.RejectedTooFarFuture);
            }

            if (requestedFrame < currentFrame)
            {
                if (!options.RemapLateInputs)
                {
                    return new BattleInputFrameScheduleResult(
                        false,
                        requestedFrame,
                        requestedFrame,
                        currentFrame,
                        delayFrames,
                        BattleInputAcceptStatus.RemappedLate);
                }

                return new BattleInputFrameScheduleResult(
                    true,
                    requestedFrame,
                    earliestFrame,
                    currentFrame,
                    delayFrames,
                    BattleInputAcceptStatus.RemappedLate);
            }

            if (requestedFrame < earliestFrame)
            {
                if (!options.RemapTooEarlyInputs)
                {
                    return new BattleInputFrameScheduleResult(
                        true,
                        requestedFrame,
                        requestedFrame,
                        currentFrame,
                        delayFrames,
                        BattleInputAcceptStatus.Accepted);
                }

                return new BattleInputFrameScheduleResult(
                    true,
                    requestedFrame,
                    earliestFrame,
                    currentFrame,
                    delayFrames,
                    BattleInputAcceptStatus.RemappedTooEarly);
            }

            return new BattleInputFrameScheduleResult(
                true,
                requestedFrame,
                requestedFrame,
                currentFrame,
                delayFrames,
                BattleInputAcceptStatus.Accepted);
        }
    }
}
