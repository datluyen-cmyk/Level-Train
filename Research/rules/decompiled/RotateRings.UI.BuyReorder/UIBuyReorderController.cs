using System;
using System.Collections.Generic;
using Game.UI;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.Shop;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using WordSolitaire.UI.Shop;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.BuyReorder;

[UIPath("UIBuyReorder")]
public class UIBuyReorderController : GameBaseController<UIBuyReorderView>
{
	private const string Tag = "[UIBuyReorderController]";

	private readonly List<BuyItemView> buyItemViewList = new List<BuyItemView>();

	private Action onConfirm;

	private bool isClosing;

	public UIBuyReorderController(UIBuyReorderView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIBuyReorderView>)this).view.btnClaim.onClick).AddListener(new UnityAction(OnClaimClick));
		((UnityEvent)((BaseController<UIBuyReorderView>)this).view.btnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		if (!(param is BuyReorderUIParam buyReorderUIParam))
		{
			Log.LogError("[UIBuyReorderController]", "打开参数无效，期望 BuyReorderUIParam");
			((BaseController<UIBuyReorderView>)this).CloseView(false);
			return;
		}
		onConfirm = buyReorderUIParam.OnConfirm;
		((BaseController<UIBuyReorderView>)this).view.Init();
		((BaseController<UIBuyReorderView>)this).view.PlayOpenAnimation();
		RenderRewards(buyReorderUIParam.Items);
	}

	private void OnClaimClick()
	{
		CloseViewHandler();
	}

	private void OnCloseClick()
	{
		CloseViewHandler();
	}

	private void RenderRewards(IReadOnlyList<ShopRewardItem> rewards)
	{
		ClearBuyList();
		if (rewards.Count != 0)
		{
			buyItemViewList.AddRange(CreateRewardList(rewards));
		}
	}

	private List<BuyItemView> CreateRewardList(IReadOnlyList<ShopRewardItem> rewards)
	{
		BuyItemView itemTemp = ((BaseController<UIBuyReorderView>)this).view.itemTemp;
		List<BuyItemView> list = new List<BuyItemView>();
		if ((Object)(object)itemTemp == (Object)null || (Object)(object)((BaseController<UIBuyReorderView>)this).view.RewardListLayout == (Object)null)
		{
			if ((Object)(object)itemTemp != (Object)null)
			{
				((Component)itemTemp).gameObject.SetActive(false);
			}
			return list;
		}
		for (int num = ((Component)((BaseController<UIBuyReorderView>)this).view.RewardListLayout).transform.childCount - 1; num >= 0; num--)
		{
			Transform child = ((Component)((BaseController<UIBuyReorderView>)this).view.RewardListLayout).transform.GetChild(num);
			if ((Object)(object)child != (Object)(object)((Component)itemTemp).transform)
			{
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
		}
		((Component)itemTemp).gameObject.SetActive(false);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		List<int> list2 = new List<int>();
		foreach (ShopRewardItem reward in rewards)
		{
			if (reward.Count > 0)
			{
				if (dictionary.TryGetValue(reward.PropId, out var value))
				{
					dictionary[reward.PropId] = value + reward.Count;
					continue;
				}
				list2.Add(reward.PropId);
				dictionary.Add(reward.PropId, reward.Count);
			}
		}
		foreach (int item in list2)
		{
			string iconName = PropIconService.GetIconName((EPropType)item);
			GameObject val = Object.Instantiate<GameObject>(((Component)itemTemp).gameObject, ((Component)((BaseController<UIBuyReorderView>)this).view.RewardListLayout).transform, false);
			BuyItemView component = val.GetComponent<BuyItemView>();
			if ((Object)(object)component == (Object)null)
			{
				Object.Destroy((Object)(object)val);
				continue;
			}
			component.Initialize(item, dictionary[item], iconName);
			val.SetActive(true);
			list.Add(component);
		}
		return list;
	}

	private void CloseViewHandler()
	{
		if (!isClosing)
		{
			isClosing = true;
			Action action = onConfirm;
			onConfirm = null;
			action?.Invoke();
			((BaseController<UIBuyReorderView>)this).CloseView(false);
		}
	}

	private void ClearBuyList()
	{
		foreach (BuyItemView buyItemView in buyItemViewList)
		{
			if ((Object)(object)buyItemView != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)buyItemView).gameObject);
			}
		}
		buyItemViewList.Clear();
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIBuyReorderView>)this).view.btnClaim.onClick).RemoveListener(new UnityAction(OnClaimClick));
		((UnityEvent)((BaseController<UIBuyReorderView>)this).view.btnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		ClearBuyList();
		onConfirm = null;
		isClosing = false;
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIBuyReorderView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		CloseViewHandler();
	}
}
