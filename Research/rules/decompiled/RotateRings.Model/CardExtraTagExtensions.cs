namespace RotateRings.Model;

public static class CardExtraTagExtensions
{
	public static bool HasTag(this CardExtraTag tag, CardExtraTag flag)
	{
		return (tag & flag) == flag;
	}

	public static CardExtraTag RemoveTag(this CardExtraTag tag, CardExtraTag flag)
	{
		return (CardExtraTag)((uint)tag & (uint)(byte)(~(int)flag));
	}
}
