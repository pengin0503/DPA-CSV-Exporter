# DPA CSV Exporter

**日本語** | [English](README.en.md)

RimWorld 1.7のDubs Performance Analyzer（DPA）で選択中の計測表をCSVに出力するアドオンです。DPAの検索フィルターで隠れた行も含みます。追加の計測開始やゲーム動作の変更は行いません。

## 導入と使い方

1. [v1.0.4 Release](https://github.com/pengin0503/DPA-CSV-Exporter/releases/tag/v1.0.4) から `DPA-CSV-Exporter-1.0.4-RW1.7.zip` を取得し、展開した `DPA-CSV-Exporter` をRimWorldの `Mods` フォルダに置きます。
2. RimWorld 1.7、Harmony、DPAが必要です。Harmony、DPA、このアドオンの順に有効化します。
3. DPAを開き、Tick、Update、GUIなどの項目を選んで計測を蓄積します。
4. DPA下部の **Export CSV**、またはアドオン設定の予備ボタンを押します。

CSVと同名の環境情報JSONは、RimWorldのユーザーデータフォルダ内 `AnalyzerExports` に保存されます。Windowsでは通常 `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\AnalyzerExports` です。設定から保存先を開く、またはパスをコピーできます。共有するときは両ファイルを一緒に扱ってください。JSONには有効なMOD名・PackageId・版・ロード順などが含まれます。

## 出力の意味と制限

CSVは選択中の1項目について、検索で隠れた行を含む全行を29列で出力します。複数項目の同時出力には対応していません。測定された0はそのまま保存し、不明な一部の値は空欄にします。FPS/TPSや検索条件は出力時の情報です。CSVはUTF-8 BOM付きです。

Windowsの実ゲームでの画面操作とCSV出力は未検証です。
