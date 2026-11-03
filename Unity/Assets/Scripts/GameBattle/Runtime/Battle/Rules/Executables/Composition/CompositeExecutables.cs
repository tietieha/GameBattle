using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.Context;

namespace AbilityKit.Triggering.Runtime.Executable
{
    [ExecutableTypeId(TypeIdRegistry.Executable.Sequence, "Sequence", isComposite: true)]
    public sealed class SequenceExecutable : ISimpleExecutable, ISequenceExecutable, ICompositeExecutable
    {
        private readonly List<ISimpleExecutable> _children = new();

        public string Name => "Sequence";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.Sequence, "Sequence", isComposite: true);
        public List<ISimpleExecutable> Children => _children;
        public int ChildCount => _children.Count;

        public SequenceExecutable Add(ISimpleExecutable child)
        {
            _children.Add(child);
            return this;
        }

        public SequenceExecutable AddRange(params ISimpleExecutable[] children)
        {
            if (children == null) return this;
            for (int i = 0; i < children.Length; i++) Add(children[i]);
            return this;
        }

        public ISimpleExecutable GetChild(int index) => index >= 0 && index < _children.Count ? _children[index] : null;

        public ExecutionResult Execute(ActionContext ctx)
        {
            var result = ExecutionResult.None;
            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                if (child == null) continue;

                try
                {
                    var childResult = child.Execute(ctx);
                    result = result.Merge(childResult);
                    if (childResult.IsFailed || childResult.IsInterrupted) return childResult;
                }
                catch (Exception ex)
                {
                    return ExecutionResult.Failed($"Sequence[{i}]: {ex.Message}");
                }
            }

            return result;
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.Selector, "Selector", isComposite: true)]
    public sealed class SelectorExecutable : ISimpleExecutable, ISelectorExecutable, ICompositeExecutable
    {
        private readonly List<ISimpleExecutable> _children = new();

        public string Name => "Selector";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.Selector, "Selector", isComposite: true);
        public List<ISimpleExecutable> Children => _children;
        public int ChildCount => _children.Count;

        public SelectorExecutable Add(ISimpleExecutable child)
        {
            _children.Add(child);
            return this;
        }

        public ISimpleExecutable GetChild(int index) => index >= 0 && index < _children.Count ? _children[index] : null;

        public ExecutionResult Execute(ActionContext ctx)
        {
            ExecutionResult lastResult = ExecutionResult.Skipped("No children");
            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                if (child == null) continue;

                try
                {
                    var result = child.Execute(ctx);
                    if (result.IsSuccess) return result;
                    if (result.IsInterrupted) return result;
                    lastResult = result;
                }
                catch (Exception ex)
                {
                    lastResult = ExecutionResult.Failed($"Selector[{i}]: {ex.Message}");
                }
            }

            return lastResult;
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.Parallel, "Parallel", isComposite: true)]
    public sealed class ParallelExecutable : ISimpleExecutable, IParallelExecutable, ICompositeExecutable
    {
        private readonly List<ISimpleExecutable> _children = new();

        public string Name => "Parallel";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.Parallel, "Parallel", isComposite: true);
        public List<ISimpleExecutable> Children => _children;
        public ECompositeMode ParallelMode { get; set; } = ECompositeMode.Parallel;
        public float TimeoutMs { get; set; }
        public int ChildCount => _children.Count;

        public ParallelExecutable Add(ISimpleExecutable child)
        {
            _children.Add(child);
            return this;
        }

        public ISimpleExecutable GetChild(int index) => index >= 0 && index < _children.Count ? _children[index] : null;

        public ExecutionResult Execute(ActionContext ctx)
        {
            var merged = ExecutionResult.None;
            var anySuccess = false;
            var anyFailure = false;
            ExecutionResult firstFailure = ExecutionResult.None;

            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                if (child == null) continue;

                ExecutionResult result;
                try
                {
                    result = child.Execute(ctx);
                }
                catch (Exception ex)
                {
                    result = ExecutionResult.Failed($"Parallel[{i}]: {ex.Message}");
                }

                if (result.IsInterrupted) return result;
                if (result.IsSuccess) anySuccess = true;
                if (result.IsFailed)
                {
                    anyFailure = true;
                    if (!firstFailure.IsFailed) firstFailure = result;
                    if (ParallelMode == ECompositeMode.ParallelSequence) return result;
                }

                if (ParallelMode == ECompositeMode.ParallelSelector && result.IsSuccess) return result;
                merged = merged.Merge(result);
            }

            if (ParallelMode == ECompositeMode.ParallelSelector)
                return anySuccess ? merged : (anyFailure ? firstFailure : ExecutionResult.Skipped("No successful parallel child"));

            if (anyFailure && ParallelMode != ECompositeMode.Parallel)
                return firstFailure;

            return merged;
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.If, "If", isComposite: true)]
    public sealed class IfExecutable : ISimpleExecutable, IConditionalExecutable, ICompositeExecutable
    {
        public string Name => "If";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.If, "If", isComposite: true);
        public ICondition Condition { get; set; }
        public ISimpleExecutable Body { get; set; }
        public int ChildCount => Body != null ? 1 : 0;

        public IfExecutable If(ICondition condition, ISimpleExecutable body)
        {
            Condition = condition;
            Body = body;
            return this;
        }

        public ISimpleExecutable GetChild(int index) => index == 0 ? Body : null;
        public int EvaluateConditionIndex(ActionContext ctx) => Condition?.Evaluate(ctx).Passed == true ? 0 : -1;

        public ExecutionResult Execute(ActionContext ctx)
        {
            if (EvaluateConditionIndex(ctx) < 0) return ExecutionResult.Skipped("Condition not passed");
            if (Body == null) return ExecutionResult.Success(0);
            try { return Body.Execute(ctx); }
            catch (Exception ex) { return ExecutionResult.Failed($"If.Body: {ex.Message}"); }
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.IfElse, "IfElse", isComposite: true)]
    public sealed class IfElseExecutable : ISimpleExecutable, IConditionalExecutable, ICompositeExecutable
    {
        private readonly List<Branch> _branches = new();
        private ISimpleExecutable _elseBody;

        public string Name => "IfElse";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.IfElse, "IfElse", isComposite: true);
        public int ChildCount => _branches.Count + (_elseBody != null ? 1 : 0);

        public IfElseExecutable If(ICondition condition, ISimpleExecutable body)
        {
            _branches.Add(new Branch(condition, body));
            return this;
        }

        public IfElseExecutable ElseIf(ICondition condition, ISimpleExecutable body) => If(condition, body);

        public IfElseExecutable Else(ISimpleExecutable body)
        {
            _elseBody = body;
            return this;
        }

        public ISimpleExecutable GetChild(int index)
        {
            if (index >= 0 && index < _branches.Count) return _branches[index].Body;
            return index == _branches.Count ? _elseBody : null;
        }

        public int EvaluateConditionIndex(ActionContext ctx)
        {
            for (int i = 0; i < _branches.Count; i++)
            {
                if (_branches[i].Condition?.Evaluate(ctx).Passed == true) return i;
            }

            return _elseBody != null ? _branches.Count : -1;
        }

        public ExecutionResult Execute(ActionContext ctx)
        {
            var index = EvaluateConditionIndex(ctx);
            if (index < 0) return ExecutionResult.Skipped("No matching branch");
            var child = GetChild(index);
            if (child == null) return ExecutionResult.Success(0);

            try { return child.Execute(ctx); }
            catch (Exception ex) { return ExecutionResult.Failed($"IfElse[{index}]: {ex.Message}"); }
        }

        private readonly struct Branch
        {
            public readonly ICondition Condition;
            public readonly ISimpleExecutable Body;

            public Branch(ICondition condition, ISimpleExecutable body)
            {
                Condition = condition;
                Body = body;
            }
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.Switch, "Switch", isComposite: true)]
    public sealed class SwitchExecutable : ISimpleExecutable, ISwitchExecutable, ICompositeExecutable
    {
        private readonly List<ISimpleExecutable> _children = new();

        public string Name => "Switch";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.Switch, "Switch", isComposite: true);
        public Func<object, int> ValueSelector { get; set; }
        public ISimpleExecutable DefaultBody { get; set; }
        public int ChildCount => _children.Count;

        public SwitchExecutable Add(ISimpleExecutable child)
        {
            _children.Add(child);
            return this;
        }

        public SwitchExecutable Case(int value, ISimpleExecutable body)
        {
            while (_children.Count <= value) _children.Add(null);
            _children[value] = body;
            return this;
        }

        public SwitchExecutable Default(ISimpleExecutable body)
        {
            DefaultBody = body;
            return this;
        }

        public ISimpleExecutable GetChild(int index) => index >= 0 && index < _children.Count ? _children[index] : null;

        public ExecutionResult Execute(ActionContext ctx)
        {
            var index = ValueSelector != null ? ValueSelector(ctx) : -1;
            var child = index >= 0 && index < _children.Count ? _children[index] : DefaultBody;
            if (child == null) return ExecutionResult.Skipped("No matching case");

            try { return child.Execute(ctx); }
            catch (Exception ex) { return ExecutionResult.Failed($"Switch[{index}]: {ex.Message}"); }
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.RandomSelector, "RandomSelector", isComposite: true)]
    public sealed class RandomSelectorExecutable : ISimpleExecutable, ICompositeExecutable
    {
        private static readonly System.Random Random = new();
        private readonly List<ISimpleExecutable> _children = new();
        private readonly List<float> _weights = new();

        public string Name => "RandomSelector";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.RandomSelector, "RandomSelector", isComposite: true);
        public List<ISimpleExecutable> Children => _children;
        public float[] Weights { get => _weights.ToArray(); set { _weights.Clear(); if (value != null) _weights.AddRange(value); } }
        public int ChildCount => _children.Count;

        public RandomSelectorExecutable Add(ISimpleExecutable child, float weight = 1f)
        {
            _children.Add(child);
            _weights.Add(weight <= 0f ? 0f : weight);
            return this;
        }

        public ISimpleExecutable GetChild(int index) => index >= 0 && index < _children.Count ? _children[index] : null;

        public ExecutionResult Execute(ActionContext ctx)
        {
            if (_children.Count == 0) return ExecutionResult.Skipped("No children");
            var index = SelectIndex();
            var child = GetChild(index);
            if (child == null) return ExecutionResult.Skipped($"Child[{index}] is null");

            try { return child.Execute(ctx); }
            catch (Exception ex) { return ExecutionResult.Failed($"RandomSelector[{index}]: {ex.Message}"); }
        }

        private int SelectIndex()
        {
            var total = 0f;
            for (int i = 0; i < _weights.Count; i++) total += _weights[i];
            if (total <= 0f) return Random.Next(_children.Count);

            var cursor = (float)(Random.NextDouble() * total);
            for (int i = 0; i < _weights.Count; i++)
            {
                cursor -= _weights[i];
                if (cursor <= 0f) return i;
            }

            return _children.Count - 1;
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.Repeat, "Repeat", isComposite: true)]
    public sealed class RepeatExecutable : ISimpleExecutable, ICompositeExecutable
    {
        public string Name => "Repeat";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.Repeat, "Repeat", isComposite: true);
        public ISimpleExecutable Body { get; set; }
        public ISimpleExecutable Child { get => Body; set => Body = value; }
        public int Count { get; set; } = 1;
        public bool StopOnFailure { get; set; } = true;
        public int ChildCount => Body != null ? 1 : 0;

        public ISimpleExecutable GetChild(int index) => index == 0 ? Body : null;

        public ExecutionResult Execute(ActionContext ctx)
        {
            if (Body == null) return ExecutionResult.Skipped("No child to repeat");

            var executed = 0;
            var iterations = Math.Max(0, Count);
            for (int i = 0; i < iterations; i++)
            {
                ExecutionResult result;
                try { result = Body.Execute(ctx); }
                catch (Exception ex) { return ExecutionResult.Failed($"Repeat[{i}]: {ex.Message}"); }

                if (result.IsInterrupted) return result;
                if (result.IsSuccess) executed += result.ExecutedCount;
                if (result.IsFailed && StopOnFailure) return result;
            }

            return ExecutionResult.Success(executed);
        }
    }

    [ExecutableTypeId(TypeIdRegistry.Executable.Until, "Until", isComposite: true)]
    public sealed class UntilExecutable : ISimpleExecutable, ICompositeExecutable
    {
        public string Name => "Until";
        public ExecutableMetadata Metadata => new(TypeIdRegistry.Executable.Until, "Until", isComposite: true);
        public ISimpleExecutable Body { get; set; }
        public ISimpleExecutable Child { get => Body; set => Body = value; }
        public int MaxIterations { get; set; } = 10;
        public bool UntilSuccess { get; set; } = true;
        public int ChildCount => Body != null ? 1 : 0;

        public ISimpleExecutable GetChild(int index) => index == 0 ? Body : null;

        public ExecutionResult Execute(ActionContext ctx)
        {
            if (Body == null) return ExecutionResult.Skipped("No child");

            var iterations = Math.Max(0, MaxIterations);
            for (int i = 0; i < iterations; i++)
            {
                ExecutionResult result;
                try { result = Body.Execute(ctx); }
                catch (Exception ex) { return ExecutionResult.Failed($"Until[{i}]: {ex.Message}"); }

                if (result.IsInterrupted) return result;
                if (UntilSuccess && result.IsSuccess) return ExecutionResult.Success(i + 1);
                if (!UntilSuccess && result.IsFailed) return ExecutionResult.Success(i + 1);
            }

            return ExecutionResult.Skipped($"Until reached max iterations {MaxIterations}");
        }
    }
}
