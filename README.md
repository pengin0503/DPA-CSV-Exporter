# DPA CSV Exporter — RimWorld 1.7

An independent addon for **Dubs Performance Analyzer** that exports the **entire current profiling table** in one CSV file, including rows hidden by the Dubs search filter. It does not start extra profiling or modify game behavior.

**Status:** The source includes newer review fixes than the checked-in `1.7/Assemblies/` DLL and `dist/` ZIP. **Those binary files still contain the original pre-review build** and must not be described as incorporating the new fixes. The full RimWorld 1.7 compilation and in-game button/CSV behavior of the updated source remain unverified. The independent .NET CSV regression tests pass.

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
- `.github/workflows/pr-checks.yml`: runs CSV regression tests for each PR. The full game-DLL compilation runs **only if this repository** has GitHub Actions repository secrets `R2_ACCESS_KEY_ID`, `R2_SECRET_ACCESS_KEY` and repository variable `R2_ENDPOINT`. Without them, compilation is explicitly skipped even when the other CI checks are green.
- `.github/workflows/build.yml`: optional, manually triggered build with the same private R2 references. No R2-heavy CI runs are needed for a CSV-only test.

Local development:

```sh
dotnet run --project Tests/CsvSmokeTest.csproj -c Release
dotnet build Source/DPA.CsvExporter.csproj -c Release -p:RimWorldRefs=/private/RimWorld1.7/refs
```

The compile references must be `Assembly-CSharp.dll`, `UnityStubs.dll`, and `Waylith.dll` from an authorized RimWorld 1.7 installation; these proprietary files are **not** tracked or packaged here.

Built from the [DPA CSV Exporter focused-build run](https://github.com/pengin0503/RimWorld-for-mod-test/actions/runs/40447166070). The unrelated test-only RimWorld host is not required to run this addon.

## Review fixes and release boundaries

- The CSV exporter serializes complete current-entry measurement rows using `CsvReport.Write`, and writes atomically via `AtomicFile`.
- Tests cover precision, a multi-row 29-column CSV report and failed file writes, and pass in the independent .NET 10 GitHub Actions job.
- Important: the `dist/` ZIP is from before these source changes. A green CSV test check is not proof that the game's addon is compiled or playable.
- Rebuilding requires `RimWorldRefs` to point to privately held, authorized RimWorld 1.7 references. Do not commit or upload those references.
