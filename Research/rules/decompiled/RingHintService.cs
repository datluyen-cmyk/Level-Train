using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.UI.Toast;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;
using UnlockRing.Game.Gameplay;

public sealed class RingHintService
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunHintQueryAsync_003Ed__16 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingHintService _003C_003E4__this;

		public CoreGameController controller;

		public CancellationToken ct;

		private TaskAwaiter<IReadOnlyList<RingHintResult>> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingHintService ringHintService = _003C_003E4__this;
			try
			{
				try
				{
					TaskAwaiter<IReadOnlyList<RingHintResult>> taskAwaiter;
					if (num != 0)
					{
						taskAwaiter = ringHintService.ringHintFinder.QueryAllAsync(controller.Level, controller.State, controller, ct).GetAwaiter();
						if (!taskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = taskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<TaskAwaiter<IReadOnlyList<RingHintResult>>, _003CRunHintQueryAsync_003Ed__16>(ref taskAwaiter, ref this);
							return;
						}
					}
					else
					{
						taskAwaiter = _003C_003Eu__1;
						_003C_003Eu__1 = default(TaskAwaiter<IReadOnlyList<RingHintResult>>);
						num = (_003C_003E1__state = -1);
					}
					IReadOnlyList<RingHintResult> result = taskAwaiter.GetResult();
					if (!ct.IsCancellationRequested)
					{
						if (result == null || result.Count == 0)
						{
							ToastService.Show("暂无提示");
							NotifyHintFailed("no_candidates");
							ringHintService.ClearPropGuidePreviewDisplayIfActive();
							ringHintService.ClearHintQueryOnly();
						}
						else
						{
							ringHintService.hintTargetPartId = result[0].PartId;
							ringHintService.TryRefreshPropGuidePreview();
						}
					}
				}
				catch (OperationCanceledException)
				{
					ringHintService.ClearHintQueryOnly();
				}
				catch (Exception ex2)
				{
					ToastService.Show("暂无提示");
					NotifyHintFailed(ex2.Message);
					ringHintService.ClearPropGuidePreviewDisplayIfActive();
					ringHintService.ClearHintQueryOnly();
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

	private const string Tag = "[UnlockRing][RingHint] ";

	private readonly RingGameManager gameManager;

	private readonly RingHintFinder ringHintFinder = new RingHintFinder();

	private PartId? hintTargetPartId;

	private CancellationTokenSource hintCts;

	private Action<RenderTexture> onPropGuidePreviewReady;

	private Action onPropGuidePreviewCleared;

	private bool isPropGuidePreviewActive;

	public PartId? TargetPartId => hintTargetPartId;

	public RingHintService(RingGameManager gameManager)
	{
		this.gameManager = gameManager;
	}

	public void StartPropGuidePreview(Action<RenderTexture> onReady, Action onCleared)
	{
		StopPropGuidePreview();
		onPropGuidePreviewReady = onReady;
		onPropGuidePreviewCleared = onCleared;
		isPropGuidePreviewActive = true;
		TryRefreshPropGuidePreview();
	}

	public void TryRefreshPropGuidePreview()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!isPropGuidePreviewActive || !hintTargetPartId.HasValue)
		{
			return;
		}
		RingGuideRTService guideRT = gameManager.GuideRT;
		if (guideRT != null)
		{
			RenderTexture val = guideRT.CreatePartGuideRT(hintTargetPartId.Value);
			if (!((Object)(object)val == (Object)null))
			{
				onPropGuidePreviewReady?.Invoke(val);
			}
		}
	}

	public void StopPropGuidePreview()
	{
		isPropGuidePreviewActive = false;
		onPropGuidePreviewReady = null;
		onPropGuidePreviewCleared = null;
		gameManager.GuideRT?.ReleaseGuideRenderTexture();
	}

	public void ClearHintSession()
	{
		ClearHintQueryOnly();
		StopPropGuidePreview();
	}

	public bool ShowHint()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		CoreBoardRenderer boardRenderer = gameManager.BoardRenderer;
		CoreGameController coreGameController = boardRenderer?.Controller;
		if ((Object)(object)boardRenderer == (Object)null || coreGameController == null || gameManager.IsLevelCompleted)
		{
			return false;
		}
		ClearHintQueryOnly();
		hintCts = new CancellationTokenSource();
		UniTaskVoid val = RunHintQueryAsync(boardRenderer, coreGameController, hintCts.Token);
		((UniTaskVoid)(ref val)).Forget();
		return true;
	}

	[AsyncStateMachine(typeof(_003CRunHintQueryAsync_003Ed__16))]
	private UniTaskVoid RunHintQueryAsync(CoreBoardRenderer board, CoreGameController controller, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRunHintQueryAsync_003Ed__16 _003CRunHintQueryAsync_003Ed__ = default(_003CRunHintQueryAsync_003Ed__16);
		_003CRunHintQueryAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunHintQueryAsync_003Ed__._003C_003E4__this = this;
		_003CRunHintQueryAsync_003Ed__.controller = controller;
		_003CRunHintQueryAsync_003Ed__.ct = ct;
		_003CRunHintQueryAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunHintQueryAsync_003Ed__._003C_003Et__builder)).Start<_003CRunHintQueryAsync_003Ed__16>(ref _003CRunHintQueryAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunHintQueryAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void ClearHintQueryOnly()
	{
		hintCts?.Cancel();
		hintCts?.Dispose();
		hintCts = null;
		hintTargetPartId = null;
	}

	private void ClearPropGuidePreviewDisplayIfActive()
	{
		if (isPropGuidePreviewActive)
		{
			onPropGuidePreviewCleared?.Invoke();
			gameManager.GuideRT?.ReleaseGuideRenderTexture();
		}
	}

	private static void NotifyHintFailed(string reason)
	{
		GameApp.Event.Send<RingHintFailedEvent>(new RingHintFailedEvent(reason));
	}
}
