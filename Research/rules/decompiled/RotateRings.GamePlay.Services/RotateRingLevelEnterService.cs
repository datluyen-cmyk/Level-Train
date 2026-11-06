using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RotateRings.Common;
using RotateRings.Services;
using RotateRings.UI.GameEnterToast.Core;
using UniLog;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RotateRingLevelEnterService : GameplayServiceBase<GameplayManager>
{
	private struct RingPose
	{
		public Transform Transform;

		public Vector3 OriginalScale;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass13_0
	{
		public RotateRingLevelEnterService _003C_003E4__this;

		public RingLevelConfig animConfig;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass13_1
	{
		public float t;

		public Transform ring;

		public Vector3 originalScale;

		public MonoBehaviour behaviour;

		public _003C_003Ec__DisplayClass13_0 CS_0024_003C_003E8__locals1;

		internal float _003CPlayAsync_003Eb__1()
		{
			return t;
		}

		internal void _003CPlayAsync_003Eb__2(float value)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			t = value;
			CS_0024_003C_003E8__locals1.animConfig.ApplyRingWaveScale(ring, originalScale, t);
			float factor = Mathf.LerpUnclamped(CS_0024_003C_003E8__locals1.animConfig.startScaleFactor, 1f, CS_0024_003C_003E8__locals1.animConfig.scaleCurve.Evaluate(t));
			CS_0024_003C_003E8__locals1._003C_003E4__this.Host.LockChain?.NotifyRingScaleFactor(behaviour as RingObject, factor);
			CS_0024_003C_003E8__locals1._003C_003E4__this.Host.PaletteLock?.NotifyRingKeyScaleFactor(behaviour as RingObject, factor);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayAsync_003Ed__13 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RotateRingLevelEnterService _003C_003E4__this;

		public IReadOnlyList<IGameplayObject> objects;

		private int _003CplayId_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_0380: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_0389: Unknown result type (might be due to invalid IL or missing references)
			//IL_038e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0392: Unknown result type (might be due to invalid IL or missing references)
			//IL_0397: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RotateRingLevelEnterService rotateRingLevelEnterService = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0135;
				}
				_003C_003Ec__DisplayClass13_0 _003C_003Ec__DisplayClass13_ = new _003C_003Ec__DisplayClass13_0
				{
					_003C_003E4__this = _003C_003E4__this
				};
				CancellationToken token = default(CancellationToken);
				List<MonoBehaviour> list = default(List<MonoBehaviour>);
				if (rotateRingLevelEnterService.sessionCts != null && !rotateRingLevelEnterService.IsDisposed)
				{
					token = rotateRingLevelEnterService.sessionCts.Token;
					rotateRingLevelEnterService.CancelAnimation();
					if (objects != null && objects.Count != 0)
					{
						list = new List<MonoBehaviour>(objects.Count);
						for (int i = 0; i < objects.Count; i++)
						{
							IGameplayObject gameplayObject = objects[i];
							MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
							if (val != null && (Object)(object)val != (Object)null)
							{
								list.Add(val);
							}
						}
						if (list.Count != 0)
						{
							list.Sort((MonoBehaviour a, MonoBehaviour b) => ((Component)b).transform.position.y.CompareTo(((Component)a).transform.position.y));
							_003CplayId_003E5__2 = ++rotateRingLevelEnterService.activeEnterPlayId;
							rotateRingLevelEnterService.animatedRings.Clear();
							rotateRingLevelEnterService.DisablePhysicColliders(objects, _003CplayId_003E5__2);
							rotateRingLevelEnterService.activeSequence = DOTween.Sequence();
							goto IL_0135;
						}
					}
				}
				goto end_IL_000e;
				IL_0135:
				try
				{
					Awaiter<bool> val3;
					if (num != 0)
					{
						_003C_003Ec__DisplayClass13_.animConfig = rotateRingLevelEnterService.Config;
						int num2 = 0;
						while (true)
						{
							if (num2 >= list.Count)
							{
								float dynamicWaveInterval = GetDynamicWaveInterval(_003C_003Ec__DisplayClass13_.animConfig, list.Count);
								int num3 = 0;
								while (true)
								{
									if (num3 >= list.Count)
									{
										GameObject gameObject = ((Component)list[0]).gameObject;
										TweenSettingsExtensions.SetLink<Sequence>(TweenSettingsExtensions.SetTarget<Sequence>(rotateRingLevelEnterService.activeSequence, (object)gameObject), gameObject, (LinkBehaviour)5);
										TweenExtensions.Play<Sequence>(rotateRingLevelEnterService.activeSequence);
										BgmController.RestartLevel();
										rotateRingLevelEnterService.PlayRoundStartSound();
										UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(GetDynamicTotalDuration(_003C_003Ec__DisplayClass13_.animConfig, list.Count)), false, (PlayerLoopTiming)8, token);
										val3 = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
										if (val3.IsCompleted)
										{
											break;
										}
										num = (_003C_003E1__state = 0);
										_003C_003Eu__1 = val3;
										((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayAsync_003Ed__13>(ref val3, ref this);
										return;
									}
									_003C_003Ec__DisplayClass13_1 CS_0024_003C_003E8__locals21 = new _003C_003Ec__DisplayClass13_1
									{
										CS_0024_003C_003E8__locals1 = _003C_003Ec__DisplayClass13_
									};
									if (!token.IsCancellationRequested)
									{
										CS_0024_003C_003E8__locals21.behaviour = list[num3];
										CS_0024_003C_003E8__locals21.ring = ((Component)CS_0024_003C_003E8__locals21.behaviour).transform;
										CS_0024_003C_003E8__locals21.originalScale = rotateRingLevelEnterService.animatedRings[num3].OriginalScale;
										CS_0024_003C_003E8__locals21.t = 0f;
										TweenerCore<float, float, FloatOptions> val4 = TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => CS_0024_003C_003E8__locals21.t), (DOSetter<float>)delegate(float value)
										{
											//IL_0019: Unknown result type (might be due to invalid IL or missing references)
											CS_0024_003C_003E8__locals21.t = value;
											CS_0024_003C_003E8__locals21.CS_0024_003C_003E8__locals1.animConfig.ApplyRingWaveScale(CS_0024_003C_003E8__locals21.ring, CS_0024_003C_003E8__locals21.originalScale, CS_0024_003C_003E8__locals21.t);
											float factor = Mathf.LerpUnclamped(CS_0024_003C_003E8__locals21.CS_0024_003C_003E8__locals1.animConfig.startScaleFactor, 1f, CS_0024_003C_003E8__locals21.CS_0024_003C_003E8__locals1.animConfig.scaleCurve.Evaluate(CS_0024_003C_003E8__locals21.t));
											CS_0024_003C_003E8__locals21.CS_0024_003C_003E8__locals1._003C_003E4__this.Host.LockChain?.NotifyRingScaleFactor(CS_0024_003C_003E8__locals21.behaviour as RingObject, factor);
											CS_0024_003C_003E8__locals21.CS_0024_003C_003E8__locals1._003C_003E4__this.Host.PaletteLock?.NotifyRingKeyScaleFactor(CS_0024_003C_003E8__locals21.behaviour as RingObject, factor);
										}, 1f, CS_0024_003C_003E8__locals21.CS_0024_003C_003E8__locals1.animConfig.waveDuration), (Ease)1), (object)((Component)CS_0024_003C_003E8__locals21.ring).gameObject), ((Component)CS_0024_003C_003E8__locals21.ring).gameObject, (LinkBehaviour)5);
										TweenSettingsExtensions.Insert(rotateRingLevelEnterService.activeSequence, (float)num3 * dynamicWaveInterval, (Tween)(object)val4);
										num3++;
										continue;
									}
									rotateRingLevelEnterService.RestorePoses();
									goto end_IL_0135;
								}
								break;
							}
							if (!token.IsCancellationRequested)
							{
								MonoBehaviour val5 = list[num2];
								Transform transform = ((Component)val5).transform;
								Vector3 localScale = transform.localScale;
								rotateRingLevelEnterService.animatedRings.Add(new RingPose
								{
									Transform = transform,
									OriginalScale = localScale
								});
								_003C_003Ec__DisplayClass13_.animConfig.ApplyInitialAllRingsScale(transform, localScale);
								rotateRingLevelEnterService.Host.LockChain?.NotifyRingScaleFactor(val5 as RingObject, _003C_003Ec__DisplayClass13_.animConfig.initialAllRingsScaleFactor);
								rotateRingLevelEnterService.Host.PaletteLock?.NotifyRingKeyScaleFactor(val5 as RingObject, _003C_003Ec__DisplayClass13_.animConfig.initialAllRingsScaleFactor);
								num2++;
								continue;
							}
							rotateRingLevelEnterService.RestorePoses();
							goto end_IL_0135;
						}
					}
					else
					{
						val3 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (val3.GetResult())
					{
						rotateRingLevelEnterService.RestorePoses();
					}
					end_IL_0135:;
				}
				catch (Exception ex)
				{
					Log.LogError("[RingLevelEnterService] ", "入场动画异常: {0}", new object[1] { ex });
					rotateRingLevelEnterService.RestorePoses();
				}
				finally
				{
					if (num < 0)
					{
						rotateRingLevelEnterService.EnablePhysicColliders(_003CplayId_003E5__2);
						rotateRingLevelEnterService.activeSequence = null;
						rotateRingLevelEnterService.animatedRings.Clear();
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string TAG = "[RingLevelEnterService] ";

	private const string EnterTag = "[EnterAnim] ";

	private readonly List<RingPose> animatedRings = new List<RingPose>();

	private readonly List<IGameplayObject> enterPhysicColliderDisabledObjects = new List<IGameplayObject>();

	private CancellationTokenSource sessionCts;

	private Sequence activeSequence;

	private int activeEnterPlayId;

	private int enterPhysicColliderDisabledPlayId;

	private const int ForceEnablePhysicCollidersOwner = -1;

	private RingLevelConfig Config => RingLevelConfig.Instance;

	public RotateRingLevelEnterService(GameplayManager host)
		: base(host)
	{
	}

	[AsyncStateMachine(typeof(_003CPlayAsync_003Ed__13))]
	public UniTask PlayAsync(IReadOnlyList<IGameplayObject> objects)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayAsync_003Ed__13 _003CPlayAsync_003Ed__ = default(_003CPlayAsync_003Ed__13);
		_003CPlayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayAsync_003Ed__._003C_003E4__this = this;
		_003CPlayAsync_003Ed__.objects = objects;
		_003CPlayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayAsync_003Ed__13>(ref _003CPlayAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void PlayRoundStartSound()
	{
		if (LevelEntryBannerConfig.ShouldShowShapeBanner(base.Host.LevelConfig?.LevelNumber ?? 0))
		{
			AudioService.ActivateExperiment("variant_f0b830e4");
			AudioService.PlayUiSound(AudioKeys.RoundStartStyling.Value);
		}
		else
		{
			AudioService.PlayUiSound(AudioKeys.RoundStart.Value);
		}
	}

	public CancellationToken BeginSession()
	{
		EndSession();
		sessionCts = new CancellationTokenSource();
		return sessionCts.Token;
	}

	public void Cancel()
	{
		CancelAnimation();
		sessionCts?.Cancel();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			EndSession();
		}
	}

	private void EndSession()
	{
		CancelAnimation();
		sessionCts?.Cancel();
		sessionCts?.Dispose();
		sessionCts = null;
	}

	private void CancelAnimation()
	{
		Sequence obj = activeSequence;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		activeSequence = null;
		RestorePoses();
		EnablePhysicColliders();
		animatedRings.Clear();
	}

	private void DisablePhysicColliders(IReadOnlyList<IGameplayObject> objects, int playId)
	{
		enterPhysicColliderDisabledObjects.Clear();
		enterPhysicColliderDisabledPlayId = playId;
		if (objects == null)
		{
			return;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			IGameplayObject gameplayObject = objects[i];
			if (gameplayObject != null && !gameplayObject.IsSolved && PhysicColliderService.TryDisableAllPhysicColliders(gameplayObject))
			{
				enterPhysicColliderDisabledObjects.Add(gameplayObject);
			}
		}
	}

	private void EnablePhysicColliders(int ownerPlayId = -1)
	{
		if (ownerPlayId >= 0 && enterPhysicColliderDisabledPlayId != ownerPlayId)
		{
			return;
		}
		for (int i = 0; i < enterPhysicColliderDisabledObjects.Count; i++)
		{
			IGameplayObject gameplayObject = enterPhysicColliderDisabledObjects[i];
			if (gameplayObject != null && !gameplayObject.IsSolved)
			{
				PhysicColliderService.TryEnableAllPhysicColliders(gameplayObject);
			}
		}
		enterPhysicColliderDisabledObjects.Clear();
		enterPhysicColliderDisabledPlayId = 0;
	}

	private static float GetDynamicWaveInterval(RingLevelConfig config, int elementCount)
	{
		if (elementCount <= 0)
		{
			return config.waveInterval;
		}
		return Mathf.Max(Mathf.Min(config.waveInterval, config.waveMaxEnterDuration / (float)elementCount), config.waveMinInterval);
	}

	private static float GetDynamicTotalDuration(RingLevelConfig config, int ringCount)
	{
		if (ringCount <= 0)
		{
			return 0f;
		}
		return (float)(ringCount - 1) * GetDynamicWaveInterval(config, ringCount) + config.waveDuration;
	}

	private void RestorePoses()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < animatedRings.Count; i++)
		{
			RingPose ringPose = animatedRings[i];
			if (!((Object)(object)ringPose.Transform == (Object)null))
			{
				DOTween.Kill((object)((Component)ringPose.Transform).gameObject, false);
				ringPose.Transform.localScale = ringPose.OriginalScale;
			}
		}
	}
}
