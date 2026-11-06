using System;
using Game.UI;
using RotateRings.Ad;
using RotateRings.Command.LevelStepLimit;
using RotateRings.Common;
using RotateRings.GamePlay.Services.LevelStepLimit;
using RotateRings.Services;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelStepLimit;

[UIPath("UILevelStepLimitPop")]
public class UILevelStepLimitPopController : GameBaseController<UILevelStepLimitPopView>
{
	private const string Tag = "[UILevelStepLimitPop] ";

	private const string RewardAdPosition = "局中激励--限定步数加步";

	private bool clicked;

	private bool isClosing;

	private bool isFreeGift;

	public UILevelStepLimitPopController(UILevelStepLimitPopView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		((BaseController<UILevelStepLimitPopView>)this).view.SetMaxSteps(levelStepLimitService?.MaxSteps ?? 0);
		((BaseController<UILevelStepLimitPopView>)this).view.SetupLocalizedTexts(levelStepLimitService?.MaxSteps ?? 0, levelStepLimitService?.AddStepsPerAd ?? 0);
		isFreeGift = levelStepLimitService?.CanClaimFreeGift ?? false;
		((BaseController<UILevelStepLimitPopView>)this).view.SetFreeGiftVisible(isFreeGift);
		if ((Object)(object)((BaseController<UILevelStepLimitPopView>)this).view.BtnAddFreeMoves != (Object)null)
		{
			((UnityEvent)((BaseController<UILevelStepLimitPopView>)this).view.BtnAddFreeMoves.onClick).AddListener(new UnityAction(OnFreeMovesClick));
		}
		if ((Object)(object)((BaseController<UILevelStepLimitPopView>)this).view.BtnAddMovesByAd != (Object)null)
		{
			((UnityEvent)((BaseController<UILevelStepLimitPopView>)this).view.BtnAddMovesByAd.onClick).AddListener(new UnityAction(OnAdMovesClick));
		}
		if ((Object)(object)((BaseController<UILevelStepLimitPopView>)this).view.BtnRestart != (Object)null)
		{
			((UnityEvent)((BaseController<UILevelStepLimitPopView>)this).view.BtnRestart.onClick).AddListener(new UnityAction(OnRestartClick));
		}
		clicked = false;
		isClosing = false;
		((BaseController<UILevelStepLimitPopView>)this).view.PlayOpenAnimation();
	}

	private void OnFreeMovesClick()
	{
		if (!clicked && !isClosing)
		{
			clicked = true;
			((BaseController<UILevelStepLimitPopView>)this).view.SetButtonsInteractable(interactable: false);
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			((BaseController<UILevelStepLimitPopView>)this).ExecuteCommand<LevelStepLimitClaimFreeGiftCommand>((object)null);
			CloseWithAnimation();
		}
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UILevelStepLimitPopView>)this).OnActive(isActive);
		((BaseController<UILevelStepLimitPopView>)this).view.OnUiStackActiveChanged(isActive);
	}

	private void OnAdMovesClick()
	{
		if (clicked || isClosing)
		{
			return;
		}
		clicked = true;
		((BaseController<UILevelStepLimitPopView>)this).view.SetButtonsInteractable(interactable: false);
		AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
		bool rewardAdSucceeded = false;
		AdService.ShowRewardAd(new RewardAdRequest
		{
			PopPosition = AdPopPosition.LevelStepLimitAddSteps,
			Callback = delegate(RewardAdResult result)
			{
				try
				{
					if (result.IsSuccess)
					{
						rewardAdSucceeded = true;
						((BaseController<UILevelStepLimitPopView>)this).ExecuteCommand<LevelStepLimitAddStepsCommand>((object)null);
						CloseWithAnimation();
					}
					else
					{
						clicked = false;
						((BaseController<UILevelStepLimitPopView>)this).view.SetButtonsInteractable(interactable: true);
					}
				}
				catch (Exception ex)
				{
					Log.LogError("[UILevelStepLimitPop] ", "看广告加步回调异常: {0}", new object[1] { ex });
					CloseWithAnimation();
				}
			}
		}, delegate
		{
			if (!rewardAdSucceeded && !isClosing)
			{
				clicked = false;
				((BaseController<UILevelStepLimitPopView>)this).view.SetButtonsInteractable(interactable: true);
			}
		});
	}

	private void OnRestartClick()
	{
		if (!clicked && !isClosing)
		{
			clicked = true;
			((BaseController<UILevelStepLimitPopView>)this).view.SetButtonsInteractable(interactable: false);
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			CloseWithAnimation();
			Gameplay.Active?.ReplayCurrentLevel();
		}
	}

	private void CloseWithAnimation()
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		((BaseController<UILevelStepLimitPopView>)this).view.PlayCloseAnimation(delegate
		{
			if (((BaseController<UILevelStepLimitPopView>)this).view != null)
			{
				((BaseController<UILevelStepLimitPopView>)this).CloseView(false);
			}
		});
	}

	public override void OnRemove()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		if ((Object)(object)((BaseController<UILevelStepLimitPopView>)this).view.BtnAddFreeMoves != (Object)null)
		{
			((UnityEvent)((BaseController<UILevelStepLimitPopView>)this).view.BtnAddFreeMoves.onClick).RemoveListener(new UnityAction(OnFreeMovesClick));
		}
		if ((Object)(object)((BaseController<UILevelStepLimitPopView>)this).view.BtnAddMovesByAd != (Object)null)
		{
			((UnityEvent)((BaseController<UILevelStepLimitPopView>)this).view.BtnAddMovesByAd.onClick).RemoveListener(new UnityAction(OnAdMovesClick));
		}
		if ((Object)(object)((BaseController<UILevelStepLimitPopView>)this).view.BtnRestart != (Object)null)
		{
			((UnityEvent)((BaseController<UILevelStepLimitPopView>)this).view.BtnRestart.onClick).RemoveListener(new UnityAction(OnRestartClick));
		}
		base.OnRemove();
	}

	public override void OnEscape()
	{
	}
}
