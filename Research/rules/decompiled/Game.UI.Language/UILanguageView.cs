using System;
using System.Collections.Generic;
using Example.Cfg;
using SuperScrollView;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace Game.UI.Language;

public class UILanguageView : GameBaseView
{
	private LanguageInitData mLanguageInitData;

	private Dictionary<GameObject, UILanguageItemView> mItemViewCache = new Dictionary<GameObject, UILanguageItemView>();

	private bool mIsInit;

	public Action<string> OnLanguageChanged;

	public Button btn_close;

	public LoopListView2 ScrollView;

	public GameObject objItem;

	public void OnRefreshView(LanguageInitData languageInitData)
	{
		if (languageInitData != null && languageInitData.SettingLanguageList != null)
		{
			mLanguageInitData = languageInitData;
			if (!mIsInit)
			{
				ScrollView.InitListView(mLanguageInitData.SettingLanguageList.Count, (Func<LoopListView2, int, LoopListViewItem2>)OnGetItemByIndex, (LoopListViewInitParam)null, (Func<int, ValueTuple<float, float>>)null);
				mIsInit = true;
			}
			else
			{
				ScrollView.SetListItemCount(mLanguageInitData.SettingLanguageList.Count, false);
				ScrollView.RefreshAllShownItem();
			}
			ScrollView.MovePanelToItemIndexImmediately(GetLanguageIndex(mLanguageInitData.CurrentLanguage), 0f);
		}
	}

	private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
	{
		if (index < 0 || index >= mLanguageInitData.SettingLanguageList.Count)
		{
			return null;
		}
		SettingLanguage settingLanguage = mLanguageInitData.SettingLanguageList[index];
		if (settingLanguage == null)
		{
			return null;
		}
		LoopListViewItem2 val = listView.NewListViewItem("UILanguageItem");
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("UILanguageView", "Error: Failed to create 'UILanguageItem'. Check LoopListView2 Inspector settings.");
			return null;
		}
		UILanguageItemView orAddWrapper = GetOrAddWrapper(val);
		bool isSelected = IsLanguageMatch(settingLanguage, mLanguageInitData.CurrentLanguage);
		orAddWrapper.SetData(settingLanguage, isSelected);
		return val;
	}

	private UILanguageItemView GetOrAddWrapper(LoopListViewItem2 item)
	{
		if (!mItemViewCache.TryGetValue(((Component)item).gameObject, out var value))
		{
			value = new UILanguageItemView();
			UIBehaviour component = ((Component)item).GetComponent<UIBehaviour>();
			if ((Object)(object)component == (Object)null)
			{
				Log.LogError("UILanguageView", "Error: Failed to get UIBehaviour. Check LoopListView2 Inspector settings.");
				return null;
			}
			((UIBaseView)value).Inject(component);
			value.Init(OnItemClicked);
			mItemViewCache[((Component)item).gameObject] = value;
		}
		return value;
	}

	private void OnItemClicked(string languageKey)
	{
		OnLanguageChanged?.Invoke(languageKey);
	}

	private bool IsLanguageMatch(SettingLanguage setting, string currentCode)
	{
		if (setting.AllLanguageCode == null)
		{
			return false;
		}
		string[] allLanguageCode = setting.AllLanguageCode;
		for (int i = 0; i < allLanguageCode.Length; i++)
		{
			if (allLanguageCode[i] == currentCode)
			{
				return true;
			}
		}
		return false;
	}

	private int GetLanguageIndex(string currentCode)
	{
		for (int i = 0; i < mLanguageInitData.SettingLanguageList.Count; i++)
		{
			if (IsLanguageMatch(mLanguageInitData.SettingLanguageList[i], currentCode))
			{
				return i;
			}
		}
		return -1;
	}

	public void UpdateItemsSelection(string newLanguageCode)
	{
		mLanguageInitData.CurrentLanguage = newLanguageCode;
		ScrollView.RefreshAllShownItem();
		RefreshTextShow();
	}

	private void RefreshTextShow()
	{
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
		if (mItemViewCache != null)
		{
			foreach (KeyValuePair<GameObject, UILanguageItemView> item in mItemViewCache)
			{
				if (item.Value != null)
				{
					((UIBaseView)item.Value).Dispose();
				}
			}
			mItemViewCache = null;
		}
		mLanguageInitData = null;
		mIsInit = false;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		btn_close = behaviour.Get<Button>("btn_close");
		ScrollView = behaviour.Get<LoopListView2>("ScrollView");
		objItem = behaviour.Get<GameObject>("objItem");
	}
}
