using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 网络事件发送器实现
    /// 提供网络事件发送的默认实现
    /// </summary>
    public sealed class NetworkEventSender : INetworkEventSender
    {
        private readonly List<INetworkEventSink> _sinks = new List<INetworkEventSink>();
        private readonly object _lock = new object();
        private bool _isDisposed;

        /// <summary>
        /// 添加网络事件接收器
        /// </summary>
        public void AddSink(INetworkEventSink sink)
        {
            if (sink == null) return;

            lock (_lock)
            {
                if (!_sinks.Contains(sink))
                {
                    _sinks.Add(sink);
                }
            }
        }

        /// <summary>
        /// 移除网络事件接收器
        /// </summary>
        public void RemoveSink(INetworkEventSink sink)
        {
            if (sink == null) return;

            lock (_lock)
            {
                _sinks.Remove(sink);
            }
        }

        #region INetworkEventSender

        /// <summary>
        /// 发送帧快照到网络
        /// </summary>
        public void SendSnapshot(int frameIndex, byte[] snapshotData)
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                foreach (var sink in _sinks)
                {
                    try
                    {
                        sink.OnSendSnapshot(frameIndex, snapshotData);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NetworkEventSender] SendSnapshot error: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// 发送触发器事件到网络
        /// </summary>
        public void SendTriggerEvent(int frameIndex, int eventType, int sourceId, int targetId)
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                foreach (var sink in _sinks)
                {
                    try
                    {
                        sink.OnSendTriggerEvent(frameIndex, eventType, sourceId, targetId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NetworkEventSender] SendTriggerEvent error: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// 发送玩家输入
        /// </summary>
        public void SendPlayerInput(int playerId, byte[] inputData)
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                foreach (var sink in _sinks)
                {
                    try
                    {
                        sink.OnSendPlayerInput(playerId, inputData);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NetworkEventSender] SendPlayerInput error: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// 广播聊天消息
        /// </summary>
        public void BroadcastChatMessage(int senderId, string message)
        {
            if (_isDisposed) return;
            if (string.IsNullOrEmpty(message)) return;

            lock (_lock)
            {
                foreach (var sink in _sinks)
                {
                    try
                    {
                        sink.OnSendChatMessage(senderId, message);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NetworkEventSender] BroadcastChatMessage error: {ex}");
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            lock (_lock)
            {
                _sinks.Clear();
            }
        }
    }

    /// <summary>
    /// 网络事件接收器接口
    /// 定义接收发送事件通知的契约
    /// </summary>
    public interface INetworkEventSink
    {
        /// <summary>
        /// 发送快照回调
        /// </summary>
        void OnSendSnapshot(int frameIndex, byte[] snapshotData);

        /// <summary>
        /// 发送触发器事件回调
        /// </summary>
        void OnSendTriggerEvent(int frameIndex, int eventType, int sourceId, int targetId);

        /// <summary>
        /// 发送玩家输入回调
        /// </summary>
        void OnSendPlayerInput(int playerId, byte[] inputData);

        /// <summary>
        /// 发送聊天消息回调
        /// </summary>
        void OnSendChatMessage(int senderId, string message);
    }

    /// <summary>
    /// 空网络事件发送器
    /// 用于不需要网络功能的场景（如本地录制）
    /// </summary>
    public sealed class NullNetworkEventSender : INetworkEventSender
    {
        public static NullNetworkEventSender Instance { get; } = new NullNetworkEventSender();

        private NullNetworkEventSender() { }

        public void SendSnapshot(int frameIndex, byte[] snapshotData) { }
        public void SendTriggerEvent(int frameIndex, int eventType, int sourceId, int targetId) { }
        public void SendPlayerInput(int playerId, byte[] inputData) { }
        public void BroadcastChatMessage(int senderId, string message) { }
    }
}
