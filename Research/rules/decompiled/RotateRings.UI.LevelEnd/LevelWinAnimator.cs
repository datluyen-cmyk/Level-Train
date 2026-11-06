using System;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using UniLog;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

public sealed class LevelWinAnimator : IDisposable
{
	private sealed class StageState
	{
		public bool Started;

		public bool Completed;

		public readonly List<Action> StartActions = new List<Action>();

		public readonly List<Action> CompleteActions = new List<Action>();
	}

	private readonly ILevelWinAnimatorHost host;

	private readonly List<Tween> timers = new List<Tween>();

	private readonly List<int> originalTimerIds = new List<int>();

	private readonly StageState[] stages = new StageState[LevelWinAnimationSettings.StageCapacity];

	private readonly CancellationTokenSource playbackCts = new CancellationTokenSource();

	private LevelWinAnimationSettings settings;

	private int levelWinAnimation;

	private bool experiment;

	private bool disposed;

	public CancellationToken CancellationToken { get; }

	public LevelWinAnimator(ILevelWinAnimatorHost host)
	{
		this.host = host ?? throw new ArgumentNullException("host");
		CancellationToken = playbackCts.Token;
		for (int i = 0; i < stages.Length; i++)
		{
			stages[i] = new StageState();
		}
	}

	public void Play(int animationVariant, LevelWinAnimationSettings snapshot)
	{
		if (disposed || stages[0].Started)
		{
			return;
		}
		levelWinAnimation = animationVariant;
		experiment = animationVariant == 1 || animationVariant == 2;
		settings = snapshot;
		settings.Validate();
		Join(LevelWinAnimationStage.EntranceComplete, LevelWinAnimationStage.RootAppear, LevelWinAnimationStage.SquirrelOpen);
		Join(LevelWinAnimationStage.PanelsComplete, LevelWinAnimationStage.IqAppear, LevelWinAnimationStage.ChestAppear);
		LevelWinAnimationStage[] array = LevelWinAnimationSettings.Stages;
		foreach (LevelWinAnimationStage levelWinAnimationStage in array)
		{
			if (levelWinAnimationStage != LevelWinAnimationStage.Ready && !LevelWinAnimationSettings.IsJoin(levelWinAnimationStage))
			{
				Bind(levelWinAnimationStage, settings.GetTiming(levelWinAnimationStage));
			}
		}
		host.View.PlaySettlementSounds();
		Begin(LevelWinAnimationStage.Ready);
		Complete(LevelWinAnimationStage.Ready);
	}

	private void Bind(LevelWinAnimationStage stage, LevelWinAnimationTiming timing)
	{
		StageState stageState = stages[(int)timing.Reference];
		((timing.Anchor == LevelWinAnimationAnchor.Start) ? stageState.StartActions : stageState.CompleteActions).Add(delegate
		{
			Delay(timing.OffsetSeconds, delegate
			{
				Execute(stage);
			}, !experiment);
		});
	}

	private void Join(LevelWinAnimationStage target, LevelWinAnimationStage first, LevelWinAnimationStage second)
	{
		stages[(int)first].StartActions.Add(TryStart);
		stages[(int)second].StartActions.Add(TryStart);
		stages[(int)first].CompleteActions.Add(TryComplete);
		stages[(int)second].CompleteActions.Add(TryComplete);
		void TryComplete()
		{
			if (stages[(int)first].Completed && stages[(int)second].Completed)
			{
				Complete(target);
			}
		}
		void TryStart()
		{
			if (stages[(int)first].Started && stages[(int)second].Started)
			{
				Begin(target);
			}
		}
	}

	private void Execute(LevelWinAnimationStage stage)
	{
		if (disposed || stages[(int)stage].Started)
		{
			return;
		}
		try
		{
			UILevelWinView view = host.View;
			switch (stage)
			{
			case LevelWinAnimationStage.RootAppear:
				PlayClip(stage, view.PlayRootAppear);
				break;
			case LevelWinAnimationStage.SquirrelOpen:
				view.PlaySpineOpenIdle(null, levelWinAnimation, delegate
				{
					Begin(stage);
				}, delegate
				{
					Complete(stage);
				}, playbackCts.Token);
				break;
			case LevelWinAnimationStage.TopBar:
				PlayClip(stage, () => view.PlayTopBarAppear(!experiment));
				break;
			case LevelWinAnimationStage.ScoreRoll:
			{
				view.ShowScoreRegion();
				host.OnScorePhaseStarted();
				float duration = ((experiment && host.SettlementScore != 0) ? settings.ScoreRollDuration : 0f);
				Begin(stage);
				if (disposed)
				{
					break;
				}
				if (experiment)
				{
					view.PlayScoreScroll(0, host.SettlementScore, duration, delegate
					{
						Complete(stage);
					});
				}
				else
				{
					Complete(stage);
				}
				break;
			}
			case LevelWinAnimationStage.ScoreEmphasis:
				PlayClip(stage, () => (!experiment) ? 0f : view.PlayScoreEmphasis(settings.ScoreEmphasisClip));
				break;
			case LevelWinAnimationStage.Encouragement:
				PlayClip(stage, () => (!experiment) ? view.ShowEncouragement(animated: true) : view.PlayEncouragement(settings.EncouragementClip));
				break;
			case LevelWinAnimationStage.IqAppear:
				view.ShowGameWinIQ(host.IqSnapshot, playGrowthAutomatically: false);
				PlayClip(stage, () => (!experiment) ? 0f : view.PlayGameWinIqAppear(settings.IqAppearClip));
				break;
			case LevelWinAnimationStage.ChestAppear:
				PlayClip(stage, () => view.PlayBottomAppear(!experiment));
				break;
			case LevelWinAnimationStage.IqGrowth:
			{
				bool flag = host.IqSnapshot.ShouldShowUi && host.IqSnapshot.PlayGrowthAnimation && host.IqSnapshot.DisplayScore > host.IqSnapshot.PreviousDisplayScore;
				Begin(stage);
				if (disposed)
				{
					break;
				}
				if (flag)
				{
					view.PlayGameWinIqGrowth(settings.IqGrowthDuration, delegate
					{
						Complete(stage);
					});
				}
				else
				{
					Complete(stage);
				}
				break;
			}
			case LevelWinAnimationStage.ChestGrowth:
				Begin(stage);
				if (!disposed)
				{
					host.PlayChestProgress(settings.ChestGrowthDuration, delegate
					{
						Complete(stage);
					});
				}
				break;
			case LevelWinAnimationStage.NextButton:
				if (!host.OnContentAppearStarted())
				{
					Begin(stage);
					Complete(stage);
				}
				else
				{
					PlayClip(stage, () => view.PlayContentAppear(!experiment), host.OnContentAppearCompleted);
				}
				break;
			default:
				throw new InvalidOperationException($"结算动画阶段没有播放注册：{stage}");
			}
		}
		catch (Exception exception)
		{
			Fail(exception);
		}
	}

	private void PlayClip(LevelWinAnimationStage stage, Func<float> play, Action afterComplete = null)
	{
		float seconds = play();
		Begin(stage);
		Delay(seconds, delegate
		{
			Complete(stage);
			if (!disposed)
			{
				afterComplete?.Invoke();
			}
		});
	}

	private void Begin(LevelWinAnimationStage stage)
	{
		if (!disposed)
		{
			StageState stageState = stages[(int)stage];
			if (!stageState.Started)
			{
				stageState.Started = true;
				InvokeAndClear(stageState.StartActions);
			}
		}
	}

	private void Complete(LevelWinAnimationStage stage)
	{
		if (!disposed)
		{
			StageState stageState = stages[(int)stage];
			if (!stageState.Started)
			{
				Begin(stage);
			}
			if (!stageState.Completed)
			{
				stageState.Completed = true;
				InvokeAndClear(stageState.CompleteActions);
			}
		}
	}

	private void InvokeAndClear(List<Action> actions)
	{
		Action[] array = actions.ToArray();
		actions.Clear();
		Action[] array2 = array;
		foreach (Action action in array2)
		{
			if (!disposed)
			{
				action();
				continue;
			}
			break;
		}
	}

	private void Delay(float seconds, Action action, bool unscaled = false)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		if (disposed)
		{
			return;
		}
		if (seconds == 0f)
		{
			action();
			return;
		}
		if (!LevelWinAnimationTiming.IsFinite(seconds) || seconds < 0f)
		{
			throw new InvalidOperationException("结算动画等待秒数非法");
		}
		if (unscaled && !experiment)
		{
			originalTimerIds.Add(GameApp.Timer.Delay((Object)(object)((UIBaseView)host.View).Behaviour, seconds, (Action)delegate
			{
				if (disposed)
				{
					return;
				}
				try
				{
					action();
				}
				catch (Exception exception)
				{
					Fail(exception);
				}
			}));
			return;
		}
		timers.Add(DOVirtual.DelayedCall(seconds, (TweenCallback)delegate
		{
			if (disposed)
			{
				return;
			}
			try
			{
				action();
			}
			catch (Exception exception)
			{
				Fail(exception);
			}
		}, unscaled));
	}

	private void Fail(Exception exception)
	{
		Log.LogError("UI", "[LevelWinAnimator] 动画编排中止: {0}", new object[1] { exception });
		Dispose();
	}

	public void ShowContent()
	{
		Execute(LevelWinAnimationStage.NextButton);
	}

	public void Dispose()
	{
		if (disposed)
		{
			return;
		}
		disposed = true;
		playbackCts.Cancel();
		playbackCts.Dispose();
		foreach (Tween timer in timers)
		{
			if (timer != null)
			{
				TweenExtensions.Kill(timer, false);
			}
		}
		timers.Clear();
		foreach (int originalTimerId in originalTimerIds)
		{
			GameApp.Timer.Kill(originalTimerId);
		}
		originalTimerIds.Clear();
		StageState[] array = stages;
		foreach (StageState obj in array)
		{
			obj.StartActions.Clear();
			obj.CompleteActions.Clear();
		}
		host.View.StopLevelWinPresentation();
	}
}
