namespace Game.DataTrack;

public static class UserSettingTrackHelper
{
	public static int ToTrackFlag(bool isOn)
	{
		if (!isOn)
		{
			return 0;
		}
		return 1;
	}

	public static string ToAvatarTrackId(int headId)
	{
		return $"avatar_{headId:D2}";
	}
}
