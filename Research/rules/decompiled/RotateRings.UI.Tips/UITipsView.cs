using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.Tips;

public class UITipsView : GameBaseView
{
	public sealed class UITipsRuntimeInstance
	{
		public GameObject aniRoot;

		public Animation animation;

		public RectTransform aniRect;

		public WidgetHandle tipsHandle;

		public RectTransform target;

		public Action onClosed;

		public int closeTimerId = -1;

		public bool isClosing;

		public UITips_CommonTips Tips => tipsHandle?.GetView<UITips_CommonTips>();
	}

	private const string OpenAnim = "Ani_tips_open";

	private const string CloseAnim = "Ani_tips_close";

	private readonly List<UITipsRuntimeInstance> activeInstances = new List<UITipsRuntimeInstance>(2);

	public UIBehaviour CommonTips;

	public UIBehaviour CommonTipsTitle;

	public Animation ani;

	public bool HasActiveInstances => activeInstances.Count > 0;

	public bool IsShowingForTarget(RectTransform target)
	{
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		for (int i = 0; i < activeInstances.Count; i++)
		{
			if ((Object)(object)activeInstances[i].target == (Object)(object)target)
			{
				return true;
			}
		}
		return false;
	}

	public void CreateInstance(UITipsOpenParam param, Action onAllClosed)
	{
		if (param == null || (Object)(object)param.Target == (Object)null)
		{
			return;
		}
		RectTransform rootRect = GetRootRect();
		if ((Object)(object)rootRect == (Object)null || (Object)(object)ani == (Object)null)
		{
			return;
		}
		EnsureTemplatesInactive();
		GameObject val = ((!string.IsNullOrEmpty(param.Title)) ? ((Component)CommonTipsTitle).gameObject : ((Component)CommonTips).gameObject);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		GameObject val2 = Object.Instantiate<GameObject>(((Component)ani).gameObject, (Transform)(object)rootRect, false);
		val2.SetActive(true);
		val2.transform.SetAsLastSibling();
		WidgetHandle widgetHandle = AttachWidget<UITips_CommonTips>(val, (Transform)(object)rootRect);
		if (widgetHandle == null)
		{
			Object.Destroy((Object)(object)val2);
			return;
		}
		UITips_CommonTips view = widgetHandle.GetView<UITips_CommonTips>();
		UITipsRuntimeInstance inst = new UITipsRuntimeInstance
		{
			aniRoot = val2,
			animation = val2.GetComponent<Animation>(),
			aniRect = (RectTransform)/*isinst with value type is only supported in some contexts*/,
			tipsHandle = widgetHandle,
			target = param.Target,
			onClosed = param.OnClosed
		};
		param.OnClosed = null;
		if (view == null || (Object)(object)inst.aniRect == (Object)null)
		{
			DetachWidget(widgetHandle);
			Object.Destroy((Object)(object)val2);
			return;
		}
		activeInstances.Add(inst);
		view.Show(param, delegate
		{
			CloseInstance(inst, onAllClosed);
		}, delegate
		{
			OnTipsPositionReady(inst);
		});
	}

	public void CloseInstance(UITipsRuntimeInstance inst, Action onAllClosed)
	{
		if (inst == null || inst.isClosing || !activeInstances.Contains(inst))
		{
			return;
		}
		inst.isClosing = true;
		StopInstanceCloseTimer(inst);
		AnimationClip val = (((Object)(object)inst.animation != (Object)null) ? inst.animation.GetClip("Ani_tips_close") : null);
		if ((Object)(object)inst.animation == (Object)null || (Object)(object)val == (Object)null)
		{
			FinalizeCloseInstance(inst, onAllClosed);
			return;
		}
		inst.animation.Stop();
		inst.animation.Rewind("Ani_tips_close");
		inst.animation.Play("Ani_tips_close", (PlayMode)4);
		inst.closeTimerId = GameApp.Timer.Delay((Object)(object)inst.aniRoot, val.length, (Action)delegate
		{
			inst.closeTimerId = -1;
			FinalizeCloseInstance(inst, onAllClosed);
		});
	}

	public void CloseLatestInstance(Action onAllClosed)
	{
		if (activeInstances.Count == 0)
		{
			onAllClosed?.Invoke();
		}
		else
		{
			CloseInstance(activeInstances[activeInstances.Count - 1], onAllClosed);
		}
	}

	public void ForceCleanup()
	{
		for (int num = activeInstances.Count - 1; num >= 0; num--)
		{
			DestroyInstance(activeInstances[num]);
		}
		activeInstances.Clear();
		EnsureTemplatesInactive();
	}

	public override void Dispose()
	{
		ForceCleanup();
		((UIBaseView)this).Dispose();
	}

	private void OnTipsPositionReady(UITipsRuntimeInstance inst)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (inst != null && inst.Tips != null && !((Object)(object)inst.aniRect == (Object)null) && activeInstances.Contains(inst))
		{
			if (!inst.Tips.TryGetActiveArrowRect(out var arrowRect))
			{
				((Transform)((UIBaseView)inst.Tips).NodeRect).SetParent(inst.aniRoot.transform, true);
				inst.Tips.SetTipsVisible(visible: true);
				PlayOpenAnimation(inst);
				return;
			}
			RectTransform obj = arrowRect;
			Rect rect = arrowRect.rect;
			Vector3 worldPoint = ((Transform)obj).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
			ApplyAniAnchorAndPivot(inst.aniRect, inst.Tips.GetAniPivot());
			AlignAniToWorldPoint(inst.aniRect, worldPoint);
			((Transform)((UIBaseView)inst.Tips).NodeRect).SetParent(inst.aniRoot.transform, true);
			inst.Tips.SetTipsVisible(visible: true);
			PlayOpenAnimation(inst);
		}
	}

	private static void PlayOpenAnimation(UITipsRuntimeInstance inst)
	{
		if (!((Object)(object)inst.animation == (Object)null) && !((Object)(object)inst.animation.GetClip("Ani_tips_open") == (Object)null))
		{
			inst.animation.Stop();
			inst.animation.Rewind("Ani_tips_open");
			inst.animation.Play("Ani_tips_open", (PlayMode)4);
		}
	}

	private static void ApplyAniAnchorAndPivot(RectTransform aniRect, Vector2 pivot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Transform)aniRect).position;
		aniRect.anchorMin = pivot;
		aniRect.anchorMax = pivot;
		aniRect.pivot = pivot;
		((Transform)aniRect).position = position;
	}

	private static void AlignAniToWorldPoint(RectTransform aniRect, Vector3 worldPoint)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = ((Transform)aniRect).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if (!((Object)(object)val == (Object)null))
		{
			Vector2 pointInParentLocal = Vector2.op_Implicit(((Transform)val).InverseTransformPoint(worldPoint));
			UITips_CommonTips.SetAnchoredPositionLocal(val, aniRect, pointInParentLocal);
		}
	}

	private void FinalizeCloseInstance(UITipsRuntimeInstance inst, Action onAllClosed)
	{
		DestroyInstance(inst);
		if (activeInstances.Count == 0)
		{
			onAllClosed?.Invoke();
		}
	}

	private void DestroyInstance(UITipsRuntimeInstance inst)
	{
		if (inst != null)
		{
			StopInstanceCloseTimer(inst);
			inst.Tips?.Hide();
			DetachWidget(inst.tipsHandle);
			inst.tipsHandle = null;
			Action onClosed = inst.onClosed;
			inst.onClosed = null;
			onClosed?.Invoke();
			if ((Object)(object)inst.aniRoot != (Object)null)
			{
				Object.Destroy((Object)(object)inst.aniRoot);
			}
			activeInstances.Remove(inst);
		}
	}

	private static void StopInstanceCloseTimer(UITipsRuntimeInstance inst)
	{
		if (inst.closeTimerId >= 0)
		{
			GameApp.Timer.Kill(inst.closeTimerId);
			inst.closeTimerId = -1;
		}
	}

	private void EnsureTemplatesInactive()
	{
		if ((Object)(object)ani != (Object)null)
		{
			((Component)ani).gameObject.SetActive(false);
		}
		if ((Object)(object)CommonTips != (Object)null)
		{
			((Component)CommonTips).gameObject.SetActive(false);
		}
		if ((Object)(object)CommonTipsTitle != (Object)null)
		{
			((Component)CommonTipsTitle).gameObject.SetActive(false);
		}
	}

	private RectTransform GetRootRect()
	{
		if (!((Object)(object)((UIBaseView)this).Behaviour != (Object)null))
		{
			return null;
		}
		Transform transform = ((Component)((UIBaseView)this).Behaviour).transform;
		return (RectTransform)(object)((transform is RectTransform) ? transform : null);
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		CommonTips = behaviour.Get<UIBehaviour>("CommonTips");
		CommonTipsTitle = behaviour.Get<UIBehaviour>("CommonTipsTitle");
		ani = behaviour.Get<Animation>("ani");
	}
}
