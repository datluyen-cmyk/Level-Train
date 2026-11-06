using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RotateRings.UI.Setting;

public sealed class UISetting_IconToggleItem
{
	private readonly Button button;

	private readonly Action<bool> setVisual;

	private readonly Animation animation;

	private readonly string animationName;

	private bool isListening;

	private Action<bool> onChanged;

	public bool IsOn { get; private set; }

	public UISetting_IconToggleItem(Button button, Action<bool> setVisual, Animation animation = null, string animationName = null)
	{
		this.button = button;
		this.setVisual = setVisual;
		this.animation = animation;
		this.animationName = animationName;
	}

	public void Initialize(bool isOn, Action<bool> onChanged)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		this.onChanged = onChanged;
		IsOn = isOn;
		RefreshVisual();
		if (!isListening && !((Object)(object)button == (Object)null))
		{
			((UnityEvent)button.onClick).AddListener(new UnityAction(HandleClick));
			isListening = true;
		}
	}

	public void Dispose()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		if (isListening && (Object)(object)button != (Object)null)
		{
			((UnityEvent)button.onClick).RemoveListener(new UnityAction(HandleClick));
		}
		isListening = false;
		onChanged = null;
	}

	private void HandleClick()
	{
		IsOn = !IsOn;
		RefreshVisual();
		PlayAnimation();
		onChanged?.Invoke(IsOn);
	}

	private void PlayAnimation()
	{
		if (!((Object)(object)animation == (Object)null) && !string.IsNullOrEmpty(animationName))
		{
			animation.Play(animationName);
		}
	}

	private void RefreshVisual()
	{
		setVisual?.Invoke(IsOn);
	}
}
