using UnityEngine;

namespace Game.UI.ClaimReward;

public class UIClaimRewardViewRefs : MonoBehaviour
{
	[Header("道具飞行轨迹")]
	[SerializeField]
	private PropFlyPathAsset propFlyPath;

	[SerializeField]
	private PropFlyPathAsset propFlyBackPath;

	public PropFlyPathAsset PropFlyPath => propFlyPath;

	public PropFlyPathAsset PropFlyBackPath => propFlyBackPath;
}
