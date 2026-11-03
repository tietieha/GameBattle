using System;
using System.IO;
using Newtonsoft.Json;

namespace AbilityKit.Core.Recording.FrameRecord
{
    public static class FrameRecordJsonReader
    {
        public static FrameRecordFile Load(string path)
        {
            var resolved = ResolvePath(path);
            var json = File.ReadAllText(resolved);
            return JsonConvert.DeserializeObject<FrameRecordFile>(json);
        }

        private static string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (Path.IsPathRooted(path)) return path;

            var baseDir = AbilityKit.Core.RuntimeEnvironment.PersistentDataDirectory;
            return Path.Combine(baseDir, path);
        }
    }
}
