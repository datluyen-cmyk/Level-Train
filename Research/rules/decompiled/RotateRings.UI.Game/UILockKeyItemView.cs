using Game.UI;
using RotateRings.Common;
using RotateRings.GamePlay.Services;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public class UILockKeyItemView : GameWidgetView
{
	private const string TAG = "[UILockKeyItemView] ";

	public const string DisappearAnimName = "anim_LockKeyItem_close";

	private float baseScaleRatio = 1f;

	private RingObject boundRing;

	public Image keyImg;

	public Animation keyAni;

	public void Bind(RingObject ring, RectTransform parentRect, PaletteLockKeyColor keyColor)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ring == (Object)null)
		{
			return;
		}
		boundRing = ring;
		ApplyKeyColor(keyColor);
		if (!((Object)(object)parentRect == (Object)null))
		{
			float innerDiameterWorld;
			float num = RingPaletteLockKeyUi.ComputeInnerDiameterPixels(ring, parentRect, out innerDiameterWorld);
			baseScaleRatio = 1f;
			if (num > 0f && num < 150f)
			{
				baseScaleRatio = num / 150f;
			}
			if ((Object)(object)((UIBaseView)this).NodeRect != (Object)null)
			{
				((Transform)((UIBaseView)this).NodeRect).localRotation = Quaternion.identity;
			}
			RefreshPosition(ring, parentRect, 1f);
			PlayIdleAnimation();
		}
	}

	public void SetEnterScaleFactor(float factor)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)((UIBaseView)this).NodeRect == (Object)null))
		{
			float num = baseScaleRatio * factor;
			((Transform)((UIBaseView)this).NodeRect).localScale = new Vector3(num, num, 1f);
		}
	}

	public void RefreshPosition(RingObject ring, RectTransform parentRect, float cameraScaleFactor)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ring == (Object)null) && !((Object)(object)parentRect == (Object)null) && !((Object)(object)((UIBaseView)this).NodeRect == (Object)null) && WorldToUIPostionUtil.WorldToLocalPoint(ring.GetPosition(), parentRect, out var localPoint))
		{
			((Transform)((UIBaseView)this).NodeRect).localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
			float num = baseScaleRatio * cameraScaleFactor;
			((Transform)((UIBaseView)this).NodeRect).localScale = new Vector3(num, num, 1f);
		}
	}

	public void ApplyKeyColor(PaletteLockKeyColor keyColor)
	{
		if (!((Object)(object)keyImg == (Object)null))
		{
			SetSprite(keyImg, GetKeySpriteName(keyColor));
		}
	}

	private static string GetKeySpriteName(PaletteLockKeyColor color)
	{
		return color switch
		{
			PaletteLockKeyColor.Green => "game_img_key_green", 
			PaletteLockKeyColor.Red => "game_img_key_red", 
			PaletteLockKeyColor.Yellow => "game_img_key_yellow", 
			_ => "game_img_key_blue", 
		};
	}

	public bool TryPlayDisappear(out float durationSeconds)
	{
		durationSeconds = 0f;
		if ((Object)(object)keyAni == (Object)null)
		{
			return false;
		}
		AnimationClip clip = keyAni.GetClip("anim_LockKeyItem_close");
		if ((Object)(object)clip == (Object)null)
		{
			return false;
		}
		keyAni.Play("anim_LockKeyItem_close");
		durationSeconds = clip.length;
		return true;
	}

	private void PlayIdleAnimation()
	{
		if (!((Object)(object)keyAni == (Object)null))
		{
			AnimationClip clip = keyAni.clip;
			if ((Object)(object)clip != (Object)null && ((Object)clip).name != "anim_LockKeyItem_close")
			{
				keyAni.Play(((Object)clip).name);
			}
			else
			{
				keyAni.GetClipCount();
			}
		}
	}

	public override void Dispose()
	{
		boundRing = null;
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		keyImg = behaviour.Get<Image>("keyImg");
		keyAni = behaviour.Get<Animation>("keyAni");
	}
}
