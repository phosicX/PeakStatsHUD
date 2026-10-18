# Changelog

## 1.3.7

- Automatically lighten dark numeric labels so purple and blue affliction values remain distinct from their black outlines.
- Improve high-contrast mode for dark and saturated colors while preserving the native hue.

## 1.3.6

- Updated the HUD screenshot URL in the package description.

## 1.3.5

- Fixed balanced rounding showing one instead of zero when stamina and afflictions decrease together; exact integer values now remain exact.
- Fixed missing affliction labels when the HUD initializes before the native stamina bar.
- Reset rounding history and refresh values immediately when switching the observed player.
- Fixed unnecessary label wrapping when segments on the same bar have slightly different vertical positions.
- Reuse label lane assignments while their inputs are unchanged and remove unused layout code.

## 1.3.4

- Kept penalty values on separate stamina-bar rows so an extra stamina penalty cannot push a main-bar value onto a second row.

## 1.3.3

- Tightened wrapped affliction label rows so overlapping HP and status values remain close together.

## 1.3.2

- Rendered numeric labels above PEAK's native stamina bars and frames so values remain fully visible.

## 1.3.1

- Centered status penalty values on their native HUD segments after PEAK's extra-stamina bar changes.
- Added balanced, standard, and truncating rounding modes with zero, one, or two decimal places.
- Made balanced rounding direction-aware so values do not jump against unchanged or one-way raw input while the displayed total stays correct.
- Fixed zero stamina retaining a displayed value of one after decreasing.
- Added support for PEAK's separate petrification value and future status entries exposed through native HUD segments.
- Removed the special less-than-one display; rounded zero values now follow the existing zero-visibility setting.

## 1.2.1

- Prevented unchanged values from jumping by one while another stamina or affliction value changes.
- Restored left-edge placement for the main and bonus stamina values.
- Tightened label row spacing and delayed lane switching until labels nearly touch.
- Added smooth configurable movement when affliction labels change rows.

## 1.2.0

- Rebuilt the overlay inside PEAK's native stamina HUD so it now follows HUD scaling, opacity, scene lifecycle, and visibility correctly.
- Made regular stamina, bonus stamina, and affliction values independently configurable.
- Added `<1` display for small positive values, optional zero values, opacity, offsets, collision-avoiding label lanes, high contrast, and refresh-rate settings.
- Reused one shared text material and removed per-frame collections, string allocations, component searches, and redundant Harmony update patches.
- Stabilized rounding near fractional boundaries while keeping the displayed total correct.
- Added automatic migration from the original `com.StaminaStats.cfg` when the new config does not exist yet.
- Added automated tests and package validation.

## 1.1.0

- Values now round together, so their total stays correct.

## 1.0.0

- First release of StaminaStatsTactikot.
- Fixed the missing stamina number on current PEAK versions.
- Kept regular stamina, bonus stamina, and status values.
- Kept the original display settings.
