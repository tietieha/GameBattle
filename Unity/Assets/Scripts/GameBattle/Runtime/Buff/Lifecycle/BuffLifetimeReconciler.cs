using System.Collections.Generic;

namespace AbilityKit.Buffs
{
    public interface IBuffLifetimePort<TRuntime> where TRuntime : BuffState
    {
        bool ShouldInterrupt(TRuntime runtime);
        void Synchronize(TRuntime runtime);
        bool HasEnded(TRuntime runtime);
        void End(List<TRuntime> list, int index, TRuntime runtime, bool interrupted);
    }

    public static class BuffLifetimeReconciler
    {
        public static void Reconcile<TRuntime, TPort>(List<TRuntime> list, ref TPort port)
            where TRuntime : BuffState where TPort : IBuffLifetimePort<TRuntime>
        {
            if (list == null) return;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var runtime = list[i];
                if (runtime == null)
                {
                    list.RemoveAt(i);
                    BuffStorage<TRuntime>.MarkDirty(list);
                    continue;
                }
                var interrupted = port.ShouldInterrupt(runtime);
                if (interrupted) runtime.Remaining = 0;
                else port.Synchronize(runtime);
                if (interrupted || port.HasEnded(runtime)) port.End(list, i, runtime, interrupted);
            }
        }
    }
}
