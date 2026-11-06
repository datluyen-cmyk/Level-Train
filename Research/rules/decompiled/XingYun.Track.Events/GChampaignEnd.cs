using System;

namespace XingYun.Track.Events;

[Serializable]
public class GChampaignEnd : BaseTrackConfig
{
	public const string EventName = "g_champaign_end";

	public int champaign_num;

	public int end_active_days;

	public int end_total_num;

	public int champaign_end_rank;

	public int champaign_end_score;

	public int champaign_end_score_1st;

	public int champaign_end_score_2nd;

	public int champaign_end_score_3rd;

	public int champaign_end_score_10th;

	public int champaign_end_game_cnt;

	public int champaign_end_collect_cards_num;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		champaign_num = 0;
		end_active_days = 0;
		end_total_num = 0;
		champaign_end_rank = 0;
		champaign_end_score = 0;
		champaign_end_score_1st = 0;
		champaign_end_score_2nd = 0;
		champaign_end_score_3rd = 0;
		champaign_end_score_10th = 0;
		champaign_end_game_cnt = 0;
		champaign_end_collect_cards_num = 0;
	}
}
