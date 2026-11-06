using System;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

public class UIBestScoreView : GameWidgetView
{
	private const string BestScoreTextKey = "rotate_score_meirizuijia_1";

	private const string OpenIconClip = "anim_BestScore_open1";

	private const string OpenTextClip = "anim_BestScore_open2";

	private int cachedShowMode;

	public Image ImageIcon_1;

	public Image ImageIcon_2;

	public Image ImageScoreBg;

	public TMP_Text TextBestScore;

	public Animation Anim;

	public void PrepareHidden()
	{
		cachedShowMode = 0;
		ResetOpenAnimations();
		ApplyAllNodesHidden();
		SetRootActive(active: false);
	}

	public void PlayOnScorePhase(int showMode)
	{
		cachedShowMode = showMode;
		switch (showMode)
		{
		case 1:
			ApplyIconModeVisual();
			SetRootActive(active: true);
			PlayOpenAnimation(1);
			break;
		case 2:
			ApplyTextModeVisual();
			SetRootActive(active: true);
			PlayOpenAnimation(2);
			break;
		}
	}

	public void RefreshLocalizedText()
	{
		if (cachedShowMode == 2 && !((Object)(object)TextBestScore == (Object)null))
		{
			TMPTextExtensions.SetLocalizedText(TextBestScore, "rotate_score_meirizuijia_1", Array.Empty<object>());
		}
	}

	private void ApplyIconModeVisual()
	{
		if ((Object)(object)ImageIcon_1 != (Object)null)
		{
			((Component)ImageIcon_1).gameObject.SetActive(true);
		}
		if ((Object)(object)ImageIcon_2 != (Object)null)
		{
			((Component)ImageIcon_2).gameObject.SetActive(false);
		}
		if ((Object)(object)ImageScoreBg != (Object)null)
		{
			((Component)ImageScoreBg).gameObject.SetActive(false);
		}
		if ((Object)(object)TextBestScore != (Object)null)
		{
			((Component)TextBestScore).gameObject.SetActive(false);
		}
	}

	private void ApplyTextModeVisual()
	{
		if ((Object)(object)ImageIcon_1 != (Object)null)
		{
			((Component)ImageIcon_1).gameObject.SetActive(false);
		}
		if ((Object)(object)ImageIcon_2 != (Object)null)
		{
			((Component)ImageIcon_2).gameObject.SetActive(true);
		}
		if ((Object)(object)ImageScoreBg != (Object)null)
		{
			((Component)ImageScoreBg).gameObject.SetActive(true);
		}
		if ((Object)(object)TextBestScore != (Object)null)
		{
			((Component)TextBestScore).gameObject.SetActive(true);
			TMPTextExtensions.SetLocalizedText(TextBestScore, "rotate_score_meirizuijia_1", Array.Empty<object>());
		}
	}

	private void PlayOpenAnimation(int showMode)
	{
		string text = showMode switch
		{
			2 => "anim_BestScore_open2", 
			1 => "anim_BestScore_open1", 
			_ => null, 
		};
		if (!string.IsNullOrEmpty(text))
		{
			PlayClipWithRewind(Anim, text);
		}
	}

	private void ResetOpenAnimations()
	{
		if (!((Object)(object)Anim == (Object)null))
		{
			Anim.Stop();
			RewindAnimationClip(Anim, "anim_BestScore_open1");
			RewindAnimationClip(Anim, "anim_BestScore_open2");
		}
	}

	private void ApplyAllNodesHidden()
	{
		if ((Object)(object)ImageIcon_1 != (Object)null)
		{
			((Component)ImageIcon_1).gameObject.SetActive(false);
		}
		if ((Object)(object)ImageIcon_2 != (Object)null)
		{
			((Component)ImageIcon_2).gameObject.SetActive(false);
		}
		if ((Object)(object)ImageScoreBg != (Object)null)
		{
			((Component)ImageScoreBg).gameObject.SetActive(false);
		}
		if ((Object)(object)TextBestScore != (Object)null)
		{
			((Component)TextBestScore).gameObject.SetActive(false);
		}
	}

	private void SetRootActive(bool active)
	{
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			((Component)((UIBaseView)this).Behaviour).gameObject.SetActive(active);
		}
	}

	private static void PlayClipWithRewind(Animation animation, string clipName)
	{
		if (!((Object)(object)animation == (Object)null) && !string.IsNullOrEmpty(clipName))
		{
			AnimationState val = animation[clipName];
			if (!((TrackedReference)(object)val == (TrackedReference)null))
			{
				animation.Stop(clipName);
				val.time = 0f;
				animation.Sample();
				val.wrapMode = (WrapMode)8;
				animation.Play(clipName);
			}
		}
	}

	private static void RewindAnimationClip(Animation animation, string clipName)
	{
		if (!((Object)(object)animation == (Object)null) && !string.IsNullOrEmpty(clipName))
		{
			AnimationState val = animation[clipName];
			if (!((TrackedReference)(object)val == (TrackedReference)null))
			{
				animation.Stop(clipName);
				val.time = 0f;
				val.enabled = true;
				animation.Sample();
			}
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		ImageIcon_1 = behaviour.Get<Image>("ImageIcon_1");
		ImageIcon_2 = behaviour.Get<Image>("ImageIcon_2");
		ImageScoreBg = behaviour.Get<Image>("ImageScoreBg");
		TextBestScore = behaviour.Get<TMP_Text>("TextBestScore");
		Anim = behaviour.Get<Animation>("Anim");
	}
}
