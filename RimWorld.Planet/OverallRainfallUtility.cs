using System;
using Verse;

namespace RimWorld.Planet
{
	public static class OverallRainfallUtility
	{
		private static int cachedEnumValuesCount = -1;

		private static readonly SimpleCurve Curve_AlmostNone = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1500f, 120f),
			new CurvePoint(3500f, 180f),
			new CurvePoint(6000f, 200f),
			new CurvePoint(12000f, 250f)
		};

		private static readonly SimpleCurve Curve_Little = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1500f, 300f),
			new CurvePoint(6000f, 1100f),
			new CurvePoint(12000f, 1400f)
		};

		private static readonly SimpleCurve Curve_LittleBitLess = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1000f, 700f),
			new CurvePoint(5000f, 4700f),
			new CurvePoint(12000f, 12000f),
			new CurvePoint(99999f, 99999f)
		};

		private static readonly SimpleCurve Curve_LittleBitMore = new SimpleCurve
		{
			new CurvePoint(0f, 50f),
			new CurvePoint(5000f, 5300f),
			new CurvePoint(12000f, 12000f),
			new CurvePoint(99999f, 99999f)
		};

		private static readonly SimpleCurve Curve_High = new SimpleCurve
		{
			new CurvePoint(0f, 500f),
			new CurvePoint(150f, 950f),
			new CurvePoint(500f, 2000f),
			new CurvePoint(1000f, 2800f),
			new CurvePoint(5000f, 6000f),
			new CurvePoint(12000f, 12000f),
			new CurvePoint(99999f, 99999f)
		};

		private static readonly SimpleCurve Curve_VeryHigh = new SimpleCurve
		{
			new CurvePoint(0f, 750f),
			new CurvePoint(125f, 2000f),
			new CurvePoint(500f, 3000f),
			new CurvePoint(1000f, 3800f),
			new CurvePoint(5000f, 7500f),
			new CurvePoint(12000f, 12000f),
			new CurvePoint(99999f, 99999f)
		};

		public static int EnumValuesCount
		{
			get
			{
				if (OverallRainfallUtility.cachedEnumValuesCount < 0)
				{
					OverallRainfallUtility.cachedEnumValuesCount = Enum.GetNames(typeof(OverallRainfall)).Length;
				}
				return OverallRainfallUtility.cachedEnumValuesCount;
			}
		}

		public static SimpleCurve GetRainfallCurve(this OverallRainfall overallRainfall)
		{
			switch (overallRainfall)
			{
			case OverallRainfall.AlmostNone:
				return OverallRainfallUtility.Curve_AlmostNone;
			case OverallRainfall.Little:
				return OverallRainfallUtility.Curve_Little;
			case OverallRainfall.LittleBitLess:
				return OverallRainfallUtility.Curve_LittleBitLess;
			case OverallRainfall.LittleBitMore:
				return OverallRainfallUtility.Curve_LittleBitMore;
			case OverallRainfall.High:
				return OverallRainfallUtility.Curve_High;
			case OverallRainfall.VeryHigh:
				return OverallRainfallUtility.Curve_VeryHigh;
			}
			return null;
		}
	}
}
