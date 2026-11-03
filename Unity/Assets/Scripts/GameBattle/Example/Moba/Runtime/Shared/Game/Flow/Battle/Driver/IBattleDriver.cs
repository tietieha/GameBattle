namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 平台无关的战斗宿主驱动契约。
    /// 外部环境只负责生命周期、固定帧驱动和表现事件接入；战斗规则、输入处理和状态输出由 moba runtime 端口承载。
    /// </summary>
    public interface IBattleDriver
    {
        // ============== 属性 ==============

        /// <summary>
        /// 当前帧索引
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 总逻辑时间（秒）
        /// </summary>
        double LogicTimeSeconds { get; }

        /// <summary>
        /// 帧率（每秒帧数）
        /// </summary>
        int TickRate { get; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 战斗视图事件接收器
        /// </summary>
        IBattleViewEventSink ViewEventSink { get; set; }

        /// <summary>
        /// 启动计划
        /// </summary>
        BattleStartPlan Plan { get; }

        // ============== 生命周期 ==============

        /// <summary>
        /// 初始化战斗驱动
        /// </summary>
        /// <param name="plan">启动计划</param>
        /// <param name="viewSink">视图事件接收器</param>
        void Initialize(in BattleStartPlan plan, IBattleViewEventSink viewSink);

        /// <summary>
        /// 启动战斗
        /// </summary>
        void Start();

        /// <summary>
        /// 停止战斗
        /// </summary>
        void Stop();

        /// <summary>
        /// 销毁战斗驱动
        /// </summary>
        void Destroy();

        // ============== 帧循环 ==============

        /// <summary>
        /// 执行一帧
        /// 由外部宿主按固定频率调用
        /// </summary>
        /// <param name="deltaTime">上一帧到当前的时间（秒）</param>
        void Tick(float deltaTime);

    }
}
