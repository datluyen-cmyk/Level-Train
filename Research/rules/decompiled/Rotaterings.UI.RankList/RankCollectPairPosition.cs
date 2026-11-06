using UnityEngine;

namespace Rotaterings.UI.RankList;

public readonly struct RankCollectPairPosition
{
	public Vector2 ImgCollectPosition { get; }

	public Vector2 TxtCollectNumPosition { get; }

	public RankCollectPairPosition(Vector2 imagePosition, Vector2 numberPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		ImgCollectPosition = imagePosition;
		TxtCollectNumPosition = numberPosition;
	}
}
