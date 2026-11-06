using System;
using System.Collections.Generic;
using System.Text;
using RotateRings.Common;
using UnityEngine;
using XingYun.Ad;
using XingYun.Core.Singletons;
using XingYun.Proxy;

namespace RotateRings.Ad;

public static class InterstitialEscapeTracker
{
	private const string EscapeRateTag = "<Ad>[EscapeRateCd] ";

	private const string TAG = "<Ad>[InterstitialEscape]";

	private const string PrefsKeyClosePending = "ad.interstitial_close_pending.v1";

	private const string PrefsKeyAfterShow = "ad.interstitial_close_pending.after_show.v1";

	private const float EscapeSkipDuration = 180f;

	private const int EscapeBufferCycleLength = 3;

	private const int EscapeTrackingActivationCount = 7;

	private static int escapeSkipTimerId;

	private static float skipPeriodEndUnscaledTime = -1f;

	private static bool? nextShowIsAfterShow;

	private static bool interstitialPlayConfirmed;

	private static Func<float?> interstitialCdElapsedProvider;

	public static int SessionInterstitialPlayCount { get; private set; }

	public static IInterstitialCooldownStrategy CooldownStrategy { get; set; }

	public static bool HasPlayedInterstitial
	{
		get
		{
			InterstitialEscapeRecord record = GetRecord();
			if (record != null)
			{
				return record.TotalCallCount > 0;
			}
			return false;
		}
	}

	public static int SuccessfulInterstitialCount => GetRecord()?.ShowAdSuccessfulCount ?? 0;

	public static bool HasSuccessfullyPlayedInterstitial => SuccessfulInterstitialCount > 0;

	public static int ShowAdPostSuccessfulCount => GetRecord()?.ShowAdPostSuccessfulCount ?? 0;

	private static InterstitialEscapeWindowRecord GetEscapeRateRecord()
	{
		return GameApp.Save.GetOrCreate<InterstitialEscapeWindowRecord>();
	}

	public static bool IsExperimentActive()
	{
		return EscapeRateCdMatrix.IsValidGroup(GetGroup());
	}

	public static int GetGroup()
	{
		return FeatureConfig.InterstitialFunction.EscapeCdGroup;
	}

	private static void RecordEscapeRateResult(bool isSuccess)
	{
		InterstitialEscapeWindowRecord escapeRateRecord = GetEscapeRateRecord();
		if (escapeRateRecord.EscapeWindow.Count >= 3)
		{
			escapeRateRecord.EscapeWindow.RemoveAt(0);
		}
		escapeRateRecord.EscapeWindow.Add(!isSuccess);
		escapeRateRecord.RewardBonusSeconds = 0;
		escapeRateRecord.RewardRequiredElapsedSeconds = 0f;
	}

	public static void OnRewardAdFinished()
	{
		if (EscapeIntensityCdExperiment.IsActive())
		{
			EscapeIntensityCdExperiment.OnRewardAdFinished();
			return;
		}
		int num = GetGroup();
		if (!EscapeRateCdMatrix.IsValidGroup(num))
		{
			return;
		}
		InterstitialEscapeWindowRecord escapeRateRecord = GetEscapeRateRecord();
		if (escapeRateRecord.EscapeWindow.Count == 0)
		{
			return;
		}
		float? interstitialCdElapsedSeconds = GetInterstitialCdElapsedSeconds();
		if (interstitialCdElapsedSeconds.HasValue)
		{
			float value = interstitialCdElapsedSeconds.Value;
			float num2 = ResolveEscapeRateBaseCdSeconds();
			if (!(num2 < 0f))
			{
				int installDay = GetInstallDay();
				int rewardBonusSeconds = EscapeRateCdMatrix.GetRewardBonusSeconds(num, installDay);
				escapeRateRecord.RewardBonusSeconds += rewardBonusSeconds;
				float num3 = Math.Max(Math.Max(value, num2), escapeRateRecord.RewardRequiredElapsedSeconds);
				escapeRateRecord.RewardRequiredElapsedSeconds = num3 + (float)rewardBonusSeconds;
				Math.Max(0f, escapeRateRecord.RewardRequiredElapsedSeconds - value);
			}
		}
	}

	public static float ResolveCooldownSeconds()
	{
		int num = GetGroup();
		InterstitialEscapeWindowRecord escapeRateRecord = GetEscapeRateRecord();
		if (!EscapeRateCdMatrix.IsValidGroup(num) || escapeRateRecord.EscapeWindow.Count == 0)
		{
			return 70f;
		}
		float num2 = ResolveEscapeRateBaseCdSeconds();
		if (num2 < 0f)
		{
			return 70f;
		}
		float num3 = num2 + (float)escapeRateRecord.RewardBonusSeconds;
		if (escapeRateRecord.RewardBonusSeconds > 0 && escapeRateRecord.RewardRequiredElapsedSeconds > num3)
		{
			num3 = escapeRateRecord.RewardRequiredElapsedSeconds;
		}
		GetInstallDay();
		GetEscapeRateBucket(escapeRateRecord.EscapeWindow);
		return num3;
	}

	private static float ResolveEscapeRateBaseCdSeconds()
	{
		int num = GetGroup();
		InterstitialEscapeWindowRecord escapeRateRecord = GetEscapeRateRecord();
		if (!EscapeRateCdMatrix.IsValidGroup(num) || escapeRateRecord.EscapeWindow.Count == 0)
		{
			return -1f;
		}
		int installDay = GetInstallDay();
		int escapeRateBucket = GetEscapeRateBucket(escapeRateRecord.EscapeWindow);
		return EscapeRateCdMatrix.GetCdSeconds(num, installDay, escapeRateBucket);
	}

	public static string BuildEscapeRateCooldownCalculationLog()
	{
		StringBuilder stringBuilder = new StringBuilder(768);
		stringBuilder.AppendLine("--- 插屏 CD 运算 ---");
		stringBuilder.AppendLine("策略=逃逸率实验CD（激活天 × 最近3次逃逸率分桶 + 激励加成）");
		int num = GetGroup();
		stringBuilder.Append("escapeCdGroup=").Append(num);
		if (!EscapeRateCdMatrix.IsValidGroup(num))
		{
			float value = 70f;
			stringBuilder.AppendLine("（未启用或无效，回退固定CD）");
			stringBuilder.Append("finalCd=InsertCd=").Append(value).Append('s');
			return stringBuilder.ToString();
		}
		stringBuilder.AppendLine();
		InterstitialEscapeWindowRecord escapeRateRecord = GetEscapeRateRecord();
		stringBuilder.Append("window=").Append(FormatWindow(escapeRateRecord.EscapeWindow)).Append(" rewardBonusSeconds=")
			.Append(escapeRateRecord.RewardBonusSeconds)
			.Append('s')
			.Append(" rewardRequiredElapsed=")
			.Append(escapeRateRecord.RewardRequiredElapsedSeconds.ToString("F1"))
			.Append('s')
			.AppendLine();
		if (escapeRateRecord.EscapeWindow.Count == 0)
		{
			float value2 = 70f;
			stringBuilder.AppendLine("窗口为空（生涯首次插屏展示前），回退固定CD");
			stringBuilder.Append("finalCd=InsertCd=").Append(value2).Append('s');
			return stringBuilder.ToString();
		}
		int installDay = GetInstallDay();
		int escapeRateBucket = GetEscapeRateBucket(escapeRateRecord.EscapeWindow);
		int count = escapeRateRecord.EscapeWindow.Count;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			if (escapeRateRecord.EscapeWindow[i])
			{
				num2++;
			}
		}
		float cdSeconds = EscapeRateCdMatrix.GetCdSeconds(num, installDay, escapeRateBucket);
		stringBuilder.Append("installDay=").Append(installDay).Append(" escapes=")
			.Append(num2)
			.Append('/')
			.Append(count)
			.Append(" bucket=")
			.Append(escapeRateBucket)
			.AppendLine();
		stringBuilder.AppendLine("分桶: 0=逃逸0; 1=(0,33.4%]; 2=(33.4%,50.1%]; 3=(50.1%,66.7%]; 4=>66.7%");
		if (cdSeconds < 0f)
		{
			float value3 = 70f;
			stringBuilder.AppendLine("矩阵未命中，回退固定CD");
			stringBuilder.Append("finalCd=InsertCd=").Append(value3).Append('s');
			return stringBuilder.ToString();
		}
		float num3 = cdSeconds + (float)escapeRateRecord.RewardBonusSeconds;
		float num4 = num3;
		if (escapeRateRecord.RewardBonusSeconds > 0 && escapeRateRecord.RewardRequiredElapsedSeconds > num4)
		{
			num4 = escapeRateRecord.RewardRequiredElapsedSeconds;
		}
		stringBuilder.Append("baseCd=matrix[group,installDay,bucket]=").Append(cdSeconds).Append('s')
			.AppendLine();
		stringBuilder.Append("nominalCd=baseCd+rewardBonus=").Append(cdSeconds).Append('+')
			.Append(escapeRateRecord.RewardBonusSeconds)
			.Append('=')
			.Append(num3)
			.Append('s')
			.AppendLine();
		stringBuilder.Append("finalCd=max(nominalCd,rewardRequiredElapsed)=").Append(num4).Append('s');
		stringBuilder.AppendLine();
		stringBuilder.Append("门控=elapsed>=interval（完整间隔，无 GP39 70% 提前放行）");
		return stringBuilder.ToString();
	}

	private static int GetEscapeRateBucket(List<bool> window)
	{
		int count = window.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (window[i])
			{
				num++;
			}
		}
		if (num == 0)
		{
			return 0;
		}
		if (num * 1000 <= count * 334)
		{
			return 1;
		}
		if (num * 1000 <= count * 501)
		{
			return 2;
		}
		if (num * 1000 <= count * 667)
		{
			return 3;
		}
		return 4;
	}

	private static int GetInstallDay()
	{
		return Math.Max(1, AdBridgeHelper.GetInstallDays());
	}

	private static string FormatWindow(List<bool> window)
	{
		if (window == null || window.Count == 0)
		{
			return "[]";
		}
		StringBuilder stringBuilder = new StringBuilder(window.Count * 2 + 2);
		stringBuilder.Append('[');
		for (int i = 0; i < window.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(window[i]);
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	public static void SetInterstitialCdElapsedProvider(Func<float?> provider)
	{
		interstitialCdElapsedProvider = provider;
	}

	internal static float? GetInterstitialCdElapsedSeconds()
	{
		return interstitialCdElapsedProvider?.Invoke();
	}

	private static InterstitialEscapeRecord GetRecord()
	{
		return GameApp.Save.GetOrCreate<InterstitialEscapeRecord>();
	}

	public static void ResetSuccessfulInterstitialCount()
	{
		InterstitialEscapeRecord record = GetRecord();
		record.ShowAdSuccessfulCount = 0;
		record.ShowAdPostSuccessfulCount = 0;
	}

	public static bool IsInActiveEscapeSkipPeriod()
	{
		return GetRecord().IsInEscapeSkipPeriod;
	}

	public static float GetEscapeSkipRemainingSeconds()
	{
		if (!IsInActiveEscapeSkipPeriod())
		{
			return 0f;
		}
		if (!(skipPeriodEndUnscaledTime > 0f))
		{
			return 180f;
		}
		return Math.Max(0f, skipPeriodEndUnscaledTime - Time.realtimeSinceStartup);
	}

	public static bool IsInterstitialPlayConfirmed()
	{
		return interstitialPlayConfirmed;
	}

	public static void PrepareNextInterstitialShow(bool isAfterShow)
	{
		nextShowIsAfterShow = isAfterShow;
		interstitialPlayConfirmed = false;
	}

	public static void ClearPreparedShow()
	{
		nextShowIsAfterShow = null;
	}

	public static bool TryMarkInterstitialShowingFromPrepared()
	{
		if (interstitialPlayConfirmed && GetRecord().InterstitialClosePending)
		{
			return true;
		}
		if (!nextShowIsAfterShow.HasValue)
		{
			return false;
		}
		bool value = nextShowIsAfterShow.Value;
		nextShowIsAfterShow = null;
		interstitialPlayConfirmed = true;
		InterstitialBackgroundEscapeTracker.NotifyAdStarted();
		MarkInterstitialShowing(value);
		return true;
	}

	public static void Record(bool isSuccess, bool isAfterShow, bool readEscapeDwellFromSdk = false)
	{
		SessionInterstitialPlayCount++;
		InterstitialEscapeRecord record = GetRecord();
		record.TotalCallCount++;
		if (isSuccess)
		{
			record.ShowAdSuccessfulCount++;
			if (isAfterShow)
			{
				record.ShowAdPostSuccessfulCount++;
			}
		}
		RecordEscapeRateResult(isSuccess);
		EscapeIntensityCdExperiment.OnInterstitialClosed(isSuccess, readEscapeDwellFromSdk);
		CooldownStrategy?.CalculateCooldown();
		bool item = !isSuccess;
		if (record.TotalCallCount > 7)
		{
			if (record.EscapeList.Count >= 3)
			{
				record.EscapeList.Clear();
			}
			record.EscapeList.Add(item);
			int count = record.EscapeList.Count;
			bool flag = AllEscape(record.EscapeList);
			_ = count == 3 && flag;
		}
	}

	public static void RecordAndEvaluate(bool isSuccess, bool isAfterShow, bool readEscapeDwellFromSdk = false)
	{
		AdProxy obj = Singleton<ProxyManager>.Instance.Get<AdProxy>();
		if (obj != null)
		{
			obj.UpdateInterAdClose(isSuccess);
		}
		Record(isSuccess, isAfterShow, readEscapeDwellFromSdk);
		InterstitialEscapeRecord record = GetRecord();
		if (!record.IsInEscapeSkipPeriod && record.EscapeSkipCount == 0)
		{
			EvaluateSkipEligibility();
		}
		LogRecordClose(isSuccess, isAfterShow, record);
	}

	public static void EvaluateSkipEligibility()
	{
		InterstitialEscapeRecord record = GetRecord();
		int count = record.EscapeList.Count;
		if (count == 1 && record.EscapeList[0])
		{
			record.IsInEscapeSkipPeriod = true;
			record.EscapeSkipBatchTotal = 0;
			StartSkipTimer();
		}
		else if (count == 2 && AllEscape(record.EscapeList))
		{
			record.EscapeSkipCount = 1;
			record.EscapeSkipBatchTotal = 1;
		}
		else if (count == 3 && AllEscape(record.EscapeList))
		{
			record.EscapeSkipCount = 2;
			record.EscapeSkipBatchTotal = 2;
		}
	}

	public static void MarkInterstitialShowing(bool isAfterShow)
	{
		InterstitialEscapeRecord record = GetRecord();
		record.InterstitialClosePending = true;
		record.PendingInterstitialIsAfterShow = isAfterShow;
		WriteKillEscapePrefs(isAfterShow);
		LogKillEscapeState("杀端检测标记已写入", isAfterShow);
	}

	public static void ClearInterstitialClosePending()
	{
		if (interstitialPlayConfirmed)
		{
			InterstitialEscapeRecord record = GetRecord();
			if (record.InterstitialClosePending)
			{
				LogKillEscapeState("杀端检测标记已清除", record.PendingInterstitialIsAfterShow);
				record.InterstitialClosePending = false;
				record.PendingInterstitialIsAfterShow = false;
				interstitialPlayConfirmed = false;
				ClearKillEscapePrefs();
			}
		}
	}

	public static bool HandlePendingKillEscape(bool recordEscape)
	{
		if (!TryConsumePendingClose(out var isAfterShow))
		{
			return false;
		}
		InterstitialBackgroundEscapeTracker.NotifyAdClosed();
		if (!recordEscape)
		{
			AdProxy obj = Singleton<ProxyManager>.Instance.Get<AdProxy>();
			if (obj != null)
			{
				obj.UpdateInterAdClose(false);
			}
			return true;
		}
		RecordAndEvaluate(isSuccess: false, isAfterShow, readEscapeDwellFromSdk: true);
		return true;
	}

	public static void RestoreFromSave()
	{
		if (GetRecord().IsInEscapeSkipPeriod)
		{
			StartSkipTimer();
		}
	}

	public static void ClearEscapeSkipState()
	{
		KillSkipTimer();
		ClearInterstitialClosePending();
		ClearPreparedShow();
		interstitialPlayConfirmed = false;
		InterstitialEscapeRecord record = GetRecord();
		record.EscapeSkipCount = 0;
		record.EscapeSkipBatchTotal = 0;
		record.IsInEscapeSkipPeriod = false;
		record.EscapeSkipExpireTimestamp = 0L;
		record.FirstInterstitialLevelAfterEscape = 0;
		record.InterstitialClosePending = false;
		record.PendingInterstitialIsAfterShow = false;
		record.EscapeList.Clear();
		ClearKillEscapePrefs();
		InterstitialBackgroundEscapeTracker.ClearState();
	}

	private static void LogRecordClose(bool isSuccess, bool isAfterShow, InterstitialEscapeRecord record)
	{
		_ = !isSuccess;
		_ = record.TotalCallCount;
	}

	private static bool TryConsumePendingClose(out bool isAfterShow)
	{
		isAfterShow = false;
		InterstitialEscapeRecord record = GetRecord();
		bool flag = PlayerPrefs.GetInt("ad.interstitial_close_pending.v1", 0) == 1;
		if (!record.InterstitialClosePending && !flag)
		{
			LogKillEscapeState("杀端检测：无待补记标记", record.PendingInterstitialIsAfterShow);
			return false;
		}
		bool interstitialClosePending = record.InterstitialClosePending;
		isAfterShow = (interstitialClosePending ? record.PendingInterstitialIsAfterShow : (PlayerPrefs.GetInt("ad.interstitial_close_pending.after_show.v1", 0) == 1));
		LogKillEscapeState(interstitialClosePending ? "杀端检测：消费待补记标记(source=save)" : "杀端检测：消费待补记标记(source=prefs)", isAfterShow);
		record.InterstitialClosePending = false;
		record.PendingInterstitialIsAfterShow = false;
		interstitialPlayConfirmed = false;
		ClearKillEscapePrefs();
		return true;
	}

	private static void LogKillEscapeState(string action, bool isAfterShow)
	{
		GetRecord();
	}

	private static void WriteKillEscapePrefs(bool isAfterShow)
	{
		PlayerPrefs.SetInt("ad.interstitial_close_pending.v1", 1);
		PlayerPrefs.SetInt("ad.interstitial_close_pending.after_show.v1", isAfterShow ? 1 : 0);
		PlayerPrefs.Save();
	}

	private static void ClearKillEscapePrefs()
	{
		if (PlayerPrefs.HasKey("ad.interstitial_close_pending.v1") || PlayerPrefs.HasKey("ad.interstitial_close_pending.after_show.v1"))
		{
			PlayerPrefs.DeleteKey("ad.interstitial_close_pending.v1");
			PlayerPrefs.DeleteKey("ad.interstitial_close_pending.after_show.v1");
			PlayerPrefs.Save();
		}
	}

	private static void StartSkipTimer()
	{
		KillSkipTimer();
		skipPeriodEndUnscaledTime = Time.realtimeSinceStartup + 180f;
		escapeSkipTimerId = GameApp.Timer.Delay(180f, (Action)OnSkipTimerExpired);
	}

	private static void KillSkipTimer()
	{
		if (escapeSkipTimerId != 0)
		{
			GameApp.Timer.Kill(escapeSkipTimerId);
			escapeSkipTimerId = 0;
			skipPeriodEndUnscaledTime = -1f;
		}
	}

	private static void OnSkipTimerExpired()
	{
		escapeSkipTimerId = 0;
		skipPeriodEndUnscaledTime = -1f;
		InterstitialEscapeRecord record = GetRecord();
		record.IsInEscapeSkipPeriod = false;
		record.EscapeSkipExpireTimestamp = 0L;
	}

	public static string BuildEscapeStateSummary()
	{
		InterstitialEscapeRecord record = GetRecord();
		float escapeSkipRemainingSeconds = GetEscapeSkipRemainingSeconds();
		return $"skipPeriod={record.IsInEscapeSkipPeriod} escape180sRemaining={escapeSkipRemainingSeconds:F1}s skipCount={record.EscapeSkipCount}/{record.EscapeSkipBatchTotal} escapeList={FormatEscapeList(record.EscapeList)} totalCallCount={record.TotalCallCount}";
	}

	public static void LogEscapeCountSkipApplied(bool isFrontAd, int remainingAfterConsume, int batchTotal, int consumedIndex)
	{
	}

	public static void LogEscapePeriodSkipApplied(bool isFrontAd, float remainingSeconds)
	{
	}

	private static bool AllEscape(List<bool> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i])
			{
				return false;
			}
		}
		return true;
	}

	private static string FormatEscapeList(List<bool> escapeList)
	{
		if (escapeList == null || escapeList.Count == 0)
		{
			return "[]";
		}
		StringBuilder stringBuilder = new StringBuilder(escapeList.Count * 6 + 2);
		stringBuilder.Append('[');
		for (int i = 0; i < escapeList.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(escapeList[i]);
		}
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}
}
