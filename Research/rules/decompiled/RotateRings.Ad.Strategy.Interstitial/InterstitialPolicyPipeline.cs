using System;
using System.Collections.Generic;
using System.Text;

namespace RotateRings.Ad.Strategy.Interstitial;

public class InterstitialPolicyPipeline
{
	private readonly List<IInterstitialPolicy> gatePolicies = new List<IInterstitialPolicy>();

	private readonly List<IInterstitialPolicy> consumePolicies = new List<IInterstitialPolicy>();

	public void Init()
	{
		ForEachPolicy(delegate(IInterstitialPolicy policy)
		{
			policy.OnInit();
		});
	}

	public void NotifyRestartClicked(int level)
	{
		ForEachPolicy(delegate(IInterstitialPolicy policy)
		{
			policy.OnRestartClicked(level);
		});
	}

	public void NotifyMidGameExit(int level)
	{
		ForEachPolicy(delegate(IInterstitialPolicy policy)
		{
			policy.OnMidGameExit(level);
		});
	}

	public void OnInterstitialFinished(InterstitialFinishedContext context)
	{
		ForEachPolicy(delegate(IInterstitialPolicy policy)
		{
			policy.OnInterstitialFinished(context);
		});
	}

	public void OnRewardAdFinished(RewardAdFinishedContext context)
	{
		ForEachPolicy(delegate(IInterstitialPolicy policy)
		{
			policy.OnRewardAdFinished(context);
		});
	}

	public void ResetForDebug()
	{
		ForEachPolicy(delegate(IInterstitialPolicy policy)
		{
			policy.ResetForDebug();
		});
	}

	public InterAdCheckResult EvaluateGates(InterstitialPolicyContext context)
	{
		return Evaluate(gatePolicies, context);
	}

	public InterAdCheckResult EvaluateConsume(InterstitialPolicyContext context)
	{
		return Evaluate(consumePolicies, context);
	}

	public string BuildDiagnostics()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		AppendStage(stringBuilder, "gate(门控级)", gatePolicies);
		AppendStage(stringBuilder, "consume(消费级)", consumePolicies);
		return stringBuilder.ToString();
	}

	private static InterAdCheckResult Evaluate(List<IInterstitialPolicy> policies, InterstitialPolicyContext context)
	{
		for (int i = 0; i < policies.Count; i++)
		{
			InterAdCheckResult result = policies[i].Evaluate(context);
			if (!result.Allowed)
			{
				return result;
			}
		}
		return InterAdCheckResult.Allow;
	}

	private static void AppendStage(StringBuilder sb, string stageName, List<IInterstitialPolicy> policies)
	{
		for (int i = 0; i < policies.Count; i++)
		{
			IInterstitialPolicy interstitialPolicy = policies[i];
			string value = interstitialPolicy.BuildDebugState();
			if (!string.IsNullOrEmpty(value))
			{
				sb.Append(stageName).Append('/').Append(interstitialPolicy.Name)
					.Append(": ")
					.Append(value)
					.Append('\n');
			}
		}
	}

	private void ForEachPolicy(Action<IInterstitialPolicy> action)
	{
		for (int i = 0; i < gatePolicies.Count; i++)
		{
			action(gatePolicies[i]);
		}
		for (int j = 0; j < consumePolicies.Count; j++)
		{
			action(consumePolicies[j]);
		}
	}

	public InterstitialPolicyPipeline AddGate(IInterstitialPolicy policy)
	{
		gatePolicies.Add(policy);
		return this;
	}

	public InterstitialPolicyPipeline AddConsume(IInterstitialPolicy policy)
	{
		consumePolicies.Add(policy);
		return this;
	}
}
