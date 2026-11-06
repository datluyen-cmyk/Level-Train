using System.Text;
using RotateRings.Ad.Bridge;
using RotateRings.Ad.Cd;
using XingYun.Ad;

namespace RotateRings.Ad.Strategy.Interstitial;

public abstract class InterAdStrategyBase : AdStrategyBase, IInterAdStrategy
{
	protected readonly InterstitialBaseboardGate BaseboardGate = new InterstitialBaseboardGate();

	protected readonly InterstitialCdManager CdManager = new InterstitialCdManager();

	protected readonly InterstitialPolicyPipeline PolicyPipeline = new InterstitialPolicyPipeline();

	private bool policiesRegistered;

	protected abstract void RegisterGatePolicies(InterstitialPolicyPipeline pipeline);

	protected virtual void RegisterConsumePolicies(InterstitialPolicyPipeline pipeline)
	{
		pipeline.AddConsume(new InterstitialEscapeCountPolicy());
	}

	protected abstract InterstitialCdStrategyKind GetCooldownKind();

	protected virtual string BuildDiagnosticsPrefix()
	{
		return string.Empty;
	}

	protected virtual void OnEvaluateCompleted(IAdConfiguration config, bool isFrontAd, string stage, InterAdCheckResult result)
	{
	}

	public void InitInterstitial(long lastInterAdTimestamp)
	{
		if (!policiesRegistered)
		{
			RegisterGatePolicies(PolicyPipeline);
			RegisterConsumePolicies(PolicyPipeline);
			policiesRegistered = true;
		}
		BaseboardGate.OnInit();
		PolicyPipeline.Init();
		CdManager.ApplyCooldownStrategy(GetCooldownKind());
		CdManager.RestoreFromSave(lastInterAdTimestamp);
	}

	public void StartInterstitialCooldown()
	{
		CdManager.StartCooldown();
	}

	public void ClearInterstitialCooldown()
	{
		CdManager.ClearCooldown();
	}

	public void RecalculateInterstitialCooldown()
	{
		CdManager.RecalculateCooldown();
	}

	public void RebindInterstitialCooldownStrategy()
	{
		CdManager.ApplyCooldownStrategy(GetCooldownKind());
	}

	public void OnInterstitialFinished(InterstitialFinishedContext context)
	{
		BaseboardGate.OnInterstitialFinished(context);
		PolicyPipeline.OnInterstitialFinished(context);
	}

	public void OnRewardAdFinished(RewardAdFinishedContext context)
	{
		BaseboardGate.OnRewardAdFinished(context);
		PolicyPipeline.OnRewardAdFinished(context);
	}

	public void SetClosedInterstitial(bool isClosed)
	{
		CdManager.SetClosedInterstitial(isClosed);
	}

	public void NotifyRestartClicked(int level)
	{
		BaseboardGate.OnRestartClicked(level);
		PolicyPipeline.NotifyRestartClicked(level);
	}

	public void NotifyMidGameExit(int level)
	{
		BaseboardGate.OnMidGameExit(level);
		PolicyPipeline.NotifyMidGameExit(level);
	}

	public void ResetExperimentPolicyStateForDebug()
	{
		BaseboardGate.ResetForDebug();
		PolicyPipeline.ResetForDebug();
	}

	public string BuildExperimentPolicyDiagnostics()
	{
		string text = BaseboardGate.BuildDebugState();
		string text2 = PolicyPipeline.BuildDiagnostics();
		if (!string.IsNullOrEmpty(text))
		{
			return text + "\n" + text2;
		}
		return text2;
	}

	public virtual InterAdCheckResult CanShowInter(IAdConfiguration config)
	{
		bool isFrontAd = config.GetIsFrontAd();
		InterAdCheckResult result = BaseboardGate.EvaluateRemoveAds();
		if (!result.Allowed)
		{
			return Finish(config, isFrontAd, "去广告", result);
		}
		InterstitialPolicyContext context = InterstitialPolicyContext.Create(config);
		result = BaseboardGate.Evaluate(context, base.Proxy, out var handled);
		if (handled)
		{
			if (!result.Allowed)
			{
				return Finish(config, isFrontAd, "InstallCampaign基础门控", result);
			}
			result = EvaluateCooldown(config, isFrontAd);
			return Finish(config, isFrontAd, "InstallCampaign基础门控", result);
		}
		if (!result.Allowed)
		{
			return Finish(config, isFrontAd, "底板门控", result);
		}
		if (InterstitialExemptConsumeGate.TryConsumeInterstitialExemption(isFrontAd, out var blockReason))
		{
			return Finish(config, isFrontAd, "豁免门控", InterAdCheckResult.Block(blockReason));
		}
		result = PolicyPipeline.EvaluateGates(context);
		if (!result.Allowed)
		{
			return Finish(config, isFrontAd, "子类门控阶段", result);
		}
		result = EvaluateCooldown(config, isFrontAd);
		if (!result.Allowed)
		{
			return Finish(config, isFrontAd, "插屏CD", result);
		}
		result = PolicyPipeline.EvaluateConsume(context);
		if (!result.Allowed)
		{
			return Finish(config, isFrontAd, "消费阶段", result);
		}
		return Finish(config, isFrontAd, "全部通过", InterAdCheckResult.Allow);
	}

	private InterAdCheckResult EvaluateCooldown(IAdConfiguration config, bool isFrontAd)
	{
		if (CdManager.CanShow(config))
		{
			return InterAdCheckResult.Allow;
		}
		return InterAdCheckResult.Block(CdManager.BuildCdBlockReason(config));
	}

	public string BuildInterstitialBlockDiagnostics(IAdConfiguration config, string reason)
	{
		StringBuilder stringBuilder = new StringBuilder(192);
		stringBuilder.Append(BuildDiagnosticsPrefix());
		if (AdBlockReason.HasCooldownReasonPrefix(reason))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" | ");
			}
			stringBuilder.Append("cdDetail=").Append(CdManager.BuildCdBlockReason(config));
		}
		return stringBuilder.ToString();
	}

	private InterAdCheckResult Finish(IAdConfiguration config, bool isFrontAd, string stage, InterAdCheckResult result)
	{
		OnEvaluateCompleted(config, isFrontAd, stage, result);
		return result;
	}
}
