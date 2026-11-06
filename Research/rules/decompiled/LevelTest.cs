using UnityEngine;
using UnlockRing.Serialization;

public class LevelTest : MonoBehaviour
{
	[Header("Data")]
	[Tooltip("PlayableLevelPack 二进制资源。")]
	public TextAsset ta;

	private void Start()
	{
		UnifiedLevelPackReader.Read(ta.text);
		_ = 0;
	}
}
