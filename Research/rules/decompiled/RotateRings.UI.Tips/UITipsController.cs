using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Tips;

[UIPath("UITips")]
public class UITipsController : BaseController<UITipsView>
{
	public UITipsController(UITipsView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		if (!(param is UITipsOpenParam uITipsOpenParam) || (Object)(object)uITipsOpenParam.Target == (Object)null)
		{
			base.CloseView(false);
		}
		else
		{
			ShowInstance(uITipsOpenParam);
		}
	}

	public void ShowInstance(UITipsOpenParam param)
	{
		if (param != null && !((Object)(object)param.Target == (Object)null) && !base.view.IsShowingForTarget(param.Target))
		{
			base.view.CreateInstance(param, CloseViewIfEmpty);
		}
	}

	public void Refresh(UITipsOpenParam param)
	{
		if (param != null && !((Object)(object)param.Target == (Object)null))
		{
			base.view.ForceCleanup();
			base.view.CreateInstance(param, CloseViewIfEmpty);
		}
	}

	public override void OnRemove()
	{
		base.view.ForceCleanup();
	}

	public override void OnActive(bool isActive)
	{
		base.OnActive(isActive);
	}

	public override void OnEscape()
	{
		base.view.CloseLatestInstance(CloseViewIfEmpty);
	}

	private void CloseViewIfEmpty()
	{
		if (!base.view.HasActiveInstances)
		{
			base.CloseView(false);
		}
	}
}
