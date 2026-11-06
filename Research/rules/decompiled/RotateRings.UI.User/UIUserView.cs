using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.User;

public class UIUserView : UIBaseView
{
	private readonly List<UIUser_IconItem> iconItems = new List<UIUser_IconItem>();

	public Button bgMask;

	public Button btn_close;

	public TMP_Text Profile;

	public ScrollRect ScrollAvatar;

	public Button btnbg;

	public UIUser_IconItem itemIcon;

	public TMP_Text userName;

	public Image userHead;

	public TMP_InputField InputField;

	public Button btnRandom;

	public Button btnEdit;

	public void Init(string playerName, List<UserIconData> headList, int selectedId, Action<int> onItemClick)
	{
		RefreshPlayerName(playerName);
		PopulateHeadList(headList, selectedId, onItemClick);
		RefreshHeadPreview(selectedId, headList);
	}

	public void RefreshPlayerName(string playerName)
	{
		if ((Object)(object)userName != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(userName, playerName ?? string.Empty);
		}
	}

	public void PopulateHeadList(List<UserIconData> headList, int selectedId, Action<int> onItemClick)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		RectTransform content = ScrollAvatar.content;
		ClearList();
		foreach (UserIconData head in headList)
		{
			UIUser_IconItem uIUser_IconItem = Object.Instantiate<UIUser_IconItem>(itemIcon, (Transform)(object)content);
			((Component)uIUser_IconItem).gameObject.SetActive(true);
			if ((Object)(object)uIUser_IconItem.icon != (Object)null && (Object)(object)head.Sprite != (Object)null)
			{
				uIUser_IconItem.icon.sprite = head.Sprite;
			}
			uIUser_IconItem.selected.SetActive(head.Id == selectedId);
			int id = head.Id;
			((UnityEvent)uIUser_IconItem.button.onClick).AddListener((UnityAction)delegate
			{
				onItemClick?.Invoke(id);
			});
			iconItems.Add(uIUser_IconItem);
		}
		((Component)itemIcon).gameObject.SetActive(false);
	}

	public void SetHeadSelected(int selectedId, List<UserIconData> headList)
	{
		for (int i = 0; i < iconItems.Count && i < headList.Count; i++)
		{
			iconItems[i].selected.SetActive(headList[i].Id == selectedId);
		}
		RefreshHeadPreview(selectedId, headList);
	}

	private void RefreshHeadPreview(int selectedId, List<UserIconData> headList)
	{
		if ((Object)(object)userHead == (Object)null || headList == null)
		{
			return;
		}
		foreach (UserIconData head in headList)
		{
			if (head.Id == selectedId)
			{
				if ((Object)(object)head.Sprite != (Object)null)
				{
					userHead.sprite = head.Sprite;
				}
				break;
			}
		}
	}

	private void ClearList()
	{
		foreach (UIUser_IconItem iconItem in iconItems)
		{
			if ((Object)(object)iconItem != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)iconItem).gameObject);
			}
		}
		iconItems.Clear();
	}

	public override void Dispose()
	{
		ClearList();
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		bgMask = behaviour.Get<Button>("bgMask");
		btn_close = behaviour.Get<Button>("btn_close");
		Profile = behaviour.Get<TMP_Text>("Profile");
		ScrollAvatar = behaviour.Get<ScrollRect>("ScrollAvatar");
		btnbg = behaviour.Get<Button>("btnbg");
		itemIcon = behaviour.Get<UIUser_IconItem>("itemIcon");
		userName = behaviour.Get<TMP_Text>("userName");
		userHead = behaviour.Get<Image>("userHead");
		InputField = behaviour.Get<TMP_InputField>("InputField");
		btnRandom = behaviour.Get<Button>("btnRandom");
		btnEdit = behaviour.Get<Button>("btnEdit");
	}
}
