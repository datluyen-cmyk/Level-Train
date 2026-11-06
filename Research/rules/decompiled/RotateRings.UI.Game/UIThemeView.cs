using Game.UI;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public class UIThemeView : GameWidgetView
{
	public RectTransform Node;

	public Button BtnTheme;

	public override void Dispose()
	{
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		Node = behaviour.Get<RectTransform>("Node");
		BtnTheme = behaviour.Get<Button>("BtnTheme");
	}
}
