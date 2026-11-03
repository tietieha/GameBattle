#nullable enable

using System;
using AbilityKit.Demo.Shooter.View.Hosting;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    internal readonly struct ShooterRemoteInputPumpResult
    {
        public ShooterRemoteInputPumpResult(ShooterHostFrameInput input, ShooterClientInputSubmitResult submitResult)
        {
            Input = input;
            SubmitResult = submitResult;
        }

        public ShooterHostFrameInput Input { get; }
        public ShooterClientInputSubmitResult SubmitResult { get; }
    }

    internal sealed class ShooterRemoteInputPump
    {
        private readonly IShooterHostInputSource _inputSource;

        public ShooterRemoteInputPump(IShooterHostInputSource inputSource)
        {
            _inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
        }

        public ShooterRemoteInputPumpResult SubmitFrameInput(
            ShooterClientSession session,
            int controlledPlayerId,
            ShooterRemoteInputSubmitStrategy? inputSubmitStrategy)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var input = _inputSource.ReadInput(controlledPlayerId);
            var command = ShooterClientInputBuilder.CreateCommand(
                controlledPlayerId,
                input.MoveX,
                input.MoveY,
                input.AimX,
                input.AimY,
                input.Fire,
                input.AttackSlot);

            var submitResult = session.SubmitLocalInput(in command);
            inputSubmitStrategy?.SubmitOrQueue(in submitResult);
            return new ShooterRemoteInputPumpResult(input, submitResult);
        }
    }
}
