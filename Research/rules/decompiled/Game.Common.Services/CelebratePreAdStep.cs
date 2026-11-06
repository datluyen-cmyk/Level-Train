using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.UI.LevelEnd;
using UnityEngine;
using XingYun.Experiment;
using XingYun.UI.Core;

namespace Game.Common.Services;

public class CelebratePreAdStep : ISettlementStep, IActivityStep
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayEffectAsync_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public string effectLocation;

		public CelebratePreAdStep _003C_003E4__this;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CelebratePreAdStep celebratePreAdStep = _003C_003E4__this;
			try
			{
				Awaiter<GameObject> val;
				if (num != 0)
				{
					val = GameApp.Res.LoadAssetAsync<GameObject>(effectLocation, (string)null).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPlayEffectAsync_003Ed__15>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
				}
				GameObject result = val.GetResult();
				if (!celebratePreAdStep.isPresentationCompleted && !((Object)(object)result == (Object)null))
				{
					RectTransform val2 = (((Object)(object)UIRoot.Instance != (Object)null) ? UIRoot.Instance.topRect : null);
					if (!((Object)(object)val2 == (Object)null))
					{
						GameObject val3 = GameApp.Pool.Spawn(result, (Transform)(object)val2);
						if (!((Object)(object)val3 == (Object)null))
						{
							val3.SetActive(false);
							Transform transform = val3.transform;
							RectTransform val4 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
							if (val4 != null)
							{
								val4.anchorMin = Vector2.zero;
								val4.anchorMax = Vector2.one;
								val4.anchoredPosition = Vector2.zero;
								val4.sizeDelta = Vector2.zero;
								((Transform)val4).localRotation = Quaternion.identity;
								((Transform)val4).localScale = Vector3.one;
							}
							val3.transform.SetAsLastSibling();
							val3.SetActive(true);
							celebratePreAdStep.effectInstances.Add(val3);
							RestartParticleSystems(val3);
						}
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string Tag = "[CelebratePreAdStep] ";

	private const string EdgeEffectLocation = "eff_results_fullscreen_edge_gp5";

	private const float EdgeEffectDuration = 1.2f;

	private const float CelebrateUiTimeout = 5f;

	private const int PriorityValue = 10;

	private readonly List<GameObject> effectInstances = new List<GameObject>(2);

	private int celebrateUiTimeoutTimerId = -1;

	private bool isPresentationCompleted;

	public int Priority => 10;

	public bool ShouldRun()
	{
		return LevelEndCelebrateService.IsEnabled();
	}

	public void Execute()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		isPresentationCompleted = false;
		StopCelebrateUiTimeout();
		RecycleEffects();
		int num = FunctionDataCenter.Get<LevelEndFunction>()?.WinCelebrateType ?? 0;
		LevelEndCelebrateService.PlayTransitionSound(num);
		switch (num)
		{
		case 1:
		{
			UniTaskVoid val = PlayEffectAsync("eff_results_fullscreen_edge_gp5");
			((UniTaskVoid)(ref val)).Forget();
			celebrateUiTimeoutTimerId = GameApp.Timer.Delay(1.2f, (Action)CompletePresentation);
			break;
		}
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
			celebrateUiTimeoutTimerId = GameApp.Timer.Delay(5f, (Action)CompletePresentation);
			GameApp.UI.Open("UILevelEndCelebrate", (UILayer)4, (object)new UILevelEndCelebrateController.OpenParam(num, CompletePresentation), (Action)null);
			break;
		default:
			CompletePresentation();
			break;
		}
	}

	private void CompletePresentation()
	{
		if (!isPresentationCompleted)
		{
			isPresentationCompleted = true;
			StopCelebrateUiTimeout();
			LevelEndCelebrateService.PlayEntrance(delegate
			{
				RecycleEffects();
				SettlementPipeline.ContinueNext();
			});
		}
	}

	private void StopCelebrateUiTimeout()
	{
		if (celebrateUiTimeoutTimerId != -1)
		{
			GameApp.Timer.Kill(celebrateUiTimeoutTimerId);
			celebrateUiTimeoutTimerId = -1;
		}
	}

	private void RecycleEffects()
	{
		for (int i = 0; i < effectInstances.Count; i++)
		{
			GameObject val = effectInstances[i];
			if ((Object)(object)val != (Object)null)
			{
				GameApp.Pool.Recycle(val);
			}
		}
		effectInstances.Clear();
	}

	[AsyncStateMachine(typeof(_003CPlayEffectAsync_003Ed__15))]
	private UniTaskVoid PlayEffectAsync(string effectLocation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayEffectAsync_003Ed__15 _003CPlayEffectAsync_003Ed__ = default(_003CPlayEffectAsync_003Ed__15);
		_003CPlayEffectAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayEffectAsync_003Ed__._003C_003E4__this = this;
		_003CPlayEffectAsync_003Ed__.effectLocation = effectLocation;
		_003CPlayEffectAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayEffectAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayEffectAsync_003Ed__15>(ref _003CPlayEffectAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayEffectAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static void RestartParticleSystems(GameObject effectInstance)
	{
		ParticleSystem[] componentsInChildren = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem obj in componentsInChildren)
		{
			obj.Stop(true, (ParticleSystemStopBehavior)0);
			obj.Simulate(0f, true, true);
			obj.Play(true);
		}
	}
}
