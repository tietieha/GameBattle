#nullable enable

using AbilityKit.Demo.Shooter.View.Hosting;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    public sealed class ShooterNullPlayViewSink : IShooterHostViewSink
    {
        public static ShooterNullPlayViewSink Shared { get; } = new();

        public void Render(in ShooterHostPresentationFrame frame)
        {
        }

        public void Clear()
        {
        }
    }
}
