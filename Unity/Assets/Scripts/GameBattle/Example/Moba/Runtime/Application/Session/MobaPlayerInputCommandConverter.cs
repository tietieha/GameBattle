using System;
using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Coordinator;
using AbilityKit.Protocol.Moba.StateSync;

/// <summary>
/// 文件名称: MobaPlayerInputCommandConverter.cs
/// 
/// 功能描述: 将表现层/Coordinator 输入转换为 moba.runtime 统一输入命令。
/// 
/// 创建日期: 2026-05-27
/// 修改日期: 2026-05-27
/// </summary>
namespace AbilityKit.Demo.Moba.Session
{
    /// <summary>
    /// 表现层输入命令转换器，保持逻辑世界驱动桥只负责调度生命周期。
    /// </summary>
    public sealed class MobaPlayerInputCommandConverter
    {
        /// <summary>
        /// 将 Coordinator 输入批量转换为世界输入命令。
        /// </summary>
        /// <param name="inputs">Coordinator 输入数组</param>
        /// <returns>可提交到 IMobaBattleRuntimePort 的命令列表</returns>
        public IReadOnlyList<PlayerInputCommand> Convert(PlayerInput[] inputs)
        {
            if (inputs == null || inputs.Length == 0)
            {
                return Array.Empty<PlayerInputCommand>();
            }

            List<PlayerInputCommand> commands = new List<PlayerInputCommand>(inputs.Length);
            for (int i = 0; i < inputs.Length; i++)
            {
                PlayerInput input = inputs[i];
                PlayerId playerId = new PlayerId(input.PlayerId.ToString());
                commands.Add(new PlayerInputCommand(
                    new FrameIndex(input.Frame),
                    playerId,
                    input.OpCode,
                    input.Payload));
            }

            return commands;
        }

        public IReadOnlyList<PlayerInputCommand> Convert(PlayerInput[] inputs, FrameIndex targetFrame)
        {
            if (inputs == null || inputs.Length == 0)
            {
                return Array.Empty<PlayerInputCommand>();
            }

            List<PlayerInputCommand> commands = new List<PlayerInputCommand>(inputs.Length);
            for (int i = 0; i < inputs.Length; i++)
            {
                PlayerInput input = inputs[i];
                PlayerId playerId = new PlayerId(input.PlayerId.ToString());
                commands.Add(new PlayerInputCommand(
                    targetFrame,
                    playerId,
                    input.OpCode,
                    input.Payload));
            }

            return commands;
        }

    }
}