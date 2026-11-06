using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Save;
using RotateRings.GamePlay.Services;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.UI.Game;
using Rotaterings.Rank;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;
using XingYun.RingGame;
using XingYun.UI.Controller;

public sealed class RingAcornCollectService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayAcornAnimationDeferred_003Ed__22 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingAcornCollectService _003C_003E4__this;

		public GameObject acornInstance;

		private CancellationTokenSource _003Ccts_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingAcornCollectService ringAcornCollectService = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					_003Ccts_003E5__2 = new CancellationTokenSource();
					ringAcornCollectService.acornAnimPlayCts[acornInstance] = _003Ccts_003E5__2;
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(Random.Range(2f, 5f)), false, (PlayerLoopTiming)8, _003Ccts_003E5__2.Token);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayAcornAnimationDeferred_003Ed__22>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult())
				{
					ringAcornCollectService.acornAnimPlayCts.Remove(acornInstance);
					_003Ccts_003E5__2.Dispose();
					if (!((Object)(object)acornInstance == (Object)null) && acornInstance.activeInHierarchy)
					{
						Animation componentInChildren = acornInstance.GetComponentInChildren<Animation>(true);
						if ((Object)(object)componentInChildren != (Object)null && !((Object)(object)componentInChildren.GetClip("anim_junei_xiangguo_idle1") == (Object)null))
						{
							componentInChildren.Play("anim_junei_xiangguo_idle1");
						}
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Ccts_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Ccts_003E5__2 = null;
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

	private const string TAG = "[RingAcornCollectService] ";

	private const string AcornIdleAnimName = "anim_junei_xiangguo_idle1";

	private GameObject acornItemPrefab;

	private string acornItemLocation;

	private readonly List<GameObject> acornItemInstances = new List<GameObject>();

	private RectTransform cachedCollectionPanel;

	private readonly Dictionary<GameObject, float> acornBaseScales = new Dictionary<GameObject, float>();

	private readonly Dictionary<GameObject, CancellationTokenSource> acornAnimPlayCts = new Dictionary<GameObject, CancellationTokenSource>();

	private readonly Dictionary<IGameplayObject, GameObject> ringToAcornInstance = new Dictionary<IGameplayObject, GameObject>();

	private readonly Queue<IGameplayObject> pendingBombCollects = new Queue<IGameplayObject>();

	public RingAcornCollectService(GameplayManager host)
		: base(host)
	{
		GameApp.Event.Listen<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
		GameApp.Event.Listen<RotateRings.GamePlay.Events.GameCameraAdaptationEndEvent>((Action<RotateRings.GamePlay.Events.GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
	}

	public void Spawn(IReadOnlyList<IGameplayObject> allRings, IReadOnlyList<IGameplayObject> collectableObjects, bool isNewGame)
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		IController val = default(IController);
		if (rankModelProxy == null || !rankModelProxy.IsRankActiveThisRound() || allRings == null || allRings.Count == 0 || !GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return;
		}
		RectTransform val2 = (cachedCollectionPanel = uIGameController.View?.CollectionPanel);
		if ((Object)(object)val2 == (Object)null)
		{
			return;
		}
		string text = RankCollectThemeResourceResolver.GetAcornItemLocation();
		if (acornItemLocation != text)
		{
			acornItemPrefab = null;
			acornItemLocation = text;
		}
		if ((Object)(object)acornItemPrefab == (Object)null)
		{
			acornItemPrefab = GameApp.Res.LoadAsset<GameObject>(text, "UnlockRing_Effect");
			if ((Object)(object)acornItemPrefab == (Object)null)
			{
				return;
			}
		}
		int collectableCount = collectableObjects?.Count ?? 0;
		int num = rankModelProxy.CalculateCollectCount(collectableCount);
		if (num <= 0)
		{
			return;
		}
		List<IGameplayObject> list = (isNewGame ? PickRandomTargets(collectableObjects, num) : RestoreTargetsFromSave(collectableObjects, num));
		if (list == null || list.Count == 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			IGameplayObject gameplayObject = list[i];
			MonoBehaviour val3 = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
			if (val3 == null || (Object)(object)val3 == (Object)null)
			{
				continue;
			}
			float innerDiameterWorld;
			float ringInnerDiameterPixels = GetRingInnerDiameterPixels(gameplayObject, val2, out innerDiameterWorld);
			_ = 0f;
			if (!WorldToUIPostionUtil.WorldToLocalPoint(gameplayObject.GetPosition(), val2, out var localPoint))
			{
				continue;
			}
			GameObject val4 = GameApp.Pool.Spawn(acornItemPrefab, (Transform)(object)val2);
			if ((Object)(object)val4 == (Object)null)
			{
				continue;
			}
			float num2 = 1f;
			if (ringInnerDiameterPixels > 0f && ringInnerDiameterPixels < 150f)
			{
				num2 = ringInnerDiameterPixels / 150f;
			}
			Transform transform = val4.transform;
			RectTransform val5 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (val5 != null)
			{
				((Transform)val5).localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
				((Transform)val5).localRotation = Quaternion.identity;
				((Transform)val5).localScale = new Vector3(num2, num2, 1f);
			}
			acornBaseScales[val4] = num2;
			Animation componentInChildren = val4.GetComponentInChildren<Animation>(true);
			if ((Object)(object)componentInChildren != (Object)null)
			{
				Transform transform2 = ((Component)componentInChildren).transform;
				RectTransform val6 = (RectTransform)(object)((transform2 is RectTransform) ? transform2 : null);
				if (val6 != null)
				{
					val6.anchoredPosition = Vector2.zero;
				}
			}
			val4.SetActive(true);
			acornItemInstances.Add(val4);
			ringToAcornInstance[gameplayObject] = val4;
			UniTaskVoid val7 = PlayAcornAnimationDeferred(val4);
			((UniTaskVoid)(ref val7)).Forget();
		}
		if (isNewGame)
		{
			SaveAcornPartIds(list);
		}
	}

	private List<IGameplayObject> PickRandomTargets(IReadOnlyList<IGameplayObject> collectableObjects, int acornCount)
	{
		if (collectableObjects == null || collectableObjects.Count == 0 || acornCount <= 0)
		{
			return null;
		}
		List<IGameplayObject> list = new List<IGameplayObject>(collectableObjects);
		int count = list.Count;
		for (int i = 0; i < acornCount && i < count; i++)
		{
			int index = Random.Range(i, count);
			IGameplayObject value = list[i];
			list[i] = list[index];
			list[index] = value;
		}
		return list.GetRange(0, Mathf.Min(acornCount, count));
	}

	private List<IGameplayObject> RestoreTargetsFromSave(IReadOnlyList<IGameplayObject> collectableObjects, int acornCount)
	{
		RotateRingInLevelSaveData rotateRingInLevelSaveData = GameApp.Proxy.Get<RingLevelSaveProxy>()?.Data;
		if (rotateRingInLevelSaveData == null || !rotateRingInLevelSaveData.HasData || rotateRingInLevelSaveData.AcornPartIds == null || rotateRingInLevelSaveData.AcornPartIds.Length == 0)
		{
			return null;
		}
		int num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		if (rotateRingInLevelSaveData.LevelNumber != num)
		{
			return null;
		}
		_ = rotateRingInLevelSaveData.AcornPartIds.Length;
		Dictionary<string, IGameplayObject> dictionary = new Dictionary<string, IGameplayObject>();
		if (collectableObjects != null)
		{
			for (int i = 0; i < collectableObjects.Count; i++)
			{
				IGameplayObject gameplayObject = collectableObjects[i];
				if (gameplayObject.TryGetPartId(out var partId))
				{
					dictionary[((PartId)(ref partId)).Value] = gameplayObject;
				}
			}
		}
		List<IGameplayObject> list = new List<IGameplayObject>(rotateRingInLevelSaveData.AcornPartIds.Length);
		for (int j = 0; j < rotateRingInLevelSaveData.AcornPartIds.Length; j++)
		{
			string text = rotateRingInLevelSaveData.AcornPartIds[j];
			if (!string.IsNullOrEmpty(text) && dictionary.TryGetValue(text, out var value) && value != null && !value.IsSolved)
			{
				list.Add(value);
			}
		}
		return list;
	}

	private void SaveAcornPartIds(List<IGameplayObject> spawnTargets)
	{
		RotateRingInLevelSaveData rotateRingInLevelSaveData = GameApp.Proxy.Get<RingLevelSaveProxy>()?.Data;
		if (rotateRingInLevelSaveData == null || spawnTargets == null)
		{
			return;
		}
		List<string> list = new List<string>(spawnTargets.Count);
		for (int i = 0; i < spawnTargets.Count; i++)
		{
			if (spawnTargets[i].TryGetPartId(out var partId))
			{
				list.Add(((PartId)(ref partId)).Value);
			}
		}
		rotateRingInLevelSaveData.AcornPartIds = list.ToArray();
	}

	public void TryCollect(IGameplayObject gameplayObject, bool isFromBomb)
	{
		if (ringToAcornInstance.TryGetValue(gameplayObject, out var value))
		{
			if (isFromBomb)
			{
				pendingBombCollects.Enqueue(gameplayObject);
			}
			else
			{
				CollectNow(gameplayObject, value);
			}
		}
	}

	private void CollectNow(IGameplayObject gameplayObject, GameObject acornInstance)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		ringToAcornInstance.Remove(gameplayObject);
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		bool flag = false;
		int collectCount = 0;
		if (rankModelProxy != null)
		{
			(flag, collectCount) = rankModelProxy.AddCurrentCollectCount();
		}
		bool hasCollectScreenPosition = false;
		Vector2 collectScreenPosition = default(Vector2);
		if (flag && (Object)(object)acornInstance != (Object)null)
		{
			Transform transform = acornInstance.transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (val != null)
			{
				Canvas componentInParent = ((Component)val).GetComponentInParent<Canvas>();
				collectScreenPosition = RectTransformUtility.WorldToScreenPoint(((Object)(object)componentInParent != (Object)null) ? componentInParent.rootCanvas.worldCamera : null, ((Transform)val).position);
				hasCollectScreenPosition = true;
			}
		}
		if ((Object)(object)acornInstance != (Object)null)
		{
			acornItemInstances.Remove(acornInstance);
			acornBaseScales.Remove(acornInstance);
			CancelAcornAnimPlayTask(acornInstance);
			GameApp.Pool.Recycle(acornInstance);
		}
		if (flag)
		{
			AudioService.PlayUiSound(AudioKeys.AcornCollect.Value);
			PartId partId = default(PartId);
			bool hasSolvedPartId = gameplayObject.TryGetPartId(out partId);
			GameApp.Event.Send<RingSolveEvent>(new RingSolveEvent
			{
				CollectCount = collectCount,
				WorldPosition = gameplayObject.GetPosition(),
				HasSolvedPartId = hasSolvedPartId,
				SolvedPartId = partId,
				HasCollectScreenPosition = hasCollectScreenPosition,
				CollectScreenPosition = collectScreenPosition
			});
		}
	}

	private void OnBombBreakTick(BombBreakTickEvent evt)
	{
		while (pendingBombCollects.Count > 0)
		{
			IGameplayObject gameplayObject = pendingBombCollects.Dequeue();
			if (gameplayObject != null && ringToAcornInstance.TryGetValue(gameplayObject, out var value))
			{
				CollectNow(gameplayObject, value);
			}
		}
	}

	public void Clear()
	{
		CancelAcornAnimPlayTasks();
		for (int i = 0; i < acornItemInstances.Count; i++)
		{
			GameObject val = acornItemInstances[i];
			if (!((Object)(object)val == (Object)null))
			{
				GameApp.Pool.Recycle(val);
			}
		}
		acornItemInstances.Clear();
		acornBaseScales.Clear();
		ringToAcornInstance.Clear();
		pendingBombCollects.Clear();
	}

	public string[] GetCurrentAcornPartIds()
	{
		List<string> list = new List<string>(ringToAcornInstance.Count);
		foreach (KeyValuePair<IGameplayObject, GameObject> item in ringToAcornInstance)
		{
			if (item.Key.TryGetPartId(out var partId))
			{
				list.Add(((PartId)(ref partId)).Value);
			}
		}
		return list.ToArray();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			GameApp.Event.Unlisten<BombBreakTickEvent>((Action<BombBreakTickEvent>)OnBombBreakTick);
			GameApp.Event.Unlisten<RotateRings.GamePlay.Events.GameCameraAdaptationEndEvent>((Action<RotateRings.GamePlay.Events.GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
			Clear();
			acornItemPrefab = null;
		}
	}

	private float GetRingInnerDiameterPixels(IGameplayObject ringObject, RectTransform parentRect, out float innerDiameterWorld)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		innerDiameterWorld = 0f;
		MonoBehaviour val = (MonoBehaviour)((ringObject is MonoBehaviour) ? ringObject : null);
		if (val == null || (Object)(object)val == (Object)null)
		{
			return 0f;
		}
		RingSegmentController componentInChildren = ((Component)val).GetComponentInChildren<RingSegmentController>(true);
		if ((Object)(object)componentInChildren == (Object)null || componentInChildren.CenterRadius <= 0.0001f)
		{
			return 0f;
		}
		float num = 2f * componentInChildren.CenterRadius - componentInChildren.BandWidth;
		Vector3 lossyScale = ((Component)val).transform.lossyScale;
		float num2 = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
		innerDiameterWorld = num * num2;
		return WorldToUIPostionUtil.WorldRadiusToLocal(ringObject.GetPosition(), innerDiameterWorld * 0.5f, parentRect) * 2f;
	}

	[AsyncStateMachine(typeof(_003CPlayAcornAnimationDeferred_003Ed__22))]
	private UniTaskVoid PlayAcornAnimationDeferred(GameObject acornInstance)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayAcornAnimationDeferred_003Ed__22 _003CPlayAcornAnimationDeferred_003Ed__ = default(_003CPlayAcornAnimationDeferred_003Ed__22);
		_003CPlayAcornAnimationDeferred_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayAcornAnimationDeferred_003Ed__._003C_003E4__this = this;
		_003CPlayAcornAnimationDeferred_003Ed__.acornInstance = acornInstance;
		_003CPlayAcornAnimationDeferred_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayAcornAnimationDeferred_003Ed__._003C_003Et__builder)).Start<_003CPlayAcornAnimationDeferred_003Ed__22>(ref _003CPlayAcornAnimationDeferred_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayAcornAnimationDeferred_003Ed__._003C_003Et__builder)).Task;
	}

	private void CancelAcornAnimPlayTask(GameObject acornInstance)
	{
		if (acornAnimPlayCts.TryGetValue(acornInstance, out var value))
		{
			value?.Cancel();
			value?.Dispose();
			acornAnimPlayCts.Remove(acornInstance);
		}
	}

	private void CancelAcornAnimPlayTasks()
	{
		foreach (KeyValuePair<GameObject, CancellationTokenSource> acornAnimPlayCt in acornAnimPlayCts)
		{
			acornAnimPlayCt.Value?.Cancel();
			acornAnimPlayCt.Value?.Dispose();
		}
		acornAnimPlayCts.Clear();
	}

	private void OnCameraAdaptationEnd(RotateRings.GamePlay.Events.GameCameraAdaptationEndEvent evt)
	{
		if (!base.IsDisposed)
		{
			RefreshAllAcorns();
		}
	}

	public void RefreshAllAcorns()
	{
		if ((Object)(object)cachedCollectionPanel == (Object)null || ringToAcornInstance.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<IGameplayObject, GameObject> item in ringToAcornInstance)
		{
			RefreshAcornPosition(item.Key, item.Value, cachedCollectionPanel);
		}
	}

	private void RefreshAcornPosition(IGameplayObject ringObject, GameObject acornInstance, RectTransform parentRect)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)acornInstance == (Object)null)
		{
			return;
		}
		MonoBehaviour val = (MonoBehaviour)((ringObject is MonoBehaviour) ? ringObject : null);
		if (val != null && !((Object)(object)val == (Object)null) && WorldToUIPostionUtil.WorldToLocalPoint(ringObject.GetPosition(), parentRect, out var localPoint))
		{
			Transform transform = acornInstance.transform;
			RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (val2 != null)
			{
				((Transform)val2).localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
				float value;
				float num = (acornBaseScales.TryGetValue(acornInstance, out value) ? value : 1f);
				float num2 = ComputeCameraScaleFactor();
				float num3 = num * num2;
				((Transform)val2).localScale = new Vector3(num3, num3, 1f);
			}
		}
	}

	private float ComputeCameraScaleFactor()
	{
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if ((Object)(object)gameplayCamera == (Object)null)
		{
			return 1f;
		}
		float num = base.Host.GetGameService<RotateCameraAdaptation>()?.CachedOrthographicSize ?? 0f;
		if (num <= 0f)
		{
			return 1f;
		}
		return num / gameplayCamera.orthographicSize;
	}
}
