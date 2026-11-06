using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.GamePlay.Services.CharacterExpression;
using RotateRings.Services;
using RotateRings.UI.Game;
using Spine;
using Spine.Unity;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Experiment;

namespace UnlockRing;

public sealed class GameplayComboEffectPlayer : IDisposable
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayAsync_003Ed__62 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameplayOperationEvaluation evaluation;

		public GameplayComboEffectPlayer _003C_003E4__this;

		public CancellationToken ct;

		public int epochAtRequest;

		public Vector3 worldPosition;

		public int unbelievableCombo;

		private string _003CeffectLocation_003E5__2;

		private GameObject _003Cinstance_003E5__3;

		private Awaiter<GameObject> _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameplayComboEffectPlayer gameplayComboEffectPlayer = _003C_003E4__this;
			try
			{
				if ((uint)num > 1u)
				{
					_003CeffectLocation_003E5__2 = GetEffectLocation(evaluation);
					_003Cinstance_003E5__3 = null;
				}
				try
				{
					Awaiter<bool> val;
					GameObject value;
					Awaiter<GameObject> val2;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__2;
							_003C_003Eu__2 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							goto IL_025e;
						}
						if (gameplayComboEffectPlayer.effectPrefabs.TryGetValue(_003CeffectLocation_003E5__2, out value))
						{
							goto IL_00df;
						}
						val2 = GameApp.Res.LoadAssetAsync<GameObject>(_003CeffectLocation_003E5__2, "UnlockRing_Effect").GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPlayAsync_003Ed__62>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
					}
					value = val2.GetResult();
					if (!((Object)(object)value == (Object)null))
					{
						gameplayComboEffectPlayer.effectPrefabs[_003CeffectLocation_003E5__2] = value;
						goto IL_00df;
					}
					goto end_IL_002b;
					IL_00df:
					if (!ct.IsCancellationRequested && !gameplayComboEffectPlayer.disposed && gameplayComboEffectPlayer.IsPlaybackEpochValid(epochAtRequest) && UIGameRingRectUiClamp.TryResolveEncouragementMountParent(out var mountParent))
					{
						_003Cinstance_003E5__3 = GameApp.Pool.Spawn(value, (Transform)(object)mountParent);
						if (!((Object)(object)_003Cinstance_003E5__3 == (Object)null))
						{
							gameplayComboEffectPlayer.TrackActiveInstance(_003Cinstance_003E5__3);
							DisableEffectRaycast(_003Cinstance_003E5__3);
							if (TrySetUiPosition(_003Cinstance_003E5__3.transform, worldPosition, mountParent, value.transform.localScale, evaluation))
							{
								_003Cinstance_003E5__3.transform.SetAsLastSibling();
								if (evaluation == GameplayOperationEvaluation.UnbelievableMulti)
								{
									TryApplyUnbelievableComboNum(_003Cinstance_003E5__3, unbelievableCombo);
								}
								RestartEffectVisuals(_003Cinstance_003E5__3, value);
								if (gameplayComboEffectPlayer.IsPlaybackEpochValid(epochAtRequest))
								{
									_003Cinstance_003E5__3.SetActive(true);
									CharacterExpressionEffectPlayer.BringActiveInstancesToFront(mountParent);
									UniTask val3 = UniTask.Delay(TimeSpan.FromSeconds(gameplayComboEffectPlayer.GetOrCacheDuration(_003CeffectLocation_003E5__2, _003Cinstance_003E5__3)), false, (PlayerLoopTiming)8, ct);
									val = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
									if (!val.IsCompleted)
									{
										num = (_003C_003E1__state = 1);
										_003C_003Eu__2 = val;
										((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayAsync_003Ed__62>(ref val, ref this);
										return;
									}
									goto IL_025e;
								}
							}
						}
					}
					goto end_IL_002b;
					IL_025e:
					val.GetResult();
					end_IL_002b:;
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception ex2)
				{
					Log.LogError("[UnlockRing][GameplayComboEffectPlayer] ", "评价特效播放失败 evaluation={0} error={1}", new object[2] { evaluation, ex2 });
				}
				finally
				{
					if (num < 0)
					{
						gameplayComboEffectPlayer.RecycleActiveInstance(_003Cinstance_003E5__3);
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CeffectLocation_003E5__2 = null;
				_003Cinstance_003E5__3 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CeffectLocation_003E5__2 = null;
			_003Cinstance_003E5__3 = null;
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
	private struct _003CPlayPendingEvaluationEffectAsync_003Ed__60 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameplayComboEffectPlayer _003C_003E4__this;

		public int epochAtRequest;

		public CancellationToken ct;

		private GameplayOperationEvaluation _003Cevaluation_003E5__2;

		private int _003CunbelievableCombo_003E5__3;

		private Vector3 _003CcenterPosition_003E5__4;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameplayComboEffectPlayer gameplayComboEffectPlayer = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00dd;
				}
				_003Cevaluation_003E5__2 = pendingEffectEvaluation;
				_003CunbelievableCombo_003E5__3 = pendingEffectCombo;
				_003CcenterPosition_003E5__4 = GetPendingEffectCenterPosition();
				isEffectPending = false;
				if (gameplayComboEffectPlayer.IsPlaybackEpochValid(epochAtRequest))
				{
					if (UIGameRingRectUiClamp.TryResolveEncouragementMountParent(out var mountParent))
					{
						Vector2 effectHalfSize = GetEffectHalfSize(_003Cevaluation_003E5__2);
						UIGameEncouragementLayout.TryRegisterEncouragementLayoutFromWorldAnchor(mountParent, _003CcenterPosition_003E5__4, effectHalfSize, reserveUpperEmojiCompanionSpace: false);
					}
					UniTask val2 = UniTask.Yield((PlayerLoopTiming)13, ct);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayPendingEvaluationEffectAsync_003Ed__60>(ref val, ref this);
						return;
					}
					goto IL_00dd;
				}
				goto end_IL_000e;
				IL_00dd:
				if (!val.GetResult() && !gameplayComboEffectPlayer.disposed && gameplayComboEffectPlayer.IsPlaybackEpochValid(epochAtRequest))
				{
					UniTaskVoid val3 = gameplayComboEffectPlayer.PlayAsync(_003Cevaluation_003E5__2, _003CcenterPosition_003E5__4, _003CunbelievableCombo_003E5__3, epochAtRequest, ct);
					((UniTaskVoid)(ref val3)).Forget();
				}
				end_IL_000e:;
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
	private struct _003CPlayPendingEvaluationSoundAsync_003Ed__73 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CancellationToken ct;

		public GameplayComboEffectPlayer _003C_003E4__this;

		public int epochAtRequest;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameplayComboEffectPlayer gameplayComboEffectPlayer = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Yield((PlayerLoopTiming)13, ct);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayPendingEvaluationSoundAsync_003Ed__73>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (val2.GetResult() || !gameplayComboEffectPlayer.IsPlaybackEpochValid(epochAtRequest))
				{
					isSoundPending = false;
				}
				else
				{
					GameplayOperationEvaluation pendingSoundEvaluation = GameplayComboEffectPlayer.pendingSoundEvaluation;
					isSoundPending = false;
					AudioService.PlayUiSound(GetEvaluationAudioKey(pendingSoundEvaluation).Value);
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

	private const string TAG = "[UnlockRing][GameplayComboEffectPlayer] ";

	public const string EffectResGroup = "UnlockRing_Effect";

	private const string GreatEffect = "eff_great_combo_01";

	private const string ExcellentEffect = "eff_excellent_combo_02";

	private const string AmazingEffect = "eff_amazing_combo_03";

	private const string PerfectEffect = "eff_fantastic_combo_04";

	private const string UnbelievableEffect = "eff_Incredible_combo_05";

	private const string UnbelievableMultiEffect = "eff_Incredible_combo_06";

	private const string GreatGp5Effect = "eff_great_combo_01_gp5";

	private const string ExcellentGp5Effect = "eff_excellent_combo_02_gp5";

	private const string AmazingGp5Effect = "eff_amazing_combo_03_gp5";

	private const string PerfectGp5Effect = "eff_fantastic_combo_04_gp5";

	private const string UnbelievableGp5Effect = "eff_Incredible_combo_05_gp5";

	private const string UnbelievableMultiGp5Effect = "eff_Incredible_combo_06_gp5";

	private const string ComboNumNodeName = "x1/combo_num";

	private const float EffectDurationFallback = 2f;

	private const float EffectZOffset = -0.02f;

	private const float EffectDisplayYOffset = -80f;

	private static readonly Vector2 GreatEffectHalfSize = new Vector2(400f, 160f) * 0.5f;

	private static readonly Vector2 ExcellentEffectHalfSize = new Vector2(560f, 160f) * 0.5f;

	private static readonly Vector2 AmazingEffectHalfSize = new Vector2(560f, 160f) * 0.5f;

	private static readonly Vector2 PerfectEffectHalfSize = new Vector2(598f, 182f) * 0.5f;

	private static readonly Vector2 UnbelievableEffectHalfSize = new Vector2(624f, 206f) * 0.5f;

	private static readonly Vector2 UnbelievableMultiEffectHalfSize = new Vector2(748f, 232f) * 0.5f;

	private static GameplayOperationEvaluation pendingSoundEvaluation;

	private static bool isSoundPending;

	private static readonly AudioKey SweetVoGreat = new AudioKey("rr_vo_sweet_1_great");

	private static readonly AudioKey SweetVoExcellent = new AudioKey("rr_vo_sweet_2_excellent");

	private static readonly AudioKey SweetVoAmazing = new AudioKey("rr_vo_sweet_3_amazing");

	private static readonly AudioKey SweetVoPerfect = new AudioKey("rr_vo_sweet_4_fantastic");

	private static readonly AudioKey SweetVoUnbelievable = new AudioKey("rr_vo_sweet_5_incredible_01");

	private static readonly AudioKey SweetVoUnbelievableMulti1 = new AudioKey("rr_vo_sweet_5_incredible_02");

	private static readonly AudioKey SweetVoUnbelievableMulti2 = new AudioKey("rr_vo_sweet_5_incredible_03");

	private static readonly AudioKey CartoonVoGreat = new AudioKey("rr_vo_cartoon_1_great");

	private static readonly AudioKey CartoonVoExcellent = new AudioKey("rr_vo_cartoon_2_excellent");

	private static readonly AudioKey CartoonVoAmazing = new AudioKey("rr_vo_cartoon_3_amazing");

	private static readonly AudioKey CartoonVoPerfect = new AudioKey("rr_vo_cartoon_4_fantastic");

	private static readonly AudioKey CartoonVoUnbelievable = new AudioKey("rr_vo_cartoon_5_incredible_01");

	private static readonly AudioKey CartoonVoUnbelievableMulti1 = new AudioKey("rr_vo_cartoon_5_incredible_02");

	private static readonly AudioKey CartoonVoUnbelievableMulti2 = new AudioKey("rr_vo_cartoon_5_incredible_03");

	private static bool nextUnbelievableMultiVoiceIsFirst = true;

	private static GameplayOperationEvaluation pendingEffectEvaluation;

	private static int pendingEffectCombo;

	private static readonly List<Vector3> pendingEffectPositions = new List<Vector3>();

	private static bool isEffectPending;

	private readonly CancellationToken cancellationToken;

	private readonly Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>();

	private readonly Dictionary<string, float> effectDurations = new Dictionary<string, float>();

	private readonly List<GameObject> activeInstances = new List<GameObject>(4);

	private bool disposed;

	private bool playbackGateOpen = true;

	private int playbackEpoch;

	private int unbelievableStreak;

	public GameplayComboEffectPlayer(CancellationToken cancellationToken)
	{
		this.cancellationToken = cancellationToken;
	}

	public void Dispose()
	{
		StopAndRecycleAllActiveEffects();
		disposed = true;
		effectPrefabs.Clear();
		effectDurations.Clear();
	}

	public void StopAndRecycleAllActiveEffects()
	{
		playbackGateOpen = false;
		playbackEpoch++;
		ClearPendingEvaluationPlaybackState();
		for (int num = activeInstances.Count - 1; num >= 0; num--)
		{
			GameObject val = activeInstances[num];
			if ((Object)(object)val != (Object)null)
			{
				GameApp.Pool.Recycle(val);
			}
		}
		activeInstances.Clear();
	}

	public void OpenPlaybackGate()
	{
		playbackGateOpen = true;
	}

	public void ClearUnbelievableStreak()
	{
		unbelievableStreak = 0;
	}

	public void PlayStep(GameplayOperationEvaluation evaluation, Vector3 worldPosition)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (playbackGateOpen && !disposed && !cancellationToken.IsCancellationRequested)
		{
			if (evaluation == GameplayOperationEvaluation.Unbelievable && unbelievableStreak > 0)
			{
				evaluation = GameplayOperationEvaluation.UnbelievableMulti;
			}
			int unbelievableCombo = 0;
			if (IsUnbelievableFamily(evaluation))
			{
				unbelievableStreak++;
				unbelievableCombo = unbelievableStreak;
			}
			else
			{
				unbelievableStreak = 0;
			}
			if (evaluation != GameplayOperationEvaluation.None)
			{
				RequestPlayEvaluationSound(evaluation);
				RequestPlayEvaluationEffect(evaluation, worldPosition, unbelievableCombo);
			}
		}
	}

	private void RequestPlayEvaluationEffect(GameplayOperationEvaluation evaluation, Vector3 worldPosition, int unbelievableCombo)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!playbackGateOpen || disposed)
		{
			return;
		}
		int epochAtRequest = playbackEpoch;
		if (!isEffectPending)
		{
			pendingEffectEvaluation = evaluation;
			pendingEffectCombo = unbelievableCombo;
			pendingEffectPositions.Clear();
			pendingEffectPositions.Add(worldPosition);
			isEffectPending = true;
			UniTaskVoid val = PlayPendingEvaluationEffectAsync(epochAtRequest, cancellationToken);
			((UniTaskVoid)(ref val)).Forget();
		}
		else
		{
			pendingEffectPositions.Add(worldPosition);
			if (GetEvaluationPriority(evaluation) > GetEvaluationPriority(pendingEffectEvaluation))
			{
				pendingEffectEvaluation = evaluation;
				pendingEffectCombo = unbelievableCombo;
			}
			else if (IsUnbelievableFamily(evaluation) && IsUnbelievableFamily(pendingEffectEvaluation) && unbelievableCombo > pendingEffectCombo)
			{
				pendingEffectCombo = unbelievableCombo;
			}
		}
	}

	[AsyncStateMachine(typeof(_003CPlayPendingEvaluationEffectAsync_003Ed__60))]
	private UniTaskVoid PlayPendingEvaluationEffectAsync(int epochAtRequest, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayPendingEvaluationEffectAsync_003Ed__60 _003CPlayPendingEvaluationEffectAsync_003Ed__ = default(_003CPlayPendingEvaluationEffectAsync_003Ed__60);
		_003CPlayPendingEvaluationEffectAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayPendingEvaluationEffectAsync_003Ed__._003C_003E4__this = this;
		_003CPlayPendingEvaluationEffectAsync_003Ed__.epochAtRequest = epochAtRequest;
		_003CPlayPendingEvaluationEffectAsync_003Ed__.ct = ct;
		_003CPlayPendingEvaluationEffectAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayPendingEvaluationEffectAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayPendingEvaluationEffectAsync_003Ed__60>(ref _003CPlayPendingEvaluationEffectAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayPendingEvaluationEffectAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static Vector3 GetPendingEffectCenterPosition()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (pendingEffectPositions.Count == 0)
		{
			return Vector3.zero;
		}
		Vector3 val = Vector3.zero;
		for (int i = 0; i < pendingEffectPositions.Count; i++)
		{
			val += pendingEffectPositions[i];
		}
		return val / (float)pendingEffectPositions.Count;
	}

	[AsyncStateMachine(typeof(_003CPlayAsync_003Ed__62))]
	private UniTaskVoid PlayAsync(GameplayOperationEvaluation evaluation, Vector3 worldPosition, int unbelievableCombo, int epochAtRequest, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayAsync_003Ed__62 _003CPlayAsync_003Ed__ = default(_003CPlayAsync_003Ed__62);
		_003CPlayAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayAsync_003Ed__._003C_003E4__this = this;
		_003CPlayAsync_003Ed__.evaluation = evaluation;
		_003CPlayAsync_003Ed__.worldPosition = worldPosition;
		_003CPlayAsync_003Ed__.unbelievableCombo = unbelievableCombo;
		_003CPlayAsync_003Ed__.epochAtRequest = epochAtRequest;
		_003CPlayAsync_003Ed__.ct = ct;
		_003CPlayAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayAsync_003Ed__62>(ref _003CPlayAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private bool IsPlaybackEpochValid(int epochAtRequest)
	{
		return epochAtRequest == playbackEpoch;
	}

	private static void ClearPendingEvaluationPlaybackState()
	{
		isEffectPending = false;
		isSoundPending = false;
		pendingEffectPositions.Clear();
		pendingEffectCombo = 0;
	}

	private void TrackActiveInstance(GameObject instance)
	{
		PruneActiveInstances();
		if ((Object)(object)instance != (Object)null && !activeInstances.Contains(instance))
		{
			activeInstances.Add(instance);
		}
	}

	private void RecycleActiveInstance(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null) && activeInstances.Remove(instance))
		{
			GameApp.Pool.Recycle(instance);
		}
	}

	private void PruneActiveInstances()
	{
		for (int num = activeInstances.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)activeInstances[num] == (Object)null)
			{
				activeInstances.RemoveAt(num);
			}
		}
	}

	private static void DisableEffectRaycast(GameObject instance)
	{
		CanvasGroup val = default(CanvasGroup);
		if (!instance.TryGetComponent<CanvasGroup>(ref val))
		{
			val = instance.AddComponent<CanvasGroup>();
		}
		val.blocksRaycasts = false;
	}

	private static void TryApplyUnbelievableComboNum(GameObject instance, int unbelievableCombo)
	{
		if ((Object)(object)instance == (Object)null || unbelievableCombo <= 0)
		{
			return;
		}
		Transform val = instance.transform.Find("x1/combo_num");
		if (!((Object)(object)val == (Object)null))
		{
			string text = unbelievableCombo.ToString();
			TextMeshProUGUI val2 = default(TextMeshProUGUI);
			Text val3 = default(Text);
			if (((Component)val).TryGetComponent<TextMeshProUGUI>(ref val2))
			{
				((TMP_Text)val2).text = text;
			}
			else if (((Component)val).TryGetComponent<Text>(ref val3))
			{
				val3.text = text;
			}
		}
	}

	private static bool TrySetUiPosition(Transform effectTransform, Vector3 worldPosition, RectTransform uiParent, Vector3 prefabScale, GameplayOperationEvaluation evaluation)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)(object)((effectTransform is RectTransform) ? effectTransform : null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		Vector2 effectHalfSize = GetEffectHalfSize(evaluation);
		if (!UIGameEncouragementLayout.TryResolvePlacedEncouragement(worldPosition, uiParent, out var centerLocal, out var _) && !UIGameEncouragementLayout.TryComputeEncouragementCenter(worldPosition, uiParent, effectHalfSize, reserveUpperEmojiCompanionSpace: false, out centerLocal))
		{
			return false;
		}
		((Transform)val).localPosition = new Vector3(centerLocal.x, centerLocal.y, -0.02f);
		((Transform)val).localRotation = Quaternion.identity;
		((Transform)val).localScale = prefabScale;
		return true;
	}

	private static Vector2 GetEffectHalfSize(GameplayOperationEvaluation evaluation)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		return (Vector2)(evaluation switch
		{
			GameplayOperationEvaluation.Great => GreatEffectHalfSize, 
			GameplayOperationEvaluation.Excellent => ExcellentEffectHalfSize, 
			GameplayOperationEvaluation.Amazing => AmazingEffectHalfSize, 
			GameplayOperationEvaluation.Perfect => PerfectEffectHalfSize, 
			GameplayOperationEvaluation.Unbelievable => UnbelievableEffectHalfSize, 
			GameplayOperationEvaluation.UnbelievableMulti => UnbelievableMultiEffectHalfSize, 
			_ => GreatEffectHalfSize, 
		});
	}

	private void RequestPlayEvaluationSound(GameplayOperationEvaluation evaluation)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (playbackGateOpen && !disposed)
		{
			int epochAtRequest = playbackEpoch;
			if (!isSoundPending)
			{
				pendingSoundEvaluation = evaluation;
				isSoundPending = true;
				UniTaskVoid val = PlayPendingEvaluationSoundAsync(epochAtRequest, cancellationToken);
				((UniTaskVoid)(ref val)).Forget();
			}
			else if (GetEvaluationPriority(evaluation) > GetEvaluationPriority(pendingSoundEvaluation))
			{
				pendingSoundEvaluation = evaluation;
			}
		}
	}

	[AsyncStateMachine(typeof(_003CPlayPendingEvaluationSoundAsync_003Ed__73))]
	private UniTaskVoid PlayPendingEvaluationSoundAsync(int epochAtRequest, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayPendingEvaluationSoundAsync_003Ed__73 _003CPlayPendingEvaluationSoundAsync_003Ed__ = default(_003CPlayPendingEvaluationSoundAsync_003Ed__73);
		_003CPlayPendingEvaluationSoundAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayPendingEvaluationSoundAsync_003Ed__._003C_003E4__this = this;
		_003CPlayPendingEvaluationSoundAsync_003Ed__.epochAtRequest = epochAtRequest;
		_003CPlayPendingEvaluationSoundAsync_003Ed__.ct = ct;
		_003CPlayPendingEvaluationSoundAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayPendingEvaluationSoundAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayPendingEvaluationSoundAsync_003Ed__73>(ref _003CPlayPendingEvaluationSoundAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayPendingEvaluationSoundAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static bool IsUnbelievableFamily(GameplayOperationEvaluation evaluation)
	{
		if (evaluation != GameplayOperationEvaluation.Unbelievable)
		{
			return evaluation == GameplayOperationEvaluation.UnbelievableMulti;
		}
		return true;
	}

	private static int GetEvaluationPriority(GameplayOperationEvaluation evaluation)
	{
		return evaluation switch
		{
			GameplayOperationEvaluation.UnbelievableMulti => 6, 
			GameplayOperationEvaluation.Unbelievable => 5, 
			GameplayOperationEvaluation.Perfect => 4, 
			GameplayOperationEvaluation.Amazing => 3, 
			GameplayOperationEvaluation.Excellent => 2, 
			GameplayOperationEvaluation.Great => 1, 
			_ => 0, 
		};
	}

	private static AudioKey GetEvaluationAudioKey(GameplayOperationEvaluation evaluation)
	{
		return GetOperationScoreSoundType() switch
		{
			1 => GetSweetEvaluationAudioKey(evaluation), 
			2 => GetCartoonEvaluationAudioKey(evaluation), 
			_ => GetDefaultEvaluationAudioKey(evaluation), 
		};
	}

	private static int GetOperationScoreSoundType()
	{
		return FunctionDataCenter.Get<OperationScoreFunction>()?.OperationScoreSound ?? 0;
	}

	private static AudioKey GetDefaultEvaluationAudioKey(GameplayOperationEvaluation evaluation)
	{
		switch (evaluation)
		{
		case GameplayOperationEvaluation.UnbelievableMulti:
		{
			AudioKey result = (nextUnbelievableMultiVoiceIsFirst ? AudioKeys.VoUnbelievableMulti1 : AudioKeys.VoUnbelievableMulti2);
			nextUnbelievableMultiVoiceIsFirst = !nextUnbelievableMultiVoiceIsFirst;
			return result;
		}
		case GameplayOperationEvaluation.Unbelievable:
			return AudioKeys.VoUnbelievable;
		case GameplayOperationEvaluation.Perfect:
			return AudioKeys.VoPerfect;
		case GameplayOperationEvaluation.Amazing:
			return AudioKeys.VoAmazing;
		case GameplayOperationEvaluation.Excellent:
			return AudioKeys.VoExcellent;
		default:
			return AudioKeys.VoGreat;
		}
	}

	private static AudioKey GetSweetEvaluationAudioKey(GameplayOperationEvaluation evaluation)
	{
		switch (evaluation)
		{
		case GameplayOperationEvaluation.UnbelievableMulti:
		{
			AudioKey result = (nextUnbelievableMultiVoiceIsFirst ? SweetVoUnbelievableMulti1 : SweetVoUnbelievableMulti2);
			nextUnbelievableMultiVoiceIsFirst = !nextUnbelievableMultiVoiceIsFirst;
			return result;
		}
		case GameplayOperationEvaluation.Unbelievable:
			return SweetVoUnbelievable;
		case GameplayOperationEvaluation.Perfect:
			return SweetVoPerfect;
		case GameplayOperationEvaluation.Amazing:
			return SweetVoAmazing;
		case GameplayOperationEvaluation.Excellent:
			return SweetVoExcellent;
		default:
			return SweetVoGreat;
		}
	}

	private static AudioKey GetCartoonEvaluationAudioKey(GameplayOperationEvaluation evaluation)
	{
		switch (evaluation)
		{
		case GameplayOperationEvaluation.UnbelievableMulti:
		{
			AudioKey result = (nextUnbelievableMultiVoiceIsFirst ? CartoonVoUnbelievableMulti1 : CartoonVoUnbelievableMulti2);
			nextUnbelievableMultiVoiceIsFirst = !nextUnbelievableMultiVoiceIsFirst;
			return result;
		}
		case GameplayOperationEvaluation.Unbelievable:
			return CartoonVoUnbelievable;
		case GameplayOperationEvaluation.Perfect:
			return CartoonVoPerfect;
		case GameplayOperationEvaluation.Amazing:
			return CartoonVoAmazing;
		case GameplayOperationEvaluation.Excellent:
			return CartoonVoExcellent;
		default:
			return CartoonVoGreat;
		}
	}

	private static string GetEffectLocation(GameplayOperationEvaluation evaluation)
	{
		if (GetOperationEvaluationUiType() == 1)
		{
			return GetGp5EffectLocation(evaluation);
		}
		return evaluation switch
		{
			GameplayOperationEvaluation.Excellent => "eff_excellent_combo_02", 
			GameplayOperationEvaluation.Amazing => "eff_amazing_combo_03", 
			GameplayOperationEvaluation.Perfect => "eff_fantastic_combo_04", 
			GameplayOperationEvaluation.Unbelievable => "eff_Incredible_combo_05", 
			GameplayOperationEvaluation.UnbelievableMulti => "eff_Incredible_combo_06", 
			_ => "eff_great_combo_01", 
		};
	}

	private static int GetOperationEvaluationUiType()
	{
		return FunctionDataCenter.Get<OperationScoreFunction>()?.OperationEvaluationUIType ?? 0;
	}

	private static string GetGp5EffectLocation(GameplayOperationEvaluation evaluation)
	{
		return evaluation switch
		{
			GameplayOperationEvaluation.Excellent => "eff_excellent_combo_02_gp5", 
			GameplayOperationEvaluation.Amazing => "eff_amazing_combo_03_gp5", 
			GameplayOperationEvaluation.Perfect => "eff_fantastic_combo_04_gp5", 
			GameplayOperationEvaluation.Unbelievable => "eff_Incredible_combo_05_gp5", 
			GameplayOperationEvaluation.UnbelievableMulti => "eff_Incredible_combo_06_gp5", 
			_ => "eff_great_combo_01_gp5", 
		};
	}

	private static void RestartEffectVisuals(GameObject instance, GameObject prefabReference)
	{
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem obj in componentsInChildren)
		{
			obj.Simulate(0f, false, true);
			obj.Play(true);
		}
		SkeletonGraphic val = (((Object)(object)prefabReference != (Object)null) ? prefabReference.GetComponentInChildren<SkeletonGraphic>(true) : null);
		string text = (((Object)(object)val != (Object)null) ? val.startingAnimation : string.Empty);
		bool flag = (Object)(object)val != (Object)null && val.startingLoop;
		SkeletonGraphic[] componentsInChildren2 = instance.GetComponentsInChildren<SkeletonGraphic>(true);
		foreach (SkeletonGraphic val2 in componentsInChildren2)
		{
			try
			{
				string text2 = val2.startingAnimation;
				bool flag2 = val2.startingLoop;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = text;
					flag2 = flag;
				}
				if (!string.IsNullOrEmpty(text2))
				{
					val2.startingLoop = flag2;
					val2.startingAnimation = string.Empty;
					val2.AnimationState.ClearTracks();
					val2.AnimationState.SetAnimation(0, text2, flag2);
				}
			}
			catch (Exception)
			{
			}
		}
	}

	private float GetOrCacheDuration(string effectLocation, GameObject instance)
	{
		if (effectDurations.TryGetValue(effectLocation, out var value))
		{
			return value;
		}
		value = GetDuration(instance);
		effectDurations[effectLocation] = value;
		return value;
	}

	private static float GetDuration(GameObject instance)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		ParticleSystem[] componentsInChildren = instance.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MainModule main = componentsInChildren[i].main;
			MinMaxCurve startLifetime = ((MainModule)(ref main)).startLifetime;
			float num2;
			if ((int)((MinMaxCurve)(ref startLifetime)).mode != 3)
			{
				startLifetime = ((MainModule)(ref main)).startLifetime;
				num2 = ((MinMaxCurve)(ref startLifetime)).constant;
			}
			else
			{
				startLifetime = ((MainModule)(ref main)).startLifetime;
				num2 = ((MinMaxCurve)(ref startLifetime)).constantMax;
			}
			float num3 = num2;
			num = Mathf.Max(num, ((MainModule)(ref main)).duration + num3);
		}
		SkeletonGraphic componentInChildren = instance.GetComponentInChildren<SkeletonGraphic>(true);
		if ((Object)(object)componentInChildren != (Object)null)
		{
			TrackEntry current = componentInChildren.AnimationState.GetCurrent(0);
			if (((current != null) ? current.Animation : null) != null && current.Animation.Duration > 0f)
			{
				num = Mathf.Max(num, current.Animation.Duration);
			}
		}
		if (!(num > 0f))
		{
			return 2f;
		}
		return num;
	}
}
