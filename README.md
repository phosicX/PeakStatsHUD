# PeakStatsHUD

[English](README.md) | [简体中文](docs/README_zh.md) | [日本語](docs/README_ja.md)

A **stamina-only** HUD mod for **PEAK**. It draws stamina, bonus stamina and affliction numbers
at the edge of the HUD bars, for the local player and for teammates.

> **2.0.0 is a stamina-only fork.** All run statistics and map information were removed:
> no timer, height, level, biome, day/night, fog, hazard or map seed display. Teammate stamina
> bars, names, inventory slots and backpack slots are kept. The configuration has three pages:
> **Stats**, **Team** and **Debug**.

PeakStatsHUD is built on top of two existing PEAK mods:

- **[PeakStatsEx 1.5.4](https://codeberg.org/yls-peak-mods/PeakStatsEx)** by **LengSword** —
  the teammate stamina bar system.
- **[StaminaStatsTactikot 1.3.7](https://thunderstore.io/c/peak/p/TactiKot/StaminaStatsTactikot/)**
  by **TactiKot** (itself based on **StaminaStats** by **pixx**) — the edge-label layout, the HUD
  font/colour treatment and the affliction label rows.

Both upstream projects are MIT licensed. The original packages are kept under
[`references/`](references) for attribution and comparison; see [`LICENSE`](LICENSE).

> **AI-generated:** this mod — the code changes, layout work and documentation — was generated
> with AI assistance using **DeepSeek-V4,1-Flash**.

## Features

### Edge-style numbers

- Stamina and bonus stamina values are drawn just above the left end of the bar.
- Affliction values are drawn at the bottom edge of their own coloured segment.
- Nearby affliction values are arranged across up to three rows to avoid overlap.
- Labels use PEAK's HUD font in bold and the StaminaStatsTactikot colour treatment: the colour is
  taken from the bar/affliction icon, slightly hue-shifted, brightened and blended toward white.
- Values render above the bar graphics, so petrify values on the extra stamina bar are not covered
  by the extra bar frame.

### Hunger countdown

- The local hunger countdown is shown to the right of the stamina and bonus stamina values,
  for example `32 +25 (20s)`.
- The format always includes parentheses and a seconds unit.
- It has its own font size option and can be disabled independently.

### Teammates

- Teammate stamina bars with proximity filtering, a bar limit and a scale option.
- Extra stamina bars and affliction indicators for teammates.
- Teammate names automatically move above the value labels when values are shown, with an extra
  configurable offset.
- All numeric values on teammate bars can be turned off while keeping the bars and names.
- Teammate inventory and backpack slots are still shown next to the stamina bar.

### Removed in 2.0.0

- Run timer, climbed height, level name, biomes and biome variants.
- Day/night countdown, fog statistics and rising hazard statistics.
- Map seed display and the PLL / Terrain Randomiser / Terrain Customiser / True Final Ascent
  compatibility code that supported it.
- The `General` and `StaminaInfo` configuration pages; their options now live on the `Stats` and
  `Team` pages.

Stamina display is initialised through a prefix and a deferred bootstrap, so another mod throwing
inside `GUIManager.Start` (for example PeakAMap in TFA's FINAL ASCENT + Apocalypse mode) can no
longer prevent the teammate bar group from appearing.

## Requirements

- PEAK (Steam, Windows x64)
- [BepInExPack_PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/) 5.4.75301

## Installation

1. Install BepInExPack_PEAK.
2. Remove or back up an existing `com.github.LengSword.PeakStatsEx.dll` in `BepInEx/plugins/`.
   PeakStatsHUD uses its own plugin ID (`com.github.phosicX.PeakStatsHUD`); running it together
   with PeakStatsEx would draw the HUD numbers twice.
3. Copy `plugins/com.github.phosicX.PeakStatsHUD.dll` from the release archive into
   `BepInEx/plugins/`.
4. Launch the game. Settings are written to
   `BepInEx/config/com.github.phosicX.PeakStatsHUD.cfg`.

Do not install PeakStatsHUD and PeakStatsEx at the same time.

## Configuration

All options are live-editable in the config file (BepInEx reloads it on save). The file has three
pages: `Stats`, `Team` and `Debug`.

### Stats

| Option | Default | Description |
|---|---|---|
| Show Stamina Info | `true` | Master switch for all stamina numbers. |
| Bar Font Size | `18` | Font size of the stamina and bonus stamina labels. |
| Affliction Font Size | `16` | Font size of the affliction labels. |
| Bar Outline Width | `0.1` | Outline width of the labels. |
| Round Stamina Bars | `true` | Rounds stamina and bonus stamina values. |
| Round Affliction Bars | `false` | Rounds affliction values. |
| Show Affliction Countdown | `true` | Appends a countdown to afflictions with a known reduction time. |
| Show Hunger Countdown | `true` | Shows the local hunger countdown next to the stamina values. |
| Hunger Countdown Font Size | `18` | Font size of the hunger countdown. |
| Show Bonus Stamina Label | `true` | Shows the bonus stamina value with a `+` prefix. |
| Show Percentage Sign | `false` | Appends `%` to stamina, bonus stamina and affliction values. |
| Stamina Label Vertical Offset | `25` | Vertical offset of the stamina labels from the bar centre. |
| Affliction Label Vertical Offset | `-15` | Vertical offset of the affliction labels from the bar centre. |
| Affliction Label Rows | `2` | Maximum label rows used to avoid overlap (1-3). |

### Team

| Option | Default | Description |
|---|---|---|
| Display Teammate Stamina Bars | `true` | Shows teammate stamina bars at the bottom left. |
| Teammate Stamina Bar Proximity | `30` | Maximum distance at which teammate bars are shown. |
| Teammate Stamina Bar Limit | `4` | Maximum number of teammate bars. |
| Teammate Stamina Bar Scale (Default: 0.72) | `0.72` | Scale of teammate bars and spacing. |
| Show Inventory Slots | `true` | Shows teammate inventory and backpack slots next to the stamina bar. |
| Show Teammate Stamina Values | `true` | Shows numbers on teammate bars. Turn off to keep only bars and names. |
| Teammate Name Vertical Offset | `0` | Extra offset for teammate names on top of the automatic spacing above the values. |

### Debug

| Option | Default | Description |
|---|---|---|
| Display Self Stamina Bar | `false` | Shows your own stamina bar through the teammate bar renderer. |
| Self Stamina Bar Count | `1` | Number of duplicate self bars when the option above is enabled. |

## Building from source

PeakStatsHUD is a .NET Standard 2.1 BepInEx plugin. The repository pins .NET SDK 10.0.100 in
`global.json`.

1. Install the [.NET SDK](https://dotnet.microsoft.com/download) (10.0 or newer).
2. Install PEAK and BepInExPack_PEAK.
3. Build:

   ```powershell
   dotnet build src/PeakStatsHUD/PeakStatsHUD.csproj -c Release `
     -p:PEAKGameRootDir="C:\Program Files (x86)\Steam\steamapps\common\PEAK\" `
     -p:RunThunderPipePackAfterBuild=false
   ```

   The DLL is written to `artifacts/bin/PeakStatsHUD/release/com.github.phosicX.PeakStatsHUD.dll`.

The project no longer references Terrain Randomiser, Terrain Customiser or PLL Library.

Optional build properties:

- `PEAKBepInExDir` — BepInEx directory used for compile-time references.
- `RunThunderPipePackAfterBuild=false` — skip Thunderstore packing.
- `DeployModFiles=true` — copy the built DLL into `PEAKBepInExDir/plugins/`.

## Release contents

`release/phosicX-PeakStatsHUD-2.0.0.zip` is a Thunderstore-style package:

```
manifest.json
README.md
CHANGELOG.md
LICENSE
icon.png
plugins/com.github.phosicX.PeakStatsHUD.dll
```

## Credits

| Project | Author | Licence |
|---|---|---|
| PeakStatsEx 1.5.4 | LengSword | MIT |
| StaminaStatsTactikot 1.3.7 | TactiKot | MIT |
| StaminaStats (upstream of Tactikot) | pixx | MIT |
| PeakStatsHUD | phosicX | MIT |
| AI assistance | DeepSeek-V4,1-Flash | — |

Thank you to all three authors for making their work available. The PeakStatsHUD code changes
and documentation were generated with AI assistance using DeepSeek-V4,1-Flash.

## Licence

MIT — see [LICENSE](LICENSE).
