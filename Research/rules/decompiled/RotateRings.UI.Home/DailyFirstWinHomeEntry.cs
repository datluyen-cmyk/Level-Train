using System;
using Game.UI;
using RotateRings.DailyFirstWin;
using RotateRings.UI.DailyFirstWin;
using UniLog;
using UnityEngine;

namespace RotateRings.UI.Home;

public sealed class DailyFirstWinHomeEntry : IHomeActivityEntry
{
	private enum MountedWidgetKind
	{
		None,
		Badge,
		Puzzle
	}

	private const string BadgeWidgetLocation = "ButtonItem_DailyFirstWin";

	private const string PuzzleWidgetLocation = "ButtonItem_DailyFirstWinPuzzle";

	private GameBaseView hostView;

	private RectTransform mountParent;

	private WidgetHandle handle;

	private MountedWidgetKind mountedKind;

	private bool isEventsBound;

	public int Order => 50;

	public HomeActivityEntrySlot Slot => HomeActivityEntrySlot.Left;

	public void Mount(GameBaseView host, RectTransform left, RectTransform right)
	{
		hostView = host;
		mountParent = left;
		if (!((Object)(object)left == (Object)null))
		{
			EnsureMountedForCurrentPresentation();
		}
	}

	public void Unmount()
	{
		handle = null;
		mountedKind = MountedWidgetKind.None;
		hostView = null;
		mountParent = null;
	}

	public void Bind()
	{
		BindWidgetEvents();
		if (!isEventsBound)
		{
			GameApp.Event.Listen<DailyFirstWinStateChangedEvent>((Action<DailyFirstWinStateChangedEvent>)OnStateChanged);
			isEventsBound = true;
		}
	}

	public void Unbind()
	{
		if (isEventsBound)
		{
			GameApp.Event.Unlisten<DailyFirstWinStateChangedEvent>((Action<DailyFirstWinStateChangedEvent>)OnStateChanged);
			isEventsBound = false;
		}
	}

	public void Refresh()
	{
		GetBadgeWidgetView()?.Refresh();
		GetPuzzleWidgetView()?.Refresh();
	}

	public bool TryPlayIconSpineAnimation(Action onComplete)
	{
		if (mountedKind == MountedWidgetKind.Puzzle)
		{
			return GetPuzzleWidgetView()?.TryPlayIconSpineAnimation(onComplete) ?? false;
		}
		return GetBadgeWidgetView()?.TryPlayIconSpineAnimation(onComplete) ?? false;
	}

	public void StopIconSpineAnimation()
	{
		GetBadgeWidgetView()?.StopIconSpineAnimation();
		GetPuzzleWidgetView()?.StopIconSpineAnimation();
	}

	private void OnEntryClick()
	{
		DailyFirstWinModule.UIHost?.ReportBtnClick("主页-每日首胜");
		DailyFirstWinService.Instance.OpenHomeUI();
	}

	private void OnStateChanged(DailyFirstWinStateChangedEvent eventData)
	{
		EnsureMountedForCurrentPresentation();
		Refresh();
	}

	private void EnsureMountedForCurrentPresentation()
	{
		if (hostView == null || (Object)(object)mountParent == (Object)null)
		{
			return;
		}
		bool flag = ShouldUsePuzzleWidget();
		MountedWidgetKind mountedWidgetKind = ((!flag) ? MountedWidgetKind.Badge : MountedWidgetKind.Puzzle);
		if (handle != null && !handle.IsDisposed && mountedKind == mountedWidgetKind)
		{
			return;
		}
		if (handle != null && !handle.IsDisposed)
		{
			hostView.DetachWidget(handle);
		}
		handle = null;
		mountedKind = MountedWidgetKind.None;
		if (flag)
		{
			handle = hostView.AttachWidget<UIButtonItemDailyFirstWinPuzzleView>("ButtonItem_DailyFirstWinPuzzle", (Transform)(object)mountParent);
			mountedKind = MountedWidgetKind.Puzzle;
			if (handle == null)
			{
				Log.LogError("UI", "[DailyFirstWinHomeEntry] AttachWidget failed: {0}", new object[1] { "ButtonItem_DailyFirstWinPuzzle" });
			}
		}
		else
		{
			handle = hostView.AttachWidget<UIButtonItemDailyFirstWinView>("ButtonItem_DailyFirstWin", (Transform)(object)mountParent);
			mountedKind = MountedWidgetKind.Badge;
			if (handle == null)
			{
				Log.LogError("UI", "[DailyFirstWinHomeEntry] AttachWidget failed: {0}", new object[1] { "ButtonItem_DailyFirstWin" });
			}
		}
		BindWidgetEvents();
	}

	private void BindWidgetEvents()
	{
		GetBadgeWidgetView()?.BindEvents(OnEntryClick);
		GetPuzzleWidgetView()?.BindEvents(OnEntryClick);
	}

	private static bool ShouldUsePuzzleWidget()
	{
		if (!DailyFirstWinModule.IsInitialized)
		{
			return false;
		}
		return GameApp.Proxy.Get<DailyFirstWinProxy>()?.UsePuzzleEntryPresentation() ?? false;
	}

	private UIButtonItemDailyFirstWinView GetBadgeWidgetView()
	{
		if (mountedKind != MountedWidgetKind.Badge)
		{
			return null;
		}
		return handle?.GetView<UIButtonItemDailyFirstWinView>();
	}

	private UIButtonItemDailyFirstWinPuzzleView GetPuzzleWidgetView()
	{
		if (mountedKind != MountedWidgetKind.Puzzle)
		{
			return null;
		}
		return handle?.GetView<UIButtonItemDailyFirstWinPuzzleView>();
	}
}
