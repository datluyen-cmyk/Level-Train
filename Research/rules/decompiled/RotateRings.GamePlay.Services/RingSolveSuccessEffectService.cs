using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.GamePlay.Objects;
using RotateRings.GamePlay.PartStyle;
using RotateRings.Services;
using UniLog;
using UnityEngine;
using UnlockRing;
using UnlockRing.Presentation;
using Vibration;

namespace RotateRings.GamePlay.Services;

public sealed class RingSolveSuccessEffectService : GameplayServiceBase<GameplayManager>
{
	private struct FragmentPlaybackInstance
	{
		public GameObject EffectInstance;

		public RingSolveMaterialBinder.Binding MaterialBinding;

		public ParticleSystem[] ParticleSystems;

		public float Duration;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayAsync_003Ed__31 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public GameObject partGo;

		public CancellationToken ct;

		public RingSolveSuccessEffectService _003C_003E4__this;

		public IGameplayObject sourcePart;

		public Mesh sourceMesh;

		public Transform sourceTransform;

		public Transform effectParent;

		private List<SolveFragmentSegment> _003Csegments_003E5__2;

		private List<FragmentPlaybackInstance> _003Cplaybacks_003E5__3;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0379: Unknown result type (might be due to invalid IL or missing references)
			//IL_037e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0382: Unknown result type (might be due to invalid IL or missing references)
			//IL_0387: Unknown result type (might be due to invalid IL or missing references)
			//IL_038b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0390: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingSolveSuccessEffectService ringSolveSuccessEffectService = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_005e;
				}
				if (!((Object)(object)partGo == (Object)null))
				{
					PhysicColliderService.DisableAllPhysicColliders(partGo);
					if (!ct.IsCancellationRequested)
					{
						_003Csegments_003E5__2 = GameApp.Pool.SpawnList<SolveFragmentSegment>();
						goto IL_005e;
					}
					ringSolveSuccessEffectService.DestroyPart(partGo);
				}
				goto end_IL_000e;
				IL_005e:
				try
				{
					if (num == 0)
					{
						goto IL_00bb;
					}
					CollectFragmentSegments(sourcePart, sourceMesh, sourceTransform, partGo, _003Csegments_003E5__2);
					if (_003Csegments_003E5__2.Count != 0)
					{
						_003Cplaybacks_003E5__3 = new List<FragmentPlaybackInstance>(_003Csegments_003E5__2.Count + 1);
						goto IL_00bb;
					}
					ringSolveSuccessEffectService.DestroyPart(partGo);
					goto end_IL_005e;
					IL_00bb:
					try
					{
						Awaiter<bool> val;
						if (num == 0)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							goto IL_03dc;
						}
						int solveRingEffect = SolveRingEffectExpResolve.ReadSolveRingEffect();
						SolveRingEffectPaths paths = SolveRingEffectExpResolve.Resolve(solveRingEffect);
						if (ct.IsCancellationRequested)
						{
							ringSolveSuccessEffectService.DestroyPart(partGo);
						}
						else
						{
							bool flag = false;
							for (int i = 0; i < _003Csegments_003E5__2.Count; i++)
							{
								SolveFragmentSegment solveFragmentSegment = _003Csegments_003E5__2[i];
								if ((Object)(object)solveFragmentSegment.PartMesh == (Object)null || (Object)(object)solveFragmentSegment.PartTransform == (Object)null)
								{
									continue;
								}
								string text = ResolveSegmentEffectPath(solveFragmentSegment.EffectPrefabName, sourcePart, paths);
								bool useExplicitPrefabCache = !string.IsNullOrEmpty(solveFragmentSegment.EffectPrefabName);
								GameObject val2 = ringSolveSuccessEffectService.LoadSolveEffectPrefab(text, useExplicitPrefabCache, paths);
								if ((Object)(object)val2 == (Object)null)
								{
									continue;
								}
								bool flag2 = text == paths.FragmentPath;
								bool applyMaterialSync = flag2 && paths.RequiresStyleFragmentMaterialSync;
								GameObject materialSourceGo = (((Object)(object)solveFragmentSegment.MaterialSourceGo != (Object)null) ? solveFragmentSegment.MaterialSourceGo : partGo);
								if (!ringSolveSuccessEffectService.TryCreateFragmentPlayback(val2, solveFragmentSegment.PartMesh, solveFragmentSegment.PartTransform, effectParent, partGo, sourcePart, applyMaterialSync, out var playback, materialSourceGo))
								{
									continue;
								}
								if (flag2 && SolveRingEffect3Exp.TryResolveFragmentCount(solveRingEffect, sourcePart, out var count))
								{
									ApplyBurstParticleCount(playback.ParticleSystems, count);
								}
								_003Cplaybacks_003E5__3.Add(playback);
								if (!(!flag && flag2) || sourcePart is FixedObject)
								{
									continue;
								}
								string text2 = ringSolveSuccessEffectService.Host.PartStyle?.GetSolveFragmentAddonEffectPath();
								if (!string.IsNullOrEmpty(text2))
								{
									GameObject orLoadAddonFragmentSolveEffectPrefab = ringSolveSuccessEffectService.GetOrLoadAddonFragmentSolveEffectPrefab(text2);
									if (!((Object)(object)orLoadAddonFragmentSolveEffectPrefab == (Object)null) && ringSolveSuccessEffectService.TryCreateFragmentPlayback(orLoadAddonFragmentSolveEffectPrefab, solveFragmentSegment.PartMesh, solveFragmentSegment.PartTransform, effectParent, partGo, sourcePart, applyMaterialSync: false, out var playback2, materialSourceGo))
									{
										_003Cplaybacks_003E5__3.Add(playback2);
									}
								}
								flag = true;
							}
							if (_003Cplaybacks_003E5__3.Count != 0)
							{
								ringSolveSuccessEffectService.HidePartRenderers(partGo);
								float num2 = 0f;
								for (int j = 0; j < _003Cplaybacks_003E5__3.Count; j++)
								{
									_003Cplaybacks_003E5__3[j].EffectInstance.SetActive(true);
									num2 = Mathf.Max(num2, _003Cplaybacks_003E5__3[j].Duration);
								}
								ringSolveSuccessEffectService.PlayBreakSounds(sourcePart);
								for (int k = 0; k < _003Cplaybacks_003E5__3.Count; k++)
								{
									PlayFragmentParticleSystems(_003Cplaybacks_003E5__3[k]);
								}
								UniTask val3 = UniTask.Delay(TimeSpan.FromSeconds(num2), false, (PlayerLoopTiming)8, ct);
								val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 0);
									_003C_003Eu__1 = val;
									((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayAsync_003Ed__31>(ref val, ref this);
									return;
								}
								goto IL_03dc;
							}
							ringSolveSuccessEffectService.DestroyPart(partGo);
						}
						goto end_IL_005e;
						IL_03dc:
						val.GetResult();
						ringSolveSuccessEffectService.RecycleFragmentPlaybacks(_003Cplaybacks_003E5__3);
						_003Cplaybacks_003E5__3.Clear();
						ringSolveSuccessEffectService.DestroyPart(partGo);
					}
					catch (Exception ex) when (!(ex is OperationCanceledException))
					{
						Log.LogError("[RingSolveSuccessEffectService] ", "播放移除动效失败: {0}", new object[1] { ex });
						ringSolveSuccessEffectService.RecycleFragmentPlaybacks(_003Cplaybacks_003E5__3);
						ringSolveSuccessEffectService.DestroyPart(partGo);
					}
					_003Cplaybacks_003E5__3 = null;
					end_IL_005e:;
				}
				finally
				{
					if (num < 0)
					{
						GameApp.Pool.RecycleList<SolveFragmentSegment>(_003Csegments_003E5__2);
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Csegments_003E5__2 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Csegments_003E5__2 = null;
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
	private struct _003CPlayBreakFromResultAsync_003Ed__26 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingSolveSuccessEffectService _003C_003E4__this;

		public RingBreakResult result;

		public CancellationToken ct;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingSolveSuccessEffectService ringSolveSuccessEffectService = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0125;
				}
				if (num == 1)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_01c3;
				}
				if (!ringSolveSuccessEffectService.IsDisposed && !((Object)(object)result?.PartGo == (Object)null))
				{
					PhysicColliderService.DisableAllPhysicColliders(result.PartGo);
					if (result.SourcePart != null)
					{
						UniTask val2;
						if (result.SourcePart is FixedButterflyObject butterfly)
						{
							val2 = ringSolveSuccessEffectService.PlayButterflyAsync(butterfly, result.PartGo, result.EffectParent, ct);
							val = ((UniTask)(ref val2)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayBreakFromResultAsync_003Ed__26>(ref val, ref this);
								return;
							}
							goto IL_0125;
						}
						val2 = ringSolveSuccessEffectService.PlayAsync(result.SourceMesh, result.SourceTransform, result.PartGo, result.EffectParent, result.SourcePart, ct);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayBreakFromResultAsync_003Ed__26>(ref val, ref this);
							return;
						}
						goto IL_01c3;
					}
					if ((Object)(object)result.PartGo != (Object)null)
					{
						_ = ((Object)result.PartGo).name;
					}
					ringSolveSuccessEffectService.DestroyPart(result.PartGo);
				}
				goto end_IL_000e;
				IL_01c3:
				((Awaiter)(ref val)).GetResult();
				goto end_IL_000e;
				IL_0125:
				((Awaiter)(ref val)).GetResult();
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayButterflyAsync_003Ed__46 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public GameObject partGo;

		public CancellationToken ct;

		public RingSolveSuccessEffectService _003C_003E4__this;

		public FixedButterflyObject butterfly;

		public Transform effectParent;

		private List<GameObject> _003CeffectInstances_003E5__2;

		private List<RingSolveMaterialBinder.Binding> _003Cbindings_003E5__3;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0376: Unknown result type (might be due to invalid IL or missing references)
			//IL_037b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0383: Unknown result type (might be due to invalid IL or missing references)
			//IL_032f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0334: Unknown result type (might be due to invalid IL or missing references)
			//IL_0338: Unknown result type (might be due to invalid IL or missing references)
			//IL_033d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0341: Unknown result type (might be due to invalid IL or missing references)
			//IL_0346: Unknown result type (might be due to invalid IL or missing references)
			//IL_035b: Unknown result type (might be due to invalid IL or missing references)
			//IL_035d: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Invalid comparison between Unknown and I4
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Invalid comparison between Unknown and I4
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Invalid comparison between Unknown and I4
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingSolveSuccessEffectService ringSolveSuccessEffectService = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_00be;
				}
				FixedButterflyObject.FragmentPartInfo[] butterflyFragmentParts = default(FixedButterflyObject.FragmentPartInfo[]);
				SolveRingEffectPaths paths = default(SolveRingEffectPaths);
				bool flag = default(bool);
				float num2 = default(float);
				if (!((Object)(object)partGo == (Object)null))
				{
					PhysicColliderService.DisableAllPhysicColliders(partGo);
					if (ct.IsCancellationRequested)
					{
						ringSolveSuccessEffectService.DestroyPart(partGo);
					}
					else
					{
						butterflyFragmentParts = butterfly.GetButterflyFragmentParts();
						if (butterflyFragmentParts != null && butterflyFragmentParts.Length != 0)
						{
							paths = SolveRingEffectExpResolve.Resolve(SolveRingEffectExpResolve.ReadSolveRingEffect());
							ringSolveSuccessEffectService.HidePartRenderers(partGo);
							_003CeffectInstances_003E5__2 = new List<GameObject>(butterflyFragmentParts.Length);
							_003Cbindings_003E5__3 = new List<RingSolveMaterialBinder.Binding>(butterflyFragmentParts.Length);
							flag = false;
							num2 = 0f;
							goto IL_00be;
						}
						ringSolveSuccessEffectService.HidePartRenderers(partGo);
						ringSolveSuccessEffectService.DestroyPart(partGo);
					}
				}
				goto end_IL_000e;
				IL_00be:
				try
				{
					Awaiter<bool> val5;
					if (num != 0)
					{
						for (int i = 0; i < butterflyFragmentParts.Length; i++)
						{
							FixedButterflyObject.FragmentPartInfo fragmentPartInfo = butterflyFragmentParts[i];
							if ((Object)(object)fragmentPartInfo.PartMesh == (Object)null || (Object)(object)fragmentPartInfo.PartTransform == (Object)null)
							{
								continue;
							}
							GameObject butterflyEffectPrefab = ringSolveSuccessEffectService.GetButterflyEffectPrefab(fragmentPartInfo.IsBridge, paths);
							if ((Object)(object)butterflyEffectPrefab == (Object)null)
							{
								continue;
							}
							GameObject val = GameApp.Pool.Spawn(butterflyEffectPrefab, effectParent);
							if ((Object)(object)val == (Object)null)
							{
								continue;
							}
							ringSolveSuccessEffectService.ApplyEffectTransform(val.transform, fragmentPartInfo.PartTransform);
							val.SetActive(false);
							RingSolveMaterialBinder.Binding binding = default(RingSolveMaterialBinder.Binding);
							ParticleSystem[] componentsInChildren = val.GetComponentsInChildren<ParticleSystem>(true);
							bool flag2 = false;
							ParticleSystem[] array = componentsInChildren;
							foreach (ParticleSystem val2 in array)
							{
								MainModule main = val2.main;
								((MainModule)(ref main)).simulationSpace = (ParticleSystemSimulationSpace)1;
								ParticleSystemRenderer component = ((Component)val2).GetComponent<ParticleSystemRenderer>();
								if ((Object)(object)component != (Object)null)
								{
									component.enableGPUInstancing = false;
									((Renderer)component).sortingOrder = 10;
								}
								ShapeModule shape = val2.shape;
								if ((int)((ShapeModule)(ref shape)).shapeType == 6)
								{
									((ShapeModule)(ref shape)).mesh = fragmentPartInfo.PartMesh;
									flag2 = true;
								}
								if (fragmentPartInfo.IsBridge && fragmentPartInfo.PartLength > 0f)
								{
									ringSolveSuccessEffectService.ApplyBridgeParticleCount(val2, fragmentPartInfo.PartLength);
								}
								val2.Stop(true, (ParticleSystemStopBehavior)0);
								val2.Clear();
								MinMaxCurve val3 = ((MainModule)(ref main)).startLifetime;
								float num3;
								if ((int)((MinMaxCurve)(ref val3)).mode != 3)
								{
									val3 = ((MainModule)(ref main)).startLifetime;
									num3 = ((MinMaxCurve)(ref val3)).constant;
								}
								else
								{
									val3 = ((MainModule)(ref main)).startLifetime;
									num3 = ((MinMaxCurve)(ref val3)).constantMax;
								}
								float num4 = num3;
								val3 = ((MainModule)(ref main)).startDelay;
								float num5;
								if ((int)((MinMaxCurve)(ref val3)).mode != 3)
								{
									val3 = ((MainModule)(ref main)).startDelay;
									num5 = ((MinMaxCurve)(ref val3)).constant;
								}
								else
								{
									val3 = ((MainModule)(ref main)).startDelay;
									num5 = ((MinMaxCurve)(ref val3)).constantMax;
								}
								float num6 = num5;
								num2 = Mathf.Max(num2, num6 + num4);
							}
							if (!flag2)
							{
								ringSolveSuccessEffectService.RecycleEffect(val, binding);
								continue;
							}
							val.SetActive(true);
							if (!flag)
							{
								ringSolveSuccessEffectService.PlayBreakSounds(butterfly);
								flag = true;
							}
							array = componentsInChildren;
							for (int j = 0; j < array.Length; j++)
							{
								array[j].Play(true);
							}
							_003CeffectInstances_003E5__2.Add(val);
							_003Cbindings_003E5__3.Add(binding);
						}
						if (num2 <= 0f)
						{
							num2 = 1f;
						}
						UniTask val4 = UniTask.Delay(TimeSpan.FromSeconds(num2), false, (PlayerLoopTiming)8, ct);
						val5 = ((UniTask)(ref val4)).SuppressCancellationThrow().GetAwaiter();
						if (!val5.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val5;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayButterflyAsync_003Ed__46>(ref val5, ref this);
							return;
						}
					}
					else
					{
						val5 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					val5.GetResult();
					for (int k = 0; k < _003CeffectInstances_003E5__2.Count; k++)
					{
						ringSolveSuccessEffectService.RecycleEffect(_003CeffectInstances_003E5__2[k], _003Cbindings_003E5__3[k]);
					}
					ringSolveSuccessEffectService.DestroyPart(partGo);
				}
				catch (Exception ex) when (!(ex is OperationCanceledException))
				{
					Log.LogError("[RingSolveSuccessEffectService] ", "蝴蝶夹分区碎裂失败: {0}", new object[1] { ex });
					for (int l = 0; l < _003CeffectInstances_003E5__2.Count; l++)
					{
						ringSolveSuccessEffectService.RecycleEffect(_003CeffectInstances_003E5__2[l], _003Cbindings_003E5__3[l]);
					}
					ringSolveSuccessEffectService.DestroyPart(partGo);
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CeffectInstances_003E5__2 = null;
				_003Cbindings_003E5__3 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CeffectInstances_003E5__2 = null;
			_003Cbindings_003E5__3 = null;
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

	public const string EffectResGroup = "UnlockRing_Effect";

	private const string TAG = "[RingSolveSuccessEffectService] ";

	private const float EffectZOffset = -5f;

	private static int lastRingBreakSoundFrame = -1;

	private static int lastFixedBreakSoundFrame = -1;

	private const float BridgeBaseLength = 0.5f;

	private const int MinBridgeParticleCount = 2;

	private readonly Queue<RingRemoveEffectEvent> pendingBombBreaks = new Queue<RingRemoveEffectEvent>(4);

	private readonly RingSolveMaterialSync materialSync = new RingSolveMaterialSync();

	private readonly RingSolveMaterialPool materialPool = new RingSolveMaterialPool();

	private readonly RingSolveMaterialBinder materialBinder;

	private GameObject basicFixedSolveEffectPrefab;

	private GameObject fixedSolveEffectPrefab;

	private GameObject fixedBridgeSolveEffectPrefab;

	private GameObject fragmentSolveEffectPrefab;

	private GameObject addonFragmentSolveEffectPrefab;

	private string loadedBasicFixedSolveEffectPath;

	private string loadedFixedSolveEffectPath;

	private string loadedFixedBridgeSolveEffectPath;

	private string loadedFragmentSolveEffectPath;

	private string loadedAddonFragmentSolveEffectPath;

	private readonly Dictionary<string, GameObject> overrideFragmentPrefabs = new Dictionary<string, GameObject>(4);

	private readonly Dictionary<string, string> overrideFragmentLoadedPaths = new Dictionary<string, string>(4);

	public RingSolveSuccessEffectService(GameplayManager host)
		: base(host)
	{
		materialBinder = new RingSolveMaterialBinder(materialSync, materialPool);
		GameApp.Event.Listen<RingRemoveEffectEvent>((Action<RingRemoveEffectEvent>)OnRingRemoveEffect);
		GameApp.Event.Listen<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			GameApp.Event.Unlisten<RingRemoveEffectEvent>((Action<RingRemoveEffectEvent>)OnRingRemoveEffect);
			GameApp.Event.Unlisten<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
			DestroyPendingBreaks();
			materialBinder.ReleaseAllActive();
			materialPool.Dispose();
			ReleaseSolveEffectPrefabs();
		}
	}

	private void OnRingRemoveEffect(RingRemoveEffectEvent evt)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsDisposed && !((Object)(object)evt?.PartGo == (Object)null) && !PresentationMode.UseTaskPresentation)
		{
			evt.Handled = true;
			PhysicColliderService.DisableAllPhysicColliders(evt.PartGo);
			if (evt.IsFromBomb)
			{
				pendingBombBreaks.Enqueue(evt);
			}
			else if (!ShouldPlayBreakEffect(evt))
			{
				DestroyPart(evt.PartGo);
			}
			else if (evt.SourcePart is FixedButterflyObject butterfly)
			{
				UniTaskExtensions.Forget(PlayButterflyAsync(butterfly, evt.PartGo, evt.EffectParent, base.DestroyToken));
			}
			else
			{
				UniTaskExtensions.Forget(PlayAsync(evt.SourceMesh, evt.SourceTransform, evt.PartGo, evt.EffectParent, evt.SourcePart, base.DestroyToken));
			}
		}
	}

	[AsyncStateMachine(typeof(_003CPlayBreakFromResultAsync_003Ed__26))]
	public UniTask PlayBreakFromResultAsync(RingBreakResult result, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayBreakFromResultAsync_003Ed__26 _003CPlayBreakFromResultAsync_003Ed__ = default(_003CPlayBreakFromResultAsync_003Ed__26);
		_003CPlayBreakFromResultAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayBreakFromResultAsync_003Ed__._003C_003E4__this = this;
		_003CPlayBreakFromResultAsync_003Ed__.result = result;
		_003CPlayBreakFromResultAsync_003Ed__.ct = ct;
		_003CPlayBreakFromResultAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayBreakFromResultAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayBreakFromResultAsync_003Ed__26>(ref _003CPlayBreakFromResultAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayBreakFromResultAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool ShouldPlayBreakEffect(RingRemoveEffectEvent evt)
	{
		return evt?.SourcePart != null;
	}

	private void OnBombBreakTick(BombBreakTickEvent evt)
	{
		if (!base.IsDisposed)
		{
			FlushPendingBreaks();
		}
	}

	private void FlushPendingBreaks()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		while (pendingBombBreaks.Count > 0)
		{
			RingRemoveEffectEvent ringRemoveEffectEvent = pendingBombBreaks.Dequeue();
			if (ringRemoveEffectEvent == null || (Object)(object)ringRemoveEffectEvent.PartGo == (Object)null)
			{
				continue;
			}
			if (ShouldPlayBreakEffect(ringRemoveEffectEvent))
			{
				if (ringRemoveEffectEvent.SourcePart is FixedButterflyObject butterfly)
				{
					UniTaskExtensions.Forget(PlayButterflyAsync(butterfly, ringRemoveEffectEvent.PartGo, ringRemoveEffectEvent.EffectParent, base.DestroyToken));
				}
				else
				{
					UniTaskExtensions.Forget(PlayAsync(ringRemoveEffectEvent.SourceMesh, ringRemoveEffectEvent.SourceTransform, ringRemoveEffectEvent.PartGo, ringRemoveEffectEvent.EffectParent, ringRemoveEffectEvent.SourcePart, base.DestroyToken));
				}
			}
			else
			{
				HidePartRenderers(ringRemoveEffectEvent.PartGo);
				DestroyPart(ringRemoveEffectEvent.PartGo);
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

	[AsyncStateMachine(typeof(_003CPlayAsync_003Ed__31))]
	public UniTask PlayAsync(Mesh sourceMesh, Transform sourceTransform, GameObject partGo, Transform effectParent, IGameplayObject sourcePart, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayAsync_003Ed__31 _003CPlayAsync_003Ed__ = default(_003CPlayAsync_003Ed__31);
		_003CPlayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayAsync_003Ed__._003C_003E4__this = this;
		_003CPlayAsync_003Ed__.sourceMesh = sourceMesh;
		_003CPlayAsync_003Ed__.sourceTransform = sourceTransform;
		_003CPlayAsync_003Ed__.partGo = partGo;
		_003CPlayAsync_003Ed__.effectParent = effectParent;
		_003CPlayAsync_003Ed__.sourcePart = sourcePart;
		_003CPlayAsync_003Ed__.ct = ct;
		_003CPlayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayAsync_003Ed__31>(ref _003CPlayAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool TryCreateFragmentPlayback(GameObject effectPrefab, Mesh sourceMesh, Transform sourceTransform, Transform effectParent, GameObject partGo, IGameplayObject sourcePart, bool applyMaterialSync, out FragmentPlaybackInstance playback, GameObject materialSourceGo = null)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Invalid comparison between Unknown and I4
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Invalid comparison between Unknown and I4
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Invalid comparison between Unknown and I4
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		playback = default(FragmentPlaybackInstance);
		GameObject val = GameApp.Pool.Spawn(effectPrefab, effectParent);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		ApplyEffectTransform(val.transform, sourceTransform);
		val.SetActive(false);
		RingSolveMaterialBinder.Binding binding = default(RingSolveMaterialBinder.Binding);
		if (applyMaterialSync)
		{
			RingPartStyleService partStyle = base.Host.PartStyle;
			materialBinder.TryApply(val, partGo, sourcePart, partStyle?.GetSolveFragmentMainTexture(), partStyle?.GetSolveFragmentColorIntensityOverride(), partStyle?.GetSolveFragmentBlendToWhiteOverride(), out binding, materialSourceGo);
		}
		ParticleSystem[] componentsInChildren = val.GetComponentsInChildren<ParticleSystem>(true);
		bool flag = false;
		float num = 0f;
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
			MinMaxCurve val2 = ((MainModule)(ref main)).startLifetime;
			float num2;
			if ((int)((MinMaxCurve)(ref val2)).mode != 3)
			{
				val2 = ((MainModule)(ref main)).startLifetime;
				num2 = ((MinMaxCurve)(ref val2)).constant;
			}
			else
			{
				val2 = ((MainModule)(ref main)).startLifetime;
				num2 = ((MinMaxCurve)(ref val2)).constantMax;
			}
			float num3 = num2;
			val2 = ((MainModule)(ref main)).startDelay;
			float num4;
			if ((int)((MinMaxCurve)(ref val2)).mode != 3)
			{
				val2 = ((MainModule)(ref main)).startDelay;
				num4 = ((MinMaxCurve)(ref val2)).constant;
			}
			else
			{
				val2 = ((MainModule)(ref main)).startDelay;
				num4 = ((MinMaxCurve)(ref val2)).constantMax;
			}
			float num5 = num4;
			num = Mathf.Max(num, num5 + num3);
		}
		if (!flag)
		{
			RecycleEffect(val, binding);
			return false;
		}
		playback = new FragmentPlaybackInstance
		{
			EffectInstance = val,
			MaterialBinding = binding,
			ParticleSystems = componentsInChildren,
			Duration = ((num <= 0f) ? 1f : num)
		};
		return true;
	}

	private static void CollectFragmentSegments(IGameplayObject sourcePart, Mesh sourceMesh, Transform sourceTransform, GameObject partGo, List<SolveFragmentSegment> buffer)
	{
		buffer.Clear();
		sourcePart?.CollectSolveFragmentSegments(buffer);
		if (buffer.Count <= 0 && !((Object)(object)sourceMesh == (Object)null) && !((Object)(object)sourceTransform == (Object)null))
		{
			buffer.Add(new SolveFragmentSegment(sourceTransform, sourceMesh, partGo));
		}
	}

	private static string ResolveSegmentEffectPath(string effectPrefabName, IGameplayObject sourcePart, SolveRingEffectPaths paths)
	{
		if (!string.IsNullOrEmpty(effectPrefabName))
		{
			return effectPrefabName;
		}
		if (!(sourcePart is FixedObject))
		{
			return paths.FragmentPath;
		}
		return paths.BasicFixedPath;
	}

	private GameObject LoadSolveEffectPrefab(string resolvedPath, bool useExplicitPrefabCache, SolveRingEffectPaths paths)
	{
		if (string.IsNullOrEmpty(resolvedPath))
		{
			return null;
		}
		if (useExplicitPrefabCache)
		{
			return GetOrLoadOverrideFragmentPrefab(resolvedPath);
		}
		if (resolvedPath == paths.BasicFixedPath)
		{
			return GetOrLoadSolveEffectPrefab(resolvedPath, ref basicFixedSolveEffectPrefab, ref loadedBasicFixedSolveEffectPath);
		}
		if (resolvedPath == paths.FragmentPath)
		{
			return GetOrLoadSolveEffectPrefab(resolvedPath, ref fragmentSolveEffectPrefab, ref loadedFragmentSolveEffectPath);
		}
		return GetOrLoadOverrideFragmentPrefab(resolvedPath);
	}

	private GameObject GetOrLoadOverrideFragmentPrefab(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		if (overrideFragmentPrefabs.TryGetValue(path, out var value) && (Object)(object)value != (Object)null && overrideFragmentLoadedPaths.TryGetValue(path, out var value2) && value2 == path)
		{
			return value;
		}
		ReleaseOverrideFragmentPrefab(path);
		GameObject val = GameApp.Res.LoadAsset<GameObject>(path, "UnlockRing_Effect");
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		overrideFragmentPrefabs[path] = val;
		overrideFragmentLoadedPaths[path] = path;
		return val;
	}

	private void ReleaseOverrideFragmentPrefab(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (overrideFragmentPrefabs.TryGetValue(path, out var value) && (Object)(object)value != (Object)null && overrideFragmentLoadedPaths.TryGetValue(path, out var value2))
			{
				GameApp.Res.UnloadAsset(value2, "UnlockRing_Effect", 1);
			}
			overrideFragmentPrefabs.Remove(path);
			overrideFragmentLoadedPaths.Remove(path);
		}
	}

	private void ReleaseOverrideFragmentPrefabs()
	{
		foreach (KeyValuePair<string, string> overrideFragmentLoadedPath in overrideFragmentLoadedPaths)
		{
			if (!string.IsNullOrEmpty(overrideFragmentLoadedPath.Value))
			{
				GameApp.Res.UnloadAsset(overrideFragmentLoadedPath.Value, "UnlockRing_Effect", 1);
			}
		}
		overrideFragmentPrefabs.Clear();
		overrideFragmentLoadedPaths.Clear();
	}

	private static void PlayFragmentParticleSystems(FragmentPlaybackInstance playback)
	{
		ParticleSystem[] particleSystems = playback.ParticleSystems;
		if (particleSystems == null)
		{
			return;
		}
		RingSolveMaterialBinder.Binding materialBinding = playback.MaterialBinding;
		foreach (ParticleSystem val in particleSystems)
		{
			if (!materialBinding.Applied || !((Object)(object)materialBinding.ShadowSystem != (Object)null) || !((Object)(object)val == (Object)(object)materialBinding.ShadowSystem) || materialBinding.ShadowEnabled)
			{
				val.Play(true);
			}
		}
	}

	private void RecycleFragmentPlaybacks(List<FragmentPlaybackInstance> playbacks)
	{
		for (int i = 0; i < playbacks.Count; i++)
		{
			FragmentPlaybackInstance fragmentPlaybackInstance = playbacks[i];
			RecycleEffect(fragmentPlaybackInstance.EffectInstance, fragmentPlaybackInstance.MaterialBinding);
		}
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

	private void PlayFixedBreakSound()
	{
		if (Time.frameCount != lastFixedBreakSoundFrame)
		{
			lastFixedBreakSoundFrame = Time.frameCount;
			AudioService.PlayUiSound(AudioKeys.FixedClampBreak.Value);
		}
	}

	private void PlayRingBreakSound()
	{
		if (Time.frameCount != lastRingBreakSoundFrame)
		{
			lastRingBreakSoundFrame = Time.frameCount;
			if (base.Host.PartStyle != null)
			{
				base.Host.PartStyle.PlaySolveBreakSound();
				return;
			}
			AudioService.PlayUiSound(AudioKeys.CeramicRingBreak.Value);
			AudioService.PlayUiSound(AudioKeys.CeramicRingRelease.Value);
			VibrateUtility.Vibrate(1102);
		}
	}

	private void PlayBreakSounds(IGameplayObject sourcePart)
	{
		if (sourcePart is FixedObject)
		{
			PlayFixedBreakSound();
		}
		else
		{
			PlayRingBreakSound();
		}
	}

	[AsyncStateMachine(typeof(_003CPlayButterflyAsync_003Ed__46))]
	public UniTask PlayButterflyAsync(FixedButterflyObject butterfly, GameObject partGo, Transform effectParent, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayButterflyAsync_003Ed__46 _003CPlayButterflyAsync_003Ed__ = default(_003CPlayButterflyAsync_003Ed__46);
		_003CPlayButterflyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayButterflyAsync_003Ed__._003C_003E4__this = this;
		_003CPlayButterflyAsync_003Ed__.butterfly = butterfly;
		_003CPlayButterflyAsync_003Ed__.partGo = partGo;
		_003CPlayButterflyAsync_003Ed__.effectParent = effectParent;
		_003CPlayButterflyAsync_003Ed__.ct = ct;
		_003CPlayButterflyAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayButterflyAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayButterflyAsync_003Ed__46>(ref _003CPlayButterflyAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayButterflyAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void ApplyBridgeParticleCount(ParticleSystem ps, float bridgeLength)
	{
		float num = bridgeLength / 0.5f;
		int particleCount = Mathf.Max(2, Mathf.RoundToInt(2f * num));
		ApplyBurstParticleCount(ps, particleCount);
	}

	private static void ApplyBurstParticleCount(ParticleSystem[] particleSystems, int particleCount)
	{
		if (particleSystems != null)
		{
			for (int i = 0; i < particleSystems.Length; i++)
			{
				ApplyBurstParticleCount(particleSystems[i], particleCount);
			}
		}
	}

	private static void ApplyBurstParticleCount(ParticleSystem ps, int particleCount)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ps == (Object)null)
		{
			return;
		}
		EmissionModule emission = ps.emission;
		int burstCount = ((EmissionModule)(ref emission)).burstCount;
		if (burstCount != 0)
		{
			MainModule main = ps.main;
			if (particleCount > ((MainModule)(ref main)).maxParticles)
			{
				((MainModule)(ref main)).maxParticles = particleCount;
			}
			for (int i = 0; i < burstCount; i++)
			{
				Burst burst = ((EmissionModule)(ref emission)).GetBurst(i);
				((Burst)(ref burst)).count = MinMaxCurve.op_Implicit((float)particleCount);
				((EmissionModule)(ref emission)).SetBurst(i, burst);
			}
		}
	}

	private GameObject GetButterflyEffectPrefab(bool isBridge, SolveRingEffectPaths paths)
	{
		if (isBridge)
		{
			return GetOrLoadSolveEffectPrefab(paths.FixedBridgePath, ref fixedBridgeSolveEffectPrefab, ref loadedFixedBridgeSolveEffectPath);
		}
		return GetOrLoadSolveEffectPrefab(paths.FixedPath, ref fixedSolveEffectPrefab, ref loadedFixedSolveEffectPath);
	}

	private GameObject GetOrLoadSolveEffectPrefab(string path, ref GameObject prefab, ref string loadedPath)
	{
		if ((Object)(object)prefab != (Object)null && loadedPath == path)
		{
			return prefab;
		}
		ReleaseSolveEffectPrefab(ref prefab, ref loadedPath);
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		prefab = GameApp.Res.LoadAsset<GameObject>(path, "UnlockRing_Effect");
		if ((Object)(object)prefab != (Object)null)
		{
			loadedPath = path;
		}
		return prefab;
	}

	private GameObject GetOrLoadAddonFragmentSolveEffectPrefab(string path)
	{
		return GetOrLoadSolveEffectPrefab(path, ref addonFragmentSolveEffectPrefab, ref loadedAddonFragmentSolveEffectPath);
	}

	private static void ReleaseSolveEffectPrefab(ref GameObject prefab, ref string loadedPath)
	{
		if ((Object)(object)prefab == (Object)null || string.IsNullOrEmpty(loadedPath))
		{
			prefab = null;
			loadedPath = null;
		}
		else
		{
			GameApp.Res.UnloadAsset(loadedPath, "UnlockRing_Effect", 1);
			prefab = null;
			loadedPath = null;
		}
	}

	private void ReleaseSolveEffectPrefabs()
	{
		ReleaseSolveEffectPrefab(ref basicFixedSolveEffectPrefab, ref loadedBasicFixedSolveEffectPath);
		ReleaseSolveEffectPrefab(ref fixedSolveEffectPrefab, ref loadedFixedSolveEffectPath);
		ReleaseSolveEffectPrefab(ref fixedBridgeSolveEffectPrefab, ref loadedFixedBridgeSolveEffectPath);
		ReleaseSolveEffectPrefab(ref fragmentSolveEffectPrefab, ref loadedFragmentSolveEffectPath);
		ReleaseSolveEffectPrefab(ref addonFragmentSolveEffectPrefab, ref loadedAddonFragmentSolveEffectPath);
		ReleaseOverrideFragmentPrefabs();
	}

	private void RecycleEffect(GameObject effectInstance, RingSolveMaterialBinder.Binding materialBinding)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		using (new ProfilerScope("RingSolveSuccessEffectService.RecycleEffect"))
		{
			if ((Object)(object)effectInstance == (Object)null)
			{
				return;
			}
			materialBinder.Release(materialBinding);
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
	}

	private void HidePartRenderers(GameObject partGo)
	{
		if (!((Object)(object)partGo == (Object)null))
		{
			MeshRenderer[] componentsInChildren = partGo.GetComponentsInChildren<MeshRenderer>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Renderer)componentsInChildren[i]).enabled = false;
			}
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
