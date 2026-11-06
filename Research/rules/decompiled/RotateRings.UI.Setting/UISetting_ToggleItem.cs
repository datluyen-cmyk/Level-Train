using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RotateRings.UI.Setting;

public class UISetting_ToggleItem : MonoBehaviour
{
	public Button BtnItem;

	public Animation AniItem;

	public Image ImageItemSwitchOn;

	public RectTransform ImageItemSwitch;

	private const float SwitchHandleOnX = 31.5f;

	private const float SwitchHandleOffX = -31.5f;

	private const float SwitchHandleY = -1f;

	private bool isAnimating;

	private Action<bool> onChange;

	public bool IsOn { get; private set; }

	public void Init(bool isOn, Action<bool> onChange)
	{
		this.onChange = onChange;
		IsOn = isOn;
		SetSwitchVisual(isOn);
	}

	private void OnEnable()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)BtnItem != (Object)null)
		{
			((UnityEvent)BtnItem.onClick).AddListener(new UnityAction(HandleClick));
		}
	}

	private void OnDisable()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)BtnItem != (Object)null)
		{
			((UnityEvent)BtnItem.onClick).RemoveListener(new UnityAction(HandleClick));
		}
		isAnimating = false;
	}

	private void HandleClick()
	{
		if (!isAnimating)
		{
			bool flag = !IsOn;
			isAnimating = true;
			PlaySwitchAnimation(flag);
			IsOn = flag;
			SetSwitchVisual(flag);
			onChange?.Invoke(flag);
			isAnimating = false;
		}
	}

	private void PlaySwitchAnimation(bool turnOn)
	{
		if (!((Object)(object)AniItem == (Object)null))
		{
			if (turnOn)
			{
				AniItem.Play("UI_UISettingNew_right");
			}
			else
			{
				AniItem.Play("UI_UISettingNew_left");
			}
		}
	}

	private void SetSwitchVisual(bool isOn)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ImageItemSwitchOn != (Object)null)
		{
			ImageItemSwitchOn.fillAmount = (isOn ? 1f : 0f);
		}
		if ((Object)(object)ImageItemSwitch != (Object)null)
		{
			ImageItemSwitch.anchoredPosition = new Vector2(isOn ? 31.5f : (-31.5f), -1f);
		}
	}
}
