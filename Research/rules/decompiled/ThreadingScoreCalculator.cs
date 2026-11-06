public static class ThreadingScoreCalculator
{
	public static int GetReleaseScore(int baseScore, GameplayPartData partData)
	{
		int num = 0;
		if (partData.LockedBy != null)
		{
			num += partData.LockedBy.Count;
		}
		if (partData.Locks != null)
		{
			num += partData.Locks.Count;
		}
		return baseScore * (1 + num) + ((partData.LockCount > 0) ? baseScore : 0);
	}
}
