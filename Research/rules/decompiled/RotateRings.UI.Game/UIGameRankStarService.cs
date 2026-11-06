using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.Services;
using RotateRings.UI.SubView;
using Rotaterings.Rank;
using Rotaterings.UI.RankList;
using UniLog;
using UnityEngine;
using UnityEngine.Pool;
using XY.Common.ParticleImage;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public sealed class UIGameRankStarService : UIGameBaseService
{
	private sealed class FlyEffectContext
	{
		private readonly UIGameRankStarService owner;

		public AutoResetUniTaskCompletionSource FirstArrived;

		public AutoResetUniTaskCompletionSource AllFinished;

		public UIRankStarCollectionView CollectionView;

		public int CollectCount;

		public readonly Action OnFirstParticleFinished;

		public readonly Action OnLastParticleFinished;

		public FlyEffectContext(UIGameRankStarService owner)
		{
			this.owner = owner;
			OnFirstParticleFinished = HandleFirstParticleFinished;
			OnLastParticleFinished = HandleLastParticleFinished;
		}

		public void Prepare(UIRankStarCollectionView collectionView, int collectCount)
		{
			FirstArrived = AutoResetUniTaskCompletionSource.Create();
			AllFinished = AutoResetUniTaskCompletionSource.Create();
			CollectionView = collectionView;
			CollectCount = collectCount;
		}

		private void HandleFirstParticleFinished()
		{
			CollectionView.SetNum(CollectCount);
			owner.PlayCollectFeedback(CollectionView);
			AudioService.PlayUiSound(AudioKeys.AcornCountUp.Value);
			FirstArrived.TrySetResult();
		}

		private void HandleLastParticleFinished()
		{
			AllFinished.TrySetResult();
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteFlyQueue_003Ed__34 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIGameRankStarService _003C_003E4__this;

		private void MoveNext()
		{
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameRankStarService uIGameRankStarService = _003C_003E4__this;
			try
			{
				if (!uIGameRankStarService.isRunningFlyQueue)
				{
					uIGameRankStarService.isRunningFlyQueue = true;
					UniTaskVoid val;
					try
					{
						while (uIGameRankStarService.runningFlyEvents.Count > 0)
						{
							if (!uIGameRankStarService.TryGetRankStarContext(out var hostView, out var collectionView))
							{
								uIGameRankStarService.runningFlyEvents.Clear();
								break;
							}
							uIGameRankStarService.CancelHideExecute();
							uIGameRankStarService.TriggerPanelShown(collectionView);
							int count = uIGameRankStarService.runningFlyEvents.Count;
							for (int i = 0; i < count; i++)
							{
								RingSolveEvent eventData = uIGameRankStarService.runningFlyEvents.Dequeue();
								val = uIGameRankStarService.StartCollectFlyEffect(hostView, collectionView, eventData);
								((UniTaskVoid)(ref val)).Forget();
							}
						}
					}
					catch (Exception ex)
					{
						Log.LogError("[UIGame][RankStarService] ", "ExecuteFlyQueue 异常，清空事件队列: {0}", new object[1] { ex.Message });
						uIGameRankStarService.runningFlyEvents.Clear();
					}
					finally
					{
						if (num < 0)
						{
							uIGameRankStarService.isRunningFlyQueue = false;
							if (uIGameRankStarService.runningFlyEvents.Count > 0)
							{
								val = uIGameRankStarService.ExecuteFlyQueue();
								((UniTaskVoid)(ref val)).Forget();
							}
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CExecuteHideAsync_003Ed__42 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIGameRankStarService _003C_003E4__this;

		public UIRankStarCollectionView collectionView;

		public CancellationToken cancellationToken;

		public int generation;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameRankStarService uIGameRankStarService = _003C_003E4__this;
			try
			{
				if ((uint)num > 1u)
				{
					uIGameRankStarService.hidingCollectionView = collectionView;
				}
				try
				{
					UniTask val2;
					Awaiter<bool> val;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							goto IL_0143;
						}
						val2 = UniTask.Delay(TimeSpan.FromSeconds(0.800000011920929), false, (PlayerLoopTiming)8, cancellationToken);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CExecuteHideAsync_003Ed__42>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (!val.GetResult())
					{
						if (IsCollectionViewActive(collectionView))
						{
							uIGameRankStarService.isPanelHiding = true;
							val2 = collectionView.Hide(cancellationToken);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CExecuteHideAsync_003Ed__42>(ref val, ref this);
								return;
							}
							goto IL_0143;
						}
						if (!cancellationToken.IsCancellationRequested)
						{
							uIGameRankStarService.ResetPanelShownState();
						}
					}
					goto end_IL_001f;
					IL_0143:
					if (!val.GetResult() && !cancellationToken.IsCancellationRequested)
					{
						uIGameRankStarService.ResetPanelShownState();
					}
					end_IL_001f:;
				}
				finally
				{
					if (num < 0 && generation == uIGameRankStarService.hideGeneration)
					{
						uIGameRankStarService.isPanelHiding = false;
						uIGameRankStarService.hidingCollectionView = null;
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CHideCollectFeedbackAsync_003Ed__46 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken cancellationToken;

		public UIRankStarCollectionView collectionView;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(0.75), false, (PlayerLoopTiming)8, cancellationToken);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CHideCollectFeedbackAsync_003Ed__46>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult() && collectionView != null)
				{
					collectionView.HideCollectFeedback();
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
	private struct _003CPlayFlyEffectAsync_003Ed__56 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public UIGameRankStarService _003C_003E4__this;

		public UIRankStarCollectionView collectionView;

		public int collectCount;

		public XYParticleImage particle;

		public GameObject instance;

		private FlyEffectContext _003Ccontext_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameRankStarService uIGameRankStarService = _003C_003E4__this;
			bool result;
			try
			{
				UniTask task;
				Awaiter val;
				switch (num)
				{
				default:
					_003Ccontext_003E5__2 = uIGameRankStarService.flyContextPool.Get();
					_003Ccontext_003E5__2.Prepare(collectionView, collectCount);
					particle.Play(1, (Transform)(object)collectionView.star, _003Ccontext_003E5__2.OnFirstParticleFinished, null, _003Ccontext_003E5__2.OnLastParticleFinished);
					if (!particle.isPlaying && !particle.isEmitting)
					{
						_003Ccontext_003E5__2.FirstArrived.TrySetResult();
						_003Ccontext_003E5__2.AllFinished.TrySetResult();
						task = _003Ccontext_003E5__2.FirstArrived.Task;
						val = ((UniTask)(ref task)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CPlayFlyEffectAsync_003Ed__56>(ref val, ref this);
							return;
						}
						goto IL_011e;
					}
					task = _003Ccontext_003E5__2.FirstArrived.Task;
					val = ((UniTask)(ref task)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CPlayFlyEffectAsync_003Ed__56>(ref val, ref this);
						return;
					}
					goto IL_0214;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_011e;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0187;
				case 2:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0214;
				case 3:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						break;
					}
					IL_0214:
					((Awaiter)(ref val)).GetResult();
					task = _003Ccontext_003E5__2.AllFinished.Task;
					val = ((UniTask)(ref task)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 3);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CPlayFlyEffectAsync_003Ed__56>(ref val, ref this);
						return;
					}
					break;
					IL_0187:
					((Awaiter)(ref val)).GetResult();
					uIGameRankStarService.flyContextPool.Release(_003Ccontext_003E5__2);
					uIGameRankStarService.ReleaseFlyEffect(instance);
					result = false;
					goto end_IL_000e;
					IL_011e:
					((Awaiter)(ref val)).GetResult();
					task = _003Ccontext_003E5__2.AllFinished.Task;
					val = ((UniTask)(ref task)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter, _003CPlayFlyEffectAsync_003Ed__56>(ref val, ref this);
						return;
					}
					goto IL_0187;
				}
				((Awaiter)(ref val)).GetResult();
				uIGameRankStarService.flyContextPool.Release(_003Ccontext_003E5__2);
				result = true;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Ccontext_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Ccontext_003E5__2 = null;
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
	private struct _003CShowPanelInternal_003Ed__40 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UIGameRankStarService _003C_003E4__this;

		public UIRankStarCollectionView collectionView;

		private int _003Cgeneration_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameRankStarService uIGameRankStarService = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					uIGameRankStarService.showCts?.Cancel();
					uIGameRankStarService.showCts?.Dispose();
					uIGameRankStarService.showCts = new CancellationTokenSource();
					_003Cgeneration_003E5__2 = ++uIGameRankStarService.panelGeneration;
				}
				try
				{
					Awaiter<bool> val2;
					if (num != 0)
					{
						UniTask val = collectionView.Show(uIGameRankStarService.showCts.Token);
						val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CShowPanelInternal_003Ed__40>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (!val2.GetResult() && _003Cgeneration_003E5__2 == uIGameRankStarService.panelGeneration && IsCollectionViewActive(collectionView))
					{
						uIGameRankStarService.isPanelShown = true;
					}
				}
				catch (Exception ex)
				{
					Log.LogError("[UIGame][RankStarService] ", "收集条入场动画异常: {0}", new object[1] { ex.Message });
				}
				finally
				{
					if (num < 0 && _003Cgeneration_003E5__2 == uIGameRankStarService.panelGeneration)
					{
						uIGameRankStarService.isShowingPanel = false;
					}
				}
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
	private struct _003CStartCollectFlyEffect_003Ed__35 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIGameRankStarService _003C_003E4__this;

		public UIGameView hostView;

		public UIRankStarCollectionView collectionView;

		public RingSolveEvent eventData;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameRankStarService uIGameRankStarService = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					uIGameRankStarService.activeFlyCount++;
				}
				try
				{
					Awaiter<bool> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_0144;
					}
					Vector2 localPoint;
					if (!TryGetSpawnAnchor(hostView, out var spawnAnchor))
					{
						uIGameRankStarService.ApplyCollectCount(collectionView, eventData.CollectCount, "生成锚点缺失");
					}
					else if (eventData.HasCollectScreenPosition)
					{
						if (uIGameRankStarService.TryScreenToLocalPoint(eventData.CollectScreenPosition, spawnAnchor, out localPoint))
						{
							goto IL_00d0;
						}
						uIGameRankStarService.ApplyCollectCount(collectionView, eventData.CollectCount, "收集物屏幕坐标转UI坐标失败");
					}
					else
					{
						if (WorldToUIPostionUtil.WorldToLocalPoint(eventData.WorldPosition, spawnAnchor, out localPoint))
						{
							goto IL_00d0;
						}
						uIGameRankStarService.ApplyCollectCount(collectionView, eventData.CollectCount, "世界坐标转UI坐标失败");
					}
					goto end_IL_0020;
					IL_0144:
					if (!val.GetResult())
					{
						uIGameRankStarService.ApplyCollectCount(collectionView, eventData.CollectCount, "飞星特效播放失败");
					}
					goto end_IL_0020;
					IL_00d0:
					val = uIGameRankStarService.TryPlayCollectFlyEffect(hostView, spawnAnchor, collectionView, localPoint, eventData.CollectCount).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CStartCollectFlyEffect_003Ed__35>(ref val, ref this);
						return;
					}
					goto IL_0144;
					end_IL_0020:;
				}
				finally
				{
					if (num < 0)
					{
						uIGameRankStarService.ReleaseFlySlot(collectionView);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CTryPlayCollectFlyEffect_003Ed__50 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public UIGameRankStarService _003C_003E4__this;

		public UIGameView hostView;

		public RectTransform spawnAnchor;

		public Vector2 localPos;

		public UIRankStarCollectionView collectionView;

		public int collectCount;

		private GameObject _003Cinstance_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameRankStarService uIGameRankStarService = _003C_003E4__this;
			bool result;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_0141;
				}
				string flyEffectLocation = GetFlyEffectLocation(uIGameRankStarService.formalCollectLayout);
				if (uIGameRankStarService.flyEffectLocation != flyEffectLocation)
				{
					uIGameRankStarService.flyEffectPool.Clear();
					uIGameRankStarService.flyEffectPrefab = null;
					uIGameRankStarService.flyEffectLocation = flyEffectLocation;
				}
				if ((Object)(object)uIGameRankStarService.flyEffectPrefab == (Object)null)
				{
					uIGameRankStarService.flyEffectPrefab = GameApp.Res.LoadAsset<GameObject>(flyEffectLocation, hostView.LoadAssetGroup);
				}
				if ((Object)(object)uIGameRankStarService.flyEffectPrefab == (Object)null)
				{
					result = false;
				}
				else
				{
					_003Cinstance_003E5__2 = uIGameRankStarService.AcquireFlyEffect(spawnAnchor, localPos);
					XYParticleImage component = _003Cinstance_003E5__2.GetComponent<XYParticleImage>();
					if (!((Object)(object)component == (Object)null))
					{
						PrepareParticle(component, _003Cinstance_003E5__2);
						val = uIGameRankStarService.PlayFlyEffectAsync(component, _003Cinstance_003E5__2, collectionView, collectCount).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CTryPlayCollectFlyEffect_003Ed__50>(ref val, ref this);
							return;
						}
						goto IL_0141;
					}
					uIGameRankStarService.ReleaseFlyEffect(_003Cinstance_003E5__2);
					result = false;
				}
				goto end_IL_000e;
				IL_0141:
				if (!val.GetResult())
				{
					result = false;
				}
				else
				{
					uIGameRankStarService.ReleaseFlyEffect(_003Cinstance_003E5__2);
					result = true;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cinstance_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cinstance_003E5__2 = null;
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
	private struct _003CWaitUntilAllFlyFinished_003Ed__37 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<bool> _003C_003Et__builder;

		public CancellationToken ct;

		public UIGameRankStarService _003C_003E4__this;

		private float _003Cdeadline_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameRankStarService uIGameRankStarService = _003C_003E4__this;
			bool result;
			try
			{
				Awaiter<bool> val;
				if (num != 0)
				{
					if (num != 1)
					{
						_003Cdeadline_003E5__2 = Time.realtimeSinceStartup + 3f;
						goto IL_00d4;
					}
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_017d;
				}
				val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter<bool>);
				num = (_003C_003E1__state = -1);
				goto IL_00c4;
				IL_00c4:
				if (!val.GetResult())
				{
					goto IL_00d4;
				}
				result = true;
				goto end_IL_000e;
				IL_00d4:
				UniTask val2;
				if (uIGameRankStarService.HasPendingFly())
				{
					if (ct.IsCancellationRequested)
					{
						result = true;
					}
					else
					{
						if (!(Time.realtimeSinceStartup >= _003Cdeadline_003E5__2))
						{
							val2 = UniTask.Yield((PlayerLoopTiming)13, ct);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitUntilAllFlyFinished_003Ed__37>(ref val, ref this);
								return;
							}
							goto IL_00c4;
						}
						result = false;
					}
				}
				else
				{
					if (!(uIGameRankStarService.lastFlyFinishedTime <= 0f))
					{
						float num2 = 0.5f - (Time.realtimeSinceStartup - uIGameRankStarService.lastFlyFinishedTime);
						if (num2 > 0f)
						{
							val2 = UniTask.Delay(TimeSpan.FromSeconds(num2), false, (PlayerLoopTiming)8, ct);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWaitUntilAllFlyFinished_003Ed__37>(ref val, ref this);
								return;
							}
							goto IL_017d;
						}
						goto IL_018a;
					}
					result = false;
				}
				goto end_IL_000e;
				IL_018a:
				result = false;
				goto end_IL_000e;
				IL_017d:
				if (!val.GetResult())
				{
					goto IL_018a;
				}
				result = true;
				end_IL_000e:;
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

	private int formalCollectLayout;

	private const string Tag = "[UIGame][RankStarService] ";

	private const int FlyEmitCount = 1;

	private const float AutoHideDelaySeconds = 0.8f;

	private const float MaxFlyWaitSeconds = 3f;

	private const float FlyTailShowSeconds = 0.5f;

	private const float FeedbackShowSeconds = 0.75f;

	private readonly Queue<RingSolveEvent> runningFlyEvents = new Queue<RingSolveEvent>();

	private readonly List<GameObject> activeFlyEffects = new List<GameObject>();

	private bool isPanelShown;

	private bool isRunningFlyQueue;

	private int activeFlyCount;

	private float lastFlyFinishedTime;

	private bool isShowingPanel;

	private CancellationTokenSource hideCts;

	private bool isPanelHiding;

	private UIRankStarCollectionView hidingCollectionView;

	private CancellationTokenSource feedbackHideCts;

	private GameObject flyEffectPrefab;

	private string flyEffectLocation;

	private readonly ObjectPool<GameObject> flyEffectPool;

	private readonly ObjectPool<FlyEffectContext> flyContextPool;

	private Canvas cachedCanvas;

	private CancellationTokenSource showCts;

	private int panelGeneration;

	private int hideGeneration;

	private void InitializePositionExperiment()
	{
		formalCollectLayout = RankCollectPositionExperiment.ReadLayout();
		RestoreCollectPosition();
	}

	private void RestoreCollectPosition()
	{
		int layout = formalCollectLayout;
		RectTransformLayoutData pose = RankCollectPositionExperiment.ReadAnchor(layout);
		base.View?.SetCollectPosition(layout, pose);
	}

	private void ResetPositionExperiment()
	{
		formalCollectLayout = RankCollectPositionExperiment.ReadLayout();
		RestoreCollectPosition();
	}

	private void DisposePositionExperiment()
	{
	}

	public UIGameRankStarService(UIGameController controller)
		: base(controller)
	{
		InitializePositionExperiment();
		GameApp.Event.Listen<LevelChanged>((Action<LevelChanged>)OnLevelChanged);
		GameApp.Event.Listen<RingSolveEvent>((Action<RingSolveEvent>)OnRingSolveEvent);
		flyEffectPool = new ObjectPool<GameObject>((Func<GameObject>)CreateFlyEffectInstance, (Action<GameObject>)null, (Action<GameObject>)OnReleaseFlyEffect, (Action<GameObject>)OnDestroyFlyEffectInstance, false, 8, 16);
		flyContextPool = new ObjectPool<FlyEffectContext>((Func<FlyEffectContext>)(() => new FlyEffectContext(this)), (Action<FlyEffectContext>)null, (Action<FlyEffectContext>)null, (Action<FlyEffectContext>)null, false, 8, 16);
	}

	public override void Dispose()
	{
		DisposePositionExperiment();
		GameApp.Event.Unlisten<LevelChanged>((Action<LevelChanged>)OnLevelChanged);
		GameApp.Event.Unlisten<RingSolveEvent>((Action<RingSolveEvent>)OnRingSolveEvent);
		ResetFlyState(hidePanel: false);
		flyEffectPool.Clear();
		base.Dispose();
	}

	private void OnLevelChanged(LevelChanged eventData)
	{
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		bool isStartGame = eventData?.IsNewGame ?? true;
		int collectableCount = eventData?.CollectableCount ?? 0;
		rankModelProxy.InitRankStarOnLevelLoad(isStartGame, collectableCount);
		ResetPositionExperiment();
		ResetFlyState(hidePanel: true);
		base.View?.RankStarCollectionView?.SetNum(rankModelProxy.GetCurrentCollectCount());
	}

	private void OnRingSolveEvent(RingSolveEvent eventData)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (eventData != null)
		{
			runningFlyEvents.Enqueue(eventData);
			UniTaskVoid val = ExecuteFlyQueue();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CExecuteFlyQueue_003Ed__34))]
	private UniTaskVoid ExecuteFlyQueue()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteFlyQueue_003Ed__34 _003CExecuteFlyQueue_003Ed__ = default(_003CExecuteFlyQueue_003Ed__34);
		_003CExecuteFlyQueue_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CExecuteFlyQueue_003Ed__._003C_003E4__this = this;
		_003CExecuteFlyQueue_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CExecuteFlyQueue_003Ed__._003C_003Et__builder)).Start<_003CExecuteFlyQueue_003Ed__34>(ref _003CExecuteFlyQueue_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CExecuteFlyQueue_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CStartCollectFlyEffect_003Ed__35))]
	private UniTaskVoid StartCollectFlyEffect(UIGameView hostView, UIRankStarCollectionView collectionView, RingSolveEvent eventData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CStartCollectFlyEffect_003Ed__35 _003CStartCollectFlyEffect_003Ed__ = default(_003CStartCollectFlyEffect_003Ed__35);
		_003CStartCollectFlyEffect_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CStartCollectFlyEffect_003Ed__._003C_003E4__this = this;
		_003CStartCollectFlyEffect_003Ed__.hostView = hostView;
		_003CStartCollectFlyEffect_003Ed__.collectionView = collectionView;
		_003CStartCollectFlyEffect_003Ed__.eventData = eventData;
		_003CStartCollectFlyEffect_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CStartCollectFlyEffect_003Ed__._003C_003Et__builder)).Start<_003CStartCollectFlyEffect_003Ed__35>(ref _003CStartCollectFlyEffect_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CStartCollectFlyEffect_003Ed__._003C_003Et__builder)).Task;
	}

	private void ReleaseFlySlot(UIRankStarCollectionView collectionView)
	{
		activeFlyCount = Math.Max(0, activeFlyCount - 1);
		if (activeFlyCount == 0)
		{
			lastFlyFinishedTime = Time.realtimeSinceStartup;
			if (collectionView != null)
			{
				ExecuteHide(collectionView);
			}
		}
	}

	[AsyncStateMachine(typeof(_003CWaitUntilAllFlyFinished_003Ed__37))]
	public UniTask<bool> WaitUntilAllFlyFinished(CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitUntilAllFlyFinished_003Ed__37 _003CWaitUntilAllFlyFinished_003Ed__ = default(_003CWaitUntilAllFlyFinished_003Ed__37);
		_003CWaitUntilAllFlyFinished_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		_003CWaitUntilAllFlyFinished_003Ed__._003C_003E4__this = this;
		_003CWaitUntilAllFlyFinished_003Ed__.ct = ct;
		_003CWaitUntilAllFlyFinished_003Ed__._003C_003E1__state = -1;
		_003CWaitUntilAllFlyFinished_003Ed__._003C_003Et__builder.Start<_003CWaitUntilAllFlyFinished_003Ed__37>(ref _003CWaitUntilAllFlyFinished_003Ed__);
		return _003CWaitUntilAllFlyFinished_003Ed__._003C_003Et__builder.Task;
	}

	private bool HasPendingFly()
	{
		if (runningFlyEvents.Count <= 0 && activeFlyCount <= 0)
		{
			return isRunningFlyQueue;
		}
		return true;
	}

	private void TriggerPanelShown(UIRankStarCollectionView collectionView)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!isPanelShown || isPanelHiding)
		{
			if (isPanelHiding)
			{
				CancelHideExecute(collectionView);
				ResetPanelShownState();
			}
			else if (isShowingPanel)
			{
				return;
			}
			isShowingPanel = true;
			UniTaskExtensions.Forget(ShowPanelInternal(collectionView));
		}
	}

	[AsyncStateMachine(typeof(_003CShowPanelInternal_003Ed__40))]
	private UniTask ShowPanelInternal(UIRankStarCollectionView collectionView)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CShowPanelInternal_003Ed__40 _003CShowPanelInternal_003Ed__ = default(_003CShowPanelInternal_003Ed__40);
		_003CShowPanelInternal_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CShowPanelInternal_003Ed__._003C_003E4__this = this;
		_003CShowPanelInternal_003Ed__.collectionView = collectionView;
		_003CShowPanelInternal_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CShowPanelInternal_003Ed__._003C_003Et__builder)).Start<_003CShowPanelInternal_003Ed__40>(ref _003CShowPanelInternal_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CShowPanelInternal_003Ed__._003C_003Et__builder)).Task;
	}

	private void ExecuteHide(UIRankStarCollectionView collectionView)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (collectionView != null)
		{
			CancelHideExecute();
			hideCts = new CancellationTokenSource();
			UniTaskVoid val = ExecuteHideAsync(collectionView, hideCts.Token, hideGeneration);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CExecuteHideAsync_003Ed__42))]
	private UniTaskVoid ExecuteHideAsync(UIRankStarCollectionView collectionView, CancellationToken cancellationToken, int generation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CExecuteHideAsync_003Ed__42 _003CExecuteHideAsync_003Ed__ = default(_003CExecuteHideAsync_003Ed__42);
		_003CExecuteHideAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CExecuteHideAsync_003Ed__._003C_003E4__this = this;
		_003CExecuteHideAsync_003Ed__.collectionView = collectionView;
		_003CExecuteHideAsync_003Ed__.cancellationToken = cancellationToken;
		_003CExecuteHideAsync_003Ed__.generation = generation;
		_003CExecuteHideAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CExecuteHideAsync_003Ed__._003C_003Et__builder)).Start<_003CExecuteHideAsync_003Ed__42>(ref _003CExecuteHideAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CExecuteHideAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void CancelHideExecute(UIRankStarCollectionView collectionView = null)
	{
		hideGeneration++;
		if (hideCts != null)
		{
			hideCts.Cancel();
			hideCts.Dispose();
			hideCts = null;
		}
		if (isPanelHiding)
		{
			(collectionView ?? hidingCollectionView)?.StopHideAnimation();
			isPanelShown = false;
			isPanelHiding = false;
			hidingCollectionView = null;
		}
	}

	private void ResetPanelShownState()
	{
		isPanelShown = false;
		isShowingPanel = false;
	}

	private void PlayCollectFeedback(UIRankStarCollectionView collectionView)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (collectionView != null && IsCollectionViewActive(collectionView))
		{
			CancelFeedbackHide();
			collectionView.PlayCollectFeedback();
			feedbackHideCts = new CancellationTokenSource();
			UniTaskVoid val = HideCollectFeedbackAsync(collectionView, feedbackHideCts.Token);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CHideCollectFeedbackAsync_003Ed__46))]
	private UniTaskVoid HideCollectFeedbackAsync(UIRankStarCollectionView collectionView, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CHideCollectFeedbackAsync_003Ed__46 _003CHideCollectFeedbackAsync_003Ed__ = default(_003CHideCollectFeedbackAsync_003Ed__46);
		_003CHideCollectFeedbackAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CHideCollectFeedbackAsync_003Ed__.collectionView = collectionView;
		_003CHideCollectFeedbackAsync_003Ed__.cancellationToken = cancellationToken;
		_003CHideCollectFeedbackAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CHideCollectFeedbackAsync_003Ed__._003C_003Et__builder)).Start<_003CHideCollectFeedbackAsync_003Ed__46>(ref _003CHideCollectFeedbackAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CHideCollectFeedbackAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void CancelFeedbackHide()
	{
		if (feedbackHideCts != null)
		{
			feedbackHideCts.Cancel();
			feedbackHideCts.Dispose();
			feedbackHideCts = null;
		}
	}

	private void ResetFlyState(bool hidePanel)
	{
		panelGeneration++;
		showCts?.Cancel();
		showCts?.Dispose();
		showCts = null;
		CancelHideExecute();
		CancelFeedbackHide();
		isPanelHiding = false;
		hidingCollectionView = null;
		runningFlyEvents.Clear();
		isRunningFlyQueue = false;
		ResetPanelShownState();
		activeFlyCount = 0;
		lastFlyFinishedTime = 0f;
		ReleaseAllFlyEffects();
		if (hidePanel)
		{
			base.View?.RankStarCollectionView?.ResetToHiddenImmediate();
		}
	}

	internal static string GetFlyEffectLocation(int layout)
	{
		return RankCollectThemeResourceResolver.GetFlyEffectLocation(layout == 2 || layout == 3);
	}

	[AsyncStateMachine(typeof(_003CTryPlayCollectFlyEffect_003Ed__50))]
	private UniTask<bool> TryPlayCollectFlyEffect(UIGameView hostView, RectTransform spawnAnchor, UIRankStarCollectionView collectionView, Vector2 localPos, int collectCount)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_003CTryPlayCollectFlyEffect_003Ed__50 _003CTryPlayCollectFlyEffect_003Ed__ = default(_003CTryPlayCollectFlyEffect_003Ed__50);
		_003CTryPlayCollectFlyEffect_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		_003CTryPlayCollectFlyEffect_003Ed__._003C_003E4__this = this;
		_003CTryPlayCollectFlyEffect_003Ed__.hostView = hostView;
		_003CTryPlayCollectFlyEffect_003Ed__.spawnAnchor = spawnAnchor;
		_003CTryPlayCollectFlyEffect_003Ed__.collectionView = collectionView;
		_003CTryPlayCollectFlyEffect_003Ed__.localPos = localPos;
		_003CTryPlayCollectFlyEffect_003Ed__.collectCount = collectCount;
		_003CTryPlayCollectFlyEffect_003Ed__._003C_003E1__state = -1;
		_003CTryPlayCollectFlyEffect_003Ed__._003C_003Et__builder.Start<_003CTryPlayCollectFlyEffect_003Ed__50>(ref _003CTryPlayCollectFlyEffect_003Ed__);
		return _003CTryPlayCollectFlyEffect_003Ed__._003C_003Et__builder.Task;
	}

	private GameObject AcquireFlyEffect(RectTransform spawnAnchor, Vector2 localPos)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = flyEffectPool.Get();
		val.transform.SetParent((Transform)(object)spawnAnchor, false);
		activeFlyEffects.Add(val);
		Transform transform = val.transform;
		RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (val2 != null)
		{
			val2.anchoredPosition = localPos;
		}
		else
		{
			val.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
		}
		return val;
	}

	private GameObject CreateFlyEffectInstance()
	{
		GameObject obj = Object.Instantiate<GameObject>(flyEffectPrefab);
		obj.SetActive(false);
		return obj;
	}

	private static void OnReleaseFlyEffect(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			instance.GetComponent<XYParticleImage>()?.Stop(stopAndClear: true);
			instance.SetActive(false);
		}
	}

	private static void OnDestroyFlyEffectInstance(GameObject instance)
	{
		if ((Object)(object)instance != (Object)null)
		{
			Object.Destroy((Object)(object)instance);
		}
	}

	private static void PrepareParticle(XYParticleImage particle, GameObject instance)
	{
		particle.PlayMode = PlayMode.None;
		instance.SetActive(true);
		particle.Stop(stopAndClear: true);
	}

	[AsyncStateMachine(typeof(_003CPlayFlyEffectAsync_003Ed__56))]
	private UniTask<bool> PlayFlyEffectAsync(XYParticleImage particle, GameObject instance, UIRankStarCollectionView collectionView, int collectCount)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayFlyEffectAsync_003Ed__56 _003CPlayFlyEffectAsync_003Ed__ = default(_003CPlayFlyEffectAsync_003Ed__56);
		_003CPlayFlyEffectAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<bool>.Create();
		_003CPlayFlyEffectAsync_003Ed__._003C_003E4__this = this;
		_003CPlayFlyEffectAsync_003Ed__.particle = particle;
		_003CPlayFlyEffectAsync_003Ed__.instance = instance;
		_003CPlayFlyEffectAsync_003Ed__.collectionView = collectionView;
		_003CPlayFlyEffectAsync_003Ed__.collectCount = collectCount;
		_003CPlayFlyEffectAsync_003Ed__._003C_003E1__state = -1;
		_003CPlayFlyEffectAsync_003Ed__._003C_003Et__builder.Start<_003CPlayFlyEffectAsync_003Ed__56>(ref _003CPlayFlyEffectAsync_003Ed__);
		return _003CPlayFlyEffectAsync_003Ed__._003C_003Et__builder.Task;
	}

	private void ReleaseFlyEffect(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null) && activeFlyEffects.Remove(instance))
		{
			flyEffectPool.Release(instance);
		}
	}

	private void ReleaseAllFlyEffects()
	{
		for (int num = activeFlyEffects.Count - 1; num >= 0; num--)
		{
			GameObject val = activeFlyEffects[num];
			if ((Object)(object)val != (Object)null)
			{
				flyEffectPool.Release(val);
			}
		}
		activeFlyEffects.Clear();
	}

	private bool TryGetRankStarContext(out UIGameView hostView, out UIRankStarCollectionView collectionView)
	{
		hostView = base.View;
		collectionView = hostView?.RankStarCollectionView;
		if (hostView != null && collectionView != null && (Object)(object)collectionView.star != (Object)null)
		{
			return true;
		}
		hostView = null;
		collectionView = null;
		return false;
	}

	private static bool TryGetSpawnAnchor(UIGameView hostView, out RectTransform spawnAnchor)
	{
		spawnAnchor = hostView.CenterContent ?? hostView.RankStarAnhor;
		return (Object)(object)spawnAnchor != (Object)null;
	}

	private bool TryScreenToLocalPoint(Vector2 screenPosition, RectTransform anchor, out Vector2 localPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		localPos = default(Vector2);
		if ((Object)(object)anchor == (Object)null)
		{
			return false;
		}
		if ((Object)(object)cachedCanvas == (Object)null)
		{
			cachedCanvas = ((Component)anchor).GetComponentInParent<Canvas>();
		}
		Camera val = (((Object)(object)cachedCanvas != (Object)null) ? cachedCanvas.rootCanvas.worldCamera : null);
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(anchor, screenPosition, val, ref localPos);
	}

	private static bool IsCollectionViewActive(UIRankStarCollectionView collectionView)
	{
		if ((Object)(object)((collectionView != null) ? ((UIBaseView)collectionView).NodeRect : null) != (Object)null)
		{
			return ((Component)((UIBaseView)collectionView).NodeRect).gameObject.activeSelf;
		}
		return false;
	}

	private void ApplyCollectCount(UIRankStarCollectionView collectionView, int collectCount, string reason)
	{
		collectionView.SetNum(collectCount);
	}
}
