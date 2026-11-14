[English](../README.md) | **简体中文** | [日本語](README_ja.md)

# PeakStatsHUD

一个 **只显示耐力** 的 **PEAK** HUD Mod：在本地玩家和队友的状态条边缘绘制耐力、额外耐力和
负面状态（affliction）数值。

> **2.0.0 是只保留耐力条的版本。** 所有本局统计与地图信息均已移除：不再显示计时、高度、
> 关卡、群系、昼夜、雾气、上升危害和地图种子。队友耐力条、名字、物品栏与背包槽位保留。
> 配置分为 **Stats**、**Team** 和 **Debug** 三个页面。

PeakStatsHUD 基于两个已有的 PEAK Mod 构建：

- **[PeakStatsEx 1.5.4](https://codeberg.org/yls-peak-mods/PeakStatsEx)**，作者 **LengSword** —
  队友耐力条系统。
- **[StaminaStatsTactikot 1.3.7](https://thunderstore.io/c/peak/p/TactiKot/StaminaStatsTactikot/)**，
  作者 **TactiKot**（其本身基于 **pixx** 的 **StaminaStats**）— 边缘标签布局、HUD 字体/配色
  处理，以及负面状态标签行。

两个上游项目均为 MIT 许可。原始包保留在 [`references/`](../references) 目录下，用于署名与
对比；详见 [`LICENSE`](../LICENSE)。

> **AI 生成：** 本 Mod —— 包括代码改动、布局工作和文档 —— 均在 **DeepSeek-V4,1-Flash** 的
> AI 辅助下生成。

## 功能

### 边缘式数值

- 耐力与额外耐力数值绘制在状态条左端上方。
- 负面状态数值绘制在其自身彩色分段的下边缘。
- 相邻的负面状态数值最多分三行排布，避免重叠。
- 标签使用 PEAK 的 HUD 字体（粗体），配色方式与 StaminaStatsTactikot 一致：颜色取自状态条或
  负面状态图标，略微偏移色相、提亮，并向白色混合以提升可读性。
- 数值绘制在状态条图形之上，因此额外耐力条上的石化（petrify）数值不再被额外条边框遮挡。

### 饥饿倒计时

- 本地饥饿倒计时显示在耐力与额外耐力数值右侧，例如 `32 +25 (20s)`。
- 格式始终包含括号和秒数单位。
- 拥有独立的字体大小选项，且可单独关闭。

### 队友

- 队友耐力条，支持距离过滤、数量上限和缩放。
- 队友的额外耐力条与负面状态指示。
- 显示数值时，队友名字会自动移到数值标签上方，并可额外配置垂直偏移。
- 可通过一个选项关闭队友条上的所有数值，同时保留状态条和名字。
- 队友物品栏与背包槽位仍显示在耐力条旁。

### 2.0.0 移除的内容

- 本局计时、攀爬高度、关卡名称、群系及群系变体。
- 昼夜倒计时、雾气统计、上升危害统计。
- 地图种子显示，以及为支持它而引入的 PLL / Terrain Randomiser / Terrain Customiser /
  True Final Ascent 兼容代码。
- `General` 与 `StaminaInfo` 配置页面；其中的选项现在位于 `Stats` 和 `Team` 页面。

队友耐力条组通过 prefix 与延迟引导初始化，因此其他 Mod 在 `GUIManager.Start` 中抛异常
（例如 TFA 的 FINAL ASCENT + Apocalypse 组合下的 PeakAMap）不会再导致队友条缺失。

## 环境要求

- PEAK（Steam，Windows x64）
- [BepInExPack_PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/) 5.4.75301

## 安装

1. 安装 BepInExPack_PEAK。
2. 移除或备份 `BepInEx/plugins/` 中已有的 `com.github.LengSword.PeakStatsEx.dll`。
   PeakStatsHUD 使用自己的插件 ID（`com.github.phosicX.PeakStatsHUD`）；与 PeakStatsEx
   同时运行会导致 HUD 数值被绘制两次。
3. 将发布包中的 `plugins/com.github.phosicX.PeakStatsHUD.dll` 复制到 `BepInEx/plugins/`。
4. 启动游戏。设置会写入 `BepInEx/config/com.github.phosicX.PeakStatsHUD.cfg`。

请勿同时安装 PeakStatsHUD 和 PeakStatsEx。

## 配置

所有选项均可在配置文件中实时修改（BepInEx 在保存时重新加载）。配置文件分为
`Stats`、`Team` 和 `Debug` 三个页面。

### Stats

| 选项 | 默认值 | 说明 |
|---|---|---|
| Show Stamina Info | `true` | 所有耐力数值的总开关。 |
| Bar Font Size | `18` | 耐力与额外耐力标签的字体大小。 |
| Affliction Font Size | `16` | 负面状态标签的字体大小。 |
| Bar Outline Width | `0.1` | 标签的描边宽度。 |
| Round Stamina Bars | `true` | 对耐力与额外耐力数值取整。 |
| Round Affliction Bars | `false` | 对负面状态数值取整。 |
| Show Affliction Countdown | `true` | 为已知衰减时间的负面状态追加倒计时。 |
| Show Hunger Countdown | `true` | 在耐力数值旁显示本地饥饿倒计时。 |
| Hunger Countdown Font Size | `18` | 饥饿倒计时的字体大小。 |
| Show Bonus Stamina Label | `true` | 以 `+` 前缀显示额外耐力数值。 |
| Show Percentage Sign | `false` | 在耐力、额外耐力与负面状态数值后追加 `%`。 |
| Stamina Label Vertical Offset | `25` | 耐力标签相对条中心的垂直偏移。 |
| Affliction Label Vertical Offset | `-15` | 负面状态标签相对条中心的垂直偏移。 |
| Affliction Label Rows | `2` | 为避免重叠使用的最大标签行数（1-3）。 |

### Team

| 选项 | 默认值 | 说明 |
|---|---|---|
| Display Teammate Stamina Bars | `true` | 在左下角显示队友耐力条。 |
| Teammate Stamina Bar Proximity | `30` | 显示队友条的最大距离。 |
| Teammate Stamina Bar Limit | `4` | 队友条的最大数量。 |
| Teammate Stamina Bar Scale (Default: 0.72) | `0.72` | 队友条及其间距的缩放。 |
| Show Inventory Slots | `true` | 在耐力条旁显示队友物品栏与背包槽位。 |
| Show Teammate Stamina Values | `true` | 在队友条上显示数值。关闭后仅保留条和名字。 |
| Teammate Name Vertical Offset | `0` | 在数值上方自动间距之外，队友名字的额外偏移。 |

### Debug

| 选项 | 默认值 | 说明 |
|---|---|---|
| Display Self Stamina Bar | `false` | 通过队友条渲染器显示自己的耐力条。 |
| Self Stamina Bar Count | `1` | 启用上一项时，自身条的复制数量。 |

## 从源码构建

PeakStatsHUD 是一个 .NET Standard 2.1 的 BepInEx 插件。仓库在 `global.json` 中固定了
.NET SDK 10.0.100。

1. 安装 [.NET SDK](https://dotnet.microsoft.com/download)（10.0 或更新）。
2. 安装 PEAK 与 BepInExPack_PEAK。
3. 构建：

   ```powershell
   dotnet build src/PeakStatsHUD/PeakStatsHUD.csproj -c Release `
     -p:PEAKGameRootDir="C:\Program Files (x86)\Steam\steamapps\common\PEAK\" `
     -p:RunThunderPipePackAfterBuild=false
   ```

   生成的 DLL 位于
   `artifacts/bin/PeakStatsHUD/release/com.github.phosicX.PeakStatsHUD.dll`。

项目不再引用 Terrain Randomiser、Terrain Customiser 或 PLL Library。

## 发布包内容

`release/phosicX-PeakStatsHUD-2.0.0.zip` 为 Thunderstore 格式：

```
manifest.json
README.md
CHANGELOG.md
LICENSE
icon.png
plugins/com.github.phosicX.PeakStatsHUD.dll
```

## 致谢

| 项目 | 作者 | 许可 |
|---|---|---|
| PeakStatsEx 1.5.4 | LengSword | MIT |
| StaminaStatsTactikot 1.3.7 | TactiKot | MIT |
| StaminaStats（Tactikot 的上游） | pixx | MIT |
| PeakStatsHUD | phosicX | MIT |
| AI 辅助 | DeepSeek-V4,1-Flash | — |

感谢三位作者公开他们的工作。PeakStatsHUD 的代码改动与文档在 DeepSeek-V4,1-Flash 的 AI
辅助下生成。

## 许可

MIT —— 详见 [`LICENSE`](../LICENSE)。
