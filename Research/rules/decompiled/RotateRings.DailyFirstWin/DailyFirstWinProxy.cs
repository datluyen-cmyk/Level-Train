using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Services;
using UniLog;
using UnityEngine;
using XingYun.Proxy;

namespace RotateRings.DailyFirstWin;

public class DailyFirstWinProxy : BaseProxy
{
	private const string TAG = "[DailyFirstWinProxy] ";

	private const int MaxPeriod = 7;

	private const string PuzzleAudioVariantId = "variant_bd6ede35";

	private static readonly DateTime OriginalTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

	private const string RotationModeSequential = "sequential";

	private const string RotationModeFixed = "fixed";

	private const string RotationModeRandom = "random";

	public DailyFirstWinData Data { get; }

	public bool HasFirstWinThisSession { get; private set; }

	public DailyFirstWinProxy()
		: base("DailyFirstWinProxy")
	{
		Data = GameApp.Save.GetOrCreate<DailyFirstWinData>();
	}

	public bool IsEnabled()
	{
		DailyFirstWinConfigSnapshot? config = GetConfig();
		if (!config.HasValue)
		{
			return false;
		}
		if (config.Value.OpenLevel == 0)
		{
			return false;
		}
		return DailyFirstWinModule.Host.GetPlayerCompletedMainLevel() >= config.Value.OpenLevel;
	}

	public void TryPerformMidnightCheck()
	{
		int currentDayInCycle = Data.CurrentDayInCycle;
		int pendingRewardDay = Data.PendingRewardDay;
		bool deferPuzzleEntry = Data.DeferPuzzleEntry;
		DailyFirstWinConfigSnapshot? config = GetConfig();
		if (!config.HasValue || Data.CurrentDayInCycle <= 0)
		{
			return;
		}
		int hour = 0;
		int minute = 0;
		int second = 0;
		if (!string.IsNullOrWhiteSpace(config.Value.RefreshTime))
		{
			TryParseRefreshTime(config.Value.RefreshTime, out hour, out minute, out second);
		}
		IDailyFirstWinHost host = DailyFirstWinModule.Host;
		DateTime now = DateTime.Now;
		DateTime dateTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
		long num = ((now < dateTime) ? ToLocalTimestamp(dateTime.AddDays(-1.0)) : ToLocalTimestamp(dateTime));
		long currentTimestamp = host.GetCurrentTimestamp();
		if (Data.LastWinTimestamp >= num && Data.LastWinTimestamp <= currentTimestamp)
		{
			return;
		}
		long num2 = ToLocalTimestamp(dateTime.AddDays(-1.0));
		if (Data.LastWinTimestamp <= 0 || Data.LastWinTimestamp < num2 || Data.LastWinTimestamp >= num)
		{
			ResetForPendingCycle(clearPendingReward: true);
			NotifyStateChangedIfNeeded(currentDayInCycle, pendingRewardDay, deferPuzzleEntry);
			return;
		}
		int period = GetPeriod(config.Value);
		if (Data.CurrentDayInCycle >= period)
		{
			ResetForPendingCycle(clearPendingReward: false);
			NotifyStateChangedIfNeeded(currentDayInCycle, pendingRewardDay, deferPuzzleEntry);
		}
	}

	private void ResetForPendingCycle(bool clearPendingReward)
	{
		Data.CurrentDayInCycle = 0;
		if (clearPendingReward)
		{
			Data.PendingRewardDay = 0;
		}
		ClearDeferPuzzleEntry();
		SelectPuzzleForNewCycle();
		Data.HasPreselectedPuzzleForPendingCycle = true;
	}

	private static bool TryParseRefreshTime(string timeStr, out int hour, out int minute, out int second)
	{
		hour = 0;
		minute = 0;
		second = 0;
		if (string.IsNullOrWhiteSpace(timeStr))
		{
			return false;
		}
		string[] array = timeStr.Split('-');
		if (array.Length != 3)
		{
			return false;
		}
		if (!int.TryParse(array[0].Trim(), out hour))
		{
			return false;
		}
		if (!int.TryParse(array[1].Trim(), out minute))
		{
			return false;
		}
		if (!int.TryParse(array[2].Trim(), out second))
		{
			return false;
		}
		if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59)
		{
			return false;
		}
		return true;
	}

	public void PreparePendingCyclePuzzleOnStartup()
	{
		if (IsEnabled() && IsExperimentPuzzleEntryMode() && Data.CurrentDayInCycle <= 0 && !Data.HasPreselectedPuzzleForPendingCycle)
		{
			SelectPuzzleForNewCycle();
			Data.HasPreselectedPuzzleForPendingCycle = true;
		}
	}

	public bool RecordFirstWin()
	{
		if (!IsEnabled())
		{
			return false;
		}
		TryPerformMidnightCheck();
		IDailyFirstWinHost host = DailyFirstWinModule.Host;
		long currentTimestamp = host.GetCurrentTimestamp();
		if (Data.LastWinTimestamp > 0 && host.IsSameDay(Data.LastWinTimestamp, currentTimestamp))
		{
			return false;
		}
		DailyFirstWinConfigSnapshot? config = GetConfig();
		if (!config.HasValue)
		{
			return false;
		}
		bool flag = Data.LastWinTimestamp > 0 && host.GetDaysBetween(Data.LastWinTimestamp, currentTimestamp) == 1;
		int period = GetPeriod(config.Value);
		bool flag2 = Data.CurrentDayInCycle <= 0;
		bool flag3 = flag2 && Data.HasPreselectedPuzzleForPendingCycle;
		if (flag2)
		{
			Data.CurrentDayInCycle = 1;
		}
		else if (flag)
		{
			if (Data.CurrentDayInCycle >= period)
			{
				Data.CurrentDayInCycle = 1;
				flag2 = true;
			}
			else
			{
				Data.CurrentDayInCycle++;
			}
		}
		else
		{
			ClearDeferPuzzleEntry();
			Data.CurrentDayInCycle = 1;
			flag2 = true;
		}
		if (flag2)
		{
			ClearDeferPuzzleEntry();
			if (!flag3)
			{
				SelectPuzzleForNewCycle();
			}
			Data.HasPreselectedPuzzleForPendingCycle = false;
		}
		else if (UsePuzzleEntryPresentation())
		{
			EnsurePuzzleIdInitialized();
		}
		Data.LastWinTimestamp = currentTimestamp;
		CheckAndSetPendingReward(config.Value);
		MarkHalloweenPuzzleCompletedIfNeeded(period);
		HasFirstWinThisSession = true;
		NotifyStateChanged();
		return true;
	}

	public bool HasPendingReward()
	{
		return Data.PendingRewardDay > 0;
	}

	public DailyFirstWinRewardInfo GetPendingRewardInfo()
	{
		if (Data.PendingRewardDay <= 0)
		{
			return default(DailyFirstWinRewardInfo);
		}
		DailyFirstWinConfigSnapshot? config = GetConfig();
		if (!config.HasValue)
		{
			return default(DailyFirstWinRewardInfo);
		}
		return FindRewardByDay(config.Value, Data.PendingRewardDay);
	}

	public void MarkPendingRewardClaimed()
	{
		if (Data.PendingRewardDay != 0)
		{
			Data.PendingRewardDay = 0;
			NotifyStateChanged();
		}
	}

	public void ClearSessionFirstWinFlag()
	{
		HasFirstWinThisSession = false;
	}

	public int GetCurrentDayInCycle()
	{
		return Data.CurrentDayInCycle;
	}

	public bool UsePuzzleEntryPresentation()
	{
		if (IsExperimentPuzzleEntryMode())
		{
			return !Data.DeferPuzzleEntry;
		}
		return false;
	}

	public void TryApplyEntryPlayModeMigrationOnce()
	{
		if (!DailyFirstWinModule.IsInitialized || Data.EntryPlayModeMigrationApplied)
		{
			return;
		}
		bool deferPuzzleEntry = Data.DeferPuzzleEntry;
		if (!IsExperimentPuzzleEntryMode())
		{
			if (Data.DeferPuzzleEntry)
			{
				Data.DeferPuzzleEntry = false;
			}
			Data.EntryPlayModeMigrationApplied = true;
			NotifyStateChangedIfNeeded(Data.CurrentDayInCycle, Data.PendingRewardDay, deferPuzzleEntry);
		}
		else
		{
			if (Data.CurrentDayInCycle > 0 && Data.CurrentPuzzleId <= 0)
			{
				Data.DeferPuzzleEntry = true;
			}
			else
			{
				Data.DeferPuzzleEntry = false;
			}
			Data.EntryPlayModeMigrationApplied = true;
			NotifyStateChangedIfNeeded(Data.CurrentDayInCycle, Data.PendingRewardDay, deferPuzzleEntry);
		}
	}

	public bool IsHalloweenPuzzleTheme()
	{
		EnsureHalloweenPuzzleThemeAvailable();
		return Data.CurrentPuzzleTheme == DailyFirstWinPuzzleTheme.Halloween;
	}

	public int GetCurrentPuzzleId()
	{
		EnsureHalloweenPuzzleThemeAvailable();
		if (UsePuzzleEntryPresentation())
		{
			EnsurePuzzleIdInitialized();
		}
		return Data.CurrentPuzzleId;
	}

	public int GetUnlockedPieceCount()
	{
		return Data.CurrentDayInCycle;
	}

	public int GetPeriodDays()
	{
		DailyFirstWinConfigSnapshot? config = GetConfig();
		if (!config.HasValue)
		{
			return 7;
		}
		return GetPeriod(config.Value);
	}

	public bool HasFirstWinToday()
	{
		IDailyFirstWinHost host = DailyFirstWinModule.Host;
		long currentTimestamp = host.GetCurrentTimestamp();
		if (Data.LastWinTimestamp > 0)
		{
			return host.IsSameDay(Data.LastWinTimestamp, currentTimestamp);
		}
		return false;
	}

	public int[] GetMilestoneDays()
	{
		DailyFirstWinConfigSnapshot? config = GetConfig();
		if (!config.HasValue)
		{
			return Array.Empty<int>();
		}
		return config.Value.MilestoneDays ?? Array.Empty<int>();
	}

	public long GetRemainSecondsUntilNextRefresh()
	{
		if (!IsEnabled())
		{
			return 0L;
		}
		int hour = 0;
		int minute = 0;
		int second = 0;
		DailyFirstWinConfigSnapshot? config = GetConfig();
		if (config.HasValue && !string.IsNullOrWhiteSpace(config.Value.RefreshTime))
		{
			TryParseRefreshTime(config.Value.RefreshTime, out hour, out minute, out second);
		}
		DateTime now = DateTime.Now;
		DateTime dateTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
		return (long)(((now >= dateTime) ? dateTime.AddDays(1.0) : dateTime) - now).TotalSeconds;
	}

	public DailyFirstWinRewardInfo GetRewardInfoByMilestoneDay(int milestoneDay)
	{
		if (milestoneDay <= 0)
		{
			return default(DailyFirstWinRewardInfo);
		}
		DailyFirstWinConfigSnapshot? config = GetConfig();
		if (!config.HasValue)
		{
			return default(DailyFirstWinRewardInfo);
		}
		return FindRewardByDay(config.Value, milestoneDay);
	}

	public void SyncPuzzleAudioExperiment()
	{
		if (!DailyFirstWinModule.IsInitialized || DailyFirstWinModule.Host == null)
		{
			return;
		}
		AudioForgeRuntime instance = AudioForgeRuntime.Instance;
		if (!((Object)(object)instance == (Object)null) && instance.IsReady)
		{
			if (DailyFirstWinModule.Host.IsExperimentPuzzleEntryMode())
			{
				AudioService.ActivateExperiment("variant_bd6ede35");
			}
			else
			{
				AudioService.DeactivateExperiment("variant_bd6ede35");
			}
		}
	}

	private void MarkHalloweenPuzzleCompletedIfNeeded(int period)
	{
		if (!Data.HasCompletedHalloweenPuzzle && Data.CurrentPuzzleTheme == DailyFirstWinPuzzleTheme.Halloween && Data.CurrentDayInCycle >= period)
		{
			Data.HasCompletedHalloweenPuzzle = true;
		}
	}

	private void CheckAndSetPendingReward(DailyFirstWinConfigSnapshot config)
	{
		int[] milestoneDays = config.MilestoneDays;
		if (milestoneDays == null)
		{
			return;
		}
		for (int i = 0; i < milestoneDays.Length; i++)
		{
			if (Data.CurrentDayInCycle == milestoneDays[i])
			{
				Data.PendingRewardDay = milestoneDays[i];
				break;
			}
		}
	}

	private DailyFirstWinConfigSnapshot? GetConfig()
	{
		return DailyFirstWinModule.Host?.GetConfig();
	}

	private static int GetPeriod(DailyFirstWinConfigSnapshot config)
	{
		if (config.Period <= 0 || config.Period > 7)
		{
			return 7;
		}
		return config.Period;
	}

	private static DailyFirstWinRewardInfo FindRewardByDay(DailyFirstWinConfigSnapshot config, int day)
	{
		int[] milestoneDays = config.MilestoneDays;
		RewardEntrySnapshot[] rewards = config.Rewards;
		if (milestoneDays == null || rewards == null)
		{
			return default(DailyFirstWinRewardInfo);
		}
		for (int i = 0; i < milestoneDays.Length && i < rewards.Length; i++)
		{
			if (milestoneDays[i] == day)
			{
				return new DailyFirstWinRewardInfo
				{
					RewardConfigId = rewards[i].RewardConfigId,
					IsMultiplierMotivated = rewards[i].IsMultiplierMotivated
				};
			}
		}
		return default(DailyFirstWinRewardInfo);
	}

	private static long ToLocalTimestamp(DateTime time)
	{
		return (long)(time - OriginalTime).TotalSeconds;
	}

	private void NotifyStateChanged()
	{
		((BaseProxy)this).SendEvent<DailyFirstWinStateChangedEvent>(new DailyFirstWinStateChangedEvent());
	}

	private void NotifyStateChangedIfNeeded(int oldDay, int oldPending)
	{
		NotifyStateChangedIfNeeded(oldDay, oldPending, Data.DeferPuzzleEntry);
	}

	private void NotifyStateChangedIfNeeded(int oldDay, int oldPending, bool oldDefer)
	{
		if (oldDay != Data.CurrentDayInCycle || oldPending != Data.PendingRewardDay || oldDefer != Data.DeferPuzzleEntry)
		{
			NotifyStateChanged();
		}
	}

	private bool IsExperimentPuzzleEntryMode()
	{
		return DailyFirstWinModule.Host?.IsExperimentPuzzleEntryMode() ?? false;
	}

	private void ClearDeferPuzzleEntry()
	{
		if (Data.DeferPuzzleEntry)
		{
			Data.DeferPuzzleEntry = false;
		}
	}

	private void SelectPuzzleForNewCycle()
	{
		DailyFirstWinPuzzleTheme currentPuzzleTheme = Data.CurrentPuzzleTheme;
		Data.CurrentPuzzleTheme = DailyFirstWinPuzzleTheme.Normal;
		if (!IsExperimentPuzzleEntryMode())
		{
			return;
		}
		if (!Data.HasCompletedHalloweenPuzzle && IsHalloweenPeriodEnabled())
		{
			IReadOnlyList<int> readOnlyList = DailyFirstWinModule.Host?.GetHalloweenPuzzlePoolIds();
			DailyFirstWinPuzzleRotation rotation = DailyFirstWinModule.Host?.GetPuzzleRotation(DailyFirstWinPuzzleTheme.Halloween);
			if (readOnlyList != null && readOnlyList.Count > 0)
			{
				Data.CurrentPuzzleTheme = DailyFirstWinPuzzleTheme.Halloween;
				SyncPuzzleRotationState(DailyFirstWinModule.Host, DailyFirstWinPuzzleTheme.Halloween);
				Data.CurrentPuzzleId = GetNextHalloweenPuzzleId(rotation, readOnlyList);
				Data.LastHalloweenPuzzleId = Data.CurrentPuzzleId;
				return;
			}
		}
		if (currentPuzzleTheme == DailyFirstWinPuzzleTheme.Halloween)
		{
			Data.CurrentPuzzleId = 0;
		}
		AdvancePuzzleIdForNewCycle();
	}

	private static bool IsHalloweenPeriodEnabled()
	{
		IDailyFirstWinHost host = DailyFirstWinModule.Host;
		if (host == null || !host.IsHalloweenFeatureEnabled())
		{
			return false;
		}
		if (host.IsHalloweenPuzzleUIAvailable())
		{
			return true;
		}
		Log.LogError("[DailyFirstWinProxy] ", "万圣节拼图 UI 预制体不可用，回退普通拼图");
		return false;
	}

	private void EnsureHalloweenPuzzleThemeAvailable()
	{
		if (Data.CurrentPuzzleTheme == DailyFirstWinPuzzleTheme.Halloween)
		{
			IDailyFirstWinHost host = DailyFirstWinModule.Host;
			if (host == null || !host.IsHalloweenPuzzleUIAvailable())
			{
				Data.CurrentPuzzleTheme = DailyFirstWinPuzzleTheme.Normal;
				Data.CurrentPuzzleId = 0;
				Log.LogError("[DailyFirstWinProxy] ", "万圣节拼图 UI 预制体不可用，已将当前周期降级为普通拼图");
			}
		}
	}

	private int GetNextHalloweenPuzzleId(DailyFirstWinPuzzleRotation rotation, IReadOnlyList<int> pool)
	{
		return AdvancePuzzleId(Data, rotation, pool, Data.LastHalloweenPuzzleId);
	}

	private void EnsurePuzzleIdInitialized()
	{
		IDailyFirstWinHost host = DailyFirstWinModule.Host;
		if (host == null)
		{
			return;
		}
		if (Data.CurrentPuzzleTheme == DailyFirstWinPuzzleTheme.Halloween)
		{
			EnsureHalloweenPuzzleIdInitialized(host);
			return;
		}
		SyncPuzzleRotationState(host, DailyFirstWinPuzzleTheme.Normal);
		EnsureCurrentPuzzleIdInRotationPool(host, DailyFirstWinPuzzleTheme.Normal);
		if (Data.CurrentPuzzleId <= 0)
		{
			DailyFirstWinPuzzleRotation puzzleRotation = host.GetPuzzleRotation(DailyFirstWinPuzzleTheme.Normal);
			IReadOnlyList<int> effectivePuzzlePoolIds = host.GetEffectivePuzzlePoolIds();
			int num = ResolveFirstPuzzleId(Data, puzzleRotation, effectivePuzzlePoolIds);
			if (num <= 0 && effectivePuzzlePoolIds != null && effectivePuzzlePoolIds.Count > 0)
			{
				num = effectivePuzzlePoolIds[0];
			}
			Data.CurrentPuzzleId = num;
		}
	}

	private void EnsureHalloweenPuzzleIdInitialized(IDailyFirstWinHost host)
	{
		IReadOnlyList<int> halloweenPuzzlePoolIds = host.GetHalloweenPuzzlePoolIds();
		if (halloweenPuzzlePoolIds != null && halloweenPuzzlePoolIds.Count != 0 && !ContainsPuzzlePoolId(halloweenPuzzlePoolIds, Data.CurrentPuzzleId))
		{
			DailyFirstWinPuzzleRotation puzzleRotation = host.GetPuzzleRotation(DailyFirstWinPuzzleTheme.Halloween);
			SyncPuzzleRotationState(host, DailyFirstWinPuzzleTheme.Halloween);
			Data.CurrentPuzzleId = GetNextHalloweenPuzzleId(puzzleRotation, halloweenPuzzlePoolIds);
			Data.LastHalloweenPuzzleId = Data.CurrentPuzzleId;
		}
	}

	private void AdvancePuzzleIdForNewCycle()
	{
		IDailyFirstWinHost host = DailyFirstWinModule.Host;
		if (host == null)
		{
			return;
		}
		SyncPuzzleRotationState(host, DailyFirstWinPuzzleTheme.Normal);
		if (Data.CurrentPuzzleId <= 0)
		{
			EnsurePuzzleIdInitialized();
			return;
		}
		DailyFirstWinPuzzleRotation puzzleRotation = host.GetPuzzleRotation(DailyFirstWinPuzzleTheme.Normal);
		IReadOnlyList<int> effectivePuzzlePoolIds = host.GetEffectivePuzzlePoolIds();
		int num = AdvancePuzzleId(Data, puzzleRotation, effectivePuzzlePoolIds, Data.CurrentPuzzleId);
		if (num > 0)
		{
			Data.CurrentPuzzleId = num;
		}
	}

	private void SyncPuzzleRotationState(IDailyFirstWinHost host, DailyFirstWinPuzzleTheme theme)
	{
		string text = BuildPuzzleRotationStateKey(host.GetPuzzleRotation(theme), theme);
		if (!string.Equals(Data.PuzzleRotationStateKey, text, StringComparison.Ordinal))
		{
			Data.PuzzleRotationStateKey = text;
			ClearPuzzleRandomBag(Data);
			EnsureCurrentPuzzleIdInRotationPool(host, theme);
		}
	}

	private void EnsureCurrentPuzzleIdInRotationPool(IDailyFirstWinHost host, DailyFirstWinPuzzleTheme theme)
	{
		if (host != null && Data.CurrentPuzzleId > 0)
		{
			IReadOnlyList<int> readOnlyList = ((theme == DailyFirstWinPuzzleTheme.Halloween) ? host.GetHalloweenPuzzlePoolIds() : host.GetEffectivePuzzlePoolIds());
			if (readOnlyList != null && readOnlyList.Count != 0 && !ContainsPuzzlePoolId(readOnlyList, Data.CurrentPuzzleId))
			{
				int currentPuzzleId = readOnlyList[0];
				Data.CurrentPuzzleId = currentPuzzleId;
				NotifyStateChanged();
			}
		}
	}

	private static int ResolveFirstPuzzleId(DailyFirstWinData data, DailyFirstWinPuzzleRotation rotation, IReadOnlyList<int> effectivePool)
	{
		if (effectivePool == null || effectivePool.Count == 0)
		{
			return 0;
		}
		string text = NormalizeRotationMode(rotation?.Mode);
		if (text == "fixed")
		{
			int num = rotation?.FixedPuzzleId ?? 0;
			if (num > 0 && ContainsPuzzlePoolId(effectivePool, num))
			{
				return num;
			}
			return effectivePool[0];
		}
		if (text == "random")
		{
			EnsurePuzzleRandomBag(data, effectivePool);
			if (data.PuzzleRandomBag.Count <= 0)
			{
				return effectivePool[0];
			}
			return data.PuzzleRandomBag[0];
		}
		return effectivePool[0];
	}

	private static int AdvancePuzzleId(DailyFirstWinData data, DailyFirstWinPuzzleRotation rotation, IReadOnlyList<int> effectivePool, int currentId)
	{
		if (effectivePool == null || effectivePool.Count == 0)
		{
			return 0;
		}
		string text = NormalizeRotationMode(rotation?.Mode);
		if (text == "fixed")
		{
			if (currentId > 0)
			{
				return currentId;
			}
			return ResolveFirstPuzzleId(data, rotation, effectivePool);
		}
		if (text == "random")
		{
			EnsurePuzzleRandomBag(data, effectivePool);
			RemoveFromPuzzleRandomBag(data, currentId);
			if (data.PuzzleRandomBag == null || data.PuzzleRandomBag.Count == 0)
			{
				RefillPuzzleRandomBag(data, effectivePool);
			}
			if (data.PuzzleRandomBag.Count <= 0)
			{
				return effectivePool[0];
			}
			return data.PuzzleRandomBag[0];
		}
		if (currentId <= 0)
		{
			return effectivePool[0];
		}
		int num = IndexOfPuzzlePoolId(effectivePool, currentId);
		if (num < 0)
		{
			return effectivePool[0];
		}
		int index = (num + 1) % effectivePool.Count;
		return effectivePool[index];
	}

	private static void ClearPuzzleRandomBag(DailyFirstWinData data)
	{
		if (data.PuzzleRandomBag == null)
		{
			data.PuzzleRandomBag = new List<int>();
		}
		else
		{
			data.PuzzleRandomBag.Clear();
		}
	}

	private static string BuildPuzzleRotationStateKey(DailyFirstWinPuzzleRotation rotation, DailyFirstWinPuzzleTheme theme)
	{
		IReadOnlyList<int> readOnlyList = rotation?.PoolIds;
		string text = ((readOnlyList == null || readOnlyList.Count == 0) ? "*" : string.Join(",", readOnlyList));
		int num = rotation?.FixedPuzzleId ?? 0;
		int num2 = rotation?.PuzzlesPerCycle ?? 0;
		return $"{theme}|{NormalizeRotationMode(rotation?.Mode)}|{num}|{text}|{num2}";
	}

	private static void EnsurePuzzleRandomBag(DailyFirstWinData data, IReadOnlyList<int> effectivePool)
	{
		if (data.PuzzleRandomBag == null || data.PuzzleRandomBag.Count <= 0)
		{
			RefillPuzzleRandomBag(data, effectivePool);
		}
	}

	private static void RefillPuzzleRandomBag(DailyFirstWinData data, IReadOnlyList<int> effectivePool)
	{
		if (data.PuzzleRandomBag == null)
		{
			data.PuzzleRandomBag = new List<int>(effectivePool.Count);
		}
		else
		{
			data.PuzzleRandomBag.Clear();
		}
		for (int i = 0; i < effectivePool.Count; i++)
		{
			data.PuzzleRandomBag.Add(effectivePool[i]);
		}
		ShufflePuzzleIdsInPlace(data.PuzzleRandomBag);
	}

	private static void RemoveFromPuzzleRandomBag(DailyFirstWinData data, int id)
	{
		if (data.PuzzleRandomBag == null || id <= 0)
		{
			return;
		}
		for (int num = data.PuzzleRandomBag.Count - 1; num >= 0; num--)
		{
			if (data.PuzzleRandomBag[num] == id)
			{
				data.PuzzleRandomBag.RemoveAt(num);
			}
		}
	}

	private static void ShufflePuzzleIdsInPlace(IList<int> list)
	{
		Random random = new Random();
		for (int num = list.Count - 1; num > 0; num--)
		{
			int num2 = random.Next(num + 1);
			int index = num;
			int index2 = num2;
			int num3 = list[num2];
			int num4 = list[num];
			int num5 = (list[index] = num3);
			num5 = (list[index2] = num4);
		}
	}

	private static string NormalizeRotationMode(string mode)
	{
		if (string.IsNullOrWhiteSpace(mode))
		{
			return "sequential";
		}
		return mode.Trim().ToLowerInvariant();
	}

	private static bool ContainsPuzzlePoolId(IReadOnlyList<int> pool, int id)
	{
		return IndexOfPuzzlePoolId(pool, id) >= 0;
	}

	private static int IndexOfPuzzlePoolId(IReadOnlyList<int> pool, int id)
	{
		for (int i = 0; i < pool.Count; i++)
		{
			if (pool[i] == id)
			{
				return i;
			}
		}
		return -1;
	}
}
