using System;
using System.Globalization;
using System.Text;
using RotateRings.Ad.Cd;
using XingYun.Ad;
using XingYun.Aot;

namespace RotateRings.Ad;

public sealed class InstallDayInterstitialCooldownStrategy : IInterstitialCooldownStrategy
{
	private const string Tag = "[InstallDayInterstitialCd] ";

	private readonly float[] cooldownSecondsByInstallDay;

	private readonly float fallbackCooldownSeconds;

	public InterstitialCdStrategyKind Kind => InterstitialCdStrategyKind.InstallDay;

	public InstallDayInterstitialCooldownStrategy(string cooldownSchedule)
	{
		cooldownSecondsByInstallDay = ParseSchedule(cooldownSchedule);
		fallbackCooldownSeconds = 70f;
		_ = cooldownSecondsByInstallDay.LongLength;
	}

	public float GetCooldownSeconds(IAdConfiguration config)
	{
		if (cooldownSecondsByInstallDay.Length == 0)
		{
			return fallbackCooldownSeconds;
		}
		int installDays = GetInstallDays();
		int val = Math.Max(0, installDays - 1);
		val = Math.Min(val, cooldownSecondsByInstallDay.Length - 1);
		return cooldownSecondsByInstallDay[val];
	}

	public void CalculateCooldown()
	{
	}

	public string BuildCalculationLog()
	{
		StringBuilder stringBuilder = new StringBuilder(384);
		stringBuilder.AppendLine("--- 插屏 CD 运算 ---");
		stringBuilder.AppendLine("策略=安装天CD（InterstitialFunction.CdStrategy）");
		if (cooldownSecondsByInstallDay.Length == 0)
		{
			stringBuilder.Append("schedule=无效，回退 InsertCd=").Append(fallbackCooldownSeconds).Append('s');
			return stringBuilder.ToString();
		}
		int installDays = GetInstallDays();
		int val = Math.Max(0, installDays - 1);
		val = Math.Min(val, cooldownSecondsByInstallDay.Length - 1);
		float num = cooldownSecondsByInstallDay[val];
		stringBuilder.Append("schedule=").Append(FormatSchedule()).AppendLine();
		stringBuilder.Append("installDays=").Append(installDays).Append(" => scheduleIndex=")
			.Append(val)
			.Append("（末档表示第")
			.Append(cooldownSecondsByInstallDay.Length)
			.Append("天及以后）")
			.AppendLine();
		stringBuilder.Append("finalCd=").Append(num.ToString(CultureInfo.InvariantCulture)).Append('s');
		stringBuilder.AppendLine();
		stringBuilder.Append("门控=elapsed>=interval（完整间隔，无 GP39 70% 提前放行）");
		return stringBuilder.ToString();
	}

	private string FormatSchedule()
	{
		if (cooldownSecondsByInstallDay.Length == 0)
		{
			return "[]";
		}
		StringBuilder stringBuilder = new StringBuilder(cooldownSecondsByInstallDay.Length * 8);
		for (int i = 0; i < cooldownSecondsByInstallDay.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append('|');
			}
			stringBuilder.Append(cooldownSecondsByInstallDay[i].ToString(CultureInfo.InvariantCulture));
		}
		return stringBuilder.ToString();
	}

	private static float[] ParseSchedule(string cooldownSchedule)
	{
		if (string.IsNullOrWhiteSpace(cooldownSchedule))
		{
			return Array.Empty<float>();
		}
		string[] array = cooldownSchedule.Split('|', StringSplitOptions.RemoveEmptyEntries);
		float[] array2 = new float[array.Length];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (float.TryParse(array[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result) && result >= 0f)
			{
				array2[num++] = result;
			}
		}
		if (num == array.Length)
		{
			return array2;
		}
		float[] array3 = new float[num];
		Array.Copy(array2, array3, num);
		return array3;
	}

	private static int GetInstallDays()
	{
		try
		{
			return Math.Max(1, SDKManager.GetInstallDays());
		}
		catch
		{
			return 1;
		}
	}
}
