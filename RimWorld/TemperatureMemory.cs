using System;
using Verse;

namespace RimWorld
{
	public class TemperatureMemory : IExposable
	{
		private const int TicksBuffer = 30000;

		private Map map;

		private int growthSeasonUntilTick = -1;

		private int noGrowUntilTick = -1;

		public bool GrowthSeasonOutdoorsNow
		{
			get
			{
				return (this.noGrowUntilTick <= 0 || Find.TickManager.TicksGame >= this.noGrowUntilTick) && Find.TickManager.TicksGame < this.growthSeasonUntilTick;
			}
		}

		public TemperatureMemory(Map map)
		{
			this.map = map;
		}

		public void GrowthSeasonMemoryTick()
		{
			if (this.map.mapTemperature.OutdoorTemp > 0f && this.map.mapTemperature.OutdoorTemp < 58f)
			{
				this.growthSeasonUntilTick = Find.TickManager.TicksGame + 30000;
			}
			else if (this.map.mapTemperature.OutdoorTemp < -2f)
			{
				this.growthSeasonUntilTick = -1;
				this.noGrowUntilTick = Find.TickManager.TicksGame + 30000;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.growthSeasonUntilTick, "growthSeasonUntilTick", 0, true);
			Scribe_Values.LookValue<int>(ref this.noGrowUntilTick, "noGrowUntilTick", 0, true);
		}
	}
}
