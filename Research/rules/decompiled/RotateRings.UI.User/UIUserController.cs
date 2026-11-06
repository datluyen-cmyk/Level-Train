using System;
using System.Collections.Generic;
using Game.UI.Language;
using RotateRings.Common;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.User;

[UIPath("UIUser")]
[UIPath("UIUserV2")]
public class UIUserController : BaseController<UIUserView>
{
	private int selectedHeadId;

	private int selectedHeadFrameId;

	private List<UserIconData> headList;

	private PlayerNameEditPresenter namePresenter;

	public UIUserController(UIUserView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		((UnityEvent)base.view.bgMask.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.btn_close.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.btnbg.onClick).AddListener(new UnityAction(OnSaveClick));
		namePresenter = new PlayerNameEditPresenter();
		namePresenter.Bind(base.view.InputField, base.view.btnRandom, base.view.btnEdit, null, SaveNameFromPresenter);
		namePresenter.SetDisplayLabel(base.view.userName);
		base.ListenEvent<UserInitData>((Action<UserInitData>)OnUserInit);
		base.ListenEvent<UserSaveSuccess>((Action<UserSaveSuccess>)OnSaveSuccess);
		base.ListenEvent<UserNameSaveSuccess>((Action<UserNameSaveSuccess>)OnUserNameSaveSuccess);
		base.ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		base.ExecuteCommand<UserInitCommand>((object)null);
		RefreshNameFromPlayerService();
	}

	private void OnUserInit(UserInitData data)
	{
		selectedHeadId = data.SelectedHeadId;
		selectedHeadFrameId = data.SelectedHeadFrameId;
		headList = data.HeadList;
		base.view.Init(data.PlayerName, data.HeadList, selectedHeadId, OnHeadItemClick);
		namePresenter.Refresh(data.PlayerName);
		namePresenter.TryAutoFocusOnFirstOpen();
	}

	private void OnHeadItemClick(int headId)
	{
		if (selectedHeadId != headId)
		{
			selectedHeadId = headId;
			base.view.SetHeadSelected(selectedHeadId, headList);
		}
	}

	private void OnSaveClick()
	{
		if (namePresenter == null || namePresenter.TryCommit())
		{
			UserSaveRequest userSaveRequest = new UserSaveRequest
			{
				HeadId = selectedHeadId,
				HeadFrameId = selectedHeadFrameId,
				PlayerName = ((namePresenter != null) ? namePresenter.GetDisplayName() : PlayerService.GetName())
			};
			base.ExecuteCommand<UserSaveCommand>((object)userSaveRequest);
		}
	}

	private void OnSaveSuccess(UserSaveSuccess evt)
	{
		base.CloseView(false);
	}

	private void OnUserNameSaveSuccess(UserNameSaveSuccess evt)
	{
		if (namePresenter != null && namePresenter.IsEditing)
		{
			namePresenter.EndEdit();
		}
		RefreshNameDisplay(evt.PlayerName);
	}

	private void OnCloseClick()
	{
		if (namePresenter == null || namePresenter.TryCommit())
		{
			namePresenter?.PersistDraftIfEditing();
			base.CloseView(false);
		}
	}

	public override void OnActive(bool isActive)
	{
		base.OnActive(isActive);
		if (isActive)
		{
			RefreshNameFromPlayerService();
			return;
		}
		namePresenter?.PersistDraftIfEditing();
		namePresenter?.SuppressAutoFocusForSession();
	}

	private void SaveNameFromPresenter(string newName, bool skipValidation)
	{
		if (!string.Equals(newName, PlayerService.GetName(), StringComparison.Ordinal))
		{
			base.ExecuteCommand<UserSaveNameCommand>((object)new UserSaveNameRequest
			{
				PlayerName = newName,
				SkipValidation = skipValidation
			});
		}
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		namePresenter?.OnLanguageChanged();
		RefreshNameFromPlayerService();
	}

	private void RefreshNameFromPlayerService()
	{
		RefreshNameDisplay(PlayerService.GetName());
	}

	private void RefreshNameDisplay(string playerName)
	{
		base.view.RefreshPlayerName(playerName);
		namePresenter?.Refresh(playerName);
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		((UnityEvent)base.view.bgMask.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.btn_close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.btnbg.onClick).RemoveListener(new UnityAction(OnSaveClick));
		base.UnlistenEvent<UserInitData>((Action<UserInitData>)OnUserInit);
		base.UnlistenEvent<UserSaveSuccess>((Action<UserSaveSuccess>)OnSaveSuccess);
		base.UnlistenEvent<UserNameSaveSuccess>((Action<UserNameSaveSuccess>)OnUserNameSaveSuccess);
		base.UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		namePresenter?.Dispose();
		namePresenter = null;
	}

	public override void OnEscape()
	{
		if (namePresenter != null && namePresenter.IsEditing)
		{
			namePresenter.EndEdit();
		}
		else
		{
			base.CloseView(false);
		}
	}
}
