#nullable enable

using System;

namespace AbilityKit.Demo.Shooter.View.Session
{
    public static class ShooterPresentationSessionHost
    {
        private static ShooterPresentationSession? _current;

        public static event Action<ShooterPresentationSession?>? SessionChanged;

        public static ShooterPresentationSession? Current => _current;

        public static bool HasSession => _current != null;

        public static ShooterPresentationSession Start(ShooterPresentationSessionOptions options, IShooterPresentationClient? client = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Stop();

            _current = new ShooterPresentationSession(options, client);
            SessionChanged?.Invoke(_current);
            return _current;
        }

        public static ShooterPresentationSession Start()
        {
            return Start(new ShooterPresentationSessionOptions());
        }

        public static void Stop()
        {
            if (_current == null)
            {
                return;
            }

            try
            {
                _current.Dispose();
            }
            finally
            {
                _current = null;
                SessionChanged?.Invoke(null);
            }
        }
    }
}