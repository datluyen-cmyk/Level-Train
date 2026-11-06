using System;
using UnityEngine;
using XingYun.Level;

namespace Game.Level;

public class RandomWeightAction : IMatchAction
{
	private const string TAG = "[RL][RandomWeight] ";

	private static readonly char[] SPLIT = new char[1] { ',' };

	public void Execute(ILevelMatchRule rule, LevelContext ctx)
	{
		if (rule != null && ctx != null)
		{
			string[] array = Split(rule.StringValue1);
			if (array.Length != 0)
			{
				int[] weights = ResolveWeights(rule, array.Length);
				int num = Pick(array, weights);
				string key = rule.Target + "_Fixed";
				ctx.Data[key] = array[num];
			}
		}
	}

	private static int[] ResolveWeights(ILevelMatchRule rule, int count)
	{
		string[] array = Split(rule.StringValue2);
		if (array.Length != count)
		{
			return null;
		}
		int[] array2 = new int[count];
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (!int.TryParse(array[i], out var result) || result < 0)
			{
				return null;
			}
			array2[i] = result;
			num += result;
		}
		if (num <= 0)
		{
			return null;
		}
		return array2;
	}

	private static int Pick(string[] values, int[] weights)
	{
		int num = values.Length;
		if (weights == null)
		{
			return Random.Range(0, num);
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += weights[i];
		}
		int num3 = Random.Range(0, num2);
		for (int j = 0; j < num; j++)
		{
			if (weights[j] > 0)
			{
				num3 -= weights[j];
				if (num3 < 0)
				{
					return j;
				}
			}
		}
		return num - 1;
	}

	private static string[] Split(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return Array.Empty<string>();
		}
		string[] array = text.Split(SPLIT, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		return array;
	}
}
