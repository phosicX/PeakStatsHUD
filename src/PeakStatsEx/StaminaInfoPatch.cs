using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PeakStatsEx.MonoBehaviours;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakStatsEx;

[HarmonyWrapSafe]
internal sealed class StaminaInfoPatch
{
    internal static GUIManager guiManager = null!;

    private const string OverlayName = "StaminaInfo";
    private const float LargeBarThreshold = 30f;
    private const float SmallBarThreshold = 15f;
    private const float BarUnitSize = 6f;
    private const float HungerIncrementThreshold = 0.025f;
    private const float AfflictionRefreshInterval = 0.5f;
    private const float ValueRefreshInterval = 0.05f;
    private const float StaminaLabelRightEdge = 20f;
    private const float BonusLabelLeftEdge = 26f;
    private const float LabelWidth = 260f;
    private const float AfflictionLabelPadding = 10f;
    private const float LaneGapRatio = 1.1f;
    private const float HungerLabelWidth = 160f;
    private const float HungerLabelGap = 6f;

    private static readonly Color DefaultStaminaColor = new Color(0.72f, 1f, 0.72f);
    private static readonly Color DefaultBonusColor = new Color(1f, 0.92f, 0.25f);
    private static readonly Color DefaultHungerColor = new Color(1f, 0.88f, 0.3f);

    private static readonly Dictionary<string, LabelEntry> labels = [];
    private static readonly Dictionary<int, float> nextTeammateRefresh = [];
    private static readonly List<AfflictionCandidate> afflictionCandidates = [];
    private static readonly float[] laneRightEdges = [float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity];
    private static readonly Vector3[] worldCorners = new Vector3[4];

    private static readonly Dictionary<int, float> hungerIncrementalStatuses = [];
    private static readonly Dictionary<int, float> lastTrackedHungerAddTimes = [];
    private static readonly Dictionary<int, float> lastObservedRealHungerIncrementalStatuses = [];
    private static readonly Dictionary<int, float> lastRealHungerProgressTimes = [];
    private static readonly Dictionary<int, bool> wasHungerBlockedByVanilla = [];
    private static readonly FieldInfo? currentIncrementalStatusesField =
        AccessTools.Field(typeof(CharacterAfflictions), "currentIncrementalStatuses");
    private static readonly FieldInfo? currentDecrementalStatusesField =
        AccessTools.Field(typeof(CharacterAfflictions), "currentDecrementalStatuses");

    private static float nextLocalRefresh;

    private enum LabelKind
    {
        Stamina,
        Bonus,
        Affliction,
        Hunger,
    }

    private sealed class LabelEntry
    {
        internal TextMeshProUGUI Text = null!;
        internal LabelKind Kind;
        internal RectTransform Overlay = null!;
        internal Image? ColorSource;
        internal Color FallbackColor;
        internal int ColorIndex;
        internal RectTransform? Segment;
    }

    private sealed class AfflictionCandidate
    {
        internal AfflictionCandidate(LabelEntry entry, Vector2 center, float width)
        {
            Entry = entry;
            Center = center;
            Width = width;
        }

        internal LabelEntry Entry { get; }
        internal Vector2 Center { get; }
        internal float Width { get; }
    }

    [HarmonyPatch(typeof(StaminaBar), "Update")]
    [HarmonyPostfix]
    private static void StaminaInfoStaminaBarUpdate(StaminaBar __instance)
    {
        try
        {
            if (guiManager == null)
            {
                labels.Clear();
                nextTeammateRefresh.Clear();
                nextLocalRefresh = 0f;
                InitStaminaInfo(__instance);
            }

            if (guiManager == null ||
                !PluginConfig.ShowStaminaInfo.Value ||
                Character.observedCharacter == null ||
                !IsHudVisible())
            {
                HideLabelsWithPrefix("S_");
                return;
            }

            if (Time.unscaledTime < nextLocalRefresh)
            {
                return;
            }

            nextLocalRefresh = Time.unscaledTime + ValueRefreshInterval;
            UpdateSelfLabels(__instance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e.Message + e.StackTrace);
        }
    }

    [HarmonyPatch(typeof(CharacterStaminaBar), "Update")]
    [HarmonyPostfix]
    public static void Update(CharacterStaminaBar __instance)
    {
        try
        {
            if (guiManager == null)
            {
                return;
            }

            int id = __instance.GetInstanceID();
            string prefix = "T" + id + "_";

            if (!PluginConfig.ShowStaminaInfo.Value ||
                !PluginConfig.ShowTeammateStaminaValues.Value ||
                Character.observedCharacter == null ||
                !IsHudVisible())
            {
                HideLabelsWithPrefix(prefix);
                return;
            }

            if (!labels.ContainsKey(prefix + "Stamina"))
            {
                InitCharacterStaminaInfo(__instance);
            }

            if (nextTeammateRefresh.TryGetValue(id, out float next) && Time.unscaledTime < next)
            {
                return;
            }

            nextTeammateRefresh[id] = Time.unscaledTime + ValueRefreshInterval;
            UpdateCharacterBarTexts(__instance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e.Message + e.StackTrace);
        }
    }

    [HarmonyPatch(typeof(CharacterAfflictions), "AddStatus")]
    [HarmonyPostfix]
    private static void CharacterAfflictionsAddStatusPostfix(
        CharacterAfflictions __instance,
        CharacterAfflictions.STATUSTYPE statusType,
        float amount,
        bool __result)
    {
        if (statusType != CharacterAfflictions.STATUSTYPE.Hunger || amount <= 0f)
        {
            return;
        }

        if (__result)
        {
            if (TryReadCurrentIncrementalHunger(__instance, out float currentIncrementalStatus))
            {
                SetFallbackHungerIncrementalStatus(__instance, currentIncrementalStatus);
            }
            else
            {
                AddFallbackHungerIncrementalStatus(__instance, amount);
            }
            return;
        }

        if (!ShouldTrackHungerAdd(__instance))
        {
            return;
        }

        AddFallbackHungerIncrementalStatus(__instance, amount);
    }

    private static bool ShouldTrackHungerAdd(CharacterAfflictions afflictions)
    {
        Character? character = Character.observedCharacter;
        if (character?.refs?.afflictions != afflictions)
        {
            return false;
        }

        return character.data.fullyConscious &&
            afflictions.canGetHungry &&
            afflictions.hungerPerSecond * Ascents.hungerRateMultiplier > 0f;
    }

    private static void AddFallbackHungerIncrementalStatus(CharacterAfflictions afflictions, float amount)
    {
        int id = afflictions.GetInstanceID();
        hungerIncrementalStatuses.TryGetValue(id, out float currentIncrementalStatus);
        currentIncrementalStatus += amount;
        SetFallbackHungerIncrementalStatus(
            afflictions,
            currentIncrementalStatus >= HungerIncrementThreshold ? 0f : currentIncrementalStatus);
    }

    private static void SetFallbackHungerIncrementalStatus(CharacterAfflictions afflictions, float currentIncrementalStatus)
    {
        int id = afflictions.GetInstanceID();
        hungerIncrementalStatuses[id] = currentIncrementalStatus;
        lastTrackedHungerAddTimes[id] = Time.time;
    }

    private static void ClearFallbackHungerIncrementalStatus(CharacterAfflictions afflictions)
    {
        hungerIncrementalStatuses[afflictions.GetInstanceID()] = 0f;
    }

    private static float GetFallbackHungerIncrementalStatus(CharacterAfflictions afflictions)
    {
        int id = afflictions.GetInstanceID();
        hungerIncrementalStatuses.TryGetValue(id, out float currentIncrementalStatus);
        return currentIncrementalStatus;
    }

    [HarmonyPatch(typeof(CharacterAfflictions), "SubtractStatus")]
    [HarmonyPrefix]
    private static void CharacterAfflictionsSubtractStatusPrefix(
        CharacterAfflictions __instance,
        CharacterAfflictions.STATUSTYPE statusType,
        ref float __state)
    {
        __state = statusType == CharacterAfflictions.STATUSTYPE.Hunger
            ? __instance.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger)
            : -1f;
    }

    [HarmonyPatch(typeof(CharacterAfflictions), "SubtractStatus")]
    [HarmonyPostfix]
    private static void CharacterAfflictionsSubtractStatusPostfix(
        CharacterAfflictions __instance,
        CharacterAfflictions.STATUSTYPE statusType,
        float __state)
    {
        if (statusType != CharacterAfflictions.STATUSTYPE.Hunger || __state <= 0f)
        {
            return;
        }

        float currentStatus = __instance.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger);
        if (currentStatus < __state)
        {
            hungerIncrementalStatuses[__instance.GetInstanceID()] = 0f;
        }
    }

    [HarmonyPatch(typeof(CharacterAfflictions), "SetStatus")]
    [HarmonyPostfix]
    private static void CharacterAfflictionsSetStatusPostfix(
        CharacterAfflictions __instance,
        CharacterAfflictions.STATUSTYPE statusType)
    {
        if (statusType == CharacterAfflictions.STATUSTYPE.Hunger)
        {
            hungerIncrementalStatuses[__instance.GetInstanceID()] = 0f;
        }
    }

    private static void InitStaminaInfo(StaminaBar staminaBar)
    {
        GameObject? guiManagerGameObject = GameObject.Find("GAME/GUIManager");
        if (guiManagerGameObject == null)
        {
            return;
        }

        guiManager = guiManagerGameObject.GetComponent<GUIManager>();
        if (guiManager == null)
        {
            return;
        }

        RectTransform overlay = EnsureOverlay(staminaBar.transform);
        if (labels.ContainsKey("S_Stamina"))
        {
            return;
        }

        TMP_FontAsset font = GetBarFont(staminaBar);
        AddLabel(
            overlay,
            "S_Stamina",
            LabelKind.Stamina,
            TextAlignmentOptions.BottomRight,
            new Vector2(1f, 0f),
            staminaBar.staminaBar.GetComponent<Image>(),
            DefaultStaminaColor,
            0,
            font,
            null);
        AddLabel(
            overlay,
            "S_Bonus",
            LabelKind.Bonus,
            TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0f),
            FindFillImage(staminaBar.extraBarStamina),
            DefaultBonusColor,
            1,
            font,
            null);

        BarAffliction[]? afflictions = staminaBar.afflictions;
        int hungerIndex = -1;
        Image? hungerColorSource = null;
        if (afflictions != null)
        {
            for (int i = 0; i < afflictions.Length; i++)
            {
                BarAffliction? affliction = afflictions[i];
                if (affliction == null)
                {
                    continue;
                }

                Image? colorSource = FindAfflictionImage(affliction);
                AddLabel(
                    overlay,
                    "S_Aff" + i,
                    LabelKind.Affliction,
                    TextAlignmentOptions.Center,
                    new Vector2(0.5f, 0.5f),
                    colorSource,
                    DefaultAfflictionColor(i),
                    i + 2,
                    font,
                    affliction.rtf);

                if (affliction.afflictionType == CharacterAfflictions.STATUSTYPE.Hunger)
                {
                    hungerIndex = i;
                    hungerColorSource = colorSource;
                }
            }
        }

        AddLabel(
            overlay,
            "S_Hunger",
            LabelKind.Hunger,
            TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0f),
            hungerColorSource,
            DefaultHungerColor,
            hungerIndex >= 0 ? hungerIndex + 2 : 2,
            font,
            null);
    }

    public static void InitCharacterStaminaInfo(CharacterStaminaBar characterStaminaBar)
    {
        if (guiManager == null)
        {
            return;
        }

        RectTransform overlay = EnsureOverlay(characterStaminaBar.transform);
        int id = characterStaminaBar.GetInstanceID();
        string prefix = "T" + id + "_";
        if (labels.ContainsKey(prefix + "Stamina"))
        {
            return;
        }

        AddLabel(
            overlay,
            prefix + "Stamina",
            LabelKind.Stamina,
            TextAlignmentOptions.BottomRight,
            new Vector2(1f, 0f),
            characterStaminaBar.staminaBarRectTransform.GetComponent<Image>(),
            DefaultStaminaColor,
            0,
            GetBarFont(null),
            null);
        AddLabel(
            overlay,
            prefix + "Bonus",
            LabelKind.Bonus,
            TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0f),
            FindFillImage(characterStaminaBar.extraBarStamina),
            DefaultBonusColor,
            1,
            GetBarFont(null),
            null);

        for (int i = 0; i < characterStaminaBar.characterBarAfflictions.Count; i++)
        {
            CharacterBarAffliction? affliction = characterStaminaBar.characterBarAfflictions[i];
            if (affliction == null)
            {
                continue;
            }

            AddLabel(
                overlay,
                prefix + "Aff" + i,
                LabelKind.Affliction,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                FindTeammateAfflictionImage((RectTransform)affliction.transform),
                DefaultAfflictionColor(i),
                i + 2,
                GetBarFont(null),
                (RectTransform)affliction.transform);
        }
    }

    private static void UpdateSelfLabels(StaminaBar staminaBar)
    {
        if (!labels.TryGetValue("S_Stamina", out LabelEntry? staminaEntry))
        {
            return;
        }

        labels.TryGetValue("S_Bonus", out LabelEntry? bonusEntry);
        LayoutStaminaLine(
            staminaEntry,
            bonusEntry,
            staminaBar.fullBar,
            staminaBar.desiredStaminaSize,
            staminaBar.desiredExtraStaminaSize);

        UpdateSelfAfflictionLabels(staminaEntry.Overlay, staminaBar, "S_");

        labels.TryGetValue("S_Hunger", out LabelEntry? hungerEntry);
        UpdateSelfHungerLabel(staminaEntry.Overlay, staminaBar, bonusEntry, hungerEntry);
    }

    public static void UpdateCharacterBarTexts(CharacterStaminaBar characterStaminaBar)
    {
        Character? character = characterStaminaBar.ObservedCharacter;
        if (character == null)
        {
            return;
        }

        int id = characterStaminaBar.GetInstanceID();
        string prefix = "T" + id + "_";
        if (!labels.TryGetValue(prefix + "Stamina", out LabelEntry? staminaEntry))
        {
            return;
        }

        float currentStaminaSize = Mathf.Max(
            0f,
            character.data.currentStamina * characterStaminaBar.fullBarRectTransform.sizeDelta.x);
        float extraStaminaSize = Mathf.Max(
            0f,
            character.data.extraStamina * characterStaminaBar.fullBarRectTransform.sizeDelta.x);

        labels.TryGetValue(prefix + "Bonus", out LabelEntry? bonusEntry);
        LayoutStaminaLine(
            staminaEntry,
            bonusEntry,
            characterStaminaBar.fullBarRectTransform,
            currentStaminaSize,
            extraStaminaSize);

        UpdateTeammateAfflictionLabels(staminaEntry.Overlay, characterStaminaBar, prefix);
    }

    private static void LayoutStaminaLine(
        LabelEntry staminaEntry,
        LabelEntry? bonusEntry,
        RectTransform fullBar,
        float staminaSize,
        float bonusSize)
    {
        RectTransform overlay = staminaEntry.Overlay;
        fullBar.GetWorldCorners(worldCorners);
        Vector3 left = overlay.InverseTransformPoint((worldCorners[0] + worldCorners[1]) * 0.5f);
        Vector3 center = overlay.InverseTransformPoint((worldCorners[0] + worldCorners[2]) * 0.5f);

        float fontSize = PluginConfig.StaminaInfoFontSize.Value;
        float height = GetLabelHeight(fontSize);
        float y = center.y + PluginConfig.StaminaInfoStaminaLabelVerticalOffset.Value;

        ApplyColor(staminaEntry);
        if (staminaSize >= SmallBarThreshold)
        {
            SetLabelText(
                staminaEntry,
                FormatBarValue(staminaSize, PluginConfig.StaminaInfoRoundStaminaBars.Value, trimWholeDecimal: false) +
                GetPercentSuffix());
            SetTextRect(
                staminaEntry.Text,
                new Vector2(1f, 0f),
                new Vector2(left.x + StaminaLabelRightEdge, y),
                new Vector2(LabelWidth, height),
                fontSize);
        }
        else
        {
            staminaEntry.Text.gameObject.SetActive(false);
        }

        if (bonusEntry == null)
        {
            return;
        }

        if (!PluginConfig.StaminaInfoShowBonusStamina.Value || bonusSize < SmallBarThreshold)
        {
            bonusEntry.Text.gameObject.SetActive(false);
            return;
        }

        ApplyColor(bonusEntry);
        SetLabelText(
            bonusEntry,
            "+" + FormatBarValue(bonusSize, PluginConfig.StaminaInfoRoundStaminaBars.Value, trimWholeDecimal: false) +
            GetPercentSuffix());
        SetTextRect(
            bonusEntry.Text,
            new Vector2(0f, 0f),
            new Vector2(left.x + BonusLabelLeftEdge, y),
            new Vector2(LabelWidth, height),
            fontSize);
    }

    private static void UpdateSelfAfflictionLabels(RectTransform overlay, StaminaBar staminaBar, string prefix)
    {
        afflictionCandidates.Clear();
        BarAffliction[]? afflictions = staminaBar.afflictions;
        if (afflictions == null)
        {
            return;
        }

        for (int i = 0; i < afflictions.Length; i++)
        {
            BarAffliction? affliction = afflictions[i];
            if (affliction == null || !labels.TryGetValue(prefix + "Aff" + i, out LabelEntry? entry))
            {
                continue;
            }

            if (!affliction.gameObject.activeSelf || affliction.size < SmallBarThreshold)
            {
                entry.Text.gameObject.SetActive(false);
                continue;
            }

            SetLabelText(
                entry,
                FormatBarValue(
                    affliction.size,
                    PluginConfig.StaminaInfoRoundAfflictionBars.Value,
                    trimWholeDecimal: true) +
                GetPercentSuffix() +
                GetAfflictionCountdownSuffix(affliction.afflictionType));
            ApplyColor(entry);
            AddAfflictionCandidate(overlay, entry, affliction.rtf);
        }

        PlaceAfflictionLabels();
    }

    private static void UpdateTeammateAfflictionLabels(
        RectTransform overlay,
        CharacterStaminaBar characterStaminaBar,
        string prefix)
    {
        afflictionCandidates.Clear();
        for (int i = 0; i < characterStaminaBar.characterBarAfflictions.Count; i++)
        {
            CharacterBarAffliction? affliction = characterStaminaBar.characterBarAfflictions[i];
            if (affliction == null || !labels.TryGetValue(prefix + "Aff" + i, out LabelEntry? entry))
            {
                continue;
            }

            affliction.FetchDesiredSize();
            if (!affliction.gameObject.activeSelf || affliction.size < SmallBarThreshold)
            {
                entry.Text.gameObject.SetActive(false);
                continue;
            }

            SetLabelText(
                entry,
                FormatBarValue(
                    affliction.size,
                    PluginConfig.StaminaInfoRoundAfflictionBars.Value,
                    trimWholeDecimal: true) +
                GetPercentSuffix() +
                GetAfflictionCountdownSuffix(affliction.afflictionType));
            ApplyColor(entry);
            AddAfflictionCandidate(overlay, entry, (RectTransform)affliction.transform);
        }

        PlaceAfflictionLabels();
    }

    private static void AddAfflictionCandidate(RectTransform overlay, LabelEntry entry, RectTransform segment)
    {
        Vector3 center = overlay.InverseTransformPoint(segment.TransformPoint(segment.rect.center));
        float width = Mathf.Max(1f, entry.Text.preferredWidth);
        afflictionCandidates.Add(new AfflictionCandidate(entry, new Vector2(center.x, center.y), width));
    }

    private static void PlaceAfflictionLabels()
    {
        if (afflictionCandidates.Count == 0)
        {
            return;
        }

        afflictionCandidates.Sort((left, right) => left.Center.x.CompareTo(right.Center.x));

        int maxLanes = Mathf.Clamp(PluginConfig.StaminaInfoAfflictionLabelRows.Value, 1, 3);
        float fontSize = PluginConfig.StaminaInfoAfflictionFontSize.Value;
        float height = GetLabelHeight(fontSize);
        float laneStep = height * LaneGapRatio;
        float offset = PluginConfig.StaminaInfoAfflictionLabelVerticalOffset.Value;
        float direction = offset <= 0f ? -1f : 1f;

        for (int i = 0; i < laneRightEdges.Length; i++)
        {
            laneRightEdges[i] = float.NegativeInfinity;
        }

        for (int i = 0; i < afflictionCandidates.Count; i++)
        {
            AfflictionCandidate candidate = afflictionCandidates[i];
            float halfWidth = candidate.Width * 0.5f;
            float left = candidate.Center.x - halfWidth;

            int lane = 0;
            while (lane < maxLanes - 1 && left < laneRightEdges[lane] + 2f)
            {
                lane++;
            }

            laneRightEdges[lane] = candidate.Center.x + halfWidth;
            Vector2 position = new Vector2(
                candidate.Center.x,
                candidate.Center.y + offset + direction * lane * laneStep);
            SetTextRect(
                candidate.Entry.Text,
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(candidate.Width + AfflictionLabelPadding, height),
                fontSize);
        }
    }

    private static void UpdateSelfHungerLabel(
        RectTransform overlay,
        StaminaBar staminaBar,
        LabelEntry? bonusEntry,
        LabelEntry? entry)
    {
        if (entry == null)
        {
            return;
        }

        if (!PluginConfig.StaminaInfoShowHungerCountdown.Value)
        {
            entry.Text.gameObject.SetActive(false);
            return;
        }

        Character? character = Character.localCharacter;
        CharacterAfflictions? afflictions = character?.refs?.afflictions;
        if (character == null || afflictions == null)
        {
            entry.Text.gameObject.SetActive(false);
            return;
        }

        float remaining = GetHungerIncreaseTimeRemaining(character, afflictions);
        if (remaining <= 0f)
        {
            entry.Text.gameObject.SetActive(false);
            return;
        }

        staminaBar.fullBar.GetWorldCorners(worldCorners);
        Vector3 left = overlay.InverseTransformPoint((worldCorners[0] + worldCorners[1]) * 0.5f);
        float centerY = overlay.InverseTransformPoint((worldCorners[0] + worldCorners[2]) * 0.5f).y;

        bool bonusShown =
            bonusEntry != null &&
            bonusEntry.Text.gameObject.activeSelf &&
            PluginConfig.StaminaInfoShowBonusStamina.Value;
        float x = bonusShown
            ? left.x + BonusLabelLeftEdge + bonusEntry!.Text.preferredWidth + HungerLabelGap
            : left.x + StaminaLabelRightEdge + HungerLabelGap;

        float fontSize = PluginConfig.StaminaInfoHungerFontSize.Value;
        SetLabelText(entry, "(" + Mathf.CeilToInt(remaining).ToString() + "s)");
        ApplyColor(entry);
        SetTextRect(
            entry.Text,
            new Vector2(0f, 0f),
            new Vector2(x, centerY + PluginConfig.StaminaInfoStaminaLabelVerticalOffset.Value),
            new Vector2(HungerLabelWidth, GetLabelHeight(fontSize)),
            fontSize);
    }

    private static string GetAfflictionCountdownSuffix(CharacterAfflictions.STATUSTYPE afflictionType)
    {
        if (!CanShowCountdown(afflictionType) || !PluginConfig.StaminaInfoShowAfflictionCountdown.Value)
        {
            return string.Empty;
        }

        float remaining = GetReductionTimeRemaining(afflictionType);
        return remaining > 0f ? "(" + FormatTime(remaining) + ")" : string.Empty;
    }

    private static string GetPercentSuffix()
    {
        return PluginConfig.StaminaInfoShowPercentageSign.Value ? "%" : string.Empty;
    }

    private static RectTransform EnsureOverlay(Transform barRoot)
    {
        if (barRoot.Find(OverlayName) is RectTransform existing)
        {
            return existing;
        }

        GameObject overlayObject = new GameObject(OverlayName, typeof(RectTransform), typeof(Canvas));
        overlayObject.layer = barRoot.gameObject.layer;
        RectTransform overlay = overlayObject.GetComponent<RectTransform>();
        overlay.SetParent(barRoot, worldPositionStays: false);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.pivot = new Vector2(0.5f, 0.5f);
        overlay.localRotation = Quaternion.identity;
        overlay.localScale = Vector3.one;
        overlay.SetAsLastSibling();

        // Render the labels above every graphic of the bar, including the extra stamina bar
        // frame that otherwise covers the petrify value.
        Canvas canvas = overlayObject.GetComponent<Canvas>();
        Canvas? parentCanvas = barRoot.GetComponentInParent<Canvas>();
        canvas.overrideSorting = true;
        if (parentCanvas != null)
        {
            canvas.sortingLayerID = parentCanvas.sortingLayerID;
            canvas.sortingOrder = parentCanvas.sortingOrder + 1;
        }

        return overlay;
    }

    private static bool IsHudVisible()
    {
        GUIManager? gui = guiManager;
        if (gui == null)
        {
            return false;
        }

        if (gui.hudCanvas != null && !gui.hudCanvas.enabled)
        {
            return false;
        }

        if (gui.staminaCanvasGroup != null && gui.staminaCanvasGroup.alpha <= 0.01f)
        {
            return false;
        }

        return true;
    }

    private static void AddLabel(
        RectTransform overlay,
        string key,
        LabelKind kind,
        TextAlignmentOptions alignment,
        Vector2 pivot,
        Image? colorSource,
        Color fallbackColor,
        int colorIndex,
        TMP_FontAsset font,
        RectTransform? segment)
    {
        TextMeshProUGUI text = CreateLabelText(overlay, key, alignment, font);
        labels[key] = new LabelEntry
        {
            Text = text,
            Kind = kind,
            Overlay = overlay,
            ColorSource = colorSource,
            FallbackColor = fallbackColor,
            ColorIndex = colorIndex,
            Segment = segment,
        };

        text.rectTransform.pivot = pivot;
    }

    private static TextMeshProUGUI CreateLabelText(
        RectTransform overlay,
        string name,
        TextAlignmentOptions alignment,
        TMP_FontAsset font)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.layer = overlay.gameObject.layer;
        textObject.transform.SetParent(overlay, worldPositionStays: false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.richText = true;
        text.text = string.Empty;

        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;

        try
        {
            text.outlineWidth = PluginConfig.StaminaInfoOutlineWidth.Value;
        }
        catch
        {
        }

        textObject.SetActive(false);
        return text;
    }

    private static void SetTextRect(
        TextMeshProUGUI text,
        Vector2 pivot,
        Vector2 position,
        Vector2 size,
        float fontSize)
    {
        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = pivot;

        if (rectTransform.anchoredPosition != position)
        {
            rectTransform.anchoredPosition = position;
        }

        if (rectTransform.sizeDelta != size)
        {
            rectTransform.sizeDelta = size;
        }

        if (!Mathf.Approximately(text.fontSize, fontSize))
        {
            text.fontSize = fontSize;
        }
    }

    private static void SetLabelText(LabelEntry entry, string value)
    {
        if (entry.Text.text != value)
        {
            entry.Text.text = value;
        }

        if (!entry.Text.gameObject.activeSelf)
        {
            entry.Text.gameObject.SetActive(true);
        }
    }

    private static void ApplyColor(LabelEntry entry)
    {
        Color source = entry.ColorSource != null ? entry.ColorSource.color : entry.FallbackColor;
        entry.Text.color = ResolveLabelColor(source, entry.ColorIndex);
    }

    private static Color ResolveLabelColor(Color source, int index)
    {
        Color.RGBToHSV(source, out float hue, out float saturation, out float value);
        hue = Mathf.Repeat(hue + index * 0.011f, 1f);
        Color boosted = Color.HSVToRGB(hue, Mathf.Clamp01(saturation * 1.15f), Mathf.Max(0.8f, value));

        float luminance = 0.2126f * boosted.r + 0.7152f * boosted.g + 0.0722f * boosted.b;
        const float luminanceThreshold = 0.55f;
        if (luminance < luminanceThreshold)
        {
            boosted = Color.Lerp(boosted, Color.white, (luminanceThreshold - luminance) / (1f - luminanceThreshold));
        }

        boosted.a = 1f;
        return boosted;
    }

    private static TMP_FontAsset GetBarFont(StaminaBar? staminaBar)
    {
        if (staminaBar?.moraleBoostText != null && staminaBar.moraleBoostText.font != null)
        {
            return staminaBar.moraleBoostText.font;
        }

        return guiManager.heroDayText.font;
    }

    private static Color DefaultAfflictionColor(int index)
    {
        return Color.HSVToRGB(Mathf.Repeat(index * 0.618034f, 1f), 0.68f, 1f);
    }

    private static Image? FindFillImage(RectTransform? rectTransform)
    {
        if (rectTransform == null)
        {
            return null;
        }

        Transform? fill = rectTransform.Find("Stamina");
        if (fill != null && fill.TryGetComponent(out Image fillImage))
        {
            return fillImage;
        }

        return rectTransform.TryGetComponent(out Image ownImage) ? ownImage : null;
    }

    private static Image? FindAfflictionImage(BarAffliction affliction)
    {
        if (affliction.icon != null)
        {
            return affliction.icon;
        }

        if (affliction.rtf != null && affliction.rtf.TryGetComponent(out Image segmentImage))
        {
            return segmentImage;
        }

        return null;
    }

    private static Image? FindTeammateAfflictionImage(RectTransform segment)
    {
        Transform? icon = segment.Find("Icon");
        if (icon != null && icon.TryGetComponent(out Image iconImage))
        {
            return iconImage;
        }

        return FindFillImage(segment);
    }

    private static float GetLabelHeight(float fontSize)
    {
        return fontSize + 8f;
    }

    private static void HideLabelsWithPrefix(string prefix)
    {
        foreach (KeyValuePair<string, LabelEntry> pair in labels)
        {
            if (pair.Key.StartsWith(prefix, StringComparison.Ordinal))
            {
                pair.Value.Text.gameObject.SetActive(false);
            }
        }
    }

    private static string FormatBarValue(float size, bool roundLargeValue, bool trimWholeDecimal)
    {
        if (size >= LargeBarThreshold)
        {
            float value = size / BarUnitSize;
            if (roundLargeValue)
            {
                return Mathf.Round(value).ToString();
            }

            string valueText = value.ToString("F1");
            return trimWholeDecimal ? valueText.Replace(".0", "") : valueText;
        }

        return Mathf.Round(size / BarUnitSize).ToString();
    }

    private static bool CanShowCountdown(CharacterAfflictions.STATUSTYPE afflictionType)
    {
        return afflictionType != CharacterAfflictions.STATUSTYPE.Weight
            && afflictionType != CharacterAfflictions.STATUSTYPE.Injury
            && afflictionType != CharacterAfflictions.STATUSTYPE.Curse
            && afflictionType != CharacterAfflictions.STATUSTYPE.Hunger;
    }

    private static float GetReductionTimeRemaining(CharacterAfflictions.STATUSTYPE statusType)
    {
        CharacterAfflictions? afflictions = Character.observedCharacter?.refs?.afflictions;
        if (afflictions == null)
        {
            return 0f;
        }
        if (statusType == CharacterAfflictions.STATUSTYPE.Hunger)
        {
            Character? character = Character.observedCharacter;
            return character == null ? 0f : GetHungerIncreaseTimeRemaining(character, afflictions);
        }
        if (statusType == CharacterAfflictions.STATUSTYPE.Thorns)
        {
            float time = 0f;
            int count = 0;
            foreach (ThornOnMe item in afflictions.physicalThorns)
            {
                if (item.stuckIn)
                {
                    count++;
                    time += Time.time - item.popOutTime;
                }
            }
            if (count == 0)
            {
                return 0f;
            }
            return Mathf.Abs(time) / count;
        }
        float currentStatus = afflictions.GetCurrentStatus(statusType);
        if (currentStatus <= 0f)
        {
            return 0f;
        }
        // PEAK only applies a status reduction in 0.025 steps: the partially accumulated
        // decrease sits in currentDecrementalStatuses until it reaches a full step. Using the
        // stepped status alone makes the countdown jump once per step, so subtract the pending
        // decrease to keep the displayed time decreasing smoothly.
        currentStatus = Mathf.Max(0f, currentStatus - GetPendingDecrement(afflictions, statusType));
        if (currentStatus <= 0f)
        {
            return 0f;
        }
        float reductionRate = GetReductionRate(afflictions, statusType);
        if (reductionRate <= 0f)
        {
            return 0f;
        }
        float cooldown = GetCooldown(afflictions, statusType);
        float lastAddedTime = afflictions.LastAddedStatus(statusType);
        float currentTime = Time.time;
        if (cooldown > 0f && currentTime - lastAddedTime < cooldown)
        {
            float cooldownRemaining = cooldown - (currentTime - lastAddedTime);
            float reductionTime = currentStatus / reductionRate;
            return cooldownRemaining + reductionTime;
        }
        return currentStatus / reductionRate;
    }

    private static float GetPendingDecrement(CharacterAfflictions afflictions, CharacterAfflictions.STATUSTYPE statusType)
    {
        if (currentDecrementalStatusesField?.GetValue(afflictions) is not float[] decrementalStatuses)
        {
            return 0f;
        }

        int index = (int)statusType;
        if (index < 0 || index >= decrementalStatuses.Length)
        {
            return 0f;
        }

        return Mathf.Max(0f, decrementalStatuses[index]);
    }

    private static float GetHungerIncreaseTimeRemaining(Character character, CharacterAfflictions afflictions)
    {
        if (character == null ||
            character != Character.localCharacter ||
            !character.data.fullyConscious ||
            afflictions.m_inAirport)
        {
            return 0f;
        }

        float hungerRate = afflictions.hungerPerSecond * Ascents.hungerRateMultiplier;
        if (hungerRate <= 0f)
        {
            return 0f;
        }

        bool blockedByVanilla = character.data.isInvincible || !afflictions.canGetHungry;
        if (blockedByVanilla)
        {
            ResetRealHungerProgressWhenBlockedStateStarts(afflictions);
        }
        else
        {
            wasHungerBlockedByVanilla[afflictions.GetInstanceID()] = false;
        }

        bool hasRealProgress = HasRecentlyObservedRealHungerProgress(afflictions);
        if (blockedByVanilla && !hasRealProgress)
        {
            ClearFallbackHungerIncrementalStatus(afflictions);
        }

        float currentIncrementalStatus = GetHungerIncrementalStatus(afflictions);
        float remainingIncrement = HungerIncrementThreshold - currentIncrementalStatus;
        if (remainingIncrement <= 0f)
        {
            remainingIncrement = HungerIncrementThreshold;
        }

        return remainingIncrement / hungerRate;
    }

    private static void ResetRealHungerProgressWhenBlockedStateStarts(CharacterAfflictions afflictions)
    {
        int id = afflictions.GetInstanceID();
        if (wasHungerBlockedByVanilla.TryGetValue(id, out bool wasBlocked) && wasBlocked)
        {
            return;
        }

        wasHungerBlockedByVanilla[id] = true;
        lastRealHungerProgressTimes.Remove(id);
        if (TryReadCurrentIncrementalHunger(afflictions, out float currentIncrementalStatus))
        {
            lastObservedRealHungerIncrementalStatuses[id] = currentIncrementalStatus;
        }
    }

    private static bool HasRecentlyObservedRealHungerProgress(CharacterAfflictions afflictions)
    {
        int id = afflictions.GetInstanceID();
        if (TryReadCurrentIncrementalHunger(afflictions, out float currentIncrementalStatus))
        {
            if (lastObservedRealHungerIncrementalStatuses.TryGetValue(id, out float lastObservedStatus) &&
                currentIncrementalStatus > lastObservedStatus)
            {
                lastRealHungerProgressTimes[id] = Time.time;
            }

            lastObservedRealHungerIncrementalStatuses[id] = currentIncrementalStatus;
        }

        return lastRealHungerProgressTimes.TryGetValue(id, out float lastProgressTime) &&
            Time.time - lastProgressTime <= AfflictionRefreshInterval * 2f;
    }

    private static float GetHungerIncrementalStatus(CharacterAfflictions afflictions)
    {
        float fallbackIncrementalStatus = GetFallbackHungerIncrementalStatus(afflictions);
        if (TryReadCurrentIncrementalHunger(afflictions, out float currentIncrementalStatus))
        {
            return Mathf.Max(currentIncrementalStatus, fallbackIncrementalStatus);
        }

        return fallbackIncrementalStatus;
    }

    private static bool TryReadCurrentIncrementalHunger(CharacterAfflictions afflictions, out float currentIncrementalStatus)
    {
        currentIncrementalStatus = 0f;
        if (currentIncrementalStatusesField?.GetValue(afflictions) is not float[] currentIncrementalStatuses)
        {
            return false;
        }

        int hungerIndex = (int)CharacterAfflictions.STATUSTYPE.Hunger;
        if (hungerIndex < 0 || hungerIndex >= currentIncrementalStatuses.Length)
        {
            return false;
        }

        currentIncrementalStatus = currentIncrementalStatuses[hungerIndex];
        return true;
    }

    private static float GetReductionRate(CharacterAfflictions afflictions, CharacterAfflictions.STATUSTYPE statusType)
    {
        return statusType switch
        {
            CharacterAfflictions.STATUSTYPE.Poison => afflictions.poisonReductionPerSecond,
            CharacterAfflictions.STATUSTYPE.Hot => afflictions.hotReductionPerSecond,
            CharacterAfflictions.STATUSTYPE.Spores => afflictions.sporesReductionPerSecond,
            CharacterAfflictions.STATUSTYPE.Drowsy => afflictions.drowsyReductionPerSecond,
            _ => 0f,
        };
    }

    private static float GetCooldown(CharacterAfflictions afflictions, CharacterAfflictions.STATUSTYPE statusType)
    {
        return statusType switch
        {
            CharacterAfflictions.STATUSTYPE.Poison => afflictions.poisonReductionCooldown,
            CharacterAfflictions.STATUSTYPE.Hot => afflictions.hotReductionCooldown,
            CharacterAfflictions.STATUSTYPE.Spores => afflictions.sporesReductionCooldown,
            CharacterAfflictions.STATUSTYPE.Drowsy => afflictions.drowsyReductionCooldown,
            _ => 0f,
        };
    }

    private static string FormatTime(float seconds)
    {
        if (seconds < 60f)
        {
            return Mathf.CeilToInt(seconds).ToString();
        }
        if (seconds < 3600f)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int remainingSeconds = Mathf.CeilToInt(seconds % 60f);
            return $"{minutes}:{remainingSeconds:D2}";
        }
        int hours = Mathf.FloorToInt(seconds / 3600f);
        int minutes2 = Mathf.FloorToInt(seconds % 3600f / 60f);
        return $"{hours}:{minutes2:D2}";
    }
}

