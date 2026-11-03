using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;

namespace AbilityKit.Demo.Moba.Services.LogicWorld
{
    public enum LogicWorldInputSubmitFailureCode
    {
        None = 0,
        NullOrEmptyCommands = 1,
        FrameRejected = 2,
        CommandFrameMismatch = 3,
        ContextMissing = 4,
        CommandRejected = 5,
        HandlerException = 6,
    }

    public readonly struct LogicWorldInputSubmitResult
    {
        public readonly bool Succeeded;
        public readonly LogicWorldInputSubmitFailureCode FailureCode;
        public readonly int AcceptedCount;
        public readonly int HandledCount;
        public readonly string Message;

        public LogicWorldInputSubmitResult(bool succeeded, LogicWorldInputSubmitFailureCode failureCode, int acceptedCount, int handledCount, string message)
        {
            Succeeded = succeeded;
            FailureCode = failureCode;
            AcceptedCount = acceptedCount;
            HandledCount = handledCount;
            Message = message;
        }

        public static LogicWorldInputSubmitResult Accepted(int acceptedCount, int handledCount)
        {
            return Accepted(acceptedCount, handledCount, null);
        }

        public static LogicWorldInputSubmitResult Accepted(int acceptedCount, int handledCount, string message)
        {
            return new LogicWorldInputSubmitResult(true, LogicWorldInputSubmitFailureCode.None, acceptedCount, handledCount, message);
        }

        public static LogicWorldInputSubmitResult Rejected(LogicWorldInputSubmitFailureCode failureCode, string message)
        {
            return new LogicWorldInputSubmitResult(false, failureCode, 0, 0, message);
        }

        public override string ToString()
        {
            return Succeeded
                ? $"Success: Accepted={AcceptedCount}, Handled={HandledCount}, Message={Message}"
                : $"Rejected: Code={FailureCode}, Message={Message}";
        }
    }

    /// <summary>
    /// 逻辑世界输入协调器统一接口，承接外部输入批次并交由具体逻辑层处理。
    /// </summary>
    public interface ILogicWorldInputCoordinator
    {
        void Submit(FrameIndex frame, IReadOnlyList<PlayerInputCommand> inputs);

        LogicWorldInputSubmitResult TrySubmit(FrameIndex frame, IReadOnlyList<PlayerInputCommand> inputs);
    }
}
