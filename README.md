# DPA CSV Exporter — RimWorld 1.7

An independent addon for **Dubs Performance Analyzer** that exports the **entire current profiling table** in one CSV file, including rows hidden by the Dubs search filter. It does not start extra profiling or modify game behavior.

**Status:** Builds successfully against genuine RimWorld 1.7 reference DLLs; CSV serializer tests passed. **The visual button and real CSV generation in a running Windows game have not yet been verified.**

## Installation

Download `dist/DPA-CSV-Exporter-1.0.0-RW1.7.zip` from this repository. Unzip and place its `DPA-CSV-Exporter` folder in the RimWorld `Mods` directory. Requires RimWorld 1.7, Harmony, and Dubs Performance Analyzer (load in that order before this addon).

1. Open Dubs Performance Analyzer.
2. Select a Tick, Update, GUI or other profiling entry and allow measurements to accumulate.
3. Press **Export CSV** in the analyzer, or use the backup button in the addon settings.
4. CSV files are saved to `GenFilePaths.SaveDataFolderPath/AnalyzerExports`.

Normally on Windows this is `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\AnalyzerExports`.

Each CSV contains 29 columns: category, selected entry, FPS/TPS, measured call counts, average/self/max/total execution time, method/assembly and best-effort MOD attribution, etc. UTF-8 with BOM for Excel. The export saves **one selected profiling entry** at a time, not every category simultaneously.

## Source and testing

- `Source/`: addon C# code (net10.0) and compiler project.
- `Tests/`: CSV formatting tests.
- `1.7/Assemblies/`: compiled and redistributable addon DLL (no game, Harmony, or Dubs DLL bundled).
- `dist/`: ready-to-install ZIP archive.
- `.github/workflows/build.yml`: **manual** build from the three private reference DLLs stored in the owner's Cloudflare R2. This workflow requires repository-local GitHub Actions secrets `R2_ACCESS_KEY_ID`, `R2_SECRET_ACCESS_KEY` and variable `R2_ENDPOINT`. No automatic R2-heavy CI runs.

Local development:

```sh
dotnet run --project Tests/CsvSmokeTest.csproj -c Release
dotnet build Source/DPA.CsvExporter.csproj -c Release -p:RimWorldRefs=/private/RimWorld1.7/refs
```

The compile references must be `Assembly-CSharp.dll`, `UnityStubs.dll`, and `Waylith.dll` from an authorized RimWorld 1.7 installation; these proprietary files are **not** tracked or packaged here.

Built from the [DPA CSV Exporter focused-build run](https://github.com/pengin0503/RimWorld-for-mod-test/actions/runs/40447166070). The unrelated test-only RimWorld host is not required to run this addon.
