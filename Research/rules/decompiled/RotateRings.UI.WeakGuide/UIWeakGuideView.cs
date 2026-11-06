using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.WeakGuide;

public class UIWeakGuideView : GameBaseView
{
	private const string OpenAnimName = "anim_SingleToast_open";

	private const string CloseAnimName = "anim_SingleToast_close";

	public TMP_Text txtGuide;

	public VerticalLayoutGroup layout;

	public Animation anim;

	public void PlayOpenAnimation()
	{
		PlayClip("anim_SingleToast_open");
	}

	public void PlayCloseAnimation()
	{
		PlayClip("anim_SingleToast_close");
	}

	private void PlayClip(string clipName)
	{
		if (!((Object)(object)anim == (Object)null) && !string.IsNullOrEmpty(clipName) && !((Object)(object)anim.GetClip(clipName) == (Object)null))
		{
			anim.Stop();
			anim.Rewind(clipName);
			anim.Play(clipName, (PlayMode)4);
		}
	}

	public float GetClipDuration(string clipName)
	{
		if ((Object)(object)anim == (Object)null || string.IsNullOrEmpty(clipName))
		{
			return 0f;
		}
		AnimationClip clip = anim.GetClip(clipName);
		if (!((Object)(object)clip != (Object)null))
		{
			return 0f;
		}
		return clip.length;
	}

	public override void Dispose()
	{
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		txtGuide = behaviour.Get<TMP_Text>("txtGuide");
		layout = behaviour.Get<VerticalLayoutGroup>("layout");
		if ((Object)(object)layout != (Object)null)
		{
			anim = ((Component)layout).gameObject.GetComponent<Animation>();
		}
		if ((Object)(object)txtGuide != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtGuide, string.Empty);
		}
		if ((Object)(object)layout != (Object)null)
		{
			((Component)layout).gameObject.SetActive(false);
		}
	}
}
