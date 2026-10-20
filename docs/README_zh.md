[English](../README.md) | **简体中文**

# PeakStatsHUD

一个 **PEAK** 的 HUD Mod：在 HUD 状态条边缘绘制耐力、额外耐力和负面状态（affliction）数值，
增加独立的饥饿倒计时，并保持队友 HUD 清晰可读。

PeakStatsHUD 基于两个已有的 PEAK Mod 构建：

- **[PeakStatsEx 1.5.4](https://codeberg.org/yls-peak-mods/PeakStatsEx)**，作者 **LengSword** —
  队友耐力条、额外耐力条、物品栏槽位，以及本局/地图统计数据。
- **[StaminaStatsTactikot 1.3.7](https://thunderstore.io/c/peak/p/TactiKot/StaminaStatsTactikot/)**，
  作者 **TactiKot**（其本身基于 **pixx** 的 **StaminaStats**）— 边缘标签布局、
  HUD 字体/配色处理，以及负面状态标签行。

两个上游项目均为 MIT 许可。原始包保留在
[`references/`](references) 目录下，用于署名与对比；详见 [`LICENSE`](LICENSE)。

> **AI 生成：** 本 Mod —— 包括代码改动、布局工作和文档 —— 均在 **DeepSeek-V4,1-Flash** 的
> AI 辅助下生成。

## 功能

### 边缘式数值（TactiKot 布局）

- 耐力与额外耐力数值绘制在状态条左端上方。
- 负面状态数值绘制在其自身彩色分段的下边缘。
- 相邻的负面状态数值最多分三行排布，避免重叠。
- 标签使用 PEAK 的 HUD 字体（粗体），配色方式与 StaminaStatsTactikot 一致：
  颜色取自状态条/负面状态图标，略微偏移色相、提亮，并向白色混合以提升可读性。
- 数值绘制在状态条图形之上，因此额外耐力条上的石化（petrify）数值不再被额外条边框遮挡。

### 饥饿倒计时

- 本地饥饿倒计时显示在耐力与额外耐力数值右侧，例如 `32 +25 (20s)`。
- 格式始终包含括号和秒数单位。
- 拥有独立的字体大小选项，且可单独关闭。

### 队友

- 显示队友数值时，队友名字会自动移到数值标签上方，并可额外配置垂直偏移。
- 可通过一个选项关闭队友条上的所有数值，同时保留状态条、名字和物品栏槽位。

### 继承自 PeakStatsEx 的全部功能

- 队友耐力条，支持距离过滤、数量上限和缩放选项。
- 队友的额外耐力条与负面状态指示。
- 队友物品栏与背包槽位显示。
- 本局计时、攀爬高度、关卡名称、生物群系及激活的群系变体。
- 昼夜倒计时、雾气统计、上升危害统计（岩浆、幽暗、虚空幽灵）。
- 在对应接口可用时，为 Terrain Randomiser、Terrain Customiser、PLL Library 和
  True Final Ascent 显示地图种子。
- 对 PLL、Terrain Randomiser、Terrain Customiser 和 True Final Ascent 的兼容处理。
- 健壮的 HUD 创建：计时/高度、地图/关卡和队友 HUD 通过前缀（prefix）与延迟引导
  （deferred bootstrap）创建，因此其他 Mod 在 `GUIManager.Start` 中抛异常也不会再阻止它们显示。
  这修复了 True Final Ascent 的 FINAL ASCENT + Apocalypse 组合下 HUD 缺失的问题
  （该组合下 PeakAMap 会在 `GUIManager.Start` 中抛异常）。

## 环境要求

- PEAK（Steam，Windows x64）
- [BepInExPack_PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/) 5.4.75301

## 安装

1. 安装 BepInExPack_PEAK。
2. 移除或备份 `BepInEx/plugins/` 中已有的 `com.github.LengSword.PeakStatsEx.dll`。
   PeakStatsHUD 使用自己的插件 ID（`com.github.phosicX.PeakStatsHUD`）；与 PeakStatsEx
   同时运行会导致 HUD 数值被绘制两次。
3. 将发布包中的 `plugins/com.github.phosicX.PeakStatsHUD.dll` 复制到
   `BepInEx/plugins/`。
4. 启动游戏。设置会写入
   `BepInEx/config/com.github.phosicX.PeakStatsHUD.cfg`。

如果你使用过 1.0.0 版本（插件 ID 为 `com.phosicX.PeakStatsHUD`），请将其配置文件重命名为
`com.github.phosicX.PeakStatsHUD.cfg` 以保留设置。

请勿同时安装 PeakStatsHUD 和 PeakStatsEx。

## 配置

所有选项均可在配置文件中实时修改（BepInEx 在保存时重新加载）。

### 通用（General）

| 选项 | 默认值 | 说明 |
|---|---|---|
| Display Teammate Stamina Bars | `true` | 在左下角显示队友耐力条。 |
| Teammate Stamina Bar Proximity | `30` | 显示队友条的最大距离。 |
| Teammate Stamina Bar Limit | `4` | 队友条的最大数量。 |
| Teammate Stamina Bar Scale (Default: 0.72) | `0.72` | 队友条及其间距的缩放。 |
| Show Inventory Slots | `true` | 显示队友物品栏槽位。 |
| Show Stamina Info | `true` | 所有耐力数值的总开关。 |
| Show Teammate Stamina Values | `true` | 在队友条上显示数值。关闭后仅保留条、名字和物品。 |
| Teammate Name Vertical Offset | `0` | 在数值上方自动间距之外，队友名字的额外偏移。 |

### 统计（Stats）

| 选项 | 默认值 | 说明 |
|---|---|---|
| Display Timer | `true` | 显示本局已用计时。 |
| Display Height | `true` | 显示攀爬高度（米）。 |
| Display Level | `true` | 显示当前关卡名称。 |
| Display Biomes | `true` | 显示当前生物群系及激活变体。 |
| Display Day Night Countdown | `true` | 显示昼夜倒计时。 |
| Display Fog Stats | `true` | 显示雾气边界统计。 |
| Display Lava Stats | `true` | 显示上升危害统计。 |
| Display Map Seed | `true` | 在支持的地图 Mod 激活时显示地图种子。 |

### 耐力信息（StaminaInfo）

| 选项 | 默认值 | 说明 |
|---|---|---|
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

### 调试（Debug）

| 选项 | 默认值 | 说明 |
|---|---|---|
| Display Self Stamina Bar | `false` | 通过队友条渲染器显示自己的耐力条。 |
| Self Stamina Bar Count | `1` | 启用上一项时，自身条的复制数量。 |

## 从源码构建

PeakStatsHUD 是一个 .NET Standard 2.1 的 BepInEx 插件。仓库在 `global.json` 中固定了
.NET SDK 10.0.100。

1. 安装 [.NET SDK](https://dotnet.microsoft.com/download)（10.0 或更新）。
2. 安装 PEAK 与 BepInExPack_PEAK。
3. 项目在编译时引用以下可选 Mod，请将它们安装到 `PEAK/BepInEx/plugins/`
   （或将 `PEAKBepInExDir` 指向包含它们的文件夹）：
   - `Snosz-TerrainRandomiser/TerrainRandomiser.dll`
   - `Snosz-TerrainCustomiser/TerrainCustomiser.dll`
   - `AeralisFoundation-PLL_Library/PLL_Library.dll`
4. 构建：

   ```powershell
   dotnet build src/PeakStatsHUD/PeakStatsHUD.csproj -c Release `
     -p:PEAKGameRootDir="C:\Program Files (x86)\Steam\steamapps\common\PEAK\" `
     -p:RunThunderPipePackAfterBuild=false