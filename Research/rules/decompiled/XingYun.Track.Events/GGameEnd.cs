using System;

namespace XingYun.Track.Events;

[Serializable]
public class GGameEnd : BaseTrackConfig
{
	public const string EventName = "g_game_end";

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

	public int mode_win_streak_day;

	public int mode_last_10_max_score;

	public int mode_max_score;

	public int max_score;

	public int game_score;

	public int game_time;

	public int game_time_real;

	public int hint_num;

	public int hammer_num;

	public int hammer_use_num;

	public int skyrocket_num;

	public int skyrocket_use_num;

	public int hammer_ad_num;

	public int peak_trial_streak;

	public int assembly_num_start;

	public int assembly_num_end;

	public string[] assembly_num_array_start;

	public string[] assembly_num_array_end;

	public string puzzle;

	public string puzzle_bank;

	public int puzzle_file;

	public double level_skill;

	public double user_skill;

	public double rate;

	public string game_end_board;

	public int first_operation_time;

	public double nondeterminacy_d;

	public double time_factor_m;

	public double level_coefficient_n;

	public double user_skill_sub_rq_before_update;

	public double user_skill_sub_rq_last_update;

	public double expected_duration_et;

	public int[] hammer_step_array;

	public int[] hammer_assembly_array;

	public int[] bomb_step_array;

	public int[] bomb_num_array;

	public int[] bomb_assembly_array;

	public int[] assembly_clear_id_array;

	public double[] assembly_clear_time_array;

	public double[] move_time_array;

	public int[] level_element;

	public int restore_num;

	public int camera_range;

	public string iq;

	public double[] board_scale;

	public string puzzle_library;

	public int initial_steps;

	public int used_steps;

	public int[] add_steps_times;

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
		mode_win_streak_day = 0;
		mode_last_10_max_score = 0;
		mode_max_score = 0;
		max_score = 0;
		game_score = 0;
		game_time = 0;
		game_time_real = 0;
		hint_num = 0;
		hammer_num = 0;
		hammer_use_num = 0;
		skyrocket_num = 0;
		skyrocket_use_num = 0;
		hammer_ad_num = 0;
		peak_trial_streak = 0;
		assembly_num_start = 0;
		assembly_num_end = 0;
		assembly_num_array_start = null;
		assembly_num_array_end = null;
		puzzle = null;
		puzzle_bank = null;
		puzzle_file = 0;
		level_skill = 0.0;
		user_skill = 0.0;
		rate = 0.0;
		game_end_board = null;
		first_operation_time = 0;
		nondeterminacy_d = 0.0;
		time_factor_m = 0.0;
		level_coefficient_n = 0.0;
		user_skill_sub_rq_before_update = 0.0;
		user_skill_sub_rq_last_update = 0.0;
		expected_duration_et = 0.0;
		hammer_step_array = null;
		hammer_assembly_array = null;
		bomb_step_array = null;
		bomb_num_array = null;
		bomb_assembly_array = null;
		assembly_clear_id_array = null;
		assembly_clear_time_array = null;
		move_time_array = null;
		level_element = null;
		restore_num = 0;
		camera_range = 0;
		iq = null;
		board_scale = null;
		puzzle_library = null;
		initial_steps = 0;
		used_steps = 0;
		add_steps_times = null;
		ring_material = null;
	}
}
