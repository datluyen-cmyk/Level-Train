using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.Theme;
using UniLog;
using UnityEngine;
using UnlockRing;
using XingYun.Experiment;
using XingYun.UI.Core;

namespace RotateRings.GamePlay.Services;

public sealed class CumulativeReleaseService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayEffectAsync_003Ed__27 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CumulativeReleaseService _003C_003E4__this;

		public string effectLocation;

		public int epochAtRequest;

		private GameObject _003Cinstance_003E5__2;

		private Awaiter<GameObject> _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CumulativeReleaseService cumulativeReleaseService = _003C_003E4__this;
			try
			{
				if ((uint)num > 1u)
				{
					_003Cinstance_003E5__2 = null;
				}
				try
				{
					Awaiter<bool> val;
					GameObject value;
					Awaiter<GameObject> val2;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__2;
							_003C_003Eu__2 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							goto IL_01f0;
						}
						if (cumulativeReleaseService.effectPrefabs.TryGetValue(effectLocation, out value))
						{
							goto IL_00ce;
						}
						val2 = GameApp.Res.LoadAssetAsync<GameObject>(effectLocation, "UnlockRing_Effect").GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPlayEffectAsync_003Ed__27>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
					}
					value = val2.GetResult();
					if (!((Object)(object)value == (Object)null))
					{
						cumulativeReleaseService.effectPrefabs[effectLocation] = value;
						goto IL_00ce;
					}
					goto end_IL_001a;
					IL_00ce:
					if (cumulativeReleaseService.IsPlaybackValid(epochAtRequest))
					{
						RectTransform val3 = (((Object)(object)UIRoot.Instance != (Object)null) ? UIRoot.Instance.topRect : null);
						if (!((Object)(object)val3 == (Object)null))
						{
							_003Cinstance_003E5__2 = GameApp.Pool.Spawn(value, (Transform)(object)val3);
							if (!((Object)(object)_003Cinstance_003E5__2 == (Object)null))
							{
								cumulativeReleaseService.TrackActiveEffect(_003Cinstance_003E5__2);
								if (TrySetupFullscreenEffect(_003Cinstance_003E5__2))
								{
									RestartParticleSystems(_003Cinstance_003E5__2);
									_003Cinstance_003E5__2.transform.SetAsLastSibling();
									_003Cinstance_003E5__2.SetActive(true);
									UniTask val4 = UniTask.Delay(TimeSpan.FromSeconds(GetEffectDuration(_003Cinstance_003E5__2)), false, (PlayerLoopTiming)8, cumulativeReleaseService.DestroyToken);
									val = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
									if (!val.IsCompleted)
									{
										num = (_003C_003E1__state = 1);
										_003C_003Eu__2 = val;
										((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayEffectAsync_003Ed__27>(ref val, ref this);
										return;
									}
									goto IL_01f0;
								}
							}
						}
					}
					goto end_IL_001a;
					IL_01f0:
					if (!val.GetResult() && cumulativeReleaseService.IsPlaybackValid(epochAtRequest))
					{
					}
					end_IL_001a:;
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception ex2)
				{
					Log.LogError("[Gameplay][CumulativeRelease] ", "累计消除特效播放失败 location={0} error={1}", new object[2] { effectLocation, ex2 });
				}
				finally
				{
					if (num < 0)
					{
						cumulativeReleaseService.RecycleActiveEffect(_003Cinstance_003E5__2);
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cinstance_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cinstance_003E5__2 = null;
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

	private const string TAG = "[Gameplay][CumulativeRelease] ";

	private const string ConfettiEffectType1 = "eff_caidai_fullscreen_01_gp4";

	private const string ConfettiEffectType2 = "eff_caidai_fullscreen_02_gp4";

	private const string ConfettiEffectDay = "eff_caidai_fullscreen_03_gp4";

	private const string ConfettiEffectNight = "eff_caidai_fullscreen_04_gp4";

	private const string FeedbackAudioEvent = "rr_sfx_combo_clear_n_reward";

	private const float EffectDurationFallback = 2f;

	private readonly Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>(2);

	private readonly List<GameObject> activeEffectInstances = new List<GameObject>(2);

	private bool hasPendingPresentation;

	private bool deferPresentation;

	private int playbackEpoch;

	public int CurrentReleaseCount => GameApp.Proxy.Get<LevelModelProxy>()?.GetCurrentReleaseCount() ?? 0;

	public int TriggeredCount => GameApp.Proxy.Get<LevelModelProxy>()?.GetCumulativeReleaseTriggeredCount() ?? 0;

	public CumulativeReleaseService(GameplayManager host)
		: base(host)
	{
		GameApp.Event.Listen<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
	}

	public void BeginDeferPresentations()
	{
		deferPresentation = true;
	}

	public void EndDeferPresentations()
	{
		deferPresentation = false;
	}

	public void RecordStep(GameplayStepRecord step)
	{
		if (base.IsDisposed || !IsReleaseStep(step))
		{
			return;
		}
		OperationScoreFunction operationScoreFunction = FunctionDataCenter.Get<OperationScoreFunction>();
		int num = operationScoreFunction?.CumulativeReleaseThreshold ?? 0;
		int feedbackType = operationScoreFunction?.CumulativeReleaseType ?? 0;
		if (num <= 0 || !IsSupportedFeedbackType(feedbackType))
		{
			return;
		}
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null)
		{
			return;
		}
		levelModelProxy.IncrementCumulativeReleaseCount();
		if (levelModelProxy.GetCurrentReleaseCount() >= num)
		{
			levelModelProxy.ResetCurrentReleaseCount();
			levelModelProxy.IncrementCumulativeReleaseTriggeredCount();
			if (deferPresentation)
			{
				hasPendingPresentation = true;
			}
			else
			{
				TriggerPresentation(num, feedbackType);
			}
		}
	}

	public override void ClearLevel()
	{
		playbackEpoch++;
		deferPresentation = false;
		ClearDeferredPresentations();
		RecycleAllActiveEffects();
		GameApp.Proxy.Get<LevelModelProxy>()?.ResetCumulativeReleaseState();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			playbackEpoch++;
			deferPresentation = false;
			ClearDeferredPresentations();
			RecycleAllActiveEffects();
			effectPrefabs.Clear();
			GameApp.Event.Unlisten<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
			base.Dispose();
		}
	}

	private void OnBombBreakTick(BombBreakTickEvent evt)
	{
		if (base.IsDisposed || !hasPendingPresentation)
		{
			return;
		}
		hasPendingPresentation = false;
		int feedbackType = FunctionDataCenter.Get<OperationScoreFunction>()?.CumulativeReleaseType ?? 0;
		if (IsSupportedFeedbackType(feedbackType))
		{
			int num = FunctionDataCenter.Get<OperationScoreFunction>()?.CumulativeReleaseThreshold ?? 0;
			if (num > 0)
			{
				TriggerPresentation(num, feedbackType);
			}
		}
	}

	private void ClearDeferredPresentations()
	{
		hasPendingPresentation = false;
	}

	private static bool IsReleaseStep(GameplayStepRecord step)
	{
		if (step != null && step.State == GameplayStepState.UnlockRelease && step.ReleasedParts != null)
		{
			return step.ReleasedParts.Count > 0;
		}
		return false;
	}

	private static bool IsSupportedFeedbackType(int feedbackType)
	{
		if (feedbackType >= 1)
		{
			return feedbackType <= 3;
		}
		return false;
	}

	private void TriggerPresentation(int threshold, int feedbackType)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		AudioService.PlayUiSound("rr_sfx_combo_clear_n_reward");
		int epochAtRequest = playbackEpoch;
		string effectLocation = ((ThemeService.CurrentThemeId == ThemeId.Night) ? "eff_caidai_fullscreen_04_gp4" : "eff_caidai_fullscreen_03_gp4");
		UniTaskVoid val;
		switch (feedbackType)
		{
		case 1:
			val = PlayEffectAsync("eff_caidai_fullscreen_01_gp4", epochAtRequest);
			((UniTaskVoid)(ref val)).Forget();
			break;
		case 2:
			val = PlayEffectAsync(effectLocation, epochAtRequest);
			((UniTaskVoid)(ref val)).Forget();
			val = PlayEffectAsync("eff_caidai_fullscreen_02_gp4", epochAtRequest);
			((UniTaskVoid)(ref val)).Forget();
			break;
		case 3:
			val = PlayEffectAsync(effectLocation, epochAtRequest);
			((UniTaskVoid)(ref val)).Forget();
			val = PlayEffectAsync("eff_caidai_fullscreen_01_gp4", epochAtRequest);
			((UniTaskVoid)(ref val)).Forget();
			break;
		}
	}

	[AsyncStateMachine(typeof(_003CPlayEffectAsync_003Ed__27))]
	private UniTaskVoid PlayEffectAsync(string effectLocation, int epochAtRequest)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayEffectAsync_003Ed__27 _003CPlayEffectAsync_003Ed__ = default(_003CPlayEffectAsync_003Ed__27);
		_003CPlayEffectAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayEffectAsync_003Ed__._003C_003E4__this = this;
		_003CPlayEffectAsync_003Ed__.effectLocation = effectLocation;
		_003CPlayEffectAsync_003Ed__.epochAtRequest = epochAtRequest;
		_003CPlayEffectAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayEffectAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayEffectAsync_003Ed__27>(ref _003CPlayEffectAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayEffectAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool IsPlaybackValid(int epochAtRequest)
	{
		if (!base.IsDisposed)
		{
			return epochAtRequest == playbackEpoch;
		}
		return false;
	}

	private static bool TrySetupFullscreenEffect(GameObject instance)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)instance == (Object)null))
		{
			Transform transform = instance.transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (val != null)
			{
				val.anchorMin = Vector2.zero;
				val.anchorMax = Vector2.one;
				val.anchoredPosition = Vector2.zero;
				val.sizeDelta = Vector2.zero;
				((Transform)val).localRotation = Quaternion.identity;
				((Transform)val).localScale = Vector3.one;
				CanvasGroup val2 = default(CanvasGroup);
				if (!instance.TryGetComponent<CanvasGroup>(ref val2))
				{
					val2 = instance.AddComponent<CanvasGroup>();
				}
				val2.blocksRaycasts = false;
				return true;
			}
		}
		return false;
	}

	private static void RestartParticleSystems(GameObject instance)
	{
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem obj in componentsInChildren)
		{
			obj.Simulate(0f, true, true);
			obj.Play(true);
		}
	}

	private static float GetEffectDuration(GameObject instance)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MainModule main = componentsInChildren[i].main;
			MinMaxCurve startLifetime = ((MainModule)(ref main)).startLifetime;
			float num2;
			if ((int)((MinMaxCurve)(ref startLifetime)).mode != 3)
			{
				startLifetime = ((MainModule)(ref main)).startLifetime;
				num2 = ((MinMaxCurve)(ref startLifetime)).constant;
			}
			else
			{
				startLifetime = ((MainModule)(ref main)).startLifetime;
				num2 = ((MinMaxCurve)(ref startLifetime)).constantMax;
			}
			float num3 = num2;
			num = Mathf.Max(num, ((MainModule)(ref main)).duration + num3);
		}
		if (!(num > 0f))
		{
			return 2f;
		}
		return num;
	}

	private void TrackActiveEffect(GameObject instance)
	{
		if ((Object)(object)instance != (Object)null && !activeEffectInstances.Contains(instance))
		{
			activeEffectInstances.Add(instance);
		}
	}

	private void RecycleActiveEffect(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null) && activeEffectInstances.Remove(instance))
		{
			GameApp.Pool.Recycle(instance);
		}
	}

	private void RecycleAllActiveEffects()
	{
		for (int num = activeEffectInstances.Count - 1; num >= 0; num--)
		{
			GameObject val = activeEffectInstances[num];
			if ((Object)(object)val != (Object)null)
			{
				GameApp.Pool.Recycle(val);
			}
		}
		activeEffectInstances.Clear();
	}
}
