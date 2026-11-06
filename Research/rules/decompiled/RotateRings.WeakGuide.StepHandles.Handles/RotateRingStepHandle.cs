using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.WeakGuide.Setup;
using UnityEngine;
using XingYun.RingGame;

namespace RotateRings.WeakGuide.StepHandles.Handles;

public sealed class RotateRingStepHandle : WeakGuideStepHandleBase
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CBuildGuideStepAsync_003Ed__12 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<GuideStep> _003C_003Et__builder;

		public WeakGuideBuildContext ctx;

		public RotateRingStepHandle _003C_003E4__this;

		public string paramsJson;

		private RotateRingHandleParams _003Cp_003E5__2;

		private WeakGuideManager _003Cmanager_003E5__3;

		private GameplayManager _003Cgameplay_003E5__4;

		private GuideStep _003Cstep_003E5__5;

		private GameObject _003Cring_003E5__6;

		private Awaiter<CircularMotion> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_048e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0493: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0402: Unknown result type (might be due to invalid IL or missing references)
			//IL_0414: Unknown result type (might be due to invalid IL or missing references)
			//IL_043d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RotateRingStepHandle rotateRingStepHandle = _003C_003E4__this;
			GuideStep result;
			try
			{
				Awaiter<CircularMotion> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CircularMotion>);
					num = (_003C_003E1__state = -1);
					goto IL_024b;
				}
				if (ctx == null || (Object)(object)ctx.Manager == (Object)null || (Object)(object)ctx.GameplayManager == (Object)null)
				{
					rotateRingStepHandle.LogWarning("BuildGuideStep 上下文缺失(manager / gameplayManager),返回 null");
					result = null;
				}
				else if (ctx.InstallToken.IsCancellationRequested)
				{
					result = null;
				}
				else
				{
					_003Cp_003E5__2 = rotateRingStepHandle.ParseParams<RotateRingHandleParams>(paramsJson);
					if (_003Cp_003E5__2 != null && !string.IsNullOrEmpty(_003Cp_003E5__2.TargetRingPhysicPartId))
					{
						_003Cmanager_003E5__3 = ctx.Manager;
						_003Cgameplay_003E5__4 = ctx.GameplayManager;
						WeakGuideGlobalConfig globalConfig = ctx.GlobalConfig;
						_003Cstep_003E5__5 = new GuideStep
						{
							GuideTextKey = ((!string.IsNullOrEmpty(_003Cp_003E5__2.GuideTextKey)) ? _003Cp_003E5__2.GuideTextKey : ctx.Step?.GuideTextKey)
						};
						if (!string.IsNullOrEmpty(_003Cp_003E5__2.AdjustOtherRingPhysicPartId))
						{
							WeakGuideSetupHelpers.AdjustRingWholeZ(_003Cgameplay_003E5__4, _003Cp_003E5__2.AdjustOtherRingPhysicPartId, _003Cp_003E5__2.AdjustOtherRingZ);
							WeakGuideSetupHelpers.AdjustRingZ(_003Cgameplay_003E5__4, _003Cp_003E5__2.AdjustOtherRingPhysicPartId, _003Cp_003E5__2.AdjustOtherRingCrossBarZ);
						}
						_003Cring_003E5__6 = WeakGuideSetupHelpers.FindRingByPhysicPartId(_003Cgameplay_003E5__4, _003Cp_003E5__2.TargetRingPhysicPartId);
						if ((Object)(object)_003Cring_003E5__6 == (Object)null)
						{
							rotateRingStepHandle.LogWarning("未找到 PhysicPartId=" + _003Cp_003E5__2.TargetRingPhysicPartId + " 的圆环,WireCenter 跳过");
						}
						else
						{
							WeakGuideSetupHelpers.WireAllCircularMotionsCenter(((Component)_003Cmanager_003E5__3).gameObject, _003Cring_003E5__6.transform);
						}
						val = WeakGuideSetupHelpers.EnsureHandInWeakGuideViewAsync(_003Cmanager_003E5__3, globalConfig, ctx.InstallToken).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CircularMotion>, _003CBuildGuideStepAsync_003Ed__12>(ref val, ref this);
							return;
						}
						goto IL_024b;
					}
					rotateRingStepHandle.LogWarning("BuildGuideStep 收到空 TargetRingPhysicPartId,返回 null");
					result = null;
				}
				goto end_IL_000e;
				IL_024b:
				CircularMotion result2 = val.GetResult();
				if ((Object)(object)result2 == (Object)null)
				{
					rotateRingStepHandle.LogWarning("HandUI 构造失败,后续 Orbit motion 可能找不到 handMotion");
				}
				if (ctx.InstallToken.IsCancellationRequested || (Object)(object)_003Cmanager_003E5__3 == (Object)null || (Object)(object)_003Cgameplay_003E5__4 == (Object)null || (Object)(object)ctx.InstallContext?.LevelRoot == (Object)null || (Object)(object)ctx.InstallContext.LevelRoot != (Object)(object)_003Cgameplay_003E5__4.CurrentLevelObject)
				{
					result = null;
				}
				else
				{
					if ((Object)(object)result2 != (Object)null && (Object)(object)_003Cring_003E5__6 != (Object)null)
					{
						Transform transform = ((Component)result2).transform;
						Transform transform2 = _003Cring_003E5__6.transform;
						Vector3 position = transform2.position;
						RingObject component = _003Cring_003E5__6.GetComponent<RingObject>();
						GameplayPartData? gameplayPartData = (((Object)(object)component != (Object)null) ? new GameplayPartData?(component.PartData) : ((GameplayPartData?)null));
						float num2 = GameCoordScaleUtil.CoreToUnity((gameplayPartData.HasValue && gameplayPartData.Value.OuterRadius > 0.0001f) ? ((gameplayPartData.Value.InnerRadius + gameplayPartData.Value.OuterRadius) * 0.5f) : 0f);
						if (gameplayPartData.HasValue)
						{
							_ = gameplayPartData.Value.GapAngleDeg;
						}
						float num3 = (transform2.eulerAngles.z + _003Cp_003E5__2.HandAngleFromGapDeg) * (MathF.PI / 180f);
						float num4 = Mathf.Cos(num3);
						float num5 = Mathf.Sin(num3);
						Vector3 position2 = default(Vector3);
						((Vector3)(ref position2))._002Ector(position.x + num2 * num4, position.y + num2 * num5, position.z - 1f);
						transform.position = position2;
						if (result2 != null)
						{
							CircularMotion circularMotion = result2;
							if (circularMotion.MotionMode == CircularMotionMode.Orbit && (Object)(object)circularMotion.Center != (Object)null)
							{
								circularMotion.InitFromPosition(position2);
							}
						}
					}
					CircularMotion circularMotion2 = WeakGuideSetupHelpers.InjectHighlightRingClone(_003Cmanager_003E5__3, _003Cgameplay_003E5__4, result2, _003Cp_003E5__2.TargetRingPhysicPartId);
					GameObject val2 = (((Object)(object)circularMotion2 != (Object)null) ? ((Component)circularMotion2).gameObject : null);
					if ((Object)(object)val2 != (Object)null)
					{
						Vector3 position3 = val2.transform.position;
						position3.z = _003Cp_003E5__2.HighlightZOffset;
						val2.transform.position = position3;
						RingSegmentController[] componentsInChildren = val2.GetComponentsInChildren<RingSegmentController>(true);
						WeakGuideSetupHelpers.ComputeHighlightRingZOffsets(_003Cring_003E5__6, out var ringZ, out var stemZ, out var crossBarZ);
						foreach (RingSegmentController ringSegmentController in componentsInChildren)
						{
							if ((Object)(object)ringSegmentController != (Object)null)
							{
								ringSegmentController.SetZLayerOffsets(ringZ, stemZ, crossBarZ);
							}
						}
						_003Cstep_003E5__5.Append(circularMotion2);
					}
					else
					{
						rotateRingStepHandle.LogWarning("Highlight 克隆 PhysicPartId=" + _003Cp_003E5__2.TargetRingPhysicPartId + " 失败,后续 Fade / SelfRotate 跳过");
					}
					if ((Object)(object)result2 != (Object)null)
					{
						result2.SetMotionMode(CircularMotionMode.Orbit);
						result2.ConfigureMotion(_003Cp_003E5__2.Direction, _003Cp_003E5__2.TotalRotation, _003Cp_003E5__2.RotationDuration, 0f);
						_003Cstep_003E5__5.Append(result2);
					}
					if ((Object)(object)circularMotion2 != (Object)null)
					{
						circularMotion2.SetMotionMode(CircularMotionMode.SelfRotate);
						circularMotion2.ConfigureMotion(_003Cp_003E5__2.Direction, _003Cp_003E5__2.TotalRotation, _003Cp_003E5__2.RotationDuration, 0f);
					}
					if ((Object)(object)val2 != (Object)null)
					{
						FadeMotion fadeMotion = val2.GetComponent<FadeMotion>();
						if ((Object)(object)fadeMotion == (Object)null)
						{
							fadeMotion = val2.AddComponent<FadeMotion>();
						}
						MeshRenderer[] componentsInChildren2 = val2.GetComponentsInChildren<MeshRenderer>(true);
						string[] colorProperties = new string[5] { "_Color", "_OutlineColor", "_ShadowTintColor", "_ShadowColor", "_UnderlayColor" };
						WeakGuideFadeTarget[] targets = WeakGuideSetupHelpers.BuildFadeTargetsForDeserializers(componentsInChildren2, colorProperties);
						fadeMotion.Configure(targets, _003Cp_003E5__2.HighlightFadeDuration);
						_003Cstep_003E5__5.Append(fadeMotion);
					}
					result = _003Cstep_003E5__5;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cp_003E5__2 = null;
				_003Cmanager_003E5__3 = null;
				_003Cgameplay_003E5__4 = null;
				_003Cstep_003E5__5 = null;
				_003Cring_003E5__6 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cp_003E5__2 = null;
			_003Cmanager_003E5__3 = null;
			_003Cgameplay_003E5__4 = null;
			_003Cstep_003E5__5 = null;
			_003Cring_003E5__6 = null;
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

	public const string Id = "RotateRing";

	public const string DisplayName = "旋转指定环到目标角度";

	protected override string Tag => "[WeakGuide][RotateRing] ";

	public override string HandleId => "RotateRing";

	public override string HandleName => "旋转指定环到目标角度";

	public override Type ParametersType => typeof(RotateRingHandleParams);

	public override string DefaultParametersJson => JsonUtility.ToJson((object)new RotateRingHandleParams(), false);

	[AsyncStateMachine(typeof(_003CBuildGuideStepAsync_003Ed__12))]
	public override UniTask<GuideStep> BuildGuideStepAsync(WeakGuideBuildContext ctx, string paramsJson)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CBuildGuideStepAsync_003Ed__12 _003CBuildGuideStepAsync_003Ed__ = default(_003CBuildGuideStepAsync_003Ed__12);
		_003CBuildGuideStepAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<GuideStep>.Create();
		_003CBuildGuideStepAsync_003Ed__._003C_003E4__this = this;
		_003CBuildGuideStepAsync_003Ed__.ctx = ctx;
		_003CBuildGuideStepAsync_003Ed__.paramsJson = paramsJson;
		_003CBuildGuideStepAsync_003Ed__._003C_003E1__state = -1;
		_003CBuildGuideStepAsync_003Ed__._003C_003Et__builder.Start<_003CBuildGuideStepAsync_003Ed__12>(ref _003CBuildGuideStepAsync_003Ed__);
		return _003CBuildGuideStepAsync_003Ed__._003C_003Et__builder.Task;
	}

	public override bool TryFinishStep(WeakGuideStepTrigger trigger, object args)
	{
		return trigger == WeakGuideStepTrigger.RingSolved;
	}
}
