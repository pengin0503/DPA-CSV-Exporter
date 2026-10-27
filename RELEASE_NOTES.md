## 修正内容

- Export CSVを専用の下部ツールバーへ配置。Tick／Updateなどの項目と重ならないよう表示領域を確保しました。
- ボタンをDPA本体より先に処理し、背面の項目がクリックを奪ったり、エクスポート前に計測対象が切り替わったりする問題を修正しました。
- MOD設定画面からのエクスポートも引き続き利用できます。

## インストール

1. Assetsの **DPA-CSV-Exporter-1.0.2-RW1.7.zip** をダウンロードしてください（Source codeではありません）。
2. 解凍した `DPA-CSV-Exporter` フォルダーをRimWorldの `Mods` に配置し、旧版は置き換えてください。
3. Harmony → Dubs Performance Analyzer → DPA CSV Exporter の順に読み込んでください。対応ゲームはRimWorld 1.7です。

ZIPにはExporter自身のDLL・About.xml・READMEだけを含み、DPA本体やゲームDLLは同梱しません。

## 検証

修正前にクリック横取りのテスト失敗を確認し、修正後に実Harmonyパッチを通した回帰テスト・既存CSVテスト・RimRefによるビルド・ZIP内DLLのハッシュ検証を実施。GUIテストはテスト用UIで行い、実ゲーム上の操作確認は未実施です。
