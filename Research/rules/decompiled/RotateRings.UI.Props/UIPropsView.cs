using System.Collections.Generic;
using System.Linq;
using Example.Cfg;
using Game.UI;
using RotateRings.Prop;
using RotateRings.UI.PropItem;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Props;

public class UIPropsView : GameWidgetView
{
	private struct PropItemEntry
	{
		public WidgetHandle Handle;

		public EPropType PropType;
	}

	private const string Tag = "[UIPropsView] ";

	private const string PropItemLocation = "PropItem";

	private readonly List<PropItemEntry> propItems = new List<PropItemEntry>();

	private bool isBuilt;

	public RectTransform Props;

	public void BuildPropItems()
	{
		if (isBuilt || (Object)(object)Props == (Object)null)
		{
			return;
		}
		PropModelProxy propProxy = GameApp.Proxy.Get<PropModelProxy>();
		List<PropConfig> list = propProxy?.GetInGamePropConfigs();
		if (list == null || list.Count == 0)
		{
			return;
		}
		list.Sort((PropConfig a, PropConfig b) => propProxy.GetPropUnlockLv(a).CompareTo(propProxy.GetPropUnlockLv(b)));
		for (int num = 0; num < list.Count; num++)
		{
			PropConfig propConfig = list[num];
			WidgetHandle widgetHandle = AttachWidget<UIPropItemView>("PropItem", (Transform)(object)Props);
			if (widgetHandle == null)
			{
				Log.LogError("[UIPropsView] ", "AttachWidget failed: {0}", new object[1] { "PropItem" });
				continue;
			}
			EPropType id = (EPropType)propConfig.Id;
			propItems.Add(new PropItemEntry
			{
				Handle = widgetHandle,
				PropType = id
			});
			widgetHandle.GetView<UIPropItemView>()?.BindEvents();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(Props);
		isBuilt = true;
	}

	public void RefreshProp(EPropType propType)
	{
		GetPropItemView(propType)?.RefreshProp(propType);
	}

	public void RefreshPropBadge(EPropType propType)
	{
		GetPropItemView(propType)?.Refresh();
	}

	public void RefreshPropCount(EPropType propType, int count)
	{
		GetPropItemView(propType)?.ShowCount(count);
	}

	public void RefreshScrollTo(EPropType propType, int fromCount, int toCount, float duration, bool openGuideOnComplete = false)
	{
		GetPropItemView(propType)?.ApplyScroll(propType, fromCount, toCount, duration, openGuideOnComplete);
	}

	public void SetPresentationLocked(EPropType propType, bool locked)
	{
		GetPropItemView(propType)?.SetPresentationLocked(locked);
	}

	public void RefreshAllProp(IReadOnlyCollection<EPropType> excludePropTypes = null)
	{
		List<PropConfig> list = GameApp.Proxy.Get<PropModelProxy>()?.GetInGamePropConfigs();
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			EPropType id = (EPropType)list[i].Id;
			if (excludePropTypes == null || !excludePropTypes.Contains(id))
			{
				RefreshProp(id);
			}
		}
	}

	public void RefreshAllProp()
	{
		RefreshAllProp(null);
	}

	public Transform GetPropFlyTarget(EPropType propType)
	{
		UIPropItemView propItemView = GetPropItemView(propType);
		if (!((Object)(object)propItemView?.Icon != (Object)null))
		{
			return null;
		}
		return (Transform)(object)((Graphic)propItemView.Icon).rectTransform;
	}

	public RectTransform GetPropItemAlignAnchor(EPropType propType)
	{
		UIPropItemView propItemView = GetPropItemView(propType);
		object obj = ((propItemView != null) ? ((UIBaseView)propItemView).NodeRect : null);
		if (obj == null)
		{
			if (propItemView == null)
			{
				return null;
			}
			obj = propItemView.Node;
		}
		return (RectTransform)obj;
	}

	public void SetPropItemCanvasAlpha(EPropType propType, float alpha)
	{
		UIPropItemView propItemView = GetPropItemView(propType);
		if (!((Object)(object)propItemView?.CanvasGroup == (Object)null))
		{
			propItemView.CanvasGroup.alpha = alpha;
		}
	}

	public void SetPropItemBadgesVisible(EPropType propType, bool visible)
	{
		GetPropItemView(propType)?.SetBadgesVisible(visible);
	}

	public void SetPropsContainerVisible(bool visible)
	{
		if (!((Object)(object)Props == (Object)null))
		{
			((Component)Props).gameObject.SetActive(visible);
		}
	}

	public void PlaySinglePropAppearFadeIn(EPropType propType, float duration)
	{
		GetPropItemView(propType)?.PlayAppearFadeIn(duration);
	}

	public void SetPropItemGray(EPropType propType, bool grayed)
	{
		GetPropItemView(propType)?.SetGray(grayed);
	}

	public void SetPropItemBadgeProvider(EPropType propType, IPropBadgeProvider provider)
	{
		GetPropItemView(propType)?.SetBadgeProvider(provider);
	}

	private UIPropItemView GetPropItemView(EPropType propType)
	{
		for (int i = 0; i < propItems.Count; i++)
		{
			if (propItems[i].PropType == propType)
			{
				return propItems[i].Handle?.GetView<UIPropItemView>();
			}
		}
		return null;
	}

	private void ClearPropItems()
	{
		for (int num = propItems.Count - 1; num >= 0; num--)
		{
			DetachWidget(propItems[num].Handle);
		}
		propItems.Clear();
		isBuilt = false;
	}

	public override void Dispose()
	{
		ClearPropItems();
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		Props = behaviour.Get<RectTransform>("Props");
	}
}
