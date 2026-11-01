# DPA CSV Exporter — RimWorld 1.7

An independent addon for **Dubs Performance Analyzer** that exports the **entire current profiling table** in one CSV file, including rows hidden by the Dubs search filter. It does not start extra profiling or modify game behavior.

**Status:** The source is built against the public RimRef 1.7.4985 signature package without Cloudflare R2 credentials. The checked-in DLL and ZIP are updated by the main-branch build workflow **only after successful compilation and packaging**. Windows in-game UI/CSV operation remains unverified.

## Installation

Download [DPA-CSV-Exporter-1.0.4-RW1.7.zip](https://github.com/pengin0503/DPA-CSV-Exporter/releases/download/v1.0.4/DPA-CSV-Exporter-1.0.4-RW1.7.zip) from the [v1.0.4 Release](https://github.com/pengin0503/DPA-CSV-Exporter/releases/tag/v1.0.4). Unzip and place its `DPA-CSV-Exporter` folder in the RimWorld `Mods` directory. Requires RimWorld 1.7, Harmony, and Dubs Performance Analyzer (load in that order before this addon).

1. Open Dubs Performance Analyzer.
2. Select a Tick, Update, GUI or other profiling entry and allow measurements to accumulate.
3. Press **Export CSV** in the dedicated bottom toolbar of the analyzer, or use the backup button in the addon settings.
4. Each export saves a CSV and a same-basename environment JSON. Both files are saved to `GenFilePaths.SaveDataFolderPath/AnalyzerExports`.

Normally on Windows this is `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\AnalyzerExports`.

Each CSV contains 29 columns: category, selected entry, FPS/TPS, measured call counts, average/self/max/total execution time, method/assembly and best-effort MOD attribution, etc. UTF-8 with BOM for Excel. The export saves **one selected profiling entry** at a time, not every category simultaneously.

## 1.0.4 environment and output-folder tools

Each explicit export produces `name.csv` and `name.json`. JSON schema version 1 contains the same UTC timestamp and game build as the CSV, exporter assembly version, DPA assembly version and declared mod version when available, and enabled mods with their name, runtime PackageId, declared mod version and engine load order. Missing version/owner metadata is null, rather than guessed. The 29 CSV columns remain unchanged. Keep both files together when sharing a result.

The two complete files are staged before publication; JSON is published first and CSV last. Ordinary write failures roll back only files created by that call, preserving pre-existing results. This is not a cross-file transaction against power loss. Environment metadata is read from loaded mods on export only; there is no periodic collection or file hashing.

**Open folder** and **Copy path** appear in addon settings and in the reserved DPA footer when it has enough width. Narrow DPA windows retain Export CSV; use addon settings for folder actions. Open creates the output directory if necessary and asks the OS to open it; Copy uses the clipboard without launching a process. If the OS/clipboard is unavailable, the addon reports the failure in settings and a notification.

This release does not add multi-entry batch capture. DPA resets previous measurements when switching entries, so collecting several entries would require a separate capture workflow.

## 日本語

DPA CSV Exporterは、RimWorld 1.7のDubs Performance Analyzerで選択中の計測表全体をCSVに保存するアドオンです。検索で非表示の行も含みます。追加の計測開始やゲーム動作の変更は行いません。Windowsの実ゲームでの画面とCSV出力は未検証です。

1. [v1.0.4の配布ページ](https://github.com/pengin0503/DPA-CSV-Exporter/releases/tag/v1.0.4)から `DPA-CSV-Exporter-1.0.4-RW1.7.zip` を取得し、展開したフォルダを `RimWorld/Mods` に置きます。
2. Harmony、Dubs Performance Analyzer、このアドオンの順に有効化します。
3. DPAでTickやUpdateなどの項目を選び、計測後に下部の **Export CSV** を押します。設定画面の予備ボタンも使えます。
4. CSVと同名の環境情報JSONが `AnalyzerExports` に保存されます。共有時は両方を一緒に扱ってください。CSVは選択中の1項目について29列を出力します。複数項目の同時出力には対応していません。

設定画面には保存先を開く・パスをコピーする操作があります。計測値の0はそのまま保存され、不明な一部の値は空欄になります。FPS/TPSなどの表示は出力時点の情報です。
