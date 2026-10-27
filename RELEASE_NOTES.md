## 追加機能

- CSVと同じ名前の環境情報JSONを同時保存します。CSVと同じUTC時刻・ゲームビルド、Exporter／DPAのアセンブリバージョン、DPAのMODバージョン、有効MODの名前・PackageId・MODバージョン・読み込み順を記録します。取得不能なメタデータはnullです。
- **Open folder**（保存先を開く）、**Copy path**（保存先パスをコピー）をMOD設定画面とDPAの専用下部領域に追加しました。幅が狭いDPAウィンドウではMOD設定から利用できます。
- CSVの29列は維持します。環境情報の取得はExport時のみで、常時監視は追加していません。通常の保存失敗時は新しく作った出力を取り消し、既存の出力は保持します。

## インストール

**DPA-CSV-Exporter-1.0.4-RW1.7.zip** を解凍し、DPA-CSV-ExporterフォルダーでMods内の旧版を置き換えてください。RimWorld 1.7向け。Harmony → Dubs Performance Analyzer → DPA CSV Exporterの順に読み込みます。

## 検証

実Exporterの23ケース、CSV・ペア保存の失敗／既存ファイル保護テスト、実HarmonyでのGUI・保存先操作の振り分け・エラー通知・狭い画面のテスト、RimRefビルドを実施しました。OSフォルダー操作・クリップボードはテスト用境界で検証し、実ゲーム上の操作確認は未実施です。
