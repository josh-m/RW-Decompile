using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StoryState : IExposable
	{
		private int lastThreatBigTick = -1;

		public Dictionary<IncidentDef, int> lastFireTicks = new Dictionary<IncidentDef, int>();

		public int LastThreatBigTick
		{
			get
			{
				if (this.lastThreatBigTick > Find.TickManager.TicksGame + 1000)
				{
					Log.Error(string.Concat(new object[]
					{
						"Latest big threat queue time was ",
						this.lastThreatBigTick,
						" at tick ",
						Find.TickManager.TicksGame,
						". This is too far in the future. Resetting."
					}));
					this.lastThreatBigTick = Find.TickManager.TicksGame - 1;
				}
				return this.lastThreatBigTick;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.lastThreatBigTick, "lastThreatBigTick", 0, true);
			Scribe_Collections.LookDictionary<IncidentDef, int>(ref this.lastFireTicks, "lastFireTicks", LookMode.Def, LookMode.Value);
		}

		public void Notify_IncidentFired(FiringIncident qi)
		{
			if (qi.parms.forced)
			{
				return;
			}
			if (qi.def.category == IncidentCategory.ThreatBig)
			{
				if (this.lastThreatBigTick <= Find.TickManager.TicksGame)
				{
					this.lastThreatBigTick = Find.TickManager.TicksGame;
				}
				else
				{
					Log.Error("Queueing threats backwards in time (" + qi + ")");
				}
				Find.StoryWatcher.statsRecord.numThreatBigs++;
			}
			if (this.lastFireTicks.ContainsKey(qi.def))
			{
				this.lastFireTicks[qi.def] = Find.TickManager.TicksGame;
			}
			else
			{
				this.lastFireTicks.Add(qi.def, Find.TickManager.TicksGame);
			}
		}
	}
}
