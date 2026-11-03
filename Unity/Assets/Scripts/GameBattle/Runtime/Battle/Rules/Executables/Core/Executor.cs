using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Context;

namespace AbilityKit.Triggering.Runtime.Executable
{
    /// <summary>
    /// 调度执行器 - 支持 IScheduledExecutable 和 IComposableExecutable
    /// </summary>
    public sealed class ScheduledExecutor
    {
        private readonly Dictionary<long, ControllerEntry> _controllers = new();
        private long _nextHandleId;

        /// <summary>行为句柄信息</summary>
        public long Start(
            IScheduledExecutable executable,
            ActionContext ctx,
            Action<long> onCompleted = null,
            Action<long, string> onInterrupted = null)
        {
            return StartInternal(executable, ctx, onCompleted, onInterrupted);
        }

        /// <summary>
        /// 启动可组合行为
        /// </summary>
        public long StartComposable(
            IComposableExecutable executable,
            ActionContext ctx,
            Action<long> onCompleted = null,
            Action<long, string> onInterrupted = null)
        {
            return StartInternal(executable, ctx, onCompleted, onInterrupted);
        }

        private long StartInternal(
            ISimpleExecutable executable,
            ActionContext ctx,
            Action<long> onCompleted,
            Action<long, string> onInterrupted)
        {
            var handleId = ++_nextHandleId;
            IScheduleController controller;

            if (executable is IScheduledExecutable scheduled)
            {
                controller = ScheduledExecutableFactory.CreateController(scheduled, ctx);
            }
            else if (executable is IComposableExecutable composable && composable.Inner is IScheduledExecutable innerScheduled)
            {
                controller = ScheduledExecutableFactory.CreateController(innerScheduled, ctx);
            }
            else
            {
                controller = NullScheduleController.Instance;
            }

            _controllers[handleId] = new ControllerEntry
            {
                Executable = executable,
                Controller = controller,
                Context = ctx,
                OnCompleted = onCompleted,
                OnInterrupted = onInterrupted
            };

            if (executable is IComposableExecutable comp)
            {
                InitializeDecorators(comp, ctx, handleId);
            }

            return handleId;
        }

        private void InitializeDecorators(IComposableExecutable executable, ActionContext ctx, long handleId)
        {
            if (executable is IDurationDecorator durationDeco)
            {
                durationDeco.OnExpired += _ =>
                {
                    if (_controllers.TryGetValue(handleId, out var entry))
                    {
                        entry.Controller.RequestInterrupt("Duration expired");
                    }
                };
            }

            if (executable.Inner is IComposableExecutable innerDeco)
            {
                InitializeDecorators(innerDeco, ctx, handleId);
            }
        }

        public bool Interrupt(long handleId, string reason)
        {
            if (!_controllers.TryGetValue(handleId, out var entry))
                return false;

            if (entry.Controller.IsCompleted || entry.Controller.IsInterrupted)
                return false;

            if (!CanBeInterrupted(entry.Executable))
                return false;

            entry.Controller.RequestInterrupt(reason);
            return true;
        }

        private bool CanBeInterrupted(ISimpleExecutable executable)
        {
            if (executable is ExternalControlledExecutable ext && !ext.CanBeInterrupted)
                return false;

            if (executable is IComposableExecutable comp && comp.Inner != null)
                return CanBeInterrupted(comp.Inner);

            return true;
        }

        public bool TryGetHandle(long handleId, out ControllerEntry entry)
        {
            return _controllers.TryGetValue(handleId, out entry);
        }

        public void Update(float deltaTimeMs)
        {
            var completed = new List<long>();

            foreach (var kvp in _controllers)
            {
                var entry = kvp.Value;

                if (entry.Controller.IsCompleted)
                {
                    completed.Add(kvp.Key);
                    entry.OnCompleted?.Invoke(kvp.Key);
                }
                else if (entry.Controller.IsInterrupted)
                {
                    completed.Add(kvp.Key);
                    entry.OnInterrupted?.Invoke(kvp.Key, entry.Controller.InterruptionReason);
                }
                else
                {
                    entry.Controller.Update(deltaTimeMs);

                    if (entry.Executable is IComposableExecutable comp)
                    {
                        UpdateDecorators(comp, entry.Context, deltaTimeMs, kvp.Key);
                    }
                }
            }

            foreach (var handleId in completed)
            {
                _controllers.Remove(handleId);
            }
        }

        private void UpdateDecorators(IComposableExecutable executable, ActionContext ctx, float deltaTimeMs, long handleId)
        {
            if (executable is IDurationDecorator durationDeco)
            {
                if (durationDeco.Update(ctx, deltaTimeMs))
                {
                    if (_controllers.TryGetValue(handleId, out var entry))
                    {
                        entry.Controller.RequestInterrupt("Duration expired");
                    }
                }
            }

            if (executable.Inner is IComposableExecutable innerDeco)
            {
                UpdateDecorators(innerDeco, ctx, deltaTimeMs, handleId);
            }
        }

        public bool TryGetDurationDecorator(long handleId, out IDurationDecorator decorator)
        {
            decorator = null;
            if (!_controllers.TryGetValue(handleId, out var entry))
                return false;

            return TryFindDurationDecorator(entry.Executable, out decorator);
        }

        private bool TryFindDurationDecorator(ISimpleExecutable executable, out IDurationDecorator decorator)
        {
            decorator = null;
            if (executable is IDurationDecorator deco)
            {
                decorator = deco;
                return true;
            }

            if (executable is IComposableExecutable comp && comp.Inner != null)
            {
                return TryFindDurationDecorator(comp.Inner, out decorator);
            }

            return false;
        }

        public bool RefreshDuration(long handleId, float additionalMs)
        {
            if (TryGetDurationDecorator(handleId, out var decorator))
            {
                decorator.Refresh(additionalMs);
                return true;
            }
            return false;
        }

        public void InterruptAll(string reason)
        {
            foreach (var kvp in _controllers)
            {
                var entry = kvp.Value;
                if (!CanBeInterrupted(entry.Executable)) continue;

                if (!entry.Controller.IsCompleted && !entry.Controller.IsInterrupted)
                {
                    entry.Controller.RequestInterrupt(reason);
                    entry.OnInterrupted?.Invoke(kvp.Key, reason);
                }
            }

            _controllers.Clear();
        }

        public int ActiveCount => _controllers.Count;
        public bool HasActive => _controllers.Count > 0;

        public struct ControllerEntry
        {
            public ISimpleExecutable Executable;
            public IScheduleController Controller;
            public ActionContext Context;
            public Action<long> OnCompleted;
            public Action<long, string> OnInterrupted;
        }
    }

    /// <summary>
    /// 行为执行器
    /// </summary>
    public static class ExecutableExecutor
    {
        public static ExecutionResult Execute(ISimpleExecutable executable, ActionContext ctx)
        {
            if (executable == null)
                return ExecutionResult.None;

            if (executable is IConditionalExecutable conditional)
            {
                return ExecuteConditional(conditional, ctx);
            }

            try
            {
                return executable.Execute(ctx);
            }
            catch (Exception ex)
            {
                return ExecutionResult.Failed($"Execute[{executable.Name}]: {ex.Message}");
            }
        }

        private static ExecutionResult ExecuteConditional(IConditionalExecutable executable, ActionContext ctx)
        {
            int matchedIndex = executable.EvaluateConditionIndex(ctx);

            if (matchedIndex >= 0 && matchedIndex < executable.ChildCount)
            {
                var child = executable.GetChild(matchedIndex);
                if (child != null)
                {
                    if (child is IConditionalExecutable childConditional)
                    {
                        return ExecuteConditional(childConditional, ctx);
                    }

                    try
                    {
                        return child.Execute(ctx);
                    }
                    catch (Exception ex)
                    {
                        return ExecutionResult.Failed($"Conditional[{matchedIndex}][{child.Name}]: {ex.Message}");
                    }
                }
            }

            return ExecutionResult.Skipped("No matching branch");
        }

        public static ExecutionResult ExecuteAll(
            IEnumerable<ISimpleExecutable> executables,
            ActionContext ctx)
        {
            var result = ExecutionResult.None;

            foreach (var executable in executables)
            {
                if (executable == null) continue;
                var execResult = Execute(executable, ctx);
                result = result.Merge(execResult);
            }

            return result;
        }
    }
}
