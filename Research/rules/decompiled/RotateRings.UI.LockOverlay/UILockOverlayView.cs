using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.LockOverlay;

public class UILockOverlayView : GameBaseView
{
	private const float PurchaseLoadingRotateDuration = 2f;

	private RectTransform loadingRect;

	private Tween loadingRotateTween;

	public Image imgLockPanel;

	public GameObject PurchaseLoading;

	public void Init()
	{
		ShowLock(isShow: false);
		ShowPurchaseLoading(isShow: false);
	}

	public void ShowLock(bool isShow)
	{
		if (!((Object)(object)imgLockPanel == (Object)null))
		{
			((Component)imgLockPanel).gameObject.SetActive(isShow);
		}
	}

	public void ShowPurchaseLoading(bool isShow)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PurchaseLoading == (Object)null)
		{
			return;
		}
		if (!isShow)
		{
			StopPurchaseLoadingRotation();
			PurchaseLoading.SetActive(false);
			return;
		}
		PurchaseLoading.SetActive(true);
		if ((Object)(object)loadingRect == (Object)null)
		{
			ref RectTransform reference = ref loadingRect;
			Transform obj = PurchaseLoading.transform.Find("ImgLoading");
			reference = (RectTransform)(object)((obj is RectTransform) ? obj : null);
		}
		if (!((Object)(object)loadingRect == (Object)null))
		{
			StopPurchaseLoadingRotation();
			loadingRotateTween = (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate((Transform)(object)loadingRect, new Vector3(0f, 0f, -360f), 2f, (RotateMode)3), (Ease)1), -1, (LoopType)2);
		}
	}

	private void StopPurchaseLoadingRotation()
	{
		Tween obj = loadingRotateTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		loadingRotateTween = null;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		imgLockPanel = behaviour.Get<Image>("imgLockPanel");
		PurchaseLoading = behaviour.Get<GameObject>("PurchaseLoading");
	}
}
