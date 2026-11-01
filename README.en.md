# DPA CSV Exporter

[日本語](README.md) | **English**

An addon for Dubs Performance Analyzer (DPA) on RimWorld 1.7 that exports the entire selected profiling table to CSV, including rows hidden by DPA's search filter. It does not start additional profiling or change game behavior.

## Installation and use

1. Download `DPA-CSV-Exporter-1.0.4-RW1.7.zip` from the [v1.0.4 release](https://github.com/pengin0503/DPA-CSV-Exporter/releases/tag/v1.0.4), extract it and place `DPA-CSV-Exporter` in RimWorld's `Mods` folder.
2. RimWorld 1.7, Harmony and DPA are required. Enable Harmony, DPA and then this addon.
3. Open DPA, select a Tick, Update, GUI or other entry, and let measurements accumulate.
4. Press **Export CSV** in DPA's bottom toolbar or use the backup button in addon settings.

The CSV and same-name environment JSON are saved in `AnalyzerExports` under RimWorld's user data folder. On Windows this is normally `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\AnalyzerExports`. Settings provide actions to open the folder or copy its path. Keep both files together when sharing; the JSON includes enabled mod names, PackageIds, versions and load order.

## Output and limitations

The 29-column CSV covers all rows of the selected entry, including rows hidden by search. It does not export multiple entries simultaneously. Measured zeros are preserved; some unknown values are blank. FPS/TPS and the search filter describe the time of export. The CSV uses UTF-8 with BOM.

Windows in-game UI and CSV operation have not been verified.
