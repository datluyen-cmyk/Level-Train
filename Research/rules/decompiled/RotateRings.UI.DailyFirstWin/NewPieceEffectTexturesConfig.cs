using UnityEngine;

namespace RotateRings.UI.DailyFirstWin;

public class NewPieceEffectTexturesConfig : MonoBehaviour
{
	public const string PrefabLocationPrefix = "NewPieceEffectTextures_Day";

	public const int PieceDayMin = 1;

	public const int PieceDayMax = 7;

	[Header("frame 粒子 + 光晕（原 tex_kuang_plz_XX_gp3）")]
	public Sprite frameSprite;

	[Header("sweep 粒子 Mask（原 tex_mask_plz_XX_gp3）")]
	public Sprite maskSprite;

	public Texture2D FrameTexture
	{
		get
		{
			if (!((Object)(object)frameSprite != (Object)null))
			{
				return null;
			}
			return frameSprite.texture;
		}
	}

	public Texture2D MaskTexture
	{
		get
		{
			if (!((Object)(object)maskSprite != (Object)null))
			{
				return null;
			}
			return maskSprite.texture;
		}
	}
}
