using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Hot.Model;
using RotateRings.Common;
using RotateRings.Model;
using Rotaterings.Rank;

namespace Game.Common.Services;

public static class HardLevelToastPreEnterService
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitForCompletionAsync_003Ed__3 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0069;
				}
				UniTaskCompletionSource completionSource = _completionSource;
				if (completionSource != null)
				{
					UniTask task = completionSource.Task;
					val = ((UniTask)(ref task)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CWaitForCompletionAsync_003Ed__3>(ref val, ref this);
						return;
					}
					goto IL_0069;
				}
				goto end_IL_0007;
				IL_0069:
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

	private static UniTaskCompletionSource _completionSource;

	public static void ResetState()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		_completionSource = new UniTaskCompletionSource();
	}

	public static bool ShouldWait()
	{
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 1;
		if (LevelDifficultyUtil.IsHardLevel(GameInfo.CurrentGameMode, level))
		{
			return true;
		}
		return RankActivityEntry.CanShowActivityOpenPopup();
	}

	[AsyncStateMachine(typeof(_003CWaitForCompletionAsync_003Ed__3))]
	public static UniTask WaitForCompletionAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitForCompletionAsync_003Ed__3 _003CWaitForCompletionAsync_003Ed__ = default(_003CWaitForCompletionAsync_003Ed__3);
		_003CWaitForCompletionAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWaitForCompletionAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWaitForCompletionAsync_003Ed__._003C_003Et__builder)).Start<_003CWaitForCompletionAsync_003Ed__3>(ref _003CWaitForCompletionAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWaitForCompletionAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public static void MarkCompleted()
	{
		UniTaskCompletionSource completionSource = _completionSource;
		if (completionSource != null)
		{
			completionSource.TrySetResult();
		}
	}

	public static string ResolveLocation(int styleId)
	{
		return styleId switch
		{
			2 => "UIGameDifficultyToastView_2", 
			3 => "UIGameDifficultyToastView_1", 
			_ => "UIGameDifficultyToastView", 
		};
	}
}
