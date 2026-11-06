using System;
using System.Collections.Generic;
using System.Linq;
using Example.Cfg;
using UnityEngine;
using XingYun.Aot;
using XingYun.Core.Singletons;
using XingYun.Experiment;
using XingYun.Vibrate;

namespace Vibration;

public static class VibrateUtility
{
	private const int ExtraMultiIntervalMs = 80;

	private const int ExtraMultiIosMinIntervalMs = 80;

	private const int ExtraMultiDroidMinIntervalMs = 10;

	private const int ExtraMultiMaxCount = 8;

	private const float MagnetContinuousDuration = 5f;

	private static int _magnetRestartTimerId = -1;

	public static int FuncIdButtonClickPlatform => 1000;

	public static void ButtonVibrate()
	{
		Vibrate(FuncIdButtonClickPlatform);
	}

	public static void Vibrate(int funcPointId)
	{
		if (GetVibrationState())
		{
			PlayConfiguredVibrate(funcPointId);
		}
	}

	private static void PlayConfiguredVibrate(int funcPointId)
	{
		ExampleTables table = GameApp.Config.GetTable<ExampleTables>();
		VibrateManager instance = MonoSingleton<VibrateManager>.Instance;
		VibrationConfig_Android orDefault = table.TbVibrationConfig_Android.GetOrDefault(funcPointId);
		if (orDefault == null)
		{
			return;
		}
		int sDKInt = SDKManager.GetSDKInt();
		if (instance.IsSupportPredefinedType() && sDKInt >= 31)
		{
			List<int> newAndroidTime = orDefault.NewAndroidTime;
			instance.SetPreDefinedVibrate(newAndroidTime);
			CheckDot();
			return;
		}
		CheckDot();
		string deviceModel = SystemInfo.deviceModel;
		string[] array = deviceModel.Split(' ');
		if (array.Length == 2)
		{
			_ = array[1];
		}
		List<uint> list = new List<uint>();
		List<uint> list2 = new List<uint>();
		if (sDKInt < 29)
		{
			list = orDefault.OldAndroidTime.Select((int x) => (uint)x).ToList();
			list2 = orDefault.OldAndroidStrong.Select((int x) => (uint)x).ToList();
		}
		else
		{
			VibrationLevel_Android orDefault2 = table.TbVibrationLevel_Android.GetOrDefault(deviceModel);
			if (orDefault2 != null)
			{
				switch (orDefault2.Level)
				{
				case 1:
					list = orDefault.AndroidTime1.Select((int x) => (uint)x).ToList();
					list2 = orDefault.AndroidStrong1.Select((int x) => (uint)x).ToList();
					break;
				case 2:
					list = orDefault.AndroidTime2.Select((int x) => (uint)x).ToList();
					list2 = orDefault.AndroidStrong2.Select((int x) => (uint)x).ToList();
					break;
				case 3:
					list = orDefault.AndroidTime3.Select((int x) => (uint)x).ToList();
					list2 = orDefault.AndroidStrong3.Select((int x) => (uint)x).ToList();
					break;
				}
			}
			else
			{
				list = orDefault.AndroidTime3.Select((int x) => (uint)x).ToList();
				list2 = orDefault.AndroidStrong3.Select((int x) => (uint)x).ToList();
			}
		}
		instance.VibrateRepeat(list, list2);
	}

	private static void CheckDot()
	{
	}

	public static bool IsExtraEnabled()
	{
		VibrateFunction vibrateFunction = FunctionDataCenter.Get<VibrateFunction>();
		if (vibrateFunction == null)
		{
			return false;
		}
		return vibrateFunction.EnableRewardVibratePoints == 1;
	}

	public static void VibrateExtra(int funcPointId)
	{
		if (IsExtraEnabled())
		{
			Vibrate(funcPointId);
		}
	}

	public static void VibrateExtraMulti(int count, int funcPointId, int intervalMs = 80, int initialDelayMs = 0, GameObject owner = null)
	{
		if (!IsExtraEnabled() || count <= 0)
		{
			return;
		}
		count = Mathf.Min(count, 8);
		int num = Mathf.Max(intervalMs, 10);
		float num2 = Mathf.Max(0f, (float)initialDelayMs / 1000f);
		float num3 = (float)num / 1000f;
		if (num2 <= 0f)
		{
			Vibrate(funcPointId);
			for (int i = 1; i < count; i++)
			{
				GameApp.Timer.Delay((Object)(object)owner, (float)i * num3, (Action)delegate
				{
					Vibrate(funcPointId);
				});
			}
			return;
		}
		for (int num4 = 0; num4 < count; num4++)
		{
			GameApp.Timer.Delay((Object)(object)owner, num2 + (float)num4 * num3, (Action)delegate
			{
				Vibrate(funcPointId);
			});
		}
	}

	public static bool GetVibrationState()
	{
		return MonoSingleton<VibrateManager>.Instance.EnableVibrateSequence;
	}

	public static void VibrateContinuous(int funcPointId)
	{
		if (!GetVibrationState())
		{
			return;
		}
		VibrationConfig_Android orDefault = GameApp.Config.GetTable<ExampleTables>().TbVibrationConfig_Android.GetOrDefault(funcPointId);
		if (orDefault != null)
		{
			List<uint> list = orDefault.OldAndroidTime.Select((int x) => (uint)x).ToList();
			List<uint> list2 = orDefault.OldAndroidStrong.Select((int x) => (uint)x).ToList();
			MonoSingleton<VibrateManager>.Instance.VibrateRepeat(list, list2);
		}
	}

	public static void StopContinuous()
	{
		MonoSingleton<VibrateManager>.Instance.VibrateCancel();
	}

	public static void VibrateRewardFlyIn(int itemCount, string source)
	{
		if (IsExtraEnabled() && itemCount > 0)
		{
			VibrateExtraMulti(itemCount, 1200);
		}
	}

	public static void VibrateRewardFlyInOnArrive(string source)
	{
		if (IsExtraEnabled())
		{
			Vibrate(1200);
		}
	}

	public static void VibrateDailyFirstWinLightUp(string source)
	{
		if (IsExtraEnabled())
		{
			Vibrate(1300);
		}
	}

	public static void VibrateRankLandDrop(string source)
	{
		if (IsExtraEnabled())
		{
			Vibrate(1400);
		}
		else
		{
			Vibrate(1502);
		}
	}
}
