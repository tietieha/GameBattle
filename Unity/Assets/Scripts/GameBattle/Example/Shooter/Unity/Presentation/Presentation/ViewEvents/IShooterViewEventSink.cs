namespace AbilityKit.Demo.Shooter.View.ViewEvents
{
    public interface IShooterViewEventSink
    {
        void HandleEvent(object viewEvent);
        void Clear();
    }
}