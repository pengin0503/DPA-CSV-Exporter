# DPA CSV Exporter — RimWorld 1.7

An independent addon for **Dubs Performance Analyzer** that exports the **entire current profiling table** in one CSV file, including rows hidden by the Dubs search filter. It does not start extra profiling or modify game behavior.

**Status:** The source is built against the public RimRef 1.7.4985 signature package without Cloudflare R2 credentials. The checked-in DLL and ZIP are updated by the main-branch build workflow **only after successful compilation and packaging**. Windows in-game UI/CSV operation remains unverified.

## Installation

Download [DPA-CSV-Exporter-1.0.3-RW1.7.zip](https://github.com/pengin0503/DPA-CSV-Exporter/releases/download/v1.0.3/DPA-CSV-Exporter-1.0.3-RW1.7.zip) from the [v1.0.3 Release](https://github.com/pengin0503/DPA-CSV-Exporter/releases/tag/v1.0.3). Unzip and place its `DPA-CSV-Exporter` folder in the RimWorld `Mods` directory. Requires RimWorld 1.7, Harmony, and Dubs Performance Analyzer (load in that order before this addon).

1. Open Dubs Performance Analyzer.
2. Select a Tick, Update, GUI or other profiling entry and allow measurements to accumulate.
3. Press **Export CSV** in the dedicated bottom toolbar of the analyzer, or use the backup button in the addon settings.
4. CSV files are saved to `GenFilePaths.SaveDataFolderPath/AnalyzerExports`.

Normally on Windows this is `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\AnalyzerExports`.

Each CSV contains 29 columns: category, selected entry, FPS/TPS, measured call counts, average/self/max/total execution time, method/assembly and best-effort MOD attribution, etc. UTF-8 with BOM for Excel. The export saves **one selected profiling entry** at a time, not every category simultaneously.

## Source and testing

- `Source/`: addon C# code (net10.0) and compiler project.
- `Tests/`: CSV formatting tests; `Tests/Gui/` runs the real addon patch with real Harmony and test-only UI boundaries.
- `1.7/Assemblies/`: compiled and redistributable addon DLL (no game, Harmony, or Dubs DLL bundled).
- `dist/`: ready-to-install ZIP archive.
- `.github/workflows/pr-checks.yml`: runs CSV regression tests and compiles a test ZIP using `Krafs.Rimworld.Ref` from NuGet on pull requests. No R2 access required.
- `.github/workflows/build.yml`: on main source changes or manual dispatch, runs tests, compiles and validates the 1.0.3 ZIP, then commits only the self-authored DLL and ZIP to the repo and publishes a versioned GitHub Release asset. No R2 access required.

Local development:

```sh
dotnet run --project Tests/CsvSmokeTest.csproj -c Release
dotnet run --project Tests/Gui/GuiRegression.csproj -c Release
dotnet run --project Tests/Exporter/ExporterRegression.csproj -c Release
dotnet build Source/DPA.CsvExporter.csproj -c Release
```

The compiler restores the Ludeon-authorized, signature-only `Krafs.Rimworld.Ref` 1.7.4985 NuGet package. The actual game binaries are **not** tracked or packaged here.

The original v1.0.0 build came from a prior CI run in a separate repository. Current source and v1.0.3 builds use no game files or R2 credentials.

## Review fixes and release boundaries

- The CSV exporter serializes complete current-entry measurement rows using `CsvReport.Write`, and writes atomically via `AtomicFile`.
- Tests cover precision, a multi-row 29-column CSV report and failed file writes, and pass in the independent .NET 10 GitHub Actions job.
- The v1.0.3 `dist/` ZIP is published by the main-branch build workflow after a .NET compiler success. This does not prove that the addon UI functions in a running game.
- Rebuilding uses publicly available metadata-only NuGet references. Do not commit or upload proprietary game DLLs.

## 1.0.2 input fix

The export action now occupies a reserved 48-pixel bottom toolbar. A Harmony prefix draws and processes the button before the original window, which receives a smaller content rectangle. Tick/Update entries, graphs and their drag handles no longer sit underneath the export button. The previous postfix could lose the click to an invisible entry button before export ran. The addon settings export remains available.

Regression tests reproduce the old failure and cover entry retention, navigation, repaint, window sizes and nonzero origins. They exercise the actual patch with Harmony using test-only UI fixtures; they do not run Unity or prove in-game rendering.

## 1.0.3 CSV integrity fixes

Required public ProfileLog fields and their types are validated once per runtime type before creating a file. An incompatible DPA model produces a named compatibility error and no CSV; genuine measured zeros are retained. Missing optional row metadata (method/type/labels, spike metadata, phase and pin state) is blank rather than a fabricated zero or false. Present optional fields with an incompatible type are rejected as well.

The 29 column names and order are unchanged. `measurement_cycles` now comes from each published row's `entries`, matching `sample_entries`, rather than the live counter which can be ahead of that batch. **`root_average_ms` is empty/unknown in 1.0.3**: the supplied DPA 1.7 API does not publish the root average belonging to its completed Logs batch. The exporter neither substitutes a live value nor infers an approximate root from the rounded fraction. `fraction_of_root` remains the actual published DPA value. FPS/TPS, TPS target and search filter are live context at export, not synchronized batch metadata.

Tests execute the production Exporter with DPA-compatible models: missing or wrong-typed required fields, unknown optional metadata, real zero values, retained DPA 1.7 measurements, and a deterministically blocked asynchronous publication whose live counters have already advanced. This verifies the export logic without running the game.
