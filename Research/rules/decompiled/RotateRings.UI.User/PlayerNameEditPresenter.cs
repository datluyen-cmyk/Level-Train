using System;
using RotateRings.Common;
using RotateRings.UI.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.User;

public sealed class PlayerNameEditPresenter : IDisposable
{
	private TMP_InputField inputField;

	private TMP_Text displayNameLabel;

	private Button btnRandom;

	private Button btnEdit;

	private Button btnConvert;

	private Action<string, bool> onSaveName;

	private Action onEnterEditMode;

	private bool isEditing;

	private bool isBound;

	private string displayName = string.Empty;

	private bool skipAutoFocusThisSession;

	public bool IsEditing => isEditing;

	public void Bind(TMP_InputField input, Button random, Button edit, Button convert, Action<string, bool> saveName, Action onEnterEditMode = null)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		Unbind();
		inputField = input;
		btnRandom = random;
		btnEdit = edit;
		btnConvert = convert;
		onSaveName = saveName;
		this.onEnterEditMode = onEnterEditMode;
		if ((Object)(object)inputField != (Object)null)
		{
			inputField.characterLimit = 12;
			TMP_InputField obj = inputField;
			obj.onValidateInput = (OnValidateInput)Delegate.Combine((Delegate?)(object)obj.onValidateInput, (Delegate?)new OnValidateInput(OnValidateInputChar));
			((UnityEvent<string>)(object)inputField.onSelect).AddListener((UnityAction<string>)OnInputFieldSelect);
			((UnityEvent<string>)(object)inputField.onEndEdit).AddListener((UnityAction<string>)OnInputEndEdit);
		}
		if ((Object)(object)btnRandom != (Object)null)
		{
			((UnityEvent)btnRandom.onClick).AddListener(new UnityAction(OnRandomClick));
		}
		if ((Object)(object)btnEdit != (Object)null)
		{
			((UnityEvent)btnEdit.onClick).AddListener(new UnityAction(OnEditClick));
		}
		if ((Object)(object)btnConvert != (Object)null)
		{
			((UnityEvent)btnConvert.onClick).AddListener(new UnityAction(OnConvertClick));
		}
		isBound = true;
		RefreshMode();
	}

	public void SetDisplayLabel(TMP_Text label)
	{
		displayNameLabel = label;
		RefreshDisplayTexts();
	}

	public void Refresh(string playerName)
	{
		if (!string.IsNullOrEmpty(playerName))
		{
			displayName = playerName;
			if (!isEditing)
			{
				ClearDraft();
			}
		}
		else
		{
			displayName = string.Empty;
			TryRestoreDraft();
		}
		if (!isEditing)
		{
			RefreshDisplayTexts();
		}
		RefreshMode();
	}

	public void RefreshMode()
	{
		bool flag = UserInfoService.IsEnglishFreeNamingEnabled();
		if ((Object)(object)btnEdit != (Object)null)
		{
			((Component)btnEdit).gameObject.SetActive(flag);
		}
		if ((Object)(object)btnRandom != (Object)null)
		{
			((Component)btnRandom).gameObject.SetActive(!flag);
		}
		if (!((Object)(object)inputField == (Object)null))
		{
			if (!flag && isEditing)
			{
				EndEdit();
			}
			((Selectable)inputField).interactable = flag;
		}
	}

	public bool BeginEdit()
	{
		if ((Object)(object)inputField == (Object)null || !UserInfoService.IsEnglishFreeNamingEnabled())
		{
			return false;
		}
		if (isEditing)
		{
			inputField.ActivateInputField();
			((Selectable)inputField).Select();
			return true;
		}
		isEditing = true;
		inputField.DeactivateInputField(false);
		inputField.text = (string.IsNullOrEmpty(displayName) ? PlayerService.GetName() : displayName);
		inputField.ActivateInputField();
		((Selectable)inputField).Select();
		onEnterEditMode?.Invoke();
		return true;
	}

	public void EndEdit()
	{
		if (isEditing)
		{
			isEditing = false;
			if ((Object)(object)inputField != (Object)null)
			{
				inputField.DeactivateInputField(false);
			}
			RefreshDisplayTexts();
			RefreshMode();
		}
	}

	public string GetDisplayName()
	{
		if (isEditing && (Object)(object)inputField != (Object)null)
		{
			return inputField.text?.Trim() ?? displayName;
		}
		return displayName;
	}

	public bool TryCommit(bool endEditAfterSuccess = true)
	{
		if (!isEditing)
		{
			return true;
		}
		TMP_InputField obj = inputField;
		string text = ((obj != null) ? obj.text : null);
		if (!UserInfoService.TryValidate(text, out var _))
		{
			RevertToSavedName(endEditAfterSuccess);
			return true;
		}
		string text2 = text.Trim();
		if (string.Equals(text2, displayName, StringComparison.Ordinal))
		{
			if (endEditAfterSuccess)
			{
				EndEdit();
			}
			ClearDraft();
			return true;
		}
		ClearDraft();
		onSaveName?.Invoke(text2, arg2: false);
		SyncDisplayFromPlayerService();
		if (endEditAfterSuccess)
		{
			EndEdit();
		}
		else
		{
			RefreshDisplayTexts();
		}
		return true;
	}

	public void TryAutoFocusOnFirstOpen()
	{
		if (!skipAutoFocusThisSession && UserInfoService.ShouldAutoFocusNameInputOnFirstOpen() && !UserInfoService.HasConsumedFirstNameInputFocus() && UserInfoService.IsEnglishFreeNamingEnabled() && BeginEdit())
		{
			UserInfoService.MarkConsumedFirstNameInputFocus();
		}
	}

	public void SuppressAutoFocusForSession()
	{
		skipAutoFocusThisSession = true;
	}

	public void OnLanguageChanged()
	{
		EndEdit();
		RefreshMode();
	}

	public void PersistDraftIfEditing()
	{
		if (isEditing && !((Object)(object)inputField == (Object)null))
		{
			GameApp.Save.GetOrCreate<SettingData>().NameEditDraft = inputField.text ?? string.Empty;
		}
	}

	public void Dispose()
	{
		Unbind();
	}

	private void Unbind()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		if (isBound)
		{
			if ((Object)(object)inputField != (Object)null)
			{
				TMP_InputField obj = inputField;
				obj.onValidateInput = (OnValidateInput)Delegate.Remove((Delegate?)(object)obj.onValidateInput, (Delegate?)new OnValidateInput(OnValidateInputChar));
				((UnityEvent<string>)(object)inputField.onSelect).RemoveListener((UnityAction<string>)OnInputFieldSelect);
				((UnityEvent<string>)(object)inputField.onEndEdit).RemoveListener((UnityAction<string>)OnInputEndEdit);
			}
			if ((Object)(object)btnRandom != (Object)null)
			{
				((UnityEvent)btnRandom.onClick).RemoveListener(new UnityAction(OnRandomClick));
			}
			if ((Object)(object)btnEdit != (Object)null)
			{
				((UnityEvent)btnEdit.onClick).RemoveListener(new UnityAction(OnEditClick));
			}
			if ((Object)(object)btnConvert != (Object)null)
			{
				((UnityEvent)btnConvert.onClick).RemoveListener(new UnityAction(OnConvertClick));
			}
			inputField = null;
			btnRandom = null;
			btnEdit = null;
			btnConvert = null;
			onSaveName = null;
			onEnterEditMode = null;
			isBound = false;
			isEditing = false;
		}
	}

	private char OnValidateInputChar(string text, int charIndex, char addedChar)
	{
		if (!UserInfoService.IsAllowedChar(addedChar))
		{
			return '\0';
		}
		return addedChar;
	}

	private void OnRandomClick()
	{
		string arg = (UserInfoService.IsEnglishFreeNamingEnabled() ? RoleNameGenerator.GenerateRandomEnglishName() : PlayerService.GenerateRandomName());
		onSaveName?.Invoke(arg, arg2: true);
		ClearDraft();
		SyncDisplayFromPlayerService();
	}

	private void OnEditClick()
	{
		BeginEdit();
	}

	private void OnInputFieldSelect(string _)
	{
		if (!isEditing)
		{
			BeginEdit();
		}
	}

	private void OnInputEndEdit(string _)
	{
		TryCommit();
	}

	private void OnConvertClick()
	{
		if (!GameApp.UI.IsOpen("UIUser"))
		{
			TryCommit();
			UserSettingService.MarkUserSettingOpened();
			GameApp.UI.Open("UIUser", (UILayer)2, (object)null, (Action)null);
		}
	}

	private void SyncDisplayFromPlayerService()
	{
		displayName = PlayerService.GetName();
		RefreshDisplayTexts();
	}

	private void RefreshDisplayTexts()
	{
		string text = (string.IsNullOrEmpty(displayName) ? PlayerService.GetName() : displayName);
		if ((Object)(object)displayNameLabel != (Object)null)
		{
			displayNameLabel.text = text;
		}
		if ((Object)(object)inputField != (Object)null && !isEditing)
		{
			inputField.text = text;
		}
	}

	private void RevertToSavedName(bool endEditAfterSuccess)
	{
		ClearDraft();
		string text = (string.IsNullOrEmpty(displayName) ? PlayerService.GetName() : displayName);
		if ((Object)(object)inputField != (Object)null)
		{
			inputField.text = text;
		}
		if ((Object)(object)displayNameLabel != (Object)null)
		{
			displayNameLabel.text = text;
		}
		if (endEditAfterSuccess)
		{
			EndEdit();
		}
	}

	private void TryRestoreDraft()
	{
		SettingData orCreate = GameApp.Save.GetOrCreate<SettingData>();
		if (!string.IsNullOrEmpty(orCreate.NameEditDraft) && UserInfoService.IsRestorableDraft(orCreate.NameEditDraft))
		{
			displayName = orCreate.NameEditDraft.Trim();
		}
	}

	private static void ClearDraft()
	{
		GameApp.Save.GetOrCreate<SettingData>().NameEditDraft = string.Empty;
	}
}
