namespace RotateRings.Ad;

public static class EscapeRateCdMatrix
{
	private sealed class GroupData
	{
		public int[] CdDays;

		public float[][] CdSeconds;

		public int[] BonusDays;

		public int[] BonusSeconds;
	}

	public const int MaxGroup = 5;

	public const int BucketCount = 5;

	private static readonly GroupData[] Groups = new GroupData[5]
	{
		new GroupData
		{
			CdDays = new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
			CdSeconds = new float[9][]
			{
				new float[5] { 240f, 270f, 300f, 330f, 360f },
				new float[5] { 155f, 195f, 230f, 275f, 310f },
				new float[5] { 130f, 165f, 200f, 235f, 270f },
				new float[5] { 110f, 140f, 175f, 205f, 235f },
				new float[5] { 95f, 125f, 150f, 180f, 210f },
				new float[5] { 80f, 110f, 135f, 160f, 185f },
				new float[5] { 70f, 95f, 120f, 145f, 170f },
				new float[5] { 65f, 90f, 110f, 135f, 160f },
				new float[5] { 60f, 85f, 105f, 130f, 150f }
			},
			BonusDays = new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
			BonusSeconds = new int[9] { 50, 35, 30, 30, 25, 20, 20, 20, 15 }
		},
		new GroupData
		{
			CdDays = new int[13]
			{
				1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
				11, 12, 13
			},
			CdSeconds = new float[13][]
			{
				new float[5] { 240f, 270f, 300f, 330f, 360f },
				new float[5] { 215f, 245f, 275f, 305f, 335f },
				new float[5] { 195f, 225f, 250f, 280f, 305f },
				new float[5] { 175f, 200f, 230f, 255f, 285f },
				new float[5] { 155f, 180f, 205f, 235f, 260f },
				new float[5] { 135f, 165f, 185f, 215f, 240f },
				new float[5] { 120f, 145f, 170f, 195f, 220f },
				new float[5] { 105f, 130f, 155f, 180f, 200f },
				new float[5] { 90f, 115f, 140f, 165f, 185f },
				new float[5] { 80f, 105f, 125f, 150f, 175f },
				new float[5] { 70f, 95f, 115f, 140f, 160f },
				new float[5] { 65f, 90f, 110f, 135f, 155f },
				new float[5] { 60f, 85f, 105f, 130f, 150f }
			},
			BonusDays = new int[13]
			{
				1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
				11, 12, 13
			},
			BonusSeconds = new int[13]
			{
				50, 45, 40, 35, 35, 30, 25, 25, 20, 20,
				20, 20, 15
			}
		},
		new GroupData
		{
			CdDays = new int[6] { 1, 2, 3, 4, 5, 6 },
			CdSeconds = new float[6][]
			{
				new float[5] { 240f, 270f, 300f, 330f, 360f },
				new float[5] { 170f, 200f, 225f, 250f, 280f },
				new float[5] { 120f, 145f, 170f, 195f, 220f },
				new float[5] { 85f, 110f, 130f, 155f, 180f },
				new float[5] { 65f, 90f, 110f, 135f, 155f },
				new float[5] { 60f, 85f, 105f, 130f, 150f }
			},
			BonusDays = new int[6] { 1, 2, 3, 4, 5, 6 },
			BonusSeconds = new int[6] { 50, 35, 25, 20, 20, 15 }
		},
		new GroupData
		{
			CdDays = new int[7] { 1, 2, 3, 4, 5, 6, 7 },
			CdSeconds = new float[7][]
			{
				new float[5] { 170f, 200f, 230f, 260f, 290f },
				new float[5] { 140f, 170f, 200f, 225f, 250f },
				new float[5] { 120f, 145f, 175f, 200f, 225f },
				new float[5] { 105f, 130f, 155f, 180f, 210f },
				new float[5] { 95f, 120f, 145f, 170f, 195f },
				new float[5] { 90f, 115f, 140f, 165f, 190f },
				new float[5] { 85f, 110f, 135f, 160f, 185f }
			},
			BonusDays = new int[7] { 1, 2, 3, 4, 5, 6, 7 },
			BonusSeconds = new int[7] { 50, 40, 35, 30, 25, 20, 15 }
		},
		new GroupData
		{
			CdDays = new int[7] { 1, 2, 3, 4, 5, 6, 7 },
			CdSeconds = new float[7][]
			{
				new float[5] { 160f, 185f, 215f, 240f, 270f },
				new float[5] { 130f, 155f, 175f, 205f, 230f },
				new float[5] { 100f, 125f, 145f, 175f, 200f },
				new float[5] { 90f, 100f, 120f, 150f, 175f },
				new float[5] { 75f, 80f, 105f, 130f, 155f },
				new float[5] { 65f, 70f, 90f, 115f, 140f },
				new float[5] { 60f, 65f, 85f, 110f, 135f }
			},
			BonusDays = new int[7] { 1, 2, 3, 4, 5, 6, 7 },
			BonusSeconds = new int[7] { 50, 40, 35, 30, 25, 20, 15 }
		}
	};

	public static bool IsValidGroup(int group)
	{
		if (group >= 1)
		{
			return group <= 5;
		}
		return false;
	}

	public static float GetCdSeconds(int group, int installDay, int bucket)
	{
		if (!IsValidGroup(group) || bucket < 0 || bucket >= 5)
		{
			return -1f;
		}
		GroupData obj = Groups[group - 1];
		int num = FindRowIndex(obj.CdDays, installDay);
		return obj.CdSeconds[num][bucket];
	}

	public static int GetRewardBonusSeconds(int group, int installDay)
	{
		if (!IsValidGroup(group))
		{
			return 0;
		}
		GroupData obj = Groups[group - 1];
		int num = FindRowIndex(obj.BonusDays, installDay);
		return obj.BonusSeconds[num];
	}

	private static int FindRowIndex(int[] thresholds, int day)
	{
		int result = 0;
		for (int i = 0; i < thresholds.Length && day >= thresholds[i]; i++)
		{
			result = i;
		}
		return result;
	}
}
