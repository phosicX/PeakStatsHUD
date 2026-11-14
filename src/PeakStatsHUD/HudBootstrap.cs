using System;
using PeakStatsEx.MonoBehaviours;
using UnityEngine;

namespace PeakStatsEx;

/// <summary>
/// Initialises the teammate stamina bar group after GUIManager.Start has finished.
///
/// The group used to be created from a GUIManager.Start postfix. Another mod's postfix can throw
/// (PeakAMap did this in TFA's FINAL ASCENT + Apocalypse mode) and Harmony then skips the remaining
/// postfixes, which left the teammate bars missing. Attaching from a prefix and waiting one frame
/// makes the initialisation independent of the postfix order.
/// </summary>
internal sealed class HudBootstrap : MonoBehaviour
{
    private GUIManager gui = null!;

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
    }

    private void Update()
    {
        if (gui == null)
        {
            Destroy(this);
            return;
        }

        if (gui.bar == null || gui.bar.transform.parent == null)
        {
            return;
        }

        try
        {
            ProximityStaminaManager.InitializeBarGroup(gui);
        }
        catch (Exception e)
        {
            Plugin.Logger?.LogError("PeakStatsHUD: failed to initialise the teammate stamina bar group. " + e);
        }

        Destroy(this);
    }
}
