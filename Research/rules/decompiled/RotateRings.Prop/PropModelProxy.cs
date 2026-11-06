using System;
using System.Collections.Generic;
using Example.Cfg;
using Game.DataTrack;
using Hot.Model;
using RotateRings.Model;
using UniLog;
using UnityEngine;
using XingYun.Experiment;
using XingYun.Proxy;

namespace RotateRings.Prop;

public class PropModelProxy : BaseProxy
{
	private readonly Dictionary<EPropType, int> lastPropCounts = new Dictionary<EPropType, int>();

	private readonly Dictionary<EPropType, int> pendingFreeProps;

	private readonly Dictionary<EPropType, TrackPropUseReason> pendingFreePropReasons;

	public const int InGamePropIdMin = 200;

	public const int InGamePropIdMaxExclusive = 300;

	private const int MaxPendingUnlockIntroCount = 8;

	public PropModelData Data { get; }

	public PropTracker Tracker { get; }

	public bool NoAd => Data.PropDict.GetValueOrDefault(EPropType.RemoveAd, 0) > 0;

	private static TbPropConfig TbPropConfig => GameApp.Config.GetTable<ExampleTables>().TbPropConfig;

	public int PendingUnlockIntroCount => Data.PendingUnlockIntroPropList?.Count ?? 0;

	public PropModelProxy()
		: base("PropModelProxy")
	{
		Data = GameApp.Save.GetOrCreate<PropModelData>();
		PropModelData data = Data;
		pendingFreeProps = data.PendingFreePropDict ?? (data.PendingFreePropDict = new Dictionary<EPropType, int>());
		data = Data;
		pendingFreePropReasons = data.PendingFreePropReasonDict ?? (data.PendingFreePropReasonDict = new Dictionary<EPropType, TrackPropUseReason>());
		Tracker = new PropTracker(this);
	}

	public bool IsInGameProp(int propId)
	{
		if (propId >= 200)
		{
			return propId < 300;
		}
		return false;
	}

	public List<PropConfig> GetInGamePropConfigs()
	{
		List<PropConfig> list = new List<PropConfig>();
		TbPropConfig tbPropConfig = TbPropConfig;
		if (tbPropConfig?.DataList == null)
		{
			return list;
		}
		List<PropConfig> dataList = tbPropConfig.DataList;
		for (int i = 0; i < dataList.Count; i++)
		{
			PropConfig propConfig = dataList[i];
			if (IsInGameProp(propConfig.Id))
			{
				list.Add(propConfig);
			}
		}
		list.Sort((PropConfig a, PropConfig b) => a.Id.CompareTo(b.Id));
		return list;
	}

	public int GetNewUserFreeSendCount(EPropType propType)
	{
		return ParseNewUserFreeSend().GetValueOrDefault((int)propType, 0);
	}

	private static Dictionary<int, int> ParseNewUserFreeSend()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		string text = FunctionDataCenter.Get<PropFunction>()?.NewUserFreeSend;
		if (string.IsNullOrWhiteSpace(text))
		{
			return dictionary;
		}
		string[] array = text.Split('|', StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Trim().Split('-');
			if (array2.Length == 2 && int.TryParse(array2[0].Trim(), out var result) && int.TryParse(array2[1].Trim(), out var result2))
			{
				dictionary[result] = dictionary.GetValueOrDefault(result, 0) + result2;
			}
		}
		return dictionary;
	}

	public PropConfig GetPropConfig(EPropType propType)
	{
		return GetPropConfig((int)propType);
	}

	public PropConfig GetPropConfig(int propId)
	{
		return TbPropConfig?.GetOrDefault(propId);
	}

	public string GetPropName(EPropType propType)
	{
		PropConfig propConfig = GetPropConfig(propType);
		if (propConfig != null && !string.IsNullOrEmpty(propConfig.propName))
		{
			return propConfig.propName;
		}
		return propType.ToString();
	}

	public string GetPropGuideKey(EPropType propType)
	{
		PropConfig propConfig = GetPropConfig(propType);
		if (propConfig != null && !string.IsNullOrEmpty(propConfig.GuideKey))
		{
			return propConfig.GuideKey;
		}
		return propType.ToString();
	}

	public string GetPropDescByKey(EPropType propType, int index)
	{
		PropConfig propConfig = GetPropConfig(propType);
		if (propConfig?.GuideDescKeys == null || index < 0 || index >= propConfig.GuideDescKeys.Count)
		{
			return string.Empty;
		}
		return propConfig.GuideDescKeys[index] ?? string.Empty;
	}

	public int GetPropCount(EPropType propType)
	{
		return Data.PropDict.GetValueOrDefault(propType, 0);
	}

	public int GetLastPropCount(EPropType propType)
	{
		return lastPropCounts.GetValueOrDefault(propType, GetPropCount(propType));
	}

	public Dictionary<EPropType, int> GetAllProps()
	{
		return Data.PropDict;
	}

	public bool IsPropUnlock(int propID)
	{
		if (!IsInGameProp(propID))
		{
			return false;
		}
		PropModelData data = Data;
		if (data.UnlockPropList == null)
		{
			data.UnlockPropList = new List<int>();
		}
		return Data.UnlockPropList.Contains(propID);
	}

	public bool IsPropUnlock(EPropType propType)
	{
		return IsPropUnlock((int)propType);
	}

	public void UnlockProp(int propID)
	{
		if (IsInGameProp(propID) && !IsPropUnlock(propID))
		{
			Data.UnlockPropList.Add(propID);
		}
	}

	public void UnlockProp(EPropType propType)
	{
		UnlockProp((int)propType);
	}

	public int GetPropUnlockLv(PropConfig propConfig)
	{
		return propConfig?.UnlockLv ?? (-1);
	}

	public int GetPropUnlockLv(EPropType propType)
	{
		return GetPropUnlockLv(GetPropConfig(propType));
	}

	public bool IsPropUnlockLevel(EPropType propType, int level)
	{
		PropConfig propConfig = GetPropConfig(propType);
		if (propConfig != null)
		{
			return GetPropUnlockLv(propConfig) == level;
		}
		return false;
	}

	public bool HasPendingFreeProp(EPropType propType)
	{
		if (pendingFreeProps.TryGetValue(propType, out var value))
		{
			return value > 0;
		}
		return false;
	}

	public void EnsurePendingFreeProp(EPropType propType, TrackPropUseReason reason = TrackPropUseReason.Free)
	{
		pendingFreeProps[propType] = 1;
		pendingFreePropReasons[propType] = reason;
	}

	public TrackPropUseReason GetPendingFreePropUseReason(EPropType propType)
	{
		if (!pendingFreePropReasons.TryGetValue(propType, out var value))
		{
			return TrackPropUseReason.Free;
		}
		return value;
	}

	public bool TryConsumePendingFreeProp(EPropType propType)
	{
		if (!pendingFreeProps.TryGetValue(propType, out var value) || value <= 0)
		{
			return false;
		}
		if (value == 1)
		{
			pendingFreeProps.Remove(propType);
			pendingFreePropReasons.Remove(propType);
		}
		else
		{
			pendingFreeProps[propType] = value - 1;
		}
		return true;
	}

	public void ClearPendingFreePropsNotForLevel(int level)
	{
		if (pendingFreeProps.Count == 0)
		{
			return;
		}
		List<EPropType> list = new List<EPropType>(pendingFreeProps.Count);
		foreach (EPropType key2 in pendingFreeProps.Keys)
		{
			if (!IsPropUnlockLevel(key2, level))
			{
				list.Add(key2);
			}
		}
		if (list.Count != 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				EPropType key = list[i];
				pendingFreeProps.Remove(key);
				pendingFreePropReasons.Remove(key);
			}
		}
	}

	public bool IsPropFirstUse(int propID)
	{
		if (!IsInGameProp(propID))
		{
			return false;
		}
		PropModelData data = Data;
		if (data.UsedPropList == null)
		{
			data.UsedPropList = new List<int>();
		}
		return !Data.UsedPropList.Contains(propID);
	}

	public bool IsPropFirstUse(EPropType propType)
	{
		return IsPropFirstUse((int)propType);
	}

	public bool MarkPropUsed(int propID)
	{
		if (!IsInGameProp(propID))
		{
			return false;
		}
		PropModelData data = Data;
		if (data.UsedPropList == null)
		{
			data.UsedPropList = new List<int>();
		}
		if (Data.UsedPropList.Contains(propID))
		{
			return false;
		}
		Data.UsedPropList.Add(propID);
		return true;
	}

	public bool MarkPropUsed(EPropType propType)
	{
		return MarkPropUsed((int)propType);
	}

	public bool IsPropGuided(int propID)
	{
		if (!IsInGameProp(propID))
		{
			return false;
		}
		PropModelData data = Data;
		if (data.GuidedPropList == null)
		{
			data.GuidedPropList = new List<int>();
		}
		return Data.GuidedPropList.Contains(propID);
	}

	public bool IsPropGuided(EPropType propType)
	{
		return IsPropGuided((int)propType);
	}

	public bool MarkPropGuided(int propID)
	{
		if (!IsInGameProp(propID))
		{
			return false;
		}
		PropModelData data = Data;
		if (data.GuidedPropList == null)
		{
			data.GuidedPropList = new List<int>();
		}
		if (Data.GuidedPropList.Contains(propID))
		{
			return false;
		}
		Data.GuidedPropList.Add(propID);
		return true;
	}

	public bool MarkPropGuided(EPropType propType)
	{
		return MarkPropGuided((int)propType);
	}

	public bool IsGuideUserInteracted(int propID)
	{
		if (!IsInGameProp(propID))
		{
			return false;
		}
		PropModelData data = Data;
		if (data.GuideUserInteractedPropList == null)
		{
			data.GuideUserInteractedPropList = new List<int>();
		}
		return Data.GuideUserInteractedPropList.Contains(propID);
	}

	public bool IsGuideUserInteracted(EPropType propType)
	{
		return IsGuideUserInteracted((int)propType);
	}

	public bool MarkGuideUserInteracted(int propID)
	{
		if (!IsInGameProp(propID))
		{
			return false;
		}
		PropModelData data = Data;
		if (data.GuideUserInteractedPropList == null)
		{
			data.GuideUserInteractedPropList = new List<int>();
		}
		if (Data.GuideUserInteractedPropList.Contains(propID))
		{
			return false;
		}
		Data.GuideUserInteractedPropList.Add(propID);
		return true;
	}

	public bool ClearGuideUserInteracted(int propID)
	{
		if (!IsInGameProp(propID))
		{
			return false;
		}
		if (Data.GuideUserInteractedPropList == null || !Data.GuideUserInteractedPropList.Remove(propID))
		{
			return false;
		}
		return true;
	}

	public bool MarkGuideUserInteracted(EPropType propType)
	{
		return MarkGuideUserInteracted((int)propType);
	}

	public bool ClearGuideUserInteracted(EPropType propType)
	{
		return ClearGuideUserInteracted((int)propType);
	}

	public void InitForEntry()
	{
		TryInitNewbieProp();
		try
		{
			if (EnsureUnlockStateFromHistory())
			{
				GameApp.Save.SaveAll();
			}
		}
		catch (Exception arg)
		{
			Log.LogError("PropModelProxy", $"[InitForEntry] 跨版本回补异常，已跳过: {arg}");
		}
	}

	public bool EnsureUnlockStateFromHistory()
	{
		if (Data.IsPropCompatMigrated)
		{
			return false;
		}
		TbPropConfig tbPropConfig = TbPropConfig;
		if (tbPropConfig?.DataList == null)
		{
			return false;
		}
		PropModelData data = Data;
		if (data.GuidedPropList == null)
		{
			data.GuidedPropList = new List<int>();
		}
		data = Data;
		if (data.UsedPropList == null)
		{
			data.UsedPropList = new List<int>();
		}
		int num = 0;
		List<PropConfig> dataList = tbPropConfig.DataList;
		for (int i = 0; i < dataList.Count; i++)
		{
			PropConfig propConfig = dataList[i];
			if (propConfig != null && IsInGameProp(propConfig.Id) && !IsPropUnlock(propConfig.Id) && (Data.GuidedPropList.Contains(propConfig.Id) || Data.UsedPropList.Contains(propConfig.Id)))
			{
				UnlockProp(propConfig.Id);
				num++;
			}
		}
		Data.IsPropCompatMigrated = true;
		return true;
	}

	public bool EnqueuePendingUnlockIntro(EPropType propType)
	{
		if (!IsInGameProp((int)propType))
		{
			return false;
		}
		PropModelData data = Data;
		if (data.PendingUnlockIntroPropList == null)
		{
			data.PendingUnlockIntroPropList = new List<int>();
		}
		if (Data.PendingUnlockIntroPropList.Contains((int)propType) || IsPropGuided((int)propType))
		{
			return false;
		}
		if (Data.PendingUnlockIntroPropList.Count >= 8)
		{
			return false;
		}
		Data.PendingUnlockIntroPropList.Add((int)propType);
		return true;
	}

	public bool DequeuePendingUnlockIntro(EPropType propType)
	{
		if (Data.PendingUnlockIntroPropList == null || Data.PendingUnlockIntroPropList.Count == 0)
		{
			return false;
		}
		return Data.PendingUnlockIntroPropList.Remove((int)propType);
	}

	public EPropType PeekPendingUnlockIntroByPriority()
	{
		if (Data.PendingUnlockIntroPropList == null || Data.PendingUnlockIntroPropList.Count == 0)
		{
			return EPropType.None;
		}
		TbPropConfig tbPropConfig = TbPropConfig;
		if (tbPropConfig?.DataList == null)
		{
			return EPropType.None;
		}
		EPropType ePropType = EPropType.None;
		int num = int.MaxValue;
		List<PropConfig> dataList = tbPropConfig.DataList;
		for (int i = 0; i < dataList.Count; i++)
		{
			PropConfig propConfig = dataList[i];
			if (propConfig != null && IsInGameProp(propConfig.Id) && Data.PendingUnlockIntroPropList.Contains(propConfig.Id))
			{
				int propUnlockLv = GetPropUnlockLv(propConfig);
				if (ePropType == EPropType.None || propUnlockLv < num)
				{
					ePropType = (EPropType)propConfig.Id;
					num = propUnlockLv;
				}
			}
		}
		return ePropType;
	}

	public void SanitizePendingUnlockIntro()
	{
		if (Data.PendingUnlockIntroPropList == null || Data.PendingUnlockIntroPropList.Count == 0)
		{
			return;
		}
		for (int num = Data.PendingUnlockIntroPropList.Count - 1; num >= 0; num--)
		{
			int num2 = Data.PendingUnlockIntroPropList[num];
			int num3;
			int num4;
			if (IsInGameProp(num2))
			{
				num3 = ((GetPropConfig(num2) == null) ? 1 : 0);
				if (num3 == 0)
				{
					num4 = (IsPropGuided(num2) ? 1 : 0);
					goto IL_006a;
				}
			}
			else
			{
				num3 = 1;
			}
			num4 = 0;
			goto IL_006a;
			IL_006a:
			bool flag = (byte)num4 != 0;
			if (num3 != 0 || flag)
			{
				Data.PendingUnlockIntroPropList.RemoveAt(num);
			}
		}
	}

	public List<PropConfig> CollectUnlockableProps(int level)
	{
		List<PropConfig> list = new List<PropConfig>();
		TbPropConfig tbPropConfig = TbPropConfig;
		if (tbPropConfig?.DataList == null)
		{
			return list;
		}
		List<PropConfig> dataList = tbPropConfig.DataList;
		for (int i = 0; i < dataList.Count; i++)
		{
			PropConfig propConfig = dataList[i];
			if (IsInGameProp(propConfig.Id) && !IsPropUnlock(propConfig.Id) && IsPropUnlockableAtLevel(propConfig, GameApp.Proxy.Get<LevelModelProxy>().MainLevel))
			{
				list.Add(propConfig);
			}
		}
		return list;
	}

	private bool IsPropUnlockableAtLevel(PropConfig propConfig, int level)
	{
		int propUnlockLv = GetPropUnlockLv(propConfig);
		if (propUnlockLv >= 0)
		{
			return propUnlockLv <= level;
		}
		return false;
	}

	public List<EPropType> CollectReplayGuideProps(int level)
	{
		List<EPropType> list = new List<EPropType>();
		if (GameInfo.CurrentGameMode != GameMode.Main)
		{
			return list;
		}
		TbPropConfig tbPropConfig = TbPropConfig;
		if (tbPropConfig?.DataList == null)
		{
			return list;
		}
		List<PropConfig> dataList = tbPropConfig.DataList;
		for (int i = 0; i < dataList.Count; i++)
		{
			PropConfig propConfig = dataList[i];
			if (IsInGameProp(propConfig.Id) && GetPropUnlockLv(propConfig) == level && IsPropUnlock(propConfig.Id))
			{
				list.Add((EPropType)propConfig.Id);
			}
		}
		return list;
	}

	public void TryInitNewbieProp()
	{
		if (!Data.IsNewbieProp)
		{
			InitNewUserProp();
			InitNewUserUnlockProp();
			Tracker.ReportNewBiePropAdd();
			Data.IsNewbieProp = true;
		}
	}

	public void AddProp(EPropType propType, int addCount, TrackPropGainReason reason, string customReasonId, string customReasonInfo, PropAnimParam? animParam = null)
	{
		int propCount = GetPropCount(propType);
		AddProp(propType, addCount);
		int propCount2 = GetPropCount(propType);
		Tracker.TrackGain(propType, addCount, propCount, reason, propCount2, customReasonId, customReasonInfo);
		RRDataTrack.Instance.ReportGetItem(propType.ToString(), addCount, propCount, propCount2, customReasonId, customReasonInfo);
		GameApp.Proxy.Get<LevelModelProxy>()?.AddGameModelPropGainData(propType, reason, addCount);
		PlayPropNumberAnim(propType, animParam);
	}

	public void UseProp(EPropType propType, int costCount, TrackPropUseReason reason, PropAnimParam? animParam = null)
	{
		int propCount = GetPropCount(propType);
		UseProp(propType, costCount);
		int propCount2 = GetPropCount(propType);
		RRDataTrack.Instance.ReportUseItem(propType.ToString(), costCount, propCount, propCount2);
		GameApp.Proxy.Get<LevelModelProxy>()?.AddGameModelPropUseData(propType, reason, costCount);
		PlayPropNumberAnim(propType, animParam);
	}

	public void UseFreeProp(EPropType propType, TrackPropUseReason reason, PropAnimParam? animParam = null)
	{
		int propCount = GetPropCount(propType);
		MarkPropUsed(propType);
		RRDataTrack.Instance.ReportUseItem(propType.ToString(), 1, propCount, propCount);
		GameApp.Proxy.Get<LevelModelProxy>()?.AddGameModelPropUseData(propType, reason, 1);
		GameApp.Event.Send<PropChangeEvent>(new PropChangeEvent
		{
			PropType = propType,
			DeltaCount = 0
		});
		PlayPropNumberAnim(propType, animParam);
	}

	public EPropType GetRewardPropType(int rewardId)
	{
		if (Enum.IsDefined(typeof(EPropType), (EPropType)rewardId))
		{
			return (EPropType)rewardId;
		}
		return EPropType.None;
	}

	private void AddProp(EPropType propType, int addCount)
	{
		lastPropCounts[propType] = GetPropCount(propType);
		if (Data.PropDict.TryGetValue(propType, out var value))
		{
			Data.PropDict[propType] = value + addCount;
		}
		else
		{
			Data.PropDict.Add(propType, addCount);
		}
		Data.PropDict[propType] = Mathf.Clamp(Data.PropDict[propType], 0, int.MaxValue);
		GameApp.Event.Send<PropChangeEvent>(new PropChangeEvent
		{
			PropType = propType,
			DeltaCount = addCount
		});
	}

	private void UseProp(EPropType propType, int costCount)
	{
		if (costCount > 0)
		{
			MarkPropUsed(propType);
		}
		AddProp(propType, -costCount);
	}

	private void PlayPropNumberAnim(EPropType type, PropAnimParam? animParam)
	{
		if (!animParam.HasValue)
		{
			return;
		}
		PropAnimParam valueOrDefault = animParam.GetValueOrDefault();
		PropNumberAniEvent evt = new PropNumberAniEvent
		{
			PropType = type,
			lastNumber = GetLastPropCount(type),
			currentNumber = GetPropCount(type),
			duration = valueOrDefault.Duration
		};
		if (valueOrDefault.Delay > 0f)
		{
			GameApp.Timer.Delay(valueOrDefault.Delay, (Action)delegate
			{
				GameApp.Event.Send<PropNumberAniEvent>(evt);
			});
		}
		else
		{
			GameApp.Event.Send<PropNumberAniEvent>(evt);
		}
	}

	private void InitNewUserProp()
	{
		if (Data.IsNewbieProp)
		{
			return;
		}
		TbPropConfig tbPropConfig = TbPropConfig;
		if (tbPropConfig?.DataList == null)
		{
			return;
		}
		int mainLevel = GameApp.Proxy.Get<LevelModelProxy>().MainLevel;
		List<PropConfig> dataList = tbPropConfig.DataList;
		for (int i = 0; i < dataList.Count; i++)
		{
			PropConfig propConfig = dataList[i];
			if (!IsPropUnlockableAtLevel(propConfig, mainLevel))
			{
				continue;
			}
			EPropType rewardPropType = GetRewardPropType(propConfig.Id);
			if (rewardPropType != EPropType.None)
			{
				int newUserFreeSendCount = GetNewUserFreeSendCount(rewardPropType);
				if (newUserFreeSendCount > 0)
				{
					AddProp(rewardPropType, newUserFreeSendCount);
				}
			}
		}
	}

	private void InitNewUserUnlockProp()
	{
		if (Data.IsNewbieProp)
		{
			return;
		}
		TbPropConfig tbPropConfig = TbPropConfig;
		if (tbPropConfig == null || tbPropConfig.DataList == null)
		{
			return;
		}
		int mainLevel = GameApp.Proxy.Get<LevelModelProxy>().MainLevel;
		List<PropConfig> dataList = tbPropConfig.DataList;
		for (int i = 0; i < dataList.Count; i++)
		{
			PropConfig propConfig = dataList[i];
			if (IsInGameProp(propConfig.Id) && IsPropUnlockableAtLevel(propConfig, mainLevel))
			{
				UnlockProp(propConfig.Id);
			}
		}
	}
}
