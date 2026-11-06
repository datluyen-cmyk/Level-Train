using System;
using Example.Cfg;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace Game.UI.Language;

public class UILanguageItemView : UIBaseView
{
	private SettingLanguage settingLanguage;

	public Button btn_language;

	public GameObject normal;

	public GameObject select;

	public TMP_Text txt_lang;

	public void Init(Action<string> onLanguageSelected)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if ((Object)(object)btn_language != (Object)null)
		{
			((UnityEvent)btn_language.onClick).AddListener((UnityAction)delegate
			{
				onLanguageSelected(settingLanguage.Key);
			});
		}
	}

	public void SetData(SettingLanguage settingLanguage, bool isSelected)
	{
		this.settingLanguage = settingLanguage;
		RefreshText();
		SetSelected(isSelected);
	}

	public void SetSelected(bool isSelected)
	{
		if ((Object)(object)normal != (Object)null)
		{
			normal.SetActive(!isSelected);
		}
		if ((Object)(object)select != (Object)null)
		{
			select.SetActive(isSelected);
		}
	}

	public void RefreshText()
	{
		if (settingLanguage != null && !((Object)(object)txt_lang == (Object)null))
		{
			TMPTextExtensions.SetLocalizedTextValue(txt_lang, settingLanguage.Key);
			txt_lang.ForceMeshUpdate(true, true);
		}
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
		settingLanguage = null;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		btn_language = behaviour.Get<Button>("btn_language");
		normal = behaviour.Get<GameObject>("normal");
		select = behaviour.Get<GameObject>("select");
		txt_lang = behaviour.Get<TMP_Text>("txt_lang");
	}
}
