# Changelog

## [1.0.0] - 2026-10-18

Initial release of PeakStatsHUD, based on PeakStatsEx 1.5.4 by LengSword and
StaminaStatsTactikot 1.3.7 by TactiKot.

### Added

- Edge-style stamina labels: stamina and bonus stamina values above the left end of the bar,
  affliction values at the bottom edge of their segment, with up to three label rows.
- PEAK HUD font in bold and the StaminaStatsTactikot colour treatment for all values.
- Local hunger countdown next to the stamina and bonus stamina values, formatted as `(20s)`,
  with its own font size option.
- `Show Teammate Stamina Values` option to hide all numbers on teammate bars.
- `Teammate Name Vertical Offset` option; teammate names also move above the values
  automatically.
- `Show Percentage Sign` option for stamina, bonus stamina and affliction values.
- Labels render on their own sorting canvas, so petrify values on the extra stamina bar are no
  longer covered by the extra bar frame.

### Changed

- The stamina info configuration was reorganised around the edge layout.
- Teammate bars no longer draw values inside the bar.

### Fixed

- Affliction countdowns now keep refreshing once per second while a status is being reduced.
  PEAK applies reductions in 0.025 steps and keeps the partial decrease in
  `currentDecrementalStatuses`; the remaining time now subtracts that pending decrease.
- The hunger countdown no longer jumps between refreshes.

### Removed

- The `Show Teammate Extra Stamina Outside Bar` option; bonus stamina is always drawn next to
  the stamina value.
