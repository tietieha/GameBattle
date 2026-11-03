using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;

/// <summary>
/// 文件名称: IMobaInputCommandHandler.cs
/// 
/// 功能描述: 定义可扩展的 MOBA 输入命令处理器接口。
/// 
/// 创建日期: 2026-05-27
/// 修改日期: 2026-05-27
/// </summary>
namespace AbilityKit.Demo.Moba.Services
{
    /// <summary>
    /// MOBA 输入命令处理器接口。
    /// </summary>
    public interface IMobaInputCommandHandler
    {
        /// <summary>
        /// 处理输入命令。
        /// </summary>
        /// <param name="context">输入处理上下文</param>
        /// <param name="frame">当前帧</param>
        /// <param name="command">输入命令</param>
        bool Handle(MobaInputCommandContext context, FrameIndex frame, PlayerInputCommand command, out MobaInputCommandResult result);
    }
}