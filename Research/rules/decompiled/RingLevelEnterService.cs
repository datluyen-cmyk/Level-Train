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
using UniLog;
using UnityEngine;

public sealed class RingLevelEnterService : IDisposable
{
	private struct RingPose
	{
		public Transform Transform;

		public Vector3 OriginalScale;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass9_0
	{
		public RingLevelConfig animConfig;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass9_1
	{
		public float t;

		public Transform ring;

		public Vector3 originalScale;

		public _003C_003Ec__DisplayClass9_0 CS_0024_003C_003E8__locals1;

		internal float _003CPlayAsync_003Eb__1()
		{
			return t;
		}

		internal void _003CPlayAsync_003Eb__2(float value)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			t = value;
			CS_0024_003C_003E8__locals1.animConfig.ApplyRingWaveScale(ring, originalScale, t);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayAsync_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingLevelEnterService _003C_003E4__this;

		public IReadOnlyList<Transform> rings;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_0318: Unknown result type (might be due to invalid IL or missing references)
			//IL_0320: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingLevelEnterService ringLevelEnterService = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_00ea;
				}
				CancellationToken token = default(CancellationToken);
				List<Transform> list = default(List<Transform>);
				if (ringLevelEnterService.sessionCts != null && !ringLevelEnterService.disposed)
				{
					token = ringLevelEnterService.sessionCts.Token;
					ringLevelEnterService.CancelAnimation();
					if (rings != null && rings.Count != 0)
					{
						list = new List<Transform>(rings.Count);
						for (int i = 0; i < rings.Count; i++)
						{
							Transform val = rings[i];
							if ((Object)(object)val != (Object)null)
							{
								list.Add(val);
							}
						}
						if (list.Count != 0)
						{
							list.Sort((Transform a, Transform b) => b.position.y.CompareTo(a.position.y));
							ringLevelEnterService.animatedRings.Clear();
							ringLevelEnterService.activeSequence = DOTween.Sequence();
							goto IL_00ea;
						}
					}
				}
				goto end_IL_000e;
				IL_00ea:
				try
				{
					Awaiter<bool> val3;
					if (num != 0)
					{
						_003C_003Ec__DisplayClass9_0 _003C_003Ec__DisplayClass9_ = new _003C_003Ec__DisplayClass9_0
						{
							animConfig = ringLevelEnterService.Config
						};
						int num2 = 0;
						while (true)
						{
							if (num2 >= list.Count)
							{
								int num3 = 0;
								while (true)
								{
									if (num3 >= list.Count)
									{
										GameObject gameObject = ((Component)list[0]).gameObject;
										TweenSettingsExtensions.SetLink<Sequence>(TweenSettingsExtensions.SetTarget<Sequence>(ringLevelEnterService.activeSequence, (object)gameObject), gameObject, (LinkBehaviour)5);
										TweenExtensions.Play<Sequence>(ringLevelEnterService.activeSequence);
										BgmController.RestartLevel();
										AudioService.PlayUiSound(AudioKeys.RoundStart.Value);
										UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(ringLevelEnterService.Config.GetTotalDuration(list.Count)), false, (PlayerLoopTiming)8, token);
										val3 = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
										if (val3.IsCompleted)
										{
											break;
										}
										num = (_003C_003E1__state = 0);
										_003C_003Eu__1 = val3;
										((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayAsync_003Ed__9>(ref val3, ref this);
										return;
									}
									_003C_003Ec__DisplayClass9_1 CS_0024_003C_003E8__locals13 = new _003C_003Ec__DisplayClass9_1
									{
										CS_0024_003C_003E8__locals1 = _003C_003Ec__DisplayClass9_
									};
									if (!token.IsCancellationRequested)
									{
										CS_0024_003C_003E8__locals13.ring = list[num3];
										CS_0024_003C_003E8__locals13.originalScale = ringLevelEnterService.animatedRings[num3].OriginalScale;
										CS_0024_003C_003E8__locals13.t = 0f;
										TweenerCore<float, float, FloatOptions> val4 = TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => CS_0024_003C_003E8__locals13.t), (DOSetter<float>)delegate(float value)
										{
											//IL_0019: Unknown result type (might be due to invalid IL or missing references)
											CS_0024_003C_003E8__locals13.t = value;
											CS_0024_003C_003E8__locals13.CS_0024_003C_003E8__locals1.animConfig.ApplyRingWaveScale(CS_0024_003C_003E8__locals13.ring, CS_0024_003C_003E8__locals13.originalScale, CS_0024_003C_003E8__locals13.t);
										}, 1f, CS_0024_003C_003E8__locals13.CS_0024_003C_003E8__locals1.animConfig.waveDuration), (Ease)1), (object)((Component)CS_0024_003C_003E8__locals13.ring).gameObject), ((Component)CS_0024_003C_003E8__locals13.ring).gameObject, (LinkBehaviour)5);
										TweenSettingsExtensions.Insert(ringLevelEnterService.activeSequence, (float)num3 * CS_0024_003C_003E8__locals13.CS_0024_003C_003E8__locals1.animConfig.waveInterval, (Tween)(object)val4);
										num3++;
										continue;
									}
									ringLevelEnterService.RestorePoses();
									goto end_IL_00ea;
								}
								break;
							}
							if (!token.IsCancellationRequested)
							{
								Transform val5 = list[num2];
								Vector3 localScale = val5.localScale;
								ringLevelEnterService.animatedRings.Add(new RingPose
								{
									Transform = val5,
									OriginalScale = localScale
								});
								_003C_003Ec__DisplayClass9_.animConfig.ApplyInitialAllRingsScale(val5, localScale);
								num2++;
								continue;
							}
							ringLevelEnterService.RestorePoses();
							goto end_IL_00ea;
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
						ringLevelEnterService.RestorePoses();
					}
					end_IL_00ea:;
				}
				catch (Exception ex)
				{
					Log.LogError("[RingLevelEnterService] ", "入场动画异常: {0}", new object[1] { ex });
					ringLevelEnterService.RestorePoses();
				}
				finally
				{
					if (num < 0)
					{
						ringLevelEnterService.activeSequence = null;
						ringLevelEnterService.animatedRings.Clear();
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

	private readonly List<RingPose> animatedRings = new List<RingPose>();

	private CancellationTokenSource sessionCts;

	private Sequence activeSequence;

	private bool disposed;

	private RingLevelConfig Config => RingLevelConfig.Instance;

	public CancellationToken BeginSession()
	{
		EndSession();
		sessionCts = new CancellationTokenSource();
		return sessionCts.Token;
	}

	[AsyncStateMachine(typeof(_003CPlayAsync_003Ed__9))]
	public UniTask PlayAsync(IReadOnlyList<Transform> rings)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayAsync_003Ed__9 _003CPlayAsync_003Ed__ = default(_003CPlayAsync_003Ed__9);
		_003CPlayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayAsync_003Ed__._003C_003E4__this = this;
		_003CPlayAsync_003Ed__.rings = rings;
		_003CPlayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayAsync_003Ed__9>(ref _003CPlayAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public void Cancel()
	{
		CancelAnimation();
		sessionCts?.Cancel();
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
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
		animatedRings.Clear();
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
