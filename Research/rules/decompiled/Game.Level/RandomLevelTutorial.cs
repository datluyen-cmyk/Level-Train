namespace Game.Level;

public static class RandomLevelTutorial
{
	public static int ResolveSpecialPositionElementId(int level)
	{
		switch (level)
		{
		case 15:
		case 16:
			return 1;
		case 101:
		case 102:
			return 2;
		default:
			return 0;
		}
	}

	public static int ResolveElementId(int level, int parts)
	{
		int num = ResolveSpecialPositionElementId(level);
		if (num > 0)
		{
			return num;
		}
		if ((parts & 1) != 0)
		{
			return 1;
		}
		if ((parts & 4) != 0)
		{
			return 2;
		}
		if ((parts & 8) != 0)
		{
			return 3;
		}
		return 0;
	}

	public static string DescribeElement(int elementId)
	{
		return elementId switch
		{
			1 => "炸弹", 
			2 => "锁链", 
			3 => "钥匙锁头", 
			_ => "无", 
		};
	}
}
