using System;

namespace AbilityKit.Core
{
    public static class RuntimeEnvironment
    {
        public static Func<string> PersistentDataDirectoryProvider { get; set; } =
            () => Environment.CurrentDirectory;

        public static Action<string> WarningHandler { get; set; } =
            message => Console.Error.WriteLine(message);

        public static string PersistentDataDirectory => PersistentDataDirectoryProvider();

        public static void LogWarning(string message)
        {
            WarningHandler(message);
        }
    }
}
