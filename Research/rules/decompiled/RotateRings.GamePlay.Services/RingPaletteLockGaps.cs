using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.GamePlay.Objects;
using UniLog;
using UnityEngine;
using XingYun.RingGame;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockGaps
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CEnsureGapPrefabAsync_003Ed__31 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<GameObject> _003C_003Et__builder;

		public RingPaletteLockGaps _003C_003E4__this;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingPaletteLockGaps ringPaletteLockGaps = _003C_003E4__this;
			GameObject gapPrefab;
			try
			{
				Awaiter<GameObject> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
					goto IL_0090;
				}
				if (!((Object)(object)ringPaletteLockGaps.gapPrefab != (Object)null))
				{
					val = GameApp.Res.LoadAssetAsync<GameObject>("Assets/02_Res/Art/Level/LevelPrefab/RingPaletteLockGap.prefab", (string)null).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CEnsureGapPrefabAsync_003Ed__31>(ref val, ref this);
						return;
					}
					goto IL_0090;
				}
				gapPrefab = ringPaletteLockGaps.gapPrefab;
				goto end_IL_000e;
				IL_0090:
				GameObject result = val.GetResult();
				ringPaletteLockGaps.gapPrefab = result;
				if ((Object)(object)ringPaletteLockGaps.gapPrefab == (Object)null)
				{
					Log.LogError("[RingPaletteLockGaps] ", "加载 RingPaletteLockGap prefab 失败 path={0}", new object[1] { "Assets/02_Res/Art/Level/LevelPrefab/RingPaletteLockGap.prefab" });
				}
				gapPrefab = ringPaletteLockGaps.gapPrefab;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(gapPrefab);
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
	private struct _003CSpawnForNewGameAsync_003Ed__19 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingPaletteLockGaps _003C_003E4__this;

		public IReadOnlyList<RingObject> rings;

		private int _003CringsNeedingGap_003E5__2;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingPaletteLockGaps ringPaletteLockGaps = _003C_003E4__this;
			try
			{
				Awaiter<GameObject> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<GameObject>);
					num = (_003C_003E1__state = -1);
					goto IL_00a3;
				}
				ringPaletteLockGaps.gapByHostRing.Clear();
				if (rings != null && rings.Count != 0)
				{
					_003CringsNeedingGap_003E5__2 = ringPaletteLockGaps.CountRingsNeedingGap(rings);
					val = ringPaletteLockGaps.EnsureGapPrefabAsync().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CSpawnForNewGameAsync_003Ed__19>(ref val, ref this);
						return;
					}
					goto IL_00a3;
				}
				goto end_IL_000e;
				IL_00a3:
				GameObject result = val.GetResult();
				if ((Object)(object)result == (Object)null)
				{
					if (_003CringsNeedingGap_003E5__2 > 0)
					{
						Log.LogError("[RingPaletteLockGaps] ", "调色板锁缺口 Prefab 未加载，{0} 个被锁环无法生成灰补块（玩法可能卡住）。请确认资源已打入包体：{1}", new object[2] { _003CringsNeedingGap_003E5__2, "Assets/02_Res/Art/Level/LevelPrefab/RingPaletteLockGap.prefab" });
					}
				}
				else
				{
					GameplayManager hostInstance = ringPaletteLockGaps.owner.HostInstance;
					RingObjectsService objectsService = hostInstance.ObjectsService;
					RingObjectSolveService solveService = hostInstance.SolveService;
					for (int i = 0; i < rings.Count; i++)
					{
						RingObject ringObject = rings[i];
						if (!((Object)(object)ringObject == (Object)null) && ringPaletteLockGaps.ShouldSpawnGap(ringObject))
						{
							RingPaletteLockGapObject ringPaletteLockGapObject = ringPaletteLockGaps.SpawnGapForRing(ringObject, result);
							if (!((Object)(object)ringPaletteLockGapObject == (Object)null))
							{
								ringPaletteLockGaps.gapByHostRing[ringObject] = ringPaletteLockGapObject;
								objectsService.AddRuntimeObject(ringPaletteLockGapObject);
								objectsService.RegisterPartId(ringPaletteLockGapObject);
								ringPaletteLockGapObject.RegisterOnSolved(hostInstance.OnInteractableSolved);
								solveService.RegisterObject(ringPaletteLockGapObject);
							}
						}
					}
					_ = _003CringsNeedingGap_003E5__2;
					_ = ringPaletteLockGaps.gapByHostRing.Count;
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

	private const string Tag = "[RingPaletteLockGaps] ";

	private const string GapPrefabPath = "Assets/02_Res/Art/Level/LevelPrefab/RingPaletteLockGap.prefab";

	private const float MinGapAngleDeg = 0.5f;

	private const float RingOpeningDirectionDeg = 0f;

	private const float FillerArcDirectionOffsetDeg = 180f;

	private const float FillerGapAngleExtraDeg = 30f;

	private static readonly Color RedLockBody = RingTheme.Hex("F14835");

	private static readonly Color RedLockHeart = RingTheme.Hex("9A282C");

	private static readonly Color YellowLockBody = RingTheme.Hex("FFC900");

	private static readonly Color YellowLockHeart = RingTheme.Hex("8F4715");

	private static readonly Color BlueLockBody = RingTheme.Hex("39C4F3");

	private static readonly Color BlueLockHeart = RingTheme.Hex("34539D");

	private static readonly Color GreenLockBody = RingTheme.Hex("8FE010");

	private static readonly Color GreenLockHeart = RingTheme.Hex("1E670E");

	private readonly RingPaletteLockService owner;

	private readonly Dictionary<RingObject, RingPaletteLockGapObject> gapByHostRing = new Dictionary<RingObject, RingPaletteLockGapObject>();

	private GameObject gapPrefab;

	internal RingPaletteLockGaps(RingPaletteLockService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
	}

	internal void Clear()
	{
		gapByHostRing.Clear();
	}

	[AsyncStateMachine(typeof(_003CSpawnForNewGameAsync_003Ed__19))]
	internal UniTask SpawnForNewGameAsync(IReadOnlyList<RingObject> rings)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CSpawnForNewGameAsync_003Ed__19 _003CSpawnForNewGameAsync_003Ed__ = default(_003CSpawnForNewGameAsync_003Ed__19);
		_003CSpawnForNewGameAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CSpawnForNewGameAsync_003Ed__._003C_003E4__this = this;
		_003CSpawnForNewGameAsync_003Ed__.rings = rings;
		_003CSpawnForNewGameAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CSpawnForNewGameAsync_003Ed__._003C_003Et__builder)).Start<_003CSpawnForNewGameAsync_003Ed__19>(ref _003CSpawnForNewGameAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CSpawnForNewGameAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private int CountRingsNeedingGap(IReadOnlyList<RingObject> rings)
	{
		int num = 0;
		for (int i = 0; i < rings.Count; i++)
		{
			RingObject ringObject = rings[i];
			if ((Object)(object)ringObject != (Object)null && ringObject.IsCurrentPaletteLock && ringObject.PartData.GapAngleDeg > 0.5f)
			{
				num++;
			}
		}
		return num;
	}

	private bool ShouldSpawnGap(RingObject ring)
	{
		GameplayPartData partData = ring.PartData;
		if (ring.IsCurrentPaletteLock)
		{
			return partData.GapAngleDeg > 0.5f;
		}
		return false;
	}

	internal bool TryGetGapForHost(RingObject host, out RingPaletteLockGapObject gap)
	{
		gap = null;
		if ((Object)(object)host == (Object)null)
		{
			return false;
		}
		if (gapByHostRing.TryGetValue(host, out gap) && (Object)(object)gap != (Object)null)
		{
			return !gap.IsSolved;
		}
		return false;
	}

	internal void TrySolveGapForRing(RingObject ring, BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext))
	{
		if (!((Object)(object)ring == (Object)null) && gapByHostRing.TryGetValue(ring, out var value) && !((Object)(object)value == (Object)null) && !value.IsSolved)
		{
			owner.HostInstance.SolveService?.SolveObject(value, sourceBomb, releaseContext);
		}
	}

	internal bool TryGetGapBodyCollider(RingObject host, out Collider2D gapBody)
	{
		gapBody = null;
		if ((Object)(object)host == (Object)null || !gapByHostRing.TryGetValue(host, out var value) || (Object)(object)value == (Object)null || value.IsSolved)
		{
			return false;
		}
		PolygonCollider2D bodyCollider = value.BodyCollider;
		if ((Object)(object)bodyCollider == (Object)null || !((Behaviour)bodyCollider).enabled)
		{
			return false;
		}
		gapBody = (Collider2D)(object)bodyCollider;
		return true;
	}

	internal void OnGapSolved(RingPaletteLockGapObject gap)
	{
		RingObject ringObject = gap?.HostRing;
		if (!((Object)(object)ringObject == (Object)null))
		{
			gapByHostRing.Remove(ringObject);
		}
	}

	private RingPaletteLockGapObject SpawnGapForRing(RingObject ring, GameObject prefab)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		RingSegmentController componentInChildren = ((Component)ring).GetComponentInChildren<RingSegmentController>(true);
		if ((Object)(object)componentInChildren == (Object)null || componentInChildren.CenterRadius <= 0.0001f)
		{
			return null;
		}
		Transform transform = ((Component)ring).transform;
		Transform transform2 = ((Component)componentInChildren).transform;
		GameObject val = Object.Instantiate<GameObject>(prefab, transform, false);
		((Object)val).name = ((Object)ring).name + "_PaletteLockGap";
		val.transform.localPosition = new Vector3(0f, 0f, -0.1f);
		val.transform.localScale = Vector3.one;
		if ((Object)(object)transform2 != (Object)(object)transform)
		{
			val.transform.localRotation = Quaternion.Inverse(transform.rotation) * transform2.rotation;
		}
		else
		{
			val.transform.localRotation = Quaternion.identity;
		}
		RingPaletteLockGapObject component = val.GetComponent<RingPaletteLockGapObject>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("[RingPaletteLockGaps] ", "RingPaletteLockGap prefab 缺少 RingPaletteLockGapObject");
			Object.Destroy((Object)(object)val);
			return null;
		}
		component.InitializeForHostRing(ring);
		ConfigureGapMesh(component, ring, componentInChildren);
		ConfigureLockVisual(component, ring);
		return component;
	}

	private void ConfigureGapMesh(RingPaletteLockGapObject gapObject, RingObject ring, RingSegmentController hostSegment)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		RingSegmentController component = ((Component)gapObject).GetComponent<RingSegmentController>();
		if (!((Object)(object)component == (Object)null))
		{
			float centerRadius = hostSegment.CenterRadius;
			float bandWidth = hostSegment.BandWidth;
			Material sharedMaterial = hostSegment.SharedMaterial;
			Material sharedMat = owner.HostInstance.PartStyle?.GetPaletteLockGapMaterial() ?? component.SharedMaterial ?? sharedMaterial;
			float capRatio = hostSegment.CapRatio;
			float gapDirectionDeg = Mathf.Repeat(180f, 360f);
			float gapAngleDeg = ring.PartData.GapAngleDeg;
			RingGapGeometry.ReadCapLengths(sharedMaterial, out var tailLength, out var headLength);
			float num = RingGapGeometry.ComputeCapShrinkDeg(centerRadius, tailLength, headLength, capRatio);
			float num2 = Mathf.Clamp(Mathf.Clamp(gapAngleDeg + 30f, 0.5f, 359f) + num, 0.5f, 359.5f);
			float num3 = Mathf.Clamp(360f - num2, 0f, 360f);
			RingSegmentBuildSettings buildSettings = component.BuildSettings;
			buildSettings.width = Mathf.Max(0.01f, bandWidth);
			buildSettings.useHeadTail = num3 < 359.9f;
			buildSettings.styleIndex = hostSegment.BuildSettings.styleIndex;
			component.Setup(centerRadius, num3, gapDirectionDeg, buildSettings, sharedMat, RingPaletteLockGapObject.GapFillerVertexColor);
			component.ApplyVertexColor(RingPaletteLockGapObject.GapFillerVertexColor);
			MeshFilter component2 = ((Component)component).GetComponent<MeshFilter>();
			if (!((Object)(object)((component2 != null) ? component2.sharedMesh : null) == (Object)null))
			{
				_ = component2.sharedMesh.vertexCount;
			}
			float innerRadius = GameCoordScaleUtil.CoreToUnity(ring.PartData.InnerRadius);
			float outerRadius = GameCoordScaleUtil.CoreToUnity(ring.PartData.OuterRadius);
			PolygonCollider2D val = gapObject.ResolveBodyCollider();
			if ((Object)(object)val != (Object)null)
			{
				RingColliderBuilder.ApplyRingBodyPolygonCollider(val, component, innerRadius, outerRadius, num3, gapDirectionDeg, (Component)(object)gapObject);
				RingColliderBuilder.ApplyRingDragAreaPolygonCollider(((Component)gapObject).transform, innerRadius, outerRadius, num3, gapDirectionDeg);
				gapObject.RefreshBlockColliders();
			}
		}
	}

	private void ConfigureLockVisual(RingPaletteLockGapObject gapObject, RingObject ring)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)gapObject == (Object)null)
		{
			return;
		}
		RingSegmentController component = ((Component)gapObject).GetComponent<RingSegmentController>();
		if (!((Object)(object)component == (Object)null) && !(component.CenterRadius <= 0.0001f))
		{
			gapObject.ResetLockLocalPosition();
			gapObject.SyncLockSegmentWithGap(component);
			if (owner.TryGetLockHeartColor(ring, out var color) && TryGetLockBodyColor(color, out var color2) && TryGetLockHeartVertexColor(color, out var color3))
			{
				gapObject.ApplyLockVertexColors(color2, color3);
			}
		}
	}

	private bool TryGetLockBodyColor(PaletteLockKeyColor keyColor, out Color color)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		switch (keyColor)
		{
		case PaletteLockKeyColor.Red:
			color = RedLockBody;
			return true;
		case PaletteLockKeyColor.Yellow:
			color = YellowLockBody;
			return true;
		case PaletteLockKeyColor.Blue:
			color = BlueLockBody;
			return true;
		case PaletteLockKeyColor.Green:
			color = GreenLockBody;
			return true;
		default:
			color = Color.white;
			return false;
		}
	}

	private bool TryGetLockHeartVertexColor(PaletteLockKeyColor keyColor, out Color color)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		switch (keyColor)
		{
		case PaletteLockKeyColor.Red:
			color = RedLockHeart;
			return true;
		case PaletteLockKeyColor.Yellow:
			color = YellowLockHeart;
			return true;
		case PaletteLockKeyColor.Blue:
			color = BlueLockHeart;
			return true;
		case PaletteLockKeyColor.Green:
			color = GreenLockHeart;
			return true;
		default:
			color = Color.white;
			return false;
		}
	}

	[AsyncStateMachine(typeof(_003CEnsureGapPrefabAsync_003Ed__31))]
	private UniTask<GameObject> EnsureGapPrefabAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CEnsureGapPrefabAsync_003Ed__31 _003CEnsureGapPrefabAsync_003Ed__ = default(_003CEnsureGapPrefabAsync_003Ed__31);
		_003CEnsureGapPrefabAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<GameObject>.Create();
		_003CEnsureGapPrefabAsync_003Ed__._003C_003E4__this = this;
		_003CEnsureGapPrefabAsync_003Ed__._003C_003E1__state = -1;
		_003CEnsureGapPrefabAsync_003Ed__._003C_003Et__builder.Start<_003CEnsureGapPrefabAsync_003Ed__31>(ref _003CEnsureGapPrefabAsync_003Ed__);
		return _003CEnsureGapPrefabAsync_003Ed__._003C_003Et__builder.Task;
	}
}
