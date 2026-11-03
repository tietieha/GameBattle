using AbilityKit.Triggering.Runtime.RuleScheduler;
using UnityEngine;

namespace AbilityKit.Samples
{
    /// <summary>
    /// 规则调度示例 - 使用正式 RuleScheduler 架构。
    ///
    /// 本示例演示：
    /// 1. 创建 RuleSchedulerRegistry（非单例，支持 DI）
    /// 2. 创建规则级时间计划
    /// 3. 注册委托式规则效果
    /// 4. 每帧更新规则调度器
    /// </summary>
    public class ScheduleManagerSample : MonoBehaviour
    {
        [Header("调度设置")]
        [Tooltip("执行间隔（秒）")]
        [SerializeField] private float _interval = 1f;

        [Tooltip("最大执行次数")]
        [SerializeField] private int _maxExecutions = 5;

        private RuleSchedulerRegistry _schedulerRegistry;
        private RuleScheduleHandle _handle;
        private int _executionCount;

        private void Start()
        {
            Debug.Log("========== RuleScheduler 示例开始 ==========");
            Debug.Log($"配置: Interval={_interval}s, MaxExecutions={_maxExecutions}");

            InitializeSchedulerSystem();
            CreateAndStartSchedule();
        }

        private void Update()
        {
            _schedulerRegistry?.Update(Time.deltaTime * 1000f);
        }

        private void OnDestroy()
        {
            if (_schedulerRegistry != null && _handle.IsValid)
            {
                _schedulerRegistry.Cancel(_handle);
            }

            Debug.Log("========== RuleScheduler 示例结束 ==========");
            Debug.Log($"最终执行次数: {_executionCount}");
        }

        /// <summary>
        /// 初始化规则调度系统（非单例，支持 DI）。
        /// </summary>
        private void InitializeSchedulerSystem()
        {
            Debug.Log("[1] 初始化规则调度系统");
            _schedulerRegistry = new RuleSchedulerRegistry();
            Debug.Log("    - RuleSchedulerRegistry 创建完成");
            Debug.Log("");
        }

        /// <summary>
        /// 创建并启动规则调度。
        /// </summary>
        private void CreateAndStartSchedule()
        {
            Debug.Log("[2] 创建并启动规则调度");

            var plan = RuleSchedulePlan.Every(
                intervalMs: _interval * 1000f,
                maxOccurrences: _maxExecutions,
                groupId: "sample:rule-scheduler",
                subjectId: "sample:periodic-log",
                label: "Unity periodic log sample");

            _handle = _schedulerRegistry.Schedule(
                in plan,
                new DelegateRuleScheduleEffect(
                    ctx =>
                    {
                        _executionCount++;
                        Debug.Log($"    ★ [执行 #{_executionCount}] Occurrence={ctx.OccurrenceIndex}");
                    },
                    onCompleted: ctx => Debug.Log("    - 调度完成回调!"),
                    onInterrupted: (ctx, reason) => Debug.Log($"    - 调度中断回调: {reason}")));

            Debug.Log($"    - 调度已启动: {_handle}");
            Debug.Log($"    - 调度计划: interval={_interval * 1000f}ms, maxOccurrences={_maxExecutions}");
            Debug.Log("");
        }
    }
}
