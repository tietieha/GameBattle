using System.Collections.Generic;

namespace AbilityKit.Demo.Shooter.View.ViewEvents
{
    internal sealed class ShooterViewEventSink : IShooterViewEventSink
    {
        private readonly List<IShooterViewEventSink> _sinks = new List<IShooterViewEventSink>();

        public void AddSink(IShooterViewEventSink sink)
        {
            if (sink != null && !_sinks.Contains(sink))
            {
                _sinks.Add(sink);
            }
        }

        public void RemoveSink(IShooterViewEventSink sink)
        {
            _sinks.Remove(sink);
        }

        public void HandleEvent(object viewEvent)
        {
            foreach (var sink in _sinks)
            {
                sink.HandleEvent(viewEvent);
            }
        }

        public void Clear()
        {
            foreach (var sink in _sinks)
            {
                sink.Clear();
            }
        }
    }
}