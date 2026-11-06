using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.UI;
using Game.UI.ClaimReward;
using RotateRings.UI.Common;
using Rotaterings.Tools;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.ReissueReward;

public class UIReissueRewardView : GameBaseView
{
	private const string Tag = "[UIReissueRewardView] ";

	private const string FlyPathCfgName = "UIClaimPropFlyBackPath";

	private const float FlyDuration = 0.5f;

	private const float FlyItemInterval = 0.15f;

	private const string RootOpenClipName = "anim_UIReissueReward_open";

	private const string RootCloseClipName = "anim_UIReissueReward_close";

	private UIPopupMaskFader maskFader;

	private PropFlyPathAsset _flyPathCfg;

	public GridLayoutGroup GridLayout;

	public Button btnTapContinue;

	public GameObject ReissueRewardItem;

	public Animation RootAnimation;

	public void Init()
	{
	}

	public void PlayRootOpen()
	{
		PlayRootClip("anim_UIReissueReward_open");
	}

	public float PlayRootClose()
	{
		return PlayRootClip("anim_UIReissueReward_close");
	}

	private float PlayRootClip(string clipName)
	{
		if ((Object)(object)RootAnimation == (Object)null || string.IsNullOrEmpty(clipName))
		{
			return 0f;
		}
		AnimationState val = RootAnimation[clipName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return 0f;
		}
		if (RootAnimation.IsPlaying(clipName))
		{
			RootAnimation.Stop(clipName);
			val.time = 0f;
			RootAnimation.Sample();
		}
		val.wrapMode = (WrapMode)8;
		RootAnimation.Play(clipName);
		return val.length;
	}

	public bool PlayCloseAndFlyItems(IReadOnlyList<RectTransform> items, RectTransform target, Action onFirstArrived, Action onCompleted)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (items == null || items.Count == 0 || (Object)(object)target == (Object)null)
		{
			return false;
		}
		for (int i = 0; i < items.Count; i++)
		{
			if ((Object)(object)items[i] != (Object)null)
			{
				((Transform)items[i]).SetParent(((Component)((UIBaseView)this).Behaviour).transform, true);
			}
		}
		PropFlyPathAsset flyPathCfg = GetFlyPathCfg();
		return RewardFlyUtility.Play(items, target, ((Component)((UIBaseView)this).Behaviour).gameObject, new RewardFlyUtility.Settings
		{
			StartDelay = 0f,
			ItemInterval = 0.15f,
			Duration = 0.5f,
			Ease = (Ease)(((Object)(object)flyPathCfg != (Object)null) ? 1 : 9),
			PathConfig = flyPathCfg,
			DeactivateOnArrive = true,
			DeactivateOnKill = true,
			OnFirstArrived = onFirstArrived,
			OnCompleted = onCompleted
		}) != null;
	}

	private PropFlyPathAsset GetFlyPathCfg()
	{
		if ((Object)(object)_flyPathCfg == (Object)null)
		{
			_flyPathCfg = GameApp.Res.LoadAsset<PropFlyPathAsset>("UIClaimPropFlyBackPath", (string)null);
			_ = (Object)(object)_flyPathCfg == (Object)null;
		}
		return _flyPathCfg;
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		GridLayout = behaviour.Get<GridLayoutGroup>("GridLayout");
		btnTapContinue = behaviour.Get<Button>("btnTapContinue");
		ReissueRewardItem = behaviour.Get<GameObject>("ReissueRewardItem");
		RootAnimation = behaviour.Get<Animation>("RootAnimation");
	}
}
