using System;
using System.Collections.Generic;
using AbilityKit.Triggering.Runtime.RuleScheduler;

namespace AbilityKit.Triggering.Samples.Scheduler
{
    /// <summary>
    /// Buff 调度数据示例。
    /// 展示如何为 Buff 系统定义项目侧调度参数。
    /// </summary>
    [Serializable]
    public class BuffScheduleData
    {
        /// <summary>Buff 唯一ID</summary>
        public int BuffId;

        /// <summary>Buff 名称</summary>
        public string BuffName;

        /// <summary>Buff 类型</summary>
        public EBuffType BuffType;

        /// <summary>正式规则调度配置，运行时直接迁移到 RuleSchedulePlan。</summary>
        public RuleSchedulePlan SchedulePlan;

        /// <summary>效果数值（如伤害值、治疗值）</summary>
        public float EffectValue;

        /// <summary>效果类型</summary>
        public EEffectType EffectType;
    }

    /// <summary>
    /// Buff 类型枚举。
    /// </summary>
    public enum EBuffType
    {
        None,
        DamageOverTime,
        HealOverTime,
        SpeedBoost,
        Slow,
        Stun,
        Silence,
        Shield,
        Invincible,
        Reflect,
    }

    /// <summary>
    /// 效果类型枚举。
    /// </summary>
    public enum EEffectType
    {
        None,
        PhysicalDamage,
        MagicDamage,
        TrueDamage,
        Heal,
        Shield,
        Buff,
        Debuff,
    }

    /// <summary>
    /// Buff 调度预设配置。
    /// 提供常见 Buff 调度预设，并直接返回正式 RuleSchedulePlan。
    /// </summary>
    public static class BuffSchedulePresets
    {
        /// <summary>
        /// 持续伤害 (DoT) 预设：每秒造成一次伤害，持续 N 次。
        /// </summary>
        public static RuleSchedulePlan DamageOverTime(float intervalMs = 1000f, int maxTicks = 5)
        {
            return RuleSchedulePlan.Every(intervalMs, maxTicks);
        }

        /// <summary>
        /// 持续治疗 (HoT) 预设：每秒治疗一次，持续 N 次。
        /// </summary>
        public static RuleSchedulePlan HealOverTime(float intervalMs = 1000f, int maxTicks = 5)
        {
            return RuleSchedulePlan.Every(intervalMs, maxTicks);
        }

        /// <summary>
        /// 延迟伤害预设：延迟 N 毫秒后造成一次伤害。
        /// </summary>
        public static RuleSchedulePlan DelayedDamage(float delayMs)
        {
            return RuleSchedulePlan.After(delayMs);
        }

        /// <summary>
        /// 护盾预设：持续 N 毫秒的护盾效果。
        /// </summary>
        public static RuleSchedulePlan Shield(float durationMs)
        {
            return RuleSchedulePlan.WhileActive(0, 0).WithReplacement(true);
        }

        /// <summary>
        /// 无敌预设：持续 N 毫秒的无敌效果。
        /// </summary>
        public static RuleSchedulePlan Invincible(float durationMs)
        {
            return RuleSchedulePlan.WhileActive(0, 0).WithReplacement(true);
        }
    }

    /// <summary>
    /// Buff 调度器管理器示例。
    /// 展示如何将 Buff 调度参数迁移到正式 RuleScheduler 管理。
    /// </summary>
    public sealed class BuffSchedulerManager
    {
        private readonly RuleSchedulerRegistry _registry;
        private readonly Dictionary<int, BuffScheduleData> _buffs = new Dictionary<int, BuffScheduleData>();
        private readonly Dictionary<int, RuleScheduleHandle> _handles = new Dictionary<int, RuleScheduleHandle>();

        public BuffSchedulerManager() : this(new RuleSchedulerRegistry())
        {
        }

        public BuffSchedulerManager(RuleSchedulerRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// 注册一个 Buff。
        /// </summary>
        public RuleScheduleHandle RegisterBuff(BuffScheduleData data, Action<object> onTick)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (onTick == null) throw new ArgumentNullException(nameof(onTick));

            _buffs[data.BuffId] = data;

            var plan = data.SchedulePlan.WithReplacement(true);
            if (plan.Mode == ERuleScheduleMode.WhileActive && plan.IntervalMs <= 0f && plan.DelayMs <= 0f)
            {
                plan = RuleSchedulePlan.WhileActive(1000f, 0f).WithReplacement(true);
            }

            var handle = _registry.Schedule(in plan, new DelegateRuleScheduleEffect(_ => onTick(data)));
            _handles[data.BuffId] = handle;
            return handle;
        }

        /// <summary>
        /// 移除一个 Buff。
        /// </summary>
        public bool RemoveBuff(int buffId)
        {
            if (!_handles.TryGetValue(buffId, out var handle))
            {
                return false;
            }

            var removed = _registry.Cancel(handle);
            if (removed)
            {
                _handles.Remove(buffId);
                _buffs.Remove(buffId);
            }

            return removed;
        }

        /// <summary>
        /// 获取 Buff 数据。
        /// </summary>
        public bool TryGetBuff(int buffId, out BuffScheduleData data)
        {
            return _buffs.TryGetValue(buffId, out data);
        }

        /// <summary>
        /// 更新所有 Buff 调度器。
        /// </summary>
        public void Update(float deltaTimeMs)
        {
            _registry.Update(deltaTimeMs);
        }

        /// <summary>
        /// 暂停所有 Buff。
        /// </summary>
        public void PauseAll()
        {
            foreach (var handle in _handles.Values)
            {
                _registry.Pause(handle);
            }
        }

        /// <summary>
        /// 恢复所有 Buff。
        /// </summary>
        public void ResumeAll()
        {
            foreach (var handle in _handles.Values)
            {
                _registry.Resume(handle);
            }
        }

        /// <summary>
        /// 清除所有 Buff。
        /// </summary>
        public void Clear()
        {
            _registry.Clear();
            _handles.Clear();
            _buffs.Clear();
        }

        private static string FormatBuffGroupId(int buffId)
        {
            return "buff:" + buffId;
        }
    }
}
