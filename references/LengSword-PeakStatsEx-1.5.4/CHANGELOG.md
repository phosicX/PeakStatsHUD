# Changelog

## [1.5.4] - 2026-10-16

### Fixed

- Fixed incorrect native biome names in PLL routes when joining an ongoing game.
- Restored biome route and variant displays with PLL 5.9.10.

## [1.5.3] - 2026-10-11

### Added

- Added support for displaying multiple active variants in custom PLL biomes.

### Fixed

- Corrected hazard statistics for reordered and repeated PLL biomes, including lava in The Kiln.
- Fixed duplicated or missing PLL route entries and preserved the order of explicitly placed Nadir biomes.
- Corrected PLL Peak target heights using the actual campfire and the next progress point.
- Prevented stale map information during PLL initialization and segment transitions.

## [1.5.2] - 2026-09-30

### Added

- Added synchronized map seed display for TrueFinalAscent.
- Added TrueFinalAscent biome route support, including current-stage highlighting and active biome variant display.

## [1.5.1] - 2026-09-19

### Fixed

- Fixed duplicated or incomplete teammate stamina bars and repeated errors when the stamina HUD is temporarily hidden.

## [1.5.0] - 2026-09-15

### Added

- Added teammate stamina bar support to the Airport.

### Changed

- **Breaking:** Moved cloned teammate stamina bars from the original stamina layout group into a dedicated `PeakStatsEx Stamina Bars` container.

### Fixed

- Prevented teammate stamina bar scaling from changing the spacing of the original stamina HUD.
- Centered teammate extra-stamina values within their visible fills.
- Kept infinite-stamina readouts anchored within the visible stamina fill.
- Restored cooked colors and fuel bars for held teammate items, including while spectating.

## [1.4.4] - 2026-09-11

### Changed

- Optimize teammate bar display performance

## [1.4.3] - 2026-09-03

### Fixed

- Fixed a stamina HUD error that could occur briefly while switching spectator targets.
- Fixed target heights for PLL Peak Bridge and Peak-to-Nadir checkpoints.

## [1.4.2] - 2026-08-31

### Changed

- Added a separate HUD line that shows the full current biome variant beneath and right-aligned with its highlighted route entry.
- Added a small right margin to the level and biome route HUD text.

### Fixed

- Corrected biome route collection by grouping map segments that belong to the same biome occurrence.
- Corrected target heights to use the higher of the next progress point and campfire, including campfires generated later by PLL Library.
- Fixed PLL Library routes showing custom biomes as Shore, duplicating the final biome, or switching to the Peak before its progress point is reached.

## [1.4.1] - 2026-08-30

### Added

- Added new preview image

## [1.4.0] - 2026-08-30

### Changed

- Reworked the biome route display to use actual map segments below the vanilla ascent text, highlight the current route, show at most six nearby entries on one line, include the normal Peak endpoint, and reveal Void only after entering Nadir.
- Unified map seed display for Terrain Randomiser, Terrain Customiser, and PLL Library under one setting, and wait for map initialization before showing the seed.
- Updated the fallback target height to follow the next unreached progress point when no campfire is available.

### Fixed

- Hid fog statistics when the fog movement rate is zero.

## [1.3.1] - 2026-08-22

### Changed

- Refactor nadir portal position getting logic

## [1.3.0] - 2026-08-16

### Changed

- Display the final progress point height when no next campfire is available.
- Display the Peak Gate portal height as the target while in Nadir.

### Fixed

- Corrected non-host fog countdowns and speed multipliers using the host's synchronized fog movement.
- Corrected non-host rising-hazard countdowns and speed multipliers using synchronized Lava, Gloom, and Void Ghosts progress.
- Prevented rising-hazard regions from showing unrelated fog information before they start or after they end.

## [1.2.18] - 2026-08-12

### Changed

- Improve teammate petrify bar display for alignment with the extra stamina bar.

## [1.2.17] - 2026-08-12

### Added

- Added rising hazard height, countdown, and speed information for Gloom and Void Ghosts.

### Fixed

- Fixed height and hazard information failing to update correctly in the Void.

## [1.2.16] - 2026-08-12

### Fixed

- Fixed compatibility issues for 2.0.a
- Synced teammate stamina bars with the current vanilla stamina and petrify display rules.
- Fixed cloned stamina bars retaining morale boost text, and corrected teammate extra stamina bar alignment and height.

## [1.2.15] - 2026-08-12

### Changed

- Improve lava speed calculation logic. Now the lava speed is calculated based on the current lava height and the last recorded lava height, rather than relying solely on the lava travel time. This change ensures that the lava speed is accurately reflected in the countdown display, even when the travel time changes during gameplay.
- Improve fog countdown and speed multiplier tracking. Fog height now follows the absolute boundary point approaching the observed character, matching the countdown target.

## [1.2.14] - 2026-08-10

### Changed

- Enable affliction countdown display by default

## [1.2.13] - 2026-08-08

### Added

- Add `Show Teammate Extra Stamina Outside Bar` StaminaInfo setting to optionally show teammate extra stamina values outside the right side of their full extra stamina bars with a `+` prefix

## [1.2.12] - 2026-08-07

### Changed

- Changed optional outside teammate extra stamina value text color to green

## [1.2.11] - 2026-07-30

### Changed

- Improve day/night countdown calculation method to fix the countdown display issue when the day/night cycle is changed by other mods

## [1.2.10] - 2026-07-28

### Fixed

- Limit hunger countdown display to the local character because remote incremental hunger progress is not synchronized

## [1.2.9] - 2026-07-21

### Fixed

- Fixed hunger countdown behavior when hunger gain is blocked by invincibility or no-hunger effects
- Keep hunger countdown visible as a paused countdown while hunger gain is blocked
- Hide hunger countdown in the airport where hunger does not accumulate

## [1.2.8] - 2026-07-21

### Fixed

- Improve hunger countdown compatibility
- Fixed misaligned teammate stamina bar name text

## [1.2.7] - 2026-07-20

### Changed

- Improve anchored position of player name text in stamina bar

## [1.2.6] - 2026-07-20

### Added

- Add `Self Stamina Bar Count` debug setting to show duplicate self stamina bars
- Add local hunger countdown display for stamina info
- Add `Show Hunger Countdown` StaminaInfo setting, enabled by default
- Show hunger pre-countdown on the stamina value, or on the max stamina area when the current stamina value cannot fit

### Fixed

- Stabilize stamina bar UI layout
- Fixed StaminaInfo value text growing and shrinking with stamina bar changes
- Fixed teammate stamina bar layout updates when `Teammate Stamina Bar Scale` is changed in game
- Fixed wrong stamina status icon setting value position

### Changed

- Changed teammate extra stamina value text color to black to display better on the background

## [1.2.5] - 2026-07-10

### Fixed

- Fixed the incorrect room TerrainRandomiser seed display issue

## [1.2.4] - 2026-06-24

### Changed

- Update README and thunderstore website URL

## [1.2.3] - 2026-06-05

### Changed

- Show correct character stats when in spectating status

## [1.2.2] - 2026-05-26

### Added

- Add `ShowTerrainCustomiser` Settings to show the current terrain customiser seed

### Changed

- Display other map stats when can't get map level name

## [1.2.1] - 2026-05-20

### Added

- Add `ShowTerrainRandomiser` Settings to show the current terrain randomiser seed

### Changed

- Refactor config section names for better organization

### Removed

- Remove `ShowItemNames` Settings due to the item name display is not commonly used

## [1.2.0] - 2026-05-15

### Added

- Add `ShowStaminaInfo` Settings to show all stamina bars info number (current stamina, extra stamina, affliction bars) when stamina bar is visible

### Fixed

- Fix campfire icon and shield icon display sync issue with the stamina bar

## [1.1.6] - 2026-03-31

### Fixed

- Fix original fog speed value

## [1.1.5] - 2026-02-28

### Added

- Add `displayFog` Settings
- Add `displayLava` Settings

## [1.1.4] - 2026-02-20

### Added

- Add `Show Inventory Slots (Default: true)` Settings

### Fixed

- Fix incorrect biomes getting

## [1.1.3] - 2026-02-20

### Fixed

- Fix error for PEAK v1.54.b

## [1.1.2] - 2026-01-29

### Changed

- Support more stamina bars display

### Fixed

- Fix error level index in any scene

## [1.1.1] - 2026-01-18

### Fixed

- Fix disabled stamina bars when turned off `displaySelfStaminaBar` setting

## [1.1.0] - 2026-01-17

### Added

- Add `Teammate Stamina Bar Scale (Default: 0.72)` Settings

### Changed

- Work even when in spectating status
- Improve stamina bars display
- Refactor config module
- `Teammate Stamina Bar Limit` setting range value: 0~10 => 0~16

## [1.0.12] - 2025-12-17

### Changed
- Remove player name colored tags

## [1.0.11] - 2025-12-13

### Changed
- Reset personal stamina bar position and reduce the spacing between all bars slightly
- `Max Teammate Stamina Bars` setting default value: 8 -> 4

## [1.0.10] - 2025-12-12

### Changed
- Refactor `TimerHeightStats` module

### Fixed
- Fix empty timer text line occupying the first line

## [1.0.9] - 2025-12-06

### Changed
- Optimize UI display to support stamina bar display for up 10 teammates

## [1.0.8] - 2025-12-05

### Added
- Add `displayDayNightCountdown` Settings

### Changed
- Improve stamina bars UI display
- Improve time string display

### Fixed
- Fix map stats display error

## [1.0.7] - 2025-12-04

### Fixed
- Fix map stats display error

## [1.0.6] - 2025-11-29

### Fixed
- Fix biomes name display in multiplayer games
- Fix teammate bars not showing when `displaySelfStaminaBar` setting is disabled

## [1.0.5] - 2025-11-24

### Changed
- Improve UI display positions
- Truncate too long player name for the bar

## [1.0.4] - 2025-11-24

### Fixed
- Fix the patch argument name error

## [1.0.3] - 2025-11-24

### Added
- Add `displaySelfStaminaBar` Settings

### Changed
- Remove redundant init function

## [1.0.2] - 2025-11-24

### Fixed
- Fix empty level name in multiplayer games

## [1.0.1] - 2025-11-22

### Added
- Add `MapStats` UI
- Add `displayLevel` Settings
- Add `displayBiomes` Settings

### Changed
- Replace `MonoMod` with `Harmony` for hooking library

## [1.0.0] - Initial Release

### Added
- Merge `PeakStats` and `FriendsInfo` features

### Fixed
- Fix the error caused by the ROOTS update
- Fix invalid fuel bar filling
