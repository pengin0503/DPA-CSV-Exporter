using System.Reflection;
namespace Verse
{
    public static class GenFilePaths { public static string SaveDataFolderPath = ""; }
    public static class LoadedModManager { public static IEnumerable<ModFixture> RunningMods => Array.Empty<ModFixture>(); }
    public sealed class ModFixture
    {
        public string Name = "Fixture";
        public Assemblies assemblies = new();
    }
    public sealed class Assemblies { public List<Assembly> loadedAssemblies = new(); }
}
namespace Analyzer.Profiling
{
    public static class Analyzer
    {
        public static readonly object LogicLock = new();
        public static List<object> Logs = new();
        public static int GetCurrentLogCount = 10;
    }
    public sealed class Entry { public string name = "Fixture"; }
    public static class GUIController
    {
        public static Entry CurrentEntry = new();
        public static string CurrentCategory = "Tick";
    }
    public static class ProfileController { public static double updateAverage = 5; }
    public static class Panel_TopRow
    {
        public static int fps = 60, tps = 60, tpsTarget = 60;
        public static string TimesFilter = "";
    }
    // Mirrors all fields of the supplied DPA 1.7 ProfileLog.
    public sealed class ProfileLog
    {
        public int entries = 10;
        public float percent = 0.25f;
        public double average = 1.25;
        public string key = "key", label = "label";
        public float max = 4, total = 12.5f, calls = 30;
        public Type? type;
        public MethodBase? meth;
        public bool pinned = true;
        public double self = 0.5;
        public int spike = 1, spikes = 2;
        public string spikeText = "fixture";
        public int seenIn = 4;
    }
}
