using System;
using System.Collections.Generic;
using Game.UI;
using RotateRings.UI.RankList;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.UI.Home;

public sealed class HomeActivityEntryHost
{
	private const float IconSpineInterval = 2f;

	private readonly List<IHomeActivityEntry> entries = new List<IHomeActivityEntry>();

	private bool isMounted;

	private MonoBehaviour iconSpineLoopOwner;

	private int iconSpineTimerId = -1;

	private int iconSpinePlayIndex;

	public void Register(IHomeActivityEntry entry)
	{
		if (entry != null && !entries.Contains(entry))
		{
			entries.Add(entry);
			entries.Sort((IHomeActivityEntry a, IHomeActivityEntry b) => a.Order.CompareTo(b.Order));
		}
	}

	public void Unregister(IHomeActivityEntry entry)
	{
		if (entry != null)
		{
			entries.Remove(entry);
		}
	}

	public void RegisterDefaults()
	{
		Register(new DailyFirstWinHomeEntry());
		Register(new RankListHomeEntry());
	}

	public void MountAll(GameBaseView host, RectTransform left, RectTransform right)
	{
		if (isMounted)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < entries.Count; i++)
		{
			IHomeActivityEntry homeActivityEntry = entries[i];
			homeActivityEntry.Mount(host, left, right);
			if (homeActivityEntry.Slot == HomeActivityEntrySlot.Left)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		if (flag && (Object)(object)left != (Object)null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(left);
		}
		if (flag2 && (Object)(object)right != (Object)null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(right);
		}
		isMounted = true;
	}

	public void UnmountAll()
	{
		StopIconSpineLoop();
		for (int i = 0; i < entries.Count; i++)
		{
			entries[i].Unmount();
		}
		isMounted = false;
	}

	public void BindAll()
	{
		for (int i = 0; i < entries.Count; i++)
		{
			entries[i].Bind();
		}
	}

	public void UnbindAll()
	{
		for (int i = 0; i < entries.Count; i++)
		{
			entries[i].Unbind();
		}
	}

	public void RefreshAll()
	{
		for (int i = 0; i < entries.Count; i++)
		{
			entries[i].Refresh();
		}
	}

	public void ClearEntries()
	{
		StopIconSpineLoop();
		entries.Clear();
		isMounted = false;
	}

	public void StartIconSpineLoop(MonoBehaviour owner)
	{
		StopIconSpineLoop();
		if (!((Object)(object)owner == (Object)null) && entries.Count != 0)
		{
			iconSpineLoopOwner = owner;
			iconSpineTimerId = GameApp.Timer.NextFrame((Object)(object)owner, (Action)delegate
			{
				iconSpineTimerId = -1;
				FindAndPlayNextIconSpine();
			});
		}
	}

	public void StopIconSpineLoop()
	{
		if (iconSpineTimerId != -1)
		{
			GameApp.Timer.Kill(iconSpineTimerId);
			iconSpineTimerId = -1;
		}
		iconSpineLoopOwner = null;
		iconSpinePlayIndex = 0;
		for (int i = 0; i < entries.Count; i++)
		{
			entries[i].StopIconSpineAnimation();
		}
	}

	private void FindAndPlayNextIconSpine()
	{
		if ((Object)(object)iconSpineLoopOwner == (Object)null)
		{
			return;
		}
		List<IHomeActivityEntry> iconSpinePlayOrder = GetIconSpinePlayOrder();
		int count = iconSpinePlayOrder.Count;
		if (count == 0)
		{
			return;
		}
		int num = iconSpinePlayIndex % count;
		for (int i = 0; i < count; i++)
		{
			int num2 = (num + i) % count;
			IHomeActivityEntry homeActivityEntry = iconSpinePlayOrder[num2];
			StopOtherIconSpineAnimations(iconSpinePlayOrder, homeActivityEntry);
			if (homeActivityEntry.TryPlayIconSpineAnimation(OnIconSpinePlayComplete))
			{
				iconSpinePlayIndex = (num2 + 1) % count;
				return;
			}
		}
		ScheduleNextIconSpinePlay(2f);
	}

	private void OnIconSpinePlayComplete()
	{
		ScheduleNextIconSpinePlay(2f);
	}

	private void ScheduleNextIconSpinePlay(float delay)
	{
		if (!((Object)(object)iconSpineLoopOwner == (Object)null))
		{
			if (iconSpineTimerId != -1)
			{
				GameApp.Timer.Kill(iconSpineTimerId);
			}
			iconSpineTimerId = GameApp.Timer.Delay((Object)(object)iconSpineLoopOwner, delay, (Action)delegate
			{
				iconSpineTimerId = -1;
				FindAndPlayNextIconSpine();
			});
		}
	}

	private static void StopOtherIconSpineAnimations(List<IHomeActivityEntry> playOrder, IHomeActivityEntry playingEntry)
	{
		for (int i = 0; i < playOrder.Count; i++)
		{
			IHomeActivityEntry homeActivityEntry = playOrder[i];
			if (homeActivityEntry != playingEntry)
			{
				homeActivityEntry.StopIconSpineAnimation();
			}
		}
	}

	private List<IHomeActivityEntry> GetIconSpinePlayOrder()
	{
		List<IHomeActivityEntry> list = new List<IHomeActivityEntry>(entries);
		list.Sort(delegate(IHomeActivityEntry a, IHomeActivityEntry b)
		{
			int num = ((int)a.Slot).CompareTo((int)b.Slot);
			return (num == 0) ? a.Order.CompareTo(b.Order) : num;
		});
		return list;
	}
}
