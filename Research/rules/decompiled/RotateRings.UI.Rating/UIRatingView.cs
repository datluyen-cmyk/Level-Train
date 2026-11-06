using System;
using Basis.Common.UI;
using Game.UI;
using Rotaterings.Tools;
using Spine;
using Spine.Unity;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Rating;

public class UIRatingView : GameBaseView
{
	private const string TAG = "[Rating][UIRatingView] ";

	private const string OpenAnimName = "anim_popup_open";

	private const string IdleAnimName = "anim_UIRating_idle";

	private const string CloseAnimName = "anim_popup_close";

	private const int AnimalAnimationTrackIndex = 0;

	private const string HouOpenAnimation = "open_hou_01";

	private const string HouIdleAnimation = "idle_hou_01";

	private const string QianOpenAnimation = "open_qian_01";

	private const string QianIdleAnimation = "idle_qian_01";

	private UIPopupMaskFader maskFader;

	private GameObject[] starLights;

	private GameObject multiHouInstance;

	private GameObject multiQianInstance;

	public TMP_Text txtm_Desc;

	public Button btn_Star1;

	public Button btn_Star2;

	public Button btn_Star3;

	public Button btn_Star4;

	public Button btn_Star5;

	public GameObject img_StarLight1;

	public GameObject img_StarLight2;

	public GameObject img_StarLight3;

	public GameObject img_StarLight4;

	public GameObject img_StarLight5;

	public TMP_Text txtm_Title;

	public Button btn_Close;

	public Button btn_Submit;

	public Button btn_SubmitGray;

	public TMP_Text txt_Submit;

	public Animation ani_Panel;

	public RectTransform rect_SpineRoot;

	public UISpineLoader spine_RatingAnimal_Play;

	public SkeletonGraphic spineAni_Hou;

	public SkeletonGraphic spineAni_Qian;

	public RectTransform rect_SingleHou;

	public RectTransform rect_SingleQian;

	public RectTransform rect_MultiHou;

	public RectTransform rect_MultiQian;

	public void SetAnimalStyle(int animalStyle)
	{
		if ((Object)(object)maskFader == (Object)null)
		{
			maskFader = ((Component)((UIBaseView)this).Behaviour).GetComponent<UIPopupMaskFader>();
		}
		int animalStyle2 = RatingAnimalStyleResolver.Normalize(animalStyle);
		bool flag = !RatingAnimalStyleResolver.IsMultiAnimal(animalStyle2);
		ReleaseMultiAnimals();
		SetNodeActive(rect_SingleHou, flag);
		SetNodeActive(rect_SingleQian, flag);
		SetNodeActive(rect_MultiHou, !flag);
		SetNodeActive(rect_MultiQian, !flag);
		if (!flag)
		{
			multiHouInstance = SpawnMultiAnimal(animalStyle2, isHou: true, rect_MultiHou);
			multiQianInstance = SpawnMultiAnimal(animalStyle2, isHou: false, rect_MultiQian);
			if (!((Object)(object)multiHouInstance != (Object)null) || !((Object)(object)multiQianInstance != (Object)null))
			{
				ReleaseMultiAnimals();
				SetNodeActive(rect_SingleHou, active: true);
				SetNodeActive(rect_SingleQian, active: true);
				SetNodeActive(rect_MultiHou, active: false);
				SetNodeActive(rect_MultiQian, active: false);
			}
		}
	}

	private GameObject SpawnMultiAnimal(int animalStyle, bool isHou, RectTransform parent)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)parent == (Object)null)
		{
			return null;
		}
		string multiAnimalResourceName = RatingAnimalStyleResolver.GetMultiAnimalResourceName(animalStyle, isHou);
		GameObject val = LoadAsset<GameObject>(multiAnimalResourceName);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		GameObject obj = Object.Instantiate<GameObject>(val, (Transform)(object)parent, false);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		return obj;
	}

	private void ReleaseMultiAnimals()
	{
		if ((Object)(object)multiHouInstance != (Object)null)
		{
			Object.Destroy((Object)(object)multiHouInstance);
			multiHouInstance = null;
		}
		if ((Object)(object)multiQianInstance != (Object)null)
		{
			Object.Destroy((Object)(object)multiQianInstance);
			multiQianInstance = null;
		}
	}

	private static void SetNodeActive(RectTransform node, bool active)
	{
		if ((Object)(object)node != (Object)null)
		{
			((Component)node).gameObject.SetActive(active);
		}
	}

	public void PlayAnimalOpenIdleAnimation()
	{
		if (!((Object)(object)rect_SingleHou == (Object)null) && ((Component)rect_SingleHou).gameObject.activeInHierarchy)
		{
			PlayAnimalOpenIdleAnimation(spineAni_Hou, "open_hou_01", "idle_hou_01", "hou");
			PlayAnimalOpenIdleAnimation(spineAni_Qian, "open_qian_01", "idle_qian_01", "qian");
		}
	}

	private static void PlayAnimalOpenIdleAnimation(SkeletonGraphic skeletonGraphic, string openAnimation, string idleAnimation, string part)
	{
		if ((Object)(object)skeletonGraphic == (Object)null)
		{
			Log.LogError("[Rating][UIRatingView] ", "Animal spine is missing. part={0}", new object[1] { part });
			return;
		}
		AnimationState animationState = skeletonGraphic.AnimationState;
		if (animationState == null)
		{
			Log.LogError("[Rating][UIRatingView] ", "Animal spine animation state is invalid. part={0}", new object[1] { part });
		}
		else
		{
			animationState.SetAnimation(0, openAnimation, false);
			animationState.AddAnimation(0, idleAnimation, true, 0f);
		}
	}

	public void SetStarState(int count)
	{
		GameObject[] array = GetStarLights();
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i] != (Object)null)
			{
				SetStarVisual(array[i], i < count);
			}
		}
	}

	private void SetStarVisual(GameObject light, bool visible)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Image component = light.GetComponent<Image>();
		if ((Object)(object)component != (Object)null)
		{
			Color color = ((Graphic)component).color;
			color.a = (visible ? 1f : 0f);
			((Graphic)component).color = color;
		}
		light.transform.localScale = Vector3.one;
	}

	public void SetSubmitInteractable(bool interactable)
	{
		if ((Object)(object)btn_Submit != (Object)null)
		{
			((Component)btn_Submit).gameObject.SetActive(interactable);
		}
		if ((Object)(object)btn_SubmitGray != (Object)null)
		{
			((Component)btn_SubmitGray).gameObject.SetActive(!interactable);
		}
	}

	public void PlayOpenAnimation()
	{
		if (!((Object)(object)ani_Panel == (Object)null) && !((Object)(object)ani_Panel.GetClip("anim_popup_open") == (Object)null))
		{
			ani_Panel.Rewind("anim_popup_open");
			ani_Panel.Play("anim_popup_open");
		}
	}

	public float GetOpenAnimationDuration()
	{
		if ((Object)(object)ani_Panel == (Object)null || (Object)(object)ani_Panel.GetClip("anim_popup_open") == (Object)null)
		{
			return 0f;
		}
		return ani_Panel.GetClip("anim_popup_open").length;
	}

	public void PlayIdleAnimation()
	{
		if (!((Object)(object)ani_Panel == (Object)null) && !((Object)(object)ani_Panel.GetClip("anim_UIRating_idle") == (Object)null))
		{
			ani_Panel.Rewind("anim_UIRating_idle");
			ani_Panel.Play("anim_UIRating_idle");
		}
	}

	public void StopIdleAnimation()
	{
		if (!((Object)(object)ani_Panel == (Object)null) && !((Object)(object)ani_Panel.GetClip("anim_UIRating_idle") == (Object)null) && ani_Panel.IsPlaying("anim_UIRating_idle"))
		{
			ani_Panel.Stop("anim_UIRating_idle");
			ani_Panel.Rewind("anim_UIRating_idle");
			ani_Panel.Sample();
		}
	}

	public void PlayCloseAnimation(Action onComplete = null)
	{
		if ((Object)(object)ani_Panel == (Object)null || (Object)(object)ani_Panel.GetClip("anim_popup_close") == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		ani_Panel.Rewind("anim_popup_close");
		ani_Panel.Play("anim_popup_close");
		float length = ani_Panel.GetClip("anim_popup_close").length;
		if ((Object)(object)((UIBaseView)this).NodeRect != (Object)null)
		{
			GameApp.Timer.Delay((Object)(object)((UIBaseView)this).NodeRect, length, (Action)delegate
			{
				onComplete?.Invoke();
			});
		}
		else
		{
			GameApp.Timer.Delay(length, (Action)delegate
			{
				onComplete?.Invoke();
			});
		}
	}

	public void OnUiStackActiveChanged(bool isActive)
	{
		if ((Object)(object)maskFader == (Object)null)
		{
			maskFader = ((Component)((UIBaseView)this).Behaviour).GetComponent<UIPopupMaskFader>();
		}
		maskFader?.OnUiStackActiveChanged(isActive);
	}

	public override void Dispose()
	{
		starLights = null;
		ReleaseMultiAnimals();
		((UIBaseView)this).Dispose();
	}

	private GameObject[] GetStarLights()
	{
		if (starLights == null)
		{
			starLights = (GameObject[])(object)new GameObject[5] { img_StarLight1, img_StarLight2, img_StarLight3, img_StarLight4, img_StarLight5 };
		}
		return starLights;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		txtm_Desc = behaviour.Get<TMP_Text>("txtm_Desc");
		btn_Star1 = behaviour.Get<Button>("btn_Star1");
		btn_Star2 = behaviour.Get<Button>("btn_Star2");
		btn_Star3 = behaviour.Get<Button>("btn_Star3");
		btn_Star4 = behaviour.Get<Button>("btn_Star4");
		btn_Star5 = behaviour.Get<Button>("btn_Star5");
		img_StarLight1 = behaviour.Get<GameObject>("img_StarLight1");
		img_StarLight2 = behaviour.Get<GameObject>("img_StarLight2");
		img_StarLight3 = behaviour.Get<GameObject>("img_StarLight3");
		img_StarLight4 = behaviour.Get<GameObject>("img_StarLight4");
		img_StarLight5 = behaviour.Get<GameObject>("img_StarLight5");
		txtm_Title = behaviour.Get<TMP_Text>("txtm_Title");
		btn_Close = behaviour.Get<Button>("btn_Close");
		btn_Submit = behaviour.Get<Button>("btn_Submit");
		btn_SubmitGray = behaviour.Get<Button>("btn_SubmitGray");
		txt_Submit = behaviour.Get<TMP_Text>("txt_Submit");
		ani_Panel = behaviour.Get<Animation>("ani_Panel");
		rect_SpineRoot = behaviour.Get<RectTransform>("rect_SpineRoot");
		spine_RatingAnimal_Play = behaviour.Get<UISpineLoader>("spine_RatingAnimal_Play");
		spineAni_Hou = behaviour.Get<SkeletonGraphic>("spineAni_Hou");
		spineAni_Qian = behaviour.Get<SkeletonGraphic>("spineAni_Qian");
		rect_SingleHou = behaviour.Get<RectTransform>("rect_SingleHou");
		rect_SingleQian = behaviour.Get<RectTransform>("rect_SingleQian");
		rect_MultiHou = behaviour.Get<RectTransform>("rect_MultiHou");
		rect_MultiQian = behaviour.Get<RectTransform>("rect_MultiQian");
	}
}
