using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.WeakGuide;
using RotateRings.WeakGuide.StepHandles;
using UnityEngine;

[Serializable]
public class GuideStep
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass58_0
	{
		public GuidMotion motion;

		public Action completion;

		public UniTaskCompletionSource completionSource;

		public CancellationToken cancellationToken;

		internal void _003CRunMotionAsync_003Eb__0()
		{
			motion.OnMotionCompleted -= completion;
			completionSource.TrySetResult();
		}

		internal void _003CRunMotionAsync_003Eb__1()
		{
			motion.OnMotionCompleted -= completion;
			completionSource.TrySetCanceled(cancellationToken);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CAcquireSpinePlayerAsync_003Ed__43 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<SpinePlayer> _003C_003Et__builder;

		public GuideStep _003C_003E4__this;

		public CancellationToken cancellationToken;

		private Awaiter<SpinePlayer> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GuideStep guideStep = _003C_003E4__this;
			SpinePlayer result;
			try
			{
				try
				{
					Awaiter<SpinePlayer> val;
					if (num != 0)
					{
						val = guideStep.spinePlayerProvider(cancellationToken).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SpinePlayer>, _003CAcquireSpinePlayerAsync_003Ed__43>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<SpinePlayer>);
						num = (_003C_003E1__state = -1);
					}
					result = val.GetResult();
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					throw;
				}
				catch (Exception)
				{
					result = null;
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDelayAsync_003Ed__57 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public float seconds;

		public CancellationToken cancellationToken;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_008e;
				}
				if (!(seconds <= 0f))
				{
					UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(seconds), true, (PlayerLoopTiming)8, cancellationToken);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CDelayAsync_003Ed__57>(ref val, ref this);
						return;
					}
					goto IL_008e;
				}
				cancellationToken.ThrowIfCancellationRequested();
				goto end_IL_0007;
				IL_008e:
				((Awaiter)(ref val)).GetResult();
				end_IL_0007:;
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunAsync_003Ed__42 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public GuideStep _003C_003E4__this;

		public CancellationToken cancellationToken;

		private bool _003CisFirstCycle_003E5__2;

		private float _003CfirstCycleDelay_003E5__3;

		private SpinePlayer _003CspinePlayer_003E5__4;

		private Awaiter _003C_003Eu__1;

		private Awaiter<SpinePlayer> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_0328: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_0303: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GuideStep guideStep = _003C_003E4__this;
			try
			{
				if ((uint)num <= 5u)
				{
					goto IL_0043;
				}
				if (guideStep.spinePlayerProvider != null && !((Object)(object)guideStep.handMotion == (Object)null) && !((Object)(object)guideStep.highlightRingMotion == (Object)null))
				{
					guideStep.IsRunning = true;
					goto IL_0043;
				}
				goto end_IL_000e;
				IL_0043:
				try
				{
					Awaiter val;
					Awaiter<SpinePlayer> val2;
					float seconds;
					UniTask val3;
					SpinePlayer result;
					switch (num)
					{
					default:
						_003CisFirstCycle_003E5__2 = true;
						_003CfirstCycleDelay_003E5__3 = 0f;
						goto IL_0345;
					case 0:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_00f1;
					case 1:
						val2 = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<SpinePlayer>);
						num = (_003C_003E1__state = -1);
						goto IL_0159;
					case 2:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_0200;
					case 3:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_0269;
					case 4:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_02cd;
					case 5:
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter);
							num = (_003C_003E1__state = -1);
							goto IL_0337;
						}
						IL_0345:
						if (cancellationToken.IsCancellationRequested)
						{
							break;
						}
						seconds = (_003CisFirstCycle_003E5__2 ? _003CfirstCycleDelay_003E5__3 : guideStep.loopDelay);
						_003CisFirstCycle_003E5__2 = false;
						val3 = DelayAsync(seconds, cancellationToken);
						val = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__42>(ref val, ref this);
							return;
						}
						goto IL_00f1;
						IL_0337:
						((Awaiter)(ref val)).GetResult();
						_003CspinePlayer_003E5__4 = null;
						goto IL_0345;
						IL_0200:
						((Awaiter)(ref val)).GetResult();
						val3 = DelayAsync(guideStep.afterOpenDelay, cancellationToken);
						val = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 3);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__42>(ref val, ref this);
							return;
						}
						goto IL_0269;
						IL_0269:
						((Awaiter)(ref val)).GetResult();
						val3 = guideStep.RunParallelMotionsAsync(cancellationToken);
						val = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 4);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__42>(ref val, ref this);
							return;
						}
						goto IL_02cd;
						IL_0159:
						result = val2.GetResult();
						_003CspinePlayer_003E5__4 = result;
						if (!((Object)(object)_003CspinePlayer_003E5__4 == (Object)null))
						{
							guideStep.cycleRefresh?.Invoke();
							guideStep.PrepareVisibleState();
							guideStep.ResetHandMotionToInitial();
							val3 = _003CspinePlayer_003E5__4.PlayOnceAsync(guideStep.openAnimation, cancellationToken);
							val = ((UniTask)(ref val3)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 2);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__42>(ref val, ref this);
								return;
							}
							goto IL_0200;
						}
						goto IL_0345;
						IL_02cd:
						((Awaiter)(ref val)).GetResult();
						val3 = guideStep.RunCloseAndFadeAsync(_003CspinePlayer_003E5__4, cancellationToken);
						val = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 5);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunAsync_003Ed__42>(ref val, ref this);
							return;
						}
						goto IL_0337;
						IL_00f1:
						((Awaiter)(ref val)).GetResult();
						val2 = guideStep.AcquireSpinePlayerAsync(cancellationToken).GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<SpinePlayer>, _003CRunAsync_003Ed__42>(ref val2, ref this);
							return;
						}
						goto IL_0159;
					}
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
				}
				finally
				{
					if (num < 0)
					{
						guideStep.IsRunning = false;
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunCloseAndFadeAsync_003Ed__56 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public GuideStep _003C_003E4__this;

		public CancellationToken cancellationToken;

		public SpinePlayer spinePlayer;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GuideStep guideStep = _003C_003E4__this;
			try
			{
				Awaiter val;
				UniTask val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_0131;
					}
					val2 = DelayAsync(guideStep.beforeCloseDelay, cancellationToken);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunCloseAndFadeAsync_003Ed__56>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val)).GetResult();
				UniTask val3 = spinePlayer.PlayOnceAsync(guideStep.closeAnimation, cancellationToken);
				UniTask val4 = (((Object)(object)guideStep.fadeMotion == (Object)null) ? UniTask.CompletedTask : RunMotionAsync(guideStep.fadeMotion, cancellationToken));
				val2 = UniTask.WhenAll((UniTask[])(object)new UniTask[2] { val3, val4 });
				val = ((UniTask)(ref val2)).GetAwaiter();
				if (!((Awaiter)(ref val)).IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__1 = val;
					((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunCloseAndFadeAsync_003Ed__56>(ref val, ref this);
					return;
				}
				goto IL_0131;
				IL_0131:
				((Awaiter)(ref val)).GetResult();
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunMotionAsync_003Ed__58 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public GuidMotion motion;

		public CancellationToken cancellationToken;

		private CancellationTokenRegistration _003C_003E7__wrap1;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num == 0)
				{
					goto IL_00ad;
				}
				_003C_003Ec__DisplayClass58_0 _003C_003Ec__DisplayClass58_ = new _003C_003Ec__DisplayClass58_0
				{
					motion = motion,
					cancellationToken = cancellationToken
				};
				if (!((Object)(object)_003C_003Ec__DisplayClass58_.motion == (Object)null))
				{
					_003C_003Ec__DisplayClass58_.completionSource = new UniTaskCompletionSource();
					_003C_003Ec__DisplayClass58_.completion = null;
					_003C_003Ec__DisplayClass58_.completion = _003C_003Ec__DisplayClass58_._003CRunMotionAsync_003Eb__0;
					_003C_003Ec__DisplayClass58_.motion.OnMotionCompleted += _003C_003Ec__DisplayClass58_.completion;
					((Component)_003C_003Ec__DisplayClass58_.motion).gameObject.SetActive(true);
					_003C_003Ec__DisplayClass58_.motion.StartMove();
					_003C_003E7__wrap1 = _003C_003Ec__DisplayClass58_.cancellationToken.Register(_003C_003Ec__DisplayClass58_._003CRunMotionAsync_003Eb__1);
					goto IL_00ad;
				}
				goto end_IL_0007;
				IL_00ad:
				try
				{
					Awaiter val;
					if (num != 0)
					{
						UniTask task = _003C_003Ec__DisplayClass58_.completionSource.Task;
						val = ((UniTask)(ref task)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunMotionAsync_003Ed__58>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
					}
					((Awaiter)(ref val)).GetResult();
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)_003C_003E7__wrap1/*cast due to .constrained prefix*/).Dispose();
					}
				}
				_003C_003E7__wrap1 = default(CancellationTokenRegistration);
				end_IL_0007:;
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunParallelMotionsAsync_003Ed__55 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public GuideStep _003C_003E4__this;

		public CancellationToken cancellationToken;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GuideStep guideStep = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = UniTask.WhenAll((UniTask[])(object)new UniTask[2]
					{
						RunMotionAsync(guideStep.handMotion, cancellationToken),
						RunMotionAsync(guideStep.highlightRingMotion, cancellationToken)
					});
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunParallelMotionsAsync_003Ed__55>(ref val2, ref this);
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

	[SerializeField]
	private string guideTextKey = string.Empty;

	[SerializeField]
	private string stepName = string.Empty;

	[SerializeField]
	private List<GuidMotion> motions = new List<GuidMotion>();

	private Func<CancellationToken, UniTask<SpinePlayer>> spinePlayerProvider;

	private Action cycleRefresh;

	private float initialDelay;

	private float loopDelay;

	private float afterOpenDelay;

	private float beforeCloseDelay;

	private string openAnimation;

	private string closeAnimation;

	private GuidMotion handMotion;

	private GuidMotion highlightRingMotion;

	private FadeMotion fadeMotion;

	private GameObject handRoot;

	private GameObject highlightRoot;

	private Vector3 handInitialPosition;

	private Quaternion initialCenterRotation;

	private bool handInitialPositionCaptured;

	private CancellationTokenSource internalCancellationTokenSource;

	private const string TAG = "[WeakGuide][GuideStep] ";

	public IWeakGuideStepHandle Handle { get; internal set; }

	public string GuideTextKey
	{
		get
		{
			return guideTextKey;
		}
		set
		{
			guideTextKey = value;
		}
	}

	public string StepName
	{
		get
		{
			return stepName;
		}
		set
		{
			stepName = value;
		}
	}

	public IReadOnlyList<GuidMotion> Motions => motions;

	public bool HasAnyMotion
	{
		get
		{
			if (motions == null)
			{
				return false;
			}
			for (int i = 0; i < motions.Count; i++)
			{
				if ((Object)(object)motions[i] != (Object)null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsRunning { get; private set; }

	public void Append(GuidMotion motion)
	{
		if (!((Object)(object)motion == (Object)null))
		{
			if (motions == null)
			{
				motions = new List<GuidMotion>();
			}
			if (!motions.Contains(motion))
			{
				motions.Add(motion);
				AssignMotion(motion);
				UpdateVisualRoots();
			}
		}
	}

	public void ConfigureRuntime(IWeakGuideStepHandle handle, Func<CancellationToken, UniTask<SpinePlayer>> spinePlayerProvider, float sequenceInitialDelay, float sequenceLoopDelay, float sequenceAfterOpenDelay, float sequenceBeforeCloseDelay, string sequenceOpenAnimation, string sequenceCloseAnimation, Action cycleRefreshCallback = null)
	{
		Handle = handle;
		this.spinePlayerProvider = spinePlayerProvider;
		initialDelay = Mathf.Max(0f, sequenceInitialDelay);
		loopDelay = Mathf.Max(0f, sequenceLoopDelay);
		afterOpenDelay = Mathf.Max(0f, sequenceAfterOpenDelay);
		beforeCloseDelay = Mathf.Max(0f, sequenceBeforeCloseDelay);
		openAnimation = (string.IsNullOrEmpty(sequenceOpenAnimation) ? "guide_drag_open" : sequenceOpenAnimation);
		closeAnimation = (string.IsNullOrEmpty(sequenceCloseAnimation) ? "guide_drag_close" : sequenceCloseAnimation);
		cycleRefresh = cycleRefreshCallback;
		RefreshMotionBindings();
	}

	public void StartSequence(bool skipInitialDelay = false)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		StopSequence();
		internalCancellationTokenSource = new CancellationTokenSource();
		UniTaskExtensions.Forget(RunAsync(internalCancellationTokenSource.Token, skipInitialDelay));
	}

	[AsyncStateMachine(typeof(_003CRunAsync_003Ed__42))]
	public UniTask RunAsync(CancellationToken cancellationToken, bool skipInitialDelay = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRunAsync_003Ed__42 _003CRunAsync_003Ed__ = default(_003CRunAsync_003Ed__42);
		_003CRunAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunAsync_003Ed__._003C_003E4__this = this;
		_003CRunAsync_003Ed__.cancellationToken = cancellationToken;
		_003CRunAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Start<_003CRunAsync_003Ed__42>(ref _003CRunAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CAcquireSpinePlayerAsync_003Ed__43))]
	private UniTask<SpinePlayer> AcquireSpinePlayerAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CAcquireSpinePlayerAsync_003Ed__43 _003CAcquireSpinePlayerAsync_003Ed__ = default(_003CAcquireSpinePlayerAsync_003Ed__43);
		_003CAcquireSpinePlayerAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<SpinePlayer>.Create();
		_003CAcquireSpinePlayerAsync_003Ed__._003C_003E4__this = this;
		_003CAcquireSpinePlayerAsync_003Ed__.cancellationToken = cancellationToken;
		_003CAcquireSpinePlayerAsync_003Ed__._003C_003E1__state = -1;
		_003CAcquireSpinePlayerAsync_003Ed__._003C_003Et__builder.Start<_003CAcquireSpinePlayerAsync_003Ed__43>(ref _003CAcquireSpinePlayerAsync_003Ed__);
		return _003CAcquireSpinePlayerAsync_003Ed__._003C_003Et__builder.Task;
	}

	public void StopSequence()
	{
		if (internalCancellationTokenSource != null)
		{
			internalCancellationTokenSource.Cancel();
			internalCancellationTokenSource.Dispose();
			internalCancellationTokenSource = null;
		}
		if (motions != null)
		{
			for (int i = 0; i < motions.Count; i++)
			{
				GuidMotion guidMotion = motions[i];
				if (!((Object)(object)guidMotion == (Object)null))
				{
					guidMotion.StopMove();
					((Component)guidMotion).gameObject.SetActive(false);
				}
			}
		}
		IsRunning = false;
	}

	public void PauseAll()
	{
		if (motions != null)
		{
			for (int i = 0; i < motions.Count; i++)
			{
				motions[i]?.PauseMove();
			}
		}
	}

	public void ResumeAll()
	{
		if (motions != null)
		{
			for (int i = 0; i < motions.Count; i++)
			{
				motions[i]?.ResumeMove();
			}
		}
	}

	public void RestartSequence()
	{
		StartSequence(skipInitialDelay: true);
	}

	public void FinishAll()
	{
		StopSequence();
	}

	public void ReactivateAll()
	{
		StartSequence();
	}

	private void RefreshMotionBindings()
	{
		handMotion = null;
		highlightRingMotion = null;
		fadeMotion = null;
		handInitialPositionCaptured = false;
		if (motions != null)
		{
			for (int i = 0; i < motions.Count; i++)
			{
				AssignMotion(motions[i]);
			}
		}
		UpdateVisualRoots();
	}

	private void AssignMotion(GuidMotion motion)
	{
		if (motion is FadeMotion fadeMotion)
		{
			this.fadeMotion = fadeMotion;
		}
		else if (motion is CircularMotion circularMotion)
		{
			if (circularMotion.MotionMode == CircularMotionMode.Orbit)
			{
				handMotion = circularMotion;
			}
			else if (circularMotion.MotionMode == CircularMotionMode.SelfRotate)
			{
				highlightRingMotion = circularMotion;
			}
		}
	}

	private void UpdateVisualRoots()
	{
		handRoot = (((Object)(object)handMotion != (Object)null) ? ((Component)handMotion).gameObject : null);
		highlightRoot = (((Object)(object)highlightRingMotion != (Object)null) ? ((Component)highlightRingMotion).gameObject : null);
	}

	private void PrepareVisibleState()
	{
		GameObject obj = handRoot;
		if (obj != null)
		{
			obj.SetActive(true);
		}
		GameObject obj2 = highlightRoot;
		if (obj2 != null)
		{
			obj2.SetActive(true);
		}
		fadeMotion?.ResetVisualState();
	}

	private void ResetHandMotionToInitial()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)handMotion == (Object)null)
		{
			return;
		}
		Transform transform = ((Component)handMotion).transform;
		if ((Object)(object)transform == (Object)null)
		{
			return;
		}
		if (!handInitialPositionCaptured)
		{
			handInitialPosition = transform.position;
			handInitialPositionCaptured = true;
			if (handMotion is CircularMotion circularMotion && (Object)(object)circularMotion.Center != (Object)null)
			{
				initialCenterRotation = circularMotion.Center.rotation;
			}
			return;
		}
		_ = transform.position;
		Vector3 position = handInitialPosition;
		if (handMotion is CircularMotion circularMotion2 && (Object)(object)circularMotion2.Center != (Object)null)
		{
			Transform center = circularMotion2.Center;
			Vector3 position2 = center.position;
			Vector3 val = handInitialPosition - position2;
			Quaternion val2 = center.rotation * Quaternion.Inverse(initialCenterRotation);
			position = position2 + val2 * val;
		}
		transform.position = position;
		if (handMotion is CircularMotion { MotionMode: CircularMotionMode.Orbit } circularMotion3 && (Object)(object)circularMotion3.Center != (Object)null)
		{
			circularMotion3.InitFromPosition(position);
		}
	}

	[AsyncStateMachine(typeof(_003CRunParallelMotionsAsync_003Ed__55))]
	private UniTask RunParallelMotionsAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRunParallelMotionsAsync_003Ed__55 _003CRunParallelMotionsAsync_003Ed__ = default(_003CRunParallelMotionsAsync_003Ed__55);
		_003CRunParallelMotionsAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunParallelMotionsAsync_003Ed__._003C_003E4__this = this;
		_003CRunParallelMotionsAsync_003Ed__.cancellationToken = cancellationToken;
		_003CRunParallelMotionsAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunParallelMotionsAsync_003Ed__._003C_003Et__builder)).Start<_003CRunParallelMotionsAsync_003Ed__55>(ref _003CRunParallelMotionsAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunParallelMotionsAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRunCloseAndFadeAsync_003Ed__56))]
	private UniTask RunCloseAndFadeAsync(SpinePlayer spinePlayer, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRunCloseAndFadeAsync_003Ed__56 _003CRunCloseAndFadeAsync_003Ed__ = default(_003CRunCloseAndFadeAsync_003Ed__56);
		_003CRunCloseAndFadeAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunCloseAndFadeAsync_003Ed__._003C_003E4__this = this;
		_003CRunCloseAndFadeAsync_003Ed__.spinePlayer = spinePlayer;
		_003CRunCloseAndFadeAsync_003Ed__.cancellationToken = cancellationToken;
		_003CRunCloseAndFadeAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunCloseAndFadeAsync_003Ed__._003C_003Et__builder)).Start<_003CRunCloseAndFadeAsync_003Ed__56>(ref _003CRunCloseAndFadeAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunCloseAndFadeAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CDelayAsync_003Ed__57))]
	private static UniTask DelayAsync(float seconds, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CDelayAsync_003Ed__57 _003CDelayAsync_003Ed__ = default(_003CDelayAsync_003Ed__57);
		_003CDelayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CDelayAsync_003Ed__.seconds = seconds;
		_003CDelayAsync_003Ed__.cancellationToken = cancellationToken;
		_003CDelayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CDelayAsync_003Ed__._003C_003Et__builder)).Start<_003CDelayAsync_003Ed__57>(ref _003CDelayAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CDelayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CRunMotionAsync_003Ed__58))]
	private static UniTask RunMotionAsync(GuidMotion motion, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRunMotionAsync_003Ed__58 _003CRunMotionAsync_003Ed__ = default(_003CRunMotionAsync_003Ed__58);
		_003CRunMotionAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CRunMotionAsync_003Ed__.motion = motion;
		_003CRunMotionAsync_003Ed__.cancellationToken = cancellationToken;
		_003CRunMotionAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CRunMotionAsync_003Ed__._003C_003Et__builder)).Start<_003CRunMotionAsync_003Ed__58>(ref _003CRunMotionAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CRunMotionAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
