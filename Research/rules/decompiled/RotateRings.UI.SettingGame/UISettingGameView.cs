using Game.UI;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.SettingGame;

public class UISettingGameView : GameBaseView
{
	public Button Mask;

	public RectTransform SettingBg;

	public Button RefreshBtn;

	public Button LanguageBtn;

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Mask = behaviour.Get<Button>("Mask");
		SettingBg = behaviour.Get<RectTransform>("SettingBg");
		RefreshBtn = behaviour.Get<Button>("RefreshBtn");
		LanguageBtn = behaviour.Get<Button>("LanguageBtn");
	}
}
