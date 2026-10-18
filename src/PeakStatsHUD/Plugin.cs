using BepInEx;
using BepInEx.Logging;
using PeakStatsEx.MonoBehaviours;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;
using System;

namespace PeakStatsEx;

/// <summary>
/// The BepInEx plugin class of PeakStatsEx.
/// </summary>
[BepInAutoPlugin]
[BepInDependency("com.snosz.terrainrandomiser", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.snosz.terraincustomiser", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("af.pll.library", BepInDependency.DependencyFlags.SoftDependency)]
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
    [HarmonyPatch(typeof(GUIManager), "Start")]
    [HarmonyPostfix]
    private static void GUIManagerStartHook(GUIManager __instance)
    {
        if (SceneManager.GetActiveScene().name != "Airport")
        {
            CreateTimerAndHeightStats();
            CreateMapStats();
        }
        ProximityStaminaManager.InitializeBarGroup(__instance);
        void CreateTimerAndHeightStats()
        {
            Transform obj = __instance.GetComponentInChildren<AscentUI>().transform;
            RectTransform obj2 = (RectTransform)UnityEngine.Object.Instantiate(obj, obj.parent);
            obj2.name = "Timer & Height UI";
            UnityEngine.Object.Destroy(obj2.GetComponent<AscentUI>());
            obj2.gameObject.AddComponent<TimerHeightStats>();
            obj2.pivot = obj2.anchorMax = obj2.anchorMin = new Vector2(0.5f, 1f);
            obj2.anchoredPosition = Vector2.zero;
        }
        void CreateMapStats()
        {
            RectTransform sourceRect = (RectTransform)__instance.GetComponentInChildren<AscentUI>().transform;
            RectTransform mapStatsRect = UnityEngine.Object.Instantiate(sourceRect, sourceRect.parent);
            mapStatsRect.name = "Map & Level UI";
            UnityEngine.Object.Destroy(mapStatsRect.GetComponent<AscentUI>());
            MapStats mapStats = mapStatsRect.gameObject.AddComponent<MapStats>();
            mapStatsRect.pivot = mapStatsRect.anchorMax = mapStatsRect.anchorMin = new Vector2(1f, 1f);
            mapStatsRect.anchoredPosition = new Vector2(-MapStats.MapTextRightMargin, 5f);

            RectTransform routeRect = UnityEngine.Object.Instantiate(sourceRect, sourceRect.parent);
            routeRect.name = "Biome Route UI";
            UnityEngine.Object.Destroy(routeRect.GetComponent<AscentUI>());
            routeRect.pivot = routeRect.anchorMax = routeRect.anchorMin = new Vector2(1f, 1f);
            routeRect.anchoredPosition = new Vector2(0f, sourceRect.anchoredPosition.y - sourceRect.rect.height + 8f);

            TMP_Text routeText = routeRect.GetComponent<TMP_Text>();
            RectTransform variantRect = UnityEngine.Object.Instantiate(sourceRect, sourceRect.parent);
            variantRect.name = "Biome Variant UI";
            UnityEngine.Object.Destroy(variantRect.GetComponent<AscentUI>());
            variantRect.pivot = variantRect.anchorMax = variantRect.anchorMin = new Vector2(1f, 1f);
            variantRect.anchoredPosition = new Vector2(0f, routeRect.anchoredPosition.y - sourceRect.rect.height + 22f);

            TMP_Text variantText = variantRect.GetComponent<TMP_Text>();
            RectTransform hudCanvasRect = __instance.hudCanvas.GetComponent<RectTransform>();
            mapStats.InitializeRouteText(routeText, variantText, hudCanvasRect);
        }
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

    [HarmonyPatch(typeof(LoadingScreenHandler), "LoadSceneProcess")]
    [HarmonyPostfix]
    private static void LoadSceneProcessHook(string sceneName)
    {
        if (!sceneName.Contains("Level_"))
        {
            return;
        }
        MapStats.currentLevelName = sceneName;
        MapStats.currentLevelIndex = -1;
        Plugin.Logger.LogDebug($"After: LoadingScreenHandler.LoadSceneProcess called. sceneName: {sceneName}");
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
