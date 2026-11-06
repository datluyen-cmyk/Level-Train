using Game.UI;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

public class UISettingView : GameWidgetView
{
	public RectTransform Node;

	public Button btnSetting;

	public override void Dispose()
	{
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		Node = behaviour.Get<RectTransform>("Node");
		btnSetting = behaviour.Get<Button>("btnSetting");
	}
}
