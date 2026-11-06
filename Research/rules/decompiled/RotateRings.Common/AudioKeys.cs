using System;
using System.Collections.Generic;
using System.Reflection;

namespace RotateRings.Common;

public static class AudioKeys
{
	private static readonly object s_cacheLock = new object();

	private static bool s_cacheBuilt;

	private static Dictionary<string, List<FieldInfo>> s_fieldsByValue;

	public static readonly AudioKey BtnGroupClick = new AudioKey("rr_sfx_group_click");

	public static readonly AudioKey BtnClick = new AudioKey("rr_sfx_click");

	public static readonly AudioKey PopUp = new AudioKey("rr_sfx_reward_open");

	public static readonly AudioKey Unlock = new AudioKey("rr_sfx_unlock");

	public static readonly AudioKey Complete = new AudioKey("rr_sfx_complete");

	public static readonly AudioKey NewItem = new AudioKey("rr_sfx_new_item");

	public static readonly AudioKey RatingPopup = new AudioKey("rr_sfx_rating_popup");

	public static readonly AudioKey LevelComplete = new AudioKey("rr_sfx_level_complete");

	public static readonly AudioKey VoCompleteSquirrel = new AudioKey("rr_vo_complete_squirrel");

	public static readonly AudioKey LevelWinSpineAnimIdle = new AudioKey("rr_vo_complete_squirrel_idle");

	public static readonly AudioKey LevelWinSpineAnimIdle2 = new AudioKey("rr_vo_complete_squirrel_idle2");

	public static readonly AudioKey LevelWinSpineAnimIdle3 = new AudioKey("rr_vo_complete_squirrel_idle4");

	public static readonly AudioKey ChestClick = new AudioKey("rr_sfx_chest_click");

	public static readonly AudioKey RoundStart = new AudioKey("rr_sfx_round_start");

	public static readonly AudioKey UiDeny = new AudioKey("rr_sfx_ui_deny");

	public static readonly AudioKey ProgressChestPop = new AudioKey("rr_sfx_progress_chest_pop");

	public static readonly AudioKey ProgressChestUnlock = new AudioKey("rr_sfx_progress_chest_unlock");

	public static readonly AudioKey ProgressUp = new AudioKey("rr_sfx_progress_up");

	public static readonly AudioKey DailyPuzzlePieceDone = new AudioKey("rr_sfx_daily_puzzle_piece_done");

	public static readonly AudioKey DailyPuzzleAllDone = new AudioKey("rr_sfx_daily_puzzle_all_done");

	public static readonly AudioKey DailyPuzzleStamp = new AudioKey("rr_sfx_daily_puzzle_stamp");

	public static readonly AudioKey DailyPuzzleResult = new AudioKey("rr_sfx_daily_puzzle_result");

	public static readonly AudioKey HardLevel = new AudioKey("rr_sfx_hardlevel");

	public static readonly AudioKey StartLevelHalloween = new AudioKey("rr_sfx_startlevel_halloween");

	public static readonly AudioKey RoundStartStyling = new AudioKey("rr_sfx_round_start_styling");

	public static readonly AudioKey StylingBannerAppear = new AudioKey("rr_sfx_styling_banner_appear");

	public static readonly AudioKey BgmMainTheme = new AudioKey("rr_bgm_maintheme");

	public static readonly AudioKey BgmLevel = new AudioKey("rr_bgm_level_2");

	public static readonly AudioKey BgmLevelNew = new AudioKey("rr_bgm_level2");

	public static readonly AudioKey BgmMainThemeHalloween = new AudioKey("rr_bgm_maintheme_halloween");

	public static readonly AudioKey BgmLevelHalloween = new AudioKey("rr_bgm_level_halloween");

	public static readonly AudioKey RewardOpen = new AudioKey("rr_sfx_reward_open");

	public static readonly AudioKey RewardDouble = new AudioKey("rr_sfx_reward_double");

	public static readonly AudioKey ItemAdd = new AudioKey("rr_sfx_item_add");

	public static readonly AudioKey ItemConfirm = new AudioKey("rr_sfx_item_confirm");

	public static readonly AudioKey ItemHammerGet = new AudioKey("rr_sfx_item_hammer_get");

	public static readonly AudioKey LuckyWheel = new AudioKey("rr_sfx_lucky_wheel");

	public static readonly AudioKey ItemHammerHit = new AudioKey("rr_sfx_item_hammer_hit");

	public static readonly AudioKey Blocked = new AudioKey("rr_sfx_blocked");

	public static readonly AudioKey JellyBlocked = new AudioKey("rr_sfx_jelly_blocked");

	public static readonly AudioKey WoodBlocked = new AudioKey("rr_sfx_wood_blocked");

	public static readonly AudioKey RockBlocked = new AudioKey("rr_sfx_rock_blocked");

	public static readonly AudioKey RingRub = new AudioKey("rr_sfx_ring_rub");

	public static readonly AudioKey RingRatchetClick = new AudioKey("rr_sfx_ring_ratchet_click");

	public static readonly AudioKey StickPullSlide = new AudioKey("rr_sfx_stick_pull_slide");

	public static readonly AudioKey StickDrawSuccess = new AudioKey("rr_sfx_stick_draw_success");

	public static readonly AudioKey CeramicRingBreak = new AudioKey("rr_sfx_ceramicring_break");

	public static readonly AudioKey CeramicRingRelease = new AudioKey("rr_sfx_ceramicring_release");

	public static readonly AudioKey MetalRingBreak = new AudioKey("rr_sfx_metal_break");

	public static readonly AudioKey MetalRingRelease = new AudioKey("rr_sfx_metal_ring_release");

	public static readonly AudioKey FixedClampBreak = new AudioKey("rr_sfx_stick_pull_metal_clamp_break");

	public static readonly AudioKey JellyRingBreak = new AudioKey("rr_sfx_jelly_break");

	public static readonly AudioKey JellyRingRelease = new AudioKey("rr_sfx_jelly_ring_release");

	public static readonly AudioKey WoodBreak = new AudioKey("rr_sfx_wood_break");

	public static readonly AudioKey RockBreak = new AudioKey("rr_sfx_rock_break");

	public static readonly AudioKey GoldBreak = new AudioKey("rr_sfx_gold_break");

	public static readonly AudioKey JadeBreak = new AudioKey("rr_sfx_jade_break");

	public static readonly AudioKey ItemBombBoom = new AudioKey("rr_sfx_item_bomb_boom");

	public static readonly AudioKey ItemBombIdle = new AudioKey("rr_sfx_item_bomb_idle");

	public static readonly AudioKey ItemRocketFire = new AudioKey("rr_sfx_item_rocket_fire");

	public static readonly AudioKey ItemRocketShoot = new AudioKey("rr_sfx_item_rocket_shoot");

	public static readonly AudioKey ItemRocketHit = new AudioKey("rr_sfx_item_rocket_hit_1");

	public static readonly AudioKey AcornCollect = new AudioKey("rr_sfx_acorn_collect");

	public static readonly AudioKey AcornCountUp = new AudioKey("rr_sfx_acorn_count_up");

	public static readonly AudioKey ChainBreak = new AudioKey("rr_sfx_chain_break");

	public static readonly AudioKey ChainShack = new AudioKey("rr_sfx_chain_shack");

	public static readonly AudioKey ChainUnlock = new AudioKey("rr_sfx_chain_unlock");

	public static readonly AudioKey ColorLockKey = new AudioKey("rr_sfx_colorlock_key");

	public static readonly AudioKey ColorLockUnlock = new AudioKey("rr_sfx_colorlock_unlock");

	public static readonly AudioKey VoGreat = new AudioKey("rr_vo_great");

	public static readonly AudioKey VoExcellent = new AudioKey("rr_vo_excellent");

	public static readonly AudioKey VoAmazing = new AudioKey("rr_vo_amazing");

	public static readonly AudioKey VoPerfect = new AudioKey("rr_vo_fantastic");

	public static readonly AudioKey VoUnbelievable = new AudioKey("rr_vo_incredible");

	public static readonly AudioKey VoUnbelievableMulti1 = new AudioKey("rr_vo_incredible2");

	public static readonly AudioKey VoUnbelievableMulti2 = new AudioKey("rr_vo_incredible3");

	public static readonly AudioKey RankUp1 = new AudioKey("zoo_sfx_gp_rankup_1");

	public static readonly AudioKey RankUp2 = new AudioKey("zoo_sfx_gp_rankup_2");

	public static readonly AudioKey RankUp3 = new AudioKey("zoo_sfx_gp_rankup_3");

	public static readonly AudioKey RankUp5 = new AudioKey("zoo_sfx_gp_rankup_5");

	public static readonly AudioKey RankUpSwap = new AudioKey("zoo_sfx_gp_rankup_4");

	public static readonly AudioKey RankUpNotch = new AudioKey("zoo_sfx_gp_rankup_notch");

	public static readonly AudioKey RankUpScoreBegin = new AudioKey("zoo_sfx_gp_rankup_score_begin");

	public static readonly AudioKey RankUpScore1 = new AudioKey("zoo_sfx_gp_rankup_score_1");

	public static readonly AudioKey RankUpScore2 = new AudioKey("zoo_sfx_gp_rankup_score_2");

	public static readonly AudioKey RankUpScore3 = new AudioKey("zoo_sfx_gp_rankup_score_3");

	public static readonly AudioKey RankUpScore4 = new AudioKey("zoo_sfx_gp_rankup_score_4");

	public static readonly AudioKey RankUpScore5 = new AudioKey("zoo_sfx_gp_rankup_score_5");

	public static readonly AudioKey RankUpScore6 = new AudioKey("zoo_sfx_gp_rankup_score_6");

	public static readonly AudioKey RankUpScore7 = new AudioKey("zoo_sfx_gp_rankup_score_7");

	public static readonly AudioKey RankUpScore8 = new AudioKey("zoo_sfx_gp_rankup_score_8");

	public static readonly AudioKey RankUpScore9 = new AudioKey("zoo_sfx_gp_rankup_score_9");

	public static readonly AudioKey LevelWinIQ = new AudioKey("rr_sfx_iqup");

	public static readonly AudioKey MovesAddUp = new AudioKey("rr_sfx_moves_add_up");

	public static readonly AudioKey MovesOutAlert = new AudioKey("rr_sfx_moves_out_alert");

	public static readonly AudioKey MovesOutFail = new AudioKey("rr_sfx_moves_out_fail");

	public static readonly AudioKey[] RankUpScores = new AudioKey[9] { RankUpScore1, RankUpScore2, RankUpScore3, RankUpScore4, RankUpScore5, RankUpScore6, RankUpScore7, RankUpScore8, RankUpScore9 };

	public static readonly AudioKey CelebratePlaceholder = new AudioKey("rr_sfx_celebrate_placeholder");

	public static readonly AudioKey CelebrateConfetti = new AudioKey("rr_sfx_celebrate_confetti");

	public static readonly AudioKey CelebrateLogo = new AudioKey("rr_sfx_celebrate_logo");

	public static readonly AudioKey CelebrateLogoDeluxe = new AudioKey("rr_sfx_celebrate_logo_deluxe");

	public static readonly AudioKey CelebrateText = new AudioKey("rr_sfx_celebrate_text");

	public static readonly AudioKey CelebratePreResultDeluxe = new AudioKey("rr_sfx_pre_result");

	public static readonly AudioKey CelebratePreResultText = new AudioKey("rr_sfx_pre_result_2");

	public static readonly AudioKey CelebratePreResultConfetti = new AudioKey("rr_sfx_pre_result_3");

	public static readonly AudioKey CelebratePreResultLogo = new AudioKey("rr_sfx_pre_result_4");

	public static readonly AudioKey OpenShop = new AudioKey("rr_sfx_store_open");

	public static int ApplyOverrides(IReadOnlyDictionary<string, string> overrides)
	{
		if (overrides == null || overrides.Count == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (KeyValuePair<string, string> @override in overrides)
		{
			if (ChangeInternal(@override.Key, @override.Value))
			{
				num++;
			}
		}
		return num;
	}

	private static bool ChangeInternal(string key, string toKey)
	{
		if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(toKey))
		{
			return false;
		}
		EnsureCache();
		bool flag = false;
		if (s_fieldsByValue.TryGetValue(key, out var value) && value != null && value.Count > 0)
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].SetValue(null, new AudioKey(toKey));
			}
			flag = true;
		}
		if (flag)
		{
			RebuildCache();
		}
		return flag;
	}

	private static void EnsureCache()
	{
		if (s_cacheBuilt)
		{
			return;
		}
		lock (s_cacheLock)
		{
			if (!s_cacheBuilt)
			{
				RebuildCache_NoLock();
				s_cacheBuilt = true;
			}
		}
	}

	private static void RebuildCache()
	{
		lock (s_cacheLock)
		{
			RebuildCache_NoLock();
			s_cacheBuilt = true;
		}
	}

	private static void RebuildCache_NoLock()
	{
		s_fieldsByValue = new Dictionary<string, List<FieldInfo>>(StringComparer.Ordinal);
		FieldInfo[] fields = typeof(AudioKeys).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!(fieldInfo.FieldType != typeof(AudioKey)))
			{
				string key = ((AudioKey)fieldInfo.GetValue(null)).Value ?? string.Empty;
				if (!s_fieldsByValue.TryGetValue(key, out var value))
				{
					value = new List<FieldInfo>(1);
					s_fieldsByValue[key] = value;
				}
				value.Add(fieldInfo);
			}
		}
	}
}
