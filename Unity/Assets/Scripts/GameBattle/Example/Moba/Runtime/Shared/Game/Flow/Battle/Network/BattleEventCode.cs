using System;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 战斗网络事件码
    /// 定义标准的战斗相关网络事件
    /// </summary>
    public static class BattleEventCode
    {
        // ============== 帧同步事件 ==============

        /// <summary>
        /// 帧快照
        /// </summary>
        public const int FrameSnapshot = 1000;

        /// <summary>
        /// 帧开始
        /// </summary>
        public const int FrameStart = 1001;

        /// <summary>
        /// 帧结束
        /// </summary>
        public const int FrameEnd = 1002;

        // ============== 玩家事件 ==============

        /// <summary>
        /// 玩家加入
        /// </summary>
        public const int PlayerJoin = 2001;

        /// <summary>
        /// 玩家离开
        /// </summary>
        public const int PlayerLeave = 2002;

        /// <summary>
        /// 玩家输入
        /// </summary>
        public const int PlayerInput = 2003;

        /// <summary>
        /// 玩家断线
        /// </summary>
        public const int PlayerDisconnect = 2004;

        /// <summary>
        /// 玩家重连
        /// </summary>
        public const int PlayerReconnect = 2005;

        // ============== 战斗事件 ==============

        /// <summary>
        /// 技能释放
        /// </summary>
        public const int SkillCast = 3001;

        /// <summary>
        /// 技能命中
        /// </summary>
        public const int SkillHit = 3002;

        /// <summary>
        /// 伤害结算
        /// </summary>
        public const int DamageDealt = 3003;

        /// <summary>
        /// 治疗
        /// </summary>
        public const int Heal = 3004;

        /// <summary>
        /// 死亡
        /// </summary>
        public const int UnitDeath = 3005;

        /// <summary>
        /// 复活
        /// </summary>
        public const int UnitRevive = 3006;

        /// <summary>
        /// 移动
        /// </summary>
        public const int UnitMove = 3007;

        /// <summary>
        /// 状态变化
        /// </summary>
        public const int StateChange = 3008;

        // ============== 游戏状态事件 ==============

        /// <summary>
        /// 游戏开始
        /// </summary>
        public const int GameStart = 4001;

        /// <summary>
        /// 游戏结束
        /// </summary>
        public const int GameEnd = 4002;

        /// <summary>
        /// 游戏暂停
        /// </summary>
        public const int GamePause = 4003;

        /// <summary>
        /// 游戏恢复
        /// </summary>
        public const int GameResume = 4004;

        /// <summary>
        /// 游戏重开
        /// </summary>
        public const int GameRestart = 4005;

        // ============== 聊天事件 ==============

        /// <summary>
        /// 聊天消息
        /// </summary>
        public const int ChatMessage = 5001;

        /// <summary>
        /// 系统消息
        /// </summary>
        public const int SystemMessage = 5002;

        /// <summary>
        /// 战绩消息
        /// </summary>
        public const int KillFeedMessage = 5003;

        // ============== 回放事件 ==============

        /// <summary>
        /// 回放开始
        /// </summary>
        public const int ReplayStart = 6001;

        /// <summary>
        /// 回放结束
        /// </summary>
        public const int ReplayEnd = 6002;

        /// <summary>
        /// 回放数据请求
        /// </summary>
        public const int ReplayDataRequest = 6003;

        /// <summary>
        /// 回放数据响应
        /// </summary>
        public const int ReplayDataResponse = 6004;
    }

    /// <summary>
    /// 事件码扩展
    /// </summary>
    public static class BattleEventCodeExtensions
    {
        /// <summary>
        /// 是否是帧同步相关事件
        /// </summary>
        public static bool IsFrameSyncEvent(this int eventCode)
        {
            return eventCode >= 1000 && eventCode < 2000;
        }

        /// <summary>
        /// 是否是玩家相关事件
        /// </summary>
        public static bool IsPlayerEvent(this int eventCode)
        {
            return eventCode >= 2000 && eventCode < 3000;
        }

        /// <summary>
        /// 是否是战斗相关事件
        /// </summary>
        public static bool IsBattleEvent(this int eventCode)
        {
            return eventCode >= 3000 && eventCode < 4000;
        }

        /// <summary>
        /// 是否是游戏状态事件
        /// </summary>
        public static bool IsGameStateEvent(this int eventCode)
        {
            return eventCode >= 4000 && eventCode < 5000;
        }

        /// <summary>
        /// 是否是聊天事件
        /// </summary>
        public static bool IsChatEvent(this int eventCode)
        {
            return eventCode >= 5000 && eventCode < 6000;
        }

        /// <summary>
        /// 是否是回放事件
        /// </summary>
        public static bool IsReplayEvent(this int eventCode)
        {
            return eventCode >= 6000 && eventCode < 7000;
        }

        /// <summary>
        /// 获取事件分类名称
        /// </summary>
        public static string GetEventCategory(this int eventCode)
        {
            if (eventCode.IsFrameSyncEvent()) return "FrameSync";
            if (eventCode.IsPlayerEvent()) return "Player";
            if (eventCode.IsBattleEvent()) return "Battle";
            if (eventCode.IsGameStateEvent()) return "GameState";
            if (eventCode.IsChatEvent()) return "Chat";
            if (eventCode.IsReplayEvent()) return "Replay";
            return "Unknown";
        }
    }
}
