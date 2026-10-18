# PeakStatsHUD

A HUD mod for **PEAK** that draws stamina, bonus stamina and affliction numbers at the edge of
the HUD bars, adds a separate hunger countdown, and keeps the teammate HUD readable.

PeakStatsHUD is built on top of two existing PEAK mods:

- **[PeakStatsEx 1.5.4](https://codeberg.org/yls-peak-mods/PeakStatsEx)** by **LengSword** —
  teammate stamina bars, extra stamina bars, inventory slots and the run/map statistics.
- **[StaminaStatsTactikot 1.3.7](https://thunderstore.io/c/peak/p/TactiKot/StaminaStatsTactikot/)**
  by **TactiKot** (itself based on **StaminaStats** by **pixx**) — the edge-label layout,
  the HUD font/colour treatment and the affliction label rows.

Both upstream projects are MIT licensed. The original packages are kept under
[`references/`](references) for attribution and comparison; see [`LICENSE`](LICENSE).

## Features

### Edge-style numbers (TactiKot layout)

- Stamina and bonus stamina values are drawn just above the left end of the bar.
- Affliction values are drawn at the bottom edge of their own coloured segment.
- Nearby affliction values are arranged across up to three rows to avoid overlap.
- Labels use PEAK's HUD font in bold and the same colour treatment as StaminaStatsTactikot:
  the colour is taken from the bar/affliction icon, slightly hue-shifted, brightened and
  blended toward white for readability.
- Values are rendered above the bar graphics, so petrify values on the extra stamina bar are
  no longer covered by the extra bar frame.

### Hunger countdown

- The local hunger countdown is shown to the right of the stamina and bonus stamina values,
  for example `32 +25 (20s)`.
- The format always includes parentheses and a seconds unit.
- It has its own font size option and can be disabled independently.

### Teammates

- Teammate names automatically move above the value labels when teammate values are shown,
  and an extra vertical offset can be configured.
- All numeric values on teammate bars can be turned off with one option while keeping the
  bars, names and inventory slots.

### Everything from PeakStatsEx

- Teammate stamina bars with proximity filtering, a bar limit and a scale option.
- Extra stamina bars and affliction indicators for teammates.
- Teammate inventory and backpack slot display.
- Run timer, climbed height, level name, biomes and active biome variants.
- Day/night countdown, fog statistics, rising hazard statistics (Lava, Gloom, Void Ghosts).
- Map seed display for Terrain Randomiser, Terrain Customiser, PLL Library and True Final Ascent
  when the corresponding interfaces are available.
- Compatibility handling for PLL, Terrain Randomiser, Terrain Customiser and True Final Ascent.

## Requirements

- PEAK (Steam, Windows x64)
- [BepInExPack_PEAK](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/) 5.4.75301

## Installation

1. Install BepInExPack_PEAK.
2. Remove or back up an existing `com.github.LengSword.PeakStatsEx.dll` in `BepInEx/plugins/`.
   PeakStatsHUD replaces PeakStatsEx and keeps the same plugin ID and config file.
3. Copy `plugins/com.github.LengSword.PeakStatsEx.dll` from the release archive into
   `BepInEx/plugins/`.
4. Launch the game. Settings are written to
   `BepInEx/config/com.github.LengSword.PeakStatsEx.cfg`.

Do not install PeakStatsHUD and PeakStatsEx at the same time.

## Configuration

All options are live-editable in the config file (BepInEx reloads it on save).

### General

| Option | Default | Description |
|---|---|---|
| Display Teammate Stamina Bars | `true` | Shows teammate stamina bars at the bottom left. |
| Teammate Stamina Bar Proximity | `30` | Maximum distance at which teammate bars are shown. |
| Teammate Stamina Bar Limit | `4` | Maximum number of teammate bars. |
| Teammate Stamina Bar Scale (Default: 0.72) | `0.72` | Scale of teammate bars and spacing. |
| Show Inventory Slots | `true` | Shows teammate inventory slots. |
| Show Stamina Info | `true` | Master switch for all stamina numbers. |
| Show Teammate Stamina Values | `true` | Shows numbers on teammate bars. Turn off to keep only bars, names and items. |
| Teammate Name Vertical Offset | `0` | Extra offset for teammate names on top of the automatic spacing above the values. |

### Stats

| Option | Default | Description |
|---|---|---|
| Display Timer | `true` | Shows the elapsed run timer. |
| Display Height | `true` | Shows the climbed height in metres. |
| Display Level | `true` | Shows the current level name. |
| Display Biomes | `true` | Shows the current biomes and active variant. |
| Display Day Night Countdown | `true` | Shows the day/night countdown. |
| Display Fog Stats | `true` | Shows fog boundary statistics. |
| Display Lava Stats | `true` | Shows rising hazard statistics. |
| Display Map Seed | `true` | Shows the map seed when a supported map mod is active. |

### StaminaInfo

| Option | Default | Description |
|---|---|---|
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
3. The project references these optional mods at compile time, so install them into
   `PEAK/BepInEx/plugins/` (or point `PEAKBepInExDir` to a folder that contains them):
   - `Snosz-TerrainRandomiser/TerrainRandomiser.dll`
   - `Snosz-TerrainCustomiser/TerrainCustomiser.dll`
   - `AeralisFoundation-PLL_Library/PLL_Library.dll`
4. Build:

   ```powershell
   dotnet build src/PeakStatsEx/PeakStatsEx.csproj -c Release `
     -p:PEAKGameRootDir="C:\Program Files (x86)\Steam\steamapps\common\PEAK\" `
     -p:RunThunderPipePackAfterBuild=false
   ```

   The DLL is written to `artifacts/bin/PeakStatsEx/release/com.github.LengSword.PeakStatsEx.dll`.

Optional build properties:

- `PEAKBepInExDir` — BepInEx directory used for the optional compile-time references.
- `RunThunderPipePackAfterBuild=false` — skip Thunderstore packing.
- `DeployModFiles=true` — copy the built DLL into `PEAKBepInExDir/plugins/`.

## Release contents

`release/PeakStatsHUD-1.0.0.zip` is a Thunderstore-style package:

```
manifest.json
README.md
CHANGELOG.md
LICENSE
icon.png
plugins/com.github.LengSword.PeakStatsEx.dll
```

The plugin file name and plugin ID intentionally stay the same as PeakStatsEx so the config
file and existing installs keep working.

## Credits

| Project | Author | Licence |
|---|---|---|
| PeakStatsEx 1.5.4 | LengSword | MIT |
| StaminaStatsTactikot 1.3.7 | TactiKot | MIT |
| StaminaStats (upstream of Tactikot) | pixx | MIT |
| PeakStatsHUD modifications | phosicX | MIT |

Thank you to all three authors for making their work available.

## Licence

MIT — see [LICENSE](LICENSE).
