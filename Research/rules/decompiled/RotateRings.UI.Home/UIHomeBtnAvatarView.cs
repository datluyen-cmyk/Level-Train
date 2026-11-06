using Game.UI;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Home;

public class UIHomeBtnAvatarView : GameWidgetView
{
	public Button btnAvatar;

	public Image imageAvatar;

	public Image imageFrame;

	public void SetVisible(bool visible)
	{
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			((Component)((UIBaseView)this).Behaviour).gameObject.SetActive(visible);
		}
	}

	public void SetInteractable(bool interactable)
	{
		if ((Object)(object)btnAvatar != (Object)null)
		{
			((Selectable)btnAvatar).interactable = interactable;
		}
	}

	public void RefreshHead(Sprite headSprite, Sprite frameSprite)
	{
		if ((Object)(object)imageAvatar != (Object)null && (Object)(object)headSprite != (Object)null)
		{
			imageAvatar.sprite = headSprite;
		}
		if ((Object)(object)imageFrame != (Object)null && (Object)(object)frameSprite != (Object)null)
		{
			imageFrame.sprite = frameSprite;
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		btnAvatar = behaviour.Get<Button>("btnAvatar");
		imageAvatar = behaviour.Get<Image>("imageAvatar");
		imageFrame = behaviour.Get<Image>("imageFrame");
	}
}
