using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Shop;

public class UIGetItemView : GameBaseView
{
	public Animation UIAnimation;

	public Button btnClose;

	public Button btnClaim;

	public Button btnShop;

	public Image imgPropIcon;

	public TMP_Text txtAddCount;

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		btnClose = behaviour.Get<Button>("btnClose");
		btnClaim = behaviour.Get<Button>("btnClaim");
		btnShop = behaviour.Get<Button>("btnShop");
		imgPropIcon = behaviour.Get<Image>("imgPropIcon");
		txtAddCount = behaviour.Get<TMP_Text>("txtAddCount");
	}
}
