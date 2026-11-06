using UnityEngine;
using XingYun.Aot.Performance;

namespace RotateRings.Common.Performance;

public static class ProjectFrameRatePolicy
{
	private const string Tag = "[Perf][FrameRate]";

	public static void Apply(string reason = null)
	{
		int recommendTargetFPS = GraphicsManager.RecommendTargetFPS;
		int num = ((recommendTargetFPS <= 30) ? 1 : 0);
		int vSyncCount = QualitySettings.vSyncCount;
		int targetFrameRate = Application.targetFrameRate;
		QualitySettings.vSyncCount = num;
		Application.targetFrameRate = recommendTargetFPS;
		if (!string.IsNullOrEmpty(reason) && vSyncCount != num)
		{
		}
	}
}
