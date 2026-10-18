using UnityEngine;

namespace PeakStatsEx.MonoBehaviours;

internal sealed class CharacterBarAffliction : MonoBehaviour
{
	internal CharacterAfflictions.STATUSTYPE afflictionType;

	internal bool isPetrify;

	private RectTransform rectTransform = null!;

	private CharacterStaminaBar characterStaminaBar = null!;

	internal float size;

	internal void FetchReferences()
	{
		rectTransform = (RectTransform)base.transform;
		base.transform.Find("Icon").gameObject.SetActive(false);
		characterStaminaBar = GetComponentInParent<CharacterStaminaBar>();
		characterStaminaBar.AddCharacterBarAffliction(this);
	}

	internal void FetchDesiredSize()
	{
		CharacterStaminaBar characterStaminaBar = this.characterStaminaBar;
		Character? character = characterStaminaBar.ObservedCharacter;
		if (!character)
		{
			return;
		}
		float currentStatus = isPetrify
			? character.data.petrifyAmount / 100f
			: character.refs.afflictions.GetCurrentStatus(afflictionType);
		size = this.characterStaminaBar.fullBarRectTransform.sizeDelta.x * currentStatus;
		if (currentStatus > 0.01f)
		{
			if (size < characterStaminaBar.minAfflictionWidth)
			{
				size = characterStaminaBar.minAfflictionWidth;
			}
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	internal void UpdateVisual()
	{
		RectTransform rectTransform = this.rectTransform;
		Vector2 sizeDelta = this.rectTransform.sizeDelta;
		sizeDelta.x = Mathf.Lerp(this.rectTransform.sizeDelta.x, size, Mathf.Min(Time.deltaTime * 10f, 0.1f));
		rectTransform.sizeDelta = sizeDelta;
	}
}
