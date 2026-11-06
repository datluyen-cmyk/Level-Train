using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.GameDifficultyToast;

[UIPath("UIGameDifficultyToastView")]
[UIPath("UIGameDifficultyToastView_2")]
public class UIGameDifficultyToastController : GameBaseController<UIGameDifficultyToastView>
{
	public sealed class OpenParam
	{
		public ELevelDifficulty Difficulty;

		public int StyleId = 1;

		public Action OnClosed;
	}

	private const string Tag = "[UIGameDifficultyToast] ";

	private const float AutoCloseDuration = 2f;

	private const float DemonRingAutoCloseDuration = 1.2f;

	private static readonly (int styleId, ELevelDifficulty difficulty, string audioPath)[] s_audioMap = new(int, ELevelDifficulty, string)[4]
	{
		(2, ELevelDifficulty.Hard, AudioKeys.HardLevel.Value),
		(2, ELevelDifficulty.VeryHard, AudioKeys.HardLevel.Value),
		(3, ELevelDifficulty.Hard, AudioKeys.HardLevel.Value),
		(3, ELevelDifficulty.VeryHard, AudioKeys.HardLevel.Value)
	};

	private int autoCloseTimerId;

	private int currentStyleIdForTimer;

	private Action onClosedCallback;

	public UIGameDifficultyToastController(UIGameDifficultyToastView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		OpenParam openParam = param as OpenParam;
		onClosedCallback = openParam?.OnClosed;
		int styleId = (currentStyleIdForTimer = openParam?.StyleId ?? 1);
		((BaseController<UIGameDifficultyToastView>)this).view.SetStyleId(styleId);
		((BaseController<UIGameDifficultyToastView>)this).view.SetDifficulty(openParam?.Difficulty ?? ELevelDifficulty.Hard, styleId);
		PlayEntranceSound(styleId, openParam?.Difficulty ?? ELevelDifficulty.Hard);
		StartAutoCloseTimer();
	}

	public override void OnRemove()
	{
		((BaseController<UIGameDifficultyToastView>)this).view.StopSquirrelSpineSynchronization();
		StopAutoCloseTimer();
		Action action = onClosedCallback;
		onClosedCallback = null;
		base.OnRemove();
		action?.Invoke();
	}

	private void StartAutoCloseTimer()
	{
		StopAutoCloseTimer();
		float num = ((currentStyleIdForTimer == 3) ? 1.2f : 2f);
		autoCloseTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIGameDifficultyToastView>)this).view).Behaviour, num, (Action)OnAutoClose);
	}

	private void StopAutoCloseTimer()
	{
		if (autoCloseTimerId != 0)
		{
			GameApp.Timer.Kill(autoCloseTimerId);
			autoCloseTimerId = 0;
		}
	}

	private void OnAutoClose()
	{
		autoCloseTimerId = 0;
		((BaseController<UIGameDifficultyToastView>)this).CloseView(false);
	}

	private void PlayEntranceSound(int styleId, ELevelDifficulty difficulty)
	{
		for (int i = 0; i < s_audioMap.Length; i++)
		{
			(int, ELevelDifficulty, string) tuple = s_audioMap[i];
			if (tuple.Item1 == styleId && tuple.Item2 == difficulty)
			{
				if (!string.IsNullOrEmpty(tuple.Item3))
				{
					AudioService.PlayUiSound(tuple.Item3);
				}
				break;
			}
		}
	}
}
