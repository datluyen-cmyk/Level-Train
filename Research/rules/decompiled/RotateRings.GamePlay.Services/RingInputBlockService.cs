using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public class RingInputBlockService : GameplayServiceBase<GameplayManager>
{
	private readonly List<IInputBlockRule> rules = new List<IInputBlockRule>();

	public RingInputBlockService(GameplayManager host)
		: base(host)
	{
		RegisterRule(new ChainInputBlockRule());
	}

	public void RegisterRule(IInputBlockRule rule)
	{
		if (rule != null && !rules.Contains(rule))
		{
			rules.Add(rule);
		}
	}

	public void UnregisterRule(IInputBlockRule rule)
	{
		if (rule != null)
		{
			rules.Remove(rule);
		}
	}

	public bool IsBlocked(Vector2 screenPosition)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < rules.Count; i++)
		{
			IInputBlockRule inputBlockRule = rules[i];
			if (inputBlockRule != null && inputBlockRule.IsBlocking(screenPosition))
			{
				return true;
			}
		}
		return false;
	}

	public override void Dispose()
	{
		rules.Clear();
		base.Dispose();
	}
}
