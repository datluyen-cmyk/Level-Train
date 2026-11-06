using TMPro;
using UniLog;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.Lang;

namespace _03_Scripts.Hot.Common.UI;

[RequireComponent(typeof(TMP_Text))]
public class LocalizationLang : MonoBehaviour
{
	[Tooltip("配置表中的语言Key")]
	public string textKey;

	[Tooltip("是否禁用RTL支持（如阿拉伯语AR，希伯来语HE）")]
	public bool disableRTL;

	private TMP_Text label;

	private object[] textArgs;

	private string rawText = string.Empty;

	public string OldLanguage;

	private void Awake()
	{
		label = ((Component)this).GetComponent<TMP_Text>();
		if ((Object)(object)label == (Object)null)
		{
			Log.LogError("XYFW-LangBinder", "LangBinder: TMP_Text not found!");
		}
	}

	private void OnEnable()
	{
		if (OldLanguage != Singleton<LangManager>.Instance.CurrentLanguage)
		{
			RefreshText();
			OldLanguage = Singleton<LangManager>.Instance.CurrentLanguage;
		}
	}

	private void OnValidate()
	{
		if (Application.isPlaying)
		{
			if ((Object)(object)label == (Object)null)
			{
				label = ((Component)this).GetComponent<TMP_Text>();
			}
			RefreshText();
		}
	}

	public void RefreshText()
	{
		if (!((Object)(object)label == (Object)null))
		{
			string text = rawText;
			if (!string.IsNullOrEmpty(textKey))
			{
				text = Singleton<LangManager>.Instance.GetText(textKey, textArgs);
			}
			else if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(label.text))
			{
				rawText = label.text;
				text = rawText;
			}
			ApplyText(text);
		}
	}

	private void ApplyText(string text)
	{
		rawText = text;
		if (!((Object)(object)label == (Object)null))
		{
			if (!disableRTL && IsRtLLanguage() && !string.IsNullOrEmpty(text))
			{
				label.text = RtlTextFormatter.FormatRtlText(text);
				label.isRightToLeftText = true;
			}
			else
			{
				label.text = text;
				label.isRightToLeftText = false;
			}
		}
	}

	private static bool IsRtLLanguage()
	{
		string currentLanguage = Singleton<LangManager>.Instance.CurrentLanguage;
		if (!(currentLanguage == "AR"))
		{
			return currentLanguage == "HE";
		}
		return true;
	}
}
