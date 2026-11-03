using System;

namespace AbilityKit.Demo.Shooter.View.Session
{
    public interface IShooterPresentationTransport : IDisposable
    {
        event Action<uint, ArraySegment<byte>>? DataReceived;
        
        bool IsConnected { get; }
        
        void Connect();
        
        void Disconnect();
        
        void Send(uint opCode, ArraySegment<byte> payload);
    }
}