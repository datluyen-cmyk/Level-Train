using System;

namespace XingYun.Track.Events;

[Serializable]
public class GGameStart : BaseTrackConfig
{
	public const string EventName = "g_game_start";

	public string status;

	public int level;

	public int level_id;

	public int game_mode;

	public string game_mode_name;

	public int game_play_method;

	public int mode_num;

	public int mode_num_all;

	public int mode_winnum;

	public int mode_winnum_all;

	public double mode_last_10_win_rate;

	public double mode_win_rate;

	public double win_rate;

	public int mode_win_streak;

	public int mode_last_10_max_score;

	public int mode_max_score;

	public int max_score;

	public int hint_num;

	public int hammer_num;

	public int skyrocket_num;

	public string puzzle;

	public string puzzle_bank;

	public int puzzle_file;

	public double level_skill;

	public double user_skill;

	public int assembly_num_start;

	public string[] assembly_num_array_start;

	public int[] level_element;

	public int camera_range;

	public string puzzle_library;

	public int initial_steps;

	public string ring_material;

	public override void Reset()
	{
		((BaseTrackConfig)this).Reset();
		status = null;
		level = 0;
		level_id = 0;
		game_mode = 0;
		game_mode_name = null;
		game_play_method = 0;
		mode_num = 0;
		mode_num_all = 0;
		mode_winnum = 0;
		mode_winnum_all = 0;
		mode_last_10_win_rate = 0.0;
		mode_win_rate = 0.0;
		win_rate = 0.0;
		mode_win_streak = 0;
		mode_last_10_max_score = 0;
		mode_max_score = 0;
		max_score = 0;
		hint_num = 0;
		hammer_num = 0;
		skyrocket_num = 0;
		puzzle = null;
		puzzle_bank = null;
		puzzle_file = 0;
		level_skill = 0.0;
		user_skill = 0.0;
		assembly_num_start = 0;
		assembly_num_array_start = null;
		level_element = null;
		camera_range = 0;
		puzzle_library = null;
		initial_steps = 0;
		ring_material = null;
	}
}
