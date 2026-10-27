using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Verse;

namespace DpaCsvExporter;

public sealed record EnabledMod(string Name, string PackageId, string? ModVersion, int LoadOrder);
public sealed record DpaVersion(string? Name, string? PackageId, string? ModVersion, string? AssemblyVersion);
public sealed record ExportEnvironment(int SchemaVersion, string CapturedAtUtc, string GameVersion,
    string? ExporterAssemblyVersion, DpaVersion Dpa, IReadOnlyList<EnabledMod> EnabledMods);

/// <summary>Read loaded metadata only when the user exports; no ongoing collection.</summary>
public static class EnvironmentReport
{
    private static readonly JsonSerializerOptions Options = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true
    };

    public static ExportEnvironment Capture(string timestamp, string gameVersion, Assembly dpaAssembly)
    {
        var mods = new List<EnabledMod>();
        EnabledMod? dpa = null;
        foreach (var mod in LoadedModManager.RunningMods)
        {
            string? version = Convert.ToString(mod.ModMetaData?.ModVersion, CultureInfo.InvariantCulture);
            var info = new EnabledMod(mod.Name, mod.PackageId,
                string.IsNullOrWhiteSpace(version) ? null : version, mod.loadOrder);
            mods.Add(info);
            if (mod.assemblies.loadedAssemblies.Contains(dpaAssembly)) dpa = info;
        }
        return new ExportEnvironment(1, timestamp, gameVersion,
            typeof(Exporter).Assembly.GetName().Version?.ToString(),
            new DpaVersion(dpa?.Name, dpa?.PackageId, dpa?.ModVersion, dpaAssembly.GetName().Version?.ToString()), mods);
    }

    public static string Serialize(ExportEnvironment environment) => JsonSerializer.Serialize(environment, Options);
}
