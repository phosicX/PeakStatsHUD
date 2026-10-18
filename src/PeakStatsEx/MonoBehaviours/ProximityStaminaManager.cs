using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakStatsEx.MonoBehaviours;

internal sealed class ProximityStaminaManager : MonoBehaviour
{
	private const float ProximityRefreshInterval = 0.1f;
	private const string StaminaBarGroupName = "PeakStatsEx Stamina Bars";
	private const float StaminaBarGroupWidth = 600f;
	private const float StaminaBarGroupHeight = 1200f;

	private static RectTransform? staminaBarGroup;
	private static VerticalLayoutGroup? staminaBarGroupLayout;

	private readonly Dictionary<Character, CharacterStaminaBar> staminaBars = [];
	private readonly List<CharacterStaminaBar> selfStaminaBars = [];
	private readonly List<ProximityCandidate> proximityCandidates = [];
	private readonly HashSet<Character> selectedCharacters = [];
	private readonly HashSet<Character> currentRoster = [];
	private readonly List<Character> staleCharacters = [];
	private static readonly Comparison<ProximityCandidate> CompareProximityCandidates =
		(left, right) => left.SquaredDistance.CompareTo(right.SquaredDistance);

	private Character? lastTargetCharacter;
	private float lastProximityValue = -1f;
	private int lastMaxBarCount = -1;
	private float nextProximityRefreshTime;
	private bool teammateBarsWereEnabled;

	internal static void InitializeBarGroup(GUIManager guiManager)
	{
		RectTransform originalGroup = (RectTransform)guiManager.bar.transform.parent;
		if (staminaBarGroup != null && staminaBarGroup.parent == originalGroup.parent)
		{
			return;
		}

		Transform? existingGroup = originalGroup.parent.Find(StaminaBarGroupName);
		if (existingGroup != null && existingGroup.TryGetComponent(out RectTransform existingRectTransform) &&
			existingGroup.TryGetComponent(out VerticalLayoutGroup existingLayoutGroup))
		{
			staminaBarGroup = existingRectTransform;
			staminaBarGroupLayout = existingLayoutGroup;
			return;
		}

		GameObject groupObject = new GameObject(
			StaminaBarGroupName,
			typeof(RectTransform),
			typeof(VerticalLayoutGroup));
		staminaBarGroup = groupObject.GetComponent<RectTransform>();
		staminaBarGroup.SetParent(originalGroup.parent, worldPositionStays: false);
		staminaBarGroup.SetSiblingIndex(originalGroup.GetSiblingIndex());
		staminaBarGroup.anchorMin = originalGroup.anchorMin;
		staminaBarGroup.anchorMax = originalGroup.anchorMax;
		staminaBarGroup.pivot = originalGroup.pivot;
		staminaBarGroup.localRotation = originalGroup.localRotation;
		staminaBarGroup.localScale = originalGroup.localScale;
		staminaBarGroup.sizeDelta = new Vector2(StaminaBarGroupWidth, StaminaBarGroupHeight);
		staminaBarGroup.anchoredPosition = originalGroup.anchoredPosition;

		VerticalLayoutGroup originalLayoutGroup = originalGroup.GetComponent<VerticalLayoutGroup>();
		staminaBarGroupLayout = groupObject.GetComponent<VerticalLayoutGroup>();
		CopyLayoutSettings(originalLayoutGroup, staminaBarGroupLayout);
		staminaBarGroupLayout.spacing = CharacterStaminaBar.GetGroupSpacing();
		RectTransform originalBar = (RectTransform)guiManager.bar.transform;
		RectTransform originalBarSpacer = CreateOriginalBarSpacer(staminaBarGroup, originalBar);
		LayoutRebuilder.ForceRebuildLayoutImmediate(originalGroup);
		LayoutRebuilder.ForceRebuildLayoutImmediate(staminaBarGroup);
		AlignSpacerWithOriginalBar(originalBarSpacer, originalBar);
	}

	private static void CopyLayoutSettings(VerticalLayoutGroup source, VerticalLayoutGroup target)
	{
		target.padding = new RectOffset(
			source.padding.left,
			source.padding.right,
			source.padding.top,
			source.padding.bottom);
		target.childAlignment = source.childAlignment;
		target.reverseArrangement = source.reverseArrangement;
		target.childControlWidth = source.childControlWidth;
		target.childControlHeight = source.childControlHeight;
		target.childForceExpandWidth = source.childForceExpandWidth;
		target.childForceExpandHeight = source.childForceExpandHeight;
		target.childScaleWidth = source.childScaleWidth;
		target.childScaleHeight = source.childScaleHeight;
	}

	private static RectTransform CreateOriginalBarSpacer(RectTransform group, RectTransform originalBar)
	{
		GameObject spacerObject = new GameObject(
			"Original Stamina Bar Spacer",
			typeof(RectTransform),
			typeof(LayoutElement));
		RectTransform spacer = spacerObject.GetComponent<RectTransform>();
		spacer.SetParent(group, worldPositionStays: false);
		spacer.sizeDelta = originalBar.sizeDelta;

		LayoutElement layoutElement = spacerObject.GetComponent<LayoutElement>();
		layoutElement.minWidth = LayoutUtility.GetMinWidth(originalBar);
		layoutElement.minHeight = LayoutUtility.GetMinHeight(originalBar);
		layoutElement.preferredWidth = Mathf.Max(originalBar.rect.width, LayoutUtility.GetPreferredWidth(originalBar));
		layoutElement.preferredHeight = Mathf.Max(originalBar.rect.height, LayoutUtility.GetPreferredHeight(originalBar));
		layoutElement.flexibleWidth = LayoutUtility.GetFlexibleWidth(originalBar);
		layoutElement.flexibleHeight = LayoutUtility.GetFlexibleHeight(originalBar);
		return spacer;
	}

	private static void AlignSpacerWithOriginalBar(RectTransform spacer, RectTransform originalBar)
	{
		Transform groupParent = staminaBarGroup!.parent;
		Vector3 originalBarTop = originalBar.TransformPoint(
			new Vector3(originalBar.rect.center.x, originalBar.rect.yMax));
		Vector3 spacerTop = spacer.TransformPoint(
			new Vector3(spacer.rect.center.x, spacer.rect.yMax));
		float verticalOffset =
			groupParent.InverseTransformPoint(originalBarTop).y -
			groupParent.InverseTransformPoint(spacerTop).y;

		Vector2 position = staminaBarGroup.anchoredPosition;
		position.y += verticalOffset;
		staminaBarGroup.anchoredPosition = position;
	}

	private static RectTransform GetStaminaBarGroup()
	{
		if (staminaBarGroup == null)
		{
			InitializeBarGroup(GUIManager.instance);
		}

		return staminaBarGroup!;
	}

	private static CharacterStaminaBar CreateStaminaBar(Character observedCharacter)
	{
		Transform barTransform = GUIManager.instance.bar.transform;
		Transform barParentTransform = Instantiate(barTransform, GetStaminaBarGroup());
		Transform[] componentsInChildren = barParentTransform.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "StaminaInfo")
			{
				Destroy(transform.gameObject);
			}
		}
		barParentTransform.SetAsFirstSibling();
		Destroy(barParentTransform.GetComponent<StaminaBar>());

		CharacterStaminaBar characterStaminaBar = barParentTransform.gameObject.AddComponent<CharacterStaminaBar>();

		BarAffliction[] componentsInChildren2 = barParentTransform.GetComponentsInChildren<BarAffliction>(includeInactive: true);
		foreach (BarAffliction barAffliction in componentsInChildren2)
		{
			CharacterBarAffliction characterBarAffliction = barAffliction.gameObject.AddComponent<CharacterBarAffliction>();
			characterBarAffliction.afflictionType = barAffliction.afflictionType;
			characterBarAffliction.isPetrify = barAffliction.isPetrify;
			characterBarAffliction.FetchReferences();
			Destroy(barAffliction);
		}
		GameObject playerNameObject = new GameObject(
			"CharacterName",
			typeof(RectTransform),
			typeof(TextMeshProUGUI));
		RectTransform rectTransform = playerNameObject.GetComponent<RectTransform>();
		rectTransform.SetParent(barParentTransform, worldPositionStays: false);
		TextMeshProUGUI playerNameText = playerNameObject.GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI textTemplate = GUIManager.instance.heroDayText;
		playerNameText.font = textTemplate.font;
		playerNameText.fontSharedMaterial = textTemplate.fontSharedMaterial;
		string playerName = observedCharacter.characterName;
		playerName = Regex.Replace(playerName, @"(<color=#\w{6}>|</color>)", "");
		string text2 = playerNameText.text = playerName.TrimToByteLength(18);
		rectTransform.name = "CharacterName: " + text2;
		playerNameText.textWrappingMode = TextWrappingModes.NoWrap;
		playerNameText.overflowMode = TextOverflowModes.Ellipsis;
		playerNameText.alignment = TextAlignmentOptions.Left;
		playerNameText.fontSize = 32f;
		playerNameText.autoSizeTextContainer = false;
		playerNameText.rectTransform.sizeDelta = new Vector2(260f, playerNameText.rectTransform.sizeDelta.y);
		playerNameText.color = observedCharacter.refs.customization.PlayerColor;
		playerNameText.outlineColor = new Color32(0, 0, 0, byte.MaxValue);
		playerNameText.outlineWidth = 0.055f;
		characterStaminaBar.InitializeHeader(rectTransform);
		characterStaminaBar.ObservedCharacter = observedCharacter;
		characterStaminaBar.AnimateEnable();
		return characterStaminaBar;
	}

	private void Update()
	{
		if (!IsStaminaHudReady())
		{
			teammateBarsWereEnabled = false;
			return;
		}

		UpdateBarGroupLayout();

		if (!Character.localCharacter || !(PluginConfig.DisplayTeammateStaminaBars.Value || PluginConfig.DisplaySelfStaminaBar.Value))
		{
			DisableTeammateStaminaBars();
			DisableSelfStaminaBars();
			teammateBarsWereEnabled = false;
			return;
		}

		UpdateSelfStaminaBars(Character.localCharacter);

		if (!PluginConfig.DisplayTeammateStaminaBars.Value)
		{
			DisableTeammateStaminaBars();
			teammateBarsWereEnabled = false;
			return;
		}

		int maxBarCount = PluginConfig.TeammateStaminaBarLimit.Value;
		float proximityValue = PluginConfig.TeammateStaminaBarProximity.Value;

		Character targetCharacter = MainCameraMovement.IsSpectating && MainCameraMovement.specCharacter
			? MainCameraMovement.specCharacter
			: Character.localCharacter;
		bool selectionChanged =
			!teammateBarsWereEnabled ||
			lastTargetCharacter != targetCharacter ||
			lastMaxBarCount != maxBarCount ||
			!Mathf.Approximately(lastProximityValue, proximityValue);
		if (!selectionChanged && Time.unscaledTime < nextProximityRefreshTime)
		{
			return;
		}

		teammateBarsWereEnabled = true;
		lastTargetCharacter = targetCharacter;
		lastMaxBarCount = maxBarCount;
		lastProximityValue = proximityValue;
		nextProximityRefreshTime = Time.unscaledTime + ProximityRefreshInterval;
		RefreshTeammateStaminaBars(targetCharacter, maxBarCount, proximityValue);
	}

	private static bool IsStaminaHudReady()
	{
		return staminaBarGroup != null &&
			staminaBarGroup.gameObject.activeInHierarchy &&
			GUIManager.instance != null &&
			GUIManager.instance.bar != null &&
			GUIManager.instance.bar.gameObject.activeInHierarchy;
	}

	private void RefreshTeammateStaminaBars(Character targetCharacter, int maxBarCount, float proximityValue)
	{
		proximityCandidates.Clear();
		selectedCharacters.Clear();
		currentRoster.Clear();
		staleCharacters.Clear();

		Vector3 targetCenter = targetCharacter.Center;
		foreach (Character character in Character.AllCharacters)
		{
			if (!character || character.IsLocal)
			{
				continue;
			}

			currentRoster.Add(character);
			if (character.isActiveAndEnabled)
			{
				float squaredDistance = (targetCenter - character.Center).sqrMagnitude;
				proximityCandidates.Add(new ProximityCandidate(character, squaredDistance));
			}
		}

		foreach (KeyValuePair<Character, CharacterStaminaBar> entry in staminaBars)
		{
			if (!entry.Key || !entry.Value || !currentRoster.Contains(entry.Key))
			{
				staleCharacters.Add(entry.Key!);
			}
		}

		foreach (Character staleCharacter in staleCharacters)
		{
			if (staminaBars.TryGetValue(staleCharacter, out CharacterStaminaBar? staleBar) && staleBar)
			{
				Destroy(staleBar.gameObject);
			}
			staminaBars.Remove(staleCharacter);
		}

		if (maxBarCount > 0)
		{
			proximityCandidates.Sort(CompareProximityCandidates);
			float squaredProximity = proximityValue * proximityValue;
			for (int i = 0; i < proximityCandidates.Count && selectedCharacters.Count < maxBarCount; i++)
			{
				ProximityCandidate candidate = proximityCandidates[i];
				if (candidate.SquaredDistance >= squaredProximity)
				{
					break;
				}

				if (!staminaBars.TryGetValue(candidate.Character, out CharacterStaminaBar? staminaBar) || !staminaBar)
				{
					staminaBar = CreateStaminaBar(candidate.Character);
					staminaBars[candidate.Character] = staminaBar;
				}

				selectedCharacters.Add(candidate.Character);
				staminaBar.AnimateEnable();
			}
		}

		foreach (KeyValuePair<Character, CharacterStaminaBar> entry in staminaBars)
		{
			if (entry.Value && !selectedCharacters.Contains(entry.Key))
			{
				entry.Value.AnimateDisable();
			}
		}
	}

	private void DisableTeammateStaminaBars()
	{
		foreach (KeyValuePair<Character, CharacterStaminaBar> entry in staminaBars)
		{
			if (entry.Value)
			{
				entry.Value.AnimateDisable();
			}
		}
	}

	private void UpdateSelfStaminaBars(Character localCharacter)
	{
		for (int i = selfStaminaBars.Count - 1; i >= 0; i--)
		{
			CharacterStaminaBar selfStaminaBar = selfStaminaBars[i];
			if (selfStaminaBar == null)
			{
				selfStaminaBars.RemoveAt(i);
			}
			else if (selfStaminaBar.ObservedCharacter != localCharacter)
			{
				Destroy(selfStaminaBar.gameObject);
				selfStaminaBars.RemoveAt(i);
			}
		}

		int visibleCount = PluginConfig.DisplaySelfStaminaBar.Value ? PluginConfig.SelfStaminaBarCount.Value : 0;
		while (selfStaminaBars.Count < visibleCount)
		{
			selfStaminaBars.Add(CreateStaminaBar(localCharacter));
		}

		for (int i = 0; i < selfStaminaBars.Count; i++)
		{
			if (i < visibleCount)
			{
				selfStaminaBars[i].AnimateEnable();
			}
			else
			{
				selfStaminaBars[i].AnimateDisable();
			}
		}
	}

	private void DisableSelfStaminaBars()
	{
		foreach (CharacterStaminaBar selfStaminaBar in selfStaminaBars)
		{
			if (selfStaminaBar != null)
			{
				selfStaminaBar.AnimateDisable();
			}
		}
	}

	private static void UpdateBarGroupLayout()
	{
		if (staminaBarGroup == null || staminaBarGroupLayout == null)
		{
			return;
		}

		float spacing = CharacterStaminaBar.GetGroupSpacing();
		if (Mathf.Approximately(staminaBarGroupLayout.spacing, spacing))
		{
			return;
		}

		staminaBarGroupLayout.spacing = spacing;
		LayoutRebuilder.MarkLayoutForRebuild(staminaBarGroup);
	}

	private readonly struct ProximityCandidate
	{
		internal ProximityCandidate(Character character, float squaredDistance)
		{
			Character = character;
			SquaredDistance = squaredDistance;
		}

		internal Character Character { get; }
		internal float SquaredDistance { get; }
	}
}
