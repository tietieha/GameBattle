#nullable enable

using System;
using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Core.Recording.Core;
using AbilityKit.Core.Recording.FrameRecord;
using AbilityKit.Demo.Shooter.View.Hosting;
using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    public sealed class ShooterFrameRecordInputSource : IShooterHostInputSource
    {
        private readonly IFrameReplaySource _replaySource;
        private int _controlledPlayerId;
        private int _frameCursor;
        private int _inputFrameCount = -1;
        private ShooterHostFrameInput _lastInput;

        public ShooterFrameRecordInputSource(FrameRecordFile record, int controlledPlayerId)
            : this(new FrameRecordReplaySource(record), controlledPlayerId)
        {
        }

        public ShooterFrameRecordInputSource(IFrameReplaySource replaySource, int controlledPlayerId)
        {
            _replaySource = replaySource ?? throw new ArgumentNullException(nameof(replaySource));
            _controlledPlayerId = controlledPlayerId;
        }

        public int FrameCursor => _frameCursor;

        public int InputFrameCount => _inputFrameCount >= 0 ? _inputFrameCount : 0;

        public void Reset()
        {
            _frameCursor = 0;
            _lastInput = default;
        }

        public ShooterHostFrameInput ReadInput(int controlledPlayerId)
        {
            _controlledPlayerId = controlledPlayerId;
            if (TryReadInput(new FrameIndex(_frameCursor), controlledPlayerId, out var input))
            {
                _lastInput = input;
            }
            else
            {
                _lastInput = default;
            }

            _frameCursor++;
            return _lastInput;
        }

        private bool TryReadInput(FrameIndex frame, int controlledPlayerId, out ShooterHostFrameInput input)
        {
            input = default;
            if (!_replaySource.TryGetInputs(frame, out var inputs) || inputs.Count == 0)
            {
                return false;
            }

            for (var i = 0; i < inputs.Count; i++)
            {
                if (TryDecodeInput(inputs[i], controlledPlayerId, out input))
                {
                    if (frame.Value + 1 > _inputFrameCount)
                    {
                        _inputFrameCount = frame.Value + 1;
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool TryDecodeInput(in PlayerInputCommand input, int controlledPlayerId, out ShooterHostFrameInput hostInput)
        {
            hostInput = default;
            if (input.OpCode != ShooterOpCodes.Input.PlayerCommand)
            {
                return false;
            }

            var commands = ShooterInputCodec.Deserialize(input.Payload);
            for (var c = 0; c < commands.Length; c++)
            {
                var command = commands[c];
                if (command.PlayerId != controlledPlayerId)
                {
                    continue;
                }

                hostInput = new ShooterHostFrameInput(
                    command.MoveX,
                    command.MoveY,
                    command.AimX,
                    command.AimY,
                    command.Fire,
                    command.AttackSlot);
                return true;
            }

            return false;
        }
    }
}
