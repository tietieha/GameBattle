using System;
using AbilityKit.Triggering.Runtime.RuleScheduler;

namespace AbilityKit.Triggering.Samples.Scheduler
{
    /// <summary>
    /// 该示例演示规则级调度注册与驱动。
    /// </summary>
    public static class SchedulerRegistryExample
    {
        public static void Run()
        {
            var registry = new RuleSchedulerRegistry();
            Console.WriteLine("RuleSchedulerRegistry ready.");
        }
    }
}
