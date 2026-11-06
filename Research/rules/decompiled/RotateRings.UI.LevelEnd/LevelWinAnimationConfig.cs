using UnityEngine;

namespace RotateRings.UI.LevelEnd;

[CreateAssetMenu(fileName = "LevelWinAnimationConfig", menuName = "Config/LevelWinAnimation")]
public sealed class LevelWinAnimationConfig : ScriptableObject
{
	public const string ResourceName = "LevelWinAnimationConfig";

	[SerializeField]
	[InspectorName("底板流程（LevelWinAnimation=0）")]
	private LevelWinAnimationSettings originalSettings = LevelWinAnimationSettings.OriginalDefault;

	[SerializeField]
	[InspectorName("实验流程（LevelWinAnimation=1/2共用）")]
	private LevelWinAnimationSettings settings = LevelWinAnimationSettings.Default;

	public LevelWinAnimationSettings Settings => GetSettings(useExperiment: true);

	public LevelWinAnimationSettings GetSettings(bool useExperiment)
	{
		LevelWinAnimationSettings result = (useExperiment ? settings : originalSettings);
		result.Validate();
		return result;
	}
}
