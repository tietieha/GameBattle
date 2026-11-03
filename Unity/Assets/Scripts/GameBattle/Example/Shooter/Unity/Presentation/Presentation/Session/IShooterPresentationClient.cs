using System;

namespace AbilityKit.Demo.Shooter.View.Session
{
    public interface IShooterPresentationClient : IDisposable
    {
        bool IsConnected { get; }
        
        event Action<uint, ArraySegment<byte>>? DataReceived;
        
        void Connect();
        
        void Disconnect();
        
        void Tick(float deltaTime);
    }
}