namespace RotateRings.RestartRetention;

public struct RestartRetentionConfig
{
	public int GroupId;

	public int StreakMin;

	public int RestartMax;

	public RestartRetentionContent LowStreakContent;

	public RestartRetentionContent HighStreakContent;

	public bool LowUseBaseArt;

	public bool HighUseBaseArt;

	public RestartRetentionContent RankInactiveContent;

	public bool RankInactiveUseBaseArt;

	public RestartRetentionCancelTarget CancelTarget;

	public RestartRetentionConfig WithThresholds(int streakMin, int restartMax)
	{
		RestartRetentionConfig result = this;
		result.StreakMin = streakMin;
		result.RestartMax = restartMax;
		return result;
	}
}
