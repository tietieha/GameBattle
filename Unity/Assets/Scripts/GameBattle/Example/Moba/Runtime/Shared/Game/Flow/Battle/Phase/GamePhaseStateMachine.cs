using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 游戏阶段类型枚举
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 加载中
        /// </summary>
        Loading = 1,

        /// <summary>
        /// 匹配中
        /// </summary>
        Matching = 2,

        /// <summary>
        /// 等待开始
        /// </summary>
        WaitingForStart = 3,

        /// <summary>
        /// 加载战斗资源
        /// </summary>
        BattleLoading = 4,

        /// <summary>
        /// 战斗准备阶段
        /// </summary>
        BattlePreparing = 5,

        /// <summary>
        /// 战斗进行中
        /// </summary>
        BattleInProgress = 6,

        /// <summary>
        /// 战斗结束
        /// </summary>
        BattleEnd = 7,

        /// <summary>
        /// 结果展示
        /// </summary>
        Result = 8,

        /// <summary>
        /// 离开
        /// </summary>
        Leaving = 9,
    }

    /// <summary>
    /// 游戏阶段转换事件参数
    /// </summary>
    public readonly struct PhaseTransitionArgs
    {
        /// <summary>
        /// 源阶段
        /// </summary>
        public GamePhase FromPhase { get; }

        /// <summary>
        /// 目标阶段
        /// </summary>
        public GamePhase ToPhase { get; }

        /// <summary>
        /// 用户数据
        /// </summary>
        public object UserData { get; }

        public PhaseTransitionArgs(GamePhase fromPhase, GamePhase toPhase, object userData = null)
        {
            FromPhase = fromPhase;
            ToPhase = toPhase;
            UserData = userData;
        }
    }

    /// <summary>
    /// 游戏阶段状态机
    /// 管理游戏阶段的状态转换
    /// </summary>
    public sealed class GamePhaseStateMachine
    {
        private readonly Dictionary<GamePhase, List<PhaseTransitionRule>> _transitionRules = new Dictionary<GamePhase, List<PhaseTransitionRule>>();
        private readonly Dictionary<GamePhase, IGamePhaseHost> _phaseHosts = new Dictionary<GamePhase, IGamePhaseHost>();

        private GamePhase _currentPhase = GamePhase.Unknown;
        private GamePhase _previousPhase = GamePhase.Unknown;
        private bool _isDisposed;

        /// <summary>
        /// 当前阶段
        /// </summary>
        public GamePhase CurrentPhase => _currentPhase;

        /// <summary>
        /// 上一阶段
        /// </summary>
        public GamePhase PreviousPhase => _previousPhase;

        /// <summary>
        /// 阶段转换事件
        /// </summary>
        public event Action<GamePhase, GamePhase> OnPhaseChanged;

        /// <summary>
        /// 阶段进入事件
        /// </summary>
        public event Action<GamePhase> OnPhaseEntered;

        /// <summary>
        /// 阶段退出事件
        /// </summary>
        public event Action<GamePhase> OnPhaseExited;

        public GamePhaseStateMachine()
        {
            RegisterDefaultTransitionRules();
        }

        /// <summary>
        /// 注册阶段转换规则
        /// </summary>
        public void RegisterTransitionRule(GamePhase from, GamePhase to, Func<bool> condition = null)
        {
            if (!_transitionRules.TryGetValue(from, out var rules))
            {
                rules = new List<PhaseTransitionRule>();
                _transitionRules[from] = rules;
            }

            rules.Add(new PhaseTransitionRule(to, condition));
        }

        /// <summary>
        /// 注册阶段处理器
        /// </summary>
        public void RegisterPhaseHost(GamePhase phase, IGamePhaseHost host)
        {
            _phaseHosts[phase] = host;
        }

        /// <summary>
        /// 尝试转换到指定阶段
        /// </summary>
        public bool TryTransitionTo(GamePhase targetPhase, object userData = null)
        {
            if (_isDisposed) return false;
            if (_currentPhase == targetPhase) return true;

            if (!CanTransition(_currentPhase, targetPhase))
            {
                return false;
            }

            var fromPhase = _currentPhase;
            var args = new PhaseTransitionArgs(fromPhase, targetPhase, userData);

            OnPhaseExited?.Invoke(fromPhase);

            if (_phaseHosts.TryGetValue(_currentPhase, out var exitingHost))
            {
                var ctx = new GamePhaseContext(null, null, userData);
                exitingHost.OnDetach(in ctx);
            }

            _previousPhase = _currentPhase;
            _currentPhase = targetPhase;

            OnPhaseEntered?.Invoke(targetPhase);
            OnPhaseChanged?.Invoke(fromPhase, targetPhase);

            if (_phaseHosts.TryGetValue(_currentPhase, out var enteringHost))
            {
                var ctx = new GamePhaseContext(null, null, userData);
                enteringHost.OnAttach(in ctx);
            }

            return true;
        }

        /// <summary>
        /// 是否可以转换到指定阶段
        /// </summary>
        public bool CanTransition(GamePhase from, GamePhase to)
        {
            if (!_transitionRules.TryGetValue(from, out var rules))
            {
                return false;
            }

            foreach (var rule in rules)
            {
                if (rule.TargetPhase == to)
                {
                    if (rule.Condition == null || rule.Condition())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_isDisposed) return;

            if (_phaseHosts.TryGetValue(_currentPhase, out var host))
            {
                var ctx = new GamePhaseContext(null, null, null);
                host.Tick(in ctx, deltaTime);
            }
        }

        /// <summary>
        /// 重置状态机
        /// </summary>
        public void Reset()
        {
            if (_currentPhase != GamePhase.Unknown)
            {
                TryTransitionTo(GamePhase.Unknown);
            }

            _previousPhase = GamePhase.Unknown;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            foreach (var kvp in _phaseHosts)
            {
                if (kvp.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _transitionRules.Clear();
            _phaseHosts.Clear();
        }

        private void RegisterDefaultTransitionRules()
        {
            RegisterTransitionRule(GamePhase.Unknown, GamePhase.Loading);
            RegisterTransitionRule(GamePhase.Loading, GamePhase.Matching);
            RegisterTransitionRule(GamePhase.Matching, GamePhase.WaitingForStart);
            RegisterTransitionRule(GamePhase.Matching, GamePhase.Loading);
            RegisterTransitionRule(GamePhase.WaitingForStart, GamePhase.BattleLoading);
            RegisterTransitionRule(GamePhase.WaitingForStart, GamePhase.Leaving);
            RegisterTransitionRule(GamePhase.BattleLoading, GamePhase.BattlePreparing);
            RegisterTransitionRule(GamePhase.BattlePreparing, GamePhase.BattleInProgress);
            RegisterTransitionRule(GamePhase.BattlePreparing, GamePhase.Leaving);
            RegisterTransitionRule(GamePhase.BattleInProgress, GamePhase.BattleEnd);
            RegisterTransitionRule(GamePhase.BattleInProgress, GamePhase.Leaving);
            RegisterTransitionRule(GamePhase.BattleEnd, GamePhase.Result);
            RegisterTransitionRule(GamePhase.Result, GamePhase.Loading);
            RegisterTransitionRule(GamePhase.Result, GamePhase.Leaving);
        }
    }

    /// <summary>
    /// 阶段转换规则
    /// </summary>
    public readonly struct PhaseTransitionRule
    {
        public GamePhase TargetPhase { get; }
        public Func<bool> Condition { get; }

        public PhaseTransitionRule(GamePhase targetPhase, Func<bool> condition)
        {
            TargetPhase = targetPhase;
            Condition = condition;
        }
    }

    /// <summary>
    /// 游戏阶段处理器基类
    /// 提供阶段处理的默认实现
    /// </summary>
    public abstract class GamePhaseHandler : IGamePhaseHost
    {
        public abstract GamePhase Phase { get; }

        public virtual void OnAttach(in GamePhaseContext ctx) { }
        public virtual void OnDetach(in GamePhaseContext ctx) { }
        public virtual void Tick(in GamePhaseContext ctx, float deltaTime) { }
    }
}
