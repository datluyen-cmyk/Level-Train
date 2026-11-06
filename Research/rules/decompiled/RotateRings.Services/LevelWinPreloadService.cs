using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.UI.LevelEnd;
using Spine.Unity;
using TMPro;
using UnityEngine;
using XYUIKit.Runtime;
using XingYun.Aot;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.Services;

public static class LevelWinPreloadService
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass32_0
	{
		public GameObject levelWinPrefab;

		internal void _003CPreloadInternalAsync_003Eb__0(GameObject prefab)
		{
			levelWinPrefab = prefab;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CCaptureLevelWinPrefabAsync_003Ed__43 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken cancellationToken;

		public Action<GameObject> onLoaded;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<GameObject> val;
				if (num != 0)
				{
					val = PreloadLevelWinPrefabAsync(cancellationToken).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CCaptureLevelWinPrefabAsync_003Ed__43>(ref val, ref this);
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
				onLoaded?.Invoke(result);
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
	private struct _003CPreloadAudioClipAsync_003Ed__40 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public string clipLocation;

		public CancellationToken cancellationToken;

		private Awaiter<AudioClip> _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<bool> val;
				Awaiter<AudioClip> val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_00e2;
					}
					val2 = GameApp.Res.LoadAssetAsync<AudioClip>(clipLocation, Define.ResGroup).GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<AudioClip>, _003CPreloadAudioClipAsync_003Ed__40>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<AudioClip>);
					num = (_003C_003E1__state = -1);
				}
				val2.GetResult();
				UniTask val3 = UniTask.Yield(cancellationToken);
				val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val;
					((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPreloadAudioClipAsync_003Ed__40>(ref val, ref this);
					return;
				}
				goto IL_00e2;
				IL_00e2:
				val.GetResult();
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
	private struct _003CPreloadInternalAsync_003Ed__32 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken cancellationToken;

		private _003C_003Ec__DisplayClass32_0 _003C_003E8__1;

		public int gen;

		private Stopwatch _003Cstopwatch_003E5__2;

		private GameObject _003CspinePrefab_003E5__3;

		private Awaiter<GameObject> _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private Awaiter _003C_003Eu__3;

		private void MoveNext()
		{
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_035f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0364: Unknown result type (might be due to invalid IL or missing references)
			//IL_036c: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0388: Unknown result type (might be due to invalid IL or missing references)
			//IL_038d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0391: Unknown result type (might be due to invalid IL or missing references)
			//IL_0396: Unknown result type (might be due to invalid IL or missing references)
			//IL_039a: Unknown result type (might be due to invalid IL or missing references)
			//IL_039f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_030a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0317: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Unknown result type (might be due to invalid IL or missing references)
			//IL_032a: Unknown result type (might be due to invalid IL or missing references)
			//IL_032f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_0344: Unknown result type (might be due to invalid IL or missing references)
			//IL_0346: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if ((uint)num > 6u)
				{
					_003Cstopwatch_003E5__2 = Stopwatch.StartNew();
				}
				try
				{
					Awaiter<GameObject> val3;
					Awaiter val2;
					Awaiter<bool> val;
					UniTask val4;
					GameObject result;
					switch (num)
					{
					default:
						_003C_003E8__1 = new _003C_003Ec__DisplayClass32_0();
						val3 = GameApp.Res.LoadAssetAsync<GameObject>(LevelWinResourceKeys.GetSpinePrefab(), "UILevelWin_Animal").GetAwaiter();
						if (!val3.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val3;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPreloadInternalAsync_003Ed__32>(ref val3, ref this);
							return;
						}
						goto IL_00a9;
					case 0:
						val3 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
						goto IL_00a9;
					case 1:
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_0121;
					case 2:
						val2 = _003C_003Eu__3;
						_003C_003Eu__3 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_01a7;
					case 3:
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_0217;
					case 4:
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_0299;
					case 5:
						val2 = _003C_003Eu__3;
						_003C_003Eu__3 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_037b;
					case 6:
						{
							val = _003C_003Eu__2;
							_003C_003Eu__2 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							break;
						}
						IL_01a7:
						((Awaiter)(ref val2)).GetResult();
						val4 = UniTask.Yield(cancellationToken);
						val = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 3);
							_003C_003Eu__2 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPreloadInternalAsync_003Ed__32>(ref val, ref this);
							return;
						}
						goto IL_0217;
						IL_00a9:
						result = val3.GetResult();
						_003CspinePrefab_003E5__3 = result;
						val4 = UniTask.Yield(cancellationToken);
						val = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPreloadInternalAsync_003Ed__32>(ref val, ref this);
							return;
						}
						goto IL_0121;
						IL_0299:
						if (!val.GetResult())
						{
							goto IL_02a7;
						}
						goto end_IL_0017;
						IL_0121:
						if (!val.GetResult())
						{
							if ((Object)(object)_003CspinePrefab_003E5__3 != (Object)null)
							{
								val4 = GameApp.Pool.PreloadAsync(_003CspinePrefab_003E5__3, 1, 1);
								val2 = ((UniTask)(ref val4)).GetAwaiter();
								if (!((Awaiter)(ref val2)).IsCompleted)
								{
									num = (_003C_003E1__state = 2);
									_003C_003Eu__3 = val2;
									((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPreloadInternalAsync_003Ed__32>(ref val2, ref this);
									return;
								}
								goto IL_01a7;
							}
							goto IL_02a7;
						}
						goto end_IL_0017;
						IL_0217:
						if (!val.GetResult())
						{
							PrewarmSpineSkeletonData(_003CspinePrefab_003E5__3);
							val4 = UniTask.Yield(cancellationToken);
							val = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 4);
								_003C_003Eu__2 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPreloadInternalAsync_003Ed__32>(ref val, ref this);
								return;
							}
							goto IL_0299;
						}
						goto end_IL_0017;
						IL_02a7:
						_003C_003E8__1.levelWinPrefab = null;
						val4 = UniTask.WhenAll((UniTask[])(object)new UniTask[4]
						{
							CaptureLevelWinPrefabAsync(cancellationToken, delegate(GameObject prefab)
							{
								_003C_003E8__1.levelWinPrefab = prefab;
							}),
							PreloadPrefabAsync("eff_uiwin_trail_01", cancellationToken, "UILevelWin_Effect"),
							PreloadAudioClipAsync("rr_sfx_level_complete_2", cancellationToken),
							PreloadVoClipsAsync(cancellationToken)
						});
						val2 = ((UniTask)(ref val4)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 5);
							_003C_003Eu__3 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPreloadInternalAsync_003Ed__32>(ref val2, ref this);
							return;
						}
						goto IL_037b;
						IL_037b:
						((Awaiter)(ref val2)).GetResult();
						val4 = UniTask.Yield(cancellationToken);
						val = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 6);
							_003C_003Eu__2 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPreloadInternalAsync_003Ed__32>(ref val, ref this);
							return;
						}
						break;
					}
					if (!val.GetResult())
					{
						isReady = true;
						_003Cstopwatch_003E5__2.Stop();
						if (!cancellationToken.IsCancellationRequested)
						{
							ShouldRunPhotoshopWarmup();
							RunPhotoshopWarmupFallbackInBackground(_003C_003E8__1.levelWinPrefab);
							_003C_003E8__1 = null;
							_003CspinePrefab_003E5__3 = null;
						}
					}
					end_IL_0017:;
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception)
				{
					if (gen == generation)
					{
						isReady = true;
					}
				}
				finally
				{
					if (num < 0 && gen == generation)
					{
						isLoading = false;
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cstopwatch_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cstopwatch_003E5__2 = null;
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
	private struct _003CPreloadLevelWinPrefabAsync_003Ed__42 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<GameObject> _003C_003Et__builder;

		public CancellationToken cancellationToken;

		private GameObject _003Cprefab_003E5__2;

		private Awaiter<GameObject> _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameObject result2;
			try
			{
				Awaiter<bool> val;
				Awaiter<GameObject> val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_00e8;
					}
					val2 = GameApp.Res.LoadAssetAsync<GameObject>("UILevelWin", (string)null).GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPreloadLevelWinPrefabAsync_003Ed__42>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
				}
				GameObject result = val2.GetResult();
				_003Cprefab_003E5__2 = result;
				UniTask val3 = UniTask.Yield(cancellationToken);
				val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val;
					_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPreloadLevelWinPrefabAsync_003Ed__42>(ref val, ref this);
					return;
				}
				goto IL_00e8;
				IL_00e8:
				val.GetResult();
				result2 = _003Cprefab_003E5__2;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cprefab_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cprefab_003E5__2 = null;
			_003C_003Et__builder.SetResult(result2);
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
	private struct _003CPreloadPrefabAsync_003Ed__39 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public string location;

		public string group;

		public CancellationToken cancellationToken;

		private Awaiter<GameObject> _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<bool> val;
				Awaiter<GameObject> val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_00ed;
					}
					val2 = GameApp.Res.LoadAssetAsync<GameObject>(location, group ?? location).GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPreloadPrefabAsync_003Ed__39>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
				}
				val2.GetResult();
				UniTask val3 = UniTask.Yield(cancellationToken);
				val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val;
					((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPreloadPrefabAsync_003Ed__39>(ref val, ref this);
					return;
				}
				goto IL_00ed;
				IL_00ed:
				val.GetResult();
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
	private struct _003CPreloadVoClipsAsync_003Ed__41 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken cancellationToken;

		private Awaiter _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<bool> val;
				UniTask val2;
				Awaiter val3;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_010b;
					}
					string[] voCompleteSquirrelClips = LevelWinResourceKeys.VoCompleteSquirrelClips;
					UniTask[] array = (UniTask[])(object)new UniTask[voCompleteSquirrelClips.Length];
					for (int i = 0; i < voCompleteSquirrelClips.Length; i++)
					{
						array[i] = PreloadAudioClipAsync(voCompleteSquirrelClips[i], cancellationToken);
					}
					val2 = UniTask.WhenAll(array);
					val3 = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val3)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val3;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPreloadVoClipsAsync_003Ed__41>(ref val3, ref this);
						return;
					}
				}
				else
				{
					val3 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val3)).GetResult();
				val2 = UniTask.Yield(cancellationToken);
				val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
				if (!val.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val;
					((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPreloadVoClipsAsync_003Ed__41>(ref val, ref this);
					return;
				}
				goto IL_010b;
				IL_010b:
				val.GetResult();
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
	private struct _003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__36 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameObject prefab;

		private CancellationToken _003CcancellationToken_003E5__2;

		private Stopwatch _003CtotalStopwatch_003E5__3;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num == 0)
				{
					goto IL_0038;
				}
				if (preloadCts != null)
				{
					_003CcancellationToken_003E5__2 = preloadCts.Token;
					isPhotoshopWarmupRunning = true;
					_003CtotalStopwatch_003E5__3 = Stopwatch.StartNew();
					goto IL_0038;
				}
				goto end_IL_0007;
				IL_0038:
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = WarmupLevelWinPhotoshopAsync(prefab, _003CcancellationToken_003E5__2, "结算兜底");
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__36>(ref val2, ref this);
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
					if (!_003CcancellationToken_003E5__2.IsCancellationRequested)
					{
						if (ShouldRunPhotoshopWarmup())
						{
							MarkPhotoshopWarmupCompleted();
						}
						_003CtotalStopwatch_003E5__3.Stop();
					}
				}
				catch (OperationCanceledException)
				{
				}
				finally
				{
					if (num < 0)
					{
						isPhotoshopWarmupRunning = false;
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CcancellationToken_003E5__2 = default(CancellationToken);
				_003CtotalStopwatch_003E5__3 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CcancellationToken_003E5__2 = default(CancellationToken);
			_003CtotalStopwatch_003E5__3 = null;
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
	private struct _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private CancellationToken _003CcancellationToken_003E5__2;

		private Stopwatch _003CtotalStopwatch_003E5__3;

		private GameObject _003Cprefab_003E5__4;

		private Awaiter<bool> _003C_003Eu__1;

		private Awaiter<GameObject> _003C_003Eu__2;

		private Awaiter _003C_003Eu__3;

		private void MoveNext()
		{
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if ((uint)num > 3u)
				{
					photoshopWarmupCts?.Cancel();
					photoshopWarmupCts?.Dispose();
					photoshopWarmupCts = new CancellationTokenSource();
					_003CcancellationToken_003E5__2 = photoshopWarmupCts.Token;
					isPhotoshopWarmupRunning = true;
					_003CtotalStopwatch_003E5__3 = Stopwatch.StartNew();
				}
				try
				{
					UniTask val4;
					Awaiter<GameObject> val3;
					Awaiter<bool> val2;
					Awaiter val;
					GameObject result;
					switch (num)
					{
					default:
						val4 = UniTask.DelayFrame(3, (PlayerLoopTiming)8, _003CcancellationToken_003E5__2);
						val2 = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34>(ref val2, ref this);
							return;
						}
						goto IL_00d3;
					case 0:
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_00d3;
					case 1:
						val3 = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
						goto IL_0146;
					case 2:
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_01bb;
					case 3:
						{
							val = _003C_003Eu__3;
							_003C_003Eu__3 = default(Awaiter);
							num = (_003C_003E1__state = -1);
							break;
						}
						IL_01bb:
						if (!val2.GetResult() && !((Object)(object)_003Cprefab_003E5__4 == (Object)null))
						{
							val4 = WarmupLevelWinPhotoshopAsync(_003Cprefab_003E5__4, _003CcancellationToken_003E5__2, "首关后台");
							val = ((UniTask)(ref val4)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 3);
								_003C_003Eu__3 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34>(ref val, ref this);
								return;
							}
							break;
						}
						goto end_IL_0057;
						IL_00d3:
						if (!val2.GetResult())
						{
							val3 = GameApp.Res.LoadAssetAsync<GameObject>("UILevelWin", (string)null).GetAwaiter();
							if (!val3.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__2 = val3;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34>(ref val3, ref this);
								return;
							}
							goto IL_0146;
						}
						goto end_IL_0057;
						IL_0146:
						result = val3.GetResult();
						_003Cprefab_003E5__4 = result;
						val4 = UniTask.Yield(_003CcancellationToken_003E5__2);
						val2 = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34>(ref val2, ref this);
							return;
						}
						goto IL_01bb;
					}
					((Awaiter)(ref val)).GetResult();
					if (!_003CcancellationToken_003E5__2.IsCancellationRequested)
					{
						MarkPhotoshopWarmupCompleted();
						_003CtotalStopwatch_003E5__3.Stop();
					}
					_003Cprefab_003E5__4 = null;
					end_IL_0057:;
				}
				catch (OperationCanceledException)
				{
				}
				finally
				{
					if (num < 0)
					{
						isPhotoshopWarmupRunning = false;
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CcancellationToken_003E5__2 = default(CancellationToken);
				_003CtotalStopwatch_003E5__3 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CcancellationToken_003E5__2 = default(CancellationToken);
			_003CtotalStopwatch_003E5__3 = null;
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
	private struct _003CRunStartupPrewarmFireAndForget_003Ed__29 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

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
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = UniTask.DelayFrame(30, (PlayerLoopTiming)8, default(CancellationToken));
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunStartupPrewarmFireAndForget_003Ed__29>(ref val2, ref this);
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
				StartPreload();
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
	private struct _003CWaitUntilReady_003Ed__30 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public CancellationToken cancellationToken;

		private Awaiter<int> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<int> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<int>);
					num = (_003C_003E1__state = -1);
					goto IL_00d5;
				}
				if (!isReady)
				{
					if (!isLoading)
					{
						StartPreload();
					}
					if (!isReady)
					{
						val = UniTask.WhenAny((UniTask[])(object)new UniTask[2]
						{
							UniTask.WaitUntil((Func<bool>)(() => isReady), (PlayerLoopTiming)8, cancellationToken),
							UniTask.Delay(15000, false, (PlayerLoopTiming)8, cancellationToken)
						}).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<int>, _003CWaitUntilReady_003Ed__30>(ref val, ref this);
							return;
						}
						goto IL_00d5;
					}
				}
				goto end_IL_0007;
				IL_00d5:
				if (val.GetResult() == 1)
				{
					_ = isReady;
				}
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
	private struct _003CWarmupLevelWinPhotoshopAsync_003Ed__44 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public GameObject prefab;

		public CancellationToken cancellationToken;

		private Stopwatch _003Cstopwatch_003E5__2;

		private int _003CrefreshedCount_003E5__3;

		private bool _003Copened_003E5__4;

		private IController _003CwarmupController_003E5__5;

		private bool _003CisWarmupBroken_003E5__6;

		private RectTransform _003CinstanceRect_003E5__7;

		private XYUITMPPhotoshop[] _003CphotoshopComponents_003E5__8;

		private int _003Ci_003E5__9;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0370: Unknown result type (might be due to invalid IL or missing references)
			//IL_0380: Unknown result type (might be due to invalid IL or missing references)
			//IL_0390: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0320: Unknown result type (might be due to invalid IL or missing references)
			//IL_0325: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if ((uint)num <= 2u)
				{
					goto IL_0063;
				}
				if (!((Object)(object)prefab == (Object)null) && !GameApp.UI.IsOpen("UILevelWin"))
				{
					_003Cstopwatch_003E5__2 = Stopwatch.StartNew();
					_003CrefreshedCount_003E5__3 = 0;
					_003Copened_003E5__4 = false;
					_003CwarmupController_003E5__5 = null;
					_003CisWarmupBroken_003E5__6 = false;
					_003CinstanceRect_003E5__7 = null;
					goto IL_0063;
				}
				goto end_IL_0007;
				IL_0063:
				try
				{
					Awaiter<bool> val;
					UniTask val4;
					switch (num)
					{
					default:
					{
						GameApp.UI.Open("UILevelWin", (UILayer)0, (object)null, (Action)null);
						IController val2 = default(IController);
						_003Copened_003E5__4 = GameApp.UI.IsOpen("UILevelWin", ref val2);
						_003CwarmupController_003E5__5 = val2;
						if (_003Copened_003E5__4 && val2 is UILevelWinController uILevelWinController && !((Object)(object)uILevelWinController.InstanceRoot == (Object)null))
						{
							GameObject instanceRoot = uILevelWinController.InstanceRoot;
							ref RectTransform reference = ref _003CinstanceRect_003E5__7;
							Transform transform = instanceRoot.transform;
							reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
							if ((Object)(object)_003CinstanceRect_003E5__7 != (Object)null)
							{
								_003CinstanceRect_003E5__7.anchoredPosition = OffscreenWarmupPosition;
							}
							ActivateWarmupHierarchy(instanceRoot);
							WarmupEvaluationTextOnInstance(instanceRoot);
							if (ShouldRunPhotoshopWarmup())
							{
								_003CphotoshopComponents_003E5__8 = instanceRoot.GetComponentsInChildren<XYUITMPPhotoshop>(true);
								_003Ci_003E5__9 = 0;
								goto IL_020f;
							}
							goto IL_02cc;
						}
						goto end_IL_0063;
					}
					case 0:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_01ef;
					case 1:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_0299;
					case 2:
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							break;
						}
						IL_020f:
						if (_003Ci_003E5__9 < _003CphotoshopComponents_003E5__8.Length)
						{
							XYUITMPPhotoshop val3 = _003CphotoshopComponents_003E5__8[_003Ci_003E5__9];
							if (!((Object)(object)val3 == (Object)null) && ((Behaviour)val3).isActiveAndEnabled)
							{
								val3.Refresh();
								TMP_Text component = ((Component)val3).GetComponent<TMP_Text>();
								if (component != null)
								{
									component.ForceMeshUpdate(true, true);
								}
								_003CrefreshedCount_003E5__3++;
								val4 = UniTask.Yield((PlayerLoopTiming)13, cancellationToken);
								val = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 0);
									_003C_003Eu__1 = val;
									((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWarmupLevelWinPhotoshopAsync_003Ed__44>(ref val, ref this);
									return;
								}
								goto IL_01ef;
							}
							goto IL_01fd;
						}
						_003Ci_003E5__9 = 0;
						goto IL_02b9;
						IL_0299:
						if (!val.GetResult())
						{
							_003Ci_003E5__9++;
							goto IL_02b9;
						}
						goto end_IL_0063;
						IL_02b9:
						if (_003Ci_003E5__9 < 4)
						{
							val4 = UniTask.Yield((PlayerLoopTiming)13, cancellationToken);
							val = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWarmupLevelWinPhotoshopAsync_003Ed__44>(ref val, ref this);
								return;
							}
							goto IL_0299;
						}
						_003CphotoshopComponents_003E5__8 = null;
						goto IL_02cc;
						IL_01ef:
						if (!val.GetResult())
						{
							goto IL_01fd;
						}
						goto end_IL_0063;
						IL_02cc:
						Canvas.ForceUpdateCanvases();
						val4 = UniTask.Yield((PlayerLoopTiming)13, cancellationToken);
						val = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CWarmupLevelWinPhotoshopAsync_003Ed__44>(ref val, ref this);
							return;
						}
						break;
						IL_01fd:
						_003Ci_003E5__9++;
						goto IL_020f;
					}
					val.GetResult();
					end_IL_0063:;
				}
				catch (Exception)
				{
					_003CisWarmupBroken_003E5__6 = true;
				}
				finally
				{
					if (num < 0)
					{
						if ((Object)(object)_003CinstanceRect_003E5__7 != (Object)null)
						{
							_003CinstanceRect_003E5__7.anchorMin = Vector2.zero;
							_003CinstanceRect_003E5__7.anchorMax = Vector2.one;
							_003CinstanceRect_003E5__7.offsetMin = Vector2.zero;
							_003CinstanceRect_003E5__7.offsetMax = Vector2.zero;
						}
						IController val5 = default(IController);
						if (_003Copened_003E5__4 && GameApp.UI.IsOpen("UILevelWin", ref val5) && val5 == _003CwarmupController_003E5__5)
						{
							GameApp.UI.CloseView("UILevelWin", (object)null, !cancellationToken.IsCancellationRequested && !_003CisWarmupBroken_003E5__6);
						}
						_003Cstopwatch_003E5__2.Stop();
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cstopwatch_003E5__2 = null;
				_003CwarmupController_003E5__5 = null;
				_003CinstanceRect_003E5__7 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cstopwatch_003E5__2 = null;
			_003CwarmupController_003E5__5 = null;
			_003CinstanceRect_003E5__7 = null;
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

	private const string Tag = "[LevelWinPreloadService]";

	private const int SpinePoolPreloadBatch = 1;

	private const int WaitReadyTimeoutMs = 15000;

	private const int PhotoshopWarmupFrameCount = 4;

	private const int LevelLoadedWarmupDeferFrames = 3;

	private const int FirstLevelNumber = 1;

	private const string EvaluationWarmupText = "Well Done!";

	private const string PhotoshopWarmupDonePrefsKey = "LevelWinPhotoshopWarmupDone";

	private static readonly Vector2 OffscreenWarmupPosition = new Vector2(10000f, 10000f);

	private const string LevelWinFontPrewarmCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,!?:;'\"()[]%+-/=<>#&@";

	private static readonly string[] LevelWinFontPrewarmTextKeys = new string[25]
	{
		"LevelComplete_Title", "LevelComplete_Title_1", "LevelComplete_Title_2", "LevelComplete_Title_3", "LevelComplete_Title_4", "LevelComplete_Title_5", "LevelComplete_Title_6", "LevelComplete_CheerUp_0", "LevelComplete_CheerUp_1", "LevelComplete_CheerUp_2",
		"LevelComplete_CheerUp_3", "LevelComplete_CheerUp_4", "LevelComplete_CheerUp_5", "LevelComplete_CheerUp_6", "LevelComplete_CheerUp_7", "LevelComplete_CheerUp_8", "LevelComplete_CheerUp_9", "LevelComplete_Score", "Text_Genius", "Text_Endless",
		"Button_Continue", "rotate_score_meirizuijia_1", "chest_progress", "chest_title", "chest_unlocked"
	};

	private static bool isLevelWinFontPrewarmed;

	private const string LevelWinFontPrewarmFontKeyword = "Roboto-ExtraBold";

	private static bool isReady;

	private static bool isLoading;

	private static bool photoshopWarmupCompleted;

	private static bool isPhotoshopWarmupRunning;

	private static CancellationTokenSource preloadCts;

	private static CancellationTokenSource photoshopWarmupCts;

	private static int generation;

	private static bool isStartupPrewarmScheduled;

	private const int StartupPrewarmDelayFrames = 30;

	public static bool IsReady => isReady;

	public static void TryPrewarmLevelWinFontGlyphs()
	{
		if (isLevelWinFontPrewarmed)
		{
			return;
		}
		isLevelWinFontPrewarmed = true;
		Stopwatch stopwatch = Stopwatch.StartNew();
		string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,!?:;'\"()[]%+-/=<>#&@" + CollectLocalizedPrewarmText();
		int num = 0;
		TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
		string value = default(string);
		foreach (TMP_FontAsset val in array)
		{
			if (!((Object)(object)val == (Object)null) && ((Object)val).name.IndexOf("Roboto-ExtraBold", StringComparison.Ordinal) >= 0)
			{
				val.TryAddCharacters(text, ref value, false);
				num++;
				string.IsNullOrEmpty(value);
			}
		}
		stopwatch.Stop();
	}

	private static string CollectLocalizedPrewarmText()
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		for (int i = 0; i < LevelWinFontPrewarmTextKeys.Length; i++)
		{
			string text = GameApp.Lang.GetText(LevelWinFontPrewarmTextKeys[i], Array.Empty<object>());
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append(text).Append(' ');
			}
		}
		return stringBuilder.ToString();
	}

	public static void TryStartPhotoshopWarmupOnLevelLoaded(int level)
	{
		if (level == 1 && ShouldRunPhotoshopWarmup())
		{
			_ = isPhotoshopWarmupRunning;
		}
	}

	public static void StartPreload()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!isReady && !isLoading)
		{
			preloadCts?.Dispose();
			preloadCts = new CancellationTokenSource();
			generation++;
			isLoading = true;
			UniTaskVoid val = PreloadInternalAsync(preloadCts.Token, generation);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	public static void PrewarmAtStartup()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!isStartupPrewarmScheduled && !isReady && !isLoading)
		{
			isStartupPrewarmScheduled = true;
			UniTaskVoid val = RunStartupPrewarmFireAndForget();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CRunStartupPrewarmFireAndForget_003Ed__29))]
	private static UniTaskVoid RunStartupPrewarmFireAndForget()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CRunStartupPrewarmFireAndForget_003Ed__29 _003CRunStartupPrewarmFireAndForget_003Ed__ = default(_003CRunStartupPrewarmFireAndForget_003Ed__29);
		_003CRunStartupPrewarmFireAndForget_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunStartupPrewarmFireAndForget_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunStartupPrewarmFireAndForget_003Ed__._003C_003Et__builder)).Start<_003CRunStartupPrewarmFireAndForget_003Ed__29>(ref _003CRunStartupPrewarmFireAndForget_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunStartupPrewarmFireAndForget_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CWaitUntilReady_003Ed__30))]
	public static UniTask WaitUntilReady(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CWaitUntilReady_003Ed__30 _003CWaitUntilReady_003Ed__ = default(_003CWaitUntilReady_003Ed__30);
		_003CWaitUntilReady_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWaitUntilReady_003Ed__.cancellationToken = cancellationToken;
		_003CWaitUntilReady_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWaitUntilReady_003Ed__._003C_003Et__builder)).Start<_003CWaitUntilReady_003Ed__30>(ref _003CWaitUntilReady_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWaitUntilReady_003Ed__._003C_003Et__builder)).Task;
	}

	public static void NotifyResourcesReleased()
	{
		preloadCts?.Cancel();
		preloadCts?.Dispose();
		preloadCts = null;
		photoshopWarmupCts?.Cancel();
		photoshopWarmupCts?.Dispose();
		photoshopWarmupCts = null;
		isReady = false;
		isLoading = false;
	}

	[AsyncStateMachine(typeof(_003CPreloadInternalAsync_003Ed__32))]
	private static UniTaskVoid PreloadInternalAsync(CancellationToken cancellationToken, int gen)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPreloadInternalAsync_003Ed__32 _003CPreloadInternalAsync_003Ed__ = default(_003CPreloadInternalAsync_003Ed__32);
		_003CPreloadInternalAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPreloadInternalAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPreloadInternalAsync_003Ed__.gen = gen;
		_003CPreloadInternalAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPreloadInternalAsync_003Ed__._003C_003Et__builder)).Start<_003CPreloadInternalAsync_003Ed__32>(ref _003CPreloadInternalAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPreloadInternalAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static void PrewarmSpineSkeletonData(GameObject spinePrefab)
	{
		SkeletonGraphic componentInChildren = spinePrefab.GetComponentInChildren<SkeletonGraphic>(true);
		SkeletonDataAsset val = (((Object)(object)componentInChildren != (Object)null) ? componentInChildren.skeletonDataAsset : null);
		if (!((Object)(object)val == (Object)null))
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			val.GetSkeletonData(true);
			stopwatch.Stop();
		}
	}

	[AsyncStateMachine(typeof(_003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34))]
	private static UniTaskVoid RunPhotoshopWarmupOnLevelLoadedFireAndForget()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34 _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__ = default(_003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34);
		_003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__._003C_003Et__builder)).Start<_003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__34>(ref _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunPhotoshopWarmupOnLevelLoadedFireAndForget_003Ed__._003C_003Et__builder)).Task;
	}

	private static void RunPhotoshopWarmupFallbackInBackground(GameObject prefab)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)prefab == (Object)null) && !isPhotoshopWarmupRunning)
		{
			UniTaskVoid val = RunPhotoshopWarmupFallbackInBackgroundAsync(prefab);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__36))]
	private static UniTaskVoid RunPhotoshopWarmupFallbackInBackgroundAsync(GameObject prefab)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__36 _003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__ = default(_003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__36);
		_003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__.prefab = prefab;
		_003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__._003C_003Et__builder)).Start<_003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__36>(ref _003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunPhotoshopWarmupFallbackInBackgroundAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static bool ShouldRunPhotoshopWarmup()
	{
		if (photoshopWarmupCompleted)
		{
			return false;
		}
		photoshopWarmupCompleted = PlayerPrefs.GetInt("LevelWinPhotoshopWarmupDone", 0) == 1;
		return !photoshopWarmupCompleted;
	}

	private static void MarkPhotoshopWarmupCompleted()
	{
		photoshopWarmupCompleted = true;
		PlayerPrefs.SetInt("LevelWinPhotoshopWarmupDone", 1);
		PlayerPrefs.Save();
	}

	[AsyncStateMachine(typeof(_003CPreloadPrefabAsync_003Ed__39))]
	private static UniTask PreloadPrefabAsync(string location, CancellationToken cancellationToken, string group = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPreloadPrefabAsync_003Ed__39 _003CPreloadPrefabAsync_003Ed__ = default(_003CPreloadPrefabAsync_003Ed__39);
		_003CPreloadPrefabAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPreloadPrefabAsync_003Ed__.location = location;
		_003CPreloadPrefabAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPreloadPrefabAsync_003Ed__.group = group;
		_003CPreloadPrefabAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPreloadPrefabAsync_003Ed__._003C_003Et__builder)).Start<_003CPreloadPrefabAsync_003Ed__39>(ref _003CPreloadPrefabAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPreloadPrefabAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPreloadAudioClipAsync_003Ed__40))]
	private static UniTask PreloadAudioClipAsync(string clipLocation, CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPreloadAudioClipAsync_003Ed__40 _003CPreloadAudioClipAsync_003Ed__ = default(_003CPreloadAudioClipAsync_003Ed__40);
		_003CPreloadAudioClipAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPreloadAudioClipAsync_003Ed__.clipLocation = clipLocation;
		_003CPreloadAudioClipAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPreloadAudioClipAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPreloadAudioClipAsync_003Ed__._003C_003Et__builder)).Start<_003CPreloadAudioClipAsync_003Ed__40>(ref _003CPreloadAudioClipAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPreloadAudioClipAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPreloadVoClipsAsync_003Ed__41))]
	private static UniTask PreloadVoClipsAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CPreloadVoClipsAsync_003Ed__41 _003CPreloadVoClipsAsync_003Ed__ = default(_003CPreloadVoClipsAsync_003Ed__41);
		_003CPreloadVoClipsAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPreloadVoClipsAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPreloadVoClipsAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPreloadVoClipsAsync_003Ed__._003C_003Et__builder)).Start<_003CPreloadVoClipsAsync_003Ed__41>(ref _003CPreloadVoClipsAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPreloadVoClipsAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CPreloadLevelWinPrefabAsync_003Ed__42))]
	private static UniTask<GameObject> PreloadLevelWinPrefabAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CPreloadLevelWinPrefabAsync_003Ed__42 _003CPreloadLevelWinPrefabAsync_003Ed__ = default(_003CPreloadLevelWinPrefabAsync_003Ed__42);
		_003CPreloadLevelWinPrefabAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<GameObject>.Create();
		_003CPreloadLevelWinPrefabAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPreloadLevelWinPrefabAsync_003Ed__._003C_003E1__state = -1;
		_003CPreloadLevelWinPrefabAsync_003Ed__._003C_003Et__builder.Start<_003CPreloadLevelWinPrefabAsync_003Ed__42>(ref _003CPreloadLevelWinPrefabAsync_003Ed__);
		return _003CPreloadLevelWinPrefabAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CCaptureLevelWinPrefabAsync_003Ed__43))]
	private static UniTask CaptureLevelWinPrefabAsync(CancellationToken cancellationToken, Action<GameObject> onLoaded)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CCaptureLevelWinPrefabAsync_003Ed__43 _003CCaptureLevelWinPrefabAsync_003Ed__ = default(_003CCaptureLevelWinPrefabAsync_003Ed__43);
		_003CCaptureLevelWinPrefabAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CCaptureLevelWinPrefabAsync_003Ed__.cancellationToken = cancellationToken;
		_003CCaptureLevelWinPrefabAsync_003Ed__.onLoaded = onLoaded;
		_003CCaptureLevelWinPrefabAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CCaptureLevelWinPrefabAsync_003Ed__._003C_003Et__builder)).Start<_003CCaptureLevelWinPrefabAsync_003Ed__43>(ref _003CCaptureLevelWinPrefabAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CCaptureLevelWinPrefabAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CWarmupLevelWinPhotoshopAsync_003Ed__44))]
	private static UniTask WarmupLevelWinPhotoshopAsync(GameObject prefab, CancellationToken cancellationToken, string warmupSource = "离屏")
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CWarmupLevelWinPhotoshopAsync_003Ed__44 _003CWarmupLevelWinPhotoshopAsync_003Ed__ = default(_003CWarmupLevelWinPhotoshopAsync_003Ed__44);
		_003CWarmupLevelWinPhotoshopAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWarmupLevelWinPhotoshopAsync_003Ed__.prefab = prefab;
		_003CWarmupLevelWinPhotoshopAsync_003Ed__.cancellationToken = cancellationToken;
		_003CWarmupLevelWinPhotoshopAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWarmupLevelWinPhotoshopAsync_003Ed__._003C_003Et__builder)).Start<_003CWarmupLevelWinPhotoshopAsync_003Ed__44>(ref _003CWarmupLevelWinPhotoshopAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWarmupLevelWinPhotoshopAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static void ActivateWarmupHierarchy(GameObject instance)
	{
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		Transform[] componentsInChildren = instance.GetComponentsInChildren<Transform>(true);
		foreach (Transform val in componentsInChildren)
		{
			if (((Object)val).name == "Root" || ((Object)val).name == "Trans_TopBar")
			{
				((Component)val).gameObject.SetActive(true);
			}
		}
	}

	private static void WarmupEvaluationTextOnInstance(GameObject instance)
	{
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		TMP_Text[] componentsInChildren = instance.GetComponentsInChildren<TMP_Text>(true);
		foreach (TMP_Text val in componentsInChildren)
		{
			if (!((Object)(object)val == (Object)null) && !(((Object)((Component)val).gameObject).name != "TxtM_Evaluation"))
			{
				TMPTextExtensions.SetLocalizedTextValue(val, "Well Done!");
				break;
			}
		}
	}
}
