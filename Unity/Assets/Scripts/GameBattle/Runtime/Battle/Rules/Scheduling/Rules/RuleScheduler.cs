using System;
using System.Collections.Generic;

namespace AbilityKit.Triggering.Runtime.RuleScheduler
{
    /// <summary>
    /// 包内默认规则调度驱动。
    /// </summary>
    public sealed class DefaultRuleSchedulerDriver : IRuleSchedulerDriver
    {
        public const string DefaultId = "abilitykit.default";

        private readonly List<RuleScheduleEntry> _entries = new List<RuleScheduleEntry>();
        private readonly Dictionary<int, RuleScheduleEntry> _entriesById = new Dictionary<int, RuleScheduleEntry>();
        private int _nextInstanceId = 1;

        public string DriverId { get; }

        public DefaultRuleSchedulerDriver(string driverId = DefaultId)
        {
            DriverId = string.IsNullOrEmpty(driverId) ? DefaultId : driverId;
        }

        public RuleScheduleHandle Schedule(in RuleSchedulePlan plan, IRuleScheduleEffect effect)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));

            if (plan.ReplaceExisting)
            {
                CancelMatching(plan.GroupId, plan.SubjectId);
            }

            var handle = new RuleScheduleHandle(DriverId, _nextInstanceId++, 1);
            var state = plan.DelayMs > 0f ? ERuleScheduleState.WaitingDelay : ERuleScheduleState.Registered;
            var entry = new RuleScheduleEntry(handle, plan, effect, state);
            _entries.Add(entry);
            _entriesById[handle.InstanceId] = entry;
            return handle;
        }

        public bool TryGet(RuleScheduleHandle handle, out RuleScheduleSnapshot snapshot)
        {
            if (TryGetEntry(handle, out var entry))
            {
                snapshot = entry.CreateSnapshot();
                return true;
            }

            snapshot = default;
            return false;
        }

        public IReadOnlyList<RuleScheduleSnapshot> FindByGroup(string groupId)
        {
            return Find(entry => string.Equals(entry.Plan.GroupId, groupId, StringComparison.Ordinal));
        }

        public IReadOnlyList<RuleScheduleSnapshot> FindBySubject(string subjectId)
        {
            return Find(entry => string.Equals(entry.Plan.SubjectId, subjectId, StringComparison.Ordinal));
        }

        public bool Pause(RuleScheduleHandle handle)
        {
            if (!TryGetEntry(handle, out var entry) || entry.IsTerminal) return false;
            entry.State = ERuleScheduleState.Paused;
            return true;
        }

        public bool Resume(RuleScheduleHandle handle)
        {
            if (!TryGetEntry(handle, out var entry) || entry.State != ERuleScheduleState.Paused) return false;
            entry.State = entry.Plan.DelayMs > 0f && entry.ElapsedMs < entry.Plan.DelayMs ? ERuleScheduleState.WaitingDelay : ERuleScheduleState.Running;
            return true;
        }

        public bool Interrupt(RuleScheduleHandle handle, string reason = null)
        {
            if (!TryGetEntry(handle, out var entry) || entry.IsTerminal || !entry.Plan.CanBeInterrupted) return false;
            InterruptEntry(entry, reason ?? "Interrupted");
            return true;
        }

        public bool Cancel(RuleScheduleHandle handle)
        {
            if (!TryGetEntry(handle, out var entry) || entry.IsTerminal) return false;
            entry.State = ERuleScheduleState.Cancelled;
            return true;
        }

        public int InterruptGroup(string groupId, string reason = null)
        {
            int count = 0;
            foreach (var entry in _entries)
            {
                if (!entry.IsTerminal && entry.Plan.CanBeInterrupted && string.Equals(entry.Plan.GroupId, groupId, StringComparison.Ordinal))
                {
                    InterruptEntry(entry, reason ?? "Interrupted");
                    count++;
                }
            }
            return count;
        }

        public int CancelGroup(string groupId)
        {
            int count = 0;
            foreach (var entry in _entries)
            {
                if (!entry.IsTerminal && string.Equals(entry.Plan.GroupId, groupId, StringComparison.Ordinal))
                {
                    entry.State = ERuleScheduleState.Cancelled;
                    count++;
                }
            }
            return count;
        }

        public void Update(float deltaTimeMs, object userContext = null)
        {
            var safeDelta = Math.Max(0f, deltaTimeMs);
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry.IsTerminal)
                {
                    RemoveAt(i, entry);
                    continue;
                }

                Tick(entry, safeDelta, userContext);

                if (entry.IsTerminal)
                {
                    RemoveAt(i, entry);
                }
            }
        }

        public void Clear()
        {
            _entries.Clear();
            _entriesById.Clear();
        }

        private void Tick(RuleScheduleEntry entry, float deltaTimeMs, object userContext)
        {
            if (entry.State == ERuleScheduleState.Paused) return;

            var scaledDelta = deltaTimeMs * entry.Plan.Speed;
            entry.ElapsedMs += scaledDelta;

            if (entry.State == ERuleScheduleState.WaitingDelay)
            {
                if (entry.ElapsedMs < entry.Plan.DelayMs) return;
                entry.State = ERuleScheduleState.Running;
                entry.LastExecuteMs = entry.ElapsedMs;
            }

            if (entry.State == ERuleScheduleState.Registered)
            {
                entry.State = ERuleScheduleState.Running;
            }

            switch (entry.Plan.Mode)
            {
                case ERuleScheduleMode.Immediate:
                case ERuleScheduleMode.Delayed:
                    ExecuteOnce(entry, deltaTimeMs, scaledDelta, userContext);
                    Complete(entry, deltaTimeMs, scaledDelta, userContext);
                    break;
                case ERuleScheduleMode.Every:
                case ERuleScheduleMode.WhileActive:
                    ExecuteInterval(entry, deltaTimeMs, scaledDelta, userContext);
                    break;
            }
        }

        private void ExecuteInterval(RuleScheduleEntry entry, float deltaTimeMs, float scaledDeltaMs, object userContext)
        {
            var interval = entry.Plan.IntervalMs;
            if (interval <= 0f)
            {
                ExecuteOnce(entry, deltaTimeMs, scaledDeltaMs, userContext);
            }
            else if (entry.ElapsedMs - entry.LastExecuteMs >= interval)
            {
                ExecuteOnce(entry, deltaTimeMs, scaledDeltaMs, userContext);
                entry.LastExecuteMs = entry.ElapsedMs;
            }

            if (entry.Plan.MaxOccurrences > 0 && entry.OccurrenceCount >= entry.Plan.MaxOccurrences)
            {
                Complete(entry, deltaTimeMs, scaledDeltaMs, userContext);
            }
        }

        private void ExecuteOnce(RuleScheduleEntry entry, float deltaTimeMs, float scaledDeltaMs, object userContext)
        {
            var context = entry.CreateContext(deltaTimeMs, scaledDeltaMs, userContext);
            if (!entry.Effect.CanExecute(in context)) return;

            entry.Effect.Execute(in context);
            entry.OccurrenceCount++;
        }

        private void Complete(RuleScheduleEntry entry, float deltaTimeMs, float scaledDeltaMs, object userContext)
        {
            if (entry.IsTerminal) return;
            entry.State = ERuleScheduleState.Completed;
            var context = entry.CreateContext(deltaTimeMs, scaledDeltaMs, userContext);
            entry.Effect.OnCompleted(in context);
        }

        private void InterruptEntry(RuleScheduleEntry entry, string reason)
        {
            entry.State = ERuleScheduleState.Interrupted;
            entry.InterruptReason = reason;
            var context = entry.CreateContext(0f, 0f, null);
            entry.Effect.OnInterrupted(in context, reason);
        }

        private void CancelMatching(string groupId, string subjectId)
        {
            if (string.IsNullOrEmpty(groupId) && string.IsNullOrEmpty(subjectId)) return;

            foreach (var entry in _entries)
            {
                if (entry.IsTerminal) continue;
                var groupMatches = string.IsNullOrEmpty(groupId) || string.Equals(entry.Plan.GroupId, groupId, StringComparison.Ordinal);
                var subjectMatches = string.IsNullOrEmpty(subjectId) || string.Equals(entry.Plan.SubjectId, subjectId, StringComparison.Ordinal);
                if (groupMatches && subjectMatches)
                {
                    entry.State = ERuleScheduleState.Cancelled;
                }
            }
        }

        private IReadOnlyList<RuleScheduleSnapshot> Find(Predicate<RuleScheduleEntry> predicate)
        {
            var result = new List<RuleScheduleSnapshot>();
            foreach (var entry in _entries)
            {
                if (!entry.IsTerminal && predicate(entry)) result.Add(entry.CreateSnapshot());
            }
            return result;
        }

        private bool TryGetEntry(RuleScheduleHandle handle, out RuleScheduleEntry entry)
        {
            if (!handle.IsValid || !string.Equals(handle.DriverId, DriverId, StringComparison.Ordinal))
            {
                entry = null;
                return false;
            }

            if (_entriesById.TryGetValue(handle.InstanceId, out entry) && entry.Handle.Version == handle.Version)
            {
                return true;
            }

            entry = null;
            return false;
        }

        private void RemoveAt(int index, RuleScheduleEntry entry)
        {
            _entries.RemoveAt(index);
            _entriesById.Remove(entry.Handle.InstanceId);
        }
    }
}
