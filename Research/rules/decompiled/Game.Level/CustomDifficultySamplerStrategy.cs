using System;
using UnityEngine;
using XingYun.Level;

namespace Game.Level;

public class CustomDifficultySamplerStrategy : IDifficultySamplerStrategy
{
	public float m_MU = 150f;

	public float m_SIGMA = 5f;

	public float m_M = 1.5f;

	private readonly Random rng = new Random();

	public void UpdateSampleParams(float mu, float sigma, float m)
	{
		m_MU = mu;
		m_SIGMA = sigma;
		m_M = m;
	}

	public float Sample(LevelContext ctx)
	{
		GameLevelProgress gameLevelProgress = GameApp.Proxy.Get<GameLevelProxy>().GetGameLevelProgress();
		DifficultySamplerConfig samplerConfig = ctx.SamplerConfig;
		float num = 0f;
		if (gameLevelProgress.InitialAbility <= 0f && gameLevelProgress.CurrentLevelDifficulty <= 0f)
		{
			if (ctx.Data.TryGetValue("initial_ability", out var value) && value is float initialAbility)
			{
				gameLevelProgress.InitialAbility = initialAbility;
			}
			else
			{
				gameLevelProgress.InitialAbility = samplerConfig.MinDifficulty;
			}
			num = Mathf.Max(gameLevelProgress.InitialAbility, samplerConfig.MinDifficulty);
		}
		else
		{
			num = Mathf.Max(ctx.PlayerAbility, samplerConfig.MinDifficulty);
		}
		float mU = m_MU;
		float num2 = num * 0.9f;
		Mathf.Max(ctx.LevelIndex - 1, 0);
		float sIGMA = m_SIGMA;
		double num3 = rng.NextDouble();
		double num4 = rng.NextDouble();
		if (num3 <= 0.0)
		{
			num3 = double.Epsilon;
		}
		if (num4 <= 0.0)
		{
			num4 = double.Epsilon;
		}
		double num5 = Math.Sqrt(-2.0 * Math.Log(num3));
		double num6 = Math.PI * 2.0 * num4;
		double num7 = num5 * Math.Cos(num6);
		double num8 = num5 * Math.Sin(num6);
		float num9 = mU + (float)(num7 * (double)sIGMA);
		float num10 = mU + (float)(num8 * (double)sIGMA);
		float num11 = ((rng.Next(0, 2) == 0) ? num9 : num10);
		return Mathf.Sqrt(num2 * num11 * m_M);
	}
}
