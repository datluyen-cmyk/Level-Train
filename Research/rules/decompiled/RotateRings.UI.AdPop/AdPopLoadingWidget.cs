using System;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.AdPop;

public class AdPopLoadingWidget : GameWidgetView
{
	public Button btnContinue;

	public TMP_Text txtRetry;

	public TMP_Text txtCountdown;

	public GameObject goAdBadge;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		btnContinue = GetOptional<Button>(behaviour, "btnContinue");
		txtRetry = GetOptional<TMP_Text>(behaviour, "txtRetry");
		txtCountdown = GetOptional<TMP_Text>(behaviour, "txtCountdown");
		goAdBadge = GetOptional<GameObject>(behaviour, "goAdBadge");
		if (txtCountdown == null)
		{
			txtCountdown = FindCountdownFallback(((Component)behaviour).transform);
		}
	}

	public void SetVisible(bool visible)
	{
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			((Component)((UIBaseView)this).Behaviour).gameObject.SetActive(visible);
		}
	}

	public void SetCountdown(int seconds)
	{
		if ((Object)(object)txtCountdown != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtCountdown, $"{seconds}s");
		}
	}

	public void SetRetryLocalized(string langKey)
	{
		TMPTextExtensions.SetLocalizedText(txtRetry, langKey, Array.Empty<object>());
	}

	private static T GetOptional<T>(UIBehaviour behaviour, string key) where T : Object
	{
		if (!behaviour.Contains(key))
		{
			return default(T);
		}
		return behaviour.Get<T>(key);
	}

	private static TMP_Text FindCountdownFallback(Transform root)
	{
		Transform val = FindTransform(root, "common_icon_badge_red");
		TMP_Text[] componentsInChildren;
		if ((Object)(object)((val != null) ? val.parent : null) != (Object)null)
		{
			componentsInChildren = ((Component)val.parent).GetComponentsInChildren<TMP_Text>(true);
			foreach (TMP_Text val2 in componentsInChildren)
			{
				if (!((Object)(object)val2.transform == (Object)(object)((Component)val).transform))
				{
					return val2;
				}
			}
		}
		componentsInChildren = ((Component)root).GetComponentsInChildren<TMP_Text>(true);
		foreach (TMP_Text val3 in componentsInChildren)
		{
			if (((Object)val3).name == "26" || int.TryParse(val3.text, out var _))
			{
				return val3;
			}
		}
		return null;
	}

	private static Transform FindTransform(Transform root, string name)
	{
		Transform[] componentsInChildren = ((Component)root).GetComponentsInChildren<Transform>(true);
		foreach (Transform val in componentsInChildren)
		{
			if (((Object)val).name == name)
			{
				return val;
			}
		}
		return null;
	}
}
