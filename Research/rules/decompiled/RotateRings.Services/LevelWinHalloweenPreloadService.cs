using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Spine.Unity;
using UnityEngine;

namespace RotateRings.Services;

public static class LevelWinHalloweenPreloadService
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPrewarmHalloweenWidgetsAsync_003Ed__6 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private Awaiter _003C_003Eu__1;

		private Stopwatch _003Cstopwatch_003E5__2;

		private Awaiter<GameObject> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					if ((uint)(num - 1) <= 1u)
					{
						goto IL_006e;
					}
					YieldAwaitable val = UniTask.Yield();
					val2 = ((YieldAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPrewarmHalloweenWidgetsAsync_003Ed__6>(ref val2, ref this);
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
				goto IL_006e;
				IL_006e:
				try
				{
					Awaiter<GameObject> val3;
					if (num != 1)
					{
						if (num == 2)
						{
							val3 = _003C_003Eu__2;
							_003C_003Eu__2 = default(Awaiter<GameObject>);
							num = (_003C_003E1__state = -1);
							goto IL_015e;
						}
						_003Cstopwatch_003E5__2 = Stopwatch.StartNew();
						val3 = GameApp.Res.LoadAssetAsync<GameObject>("LevelWin_HalloweenTitle", "LevelWinHalloweenPreloadGroup").GetAwaiter();
						if (!val3.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = val3;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPrewarmHalloweenWidgetsAsync_003Ed__6>(ref val3, ref this);
							return;
						}
					}
					else
					{
						val3 = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
					}
					WarmupSpineData(val3.GetResult(), "LevelWin_HalloweenTitle");
					val3 = GameApp.Res.LoadAssetAsync<GameObject>("HalloweenBtnOK", "LevelWinHalloweenPreloadGroup").GetAwaiter();
					if (!val3.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__2 = val3;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPrewarmHalloweenWidgetsAsync_003Ed__6>(ref val3, ref this);
						return;
					}
					goto IL_015e;
					IL_015e:
					WarmupSpineData(val3.GetResult(), "HalloweenBtnOK");
					_003Cstopwatch_003E5__2.Stop();
					_003Cstopwatch_003E5__2 = null;
				}
				catch (Exception)
				{
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

	private const string Tag = "[LevelWinHalloween]";

	private const string PreloadResGroup = "LevelWinHalloweenPreloadGroup";

	private const string TitlePrefabLocation = "LevelWin_HalloweenTitle";

	private const string BtnOKPrefabLocation = "HalloweenBtnOK";

	private static bool isPrewarmStarted;

	public static void PrewarmHalloweenWidgets()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!isPrewarmStarted)
		{
			isPrewarmStarted = true;
			UniTaskVoid val = PrewarmHalloweenWidgetsAsync();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CPrewarmHalloweenWidgetsAsync_003Ed__6))]
	private static UniTaskVoid PrewarmHalloweenWidgetsAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPrewarmHalloweenWidgetsAsync_003Ed__6 _003CPrewarmHalloweenWidgetsAsync_003Ed__ = default(_003CPrewarmHalloweenWidgetsAsync_003Ed__6);
		_003CPrewarmHalloweenWidgetsAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPrewarmHalloweenWidgetsAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPrewarmHalloweenWidgetsAsync_003Ed__._003C_003Et__builder)).Start<_003CPrewarmHalloweenWidgetsAsync_003Ed__6>(ref _003CPrewarmHalloweenWidgetsAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPrewarmHalloweenWidgetsAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static void WarmupSpineData(GameObject prefab, string location)
	{
		if (!((Object)(object)prefab == (Object)null))
		{
			SkeletonGraphic componentInChildren = prefab.GetComponentInChildren<SkeletonGraphic>(true);
			if (!((Object)(object)componentInChildren == (Object)null) && !((Object)(object)componentInChildren.skeletonDataAsset == (Object)null))
			{
				componentInChildren.skeletonDataAsset.GetSkeletonData(false);
			}
		}
	}
}
