using System;
using XingYun.Track;

namespace Game.DataTrack;

[Serializable]
public class LevelEntryBannerShowData : BaseTrackConfig
{
	public int level;

	public string difficulty;

	public int style_id;

	public string banner_key;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		level = 0;
		difficulty = string.Empty;
		style_id = 0;
		banner_key = string.Empty;
	}
}
