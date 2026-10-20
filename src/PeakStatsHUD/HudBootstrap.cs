using System;
using PeakStatsEx.MonoBehaviours;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeakStatsEx;

/// <summary>
/// Creates the extra HUD elements (timer/height, map/level, teammate bar group) after
/// GUIManager.Start has finished.
///
/// Other mods can throw from their GUIManager.Start postfix (for example PeakAMap throws an
/// IndexOutOfRangeException while TFA's FINAL ASCENT + Apocalypse mode is active). Harmony stops
/// calling the remaining postfixes when that happens, so a normal postfix is not reliable here.
/// This component is attached from a prefix instead, which always runs before the postfixes, and
/// creates the HUD once the GUI is ready.
/// </summary>
internal sealed class HudBootstrap : MonoBehaviour
{
    private const float TemplateCheckInterval = 0.25f;
    private const float WarnAfterSeconds = 60f;

    private GUIManager gui = null!;
    private bool timerCreated;
    private bool mapCreated;
    private bool barsInitialized;
    private bool missingTemplateWarned;
    private float nextTemplateCheck;
    private float startedAt;

    internal static void Attach(GUIManager manager)
    {
        if (manager == null)
        {
            return;
        }

        if (manager.GetComponent<HudBootstrap>() != null)
        {
            return;
        }

        HudBootstrap bootstrap = manager.gameObject.AddComponent<HudBootstrap>();
        bootstrap.gui = manager;
        bootstrap.startedAt = Time.realtimeSinceStartup;
        bootstrap.nextTemplateCheck = 0f;
    }

    private void Update()
    {
        if (gui == null)
        {
            Destroy(this);
            return;
        }

        // The map generation of TFA's FINAL ASCENT can take minutes, so keep waiting for the
        // AscentUI template instead of giving up after a short timeout.
        if ((!timerCreated || !mapCreated) && Time.unscaledTime >= nextTemplateCheck)
        {
            nextTemplateCheck = Time.unscaledTime + TemplateCheckInterval;

            if (SceneManager.GetActiveScene().name != "Airport")
            {
                AscentUI? template = gui.GetComponentInChildren<AscentUI>(includeInactive: true);
                if (template != null)
                {
                    if (!timerCreated)
                    {
                        timerCreated = CreateTimerUi(template);
                    }

                    if (!mapCreated)
                    {
                        mapCreated = CreateMapUi(gui, template);
                    }
                }
                else if (!missingTemplateWarned && Time.realtimeSinceStartup - startedAt > WarnAfterSeconds)
                {
                    missingTemplateWarned = true;
                    Plugin.Logger?.LogWarning(
                        "PeakStatsHUD: no AscentUI template was found yet; timer/height and map/level statistics are still waiting.");
                }
            }
        }

        if (!barsInitialized && gui.bar != null && gui.bar.transform.parent != null)
        {
            try
            {
                ProximityStaminaManager.InitializeBarGroup(gui);
                barsInitialized = true;
            }
            catch (Exception e)
            {
                Plugin.Logger?.LogError("PeakStatsHUD: failed to initialise the teammate stamina bar group. " + e);
                barsInitialized = true;
            }
        }

        if (timerCreated && mapCreated && barsInitialized)
        {
            Destroy(this);
        }
    }

    private static bool CreateTimerUi(AscentUI template)
    {
        try
        {
            Transform templateTransform = template.transform;
            RectTransform clone = Instantiate((RectTransform)templateTransform, templateTransform.parent);
            clone.name = "Timer & Height UI";
            Destroy(clone.GetComponent<AscentUI>());
            clone.gameObject.AddComponent<TimerHeightStats>();

            Vector2 anchor = new Vector2(0.5f, 1f);
            clone.anchorMin = anchor;
            clone.anchorMax = anchor;
            clone.pivot = anchor;
            clone.anchoredPosition = Vector2.zero;
            clone.gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogError("PeakStatsHUD: failed to create the timer/height UI. " + e);
        }

        return true;
    }

    private static bool CreateMapUi(GUIManager gui, AscentUI template)
    {
        try
        {
            RectTransform source = (RectTransform)template.transform;

            RectTransform mapRect = Instantiate(source, source.parent);
            mapRect.name = "Map & Level UI";
            Destroy(mapRect.GetComponent<AscentUI>());
            MapStats mapStats = mapRect.gameObject.AddComponent<MapStats>();
            mapRect.anchorMin = new Vector2(1f, 1f);
            mapRect.anchorMax = new Vector2(1f, 1f);
            mapRect.pivot = new Vector2(1f, 1f);
            mapRect.anchoredPosition = new Vector2(-MapStats.MapTextRightMargin, 5f);
            mapRect.gameObject.SetActive(true);

            RectTransform routeRect = Instantiate(source, source.parent);
            routeRect.name = "Biome Route UI";
            Destroy(routeRect.GetComponent<AscentUI>());
            routeRect.anchorMin = new Vector2(1f, 1f);
            routeRect.anchorMax = new Vector2(1f, 1f);
            routeRect.pivot = new Vector2(1f, 1f);
            routeRect.anchoredPosition = new Vector2(
                0f,
                source.anchoredPosition.y - source.rect.height + 8f);
            TMP_Text routeText = routeRect.GetComponent<TMP_Text>();
            routeRect.gameObject.SetActive(true);

            RectTransform variantRect = Instantiate(source, source.parent);
            variantRect.name = "Biome Variant UI";
            Destroy(variantRect.GetComponent<AscentUI>());
            variantRect.anchorMin = new Vector2(1f, 1f);
            variantRect.anchorMax = new Vector2(1f, 1f);
            variantRect.pivot = new Vector2(1f, 1f);
            variantRect.anchoredPosition = new Vector2(
                0f,
                routeRect.anchoredPosition.y - source.rect.height + 22f);
            TMP_Text variantText = variantRect.GetComponent<TMP_Text>();
            variantRect.gameObject.SetActive(true);

            RectTransform hudCanvasRect = gui.hudCanvas.GetComponent<RectTransform>();
            mapStats.InitializeRouteText(routeText, variantText, hudCanvasRect);
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogError("PeakStatsHUD: failed to create the map/level UI. " + e);
        }

        return true;
    }
}
