using System;
using System.Threading.Tasks;
using AbilityKit.Core.Logging;

namespace AbilityKit.Game.Flow
{
    internal static class GatewaySessionFailurePolicy
    {
        public static Exception WrapPreparationFailure(Task task)
        {
            var ex = task?.Exception != null ? task.Exception.GetBaseException() : null;
            return new InvalidOperationException("Gateway room preparation failed.", ex);
        }

        public static bool ShouldNotifyTimeSyncFailure(Exception exception, int consecutiveFailures, int notifyThreshold)
        {
            if (exception == null) return false;
            if (exception is OperationCanceledException) return false;
            if (notifyThreshold <= 0) return true;
            return consecutiveFailures >= notifyThreshold;
        }

        public static void LogPreparationFailure(Exception exception)
        {
            Log.Exception(exception, "[BattleSessionFeature] Gateway room preparation failed");
        }

        public static void LogTimeSyncFailure(Exception exception, int consecutiveFailures)
        {
            Log.Exception(exception, $"[BattleSessionFeature] TimeSync loop error. consecutiveFailures={consecutiveFailures}");
        }
    }
}
