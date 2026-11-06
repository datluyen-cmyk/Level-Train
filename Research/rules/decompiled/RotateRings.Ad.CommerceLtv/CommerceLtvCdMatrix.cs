namespace RotateRings.Ad.CommerceLtv;

public static class CommerceLtvCdMatrix
{
	public static void Resolve(int group, int installDays, out float minCd, out float maxCd)
	{
		int day = ((installDays < 1) ? 1 : installDays);
		switch (group)
		{
		case 1:
			ResolveGroup1(day, out minCd, out maxCd);
			break;
		case 2:
			ResolveGroup2(day, out minCd, out maxCd);
			break;
		case 3:
			ResolveGroup3(day, out minCd, out maxCd);
			break;
		default:
			minCd = 90f;
			maxCd = 300f;
			break;
		}
	}

	private static void ResolveGroup1(int day, out float minCd, out float maxCd)
	{
		if (day > 30)
		{
			minCd = 60f;
			maxCd = 250f;
			return;
		}
		if (day > 14)
		{
			minCd = 75f;
			maxCd = 260f;
			return;
		}
		switch (day)
		{
		case 14:
			minCd = 75f;
			maxCd = 270f;
			break;
		case 13:
			minCd = 75f;
			maxCd = 280f;
			break;
		case 12:
			minCd = 75f;
			maxCd = 290f;
			break;
		case 11:
			minCd = 75f;
			maxCd = 300f;
			break;
		case 10:
			minCd = 90f;
			maxCd = 310f;
			break;
		case 9:
			minCd = 90f;
			maxCd = 320f;
			break;
		case 8:
			minCd = 90f;
			maxCd = 330f;
			break;
		case 7:
			minCd = 90f;
			maxCd = 340f;
			break;
		case 6:
			minCd = 90f;
			maxCd = 350f;
			break;
		case 5:
			minCd = 120f;
			maxCd = 360f;
			break;
		case 4:
			minCd = 150f;
			maxCd = 390f;
			break;
		case 3:
			minCd = 180f;
			maxCd = 420f;
			break;
		case 2:
			minCd = 210f;
			maxCd = 450f;
			break;
		default:
			minCd = 240f;
			maxCd = 480f;
			break;
		}
	}

	private static void ResolveGroup2(int day, out float minCd, out float maxCd)
	{
		if (day > 30)
		{
			minCd = 30f;
			maxCd = 120f;
			return;
		}
		if (day > 14)
		{
			minCd = 60f;
			maxCd = 120f;
			return;
		}
		switch (day)
		{
		case 14:
			minCd = 60f;
			maxCd = 120f;
			break;
		case 13:
			minCd = 60f;
			maxCd = 120f;
			break;
		case 12:
			minCd = 60f;
			maxCd = 150f;
			break;
		case 11:
			minCd = 60f;
			maxCd = 150f;
			break;
		case 10:
			minCd = 75f;
			maxCd = 180f;
			break;
		case 9:
			minCd = 90f;
			maxCd = 180f;
			break;
		case 8:
			minCd = 105f;
			maxCd = 210f;
			break;
		case 7:
			minCd = 120f;
			maxCd = 240f;
			break;
		case 6:
			minCd = 150f;
			maxCd = 300f;
			break;
		case 5:
			minCd = 180f;
			maxCd = 360f;
			break;
		case 4:
			minCd = 210f;
			maxCd = 420f;
			break;
		case 3:
			minCd = 240f;
			maxCd = 480f;
			break;
		case 2:
			minCd = 270f;
			maxCd = 540f;
			break;
		default:
			minCd = 300f;
			maxCd = 600f;
			break;
		}
	}

	private static void ResolveGroup3(int day, out float minCd, out float maxCd)
	{
		if (day > 30)
		{
			minCd = 60f;
			maxCd = 250f;
			return;
		}
		if (day > 14)
		{
			minCd = 75f;
			maxCd = 260f;
			return;
		}
		switch (day)
		{
		case 14:
			minCd = 75f;
			maxCd = 270f;
			break;
		case 13:
			minCd = 75f;
			maxCd = 280f;
			break;
		case 12:
			minCd = 75f;
			maxCd = 290f;
			break;
		case 11:
			minCd = 75f;
			maxCd = 300f;
			break;
		case 10:
			minCd = 90f;
			maxCd = 310f;
			break;
		case 9:
			minCd = 90f;
			maxCd = 320f;
			break;
		case 8:
			minCd = 90f;
			maxCd = 330f;
			break;
		case 7:
			minCd = 90f;
			maxCd = 340f;
			break;
		case 6:
			minCd = 90f;
			maxCd = 350f;
			break;
		case 5:
			minCd = 120f;
			maxCd = 360f;
			break;
		case 4:
			minCd = 150f;
			maxCd = 420f;
			break;
		case 3:
			minCd = 180f;
			maxCd = 480f;
			break;
		case 2:
			minCd = 210f;
			maxCd = 540f;
			break;
		default:
			minCd = 240f;
			maxCd = 600f;
			break;
		}
	}
}
