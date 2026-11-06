using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using Spine;
using Spine.Unity;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

public class UILevelEndCelebrateView : GameBaseView
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__20 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public int delayMilliseconds;

		public int effectVersion;

		public UILevelEndCelebrateView _003C_003E4__this;

		public string effectLocation;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILevelEndCelebrateView uILevelEndCelebrateView = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(delayMilliseconds, false, (PlayerLoopTiming)8, default(CancellationToken));
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__20>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				if (effectVersion == uILevelEndCelebrateView.celebrateEffectVersion)
				{
					UniTaskVoid val3 = uILevelEndCelebrateView.LoadAndPlayCelebrateEffectAsync(effectLocation, effectVersion);
					((UniTaskVoid)(ref val3)).Forget();
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoadAndPlayCelebrateEffectAsync_003Ed__21 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UILevelEndCelebrateView _003C_003E4__this;

		public string effectLocation;

		public int effectVersion;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILevelEndCelebrateView uILevelEndCelebrateView = _003C_003E4__this;
			try
			{
				Awaiter<GameObject> val;
				if (num != 0)
				{
					val = ((GameBaseView)uILevelEndCelebrateView).LoadAssetAsync<GameObject>(effectLocation).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CLoadAndPlayCelebrateEffectAsync_003Ed__21>(ref val, ref this);
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
				if (effectVersion == uILevelEndCelebrateView.celebrateEffectVersion && !((Object)(object)result == (Object)null))
				{
					RectTransform val2 = (RectTransform)(((Object)(object)uILevelEndCelebrateView.EffectNode != (Object)null) ? /*isinst with value type is only supported in some contexts*/: null);
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
							uILevelEndCelebrateView.celebrateEffectInstances.Add(val3);
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

	private const string Tag = "[UILevelEndCelebrateView] ";

	private const int SpineTrackIndex = 0;

	private const int MiddleEffectDelayMilliseconds = 300;

	private const string EdgeEffectLocation = "eff_results_fullscreen_edge_gp5";

	private const string MiddleEffectLocation = "eff_results_fullscreen_middle_gp5";

	private const string DeluxeEffectOneLocation = "eff_logo_fullscreen_Deluxe_1_gp5";

	private const string DeluxeEffectTwoLocation = "eff_logo_fullscreen_Deluxe_2_gp5";

	private const string LogoStartAnimation = "start";

	private const string SecondLogoCloseAnimation = "close";

	private const string BgMaskOpenAnimation = "anim_bgmask_open";

	private const string BgMaskCloseAnimation = "anim_bgmask_close";

	private readonly List<GameObject> celebrateEffectInstances = new List<GameObject>(2);

	private SkeletonGraphic activeLogoGraph;

	private TrackEntry activeLogoTrackEntry;

	private TrackEntryDelegate activeLogoCompleteHandler;

	private bool shouldPlayBackgroundAnimation;

	private int celebrateEffectVersion;

	public Animation bgAnim;

	public GameObject logoNode1;

	public GameObject logoNode2;

	public SkeletonGraphic SkGraph1;

	public SkeletonGraphic SkGraph2;

	public GameObject EffectNode;

	public void ConfigureLogo(int winCelebrateType)
	{
		bool flag = winCelebrateType == 4 || winCelebrateType == 6;
		if ((Object)(object)logoNode1 != (Object)null)
		{
			logoNode1.SetActive(!flag);
		}
		if ((Object)(object)logoNode2 != (Object)null)
		{
			logoNode2.SetActive(flag);
		}
		activeLogoGraph = (flag ? SkGraph2 : SkGraph1);
		shouldPlayBackgroundAnimation = winCelebrateType != 2;
		if ((Object)(object)bgAnim != (Object)null)
		{
			((Component)bgAnim).gameObject.SetActive(shouldPlayBackgroundAnimation);
		}
	}

	public void PlayCelebrateEffects(int winCelebrateType)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		RecycleCelebrateEffects();
		int effectVersion = celebrateEffectVersion;
		UniTaskVoid val;
		switch (winCelebrateType)
		{
		case 2:
			val = LoadAndPlayCelebrateEffectAsync("eff_results_fullscreen_edge_gp5", effectVersion);
			((UniTaskVoid)(ref val)).Forget();
			val = LoadAndPlayCelebrateEffectAfterDelayAsync("eff_results_fullscreen_middle_gp5", effectVersion, 300);
			((UniTaskVoid)(ref val)).Forget();
			break;
		case 3:
		case 5:
			val = LoadAndPlayCelebrateEffectAsync("eff_logo_fullscreen_Deluxe_2_gp5", effectVersion);
			((UniTaskVoid)(ref val)).Forget();
			val = LoadAndPlayCelebrateEffectAsync("eff_logo_fullscreen_Deluxe_1_gp5", effectVersion);
			((UniTaskVoid)(ref val)).Forget();
			break;
		}
	}

	public void RecycleCelebrateEffects()
	{
		celebrateEffectVersion++;
		for (int i = 0; i < celebrateEffectInstances.Count; i++)
		{
			GameObject val = celebrateEffectInstances[i];
			if ((Object)(object)val != (Object)null)
			{
				GameApp.Pool.Recycle(val);
			}
		}
		celebrateEffectInstances.Clear();
	}

	[AsyncStateMachine(typeof(_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__20))]
	private UniTaskVoid LoadAndPlayCelebrateEffectAfterDelayAsync(string effectLocation, int effectVersion, int delayMilliseconds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__20 _003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__ = default(_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__20);
		_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__._003C_003E4__this = this;
		_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__.effectLocation = effectLocation;
		_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__.effectVersion = effectVersion;
		_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__.delayMilliseconds = delayMilliseconds;
		_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__._003C_003Et__builder)).Start<_003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__20>(ref _003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CLoadAndPlayCelebrateEffectAfterDelayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CLoadAndPlayCelebrateEffectAsync_003Ed__21))]
	private UniTaskVoid LoadAndPlayCelebrateEffectAsync(string effectLocation, int effectVersion)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadAndPlayCelebrateEffectAsync_003Ed__21 _003CLoadAndPlayCelebrateEffectAsync_003Ed__ = default(_003CLoadAndPlayCelebrateEffectAsync_003Ed__21);
		_003CLoadAndPlayCelebrateEffectAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CLoadAndPlayCelebrateEffectAsync_003Ed__._003C_003E4__this = this;
		_003CLoadAndPlayCelebrateEffectAsync_003Ed__.effectLocation = effectLocation;
		_003CLoadAndPlayCelebrateEffectAsync_003Ed__.effectVersion = effectVersion;
		_003CLoadAndPlayCelebrateEffectAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CLoadAndPlayCelebrateEffectAsync_003Ed__._003C_003Et__builder)).Start<_003CLoadAndPlayCelebrateEffectAsync_003Ed__21>(ref _003CLoadAndPlayCelebrateEffectAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CLoadAndPlayCelebrateEffectAsync_003Ed__._003C_003Et__builder)).Task;
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

	private void PlayActiveLogoAnimation(string animationName, Action onCompleted, Action onUnavailable)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		if ((Object)(object)activeLogoGraph == (Object)null)
		{
			onUnavailable?.Invoke();
			return;
		}
		activeLogoGraph.Initialize(false);
		AnimationState animationState = activeLogoGraph.AnimationState;
		if (animationState != null)
		{
			Skeleton skeleton = activeLogoGraph.Skeleton;
			object obj;
			if (skeleton == null)
			{
				obj = null;
			}
			else
			{
				SkeletonData data = skeleton.Data;
				obj = ((data != null) ? data.FindAnimation(animationName) : null);
			}
			if (obj != null)
			{
				UnsubscribeLogoAnimationComplete();
				activeLogoTrackEntry = animationState.SetAnimation(0, animationName, false);
				activeLogoCompleteHandler = (TrackEntryDelegate)delegate
				{
					UnsubscribeLogoAnimationComplete();
					onCompleted?.Invoke();
				};
				activeLogoTrackEntry.Complete += activeLogoCompleteHandler;
				return;
			}
		}
		onUnavailable?.Invoke();
	}

	private void UnsubscribeLogoAnimationComplete()
	{
		if (activeLogoTrackEntry != null && activeLogoCompleteHandler != null)
		{
			activeLogoTrackEntry.Complete -= activeLogoCompleteHandler;
		}
		activeLogoTrackEntry = null;
		activeLogoCompleteHandler = null;
	}

	private void PlayBackgroundAnimation(string animationName)
	{
		if (shouldPlayBackgroundAnimation && !((Object)(object)bgAnim == (Object)null) && !((Object)(object)bgAnim.GetClip(animationName) == (Object)null))
		{
			bgAnim.Rewind(animationName);
			bgAnim.Play(animationName);
		}
	}

	public void PlayLogoOpen(Action onCompleted, Action onUnavailable)
	{
		PlayBackgroundAnimation("anim_bgmask_open");
		PlayActiveLogoAnimation("start", onCompleted, onUnavailable);
	}

	public void PlayLogoClose(Action onCompleted, Action onUnavailable)
	{
		PlayBackgroundAnimation("anim_bgmask_close");
		PlayActiveLogoAnimation("close", onCompleted, onUnavailable);
	}

	public override void Dispose()
	{
		RecycleCelebrateEffects();
		UnsubscribeLogoAnimationComplete();
		SkeletonGraphic skGraph = SkGraph1;
		if (skGraph != null)
		{
			AnimationState animationState = skGraph.AnimationState;
			if (animationState != null)
			{
				animationState.ClearTracks();
			}
		}
		SkeletonGraphic skGraph2 = SkGraph2;
		if (skGraph2 != null)
		{
			AnimationState animationState2 = skGraph2.AnimationState;
			if (animationState2 != null)
			{
				animationState2.ClearTracks();
			}
		}
		activeLogoGraph = null;
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		bgAnim = behaviour.Get<Animation>("bgAnim");
		logoNode1 = behaviour.Get<GameObject>("logoNode1");
		logoNode2 = behaviour.Get<GameObject>("logoNode2");
		SkGraph1 = behaviour.Get<SkeletonGraphic>("SkGraph1");
		SkGraph2 = behaviour.Get<SkeletonGraphic>("SkGraph2");
		EffectNode = behaviour.Get<GameObject>("EffectNode");
	}
}
