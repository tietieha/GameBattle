using System;
using AbilityKit.Core.Logging;
using AbilityKit.Triggering.Registry;
using AbilityKit.Triggering.Runtime.Plan;
using AbilityKit.Triggering.Variables.Numeric;
using AbilityKit.Triggering.Runtime.Context;
namespace AbilityKit.Triggering.Runtime.Executable
{
    /// <summary>
    /// 空操作行为。
    /// </summary>
    public sealed class NoOpExecutable : IAtomicExecutable, ISimpleExecutable
    {
        public static readonly NoOpExecutable Instance = new();

        public string Name => "NoOp";
        public ExecutableMetadata Metadata => new(0, "NoOp");

        public ExecutionResult Execute(ActionContext ctx)
            => ExecutionResult.Success(0);
    }

    /// <summary>
    /// 失败行为
    /// </summary>
    public sealed class FailExecutable : IAtomicExecutable, ISimpleExecutable
    {
        public static readonly FailExecutable Instance = new();

        public string Name => "Fail";
        public ExecutableMetadata Metadata => new(1, "Fail");

        public string Reason { get; set; }

        public ExecutionResult Execute(ActionContext ctx)
            => ExecutionResult.Failed(Reason ?? "Explicit failure");
    }

    /// <summary>
    /// 成功行为
    /// </summary>
    public sealed class SuccessExecutable : IAtomicExecutable, ISimpleExecutable
    {
        public static readonly SuccessExecutable Instance = new();

        public string Name => "Success";
        public ExecutableMetadata Metadata => new(2, "Success");

        public ExecutionResult Execute(ActionContext ctx)
            => ExecutionResult.Success(0);
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.ActionCall, "ActionCall")]
    public sealed class ActionCallExecutable : IAtomicExecutable, ISimpleExecutable
    {
        public string Name => "ActionCall";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.ActionCall, "ActionCall");

        public ActionId ActionId { get; set; }
        public NumericValueRef Arg0 { get; set; }
        public NumericValueRef Arg1 { get; set; }
        public int Arity { get; set; }

        private readonly Action<ActionContext> _action0;
        private readonly Action<ActionContext, double> _action1;
        private readonly Action<ActionContext, double, double> _action2;

        public ActionCallExecutable(Action<ActionContext> action, ActionId actionId)
        {
            _action0 = action ?? throw new ArgumentNullException(nameof(action));
            ActionId = actionId;
            Arity = 0;
        }

        public ActionCallExecutable(Action<ActionContext, double> action, ActionId actionId, NumericValueRef arg0)
        {
            _action1 = action ?? throw new ArgumentNullException(nameof(action));
            ActionId = actionId;
            Arity = 1;
            Arg0 = arg0;
        }

        public ActionCallExecutable(Action<ActionContext, double, double> action, ActionId actionId, NumericValueRef arg0, NumericValueRef arg1)
        {
            _action2 = action ?? throw new ArgumentNullException(nameof(action));
            ActionId = actionId;
            Arity = 2;
            Arg0 = arg0;
            Arg1 = arg1;
        }

        public ActionCallExecutable() { }

        public ExecutionResult Execute(ActionContext ctx)
        {
            try
            {
                var actionCtx = ctx ?? new ActionContext();

                ExecuteBound(actionCtx);
                return ExecutionResult.Success();
            }
            catch (Exception ex)
            {
                return ExecutionResult.Failed($"ActionCall[{ActionId}]: {ex.Message}");
            }
        }

        private void ExecuteBound(ActionContext ctx)
        {
            switch (Arity)
            {
                case 0 when _action0 != null:
                    _action0(ctx);
                    return;
                case 1 when _action1 != null:
                    _action1(ctx, Arg0.Resolve(ctx));
                    return;
                case 2 when _action2 != null:
                    _action2(ctx, Arg0.Resolve(ctx), Arg1.Resolve(ctx));
                    return;
                default:
                    throw new InvalidOperationException($"ActionCall[{ActionId}] is not bound. Create it through ActionDelegateFactory or a constructor with an action delegate.");
            }
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.Delay, "Delay")]
    public sealed class DelayExecutable : IAtomicExecutable, ISimpleExecutable
    {
        public string Name => "Delay";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.Delay, "Delay");

        public float DelayMs { get; set; }
        public float ActualDelayMs { get; private set; }

        public ExecutionResult Execute(ActionContext ctx) { ActualDelayMs = DelayMs; return ExecutionResult.Success(); }
    }

    public sealed class WaitExecutable : IAtomicExecutable, ISimpleExecutable
    {
        public string Name => "Wait";
        public ExecutableMetadata Metadata => new(201, "Wait");

        public float DurationMs { get; set; }
        private float _elapsed;

        public ExecutionResult Execute(ActionContext ctx)
        {
            _elapsed = 0f;
            return ExecutionResult.Success();
        }

        public void Update(float deltaTimeMs)
        {
            _elapsed += deltaTimeMs;
        }

        public bool IsCompleted => _elapsed >= DurationMs;
    }

    public sealed class EventSendExecutable : IAtomicExecutable, ISimpleExecutable
    {
        public string Name => "EventSend";
        public ExecutableMetadata Metadata => new(300, "EventSend");

        public string EventName { get; set; }
        public ActionRegistry Events { get; set; }

        public ExecutionResult Execute(ActionContext ctx)
        {
            try
            {
                int eventId = EventName?.GetHashCode() ?? 0;
                if (Events?.TryGet<Action<ActionContext>>(new ActionId(eventId), out var action, out _) == true)
                {
                    action(ctx);
                }
                return ExecutionResult.Success();
            }
            catch (Exception ex)
            {
                return ExecutionResult.Failed($"EventSend[{EventName}]: {ex.Message}");
            }
        }
    }

    public sealed class DebugLogExecutable : IAtomicExecutable, ISimpleExecutable
    {
        public string Name => "DebugLog";
        public ExecutableMetadata Metadata => new(999, "DebugLog");

        public string Message { get; set; }
        public bool LogToConsole { get; set; } = true;

        public ExecutionResult Execute(ActionContext ctx)
        {
            if (LogToConsole)
            {
                Log.Info($"[Triggering] {Message}");
            }
            return ExecutionResult.Success();
        }
    }
}

