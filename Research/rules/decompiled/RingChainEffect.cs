using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.GamePlay.Services;
using RotateRings.Services;
using Spine;
using Spine.Unity;
using UnityEngine;

internal class RingChainEffect
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayChainBreakDelayedAsync_003Ed__29 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingChainEffect _003C_003E4__this;

		public int slot;

		public int ringOrder;

		public ChainInstanceData data;

		public RingObject lockOpenRing;

		public bool muteBreakSound;

		private CancellationToken _003CdestroyToken_003E5__2;

		private float _003CbreakDuration_003E5__3;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0255: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingChainEffect ringChainEffect = _003C_003E4__this;
			try
			{
				UniTask val2;
				Awaiter<bool> val;
				switch (num)
				{
				default:
					_003CdestroyToken_003E5__2 = ringChainEffect.owner.CancelToken;
					if (!_003CdestroyToken_003E5__2.IsCancellationRequested)
					{
						float num2 = Random.Range(RingLevelConfig.Instance.chainRingBreakIntervalSecondsMin, RingLevelConfig.Instance.chainRingBreakIntervalSecondsMax);
						float num3 = (float)slot * RingLevelConfig.Instance.chainBreakIntervalSeconds + (float)ringOrder * num2;
						if (num3 > 0f)
						{
							val2 = UniTask.Delay(TimeSpan.FromSeconds(num3), false, (PlayerLoopTiming)8, _003CdestroyToken_003E5__2);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayChainBreakDelayedAsync_003Ed__29>(ref val, ref this);
								return;
							}
							goto IL_00f7;
						}
						goto IL_0105;
					}
					goto end_IL_000e;
				case 0:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00f7;
				case 1:
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_01d7;
				case 2:
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_0271;
					}
					IL_0271:
					if (!val.GetResult())
					{
						break;
					}
					goto end_IL_000e;
					IL_01d7:
					if (!val.GetResult())
					{
						AudioService.PlaySound(AudioKeys.ChainBreak.Value);
					}
					goto IL_01f0;
					IL_01f0:
					if (!(_003CbreakDuration_003E5__3 > 0f))
					{
						break;
					}
					val2 = UniTask.Delay(TimeSpan.FromSeconds(_003CbreakDuration_003E5__3), false, (PlayerLoopTiming)8, _003CdestroyToken_003E5__2);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 2);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayChainBreakDelayedAsync_003Ed__29>(ref val, ref this);
						return;
					}
					goto IL_0271;
					IL_00f7:
					if (!val.GetResult())
					{
						goto IL_0105;
					}
					goto end_IL_000e;
					IL_0105:
					if (!((Object)(object)data.Instance == (Object)null))
					{
						_003CbreakDuration_003E5__3 = ringChainEffect.PlayChainBreak(in data);
						if ((Object)(object)lockOpenRing != (Object)null)
						{
							ringChainEffect.owner.ChainLockUi.PlayChainLockOpen(lockOpenRing);
						}
						if (!muteBreakSound)
						{
							val2 = UniTask.Delay(TimeSpan.FromSeconds(0.6000000238418579), false, (PlayerLoopTiming)8, _003CdestroyToken_003E5__2);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayChainBreakDelayedAsync_003Ed__29>(ref val, ref this);
								return;
							}
							goto IL_01d7;
						}
						goto IL_01f0;
					}
					goto end_IL_000e;
				}
				ringChainEffect.RecycleChainInstance(data.Instance);
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CdestroyToken_003E5__2 = default(CancellationToken);
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CdestroyToken_003E5__2 = default(CancellationToken);
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

	private const string TAG = "[GamePlay][RingLockChain] ";

	private const string ChainEffectAssetPath = "eff_junei_unlock_suolian";

	private const float BreakSoundDelaySeconds = 0.6f;

	private static readonly ChainDirectionData[] ChainDirections = new ChainDirectionData[4]
	{
		new ChainDirectionData
		{
			AppearFirst = "rr_suolian_mid_appear",
			IdleFirst = "rr_suolian_mid_jingzhen",
			HuangdongFirst = "rr_suolian_mid_huangdong",
			BreakFirst = "rr_suolian_mid_break",
			ScondChainDirection = ChainDirection.LeftSlant,
			ThirdChainDirection = ChainDirection.RightSlant,
			RootAngleZFirst = 180f,
			InitAngleZ = 180f,
			ChildAngleZFirst = 180f
		},
		new ChainDirectionData
		{
			AppearFirst = "rr_suolian_low_appear",
			IdleFirst = "rr_suolian_low_jingzhen",
			HuangdongFirst = "rr_suolian_low_huangdong",
			BreakFirst = "rr_suolian_low_break",
			ScondChainDirection = ChainDirection.Vertical,
			ThirdChainDirection = ChainDirection.Horizontal,
			RootAngleZFirst = 40f,
			InitAngleZ = 45f,
			ChildAngleZFirst = -40f
		},
		new ChainDirectionData
		{
			AppearFirst = "rr_suolian_high_appear",
			IdleFirst = "rr_suolian_high_jingzhen",
			HuangdongFirst = "rr_suolian_high_huangdong",
			BreakFirst = "rr_suolian_high_break",
			ScondChainDirection = ChainDirection.Horizontal,
			ThirdChainDirection = ChainDirection.Vertical,
			RootAngleZFirst = 140f,
			InitAngleZ = 135f,
			ChildAngleZFirst = -140f
		},
		new ChainDirectionData
		{
			AppearFirst = "rr_suolian_shu_appear",
			IdleFirst = "rr_suolian_shu_jingzhen",
			HuangdongFirst = "rr_suolian_shu_huangdong",
			BreakFirst = "rr_suolian_shu_break",
			ScondChainDirection = ChainDirection.LeftSlant,
			ThirdChainDirection = ChainDirection.RightSlant,
			RootAngleZFirst = 90f,
			InitAngleZ = 90f,
			ChildAngleZFirst = -90f
		}
	};

	private const string ChainRootName = "ChainRoot";

	private const string ChainChildNodeName = "suolian";

	private const string ChainHitColliderName = "ChainHitArea";

	private const float ChainScaleReferenceOuterRadius = 48f;

	private GameObject chainEffectPrefab;

	private readonly Dictionary<RingObject, List<ChainInstanceData>> ringToChainInstances = new Dictionary<RingObject, List<ChainInstanceData>>();

	private readonly Dictionary<RingObject, GameObject> ringToChainRoot = new Dictionary<RingObject, GameObject>();

	private readonly List<GameObject> detachedChainRoots = new List<GameObject>();

	private readonly RingLockChainService owner;

	internal RingChainEffect(RingLockChainService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
	}

	internal void SetupChainOnLevelLoaded()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<RingObject> readOnlyList = owner.HostInstance.ObjectsService?.Rings;
		if (readOnlyList == null)
		{
			return;
		}
		if ((Object)(object)chainEffectPrefab == (Object)null)
		{
			chainEffectPrefab = GameApp.Res.LoadAsset<GameObject>("eff_junei_unlock_suolian", (string)null);
			if ((Object)(object)chainEffectPrefab == (Object)null)
			{
				return;
			}
		}
		int num = 0;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			RingObject ringObject = readOnlyList[i];
			if ((Object)(object)ringObject == (Object)null || ringObject.LockCount <= 0)
			{
				continue;
			}
			GameObject val = new GameObject("ChainRoot");
			Transform transform = val.transform;
			transform.SetParent(((Component)ringObject).transform, false);
			transform.localPosition = new Vector3(0f, 0f, -0.5f);
			transform.localRotation = Quaternion.Euler(0f, 0f, 0f - ((Component)ringObject).transform.localEulerAngles.z);
			BoardFlipEffectUtility.ApplyFlipCompensation(transform, owner.HostInstance);
			transform.localScale = Vector3.one;
			GameObject val2 = new GameObject("ChainHitArea");
			Transform transform2 = val2.transform;
			transform2.SetParent(transform, false);
			transform2.localPosition = Vector3.zero;
			transform2.localRotation = Quaternion.identity;
			transform2.localScale = Vector3.one;
			val2.layer = 6;
			CircleCollider2D obj = val2.AddComponent<CircleCollider2D>();
			((Collider2D)obj).isTrigger = true;
			obj.radius = GameCoordScaleUtil.CoreToUnity(ringObject.PartData.InnerRadius);
			ChainDirection[] array = ResolveMainDirections(ringObject, ringObject.LockCount);
			List<ChainInstanceData> list = new List<ChainInstanceData>(array.Length);
			for (int j = 0; j < array.Length; j++)
			{
				ChainDirection chainDirection = array[j];
				GameObject val3 = GameApp.Pool.Spawn(chainEffectPrefab, transform);
				if ((Object)(object)val3 == (Object)null)
				{
					continue;
				}
				list.Add(new ChainInstanceData(val3, chainDirection));
				((Object)val3).name = $"{GetChainDirectionDisplayName(chainDirection)}_{j + 1}";
				ApplyLayerRecursively(val3.transform, 7);
				Transform transform3 = val3.transform;
				transform3.localPosition = Vector3.zero;
				float num2 = ResolveChainScaleX(ringObject.PartData.OuterRadius);
				transform3.localScale = new Vector3(num2, num2, 1f);
				ChainDirectionData chainDirectionData = ChainDirections[(int)chainDirection];
				float num3 = (((ringObject.LockCount != 1 && ringObject.LockCount < 3) || j != 0) ? ResolveEdgeAngle(halfGap: ringObject.PartData.GapAngleDeg * 0.5f, initAngle: chainDirectionData.InitAngleZ, ringLocalZ: ((Component)ringObject).transform.localEulerAngles.z) : ResolveNearestAngle(target: Mathf.Repeat(((Component)ringObject).transform.localEulerAngles.z + 90f, 360f), baseAngle: chainDirectionData.InitAngleZ));
				transform3.localRotation = Quaternion.Euler(0f, 0f, num3);
				Transform val4 = transform3.Find("suolian");
				if ((Object)(object)val4 != (Object)null)
				{
					Vector3 localEulerAngles = val4.localEulerAngles;
					localEulerAngles.z = chainDirectionData.ChildAngleZFirst;
					val4.localEulerAngles = localEulerAngles;
				}
				SkeletonAnimation componentInChildren = val3.GetComponentInChildren<SkeletonAnimation>(true);
				if ((Object)(object)componentInChildren != (Object)null)
				{
					string idleFirst = chainDirectionData.IdleFirst;
					if (!HasSpineAnimation(componentInChildren, idleFirst))
					{
						idleFirst = ChainDirections[0].IdleFirst;
					}
					componentInChildren.AnimationState.SetAnimation(0, idleFirst, true);
				}
				num++;
			}
			ringToChainRoot[ringObject] = val;
			ringToChainInstances[ringObject] = list;
		}
	}

	private ChainDirection[] ResolveMainDirections(RingObject ring, int lockCount)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ring == (Object)null)
		{
			return new ChainDirection[1];
		}
		float angleDeg = Mathf.Repeat(((Component)ring).transform.localEulerAngles.z + 90f, 360f);
		ChainDirection chainDirection = ResolveDirectionBySector(angleDeg);
		if (lockCount > 1)
		{
			ChainDirectionData chainDirectionData = ChainDirections[(int)chainDirection];
			if (lockCount != 2)
			{
				return new ChainDirection[3] { chainDirection, chainDirectionData.ScondChainDirection, chainDirectionData.ThirdChainDirection };
			}
			return new ChainDirection[2] { chainDirectionData.ScondChainDirection, chainDirectionData.ThirdChainDirection };
		}
		return new ChainDirection[1] { chainDirection };
	}

	private float ResolveNearestAngle(float baseAngle, float target)
	{
		float num = Mathf.Repeat(target, 360f);
		float num2 = Mathf.Repeat(target + 180f, 360f);
		float num3 = Mathf.Abs(Mathf.DeltaAngle(baseAngle, num));
		float num4 = Mathf.Abs(Mathf.DeltaAngle(baseAngle, num2));
		if (!(num3 <= num4))
		{
			return num2;
		}
		return num;
	}

	private float ResolveChainScaleX(float outerRadius)
	{
		if (outerRadius <= 0f)
		{
			return 1f;
		}
		return outerRadius / 48f;
	}

	private float ResolveEdgeAngle(float initAngle, float ringLocalZ, float halfGap)
	{
		float chainEdgeInsetDeg = RingLevelConfig.Instance.chainEdgeInsetDeg;
		float target = Mathf.Repeat(ringLocalZ + halfGap + chainEdgeInsetDeg, 360f);
		float target2 = Mathf.Repeat(ringLocalZ - halfGap - chainEdgeInsetDeg, 360f);
		float num = ResolveNearestAngle(initAngle, target);
		float num2 = ResolveNearestAngle(initAngle, target2);
		if (!(Mathf.Abs(Mathf.DeltaAngle(initAngle, num)) <= Mathf.Abs(Mathf.DeltaAngle(initAngle, num2))))
		{
			return num2;
		}
		return num;
	}

	private ChainDirection ResolveDirectionBySector(float angleDeg)
	{
		switch (Mathf.RoundToInt(angleDeg / 45f) % 8)
		{
		case 1:
		case 5:
			return ChainDirection.RightSlant;
		case 2:
		case 6:
			return ChainDirection.Vertical;
		case 3:
		case 7:
			return ChainDirection.LeftSlant;
		default:
			return ChainDirection.Horizontal;
		}
	}

	private string GetChainDirectionDisplayName(ChainDirection direction)
	{
		return direction switch
		{
			ChainDirection.Horizontal => "mid", 
			ChainDirection.RightSlant => "low", 
			ChainDirection.LeftSlant => "high", 
			ChainDirection.Vertical => "shu", 
			_ => direction.ToString(), 
		};
	}

	private string ResolveChainBreakAnimation(ChainDirection direction, SkeletonAnimation skeleton)
	{
		string breakFirst = ChainDirections[(int)direction].BreakFirst;
		if (!HasSpineAnimation(skeleton, breakFirst))
		{
			return null;
		}
		return breakFirst;
	}

	private bool HasSpineAnimation(SkeletonAnimation skeleton, string animName)
	{
		if (!((Object)(object)skeleton == (Object)null))
		{
			Skeleton skeleton2 = ((SkeletonRenderer)skeleton).Skeleton;
			if (((skeleton2 != null) ? skeleton2.Data : null) != null && !string.IsNullOrEmpty(animName))
			{
				return ((SkeletonRenderer)skeleton).Skeleton.Data.FindAnimation(animName) != null;
			}
		}
		return false;
	}

	private int GetLockedRingCount(IReadOnlyList<RingObject> rings)
	{
		if (rings == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < rings.Count; i++)
		{
			if ((Object)(object)rings[i] != (Object)null && rings[i].LockCount > 0)
			{
				num++;
			}
		}
		return num;
	}

	internal void PlayChainShake(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null))
		{
			AudioService.PlaySound(AudioKeys.ChainShack.Value);
			PlayChainShakeAnim(ring);
		}
	}

	internal void PlayChainShakeNoSound(RingObject ring)
	{
		PlayChainShakeAnim(ring);
	}

	private void PlayChainShakeAnim(RingObject ring)
	{
		if ((Object)(object)ring == (Object)null || !ringToChainInstances.TryGetValue(ring, out var value) || value == null)
		{
			return;
		}
		int num = Math.Min(ring.LockCount, value.Count);
		for (int i = 0; i < num; i++)
		{
			ChainInstanceData chainInstanceData = value[i];
			if ((Object)(object)chainInstanceData.Instance == (Object)null)
			{
				continue;
			}
			SkeletonAnimation componentInChildren = chainInstanceData.Instance.GetComponentInChildren<SkeletonAnimation>(true);
			if (!((Object)(object)componentInChildren == (Object)null))
			{
				string huangdongFirst = ChainDirections[(int)chainInstanceData.Direction].HuangdongFirst;
				if (HasSpineAnimation(componentInChildren, huangdongFirst))
				{
					componentInChildren.AnimationState.SetAnimation(0, huangdongFirst, false);
				}
			}
		}
	}

	internal void DetachChainRoot(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null) && ringToChainRoot.TryGetValue(ring, out var value) && !((Object)(object)value == (Object)null))
		{
			Transform val = value.transform.Find("ChainHitArea");
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
			Transform val2 = (((Object)(object)owner.HostInstance != (Object)null && (Object)(object)owner.HostInstance.CurrentLevelObject != (Object)null) ? owner.HostInstance.CurrentLevelObject.transform : null);
			value.transform.SetParent(val2, true);
			if (!detachedChainRoots.Contains(value))
			{
				detachedChainRoots.Add(value);
			}
		}
	}

	internal void PlayChainBreakDelayed(in ChainInstanceData data, int slot, bool muteBreakSound = false, RingObject lockOpenRing = null, int ringOrder = 0)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)data.Instance == (Object)null))
		{
			ApplyDefaultLayer(data.Instance);
			UniTaskVoid val = PlayChainBreakDelayedAsync(data, slot, muteBreakSound, lockOpenRing, ringOrder);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CPlayChainBreakDelayedAsync_003Ed__29))]
	private UniTaskVoid PlayChainBreakDelayedAsync(ChainInstanceData data, int slot, bool muteBreakSound, RingObject lockOpenRing, int ringOrder)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayChainBreakDelayedAsync_003Ed__29 _003CPlayChainBreakDelayedAsync_003Ed__ = default(_003CPlayChainBreakDelayedAsync_003Ed__29);
		_003CPlayChainBreakDelayedAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayChainBreakDelayedAsync_003Ed__._003C_003E4__this = this;
		_003CPlayChainBreakDelayedAsync_003Ed__.data = data;
		_003CPlayChainBreakDelayedAsync_003Ed__.slot = slot;
		_003CPlayChainBreakDelayedAsync_003Ed__.muteBreakSound = muteBreakSound;
		_003CPlayChainBreakDelayedAsync_003Ed__.lockOpenRing = lockOpenRing;
		_003CPlayChainBreakDelayedAsync_003Ed__.ringOrder = ringOrder;
		_003CPlayChainBreakDelayedAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayChainBreakDelayedAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayChainBreakDelayedAsync_003Ed__29>(ref _003CPlayChainBreakDelayedAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayChainBreakDelayedAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private float PlayChainBreak(in ChainInstanceData data)
	{
		if ((Object)(object)data.Instance == (Object)null)
		{
			return 0f;
		}
		SkeletonAnimation componentInChildren = data.Instance.GetComponentInChildren<SkeletonAnimation>(true);
		if ((Object)(object)componentInChildren == (Object)null)
		{
			return 0f;
		}
		string text = ResolveChainBreakAnimation(data.Direction, componentInChildren);
		if (text == null)
		{
			text = ChainDirections[0].BreakFirst;
			if (!HasSpineAnimation(componentInChildren, text))
			{
				return 0f;
			}
		}
		componentInChildren.AnimationState.SetAnimation(0, text, false);
		return ((SkeletonRenderer)componentInChildren).Skeleton.Data.FindAnimation(text).Duration;
	}

	private void RecycleChainInstance(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			GameApp.Pool.Recycle(instance);
		}
	}

	private void ApplyDefaultLayer(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			ApplyLayerRecursively(instance.transform, LayerMask.NameToLayer("Default"));
		}
	}

	private void ApplyLayerRecursively(Transform root, int layer)
	{
		if (!((Object)(object)root == (Object)null))
		{
			((Component)root).gameObject.layer = layer;
			for (int i = 0; i < root.childCount; i++)
			{
				ApplyLayerRecursively(root.GetChild(i), layer);
			}
		}
	}

	internal bool AllChainsUnlocked()
	{
		if (ringToChainInstances.Count == 0)
		{
			return true;
		}
		foreach (KeyValuePair<RingObject, List<ChainInstanceData>> ringToChainInstance in ringToChainInstances)
		{
			RingObject key = ringToChainInstance.Key;
			if ((Object)(object)key != (Object)null && key.LockCount > 0)
			{
				return false;
			}
		}
		return true;
	}

	internal bool TryGetInstances(RingObject ring, out List<ChainInstanceData> instances)
	{
		return ringToChainInstances.TryGetValue(ring, out instances);
	}

	internal void CollectLockedRings(List<RingObject> output)
	{
		output.Clear();
		foreach (KeyValuePair<RingObject, List<ChainInstanceData>> ringToChainInstance in ringToChainInstances)
		{
			RingObject key = ringToChainInstance.Key;
			if ((Object)(object)key != (Object)null && key.LockCount > 0)
			{
				output.Add(key);
			}
		}
	}

	internal void Dispose()
	{
		ringToChainInstances.Clear();
		ringToChainRoot.Clear();
		DestroyDetachedChainRoots();
		chainEffectPrefab = null;
	}

	private void DestroyDetachedChainRoots()
	{
		for (int i = 0; i < detachedChainRoots.Count; i++)
		{
			GameObject val = detachedChainRoots[i];
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)val);
			}
		}
		detachedChainRoots.Clear();
	}
}
