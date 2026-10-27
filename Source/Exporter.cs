using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace DpaCsvExporter;

/// <summary>
/// Exports the complete published Analyzer.Logs table, including rows hidden by GUI filters.
/// No additional profiling is performed.
/// </summary>
public static class Exporter
{
    private const BindingFlags StaticPublic = BindingFlags.Public | BindingFlags.Static;
    private const BindingFlags InstancePublic = BindingFlags.Public | BindingFlags.Instance;
    private sealed class Row
    {
        public string Key = "", Label = "", Mod = "", Assembly = "", Type = "", Method = "";
        public int Entries;
        public int? Spikes, Severity, Phase;
        public double Average, Self, Max, Total, Calls, Fraction;
        public bool? Pinned;
    }

    private static Type RequireType(string name)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(name, false);
            if (type != null) return type;
        }
        throw new InvalidOperationException("Dubs Performance Analyzer is not loaded: " + name);
    }

    private static object? Static(Type type, string name)
        => type.GetProperty(name, StaticPublic)?.GetValue(null)
           ?? type.GetField(name, StaticPublic)?.GetValue(null);

    private static object? Field(object instance, string name)
        => instance.GetType().GetField(name, InstancePublic)?.GetValue(instance);

    private static string Str(object? value) => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
    private static int Int(object? value) => value == null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    private static double Num(object? value) => value == null ? 0 : Convert.ToDouble(value, CultureInfo.InvariantCulture);

    private static int? OptionalInt(object? value) => value == null ? null : Int(value);
    private static string OptionalNumber(int? value) => value.HasValue ? CsvFormat.Number(value.Value) : "";

    public static string ExportCurrent()
    {
        var analyzer = RequireType("Analyzer.Profiling.Analyzer");
        var gui = RequireType("Analyzer.Profiling.GUIController");
        var toolbar = RequireType("Analyzer.Profiling.Panel_TopRow");
        object? entry = Static(gui, "CurrentEntry");
        string category = Str(Static(gui, "CurrentCategory"));
        if (entry == null || category == "Settings")
            throw new InvalidOperationException("Select a Tick, Update, GUI or other profiling entry first.");

        var sync = Static(analyzer, "LogicLock")
                   ?? throw new InvalidOperationException("Dubs Analyzer.LogicLock not found.");
        var rows = new List<Row>();

        // DPA publishes a fresh List<ProfileLog> under LogicLock. Copy its object
        // references while locked; do all reflection and method stringification
        // after releasing the lock so export doesn't stall analyzer updates.
        object[] sampledLogs;
        lock (sync)
        {
            var logs = Static(analyzer, "Logs") as IEnumerable
                       ?? throw new InvalidOperationException("Dubs Analyzer.Logs not found.");
            sampledLogs = logs.Cast<object>().Where(item => item != null).ToArray();
        }
        var modMap = ModAssemblyNames();
        foreach (object item in sampledLogs)
        {
                if (item == null) continue;
                var schema = DpaLogSchema.For(item.GetType());
                var method = schema.Read(item, "meth") as MethodBase;
                // A profiler wrapper type is not necessarily the measured method's owner.
                var type = method?.DeclaringType ?? schema.Read(item, "type") as Type;
                string assFull = type?.Assembly.FullName ?? "";
                string mod = "";
                if (assFull.Length > 0 && modMap.TryGetValue(assFull, out var known))
                    mod = known;
                else if (assFull.StartsWith("Assembly-CSharp,", StringComparison.Ordinal))
                    mod = "RimWorld Core";
                rows.Add(new Row {
                    Key = Str(schema.Read(item, "key")), Label = Str(schema.Read(item, "label")), Mod = mod,
                    Assembly = type?.Assembly.GetName().Name ?? "", Type = type?.FullName ?? "",
                    Method = method?.ToString() ?? "",
                    Entries = Int(schema.Read(item, "entries")),
                    Average = Num(schema.Read(item, "average")),
                    Self = Num(schema.Read(item, "self")),
                    Max = Num(schema.Read(item, "max")),
                    Total = Num(schema.Read(item, "total")),
                    Calls = Num(schema.Read(item, "calls")),
                    Fraction = Num(schema.Read(item, "percent")),
                    Spikes = OptionalInt(schema.Read(item, "spikes")),
                    Severity = OptionalInt(schema.Read(item, "spike")),
                    Phase = OptionalInt(schema.Read(item, "seenIn")),
                    Pinned = schema.Read(item, "pinned") as bool?
                });
        }
        if (rows.Count == 0)
            throw new InvalidOperationException("No measurements yet. Start Dubs profiling and retry.");

        var now = DateTime.UtcNow;
        var timestamp = now.ToString("O", CultureInfo.InvariantCulture);
        string entryName = Str(Field(entry, "name"));
        int fps = Int(Static(toolbar, "fps")), tps = Int(Static(toolbar, "tps"));
        int tpsTarget = Int(Static(toolbar, "tpsTarget"));
        string filter = Str(Static(toolbar, "TimesFilter"));

        string folder = Path.Combine(GenFilePaths.SaveDataFolderPath, "AnalyzerExports");
        Directory.CreateDirectory(folder);
        string fileName = now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture)
            + "_" + SafeName(category) + "_" + SafeName(entryName)
            + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".csv";
        string path = Path.Combine(folder, fileName);

        string gameVersion = GameVersion();
        var csvRows = new List<string?[]>(rows.Count);
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            string?[] fields = {
                timestamp, gameVersion, category, entryName, filter,
                CsvFormat.Number(fps), CsvFormat.Number(tps), CsvFormat.Number(tpsTarget),
                // DPA does not publish the root average associated with this batch.
                // Unknown stays empty; never substitute live state or infer a ratio.
                "", CsvFormat.Number(row.Entries), CsvFormat.Number(i+1),
                row.Key, row.Label, row.Mod, row.Assembly, row.Type, row.Method,
                CsvFormat.Number(row.Entries), CsvFormat.Number(row.Average),
                CsvFormat.Number(row.Self), CsvFormat.Number(row.Max), CsvFormat.Number(row.Total),
                CsvFormat.Number(row.Calls),
                CsvFormat.Number(row.Entries == 0 ? 0 : row.Calls / row.Entries),
                CsvFormat.Number(row.Fraction), OptionalNumber(row.Spikes),
                OptionalNumber(row.Severity), OptionalNumber(row.Phase),
                row.Pinned.HasValue ? (row.Pinned.Value ? "1" : "0") : ""
            };
            csvRows.Add(fields);
        }
        AtomicFile.Write(path, writer => CsvReport.Write(writer, csvRows));
        return path;
    }

    private static Dictionary<string,string> ModAssemblyNames()
    {
        var mapping = new Dictionary<string,string>(StringComparer.Ordinal);
        var cache = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("Analyzer.ModInfoCache", false)).FirstOrDefault(t => t != null);
        if (cache != null && Static(cache, "AssemblyToModname") is IDictionary source)
            foreach (DictionaryEntry kv in source)
                if (kv.Key is string key && kv.Value is string value) mapping[key] = value;

        foreach (var mod in LoadedModManager.RunningMods)
            foreach (var assembly in mod.assemblies.loadedAssemblies)
                if (assembly.FullName != null && !mapping.ContainsKey(assembly.FullName))
                    mapping[assembly.FullName] = mod.Name;
        return mapping;
    }

    private static string GameVersion()
    {
        // Version.txt sits next to the shipped Windows or Linux game executable.
        // Prefer this exact build identifier to a generic 1.7 label.
        foreach (var baseFolder in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
        {
            try
            {
                string path = Path.Combine(baseFolder, "Version.txt");
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path).Trim();
                    if (!string.IsNullOrWhiteSpace(content)) return content;
                }
            }
            catch (IOException) { /* try the runtime API */ }
            catch (UnauthorizedAccessException) { /* try the runtime API */ }
        }
        // The RimWorld 1.7 runtime's version API may differ from older assemblies.
        // Never record a guessed release string as though it were the exact build.
        var candidates = new[] {
            "Verse.VersionControl",
            "RimWorld.VersionControl",
            "Verse.VersionInfo"
        };
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var name in candidates)
            {
                var type = assembly.GetType(name, false);
                if (type == null) continue;
                var value = Static(type, "CurrentVersionString")
                            ?? Static(type, "CurrentVersionStringWithRev")
                            ?? Static(type, "CurrentVersionStringWithoutBuild")
                            ?? Static(type, "CurrentVersion");
                var text = Str(value);
                if (text.Length > 0) return text;
            }
        return "unknown";
    }

    private static string SafeName(string text)
    {
        var invalid = Path.GetInvalidFileNameChars();
        string cleaned = new(text.Select(c =>
            Array.IndexOf(invalid, c) >= 0 || c == '/' || c == '\\' || c == ':' ? '_' : c).ToArray());
        cleaned = cleaned.Trim();
        return cleaned.Length == 0 ? "unknown" : cleaned.Length > 48 ? cleaned.Substring(0,48) : cleaned;
    }
}
