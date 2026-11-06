using System;
using RotateRings.Common;
using RotateRings.UI.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.User;

public class UIUser_NameTab : MonoBehaviour
{
	public Text nameText;

	public Button buttonEdit;

	public Button buttonRandom;

	public Image icon;

	public Image frame;

	public InputField inputField;

	public Button ButtonSettingUser;

	public Action<string> onNameChanged;

	public Func<bool> onBeforeOpenUIUser;

	public Action<string, bool> onRandomNameGenerated;

	public Action onEditClick;

	private bool isEditing;

	private bool isQuitting;

	private static bool applicationIsQuitting;

	private const int MaxNameLength = 12;

	public bool IsEditing => isEditing;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void ResetApplicationState()
	{
		applicationIsQuitting = false;
	}

	private void OnEnable()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		isQuitting = false;
		Application.quitting += HandleApplicationQuitting;
		if ((Object)(object)buttonEdit != (Object)null)
		{
			((UnityEvent)buttonEdit.onClick).AddListener(new UnityAction(HandleSettingUserClick));
		}
		if ((Object)(object)buttonRandom != (Object)null)
		{
			((UnityEvent)buttonRandom.onClick).AddListener(new UnityAction(HandleRandomClick));
		}
		if ((Object)(object)inputField != (Object)null)
		{
			((UnityEvent<string>)(object)inputField.onEndEdit).AddListener((UnityAction<string>)HandleInputEndEdit);
		}
		if ((Object)(object)ButtonSettingUser != (Object)null)
		{
			((UnityEvent)ButtonSettingUser.onClick).AddListener(new UnityAction(HandleSettingUserClick));
		}
	}

	private void OnDisable()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		Application.quitting -= HandleApplicationQuitting;
		if ((Object)(object)buttonEdit != (Object)null)
		{
			((UnityEvent)buttonEdit.onClick).RemoveListener(new UnityAction(HandleEditClick));
		}
		if ((Object)(object)buttonRandom != (Object)null)
		{
			((UnityEvent)buttonRandom.onClick).RemoveListener(new UnityAction(HandleRandomClick));
		}
		if ((Object)(object)inputField != (Object)null)
		{
			((UnityEvent<string>)(object)inputField.onEndEdit).RemoveListener((UnityAction<string>)HandleInputEndEdit);
		}
		if ((Object)(object)ButtonSettingUser != (Object)null)
		{
			((UnityEvent)ButtonSettingUser.onClick).RemoveListener(new UnityAction(HandleSettingUserClick));
		}
	}

	private void OnDestroy()
	{
		onEditClick = null;
		onNameChanged = null;
		onRandomNameGenerated = null;
		onBeforeOpenUIUser = null;
	}

	private void OnApplicationQuit()
	{
		HandleApplicationQuitting();
	}

	private void HandleApplicationQuitting()
	{
		applicationIsQuitting = true;
		isQuitting = true;
	}

	public void Refresh(string playerName, Sprite headSprite, Sprite frameSprite)
	{
		RefreshName(playerName);
		if ((Object)(object)inputField != (Object)null)
		{
			inputField.text = playerName ?? string.Empty;
		}
		RefreshHead(headSprite);
		RefreshFrame(frameSprite);
	}

	public void RefreshName(string playerName)
	{
		if ((Object)(object)nameText != (Object)null)
		{
			nameText.text = playerName;
		}
	}

	public void RefreshHead(Sprite headSprite)
	{
		if ((Object)(object)icon != (Object)null && (Object)(object)headSprite != (Object)null)
		{
			icon.sprite = headSprite;
		}
	}

	public void RefreshFrame(Sprite frameSprite)
	{
		if ((Object)(object)frame != (Object)null && (Object)(object)frameSprite != (Object)null)
		{
			frame.sprite = frameSprite;
		}
	}

	public void BeginEdit(string currentName)
	{
		if (!((Object)(object)inputField == (Object)null) && !isEditing)
		{
			isEditing = true;
			string text = currentName;
			if (string.IsNullOrEmpty(text))
			{
				text = GetDisplayedName();
			}
			inputField.characterLimit = 12;
			inputField.text = text;
			((Component)inputField).gameObject.SetActive(true);
			if ((Object)(object)nameText != (Object)null)
			{
				((Component)nameText).gameObject.SetActive(false);
			}
			inputField.ActivateInputField();
			((Selectable)inputField).Select();
		}
	}

	public void EndEdit()
	{
		if (isEditing)
		{
			isEditing = false;
			if ((Object)(object)inputField != (Object)null)
			{
				inputField.DeactivateInputField();
				((Component)inputField).gameObject.SetActive(false);
			}
			if ((Object)(object)nameText != (Object)null)
			{
				((Component)nameText).gameObject.SetActive(true);
			}
		}
	}

	public string GetInputText()
	{
		if ((Object)(object)inputField != (Object)null)
		{
			return inputField.text;
		}
		return string.Empty;
	}

	private string GetDisplayedName()
	{
		if ((Object)(object)nameText == (Object)null)
		{
			return string.Empty;
		}
		TMP_Text component = ((Component)nameText).GetComponent<TMP_Text>();
		if (!((Object)(object)component != (Object)null))
		{
			return string.Empty;
		}
		return component.text;
	}

	public void SetRedPointActive(bool active)
	{
	}

	public void SetSettingUserButtonActive(bool active)
	{
		if ((Object)(object)ButtonSettingUser != (Object)null)
		{
			((Component)ButtonSettingUser).gameObject.SetActive(active);
		}
	}

	private void HandleEditClick()
	{
		onEditClick?.Invoke();
	}

	private void HandleRandomClick()
	{
		string text = PlayerService.GenerateRandomName();
		RefreshName(text);
		if ((Object)(object)inputField != (Object)null)
		{
			inputField.text = text;
		}
		onRandomNameGenerated?.Invoke(text, arg2: true);
	}

	private void HandleSettingUserClick()
	{
		if (!isQuitting && !applicationIsQuitting && Application.isPlaying && !((Object)(object)UIRoot.Instance == (Object)null) && !GameApp.UI.IsOpen("UIUser") && (onBeforeOpenUIUser == null || onBeforeOpenUIUser()))
		{
			SetRedPointActive(active: false);
			UserSettingService.MarkUserSettingOpened();
			GameApp.UI.Open("UIUser", (UILayer)2, (object)null, (Action)null);
		}
	}

	private void HandleInputEndEdit(string text)
	{
		string text2 = text?.Trim();
		EndEdit();
		if (!string.IsNullOrEmpty(text2))
		{
			RefreshName(text2);
			onNameChanged?.Invoke(text2);
		}
	}
}
