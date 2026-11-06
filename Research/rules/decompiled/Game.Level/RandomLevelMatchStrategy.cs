using System.Collections.Generic;
using XingYun.Level;

namespace Game.Level;

public class RandomLevelMatchStrategy : UniversalLevelStrategy, ILevelStrategy
{
	private const string TAG = "[RL][MatchStrategy] ";

	private readonly List<int> groupOrder = new List<int>();

	private readonly Dictionary<int, List<ILevelMatchRule>> rulesByGroup = new Dictionary<int, List<ILevelMatchRule>>();

	public RandomLevelMatchStrategy(string matchFilterInfo, List<ILevelMatchRule> rules)
		: base(matchFilterInfo, new List<ILevelMatchRule>())
	{
		Build(rules);
	}

	public string GetLevel(LevelContext ctx, LevelRepository repo)
	{
		return GetLevelInfo(ctx, repo)?.LevelId;
	}

	public LevelInfo GetLevelInfo(LevelContext ctx, LevelRepository repo)
	{
		ExecuteByGroup(ctx);
		return ((UniversalLevelStrategy)this).GetLevelInfo(ctx, repo);
	}

	private void Build(List<ILevelMatchRule> rules)
	{
		groupOrder.Clear();
		rulesByGroup.Clear();
		if (rules == null)
		{
			return;
		}
		for (int i = 0; i < rules.Count; i++)
		{
			ILevelMatchRule val = rules[i];
			if (val != null)
			{
				if (!rulesByGroup.TryGetValue(val.GroupId, out var value))
				{
					value = new List<ILevelMatchRule>(4);
					rulesByGroup[val.GroupId] = value;
					groupOrder.Add(val.GroupId);
				}
				value.Add(val);
			}
		}
		for (int j = 0; j < groupOrder.Count; j++)
		{
			SortGroup(rulesByGroup[groupOrder[j]]);
		}
	}

	private static void SortGroup(List<ILevelMatchRule> list)
	{
		if (list.Count <= 1)
		{
			return;
		}
		List<int> list2 = new List<int>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(i);
		}
		list2.Sort(delegate(int a, int b)
		{
			int num2 = list[a].GroupPriority.CompareTo(list[b].GroupPriority);
			if (num2 != 0)
			{
				return num2;
			}
			num2 = list[a].Priority.CompareTo(list[b].Priority);
			return (num2 == 0) ? a.CompareTo(b) : num2;
		});
		List<ILevelMatchRule> list3 = new List<ILevelMatchRule>(list.Count);
		for (int num = 0; num < list2.Count; num++)
		{
			list3.Add(list[list2[num]]);
		}
		list.Clear();
		list.AddRange(list3);
	}

	private void ExecuteByGroup(LevelContext ctx)
	{
		int levelIndex = ctx.LevelIndex;
		if (groupOrder.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>(groupOrder.Count);
		List<int> list2 = new List<int>(groupOrder.Count);
		for (int i = 0; i < groupOrder.Count; i++)
		{
			int num = groupOrder[i];
			if (ExecuteGroup(rulesByGroup[num], levelIndex, ctx))
			{
				list.Add(num);
			}
			else
			{
				list2.Add(num);
			}
		}
	}

	private bool ExecuteGroup(List<ILevelMatchRule> list, int levelIndex, LevelContext ctx)
	{
		int count = list.Count;
		int num = 0;
		while (num < count)
		{
			int groupPriority = list[num].GroupPriority;
			int i;
			for (i = num + 1; i < count && list[i].GroupPriority == groupPriority; i++)
			{
			}
			int num2 = num;
			while (num2 < i)
			{
				int priority = list[num2].Priority;
				int j;
				for (j = num2 + 1; j < i && list[j].Priority == priority; j++)
				{
				}
				bool flag = true;
				for (int k = num2; k < j; k++)
				{
					if (!((UniversalLevelStrategy)this).EvaluateCondition(list[k], levelIndex, ctx))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					((UniversalLevelStrategy)this).ExecuteAction(list[num2], ctx);
					List<string> list2 = null;
					for (int l = num2 + 1; l < j; l++)
					{
						if (list[l].Operator == 18)
						{
							((UniversalLevelStrategy)this).ExecuteAction(list[l], ctx);
							(list2 ?? (list2 = new List<string>(2))).Add(list[l].Id);
						}
					}
					return true;
				}
				num2 = j;
			}
			num = i;
		}
		return false;
	}
}
