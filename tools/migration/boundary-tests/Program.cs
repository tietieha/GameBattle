using AbilityKit.Core;
using AbilityKit.Core.Recording.Core;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

if (args.Length > 0 && args[0] == "inspect")
{
    var runtime = Path.GetFullPath("Unity/Assets/Scripts/GameBattle/Runtime");
    var names = Directory.EnumerateFiles(runtime, "*.asmdef", SearchOption.AllDirectories)
        .Where(file => !file.Replace('\\', '/').Contains("/Tests/") &&
            !file.Replace('\\', '/').Split('/').Any(part => part.EndsWith("~")))
        .Select(file => JsonDocument.Parse(File.ReadAllText(file)).RootElement.GetProperty("name").GetString())
        .Distinct().ToArray();
    var missing = new List<string>();
    var engineReferences = new List<string>();
    foreach (var name in names)
    {
        var binary = Path.GetFullPath($"Unity/Temp/Bin/Debug/{name}/{name}.dll");
        if (!File.Exists(binary)) { missing.Add(name); continue; }
        using var stream = File.OpenRead(binary);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        foreach (var handle in metadata.AssemblyReferences)
        {
            var dependency = metadata.GetString(metadata.GetAssemblyReference(handle).Name);
            if (dependency == "UnityEngine" || dependency == "UnityEditor" ||
                dependency.StartsWith("UnityEngine.") || dependency.StartsWith("UnityEditor.") ||
                dependency.StartsWith("Unity."))
                engineReferences.Add($"{name} -> {dependency}");
        }
    }
    var report = new { assemblies = names.Length, inspected = names.Length - missing.Count, missing, engineReferences };
    var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    var reportDirectory = Directory.Exists("Docs/GameBattle/AssemblyConsolidation")
        ? "Docs/GameBattle/AssemblyConsolidation"
        : "Docs/GameBattle/Reorganization";
    File.WriteAllText(Path.Combine(reportDirectory, "runtime-binaries.json"), json);
    Console.WriteLine(json);
    if (missing.Count > 0 || engineReferences.Count > 0) Environment.ExitCode = 1;
    return;
}

var originalDirectory = RuntimeEnvironment.PersistentDataDirectoryProvider;
var originalWarning = RuntimeEnvironment.WarningHandler;
var temporary = Path.Combine(Path.GetTempPath(), "abilitykit-boundary-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temporary);
try
{
    Check(RuntimeEnvironment.PersistentDataDirectory == Environment.CurrentDirectory, "default directory");
    RuntimeEnvironment.PersistentDataDirectoryProvider = () => temporary;
    Check(RecordFileStore.ResolvePath("test.bin") == Path.Combine(temporary, "test.bin"), "injected directory");
    Check(RecordFileStore.ResolvePath(temporary) == temporary, "absolute path unchanged");
    Check(RecordFileStore.ResolvePath(null) == null, "null path unchanged");
    Check(RecordFileStore.ResolvePath("") == "", "empty path unchanged");
    var data = new byte[] { 1, 2, 3, 4 };
    RecordFileStore.Save("nested/test.bin", data);
    Check(RecordFileStore.Load("nested/test.bin").SequenceEqual(data), "record save/load roundtrip");
    string warning = null;
    RuntimeEnvironment.WarningHandler = message => warning = message;
    RuntimeEnvironment.LogWarning("boundary warning");
    Check(warning == "boundary warning", "injected warning handler");
    Console.WriteLine("Passed 7 engine-free boundary checks.");
}
finally
{
    RuntimeEnvironment.PersistentDataDirectoryProvider = originalDirectory;
    RuntimeEnvironment.WarningHandler = originalWarning;
    var verified = Path.GetFullPath(temporary);
    var tempRoot = Path.GetFullPath(Path.GetTempPath());
    if (!verified.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Temporary cleanup escaped its boundary.");
    Directory.Delete(verified, true);
}

static void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
}
