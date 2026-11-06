using System.Collections.Generic;

namespace RotateRings.Ad.CommerceLtv;

public static class InterEcpmBaseline
{
	public static double GetMedian(IReadOnlyList<double> values)
	{
		switch (values?.Count ?? 0)
		{
		case 0:
			return 0.0;
		case 1:
			return values[0];
		case 2:
			return (values[0] + values[1]) / 2.0;
		default:
		{
			double num = values[0];
			double num2 = values[1];
			double num3 = values[2];
			if (num > num2)
			{
				double num4 = num2;
				double num5 = num;
				num = num4;
				num2 = num5;
			}
			if (num2 > num3)
			{
				double num6 = num3;
				double num5 = num2;
				num2 = num6;
				num3 = num5;
			}
			if (num > num2)
			{
				double num7 = num2;
				double num5 = num;
				num = num7;
				num2 = num5;
			}
			return num2;
		}
		}
	}
}
