using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;

namespace RotateRings.UI.Privacy;

public class UIPrivacyView : MonoBehaviour
{
	private const string LinkIdTerms = "terms";

	private const string LinkIdPrivacy = "privacy";

	private const string OpenAnimName = "anim_uiprivacy_open";

	private const string CloseAnimName = "anim_uiprivacy_close";

	private const string SpineAnimStart = "start";

	private const string SpineAnimIdle = "idle";

	private const int SpineTrackIndex = 0;

	[SerializeField]
	private TMP_Text txtDesc;

	[SerializeField]
	private Button btnAccept;

	[SerializeField]
	private Animation animPrivacy;

	[SerializeField]
	private SkeletonGraphic spinePrivacyCompliance;

	private EventTrigger descEventTrigger;

	private Entry descClickEntry;

	private Action onTermsLink;

	private Action onPrivacyLink;

	private TrackEntry spineStartTrackEntry;

	private TrackEntryDelegate spineStartCompleteHandler;

	public Button BtnAccept => btnAccept;

	public void PlayOpenAnimation()
	{
		PlayAnimation("anim_uiprivacy_open");
		PlaySpineStartIdle();
	}

	public float PlayCloseAnimation()
	{
		return PlayAnimation("anim_uiprivacy_close");
	}

	private float PlayAnimation(string animName)
	{
		if ((Object)(object)animPrivacy == (Object)null)
		{
			return 0f;
		}
		AnimationState val = animPrivacy[animName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return 0f;
		}
		if (animPrivacy.IsPlaying(animName))
		{
			animPrivacy.Stop(animName);
			val.time = 0f;
			animPrivacy.Sample();
		}
		animPrivacy.Play(animName);
		return val.length;
	}

	private void PlaySpineStartIdle()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		if ((Object)(object)spinePrivacyCompliance == (Object)null)
		{
			return;
		}
		spinePrivacyCompliance.Initialize(false);
		UnsubscribeSpineStartComplete();
		spinePrivacyCompliance.freeze = false;
		AnimationState animationState = spinePrivacyCompliance.AnimationState;
		if (animationState == null)
		{
			return;
		}
		spineStartTrackEntry = animationState.SetAnimation(0, "start", false);
		spineStartCompleteHandler = (TrackEntryDelegate)delegate
		{
			if (!((Object)(object)spinePrivacyCompliance == (Object)null))
			{
				spinePrivacyCompliance.AnimationState.SetAnimation(0, "idle", true);
			}
		};
		spineStartTrackEntry.Complete += spineStartCompleteHandler;
	}

	private void UnsubscribeSpineStartComplete()
	{
		if (spineStartTrackEntry != null && spineStartCompleteHandler != null)
		{
			spineStartTrackEntry.Complete -= spineStartCompleteHandler;
		}
		spineStartTrackEntry = null;
		spineStartCompleteHandler = null;
	}

	public void RefreshPrivacyDescription()
	{
		if (!((Object)(object)txtDesc == (Object)null))
		{
			string text = EscapeTmpPlain(GameApp.Lang.GetText("Common_Terms_Terms", Array.Empty<object>()));
			string text2 = EscapeTmpPlain(GameApp.Lang.GetText("Common_Terms_Policy", Array.Empty<object>()));
			string arg = "<link=\"terms\">" + text + "</link>";
			string arg2 = "<link=\"privacy\">" + text2 + "</link>";
			string text3 = GameApp.Lang.GetText("Common_Privacy", Array.Empty<object>());
			txtDesc.richText = true;
			((Graphic)txtDesc).raycastTarget = true;
			TMPTextExtensions.SetLocalizedTextValue(txtDesc, string.Format(text3, arg, arg2));
			txtDesc.ForceMeshUpdate(true, false);
		}
	}

	public void RegisterDescLinkClick(Action onTermsClick, Action onPrivacyClick)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		UnregisterDescLinkClick();
		onTermsLink = onTermsClick;
		onPrivacyLink = onPrivacyClick;
		if (!((Object)(object)txtDesc == (Object)null))
		{
			descEventTrigger = ((Component)txtDesc).GetComponent<EventTrigger>();
			if ((Object)(object)descEventTrigger == (Object)null)
			{
				descEventTrigger = ((Component)txtDesc).gameObject.AddComponent<EventTrigger>();
			}
			if (descEventTrigger.triggers == null)
			{
				descEventTrigger.triggers = new List<Entry>();
			}
			descClickEntry = new Entry
			{
				eventID = (EventTriggerType)4
			};
			((UnityEvent<BaseEventData>)(object)descClickEntry.callback).AddListener((UnityAction<BaseEventData>)OnDescPointerClick);
			descEventTrigger.triggers.Add(descClickEntry);
		}
	}

	public void UnregisterDescLinkClick()
	{
		if (descClickEntry != null && (Object)(object)descEventTrigger != (Object)null)
		{
			((UnityEvent<BaseEventData>)(object)descClickEntry.callback).RemoveListener((UnityAction<BaseEventData>)OnDescPointerClick);
			descEventTrigger.triggers.Remove(descClickEntry);
			descClickEntry = null;
		}
		descEventTrigger = null;
		onTermsLink = null;
		onPrivacyLink = null;
	}

	private void OnDestroy()
	{
		UnregisterDescLinkClick();
		UnsubscribeSpineStartComplete();
	}

	private void OnDescPointerClick(BaseEventData baseEventData)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)txtDesc == (Object)null)
		{
			return;
		}
		PointerEventData val = (PointerEventData)(object)((baseEventData is PointerEventData) ? baseEventData : null);
		if (val == null)
		{
			return;
		}
		int num = TMP_TextUtilities.FindIntersectingLink(txtDesc, Vector2.op_Implicit(val.position), val.pressEventCamera);
		if (num >= 0 && txtDesc.textInfo.linkInfo != null && num < txtDesc.textInfo.linkCount)
		{
			string linkID = ((TMP_LinkInfo)(ref txtDesc.textInfo.linkInfo[num])).GetLinkID();
			if (linkID == "terms")
			{
				onTermsLink?.Invoke();
			}
			else if (linkID == "privacy")
			{
				onPrivacyLink?.Invoke();
			}
		}
	}

	private static string EscapeTmpPlain(string raw)
	{
		if (string.IsNullOrEmpty(raw))
		{
			return string.Empty;
		}
		return raw.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
	}
}
