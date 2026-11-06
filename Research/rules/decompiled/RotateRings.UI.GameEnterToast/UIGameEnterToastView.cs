using System;
using System.Text.RegularExpressions;
using Coffee.UIExtensions;
using Game.UI;
using HsTileMahJong;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Core.Singletons;
using XingYun.Extensions;
using XingYun.Lang;
using XingYun.UI.Core;

namespace RotateRings.UI.GameEnterToast;

public class UIGameEnterToastView : GameBaseView
{
	private const string TAG = "[UIGameEnterToastView] ";

	private const string ShapeColorParticleEffectLocation = "eff_UIGameEnterToastView_prompt_01_gp5";

	private const string ColorBoarderRootRoleTypo = "ColorBoardeRoot";

	private const string AnimIn = "anim_UILevelEntryBannerView_in";

	private const string AnimOut = "anim_UILevelEntryBannerView_out";

	private const string BgMaskAnimIn = "anim_bgmask_open";

	private const string BgMaskAnimOut = "anim_bgmask_close";

	private const string LightNumberColorHex = "#389C41FF";

	private static readonly Regex LightColorTagRegex = new Regex("<color=[^>]+>", RegexOptions.Compiled);

	private int styleId = 1;

	private float halloweenSpineDuration;

	private bool halloweenSpineCompleted;

	private Action halloweenSpineCompleteCallback;

	private TrackEntryDelegate halloweenSpineCompleteHandler;

	private GameObject shapeColorParticleEffect;

	private ParticleSystem ColorBoarder;

	private const string HalloweenSpineRole = "eff_spine_transition_halloween";

	private const string ColorBoarderRootRole = "ColorBoarderRoot";

	public Animation Animation;

	public TMP_Text TxtContent;

	public Animation BgMaskAnimation;

	public Image BgMaskImage;

	public AutoPlaySpineAnimation HalloweenSpineAnimation;

	public GameObject ColorBoarderRoot;

	public void SetStyleId(int value)
	{
		styleId = ((value < 1 || value > 5) ? 1 : value);
	}

	public void SetLocalizedText(string langKey, object[] args)
	{
		if ((Object)(object)TxtContent == (Object)null || string.IsNullOrEmpty(langKey))
		{
			return;
		}
		int num = styleId;
		if (num == 3 || num == 4)
		{
			string text = Singleton<LangManager>.Instance.GetText(langKey, args);
			if (!string.IsNullOrEmpty(text) && text.IndexOf("<color=", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				text = LightColorTagRegex.Replace(text, "<color=#389C41FF>");
			}
			TMPTextExtensions.SetLocalizedTextValue(TxtContent, text);
		}
		else
		{
			TMPTextExtensions.SetLocalizedText(TxtContent, langKey, args);
		}
	}

	public void SetBgMaskVisible(bool visible)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)BgMaskImage == (Object)null))
		{
			Color color = ((Graphic)BgMaskImage).color;
			color.a = (visible ? 0.8f : 0f);
			((Graphic)BgMaskImage).color = color;
		}
	}

	public void PlayShapeColorParticle()
	{
		EnsureShapeColorParticleEffect();
		if (!((Object)(object)ColorBoarder == (Object)null))
		{
			GameObject gameObject = ((Component)ColorBoarder).gameObject;
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}
			WarnShapeColorParticleMeshMissing();
			ColorBoarder.Clear();
			ColorBoarder.Play();
		}
	}

	private void EnsureShapeColorParticleEffect()
	{
		if ((Object)(object)ColorBoarder != (Object)null || (Object)(object)shapeColorParticleEffect != (Object)null)
		{
			return;
		}
		Transform val = ResolveShapeColorParticleParent();
		if (!((Object)(object)val == (Object)null))
		{
			GameObject val2 = LoadAsset<GameObject>("eff_UIGameEnterToastView_prompt_01_gp5");
			if (!((Object)(object)val2 == (Object)null))
			{
				shapeColorParticleEffect = Object.Instantiate<GameObject>(val2, val, false);
				((Object)shapeColorParticleEffect).name = "eff_UIGameEnterToastView_prompt_01_gp5";
				Transform transform = shapeColorParticleEffect.transform;
				ApplyShapeColorParticleTransform((RectTransform)(object)((transform is RectTransform) ? transform : null));
				ColorBoarder = ResolveShapeColorParticle(shapeColorParticleEffect);
				_ = (Object)(object)ColorBoarder == (Object)null;
			}
		}
	}

	private static ParticleSystem ResolveShapeColorParticle(GameObject effect)
	{
		UIParticle componentInChildren = effect.GetComponentInChildren<UIParticle>(true);
		if ((Object)(object)componentInChildren != (Object)null && componentInChildren.particles.Count > 0)
		{
			return componentInChildren.particles[0];
		}
		Transform val = effect.transform.Find("color");
		if ((Object)(object)val != (Object)null)
		{
			ParticleSystem component = ((Component)val).GetComponent<ParticleSystem>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return effect.GetComponentInChildren<ParticleSystem>(true);
	}

	private Transform ResolveShapeColorParticleParent()
	{
		if ((Object)(object)ColorBoarderRoot != (Object)null)
		{
			return ColorBoarderRoot.transform;
		}
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null && ((UIBaseView)this).Behaviour.Contains("ColorBoardeRoot"))
		{
			GameObject val = ((UIBaseView)this).Behaviour.Get<GameObject>("ColorBoardeRoot");
			if ((Object)(object)val != (Object)null)
			{
				return val.transform;
			}
		}
		if (!((Object)(object)((UIBaseView)this).Behaviour != (Object)null))
		{
			return null;
		}
		return ((Component)((UIBaseView)this).Behaviour).transform;
	}

	private static void ApplyShapeColorParticleTransform(RectTransform rect)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rect == (Object)null))
		{
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = new Vector2(100f, 100f);
			rect.anchoredPosition = Vector2.zero;
			((Transform)rect).localRotation = Quaternion.identity;
			((Transform)rect).localScale = Vector3.one;
		}
	}

	private void WarnShapeColorParticleMeshMissing()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		ParticleSystemRenderer component = ((Component)ColorBoarder).GetComponent<ParticleSystemRenderer>();
		if (!((Object)(object)component == (Object)null) && (int)component.renderMode == 4)
		{
			_ = (Object)(object)component.mesh != (Object)null;
		}
	}

	private void ReleaseShapeColorParticleEffect()
	{
		if ((Object)(object)shapeColorParticleEffect != (Object)null)
		{
			Object.Destroy((Object)(object)shapeColorParticleEffect);
			shapeColorParticleEffect = null;
		}
		ColorBoarder = null;
	}

	public override void Dispose()
	{
		ReleaseShapeColorParticleEffect();
		((UIBaseView)this).Dispose();
	}

	public void StopShapeColorParticle()
	{
		if ((Object)(object)ColorBoarder != (Object)null)
		{
			ColorBoarder.Stop();
			ColorBoarder.Clear();
		}
		ReleaseShapeColorParticleEffect();
	}

	public void PlayBgMaskInAnimation()
	{
		PlayBgMaskAnimation("anim_bgmask_open");
	}

	public void PlayBgMaskOutAnimation()
	{
		PlayBgMaskAnimation("anim_bgmask_close");
	}

	private void PlayBgMaskAnimation(string clipName)
	{
		if ((Object)(object)BgMaskAnimation == (Object)null)
		{
			return;
		}
		AnimationState val = BgMaskAnimation[clipName];
		if (!((TrackedReference)(object)val == (TrackedReference)null))
		{
			if (BgMaskAnimation.IsPlaying(clipName))
			{
				BgMaskAnimation.Stop(clipName);
				val.time = 0f;
				BgMaskAnimation.Sample();
			}
			val.wrapMode = (WrapMode)8;
			BgMaskAnimation.Play(clipName);
		}
	}

	public void PlayInAnimation()
	{
		PlayAnimation("anim_UILevelEntryBannerView_in");
	}

	public void PlayHalloweenBannerSpine()
	{
		if (!((Object)(object)HalloweenSpineAnimation == (Object)null))
		{
			HalloweenSpineAnimation.PlaySpine();
			SetupHalloweenSpineCompletionTracking();
		}
	}

	public float GetHalloweenSpineDuration()
	{
		return halloweenSpineDuration;
	}

	public bool IsHalloweenSpinePlaying()
	{
		if (halloweenSpineDuration > 0f)
		{
			return !halloweenSpineCompleted;
		}
		return false;
	}

	public void RegisterHalloweenSpineComplete(Action onComplete)
	{
		halloweenSpineCompleteCallback = onComplete;
	}

	public void ClearHalloweenSpineComplete()
	{
		halloweenSpineCompleteCallback = null;
	}

	private void SetupHalloweenSpineCompletionTracking()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		halloweenSpineCompleted = false;
		halloweenSpineDuration = 0f;
		SkeletonGraphic val = (((Object)(object)HalloweenSpineAnimation != (Object)null) ? HalloweenSpineAnimation.skeletonGraphic : null);
		if (!((Object)(object)val == (Object)null) && val.SkeletonData != null && val.AnimationState != null)
		{
			Animation val2 = val.SkeletonData.FindAnimation(val.startingAnimation);
			halloweenSpineDuration = ((val2 != null) ? val2.Duration : 0f);
			if (halloweenSpineCompleteHandler == null)
			{
				halloweenSpineCompleteHandler = new TrackEntryDelegate(OnHalloweenSpineTrackComplete);
				val.AnimationState.Complete += halloweenSpineCompleteHandler;
			}
		}
	}

	private void OnHalloweenSpineTrackComplete(TrackEntry entry)
	{
		if (entry != null && entry.TrackIndex == 0)
		{
			halloweenSpineCompleted = true;
			Action action = halloweenSpineCompleteCallback;
			halloweenSpineCompleteCallback = null;
			action?.Invoke();
		}
	}

	public void PlayOutAnimation()
	{
		PlayAnimation("anim_UILevelEntryBannerView_out");
	}

	public float GetOutAnimationDuration()
	{
		if ((Object)(object)Animation == (Object)null)
		{
			return 0f;
		}
		AnimationClip clip = Animation.GetClip("anim_UILevelEntryBannerView_out");
		if (!((Object)(object)clip != (Object)null))
		{
			return 0f;
		}
		return clip.length;
	}

	public float GetBgMaskOutAnimationDuration()
	{
		if ((Object)(object)BgMaskAnimation == (Object)null)
		{
			return 0f;
		}
		AnimationClip clip = BgMaskAnimation.GetClip("anim_bgmask_close");
		if (!((Object)(object)clip != (Object)null))
		{
			return 0f;
		}
		return clip.length;
	}

	private void PlayAnimation(string clipName)
	{
		if ((Object)(object)Animation == (Object)null)
		{
			return;
		}
		AnimationState val = Animation[clipName];
		if (!((TrackedReference)(object)val == (TrackedReference)null))
		{
			if (Animation.IsPlaying(clipName))
			{
				Animation.Stop(clipName);
				val.time = 0f;
				Animation.Sample();
			}
			val.wrapMode = (WrapMode)8;
			Animation.Play(clipName);
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Animation = behaviour.Get<Animation>("Animation");
		TxtContent = behaviour.Get<TMP_Text>("TextContent");
		GameObject val = behaviour.Get<GameObject>("BgMask");
		if ((Object)(object)val != (Object)null)
		{
			BgMaskAnimation = val.GetComponent<Animation>();
			BgMaskImage = val.GetComponent<Image>();
		}
		if (behaviour.Contains("eff_spine_transition_halloween"))
		{
			HalloweenSpineAnimation = behaviour.Get<AutoPlaySpineAnimation>("eff_spine_transition_halloween");
			HalloweenSpineAnimation.autoPlayOnEnable = false;
			HalloweenSpineAnimation.StopSpine();
		}
		if (behaviour.Contains("ColorBoarderRoot"))
		{
			ColorBoarderRoot = behaviour.Get<GameObject>("ColorBoarderRoot");
		}
	}
}
