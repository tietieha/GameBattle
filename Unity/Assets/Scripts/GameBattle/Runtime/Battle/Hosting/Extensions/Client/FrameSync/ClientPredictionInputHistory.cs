#nullable enable

using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.Host.Extensions.Client.FrameSync
{
    public readonly struct ClientPredictionReplayResult
    {
        public readonly int ReplayTicks;
        public readonly int FinalFrame;
        public readonly bool Completed;

        public ClientPredictionReplayResult(int replayTicks, int finalFrame, bool completed)
        {
            ReplayTicks = replayTicks;
            FinalFrame = finalFrame;
            Completed = completed;
        }
    }

    public sealed class ClientPredictionInputHistory<TInput>
    {
        private readonly List<PendingInputFrame> _pendingInputs;

        public ClientPredictionInputHistory(int capacity = 32)
        {
            _pendingInputs = new List<PendingInputFrame>(capacity > 0 ? capacity : 32);
        }

        public int Count => _pendingInputs.Count;

        public void Clear()
        {
            _pendingInputs.Clear();
        }

        public void Record(int frame, TInput[] inputs)
        {
            if (frame < 0 || inputs == null || inputs.Length == 0)
            {
                return;
            }

            var copy = new TInput[inputs.Length];
            Array.Copy(inputs, copy, inputs.Length);

            for (int i = 0; i < _pendingInputs.Count; i++)
            {
                if (_pendingInputs[i].Frame == frame)
                {
                    _pendingInputs[i] = _pendingInputs[i].Append(copy);
                    return;
                }
            }

            _pendingInputs.Add(new PendingInputFrame(frame, copy));
        }

        public void TrimBefore(int frameExclusive)
        {
            for (int i = _pendingInputs.Count - 1; i >= 0; i--)
            {
                if (_pendingInputs[i].Frame < frameExclusive)
                {
                    _pendingInputs.RemoveAt(i);
                }
            }
        }

        public int SubmitFrame(int frame, Func<int, TInput[], int> submit)
        {
            if (submit == null)
            {
                throw new ArgumentNullException(nameof(submit));
            }

            var submitted = 0;
            for (int i = 0; i < _pendingInputs.Count; i++)
            {
                var pending = _pendingInputs[i];
                if (pending.Frame == frame)
                {
                    submitted += submit(frame, pending.Inputs);
                }
            }

            return submitted;
        }

        public ClientPredictionReplayResult ReplayTo(int targetFrame, Func<int> getCurrentFrame, Func<int, TInput[], int> submit, Func<bool> stepFrame)
        {
            if (getCurrentFrame == null)
            {
                throw new ArgumentNullException(nameof(getCurrentFrame));
            }

            if (submit == null)
            {
                throw new ArgumentNullException(nameof(submit));
            }

            if (stepFrame == null)
            {
                throw new ArgumentNullException(nameof(stepFrame));
            }

            var replayTicks = 0;
            while (getCurrentFrame() < targetFrame)
            {
                var frame = getCurrentFrame();
                SubmitFrame(frame, submit);
                if (!stepFrame())
                {
                    break;
                }

                replayTicks++;
            }

            var finalFrame = getCurrentFrame();
            return new ClientPredictionReplayResult(replayTicks, finalFrame, finalFrame >= targetFrame);
        }

        private readonly struct PendingInputFrame
        {
            public readonly int Frame;
            public readonly TInput[] Inputs;

            public PendingInputFrame(int frame, TInput[] inputs)
            {
                Frame = frame;
                Inputs = inputs;
            }

            public PendingInputFrame Append(TInput[] inputs)
            {
                var merged = new TInput[Inputs.Length + inputs.Length];
                Array.Copy(Inputs, merged, Inputs.Length);
                Array.Copy(inputs, 0, merged, Inputs.Length, inputs.Length);
                return new PendingInputFrame(Frame, merged);
            }
        }
    }
}
