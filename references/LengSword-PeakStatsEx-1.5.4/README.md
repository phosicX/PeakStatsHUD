# PeakStatsEx

A mod for PEAK that enhances cooperative gameplay by displaying information about your teammates.

Combine `PeakStats`, `FriendsInfo` and `StaminaInfo`.

![staminabars.jpg](https://tg-image-4ja.pages.dev/file/AgACAgUAAyEGAAMBCNy-6QADUWqTINCnxum5ftvgpwf3KuchR7DjAAKeEGsb1cOYVFXfcX2dkzd2AQADAgADeAADPQQ.jpg)

![timerheightstats.png](https://tg-image-4ja.pages.dev/file/AgACAgUAAyEGAAMBCNy-6QADBmp7UC9ExxOPVNSD_Dp40UkuOrWTAAJ6E2sb-vjZVxN2g5D5eICOAQADAgADeAADPQQ.png)

![mapstats.jpg](https://tg-image-4ja.pages.dev/file/AgACAgUAAyEGAAMBCNy-6QADVWqWvax4-RJfemWS_TsjctTII_wHAAJ9FGsbJkOwVLNb7WsxB2dBAQADAgADeAADPQQ.jpg)

![mapstats2.jpg](https://tg-image-4ja.pages.dev/file/AgACAgUAAyEGAAMBCNy-6QADVmqWvaz1MBNZdSgvGKkfIENhE4gUAAJ-FGsbJkOwVIiXtq-3Llr1AQADAgADeAADPQQ.jpg)

## Features

- Teammate stamina bars
  - Displays stamina bars for nearby teammates
  - Supports proximity-based visibility
  - Supports a configurable bar limit and scale

- Extra stamina information
  - Shows extra stamina bars for teammates
  - Displays affliction-related status indicators
  - Optionally shows stamina info text such as current stamina, extra stamina, and affliction bar values

- Teammate inventory and backpack visibility
  - Shows teammate inventory slot icons
  - Shows backpack-related slot information when available
  - Supports optional item name display behavior via configuration

- Run and map statistics
  - Displays elapsed run timer
  - Displays climbed height in meters
  - Displays current level name
  - Displays current biomes
  - Displays the current biome variant on a separate HUD line when available
  - Displays day/night countdown
  - Displays fog boundary height, arrival countdown, and movement speed
  - Displays rising hazard height, arrival countdown, and movement speed for Lava, Gloom, and Void Ghosts

- Compatibility support
  - Detects the map seed from Terrain Randomiser, Terrain Customiser, or PLL Library when exactly one is loaded
  - Displays True Final Ascent's actual synchronized map seed when its mode is active
  - Uses PLL Library's actual slot order, source identities for native biome names, and progress titles for custom biomes, including repeated biomes and explicitly placed Nadir
  - Displays multiple active variants in custom PLL biomes
  - Uses the actual PLL Peak campfire and the next progress point to determine the higher target
  - Uses True Final Ascent's biome stages for its route, current-stage highlight, and active biome variant
  - Works automatically when local characters are registered

## Configuration

The mod creates a configuration file with the following options:

### General

- Display Teammate Stamina Bars
  - Default: true
  - Enables or disables teammate stamina bar display

- Teammate Stamina Bar Proximity
  - Default: 30.0
  - Maximum distance at which teammate stamina bars are shown

- Teammate Stamina Bar Limit
  - Default: 4
  - Maximum number of stamina bars displayed at once

- Teammate Stamina Bar Scale (Default: 0.72)
  - Default: 0.72
  - Controls the size of teammate stamina bars and automatically adjusts related vertical spacing

- Show Inventory Slots
  - Default: true
  - Shows teammate inventory slot UI

- Show Stamina Info
  - Default: true
  - Shows stamina-related numeric info when stamina bars are visible

### Stats

- Display Timer
  - Default: true
  - Shows how long you have been climbing

- Display Height
  - Default: true
  - Shows your current climbed height in meters

- Display Level
  - Default: true
  - Shows the current level name

- Display Biomes
- Default: true
- Shows the current map biomes and the active biome variant when available

- Display Day Night Countdown
  - Default: true
  - Shows current day/night countdown information

- Display Fog Stats
  - Default: true
  - Shows fog-related statistics when relevant

- Display Lava Stats
  - Default: true
  - Shows rising hazard height, arrival countdown, and movement speed for Lava, Gloom, and Void Ghosts when relevant

- Display Map Seed
  - Default: true
  - Shows the current map seed from Terrain Randomiser, Terrain Customiser, PLL Library, or active True Final Ascent when supported

### StaminaInfo

- Bar Font Size
  - Default: 20.0
  - Adjusts the font size of stamina info text

- Bar Outline Width
  - Default: 0.08
  - Adjusts the outline width of stamina info text

- Round Stamina Bars
  - Default: true
  - Rounds stamina and extra stamina bar values to whole numbers

- Round Affliction Bars
  - Default: false
  - Rounds affliction bar values to whole numbers

- Show Affliction Countdown
  - Default: true
  - Shows affliction countdown values when enabled

- Show Hunger Countdown
  - Default: true
  - Shows local hunger countdown values in stamina info

- Show Teammate Extra Stamina Outside Bar
  - Default: false
  - Shows teammate extra stamina values outside the right side of the full extra stamina bar with a `+` prefix

### Debug

- Display Self Stamina Bar
  - Default: false
  - Displays your own stamina bar for debugging or personal tracking

- Self Stamina Bar Count
  - Default: 1
  - Controls how many duplicate self stamina bars are shown when Display Self Stamina Bar is enabled

## Installation

1. Install BepInEx
2. Place `com.github.LengSword.PeakStatsEx.dll` into the `BepInEx/plugins/` folder
3. Launch the game and configure settings via the config file

## Compatibility

PLL compatibility is based on the 5.9.4b runtime contracts. Routes retain released slots and use actual progress-point ownership rather than inferred biome groups. Rising hazards are resolved within the current occurrence, so repeated biomes use their own Lava, Gloom, or Void Ghosts. Custom biome variants show their actual active selections together. Peak target heights use the higher of the actual campfire and the next progress point; Nadir uses its current portal. Information is hidden while PLL is preparing or transitioning, and an incompatible interface disables only the affected feature. Older PLL versions have not been verified in game.

Terrain Randomiser, Terrain Customiser, and PLL Library conflict with each other and must not be enabled together. When True Final Ascent is inactive, PeakStatsEx displays a map seed only when exactly one of these three mods is loaded.

When True Final Ascent is active, its actual synchronized map seed takes priority, including when Terrain Randomiser remains loaded but its map generation has been disabled by True Final Ascent. The seed is hidden while initialization is incomplete. This display priority does not imply that True Final Ascent can generate maps alongside Terrain Customiser or PLL Library.

True Final Ascent biome routes and active variants are supported. Seed and route compatibility are validated together: if either interface is incompatible or a completed route cannot be resolved, PeakStatsEx stops using both TFA sources, hides the seed, falls back to its original biome collection, and logs one warning. That fallback may not show all internal stages correctly. Normal initialization waits are retried rather than treated as incompatibility.

Other features activate automatically when local characters are registered.
