using UnityEngine;

namespace RotateRings.Common;

public static class UIAnimUtil
{
	public const string DefaultOpenAnimName = "ui_open";

	public static void PlayUIOpenAnimation(Animation anim, string animName = "ui_open")
	{
		if (!((Object)(object)anim == (Object)null))
		{
			if (string.IsNullOrEmpty(animName))
			{
				animName = "ui_open";
			}
			if ((Object)(object)anim.GetClip(animName) != (Object)null)
			{
				anim.Play(animName);
			}
		}
	}
}
