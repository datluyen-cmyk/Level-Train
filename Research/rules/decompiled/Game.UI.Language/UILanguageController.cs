using System;
using RotateRings.Common;
using RotateRings.Services;
using RotateRings.UI.Toast;
using UniLog;
using UnityEngine.Events;
using XingYun.Core.Singletons;
using XingYun.Font;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace Game.UI.Language;

[UIPath("UILanguage")]
public class UILanguageController : GameBaseController<UILanguageView>
{
	private const string LangListFontName = "LangListSdf";

	public UILanguageController(UILanguageView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((UnityEvent)((BaseController<UILanguageView>)this).view.btn_close.onClick).AddListener(new UnityAction(OnCloseClick));
		((BaseController<UILanguageView>)this).view.OnLanguageChanged = OnLanguageChanged;
		((BaseController<UILanguageView>)this).ListenEvent<LanguageInitSuccess>((Action<LanguageInitSuccess>)OnLanguageInitSuccess);
		((BaseController<UILanguageView>)this).ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UILanguageView>)this).ListenEvent<LanguageFailed>((Action<LanguageFailed>)OnLanguageFailed);
		FontManager instance = Singleton<FontManager>.Instance;
		if (instance != null)
		{
			instance.RegisterExemptFont("LangListSdf");
		}
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIToast", ref val) && val is UIToastController uIToastController)
		{
			uIToastController.RefreshVisibleText();
		}
		((BaseController<UILanguageView>)this).ExecuteCommand<LanguageInitCommand>((object)null);
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		((BaseController<UILanguageView>)this).view.UpdateItemsSelection(evt.LanguageCode);
	}

	private void OnLanguageInitSuccess(LanguageInitSuccess evt)
	{
		((BaseController<UILanguageView>)this).view.OnRefreshView(evt.languageInitData);
	}

	private void OnLanguageChanged(string languageKey)
	{
		AudioService.PlayUiSound(AudioKeys.Unlock.Value);
		((BaseController<UILanguageView>)this).ExecuteCommand<LanguageChangeCommand>((object)new LanguageChangeRequest
		{
			LanguageKey = languageKey
		});
	}

	private void OnCloseClick()
	{
		AudioService.PlayUiSound(AudioKeys.Unlock.Value);
		((BaseController<UILanguageView>)this).CloseView(false);
	}

	public override void OnRemove()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		((BaseController<UILanguageView>)this).ExecuteCommand<LanguageFlashCommand>((object)null);
		((UnityEvent)((BaseController<UILanguageView>)this).view.btn_close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((BaseController<UILanguageView>)this).view.OnLanguageChanged = null;
		((BaseController<UILanguageView>)this).UnlistenEvent<LanguageInitSuccess>((Action<LanguageInitSuccess>)OnLanguageInitSuccess);
		((BaseController<UILanguageView>)this).UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UILanguageView>)this).UnlistenEvent<LanguageFailed>((Action<LanguageFailed>)OnLanguageFailed);
		base.OnRemove();
	}

	private void OnLanguageFailed(LanguageFailed failed)
	{
		Log.LogError("UILanguageController", "语言切换失败: " + failed.ErrorMessage);
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UILanguageView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		((BaseController<UILanguageView>)this).CloseView(false);
	}
}
