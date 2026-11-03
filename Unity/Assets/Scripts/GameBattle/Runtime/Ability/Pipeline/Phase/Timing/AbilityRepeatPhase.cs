using System;

namespace AbilityKit.Pipeline
{
    /// <summary>
    /// 重复阶段：重复执行子阶段或动作指定次数。
    /// </summary>
    /// <typeparam name="TCtx">管线上下文类型。</typeparam>
    public class AbilityRepeatPhase<TCtx> : AbilityDurationalPhaseBase<TCtx>, IAbilityPipelinePhaseInstanceFactory<TCtx>
        where TCtx : IAbilityPipelineContext
    {
        /// <summary>
        /// 重复次数；小于 0 表示无限重复。
        /// </summary>
        public int RepeatCount { get; set; } = -1;
        
        /// <summary>
        /// 当前重复次数。
        /// </summary>
        public int CurrentRepeatIndex { get; private set; }
        
        /// <summary>
        /// 每次重复的间隔时间。
        /// </summary>
        public float RepeatInterval { get; set; } = 0f;
        
        /// <summary>
        /// 当前间隔计时。
        /// </summary>
        private float _intervalTimer;
        
        /// <summary>
        /// 是否正在等待间隔。
        /// </summary>
        private bool _isWaitingInterval;
        
        /// <summary>
        /// 要重复执行的阶段。
        /// </summary>
        private IAbilityPipelinePhase<TCtx>? _repeatPhase;
        
        /// <summary>
        /// 要重复执行的动作。
        /// </summary>
        private Action<TCtx, int>? _repeatAction;
        
        /// <summary>
        /// 当前正在执行的子阶段。
        /// </summary>
        private IAbilityPipelinePhase<TCtx>? _currentSubPhase;

        /// <summary>
        /// 创建重复阶段。
        /// </summary>
        public AbilityRepeatPhase(int repeatCount = -1) : base("Repeat")
        {
            RepeatCount = repeatCount;
        }

        /// <summary>
        /// 使用指定阶段 ID 创建重复阶段。
        /// </summary>
        public AbilityRepeatPhase(AbilityPipelinePhaseId phaseId, int repeatCount = -1) : base(phaseId)
        {
            RepeatCount = repeatCount;
        }

        /// <summary>
        /// 设置要重复执行的阶段。
        /// </summary>
        public AbilityRepeatPhase<TCtx> SetRepeatPhase(IAbilityPipelinePhase<TCtx> phase)
        {
            _repeatPhase = phase;
            _repeatAction = null;
            return this;
        }

        /// <summary>
        /// 设置要重复执行的动作。
        /// </summary>
        public AbilityRepeatPhase<TCtx> SetRepeatAction(Action<TCtx, int> action)
        {
            _repeatAction = action;
            _repeatPhase = null;
            return this;
        }

        /// <inheritdoc />
        protected override void OnEnter(TCtx context)
        {
            base.OnEnter(context);
            CurrentRepeatIndex = 0;
            _intervalTimer = 0f;
            _isWaitingInterval = false;
            _currentSubPhase = null;
        }

        /// <inheritdoc />
        protected override void OnExecute(TCtx context)
        {
            ExecuteRepeat(context);
        }

        /// <inheritdoc />
        protected override void OnTick(TCtx context, float deltaTime)
        {
            if (_currentSubPhase != null)
            {
                _currentSubPhase.OnUpdate(context, deltaTime);
                if (_currentSubPhase.IsComplete)
                {
                    _currentSubPhase = null;
                    OnRepeatComplete(context);
                }
                return;
            }

            if (_isWaitingInterval)
            {
                _intervalTimer += deltaTime;
                if (_intervalTimer >= RepeatInterval)
                {
                    _isWaitingInterval = false;
                    _intervalTimer = 0f;
                    ExecuteRepeat(context);
                }
            }
        }

        private void ExecuteRepeat(TCtx context)
        {
            if (RepeatCount > 0 && CurrentRepeatIndex >= RepeatCount)
            {
                Complete(context);
                return;
            }

            if (_repeatAction != null)
            {
                try
                {
                    _repeatAction.Invoke(context, CurrentRepeatIndex);
                }
                catch (Exception e)
                {
                    HandleError(context, e);
                    return;
                }
                OnRepeatComplete(context);
            }
            else if (_repeatPhase != null)
            {
                _repeatPhase.Reset();
                _repeatPhase.Execute(context);
                
                if (!_repeatPhase.IsComplete)
                {
                    _currentSubPhase = _repeatPhase;
                }
                else
                {
                    OnRepeatComplete(context);
                }
            }
            else
            {
                Complete(context);
            }
        }

        private void OnRepeatComplete(TCtx context)
        {
            CurrentRepeatIndex++;
            
            if (RepeatCount > 0 && CurrentRepeatIndex >= RepeatCount)
            {
                Complete(context);
                return;
            }

            if (RepeatInterval > 0)
            {
                _isWaitingInterval = true;
                _intervalTimer = 0f;
            }
            else
            {
                ExecuteRepeat(context);
            }
        }

        /// <inheritdoc />
        public IAbilityPipelinePhase<TCtx> CreateRunPhase()
        {
            var phase = new AbilityRepeatPhase<TCtx>(PhaseId, RepeatCount)
            {
                RepeatInterval = RepeatInterval,
                Duration = Duration
            };

            if (_repeatPhase != null)
            {
                phase.SetRepeatPhase(AbilityPipelinePhaseRuntime.CreateRunPhase(_repeatPhase));
            }

            if (_repeatAction != null)
            {
                phase.SetRepeatAction(_repeatAction);
            }

            return phase;
        }

        /// <summary>
        /// 创建重复阶段。
        /// </summary>
        public static AbilityRepeatPhase<TCtx> Create(int repeatCount = -1)
        {
            return new AbilityRepeatPhase<TCtx>(repeatCount);
        }

        /// <summary>
        /// 创建带动作的重复阶段。
        /// </summary>
        public static AbilityRepeatPhase<TCtx> Create(Action<TCtx, int> action, int repeatCount = -1, float interval = 0f)
        {
            var phase = new AbilityRepeatPhase<TCtx>(repeatCount);
            phase.SetRepeatAction(action);
            phase.RepeatInterval = interval;
            return phase;
        }
    }
}
