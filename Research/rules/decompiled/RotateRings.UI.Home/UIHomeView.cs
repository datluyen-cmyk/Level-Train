using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Halloween;
using RotateRings.Model;
using Spine.Unity;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Home;

public class UIHomeView : GameBaseView
{
	private const string HomeOpenAnim = "anim_uihome_in";

	private const string HomeCloseAnim = "anim_uihome_out";

	private const string LevelButtonEffectName = "eff_rank_btn_gp1";

	private const string LevelButtonEffectInstanceName = "eff_rank_btn_gp1(Clone)";

	private const string HalloweenHomeBgLocation = "HalloweenHomeBg";

	private const string HalloweenBtnOKLocation = "HalloweenBtnOK";

	private const string DefaultHomeLogoLocation = "UIHomeLogo";

	private const string HalloweenHomeLogoLocation = "UIHomeLogo_Halloween";

	private HomeActivityEntryHost activityEntryHost;

	private GameObject levelButtonEffect;

	private GameObject halloweenHomeBg;

	private GameObject halloweenBtnOK;

	private bool isHalloweenBtnOKAttached;

	private int defaultBtnOKSiblingIndex = -1;

	private WidgetHandle logoHandle;

	private WidgetHandle btnOKHandle;

	private WidgetHandle btnAvatarHandle;

	public GameObject UIGameBg;

	public GameObject UIPanel;

	public RectTransform ContentLeft;

	public RectTransform ContentRight;

	public Button btnSetting;

	public RectTransform ButtonGroup;

	public UIBehaviour btnOK;

	public Animation animHome;

	public RectTransform logo_home;

	public UIBehaviour btnAvatar;

	public Button btnShop;

	public SkeletonGraphic btnShopSpine;

	public UIHomeBtnOKView BtnOKView => btnOKHandle?.GetView<UIHomeBtnOKView>();

	public UIHomeBtnAvatarView BtnAvatarView => btnAvatarHandle?.GetView<UIHomeBtnAvatarView>();

	public void SetShopEntryVisible(bool visible)
	{
		if ((Object)(object)btnShop != (Object)null)
		{
			((Component)btnShop).gameObject.SetActive(visible);
		}
		if ((Object)(object)btnShopSpine != (Object)null)
		{
			((Component)btnShopSpine).gameObject.SetActive(visible);
		}
	}

	public void InitHalloweenHomeBackground()
	{
		ShutdownHalloweenHomeBackground();
		if (!HalloweenFeatureService.IsHomeEnabled())
		{
			return;
		}
		GameObject val = LoadAsset<GameObject>("HalloweenHomeBg");
		if (!((Object)(object)val == (Object)null))
		{
			halloweenHomeBg = Object.Instantiate<GameObject>(val, UIPanel.transform, false);
			halloweenHomeBg.transform.SetAsFirstSibling();
			if ((Object)(object)UIGameBg != (Object)null)
			{
				UIGameBg.SetActive(false);
			}
		}
	}

	public void ShutdownHalloweenHomeBackground()
	{
		if ((Object)(object)halloweenHomeBg != (Object)null)
		{
			Object.Destroy((Object)(object)halloweenHomeBg);
			halloweenHomeBg = null;
		}
		if ((Object)(object)UIGameBg != (Object)null)
		{
			UIGameBg.SetActive(true);
		}
	}

	public void InitActivityEntries()
	{
		ShutdownActivityEntries();
		activityEntryHost = new HomeActivityEntryHost();
		activityEntryHost.RegisterDefaults();
		activityEntryHost.MountAll(this, ContentLeft, ContentRight);
		activityEntryHost.BindAll();
	}

	public void ShutdownActivityEntries()
	{
		activityEntryHost?.UnbindAll();
		activityEntryHost?.UnmountAll();
		activityEntryHost?.ClearEntries();
		activityEntryHost = null;
	}

	public void RefreshActivityEntries()
	{
		activityEntryHost?.RefreshAll();
	}

	public void StartIconSpineLoop()
	{
		activityEntryHost?.StartIconSpineLoop((MonoBehaviour)(object)((UIBaseView)this).Behaviour);
	}

	public void StopIconSpineLoop()
	{
		activityEntryHost?.StopIconSpineLoop();
	}

	public void RefreshAvatarEntry(bool visible)
	{
		BtnAvatarView?.SetVisible(visible);
	}

	public void RefreshAvatarHead()
	{
		BtnAvatarView?.RefreshHead(PlayerService.GetHeadSprite(), PlayerService.GetHeadFrameSprite());
	}

	public void InitBtnAvatar()
	{
		ShutdownBtnAvatar();
		if (!((Object)(object)btnAvatar == (Object)null))
		{
			btnAvatarHandle = AttachWidget<UIHomeBtnAvatarView>(btnAvatar);
			if (btnAvatarHandle == null)
			{
				Log.LogError("UIHome", "[UIHomeView] AttachWidget UIHomeBtnAvatarView failed");
			}
		}
	}

	public void ShutdownBtnAvatar()
	{
		if (btnAvatarHandle != null)
		{
			DetachWidget(btnAvatarHandle);
			btnAvatarHandle = null;
		}
	}

	public void InitBtnOK(bool playIdleAnimation)
	{
		ShutdownBtnOK();
		if ((Object)(object)btnOK == (Object)null)
		{
			return;
		}
		if (HalloweenFeatureService.IsHomeEnabled())
		{
			AttachHalloweenBtnOK();
		}
		if (btnOKHandle == null)
		{
			btnOKHandle = AttachWidget<UIHomeBtnOKView>(btnOK);
			if (btnOKHandle == null)
			{
				Log.LogError("UIHome", "[UIHomeView] AttachWidget UIHomeBtnOKView failed");
			}
		}
		BtnOKView?.Initialize(playIdleAnimation);
	}

	public void ShutdownBtnOK()
	{
		if (btnOKHandle != null)
		{
			DetachWidget(btnOKHandle);
			btnOKHandle = null;
		}
		if (!isHalloweenBtnOKAttached)
		{
			return;
		}
		if (isHalloweenBtnOKAttached && (Object)(object)halloweenBtnOK != (Object)null)
		{
			Object.Destroy((Object)(object)halloweenBtnOK);
		}
		halloweenBtnOK = null;
		isHalloweenBtnOKAttached = false;
		if ((Object)(object)btnOK == (Object)null)
		{
			defaultBtnOKSiblingIndex = -1;
			return;
		}
		((Component)btnOK).gameObject.SetActive(true);
		if (defaultBtnOKSiblingIndex >= 0 && (Object)(object)((Component)btnOK).transform.parent != (Object)null)
		{
			((Component)btnOK).transform.SetSiblingIndex(Mathf.Min(defaultBtnOKSiblingIndex, ((Component)btnOK).transform.parent.childCount - 1));
		}
		defaultBtnOKSiblingIndex = -1;
	}

	private void AttachHalloweenBtnOK()
	{
		Transform transform = ((Component)btnOK).transform;
		Transform parent = transform.parent;
		if ((Object)(object)parent == (Object)null)
		{
			return;
		}
		GameObject val = LoadAsset<GameObject>("HalloweenBtnOK");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		defaultBtnOKSiblingIndex = transform.GetSiblingIndex();
		((Component)transform).gameObject.SetActive(false);
		GameObject val2 = (halloweenBtnOK = Object.Instantiate<GameObject>(val, parent, false));
		val2.transform.SetSiblingIndex(defaultBtnOKSiblingIndex);
		UIBehaviour component = val2.GetComponent<UIBehaviour>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("UIHome", "[UIHomeView] Halloween btnOK prefab missing UIBehaviour");
			Object.Destroy((Object)(object)val2);
			halloweenBtnOK = null;
			((Component)transform).gameObject.SetActive(true);
			defaultBtnOKSiblingIndex = -1;
			return;
		}
		btnOKHandle = AttachWidget<UIHomeBtnOKView>(component);
		if (btnOKHandle == null)
		{
			Log.LogError("UIHome", "[UIHomeView] AttachWidget Halloween UIHomeBtnOKView failed");
			Object.Destroy((Object)(object)val2);
			halloweenBtnOK = null;
			((Component)transform).gameObject.SetActive(true);
			defaultBtnOKSiblingIndex = -1;
		}
		else
		{
			isHalloweenBtnOKAttached = true;
		}
	}

	public void RefreshPlayButton(ELevelDifficulty difficulty, bool hasUnfinishedLevel, GameMode gameMode, int level)
	{
		BtnOKView?.Refresh(difficulty, hasUnfinishedLevel, gameMode, level);
	}

	public float PlayHomeOpenAnimation()
	{
		return PlayAnimationClip(animHome, "anim_uihome_in");
	}

	public float PlayHomeCloseAnimation()
	{
		return PlayAnimationClip(animHome, "anim_uihome_out");
	}

	public float GetHomeCloseAnimationDuration()
	{
		return GetAnimationClipDuration(animHome, "anim_uihome_out");
	}

	private static float GetAnimationClipDuration(Animation anim, string animName)
	{
		if ((Object)(object)anim == (Object)null || string.IsNullOrEmpty(animName))
		{
			return 0f;
		}
		AnimationState val = anim[animName];
		if (!((TrackedReference)(object)val != (TrackedReference)null))
		{
			return 0f;
		}
		return val.length;
	}

	private static float PlayAnimationClip(Animation anim, string animName)
	{
		if ((Object)(object)anim == (Object)null || string.IsNullOrEmpty(animName))
		{
			return 0f;
		}
		AnimationState val = anim[animName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return 0f;
		}
		if (anim.IsPlaying(animName))
		{
			anim.Stop(animName);
			val.time = 0f;
			anim.Sample();
		}
		val.wrapMode = (WrapMode)8;
		anim.Play(animName);
		return val.length;
	}

	public void InitLogo()
	{
		ShutdownLogo();
		if (!((Object)(object)logo_home == (Object)null))
		{
			string text = (HalloweenFeatureService.IsHomeEnabled() ? "UIHomeLogo_Halloween" : "UIHomeLogo");
			logoHandle = AttachWidget<UIHomeLogoView>(text, (Transform)(object)logo_home);
			if (logoHandle == null)
			{
				Log.LogError("UIHome", "[UIHomeView] AttachWidget {0} failed", new object[1] { text });
			}
			else
			{
				UIHomeLogoView view = logoHandle.GetView<UIHomeLogoView>();
				view?.SetAlphaImmediate(1f);
				view?.PlayOpenAnimation();
			}
		}
	}

	public void FadeOutLogo(float duration, Action onComplete)
	{
		logoHandle?.GetView<UIHomeLogoView>()?.FadeOut(duration, onComplete);
	}

	public void ShutdownLogo()
	{
		if (logoHandle != null)
		{
			DetachWidget(logoHandle);
			logoHandle = null;
		}
	}

	public bool PlayLevelButtonEffect()
	{
		UIHomeBtnOKView btnOKView = BtnOKView;
		object obj;
		if (btnOKView == null)
		{
			obj = null;
		}
		else
		{
			Button obj2 = btnOKView.btnOK;
			obj = ((obj2 != null) ? ((Component)obj2).transform : null);
		}
		RectTransform val = (RectTransform)((obj is RectTransform) ? obj : null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		if ((Object)(object)levelButtonEffect == (Object)null)
		{
			Transform val2 = ((Transform)val).Find("eff_rank_btn_gp1(Clone)");
			levelButtonEffect = (((Object)(object)val2 != (Object)null) ? ((Component)val2).gameObject : CreateLevelButtonEffect(val));
		}
		if ((Object)(object)levelButtonEffect == (Object)null)
		{
			return false;
		}
		levelButtonEffect.SetActive(true);
		PlayUIParticleSystems(levelButtonEffect);
		return true;
	}

	private GameObject CreateLevelButtonEffect(RectTransform target)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = LoadAsset<GameObject>("eff_rank_btn_gp1");
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		GameObject obj = Object.Instantiate<GameObject>(val, (Transform)(object)target, false);
		((Object)obj).name = "eff_rank_btn_gp1(Clone)";
		obj.transform.localPosition = Vector3.zero;
		return obj;
	}

	private static void PlayUIParticleSystems(GameObject effect)
	{
		ParticleSystem[] componentsInChildren = effect.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Simulate(0f, false, true);
			componentsInChildren[i].Play();
		}
	}

	public override void Dispose()
	{
		StopIconSpineLoop();
		ShutdownLogo();
		ShutdownBtnOK();
		ShutdownBtnAvatar();
		ShutdownActivityEntries();
		ShutdownHalloweenHomeBackground();
		if ((Object)(object)levelButtonEffect != (Object)null)
		{
			Object.Destroy((Object)(object)levelButtonEffect);
			levelButtonEffect = null;
		}
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIGameBg = behaviour.Get<GameObject>("UIGameBg");
		UIPanel = behaviour.Get<GameObject>("UIPanel");
		ContentLeft = behaviour.Get<RectTransform>("ContentLeft");
		ContentRight = behaviour.Get<RectTransform>("ContentRight");
		btnSetting = behaviour.Get<Button>("btnSetting");
		ButtonGroup = behaviour.Get<RectTransform>("ButtonGroup");
		btnOK = behaviour.Get<UIBehaviour>("btnOK");
		animHome = behaviour.Get<Animation>("animHome");
		logo_home = behaviour.Get<RectTransform>("logo_home");
		btnAvatar = behaviour.Get<UIBehaviour>("btnAvatar");
		btnShop = behaviour.Get<Button>("btnShop");
		btnShopSpine = behaviour.Get<SkeletonGraphic>("btnShopSpine");
	}
}
