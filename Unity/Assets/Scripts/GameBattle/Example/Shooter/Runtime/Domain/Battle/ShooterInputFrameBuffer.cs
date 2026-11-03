using System;
using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public sealed class ShooterInputFrameBuffer
    {
        private readonly FrameCommandBuffer<int, ShooterPlayerCommand> _frames = new FrameCommandBuffer<int, ShooterPlayerCommand>(
            120,
            ShooterPlayerCommandComparer.Instance);
        private readonly Dictionary<int, ShooterPlayerCommand> _latest = new Dictionary<int, ShooterPlayerCommand>();

        public int OldestRetainedFrame => _frames.OldestRetainedFrame;

        public IReadOnlyDictionary<int, ShooterPlayerCommand> LatestCommands => _latest;

        public int RetainedFrameWindow => _frames.RetainedFrameWindow;

        public void Clear()
        {
            _frames.Clear();
            _latest.Clear();
        }

        public void SetRetainedFrameWindow(int frames)
        {
            _frames.SetRetainedFrameWindow(frames, _latest.Count == 0 ? 0 : _frames.LatestFrame);
        }

        public void SetCommand(int frame, in ShooterPlayerCommand command)
        {
            SubmitCommand(frame, in command);
        }

        public void SubmitCommand(int frame, in ShooterPlayerCommand command)
        {
            _frames.SubmitCommand(frame, command.PlayerId, in command);
            _latest[command.PlayerId] = command;
        }

        public bool TryGetLatestCommand(int playerId, out ShooterPlayerCommand command)
        {
            return _latest.TryGetValue(playerId, out command);
        }

        public bool TryGetCommand(int frame, int playerId, out ShooterPlayerCommand command)
        {
            return _frames.TryGetCommand(frame, playerId, out command);
        }

        public IReadOnlyDictionary<int, ShooterPlayerCommand> GetFrameCommandsOrEmpty(int frame)
        {
            return _frames.GetFrameCommandsOrEmpty(frame);
        }

        public bool TryGetFrameCommands(int frame, out IReadOnlyDictionary<int, ShooterPlayerCommand> commands)
        {
            return _frames.TryGetFrameCommands(frame, out commands);
        }

        public int CopyFrameCommands(int frame, List<ShooterPlayerCommand> destination)
        {
            return _frames.CopyFrameCommands(frame, destination);
        }

        public int CopyRetainedFrameNumbers(List<int> destination, int startFrameInclusive = 0, int endFrameExclusive = int.MaxValue)
        {
            return _frames.CopyRetainedFrameNumbers(destination, startFrameInclusive, endFrameExclusive);
        }

        public void SetLatestCommand(in ShooterPlayerCommand command)
        {
            _latest[command.PlayerId] = command;
        }

        public bool TryConsumeLatestFire(int frame, int playerId, out ShooterPlayerCommand command)
        {
            if (!_latest.TryGetValue(playerId, out command) || !command.Fire)
            {
                return false;
            }

            command.Fire = false;
            _latest[playerId] = command;
            if (_frames.TryGetCommand(frame, playerId, out _))
            {
                _frames.SetCommand(frame, playerId, in command);
            }

            return true;
        }

        public bool RemoveLatestCommand(int playerId)
        {
            return _latest.Remove(playerId);
        }

        public void TrimBefore(int frame)
        {
            _frames.TrimBefore(frame);
        }

        public void TrimToWindow(int currentFrame)
        {
            _frames.TrimToWindow(currentFrame);
        }

        private sealed class ShooterPlayerCommandComparer : IComparer<ShooterPlayerCommand>
        {
            public static readonly ShooterPlayerCommandComparer Instance = new ShooterPlayerCommandComparer();

            public int Compare(ShooterPlayerCommand left, ShooterPlayerCommand right)
            {
                return left.PlayerId.CompareTo(right.PlayerId);
            }
        }
    }
}
