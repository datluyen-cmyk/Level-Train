using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.UI.WeakGuide;
using RotateRings.WeakGuide.Setup;
using UnityEngine;
using XingYun.RingGame;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.WeakGuide;

public class WeakGuideSetupHelpers
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CEnsureHandInWeakGuideViewAsync_003Ed__30 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CircularMotion> _003C_003Et__builder;

		public WeakGuideManager manager;

		public WeakGuideGlobalConfig globalConfig;

		public CancellationToken ct;

		private Transform _003CworldHand_003E5__2;

		private CircularMotion _003ChandMotion_003E5__3;

		private GameObject _003CweakGuideViewRoot_003E5__4;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CircularMotion result;
			try
			{
				Awaiter<GameObject> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
					goto IL_0164;
				}
				_003CworldHand_003E5__2 = FindWorldSpaceHandNode(manager, globalConfig);
				if ((Object)(object)_003CworldHand_003E5__2 == (Object)null)
				{
					result = null;
				}
				else
				{
					_003ChandMotion_003E5__3 = ((Component)_003CworldHand_003E5__2).GetComponent<CircularMotion>();
					if ((Object)(object)_003ChandMotion_003E5__3 == (Object)null)
					{
						result = null;
					}
					else
					{
						_003ChandMotion_003E5__3.SetMotionMode(CircularMotionMode.Orbit);
						if (!((Component)_003CworldHand_003E5__2).gameObject.activeSelf)
						{
							((Component)_003CworldHand_003E5__2).gameObject.SetActive(true);
						}
						if (ct.IsCancellationRequested)
						{
							result = null;
						}
						else
						{
							if (!GameApp.UI.IsOpen("WeakGuideView"))
							{
								GameApp.UI.Open("WeakGuideView", (UILayer)2, (object)null, (Action)null);
							}
							if (ct.IsCancellationRequested)
							{
								result = null;
							}
							else
							{
								_003CweakGuideViewRoot_003E5__4 = TryGetWeakGuideViewRoot();
								if (!((Object)(object)_003CweakGuideViewRoot_003E5__4 == (Object)null))
								{
									val = LoadHandPrefabAsync(globalConfig, ct).GetAwaiter();
									if (!val.IsCompleted)
									{
										num = (_003C_003E1__state = 0);
										_003C_003Eu__1 = val;
										_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CEnsureHandInWeakGuideViewAsync_003Ed__30>(ref val, ref this);
										return;
									}
									goto IL_0164;
								}
								result = _003ChandMotion_003E5__3;
							}
						}
					}
				}
				goto end_IL_0007;
				IL_0164:
				GameObject result2 = val.GetResult();
				if (ct.IsCancellationRequested || (Object)(object)manager == (Object)null || (Object)(object)_003CworldHand_003E5__2 == (Object)null || (Object)(object)_003ChandMotion_003E5__3 == (Object)null)
				{
					result = null;
				}
				else if ((Object)(object)result2 == (Object)null)
				{
					result = _003ChandMotion_003E5__3;
				}
				else if ((Object)(object)_003CweakGuideViewRoot_003E5__4 == (Object)null)
				{
					result = _003ChandMotion_003E5__3;
				}
				else
				{
					string name = (((Object)(object)globalConfig != (Object)null && !string.IsNullOrEmpty(globalConfig.HandInstanceName)) ? globalConfig.HandInstanceName : "GuideHand1");
					GameObject val2 = Object.Instantiate<GameObject>(result2, _003CweakGuideViewRoot_003E5__4.transform, false);
					((Object)val2).name = name;
					val2.transform.localScale = Vector3.one;
					val2.transform.localRotation = Quaternion.identity;
					Transform transform = val2.transform;
					RectTransform val3 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
					if ((Object)(object)val3 == (Object)null)
					{
						Object.Destroy((Object)(object)val2);
						result = _003ChandMotion_003E5__3;
					}
					else
					{
						HandWorldToUIFollower handWorldToUIFollower = val2.GetComponent<HandWorldToUIFollower>();
						if ((Object)(object)handWorldToUIFollower == (Object)null)
						{
							handWorldToUIFollower = val2.AddComponent<HandWorldToUIFollower>();
						}
						handWorldToUIFollower.Bind(_003CworldHand_003E5__2, val3);
						Transform val4 = FindHandMotionTransform(val2, globalConfig);
						if ((Object)(object)val4 != (Object)null && (Object)(object)((Component)val4).GetComponentInChildren<SpinePlayer>(true) == (Object)null)
						{
							((Component)val4).gameObject.AddComponent<SpinePlayer>();
						}
						result = _003ChandMotion_003E5__3;
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CworldHand_003E5__2 = null;
				_003ChandMotion_003E5__3 = null;
				_003CweakGuideViewRoot_003E5__4 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CworldHand_003E5__2 = null;
			_003ChandMotion_003E5__3 = null;
			_003CweakGuideViewRoot_003E5__4 = null;
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
	private struct _003CLoadHandPrefabAsync_003Ed__34 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<GameObject> _003C_003Et__builder;

		public WeakGuideGlobalConfig globalConfig;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameObject result;
			try
			{
				Awaiter<GameObject> val;
				if (num != 0)
				{
					string text = (((Object)(object)globalConfig != (Object)null && !string.IsNullOrEmpty(globalConfig.HandPrefabPath)) ? globalConfig.HandPrefabPath : "Assets/02_Res/Art/Effect/Prefab/eff_rr_gp1/eff_guide_hand.prefab");
					string text2 = (((Object)(object)globalConfig != (Object)null && !string.IsNullOrEmpty(globalConfig.ResGroup)) ? globalConfig.ResGroup : "LevelSceneGroup");
					val = GameApp.Res.LoadAssetAsync<GameObject>(text, text2).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CLoadHandPrefabAsync_003Ed__34>(ref val, ref this);
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
	private struct _003CLoadWeakGuidePrefabAsync_003Ed__28 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<GameObject> _003C_003Et__builder;

		public WeakGuideGlobalConfig globalConfig;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameObject result;
			try
			{
				Awaiter<GameObject> val;
				if (num != 0)
				{
					string text = (((Object)(object)globalConfig != (Object)null && !string.IsNullOrEmpty(globalConfig.MainPrefabPath)) ? globalConfig.MainPrefabPath : "Assets/02_Res/Art/Level/LevelPrefab/WeakGuidManager.prefab");
					string text2 = (((Object)(object)globalConfig != (Object)null && !string.IsNullOrEmpty(globalConfig.ResGroup)) ? globalConfig.ResGroup : "LevelSceneGroup");
					val = GameApp.Res.LoadAssetAsync<GameObject>(text, text2).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CLoadWeakGuidePrefabAsync_003Ed__28>(ref val, ref this);
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
	private struct _003CRebuildHandSpinePlayerAsync_003Ed__31 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<SpinePlayer> _003C_003Et__builder;

		public WeakGuideManager manager;

		public CancellationToken ct;

		private Awaiter<SpinePlayer> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			SpinePlayer result;
			try
			{
				Awaiter<SpinePlayer> val;
				if (num != 0)
				{
					val = RebuildHandSpinePlayerAsync(manager, null, ct).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SpinePlayer>, _003CRebuildHandSpinePlayerAsync_003Ed__31>(ref val, ref this);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRebuildHandSpinePlayerAsync_003Ed__32 : IAsyncStateMachine
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
					val = RebuildHandSpinePlayerAsyncForGeneric(manager, globalConfig, ct).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<SpinePlayer>, _003CRebuildHandSpinePlayerAsync_003Ed__32>(ref val, ref this);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__33 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<SpinePlayer> _003C_003Et__builder;

		public WeakGuideManager manager;

		public CancellationToken ct;

		public WeakGuideGlobalConfig globalConfig;

		private GameObject _003CweakGuideViewRoot_003E5__2;

		private string _003ChandInstanceName_003E5__3;

		private Transform _003CworldHand_003E5__4;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			SpinePlayer result;
			try
			{
				Awaiter<GameObject> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
					goto IL_01ac;
				}
				if ((Object)(object)manager == (Object)null || ct.IsCancellationRequested)
				{
					result = null;
				}
				else
				{
					if (!GameApp.UI.IsOpen("WeakGuideView"))
					{
						GameApp.UI.Open("WeakGuideView", (UILayer)2, (object)null, (Action)null);
					}
					if (ct.IsCancellationRequested)
					{
						result = null;
					}
					else
					{
						_003CweakGuideViewRoot_003E5__2 = TryGetWeakGuideViewRoot();
						if ((Object)(object)_003CweakGuideViewRoot_003E5__2 == (Object)null)
						{
							result = null;
						}
						else
						{
							_003ChandInstanceName_003E5__3 = (((Object)(object)globalConfig != (Object)null && !string.IsNullOrEmpty(globalConfig.HandInstanceName)) ? globalConfig.HandInstanceName : "GuideHand1");
							Transform val2 = _003CweakGuideViewRoot_003E5__2.transform.Find(_003ChandInstanceName_003E5__3);
							if (!((Object)(object)val2 != (Object)null))
							{
								goto IL_011a;
							}
							Transform val3 = FindHandMotionTransform(((Component)val2).gameObject, globalConfig);
							SpinePlayer spinePlayer = (((Object)(object)val3 != (Object)null) ? ((Component)val3).GetComponentInChildren<SpinePlayer>(true) : null);
							if (!((Object)(object)spinePlayer != (Object)null))
							{
								goto IL_011a;
							}
							result = spinePlayer;
						}
					}
				}
				goto end_IL_0007;
				IL_011a:
				_003CworldHand_003E5__4 = FindWorldSpaceHandNode(manager, globalConfig);
				if (!((Object)(object)_003CworldHand_003E5__4 == (Object)null))
				{
					val = LoadHandPrefabAsync(globalConfig, ct).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__33>(ref val, ref this);
						return;
					}
					goto IL_01ac;
				}
				result = null;
				goto end_IL_0007;
				IL_01ac:
				GameObject result2 = val.GetResult();
				if (ct.IsCancellationRequested || (Object)(object)result2 == (Object)null || (Object)(object)manager == (Object)null || (Object)(object)_003CworldHand_003E5__4 == (Object)null)
				{
					result = null;
				}
				else if ((Object)(object)_003CweakGuideViewRoot_003E5__2 == (Object)null)
				{
					result = null;
				}
				else
				{
					GameObject val4 = Object.Instantiate<GameObject>(result2, _003CweakGuideViewRoot_003E5__2.transform, false);
					((Object)val4).name = _003ChandInstanceName_003E5__3;
					val4.transform.localScale = Vector3.one;
					val4.transform.localRotation = Quaternion.identity;
					Transform transform = val4.transform;
					RectTransform val5 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
					if ((Object)(object)val5 == (Object)null)
					{
						Object.Destroy((Object)(object)val4);
						result = null;
					}
					else
					{
						HandWorldToUIFollower handWorldToUIFollower = val4.GetComponent<HandWorldToUIFollower>();
						if ((Object)(object)handWorldToUIFollower == (Object)null)
						{
							handWorldToUIFollower = val4.AddComponent<HandWorldToUIFollower>();
						}
						handWorldToUIFollower.Bind(_003CworldHand_003E5__4, val5);
						Transform val6 = FindHandMotionTransform(val4, globalConfig);
						SpinePlayer spinePlayer2 = (((Object)(object)val6 != (Object)null) ? ((Component)val6).GetComponentInChildren<SpinePlayer>(true) : null);
						if ((Object)(object)spinePlayer2 == (Object)null && (Object)(object)val6 != (Object)null)
						{
							spinePlayer2 = ((Component)val6).gameObject.AddComponent<SpinePlayer>();
						}
						result = spinePlayer2;
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CweakGuideViewRoot_003E5__2 = null;
				_003ChandInstanceName_003E5__3 = null;
				_003CworldHand_003E5__4 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CweakGuideViewRoot_003E5__2 = null;
			_003ChandInstanceName_003E5__3 = null;
			_003CworldHand_003E5__4 = null;
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

	internal const string FirstLevelTargetRingPhysicPartId = "ring_2";

	internal const string FirstLevelRing1AdjustmentPhysicPartId = "ring_1";

	internal const string FirstLevelWeakGuidePrefabPath = "Assets/02_Res/Art/Level/LevelPrefab/WeakGuidManager.prefab";

	internal const string FirstLevelWeakGuideResGroup = "LevelSceneGroup";

	internal const string EffGuideHandPrefabPath = "Assets/02_Res/Art/Effect/Prefab/eff_rr_gp1/eff_guide_hand.prefab";

	internal const string HandInstanceName = "GuideHand1";

	internal const string HandMotionTransformName = "hand";

	internal const string WorldSpaceHandNodeName = "GuidHand1";

	private const string TAG = "[WeakGuide][RuntimeHelpers] ";

	public const float HighlightCloneRingZ = 0.01f;

	public const float HighlightCloneStemZ = 0.015f;

	public const float HighlightCloneCrossBarZ = -0.02f;

	public static void RefreshGuideRotations(WeakGuideManager manager)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)manager == (Object)null)
		{
			return;
		}
		CircularMotion[] componentsInChildren = ((Component)manager).GetComponentsInChildren<CircularMotion>(true);
		CircularMotion circularMotion = null;
		CircularMotion circularMotion2 = null;
		foreach (CircularMotion circularMotion3 in componentsInChildren)
		{
			if (!((Object)(object)circularMotion3 == (Object)null))
			{
				if (circularMotion3.MotionMode == CircularMotionMode.Orbit)
				{
					circularMotion = circularMotion3;
				}
				else
				{
					circularMotion2 = circularMotion3;
				}
			}
		}
		if ((Object)(object)circularMotion == (Object)null || (Object)(object)circularMotion.Center == (Object)null)
		{
			return;
		}
		float z = circularMotion.Center.localEulerAngles.z;
		float originalTotalRotation = circularMotion.OriginalTotalRotation;
		if (originalTotalRotation <= 0.01f)
		{
			return;
		}
		float num = Mathf.Repeat(z - originalTotalRotation + 360f, 360f);
		if (!(num < 0.01f))
		{
			float rotationDuration = circularMotion.RotationDuration;
			float newRotationDuration = (((Object)(object)circularMotion2 != (Object)null) ? circularMotion2.RotationDuration : rotationDuration);
			circularMotion.ConfigureMotion(RotationDirection.Clockwise, num, rotationDuration, 0f);
			if ((Object)(object)circularMotion2 != (Object)null)
			{
				circularMotion2.SetInitialLocalRotationZ(z);
				circularMotion2.ConfigureMotion(RotationDirection.Clockwise, num, newRotationDuration, 0f);
			}
		}
	}

	public static void ResetRingZ(GameplayManager gameplayManager, string physicPartId)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(physicPartId))
		{
			return;
		}
		GameObject val = FindRingByPhysicPartId(gameplayManager, physicPartId);
		if (!((Object)(object)val == (Object)null))
		{
			Vector3 localEulerAngles = val.transform.localEulerAngles;
			float num = localEulerAngles.z;
			if (num > 180f)
			{
				num -= 360f;
			}
			if (!(Mathf.Abs(num) < 0.01f))
			{
				Vector3 localEulerAngles2 = localEulerAngles;
				localEulerAngles2.z = 0f;
				val.transform.localEulerAngles = localEulerAngles2;
			}
		}
	}

	public static void WireAllCircularMotionsCenter(GameObject root, Transform ringCenter)
	{
		if ((Object)(object)root == (Object)null || (Object)(object)ringCenter == (Object)null)
		{
			return;
		}
		CircularMotion[] componentsInChildren = root.GetComponentsInChildren<CircularMotion>(true);
		foreach (CircularMotion circularMotion in componentsInChildren)
		{
			if ((Object)(object)circularMotion != (Object)null)
			{
				circularMotion.SetCenter(ringCenter);
			}
		}
	}

	public static GameObject FindRingByPhysicPartId(GameplayManager gameplayManager, string physicPartId)
	{
		if ((Object)(object)gameplayManager == (Object)null || string.IsNullOrEmpty(physicPartId))
		{
			return null;
		}
		IReadOnlyList<IGameplayObject> gameplayObjects = gameplayManager.GetGameplayObjects();
		if (gameplayObjects == null)
		{
			return null;
		}
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			if (gameplayObjects[i] is RingObject ringObject && (Object)(object)ringObject != (Object)null && ringObject.PartData.PhysicPartId == physicPartId)
			{
				return ((Component)ringObject).gameObject;
			}
		}
		return null;
	}

	private static void ApplyWeakGuideRingMaterial(GameObject clone, Material material)
	{
		if ((Object)(object)clone == (Object)null || (Object)(object)material == (Object)null)
		{
			return;
		}
		RingSegmentController[] componentsInChildren = clone.GetComponentsInChildren<RingSegmentController>(true);
		foreach (RingSegmentController ringSegmentController in componentsInChildren)
		{
			if (!((Object)(object)ringSegmentController == (Object)null))
			{
				ringSegmentController.BindSharedMaterial(material);
			}
		}
	}

	public static void AdjustRingZ(GameplayManager gameplayManager, string physicPartId, float crossBarZ)
	{
		if (string.IsNullOrEmpty(physicPartId))
		{
			return;
		}
		GameObject val = FindRingByPhysicPartId(gameplayManager, physicPartId);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		RingSegmentController[] componentsInChildren = val.GetComponentsInChildren<RingSegmentController>(true);
		foreach (RingSegmentController ringSegmentController in componentsInChildren)
		{
			if ((Object)(object)ringSegmentController != (Object)null)
			{
				ringSegmentController.SetCrossBarZ(crossBarZ);
			}
		}
	}

	public static void AdjustRingWholeZ(GameplayManager gameplayManager, string physicPartId, float wholeZDelta)
	{
		if (string.IsNullOrEmpty(physicPartId))
		{
			return;
		}
		GameObject val = FindRingByPhysicPartId(gameplayManager, physicPartId);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		RingSegmentController[] componentsInChildren = val.GetComponentsInChildren<RingSegmentController>(true);
		foreach (RingSegmentController ringSegmentController in componentsInChildren)
		{
			if (!((Object)(object)ringSegmentController == (Object)null))
			{
				ringSegmentController.GetZLayerOffsets(out var ringZ, out var stemZ, out var crossBarZ);
				ringSegmentController.SetZLayerOffsets(ringZ + wholeZDelta, stemZ + wholeZDelta, crossBarZ);
			}
		}
	}

	public static void ComputeHighlightRingZOffsets(GameObject source, out float ringZ, out float stemZ, out float crossBarZ)
	{
		ringZ = 0.01f;
		stemZ = 0.015f;
		crossBarZ = -0.02f;
	}

	public static CircularMotion InjectHighlightRingClone(WeakGuideManager manager, GameplayManager gameplayManager, CircularMotion handMotion)
	{
		return InjectHighlightRingClone(manager, gameplayManager, handMotion, "ring_2");
	}

	public static CircularMotion InjectHighlightRingClone(WeakGuideManager manager, GameplayManager gameplayManager, CircularMotion handMotion, string sourcePhysicPartId)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)manager == (Object)null || (Object)(object)gameplayManager == (Object)null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(sourcePhysicPartId))
		{
			sourcePhysicPartId = "ring_2";
		}
		GameObject val = FindRingByPhysicPartId(gameplayManager, sourcePhysicPartId);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val, ((Component)manager).transform, true);
		((Object)val2).name = ((Object)val).name + "_WeakGuideHighlight";
		Vector3 position = val2.transform.position;
		position.z = -0.015f;
		val2.transform.position = position;
		manager.ApplyHighlightRingCloneScale(val2, val);
		Material highlightRingMaterial = manager.HighlightRingMaterial;
		if ((Object)(object)highlightRingMaterial != (Object)null)
		{
			ApplyWeakGuideRingMaterial(val2, highlightRingMaterial);
		}
		RingSegmentController[] componentsInChildren = val2.GetComponentsInChildren<RingSegmentController>(true);
		ComputeHighlightRingZOffsets(val, out var ringZ, out var stemZ, out var crossBarZ);
		foreach (RingSegmentController ringSegmentController in componentsInChildren)
		{
			if ((Object)(object)ringSegmentController != (Object)null)
			{
				ringSegmentController.SetZLayerOffsets(ringZ, stemZ, crossBarZ);
			}
		}
		CircularMotion result = AddCloneCircularMotion(val2, handMotion);
		DisableRingSegmentControllers(val2);
		StripNonVisualComponents(val2);
		return result;
	}

	private static void StripNonVisualComponents(GameObject clone)
	{
		Collider2D[] componentsInChildren = clone.GetComponentsInChildren<Collider2D>(true);
		int num = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)componentsInChildren[i] != (Object)null)
			{
				num++;
				Object.Destroy((Object)(object)componentsInChildren[i]);
			}
		}
		Collider[] componentsInChildren2 = clone.GetComponentsInChildren<Collider>(true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if ((Object)(object)componentsInChildren2[j] != (Object)null)
			{
				Object.Destroy((Object)(object)componentsInChildren2[j]);
			}
		}
		Rigidbody2D[] componentsInChildren3 = clone.GetComponentsInChildren<Rigidbody2D>(true);
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			if ((Object)(object)componentsInChildren3[k] != (Object)null)
			{
				Object.Destroy((Object)(object)componentsInChildren3[k]);
			}
		}
		Rigidbody[] componentsInChildren4 = clone.GetComponentsInChildren<Rigidbody>(true);
		for (int l = 0; l < componentsInChildren4.Length; l++)
		{
			if ((Object)(object)componentsInChildren4[l] != (Object)null)
			{
				Object.Destroy((Object)(object)componentsInChildren4[l]);
			}
		}
		RingObject[] componentsInChildren5 = clone.GetComponentsInChildren<RingObject>(true);
		for (int m = 0; m < componentsInChildren5.Length; m++)
		{
			if ((Object)(object)componentsInChildren5[m] != (Object)null)
			{
				Object.Destroy((Object)(object)componentsInChildren5[m]);
			}
		}
	}

	private static void DisableRingSegmentControllers(GameObject clone)
	{
		RingSegmentController[] componentsInChildren = clone.GetComponentsInChildren<RingSegmentController>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((Object)(object)componentsInChildren[i] != (Object)null)
			{
				((Behaviour)componentsInChildren[i]).enabled = false;
			}
		}
	}

	private static CircularMotion AddCloneCircularMotion(GameObject clone, CircularMotion handMotion)
	{
		if ((Object)(object)clone == (Object)null)
		{
			return null;
		}
		CircularMotion circularMotion = clone.GetComponent<CircularMotion>();
		if ((Object)(object)circularMotion == (Object)null)
		{
			circularMotion = clone.AddComponent<CircularMotion>();
		}
		if ((Object)(object)circularMotion == (Object)null)
		{
			return circularMotion;
		}
		circularMotion.SetMotionMode(CircularMotionMode.SelfRotate);
		if ((Object)(object)handMotion == (Object)null)
		{
			return circularMotion;
		}
		circularMotion.ConfigureMotion(handMotion.Direction, handMotion.TotalRotation, handMotion.RotationDuration, handMotion.PauseDuration);
		return circularMotion;
	}

	public static WeakGuideFadeTarget[] BuildFadeTargetsForDeserializers(MeshRenderer[] renderers, string[] colorProperties)
	{
		return BuildFadeTargetsInternal(renderers, colorProperties);
	}

	public static WeakGuideFadeTarget[] BuildFadeTargetsForDeserializersArr(MeshRenderer[] renderers, string[] colorProperties)
	{
		return BuildFadeTargetsInternal(renderers, colorProperties);
	}

	private static WeakGuideFadeTarget[] BuildFadeTargetsInternal(MeshRenderer[] renderers, string[] colorProperties)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (renderers == null || renderers.Length == 0)
		{
			return Array.Empty<WeakGuideFadeTarget>();
		}
		List<WeakGuideFadeTarget> list = new List<WeakGuideFadeTarget>();
		string[] array = colorProperties ?? Array.Empty<string>();
		foreach (MeshRenderer val in renderers)
		{
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			Material[] materials = ((Renderer)val).materials;
			foreach (Material val2 in materials)
			{
				if ((Object)(object)val2 == (Object)null)
				{
					continue;
				}
				foreach (string text in array)
				{
					if (!string.IsNullOrEmpty(text) && val2.HasProperty(text))
					{
						list.Add(new WeakGuideFadeTarget(val2, text, val2.GetColor(text), (Renderer)(object)val));
					}
				}
			}
		}
		return list.ToArray();
	}

	[AsyncStateMachine(typeof(_003CLoadWeakGuidePrefabAsync_003Ed__28))]
	public static UniTask<GameObject> LoadWeakGuidePrefabAsync(WeakGuideGlobalConfig globalConfig, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadWeakGuidePrefabAsync_003Ed__28 _003CLoadWeakGuidePrefabAsync_003Ed__ = default(_003CLoadWeakGuidePrefabAsync_003Ed__28);
		_003CLoadWeakGuidePrefabAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<GameObject>.Create();
		_003CLoadWeakGuidePrefabAsync_003Ed__.globalConfig = globalConfig;
		_003CLoadWeakGuidePrefabAsync_003Ed__._003C_003E1__state = -1;
		_003CLoadWeakGuidePrefabAsync_003Ed__._003C_003Et__builder.Start<_003CLoadWeakGuidePrefabAsync_003Ed__28>(ref _003CLoadWeakGuidePrefabAsync_003Ed__);
		return _003CLoadWeakGuidePrefabAsync_003Ed__._003C_003Et__builder.Task;
	}

	public static UniTask<CircularMotion> EnsureHandInWeakGuideViewAsync(WeakGuideManager manager, CancellationToken ct)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return EnsureHandInWeakGuideViewAsync(manager, null, ct);
	}

	[AsyncStateMachine(typeof(_003CEnsureHandInWeakGuideViewAsync_003Ed__30))]
	public static UniTask<CircularMotion> EnsureHandInWeakGuideViewAsync(WeakGuideManager manager, WeakGuideGlobalConfig globalConfig, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CEnsureHandInWeakGuideViewAsync_003Ed__30 _003CEnsureHandInWeakGuideViewAsync_003Ed__ = default(_003CEnsureHandInWeakGuideViewAsync_003Ed__30);
		_003CEnsureHandInWeakGuideViewAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CircularMotion>.Create();
		_003CEnsureHandInWeakGuideViewAsync_003Ed__.manager = manager;
		_003CEnsureHandInWeakGuideViewAsync_003Ed__.globalConfig = globalConfig;
		_003CEnsureHandInWeakGuideViewAsync_003Ed__.ct = ct;
		_003CEnsureHandInWeakGuideViewAsync_003Ed__._003C_003E1__state = -1;
		_003CEnsureHandInWeakGuideViewAsync_003Ed__._003C_003Et__builder.Start<_003CEnsureHandInWeakGuideViewAsync_003Ed__30>(ref _003CEnsureHandInWeakGuideViewAsync_003Ed__);
		return _003CEnsureHandInWeakGuideViewAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CRebuildHandSpinePlayerAsync_003Ed__31))]
	public static UniTask<SpinePlayer> RebuildHandSpinePlayerAsync(WeakGuideManager manager, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRebuildHandSpinePlayerAsync_003Ed__31 _003CRebuildHandSpinePlayerAsync_003Ed__ = default(_003CRebuildHandSpinePlayerAsync_003Ed__31);
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<SpinePlayer>.Create();
		_003CRebuildHandSpinePlayerAsync_003Ed__.manager = manager;
		_003CRebuildHandSpinePlayerAsync_003Ed__.ct = ct;
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003E1__state = -1;
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder.Start<_003CRebuildHandSpinePlayerAsync_003Ed__31>(ref _003CRebuildHandSpinePlayerAsync_003Ed__);
		return _003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CRebuildHandSpinePlayerAsync_003Ed__32))]
	public static UniTask<SpinePlayer> RebuildHandSpinePlayerAsync(WeakGuideManager manager, WeakGuideGlobalConfig globalConfig, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRebuildHandSpinePlayerAsync_003Ed__32 _003CRebuildHandSpinePlayerAsync_003Ed__ = default(_003CRebuildHandSpinePlayerAsync_003Ed__32);
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<SpinePlayer>.Create();
		_003CRebuildHandSpinePlayerAsync_003Ed__.manager = manager;
		_003CRebuildHandSpinePlayerAsync_003Ed__.globalConfig = globalConfig;
		_003CRebuildHandSpinePlayerAsync_003Ed__.ct = ct;
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003E1__state = -1;
		_003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder.Start<_003CRebuildHandSpinePlayerAsync_003Ed__32>(ref _003CRebuildHandSpinePlayerAsync_003Ed__);
		return _003CRebuildHandSpinePlayerAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__33))]
	public static UniTask<SpinePlayer> RebuildHandSpinePlayerAsyncForGeneric(WeakGuideManager manager, WeakGuideGlobalConfig globalConfig, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__33 _003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__ = default(_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__33);
		_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<SpinePlayer>.Create();
		_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__.manager = manager;
		_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__.globalConfig = globalConfig;
		_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__.ct = ct;
		_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__._003C_003E1__state = -1;
		_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__._003C_003Et__builder.Start<_003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__33>(ref _003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__);
		return _003CRebuildHandSpinePlayerAsyncForGeneric_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CLoadHandPrefabAsync_003Ed__34))]
	public static UniTask<GameObject> LoadHandPrefabAsync(WeakGuideGlobalConfig globalConfig, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadHandPrefabAsync_003Ed__34 _003CLoadHandPrefabAsync_003Ed__ = default(_003CLoadHandPrefabAsync_003Ed__34);
		_003CLoadHandPrefabAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<GameObject>.Create();
		_003CLoadHandPrefabAsync_003Ed__.globalConfig = globalConfig;
		_003CLoadHandPrefabAsync_003Ed__._003C_003E1__state = -1;
		_003CLoadHandPrefabAsync_003Ed__._003C_003Et__builder.Start<_003CLoadHandPrefabAsync_003Ed__34>(ref _003CLoadHandPrefabAsync_003Ed__);
		return _003CLoadHandPrefabAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static GameObject TryGetWeakGuideViewRoot()
	{
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("WeakGuideView", ref val) || !(val is UIWeakGuideController { View: not null, View: var view }))
		{
			return null;
		}
		if ((Object)(object)((UIBaseView)view).Behaviour != (Object)null)
		{
			return ((Component)((UIBaseView)view).Behaviour).gameObject;
		}
		if (!((Object)(object)((UIBaseView)view).NodeRect != (Object)null))
		{
			return null;
		}
		return ((Component)((UIBaseView)view).NodeRect).gameObject;
	}

	public static Transform FindWorldSpaceHandNode(WeakGuideManager manager)
	{
		return FindWorldSpaceHandNode(manager, null);
	}

	public static Transform FindWorldSpaceHandNode(WeakGuideManager manager, WeakGuideGlobalConfig globalConfig)
	{
		if ((Object)(object)manager == (Object)null)
		{
			return null;
		}
		string text = (((Object)(object)globalConfig != (Object)null && !string.IsNullOrEmpty(globalConfig.WorldSpaceHandNodeName)) ? globalConfig.WorldSpaceHandNodeName : "GuidHand1");
		Transform val = ((Component)manager).transform.Find(text);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		CircularMotion componentInChildren = ((Component)manager).GetComponentInChildren<CircularMotion>(true);
		if (!((Object)(object)componentInChildren != (Object)null))
		{
			return null;
		}
		return ((Component)componentInChildren).transform;
	}

	public static CircularMotion FindOrbitHandMotion(WeakGuideManager manager, string centerPhysicPartId = null)
	{
		if ((Object)(object)manager == (Object)null)
		{
			return null;
		}
		Transform val = FindWorldSpaceHandNode(manager);
		if ((Object)(object)val != (Object)null)
		{
			CircularMotion component = ((Component)val).GetComponent<CircularMotion>();
			if ((Object)(object)component != (Object)null && component.MotionMode == CircularMotionMode.Orbit)
			{
				return component;
			}
		}
		CircularMotion[] componentsInChildren = ((Component)manager).GetComponentsInChildren<CircularMotion>(true);
		foreach (CircularMotion circularMotion in componentsInChildren)
		{
			if ((Object)(object)circularMotion != (Object)null && circularMotion.MotionMode == CircularMotionMode.Orbit)
			{
				return circularMotion;
			}
		}
		return null;
	}

	private static Transform FindHandMotionTransform(GameObject handInstance)
	{
		return FindHandMotionTransform(handInstance, null);
	}

	private static Transform FindHandMotionTransform(GameObject handInstance, WeakGuideGlobalConfig globalConfig)
	{
		if ((Object)(object)handInstance == (Object)null)
		{
			return null;
		}
		string text = (((Object)(object)globalConfig != (Object)null && !string.IsNullOrEmpty(globalConfig.HandSkeletonNodeName)) ? globalConfig.HandSkeletonNodeName : "hand");
		Transform val = handInstance.transform.Find(text);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		return handInstance.transform;
	}
}
