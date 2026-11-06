using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.WeakGuide.Data;
using RotateRings.WeakGuide.StepHandles;
using UnityEngine;

namespace RotateRings.WeakGuide.Setup;

public class GenericWeakGuideSetup : IWeakGuideSetup
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass4_0
	{
		public WeakGuideManager manager;

		public Action _003C_003E9__0;

		internal void _003CInstallAsync_003Eb__0()
		{
			RefreshGuideRotations(manager);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass9_0
	{
		[StructLayout(LayoutKind.Auto)]
		private struct _003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder<SpinePlayer> _003C_003Et__builder;

			public _003C_003Ec__DisplayClass9_0 _003C_003E4__this;

			public CancellationToken ct;

			private Awaiter<SpinePlayer> _003C_003Eu__1;

			private void MoveNext()
			{
				//IL_0078: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0085: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Unknown result type (might be due to invalid IL or missing references)
				int num = _003C_003E1__state;
				_003C_003Ec__DisplayClass9_0 _003C_003Ec__DisplayClass9_ = _003C_003E4__this;
				SpinePlayer result;
				try
				{
					Awaiter<SpinePlayer> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<SpinePlayer>);
						num = (_003C_003E1__state = -1);
						goto IL_0094;
					}
					if (!((Object)(object)_003C_003Ec__DisplayClass9_.cached != (Object)null))
					{
						val = RebuildHandSpinePlayerAsync(_003C_003Ec__DisplayClass9_.manager, _003C_003Ec__DisplayClass9_.globalConfig, ct).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SpinePlayer>, _003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed>(ref val, ref this);
							return;
						}
						goto IL_0094;
					}
					result = _003C_003Ec__DisplayClass9_.cached;
					goto end_IL_000e;
					IL_0094:
					SpinePlayer result2 = val.GetResult();
					_003C_003Ec__DisplayClass9_.cached = result2;
					result = _003C_003Ec__DisplayClass9_.cached;
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

		public SpinePlayer cached;

		public WeakGuideManager manager;

		public WeakGuideGlobalConfig globalConfig;

		[AsyncStateMachine(typeof(_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed))]
		internal UniTask<SpinePlayer> _003CCreateHandSpinePlayerProvider_003Eb__0(CancellationToken ct)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed _003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed = default(_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed);
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003Et__builder = AsyncUniTaskMethodBuilder<SpinePlayer>.Create();
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003E4__this = this;
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed.ct = ct;
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003E1__state = -1;
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003Et__builder.Start<_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed>(ref _003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed);
			return _003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003Et__builder.Task;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CInstallAsync_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public IWeakGuideInstallContext context;

		public GenericWeakGuideSetup _003C_003E4__this;

		public CancellationToken ct;

		private _003C_003Ec__DisplayClass4_0 _003C_003E8__1;

		private string _003CresGroup_003E5__2;

		private List<GuideStepHandleData> _003CstepHandleDataList_003E5__3;

		private List<GuideStep> _003Csteps_003E5__4;

		private WeakGuideBuildContext _003CbuildCtx_003E5__5;

		private Awaiter<GameObject> _003C_003Eu__1;

		private Awaiter<Material> _003C_003Eu__2;

		private int _003Ci_003E5__6;

		private GuideStepHandleData _003CstepData_003E5__7;

		private IWeakGuideStepHandle _003Chandle_003E5__8;

		private Awaiter<GuideStep> _003C_003Eu__3;

		private void MoveNext()
		{
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_03af: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GenericWeakGuideSetup genericWeakGuideSetup = _003C_003E4__this;
			try
			{
				Awaiter<GameObject> val2;
				Awaiter<Material> val;
				GameObject result;
				Material result3;
				switch (num)
				{
				default:
					_003C_003E8__1 = new _003C_003Ec__DisplayClass4_0();
					if (context != null && !((Object)(object)context.LevelRoot == (Object)null) && !((Object)(object)context.LevelInfo == (Object)null) && genericWeakGuideSetup.entry != null && !((Object)(object)genericWeakGuideSetup.globalConfig == (Object)null))
					{
						string mainPrefabPath = genericWeakGuideSetup.globalConfig.MainPrefabPath;
						_003CresGroup_003E5__2 = genericWeakGuideSetup.globalConfig.ResGroup;
						val2 = LoadPrefabAsync(mainPrefabPath, _003CresGroup_003E5__2, ct).GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CInstallAsync_003Ed__4>(ref val2, ref this);
							return;
						}
						goto IL_0100;
					}
					goto end_IL_000e;
				case 0:
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
					goto IL_0100;
				case 1:
					val = _003C_003Eu__2;
					_003C_003Eu__2 = default(Awaiter<Material>);
					num = (_003C_003E1__state = -1);
					goto IL_0242;
				case 2:
					{
						GuideStep result2;
						try
						{
							Awaiter<GuideStep> val4;
							if (num != 2)
							{
								val4 = _003Chandle_003E5__8.BuildGuideStepAsync(_003CbuildCtx_003E5__5, _003CstepData_003E5__7.ParamsJson).GetAwaiter();
								if (!val4.IsCompleted)
								{
									num = (_003C_003E1__state = 2);
									_003C_003Eu__3 = val4;
									((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GuideStep>, _003CInstallAsync_003Ed__4>(ref val4, ref this);
									return;
								}
							}
							else
							{
								val4 = _003C_003Eu__3;
								_003C_003Eu__3 = default(Awaiter<GuideStep>);
								num = (_003C_003E1__state = -1);
							}
							result2 = val4.GetResult();
						}
						catch (Exception)
						{
							if (!ct.IsCancellationRequested)
							{
								goto IL_05b3;
							}
							goto end_IL_000e;
						}
						if (!ct.IsCancellationRequested && !((Object)(object)_003C_003E8__1.manager == (Object)null) && !((Object)(object)context.LevelRoot == (Object)null) && !((Object)(object)context.GameplayManager == (Object)null) && !((Object)(object)context.LevelRoot != (Object)(object)context.GameplayManager.CurrentLevelObject))
						{
							if (result2 != null)
							{
								if (!string.IsNullOrEmpty(_003CstepData_003E5__7.GuideTextKey))
								{
									result2.GuideTextKey = _003CstepData_003E5__7.GuideTextKey;
								}
								result2.StepName = _003CstepData_003E5__7.StepName;
								result2.ConfigureRuntime(_003Chandle_003E5__8, CreateHandSpinePlayerProvider(_003C_003E8__1.manager, genericWeakGuideSetup.globalConfig), _003CstepData_003E5__7.InitialDelay, _003CstepData_003E5__7.LoopDelay, _003CstepData_003E5__7.AfterOpenDelay, _003CstepData_003E5__7.BeforeCloseDelay, string.IsNullOrEmpty(_003CstepData_003E5__7.OpenAnimation) ? "guide_drag_open" : _003CstepData_003E5__7.OpenAnimation, string.IsNullOrEmpty(_003CstepData_003E5__7.CloseAnimation) ? "guide_drag_close" : _003CstepData_003E5__7.CloseAnimation, delegate
								{
									RefreshGuideRotations(_003C_003E8__1.manager);
								});
								_003Csteps_003E5__4.Add(result2);
								_003CstepData_003E5__7 = null;
								_003Chandle_003E5__8 = null;
							}
							goto IL_05b3;
						}
						goto end_IL_000e;
					}
					IL_0100:
					result = val2.GetResult();
					if (!((Object)(object)result == (Object)null) && !ct.IsCancellationRequested && !((Object)(object)context.LevelRoot == (Object)null))
					{
						GameObject val3 = Object.Instantiate<GameObject>(result, context.LevelRoot.transform);
						((Object)val3).name = ((Object)result).name;
						if (ct.IsCancellationRequested)
						{
							Object.Destroy((Object)(object)val3);
						}
						else
						{
							_003C_003E8__1.manager = val3.GetComponentInChildren<WeakGuideManager>(true);
							if (!((Object)(object)_003C_003E8__1.manager == (Object)null))
							{
								context.LevelInfo.weakGuideManager = _003C_003E8__1.manager;
								string highlightMaterialPath = genericWeakGuideSetup.globalConfig.HighlightMaterialPath;
								if (!string.IsNullOrEmpty(highlightMaterialPath))
								{
									val = LoadMaterialAsync(highlightMaterialPath, _003CresGroup_003E5__2, ct).GetAwaiter();
									if (!val.IsCompleted)
									{
										num = (_003C_003E1__state = 1);
										_003C_003Eu__2 = val;
										((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<Material>, _003CInstallAsync_003Ed__4>(ref val, ref this);
										return;
									}
									goto IL_0242;
								}
								goto IL_02d4;
							}
							Object.Destroy((Object)(object)val3);
						}
					}
					goto end_IL_000e;
					IL_05c5:
					if (_003Ci_003E5__6 >= _003CstepHandleDataList_003E5__3.Count)
					{
						break;
					}
					_003CstepData_003E5__7 = _003CstepHandleDataList_003E5__3[_003Ci_003E5__6];
					if (_003CstepData_003E5__7 == null)
					{
						goto IL_05b3;
					}
					if (!ct.IsCancellationRequested)
					{
						_003Chandle_003E5__8 = WeakGuideStepHandleRegistry.GetByName(_003CstepData_003E5__7.HandleId);
						if (_003Chandle_003E5__8 != null)
						{
							goto case 2;
						}
						goto IL_05b3;
					}
					goto end_IL_000e;
					IL_05b3:
					_003Ci_003E5__6++;
					goto IL_05c5;
					IL_02d4:
					WeakGuideStepHandleRegistry.RegisterDefaults();
					_003CstepHandleDataList_003E5__3 = DeserializeStepHandlesJson(genericWeakGuideSetup.entry.StepHandlesJson);
					_003Csteps_003E5__4 = new List<GuideStep>();
					_003CbuildCtx_003E5__5 = new WeakGuideBuildContext(context, _003C_003E8__1.manager, genericWeakGuideSetup.globalConfig, ct);
					if (_003CstepHandleDataList_003E5__3 == null)
					{
						break;
					}
					_003Ci_003E5__6 = 0;
					goto IL_05c5;
					IL_0242:
					result3 = val.GetResult();
					if (!ct.IsCancellationRequested && !((Object)(object)_003C_003E8__1.manager == (Object)null) && !((Object)(object)context.LevelRoot == (Object)null) && !((Object)(object)context.GameplayManager == (Object)null) && !((Object)(object)context.LevelRoot != (Object)(object)context.GameplayManager.CurrentLevelObject))
					{
						if ((Object)(object)result3 != (Object)null)
						{
							_003C_003E8__1.manager.SetHighlightRingMaterial(result3);
						}
						goto IL_02d4;
					}
					goto end_IL_000e;
				}
				if (!ct.IsCancellationRequested && !((Object)(object)_003C_003E8__1.manager == (Object)null) && !((Object)(object)context.LevelRoot == (Object)null) && !((Object)(object)context.GameplayManager == (Object)null) && !((Object)(object)context.LevelRoot != (Object)(object)context.GameplayManager.CurrentLevelObject))
				{
					_003C_003E8__1.manager.InjectSteps(_003Csteps_003E5__4);
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003E8__1 = null;
				_003CresGroup_003E5__2 = null;
				_003CstepHandleDataList_003E5__3 = null;
				_003Csteps_003E5__4 = null;
				_003CbuildCtx_003E5__5 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003E8__1 = null;
			_003CresGroup_003E5__2 = null;
			_003CstepHandleDataList_003E5__3 = null;
			_003Csteps_003E5__4 = null;
			_003CbuildCtx_003E5__5 = null;
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
	private struct _003CLoadMaterialAsync_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<Material> _003C_003Et__builder;

		public string path;

		public string resGroup;

		private Awaiter<Material> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			Material result;
			try
			{
				if (num != 0 && string.IsNullOrEmpty(path))
				{
					result = null;
				}
				else
				{
					try
					{
						Awaiter<Material> val;
						if (num != 0)
						{
							val = GameApp.Res.LoadAssetAsync<Material>(path, resGroup).GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<Material>, _003CLoadMaterialAsync_003Ed__8>(ref val, ref this);
								return;
							}
						}
						else
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<Material>);
							num = (_003C_003E1__state = -1);
						}
						result = val.GetResult();
					}
					catch (Exception)
					{
						result = null;
					}
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
	private struct _003CLoadPrefabAsync_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<GameObject> _003C_003Et__builder;

		public string path;

		public string resGroup;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameObject result;
			try
			{
				if (num != 0 && string.IsNullOrEmpty(path))
				{
					result = null;
				}
				else
				{
					try
					{
						Awaiter<GameObject> val;
						if (num != 0)
						{
							val = GameApp.Res.LoadAssetAsync<GameObject>(path, resGroup).GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CLoadPrefabAsync_003Ed__7>(ref val, ref this);
								return;
							}
						}
						else
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<GameObject>);
							num = (_003C_003E1__state = -1);
						}
						result = val.GetResult();
					}
					catch (Exception)
					{
						result = null;
					}
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
	private struct _003CRebuildHandSpinePlayerAsync_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<SpinePlayer> _003C_003Et__builder;

		public WeakGuideManager manager;

		public WeakGuideGlobalConfig globalConfig;

		public CancellationToken ct;

		private Awaiter<SpinePlayer> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			SpinePlayer result;
			try
			{
				Awaiter<SpinePlayer> val;
				if (num != 0)
				{
					val = WeakGuideSetupHelpers.RebuildHandSpinePlayerAsyncForGeneric(manager, globalConfig, ct).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SpinePlayer>, _003CRebuildHandSpinePlayerAsync_003Ed__10>(ref val, ref this);
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

	private const string TAG = "[WeakGuide][GenericSetup] ";

	private readonly GuideStepsAsset.Entry entry;

	private readonly WeakGuideGlobalConfig globalConfig;

	public GenericWeakGuideSetup(GuideStepsAsset.Entry entry, WeakGuideGlobalConfig global = null)
	{
		this.entry = entry;
		globalConfig = (((Object)(object)global != (Object)null) ? global : WeakGuideGlobalConfig.Default);
	}

	[AsyncStateMachine(typeof(_003CInstallAsync_003Ed__4))]
	public UniTask InstallAsync(IWeakGuideInstallContext context, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CInstallAsync_003Ed__4 _003CInstallAsync_003Ed__ = default(_003CInstallAsync_003Ed__4);
		_003CInstallAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CInstallAsync_003Ed__._003C_003E4__this = this;
		_003CInstallAsync_003Ed__.context = context;
		_003CInstallAsync_003Ed__.ct = ct;
		_003CInstallAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CInstallAsync_003Ed__._003C_003Et__builder)).Start<_003CInstallAsync_003Ed__4>(ref _003CInstallAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CInstallAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static List<GuideStepHandleData> DeserializeStepHandlesJson(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}
		try
		{
			return GameApp.Json.Deserialize<List<GuideStepHandleData>>(json);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static void PrewarmJsonDeserialization()
	{
		DeserializeStepHandlesJson("[]");
	}

	[AsyncStateMachine(typeof(_003CLoadPrefabAsync_003Ed__7))]
	private static UniTask<GameObject> LoadPrefabAsync(string path, string resGroup, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadPrefabAsync_003Ed__7 _003CLoadPrefabAsync_003Ed__ = default(_003CLoadPrefabAsync_003Ed__7);
		_003CLoadPrefabAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<GameObject>.Create();
		_003CLoadPrefabAsync_003Ed__.path = path;
		_003CLoadPrefabAsync_003Ed__.resGroup = resGroup;
		_003CLoadPrefabAsync_003Ed__._003C_003E1__state = -1;
		_003CLoadPrefabAsync_003Ed__._003C_003Et__builder.Start<_003CLoadPrefabAsync_003Ed__7>(ref _003CLoadPrefabAsync_003Ed__);
		return _003CLoadPrefabAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CLoadMaterialAsync_003Ed__8))]
	private static UniTask<Material> LoadMaterialAsync(string path, string resGroup, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadMaterialAsync_003Ed__8 _003CLoadMaterialAsync_003Ed__ = default(_003CLoadMaterialAsync_003Ed__8);
		_003CLoadMaterialAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<Material>.Create();
		_003CLoadMaterialAsync_003Ed__.path = path;
		_003CLoadMaterialAsync_003Ed__.resGroup = resGroup;
		_003CLoadMaterialAsync_003Ed__._003C_003E1__state = -1;
		_003CLoadMaterialAsync_003Ed__._003C_003Et__builder.Start<_003CLoadMaterialAsync_003Ed__8>(ref _003CLoadMaterialAsync_003Ed__);
		return _003CLoadMaterialAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static Func<CancellationToken, UniTask<SpinePlayer>> CreateHandSpinePlayerProvider(WeakGuideManager manager, WeakGuideGlobalConfig globalConfig)
	{
		_003C_003Ec__DisplayClass9_0 _003C_003Ec__DisplayClass9_ = new _003C_003Ec__DisplayClass9_0();
		_003C_003Ec__DisplayClass9_.manager = manager;
		_003C_003Ec__DisplayClass9_.globalConfig = globalConfig;
		_003C_003Ec__DisplayClass9_.cached = null;
		return [AsyncStateMachine(typeof(_003C_003Ec__DisplayClass9_0._003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed))] (CancellationToken ct) =>
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			_003C_003Ec__DisplayClass9_0._003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed _003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed = default(_003C_003Ec__DisplayClass9_0._003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed);
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003Et__builder = AsyncUniTaskMethodBuilder<SpinePlayer>.Create();
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003E4__this = _003C_003Ec__DisplayClass9_;
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed.ct = ct;
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003E1__state = -1;
			_003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003Et__builder.Start<_003C_003Ec__DisplayClass9_0._003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed>(ref _003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed);
			return _003C_003CCreateHandSpinePlayerProvider_003Eb__0_003Ed._003C_003Et__builder.Task;
		};
	}

	[AsyncStateMachine(typeof(_003CRebuildHandSpinePlayerAsync_003Ed__10))]
	private static UniTask<SpinePlayer> RebuildHandSpinePlayerAsync(WeakGuideManager manager, WeakGuideGlobalConfig globalConfig, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRebuildHandSpinePlayerAsync_003Ed__10 _003CRebuildHandSpinePlayerAsync_003Ed__ = default(_003CRebuildHandSpinePlayerAsync_003Ed__10);
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<SpinePlayer>.Create();
		_003CRebuildHandSpinePlayerAsync_003Ed__.manager = manager;
		_003CRebuildHandSpinePlayerAsync_003Ed__.globalConfig = globalConfig;
		_003CRebuildHandSpinePlayerAsync_003Ed__.ct = ct;
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003E1__state = -1;
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder.Start<_003CRebuildHandSpinePlayerAsync_003Ed__10>(ref _003CRebuildHandSpinePlayerAsync_003Ed__);
		return _003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static void RefreshGuideRotations(WeakGuideManager manager)
	{
		WeakGuideSetupHelpers.RefreshGuideRotations(manager);
	}
}
