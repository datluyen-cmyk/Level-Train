using UnityEngine;

namespace RotateRings.GamePlay.Services.CharacterExpression;

internal static class CharacterExpressionEffectCatalog
{
	private const string SoyLike = "eff_emoji_combo_01";

	private const string SoyConfetti = "eff_emoji_combo_02";

	private const string SoyHeart = "eff_emoji_combo_03";

	private const string SquirrelLike = "eff_emoji_combo_04";

	private const string SquirrelConfetti = "eff_emoji_combo_05";

	private const string SquirrelHeart = "eff_emoji_combo_06";

	private static readonly string[] SoyRandomPool = new string[3] { "eff_emoji_combo_01", "eff_emoji_combo_02", "eff_emoji_combo_03" };

	private static readonly string[] SquirrelRandomPool = new string[3] { "eff_emoji_combo_04", "eff_emoji_combo_05", "eff_emoji_combo_06" };

	public static bool TryPickEffect(int feedback, out string effectLocation)
	{
		effectLocation = null;
		switch (feedback)
		{
		case 1:
			effectLocation = SquirrelRandomPool[Random.Range(0, SquirrelRandomPool.Length)];
			return true;
		case 2:
			effectLocation = SoyRandomPool[Random.Range(0, SoyRandomPool.Length)];
			return true;
		case 3:
			effectLocation = "eff_emoji_combo_04";
			return true;
		case 4:
			effectLocation = "eff_emoji_combo_01";
			return true;
		default:
			return false;
		}
	}
}
