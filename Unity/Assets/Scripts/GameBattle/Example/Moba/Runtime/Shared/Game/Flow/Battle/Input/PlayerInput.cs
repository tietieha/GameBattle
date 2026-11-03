using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 玩家输入接口
    /// 定义玩家输入的抽象表示
    /// </summary>
    public readonly struct PlayerInputData
    {
        /// <summary>
        /// 玩家 ID
        /// </summary>
        public int PlayerId { get; }

        /// <summary>
        /// 帧索引
        /// </summary>
        public int FrameIndex { get; }

        /// <summary>
        /// 输入操作码
        /// </summary>
        public int OpCode { get; }

        /// <summary>
        /// 目标位置 X
        /// </summary>
        public float TargetX { get; }

        /// <summary>
        /// 目标位置 Y
        /// </summary>
        public float TargetY { get; }

        /// <summary>
        /// 目标位置 Z
        /// </summary>
        public float TargetZ { get; }

        /// <summary>
        /// 原始数据
        /// </summary>
        public byte[] RawData { get; }

        public PlayerInputData(
            int playerId,
            int frameIndex,
            int opCode,
            float targetX = 0,
            float targetY = 0,
            float targetZ = 0,
            byte[] rawData = null)
        {
            PlayerId = playerId;
            FrameIndex = frameIndex;
            OpCode = opCode;
            TargetX = targetX;
            TargetY = targetY;
            TargetZ = targetZ;
            RawData = rawData;
        }
    }

    /// <summary>
    /// 输入操作码枚举
    /// 定义标准的玩家输入操作
    /// </summary>
    public static class InputOpCode
    {
        /// <summary>
        /// 空输入
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// 移动
        /// </summary>
        public const int Move = 1;

        /// <summary>
        /// 停止移动
        /// </summary>
        public const int Stop = 2;

        /// <summary>
        /// 释放技能
        /// </summary>
        public const int Skill = 3;

        /// <summary>
        /// 普通攻击
        /// </summary>
        public const int Attack = 4;

        /// <summary>
        /// 停止技能
        /// </summary>
        public const int StopSkill = 5;

        /// <summary>
        /// 购买物品
        /// </summary>
        public const int BuyItem = 6;

        /// <summary>
        /// 出售物品
        /// </summary>
        public const int SellItem = 7;

        /// <summary>
        /// 使用物品
        /// </summary>
        public const int UseItem = 8;

        /// <summary>
        /// 原地待命
        /// </summary>
        public const int Hold = 9;

        /// <summary>
        /// 跟随
        /// </summary>
        public const int Follow = 10;

        /// <summary>
        /// 撤退
        /// </summary>
        public const int Retreat = 11;
    }

    /// <summary>
    /// 玩家输入源接口
    /// 定义获取玩家输入的契约
    /// </summary>
    public interface IPlayerInputSource
    {
        /// <summary>
        /// 获取当前帧的玩家输入
        /// </summary>
        /// <param name="playerId">玩家 ID</param>
        /// <param name="frameIndex">帧索引</param>
        /// <returns>玩家输入数据</returns>
        PlayerInputData GetInput(int playerId, int frameIndex);

        /// <summary>
        /// 是否有待处理的输入
        /// </summary>
        bool HasPendingInput(int playerId);

        /// <summary>
        /// 获取待处理的输入数量
        /// </summary>
        int GetPendingInputCount(int playerId);
    }

    /// <summary>
    /// 玩家输入提交器接口
    /// 定义提交玩家输入的契约
    /// </summary>
    public interface IPlayerInputSubmitter
    {
        /// <summary>
        /// 提交玩家输入
        /// </summary>
        /// <param name="playerId">玩家 ID</param>
        /// <param name="input">输入数据</param>
        void SubmitInput(int playerId, in PlayerInputData input);

        /// <summary>
        /// 批量提交输入
        /// </summary>
        void SubmitInputBatch(int playerId, IReadOnlyList<PlayerInputData> inputs);
    }

    /// <summary>
    /// 输入缓冲接口
    /// 定义输入缓冲管理
    /// </summary>
    public interface IInputBuffer
    {
        /// <summary>
        /// 添加输入到缓冲
        /// </summary>
        void Enqueue(int playerId, in PlayerInputData input);

        /// <summary>
        /// 获取指定帧的输入
        /// </summary>
        bool TryDequeue(int playerId, int frameIndex, out PlayerInputData input);

        /// <summary>
        /// 获取所有待处理输入
        /// </summary>
        IReadOnlyList<PlayerInputData> GetPendingInputs(int playerId);

        /// <summary>
        /// 清空指定玩家的缓冲
        /// </summary>
        void Clear(int playerId);

        /// <summary>
        /// 清空所有缓冲
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 获取最早一帧的输入帧索引
        /// </summary>
        int GetEarliestFrame(int playerId);

        /// <summary>
        /// 获取最新一帧的输入帧索引
        /// </summary>
        int GetLatestFrame(int playerId);
    }
}
