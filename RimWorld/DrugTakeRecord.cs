using System;
using Verse;

namespace RimWorld
{
	public class DrugTakeRecord : IExposable
	{
		public ThingDef drug;

		public int lastTakenTicks;

		private int timesTakenThisDayInt;

		private int thisDay;

		public int LastTakenDays
		{
			get
			{
				return GenDate.DaysPassedAt(this.lastTakenTicks);
			}
		}

		public int TimesTakenThisDay
		{
			get
			{
				if (this.thisDay != GenDate.DaysPassed)
				{
					return 0;
				}
				return this.timesTakenThisDayInt;
			}
			set
			{
				this.timesTakenThisDayInt = value;
				this.thisDay = GenDate.DaysPassed;
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.LookDef<ThingDef>(ref this.drug, "drug");
			Scribe_Values.LookValue<int>(ref this.lastTakenTicks, "lastTakenTicks", 0, false);
			Scribe_Values.LookValue<int>(ref this.timesTakenThisDayInt, "timesTakenThisDay", 0, false);
			Scribe_Values.LookValue<int>(ref this.thisDay, "thisDay", 0, false);
		}
	}
}
