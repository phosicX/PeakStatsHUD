using BepInEx;
using BepInEx.Logging;
using PeakStatsEx.MonoBehaviours;
using HarmonyLib;
using UnityEngine;
using System.Text;
using System;

namespace PeakStatsEx;

/// <summary>
/// The BepInEx plugin class of PeakStatsEx.
/// </summary>
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;

        Logger.LogInfo($"Plugin {Name} is loaded!");

        PluginConfig.Initialize(Config, Logger);

        var harmony = new Harmony(Id);
        harmony.PatchAll();
    }
}

[HarmonyWrapSafe]
internal sealed class Patches
{
    // Attach the HUD bootstrap from a prefix. Prefixes run before every postfix, so the HUD is
    // still created when another mod's GUIManager.Start postfix throws and Harmony skips the
    // remaining postfixes (PeakAMap does this in TFA's FINAL ASCENT + Apocalypse mode).
    [HarmonyPatch(typeof(GUIManager), "Awake")]
    [HarmonyPrefix]
    private static void GUIManagerAwakePrefix(GUIManager __instance)
    {
        HudBootstrap.Attach(__instance);
    }

    [HarmonyPatch(typeof(GUIManager), "Start")]
    [HarmonyPrefix]
    private static void GUIManagerStartPrefix(GUIManager __instance)
    {
        HudBootstrap.Attach(__instance);
    }

    [HarmonyPatch(typeof(PlayerHandler), "RegisterCharacter")]
    [HarmonyPostfix]
    private static void PlayerHandlerRegisterCharacterHook(Character character)
    {
        if (character != null
            && character.photonView != null
            && character.photonView.IsMine
            && character.GetComponent<ProximityStaminaManager>() == null)
        {
            character.gameObject.AddComponent<ProximityStaminaManager>();
        }
    }
}

internal static class StringExtensions
{
    public static string TrimToByteLength(this string input, int byteLength)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        int currentBytes = Encoding.UTF8.GetByteCount(input);
        if (currentBytes <= byteLength)
            return input;

        // If the string contains only 1-byte characters (like ASCII), use substring for efficiency
        if (currentBytes == input.Length)
            return input[..byteLength];

        // Convert the string to a byte array
        byte[] bytesArray = Encoding.UTF8.GetBytes(input);

        // Resize the byte array to the desired byte length
        Array.Resize(ref bytesArray, byteLength);

        // Convert the byte array back to a string
        string result = Encoding.UTF8.GetString(bytesArray, 0, byteLength);

        // Trim off the potential replacement character '�' if multi-byte characters are cut
        return result.TrimEnd('�');
    }
}
