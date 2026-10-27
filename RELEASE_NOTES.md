## 修正内容

- #14：必須計測フィールドの欠落・型変更を検出して保存を中止し、互換性エラーを表示します。正常な実測値0と、取得できない値を区別します。任意の行メタデータがない場合は空欄です。
- #15：measurement_cyclesは公開済み計測行の値を使い、先行するライブカウンターとの混在を解消しました。
- root_average_msは空欄（取得不能）に変更しました。添付DPA 1.7は公開済み計測行と同時点のroot平均を提供しないため、現在値や逆算値を確定値として出力しません。既存29列の名前・順序、fraction_of_rootおよび各計測値は維持します。
- v1.0.2のExport専用ツールバー修正を含みます。

## インストール

Assetsの **DPA-CSV-Exporter-1.0.3-RW1.7.zip** をダウンロードし、解凍した DPA-CSV-Exporter フォルダーで Mods 内の旧版を置き換えてください。RimWorld 1.7向け。読み込み順は Harmony → Dubs Performance Analyzer → DPA CSV Exporter です。

## 検証

実Exporterを使った22ケースの回帰テスト、既存CSVテスト、実HarmonyでのGUI回帰テスト、RimRefビルド、公開ZIP内DLLの照合を実施。修正前のExporter回帰テストは2/20（当初の20ケース）通過で、問題を検出しました。実ゲームでの操作確認は未実施です。ゲームDLL・DPA本体・Harmonyは同梱しません。
