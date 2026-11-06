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
using UniLog;
using UnityEngine;
using UnlockRing;
using Vibration;

public sealed class RingSolveSuccessService : IDisposable
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayAsync_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameObject partGo;

		public CancellationToken ct;

		public RingSolveSuccessService _003C_003E4__this;

		public Mesh sourceMesh;

		public Transform sourceTransform;

		public Transform effectParent;

		private GameObject _003CeffectInstance_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0311: Unknown result type (might be due to invalid IL or missing references)
			//IL_0316: Unknown result type (might be due to invalid IL or missing references)
			//IL_031e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Invalid comparison between Unknown and I4
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Invalid comparison between Unknown and I4
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingSolveSuccessService ringSolveSuccessService = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0082;
				}
				if (!((Object)(object)partGo == (Object)null))
				{
					PhysicColliderService.DisableAllPhysicColliders(partGo);
					if (ct.IsCancellationRequested)
					{
						ringSolveSuccessService.DestroyPart(partGo);
					}
					else
					{
						if (!((Object)(object)sourceMesh == (Object)null) && !((Object)(object)sourceTransform == (Object)null))
						{
							_003CeffectInstance_003E5__2 = null;
							goto IL_0082;
						}
						ringSolveSuccessService.DestroyPart(partGo);
					}
				}
				goto end_IL_000e;
				IL_0082:
				try
				{
					Awaiter<bool> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
						goto IL_032d;
					}
					if ((Object)(object)ringSolveSuccessService.effectPrefab == (Object)null)
					{
						ringSolveSuccessService.effectPrefab = GameApp.Res.LoadAsset<GameObject>("eff_cycle_boom", "UnlockRing_Effect");
					}
					if (ct.IsCancellationRequested)
					{
						ringSolveSuccessService.DestroyPart(partGo);
					}
					else if ((Object)(object)ringSolveSuccessService.effectPrefab == (Object)null)
					{
						ringSolveSuccessService.DestroyPart(partGo);
					}
					else
					{
						_003CeffectInstance_003E5__2 = GameApp.Pool.Spawn(ringSolveSuccessService.effectPrefab, effectParent);
						if ((Object)(object)_003CeffectInstance_003E5__2 == (Object)null)
						{
							ringSolveSuccessService.DestroyPart(partGo);
						}
						else
						{
							ringSolveSuccessService.ApplyEffectTransform(_003CeffectInstance_003E5__2.transform, sourceTransform);
							_003CeffectInstance_003E5__2.SetActive(false);
							ParticleSystem[] componentsInChildren = _003CeffectInstance_003E5__2.GetComponentsInChildren<ParticleSystem>(true);
							bool flag = false;
							float num2 = 0f;
							ParticleSystem[] array = componentsInChildren;
							foreach (ParticleSystem obj in array)
							{
								MainModule main = obj.main;
								((MainModule)(ref main)).simulationSpace = (ParticleSystemSimulationSpace)1;
								ParticleSystemRenderer component = ((Component)obj).GetComponent<ParticleSystemRenderer>();
								if ((Object)(object)component != (Object)null)
								{
									component.enableGPUInstancing = false;
									((Renderer)component).sortingOrder = 10;
								}
								ShapeModule shape = obj.shape;
								if ((int)((ShapeModule)(ref shape)).shapeType == 6)
								{
									((ShapeModule)(ref shape)).mesh = sourceMesh;
									flag = true;
								}
								obj.Stop(true, (ParticleSystemStopBehavior)0);
								obj.Clear();
								MinMaxCurve startLifetime = ((MainModule)(ref main)).startLifetime;
								float num3;
								if ((int)((MinMaxCurve)(ref startLifetime)).mode != 3)
								{
									startLifetime = ((MainModule)(ref main)).startLifetime;
									num3 = ((MinMaxCurve)(ref startLifetime)).constant;
								}
								else
								{
									startLifetime = ((MainModule)(ref main)).startLifetime;
									num3 = ((MinMaxCurve)(ref startLifetime)).constantMax;
								}
								float num4 = num3;
								num2 = Mathf.Max(num2, ((MainModule)(ref main)).duration + num4);
							}
							if (flag)
							{
								MeshRenderer[] componentsInChildren2 = partGo.GetComponentsInChildren<MeshRenderer>(true);
								for (int i = 0; i < componentsInChildren2.Length; i++)
								{
									((Renderer)componentsInChildren2[i]).enabled = false;
								}
								_003CeffectInstance_003E5__2.SetActive(true);
								PlayBreakSounds();
								array = componentsInChildren;
								for (int i = 0; i < array.Length; i++)
								{
									array[i].Play(true);
								}
								if (num2 <= 0f)
								{
									num2 = 1f;
								}
								UniTask val2 = UniTask.Delay(TimeSpan.FromSeconds(num2), false, (PlayerLoopTiming)8, ct);
								val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 0);
									_003C_003Eu__1 = val;
									((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayAsync_003Ed__15>(ref val, ref this);
									return;
								}
								goto IL_032d;
							}
							RecycleEffect(_003CeffectInstance_003E5__2);
							ringSolveSuccessService.DestroyPart(partGo);
						}
					}
					goto end_IL_0082;
					IL_032d:
					if (val.GetResult())
					{
						RecycleEffect(_003CeffectInstance_003E5__2);
						ringSolveSuccessService.DestroyPart(partGo);
					}
					else
					{
						RecycleEffect(_003CeffectInstance_003E5__2);
						ringSolveSuccessService.DestroyPart(partGo);
					}
					end_IL_0082:;
				}
				catch (Exception ex) when (!(ex is OperationCanceledException))
				{
					Log.LogError("[RingSolveSuccessService] ", "播放移除动效失败: {0}", new object[1] { ex });
					RecycleEffect(_003CeffectInstance_003E5__2);
					ringSolveSuccessService.DestroyPart(partGo);
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CeffectInstance_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CeffectInstance_003E5__2 = null;
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

	public const string EffectLocation = "eff_cycle_boom";

	public const string EffectResGroup = "UnlockRing_Effect";

	private const string TAG = "[RingSolveSuccessService] ";

	private const float EffectZOffset = -5f;

	private static int lastBreakSoundFrame = -1;

	private readonly CancellationToken cancelToken;

	private readonly Queue<RingRemoveEffectEvent> pendingBombBreaks = new Queue<RingRemoveEffectEvent>(4);

	private GameObject effectPrefab;

	private bool disposed;

	public RingSolveSuccessService(CancellationToken cancelToken)
	{
		this.cancelToken = cancelToken;
		GameApp.Event.Listen<RingRemoveEffectEvent>((Action<RingRemoveEffectEvent>)OnRingRemoveEffect);
		GameApp.Event.Listen<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			GameApp.Event.Unlisten<RingRemoveEffectEvent>((Action<RingRemoveEffectEvent>)OnRingRemoveEffect);
			GameApp.Event.Unlisten<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
			DestroyPendingBreaks();
		}
	}

	private void OnRingRemoveEffect(RingRemoveEffectEvent evt)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!disposed && !((Object)(object)evt?.PartGo == (Object)null))
		{
			evt.Handled = true;
			PhysicColliderService.DisableAllPhysicColliders(evt.PartGo);
			if (evt.IsFromBomb)
			{
				pendingBombBreaks.Enqueue(evt);
				return;
			}
			UniTaskVoid val = PlayAsync(evt.SourceMesh, evt.SourceTransform, evt.PartGo, evt.EffectParent, cancelToken);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	private void OnBombBreakTick(BombBreakTickEvent evt)
	{
		if (!disposed)
		{
			FlushPendingBreaks();
		}
	}

	private void FlushPendingBreaks()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		while (pendingBombBreaks.Count > 0)
		{
			RingRemoveEffectEvent ringRemoveEffectEvent = pendingBombBreaks.Dequeue();
			if (ringRemoveEffectEvent != null && !((Object)(object)ringRemoveEffectEvent.PartGo == (Object)null))
			{
				UniTaskVoid val = PlayAsync(ringRemoveEffectEvent.SourceMesh, ringRemoveEffectEvent.SourceTransform, ringRemoveEffectEvent.PartGo, ringRemoveEffectEvent.EffectParent, cancelToken);
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	private void DestroyPendingBreaks()
	{
		while (pendingBombBreaks.Count > 0)
		{
			DestroyPart(pendingBombBreaks.Dequeue()?.PartGo);
		}
	}

	[AsyncStateMachine(typeof(_003CPlayAsync_003Ed__15))]
	public UniTaskVoid PlayAsync(Mesh sourceMesh, Transform sourceTransform, GameObject partGo, Transform effectParent, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayAsync_003Ed__15 _003CPlayAsync_003Ed__ = default(_003CPlayAsync_003Ed__15);
		_003CPlayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayAsync_003Ed__._003C_003E4__this = this;
		_003CPlayAsync_003Ed__.sourceMesh = sourceMesh;
		_003CPlayAsync_003Ed__.sourceTransform = sourceTransform;
		_003CPlayAsync_003Ed__.partGo = partGo;
		_003CPlayAsync_003Ed__.effectParent = effectParent;
		_003CPlayAsync_003Ed__.ct = ct;
		_003CPlayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayAsync_003Ed__15>(ref _003CPlayAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void ApplyEffectTransform(Transform effectTransform, Transform sourceTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = sourceTransform.position;
		position.z += -5f;
		float z = sourceTransform.eulerAngles.z;
		effectTransform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, z));
		effectTransform.localScale = Vector3.one;
	}

	private static void PlayBreakSounds()
	{
		if (Time.frameCount != lastBreakSoundFrame)
		{
			lastBreakSoundFrame = Time.frameCount;
			AudioService.PlayUiSound(AudioKeys.CeramicRingBreak.Value);
			AudioService.PlayUiSound(AudioKeys.CeramicRingRelease.Value);
			VibrateUtility.Vibrate(1102);
		}
	}

	private static void RecycleEffect(GameObject effectInstance)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		if ((Object)(object)effectInstance == (Object)null)
		{
			return;
		}
		ParticleSystem[] componentsInChildren = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem val in componentsInChildren)
		{
			if (!((Object)(object)val == (Object)null))
			{
				val.Stop(true, (ParticleSystemStopBehavior)0);
				val.Clear();
				ShapeModule shape = val.shape;
				if ((int)((ShapeModule)(ref shape)).shapeType == 6)
				{
					((ShapeModule)(ref shape)).mesh = null;
				}
			}
		}
		if (Application.isPlaying)
		{
			GameApp.Pool.Recycle(effectInstance);
		}
		else
		{
			Object.DestroyImmediate((Object)(object)effectInstance);
		}
	}

	private void DestroyPart(GameObject partGo)
	{
		if (!((Object)(object)partGo == (Object)null) && Application.isPlaying)
		{
			Object.Destroy((Object)(object)partGo);
		}
	}
}
