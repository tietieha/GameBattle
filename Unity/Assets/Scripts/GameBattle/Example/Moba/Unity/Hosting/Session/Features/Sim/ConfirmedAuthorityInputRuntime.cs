using System;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.Host.Extensions.FrameSync;
using AbilityKit.Network.Abstractions;
using AbilityKit.Network.Runtime;

namespace AbilityKit.Game.Flow
{
    internal sealed class ConfirmedAuthorityInputRuntime : IDisposable
    {
        private FrameJitterBufferHub<PlayerInputCommand[]> _hub;

        private ConfirmedAuthorityInputRuntime(FrameJitterBufferHub<PlayerInputCommand[]> hub)
        {
            _hub = hub;
        }

        public IRemoteFrameSource<PlayerInputCommand[]> Source => _hub;

        public IConsumableRemoteFrameSource<PlayerInputCommand[]> Consumable => _hub;

        public IRemoteFrameSink<PlayerInputCommand[]> Sink => _hub;

        public static ConfirmedAuthorityInputRuntime Create()
        {
            var hub = FrameSyncInputHubFactory.CreateJitterBufferHub<PlayerInputCommand[]>(
                delayFrames: 0,
                missingMode: MissingFrameMode.FillDefault,
                missingFrameFactory: Array.Empty<PlayerInputCommand>,
                initialCapacity: 256);

            return new ConfirmedAuthorityInputRuntime(hub);
        }

        public void Dispose()
        {
            _hub?.Dispose();
            _hub = null;
        }
    }
}
