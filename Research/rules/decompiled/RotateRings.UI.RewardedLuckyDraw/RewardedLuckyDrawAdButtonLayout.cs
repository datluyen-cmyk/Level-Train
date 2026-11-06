using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.UI.RewardedLuckyDraw;

internal static class RewardedLuckyDrawAdButtonLayout
{
	public static void Refresh(TMP_Text claimText)
	{
		if (!((Object)(object)claimText == (Object)null) && ((Behaviour)claimText).isActiveAndEnabled)
		{
			claimText.ForceMeshUpdate(false, false);
			LayoutRebuilder.ForceRebuildLayoutImmediate(claimText.rectTransform);
			Transform parent = ((Transform)claimText.rectTransform).parent;
			RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
			if ((Object)(object)val != (Object)null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(val);
			}
		}
	}
}
