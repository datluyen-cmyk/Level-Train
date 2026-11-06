using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace Rotaterings.UI.RankList;

public class UIRankListTipView : GameBaseView
{
	private string clipName;

	public Button btnClose;

	public Animation anim;

	public Image howtoplay_jigsaw03;

	public Image howtoplay_jigsaw02;

	public TMP_Text txtCountDown;

	public void InitAnimation()
	{
		if ((Object)(object)anim != (Object)null && (Object)(object)anim.clip != (Object)null)
		{
			clipName = ((Object)anim.clip).name;
		}
	}

	public bool IsAnimationPlaying()
	{
		if ((Object)(object)anim != (Object)null)
		{
			return anim.isPlaying;
		}
		return false;
	}

	public void SkipAnimation()
	{
		if (!((Object)(object)anim == (Object)null) && !string.IsNullOrEmpty(clipName))
		{
			anim[clipName].normalizedTime = 1f;
			anim.Sample();
			anim.Stop();
		}
	}

	public override void Dispose()
	{
		clipName = null;
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		btnClose = behaviour.Get<Button>("btnClose");
		anim = behaviour.Get<Animation>("anim");
		howtoplay_jigsaw03 = behaviour.Get<Image>("howtoplay_jigsaw03");
		howtoplay_jigsaw02 = behaviour.Get<Image>("howtoplay_jigsaw02");
		txtCountDown = behaviour.Get<TMP_Text>("txtCountDown");
	}
}
