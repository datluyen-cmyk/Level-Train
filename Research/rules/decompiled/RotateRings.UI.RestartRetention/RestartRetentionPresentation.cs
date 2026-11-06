using System;
using System.Globalization;
using RotateRings.RestartRetention;
using Rotaterings.Rank;
using UnityEngine;

namespace RotateRings.UI.RestartRetention;

public sealed class RestartRetentionPresentation
{
	public string Title;

	public string Body;

	public string Continue;

	public string Restart;

	public string[] TierTexts;

	public bool Leaderboard;

	public int SelectedIndex;

	public int ArrowIndex;

	public static RestartRetentionPresentation Build(RestartRetentionDecision decision, Color highlight, bool leaderboardAvailable)
	{
		string argument = ((decision.Content == RestartRetentionContent.Leaderboard) ? decision.Multiplier : decision.Streak).ToString(CultureInfo.InvariantCulture);
		object key = decision.Content switch
		{
			RestartRetentionContent.Leaderboard => "Restart_LeaderboardStreakPrompt", 
			RestartRetentionContent.Streak => "Restart_StreakPrompt", 
			_ => "Restart_Prompt", 
		};
		bool flag = !decision.UseBaseArt && leaderboardAvailable && decision.DisplayTier >= 1;
		string body = ReadText((string)key, argument);
		if (!decision.UseBaseArt && !flag)
		{
			flag = false;
			body = ReadText("Restart_Prompt");
		}
		string[] array = new string[5];
		string text = ReadText("Word_X");
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = text + RankScoreService.GetTierByIndex(i + 1).ToString(CultureInfo.InvariantCulture);
		}
		return new RestartRetentionPresentation
		{
			Title = ReadText("Level_LeaveTitle"),
			Body = body,
			Continue = ReadText("Button_Continue"),
			Restart = ReadText("Button_Restart"),
			TierTexts = array,
			Leaderboard = flag,
			SelectedIndex = Math.Max(0, decision.DisplayTier - 1),
			ArrowIndex = decision.DisplayTier - 2
		};
	}

	private static string ReadText(string key, string argument = null)
	{
		string text = string.Empty;
		try
		{
			text = GameApp.Lang.GetText(key, Array.Empty<object>());
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			return (argument == null) ? text : string.Format(text, argument);
		}
		catch (FormatException)
		{
			return text ?? string.Empty;
		}
	}
}
