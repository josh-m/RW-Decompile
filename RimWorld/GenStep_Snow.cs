using System;
using Verse;

namespace RimWorld
{
	public class GenStep_Snow : GenStep
	{
		public override void Generate(Map map)
		{
			int num = 0;
			for (int i = (int)(GenLocalDate.Month(map) - Month.Mar); i <= (int)GenLocalDate.Month(map); i++)
			{
				int num2 = i;
				if (num2 < 0)
				{
					num2 += 12;
				}
				Month month = (Month)num2;
				float num3 = GenTemperature.AverageTemperatureAtTileForMonth(map.Tile, month);
				if (num3 < 0f)
				{
					num++;
				}
			}
			float num4 = 0f;
			switch (num)
			{
			case 0:
				return;
			case 1:
				num4 = 0.3f;
				break;
			case 2:
				num4 = 0.7f;
				break;
			case 3:
				num4 = 1f;
				break;
			}
			if (map.mapTemperature.SeasonalTemp > 0f)
			{
				num4 *= 0.4f;
			}
			if ((double)num4 < 0.3)
			{
				return;
			}
			foreach (IntVec3 current in map.AllCells)
			{
				if (!current.Roofed(map))
				{
					map.steadyAtmosphereEffects.AddFallenSnowAt(current, num4);
				}
			}
		}
	}
}
