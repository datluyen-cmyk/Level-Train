using System;
using RotateRings.Ad.Strategy.Interstitial;
using UnityEngine;
using XingYun.Ad;

namespace RotateRings.Ad.Cd;

public class InterstitialCdManager
{
	private readonly struct CdGateSnapshot
	{
		public InterstitialCdStrategyKind Kind { get; }

		public float Elapsed { get; }

		public float BaseInterval { get; }

		public float EffectiveInterval { get; }

		public int CacheCount { get; }

		public bool AdWorth { get; }

		public CdGateSnapshot(InterstitialCdStrategyKind kind, float elapsed, float baseInterval, float effectiveInterval, int cacheCount, bool adWorth)
		{
			Kind = kind;
			Elapsed = elapsed;
			BaseInterval = baseInterval;
			EffectiveInterval = effectiveInterval;
			CacheCount = cacheCount;
			AdWorth = adWorth;
		}
	}

	private float lastClosedTime = -1f;

	private bool hasClosedInterstitial;

	private float lastLoggedStrategyInterval = -1f;

	private float lastLoggedArrMultiplier = -1f;

	private float lastLoggedEcpmMultiplier = -1f;

	private float lastLoggedInstallCampaignMultiplier = -1f;

	public IInterstitialCooldownStrategy CooldownStrategy { get; private set; }

	public InterstitialCdStrategyKind StrategyKind => CooldownStrategy?.Kind ?? InterstitialCdStrategyKind.Fixed;

	public bool IsCooldownActive => hasClosedInterstitial;

	public InterstitialCdManager()
	{
		BindCooldownStrategy(InterstitialCdStrategyFactory.Create(InterstitialCdStrategyKind.Fixed));
	}

	public void ApplyCooldownStrategy(InterstitialCdStrategyKind kind)
	{
		IInterstitialCooldownStrategy strategy = InterstitialCdStrategyFactory.Create(kind);
		BindCooldownStrategy(strategy);
		InterstitialCdStrategyFactory.LogResolvedStrategy(kind, strategy);
	}

	public void RestoreFromSave(long lastInterAdTimestamp)
	{
		if (lastInterAdTimestamp > 0)
		{
			long num = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastInterAdTimestamp;
			if (num < 0)
			{
				num = 0L;
			}
			hasClosedInterstitial = true;
			lastClosedTime = Time.realtimeSinceStartup - (float)num;
		}
	}

	public bool CanShow(IAdConfiguration config)
	{
		if (!hasClosedInterstitial)
		{
			return true;
		}
		CdGateSnapshot snapshot = BuildGateSnapshot(config);
		return EvaluateCanShow(snapshot);
	}

	public string BuildCdBlockReason(IAdConfiguration config)
	{
		if (!hasClosedInterstitial)
		{
			return "CD未生效(无历史关闭记录)";
		}
		CdGateSnapshot snapshot = BuildGateSnapshot(config);
		return BuildBlockReason(snapshot);
	}

	public bool TryGetElapsedSinceLastClose(out float elapsedSeconds)
	{
		if (!hasClosedInterstitial)
		{
			elapsedSeconds = 0f;
			return false;
		}
		elapsedSeconds = GetElapsedSinceLastClose();
		return true;
	}

	public void StartCooldown()
	{
		hasClosedInterstitial = true;
		lastClosedTime = Time.realtimeSinceStartup;
	}

	public void ClearCooldown()
	{
		hasClosedInterstitial = false;
		lastClosedTime = -1f;
	}

	public void SetClosedInterstitial(bool isClosed)
	{
		hasClosedInterstitial = isClosed;
	}

	public void RecalculateCooldown()
	{
		CooldownStrategy?.CalculateCooldown();
	}

	private void BindCooldownStrategy(IInterstitialCooldownStrategy strategy)
	{
		CooldownStrategy = strategy;
		InterstitialEscapeTracker.CooldownStrategy = strategy;
		RegisterInterstitialCdElapsedProvider();
	}

	private void RegisterInterstitialCdElapsedProvider()
	{
		InterstitialEscapeTracker.SetInterstitialCdElapsedProvider(() => (!hasClosedInterstitial) ? ((float?)null) : new float?(GetElapsedSinceLastClose()));
	}

	private float GetElapsedSinceLastClose()
	{
		float num = Time.realtimeSinceStartup - lastClosedTime;
		if (!(num < 0f))
		{
			return num;
		}
		return 0f;
	}

	private CdGateSnapshot BuildGateSnapshot(IAdConfiguration config)
	{
		InterstitialCdStrategyKind strategyKind = StrategyKind;
		float elapsedSinceLastClose = GetElapsedSinceLastClose();
		float baseInterval = ResolveBaseInterval(config);
		int cacheCount = 0;
		bool adWorth = false;
		if (strategyKind == InterstitialCdStrategyKind.Gp39)
		{
			cacheCount = InterstitialCacheHelper.GetInterstitialCacheCount();
			adWorth = ColdStartEcpmTracker.GetAdWorth();
		}
		float effectiveInterval = ResolveEffectiveInterval(baseInterval, elapsedSinceLastClose, cacheCount);
		return new CdGateSnapshot(strategyKind, elapsedSinceLastClose, baseInterval, effectiveInterval, cacheCount, adWorth);
	}

	private float ResolveBaseInterval(IAdConfiguration config)
	{
		float cooldownSeconds = CooldownStrategy.GetCooldownSeconds(config);
		float cdMultiplier = ARRExperiment.GetCdMultiplier();
		float cdMultiplier2 = FirstEcpmTierCdExperiment.GetCdMultiplier();
		float cdMultiplier3 = InstallCampaignPolicy.GetCdMultiplier();
		float num = cooldownSeconds * cdMultiplier * cdMultiplier2 * cdMultiplier3;
		LogCdMultiplierCalculation(cooldownSeconds, cdMultiplier, cdMultiplier2, cdMultiplier3, num);
		return num;
	}

	private void LogCdMultiplierCalculation(float strategyInterval, float arrMultiplier, float ecpmMultiplier, float installCampaignMultiplier, float finalInterval)
	{
		if (!Mathf.Approximately(strategyInterval, lastLoggedStrategyInterval) || !Mathf.Approximately(arrMultiplier, lastLoggedArrMultiplier) || !Mathf.Approximately(ecpmMultiplier, lastLoggedEcpmMultiplier) || !Mathf.Approximately(installCampaignMultiplier, lastLoggedInstallCampaignMultiplier))
		{
			lastLoggedStrategyInterval = strategyInterval;
			lastLoggedArrMultiplier = arrMultiplier;
			lastLoggedEcpmMultiplier = ecpmMultiplier;
			lastLoggedInstallCampaignMultiplier = installCampaignMultiplier;
			AdBridgeHelper.GetInstallDays();
		}
	}

	private float ResolveEffectiveInterval(float baseInterval, float elapsed, int cacheCount)
	{
		if (StrategyKind != InterstitialCdStrategyKind.Gp39)
		{
			return baseInterval;
		}
		if (elapsed >= baseInterval && cacheCount == 1)
		{
			return baseInterval + 60f;
		}
		return baseInterval;
	}

	private bool EvaluateCanShow(CdGateSnapshot snapshot)
	{
		return snapshot.Kind switch
		{
			InterstitialCdStrategyKind.CommerceLtv => EvaluateCommerceLtvCanShow(snapshot), 
			InterstitialCdStrategyKind.Gp39 => EvaluateGp39CanShow(snapshot), 
			_ => EvaluateFixedIntervalCanShow(snapshot), 
		};
	}

	private bool EvaluateCommerceLtvCanShow(CdGateSnapshot snapshot)
	{
		if (CooldownStrategy is CommerceLtvInterstitialCooldownStrategy commerceLtvInterstitialCooldownStrategy && commerceLtvInterstitialCooldownStrategy.TryHighPriceInstantPass(snapshot.Elapsed))
		{
			return true;
		}
		return EvaluateFixedIntervalCanShow(snapshot);
	}

	private bool EvaluateGp39CanShow(CdGateSnapshot snapshot)
	{
		float num = snapshot.BaseInterval * 0.7f;
		if (snapshot.Elapsed >= num && snapshot.AdWorth)
		{
			return true;
		}
		if (snapshot.Elapsed >= num && snapshot.CacheCount >= 3)
		{
			return true;
		}
		if (snapshot.Elapsed < snapshot.EffectiveInterval)
		{
			return false;
		}
		return true;
	}

	private bool EvaluateFixedIntervalCanShow(CdGateSnapshot snapshot)
	{
		if (snapshot.Elapsed < snapshot.EffectiveInterval)
		{
			return false;
		}
		return true;
	}

	private string BuildBlockReason(CdGateSnapshot snapshot)
	{
		return snapshot.Kind switch
		{
			InterstitialCdStrategyKind.CommerceLtv => BuildCommerceLtvBlockReason(snapshot), 
			InterstitialCdStrategyKind.Gp39 => BuildGp39BlockReason(snapshot), 
			_ => BuildFixedBlockReason(snapshot), 
		};
	}

	private string BuildCommerceLtvBlockReason(CdGateSnapshot snapshot)
	{
		CommerceLtvInterstitialCooldownStrategy commerceLtvInterstitialCooldownStrategy = CooldownStrategy as CommerceLtvInterstitialCooldownStrategy;
		bool highPriceInstantPass = commerceLtvInterstitialCooldownStrategy?.TryHighPriceInstantPass(snapshot.Elapsed) ?? false;
		float remaining = Mathf.Max(0f, snapshot.EffectiveInterval - snapshot.Elapsed);
		float minCd = commerceLtvInterstitialCooldownStrategy?.MinCooldownSeconds ?? 0f;
		return AdBlockReason.FormatCommerceLtvCooldownBlocked(snapshot.Elapsed, snapshot.EffectiveInterval, remaining, minCd, highPriceInstantPass);
	}

	private string BuildGp39BlockReason(CdGateSnapshot snapshot)
	{
		float remaining = Mathf.Max(0f, snapshot.EffectiveInterval - snapshot.Elapsed);
		return AdBlockReason.FormatGp39CooldownBlocked(snapshot.Elapsed, snapshot.EffectiveInterval, remaining, snapshot.CacheCount, snapshot.BaseInterval * 0.7f, snapshot.AdWorth, 3);
	}

	private string BuildFixedBlockReason(CdGateSnapshot snapshot)
	{
		float remaining = Mathf.Max(0f, snapshot.EffectiveInterval - snapshot.Elapsed);
		return AdBlockReason.FormatFixedCooldownBlocked(snapshot.Elapsed, snapshot.EffectiveInterval, remaining, InterstitialEscapeTracker.IsInActiveEscapeSkipPeriod());
	}
}
