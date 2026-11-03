using System;
using AbilityKit.Network.Abstractions;
using GameFramework.Network;

namespace AbilityKit.GameFramework.Network
{
    public static class GameFrameworkGatewayConnectionFactory
    {
        public static IConnection Create(INetworkManager networkManager, string channelName, ServiceType serviceType = ServiceType.Tcp)
        {
            if (networkManager == null) throw new ArgumentNullException(nameof(networkManager));
            if (string.IsNullOrWhiteSpace(channelName)) throw new ArgumentException("Channel name is required.", nameof(channelName));

            var channel = networkManager.HasNetworkChannel(channelName)
                ? networkManager.GetNetworkChannel(channelName)
                : networkManager.CreateNetworkChannel(channelName, serviceType, new AbilityKitGatewayNetworkChannelHelper());

            return Wrap(channel);
        }

        public static IConnection Wrap(INetworkChannel channel)
        {
            return new GameFrameworkNetworkChannelConnection(channel);
        }
    }
}
