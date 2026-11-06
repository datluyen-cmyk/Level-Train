using Game.UI;
using TMPro;
using UnityEngine;
using XingYun.UI.Controller;

namespace RotateRings.UI.ClaimReward;

public class UIPropItemTemplateController : GameBaseController<UIPropItemTemplateView>
{
	private WidgetHandle _handle;

	public RectTransform RootRect
	{
		get
		{
			WidgetHandle handle = _handle;
			object obj;
			if (handle == null)
			{
				obj = null;
			}
			else
			{
				GameObject gameObject = handle.GameObject;
				obj = ((gameObject != null) ? gameObject.transform : null);
			}
			return (RectTransform)((obj is RectTransform) ? obj : null);
		}
	}

	public RectTransform PropCountRect
	{
		get
		{
			UIPropItemTemplateView view = ((BaseController<UIPropItemTemplateView>)this).view;
			if (view == null)
			{
				return null;
			}
			TMP_Text txtPropCount = view.TxtPropCount;
			if (txtPropCount == null)
			{
				return null;
			}
			return txtPropCount.rectTransform;
		}
	}

	public Animation ItemAnim => ((BaseController<UIPropItemTemplateView>)this).view?.Anim;

	public UIPropItemTemplateController(UIPropItemTemplateView view)
		: base(view)
	{
	}

	public void BindHandle(WidgetHandle handle)
	{
		_handle = handle;
	}

	public override void OnRegister(object param = null)
	{
		if ((Object)(object)((BaseController<UIPropItemTemplateView>)this).view.bg != (Object)null)
		{
			((BaseController<UIPropItemTemplateView>)this).view.bg.SetActive(false);
		}
	}

	public void Render(int propId, int count)
	{
		((BaseController<UIPropItemTemplateView>)this).view.OnRender(propId, count);
	}

	public void SetPropCount(int count)
	{
		((BaseController<UIPropItemTemplateView>)this).view.SetPropCount(count);
	}

	public void SetActive(bool active)
	{
		((BaseController<UIPropItemTemplateView>)this).view.SetNodeActive(active);
	}

	public void Dispose()
	{
		_handle?.Dispose();
		_handle = null;
	}

	public override void OnRemove()
	{
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIPropItemTemplateView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
	}
}
