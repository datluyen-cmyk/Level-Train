using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Input;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Queries;
using UnlockRing.Core.Runtime;
using UnlockRing.Game.Gameplay;
using XingYun.Core.Singletons;

[RequireComponent(typeof(CoreBoardRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SimpleGestureInput : MonoBehaviour, IGameplayPointerHandler
{
	private const string TAG = "[SimpleGestureInput] ";

	private CoreBoardRenderer _board;

	private BoxCollider2D _collider;

	private bool _isPointerActive;

	private Vector2 _pointerDownScreen;

	private Vector2 _gestureStartWorld;

	private Vector2 _lastPointerWorld;

	private Vector2 _lastPointerScreen;

	private double _gestureUnclampedAmount;

	private double _lastPointerAngle;

	private long _gesturePreviewGeneration;

	private CancellationTokenSource _gesturePreviewCts;

	private Task _gesturePreviewTask;

	private bool _hasQueuedGesturePreview;

	private double _queuedGesturePreviewAmount;

	private bool _domainsReady;

	private CancellationTokenSource _domainCts;

	private long _domainGeneration;

	private IBoardGestureOverride GestureOverride => RingGameManager.Instance?.BoardGestureOverride;

	private void Awake()
	{
		_board = ((Component)this).GetComponent<CoreBoardRenderer>();
		_collider = ((Component)this).GetComponent<BoxCollider2D>();
		if ((Object)(object)_collider == (Object)null)
		{
			_collider = ((Component)this).gameObject.AddComponent<BoxCollider2D>();
		}
		((Component)this).gameObject.layer = 6;
	}

	private void OnDisable()
	{
		Singleton<GameInputManager>.Instance?.DisableGameplayInput();
		CancelDomainBuild();
		CancelPreview();
		RingGameManager.Instance?.DragBlockEffectService?.ResetDragBlockTracking();
		_board.ClearGesture();
		_isPointerActive = false;
	}

	private void Start()
	{
		UpdateColliderBounds();
		EnableBoardInput();
	}

	private void OnDestroy()
	{
		Singleton<GameInputManager>.Instance?.DisableGameplayInput();
	}

	public void ResetPointerCapture()
	{
		_isPointerActive = false;
		_board.SetPartOperateHighlight(_board.ActivePart, highlight: false);
		_board.ActivePart = null;
		_board.ActiveDomain = null;
		Singleton<GameInputManager>.Instance?.ClearGameplayCapture();
	}

	private bool TryGestureOverride(BoardGesturePhase phase, Vector2 screen, Vector2 world, PartId? activePart, out BoardGestureOverrideResult result)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		IBoardGestureOverride gestureOverride = GestureOverride;
		if (gestureOverride == null)
		{
			result = default(BoardGestureOverrideResult);
			return false;
		}
		return phase switch
		{
			BoardGesturePhase.Begin => gestureOverride.TryOverrideBegin(_board, screen, out result), 
			BoardGesturePhase.Update => gestureOverride.TryOverrideUpdate(_board, screen, world, activePart, out result), 
			_ => gestureOverride.TryOverrideEnd(_board, screen, world, activePart, out result), 
		};
	}

	public void RequestDomainRebuild()
	{
		BeginDomainBuild();
	}

	private void EnableBoardInput()
	{
		Camera val = (((Object)(object)_board != (Object)null) ? _board.Cam : null);
		if (!((Object)(object)val == (Object)null))
		{
			Singleton<GameInputManager>.Instance.EnsureInitialized();
			Singleton<GameInputManager>.Instance.EnableGameplayInput(val);
		}
	}

	private void UpdateColliderBounds()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_collider == (Object)null) && !((Object)(object)_board == (Object)null) && !((Object)(object)_board.Cam == (Object)null))
		{
			Camera cam = _board.Cam;
			float orthographicSize = cam.orthographicSize;
			float num = ((Screen.width > 0 && Screen.height > 0) ? ((float)Screen.width / (float)Screen.height) : 1.7777778f);
			float num2 = orthographicSize * num;
			_collider.size = new Vector2(num2 * 2f, orthographicSize * 2f);
			Vector3 position = ((Component)cam).transform.position;
			Vector3 val = ((Component)this).transform.InverseTransformPoint(position);
			((Collider2D)_collider).offset = new Vector2(val.x, val.y);
		}
	}

	public bool CanReceivePointer()
	{
		if (_board.Controller != null)
		{
			return !_isPointerActive;
		}
		return false;
	}

	public bool IsPointerCaptured()
	{
		return true;
	}

	public void OnPointerDown(GameplayPointerData data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		BeginPointer(data.ScreenPosition);
	}

	public void OnPointerDrag(GameplayPointerData data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		UpdatePointer(data.ScreenPosition);
	}

	public void OnPointerUp(GameplayPointerData data)
	{
		EndPointer();
	}

	private void BeginPointer(Vector2 screen)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		CoreGameController controller = _board.Controller;
		if (controller == null || _isPointerActive)
		{
			return;
		}
		_pointerDownScreen = screen;
		Vector2 val = _board.ScreenToWorld(screen);
		if (TryGestureOverride(BoardGesturePhase.Begin, screen, val, null, out var result))
		{
			if (result.InitPointerTracking)
			{
				InitPointerTracking(screen, val);
			}
			return;
		}
		PartId? activePart = _board.HitTestWorld(val);
		if (!_domainsReady && _domainCts != null)
		{
			_board.RaiseStatus("完整试玩操作域仍在准备中，请稍候…");
			if (_isPointerActive)
			{
				ClearPointer();
			}
			return;
		}
		if (!activePart.HasValue)
		{
			_board.ActivePart = null;
			_board.RaiseSelection("未选择部件");
			return;
		}
		GameApp.Event.Send<BoardPartPointerDownEvent>(new BoardPartPointerDownEvent(BoardPartPointerDownKind.Gesture, activePart.Value, screen, val));
		_board.ActivePart = activePart;
		InitPointerTracking(screen, val);
		_board.GestureAmount = 0.0;
		_gestureUnclampedAmount = 0.0;
		_board.GestureCommitAmount = 0.0;
		CoreBoardRenderer board = _board;
		PartId value = activePart.Value;
		board.RaiseSelection(((PartId)(ref value)).Value + " · " + controller.DescribeOperations(activePart.Value));
		if (!_domainsReady)
		{
			BeginDomainBuild();
		}
		BeginCachedPointerGesture();
	}

	private void InitPointerTracking(Vector2 screen, Vector2 world)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_gestureStartWorld = world;
		_lastPointerWorld = world;
		_lastPointerScreen = screen;
		_isPointerActive = true;
	}

	private void BeginCachedPointerGesture()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Invalid comparison between Unknown and I4
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		CoreGameController controller = _board.Controller;
		if (controller == null || !_board.ActivePart.HasValue || !_isPointerActive)
		{
			return;
		}
		int num = 0;
		foreach (AvailableOperation operation in controller.GetOperations(_board.ActivePart.Value))
		{
			if ((int)operation.Availability == 0 && operation.MotionDomain != null && ((int)operation.Kind == 1 || (int)operation.Kind == 0))
			{
				num++;
			}
		}
		switch (num)
		{
		case 0:
		{
			IBoardGestureOverride gestureOverride = GestureOverride;
			if (gestureOverride == null || !gestureOverride.SuppressBlockedFeedback)
			{
				_board.RaiseStatus("无法操作 · " + controller.DescribeOperations(_board.ActivePart.Value));
				_board.PlayBlockedShake(_board.ActivePart.Value);
			}
			ClearPointer();
			break;
		}
		case 1:
		{
			Vector2D val = default(Vector2D);
			((Vector2D)(ref val))._002Ector((double)_gestureStartWorld.x, (double)_gestureStartWorld.y);
			if (!controller.TryResolveGestureIntent(_board.ActivePart.Value, val, val, 0.0, out var kind, out var reason) || !TryActivateGesture(kind, out reason))
			{
				_board.RaiseStatus("无法操作 · " + reason);
				ClearPointer();
			}
			break;
		}
		default:
			_board.RaiseStatus("双操作待选择 · 沿导轨拖动可平移，绕中心切向拖动可旋转");
			break;
		}
	}

	private void UpdatePointer(Vector2 screen)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (!_isPointerActive)
		{
			return;
		}
		Vector2 val = (_lastPointerWorld = _board.ScreenToWorld(screen));
		_lastPointerScreen = screen;
		if (TryGestureOverride(BoardGesturePhase.Update, screen, val, _board.ActivePart, out var _))
		{
			return;
		}
		CoreGameController controller = _board.Controller;
		if (controller == null || !_board.ActivePart.HasValue)
		{
			return;
		}
		if (_board.ActiveDomain == null)
		{
			double minimumDistance = 6.0 / (double)Math.Max(_board.PixelsPerWorld(), 1E-06f);
			if (!controller.TryResolveGestureIntent(_board.ActivePart.Value, new Vector2D((double)_gestureStartWorld.x, (double)_gestureStartWorld.y), new Vector2D((double)val.x, (double)val.y), minimumDistance, out var kind, out var _))
			{
				return;
			}
			if (!TryActivateGesture(kind, out var reason2))
			{
				_board.RaiseStatus("无法操作 · " + reason2);
				ClearPointer();
				return;
			}
		}
		GestureMotionDomain activeDomain = _board.ActiveDomain;
		double amount;
		if ((int)activeDomain.MotionKind == 0)
		{
			Vector2D value = activeDomain.RotationPivot.Value;
			double num = Math.Atan2((double)val.y - ((Vector2D)(ref value)).Y, (double)val.x - ((Vector2D)(ref value)).X);
			double num2 = WrapDegrees((num - _lastPointerAngle) * 180.0 / Math.PI);
			_gestureUnclampedAmount += num2;
			_lastPointerAngle = num;
			amount = _gestureUnclampedAmount;
		}
		else
		{
			Vector2D value2 = activeDomain.TranslationWorldAxis.Value;
			Vector2 val2 = val - _gestureStartWorld;
			amount = (_gestureUnclampedAmount = (double)val2.x * ((Vector2D)(ref value2)).X + (double)val2.y * ((Vector2D)(ref value2)).Y);
		}
		double amount2 = OperationDomainRefiner.ClampLiveGestureAmount(activeDomain, amount);
		QueueGesturePreview(amount2);
	}

	private async void EndPointer()
	{
		CoreGameController controller = _board.Controller;
		Task gesturePreviewTask = _gesturePreviewTask;
		if (gesturePreviewTask != null)
		{
			try
			{
				await gesturePreviewTask;
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex2)
			{
				_board.RaiseStatus("实时姿态计算失败 · " + ex2.Message);
			}
		}
		if (TryGestureOverride(BoardGesturePhase.End, _lastPointerScreen, _lastPointerWorld, _board.ActivePart, out var _))
		{
			ClearPointer();
		}
		else if (controller == null || _board.ActiveDomain == null)
		{
			if (_isPointerActive && !(GestureOverride?.SuppressBlockedFeedback ?? false))
			{
				_board.RaiseStatus("手势取消 · 尚未形成明确的旋转或平移意图");
			}
			ClearPointer();
		}
		else
		{
			CommitGestureAndFinalize(_board.ActiveDomain, _board.GestureCommitAmount, "手势取消 · 状态未改变");
		}
	}

	private bool TryActivateGesture(AvailableOperationKind kind, out string reason)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		reason = "无法启动手势";
		CoreGameController controller = _board.Controller;
		if (controller == null || !_board.ActivePart.HasValue)
		{
			return false;
		}
		if (!controller.TryBeginGesture(_board.ActivePart.Value, kind, out GestureMotionDomain domain, out reason) || domain == null)
		{
			if (reason == null)
			{
				reason = "无法启动手势";
			}
			return false;
		}
		_board.ActiveDomain = domain;
		if ((int)domain.MotionKind == 0)
		{
			Vector2D value = domain.RotationPivot.Value;
			_lastPointerAngle = Math.Atan2((double)_gestureStartWorld.y - ((Vector2D)(ref value)).Y, (double)_gestureStartWorld.x - ((Vector2D)(ref value)).X);
		}
		CoreBoardRenderer board = _board;
		string[] array = new string[5];
		PartId value2 = _board.ActivePart.Value;
		array[0] = ((PartId)(ref value2)).Value;
		array[1] = " · ";
		array[2] = DescribeDomain(domain);
		array[3] = " · ";
		array[4] = controller.DescribeOperations(_board.ActivePart.Value);
		board.RaiseSelection(string.Concat(array));
		_board.RaiseStatus(((int)domain.MotionKind == 0) ? "已选择旋转 · 拖动期间由 Core 连续预览可达姿态" : "已选择平移 · 拖动期间由 Core 连续预览可达姿态");
		_board.SetPartOperateHighlight(_board.ActivePart, highlight: true);
		return true;
	}

	private void ClearPointer()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (_isPointerActive && _board.ActivePart.HasValue)
		{
			GameApp.Event.Send<BoardPartPointerUpEvent>(new BoardPartPointerUpEvent(_board.ActivePart.Value, _lastPointerScreen, _lastPointerWorld));
		}
		_gesturePreviewGeneration++;
		_gesturePreviewCts?.Cancel();
		_gesturePreviewCts?.Dispose();
		_gesturePreviewCts = null;
		_gesturePreviewTask = null;
		_hasQueuedGesturePreview = false;
		RingGameManager.Instance?.DragBlockEffectService?.ResetDragBlockTracking();
		_isPointerActive = false;
		_board.SetPartOperateHighlight(_board.ActivePart, highlight: false);
		_board.ActivePart = null;
		_board.ActiveDomain = null;
		_board.GestureAmount = 0.0;
		_gestureUnclampedAmount = 0.0;
		_board.GestureCommitAmount = 0.0;
	}

	private void CancelPreview()
	{
		_gesturePreviewGeneration++;
		_gesturePreviewCts?.Cancel();
		_gesturePreviewCts?.Dispose();
		_gesturePreviewCts = null;
		_gesturePreviewTask = null;
	}

	private void CommitGestureAndFinalize(GestureMotionDomain domain, double amount, string cancelMessage)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		CoreGameController controller = _board.Controller;
		if (controller == null)
		{
			ClearPointer();
			return;
		}
		_ = domain.PartId;
		MotionResult val = controller.CommitPreviewedGesture(domain, amount);
		_board.RaiseStatus((val == null) ? cancelMessage : DescribeResult(val));
		if (val != null)
		{
			_board.SyncChangedParts(val.Delta.ChangedParts);
			_board.RemoveParts(val.Delta.ReleasedParts);
			_board.RemoveParts(val.Delta.RemovedParts);
		}
		_board.RaiseVictory(_board.IsSolved);
		ClearPointer();
		BeginDomainBuild();
	}

	private void QueueGesturePreview(double amount)
	{
		if (_board.Controller != null && _board.ActiveDomain != null && _isPointerActive)
		{
			_queuedGesturePreviewAmount = amount;
			_hasQueuedGesturePreview = true;
			Task gesturePreviewTask = _gesturePreviewTask;
			if (gesturePreviewTask == null || gesturePreviewTask.IsCompleted)
			{
				_gesturePreviewCts?.Dispose();
				_gesturePreviewCts = new CancellationTokenSource();
				_gesturePreviewTask = PumpGesturePreviews(++_gesturePreviewGeneration, _gesturePreviewCts.Token);
			}
		}
	}

	private async Task PumpGesturePreviews(long generation, CancellationToken ct)
	{
		while (_hasQueuedGesturePreview)
		{
			ct.ThrowIfCancellationRequested();
			CoreGameController controller = _board.Controller;
			if (controller == null || _board.ActiveDomain == null || !_isPointerActive)
			{
				break;
			}
			GestureMotionDomain domain = _board.ActiveDomain;
			double amount = _queuedGesturePreviewAmount;
			_hasQueuedGesturePreview = false;
			MotionPreview val = await controller.PreviewGestureAsync(domain, amount, ct);
			if (ct.IsCancellationRequested || generation != _gesturePreviewGeneration || controller != _board.Controller || _board.ActiveDomain != domain || !_isPointerActive)
			{
				break;
			}
			if (val != null && val.CanApply)
			{
				double actualAmount = val.Result.ActualAmount;
				_board.GestureAmount = (((int)domain.MotionKind == 1) ? CopySignLocal(Math.Abs(actualAmount), amount) : actualAmount);
				_board.GestureCommitAmount = amount;
				RingGameManager.Instance?.DragBlockEffectService?.OnDragBlockPreview(_board, domain, val, amount, GestureOverride?.SuppressBlockedFeedback ?? false);
				if (val.Result.Delta.RemovedParts.Count > 0)
				{
					CommitGestureAndFinalize(domain, amount, "炸弹接触已证明，但正式提交失败");
					break;
				}
			}
		}
	}

	public void BeginDomainBuild()
	{
		CoreGameController controller = _board.Controller;
		if (controller != null)
		{
			UpdateColliderBounds();
			CancelDomainBuild();
			long generation = ++_domainGeneration;
			CancellationTokenSource cancellationTokenSource = (_domainCts = new CancellationTokenSource());
			_domainsReady = false;
			BuildAsync(controller, generation, cancellationTokenSource.Token);
		}
	}

	private async Task BuildAsync(CoreGameController controller, long generation, CancellationToken ct)
	{
		try
		{
			_board.RaiseStatus("正在预计算全部物件的移动区域…");
			AvailableOperationsSnapshot val = await controller.PrecomputeOperationsAsync(ct);
			if (ct.IsCancellationRequested || generation != _domainGeneration || controller != _board.Controller)
			{
				return;
			}
			if (!controller.InstallOperations(val))
			{
				_board.RaiseStatus("操作快照已过期或不完整，正在重试…");
				BeginDomainBuild();
				return;
			}
			_domainsReady = true;
			int num = val.Operations.Count((AvailableOperation o) => (int)o.Availability == 0);
			int num2 = val.Operations.Count((AvailableOperation o) => (int)o.Availability == 1);
			_board.RaiseStatus($"操作快照就绪 · 可用 {num} / 阻挡 {num2}");
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			if (generation == _domainGeneration)
			{
				_board.RaiseStatus("移动区域预计算失败 · " + ex2.Message);
			}
		}
	}

	private void CancelDomainBuild()
	{
		_domainGeneration++;
		_domainCts?.Cancel();
		_domainCts?.Dispose();
		_domainCts = null;
		_domainsReady = false;
	}

	private static double WrapDegrees(double degrees)
	{
		while (degrees > 180.0)
		{
			degrees -= 360.0;
		}
		while (degrees < -180.0)
		{
			degrees += 360.0;
		}
		return degrees;
	}

	private static double CopySignLocal(double value, double sign)
	{
		if (!(sign >= 0.0))
		{
			return 0.0 - Math.Abs(value);
		}
		return Math.Abs(value);
	}

	private static string DescribeDomain(GestureMotionDomain domain)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)domain.MotionKind != 0)
		{
			return $"平移 [{domain.MinimumSignedAmount:0.#}, {domain.MaximumSignedAmount:0.#}]";
		}
		return $"旋转 [{domain.MinimumSignedAmount:0.#}°, {domain.MaximumSignedAmount:0.#}°]";
	}

	private static string DescribeResult(MotionResult result)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		int num = result.Delta.ReleasedParts.Count + result.Delta.RemovedParts.Count;
		string arg = (((int)result.AmountKind == 0) ? $"{result.ActualAmount:0.##}°" : $"{result.ActualAmount:0.##}");
		if (num <= 0)
		{
			return $"Core 已提交一次手势 · {arg} · {result.Status}";
		}
		return $"Core 已提交一次手势 · {arg} · 退出 {num} 个部件";
	}
}
