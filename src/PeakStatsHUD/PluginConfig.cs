using BepInEx.Configuration;
using BepInEx.Logging;

namespace PeakStatsEx;

public static class PluginConfig
{
    internal static ConfigEntry<bool> DisplayTeammateStaminaBars { get; private set; } = null!;
    internal static ConfigEntry<float> TeammateStaminaBarProximity { get; private set; } = null!;
    internal static ConfigEntry<int> TeammateStaminaBarLimit { get; private set; } = null!;
    internal static ConfigEntry<float> TeammateStaminaBarScale { get; private set; } = null!;
    internal static ConfigEntry<bool> ShowInventorySlots { get; private set; } = null!;
    internal static ConfigEntry<bool> ShowStaminaInfo { get; private set; } = null!;
    internal static ConfigEntry<bool> ShowTeammateStaminaValues { get; private set; } = null!;
    internal static ConfigEntry<float> TeammateNameVerticalOffset { get; private set; } = null!;

    internal static ConfigEntry<float> StaminaInfoFontSize { get; private set; } = null!;
    internal static ConfigEntry<float> StaminaInfoAfflictionFontSize { get; private set; } = null!;
    internal static ConfigEntry<float> StaminaInfoOutlineWidth { get; private set; } = null!;
    internal static ConfigEntry<bool> StaminaInfoRoundStaminaBars { get; private set; } = null!;
    internal static ConfigEntry<bool> StaminaInfoRoundAfflictionBars { get; private set; } = null!;
    internal static ConfigEntry<bool> StaminaInfoShowAfflictionCountdown { get; private set; } = null!;
    internal static ConfigEntry<bool> StaminaInfoShowHungerCountdown { get; private set; } = null!;
    internal static ConfigEntry<float> StaminaInfoHungerFontSize { get; private set; } = null!;
    internal static ConfigEntry<bool> StaminaInfoShowBonusStamina { get; private set; } = null!;
    internal static ConfigEntry<bool> StaminaInfoShowPercentageSign { get; private set; } = null!;
    internal static ConfigEntry<float> StaminaInfoStaminaLabelVerticalOffset { get; private set; } = null!;
    internal static ConfigEntry<float> StaminaInfoAfflictionLabelVerticalOffset { get; private set; } = null!;
    internal static ConfigEntry<int> StaminaInfoAfflictionLabelRows { get; private set; } = null!;

    internal static ConfigEntry<bool> DisplaySelfStaminaBar { get; private set; } = null!;
    internal static ConfigEntry<int> SelfStaminaBarCount { get; private set; } = null!;

    public static void Initialize(ConfigFile config, ManualLogSource logger)
    {
        DisplayTeammateStaminaBars = config.Bind("Team", "Display Teammate Stamina Bars", true, "Displays your teammates' stamina bars at the bottom left of your screen.");
        TeammateStaminaBarProximity = config.Bind("Team", "Teammate Stamina Bar Proximity", 30f, new ConfigDescription("How close you need to be to your teammate to see their stamina bar.", new AcceptableValueRange<float>(1f, 10000f)));
        TeammateStaminaBarLimit = config.Bind("Team", "Teammate Stamina Bar Limit", 4, new ConfigDescription("Max teammate stamina bars.", new AcceptableValueRange<int>(0, 16)));
        TeammateStaminaBarScale = config.Bind("Team", "Teammate Stamina Bar Scale (Default: 0.72)", 0.72f, new ConfigDescription("Teammate stamina bars scale.", new AcceptableValueRange<float>(0.2f, 1f)));
        ShowInventorySlots = config.Bind("Team", "Show Inventory Slots", true, "Shows the teammate inventory and backpack slots next to the stamina bar.");
        ShowTeammateStaminaValues = config.Bind("Team", "Show Teammate Stamina Values", true, "Shows the numeric stamina, bonus stamina and affliction values on teammate stamina bars. Turn off to keep only the bars and names.");
        TeammateNameVerticalOffset = config.Bind("Team", "Teammate Name Vertical Offset", 0f, new ConfigDescription("Extra vertical offset applied to teammate names. When teammate values are shown, names are automatically moved above the value labels first.", new AcceptableValueRange<float>(-20f, 120f)));

        ShowStaminaInfo = config.Bind("Stats", "Show Stamina Info", true, "Shows the numeric stamina, bonus stamina and affliction values on the stamina bars.");

        StaminaInfoFontSize = config.Bind("Stats", "Bar Font Size", 18f, new ConfigDescription("Font size of the stamina and bonus stamina labels drawn at the bar edge.", new AcceptableValueRange<float>(8f, 48f)));
        StaminaInfoAfflictionFontSize = config.Bind("Stats", "Affliction Font Size", 16f, new ConfigDescription("Font size of the affliction labels drawn below their bar segment.", new AcceptableValueRange<float>(8f, 40f)));
        StaminaInfoOutlineWidth = config.Bind("Stats", "Bar Outline Width", 0.1f, new ConfigDescription("Outline width of the stamina info labels.", new AcceptableValueRange<float>(0f, 0.3f)));
        StaminaInfoRoundStaminaBars = config.Bind("Stats", "Round Stamina Bars", true, "If true, rounds to the nearest whole number for the stamina and extra stamina bars.");
        StaminaInfoRoundAfflictionBars = config.Bind("Stats", "Round Affliction Bars", false, "If true, rounds to the nearest whole number for affliction bars.");
        StaminaInfoShowAfflictionCountdown = config.Bind("Stats", "Show Affliction Countdown", true, "If true, appends a countdown to affliction labels that have a known reduction time.");
        StaminaInfoShowHungerCountdown = config.Bind("Stats", "Show Hunger Countdown", true, "If true, shows the local hunger countdown to the right of the stamina and bonus stamina values.");
        StaminaInfoHungerFontSize = config.Bind("Stats", "Hunger Countdown Font Size", 18f, new ConfigDescription("Font size of the hunger countdown label.", new AcceptableValueRange<float>(8f, 48f)));
        StaminaInfoShowBonusStamina = config.Bind("Stats", "Show Bonus Stamina Label", true, "If true, shows the bonus stamina value with a + prefix next to the stamina value.");
        StaminaInfoShowPercentageSign = config.Bind("Stats", "Show Percentage Sign", false, "If true, appends a % sign to the stamina, bonus stamina, and affliction values.");
        StaminaInfoStaminaLabelVerticalOffset = config.Bind("Stats", "Stamina Label Vertical Offset", 25f, new ConfigDescription("Vertical offset of the stamina labels, relative to the center of the stamina bar.", new AcceptableValueRange<float>(-80f, 120f)));
        StaminaInfoAfflictionLabelVerticalOffset = config.Bind("Stats", "Affliction Label Vertical Offset", -15f, new ConfigDescription("Vertical offset of the affliction labels, relative to the center of the stamina bar. Negative values place them at the bottom edge.", new AcceptableValueRange<float>(-100f, 100f)));
        StaminaInfoAfflictionLabelRows = config.Bind("Stats", "Affliction Label Rows", 2, new ConfigDescription("Maximum number of rows used to keep nearby affliction labels from overlapping.", new AcceptableValueRange<int>(1, 3)));

        DisplaySelfStaminaBar = config.Bind("Debug", "Display Self Stamina Bar", false, "Displays your stamina bar at the bottom left of your screen.");
        SelfStaminaBarCount = config.Bind("Debug", "Self Stamina Bar Count", 1, new ConfigDescription("Number of duplicate self stamina bars to display when Display Self Stamina Bar is enabled.", new AcceptableValueRange<int>(1, 16)));

        logger.LogInfo("Plugin Config Loaded.");
    }
}
