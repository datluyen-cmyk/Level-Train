using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Tag;

namespace XingYun.GM;

public static class TagGM
{
	public static void BuildTagPanel(Transform root)
	{
		RuntimeGUIBuilder.CreateButton(root, "刷新所有 Tag", (Action)delegate
		{
			Transform obj2 = root.Find("TagInfoText");
			Text val = ((obj2 != null) ? ((Component)obj2).GetComponent<Text>() : null);
			if ((Object)(object)val != (Object)null)
			{
				val.text = GetAllTagsString();
				Transform obj3 = root;
				LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((obj3 is RectTransform) ? obj3 : null));
			}
		}, 80f, (TextAnchor)4);
		Text obj = RuntimeGUIBuilder.CreateLabel(root, "Click Refresh...", 24, (Color?)null, (TextAnchor)3);
		((Object)obj).name = "TagInfoText";
		obj.alignment = (TextAnchor)0;
		((Component)obj).gameObject.AddComponent<ContentSizeFitter>().verticalFit = (FitMode)2;
	}

	private static string GetAllTagsString()
	{
		TagManager tag = GameApp.Tag;
		if (tag == null)
		{
			return "TagManager instance is null";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("========== All Tags ==========");
		AppendTag(stringBuilder, "game_version", tag.GameVersion);
		AppendTag(stringBuilder, "res_version", tag.ResVersion);
		AppendTag(stringBuilder, "aot_version", tag.AotVersion);
		AppendTag(stringBuilder, "device_info", tag.DeviceInfo);
		AppendTag(stringBuilder, "language", tag.Language);
		AppendTag(stringBuilder, "#distinct_id", tag.DistinctId);
		AppendTag(stringBuilder, "app_memory", tag.AppMemory);
		AppendTag(stringBuilder, "app_version", tag.AppVersion);
		AppendTag(stringBuilder, "first_app_version", tag.FirstAppVersion);
		AppendTag(stringBuilder, "active_day", tag.ActiveDay);
		AppendTag(stringBuilder, "install_day", tag.InstallDay);
		AppendTag(stringBuilder, "network_country", tag.NetworkCountry);
		AppendTag(stringBuilder, "sim_country", tag.SimCountry);
		AppendTag(stringBuilder, "screen_size", tag.ScreenSize);
		AppendTag(stringBuilder, "device_level", tag.DeviceLevel);
		AppendTag(stringBuilder, "vibration", tag.Vibration);
		AppendTag(stringBuilder, "sound", tag.Sound);
		AppendTag(stringBuilder, "music", tag.Music);
		AppendTag(stringBuilder, "highlight_free_tiles", tag.HighlightFreeTiles);
		AppendTag(stringBuilder, "layer_id", tag.LayerId);
		AppendTag(stringBuilder, "first_userwaynum", tag.FirstUserwaynum);
		AppendTag(stringBuilder, "userwaynum", tag.Userwaynum);
		AppendTag(stringBuilder, "active_ab", tag.ActiveAb);
		AppendTag(stringBuilder, "history_ab", tag.HistoryAb);
		AppendTag(stringBuilder, "last_7d_info", tag.Last7dInfo);
		AppendTag(stringBuilder, "last_3_inter_info", tag.Last3InterInfo);
		AppendTag(stringBuilder, "first3_inter_income_list", tag.First3InterIncomeList);
		AppendTag(stringBuilder, "last3_inter_income_list", tag.Last3InterIncomeList);
		AppendTag(stringBuilder, "first3_reward_income_list", tag.First3RewardIncomeList);
		AppendTag(stringBuilder, "last3_reward_income_list", tag.Last3RewardIncomeList);
		AppendTag(stringBuilder, "last_30_day_timestamp", tag.Last30DayTimestamp);
		AppendTag(stringBuilder, "last30_day_ad_revenue_info_list", tag.Last30DayAdRevenueInfoList);
		AppendTag(stringBuilder, "ad_revenue_info", tag.AdRevenueInfo);
		AppendTag(stringBuilder, "ad_request_info", tag.AdRequestInfo);
		AppendTag(stringBuilder, "first3_inter_ecpm", tag.First3InterEcpm);
		AppendTag(stringBuilder, "first3_inter_group", tag.First3InterGroup);
		AppendTag(stringBuilder, "last3_inter_ecpm", tag.Last3InterEcpm);
		AppendTag(stringBuilder, "last3_inter_group", tag.Last3InterGroup);
		AppendTag(stringBuilder, "game_num_all", tag.GameNumAll);
		AppendTag(stringBuilder, "game_start_num_all", tag.GameStartNumAll);
		AppendTag(stringBuilder, "game_win_num_all", tag.GameWinNumAll);
		AppendTag(stringBuilder, "appsflyer_gaid", tag.AppsflyerGaid);
		AppendTag(stringBuilder, "loading_type", tag.LoadingType);
		AppendTag(stringBuilder, "expt_period", tag.ExptPeriod);
		AppendTag(stringBuilder, "physics_width", tag.PhysicsWidth);
		AppendTag(stringBuilder, "physics_height", tag.PhysicsHeight);
		AppendTag(stringBuilder, "physics_unit_per100", tag.PhysicsUnitPer100);
		AppendTag(stringBuilder, "screen_unit_per100", tag.ScreenUnitPer100);
		AppendTag(stringBuilder, "screen_data_is_trust", tag.ScreenDataIsTrust);
		AppendTag(stringBuilder, "age_group", tag.AgeGroup);
		AppendTag(stringBuilder, "acquisition_channel", tag.AcquisitionChannel);
		AppendTag(stringBuilder, "acquisition_channel_value", tag.AcquisitionChannelValue);
		AppendTag(stringBuilder, "country_gdp_group", tag.CountryGdpGroup);
		AppendTag(stringBuilder, "primary_control_method", tag.PrimaryControlMethod);
		AppendTag(stringBuilder, "secondary_control_method", tag.SecondaryControlMethod);
		AppendTag(stringBuilder, "time_since_last_session", tag.TimeSinceLastSession);
		AppendTag(stringBuilder, "rem_shuffle_count", tag.RemShuffleCount);
		AppendTag(stringBuilder, "rem_hint_count", tag.RemHintCount);
		AppendTag(stringBuilder, "main_stage_pass_count", tag.MainStagePassCount);
		AppendTag(stringBuilder, "travel_stage_pass_count", tag.TravelStagePassCount);
		AppendTag(stringBuilder, "daily_challenge_pass_count", tag.DailyChallengePassCount);
		AppendTag(stringBuilder, "avg_inter_ads_per_launch_last3", tag.AvgInterAdsPerLaunchLast3);
		AppendTag(stringBuilder, "avg_reward_ads_per_launch_last3", tag.AvgRewardAdsPerLaunchLast3);
		AppendTag(stringBuilder, "last7_restart_count_all", tag.Last7RestartCountAll);
		AppendTag(stringBuilder, "last7_restart_count_main", tag.Last7RestartCountMain);
		AppendTag(stringBuilder, "last7_restart_count_travel", tag.Last7RestartCountTravel);
		AppendTag(stringBuilder, "last7_death_count_all", tag.Last7DeathCountAll);
		AppendTag(stringBuilder, "last7_death_count_main", tag.Last7DeathCountMain);
		AppendTag(stringBuilder, "last7_death_count_travel", tag.Last7DeathCountTravel);
		AppendTag(stringBuilder, "last7_perfect_combo_break_count_main", tag.Last7PerfectComboBreakCountMain);
		AppendTag(stringBuilder, "last7_shuffle_count_all", tag.Last7ShuffleCountAll);
		AppendTag(stringBuilder, "last7_shuffle_count_main", tag.Last7ShuffleCountMain);
		AppendTag(stringBuilder, "last7_shuffle_count_travel", tag.Last7ShuffleCountTravel);
		AppendTag(stringBuilder, "last7_hint_count_all", tag.Last7HintCountAll);
		AppendTag(stringBuilder, "last7_hint_count_main", tag.Last7HintCountMain);
		AppendTag(stringBuilder, "last7_hint_count_travel", tag.Last7HintCountTravel);
		AppendTag(stringBuilder, "avg_games_per_launch_last3", tag.AvgGamesPerLaunchLast3);
		AppendTag(stringBuilder, "avg_games_per_launch_last7", tag.AvgGamesPerLaunchLast7);
		AppendTag(stringBuilder, "avg_ability_score_last7", tag.AvgAbilityScoreLast7);
		AppendTag(stringBuilder, "last7_win_rate_main", tag.Last7WinRateMain);
		AppendTag(stringBuilder, "last7_win_rate_travel", tag.Last7WinRateTravel);
		AppendTag(stringBuilder, "last7_avg_op_duration_main", tag.Last7AvgOpDurationMain);
		AppendTag(stringBuilder, "last7_avg_op_duration_travel", tag.Last7AvgOpDurationTravel);
		AppendTag(stringBuilder, "avg_reward_ecpm_first3", tag.AvgRewardEcpmFirst3);
		AppendTag(stringBuilder, "avg_reward_ecpm_last3", tag.AvgRewardEcpmLast3);
		AppendTag(stringBuilder, "new_user_ltv_day1", tag.NewUserLtvDay1);
		AppendTag(stringBuilder, "new_user_ltv_day3", tag.NewUserLtvDay3);
		AppendTag(stringBuilder, "new_user_ltv_day7", tag.NewUserLtvDay7);
		AppendTag(stringBuilder, "new_user_ltv_day14", tag.NewUserLtvDay14);
		AppendTag(stringBuilder, "inter_ad_revenue", tag.InterAdRevenue);
		AppendTag(stringBuilder, "reward_ad_revenue", tag.RewardAdRevenue);
		AppendTag(stringBuilder, "total_ad_revenue", tag.TotalAdRevenue);
		AppendTag(stringBuilder, "last7_ad_play_info", tag.Last7AdPlayInfo);
		AppendTag(stringBuilder, "last7_inter_ad_escape_rate", tag.Last7InterAdEscapeRate);
		AppendTag(stringBuilder, "last7_reward_ad_escape_rate", tag.Last7RewardAdEscapeRate);
		return stringBuilder.ToString();
	}

	private static void AppendTag(StringBuilder sb, string name, object value)
	{
		string text = "null";
		if (value != null)
		{
			if (value is string || value.GetType().IsPrimitive)
			{
				text = value.ToString();
			}
			else
			{
				try
				{
					text = JsonConvert.SerializeObject(value);
				}
				catch
				{
					text = value.ToString();
				}
			}
		}
		sb.AppendLine("[Tag] " + name + ": " + text);
	}
}
