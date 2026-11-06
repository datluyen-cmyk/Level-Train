using System.Text;

namespace Game.Level;

public static class LevelContentCodec
{
	public const char SEPARATOR = '|';

	public static void Decode(string content, out string coreLevelPath, out int parts)
	{
		parts = 0;
		coreLevelPath = content;
		if (string.IsNullOrEmpty(content))
		{
			coreLevelPath = string.Empty;
			return;
		}
		int num = content.IndexOf('|');
		if (num >= 0)
		{
			coreLevelPath = content.Substring(0, num);
			if (!int.TryParse(content.Substring(num + 1), out parts))
			{
				parts = 0;
			}
		}
	}

	public static string CoreLevelPathOf(string content)
	{
		Decode(content, out var coreLevelPath, out var _);
		return coreLevelPath;
	}

	public static int PartsOf(string content)
	{
		Decode(content, out var _, out var parts);
		return parts;
	}

	public static string Encode(string coreLevelPath, int parts)
	{
		return $"{coreLevelPath}{'|'}{parts}";
	}

	public static string DescribeParts(int parts)
	{
		if (parts == 0)
		{
			return "无";
		}
		StringBuilder stringBuilder = new StringBuilder();
		Append(stringBuilder, parts, 1, "炸弹");
		Append(stringBuilder, parts, 2, "会死");
		Append(stringBuilder, parts, 4, "锁链");
		Append(stringBuilder, parts, 8, "钥匙锁头");
		return stringBuilder.ToString();
	}

	public static string DescribePartBits(int parts)
	{
		return $"炸弹={(((parts & 1) != 0) ? 1 : 0)} " + $"会死={(((parts & 2) != 0) ? 1 : 0)} " + $"锁链={(((parts & 4) != 0) ? 1 : 0)} " + $"钥匙锁头={(((parts & 8) != 0) ? 1 : 0)}";
	}

	private static void Append(StringBuilder sb, int parts, int bit, string name)
	{
		if ((parts & bit) != 0)
		{
			if (sb.Length > 0)
			{
				sb.Append('|');
			}
			sb.Append(name);
		}
	}
}
