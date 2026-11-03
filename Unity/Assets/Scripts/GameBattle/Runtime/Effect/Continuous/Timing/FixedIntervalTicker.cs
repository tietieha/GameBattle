using System;

namespace AbilityKit.Continuous
{
    public interface IFixedIntervalTickPort
    {
        long RemainingRaw { get; set; }
        bool CanExecute { get; }
        void OnInterval();
    }

    public static class FixedIntervalTicker
    {
        public const int DefaultExecutionBudget = 32;

        public static int Tick<TPort>(ref TPort port, long deltaRaw, long intervalRaw,
            int maxExecutions = DefaultExecutionBudget) where TPort : IFixedIntervalTickPort
        {
            if (deltaRaw <= 0 || intervalRaw <= 0 || maxExecutions <= 0 || !port.CanExecute) return 0;
            var remaining = checked(port.RemainingRaw - deltaRaw);
            port.RemainingRaw = remaining;
            var count = 0;
            while (remaining <= 0 && count < maxExecutions && port.CanExecute)
            {
                remaining = checked(remaining + intervalRaw);
                port.RemainingRaw = remaining;
                count++;
                port.OnInterval();
            }
            return count;
        }

        public static long NormalizeRemainder(long remainingRaw, long intervalRaw)
        {
            if (intervalRaw <= 0) throw new ArgumentOutOfRangeException(nameof(intervalRaw));
            if (remainingRaw > 0 && remainingRaw <= intervalRaw) return remainingRaw;
            var remainder = remainingRaw % intervalRaw;
            return remainder <= 0 ? remainder + intervalRaw : remainder;
        }
    }
}
