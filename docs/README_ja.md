[English](../README.md) | [简体中文](README_zh.md) | **日本語**

# PeakStatsHUD

**PEAK** 用の**スタミナ表示のみ**に絞った HUD Mod です。ローカルプレイヤーとチームメイトの
バーの端に、スタミナ、ボーナススタミナ、状態異常（affliction）の数値を描画します。

> **2.0.0 はスタミナ表示のみのフォークです。** ランの統計とマップ情報はすべて削除しました。
> タイマー、高度、レベル、バイオーム、昼夜、霧、上昇ハザード、マップシードは表示しません。
> チームメイトのスタミナバー、名前、インベントリとバックパックスロットは残しています。
> 設定ページは **Stats**、**Team**、**Debug** の 3 つです。

PeakStatsHUD は、既存の 2 つの PEAK Mod をベースにしています。

- **[PeakStatsEx 1.5.4](https://codeberg.org/yls-peak-mods/PeakStatsEx)**（作者 **LengSword**）—
  チームメイトのスタミナバー機構。
- **[StaminaStatsTactikot 1.3.7](https://thunderstore.io/c/peak/p/TactiKot/StaminaStatsTactikot/)**
  （作者 **TactiKot**、**pixx** の **StaminaStats** をベース）— 端ラベルのレイアウト、
  HUD のフォント/配色処理、状態異常ラベルの行。

どちらの上流プロジェクトも MIT ライセンスです。元のパッケージは帰属表示と比較のため
[`references/`](../references) に保存しています。詳細は [`LICENSE`](../LICENSE) を参照してください。

> **AI 生成：** 本 Mod（コード変更、レイアウト調整、ドキュメント）は、**DeepSeek-V4,1-Flash** による
> AI 支援で生成されました。

## 機能

### 端表示の数値

- スタミナとボーナススタミナの数値は、バーの左端のすぐ上に表示されます。
- 状態異常の数値は、それぞれの色付きセグメントの下端に表示されます。
- 隣接する状態異常の数値は、重ならないように最大 3 行に分けて配置されます。
- ラベルは PEAK の HUD フォント（太字）を使用し、StaminaStatsTactikot と同じ配色処理を行います。
  色はバー/状態異常アイコンから取得し、色相を少しずらして明るくし、白に混ぜて読みやすくしています。
- 数値はバーのグラフィックより上に描画されるため、追加スタミナバーの石化（petrify）数値が
  追加バーの枠に隠れることはありません。

### 空腹カウントダウン

- ローカルの空腹カウントダウンは、スタミナとボーナススタミナの数値の右側に表示されます
  （例：`32 +25 (20s)`）。
- 形式は常に括弧と秒単位を含みます。
- 専用のフォントサイズ設定があり、個別にオフにできます。

### チームメイト

- 距離フィルター、本数上限、スケール設定を備えたチームメイトのスタミナバー。
- チームメイトの追加スタミナバーと状態異常インジケーター。
- 数値表示時はチームメイト名が自動で数値ラベルの上に移動し、追加の垂直オフセットも設定できます。
- チームメイトバーの数値をまとめて非表示にし、バーと名前だけを残すこともできます。
- チームメイトのインベントリとバックパックスロットは引き続きスタミナバーの横に表示されます。

### 2.0.0 で削除したもの

- ランのタイマー、登攀高度、レベル名、バイオームとバイオームバリアント。
- 昼夜カウントダウン、霧の統計、上昇ハザードの統計。
- マップシード表示と、それを支えていた PLL / Terrain Randomiser / Terrain Customiser /
  True Final Ascent の互換コード。
- `General` と `StaminaInfo` の設定ページ（項目は `Stats` と `Team` ページに移動）。

チームメイトのスタミナバーグループは prefix と遅延ブートストラップで初期化されるため、
他の Mod が `GUIManager.Start` で例外を投げても（例：TFA の FINAL ASCENT + Apocalypse に
おける PeakAMap）、チームメイトバーが欠けることはありません。

## 必要条件

- PEAK（Steam、Windows x64）
- [BepInExPack_PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/) 5.4.75301

## インストール

1. BepInExPack_PEAK をインストールします。
2. `BepInEx/plugins/` にある既存の `com.github.LengSword.PeakStatsEx.dll` を削除または
   バックアップします。PeakStatsHUD は独自のプラグイン ID
   （`com.github.phosicX.PeakStatsHUD`）を使用します。PeakStatsEx と同時に動かすと数値が
   二重に描画されます。
3. リリースアーカイブの `plugins/com.github.phosicX.PeakStatsHUD.dll` を
   `BepInEx/plugins/` にコピーします。
4. ゲームを起動します。設定は
   `BepInEx/config/com.github.phosicX.PeakStatsHUD.cfg` に保存されます。

PeakStatsHUD と PeakStatsEx を同時にインストールしないでください。

## 設定

すべての項目は設定ファイルで即時変更できます（BepInEx が保存時に再読み込みします）。
ページは `Stats`、`Team`、`Debug` の 3 つです。

### Stats

| 項目 | 既定値 | 説明 |
|---|---|---|
| Show Stamina Info | `true` | すべてのスタミナ数値のマスタースイッチ。 |
| Bar Font Size | `18` | スタミナとボーナススタミナのラベルのフォントサイズ。 |
| Affliction Font Size | `16` | 状態異常ラベルのフォントサイズ。 |
| Bar Outline Width | `0.1` | ラベルのアウトライン幅。 |
| Round Stamina Bars | `true` | スタミナとボーナススタミナの数値を丸めます。 |
| Round Affliction Bars | `false` | 状態異常の数値を丸めます。 |
| Show Affliction Countdown | `true` | 減少時間が分かる状態異常にカウントダウンを追加します。 |
| Show Hunger Countdown | `true` | スタミナ数値の横にローカルの空腹カウントダウンを表示します。 |
| Hunger Countdown Font Size | `18` | 空腹カウントダウンのフォントサイズ。 |
| Show Bonus Stamina Label | `true` | ボーナススタミナを `+` 付きで表示します。 |
| Show Percentage Sign | `false` | スタミナ、ボーナススタミナ、状態異常の数値に `%` を追加します。 |
| Stamina Label Vertical Offset | `25` | バー中心からのスタミナラベルの垂直オフセット。 |
| Affliction Label Vertical Offset | `-15` | バー中心からの状態異常ラベルの垂直オフセット。 |
| Affliction Label Rows | `2` | 重なりを避けるための最大ラベル行数（1〜3）。 |

### Team

| 項目 | 既定値 | 説明 |
|---|---|---|
| Display Teammate Stamina Bars | `true` | 画面左下にチームメイトのスタミナバーを表示します。 |
| Teammate Stamina Bar Proximity | `30` | チームメイトバーを表示する最大距離。 |
| Teammate Stamina Bar Limit | `4` | チームメイトバーの最大本数。 |
| Teammate Stamina Bar Scale (Default: 0.72) | `0.72` | チームメイトバーと間隔のスケール。 |
| Show Inventory Slots | `true` | スタミナバーの横にチームメイトのインベントリとバックパックスロットを表示します。 |
| Show Teammate Stamina Values | `true` | チームメイトバーに数値を表示します。オフにするとバーと名前だけ残ります。 |
| Teammate Name Vertical Offset | `0` | 数値の上への自動間隔に加える、チームメイト名の追加オフセット。 |

### Debug

| 項目 | 既定値 | 説明 |
|---|---|---|
| Display Self Stamina Bar | `false` | チームメイトバーのレンダラーで自分のスタミナバーを表示します。 |
| Self Stamina Bar Count | `1` | 上の項目を有効にしたときの自分のバーの複製数。 |

## ソースからのビルド

PeakStatsHUD は .NET Standard 2.1 の BepInEx プラグインです。リポジトリの `global.json` は
.NET SDK 10.0.100 を指定しています。

1. [.NET SDK](https://dotnet.microsoft.com/download)（10.0 以降）をインストールします。
2. PEAK と BepInExPack_PEAK をインストールします。
3. ビルドします。

   ```powershell
   dotnet build src/PeakStatsHUD/PeakStatsHUD.csproj -c Release `
     -p:PEAKGameRootDir="C:\Program Files (x86)\Steam\steamapps\common\PEAK\" `
     -p:RunThunderPipePackAfterBuild=false
   ```

   DLL は
   `artifacts/bin/PeakStatsHUD/release/com.github.phosicX.PeakStatsHUD.dll` に出力されます。

Terrain Randomiser、Terrain Customiser、PLL Library への参照はなくなりました。

## リリース内容

`release/phosicX-PeakStatsHUD-2.0.0.zip` は Thunderstore 形式です。

```
manifest.json
README.md
CHANGELOG.md
LICENSE
icon.png
plugins/com.github.phosicX.PeakStatsHUD.dll
```

## クレジット

| プロジェクト | 作者 | ライセンス |
|---|---|---|
| PeakStatsEx 1.5.4 | LengSword | MIT |
| StaminaStatsTactikot 1.3.7 | TactiKot | MIT |
| StaminaStats（Tactikot の上流） | pixx | MIT |
| PeakStatsHUD | phosicX | MIT |
| AI 支援 | DeepSeek-V4,1-Flash | — |

3 人の作者に感謝します。PeakStatsHUD のコード変更とドキュメントは、DeepSeek-V4,1-Flash の
AI 支援で生成されました。

## ライセンス

MIT — [`LICENSE`](../LICENSE) を参照してください。
