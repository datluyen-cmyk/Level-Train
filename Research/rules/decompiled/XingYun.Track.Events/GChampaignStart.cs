using System;

namespace XingYun.Track.Events;

[Serializable]
public class GChampaignStart : BaseTrackConfig
{
	public const string EventName = "g_champaign_start";

	public int champaign_num;

	public int init_active_days;

	public int init_total_num;

	public int champaign_init_rank;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		champaign_num = 0;
		init_active_days = 0;
		init_total_num = 0;
		champaign_init_rank = 0;
	}
}
