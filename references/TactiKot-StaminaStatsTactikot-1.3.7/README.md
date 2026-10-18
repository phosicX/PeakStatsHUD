# StaminaStatsTactikot

See your remaining stamina, bonus stamina, and affliction penalties at a glance. StaminaStatsTactikot adds configurable numbers to PEAK's existing HUD.

![Stamina, bonus stamina, and affliction values on PEAK's HUD](https://bsc.tactikot.ru/mods%2Fpeak%2Fstamina-hud.webp)

## Highlights

- Follows PEAK's HUD scale, opacity, and visibility, with support for ultrawide layouts.
- Uses the game's font and HUD hues, including its colorblind adjustments, and lightens dark label colors for readability.
- Reduces label overlap by arranging nearby affliction values across rows.

## Installation

Requires **BepInExPack_PEAK**. Remove the original StaminaStats mod first: the two mods cannot run together. For a manual installation of the original, remove `com.StaminaStats.dll` from your plugins folder.

**With a mod manager:** install this package and its listed BepInEx dependency, then launch PEAK.

**Manually:** install BepInExPack_PEAK first, copy `StaminaStatsTactikot.dll` to `PEAK/BepInEx/plugins`, then launch PEAK.

Settings are saved in `PEAK/BepInEx/config/com.tactikot.StaminaStatsTactikot.cfg`. On first launch, compatible display settings are copied from `com.StaminaStats.cfg` if that file is present.

## Settings

All display settings take effect immediately:

- Toggle the overlay or show stamina, bonus stamina, and afflictions independently.
- Choose **Balanced** rounding to preserve the combined rounded total and reduce rounding jumps, **Standard** to round each value independently, or **Truncate** to drop extra digits. Bonus stamina is rounded separately in Balanced mode.
- Show zero, one, or two decimal places, add a `%` sign, and optionally display values that round to zero.
- Set separate font sizes for stamina and afflictions, outline thickness, and label opacity.
- Adjust vertical label positions, use one to three affliction rows, and set how smoothly labels move between rows.
- Choose the game's HUD colors or an adjusted palette, with optional high contrast.
- Set the value refresh rate from 5 to 60 Hz (20 Hz by default). Positions and visibility continue to follow the HUD every frame.

Verbose diagnostic logging is available under `Debug`.

## Credits

Based on [StaminaStats 1.1.5 by pixx](https://thunderstore.io/c/peak/p/pixx/StaminaStats/). Compatibility fork by TactiKot, distributed under the MIT License.

## Build and test

The game path defaults to the standard Steam location and can be overridden without editing the project:

```powershell
dotnet test tests\StaminaStatsTactikot.Tests\StaminaStatsTactikot.Tests.csproj -c Release
dotnet build StaminaStatsTactikot.csproj -c Release -p:PeakGameDir="D:\Games\PEAK"
```

To test, build, validate, and create a Thunderstore ZIP in `dist`:

```powershell
.\Build-ThunderstorePackage.ps1
```
