using System;
using Game.DataTrack;
using Game.UI;
using RotateRings.UI.Home;
using Rotaterings.Rank;
using UniLog;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.RankList;

public sealed class RankListHomeEntry : IHomeActivityEntry
{
	private const string WidgetLocation = "ButtonItem_RankList";

	private static RankListHomeEntry activeInstance;

	private WidgetHandle handle;

	public int Order => 60;

	public HomeActivityEntrySlot Slot => HomeActivityEntrySlot.Right;

	public void Mount(GameBaseView host, RectTransform left, RectTransform right)
	{
		if (handle != null && !handle.IsDisposed)
		{
			return;
		}
		handle = null;
		activeInstance = this;
		if (!((Object)(object)right == (Object)null))
		{
			handle = host.AttachWidget<UIButtonItemRankListView>("ButtonItem_RankList", (Transform)(object)right);
			if (handle == null)
			{
				Log.LogError("UI", "[RankListHomeEntry] AttachWidget failed: {0}", new object[1] { "ButtonItem_RankList" });
			}
			else
			{
				GetWidgetView()?.Refresh();
			}
		}
	}

	public void Unmount()
	{
		if (activeInstance == this)
		{
			activeInstance = null;
		}
		handle = null;
	}

	public static void SetEntryVisible(bool visible)
	{
		activeInstance?.GetWidgetView()?.SetRootVisible(visible);
	}

	public void Bind()
	{
		GetWidgetView()?.BindEvents(OnEntryClick);
	}

	public void Unbind()
	{
	}

	public void Refresh()
	{
		GetWidgetView()?.Refresh();
	}

	public bool TryPlayIconSpineAnimation(Action onComplete)
	{
		return GetWidgetView()?.TryPlayIconSpineAnimation(onComplete) ?? false;
	}

	public void StopIconSpineAnimation()
	{
		GetWidgetView()?.StopIconSpineAnimation();
	}

	private void OnEntryClick()
	{
		RRDataTrack.Instance.ReportBtnClick("主页-排行榜");
		RankActivityEntry.OpenRankList((UILayer)2);
	}

	private UIButtonItemRankListView GetWidgetView()
	{
		return handle?.GetView<UIButtonItemRankListView>();
	}
}
