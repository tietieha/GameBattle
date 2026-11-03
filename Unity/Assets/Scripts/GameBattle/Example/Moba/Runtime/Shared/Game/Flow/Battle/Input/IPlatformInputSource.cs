using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 平台输入源接口
    /// 定义获取平台原生输入的契约
    /// 
    /// 不同平台实现此接口：
    /// - Unity：采集键盘、鼠标、触屏输入
    /// - Console：采集终端输入事件
    /// </summary>
    public interface IPlatformInputSource
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 更新输入状态（每帧调用）
        /// </summary>
        void Update();

        /// <summary>
        /// 获取当前帧的移动输入
        /// 返回 (x, z) 方向的向量，y 分量忽略
        /// </summary>
        /// <returns>移动输入向量 (x, z)，范围 [-1, 1]</returns>
        (float x, float z) GetMoveInput();

        /// <summary>
        /// 获取当前帧的攻击输入
        /// </summary>
        bool IsAttackPressed();

        /// <summary>
        /// 获取当前帧的技能输入
        /// </summary>
        /// <returns>技能索引（0-3），-1 表示无输入</returns>
        int GetSkillInput();

        /// <summary>
        /// 获取停止技能输入
        /// </summary>
        bool IsStopSkillPressed();

        /// <summary>
        /// 获取停止移动输入
        /// </summary>
        bool IsStopPressed();

        /// <summary>
        /// 获取鼠标/屏幕点击位置
        /// </summary>
        /// <returns>(x, y) 屏幕坐标，(-1, -1) 表示无点击</returns>
        (float x, float y) GetClickPosition();

        /// <summary>
        /// 获取视角旋转输入
        /// </summary>
        /// <returns>旋转角度增量</returns>
        float GetCameraRotationInput();

        /// <summary>
        /// 重置所有输入状态
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// 输入处理器接口
    /// 定义处理输入的契约
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        /// 处理输入，生成输入数据
        /// </summary>
        /// <param name="platformInput">平台输入源</param>
        /// <param name="playerId">玩家 ID</param>
        /// <param name="frameIndex">帧索引</param>
        /// <returns>生成的输入数据</returns>
        PlayerInputData ProcessInput(
            IPlatformInputSource platformInput,
            int playerId,
            int frameIndex);

        /// <summary>
        /// 是否有有效的输入
        /// </summary>
        bool HasValidInput();
    }
}
