using System.Reflection;
using System.Reflection.Emit;
using DpaCsvExporter;
using Dpa = Analyzer.Profiling;

static void Check(bool condition, string reason) { if (!condition) throw new Exception(reason); }
static string[][] Export()
{
    var lines = File.ReadAllLines(Exporter.ExportCurrent());
    Check(lines.Length == 2, "Expected header and one row");
    var cells = lines.Select(line => line.TrimStart('\uFEFF').Split(',')).ToArray();
    Check(cells.All(row => row.Length == 29), "CSV schema must remain 29 columns");
    return cells;
}
static object Variant(string? omit = null, string? change = null, bool coreOnly = false)
{
    var module = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DpaFixture" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run).DefineDynamicModule("Main");
    var type = module.DefineType("ProfileLog", TypeAttributes.Public);
    var core = new HashSet<string> { "entries", "average", "self", "max", "total", "calls", "percent" };
    foreach (var field in typeof(Dpa.ProfileLog).GetFields())
    {
        if (field.Name == omit || (coreOnly && !core.Contains(field.Name))) continue;
        type.DefineField(field.Name, field.Name == change ? typeof(string) : field.FieldType, FieldAttributes.Public);
    }
    return Activator.CreateInstance(type.CreateType()!)!;
}
static void Incompatible(object row, string field)
{
    Dpa.Analyzer.Logs = new() { row };
    var before = Directory.GetFiles(Verse.GenFilePaths.SaveDataFolderPath, "*", SearchOption.AllDirectories).Length;
    string? error = null;
    try { Exporter.ExportCurrent(); }
    catch (InvalidOperationException e) { error = e.Message; }
    Check(error != null && error.Contains(field), "Missing/wrong field " + field + " must report compatibility failure");
    Check(Directory.GetFiles(Verse.GenFilePaths.SaveDataFolderPath, "*", SearchOption.AllDirectories).Length == before,
        "Compatibility failure must not create a CSV or temporary file");
}
var root = Path.Combine(Path.GetTempPath(), "dpa-exporter-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
Verse.GenFilePaths.SaveDataFolderPath = root;
var tests = new List<(string Name, Action Run)>();
foreach (string name in new[] { "entries", "average", "self", "max", "total", "calls", "percent" })
{
    var field = name;
    tests.Add(("missing " + field, () => Incompatible(Variant(omit: field), field)));
    tests.Add(("changed type " + field, () => Incompatible(Variant(change: field), field)));
}
tests.Add(("changed optional field type is rejected", () => Incompatible(Variant(change: "pinned"), "pinned")));
tests.Add(("root average cannot be recovered from a published ratio", () => {
    Dpa.Analyzer.Logs = new() { new Dpa.ProfileLog() };
    var row = Export()[1];
    Check(row[8] == "", "Unknown root average must stay blank even if live root happens to match");
}));
tests.Add(("real zero measurements survive", () => {
    Dpa.Analyzer.Logs = new() { Variant() };
    var row = Export()[1];
    foreach (int i in new[] { 17,18,19,20,21,22,23,24 }) Check(row[i] == "0", "Real zero must remain zero at " + i);
}));
tests.Add(("missing optional metadata stays blank", () => {
    Dpa.Analyzer.Logs = new() { Variant(coreOnly: true) };
    var row = Export()[1];
    foreach (int i in new[] { 11,12,13,14,15,16,25,26,27,28 }) Check(row[i] == "", "Unknown metadata must be blank at " + i);
}));
tests.Add(("DPA 1.7 values and schema retained", () => {
    Dpa.Analyzer.Logs = new() { new Dpa.ProfileLog() };
    var row = Export()[1];
    var expected = new Dictionary<int,string> { [17]="10",[18]="1.25",[19]="0.5",[20]="4",[21]="12.5",[22]="30",[23]="3",[24]="0.25",[25]="2",[26]="1",[27]="4",[28]="1" };
    foreach (var pair in expected) Check(row[pair.Key] == pair.Value, "Changed DPA value at " + pair.Key);
}));
tests.Add(("measured method owner wins over profiler wrapper", () => {
    Dpa.Analyzer.Logs = new() { new Dpa.ProfileLog { type = typeof(Dpa.ProfileLog), meth = typeof(string).GetMethod(nameof(string.Clone), Type.EmptyTypes) } };
    var row = Export()[1];
    Check(row[14] == typeof(string).Assembly.GetName().Name && row[15] == "System.String",
        "Attribution must use the measured method declaring type");
}));
tests.Add(("exact game version distinguishes build files", () => {
    string cwd = Environment.CurrentDirectory;
    try
    {
        Environment.CurrentDirectory = root;
        Dpa.Analyzer.Logs = new() { new Dpa.ProfileLog() };
        foreach (string version in new[] { "1.7.4985 unstable", "1.7.5000 fixture" })
        {
            File.WriteAllText(Path.Combine(root, "Version.txt"), version);
            Check(Export()[1][1] == version, "Exact build version must be retained");
        }
    }
    finally { Environment.CurrentDirectory = cwd; File.Delete(Path.Combine(root, "Version.txt")); }
}));
tests.Add(("in-flight calculation cannot contaminate published row metadata", () => {
    Dpa.Analyzer.Logs = new() { new Dpa.ProfileLog() };
    using var started = new ManualResetEventSlim();
    using var publish = new ManualResetEventSlim();
    var worker = Task.Run(() => {
        // Matches DPA: update live root/cycle state, calculate asynchronously,
        // then replace Logs. Keep the previous published batch during export.
        Dpa.ProfileController.updateAverage = 99;
        Dpa.Analyzer.GetCurrentLogCount = 200;
        started.Set();
        publish.Wait();
        lock (Dpa.Analyzer.LogicLock) Dpa.Analyzer.Logs = new() { new Dpa.ProfileLog { entries = 200 } };
    });
    try
    {
        Check(started.Wait(TimeSpan.FromSeconds(10)), "Worker did not start");
        var row = Export()[1];
        Check(row[9] == "10" && row[17] == "10", "Cycles must come from published row, not live counter 200");
        Check(row[8] == "", "Unpublished snapshot root must be blank, not live 99 or an inferred estimate");
    }
    finally { publish.Set(); worker.GetAwaiter().GetResult(); }
}));
tests.Add(("environment JSON accompanies CSV with matching context", () => {
    var original = Verse.LoadedModManager.RunningMods;
    try
    {
        Verse.LoadedModManager.RunningMods = new[] {
            new Verse.ModFixture { Name = "先頭MOD", PackageId = "example.first", loadOrder = 3 },
            new Verse.ModFixture { Name = "DPA 日本語 \"quoted\"", PackageId = "dubwise.dubsperformanceanalyzer.steam", loadOrder = 7,
                ModMetaData = new Verse.ModMetaData { ModVersion = "2.3-test" },
                assemblies = new Verse.Assemblies { loadedAssemblies = new() { typeof(Dpa.Analyzer).Assembly } } }
        };
        Dpa.Analyzer.Logs = new() { new Dpa.ProfileLog() };
        string csv = Exporter.ExportCurrent();
        string json = Path.ChangeExtension(csv, ".json");
        Check(File.Exists(json), "A successful export must include its environment JSON");
        using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(json));
        var env = doc.RootElement;
        var row = File.ReadAllLines(csv)[1].Split(',');
        Check(env.GetProperty("schemaVersion").GetInt32() == 1, "Environment schema version required");
        Check(env.GetProperty("capturedAtUtc").GetString() == row[0], "JSON/CSV timestamps must match");
        Check(env.GetProperty("gameVersion").GetString() == row[1], "JSON/CSV game builds must match");
        Check(!string.IsNullOrEmpty(env.GetProperty("exporterAssemblyVersion").GetString()), "Exporter version required");
        var mods = env.GetProperty("enabledMods");
        Check(mods.GetArrayLength() == 2, "Record every enabled mod");
        Check(mods[0].GetProperty("name").GetString() == "先頭MOD" && mods[0].GetProperty("loadOrder").GetInt32() == 3,
            "Preserve Unicode and actual engine load order");
        Check(mods[0].GetProperty("modVersion").ValueKind == System.Text.Json.JsonValueKind.Null, "Undeclared version stays unknown");
        Check(mods[1].GetProperty("name").GetString() == "DPA 日本語 \"quoted\"" && mods[1].GetProperty("packageId").GetString() == "dubwise.dubsperformanceanalyzer.steam",
            "JSON must preserve quotes and PackageId");
        Check(env.GetProperty("dpa").GetProperty("modVersion").GetString() == "2.3-test", "DPA declared version required");
        Check(env.GetProperty("dpa").GetProperty("assemblyVersion").GetString() == typeof(Dpa.Analyzer).Assembly.GetName().Version?.ToString(),
            "DPA assembly version must be identified separately");
        Check(row.Length == 29, "Sidecar must not change CSV columns");
    }
    finally { Verse.LoadedModManager.RunningMods = original; }
}));
int failed = 0;
try
{
    foreach (var test in tests)
    {
        try { test.Run(); Console.WriteLine("PASS " + test.Name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + test.Name + ": " + e.Message); }
    }
}
finally { Directory.Delete(root, true); }
Console.WriteLine($"Exporter regression: {tests.Count-failed}/{tests.Count} passed");
return failed == 0 ? 0 : 1;
