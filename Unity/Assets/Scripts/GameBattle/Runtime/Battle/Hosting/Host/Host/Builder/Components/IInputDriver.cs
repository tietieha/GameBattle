using System;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host.Framework;
using AbilityKit.Ability.Host.Transport;
using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Ability.Host.Builder.Components
{
    /// <summary>
    /// 输入驱动接口
    /// 负责收集和分发玩家输入
    /// </summary>
    public interface IInputDriver
    {
        /// <summary>
        /// 附加到 Runtime
        /// </summary>
        void Attach(HostRuntime runtime, HostRuntimeOptions options);

        /// <summary>
        /// 从 Runtime 分离
        /// </summary>
        void Detach();

        /// <summary>
        /// 提交输入
        /// </summary>
        bool SubmitInput(ServerClientId clientId, WorldId worldId, PlayerInputCommand input);

        /// <summary>
        /// 注册输入处理回调
        /// </summary>
        void AddInputsFlushed(Action<WorldId, FrameIndex, PlayerInputCommand[]> handler);
        void RemoveInputsFlushed(Action<WorldId, FrameIndex, PlayerInputCommand[]> handler);
    }
}
