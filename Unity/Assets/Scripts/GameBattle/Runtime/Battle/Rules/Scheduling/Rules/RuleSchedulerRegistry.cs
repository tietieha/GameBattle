using System;
using System.Collections.Generic;

namespace AbilityKit.Triggering.Runtime.RuleScheduler
{
    /// <summary>
    /// 规则调度驱动注册表。
    /// </summary>
    public sealed class RuleSchedulerRegistry
    {
        private readonly Dictionary<string, IRuleSchedulerDriver> _drivers = new Dictionary<string, IRuleSchedulerDriver>(StringComparer.Ordinal);

        public string DefaultDriverId { get; private set; }
        public int DriverCount => _drivers.Count;

        public RuleSchedulerRegistry(IRuleSchedulerDriver defaultDriver = null)
        {
            RegisterDriver(defaultDriver ?? new DefaultRuleSchedulerDriver(), setAsDefault: true);
        }

        public void RegisterDriver(IRuleSchedulerDriver driver, bool setAsDefault = false)
        {
            if (driver == null) throw new ArgumentNullException(nameof(driver));
            if (string.IsNullOrEmpty(driver.DriverId)) throw new ArgumentException("Rule scheduler driver id cannot be empty.", nameof(driver));

            _drivers[driver.DriverId] = driver;
            if (setAsDefault || string.IsNullOrEmpty(DefaultDriverId))
            {
                DefaultDriverId = driver.DriverId;
            }
        }

        public bool TryGetDriver(string driverId, out IRuleSchedulerDriver driver)
        {
            return _drivers.TryGetValue(NormalizeDriverId(driverId), out driver);
        }

        public IRuleSchedulerDriver GetDriver(string driverId = null)
        {
            if (TryGetDriver(driverId, out var driver)) return driver;
            throw new InvalidOperationException($"Rule scheduler driver not registered: {NormalizeDriverId(driverId)}");
        }

        public RuleScheduleHandle Schedule(in RuleSchedulePlan plan, IRuleScheduleEffect effect, string driverId = null)
        {
            return GetDriver(driverId).Schedule(in plan, effect);
        }

        public bool Pause(RuleScheduleHandle handle) => handle.IsValid && TryGetDriver(handle.DriverId, out var driver) && driver.Pause(handle);
        public bool Resume(RuleScheduleHandle handle) => handle.IsValid && TryGetDriver(handle.DriverId, out var driver) && driver.Resume(handle);
        public bool Interrupt(RuleScheduleHandle handle, string reason = null) => handle.IsValid && TryGetDriver(handle.DriverId, out var driver) && driver.Interrupt(handle, reason);
        public bool Cancel(RuleScheduleHandle handle) => handle.IsValid && TryGetDriver(handle.DriverId, out var driver) && driver.Cancel(handle);

        public void Update(float deltaTimeMs, object userContext = null)
        {
            foreach (var driver in _drivers.Values)
            {
                driver.Update(deltaTimeMs, userContext);
            }
        }

        public void Clear()
        {
            foreach (var driver in _drivers.Values)
            {
                driver.Clear();
            }
        }

        private string NormalizeDriverId(string driverId)
        {
            return string.IsNullOrEmpty(driverId) ? DefaultDriverId : driverId;
        }
    }
}
