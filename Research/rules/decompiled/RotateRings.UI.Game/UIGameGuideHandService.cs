using System;
using System.Collections.Generic;
using DG.Tweening;
using RotateRings.Common;
using Spine;
using Spine.Unity;
using UniLog;
using UnityEngine;

namespace RotateRings.UI.Game;

public class UIGameGuideHandService : UIGameBaseService
{
	private const string TAG = "[UI][GuideHand] ";

	private const string PrefabLocation = "eff_guide_hand";

	private const string AnimOpen = "guide_drag_open";

	private const string AnimClose = "guide_drag_close";

	private GameObject guideHandPrefab;

	private GameObject activeInstance;

	private Tween gestureTween;

	private bool isHiding;

	private readonly Dictionary<Type, IGuideHandTarget> targetCache = new Dictionary<Type, IGuideHandTarget>();

	private readonly Dictionary<Type, Func<object, IGuideHandTarget>> targetResolvers = new Dictionary<Type, Func<object, IGuideHandTarget>>();

	internal RectTransform CurrentParent { get; private set; }

	internal Transform ActiveInstanceTransform => activeInstance.transform;

	internal RectTransform ActiveInstanceRect
	{
		get
		{
			GameObject obj = activeInstance;
			Transform obj2 = ((obj != null) ? obj.transform : null);
			return (RectTransform)(object)((obj2 is RectTransform) ? obj2 : null);
		}
	}

	internal Transform ActiveSpineHandTransform
	{
		get
		{
			if (!((Object)(object)activeInstance != (Object)null))
			{
				return null;
			}
			return activeInstance.transform.GetChild(0);
		}
	}

	public UIGameGuideHandService(UIGameController controller)
		: base(controller)
	{
		RegisterTarget((RingObject r) => new RingGuideHandTarget(this, r), delegate(RingGuideHandTarget t, RingObject r)
		{
			t.Reset(r);
		});
		RegisterTarget((PullObject p) => new PullGuideHandTarget(this, p), delegate(PullGuideHandTarget t, PullObject p)
		{
			t.Reset(p);
		});
	}

	private void RegisterTarget<TTarget, TSource>(Func<TSource, TTarget> factory, Action<TTarget, TSource> reset) where TTarget : class, IGuideHandTarget where TSource : class, IGameplayObject
	{
		targetResolvers[typeof(TSource)] = (object source) => GetOrResetTarget((TSource)source, factory, reset);
	}

	private TTarget GetOrResetTarget<TTarget, TSource>(TSource source, Func<TSource, TTarget> factory, Action<TTarget, TSource> reset) where TTarget : class, IGuideHandTarget where TSource : class, IGameplayObject
	{
		Type typeFromHandle = typeof(TTarget);
		if (!targetCache.TryGetValue(typeFromHandle, out var value))
		{
			value = factory(source);
			targetCache[typeFromHandle] = value;
		}
		else
		{
			reset((TTarget)value, source);
		}
		return (TTarget)value;
	}

	public void ShowGuideHand<TSource>(TSource source, RectTransform parent) where TSource : class, IGameplayObject
	{
		if (source != null && targetResolvers.TryGetValue(typeof(TSource), out var value))
		{
			ShowGuideHandInternal(value(source), parent);
		}
	}

	private void ShowGuideHandInternal(IGuideHandTarget target, RectTransform parent)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (target == null || (Object)(object)parent == (Object)null)
		{
			return;
		}
		EnsurePrefabLoaded();
		if (!((Object)(object)guideHandPrefab == (Object)null))
		{
			RecycleExistingImmediate();
			activeInstance = GameApp.Pool.Spawn(guideHandPrefab, (Transform)(object)parent);
			if ((Object)(object)activeInstance == (Object)null)
			{
				Log.LogError("[UI][GuideHand] ", "Spawn 失败 prefab={0}", new object[1] { "eff_guide_hand" });
				return;
			}
			CurrentParent = parent;
			activeInstance.transform.localScale = guideHandPrefab.transform.localScale * GameUIScaleByCameraOrtho.GetScaleFactor();
			target.SetupRotation();
			target.SetupInstance();
			PlaySpineAnimation("guide_drag_open", loop: false);
			gestureTween = target.CreateGestureTween();
			isHiding = false;
		}
	}

	public void HideGuideHand()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		if ((Object)(object)activeInstance == (Object)null || isHiding)
		{
			return;
		}
		isHiding = true;
		KillGestureTween();
		if (PlaySpineAnimation("guide_drag_close", loop: false) == null)
		{
			RecycleActiveInstance();
			return;
		}
		SkeletonGraphic spineGraphic = GetSpineGraphic();
		TrackEntryDelegate handler = null;
		GameObject instanceToRecycle = activeInstance;
		handler = (TrackEntryDelegate)delegate
		{
			if ((Object)(object)spineGraphic != (Object)null)
			{
				spineGraphic.AnimationState.Complete -= handler;
			}
			if ((Object)(object)activeInstance == (Object)(object)instanceToRecycle)
			{
				RecycleActiveInstance();
			}
			else if ((Object)(object)instanceToRecycle != (Object)null)
			{
				GameApp.Pool.Recycle(instanceToRecycle);
			}
		};
		spineGraphic.AnimationState.Complete += handler;
	}

	public override void Dispose()
	{
		KillGestureTween();
		if ((Object)(object)activeInstance != (Object)null)
		{
			GameApp.Pool.Recycle(activeInstance);
			activeInstance = null;
		}
		isHiding = false;
		base.Dispose();
	}

	internal Vector2 ResolveLocalPosition(Vector3 worldPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!WorldToUIPostionUtil.WorldToLocalPoint(worldPosition, CurrentParent, out var localPoint))
		{
			return Vector2.zero;
		}
		return localPoint;
	}

	private SkeletonGraphic GetSpineGraphic()
	{
		if (!((Object)(object)activeInstance != (Object)null))
		{
			return null;
		}
		return activeInstance.GetComponentInChildren<SkeletonGraphic>(true);
	}

	private void EnsurePrefabLoaded()
	{
		if ((Object)(object)guideHandPrefab == (Object)null)
		{
			guideHandPrefab = GameApp.Res.LoadAsset<GameObject>("eff_guide_hand", base.View.LoadAssetGroup);
			if ((Object)(object)guideHandPrefab == (Object)null)
			{
				Log.LogError("[UI][GuideHand] ", "加载预制体失败 location={0}", new object[1] { "eff_guide_hand" });
			}
		}
	}

	private TrackEntry PlaySpineAnimation(string animName, bool loop)
	{
		SkeletonGraphic spineGraphic = GetSpineGraphic();
		if ((Object)(object)spineGraphic == (Object)null)
		{
			return null;
		}
		return spineGraphic.AnimationState.SetAnimation(0, animName, loop);
	}

	private void KillGestureTween()
	{
		if (gestureTween != null)
		{
			TweenExtensions.Kill(gestureTween, false);
			gestureTween = null;
		}
	}

	private void RecycleExistingImmediate()
	{
		KillGestureTween();
		if ((Object)(object)activeInstance != (Object)null)
		{
			GameApp.Pool.Recycle(activeInstance);
			activeInstance = null;
		}
		isHiding = false;
	}

	private void RecycleActiveInstance()
	{
		if ((Object)(object)activeInstance != (Object)null)
		{
			GameApp.Pool.Recycle(activeInstance);
			activeInstance = null;
		}
		isHiding = false;
	}
}
