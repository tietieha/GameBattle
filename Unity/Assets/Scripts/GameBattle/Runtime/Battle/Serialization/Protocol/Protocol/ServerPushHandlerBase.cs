using System;
using System.Reflection;

namespace AbilityKit.Protocol
{
    /// <summary>
    /// 标记 Server Push 处理器
    /// 
    /// 使用方式：
    /// <code>
    /// [ServerPushHandler(9002)]
    /// public sealed class MySnapshotHandler : ServerPushHandlerBase&lt;WireSnapshotPush&gt;
    /// {
    ///     protected override void OnPush(WireSnapshotPush payload)
    ///     {
    ///         // 处理推送数据
    ///     }
    /// }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ServerPushHandlerAttribute : Attribute
    {
        /// <summary>
        /// 处理的 OpCode
        /// </summary>
        public uint OpCode { get; }

        /// <summary>
        /// 标记 Server Push 处理器
        /// </summary>
        /// <param name="opCode">处理的 OpCode</param>
        public ServerPushHandlerAttribute(uint opCode)
        {
            OpCode = opCode;
        }
    }

    /// <summary>
    /// Server Push 处理器接口
    /// </summary>
    public interface IServerPushHandler
    {
        /// <summary>
        /// 处理的 OpCode
        /// </summary>
        uint OpCode { get; }

        /// <summary>
        /// 处理推送数据
        /// </summary>
        void Handle(byte[] payload);
    }

    /// <summary>
    /// Server Push 处理器基类（泛型版本）
    /// 
    /// 继承此类并标记 [ServerPushHandler] Attribute 来创建处理器
    /// </summary>
    /// <typeparam name="T">推送数据类型（必须是结构体）</typeparam>
    public abstract class ServerPushHandlerBase<T> : IServerPushHandler where T : struct
    {
        private static readonly uint? CachedOpCode;

        static ServerPushHandlerBase()
        {
            var attr = typeof(T).GetTypeInfo().GetCustomAttribute<ProtocolOpCodeAttribute>();
            CachedOpCode = attr?.OpCode;
        }

        /// <inheritdoc />
        public uint OpCode => CachedOpCode ?? throw new InvalidOperationException(
            $"Type {typeof(T).FullName} does not have a ProtocolOpCodeAttribute. " +
            "Add [ProtocolOpCode(OpCode, Direction)] to your protocol struct.");

        /// <summary>
        /// 处理推送数据（被子类重写）
        /// </summary>
        protected abstract void OnPush(T payload);

        /// <inheritdoc />
        void IServerPushHandler.Handle(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
            {
                OnPush(default);
                return;
            }

            try
            {
                var data = ProtocolRegistry.Instance.Decode<T>(payload);
                OnPush(data);
            }
            catch (Exception ex)
            {
                OnDeserializeError(ex, payload);
            }
        }

        /// <summary>
        /// 反序列化失败时的回调（可选重写）
        /// </summary>
        protected virtual void OnDeserializeError(Exception ex, byte[] payload)
        {
            Console.WriteLine($"[ServerPushHandlerBase] Failed to deserialize {typeof(T).Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Server Push 处理器基类（非泛型版本）
    /// 
    /// 用于需要手动处理 OpCode 的场景
    /// </summary>
    public abstract class ServerPushHandlerBase : IServerPushHandler
    {
        /// <inheritdoc />
        public abstract uint OpCode { get; }

        /// <summary>
        /// 处理推送数据（被子类重写）
        /// </summary>
        protected abstract void OnPush(ReadOnlySpan<byte> payload);

        /// <inheritdoc />
        void IServerPushHandler.Handle(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
            {
                OnPush(ReadOnlySpan<byte>.Empty);
                return;
            }

            try
            {
                OnPush(payload);
            }
            catch (Exception ex)
            {
                OnDeserializeError(ex, payload);
            }
        }

        /// <summary>
        /// 反序列化失败时的回调（可选重写）
        /// </summary>
        protected virtual void OnDeserializeError(Exception ex, byte[] payload)
        {
            Console.WriteLine($"[ServerPushHandlerBase] Handler {GetType().Name} failed to process: {ex.Message}");
        }
    }
}
