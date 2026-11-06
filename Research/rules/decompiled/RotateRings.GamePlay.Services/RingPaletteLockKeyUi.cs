using System;
using System.Collections.Generic;
using Game.UI;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.UI.Game;
using UnityEngine;
using XingYun.RingGame;
using XingYun.UI.Controller;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockKeyUi
{
	private const string Tag = "[RingPaletteLockKeyUi] ";

	private const string LockKeyItemLocation = "LockKeyItem";

	internal const float BaseInnerDiameterPixels = 150f;

	private readonly RingPaletteLockService owner;

	private readonly List<WidgetHandle> keyUiHandles = new List<WidgetHandle>();

	private readonly Dictionary<RingObject, WidgetHandle> ringToKeyUi = new Dictionary<RingObject, WidgetHandle>();

	private UIGameView hostView;

	private readonly HashSet<RingObject> keyUiDisappearingRings = new HashSet<RingObject>();

	private readonly Dictionary<RingObject, int> disappearTimerIdByRing = new Dictionary<RingObject, int>();

	internal RingPaletteLockKeyUi(RingPaletteLockService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
		GameApp.Event.Listen<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
	}

	internal void Dispose()
	{
		ClearKeyUi();
		hostView = null;
		GameApp.Event.Unlisten<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
	}

	internal void ClearKeyUi()
	{
		CancelAllDisappearTimers();
		keyUiDisappearingRings.Clear();
		for (int num = keyUiHandles.Count - 1; num >= 0; num--)
		{
			DetachKeyUi(keyUiHandles[num]);
		}
		keyUiHandles.Clear();
		ringToKeyUi.Clear();
	}

	internal void SetupKeyUiOnLevelLoaded()
	{
		IReadOnlyList<RingObject> readOnlyList = owner.HostInstance.ObjectsService?.Rings;
		if (readOnlyList != null)
		{
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				CreateKeyUiForRing(readOnlyList[i]);
			}
		}
	}

	internal void OnRingScaleFactor(RingObject ring, float factor)
	{
		if ((Object)(object)ring == (Object)null || !ShouldShowKey(ring))
		{
			return;
		}
		if (!ringToKeyUi.TryGetValue(ring, out var value))
		{
			CreateKeyUiForRing(ring);
			if (!ringToKeyUi.TryGetValue(ring, out value))
			{
				return;
			}
		}
		value.GetView<UILockKeyItemView>()?.SetEnterScaleFactor(factor);
	}

	internal bool HasKeyUi(RingObject ring)
	{
		if ((Object)(object)ring != (Object)null)
		{
			return ringToKeyUi.ContainsKey(ring);
		}
		return false;
	}

	internal void OnKeyRingSolved(RingObject ring)
	{
		CancelDisappearTimer(ring);
		if (!((Object)(object)ring == (Object)null) && ringToKeyUi.TryGetValue(ring, out var value))
		{
			keyUiDisappearingRings.Remove(ring);
			keyUiHandles.Remove(value);
			ringToKeyUi.Remove(ring);
			DetachKeyUi(value);
		}
	}

	internal void PlayDisappearAndDetach(RingObject keyRing)
	{
		if ((Object)(object)keyRing == (Object)null || !ringToKeyUi.TryGetValue(keyRing, out var handle) || keyUiDisappearingRings.Contains(keyRing))
		{
			return;
		}
		keyUiDisappearingRings.Add(keyRing);
		UILockKeyItemView view = handle.GetView<UILockKeyItemView>();
		if (view == null || !view.TryPlayDisappear(out var durationSeconds))
		{
			keyUiDisappearingRings.Remove(keyRing);
			OnKeyRingSolved(keyRing);
			return;
		}
		RectTransform val = hostView?.ChainLockContent;
		if ((Object)(object)val == (Object)null || durationSeconds <= 0f)
		{
			FinishDisappearDetach(keyRing, handle);
			return;
		}
		CancelDisappearTimer(keyRing);
		int num = GameApp.Timer.Delay((Object)(object)val, durationSeconds, (Action)delegate
		{
			if (disappearTimerIdByRing.Remove(keyRing))
			{
				FinishDisappearDetach(keyRing, handle);
			}
		});
		if (num >= 0)
		{
			disappearTimerIdByRing[keyRing] = num;
		}
		else
		{
			FinishDisappearDetach(keyRing, handle);
		}
	}

	private void CancelDisappearTimer(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null) && disappearTimerIdByRing.TryGetValue(ring, out var value))
		{
			disappearTimerIdByRing.Remove(ring);
			if (value >= 0)
			{
				GameApp.Timer.Kill(value);
			}
		}
	}

	private void CancelAllDisappearTimers()
	{
		foreach (KeyValuePair<RingObject, int> item in disappearTimerIdByRing)
		{
			if (item.Value >= 0)
			{
				GameApp.Timer.Kill(item.Value);
			}
		}
		disappearTimerIdByRing.Clear();
	}

	private void FinishDisappearDetach(RingObject keyRing, WidgetHandle handle)
	{
		keyUiDisappearingRings.Remove(keyRing);
		if (ringToKeyUi.TryGetValue(keyRing, out var value) && value == handle)
		{
			keyUiHandles.Remove(handle);
			ringToKeyUi.Remove(keyRing);
			DetachKeyUi(handle);
		}
	}

	private static bool ShouldShowKey(RingObject ring)
	{
		if ((Object)(object)ring != (Object)null && ring.IsCurrentPaletteLockKey)
		{
			return !ring.IsSolved;
		}
		return false;
	}

	private void CreateKeyUiForRing(RingObject ring)
	{
		IController val = default(IController);
		if (!ShouldShowKey(ring) || ringToKeyUi.ContainsKey(ring) || !GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController { View: var view }))
		{
			return;
		}
		RectTransform val2 = view?.ChainLockContent;
		if ((Object)(object)val2 == (Object)null)
		{
			return;
		}
		hostView = view;
		WidgetHandle widgetHandle = view.AttachWidget<UILockKeyItemView>("LockKeyItem", (Transform)(object)val2);
		if (widgetHandle != null)
		{
			keyUiHandles.Add(widgetHandle);
			ringToKeyUi[ring] = widgetHandle;
			if (!owner.TryGetKeyColor(ring, out var color))
			{
				color = PaletteLockKeyColor.Blue;
			}
			widgetHandle.GetView<UILockKeyItemView>()?.Bind(ring, val2, color);
		}
	}

	private void OnCameraAdaptationEnd(GameCameraAdaptationEndEvent evt)
	{
		RefreshAllKeyItems();
	}

	private void RefreshAllKeyItems()
	{
		if (hostView == null || ringToKeyUi.Count == 0)
		{
			return;
		}
		RectTransform chainLockContent = hostView.ChainLockContent;
		if ((Object)(object)chainLockContent == (Object)null)
		{
			return;
		}
		float cameraScaleFactor = ComputeCameraScaleFactor();
		foreach (KeyValuePair<RingObject, WidgetHandle> item in ringToKeyUi)
		{
			RingObject key = item.Key;
			if (!((Object)(object)key == (Object)null))
			{
				item.Value?.GetView<UILockKeyItemView>()?.RefreshPosition(key, chainLockContent, cameraScaleFactor);
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
		float num = owner.HostInstance.GetGameService<RotateCameraAdaptation>()?.CachedOrthographicSize ?? 0f;
		if (num <= 0f)
		{
			return 1f;
		}
		return num / gameplayCamera.orthographicSize;
	}

	internal static float ComputeInnerDiameterPixels(RingObject ring, RectTransform parentRect, out float innerDiameterWorld)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		innerDiameterWorld = 0f;
		if ((Object)(object)ring == (Object)null || (Object)(object)parentRect == (Object)null)
		{
			return 0f;
		}
		RingSegmentController componentInChildren = ((Component)ring).GetComponentInChildren<RingSegmentController>(true);
		if ((Object)(object)componentInChildren == (Object)null || componentInChildren.CenterRadius <= 0.0001f)
		{
			return 0f;
		}
		float num = 2f * componentInChildren.CenterRadius - componentInChildren.BandWidth;
		Vector3 lossyScale = ((Component)ring).transform.lossyScale;
		float num2 = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
		innerDiameterWorld = num * num2;
		return WorldToUIPostionUtil.WorldRadiusToLocal(ring.GetPosition(), innerDiameterWorld * 0.5f, parentRect) * 2f;
	}

	private void DetachKeyUi(WidgetHandle handle)
	{
		if (handle != null && !handle.IsDisposed)
		{
			hostView?.DetachWidget(handle);
		}
	}
}
